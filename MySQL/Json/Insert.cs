using System;
using System.Linq;using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using StringUtils;

namespace MySQL.Json
{
    public class FieldItem
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool index { get; set; }
        public string table { get; set; }

        public FieldItem(string name, string value, bool index, string table)
        {
            this.name = name;
            this.value = value;
            this.index = index;
            this.table = table;
        }
    }


    public class Insert
    {
        private List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
        private List<Models.QueryList> queries = new List<Models.QueryList>();
        private string table { get; set; }
        private Newtonsoft.Json.Linq.JObject data { get; set; }
        private Models.KeyValueList sqlvars = new Models.KeyValueList();
        private Models.KeyValueList exceptions = new Models.KeyValueList();
        private Models.KeyValueList[] exceptionsArr = new Models.KeyValueList[0];
        private bool update = false;
        private bool fieldsLower = false;
        private bool nullable = false;

        public Insert(Newtonsoft.Json.Linq.JObject data)
        {
            this.data = data;
        }

        public Insert(Newtonsoft.Json.Linq.JObject data, string table)
        {
            this.data = data;
            this.table = table;
        }

        public static Insert Into(Newtonsoft.Json.Linq.JObject data, string table)
        {
            return new Insert(data, table);
        }


        public static Insert Into(Newtonsoft.Json.Linq.JObject data)
        {
            return new Insert(data);
        }


        public Insert Update()
        {
            this.update = true;
            return this;
        }

        public Insert FieldsLower()
        {
            this.fieldsLower = true;
            return this;
        }

        public Insert Exceptions(Models.KeyValueList exceptions)
        {
            this.exceptions = exceptions;
            return this;
        }

        public Insert Exceptions(Models.KeyValueList[] exceptionsArr)
        {
            this.exceptionsArr = exceptionsArr;
            return this;
        }

        public Insert SqlVars(Models.KeyValueList vars)
        {
            this.sqlvars = vars;
            return this;
        }

        public Insert Nullable()
        {
            this.nullable = true;
            return this;
        }


        public string Query()
        {
            queries.Clear();
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            string r = "";
            int i = 0;

            foreach (var item in data)
            {

                if (!exceptions.Contains(item.Key))
                {
                    string v = ((Newtonsoft.Json.Linq.JValue)item.Value).ToString(CultureInfo.InvariantCulture);

                    if (!nullable || (nullable && !String.IsNullOrEmpty(v.Replace(" ", ""))))
                    {
                        fields.Add(table + "." + (fieldsLower ? item.Key.ToLower() : item.Key));
                        values.Add("'" + v + "'");
                        i++;
                    }
                }
            }

            if (this.update)
            {
                r += "INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ") ON DUPLICATE KEY UPDATE ";
                for (int z = 0; z < fields.Count; z++)
                {
                    r += (z == 0 ? "" : ",") + fields[z] + "=" + values[z];

                }
            }
            else
            {
                r += "INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ");";
            }

            return r;
        }






        public int Run()
        {
            queries.Clear();
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
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
                        parms.Add(new MySQL.MySQLParameter("?" + table + i, ((Newtonsoft.Json.Linq.JValue)item.Value).ToString(CultureInfo.InvariantCulture)));
                        //values.Add(item.Value.ToString());
                        values.Add("?" + table + i);
                        i++;
                    }
                }
            }

            r += "INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ");";

            queries.Add(new Models.QueryList(table, fields, values, parms));

            Models.QueryList[] t = queries.ToArray<Models.QueryList>();
            string id = "";

            for (int j = t.Length - 1; j >= 0; j--)
            {
                if (id == "")
                {
                    if (this.update)
                    {
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    else
                    {
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                }
                else
                {
                    t[j].fields.Add(table + "id");
                    t[j].values.Add(id);
                    if (this.update)
                    {
                        try
                        {
                            MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            MySQL.Data.Query(QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    else
                    {
                        try
                        {
                            MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            MySQL.Data.Query(QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                }
            }


            return Convert.ToInt32(id);
        }




        public string RunTables<T>()
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
            object[] fattrs;
            FieldAttributes fattr;

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

                    //Behavior ----------------------------------------------------------------------------------------------
                    try
                    {
                        val = data.GetValue(fieldInfo.Name).ToObject<string>();
                    }
                    catch (Exception err)
                    {

                    }

                    try
                    {
                        fattrs = fieldInfo.GetCustomAttributes(typeof(MySQL.FieldAttributes), false);
                        fattr = (MySQL.FieldAttributes)fattrs[0];

                        //Debug.WriteLine("Behavior " + fieldInfo.Name + "|" + fattr.CaseMode + "|" + val + " ----------------------------------------------------------------------------------------------");

                        if (fattr.CaseMode)
                        {
                            val = StringUtils.Optimize.CaseMode(val);
                            //Debug.WriteLine(val + " ----------------------------------------------------------------------------------------------");
                        }
                    }
                    catch (Exception err)
                    {

                    }
                    //Behavior ----------------------------------------------------------------------------------------------


                    try
                    {
                        //val = data.GetValue(fieldInfo.Name).ToObject<string>();

                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = ty.GetMethod(fieldInfo.Name + "InsertFilter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { val });
                        val = mValue.ToString();
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
                if (!String.IsNullOrEmpty(_id))
                {
                    foreach (FieldItem field in fields)
                    {
                        if (field.index)
                        {
                            field.value = _id;
                        }
                    }
                }

                if (this.update)
                {
                    //Debug.WriteLine(QueryFy3Update(_mytable, fields));
                    string r1 = MySQL.Data.Query(QueryFy3Update(_mytable, fields) + "SELECT LAST_INSERT_ID();", parms).Tables[0].Rows[0][0].ToString();
                    if (String.IsNullOrEmpty(_id)) _id = r1;
                }
                else
                {
                    //Debug.WriteLine(QueryFy3(_mytable, fields));
                    string r2 = MySQL.Data.Query(QueryFy3(_mytable, fields) + "SELECT LAST_INSERT_ID();", parms).Tables[0].Rows[0][0].ToString();
                    if (String.IsNullOrEmpty(_id)) _id = r2;
                }
            }


            return _id;
        }

        public int Run<T>()
        {
            queries.Clear();
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            //List<String> values1 = new List<String>();
            string r = "";
            int i = 0;
            Type ty = typeof(T);
            object[] fattrs;
            FieldAttributes fattr;

            PropertyInfo[] propInfos = ty.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = ty.GetConstructor(Type.EmptyTypes);

            foreach (PropertyInfo fieldInfo in propInfos)
            {
                if (!exceptions.Contains(fieldInfo.Name))
                {
                    string val = "";


                    //Behavior ----------------------------------------------------------------------------------------------
                    try
                    {
                        val = data.GetValue(fieldInfo.Name).ToObject<string>();
                    }
                    catch (Exception err)
                    {

                    }

                    try
                    {
                        fattrs = fieldInfo.GetCustomAttributes(typeof(MySQL.FieldAttributes), false);
                        fattr = (MySQL.FieldAttributes)fattrs[0];

                        if (fattr.CaseMode)
                        {
                            val = StringUtils.Optimize.CaseMode(val);
                        }
                    }
                    catch (Exception err)
                    {

                    }
                    //Behavior ----------------------------------------------------------------------------------------------

                    try
                    {
                        //val = data.GetValue(fieldInfo.Name).ToObject<string>();

                        object mClassObject = mConstructor.Invoke(new object[] { });
                        MethodInfo mMethod = ty.GetMethod(fieldInfo.Name + "InsertFilter");
                        object mValue = mMethod.Invoke(mClassObject, new object[] { val });
                        val = mValue.ToString();
                    }
                    catch (Exception e)
                    {

                    }

                    if (!nullable || (nullable && !String.IsNullOrEmpty(val.Replace(" ", "").Replace("          ", ""))))
                    {
                        fields.Add(table + "." + (fieldsLower ? fieldInfo.Name.ToLower() : fieldInfo.Name));
                        parms.Add(new MySQL.MySQLParameter("?" + table + i, val));
                        //values1.Add("'"+val+"'");
                        values.Add("?" + table + i);
                        i++;
                    }
                }
            }

            r += "INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ");";
            //Debug.WriteLine(r);
            queries.Add(new Models.QueryList(table, fields, values, parms));

            Models.QueryList[] t = queries.ToArray<Models.QueryList>();
            string id = "";

            for (int j = t.Length - 1; j >= 0; j--)
            {
                if (id == "")
                {
                    if (this.update)
                    {
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    else
                    {
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                            Debug.WriteLine((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();");
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                }
                else
                {
                    t[j].fields.Add(table + "id");
                    t[j].values.Add(id);
                    if (this.update)
                    {
                        try
                        {
                            MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            MySQL.Data.Query(QueryFyUpdate(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    else
                    {
                        //Debug.WriteLine(QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();");
                        try
                        {
                            MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            MySQL.Data.Query(QueryFy(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                }
            }

            return Convert.ToInt32(id);
        }


        public int Multiple()
        {
            queries.Clear();
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
            List<String> fields = new List<String>();
            List<String> values = new List<String>();
            int i = 0;
            int exceptionsCount = 0;

            foreach (var item in data)
            {
                bool chk = true;
                try
                {
                    chk = !exceptionsArr[exceptionsCount].Contains(item.Key);
                }
                catch (Exception ee)
                {

                }

                if (chk)
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JArray arr = (Newtonsoft.Json.Linq.JArray)item.Value;
                        Partial2(arr, (fieldsLower ? item.Key.ToLower() : item.Key), exceptionsArr, exceptionsCount + 1, table);
                    }
                    catch (Exception er)
                    {
                        fields.Add(table + "." + (fieldsLower ? item.Key.ToLower() : item.Key));
                        parms.Add(new MySQL.MySQLParameter("?" + table + i, item.Value.ToString()));
                        //values.Add(item.Value.ToString());
                        values.Add("?" + table + i);
                        i++;
                    }
                }
            }

            queries.Add(new Models.QueryList(table, fields, values, parms, true, String.Empty));

            Models.QueryList[] t = queries.ToArray<Models.QueryList>();

            string id = "";
            List<Models.Pair> dependencies = new List<Models.Pair>();


            for (int j = t.Length - 1; j >= 0; j--)
            {
                if (!String.IsNullOrEmpty(t[j].dependency) && dependencies.Count > 0)
                {
                    foreach (Models.Pair p in dependencies)
                    {
                        if (p.key != t[j].table)
                        {
                            t[j].fields.Add(p.key + "id");
                            t[j].values.Add(p.value);
                        }
                    }
                }

                if (t[j].master)
                {
                    if (!this.update)
                    {
                        Debug.WriteLine("Dependency:" + t[j].dependency + " Master: " + QueryFy2(t[j].table, t[j].fields, t[j].values));
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFy2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Dependency:" + t[j].dependency + " Master: " + QueryFyUpdate2(t[j].table, t[j].fields, t[j].values));
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {

                            id = MySQL.Data.Query(QueryFyUpdate2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }

                    if (dependencies.Count > 0)
                    {
                        int index = dependencies.IndexOf(dependencies.Where(x => x.key == t[j].table).FirstOrDefault<Models.Pair>());
                        if (index >= 0)
                        {
                            dependencies[index].value = id;
                        }
                        else
                        {
                            dependencies.Add(new Models.Pair(t[j].table, id));
                        }
                    }
                    else
                    {
                        dependencies.Add(new Models.Pair(t[j].table, id));
                    }
                }
                else
                {
                    if (!this.update)
                    {
                        Debug.WriteLine("Dependency:" + t[j].dependency + " Master: " + QueryFy2(t[j].table, t[j].fields, t[j].values));
                        try
                        {
                            MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFy2(t[j].table, t[j].fields, t[j].values), t[j].parms);
                        }
                        catch (Exception err)
                        {
                            MySQL.Data.Query(QueryFy2(t[j].table, t[j].fields, t[j].values), t[j].parms);
                        }
                    }
                    else
                    {
                        try
                        {
                            id = MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + QueryFyUpdate2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                        catch (Exception err)
                        {
                            id = MySQL.Data.Query(QueryFyUpdate2(t[j].table, t[j].fields, t[j].values) + "SELECT LAST_INSERT_ID();", t[j].parms).Tables[0].Rows[0][0].ToString();
                        }
                    }
                }
            }

            return Convert.ToInt32(id);
        }








        #region Functions 1.0 -------------------------------------------------------------------------------------------------------------------
        private string QueryFy(string table, List<String> fields, List<String> values)
        {
            return "SET FOREIGN_KEY_CHECKS = 0;INSERT IGNORE INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ");";
        }

        private static string QueryFyUpdate(string table, List<String> fields, List<String> values)
        {
            string q = "SET FOREIGN_KEY_CHECKS = 0;INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ") ON DUPLICATE KEY UPDATE ";
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i] + "=" + values[i];

            }
            return q + ";";
        }
        #endregion



        #region Functions 2.0 -------------------------------------------------------------------------------------------------------------------
        private string QueryFy2(string table, List<String> fields, List<String> values)
        {
            return "SET FOREIGN_KEY_CHECKS = 0;INSERT IGNORE INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ");";
        }

        private string QueryFyUpdate2(string table, List<String> fields, List<String> values)
        {
            string q = "SET FOREIGN_KEY_CHECKS = 0;INSERT INTO " + table + " (" + String.Join<String>(",", fields) + ") VALUES (" + String.Join<String>(",", values) + ") ON DUPLICATE KEY UPDATE ";
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i] + "=" + values[i];

            }
            return q + ";";
        }


        public void Partial2(Newtonsoft.Json.Linq.JArray jarr, string table, Models.KeyValueList[] exceptionsArr, int exceptionsCount, string dependencyTable)
        {
            foreach (Newtonsoft.Json.Linq.JObject item in jarr)
            {
                List<MySQL.MySQLParameter> jparms = new List<MySQL.MySQLParameter>();
                List<String> fields = new List<String>();
                List<String> values = new List<String>();
                int i = 0;
                bool master = false;

                foreach (var jitem in item)
                {

                    bool chk = true;
                    try
                    {
                        chk = !exceptionsArr[exceptionsCount].Contains(jitem.Key);
                    }
                    catch (Exception ee)
                    {

                    }

                    if (chk)
                    {

                        try
                        {
                            Newtonsoft.Json.Linq.JArray arr = (Newtonsoft.Json.Linq.JArray)jitem.Value;
                            Partial2(arr, (fieldsLower ? jitem.Key.ToLower() : jitem.Key), exceptionsArr, exceptionsCount + 1, table);
                            master = true;
                        }
                        catch (Exception er)
                        {
                            fields.Add(table + "." + (fieldsLower ? jitem.Key.ToLower() : jitem.Key));
                            jparms.Add(new MySQL.MySQLParameter("?" + table + i, jitem.Value.ToString()));
                            values.Add("?" + table + i);
                            i++;
                        }
                    }
                }

                queries.Add(new Models.QueryList(table, fields, values, jparms, master, dependencyTable));
            }
        }
        #endregion



        #region Functions 3.0 -------------------------------------------------------------------------------------------------------------------
        private string QueryFy3(string table, List<FieldItem> fields)
        {
            return "SET FOREIGN_KEY_CHECKS = 0;INSERT IGNORE INTO " + table + " (" + String.Join<String>(",", fields.Where(r => r.table == table || r.index == true).Select(r => r.name)) + ") VALUES (" + String.Join<String>(",", fields.Where(r => r.table == table || r.index == true).Select(r => r.value)) + ");";
        }

        private static string QueryFy3Update(string table, List<FieldItem> fields)
        {
            string q = "SET FOREIGN_KEY_CHECKS = 0;INSERT INTO " + table + " (" + String.Join<String>(",", fields.Where(r => r.table == table || r.index).Select(r => r.name)) + ") VALUES (" + String.Join<String>(",", fields.Where(r => r.table == table || r.index).Select(r => r.value)) + ") ON DUPLICATE KEY UPDATE ";
            for (int i = 0; i < fields.Count; i++)
            {
                q += (i == 0 ? "" : ",") + fields[i].name + "=" + fields[i].value;

            }
            return q + ";";
        }
        #endregion
    }
}
