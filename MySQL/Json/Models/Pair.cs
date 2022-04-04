using System;
using System.Collections;
using System.Collections.Generic;

namespace MySQL.Json.Models
{
    public class Pair
    {
        public string key { get; set; }
        public string value { get; set; }

        public Pair(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class KeyValueList : IEnumerable<Pair>
    {
        public List<Pair> list = new List<Pair>();

        public void Add(string key, string value)
        {
            list.Add(new Pair(key, value));
        }

        public void Add(string key)
        {
            list.Add(new Pair(key, ""));
        }

        public int Count()
        {
            return list.Count;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //throw new NotImplementedException();

            foreach (Pair t in list)
            {
                if (t == null)
                {
                    break;
                }
                yield return t;
            }
        }

        IEnumerator<Pair> IEnumerable<Pair>.GetEnumerator()
        {
            foreach (Pair t in list)
            {
                if (t == null)
                {
                    break;
                }
                yield return t;
            }

            //throw new NotImplementedException();
        }

        public string Get(string key)
        {
            return list.Find(i => i.key == key).value;
        }

        public List<Pair> ToList()
        {
            return list;
        }

        public bool Contains(string key)
        {
            foreach (Pair p in list)
            {
                if (p.key == key) return true;
            }

            return false;
        }
    }
}
