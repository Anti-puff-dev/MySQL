using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json
{
    public class Update
    {
        private List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
        private List<Models.QueryList> queries = new List<Models.QueryList>();
        private string table { get; set; }
        private Newtonsoft.Json.Linq.JObject data { get; set; }
        private Models.KeyValueList sqlvars = new Models.KeyValueList();
        private Models.KeyValueList exceptions = new Models.KeyValueList();
        private Models.KeyValueList wheres = new Models.KeyValueList();
        private string[] where = new string[] { "", "" };
        private bool fieldsLower = false;
        private bool nullable = false;


        public Update(Newtonsoft.Json.Linq.JObject data, string table)
        {
            this.data = data;
            this.table = table;
        }

        public static Update Table(Newtonsoft.Json.Linq.JObject data, string table)
        {
            return new Update(data, table);
        }

        public Update Exceptions(Models.KeyValueList exceptions)
        {
            this.exceptions = exceptions;
            return this;
        }

        public Update SqlVars(Models.KeyValueList vars)
        {
            this.sqlvars = vars;
            return this;
        }

        public Update Where(string field, string value)
        {
            this.where = new string[] { field, value };
            return this;
        }

        public Update Where(Models.KeyValueList pairs)
        {
            this.wheres = pairs;
            return this;
        }

        public Update FieldsLower()
        {
            this.fieldsLower = true;
            return this;
        }

        public Update Nullable()
        {
            this.nullable = true;
            return this;
        }


        public string Query()
        {
            queries.Clear();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            string q = "";
            int z = 0;
            foreach (var item in data)
            {
                if (!exceptions.Contains(item.Key))
                {
                    string v = ((Newtonsoft.Json.Linq.JValue)item.Value).ToString(CultureInfo.InvariantCulture);
                    if (!nullable || (nullable && !String.IsNullOrEmpty(v.Replace(" ", "").Replace("          ", ""))))
                    {
                        fields.Add(table + "." + (fieldsLower ? item.Key.ToLower() : item.Key));
                        values.Add(v);
                        z++;
                    }
                }
            }

            q = "UPDATE " + table + " SET ";
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i] + "='" + values[i] + "' ";
            }

            if (this.wheres.Count() > 0)
            {
                q += " WHERE ";
                int n = 0;
                foreach (Models.Pair arr in this.wheres.list)
                {
                    q += (n > 0 ? " AND " : "") + table + "." + arr.key + "='" + arr.value + "' ";
                    n++;
                }
                q += ";";
            }
            else
            {
                q += " WHERE " + (this.where[0] == "" ? table + "." + "id='" + this.where[1] + "'" : table + "." + this.where[0] + "='" + this.where[1] + "' ");
            }

            parms = parms.Distinct().ToList();


            return q;
        }


        public void Set()
        {
            queries.Clear();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            string r = "";
            int i = 0;
            foreach (var item in data)
            {
                if (!exceptions.Contains(item.Key))
                {
                    string v = ((Newtonsoft.Json.Linq.JValue)item.Value).ToString(CultureInfo.InvariantCulture);
                    if (!nullable || (nullable && !String.IsNullOrEmpty(v.Replace(" ", "").Replace("          ", ""))))
                    {
                        fields.Add(table + "." + (fieldsLower ? item.Key.ToLower() : item.Key));
                        parms.Add(new MySQL.MySQLParameter("?" + table + i, item.Value.ToString()));
                        values.Add("?" + table + i);
                        i++;
                    }
                }
            }

            string id = MySQL.Json.Utils.Exists.String(data, "id");// data.GetValue("id").ToObject<string>();
            if (!String.IsNullOrEmpty(id)) parms.Add(new MySQL.MySQLParameter("?id", id));
            queries.Add(new Models.QueryList(table, fields, values, parms));

            Models.QueryList[] t1 = queries.ToArray<Models.QueryList>();
            r += QueryFy(table, fields, values);

            for (int j = t1.Length - 1; j >= 0; j--)
            {
                if (j < t1.Length - 1)
                {
                    t1[j].fields.Add(table + "id");
                    t1[j].values.Add(id);
                }
                try
                {
                    MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t1[j].table, t1[j].fields, t1[j].values), t1[j].parms);
                }
                catch (Exception err)
                {
                    MySQL.Data.Query(QueryFy(t1[j].table, t1[j].fields, t1[j].values), t1[j].parms);
                }
            }
        }



        public void SetTables<T>()
        {
            queries.Clear();
            List<FieldItem> fields = new List<FieldItem>();
            List<string> tables = new List<string>();
            int i = 0;
            string _old_table = "";
            string _table = table;
            bool _index = false;
            string _id = "";
            object[] attrs;
            MySQLAttributes attr;

            Type ty = typeof(T);

            PropertyInfo[] propInfos = ty.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = ty.GetConstructor(Type.EmptyTypes);

            foreach (PropertyInfo fieldInfo in propInfos)
            {

                try
                {
                    attrs = fieldInfo.GetCustomAttributes(typeof(MySQLAttributes), false);
                    attr = (MySQLAttributes)attrs[0];

                    try
                    {
                        if (!String.IsNullOrEmpty(attr.Table))
                        {
                            _table = table = attr.Table;
                        }
                        else
                        {
                            _table = table;
                        }
                    }
                    catch (Exception err)
                    {
                        _table = table;
                    }

                    try
                    {
                        _index = attr.Index;
                    }
                    catch (Exception err)
                    {
                        _index = false;
                    }
                }
                catch (Exception err)
                {
                    _index = false;
                    _table = table;
                }



                if (!exceptions.Contains(fieldInfo.Name))
                {
                    string val = "";


                    try
                    {
                        val = data.GetValue(fieldInfo.Name).ToObject<string>();

                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = ty.GetMethod(fieldInfo.Name + "UpdateFilter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { val });
                        val = ((String)mValue).ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {

                    }


                    if (!nullable || (nullable && !String.IsNullOrEmpty(val.Replace(" ", "").Replace("          ", ""))))
                    {
                        fields.Add(new FieldItem((fieldsLower ? fieldInfo.Name.ToLower() : fieldInfo.Name), "?" + _table + i, _index, _table));
                        parms.Add(new MySQL.MySQLParameter("?" + _table + i, val));
                        i++;
                    }

                    if (_old_table != _table)
                    {
                        _old_table = _table;
                        tables.Add(_old_table);
                    }

                }
            }


            tables.Reverse();

            foreach (string _mytable in tables)
            {
                //Debug.WriteLine(QueryFy2(_mytable, fields));
                try
                {
                    MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy2(_mytable, fields), parms);
                }
                catch (Exception err)
                {
                    MySQL.Data.Query(QueryFy2(_mytable, fields), parms);
                }


            }
        }



        public void Set<T>()
        {
            queries.Clear();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            string r = "";
            int i = 0;
            Type t = typeof(T);

            PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = t.GetConstructor(Type.EmptyTypes);

            foreach (PropertyInfo fieldInfo in propInfos)
            {
                if (!exceptions.Contains(fieldInfo.Name))
                {
                    //string val = data.GetValue(fieldInfo.Name).ToObject<string>();
                    string val = MySQL.Json.Utils.Exists.String(data, fieldInfo.Name);
                    try
                    {
                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = t.GetMethod(fieldInfo.Name + "UpdateFilter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { val });
                        val = ((String)mValue).ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {

                    }

                    if (!nullable || (nullable && !String.IsNullOrEmpty(val.Replace(" ", "").Replace("          ", ""))))
                    {
                        fields.Add(table + "." + (fieldsLower ? fieldInfo.Name.ToLower() : fieldInfo.Name));
                        parms.Add(new MySQL.MySQLParameter("?" + table + i, val));
                        values.Add("?" + table + i);
                        i++;
                    }
                }
            }

            string id = MySQL.Json.Utils.Exists.String(data, "id");// data.GetValue("id").ToObject<string>();
            if (!String.IsNullOrEmpty(id)) parms.Add(new MySQL.MySQLParameter("?id", id));
            queries.Add(new Models.QueryList(table, fields, values, parms));

            Models.QueryList[] t1 = queries.ToArray<Models.QueryList>();
            r += QueryFy(table, fields, values);



            for (int j = t1.Length - 1; j >= 0; j--)
            {
                if (j < t1.Length - 1)
                {
                    t1[j].fields.Add(table + "id");
                    t1[j].values.Add(id);
                }
                try
                {
                    MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t1[j].table, t1[j].fields, t1[j].values), t1[j].parms);
                }
                catch (Exception err)
                {
                    MySQL.Data.Query(QueryFy(t1[j].table, t1[j].fields, t1[j].values), t1[j].parms);
                }
            }

        }


        #region Private Functions --------------------------------------------------------------------------------------------------------------------------------
        private string QueryFy(string table, List<String> fields, List<String> values)
        {

            string q = "SET FOREIGN_KEY_CHECKS = 0;UPDATE " + table + " SET ";
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i] + "=" + values[i];
            }

            if (this.wheres.Count() > 0)
            {
                q += " WHERE ";
                int n = 0;
                foreach (Models.Pair arr in this.wheres.list)
                {
                    parms.Add(new MySQL.MySQLParameter("?akey" + n, arr.value));
                    q += (n > 0 ? " AND " : "") + table + "." + arr.key + "=?akey" + n + " ";
                    n++;
                }
                q += ";";
            }
            else
            {
                parms.Add(new MySQL.MySQLParameter("?akey", this.where[1]));
                q += " WHERE " + (this.where[0] == "" ? table + "." + "id=?id" : table + "." + this.where[0] + "=?akey");
            }

            parms = parms.Distinct().ToList();
            //Debug.WriteLine(q);
            return q;
        }


        private string QueryFy2(string mtable, List<FieldItem> mfields)
        {
            string q = "SET FOREIGN_KEY_CHECKS = 0;UPDATE " + mtable + " SET ";

            List<FieldItem> fields = mfields.Where(r => r.table == mtable || r.index).Select(r => r).ToList<FieldItem>();
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i].name + "=" + fields[i].value;
            }

            if (this.wheres.Count() > 0)
            {
                q += " WHERE ";
                int n = 0;
                foreach (Models.Pair arr in this.wheres.list)
                {
                    parms.Add(new MySQL.MySQLParameter("?akey" + mtable + n, arr.value));
                    q += (n > 0 ? " AND " : "") + arr.key + "=?akey" + table + n + " ";
                    n++;
                }
                q += ";";
            }
            else
            {
                parms.Add(new MySQL.MySQLParameter("?akey" + mtable, this.where[1]));
                q += " WHERE " + (this.where[0] == "" ? "id=?id" : mtable + "." + this.where[0] + "=?akey" + mtable);
            }

            parms = parms.Distinct().ToList();
            //Debug.WriteLine(q);
            return q;
        }
        #endregion
    }
}
