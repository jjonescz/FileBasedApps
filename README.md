# FileBasedApps

Utilities for file-based apps.

## Packages

### Isolated

MSBuild SDKs for isolating file-based apps from `Directory.Build.props` and `Directory.Build.targets`.

#### [Isolated.NET.Sdk](https://www.nuget.org/packages/Isolated.NET.Sdk)

Sets `ImportDirectoryBuildProps` and `ImportDirectoryBuildTargets` to `false`.
Also implicitly imports `Microsoft.NET.Sdk` (the default SDK).

```cs
#:sdk Isolated.NET.Sdk
```

#### [Isolated.Sdk](https://www.nuget.org/packages/Isolated.Sdk)

A bare isolation SDK. Import any SDK you want afterwards.

```cs
#:sdk Isolated.Sdk
// use any SDK you want:
#:sdk ...
```

## Release

```powershell
$version='<the next version here (without v prefix)>'
dotnet pack -p:PackageVersion=$version

# authenticate to nuget.org (only needed once)
winget install microsoft.nuget
nuget setapikey '<api key here>' -source https://api.nuget.org/v3/index.json

dotnet nuget push artifacts/package/release/Isolated.Sdk.$version.nupkg --source https://api.nuget.org/v3/index.json
dotnet nuget push artifacts/package/release/Isolated.NET.Sdk.$version.nupkg --source https://api.nuget.org/v3/index.json
git tag v$version && git push origin v$version
```
