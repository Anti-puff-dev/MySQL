using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json.Utils
{
    public class Serialize
    {
        public static string SqlVars(Models.KeyValueList p)
        {
            string re = "";
            foreach (Models.Pair arr in p)
            {
                Debug.WriteLine("SET @" + arr.key + " := '" + arr.value + "';");
                re += "SET @" + arr.key + " := '" + arr.value + "';";
            }
            return re;
        }

        public static Models.KeyValueList JsonToKeyValueList(Newtonsoft.Json.Linq.JObject data)
        {
            Models.KeyValueList kvl = new Models.KeyValueList();

            foreach (var item in data)
            {
                //Debug.WriteLine(item.Key + " " + item.Value);
                kvl.Add(item.Key, item.Value.ToString());
            }

            return kvl;
        }


        public static Models.KeyValueList JsonToKeyValueList(Newtonsoft.Json.Linq.JArray data)
        {
            Models.KeyValueList kvl = new Models.KeyValueList();

            foreach (var item in data)
            {
                Newtonsoft.Json.Linq.JObject o = item.ToObject<Newtonsoft.Json.Linq.JObject>();
                //Debug.WriteLine(o["key"] + " " + o["value"]);
                kvl.Add(o["key"].ToString(), o["value"].ToString());
            }

            return kvl;
        }



        public static T GROUP_CONCAT_SINGLE<T>(string raw, string[] fields, char[] field_separator)
        {
            object p = new object();
            System.Type t = typeof(T);
            PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = t.GetConstructor(System.Type.EmptyTypes);
            object[] attrs;
            MySQL.FieldAttributes attr;

            p = Activator.CreateInstance(t);


            //Objectfy ---------------------------------------------------------------------------------------

            string[] _fields = raw.Split(field_separator);

            for (int j=0; j<fields.Length;j++)
            {
                foreach (PropertyInfo fieldInfo in propInfos)
                {
                    if (fieldInfo.Name == fields[j])
                    {
                        try
                        {
                            fieldInfo.SetValue(p, (String)_fields[j]);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                fieldInfo.SetValue(p, Convert.ChangeType(_fields[j], fieldInfo.PropertyType));
                            }
                            catch (Exception er)
                            {

                            }
                        }
                    }
                }
            }
            
            //Objectfy ---------------------------------------------------------------------------------------


            return (T)p;
        }


        public static T[] GROUP_CONCAT_MULTIPLE<T>(string raw, string[] fields, char[] row_separator, char[] field_separator)
        {
            string[] rows = raw.Split(row_separator);
            System.Type t = typeof(T);

            T[] units = new T[rows.Length];
            object[] p = new object[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                p[i] = Activator.CreateInstance(t);

                PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                ConstructorInfo mConstructor = t.GetConstructor(System.Type.EmptyTypes);

                string[] _fields = rows[i].Split(field_separator);

                for (int j = 0; j < _fields.Length; j++)
                {
                    foreach (PropertyInfo fieldInfo in propInfos)
                    {
                        if (fieldInfo.Name == fields[j])
                        {
                            try
                            {
                                fieldInfo.SetValue(p[i], (String)_fields[j]);
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    fieldInfo.SetValue(p[i], Convert.ChangeType(_fields[j], fieldInfo.PropertyType));
                                }
                                catch (Exception er)
                                {

                                }
                            }
                        }
                    }
                }
                units[i] = (T)p[i];
            }

            p = null;
            return (T[])units;
        }
    }
}
