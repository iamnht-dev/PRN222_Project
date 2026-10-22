#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
$serviceName = 'ThaiNH_SE1905_WS'
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if (-not $service) {
    Write-Host "Service $serviceName is not installed."
    return
}
if ($service.Status -ne 'Stopped') {
    Stop-Service -Name $serviceName
    $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(20))
}
& sc.exe delete $serviceName
if ($LASTEXITCODE -ne 0) { throw 'Windows could not remove the service registration.' }
Write-Host 'Service registration removed. Published files, text output and Event Log entries are preserved.'
