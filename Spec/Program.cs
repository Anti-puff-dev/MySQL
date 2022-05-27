using System;
using System.IO;
using MySQL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Data;

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
            /*var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
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


            Edital ed1 = MySQL.Json.Select.Fill("SELECT edital_id, titulo, descricao, data_cadastro FROM editais WHERE edital_id=?edital_id LIMIT 1", new string[] { "2" }).Cache("editais1", "daily", true, MySQL.DbConnection.CacheConnString).Single<Edital>();
            Console.WriteLine($"{ed1.edital_id} {ed1.titulo} {ed1.data_cadastro}");
            */

            //TestTree();
            TestQueue();
            Console.ReadKey();
        }

        static void TestTree()
        {
            /*string jtext = "{ payment: { mode: \"default\", method: \"creditCard\", sender: { name: \"Teste\", email: \"\", phone: { areaCode: \"\", number: \"\" }, documents:  [{ document: { type: \"CPF\", value: \"333\" } }] , hash: \"\" }, currency: \"BRL\", notificationURL: \"https://lgpdoctor.com/api/Pagseguro/Notification\", items: [ { item: { id: \"\", description: \"\", quantity: 1, amount: 0.00 } } ], extraAmount: 0.00, reference: \"\", shipping: { addressRequired: false }, creditCard: { token: \"\", installment: { quantity: 1, value: 0.00 }, holder: { name: \"\", documents: [ { document: { type: \"CPF\", value: \"\" } } ], birthDate: \"00/00/0000\", phone: { areaCode: \"\", number: \"\" }, }, billingAddress: { street: \"Av. Brigadeiro Faria Lima\", number: \"1384\", complement: \"1 andar\", district: \"Jardim Paulistano\", city: \"Sao Paulo\", state: \"SP\", country: \"BRA\", postalCode: \"01452002\" } } } }";

            Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(jtext);
            Console.WriteLine(MySQL.Json.Utils.Exists.TreeString(json, new string[] { "payment", "sender", "documents[0]", "document", "value" }));*/

            string text = "{birthDate: \"10/05/1978\", brand: 101, cardName: \"Felipe Martins dos Santos\", cardNumber: \"4111111111111111\", celular: \"(12) 98122-1043\", cnpj: \"11.111.111/1111-11\", cpf: \"315.757.808-10\", cvv: \"123\", email: \"comprador@sandbox.pagseguro.com.br\", empresa: \"Empresa Teste\", nome: \"Felipe Martins dos Santos\", pacote: 1, payment: { mode: \"default\", method: \"creditCard\", sender: { name: \"Teste\", email: \"\", phone: { areaCode: \"\", number: \"\" }, documents: [ { document: { type: \"CPF\", value: \"\" } } ], hash: \"\" }, currency: \"BRL\", notificationURL: \"\", items: [ { item: { id: \"\", description: \"\", quantity: 1, amount: 0.00 } } ], extraAmount: 0.00, reference: \"\", shipping: { addressRequired: false }, creditCard: { token: \"\", installment: { quantity: 1, value: 0.00 }, holder: { name: \"\", documents: [ { document: { type: \"CPF\", value: \"\" } } ], birthDate: \"00/00/0000\", phone: { areaCode: \"\", number: \"\" }, }, billingAddress: { street: \"Av. Brigadeiro Faria Lima\", number: \"1384\", complement: \"1 andar\", district: \"Jardim Paulistano\", city: \"Sao Paulo\", state: \"SP\", country: \"BRA\", postalCode: \"01452002\" } } }, validade: \"12/2030\"}";

            Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(text);
            Console.WriteLine(MySQL.Json.Utils.Exists.String(json, "cardName"));

            Newtonsoft.Json.Linq.JObject payment = MySQL.Json.Utils.Exists.JObject(json, "payment");
            Console.WriteLine(MySQL.Json.Utils.Exists.String(payment, "mode"));

            Console.WriteLine(MySQL.Json.Utils.Exists.TreeString(json, new string[] { "payment", "mode" }));
            Console.WriteLine(MySQL.Json.Utils.Exists.TreeString(json, new string[] { "payment", "sender", "name" }));
        }


        static void TestQueue()
        {
            //MySQL.DbConnection.ConnString = "";
            //MySQL.DbConnection.CacheConnString = "";
            DbConnection.DbQueueServer = "";

            DataSet ds = Data.Query("SELECT * FROM users LIMIT 10", new string[] { });
      

            foreach (DataRow row1 in ds.Tables[0].Rows)
            {
                Console.WriteLine( row1[0].ToString());
            }
        }
    } 
}
