using System;

namespace MySQL.Json.Utils
{
    public class Exists
    {
        public static string String(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<string>();
            }
            catch (Exception err)
            {
                return "";
            }
        }

        public static int? Integer(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<int>();
            }
            catch (Exception err)
            {
                return null;
            }
        }

        public static double? Double(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<double>();
            }
            catch (Exception err)
            {
                return null;
            }
        }


        public static Newtonsoft.Json.Linq.JObject JObject(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<Newtonsoft.Json.Linq.JObject>();
            }
            catch (Exception err)
            {
                return null;
            }
        }

        public static Newtonsoft.Json.Linq.JArray JArray(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<Newtonsoft.Json.Linq.JArray>();
            }
            catch (Exception err)
            {
                return null;
            }
        }


        public static bool Boolean(Newtonsoft.Json.Linq.JObject o, string name)
        {
            try
            {
                return o.GetValue(name).ToObject<bool>();
            }
            catch (Exception err)
            {
                return false;
            }
        }


        public static string Date(Newtonsoft.Json.Linq.JObject o, string name, string format = "dd/MM/yyyy")
        {
            try
            {
                string d = o.GetValue(name).ToObject<string>();
                DateTime dt = Convert.ToDateTime(d.Replace("\\",""));
                return dt.ToString(format);
            }
            catch (Exception err)
            {
                return "";
            }
        }

    }
}
