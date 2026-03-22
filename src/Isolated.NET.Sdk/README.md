# Isolated.NET.Sdk

An MSBuild SDK that isolates projects from `Directory.Build.props` and `Directory.Build.targets`
while implicitly importing `Microsoft.NET.Sdk` (the default SDK).

## Usage

```cs
#:sdk Isolated.NET.Sdk@1.0.0
```

## See also

- [Isolated.Sdk](https://www.nuget.org/packages/Isolated.Sdk) — a bare isolation SDK that lets you import any SDK you want afterwards.
