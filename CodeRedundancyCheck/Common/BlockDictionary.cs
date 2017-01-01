using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Common
{
    using System.Collections.Concurrent;

    public class BlockDictionary<TKey, TValue>
    {
        // private ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount * 4, 150000);

        private readonly object lockObject = new object();

        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(150000);

        public bool TryAdd(TKey key, TValue value, Func<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> addIfKeyIsAddedFunc = null)
        {
            lock (this.lockObject)
            {
                var dic = this.dictionary;

                if (dic.ContainsKey(key))
                {
                    return false;
                }

                dic.Add(key, value);

                if (addIfKeyIsAddedFunc != null)
                {
                    var itemsToAdd = addIfKeyIsAddedFunc.Invoke();

                    foreach (var pair in itemsToAdd)
                    {
                        var pairKey = pair.Key;

                        if (dic.ContainsKey(pairKey) == false)
                        {
                            dic.Add(pairKey, pair.Value);
                        }
                    }
                }
            }

            return true;
        }

    }
}
