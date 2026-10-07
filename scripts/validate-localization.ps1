$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$LocalizationDir = Join-Path $RepoRoot "localization\uk-UA"

$ExpectedDomains = @(
    "AUDIO_FILES",
    "Achievement",
    "Assets",
    "BUILDINGS_UI",
    "Brothel_UIandQuests",
    "Buildings",
    "Character_Progression",
    "Characters",
    "ConditionQuests",
    "ConditionQuests_EA",
    "ConditionQuests_New",
    "CutScenes",
    "DECORATIONS_UI",
    "Dialogue",
    "Dioramas_EA",
    "FAQ",
    "Gameplay_Effects",
    "Journey",
    "Keybind_Keys",
    "MAINMENU_UI",
    "MiniGame_UI",
    "Quests_EA",
    "Quests_New",
    "Resources",
    "StartAndEndOfDay",
    "StrategicMap_Events",
    "StrategicMap_Locations",
    "StrategicMap_UI",
    "Technology",
    "Technology_EA",
    "Tooltips",
    "TownHall_Milestones",
    "Town_Hall_Officials",
    "Tutorial_Quests_New",
    "UI",
    "UI_BottomBar",
    "UI_Buildings",
    "UI_InGameConsole",
    "UI_Satisfaction"
)

$ExpectedFileCount = 39
$ExpectedKeyCount = 7036

if (-not (Test-Path $LocalizationDir)) {
    throw "Localization directory not found: $LocalizationDir"
}

$Files = @(Get-ChildItem -Path $LocalizationDir -File -Filter "*.json" | Sort-Object Name)

if ($Files.Count -ne $ExpectedFileCount) {
    throw "Expected $ExpectedFileCount JSON files, found $($Files.Count)."
}

$ActualDomains = @($Files | ForEach-Object { $_.BaseName })
$MissingDomains = @($ExpectedDomains | Where-Object { $_ -notin $ActualDomains })
$ExtraDomains = @($ActualDomains | Where-Object { $_ -notin $ExpectedDomains })

if ($MissingDomains.Count -gt 0) {
    throw "Missing domains: $($MissingDomains -join ', ')"
}

if ($ExtraDomains.Count -gt 0) {
    throw "Unexpected domains: $($ExtraDomains -join ', ')"
}

$SeenKeys = @{}
$TotalKeys = 0

foreach ($File in $Files) {
    $Domain = $File.BaseName
    $RawJson = Get-Content -Raw -Encoding UTF8 $File.FullName

    try {
        $Data = $RawJson | ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in $($File.Name): $($_.Exception.Message)"
    }

    if ($null -eq $Data) {
        throw "Localization file contains no data: $($File.Name)"
    }

    $Properties = @($Data.PSObject.Properties)

    foreach ($Property in $Properties) {
        $Key = [string]$Property.Name
        $Value = $Property.Value

        if ([string]::IsNullOrWhiteSpace($Key)) {
            throw "Empty localization key in $($File.Name)"
        }

        if ($null -eq $Value) {
            throw "Null localization value in $($File.Name): $Key"
        }

        if (-not ($Value -is [string])) {
            throw "Localization value is not a string in $($File.Name): $Key"
        }

        $SeparatorIndex = $Key.IndexOf("/")

        if ($SeparatorIndex -le 0) {
            throw "Invalid localization key in $($File.Name): $Key"
        }

        $KeyDomain = $Key.Substring(0, $SeparatorIndex)

        if ($KeyDomain -cne $Domain) {
            throw "Domain mismatch in $($File.Name): $Key"
        }

        if ($SeenKeys.ContainsKey($Key)) {
            throw "Duplicate localization key across files: $Key"
        }

        if ([string]$Value -match "[—–]") {
            throw "Long dash found in $($File.Name): $Key"
        }

        $SeenKeys[$Key] = $true
        $TotalKeys++
    }
}

if ($TotalKeys -ne $ExpectedKeyCount) {
    throw "Expected $ExpectedKeyCount localization keys, found $TotalKeys."
}

Write-Host ""
Write-Host "Localization validation passed."
Write-Host "Files: $($Files.Count)"
Write-Host "Keys: $TotalKeys"
Write-Host "PowerShell: $($PSVersionTable.PSVersion)"
