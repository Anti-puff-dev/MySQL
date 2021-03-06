using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace MySQL
{
    [AttributeUsage(AttributeTargets.All)]
    public class MySQLAttributes : Attribute
    {
        public string Table { get; set; }
        public bool Index { get; set; }


        public MySQLAttributes(string table)
        {
            Table = table;
        }

        public MySQLAttributes(string table, bool index)
        {
            Table = table;
            Index = index;
        }

        public MySQLAttributes(bool index)
        {
            Index = index;
        }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class FieldAttributes : Attribute
    {
        public string Query { get; set; }
        public string[] Fields { get; set; }
        public bool CaseMode { get; set; }

        public FieldAttributes(string query, string[] fields)
        {
            Query = query;
            Fields = fields;
        }

        public FieldAttributes(bool casemode)
        {
            CaseMode = casemode;
        }
    }


    public class MySQLParameter
    {
        public string name { get; set; }
        public string value { get; set; }
        public MySqlDbType type { get; set; }
        public Int32 len { get; set; }

        public MySQLParameter(String name, String value, MySqlDbType type, Int32 len)
        {
            this.name = name;
            this.value = value;
            this.type = type;
            this.len = len;
        }

        public MySQLParameter(String name, String value)
        {
            this.name = name;
            this.value = value;
        }
    }


    public class Data
    {
        public static bool resync = false;
       

        public static DataSet Query(string _query, List<MySQLParameter> list)
        {
            return Query(_query, list, -1, 0);
        }


        public static DataSet Query(string _query, List<MySQLParameter> list, int page, int results)
        {
            if (page > -1 && results > 0)
            {
                _query += " LIMIT " + results + " OFFSET " + ((page - (page == 0 ? 0 : 1)) * results);
            }

            using (MySqlConnection myConnection = new MySqlConnection(DbConnection.ConnString))
            {
                MySqlCommand cmd = new MySqlCommand(_query, myConnection);

                foreach (MySQLParameter mp in list)
                {
                    if (mp.value == "NULL")
                    {
                        cmd.Parameters.AddWithValue(mp.name, DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(mp.name, mp.value);
                    }
                }

                DataSet myDataSet = new DataSet();

                if (resync)
                {
                    try
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                        myDataAdapter.Fill(myDataSet);
                        myDataAdapter.Dispose();
                        myConnection.Close();
                        myConnection.Dispose();
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(20);
                        return Query(_query, list, page, results);
                    }
                }
                else
                {
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                    myDataAdapter.Fill(myDataSet);
                    myDataAdapter.Dispose();
                    myConnection.Close();
                    myConnection.Dispose();
                }
                return myDataSet;
               
            }
        }


        public static DataSet Query(string _query, string list)
        {
            return Query(_query, list.Split(new char[] { ',' }), -1, 0);
        }


        public static DataSet Query(string _query, string[] list)
        {
            return Query(_query, list, -1, 0);
        }


        public static DataSet Query(string _query, string[] list, int page, int results)
        {
            if (page > -1 && results > 0)
            {
                _query += " LIMIT " + results + " OFFSET " + (page - (page == 0 ? 0 : 1));
            }

            using (MySqlConnection myConnection = new MySqlConnection(DbConnection.ConnString))
            {
                MySqlCommand cmd = new MySqlCommand(_query, myConnection);
                List<string> arr = new List<string>();

                int p = _query.IndexOf("?");
                while (p > -1)
                {
                    /*int p1 = _query.IndexOf(",", p);
                    int p2 = _query.IndexOf(" ", p);
                    int p3 = _query.IndexOf(")", p);
                    int p4 = _query.IndexOf(";", p);
                    int pf = MySQL.Json.Utils.Functions.MinorPositive(new int[] { p1, p2, p3, p4 });

                    if (pf == -1)
                    {
                        arr.Add(_query.Substring(p));
                    }
                    else
                    {
                        arr.Add(_query.Substring(p, pf - p));
                    }

                    p = _query.IndexOf("?", p + 1); */

                    int p1 = _query.IndexOf(",", p);
                    int p2 = _query.IndexOf(" ", p);
                    int p3 = _query.IndexOf(")", p);
                    int p4 = _query.IndexOf(";", p);
                    int p5 = _query.IndexOf("+", p);
                    int p6 = _query.IndexOf("-", p);
                    int pf = MySQL.Json.Utils.Functions.MinorPositive(new int[] { p1, p2, p3, p4, p5, p6 });

                    if (pf == -1)
                    {
                        arr.Add(_query.Substring(p));
                    }
                    else
                    {
                        arr.Add(_query.Substring(p, pf - p));
                    }

                    p = _query.IndexOf("?", p + 1);
                }


                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] == "NULL")
                    {
                        cmd.Parameters.AddWithValue(arr[i], DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(arr[i], list[i]);
                    }
                }

                

                if (String.IsNullOrEmpty(DbConnection.DbQueueServer))
                { 
                    DataSet myDataSet = new DataSet();
                    if (resync)
                    {
                        try
                        {
                            MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                            myDataAdapter.Fill(myDataSet);
                            myDataAdapter.Dispose();
                            myConnection.Close();
                            myConnection.Dispose();
                        }
                        catch (Exception err)
                        {
                            Thread.Sleep(20);
                            return Query(_query, list, page, results);
                        }
                    }
                    else
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                        myDataAdapter.Fill(myDataSet);
                        myDataAdapter.Dispose();
                        myConnection.Close();
                        myConnection.Dispose();
                    }

                    return myDataSet;
                } else
                {
                    return LazyQuery(_query, list);
                }
            }
        }


        public static DataSet Query(string _query)
        {
            return Query(_query, -1, 0);
        }


        public static DataSet Query(string _query, int page, int results)
        {
            if (page > -1 && results > 0)
            {
                _query += " LIMIT " + results + " OFFSET " + (page - (page == 0 ? 0 : 1));
            }

            using (MySqlConnection myConnection = new MySqlConnection(DbConnection.ConnString))
            {
                if (String.IsNullOrEmpty(DbConnection.DbQueueServer))
                {
                    DataSet myDataSet = new DataSet();
                    if (resync)
                    {
                        try
                        {
                            MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(_query, myConnection);
                            myDataAdapter.Fill(myDataSet);
                            myDataAdapter.Dispose();
                            myConnection.Close();
                            myConnection.Dispose();
                        }
                        catch (Exception err)
                        {
                            Thread.Sleep(20);
                            return Query(_query, page, results);
                        }
                    }
                    else
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(_query, myConnection);
                        myDataAdapter.Fill(myDataSet);
                        myDataAdapter.Dispose();
                        myConnection.Close();
                        myConnection.Dispose();
                    }
                    return myDataSet;
                } else
                {
                    return LazyQuery(_query, new string[] { });
                }
            }
        }


        public static DataSet IQuery(string _query, string _conn, string[] ilist)
        {
            List<MySQLParameter> list = new List<MySQLParameter>();

            int p = _query.IndexOf("?");
            while (p > -1)
            {
                /*int p1 = _query.IndexOf(",", p);
                int p2 = _query.IndexOf(" ", p);
                int p3 = _query.IndexOf(")", p);
                int p4 = _query.IndexOf(";", p);
                int pf = MySQL.Json.Utils.Functions.MinorPositive(new int[] { p1, p2, p3, p4 });

                if (pf == -1)
                {
                    list.Add(new MySQLParameter(_query.Substring(p), ""));
                }
                else
                {
                    list.Add(new MySQLParameter(_query.Substring(p, pf - p), ""));
                }
                p = _query.IndexOf("?", p + 1);*/

                int p1 = _query.IndexOf(",", p);
                int p2 = _query.IndexOf(" ", p);
                int p3 = _query.IndexOf(")", p);
                int p4 = _query.IndexOf(";", p);
                int p5 = _query.IndexOf("+", p);
                int p6 = _query.IndexOf("-", p);
                int pf = MySQL.Json.Utils.Functions.MinorPositive(new int[] { p1, p2, p3, p4, p5, p6 });

                if (pf == -1)
                {
                    list.Add(new MySQLParameter(_query.Substring(p), ""));
                }
                else
                {
                    list.Add(new MySQLParameter(_query.Substring(p, pf - p), ""));
                }

                p = _query.IndexOf("?", p + 1);
            }


            for (int i = 0; i < list.Count; i++)
            {
                list[i].value = ilist[i];
            }

            return IQuery(_query, _conn, list, -1, 0);
        }


        public static DataSet IQuery(string _query, string _conn, List<MySQLParameter> list)
        {
            return IQuery(_query, _conn, list, -1, 0);
        }


        public static DataSet IQuery(string _query, string _conn, List<MySQLParameter> list, int page, int results)
        {
            if (page > -1 && results > 0)
            {
                _query += " LIMIT " + results + " OFFSET " + (page - (page == 0 ? 0 : 1));
            }

            using (MySqlConnection myConnection1 = new MySqlConnection(_conn))
            {
                MySqlCommand cmd = new MySqlCommand(_query, myConnection1);

                foreach (MySQLParameter mp in list)
                {
                    //cmd.Parameters.Add(mp.name, mp.type, mp.len, mp.value);
                    if (mp.value == "NULL")
                    {
                        cmd.Parameters.AddWithValue(mp.name, DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(mp.name, mp.value);
                    }
                }
                //MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(_query, myConnection1);

                DataSet myDataSet = new DataSet();
                if (resync)
                {
                    try
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                        myDataAdapter.Fill(myDataSet);
                        myDataAdapter.Dispose();
                        myConnection1.Close();
                        myConnection1.Dispose();
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(20);
                        return IQuery(_query, _conn, list, page, results);
                    }
                }
                else
                {
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(cmd);
                    myDataAdapter.Fill(myDataSet);
                    myDataAdapter.Dispose();
                    myConnection1.Close();
                    myConnection1.Dispose();
                }

                return myDataSet;
            }
        }


        public static DataSet IQuery(string _query, string _conn)
        {
            return IQuery(_query, _conn, -1, 0);
        }


        public static DataSet IQuery(string _query, string _conn, int page, int results)
        {
            if (page > -1 && results > 0)
            {
                _query += " LIMIT " + results + " OFFSET " + (page - (page == 0 ? 0 : 1));
            }

            using (MySqlConnection myConnection1 = new MySqlConnection(_conn))
            {

                DataSet myDataSet = new DataSet();
                if (resync)
                {
                    try
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(_query, myConnection1);
                        myDataAdapter.Fill(myDataSet);
                        myDataAdapter.Dispose();
                        myConnection1.Close();
                        myConnection1.Dispose();
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(20);
                        return IQuery(_query, _conn, page, results);
                    }
                }
                else
                {
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(_query, myConnection1);
                    myDataAdapter.Fill(myDataSet);
                    myDataAdapter.Dispose();
                    myConnection1.Close();
                    myConnection1.Dispose();
                }
                return myDataSet;

            }
        }


        public static void Config(string _connString)
        {
            DbConnection.ConnString = _connString;
        }



        static HubConnection connection = null;


        static DataSet LazyQuery(string query, string[] parms)
        {
            DataSet _ds = null;
            //Console.WriteLine("LazyQuery " + query);
            LQuery(query, parms, DbConnection.DbQueueServer, Guid.NewGuid().ToString(), ((DataSet ds) =>
            {
                _ds = ds;
                return true;
            }));

            while (_ds == null) {
                //Console.WriteLine("wainting...");
                Task.Delay(10);
            }

            return _ds;
        }

        static void LQuery(string query, string[] parms, string dbQueueServer, string hash, Func<DataSet, bool> func)
        {
            DbConnection.DbMemoryHash.Add(hash, func);

            if (connection == null)
            {
                //Console.WriteLine("Connection Initlized " + dbQueueServer);
                connection = new HubConnectionBuilder().WithUrl(dbQueueServer).Build();
               

                connection.Closed += async (error) =>
                {
                    await Task.Delay(1);
                };

                connection.On<string, string>("Refused", (descrition, message) =>
                {
                    //Console.WriteLine($"Refuse: {descrition} / {message}");
                });

                connection.On<string, string>("Accepted", (descrition, message) =>
                {
                    //Console.WriteLine($"Accepted: {descrition} / {message}");
                });

                connection.On<string, string>("Disconected", (descrition, message) =>
                {
                    //Console.WriteLine($"Disconected: {descrition} / {message}");
                });

                connection.On<string, string>("QuerySuccess", (_hash, result) =>
                {
                    DataSet ds = JsonConvert.DeserializeObject<DataSet>(result);
                    //Console.WriteLine($"QuerySuccess: {_hash} / {ds.Tables[0].Rows[0][0].ToString()}");
                    DbConnection.DbMemoryHash[_hash](ds);
                    DbConnection.DbMemoryHash.Remove(_hash);
                });

                connection.On<string, string>("QueryError", (descrition, message) =>
                {
                    //Console.WriteLine($"QueryError: {descrition} / {message}");
                });
            }


            try
            {
                connection.StartAsync().ContinueWith(t => {
                    if (t.IsFaulted)
                        Console.WriteLine(t.Exception.GetBaseException());
                    /*else
                        Console.WriteLine("Connected to Hub");*/

                }).Wait();
                connection.InvokeAsync("Connect", Guid.NewGuid());
            }
            catch (Exception err) {

            }

            //Console.WriteLine(DbConnection.DbMemoryHash.Count);
            //Console.WriteLine("Enqueue " + hash + " " + query);
            connection.InvokeAsync("Enqueue", hash, query, parms, 0);
        }
    }
}
