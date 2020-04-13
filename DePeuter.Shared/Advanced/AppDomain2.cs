using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using log4net;

public class AppDomain2
{
    private static readonly ILog Log = LogManager.GetLogger("Global");

    public delegate void OnTypesLoadedHandler(Type[] types);
    private static event OnTypesLoadedHandler TypesLoaded;

    static AppDomain2()
    {
        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyLoad += (object sender, AssemblyLoadEventArgs args) =>
        {
            HandleAssembly(args.LoadedAssembly);
        };
    }

    private static void HandleAssembly(Assembly assembly)
    {
        try
        {
            var types = assembly.GetTypes();

            if (TypesLoaded != null)
                TypesLoaded(types);
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var ex in e.LoaderExceptions)
            {
                Log.Error(ex);
            }

            var types = e.Types.Where(t => t != null).ToArray();
            if (TypesLoaded != null)
                TypesLoaded(types);
        }
        catch (System.Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnLoadedTypes(OnTypesLoadedHandler onLoadedTypes)
    {
        TypesLoaded += onLoadedTypes;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var allLoadedTypes = assemblies.SelectMany(x => x.GetLoadedTypes()).ToArray();
        onLoadedTypes(allLoadedTypes);
    }
}