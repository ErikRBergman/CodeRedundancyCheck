namespace CodeRedundancyCheck.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using CodeRedundancyCheck.Extensions;

    public class DivideAndConquerDictionary<T> : IReadOnlyDictionary<int, T>
    {
        public readonly int Length;

        private readonly bool isPadded = false;

        private readonly int[] keys;

        private readonly T[] values;

        private struct IntKeyValuePairComparer : IComparer<KeyValuePair<int, T>>
        {
            public int Compare(KeyValuePair<int, T> x, KeyValuePair<int, T> y)
            {
                return x.Key - y.Key;
            }

            public static IntKeyValuePairComparer Default { get; } = new IntKeyValuePairComparer();
        }

        public DivideAndConquerDictionary(ICollection<KeyValuePair<int, T>> elements, bool isSorted = false)
        {
            var length = elements.Count;
            if (length == 0)
            {
                this.keys = Array.Empty<int>();
                this.values = Array.Empty<T>();
                this.Length = 0;
                return;
            }

            var isEven = (length & 1) == 0;

            if (isEven)
            {
                length++;
            }

            var itemsArray = new KeyValuePair<int, T>[length];
            elements.CopyTo(itemsArray, 0);

            // Always have odd number of items - odd right?
            if (isEven)
            {
                itemsArray[length - 1] = new KeyValuePair<int, T>(int.MaxValue, default(T));
                this.isPadded = true;
            }

            if (isSorted == false)
            {
                Array.Sort(itemsArray, IntKeyValuePairComparer.Default);
            }

            if (isEven)
            {
                // The padding item must be of lower value than it may occur when using the dictionary
                itemsArray[length - 1] = new KeyValuePair<int, T>(int.MinValue, default(T));
            }

            this.keys = itemsArray.ToArray(pair => pair.Key, length);
            this.values = itemsArray.ToArray(pair => pair.Value, length);

            this.Length = length;
        }

        public int[] Keys => this.keys;

        IEnumerable<int> IReadOnlyDictionary<int, T>.Keys => this.Keys;

        public IEnumerable<T> Values => this.values;

        public bool ContainsKey(int key)
        {
            T value;
            return this.TryGetValue(key, out value);
        }

        public bool TryGetValue(int key, out T value)
        {
            var rangeLength = this.Length;
            var rangeIndex = 0;

            do
            {
                // for puny ranges
                if (rangeLength <= 5)
                {
                    var maxValue = rangeIndex + rangeLength;

                    for (var i = rangeIndex; i < maxValue; i++)
                    {
                        if (this.keys[i] == key)
                        {
                            value = this.values[i];
                            return true;
                        }
                    }

                    value = default(T);
                    return false;
                }

                var halfRangeLength = rangeLength >> 1;
                var midpointIndex = rangeIndex + halfRangeLength;
                var midPointKey = this.keys[midpointIndex];

                if (key > midPointKey)
                {
                    var rangeLengthCarry = 1 - (rangeLength & 1);
                    rangeLength = halfRangeLength;

                    rangeIndex = midpointIndex + 1;
                    rangeLength -= rangeLengthCarry;
                }
                else
                {
                    if (midPointKey == key)
                    {
                        value = this.values[midpointIndex];
                        return true;
                    }

                    // rangeIndex remains the same
                    rangeLength = halfRangeLength;
                }
            }
            while (true);
        }

        public T this[int key]
        {
            get
            {
                T value;

                if (this.TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return new KeyValuePairEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => this.Length;

        private struct KeyValuePairEnumerator : IEnumerator<KeyValuePair<int, T>>
        {
            private readonly DivideAndConquerDictionary<T> dictionary;

            private int currentItemIndex;

            private readonly int length;

            public KeyValuePairEnumerator(DivideAndConquerDictionary<T> dictionary)
            {
                this.dictionary = dictionary;
                this.currentItemIndex = -1;
                this.length = dictionary.Length;
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                return (this.currentItemIndex++) < this.length;
            }

            public void Reset()
            {
                this.currentItemIndex = -1;
            }

            public KeyValuePair<int, T> Current
            {
                get
                {
                    var index = this.currentItemIndex;
                    return new KeyValuePair<int, T>(this.dictionary.keys[index], this.dictionary.values[index]);
                }
            }

            object IEnumerator.Current => this.Current;
        }
    }
}