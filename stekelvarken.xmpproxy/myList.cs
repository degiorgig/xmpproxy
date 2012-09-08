using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stekelvarken.xmpproxy
{
    public class myList<T> : List<T>
    {
        public myList()
        {
        }
        public myList(IEnumerable<T> p) : base (p)
        {
        }
        public void AddRange(IEnumerable<T> range, int len)
        {
            for (int i = 0; i < len; i++)
            {
                this.Add(range.ElementAt(i));
            }
        }
    }
}
