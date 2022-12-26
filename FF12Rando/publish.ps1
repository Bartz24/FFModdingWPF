$Version = $args[0]

$Update = $args[1]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    Write-Host "Updating bin\data from Debug..."
    Remove-Item -Recurse -Force "bin\data" -ErrorAction Ignore
    Copy-Item -Path "bin\Debug\net5.0-windows\data\" -Destination "bin\data" -Recurse -Force
    Remove-Item -Recurse -Force "bin\data\musicPacks" -ErrorAction Ignore
    Remove-Item -Recurse -Force "bin\data\tools" -ErrorAction Ignore
}

$Update = $args[2]
if ( ($Update -eq "Y") -or ($Update -eq "y") )
{
    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\publish"

    Write-Host "Copying data to bin\publish..."

    Copy-Item -Path "bin\publish\FF12Rando.exe" -Destination "bin\publish\rando" -Force

    Remove-Item -Recurse -Force "bin\publish\rando\data" -ErrorAction Ignore
    Copy-Item -Path "bin\data\" -Destination "bin\publish\rando\data" -Recurse -Force

    Remove-Item -Recurse -Force "bin\publish\scripts" -ErrorAction Ignore
    Copy-Item -Path "bin\scripts\" -Destination "bin\publish\scripts" -Recurse -Force

    Remove-Item -Recurse -Force "bin\publish\fomod" -ErrorAction Ignore
    Copy-Item -Path "bin\fomod\" -Destination "bin\publish\fomod" -Recurse -Force

    Write-Host "Creating 7z file..."
    Remove-Item -Recurse -Force "bin\publish\FF12OpenWorldRando$Version.7z" -ErrorAction Ignore
    Push-Location -Path "bin\publish"
    & "7z.exe" a -t7z -mx=9 "FF12OpenWorldRando$Version.7z" "rando" "scripts" "fomod"
    Pop-Location

    Copy-Item -Path "bin\publish\FF12OpenWorldRando$Version.7z" -Destination "bin\build\FF12OpenWorldRandoPreview.7z" -Force
}