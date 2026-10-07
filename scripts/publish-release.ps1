param(
    [string]$Repository = "UAKrogan/WWP.Ukrainian",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$BuildScript = Join-Path $PSScriptRoot "build-release.cmd"
$ZipPath = Join-Path $RepoRoot "dist\WWP.Ukrainian_v$Version.zip"
$NotesPath = Join-Path $RepoRoot "RELEASE_NOTES.md"
$Tag = "v$Version"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is not installed. Install it with: winget install --id GitHub.cli"
}

gh auth status

if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

if (-not (Test-Path $BuildScript)) {
    throw "Build script not found: $BuildScript"
}

if (-not (Test-Path $NotesPath)) {
    throw "Release notes file not found: $NotesPath"
}

Write-Host ""
Write-Host "Building release package..."

& $BuildScript

if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path $ZipPath)) {
    throw "Release archive not found: $ZipPath"
}

Write-Host ""
Write-Host "Creating GitHub release $Tag in $Repository..."

gh release create $Tag $ZipPath `
    --repo $Repository `
    --target master `
    --title "Wild West Pioneers Ukrainian v$Version" `
    --notes-file $NotesPath `
    --latest

if ($LASTEXITCODE -ne 0) {
    throw "GitHub release creation failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "GitHub release created successfully:"
Write-Host "https://github.com/$Repository/releases/tag/$Tag"
