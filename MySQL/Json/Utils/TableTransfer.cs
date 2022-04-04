using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using StringUtils;
using Newtonsoft.Json;



namespace MySQL.Json.Utils
{
    public class TableTransfer
    {
        //Functions ----------------------------------------------------------------------------------------------------
        private static string _fieldName(string _text)
        {
            int _p1 = _text.IndexOf("`", 0);
            int _p2 = _text.IndexOf("`", _p1 + 1);

            return _text.Substring(_p1 + 1, _p2 - _p1 - 1);
        }


        private static string _fieldComment(string _text)
        {
            int _p0 = _text.IndexOf("COMMENT", 0);
            if (_p0 > 0)
            {
                int _p1 = _text.IndexOf("'", _p0);
                int _p2 = _text.IndexOf("'", _p1 + 1);

                return _text.Substring(_p1 + 1, _p2 - _p1 - 1);
            }
            else
            {
                return "";
            }
        }


        private static string _fieldType(string _text)
        {
            _text = _text.Replace("AUTO_INCREMENT", "").Replace("NOT NULL", "").Replace("DEFAULT NULL", "").Replace(" ", "").Replace(",", "");
            int _p1 = _text.LastIndexOf("`");
            int _p2 = _text.IndexOf("(", _p1 + 1);


            if (_p2 == -1)
            {
                _p2 = _text.Length;
            }

            return _text.Substring(_p1 + 1, _p2 - _p1 - 1);
        }
        //Functions ----------------------------------------------------------------------------------------------------



        public static string Run(string origin_connection, string origin_table, string target_connection, string target_table, int offset = 0, int interval = 1000, string where = "")
        {
            DataSet ds = MySQL.Data.IQuery("SHOW CREATE TABLE " + origin_table, origin_connection);
            string _fullTable = ds.Tables[0].Rows[0][1].ToString();

            int _p1 = _fullTable.IndexOf("(") + 1;
            int _p2 = _fullTable.LastIndexOf(")");

            string _resultTable = _fullTable.Substring(_p1, _p2 - _p1);

            _resultTable = _resultTable.Replace(",\n", "|");

            string[] _elements = _resultTable.Split(new char[] { '|' });

            ArrayList _fieldNames_Arr = new ArrayList();
            ArrayList _fieldTypes_Arr = new ArrayList();

            for (int _k = 0; _k < _elements.Length; _k++)
            {
                if (_elements[_k].IndexOf("PRIMARY KEY ", 0) < 0)
                {
                    //Console.WriteLine(_fieldName(_elements[_k]) + " " + _fieldType(_elements[_k]));
                    _fieldNames_Arr.Add(_fieldName(_elements[_k]));
                    _fieldTypes_Arr.Add(_fieldType(_elements[_k]));
                }
            }

            string _query = "SELECT " + String.Join(",", _fieldNames_Arr.ToArray()) + " FROM " + origin_table + " " + where + "  LIMIT " + interval + " OFFSET " + offset;
            //Console.WriteLine(_query);
            DataSet ds0 = Data.IQuery(_query, origin_connection);
            //Console.WriteLine(ds0.Tables[0].Rows.Count);

            string _arr = "";
            string _query1 = "";
            List<MySQLParameter> _parms = new List<MySQLParameter>();
            _parms.Clear();

            int c = 0;
            foreach (DataRow row in ds0.Tables[0].Rows)
            {
                _arr = "";
                //try
                //{
                for (int i = 0; i < _fieldNames_Arr.Count; i++)
                {
                    if (_fieldTypes_Arr[i].ToString() == "datetime")
                    {
                        try
                        {
                            //_arr += (_arr == "" ? "" : ",") + "'" + Convert.ToDateTime(row[_fieldNames_Arr[i].ToString()]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            _parms.Add(new MySQLParameter("p" + c, Convert.ToDateTime(row[_fieldNames_Arr[i].ToString()]).ToString("yyyy-MM-dd HH:mm:ss")));
                        }
                        catch (Exception err)
                        {
                            _parms.Add(new MySQLParameter("p" + c, "NULL"));
                        }
                    }
                    else if (_fieldTypes_Arr[i].ToString() == "date")
                    {
                        try
                        {
                            //_arr += (_arr == "" ? "" : ",") + "'" + Convert.ToDateTime(row[_fieldNames_Arr[i].ToString()]).ToString("yyyy-MM-dd") + "'";
                            _parms.Add(new MySQLParameter("p" + c, Convert.ToDateTime(row[_fieldNames_Arr[i].ToString()]).ToString("yyyy-MM-dd")));
                        }
                        catch (Exception err)
                        {
                            _parms.Add(new MySQLParameter("p" + c, "NULL"));
                        }
                    }
                    else
                    {
                        //_arr += (_arr == "" ? "" : ",") + "'" + row[_fieldNames_Arr[i].ToString()].ToString() + "'";
                        _parms.Add(new MySQLParameter("p" + c, row[_fieldNames_Arr[i].ToString()].ToString()));
                    }

                    _arr += (_arr == "" ? "" : ",") + "?p" + c;
                    c++;
                }


                _query1 += "INSERT INTO " + target_table + " (" + String.Join(",", _fieldNames_Arr.ToArray()) + ") VALUES (" + _arr + ");";

                //Console.WriteLine(_query1);
                /*}
                catch(Exception err)
                {

                }*/

            }


            DataSet ds1 = Data.IQuery(_query1, target_connection, _parms);


            return "loaded";
        }
    }
}
