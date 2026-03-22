# Isolated.Sdk

An MSBuild SDK that isolates projects from `Directory.Build.props` and `Directory.Build.targets`.

This is a bare isolation SDK — import any SDK you want afterwards.

## Usage

```cs
#:sdk Isolated.Sdk@1.0.0
// use any SDK you want:
#:sdk ...
```
