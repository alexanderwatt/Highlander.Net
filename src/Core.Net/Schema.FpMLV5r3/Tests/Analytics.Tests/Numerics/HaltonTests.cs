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

using System;
using System.Diagnostics;
using System.Globalization;
using Highlander.Reporting.Analytics.V5r3.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// </summary>
    [TestClass]
    public class HaltonTests
    {
        [TestMethod]
        public void TestHalton()
        {
            for (var m = 1; m <= 3; m++)
            {
                int i;
                for (i = 0; i <= 10; i++)
                {
                    var r = HaltonSequence.Halton(i, m);
                    int j;
                    for (j = 0; j < m; j++)
                    {
                        Debug.Print(r[j].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        [TestMethod]
        public void TestHaltonBase()
        {
            var b1 = new[] { 2, 3, 5 };
            var b2 = new[] { 3, 10, 2 };
            int i;
            int j;
            double[] r;
            var m = 3;
            for (j = 0; j < m; j++)
            {
                Debug.Print(b1[j].ToString(CultureInfo.InvariantCulture));
            }           
            for (i = 0; i <= 10; i++)
            {
                r = HaltonSequence.HaltonBase(i, m, b1);
                Debug.Print(i.ToString(CultureInfo.InvariantCulture));
                for (j = 0; j < m; j++)
                {
                    Debug.Print(r[j].ToString(CultureInfo.InvariantCulture));
                }
            }
            for (j = 0; j < m; j++)
            {
                Debug.Print(b2[j].ToString(CultureInfo.InvariantCulture));
            }
            for (i = 0; i <= 10; i++)
            {
                r = HaltonSequence.HaltonBase(i, m, b2);
                for (j = 0; j < m; j++)
                {
                    Debug.Print(r[j].ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        [TestMethod]
        public void TestHaltonSequence()
        {
            for (var m = 1; m <= 3; m++)
            {
                var r = HaltonSequence.Sequence(0, 10, m);
                Debug.Print(r[m].ToString(CultureInfo.InvariantCulture));
            }
            var s = HaltonSequence.Sequence(10, 0, 2);
            Debug.Print(s[0].ToString(CultureInfo.InvariantCulture));
            Debug.Print(s[1].ToString(CultureInfo.InvariantCulture));
        }

        public void TestHalton2()
        {
            //("Testing Halton sequences...");
            double[] point;
            double tolerance = 1.0e-15;
            // testing "high" dimensionality
            int dimensionality = 2;
            int points = 100, i;
            for (i = 0; i < points; i++)
            {
                point = HaltonSequence.Halton(i, dimensionality);
                Assert.AreEqual(point.Length, dimensionality);
            }
            // testing first and second dimension (van der Corput sequence)
            double[] vanderCorputSequenceModuloTwo =
            {
                // first cycle (zero excluded)
                0.50000,
                // second cycle
                0.25000, 0.75000,
                // third cycle
                0.12500, 0.62500, 0.37500, 0.87500,
                // fourth cycle
                0.06250, 0.56250, 0.31250, 0.81250, 0.18750, 0.68750, 0.43750,
                0.93750,
                // fifth cycle
                0.03125, 0.53125, 0.28125, 0.78125, 0.15625, 0.65625, 0.40625,
                0.90625,
                0.09375, 0.59375, 0.34375, 0.84375, 0.21875, 0.71875, 0.46875,
                0.96875,
            };
            dimensionality = 1;
            points = (int)Math.Pow(2.0, 5) - 1; // five cycles
            for (i = 0; i < points; i++)
            {
                point = HaltonSequence.Halton(i, dimensionality);
                double error = Math.Abs(point[0] - vanderCorputSequenceModuloTwo[i]);
                if (error > tolerance)
                {
                    Debug.Print(i + 1 + " draw ("
                                 + +point[0]
                                 + ") in 1-D Halton sequence is not in the "
                                 + "van der Corput sequence modulo two: "
                                 + "it should have been "
                                 + vanderCorputSequenceModuloTwo[i]
                                 + " (error = " + error + ")");
                }
            }
            double[] vanderCorputSequenceModuloThree =
            {
                // first cycle (zero excluded)
                1.0/3, 2.0/3,
                // second cycle
                1.0/9, 4.0/9, 7.0/9, 2.0/9, 5.0/9, 8.0/9,
                // third cycle
                1.0/27, 10.0/27, 19.0/27, 4.0/27, 13.0/27, 22.0/27,
                7.0/27, 16.0/27, 25.0/27, 2.0/27, 11.0/27, 20.0/27,
                5.0/27, 14.0/27, 23.0/27, 8.0/27, 17.0/27, 26.0/27
            };
            dimensionality = 2;
            points = (int)Math.Pow(3.0, 3) - 1; // three cycles of the higher dimension
            for (i = 0; i < points; i++)
            {
                point = HaltonSequence.Halton(i, dimensionality);
                double error = Math.Abs(point[0] - vanderCorputSequenceModuloTwo[i]);
                if (error > tolerance)
                {
                    Debug.Print("First component of " + i + 1
                                 + " draw (" + +point[0]
                                 + ") in 2-D Halton sequence is not in the "
                                 + "van der Corput sequence modulo two: "
                                 + "it should have been "
                                 + vanderCorputSequenceModuloTwo[i]
                                 + " (error = " + error + ")");
                }
                error = Math.Abs(point[1] - vanderCorputSequenceModuloThree[i]);
                if (error > tolerance)
                {
                    Debug.Print("Second component of " + i + 1
                                 + " draw (" + +point[1]
                                 + ") in 2-D Halton sequence is not in the "
                                 + "van der Corput sequence modulo three: "
                                 + "it should have been "
                                 + vanderCorputSequenceModuloThree[i]
                                 + " (error = " + error + ")");
                }
            }
        }
    }
}