dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\publish"

Read-Host -Prompt "Press Enter to exit"