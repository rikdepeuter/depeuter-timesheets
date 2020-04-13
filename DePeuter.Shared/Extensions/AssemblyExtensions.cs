using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

public static class AssemblyExtensions
{
    public static Type[] GetLoadedTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).ToArray();
        }
        catch (Exception)
        {
            return new Type[] { };
        }
    }
}