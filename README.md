[![.NET Deploy](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-deploy.yml)
[![.NET Unit Test](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml/badge.svg?branch=main)](https://github.com/terellison/NVUpdateManager/actions/workflows/dotnet-unit-test.yml)
# NVUpdateManager
Windows application suite for managing NVIDIA Game Ready Driver updates without using GeForce Experience. Intended for enterprise environments who have multiple machines running NVIDIA GPUs or individuals who don't want to run GeForce Experience.

## Example update notification

<p><p>Version: 552.44</p><p>Release Date: 2024.5.9</p><p>Download Link: https://us.download.nvidia.com/Windows/552.44/552.44-desktop-win10-win11-64bit-international-dch-whql.exe</p><p>Details:

<b>Game Ready for Ghost of Tsushima: Director’s Cut</b><br> <br> This new Game Ready Driver provides the best gaming experience for the latest new games supporting DLSS 3 technology including Ghost of Tsushima: Director’s Cut. Further support for new titles leveraging NVIDIA DLSS technology includes the launch of Homeworld 3 which supports DLSS Super Resolution.<br> <br> <b>Fixed Gaming Bugs</b><br> <br> <ul>   <li>Ghost of Tsushima DIRECTOR'S CUT: Resizable BAR profile [4597841]</li> </ul>     <br>     <a href="https://www.nvidia.com/en-us/geforce/news/ghost-of-tsushima-geforce-game-ready-driver"> Learn more in our Game Ready Driver article here. </a><br>     <br>     <p align="justify"> <img title="Game Ready Driver" alt="Game Ready Driver" src="https://images.nvidia.com/content/GRD/R550GA6.5/ghost-of-tsushima-geforce-game-ready-driver-gfe-grd-1144x298-banner.jpg" width="100%"> </p></p></p>

## Components

### NotificationService
Checks the currently installed driver version and asks NVIDIA whether a newer one is available.
If it finds one, it tells you. Runs as a scheduled task rather than a resident service, so it
costs nothing between checks.

- Installation instructions
  -   Download and unzip [NVUpdateManager.NotificationService.zip](https://github.com/terellison/NVUpdateManager/releases/latest/download/NVUpdateManager.NotificationService.zip)
  -   Run `NVUpdateManager.NotificationService.Installer.msi`
  -   Schedule it to run as often as you would like updates checked

There is nothing to configure. There is also no list of supported GPUs to maintain: the service
identifies the installed GPU and looks it up against NVIDIA's catalogue, so any GPU NVIDIA
publishes drivers for is supported, including ones released after your installed version.

## Notifications

With no configuration the service shows a Windows desktop notification. Azure is not required,
and neither is anything else.

Notification channels are selected in `appsettings.json`. Leave `Channels` empty and every
channel that has what it needs is used, which on a fresh install means the desktop notification.

| Channel | Setup | Use it when |
| --- | --- | --- |
| `Toast` | none | You are at the machine. The default. |
| `Smtp` | mail host and account | You want email, or the machine is headless or remote. |
| `LogicApp` | an Azure Logic App | You already run one. Kept for existing installations. |

### Email without Azure

Any mail account works. For Gmail or Outlook this needs an [app password](https://support.google.com/accounts/answer/185833),
not the account password:

```json
"Notifications": {
  "Channels": [ "Smtp" ],
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseStartTls": true,
    "Username": "you@gmail.com",
    "Password": "your-app-password",
    "To": "you@gmail.com"
  }
}
```

`From` defaults to `Username` and `To` defaults to `From`, so notifying yourself needs neither.

Channels are independent: listing more than one sends through all of them, and one failing is
logged without stopping the others or failing the update check.

A note on why this cannot be entirely automatic. Sending internet email requires either an
account to send through or a third-party sending service — there is no credential-free path,
because that is precisely what the anti-spam machinery of email exists to prevent. The desktop
notification is what genuinely needs no setup; SMTP is the smallest possible amount of it.

### Using the Azure Logic App

Still supported, and existing `EmailConfiguration` settings are read as before, so upgrading
does not stop the mail. You can create a Logic App using this
[tutorial](https://learn.microsoft.com/en-us/azure/app-service/tutorial-send-email?tabs=dotnet).
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

Then point the service at it:

```json
"Notifications": {
  "Channels": [ "LogicApp" ],
  "LogicApp": {
    "EncryptedAzLogicAppEndpoint": "<encrypted endpoint>",
    "Entropy": "<entropy>",
    "NotificationAddress": "<email-address>"
  }
}
```

Encrypt the endpoint with `NVUpdateManager.NotificationService.exe /EncryptEndpoint "your-endpoint-here"`.

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


