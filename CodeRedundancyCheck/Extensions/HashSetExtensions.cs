using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Extensions
{
    public static class HashSetExtensions
    {
        public static bool ContainsAny<T>(this HashSet<T> hashSet, T item1, T item2)
        {
            return hashSet.Contains(item1) || hashSet.Contains(item2);
        }

        public static bool ContainsAny<T>(this HashSet<T> hashSet, T item1, T item2, T item3)
        {
            return hashSet.Contains(item1) || hashSet.Contains(item2) || hashSet.Contains((item3));
        }

        public static bool ContainsAny<T>(this HashSet<T> hashSet, params T[] items)
        {
            foreach (var item in items)
            {
                if (hashSet.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesNotContainAll<T>(this HashSet<T> hashSet, T item1, T item2)
        {
            return hashSet.Contains(item1) == false || hashSet.Contains(item2) == false;
        }


        public static bool ContainsAll<T>(this HashSet<T> hashSet, T item1, T item2)
        {
            return hashSet.Contains(item1) && hashSet.Contains(item2);
        }

        public static bool ContainsAll<T>(this HashSet<T> hashSet, T item1, T item2, T item3)
        {
            return hashSet.Contains(item1) && hashSet.Contains(item2) && hashSet.Contains((item3));
        }

        public static bool ContainsAll<T>(this HashSet<T> hashSet, params T[] items)
        {
            foreach (var item in items)
            {
                if (hashSet.Contains(item) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static void AddMultiple<T>(this HashSet<T> hashSet, T item1, T item2)
        {
            hashSet.Add(item1);
            hashSet.Add(item2);
        }

        public static void AddMultiple<T>(this HashSet<T> hashSet, params T[] items)
        {
            foreach (var item in items)
            {
                hashSet.Add(item);
            }
        }



    }
}
