using System;
using Orion.Util.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for PairTest and is intended
    ///to contain all PairTest Unit Tests
    ///</summary>
    [TestClass]
    public class PairTest
    {
        /// <summary>
        ///A test for Pair Constructor
        ///</summary>
        private static void PairConstructorTestHelper<T1, T2>(T1 default1, T2 default2)
        {
            T1 first = default1;
            T2 second = default2;
            Pair<T1, T2> target = new Pair<T1, T2>(first, second);
            Assert.IsNotNull(target);
            Assert.AreEqual(default1, target.First);
            Assert.AreEqual(default2, target.Second);
        }

        [TestMethod]
        public void PairConstructorTest()
        {
            PairConstructorTestHelper("value1", 1);
            PairConstructorTestHelper('d', 2.1);
            PairConstructorTestHelper(2.2m, DateTime.Today);
        }
    }
}