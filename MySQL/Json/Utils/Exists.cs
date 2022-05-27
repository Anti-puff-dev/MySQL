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
                DateTime dt = Convert.ToDateTime(d.Replace("\\", ""));
                return dt.ToString(format);
            }
            catch (Exception err)
            {
                return "";
            }
        }



        public static string TreeString(Newtonsoft.Json.Linq.JObject o, string[] names)
        {
            try
            {
                int index = -1;
                Newtonsoft.Json.Linq.JObject jo = o;
                Newtonsoft.Json.Linq.JArray ja = null;

                for (int i=0; i<names.Length; i++)
                {
                    if(i == names.Length-1)
                    {
                        return jo.GetValue(names[i]).ToObject<string>();
                    }
                    else
                    {
                        if(names[i].IndexOf("[") > -1)
                        {
                            int p1 = names[i].IndexOf("[");
                            int p2 = names[i].IndexOf("]");

                            string _name = names[i].Substring(0, p1);
                            string _index = names[i].Substring(p1+1, p2-p1-1);

                            index = Convert.ToInt32(_index);
                            ja = jo.GetValue(_name).ToObject<Newtonsoft.Json.Linq.JArray>();
                        }
                        else
                        {
                            if(index == -1)
                            {
                                jo = jo.GetValue(names[i]).ToObject<Newtonsoft.Json.Linq.JObject>();
                            }
                            else
                            {
                                jo = (Newtonsoft.Json.Linq.JObject)ja[index];
                                jo = jo.GetValue(names[i]).ToObject<Newtonsoft.Json.Linq.JObject>();
                                index = -1;
                            } 
                        }
                        
                    }
                }

                return "";
            }
            catch (Exception err)
            {
                return "";
            }
        }
    }
}
