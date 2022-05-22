$Version = Read-Host "Enter the version"

$Update = Read-Host "Update bin\data? (Y/N)"
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    Write-Host "Updating bin\data from Debug..."
    Remove-Item -Recurse -Force "bin\data" -ErrorAction Ignore
    Copy-Item -Path "bin\Debug\net5.0-windows\data\" -Destination "bin\data" -Recurse -Force
    Remove-Item -Recurse -Force "bin\data\musicPacks" -ErrorAction Ignore
    Remove-Item -Recurse -Force "bin\data\tools" -ErrorAction Ignore
}

$Update = Read-Host "Publish and create 7z? (Y/N)"
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\publish"

    Write-Host "Copying data to bin\publish..."

    Copy-Item -Path "bin\publish\FF12Rando.exe" -Destination "bin\publish\rando" -Force

    Remove-Item -Recurse -Force "bin\publish\rando\data" -ErrorAction Ignore
    New-Item -Path "bin\publish\rando\data" -ItemType Directory
    Copy-Item -Path "bin\data\" -Destination "bin\publish\rando\data" -Recurse -Force

    Remove-Item -Recurse -Force "bin\publish\scripts" -ErrorAction Ignore
    New-Item -Path "bin\publish\scripts" -ItemType Directory
    Copy-Item -Path "bin\scripts\" -Destination "bin\publish\scripts" -Recurse -Force

    Remove-Item -Recurse -Force "bin\publish\fomod" -ErrorAction Ignore
    New-Item -Path "bin\publish\fomod" -ItemType Directory
    Copy-Item -Path "bin\fomod\" -Destination "bin\publish\fomod" -Recurse -Force

    Write-Host "Creating 7z file..."
    Remove-Item -Recurse -Force "bin\publish\FF12OpenWorldRando$Version.7z" -ErrorAction Ignore
    & "7z.exe" a -t7z "bin\publish\FF12OpenWorldRando$Version.7z" "bin\publish\rando" "bin\publish\scripts" "bin\publish\fomod"
}
Read-Host -Prompt "Press Enter to exit"