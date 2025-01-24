$TempPackagesFolder = Join-Path  $PSScriptRoot "obj\packages"
$TempPublishFolder = Join-Path  $PSScriptRoot "obj\pub"
$TargetFolder = Join-Path  $PSScriptRoot "deployment"
$TargetStoreFolder = Join-Path  $TargetFolder "store"
$TargetDepsFolder = Join-Path  $TargetFolder "additionalDeps\shared\Microsoft.AspNetCore.App\8.0.0\"
$MsbuildFlags = @("-v", "q", "/nologo");

function RemoveManifestFromDeps ($depsLocation, $depsTarget) {
    $deps = Get-Content $depsLocation -Raw | ConvertFrom-Json
    # remove item from libraries
    $deps.libraries.PSObject.Properties.Remove("Serilog.Sinks.KineticSeqPlugIn.Store/1.0.0");
    # remove item from targets
    $deps.targets.PSObject.properties.Value.PSObject.Properties.Remove("Serilog.Sinks.KineticSeqPlugIn.Store/1.0.0");

    $deps | ConvertTo-Json -Depth 5 | Set-Content $depsTarget
}

if (Test-Path $TempPackagesFolder)
{
    Remove-Item $TempPackagesFolder -Recurse -Force
}

if (Test-Path $TempPublishFolder)
{
    Remove-Item $TempPublishFolder -Recurse -Force
}

if (!(Test-Path $TargetDepsFolder))
{
    mkdir $TargetDepsFolder | Out-Null;
}

Write-Host "Cleaning..."
dotnet clean ..\Serilog.Sinks.KineticSeqPlugIn\Serilog.Sinks.KineticSeqPlugIn.csproj $MsbuildFlags
dotnet clean Serilog.Sinks.KineticSeqPlugIn.Store.csproj $MsbuildFlags

Write-Host "Generating Serilog.Sinks.KineticSeqPlugIn package"
dotnet pack ..\Serilog.Sinks.KineticSeqPlugIn\Serilog.Sinks.KineticSeqPlugIn.csproj -o $TempPackagesFolder $MsbuildFlags

Write-Host "Generating runtime store for Serilog.Sinks.KineticSeqPlugIn at $TargetStoreFolder"
dotnet store -r win-x64 -o $TargetStoreFolder --manifest Serilog.Sinks.KineticSeqPlugIn.Store.csproj --skip-optimization $MsbuildFlags

Write-Host "Generating additionalDeps for Serilog.Sinks.KineticSeqPlugIn at $TargetDepsFolder"
dotnet publish Serilog.Sinks.KineticSeqPlugIn.Store.csproj -o $TempPublishFolder $MsbuildFlags
RemoveManifestFromDeps (Join-Path  $TempPublishFolder "Serilog.Sinks.KineticSeqPlugIn.Store.deps.json") "$TargetDepsFolder\Serilog.Sinks.KineticSeqPlugIn.deps.json"

Write-Host "Placing deploy.ps1 file to $TargetFolder"
Copy-Item deploy.ps1 $TargetFolder -Force
