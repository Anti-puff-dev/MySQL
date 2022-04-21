# MySQL Query-Object Class  
Class to convert query to object (MySQL/MariaDB)  

### Important Dll Import
StringUtils.dll  (https://github.com/Anti-puff-dev/StringUtils)

### Example 1 - Select  
Filter fields on query results with: <b>fieldnameFilter</b>   
Return Array Objects or  Single Object (for single result)  

```
class Edital
{
    public int edital_id { get; set; }
    public string titulo { get; set; }
    public string descricao { get; set; }
    public string data_cadastro { get; set; }

    public string data_cadastroFilter(object o)
    {
        string t = "";
        try
        {
            t = Convert.ToDateTime(o).ToString("dd/MM/yyyy HH:mm");
            if (String.IsNullOrEmpty(t)) t = "";
        }
        catch (Exception err)
        {
            t = "";
        }
        return t;
    }
}
    
class Program
{
    static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
        var configuration = builder.Build();

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        MySQL.DbConnection.ConnString = configuration["DefaultConnectionString"];
        MySQL.DbConnection.CacheConnString = configuration["CacheConnectionString"];


        Edital[] editais = MySQL.Json.Select.Fill(Data.Query("SELECT edital_id, titulo, descricao, data_cadastro FROM editais LIMIT 10", new string[] { })).Multiple<Edital>();

        foreach (Edital ed in editais)
        {
            Console.WriteLine($"{ed.edital_id} {ed.titulo} {ed.data_cadastro}");
        }

        Edital ed1 = MySQL.Json.Select.Fill("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id LIMIT 1", new string[] { "2" }).Cache("editais1", "daily", true, MySQL.DbConnection.CacheConnString).Single<Edital>();
        Console.WriteLine($"{ed1.edital_id} {ed1.titulo} {ed1.data_cadastro}");

        Console.ReadKey();
    }
}
```

# Return DataSet 
```
Data.Query("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id", new string[] { "2" });
```

# Return DataSet with connection string (can connect external databases)  
```
Data.IQuery("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id", "server=localhost; user id=root; password=pwd; port=3306; database=dbname; Allow Zero Datetime=True;Allow User Variables=True;CharSet=utf8", new string[] { "2" });
```

# Use temporary caches  (You can connect external databases for caching data) 
Example renew cache daily at first runing 
You can force renew cache using <b>true</b> in param <b>force</b> 
```
Edital ed1 = MySQL.Json.Select.Fill("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id LIMIT 1", new string[] { "2" }).Cache("editais1", "daily", false, MySQL.DbConnection.CacheConnString).Single<Edital>();
```

Renew Methods at Cache.cs    
daily  
weekly  
2 days  
if update field = 0 (set update field value = 1 then cache will be renewed) 

# Caching Selects into MySQL table 
First you must create table "cache_object" in CacheDatabse (CHARACTER and COLLATE can be changend)
```
CREATE TABLE `cache_object` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `object` longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci,
  `datetime` datetime DEFAULT NULL,
  `date` date DEFAULT NULL,
  `check_update` int(1) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `name` (`name`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1
```


# Insert And Update
Normal Query
```
Data.Query("INSERT INTO table (id, name) VALUES (?id, ?name)", new string[] { "2", "Your name" });
```

From JObject (good for webapis and mvc)
```
[HttpPost("Save")]
public string Save([FromBody]Newtonsoft.Json.Linq.JObject data)
{
  string user_id = MySQL.Json.Utils.Exists.String(data, "user_id")

  MySQL.Json.Models.KeyValueList klv = new MySQL.Json.Models.KeyValueList();
  klv.Add("timeout"); //fields in data but not in table
  
  if (String.IsNullOrEmpty(user_id))
  {
      user_id = MySQL.Json.Insert.Into(data, "users").Exceptions(klv).Run().ToString();
  }
  else
  {
      MySQL.Json.Models.KeyValueList klv1 = new MySQL.Json.Models.KeyValueList();
      klv1.Add("user_id", user_id);
      MySQL.Json.Update.Table(data, "users").Exceptions(klv).Where(klv1).Set();
  }
  
  return user_id;
}
```

# Delete
```
MySQL.Json.Models.KeyValueList klv1 = new MySQL.Json.Models.KeyValueList();
klv1.Add("categoria_id", categoria_id);
MySQL.Json.Delete.From("categorias").Where(klv1).Run();
```



