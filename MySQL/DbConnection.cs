using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySQL
{
    public class DbConnection
    {
        public static string ConnString = "";
        public static string CacheConnString = "";

        public static string DbQueueServer = "";
        public static Dictionary<string, Func<DataSet, bool>> DbMemoryHash = new Dictionary<string, Func<DataSet, bool>>();
    }
}
