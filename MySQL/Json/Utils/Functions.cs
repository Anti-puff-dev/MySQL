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
    }
}
