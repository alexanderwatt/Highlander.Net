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

#region Using directives

using System.Collections;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// </summary>
    [TestClass]
    public class ConcatCollectionTests
    {
        /// <summary>
        /// Testing the method <see cref="Collection.ConcatCollection.Count"/>.
        /// </summary>
        [TestMethod]
        public void ConcatCount()
        {
            int[] array0 = new int[0], array1 = new int[7], array2 = new int[13];

            Assert.AreEqual(array0.Length + array0.Length,
                            (new Collection.ConcatCollection(array0, array0)).Count, "#A00");
            Assert.AreEqual(array0.Length + array1.Length,
                            (new Collection.ConcatCollection(array0, array1)).Count, "#A01");
            Assert.AreEqual(array0.Length + array2.Length,
                            (new Collection.ConcatCollection(array0, array2)).Count, "#A02");

            Assert.AreEqual(array1.Length + array0.Length,
                            (new Collection.ConcatCollection(array1, array0)).Count, "#A03");
            Assert.AreEqual(array1.Length + array1.Length,
                            (new Collection.ConcatCollection(array1, array1)).Count, "#A04");
            Assert.AreEqual(array1.Length + array2.Length,
                            (new Collection.ConcatCollection(array1, array2)).Count, "#A05");
        }

        /// <summary>
        /// Testing the method <see cref="Collection.ConcatCollection.GetEnumerator"/>.
        /// </summary>
        [TestMethod]
        public void ConcatGetEnumerator()
        {
            // generating two arrays
            int[] array1 = new int[10], array2 = new int[13];
            for (int i = 0; i < array1.Length; i++) array1[i] = i;
            for (int i = 0; i < array2.Length; i++) array2[i] = i + array1.Length;

            Collection.ConcatCollection union = new Collection.ConcatCollection(array1, array2);

            int index = 0;

            foreach (int value in union)
            {
                Assert.AreEqual(index++, value, "#A00 Unexpected value in collection.");
            }

            Assert.AreEqual(array1.Length + array2.Length, index,
                            "#A01 Unexpected count of enumerated element in collection.");
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void InterGetEnumerator()
        {
            int LENGTH = 100;
            int[] array1 = new int[LENGTH], array2 = new int[LENGTH];

            for (int i = 0; i < LENGTH; i++)
            {
                array1[i] = i;
                array2[i] = i / 2;
            }

            ICollection intersection = Collection.Inter(array1, array2);

            Assert.AreEqual(LENGTH / 2, intersection.Count,
                            "#A00 Unexpected intersection count.");

            foreach (int i in intersection)
                Assert.IsTrue(i >= 0 && i <= LENGTH,
                              "#A01-" + i + " Unexpected intersection item.");
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void MinusGetEnumerator()
        {
            int LENGTH = 100;
            int[] array1 = new int[LENGTH], array2 = new int[LENGTH];

            for (int i = 0; i < LENGTH; i++)
            {
                array1[i] = i;
                array2[i] = i / 2;
            }

            ICollection minus = Collection.Minus(array1, array2);

            Assert.AreEqual(LENGTH / 2, minus.Count,
                            "#A00 Unexpected minus count.");

            foreach (int i in minus)
                Assert.IsTrue(i >= LENGTH / 2,
                              "#A01-" + i + " Unexpected minus item.");
        }

        /// <summary>
        /// Testing the method <see cref="UnionCollection.GetEnumerator"/>.
        /// </summary>
        [TestMethod]
        public void UnionGetEnumerator()
        {
            int LENGTH = 100;

            int[] array1 = new int[LENGTH], array2 = new int[LENGTH];

            for (int i = 0; i < LENGTH; i++)
            {
                array1[i] = i;
                array2[i] = i / 2;
            }

            ICollection union = Collection.Union(array1, array2);

            Assert.AreEqual(LENGTH, union.Count,
                            "#A00 Unexpected union count.");

            foreach (int i in union)
                Assert.IsTrue(i >= 0 && i < LENGTH,
                              "#A01-" + i + " Unexpected union item.");
        }
    }
}