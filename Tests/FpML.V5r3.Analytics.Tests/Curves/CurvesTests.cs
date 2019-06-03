#region Using Directives

using System;
using System.Diagnostics;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Rates;
using Orion.ModelFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.Curves
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class CurvesTests 
    {
        private static readonly double[] rates = new[] { 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
        private static readonly double[] times = new[] {0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d};

        private static readonly double[] testpts = new[] {0.5, 0.75, 1.0, 1.5, 2.3, 3.1};
        private const string interp = "LinearInterpolation";

        private readonly decimal[] compounding = { 0.0m, (decimal)1 / 365, (decimal)1 / 52, (decimal)1 / 12, (decimal)1 / 6, (decimal)1 / 4, (decimal)1 / 2, 1 };

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestGetValue1()
        {
            foreach(var pt in testpts)
            {
                var result = CurveAnalytics.GetValue(pt, interp, true, times, rates);
                Debug.WriteLine(String.Format("rate : {0} Time: {1}", pt, result));
            }
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestGetValue2()
        {
            var baseDate = new DateTime(2008, 3, 3);
            for (var i = 1; i < 10; i++ )
            {
                var targetDate = baseDate.AddMonths(3*i);
                IPoint point = new DateTimePoint1D(baseDate, targetDate);
                var result = CurveAnalytics.GetDateValue(point, interp, true, times, rates);
                Debug.WriteLine(String.Format("rate : {0} Date: {1}", targetDate, result));
            }

        }
    }
}