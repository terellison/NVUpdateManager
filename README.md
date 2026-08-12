[![.NET Deploy](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml)
[![.NET Unit Test](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml)
# NVUpdateManager
Windows application suite for managing NVIDIA Game Ready Driver updates without using GeForce Experience. Intended for enterprise environments who have multiple machines running NVIDIA GPUs or individuals who don't want to run GeForce Experience.

## Example update notification

<p><p>Version: 552.44</p><p>Release Date: 2024.5.9</p><p>Download Link: https://us.download.nvidia.com/Windows/552.44/552.44-desktop-win10-win11-64bit-international-dch-whql.exe</p><p>Details:

<b>Game Ready for Ghost of Tsushima: Director’s Cut</b><br> <br> This new Game Ready Driver provides the best gaming experience for the latest new games supporting DLSS 3 technology including Ghost of Tsushima: Director’s Cut. Further support for new titles leveraging NVIDIA DLSS technology includes the launch of Homeworld 3 which supports DLSS Super Resolution.<br> <br> <b>Fixed Gaming Bugs</b><br> <br> <ul>   <li>Ghost of Tsushima DIRECTOR'S CUT: Resizable BAR profile [4597841]</li> </ul>     <br>     <a href="https://www.nvidia.com/en-us/geforce/news/ghost-of-tsushima-geforce-game-ready-driver"> Learn more in our Game Ready Driver article here. </a><br>     <br>     <p align="justify"> <img title="Game Ready Driver" alt="Game Ready Driver" src="https://images.nvidia.com/content/GRD/R550GA6.5/ghost-of-tsushima-geforce-game-ready-driver-gfe-grd-1144x298-banner.jpg" width="100%"> </p></p></p>

## Components

### NotificationService
Windows background service that checks the currently installed driver version and asks NVIDIA whether a newer one is available. If it finds an available update, it uses an Azure Logic App to send an email to a user (configured in `appsettings.json`)

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
  -   Configure your email settings in `C:\Program Files\NVUpdateManager.NotificationService\appsettings.json`
  -   Start the service in the Windows Services manager

There is no list of supported GPUs to maintain. The service identifies the installed GPU and
looks it up against NVIDIA's catalogue, so any GPU NVIDIA publishes drivers for is supported,
including ones released after your installed version.

## How GPU detection works

NVIDIA's driver search is driven by numeric identifiers — a product series (`psid`) and a
product family (`pfid`) — that are not guessable and change with every product launch. Rather
than transcribing them into the source, the application reads them from the same endpoint the
website's own dropdowns call:

| Request | Returns |
| --- | --- |
| `lookupValueSearch.aspx?TypeID=1` | Product types (GeForce, RTX / Quadro, Data Center, …) |
| `lookupValueSearch.aspx?TypeID=2&ParentID=<type>` | Product series, with its `psid` |
| `lookupValueSearch.aspx?TypeID=3&ParentID=<psid>` | Products in that series, with each `pfid` |
| `lookupValueSearch.aspx?TypeID=4&ParentID=<psid>` | Operating systems offered, with each `osID` |

Walking that tree yields every GPU NVIDIA ships drivers for (currently ~960 across ~106
series). The result is cached under `%ProgramData%\NVUpdateManager\gpu-catalog.json` and
refreshed weekly, so the walk costs a few seconds roughly once a week rather than on every
check.

The adapter name reported by WMI is then matched against that catalogue. The two sources
disagree on spelling in ways the matcher normalises: Windows always prefixes names with
`NVIDIA`, while NVIDIA's catalogue only does so for recent products. Where a desktop and a
notebook GPU share a name — common before the Ampere generation, which introduced the
`Laptop GPU` suffix — the machine's chassis type decides which one applies.

Driver details come from NVIDIA's `AjaxDriverService` JSON endpoint, which returns the version,
release date, release notes, and download URL in a single request.

### Optional settings

Everything in `DriverSearchConfiguration` is optional:

```json
"DriverSearchConfiguration": {
  "Branch": "GameReady"
}
```

| Setting | Purpose |
| --- | --- |
| `Branch` | `GameReady` (default) or `Studio` |
| `ProductNameOverride` | The name NVIDIA lists your GPU under, if Windows reports a different one |
| `ProductSeriesId` / `ProductFamilyId` | Pin `psid` / `pfid` directly, bypassing the catalogue |


