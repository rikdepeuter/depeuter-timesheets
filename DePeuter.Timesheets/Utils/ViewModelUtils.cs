using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Utils
{
    public static class ViewModelUtils
    {
        private class CacheData
        {
            public Type[] ViewModelTypes;
            public Type[] ViewTypes;
        }

        private static readonly CacheData Data = new CacheData();

        private static Type[] ViewModelTypes
        {
            get
            {
                lock (Data)
                {
                    if (Data.ViewModelTypes == null)
                    {
                        var viewModelBaseType = typeof(ViewModelBase);
                        Data.ViewModelTypes = viewModelBaseType.Assembly.GetTypes().Where(x => viewModelBaseType.IsAssignableFrom(x) && !x.IsAbstract).ToArray();
                    }
                    return Data.ViewModelTypes;
                }
            }
        }
        private static Type[] ViewTypes
        {
            get
            {
                lock (Data)
                {
                    if (Data.ViewTypes == null)
                    {
                        var viewModelBaseType = typeof(ViewModelBase);
                        var userControlType = typeof(UserControl);
                        Data.ViewTypes = viewModelBaseType.Assembly.GetTypes().Where(x => userControlType.IsAssignableFrom(x) && !x.IsAbstract).ToArray();
                    }
                    return Data.ViewTypes;
                }
            }
        }

        private static readonly Dictionary<Type, Type> ViewModelViewMapping = new Dictionary<Type, Type>();

        private static string GetViewName(Type viewModelType)
        {
            if (!viewModelType.Name.EndsWith("ViewModel"))
            {
                return null;
            }

            return viewModelType.Name.Substring(0, viewModelType.Name.Length - "Model".Length);
        }
        public static Type GetViewType(Type viewModelType)
        {
            lock (ViewModelViewMapping)
            {
                if (!ViewModelViewMapping.ContainsKey(viewModelType))
                {
                    var viewTypeName = GetViewName(viewModelType);
                    if (viewTypeName == null)
                    {
                        throw new InvalidOperationException("Invalid ViewModel name");
                    }

                    var viewType = ViewTypes.FirstOrDefault(x => x.Name == viewTypeName);
                    if (viewType == null)
                    {
                        throw new InvalidOperationException("No View found");
                    }

                    ViewModelViewMapping.Add(viewModelType, viewType);
                }
                return ViewModelViewMapping[viewModelType];
            }
        }

        public static Type GetViewModelType(string typeName)
        {
            return ViewModelTypes.SingleOrDefault(x => x.Name == typeName);
        }
    }
}
