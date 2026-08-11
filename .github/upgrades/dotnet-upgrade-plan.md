# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade NVUpdateManager.Core\NVUpdateManager.Core.csproj
4. Upgrade NVUpdateManager.EmailHandler\NVUpdateManager.EmailHandler.csproj
5. Upgrade NVUpdateManager.Web\NVUpdateManager.Web.csproj
6. Upgrade NVUpdateManager.EmailHandler.Tests\NVUpdateManager.EmailHandler.Tests.csproj
7. Upgrade NVUpdateManager.Core.Tests\NVUpdateManager.Core.Tests.csproj
8. Upgrade NVUpdateManager.NotificationService\NVUpdateManager.NotificationService.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

No projects are excluded from this upgrade.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                          | Current Version | New Version | Description                          |
|:------------------------------------------------------|:---------------:|:-----------:|:-------------------------------------|
| AngleSharp                                            | 1.2.0           | 1.7.1       | Security vulnerability               |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |
| Microsoft.Extensions.Hosting                          | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |
| Microsoft.Extensions.Http                             | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |
| Microsoft.Windows.Compatibility                       | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |
| MSTest.TestAdapter                                    | 3.7.1           | 4.3.3       | Deprecated, replacement recommended  |
| MSTest.TestFramework                                  | 3.7.1           | 4.3.3       | Deprecated, replacement recommended  |
| System.Security.Cryptography.ProtectedData            | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |
| System.Text.Json                                      | 9.0.1           | 9.0.18      | Recommended for .NET 9.0             |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### NVUpdateManager.Core modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - Microsoft.Extensions.DependencyInjection.Abstractions should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)
  - Microsoft.Extensions.Http should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)
  - Microsoft.Windows.Compatibility should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)

#### NVUpdateManager.EmailHandler modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - System.Security.Cryptography.ProtectedData should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)
  - System.Text.Json should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)

#### NVUpdateManager.Web modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - AngleSharp should be updated from `1.2.0` to `1.7.1` (*security vulnerability*)
  - Microsoft.Extensions.Http should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)

#### NVUpdateManager.EmailHandler.Tests modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - MSTest.TestAdapter should be updated from `3.7.1` to `4.3.3` (*deprecated, replacement recommended*)
  - MSTest.TestFramework should be updated from `3.7.1` to `4.3.3` (*deprecated, replacement recommended*)

#### NVUpdateManager.Core.Tests modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - MSTest.TestAdapter should be updated from `3.7.1` to `4.3.3` (*deprecated, replacement recommended*)
  - MSTest.TestFramework should be updated from `3.7.1` to `4.3.3` (*deprecated, replacement recommended*)

#### NVUpdateManager.NotificationService modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting should be updated from `9.0.1` to `9.0.18` (*recommended for .NET 9.0*)
