$Version = Get-Item -Path VERSION.txt | Get-Content -Tail 1

$VersionPattern = [Regex]::new("(\d+).(\d+).(\d+).(\d+)")

$VersionNumbers = $VersionPattern.Matches($Version)
$VersionMajor = [int]$VersionNumbers.Groups[1].Value
$VersionMinor = [int]$VersionNumbers.Groups[2].Value
$VersionBuild = [int]$VersionNumbers.Groups[3].Value
$VersionRevision = [int]$VersionNumbers.Groups[4].Value

"Current version: $VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"

$Update = Read-Host "Increment version? (Y/N)"
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
$VersionRevision = $VersionRevision + 1
"New version: $VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"
"$VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision" | Out-File -FilePath VERSION.txt
}

$VersionFull = "$VersionMajor.$VersionMinor.$VersionBuild.$VersionRevision"

"Updating versions in code..."
(Get-Content "RandoWPF\data\SetupData.cs") `
    -replace 'public static string Version \{ get; set; \} = ".*";', "public static string Version { get; set; } = `"$VersionFull`";" |
Out-File "RandoWPF\data\SetupData.cs"

"Building FF12 Rando..."
Push-Location -Path "FF12Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location

"Building FF13 Rando..."
Push-Location -Path "FF13Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location

"Building FF13-2 Rando..."
Push-Location -Path "FF13_2Rando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location

"Building LR Rando..."
Push-Location -Path "LRRando"
Invoke-Expression ".\publish.ps1 $VersionFull Y Y"
Pop-Location

Read-Host -Prompt "Press Enter to exit"