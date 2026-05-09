# FileBasedApps

MSBuild utilities for file-based apps.

## Include README.md when packing

Set `FileBasedAppsIncludeReadme` to `true` to include `README.md` from the app directory at the package root when packing.

```cs
#:package FileBasedApps@*
#:property FileBasedAppsIncludeReadme=true
```

Override `FileBasedAppsReadmeFile` to include a README from a different path.
