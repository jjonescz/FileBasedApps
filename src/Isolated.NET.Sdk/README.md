# Isolated.NET.Sdk

An MSBuild SDK that isolates projects from `Directory.Build.props` and `Directory.Build.targets`
while implicitly importing `Microsoft.NET.Sdk` (the default SDK).

## Usage

```cs
#:sdk Isolated.NET.Sdk
```
