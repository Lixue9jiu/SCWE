using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SCWE.Windows
{
    public static class Language
    {
        const string DEFAULT_LANG = "en";

        public static Dictionary<string, string> default_lookup = new Dictionary<string, string>();

        public static Dictionary<string, string> str_lookup = new Dictionary<string, string>();

        public static string GetString(string key)
        {
            if (str_lookup.ContainsKey(key))
            {
                return str_lookup[key];
            }
            if (default_lookup.ContainsKey(key))
            {
                return default_lookup[key];
            }
            return key;
        }

        public static void Initialize(string langName)
        {
            using (var s = File.OpenRead("languages.xml"))
            {
                var e = XDocument.Load(s).Root;
                LoadStrings(e, langName, ref str_lookup);
                LoadStrings(e, DEFAULT_LANG, ref default_lookup);
            }
        }

        private static bool LoadStrings(XElement elem, string language, ref Dictionary<string, string> dict)
        {
            XElement lookup = elem.Elements("table").FirstOrDefault(e => e.Attribute("language").Value == language);
            if (lookup == null)
            {
                return false;
            }
            else
            {
                foreach (XElement str in lookup.Elements("string"))
                {
                    dict.Add(str.Attribute("key").Value, str.Value);
                }
                return true;
            }
        }
    }
}
