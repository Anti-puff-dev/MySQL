using System;
using System.Collections.Generic;

namespace MySQL.Json.Models
{
    public class QueryList
    {
        public string table { get; set; }
        public List<String> fields { get; set; }
        public List<String> values { get; set; }
        public List<MySQL.MySQLParameter> parms { get; set; }
        public bool master { get; set; }
        public string dependency { get; set; }

        public QueryList(string table, List<String> fields, List<String> values, List<MySQL.MySQLParameter> parms)
        {
            this.table = table;
            this.fields = fields;
            this.values = values;
            this.parms = parms;
        }

        public QueryList(string table, List<String> fields, List<String> values, List<MySQL.MySQLParameter> parms, bool master, string dependency)
        {
            this.table = table;
            this.fields = fields;
            this.values = values;
            this.parms = parms;
            this.master = master;
            this.dependency = dependency;
        }
    }
}
