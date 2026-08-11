# .NET 10.0 Upgrade Report

## Project target framework modifications

| Project name                                                   | Old Target Framework | New Target Framework | Commits                    |
|:---------------------------------------------------------------|:--------------------:|:--------------------:|:---------------------------|
| NVUpdateManager.Core\NVUpdateManager.Core.csproj               | net8.0               | net10.0              | 39171513                   |
| NVUpdateManager.EmailHandler\NVUpdateManager.EmailHandler.csproj | net8.0             | net10.0              | a2860e7a                   |
| NVUpdateManager.Web\NVUpdateManager.Web.csproj                 | net8.0               | net10.0              | e076dbeb                   |
| NVUpdateManager.EmailHandler.Tests\NVUpdateManager.EmailHandler.Tests.csproj | net8.0   | net10.0              | 24f8a6c6                   |
| NVUpdateManager.Core.Tests\NVUpdateManager.Core.Tests.csproj   | net8.0               | net10.0              | 3c2e009f                   |
| NVUpdateManager.NotificationService\NVUpdateManager.NotificationService.csproj | net8.0 | net10.0              | fc3a98c3                   |

## NuGet Packages

| Package Name                                          | Old Version | New Version | Commit Id |
|:------------------------------------------------------|:-----------:|:-----------:|:----------|
| AngleSharp                                            | 1.2.0       | 1.7.1       | 1946b5f3  |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.1       | 10.0.10     | 468628b6  |
| Microsoft.Extensions.Hosting                          | 9.0.1       | 10.0.10     | 0c5a1794  |
| Microsoft.Extensions.Http                             | 9.0.1       | 10.0.10     | 468628b6, 1946b5f3 |
| Microsoft.Windows.Compatibility                       | 9.0.1       | 10.0.10     | 468628b6  |
| MSTest.TestAdapter                                    | 3.7.1       | 4.3.3       | 84680f25, 87ca6465 |
| MSTest.TestFramework                                  | 3.7.1       | 4.3.3       | 84680f25, 87ca6465 |
| System.Security.Cryptography.ProtectedData            | 9.0.1       | 10.0.10     | daa8a5b1  |
| System.Text.Json                                      | 9.0.1       | removed     | 2df6c531  |

## All commits

| Commit ID | Description                                                              |
|:----------|:-------------------------------------------------------------------------|
| 16ade89b  | Commit upgrade plan                                                      |
| 39171513  | Update NVUpdateManager.Core.csproj to target .NET 10.0                   |
| 468628b6  | Update NuGet package versions in NVUpdateManager.Core.csproj             |
| 76c088db  | InitialUpgrade complete - validation                                     |
| a2860e7a  | Update target framework to net10.0 in EmailHandler.csproj                |
| daa8a5b1  | Update package versions in NVUpdateManager.EmailHandler.csproj           |
| 2df6c531  | Remove System.Text.Json package from EmailHandler.csproj                 |
| e076dbeb  | Update NVUpdateManager.Web.csproj to target .NET 10.0                    |
| 1946b5f3  | Update NuGet package versions in NVUpdateManager.Web.csproj              |
| 24f8a6c6  | Update target framework to net10.0 in EmailHandler.Tests.csproj          |
| 84680f25  | Update MSTest packages in NVUpdateManager.EmailHandler.Tests.csproj      |
| 3c2e009f  | Update NVUpdateManager.Core.Tests.csproj to net10.0                      |
| 87ca6465  | Update MSTest packages in NVUpdateManager.Core.Tests.csproj              |
| fc3a98c3  | Update target framework to net10.0 in NotificationService.csproj         |
| 0c5a1794  | Update Microsoft.Extensions.Hosting to 10.0.10 in NotificationService    |

## Project feature upgrades

### NVUpdateManager.EmailHandler

- System.Text.Json package was removed as it is included in the .NET 10.0 framework and no longer needed as a direct dependency.

## Next steps

- Review and test the application to ensure all functionality works correctly on .NET 10.0.
- Consider merging the `upgrade-to-NET10` branch into `main` after validation.
- The `nuget.config` file was added to use only public nuget.org feeds — keep or remove based on your needs.
