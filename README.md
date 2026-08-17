# FileBasedApps

Utilities for file-based apps.

## Packages

### [dn](https://www.nuget.org/packages/dn)

Run a file-based C# app with SDK discovery rooted at the app file while
preserving the caller's working directory for the app itself. The tool is
Native AOT, so starting `dn` does not consult `global.json` in that directory.

```powershell
dotnet tool install --global dn
dn file.cs arg0 arg1
```

### [FileBasedApps](https://www.nuget.org/packages/FileBasedApps)

MSBuild utilities for file-based apps.

Set `FileBasedAppsIncludeReadme` to `true` to include `README.md` from the app directory at the package root when packing.

```cs
#:package FileBasedApps@*
#:property FileBasedAppsIncludeReadme=true
```

Override `FileBasedAppsReadmeFile` to include a README from a different path.

### Isolated

MSBuild SDKs for isolating file-based apps from `Directory.Build.props`, `Directory.Packages.props`, and `Directory.Build.targets`.

#### [Isolated.NET.Sdk](https://www.nuget.org/packages/Isolated.NET.Sdk)

Sets `ImportDirectoryBuildProps` and `ImportDirectoryBuildTargets` to `false`.
Also implicitly imports `Microsoft.NET.Sdk` (the default SDK).

```cs
#:sdk Isolated.NET.Sdk@1.0.2
```

#### [Isolated.Sdk](https://www.nuget.org/packages/Isolated.Sdk)

A bare isolation SDK. Import any SDK you want afterwards.

```cs
#:sdk Isolated.Sdk@1.0.2
// use any SDK you want:
#:sdk ...
```

## Release

Run the **Release** workflow manually, enter the package version without a `v` prefix,
and select one or more package families. The workflow:

1. updates versioned README examples for the selected packages;
2. pushes a release commit when those examples changed;
3. packs the selected packages and runs the native package integration tests for `dn`;
4. publishes them to NuGet;
5. creates package-specific tags and GitHub releases, such as `dn-v1.0.0`.

### One-time release setup

1. Create a GitHub environment named `release`, optionally with required reviewers.
2. Add an environment secret named `NUGET_USER` containing the nuget.org username, not an email address.
3. At [nuget.org trusted publishing](https://www.nuget.org/account/trustedpublishing), add the corresponding policy.
4. Allow GitHub Actions to write repository contents. If `main` or tags are protected,
   allow this workflow to push its optional README commit and package-specific tags.
