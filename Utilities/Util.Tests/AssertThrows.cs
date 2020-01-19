/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests
{
    public static partial class UnitTestHelper
    {
        public delegate void AssertThrowsDelegate();

        public static void AssertThrows<E>(AssertThrowsDelegate code) where E : System.Exception
        {
            bool exceptionNotThrown = false;
            try
            {
                code();
                exceptionNotThrown = true;
            }
            catch (System.Exception e)
            {
                Assert.AreEqual(typeof(E).FullName, e.GetType().FullName);
                //pass
            }
            if(exceptionNotThrown)
                Assert.Fail("No exception was thrown");
        }

        public static void AssertThrows<E>(string expectedMessage, AssertThrowsDelegate code) where E : System.Exception
        {
            bool exceptionNotThrown = false;
            try
            {
                code();
                exceptionNotThrown = true;
            }
            catch (System.Exception e)
            {
                Assert.AreEqual(typeof(E).FullName, e.GetType().FullName);
                Assert.AreEqual(expectedMessage, e.Message);
                //pass
            }
            if (exceptionNotThrown)
                Assert.Fail("No exception was thrown");
        }

        public static void TrapInconclusiveException<E>(AssertThrowsDelegate code) where E : System.Exception
        {
            try
            {
                code();
            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(E))
                    Assert.Inconclusive(e.ToString());
                else
                    throw;
            }
        }
    }
}
