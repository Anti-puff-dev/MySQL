using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json
{
    public class Transaction
    {
        string _query = "";
        List<string> _parameters = new List<string>();

        public Transaction()
        {
            _query = "";
            _parameters.Clear();
        }

        public void Add(string query, string[] parameters)
        {
            _query += query + ";";
            foreach(string s in parameters)
            {
                _parameters.Add(s);
            }
        }

        public void Add(string query)
        {
            _query += query + ";";
        }

        public bool Commit()
        {
            string a = "";

            try
            {
                Debug.WriteLine("START TRANSACTION;" + _query.Replace(";;",";") + "SELECT 1;COMMIT;");
                if (_parameters.Count() > 0)
                {

                    a = MySQL.Data.Query("START TRANSACTION;" + _query.Replace(";;", ";") + "SELECT 1;COMMIT;", _parameters.ToArray<string>()).Tables[0].Rows[0][0].ToString();
                }
                else
                {
                    a = MySQL.Data.Query("START TRANSACTION;" + _query.Replace(";;", ";") + "SELECT 1;COMMIT;").Tables[0].Rows[0][0].ToString();
                }
                
                if (a == "1") return true;
            }
            catch(Exception err)
            {

            }

            return false;
        }
    }
}
