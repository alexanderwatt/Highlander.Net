/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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

namespace Highlander.TestHelpers.V5r3
{
    public static class AssertExtension
    {
        public static void LessOrEqual(int item1, int item2)
        {
            Assert.IsTrue(item1 <= item2);
        }

        public static void LessOrEqual(double item1, double item2)
        {
            Assert.IsTrue(item1 <= item2);
        }

        public static void Less(int item1, int item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Less(double item1, double item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Less(decimal item1, decimal item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Greater(decimal item1, decimal item2)
        {
            Assert.IsTrue(item1 > item2);
        }
    }
}
