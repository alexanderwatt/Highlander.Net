#region Using directives

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;

#endregion

namespace Orion.Analytics.Tests.Numerics
{
    /// <summary>
    /// Testing the class <see cref="InvCumulativeNormalDistribution"/>.
    /// </summary>
    [TestClass]
    public class InvCumulativeNormalDistributionTests
    {
        /// <summary>
        /// Testing the inverse cumulative normal distribution on few values.
        /// </summary>
        [TestMethod]
        public void SmallSampleCheck()
        {
            InvCumulativeNormalDistribution invCnd = new InvCumulativeNormalDistribution();

            Assert.AreEqual(0.0, invCnd.ValueOf(0.5));

            // values obtained with Maple 9.01
            Assert.AreEqual(0.5, invCnd.ValueOf(.6914624613), 1e-6);
            Assert.AreEqual(-0.5, invCnd.ValueOf(0.3085375387), 1e-6);
            Assert.AreEqual(1.5, invCnd.ValueOf(.9331927987), 1e-6);
            Assert.AreEqual(-1.5, invCnd.ValueOf(0.06680720127), 1e-6);


            invCnd = new InvCumulativeNormalDistribution(1.0, 2.0);

            Assert.AreEqual(1.0, invCnd.ValueOf(0.5), 1e-6);

            // values obtained with Maple 9.01
            Assert.AreEqual(0.5, invCnd.ValueOf(.4012936743), 1e-6);
            Assert.AreEqual(-0.5, invCnd.ValueOf(.2266273524), 1e-6);
            Assert.AreEqual(1.5, invCnd.ValueOf(.5987063257), 1e-6);
            Assert.AreEqual(-1.5, invCnd.ValueOf(.1056497737), 1e-6);

        }
    }
}