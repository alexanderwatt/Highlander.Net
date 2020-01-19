#region Using directives

using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.Numerics
{
    [TestClass]
    public class IntDoubleParseTests
    {                                                                                                                                   
        [TestMethod]                                                                  
        public void TestIntParse()                                         
        {
            string sInt = "2323";

            int i = int.Parse(sInt);

            Assert.AreEqual(i, 2323);
        }

        [TestMethod]                                                                  
        public void TestDoubleParse1()                                         
        {
            string sDouble = "2323.3434";

            double d = double.Parse(sDouble);

            Assert.AreEqual(d, 2323.3434);
        }


        //[TestMethod]                      
        //[ExpectedException(typeof(FormatException))]                                    
        //public void TestIntParseFail()                                         
        //{
        //    string sInt = "2323.3434";

        //    int i = int.Parse(sInt);

        //    Assert.AreEqual(i, "Input ");
        //}

        //[TestMethod]
        //[ExpectedException(typeof(FormatException))]
        //public void TestIntParseFail2()                                         
        //{
        //    string sInt = "2323,3434";

        //    int i = int.Parse(sInt);

        //    Assert.AreEqual(i, 2323);
        //}


    }
}