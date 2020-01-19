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

using Highlander.Reporting.Analytics.V5r3.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// </summary>
    [TestClass]
    public class NormalDistributionTests
    {
        /// <summary>
        /// </summary>
        [TestMethod]
        public void SmallSampleCheck()
        {
            NormalDistribution cnd = new NormalDistribution();
            Assert.AreEqual(0.3989422, cnd.ValueOf(0.0), 1e-6);
            Assert.AreEqual(0.0, cnd.ValueOf(double.PositiveInfinity));
            Assert.AreEqual(0.0, cnd.ValueOf(double.NegativeInfinity));
            // values obtained with Maple 9.01
            Assert.AreEqual(.35206532, cnd.ValueOf(0.5), 1e-6);
            Assert.AreEqual(.35206532, cnd.ValueOf(-0.5), 1e-6);
            Assert.AreEqual(.12951759, cnd.ValueOf(1.5), 1e-6);
            Assert.AreEqual(.12951759, cnd.ValueOf(-1.5), 1e-6);
            cnd = new NormalDistribution(1.0, 2.0);
            Assert.AreEqual(0.0, cnd.ValueOf(1.0), 0.5);
            Assert.AreEqual(0.0, cnd.ValueOf(double.PositiveInfinity));
            Assert.AreEqual(0.0, cnd.ValueOf(double.NegativeInfinity));
            // values obtained with Maple 9.01
            Assert.AreEqual(.193334058, cnd.ValueOf(0.5), 1e-6);
            Assert.AreEqual(.150568716, cnd.ValueOf(-0.5), 1e-6);
            Assert.AreEqual(.193334058, cnd.ValueOf(1.5), 1e-6);
            Assert.AreEqual(.0913245426, cnd.ValueOf(-1.5), 1e-6);
        }
    }
}