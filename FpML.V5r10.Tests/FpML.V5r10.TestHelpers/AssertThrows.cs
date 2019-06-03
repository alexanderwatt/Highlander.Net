using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.TestHelpers
{
    public static partial class UnitTestHelper
    {
        public delegate void AssertThrowsDelegate();

        public static void AssertThrows<E>(AssertThrowsDelegate code) where E : Exception
        {
            bool exceptionNotThrown = false;
            try
            {
                code();
                exceptionNotThrown = true;
            }
            catch (Exception e)
            {
                Assert.AreEqual<string>(typeof(E).FullName, e.GetType().FullName);
                //pass
            }
            if(exceptionNotThrown)
                Assert.Fail("No exception was thrown");
        }
        public static void AssertThrows<E>(string expectedMessage, AssertThrowsDelegate code) where E : Exception
        {
            bool exceptionNotThrown = false;
            try
            {
                code();
                exceptionNotThrown = true;
            }
            catch (Exception e)
            {
                Assert.AreEqual<string>(typeof(E).FullName, e.GetType().FullName);
                Assert.AreEqual<string>(expectedMessage, e.Message);
                //pass
            }
            if (exceptionNotThrown)
                Assert.Fail("No exception was thrown");
        }
        public static void TrapInconclusiveException<E>(AssertThrowsDelegate code) where E : Exception
        {
            try
            {
                code();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(E))
                    Assert.Inconclusive(e.ToString());
                else
                    throw;
            }
        }
    }
}
