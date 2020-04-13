using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DePeuter.Shared.Helpers
{
    public class IniParser
    {
        private Hashtable keyPairs = new Hashtable();
        private string iniFilePath;
        private const string ROOT_SECTION = "";

        private struct SectionPair
        {
            public string Section;
            public string Key;
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public IniParser(string iniPath)
        {
            TextReader iniFile = null;
            string strLine = null;
            string currentSection = null;
            string[] keyPair = null;

            iniFilePath = iniPath;

            if(File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath);

                    strLine = iniFile.ReadLine();

                    while(strLine != null)
                    {
                        strLine = strLine.Trim();

                        if(strLine != string.Empty)
                        {
                            if(strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentSection = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                keyPair = strLine.Split(new char[] { '=' }, 2);

                                SectionPair sectionPair;
                                string value = null;

                                sectionPair.Section = (currentSection ?? ROOT_SECTION).ToUpper();
                                sectionPair.Key = (keyPair[0]).ToUpper();

                                if(keyPair.Length > 1)
                                    value = keyPair[1];

                                keyPairs.Add(sectionPair, value);
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }

                }
                finally
                {
                    if(iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
        }

        public bool? GetBooleanValue(string section, string key)
        {
            var value = GetSetting(section, key);
            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            return bool.Parse(value.ToLower());
        }
        public int? GetIntegerValue(string section, string key)
        {
            var value = GetSetting(section, key);
            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            return int.Parse(value);
        }
        public double? GetDoubleValue(string section, string key, CultureInfo ci = null)
        {
            var value = GetSetting(section, key);
            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            return double.Parse(value, ci ?? CultureInfo.InvariantCulture);
        }
        public DateTime? GetDateTimeValue(string section, string key, CultureInfo ci = null)
        {
            var value = GetSetting(section, key);
            if(string.IsNullOrEmpty(value))
            {
                return null;
            }

            return DateTime.Parse(value, ci ?? CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="section">Section name.</param>
        /// <param name="key">Key name.</param>
        public string GetSetting(string section, string key)
        {
            SectionPair sectionPair;
            sectionPair.Section = (section ?? ROOT_SECTION).ToUpper();
            sectionPair.Key = key.ToUpper();

            return (string)keyPairs[sectionPair];
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public string[] EnumSection(string sectionName)
        {
            var tmpArray = new ArrayList();

            foreach(SectionPair pair in keyPairs.Keys)
            {
                if(pair.Section == (sectionName ?? ROOT_SECTION).ToUpper())
                    tmpArray.Add(pair.Key);
            }

            return (string[])tmpArray.ToArray(typeof(string));
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="section">Section to add under.</param>
        /// <param name="key">Key name to add.</param>
        /// <param name="value">Value of key.</param>
        public void AddSetting(string section, string key, string value)
        {
            SectionPair sectionPair;
            sectionPair.Section = (section ?? ROOT_SECTION).ToUpper();
            sectionPair.Key = key.ToUpper();

            if(keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, value);
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="section">Section to add under.</param>
        /// <param name="key">Key name to add.</param>
        public void AddSetting(string section, string key)
        {
            AddSetting(section, key, null);
        }

        /// <summary>
        /// Remove a setting.
        /// </summary>
        /// <param name="section">Section to add under.</param>
        /// <param name="key">Key name to add.</param>
        public void DeleteSetting(string section, string key)
        {
            SectionPair sectionPair;
            sectionPair.Section = (section ?? ROOT_SECTION).ToUpper();
            sectionPair.Key = key.ToUpper();

            if(keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        public void SaveSettings(string newFilePath = null)
        {
            var sections = new ArrayList();
            string tmpValue = "";
            string strToSave = "";

            foreach(SectionPair sectionPair in keyPairs.Keys)
            {
                if(!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach(string section in sections)
            {
                if(section != ROOT_SECTION)
                {
                    strToSave += ("[" + section + "]\r\n");
                }

                foreach(SectionPair sectionPair in keyPairs.Keys)
                {
                    if(sectionPair.Section == section)
                    {
                        tmpValue = (string)keyPairs[sectionPair];

                        if(tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }

                strToSave += "\r\n";
            }

            var tw = new StreamWriter(newFilePath ?? iniFilePath);
            tw.Write(strToSave);
            tw.Close();
        }
    }
}
