#region Using Directives

using System;
using System.Diagnostics;
using nabCap.QR.Analytics.Convexity;

using NUnit.Framework;

#endregion

namespace nabCap.QR.Analytics.Tests.Convexity
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class BGMConvexityTests 
    {
        ///// <summary>
        ///// Testing the FuturesMarginConvexityAdjustment.
        ///// </summary>
        [Test]
        public void TestBGMConvexityAdjustment()
        {
            //double[] futuresPrice = {0.0d, .05d};
            double[,] volatility = { { 0, 0.2 } };
            double[, ,] correlation = new double[2, 2, 1];
            double[] shift = {0.0, 0.0};
            double[] coverage = {0.25, 0.25};
            double[] timeNodes = {0, 0.25, 0.5};
            
            double[] knownvalue = {0.049993797154200, 0.050993548171811, 0.051993294354016, 0.052993035704439, 0.053992772226696, 0.054992503924403, 0.055992230801171, 0.056991952860608, 0.057991670106317, 0.058991382541898};
            double tolerance = 1e-9;

            for (var j = 0; j < 2; j++)
            {
                for (var k = 0; k < 2; k++)
                {
                    correlation[j, k, 0] = 1;
                }
            }

            for (var i = 0; i < 10; i++)
            {
                var rate = new[] {0, (50.0 + i)*0.001};
                var result = CashForward.CalculateCashForward(rate, volatility, correlation, shift, coverage, timeNodes);
                Assert.AreEqual(knownvalue[i], result[i].x, tolerance);
                Debug.WriteLine(String.Format("rate : {0} implied: {1}", rate, result[1].x));//TODO the 1 needs to ba an i.
            }
        }
    }
}