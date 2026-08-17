using System.Diagnostics;

namespace Dn.Tests;

[TestClass]
public sealed class DnCommandTests
{
    [TestMethod]
    public void CreateStartInfoBuildsExpectedCommand()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string callerDirectory = temporaryDirectory.CreateDirectory("caller");
        string appDirectory = temporaryDirectory.CreateDirectory("app");
        string appPath = Path.Combine(appDirectory, "file.cs");
        File.WriteAllText(appPath, string.Empty);

        ProcessStartInfo startInfo = DnCommand.CreateStartInfo(
            Path.Combine("..", "app", "file.cs"),
            ["arg with spaces", "--", "--help"],
            callerDirectory);

        Assert.AreEqual("dotnet", startInfo.FileName);
        Assert.IsFalse(startInfo.UseShellExecute);
        Assert.AreEqual(appDirectory, startInfo.WorkingDirectory);
        CollectionAssert.AreEqual(
            new[]
            {
                "run",
                $"-p:RunWorkingDirectory={callerDirectory}",
                "--file",
                appPath,
                "--",
                "arg with spaces",
                "--",
                "--help",
            },
            startInfo.ArgumentList.ToArray());
    }

    [TestMethod]
    public async Task ExecuteUsesAppForSdkDiscoveryAndCallerForAppWorkingDirectory()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string callerDirectory = temporaryDirectory.CreateDirectory("caller,semicolon;percent%");
        string appDirectory = temporaryDirectory.CreateDirectory("app");
        string outputPath = Path.Combine(callerDirectory, "output.txt");
        string appPath = Path.Combine(appDirectory, "file.cs");

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

            File.WriteAllLines(args[0], args[1..]);
            """);

        int exitCode = await DnCommand.ExecuteAsync(
            appPath,
            ["output.txt", "arg with spaces", "--", "--help"],
            callerDirectory);

        Assert.AreEqual(0, exitCode);
        CollectionAssert.AreEqual(
            new[] { "arg with spaces", "--", "--help" },
            File.ReadAllLines(outputPath));
    }

    [TestMethod]
    public async Task ExecuteReturnsAppExitCode()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string callerDirectory = temporaryDirectory.CreateDirectory("caller");
        string appDirectory = temporaryDirectory.CreateDirectory("app");
        string appPath = Path.Combine(appDirectory, "file.cs");
        File.WriteAllText(
            appPath,
            """
            #:property PublishAot=false

            Environment.Exit(int.Parse(args[0]));
            """);

        int exitCode = await DnCommand.ExecuteAsync(appPath, ["23"], callerDirectory);

        Assert.AreEqual(23, exitCode);
    }

    [TestMethod]
    public void CreateStartInfoRejectsMissingFile()
    {
        string missingFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "missing.cs");

        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(
            () => DnCommand.CreateStartInfo(missingFile, [], Directory.GetCurrentDirectory()));

        Assert.AreEqual(Path.GetFullPath(missingFile), exception.FileName);
    }
}
