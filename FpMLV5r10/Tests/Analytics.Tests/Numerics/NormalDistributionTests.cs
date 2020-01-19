#region Using directives

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;

#endregion

namespace Orion.Analytics.Tests.Numerics
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