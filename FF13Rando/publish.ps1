$Version = $args[0]

$Version = $args[0]

$Update = $args[1]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    # No need to copy from Debug anymore - data is copied during build
    # Just clean up any unwanted files from bin\data if they exist
    Remove-Item -Recurse -Force "bin\data\RandoPaths.csv" -ErrorAction Ignore

    Write-Host "Updating versions in bin\data..."
    (Get-Content "bin\data\modpack\modconfig.ini") `
        -replace '_VERSION_', "$Version" |
    Out-File "bin\data\modpack\modconfig.ini"
    (Get-Content "bin\data\modpack\readme.txt") `
        -replace '_VERSION_', "$Version" |
    Out-File "bin\data\modpack\readme.txt"
}

$Update = $args[2]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    dotnet publish -c Release --output "bin\publish"

    # Remove the libs folder if it exists
    if (Test-Path -Path "bin\publish\libs") {
		Write-Host "Removing existing libs folder..."
		Remove-Item -Recurse -Force "bin\publish\libs" -ErrorAction Ignore
	}
    
    Write-Host "Organizing DLLs into libs folder..."
    New-Item -ItemType Directory -Force -Path "bin\publish\libs"
    Get-ChildItem -Path "bin\publish" -Filter "*.dll" | Where-Object { $_.Name -ne "FF13Rando.dll" } | Move-Item -Destination "bin\publish\libs" -Force

    Write-Host "Copying data to bin\publish..."

    Remove-Item -Recurse -Force "bin\publish\data" -ErrorAction Ignore
    Copy-Item -Path "bin\data\" -Destination "bin\publish\data" -Recurse -Force

    Write-Host "Creating 7z file..."
    Remove-Item -Recurse -Force "bin\publish\FF13Randomizer$Version.7z" -ErrorAction Ignore
    Push-Location -Path "bin\publish"
    & "7z.exe" a -t7z -mx=9 "FF13Randomizer$Version.7z" "data" "README.pdf" "FF13Rando.exe" "libs" "*.json" "FF13Rando.dll"
    Pop-Location

}
