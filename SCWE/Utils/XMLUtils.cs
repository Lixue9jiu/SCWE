using System.Linq;
using System.Xml.Linq;

namespace SCWE.Utils
{
    public static class XMLUtils
    {
        public static XElement FindValuesByName(XElement elem, string name)
        {
            try
            {
                return elem.Elements("Values").FirstOrDefault(e => e.Attribute("Name").Value == name);
            }
            catch
            {
                throw new System.Exception(string.Format("xml error when finding {0} in {1}", name, elem.Name));
            }
        }

        public static string FindValueByName(XElement elem, string name)
        {
            try
            {
                return elem.Elements("Value").First(e => e.Attribute("Name").Value == name).Attribute("Value").Value;
            }
            catch
            {
                throw new System.Exception(string.Format("xml error when finding {0} in {1}", name, elem.Name));
            }
        }

        public static void GetValueOrDefault<T>(this XElement elem, string name, out T value)
        {
            try
            {
                value = GetValue<T>(elem, name);
            }
            catch
            {
                value = default;
            }
        }

        public static void GetValueOrDefault<T>(this XElement elem, string name, out T value, T def)
        {
            try
            {
                value = GetValue<T>(elem, name);
            }
            catch
            {
                value = def;
            }
        }

        public static void GetValue<T>(this XElement elem, string name, out T value)
        {
            value = GetValue<T>(elem, name);
        }

        public static T GetValue<T>(this XElement elem, string name)
        {
            return (T)Convert(FindValueByName(elem, name), typeof(T));
        }

        public static XElement GetValues(this XElement elem, string name)
        {
            return FindValuesByName(elem, name);
        }

        static object Convert(string value, System.Type type)
        {
            if (type == typeof(Vector3))
            {
                string[] strs = value.Split(',');
                return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
            }
            return System.Convert.ChangeType(value, type);
        }
    }

}
