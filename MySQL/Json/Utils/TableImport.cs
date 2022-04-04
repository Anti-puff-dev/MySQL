using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json.Utils
{
    public class ImportResult
    {
        public DataSet Data { get; set; }
        public int LastId { get; set; }
    }



    public class TableImport
    {
        public static ImportResult Run(string origin_connection, string table, string field_id, int inital_id, int limit = 0)
        {
            string query = "SELECT * FROM " + table + " WHERE " + field_id + ">=" + inital_id + (limit>0?" LIMIT " + limit:"");
            //Console.WriteLine(query);
            DataSet ds = Data.IQuery(query, origin_connection);

            int rcount = ds.Tables[0].Rows.Count;

            ImportResult ir = new ImportResult();
            ir.Data = ds;
            try
            {
                ir.LastId = Convert.ToInt32(ds.Tables[0].Rows[rcount - 1][field_id]);
            } catch(Exception err)
            {
                ir.LastId = inital_id;
            }
            return ir;
        }
    }
}
