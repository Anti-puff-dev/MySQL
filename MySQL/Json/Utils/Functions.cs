using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL.Json.Utils
{
    public class Functions
    {
        public static int MinorPositive(int[] arr)
        {
            int p = arr[0];
            for(int i=1; i<arr.Length; i++)
            {
                if ((arr[i] < p && arr[i] > -1 && p > -1) || (p==-1)) p = arr[i];
            }

            return p;
        }

        public static T JsonToObject<T>(string json)
        {
            T deserialized = JsonConvert.DeserializeObject<T>(json);
            return deserialized;
        }


        public static string ObjectToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
