using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQL
{
    public class CacheConfig
    {
        public string Name { get; set; }
        public string StringConnection { get; set; }
        public string Frequency { get; set; }
        public bool ForceUpdate { get; set; }
    }


    public class Cache
    {
        static List<MySQL.MySQLParameter> parms = new List<MySQL.MySQLParameter>();

        public static T Select<T>(string name, string frequency, T obj, string sconn = "")
        {
            parms.Clear();
            string add = "";

            switch (frequency)
            {
                case "daily":
                    add = " AND cache_object.date='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                    break;

                case "weekly":
                    add = " AND DATEDIFF(NOW(), cache_object.date) < 7";
                    break;
                case "days2":
                    add = " AND DATEDIFF(NOW(), cache_object.date) < 2";
                    break;
                case "update":
                    add = " AND cache_object.check_update<>1";
                    break;

            }

            parms.Add(new MySQL.MySQLParameter("?name", name));
            string result = "";
            try
            {
                if (String.IsNullOrEmpty(sconn))
                {
                    result = Data.Query("SELECT cache_object.object FROM cache_object WHERE cache_object.name=?name" + add, parms).Tables[0].Rows[0][0].ToString();
                } else
                {
                    result = Data.IQuery("SELECT cache_object.object FROM cache_object WHERE cache_object.name=?name" + add, sconn, parms).Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception err)
            {

            }

            if (!String.IsNullOrEmpty(result))
            {
                return JsonToObject<T>(result, obj);
            }

            return default(T);
        }



        public static int? Insert<T>(string name, T obj, string sconn = "")
        {
            parms.Clear();
            parms.Add(new MySQL.MySQLParameter("?name", name));
            parms.Add(new MySQL.MySQLParameter("?datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            parms.Add(new MySQL.MySQLParameter("?date", DateTime.Now.ToString("yyyy-MM-dd")));

            string o = JsonConvert.SerializeObject(obj);
            parms.Add(new MySQL.MySQLParameter("?object", o));
            Int32? id = null;

            try
            {
                id = Convert.ToInt32(Data.Query("SELECT id FROM cache_object WHERE cache_object.name=?name", parms).Tables[0].Rows[0][0]);
                if (id > 0 && id != null)
                {
                    if (String.IsNullOrEmpty(sconn))
                    {
                        Data.Query("UPDATE cache_object SET cache_object.object=?object, cache_object.datetime=?datetime, cache_object.date=?date, cache_object.check_update=0 WHERE cache_object.id=" + id, parms);
                    } else
                    {
                        Data.IQuery("UPDATE cache_object SET cache_object.object=?object, cache_object.datetime=?datetime, cache_object.date=?date, cache_object.check_update=0 WHERE cache_object.id=" + id, sconn, parms);
                    }
                   
                }
                else
                {
                    if (String.IsNullOrEmpty(sconn))
                    {
                        id = Convert.ToInt32(Data.Query("INSERT INTO cache_object (cache_object.name, cache_object.object, cache_object.datetime, cache_object.date,cache_object.check_update) VALUES (?name, ?object, ?datetime, ?date, 0);SELECT LAST_INSERT_ID()", parms).Tables[0].Rows[0][0]);
                    } else
                    {
                        id = Convert.ToInt32(Data.IQuery("INSERT INTO cache_object (cache_object.name, cache_object.object, cache_object.datetime, cache_object.date,cache_object.check_update) VALUES (?name, ?object, ?datetime, ?date, 0);SELECT LAST_INSERT_ID()", sconn, parms).Tables[0].Rows[0][0]);
                    }
                }
            }
            catch (Exception err)
            {
                if (String.IsNullOrEmpty(sconn))
                {
                    id = Convert.ToInt32(Data.Query("INSERT INTO cache_object (cache_object.name, cache_object.object, cache_object.datetime, cache_object.date,cache_object.check_update) VALUES (?name, ?object, ?datetime, ?date, 0);SELECT LAST_INSERT_ID()", parms).Tables[0].Rows[0][0]);
                } else
                {
                    id = Convert.ToInt32(Data.IQuery("INSERT INTO cache_object (cache_object.name, cache_object.object, cache_object.datetime, cache_object.date,cache_object.check_update) VALUES (?name, ?object, ?datetime, ?date, 0);SELECT LAST_INSERT_ID()", sconn, parms).Tables[0].Rows[0][0]);
                }
            }

            return id;
        }


        public static bool CheckUpdate(string name, bool v, string sconn = "")
        {
            parms.Clear();
            parms.Add(new MySQL.MySQLParameter("?bupdate", v ? "1" : "0"));
            parms.Add(new MySQL.MySQLParameter("?name", name));
            if (String.IsNullOrEmpty(sconn))
            {
                Data.Query("UPDATE cache_object SET cache_object.check_update=?bupdate WHERE cache_object.name=?name", parms);
            } else
            {
                Data.IQuery("UPDATE cache_object SET cache_object.check_update=?bupdate WHERE cache_object.name=?name", sconn, parms);
            }

            return v;
        }


        public static void Delete(string name, string sconn = "")
        {
            parms.Clear();
            parms.Add(new MySQL.MySQLParameter("?name", name));
            if (String.IsNullOrEmpty(sconn))
            {
                Data.Query("DELETE FROM cache_object WHERE name=?name", parms);
            }
            else
            {
                Data.IQuery("DELETE FROM cache_object WHERE name=?name", sconn, parms);
            }
        }


        public static T JsonToObject<T>(string json, T obj)
        {
            T deserialized = JsonConvert.DeserializeObject<T>(json);
            return deserialized;
        }
    }
}
