using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DePeuter.Shared.Managers
{
    public class Translation
    {
        public string Language { get; set; }
        public string Module { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
    }

    public interface ITranslator
    {
        string Get(string code);
        string Get(string module, string code);
        string Get(string language, string module, string code);
    }

    public interface ITranslationManager
    {
        List<string> GetLanguages();
        List<Translation> GetList(string language);
        Dictionary<string, Dictionary<string, string>> GetDictionary(string language);
        void SaveList(List<Translation> translations, bool append);
    }

    public class TranslationManager : ITranslationManager, ITranslator
    {
        private readonly string _resourcesDirectory;
        private readonly string _defaultLanguage;
        private readonly Func<string> _getCurrentLanguage;

        private const string RESOURCE_EXTENSION = ".resx";
        private const string DEFAULT_MODULE = "global";

        public string CurrentLanguage { get; set; }

        private static readonly object DataLock = new object();

        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _data;

        public TranslationManager(string resourcesDirectory, string defaultLanguage, string currentLanguage = null)
        {
            _resourcesDirectory = resourcesDirectory;
            _defaultLanguage = defaultLanguage;
            CurrentLanguage = currentLanguage;

            if(_data == null)
            {
                RefreshData();
            }
        }

        public TranslationManager(string resourcesDirectory, string defaultLanguage, Func<string> getCurrentLanguage)
            : this(resourcesDirectory, defaultLanguage)
        {
            _getCurrentLanguage = getCurrentLanguage;
        }

        private void RefreshData()
        {
            lock(DataLock)
            {
                var languages = GetLanguages();

                _data = languages.ToDictionary(x => x, GetDictionary);
            }
        }

        private string GetResourcePath(string language)
        {
            return Path.Combine(_resourcesDirectory, language + RESOURCE_EXTENSION);
        }

        private string GetResourceDataName(string module, string code)
        {
            return string.Format("{0}.{1}", (module ?? DEFAULT_MODULE).ToLower(), code);
        }

        public List<string> GetLanguages()
        {
            return Directory.GetFiles(_resourcesDirectory, "*" + RESOURCE_EXTENSION, SearchOption.TopDirectoryOnly).Select(x =>
            {
                var lang = new FileInfo(x).Name;
                return lang.Substring(0, lang.Length - RESOURCE_EXTENSION.Length);
            }).ToList();
        }

        public string Get(string code)
        {
            return Get(null, code);
        }

        public string Get(string module, string code)
        {
            var language = CurrentLanguage;

            if(language == null)
            {
                if(_getCurrentLanguage == null)
                {
                    throw new NullReferenceException("CurrentLanguage or getCurrentLanguage");
                }

                language = _getCurrentLanguage();
            }

            return Get(language, module, code);
        }

        public string Get(string language, string module, string code)
        {
            try
            {
                return _data[language][(module ?? DEFAULT_MODULE).ToLower()][code];
            }
            catch
            {
                return GetResourceDataName(module, code);
            }
        }

        public List<Translation> GetList(string language)
        {
            var languages = language != null ? new List<string>() { language } : GetLanguages();

            var result = new List<Translation>();

            foreach(var lang in languages)
            {
                var resourcePath = GetResourcePath(lang);
                var xdoc = XDocument.Load(resourcePath);

                if(xdoc.Root == null)
                {
                    return null;
                }

                result.AddRange(xdoc.Root.Elements("data").Select(dataElement =>
                {
                    var nameAttribute = dataElement.Attribute("name");
                    if(nameAttribute == null)
                    {
                        return null;
                    }

                    var name = nameAttribute.Value;
                    var nameData = name.Split('.');

                    var valueElement = dataElement.Element("value");
                    var value = valueElement != null ? valueElement.Value : null;

                    return new Translation()
                    {
                        Module = nameData[0].ToLower(),
                        Code = nameData[1],
                        Label = value,
                        Language = lang
                    };
                }).Where(x => x != null));
            }

            return result;
        }

        public Dictionary<string, Dictionary<string, string>> GetDictionary(string language)
        {
            var translations = GetList(language);
            var defaultTranslations = language != _defaultLanguage ? GetList(_defaultLanguage) : translations;

            var modules = defaultTranslations.Select(x => x.Module.ToLower()).Distinct().ToArray();

            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach(var module in modules)
            {
                var modulesDict = new Dictionary<string, string>();
                result.Add(module, modulesDict);

                var codes = defaultTranslations.Where(x => string.Equals(x.Module, module, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Code).Distinct().ToArray();
                foreach(var code in codes)
                {
                    var vertaling = translations.SingleOrDefault(x => string.Equals(x.Module, module, StringComparison.InvariantCultureIgnoreCase) && x.Code == code);
                    if(vertaling != null && !string.IsNullOrEmpty(vertaling.Label))
                    {
                        modulesDict.Add(code, vertaling.Label);
                    }
                    else
                    {
                        vertaling = defaultTranslations.SingleOrDefault(x => string.Equals(x.Module, module, StringComparison.InvariantCultureIgnoreCase) && x.Code == code);
                        if(vertaling != null && !string.IsNullOrEmpty(vertaling.Label))
                        {
                            modulesDict.Add(code, vertaling.Label);
                        }
                        else
                        {
                            modulesDict.Add(code, GetResourceDataName(module, code));
                        }
                    }
                }
            }

            return result;
        }

        public void SaveList(List<Translation> translations, bool append)
        {
            if(translations == null)
            {
                return;
            }

            var languages = translations.Where(x => x != null).Select(x => x.Language).Distinct().ToArray();

            foreach(var language in languages)
            {
                var resourcePath = GetResourcePath(language);

                var xdoc = XDocument.Load(resourcePath);

                if(xdoc.Root == null)
                {
                    return;
                }

                if(!append)
                {
                    xdoc.Root.Elements("data").Remove();
                }

                foreach(var vertaling in translations.Where(x => x != null).Where(x => string.Equals(x.Language, language, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var name = GetResourceDataName(vertaling.Module, vertaling.Code);

                    var dataElement = xdoc.XPathSelectElement("root/data[@name='" + name + "']");
                    if(dataElement == null)
                    {
                        dataElement = new XElement("data", new XAttribute("name", name), new XAttribute(XNamespace.Xml + "space", "preserve"), new XText(""));

                        xdoc.Root.Add(dataElement);
                    }

                    var valueElement = dataElement.Element("value");
                    if(valueElement == null)
                    {
                        valueElement = new XElement("value");
                        dataElement.Add(valueElement);
                    }

                    valueElement.Value = vertaling.Label;
                }

                xdoc.Save(resourcePath);
            }
        }

        public void DeleteList(List<Translation> translations)
        {
            if(translations == null)
            {
                return;
            }

            var languages = translations.Where(x => x != null).Select(x => x.Language).Distinct().ToArray();

            foreach(var language in languages)
            {
                var resourcePath = GetResourcePath(language);

                var xdoc = XDocument.Load(resourcePath);

                if(xdoc.Root == null)
                {
                    return;
                }
                
                foreach(var vertaling in translations.Where(x => x != null).Where(x => string.Equals(x.Language, language, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var name = GetResourceDataName(vertaling.Module, vertaling.Code);

                    var dataElement = xdoc.XPathSelectElement("root/data[@name='" + name + "']");
                    if(dataElement != null)
                    {
                        dataElement.Remove();
                    }
                }

                xdoc.Save(resourcePath);
            }
        }
    }
}
