[![.NET Deploy](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml)
[![.NET Unit Test](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml)
# NVUpdateManager
Windows application suite for managing NVIDIA Game Ready Driver updates without using GeForce Experience. Intended for enterprise environments who have multiple machines running NVIDIA GPUs or individuals who don't want to run GeForce Experience.

## Componenets

### NotificationService
Windows background service that checks the currently installed Game Ready Driver version and checks the NVIDIA driver search page for a newer version. If it finds an available update, it uses an Azure Logic App to send an email to a user (configured in `appsettings.json`)

You can create your own Logic App using this [tutorial](https://learn.microsoft.com/en-us/azure/app-service/tutorial-send-email?tabs=dotnet).
Use this as the sample payload:

```json
{
    "emailbody": "<description>",
    "to": "<email-address>",
    "from": "<email-address>",
    "priority": "<description>",
    "subject": "<description>"
}
```

- Installation instructions
  -   Download and unzip [NVUpdateManager.NotificationService.zip](https://github.com/terellison/NVUpdateManager/releases/latest/download/NVUpdateManager.NotificationService.zip)
  -   Run `NVUpdateManager.NotificationService.Installer.msi`
  -   Configure the drivers you want to search for in `C:\Program Files\NVUpdateManager.NotificationService\appsettings.json`
  -   Start the service in the Windows Services manager

## Functionality in development
- Installing updates automatically
- Batch update installation
- Desktop notifications
- Windows desktop app


