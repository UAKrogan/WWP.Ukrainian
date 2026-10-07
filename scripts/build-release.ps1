param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$Version = "1.0.0"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $RepoRoot "src\WWP.Ukrainian\WWP.Ukrainian.csproj"
$LocalizationDir = Join-Path $RepoRoot "localization\uk-UA"
$LibDir = Join-Path $RepoRoot "lib"
$DistDir = Join-Path $RepoRoot "dist"
$StageDir = Join-Path $DistDir "stage"
$PluginDir = Join-Path $StageDir "BepInEx\plugins\WWP.Ukrainian"
$TargetLocalizationDir = Join-Path $PluginDir "uk-UA"
$ZipPath = Join-Path $DistDir "WWP.Ukrainian_v$Version.zip"

$RequiredReferences = @(
    "BepInEx.Core.dll",
    "BepInEx.Unity.IL2CPP.dll",
    "Il2CppInterop.Runtime.dll",
    "Il2Cppmscorlib.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.UI.dll",
    "Unity.TextMeshPro.dll",
    "I2Loc.dll",
    "Core.dll",
    "UI.dll"
)

if (-not (Test-Path $LibDir)) {
    throw "Build references not found. Run scripts\setup-dev.ps1 first."
}

$MissingReferences = @(
    $RequiredReferences |
    Where-Object { -not (Test-Path (Join-Path $LibDir $_)) }
)

if ($MissingReferences.Count -gt 0) {
    throw "Missing build references in lib/: $($MissingReferences -join ', '). Run scripts\setup-dev.ps1 again."
}

& (Join-Path $PSScriptRoot "validate-localization.ps1")

Write-Host ""
Write-Host "Building WWP.Ukrainian v$Version..."

dotnet build $Project -c $Configuration

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

if (-not (Test-Path $DistDir)) {
    New-Item -ItemType Directory -Force -Path $DistDir | Out-Null
}

if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path (Join-Path $StageDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
Remove-Item $StageDir -Recurse -Force

Write-Host ""
Write-Host "Release created:"
Write-Host $ZipPath
