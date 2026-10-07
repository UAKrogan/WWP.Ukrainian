param(
    [string]$GameDir = "",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Version = "1.0.0"
$ExpectedLocalizationFiles = 39

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $RepoRoot "src\WWP.Ukrainian\WWP.Ukrainian.csproj"
$LocalizationDir = Join-Path $RepoRoot "localization\uk-UA"
$DistDir = Join-Path $RepoRoot "dist"
$StageDir = Join-Path $DistDir "stage"
$PluginDir = Join-Path $StageDir "BepInEx\plugins\WWP.Ukrainian"
$TargetLocalizationDir = Join-Path $PluginDir "uk-UA"
$ZipPath = Join-Path $DistDir "WWP.Ukrainian_v$Version.zip"

if ([string]::IsNullOrWhiteSpace($GameDir)) {
    if (-not [string]::IsNullOrWhiteSpace($env:WWP_GAME_DIR)) {
        $GameDir = $env:WWP_GAME_DIR
    }
    else {
        $GameDir = "D:\SteamLibrary\steamapps\common\Wild West Pioneers"
    }
}

if (-not (Test-Path $GameDir)) {
    throw "Game directory not found: $GameDir"
}

& (Join-Path $PSScriptRoot "validate-localization.ps1")

Write-Host "Building WWP.Ukrainian v$Version..."

dotnet build $Project -c $Configuration -p:GameDir="$GameDir"

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

$DllPath = Join-Path $RepoRoot "src\WWP.Ukrainian\bin\$Configuration\net6.0\WWP.Ukrainian.dll"

if (-not (Test-Path $DllPath)) {
    throw "Built DLL not found: $DllPath"
}

if (Test-Path $StageDir) {
    Remove-Item $StageDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $TargetLocalizationDir | Out-Null

Copy-Item $DllPath (Join-Path $PluginDir "WWP.Ukrainian.dll")
Copy-Item (Join-Path $LocalizationDir "*.json") $TargetLocalizationDir

if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path (Join-Path $StageDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
Remove-Item $StageDir -Recurse -Force

Write-Host ""
Write-Host "Release created:"
Write-Host $ZipPath
