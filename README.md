[![.NET Deploy](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml)
[![.NET Unit Test](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml)
# NVUpdateManager
Windows application suite for managing NVIDIA Game Ready Driver updates without using GeForce Experience. Intended for enterprise environments who have multiple machines running NVIDIA GPUs or individuals who don't want to run GeForce Experience.

## Example update notification

<p><p>Version: 552.44</p><p>Release Date: 2024.5.9</p><p>Download Link: https://us.download.nvidia.com/Windows/552.44/552.44-desktop-win10-win11-64bit-international-dch-whql.exe</p><p>Details:

<b>Game Ready for Ghost of Tsushima: Director’s Cut</b><br> <br> This new Game Ready Driver provides the best gaming experience for the latest new games supporting DLSS 3 technology including Ghost of Tsushima: Director’s Cut. Further support for new titles leveraging NVIDIA DLSS technology includes the launch of Homeworld 3 which supports DLSS Super Resolution.<br> <br> <b>Fixed Gaming Bugs</b><br> <br> <ul>   <li>Ghost of Tsushima DIRECTOR'S CUT: Resizable BAR profile [4597841]</li> </ul>     <br>     <a href="https://www.nvidia.com/en-us/geforce/news/ghost-of-tsushima-geforce-game-ready-driver"> Learn more in our Game Ready Driver article here. </a><br>     <br>     <p align="justify"> <img title="Game Ready Driver" alt="Game Ready Driver" src="https://images.nvidia.com/content/GRD/R550GA6.5/ghost-of-tsushima-geforce-game-ready-driver-gfe-grd-1144x298-banner.jpg" width="100%"> </p></p></p>

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


