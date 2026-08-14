using System.Diagnostics;
using System.Security;

namespace Dn.Tests;

[TestClass]
public sealed class NativePackageTests
{
    private readonly TestContext _testContext;

    public NativePackageTests(TestContext testContext)
    {
        _testContext = testContext;
    }

    [TestMethod]
    [TestCategory("NativePackage")]
    [Timeout(120_000, CooperativeCancellation = true)]
    public async Task InstalledNativeToolIgnoresCallerGlobalJson()
    {
        string packageDirectory = GetRequiredEnvironmentVariable("DN_TEST_PACKAGE_DIRECTORY");
        string packageVersion = GetRequiredEnvironmentVariable("DN_TEST_PACKAGE_VERSION");
        string runtimeIdentifier = GetRequiredEnvironmentVariable("DN_TEST_RUNTIME_IDENTIFIER");

        Assert.IsTrue(
            Directory.Exists(packageDirectory),
            $"Package directory '{packageDirectory}' does not exist.");

        using TemporaryDirectory temporaryDirectory = new();
        string appDirectory = temporaryDirectory.CreateDirectory("app");
        string callerDirectory = temporaryDirectory.CreateDirectory("caller,semicolon;percent%");
        string toolDirectory = Path.Combine(temporaryDirectory.Path, "tool");
        string outputPath = Path.Combine(callerDirectory, "output.txt");
        string configPath = Path.Combine(temporaryDirectory.Path, "nuget.config");
        string appPath = Path.Combine(appDirectory, "file.cs");

        File.WriteAllText(
            configPath,
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="local" value="{SecurityElement.Escape(Path.GetFullPath(packageDirectory))}" />
              </packageSources>
            </configuration>
            """);
        File.WriteAllText(
            Path.Combine(callerDirectory, "global.json"),
            """
            {
              "sdk": {
                "version": "1.0.0",
                "rollForward": "disable"
              }
            }
            """);
        File.WriteAllText(
            appPath,
            """
            #:property PublishAot=false

            File.WriteAllLines(args[0], args[2..]);
            Environment.Exit(int.Parse(args[1]));
            """);

        ProcessStartInfo installStartInfo = new()
        {
            FileName = "dotnet",
            UseShellExecute = false,
            WorkingDirectory = temporaryDirectory.Path,
        };
        installStartInfo.ArgumentList.Add("tool");
        installStartInfo.ArgumentList.Add("install");
        installStartInfo.ArgumentList.Add("dn");
        installStartInfo.ArgumentList.Add("--tool-path");
        installStartInfo.ArgumentList.Add(toolDirectory);
        installStartInfo.ArgumentList.Add("--version");
        installStartInfo.ArgumentList.Add(packageVersion);
        installStartInfo.ArgumentList.Add("--configfile");
        installStartInfo.ArgumentList.Add(configPath);

        ProcessResult installResult = await RunAsync(
            installStartInfo,
            _testContext.CancellationToken);

        Assert.AreEqual(
            0,
            installResult.ExitCode,
            $"Tool installation failed.{Environment.NewLine}{installResult.StandardOutput}{installResult.StandardError}");

        string executableName = OperatingSystem.IsWindows() ? "dn.exe" : "dn";
        string storeDirectory = $"{Path.DirectorySeparatorChar}.store{Path.DirectorySeparatorChar}";
        string[] nativeExecutables = Directory
            .GetFiles(toolDirectory, executableName, SearchOption.AllDirectories)
            .Where(path => path.Contains(storeDirectory, StringComparison.Ordinal))
            .ToArray();

        Assert.HasCount(
            1,
            nativeExecutables,
            $"Expected one installed native executable, found: {string.Join(", ", nativeExecutables)}");
        Assert.Contains(runtimeIdentifier, nativeExecutables[0]);

        ProcessStartInfo runStartInfo = new()
        {
            FileName = nativeExecutables[0],
            UseShellExecute = false,
            WorkingDirectory = callerDirectory,
        };
        runStartInfo.ArgumentList.Add(appPath);
        runStartInfo.ArgumentList.Add("output.txt");
        runStartInfo.ArgumentList.Add("23");
        runStartInfo.ArgumentList.Add("arg with spaces");
        runStartInfo.ArgumentList.Add("--");
        runStartInfo.ArgumentList.Add("--help");

        ProcessResult runResult = await RunAsync(
            runStartInfo,
            _testContext.CancellationToken);

        Assert.AreEqual(
            23,
            runResult.ExitCode,
            $"Unexpected tool exit code.{Environment.NewLine}{runResult.StandardOutput}{runResult.StandardError}");
        CollectionAssert.AreEqual(
            new[] { "arg with spaces", "--", "--help" },
            File.ReadAllLines(outputPath));
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            Assert.Inconclusive($"Set {name} to run native package integration tests.");
        }

        return value;
    }

    private static async Task<ProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken)
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using Process process = new()
        {
            StartInfo = startInfo,
        };

        process.Start();
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            throw;
        }

        return new(process.ExitCode, await standardOutput, await standardError);
    }

    private sealed record ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);
}
