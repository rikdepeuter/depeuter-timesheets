using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Helpers
{
    public class ArgsException : ValidationException
    {
        public ArgsException(string message)
            : base(message)
        {
        }
    }

    public class ArgsHelper
    {
        public string KeyPrefix { get; set; }
        public bool KeyIsCaseSensitive { get; set; }

        private readonly List<string> _args;

        public ArgsHelper(string[] args, string keyPrefix = "/")
        {
            _args = args != null ? args.ToList() : new List<string>();
            KeyPrefix = keyPrefix;
            KeyIsCaseSensitive = true;
        }

        public string[] ToArray()
        {
            return _args.ToArray();
        }

        private bool ArgEqualsKey(string arg, string key)
        {
            if(KeyIsCaseSensitive)
            {
                return arg == KeyPrefix + key;
            }
            
            return string.Equals(arg, KeyPrefix + key, StringComparison.InvariantCultureIgnoreCase);
        }
        private int FindKeyIndex(string key)
        {
            return _args.FindIndex(x => ArgEqualsKey(x, key));
        }

        public bool HasKey(string key)
        {
            return FindKeyIndex(key) != -1;
        }

        public string GetValue(string key, string defaultValue = null, bool required = false)
        {
            var index = FindKeyIndex(key);
            if(index == -1)
            {
                if(required)
                {
                    throw new ArgsException("Missing parameter " + KeyPrefix + key);
                }
                return defaultValue;
            }

            if(_args.Count == index + 1)
            {
                if(required)
                {
                    throw new ArgsException("Missing value for parameter " + KeyPrefix + key);
                }
                return defaultValue;
            }

            var value = _args[index + 1];

            if(value.StartsWith(KeyPrefix.ToString()))
            {
                return defaultValue;
            }

            return value;
        }

        public List<string> GetValues(string key, bool required = false)
        {
            var res = new List<string>();

            for(var i = 0; i < _args.Count; i++)
            {
                var arg = _args[i];

                if(ArgEqualsKey(arg, key))
                {
                    if(_args.Count == i + 1)
                    {
                        throw new ArgsException("Missing value for parameter " + KeyPrefix + key);
                    }

                    var value = _args[i + 1];

                    res.Add(value);
                }
            }

            if(required && !res.Any())
            {
                throw new ArgsException("Missing parameter " + KeyPrefix + key);
            }

            return res;
        }
    }
}
