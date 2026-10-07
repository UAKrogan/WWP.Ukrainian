param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$LibDir = Join-Path $RepoRoot "lib"

$CoreDir = Join-Path $GameDir "BepInEx\core"
$InteropDir = Join-Path $GameDir "BepInEx\interop"

if (-not (Test-Path $GameDir)) {
    throw "Game directory not found: $GameDir"
}

if (-not (Test-Path $CoreDir)) {
    throw "BepInEx core directory not found: $CoreDir"
}

if (-not (Test-Path $InteropDir)) {
    throw "BepInEx interop directory not found: $InteropDir. Start the game with BepInEx at least once first."
}

$References = @(
    @{ Source = Join-Path $CoreDir "BepInEx.Core.dll"; Name = "BepInEx.Core.dll" },
    @{ Source = Join-Path $CoreDir "BepInEx.Unity.IL2CPP.dll"; Name = "BepInEx.Unity.IL2CPP.dll" },
    @{ Source = Join-Path $CoreDir "Il2CppInterop.Runtime.dll"; Name = "Il2CppInterop.Runtime.dll" },
    @{ Source = Join-Path $InteropDir "Il2Cppmscorlib.dll"; Name = "Il2Cppmscorlib.dll" },
    @{ Source = Join-Path $InteropDir "UnityEngine.CoreModule.dll"; Name = "UnityEngine.CoreModule.dll" },
    @{ Source = Join-Path $InteropDir "UnityEngine.UI.dll"; Name = "UnityEngine.UI.dll" },
    @{ Source = Join-Path $InteropDir "Unity.TextMeshPro.dll"; Name = "Unity.TextMeshPro.dll" },
    @{ Source = Join-Path $InteropDir "I2Loc.dll"; Name = "I2Loc.dll" },
    @{ Source = Join-Path $InteropDir "Core.dll"; Name = "Core.dll" },
    @{ Source = Join-Path $InteropDir "UI.dll"; Name = "UI.dll" }
)

$Missing = @($References | Where-Object { -not (Test-Path $_.Source) })

if ($Missing.Count -gt 0) {
    $MissingPaths = $Missing | ForEach-Object { $_.Source }
    throw "Required build references are missing:`n$($MissingPaths -join "`n")"
}

if (Test-Path $LibDir) {
    Remove-Item $LibDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $LibDir | Out-Null

foreach ($Reference in $References) {
    Copy-Item $Reference.Source (Join-Path $LibDir $Reference.Name)
}

Write-Host ""
Write-Host "Development references prepared."
Write-Host "Target: $LibDir"
Write-Host "Files: $($References.Count)"
