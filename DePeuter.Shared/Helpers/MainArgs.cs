using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DePeuter.Shared.Helpers;

namespace DePeuter.Shared
{
    public static class MainArgs
    {
        public static string KeyPrefix
        {
            get { return Helper.KeyPrefix; }
            set { Helper.KeyPrefix = value; }
        }

        private static readonly ArgsHelper Helper;

        static MainArgs()
        {
            Helper = new ArgsHelper(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        public static bool HasKey(string key)
        {
            return Helper.HasKey(key);
        }

        public static string GetValue(string key, string defaultValue = null, bool required = false)
        {
            return Helper.GetValue(key, defaultValue, required);
        }

        public static List<string> GetValues(string key, bool required = false)
        {
            return Helper.GetValues(key, required);
        }
    }
}
