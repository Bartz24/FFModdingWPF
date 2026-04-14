param(
    [string]$Selection = "",
    [Nullable[bool]]$IncrementVersion = $null,
    [switch]$NoPause
)

if ([string]::IsNullOrWhiteSpace($Selection))
{
    "Select projects to build (xii,xiii,xiii2,lr) or 'all'"
    $Selection = Read-Host "Enter selection (leave blank for all)"
}

if ([string]::IsNullOrWhiteSpace($Selection)) { $Selection = "all" }
$Targets = $Selection.ToLower().Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ }

$Build12 = ($Targets -contains "xii") -or ($Targets -contains "all")
$Build13 = ($Targets -contains "xiii") -or ($Targets -contains "all")
$Build132 = ($Targets -contains "xiii2") -or ($Targets -contains "all")
$BuildLR = ($Targets -contains "lr") -or ($Targets -contains "all")

$Version = Get-Item -Path VERSION.txt | Get-Content -Tail 1

$VersionPattern = [Regex]::new("(\d+).(\d+).(\d+).(\d+)")

$VersionNumbers = $VersionPattern.Matches($Version)
$VersionMajor = [int]$VersionNumbers.Groups[1].Value
$VersionMinor = [int]$VersionNumbers.Groups[2].Value
$VersionBuild = [int]$VersionNumbers.Groups[3].Value
$VersionRevision = [int]$VersionNumbers.Groups[4].Value

"Current version: $VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"

if ($null -eq $IncrementVersion)
{
    $Update = Read-Host "Increment version? (Y/N)"
    $IncrementVersion = ($Update -eq "Y") -or ($Update -eq "y")
}

if ($IncrementVersion)
{
    $VersionRevision = $VersionRevision + 1
    "New version: $VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"
    Set-Content -Path VERSION.txt -Value "$VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"
}

$VersionFull = "$VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"

"Updating versions in code..."
(Get-Content "RandoWPF\data\SetupData.cs") `
    -replace 'public static string Version \{ get; set; \} = ".*";', "public static string Version { get; set; } = `"$VersionFull`";" |
Set-Content "RandoWPF\data\SetupData.cs"

if ( $Build12 )
{
"Building FF12 Rando..."
Push-Location -Path "FF12Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location
}

if ( $Build13 )
{
"Building FF13 Rando..."
Push-Location -Path "FF13Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location
}

if ( $Build132 )
{
"Building FF13-2 Rando..."
Push-Location -Path "FF13_2Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location
}

if ( $BuildLR )
{
"Building LR Rando..."
Push-Location -Path "LRRando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location
}

if (-not $NoPause)
{
    Read-Host -Prompt "Press Enter to exit"
}
