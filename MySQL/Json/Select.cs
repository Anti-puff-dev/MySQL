using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;

namespace MySQL.Json
{
    public class Select
    {
        private List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
        private DataSet ds { get; set; }
        private object obj { get; set; }
        private Models.KeyValueList types = new Models.KeyValueList();
        private CacheConfig cache { get; set; }
        private string query = "";
        private string[] qparms = new string[] { };

        public Select(DataSet ds)
        {
            this.ds = ds;
        }

        public Select(string query, string[] qparms)
        {
            this.query = query;
            this.qparms = qparms;
        }

        public static Select Fill(DataSet ds)
        {
            return new Select(ds);
        }

        public static Select Fill(string query, string[] qparms)
        {

            return new Select(query, qparms);
        }


        public static Select Auto<T>()
        {
            return Auto<T>("", new string[] { });
        }

        public static Select Auto<T>(string where, string[] parms)
        {
            DataSet ds1;
            Utils.QueryModel qm = Utils.QueryBuilder.SelectModel<T>();

            if (!String.IsNullOrEmpty(where))
            {
                ds1 = Data.Query("SELECT " + qm.fields + " FROM " + qm.tables + " WHERE " + where, parms);
            }
            else
            {
                ds1 = Data.Query("SELECT " + qm.fields + " FROM " + qm.tables);
            }

            return new Select(ds1);
        }

        public Select Types(Models.KeyValueList types)
        {
            this.types = types;
            return this;
        }

        public Select Types(string types)
        {
            //this.types = types;
            return this;
        }


        public Select Cache(string name, string frequency, bool force_update = false, string string_connection = "")
        {
            cache = new CacheConfig() { Name = name, StringConnection = string_connection, Frequency = frequency, ForceUpdate = force_update };
            return this;
        }

        public Newtonsoft.Json.Linq.JArray Multiple()
        {
            Newtonsoft.Json.Linq.JArray results = new Newtonsoft.Json.Linq.JArray();

            int i = 0;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Newtonsoft.Json.Linq.JObject o = new Newtonsoft.Json.Linq.JObject();
                foreach (DataColumn column in ds.Tables[0].Columns)
                {
                    try
                    {
                        if (types.Get(column.ColumnName) == "string" || String.IsNullOrEmpty(types.Get(column.ColumnName)))
                        {
                            o.Add(column.ColumnName, row[column.ColumnName].ToString());
                        }
                        else if (types.Get(column.ColumnName) == "int")
                        {
                            o.Add(column.ColumnName, Convert.ToInt32(row[column.ColumnName]));
                        }
                        else if (types.Get(column.ColumnName) == "bool" || types.Get(column.ColumnName) == "Boolean")
                        {
                            o.Add(column.ColumnName, row[column.ColumnName].ToString() == "1" || row[column.ColumnName].ToString().ToLower() == "true" || row[column.ColumnName].ToString().ToLower() == "t");
                        }
                        else if (types.Get(column.ColumnName) == "date")
                        {
                            o.Add(column.ColumnName, Convert.ToDateTime(row[column.ColumnName]));
                        }
                        else if (types.Get(column.ColumnName).ToLower().IndexOf("yy") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("dd") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("mm") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("hh") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("ss") > -1)
                        {
                            string t = "";
                            try
                            {
                                t = Convert.ToDateTime(row[column.ColumnName]).ToString(types.Get(column.ColumnName));
                                if (String.IsNullOrEmpty(t)) t = "NULL";
                            }
                            catch (Exception err)
                            {
                                t = "NULL";
                            }
                            o.Add(column.ColumnName, t);
                        }

                    }
                    catch (Exception err)
                    {
                        o.Add(column.ColumnName, row[column.ColumnName].ToString());
                    }

                }
                results.Add((Newtonsoft.Json.Linq.JToken)o);
                i++;
            }

            return results;
        }


        public T[] MultipleTest<T>(int page, int interval)
        {
            T[] units = new T[interval];
            object[] p = new object[interval];
            System.Type t = typeof(T);


            for (int i = 0; i < interval; i++)
            {
                p[i] = Activator.CreateInstance(t);

                PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                ConstructorInfo mConstructor = t.GetConstructor(System.Type.EmptyTypes);
                foreach (PropertyInfo fieldInfo in propInfos)
                {
                    string tmp_value = ((page * interval) + i).ToString();

                    try
                    {
                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = t.GetMethod(fieldInfo.Name + "Filter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { tmp_value });

                        if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                        {
                            fieldInfo.SetValue(p[i], (bool)mValue);
                        }
                        else
                        {
                            fieldInfo.SetValue(p[i], (String)mValue);
                        }


                    }
                    catch (Exception e)
                    {
                        try
                        {
                            //fieldInfo.SetValue(p[i], Convert.ChangeType(tmp_value, fieldInfo.PropertyType));

                            if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                            {
                                fieldInfo.SetValue(p[i], tmp_value == "1" || tmp_value.ToLower() == "true" || tmp_value.ToLower() == "t");
                            }
                            else
                            {
                                fieldInfo.SetValue(p[i], Convert.ChangeType(tmp_value, fieldInfo.PropertyType));
                            }
                        }
                        catch (Exception err)
                        {

                        }
                    }
                }
                units[i] = (T)p[i];
            }
            p = null;
            return (T[])units;

        }


        public T[] Multiple<T>()
        {
            bool caching = false;

            if (!String.IsNullOrEmpty(this.query))
            {
                if (this.cache != null)
                { 
                    if (!String.IsNullOrEmpty(this.cache.Name))
                    {
                        T[] _units = new T[0];
                        T[] _result = MySQL.Cache.Select<T[]>(this.cache.Name, this.cache.Frequency, _units, this.cache.StringConnection);
                        if (_result != null && !this.cache.ForceUpdate)
                        {
                            //Console.WriteLine("### FROM CACHE ###");
                            return _result;
                        }
                        else
                        {
                            this.ds = Data.Query(query, qparms);
                            caching = true;
                        }

                    }
                    else
                    {
                        this.ds = Data.Query(query, qparms);
                    }
                }
                else
                {
                    this.ds = Data.Query(query, qparms);
                }
            }

            //Console.WriteLine("### FROM DATABASE ###");

            T[] units = new T[this.ds.Tables[0].Rows.Count];
            object[] p = new object[this.ds.Tables[0].Rows.Count];
            System.Type t = typeof(T);
            object[] attrs;
            MySQL.FieldAttributes attr;

            int i = 0;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                p[i] = Activator.CreateInstance(t);

                PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                ConstructorInfo mConstructor = t.GetConstructor(System.Type.EmptyTypes);

                foreach (PropertyInfo fieldInfo in propInfos)
                {
                    string tmp_value = "";

                    try
                    {
                        tmp_value = row[fieldInfo.Name].ToString();
                    }
                    catch (Exception err)
                    {
                       
                    }
                    finally
                    {

                    }

                    //Behavior ----------------------------------------------------------------------------------------------
                    try
                    {
                        attrs = fieldInfo.GetCustomAttributes(typeof(MySQL.FieldAttributes), false);
                        attr = (MySQL.FieldAttributes)attrs[0];

                        Debug.WriteLine("Behavior ----------------------------------------------------------------------------------------------");

                        if (!String.IsNullOrEmpty(attr.Query) && attr.Fields.Length > 0)
                        {
                            string[] parms = new string[attr.Fields.Length];
                            for (int n = 0; n < attr.Fields.Length; n++)
                            {
                                parms[n] = row[attr.Fields[n]].ToString();
                            }

                            //Debug.WriteLine(attr.Query);
                            tmp_value = Data.Query(attr.Query, parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    catch (Exception err)
                    {
                        
                    }
                    finally
                    {

                    }
                    //Behavior ----------------------------------------------------------------------------------------------

                    try
                    {
                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = t.GetMethod(fieldInfo.Name + "Filter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { tmp_value });


                        if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                        {
                            fieldInfo.SetValue(p[i], (bool)mValue);
                        }
                        else
                        {
                            fieldInfo.SetValue(p[i], mValue);
                        }


                    }
                    catch (Exception e)
                    {
                        try
                        {
                            //isArray = fieldInfo.PropertyType.ToString().IndexOf("[]") > -1;

                            if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                            {
                                fieldInfo.SetValue(p[i], tmp_value == "1" || tmp_value.ToLower() == "true" || tmp_value.ToLower() == "t");
                            }
                            else
                            {                           
                                fieldInfo.SetValue(p[i], Convert.ChangeType(tmp_value, fieldInfo.PropertyType));
                            }
                        }
                        catch (Exception err)
                        {

                        }
                        finally
                        {

                        }
                    }
                    finally
                    {

                    }
                }
                units[i] = (T)p[i];
                i++;
            }
            p = null;
            if (caching) MySQL.Cache.Insert<T[]>(this.cache.Name, (T[])units, this.cache.StringConnection);
            return (T[])units;
        }


        public Newtonsoft.Json.Linq.JObject Single()
        {
            Newtonsoft.Json.Linq.JObject o = new Newtonsoft.Json.Linq.JObject();

            foreach (DataColumn column in ds.Tables[0].Columns)
            {
                try
                {
                    if (types.Get(column.ColumnName) == "string" || !String.IsNullOrEmpty(types.Get(column.ColumnName)))
                    {
                        o.Add(column.ColumnName, ds.Tables[0].Rows[0][column.ColumnName].ToString());
                    }
                    else if (types.Get(column.ColumnName) == "int")
                    {
                        o.Add(column.ColumnName, Convert.ToInt32(ds.Tables[0].Rows[0][column.ColumnName]));
                    }
                    else if (types.Get(column.ColumnName) == "bool" || types.Get(column.ColumnName) == "Boolean")
                    {
                        o.Add(column.ColumnName, ds.Tables[0].Rows[0][column.ColumnName].ToString() == "1" || ds.Tables[0].Rows[0][column.ColumnName].ToString().ToLower() == "true" || ds.Tables[0].Rows[0][column.ColumnName].ToString().ToLower() == "t");
                    }
                    else if (types.Get(column.ColumnName) == "date")
                    {
                        o.Add(column.ColumnName, Convert.ToDateTime(ds.Tables[0].Rows[0][column.ColumnName]));
                    }
                    else if (types.Get(column.ColumnName).ToLower().IndexOf("yy") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("dd") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("mm") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("hh") > -1 || types.Get(column.ColumnName).ToLower().IndexOf("ss") > -1)
                    {
                        string t = "";
                        try
                        {
                            t = Convert.ToDateTime(ds.Tables[0].Rows[0][column.ColumnName]).ToString(types.Get(column.ColumnName));
                            if (String.IsNullOrEmpty(t)) t = "NULL";
                        }
                        catch (Exception err)
                        {
                            t = "NULL";
                        }
                        o.Add(column.ColumnName, t);
                    }

                }
                catch (Exception err)
                {
                    o.Add(column.ColumnName, ds.Tables[0].Rows[0][column.ColumnName].ToString());
                }


            }
            return o;
        }


        public T Single<T>()
        {
            bool caching = false;

            if (!String.IsNullOrEmpty(this.query))
            {
                if (this.cache != null)
                {
                    if (!String.IsNullOrEmpty(this.cache.Name))
                    {
                        System.Type _t = typeof(T);
                        T _result = MySQL.Cache.Select<T>(this.cache.Name, this.cache.Frequency, (T)Activator.CreateInstance(_t), this.cache.StringConnection);
                        if (_result != null && !this.cache.ForceUpdate)
                        {
                            //Console.WriteLine("### FROM CACHE ###");
                            return _result;
                        }
                        else
                        {
                            this.ds = Data.Query(query, qparms);
                            caching = true;
                        }

                    }
                    else
                    {
                        this.ds = Data.Query(query, parms);
                    }
                }
                else
                {
                    this.ds = Data.Query(query, parms);
                }
            }

            //Console.WriteLine("### FROM DATABASE ###");


            object p = (object)obj;
            System.Type t = typeof(T);
            PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = t.GetConstructor(System.Type.EmptyTypes);
            object[] attrs;
            MySQL.FieldAttributes attr;

            p = Activator.CreateInstance(t);

            foreach (PropertyInfo fieldInfo in propInfos)
            {
                string tmp_value = "";

                try
                {
                    tmp_value = ds.Tables[0].Rows[0][fieldInfo.Name].ToString();
                }
                catch (Exception err)
                {

                }

                //Behavior ----------------------------------------------------------------------------------------------
                try
                {
                    attrs = fieldInfo.GetCustomAttributes(typeof(MySQL.FieldAttributes), false);
                    attr = (MySQL.FieldAttributes)attrs[0];

                    if (!String.IsNullOrEmpty(attr.Query) && attr.Fields.Length > 0)
                    {
                        string[] parms = new string[attr.Fields.Length];
                        for (int n = 0; n < attr.Fields.Length; n++)
                        {
                            parms[n] = ds.Tables[0].Rows[0][attr.Fields[n]].ToString();
                        }

                        tmp_value = Data.Query(attr.Query, parms).Tables[0].Rows[0][0].ToString();
                    }
                }
                catch (Exception err)
                {

                }
                finally
                {

                }
                //Behavior ----------------------------------------------------------------------------------------------




                try
                {
                    object mClassObject = mConstructor.Invoke(new object[] { });
                    MethodInfo mMethod = t.GetMethod(fieldInfo.Name + "Filter");
                    object mValue = mMethod.Invoke(mClassObject, new object[] { tmp_value });

                    if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                    {
                        fieldInfo.SetValue(p, (bool)mValue);
                    }
                    else
                    {
                        fieldInfo.SetValue(p, (String)mValue);
                    }
                }


                catch (Exception e)
                {
                    try
                    {
                        //fieldInfo.SetValue(p[i], Convert.ChangeType(tmp_value, fieldInfo.PropertyType));

                        if (fieldInfo.PropertyType == typeof(bool) || fieldInfo.PropertyType == typeof(Boolean))
                        {
                            fieldInfo.SetValue(p, tmp_value == "1" || tmp_value.ToLower() == "true" || tmp_value.ToLower() == "t");
                        }
                        else
                        {
                            fieldInfo.SetValue(p, Convert.ChangeType(tmp_value, fieldInfo.PropertyType));
                        }
                    }
                    catch (Exception err)
                    {

                    }
                    finally
                    {

                    }
                }
                finally
                {

                }
            }

            obj = (T)p;
            p = null;
            if (caching) MySQL.Cache.Insert<T>(this.cache.Name, (T)obj, this.cache.StringConnection);
            return (T)obj;
        }


        public Newtonsoft.Json.Linq.JArray MultipleJSON()
        {
            return MultipleJSON("");
        }


        public Newtonsoft.Json.Linq.JArray MultipleJSON(string format)
        {
            dynamic[] r = new System.Dynamic.ExpandoObject[ds.Tables[0].Rows.Count];
            int i = 0;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                r[i] = new System.Dynamic.ExpandoObject();

                foreach (DataColumn column in ds.Tables[0].Columns)
                {

                    DateTime dateValue;
                    if (DateTime.TryParse(row[column.ColumnName].ToString(), out dateValue))
                    {
                        if (!String.IsNullOrEmpty(format))
                        {
                            ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, dateValue.ToString(format));
                        }
                        else
                        {
                            ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, dateValue.ToString("yyyy-MM-dd"));
                        }
                    }
                    else
                    {
                        ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, row[column.ColumnName].ToString());
                    }
                }

                i++;
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(r);
            return Newtonsoft.Json.Linq.JArray.Parse(json);
        }


        public dynamic MultipleObject()
        {
            return MultipleObject("");
        }


        public dynamic MultipleObject(string format)
        {
            dynamic[] r = new System.Dynamic.ExpandoObject[ds.Tables[0].Rows.Count];
            int i = 0;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                r[i] = new System.Dynamic.ExpandoObject();

                foreach (DataColumn column in ds.Tables[0].Columns)
                {

                    DateTime dateValue;
                    if (DateTime.TryParse(row[column.ColumnName].ToString(), out dateValue))
                    {
                        if (!String.IsNullOrEmpty(format))
                        {
                            ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, dateValue.ToString(format));
                        }
                        else
                        {
                            ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, dateValue.ToString("yyyy-MM-dd"));
                        }
                    }
                    else
                    {
                        ((IDictionary<String, Object>)r[i]).Add(column.ColumnName, row[column.ColumnName].ToString());
                    }
                }

                i++;
            }

            return r;
        }


        public Newtonsoft.Json.Linq.JObject SingleJSON()
        {
            return SingleJSON("");
        }


        public Newtonsoft.Json.Linq.JObject SingleJSON(string format)
        {
            dynamic exo = new System.Dynamic.ExpandoObject();

            foreach (DataColumn column in ds.Tables[0].Columns)
            {
                DateTime dateValue;
                if (DateTime.TryParse(ds.Tables[0].Rows[0][column.ColumnName].ToString(), out dateValue))
                {
                    if (!String.IsNullOrEmpty(format))
                    {
                        ((IDictionary<String, Object>)exo).Add(column.ColumnName, dateValue.ToString(format));
                    }
                    else
                    {
                        ((IDictionary<String, Object>)exo).Add(column.ColumnName, dateValue.ToString("yyyy-MM-dd"));
                    }
                }
                else
                {
                    ((IDictionary<String, Object>)exo).Add(column.ColumnName, ds.Tables[0].Rows[0][column.ColumnName].ToString());
                }
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(exo);
            return Newtonsoft.Json.Linq.JArray.Parse(json);
        }


        public dynamic SingleObject()
        {
            return SingleObject("");
        }


        public dynamic SingleObject(string format)
        {
            dynamic exo = new System.Dynamic.ExpandoObject();

            foreach (DataColumn column in ds.Tables[0].Columns)
            {

                DateTime dateValue;
                if (DateTime.TryParse(ds.Tables[0].Rows[0][column.ColumnName].ToString(), out dateValue))
                {
                    if (!String.IsNullOrEmpty(format))
                    {
                        ((IDictionary<String, Object>)exo).Add(column.ColumnName, dateValue.ToString(format));
                    }
                    else
                    {
                        ((IDictionary<String, Object>)exo).Add(column.ColumnName, dateValue.ToString("yyyy-MM-dd"));
                    }
                }
                else
                {
                    ((IDictionary<String, Object>)exo).Add(column.ColumnName, ds.Tables[0].Rows[0][column.ColumnName].ToString());
                }
            }

            return exo;
        }
    }
}
