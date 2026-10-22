#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
$serviceName = 'ThaiNH_SE1905_WS'
$executable = Join-Path $PSScriptRoot 'publish\zCoach.WorkerService.ThaiNH.exe'

if (-not (Test-Path -LiteralPath $executable)) {
    throw 'Publish the Worker project into the publish folder before installing the service.'
}
if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot 'publish\appsettings.Local.json'))) {
    throw 'Configure publish\appsettings.Local.json for your SQL Server before installing.'
}
if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
    throw "Service $serviceName already exists. Use Services to start/stop it; this script will not overwrite it."
}

# AddWindowsService writes to Windows Application Event Log while hosted by SCM.
if (-not [System.Diagnostics.EventLog]::SourceExists($serviceName)) {
    New-EventLog -LogName Application -Source $serviceName
}
New-Service -Name $serviceName -DisplayName $serviceName `
    -BinaryPathName ('"' + $executable + '"') -StartupType Manual `
    -Description 'PRN222 ASM04 ThaiNH - append Coach database snapshots to a text file every 5 seconds.' | Out-Null
Start-Service -Name $serviceName
$service = Get-Service -Name $serviceName
$service.WaitForStatus('Running', [TimeSpan]::FromSeconds(20))
Get-Service -Name $serviceName | Select-Object Name, Status, StartType
Write-Host ('Text file: ' + (Join-Path $PSScriptRoot 'publish\Logs\ThaiNH_SE1905_WS.txt'))
