using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json.Utils
{
    public class QueryModel
    {
        public string fields { get; set; }
        public string tables { get; set; }

        public QueryModel(string fields, string tables)
        {
            this.fields = fields;
            this.tables = tables;
        }
    }

    public class QueryBuilder
    {
        public static QueryModel SelectModel<T>()
        {
            return SelectModel<T>(new string[] { });
        }


        public static QueryModel SelectModel<T>(string[] exceptions)
        {
            string fields = "";
            string tables = "";
            string _old_table = "";
            string _table = "";
            bool _index = false;
            object[] attrs;
            MySQLAttributes attr;

            Type ty = typeof(T);

            PropertyInfo[] propInfos = ty.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = ty.GetConstructor(Type.EmptyTypes);

            foreach (PropertyInfo fieldInfo in propInfos)
            {

                if (!exceptions.Contains(fieldInfo.Name))
                {

                    try
                    {
                        attrs = fieldInfo.GetCustomAttributes(typeof(MySQLAttributes), false);
                        attr = (MySQLAttributes)attrs[0];

                        try
                        {
                            if (!String.IsNullOrEmpty(attr.Table))
                            {
                                _table = attr.Table;
                            }
                            else
                            {
                                _table = _old_table;
                            }
                        }
                        catch (Exception err)
                        {
                            _table = _old_table;
                        }

                        if (_old_table != _table)
                        {
                            tables += (tables == "" ? "" : ",") + _table;
                            _old_table = _table;
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
                        _table = _old_table;
                    }


                    fields += (fields == "" ? "" : ",") + _table + "." + fieldInfo.Name;
                }
            }

            return new QueryModel(fields, tables);
        }

        /*
        public static QueryModel GROUP_CONCAT<T>(string[] exceptions)
        {
            string fields = "";
            string tables = "";
            string _old_table = "";
            string _table = "";
            bool _index = false;
            object[] attrs;
            MySQLAttributes attr;

            Type ty = typeof(T);

            PropertyInfo[] propInfos = ty.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo mConstructor = ty.GetConstructor(Type.EmptyTypes);

            foreach (PropertyInfo fieldInfo in propInfos)
            {

            }

            return new QueryModel(fields, tables);
        }*/
    }
}
