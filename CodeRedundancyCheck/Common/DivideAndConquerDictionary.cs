using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Common
{
    using System.Diagnostics;

    public class DivideAndConquerDictionary<T>
    {
        private readonly int[] keys;

        private readonly T[] values;

        public readonly int Length;

        private readonly bool isPadded = false;

        public int[] Keys => this.keys;

        public IEnumerable<T> Values => this.values;

        public DivideAndConquerDictionary(IEnumerable<KeyValuePair<int, T>> items)
        {
            var itemsList = items.OrderBy(item => item.Key).ToList();

            // Allways have odd number of items
            if ((itemsList.Count & 1) == 0)
            {
                itemsList.Add(new KeyValuePair<int, T>(0, default(T)));
                this.isPadded = true;
            }

            //while ((itemsList.Count & 3) != 3)
            //{
            //    itemsList.Add(new KeyValuePair<int, T>(0, default(T)));
            //    this.isPadded = true;
            //}


            this.keys = itemsList.Select(i => i.Key).ToArray();
            this.values = itemsList.Select(i => i.Value).ToArray();
            this.Length = itemsList.Count;
        }

        public bool TryGetValue(int key, out T value)
        {
            var rangeLength = this.Length;
            var rangeIndex = 0;

            int iterationCount = 0;

            do
            {
                iterationCount++;

                // Debug.WriteLine("Iteration: " + iterationCount + ", rangeIndex: " + rangeIndex + ", rangeLength: " + rangeLength + ", last range index: " + (rangeIndex + rangeLength - 1) + ", length last bit: " + (rangeLength & 1));

                // for puny ranges
                if (rangeLength < 3)
                {
                    for (int i = rangeIndex; i < rangeIndex + rangeLength; i++)
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


                // divided by 2 is always floor
                var midpointIndex = rangeIndex + (rangeLength / 2);

                var midPointKey = this.keys[midpointIndex];

                // Debug.WriteLine("Midpoint index: " + midpointIndex);


                // var zz = this.keys[rangeIndex + rangeLength - 1];


                // Play around chaning order of the if statements when it's working
                if (midPointKey == key)
                {
                    value = this.values[midpointIndex];
                    return true;
                }

                var rangeLengthOdd = (rangeLength & 1) == 0;

                rangeLength = rangeLength / 2;


                if (key > midPointKey)
                {
                    rangeIndex = midpointIndex + 1;

                    if (rangeLengthOdd)
                    {
                        rangeLength--;
                    }
//                    rangeLength--;
                }
                else
                {
                    // rangeIndex remains the same
                }

            }
            while (true);

        }


    }
}
