using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Extensions
{
    public static class ListExtensions
    {
        public static T LastItem<T>(this List<T> list)
        {
            return list.Count == 0 ? default(T) : list[list.Count - 1];
        }
    }
}
