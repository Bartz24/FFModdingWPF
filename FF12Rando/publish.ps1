$Version = $args[0]

$Update = $args[1]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    Write-Host "Updating bin\data from Debug..."
    Remove-Item -Recurse -Force "bin\data" -ErrorAction Ignore
    Copy-Item -Path "bin\Debug\net6.0-windows\data\" -Destination "bin\data" -Recurse -Force
    Remove-Item -Recurse -Force "bin\data\musicPacks" -ErrorAction Ignore
    Remove-Item -Recurse -Force "bin\data\tools" -ErrorAction Ignore
    Remove-Item -Recurse -Force "bin\data\RandoPaths.csv" -ErrorAction Ignore
}

$Update = $args[2]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\publish"

    Write-Host "Copying data to bin\publish..."

    Remove-Item -Recurse -Force "bin\publish\data" -ErrorAction Ignore
    Copy-Item -Path "bin\data\" -Destination "bin\publish\data" -Recurse -Force

    Write-Host "Creating 7z file..."
    Remove-Item -Recurse -Force "bin\publish\FF12Randomizer$Version.7z" -ErrorAction Ignore
    Push-Location -Path "bin\publish"
    & "7z.exe" a -t7z -mx=9 "FF12Randomizer$Version.7z" "data" "README.pdf" "FF12Rando.exe"
    Pop-Location

    Copy-Item -Path "bin\publish\FF12Randomizer$Version.7z" -Destination "bin\build\FF12RandomizerPreview.7z" -Force
}