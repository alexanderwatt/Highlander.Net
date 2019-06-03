using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;
using Double = System.Double;


namespace Orion.Analytics.Tests.Distributions
{
    /// <summary>
    /// Unit Tests for the class CumulativeNormal.
    /// </summary>
    [TestClass]
    public class CumulativeNormalUnitTests
    {

        /// <summary>
        /// Accuracy used to compare actual and expected values.
        /// </summary>
        private const double Tolerance = 1.0E-12;

            /// <summary>
            /// Can create standard normal.
            /// </summary>
            [TestMethod]
            public void CanCreateStandardNormal()
            {
                var n = new NormalDistribution();
                Assert.AreEqual(0.0, n.Mean);
                Assert.AreEqual(1.0, n.Sigma);
            }

        /// <summary>
        /// Can create normal.
        /// </summary>
        [TestMethod]
        public void CanCreateNormal()
            {
                double[] meanArray = {0.0 , 10.0 , -5.0 , 0.0 , 10.0, -5.0 };
                double[] sdevArray = { 0.0, 0.1, 1.0, 10.0, 100.0, Double.PositiveInfinity };
                for(var i =0;i< meanArray.Length; i++)
                {
                    var n = new NormalDistribution(meanArray[i], sdevArray[i]);
                    Assert.AreEqual(meanArray[i], n.Mean);
                    Assert.AreEqual(sdevArray[i], n.Sigma);
                }
            }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        [TestMethod]
        public void ValidateSkewness()
            {
                double[] sdevArray = { -0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity };
                foreach (double t in sdevArray)
                {
                    var n = new NormalDistribution(1.0, t);
                    Assert.AreEqual(0.0, n.Skewness);
                }
            }

        [TestMethod]
        public void ValidateCmDist()
        {
            double[] xArray = { Double.NegativeInfinity, -5.0, -2.0, -0.0, 0.0, 4.0, 5.0, 6.0, 10.0, Double.PositiveInfinity };
            double[] pArray = { 0.0,
                0.00000028665157187919391167375233287464535385442301361187883,
                0.0002326290790355250363499258867279847735487493358890356,
                0.0062096653257761351669781045741922211278977469230927036,
                0.0062096653257761351669781045741922211278977469230927036,
                0.30853753872598689636229538939166226011639782444542207,
                0.5,
                0.69146246127401310363770461060833773988360217555457859,
                0.9937903346742238648330218954258077788721022530769078,
                1.0};
            for (int i = 0; i < xArray.Length; i++)
                {
                    ValidateCumulativeDistribution(xArray[i], pArray[i]);
                }
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
        public void ValidateCumulativeDistribution(double x, double p)
            {
            var n = new NormalDistribution(5.0, 2.0);
                Assert.AreEqual(p, n.CumulativeDistribution(x), 9);
            }

        [TestMethod]
        public void ValidateInvCmDist()
        {
            double[] xArray = { Double.NegativeInfinity, -5.0, -2.0, -0.0, 0.0, 4.0, 5.0, 6.0, 10.0, Double.PositiveInfinity };
            double[] pArray = { 0.0,
                0.00000028665157187919391167375233287464535385442301361187883,
                0.0002326290790355250363499258867279847735487493358890356,
                0.0062096653257761351669781045741922211278977469230927036,
                0.0062096653257761351669781045741922211278977469230927036,
                0.30853753872598689636229538939166226011639782444542207,
                0.5,
                0.69146246127401310363770461060833773988360217555457859,
                0.9937903346742238648330218954258077788721022530769078,
                1.0};
            for (int i = 0; i < xArray.Length; i++)
            {
                ValidateInverseCumulativeDistribution(xArray[i], pArray[i]);
            }
        }

        /// <summary>
        /// Validate inverse cumulative distribution.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="p">Expected value.</param>
            public void ValidateInverseCumulativeDistribution(double x, double p)
            {
                var n = new NormalDistribution(5.0, 2.0);
                Assert.AreEqual(x, n.InverseCumulativeDistribution(p), 14);
            }
        }
}