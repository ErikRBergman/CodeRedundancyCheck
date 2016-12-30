using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Extensions
{
    public static class IEnumerableExtensions
    {
        public static TDestination[] ToArray<TSource, TDestination>(this IEnumerable<TSource> sourceItems, Func<TSource, TDestination> func, int itemCount)
        {
            var result = new TDestination[itemCount];

            int index = 0;
            foreach (var item in sourceItems)
            {
                result[index++] = func.Invoke(item);
            }

            return result;
        }

    }
}
