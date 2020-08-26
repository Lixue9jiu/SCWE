using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SCWE.Windows
{
    public static class Language
    {
        public static Dictionary<string, string> str_lookup = new Dictionary<string, string>();

        public static string GetString(string key)
        {
            return str_lookup[key];
        }

        public static void Initialize(string langName)
        {
            using (var s = File.OpenRead("languages.xml"))
            {
                var e = XDocument.Load(s).Root;
                if (!LoadStrings(e, langName))
                {
                    LoadStrings(e, "en");
                }
            }
        }

        private static bool LoadStrings(XElement elem, string language)
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
                    str_lookup.Add(str.Attribute("key").Value, str.Value);
                }
                return true;
            }
        }
    }
}
