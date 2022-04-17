using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json
{
    public class Delete
    {
        private Models.KeyValueList sqlvars { get; set; }
        private string table { get; set; }
        private string field { get; set; }
        private string value { get; set; }
        private bool fieldsLower = false;
        private Models.KeyValueList wheres = new Models.KeyValueList();

        public Delete(string table, string field, string value)
        {
            this.table = table;
            this.field = field;
            this.value = value;
        }

        public Delete(string table)
        {
            this.table = table;
        }

        public static Delete From(string table, string field, string value)
        {
            return new Delete(table, field, value);
        }

        public static Delete From(string table)
        {
            return new Delete(table);
        }

        public Delete FieldsLower()
        {
            this.fieldsLower = true;
            return this;
        }

        public Delete SqlVars(Models.KeyValueList vars)
        {
            this.sqlvars = vars;
            return this;
        }

        public Delete Where(Models.KeyValueList pairs)
        {
            this.wheres = pairs;
            return this;
        }


        public string Query()
        {
            string q = "";
            string r = "";

            if (this.wheres.Count() > 0)
            {
                q += " WHERE ";
                int n = 0;
                foreach (Models.Pair arr in this.wheres.list)
                {
                    q += (n > 0 ? " AND " : "") + table + "." + (fieldsLower ? arr.key.ToLower() : arr.key) + "='" + arr.value + "' ";
                    n++;
                }
                q += ";";

                try
                {
                   r = (sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "DELETE FROM " + table + " " + q;
                }
                catch (Exception err)
                {
                    r = ("DELETE FROM " + table + " " + q);
                }
            }
            else
            {
                try
                {
                   r = ((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "DELETE FROM " + table + " WHERE " + table + "." + (fieldsLower ? field.ToLower() : field) + "='" + value + "' ");
                }
                catch (Exception err)
                {
                    r = ("DELETE FROM " + table + " WHERE " + table + "." + (fieldsLower ? field.ToLower() : field) + "='" + value + "' ");
                }
            }

            return r;
        }


        public void RunTables<T>()
        {
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
            parms.Add(new MySQL.MySQLParameter("?value", value));
            List<string> tables = new List<string>();
            string _old_table = "";
            string _table = table;
            bool _index = false;
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

                if (_old_table != _table)
                {
                    _old_table = _table;
                    tables.Add(_old_table);
                }
                
            }

            tables.Reverse();

            foreach (string _mytable in tables)
            {
                string q = "";

                if (this.wheres.Count() > 0)
                {
                    q += " WHERE ";
                    int n = 0;
                    foreach (Models.Pair arr in this.wheres.list)
                    {
                        parms.Add(new MySQL.MySQLParameter("?akey" + _mytable + n, arr.value));
                        q += (n > 0 ? " AND " : "") + (fieldsLower ? arr.key.ToLower() : arr.key) + "=?akey" + _mytable + n + " ";
                        n++;
                    }
                    q += ";";

                    try
                    {
                        MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "DELETE FROM " + _mytable + " " + q, parms);
                    }
                    catch (Exception err)
                    {
                        MySQL.Data.Query("DELETE FROM " + _mytable + " " + q, parms);

                    }
                }
                else
                {
                    try
                    {
                        MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + _mytable + " WHERE " + (fieldsLower ? field.ToLower() : field) + "=?value", parms);
                    }
                    catch (Exception err)
                    {
                        MySQL.Data.Query("SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + _mytable + " WHERE " + (fieldsLower ? field.ToLower() : field) + "=?value", parms);
                    }
                }
            }
        }


        public void Run()
        {
            List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();
            parms.Add(new MySQL.MySQLParameter("?value", value));
            string q = "";

            if (this.wheres.Count() > 0)
            {
                q += " WHERE ";
                int n = 0;
                foreach (Models.Pair arr in this.wheres.list)
                {
                    parms.Add(new MySQL.MySQLParameter("?akey" + n, arr.value));
                    q += (n > 0 ? " AND " : "") + table + "." + (fieldsLower ? arr.key.ToLower() : arr.key) + "=?akey" + n + " ";
                    n++;
                }
                q += ";";

                try
                {
                    MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + table + " " + q, parms);
                }
                catch (Exception err)
                {
                    MySQL.Data.Query("SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + table + " " + q, parms);

                }
            }
            else
            {

                try
                {
                    MySQL.Data.Query((sqlvars.Count() > 0 ? Utils.Serialize.SqlVars(sqlvars) : "") + "SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + table + " WHERE " + table + "." + (fieldsLower ? field.ToLower() : field) + "=?value", parms);
                }
                catch (Exception err)
                {
                    MySQL.Data.Query("SET FOREIGN_KEY_CHECKS = 0;DELETE FROM " + table + " WHERE " + table + "." + (fieldsLower ? field.ToLower() : field) + "=?value", parms);
                }


            }
        }
    }
}
