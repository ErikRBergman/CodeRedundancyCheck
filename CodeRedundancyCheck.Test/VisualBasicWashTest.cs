//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace CodeRedundancyCheck.Test
//{
//    [TestClass]
//    public class VisualBasicWashTest
//    {
//        [TestMethod]
//        public void TestSplit()
//        {
//            var wash = new VisualBasicSourceWash();

//            // ABC "NISSE" CBA """Olle""X" 
//            var result = wash.SplitIntoExpressions("ABC DEF \"NI \t SE\"\tCBA \"\"\"Olle\"\"X\" ");

//            // Expects:
//            // 'ABC'
//            // '"NISSE"'
//            // ' CBA '
//            // '"""Olle""X"'

//            Assert.IsNotNull(result);
//            Assert.AreEqual(5, result.Count);

//            Assert.AreEqual("ABC", result[0].Text);
//            Assert.AreEqual(1, result[0].Line);
//            Assert.AreEqual(1, result[0].Position);
            
//            Assert.AreEqual("DEF", result[1].Text);

//            Assert.AreEqual("\"NI \t SE\"", result[2].Text);

//            Assert.AreEqual("CBA", result[3].Text);

//            Assert.AreEqual("\"\"\"Olle\"\"X\"", result[4].Text);


//        }
//    }
//}
