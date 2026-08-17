using System.ComponentModel;
using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dn <file.cs> [arguments...]");
    return 1;
}

try
{
    return await DnCommand.ExecuteAsync(args[0], args[1..], Directory.GetCurrentDirectory());
}
catch (FileNotFoundException exception)
{
    Console.Error.WriteLine($"dn: {exception.Message}");
    return 1;
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine($"dn: {exception.Message}");
    return 1;
}
catch (Win32Exception exception)
{
    Console.Error.WriteLine($"dn: Could not start dotnet: {exception.Message}");
    return 1;
}

internal static class DnCommand
{
    internal static async Task<int> ExecuteAsync(
        string filePath,
        IEnumerable<string> arguments,
        string callerWorkingDirectory,
        CancellationToken cancellationToken = default)
    {
        using Process process = new()
        {
            StartInfo = CreateStartInfo(filePath, arguments, callerWorkingDirectory),
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    internal static ProcessStartInfo CreateStartInfo(
        string filePath,
        IEnumerable<string> arguments,
        string callerWorkingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(callerWorkingDirectory);

        string fullCallerWorkingDirectory = Path.GetFullPath(callerWorkingDirectory);
        string fullFilePath = Path.GetFullPath(filePath, fullCallerWorkingDirectory);

        if (!File.Exists(fullFilePath))
        {
            throw new FileNotFoundException($"File-based app not found: '{fullFilePath}'.", fullFilePath);
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(fullFilePath)!,
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add(
            $"-p:RunWorkingDirectory={EscapeMsBuildValue(fullCallerWorkingDirectory)}");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(fullFilePath);
        startInfo.ArgumentList.Add("--");

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static string EscapeMsBuildValue(string value) =>
        value
            .Replace("%", "%25", StringComparison.Ordinal)
            .Replace(",", "%2C", StringComparison.Ordinal)
            .Replace(";", "%3B", StringComparison.Ordinal);
}
