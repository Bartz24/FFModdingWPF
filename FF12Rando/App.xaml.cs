using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;

namespace FF12Rando;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    static App()
    {
        AssemblyLoadContext.Default.Resolving += OnAssemblyResolving;
    }

    private static Assembly? OnAssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        string? assemblyPath = GetLibsAssemblyPath(assemblyName.Name);
        return assemblyPath != null ? context.LoadFromAssemblyPath(assemblyPath) : null;
    }

    private static string? GetLibsAssemblyPath(string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return null;

        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string libsDir = Path.Combine(appDir, "libs");
        string assemblyPath = Path.Combine(libsDir, $"{assemblyName}.dll");

        return File.Exists(assemblyPath) ? assemblyPath : null;
    }
}
