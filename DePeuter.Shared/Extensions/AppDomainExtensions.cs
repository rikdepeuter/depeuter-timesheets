using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

public static class AppDomainExtensions
{
    public static void OnAllLoadedTypes(this AppDomain domain, Action<Type[]> onTypes)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
            RegisterAssembly(assembly, onTypes);
    }

    public static Type[] AllLoadedTypes(this AppDomain domain)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var res = new List<Type>();
        foreach(var assembly in assemblies)
            RegisterAssembly(assembly, res.AddRange);

        return res.ToArray();
    }

    public static void OnAllTypes(this AppDomain domain, Action<Type[]> onTypes)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
            RegisterAssembly(assembly, onTypes);

        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyLoad += (object sender, AssemblyLoadEventArgs args) => RegisterAssembly(args.LoadedAssembly, onTypes);
    }

    private static void RegisterAssembly(Assembly assembly, Action<Type[]> onTypes)
    {
        try
        {
            if (!assembly.FullName.StartsWith("Revit.IFC.Import"))
            {
                var types = assembly.GetTypes();
                onTypes(types);
            }
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var ex in e.LoaderExceptions)
            {
                //Log.Error(ex);
            }
            var types = e.Types.Where(t => t != null).ToArray();
            onTypes(types);
        }
        catch (Exception)
        {
            //Log.Error(ex);
        }
    }

    public static void LoadAllAssemblies(this AppDomain appdomain, IEnumerable<string> dllFolders = null, string[] ignoreDllFileNames = null)
    {
        var directories = new List<string>();
        
        if (dllFolders != null)
        {
            directories.AddRange(dllFolders);
        }

        if (!directories.Any())
        {
            directories.Add(appdomain.BaseDirectory);
        }

        foreach (var path in directories.Distinct())
        {
            var files = new DirectoryInfo(path).GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (var fi in files)
            {
                try
                {
                    if(ignoreDllFileNames != null && ignoreDllFileNames.Contains(fi.Name))
                        continue;

                    var a = AssemblyName.GetAssemblyName(fi.FullName);

                    if (!appdomain.GetAssemblies().Any(assembly => AssemblyName.ReferenceMatchesDefinition(a, assembly.GetName())))
                    {
                        Assembly.Load(a);
                    }
                }
                catch (Exception)
                {
                    //Log.Error(ex);
                }
            }
        }
    }
}