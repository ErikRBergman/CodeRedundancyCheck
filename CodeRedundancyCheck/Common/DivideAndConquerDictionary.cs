namespace CodeRedundancyCheck.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public class DivideAndConquerDictionary<T>
    {
        public readonly int Length;

        private readonly bool isPadded = false;

        private readonly int[] keys;

        private readonly T[] values;

        public DivideAndConquerDictionary(IEnumerable<KeyValuePair<int, T>> items)
        {
            var itemsList = items.OrderBy(item => item.Key).ToList();

            // Allways have odd number of items
            if ((itemsList.Count & 1) == 0)
            {
                itemsList.Add(new KeyValuePair<int, T>(0, default(T)));
                this.isPadded = true;
            }

            // while ((itemsList.Count & 3) != 3)
            // {
            // itemsList.Add(new KeyValuePair<int, T>(0, default(T)));
            // this.isPadded = true;
            // }
            this.keys = itemsList.Select(i => i.Key).ToArray();
            this.values = itemsList.Select(i => i.Value).ToArray();
            this.Length = itemsList.Count;
        }

        public int[] Keys => this.keys;

        public IEnumerable<T> Values => this.values;

        public bool TryGetValue(int key, out T value)
        {
            var rangeLength = this.Length;
            var rangeIndex = 0;

            // int iterationCount = 0;
            do
            {
                // iterationCount++;

                // Debug.WriteLine("Iteration: " + iterationCount + ", rangeIndex: " + rangeIndex + ", rangeLength: " + rangeLength + ", last range index: " + (rangeIndex + rangeLength - 1) + ", length last bit: " + (rangeLength & 1));

                // for puny ranges
                if (rangeLength < 3)
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

                // Debug.WriteLine("Midpoint index: " + midpointIndex);
                var isRangeLengthEven = (rangeLength & 1) == 0;
                rangeLength = halfRangeLength;

                if (key > midPointKey)
                {
                    rangeIndex = midpointIndex + 1;

                    if (isRangeLengthEven)
                    {
                        rangeLength--;
                    }

                    continue;
                }
                else
                {
                    // rangeIndex remains the same

                    // Play around chaning order of the if statements when it's working
                    if (midPointKey == key)
                    {
                        value = this.values[midpointIndex];
                        return true;
                    }
                }
            }
            while (true);
        }
    }
}