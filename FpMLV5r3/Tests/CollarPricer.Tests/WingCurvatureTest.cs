using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class WingCurvatureTest
    {
        private const Double CPropertyValue = 100.01;
        static readonly DateTime CEtoDate = DateTime.Today;

        /// <summary>
        /// Creates this instance.
        /// </summary>
        [TestMethod]
        public void CreateCurvature()
        {
            Create();
        }

        static public WingCurvature Create()
        {
            return CreateWingCurvature(CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CPropertyValue, CEtoDate);
        }

        /// <summary>
        /// Creates the wing curvature.
        /// </summary>
        /// <param name="cv">The cv.</param>
        /// <param name="sr">The sr.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="pc">The pc.</param>
        /// <param name="dc">The dc.</param>
        /// <param name="uc">The uc.</param>
        /// <param name="rf">The rf.</param>
        /// <param name="vcr">The VCR.</param>
        /// <param name="scr">The SCR.</param>
        /// <param name="ssr">The SSR.</param>
        /// <param name="dsr">The DSR.</param>
        /// <param name="usr">The usr.</param>
        /// <param name="etoDate"></param>
        /// <returns></returns>
        static public WingCurvature CreateWingCurvature(Double cv, Double sr, Double cc, Double pc, Double dc, Double uc, Double rf, Double vcr, Double scr, Double ssr, Double dsr, Double usr, DateTime etoDate)
        {
            var curvature = new WingCurvature();
            curvature[WingCurvature.WingCurvatureProperty.CurrentVolatility] = cv;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.SlopeReference] = sr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.CallCurvature] = cc;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.PutCurvature] = pc;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.DownCutOff] = dc;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.UpCutOff] = uc;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.ReferenceForward] = rf;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.VolChangeRate] = vcr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.SlopeChangeRate] = scr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = ssr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.DownSmoothingRange] = dsr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature[WingCurvature.WingCurvatureProperty.UpSmoothingRange] = usr;
            Assert.AreEqual(curvature.IsComplete, false);
            curvature.EtoDate = etoDate;
            Assert.AreEqual(curvature.IsComplete, true);
            return curvature;
        }
    }
}
