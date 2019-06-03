using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Orion.Util.Helpers;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for TripletTest and is intended
    ///to contain all TripletTest Unit Tests
    ///</summary>
    [TestClass]
    public class TripletTest
    {
        /// <summary>
        ///A test for Triplet`3 Constructor
        ///</summary>
        private static void TripletConstructorTestHelper<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            T1 first = value1;
            T2 second = value2;
            T3 third = value3;
            Triplet<T1, T2, T3> target = new Triplet<T1, T2, T3>(first, second, third);
            Assert.IsNotNull(target);
            Assert.AreEqual(value1, target.First);
            Assert.AreEqual(value2, target.Second);
            Assert.AreEqual(value3, target.Third);
        }

        [TestMethod]
        public void TripletConstructorTest()
        {
            TripletConstructorTestHelper("a", 3, 2.3);
            TripletConstructorTestHelper(DateTime.Today, 3m, 'c');
        }
    }
}