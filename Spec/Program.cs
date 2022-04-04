using System;
using System.IO;
using MySQL;
using Microsoft.Extensions.Configuration;


namespace Spec
{
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


            //Edital[] editais = MySQL.Json.Select.Fill(Data.Query("SELECT edital_id, titulo, descricao, data_cadastro FROM editais LIMIT 10", new string[] { })).Multiple<Edital>();
            Edital[] editais = MySQL.Json.Select.Fill("SELECT edital_id, titulo, descricao, data_cadastro FROM editais LIMIT 10", new string[] { }).Cache("editais10","daily", false, MySQL.DbConnection.CacheConnString).Multiple<Edital>();

            foreach (Edital ed in editais)
            {
                Console.WriteLine($"{ed.edital_id} {ed.titulo} {ed.data_cadastro}");
            }


            Edital ed = MySQL.Json.Select.Fill("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id LIMIT 1", new string[] { "2" }).Cache("editais1", "daily", true, MySQL.DbConnection.CacheConnString).Single<Edital>();
            Console.WriteLine($"{ed.edital_id} {ed.titulo} {ed.data_cadastro}");

            Console.ReadKey();
        }
    }
}
