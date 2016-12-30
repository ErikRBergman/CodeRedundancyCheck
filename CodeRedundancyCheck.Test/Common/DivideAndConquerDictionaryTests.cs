using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRedundancyCheck.Test.Common
{
    using System.Diagnostics;

    using CodeRedundancyCheck.Common;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DivideAndConquerDictionaryTests
    {
        [TestMethod]
        public void TestTryGetValue5Items()
        {

            var dic1 = new DivideAndConquerDictionary<int>(GetPairFromInts(100, 200, 300, 400, 500));

            int result;

            var found = dic1.TryGetValue(100, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(100, result);


            found = dic1.TryGetValue(300, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(300, result);

            found = dic1.TryGetValue(500, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(500, result);

            found = dic1.TryGetValue(200, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(200, result);


            found = dic1.TryGetValue(250, out result);
            Assert.IsFalse(found);

        }


        [TestMethod]
        public void TestTryGetValue11Items()
        {
            var range = Enumerable.Range(1, 11).Select(v => v * 100).ToArray();

            var dic1 = new DivideAndConquerDictionary<int>(GetPairFromInts(range));
            int result;

            Assert.IsFalse(dic1.TryGetValue(Int32.MaxValue, out result));


            foreach (var value in range)
            {
                var found = dic1.TryGetValue(value, out result);
                Assert.IsTrue(found);
                Assert.AreEqual(value, result);
            }

            Assert.IsFalse(dic1.TryGetValue(-245, out result));
        }

        [TestMethod]
        public void TestTryGetValueLotsOfItems()
        {
            var range = new List<int>(5000000);

            var random = new Random(123);

            for (int i = 0; i < range.Capacity; i++)
            {
                var value = random.Next();              
                range.Add(value);
            }

            var dic1 = new DivideAndConquerDictionary<int>(GetPairFromInts(range));

            int result;

            var found1118868921 = dic1.TryGetValue(1118868921, out result);
            Assert.IsTrue(found1118868921);

            var foundz = dic1.TryGetValue(int.MaxValue, out result);
            
            foreach (var value in range)
            {

                //var pos = -1;

                //for (int i = 0; i < dic1.Keys.Length; i++)
                //{
                //    if (dic1.Keys[i] == value)
                //    {
                //        pos = i;
                //    }
                //}

                //Assert.IsTrue(pos != -1);

                //Debug.WriteLine("\r\n********\r\nPosition of the item " + value + " to find: " + pos);

                var found = dic1.TryGetValue(value, out result);
                Assert.IsTrue(found);
                Assert.AreEqual(value, result);
            }

            Assert.IsFalse(dic1.TryGetValue(-245, out result));

        }


        private static ICollection<KeyValuePair<int, int>> GetPairFromInts(params int[] values)
        {
            return values.Select(v => new KeyValuePair<int, int>(v, v)).ToArray();
        }

        private static ICollection<KeyValuePair<int, int>> GetPairFromInts(IEnumerable<int> values)
        {
            return values.Select(v => new KeyValuePair<int, int>(v, v)).ToArray();
        }
    }
}
