# dn

Run a file-based C# app with SDK discovery rooted at the app file.

`dn` is distributed as a Native AOT tool for Windows, Linux, and macOS on
x64 and Arm64.

## Install

```console
dotnet tool install --global dn
```

## Usage

```console
dn file.cs arg0 arg1
```

This is equivalent to:

```console
dotnet run --file file.cs -- arg0 arg1
```

All arguments after the file path are passed to the app verbatim.

`dn` starts `dotnet` from the directory containing the app so SDK resolution,
including the search for `global.json`, starts there. The app itself keeps the
directory from which `dn` was invoked as its working directory.

Starting the installed `dn` tool does not use the .NET runtime or consult
`global.json` in the caller's directory. Only the `dotnet` process that `dn`
starts for the file-based app performs SDK resolution.

For example, when invoked as:

```console
cd /x
dn /y/file.cs
```

SDK resolution and implicit build-file discovery start from `/y`, while
`Environment.CurrentDirectory` in the app is `/x`.

Requires a .NET 10 or later SDK with file-based app support.
