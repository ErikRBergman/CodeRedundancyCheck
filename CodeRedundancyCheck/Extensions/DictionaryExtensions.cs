using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Extensions
{
    using System.Collections.Concurrent;

    public static class DictionaryExtensions
    {
        public static bool DoesNotContainAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey item1, TKey item2)
        {
            return dictionary.ContainsKey(item1) == false || dictionary.ContainsKey(item2) == false;
        }


        public static void AddMultiple<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey item1, TKey item2, TValue value)
        {
            dictionary.TryAdd(item1, value);
            dictionary.TryAdd(item2, value);
        }



    }
}
