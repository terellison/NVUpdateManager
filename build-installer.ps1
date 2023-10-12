$devenv_location = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" '-property' productPath
$devenv_location = $devenv_location.replace('exe','com')
echo "DEVENV_LOCATION=$devenv_location"
$args = ".\NVUpdateManager.sln /Deploy Release /Project `"NVUpdateManager.NotificationService.Installer\NVUpdateManager.NotificationService.Installer.wixproj`"".Split(" ")
Write-Host running '& "$devenv_location" $args'
& "$devenv_location" $args