// <copyright file="CurveProviderTest.cs" company="National Australia Bank">
// Copyright (c) All Rights Reserved
// </copyright>

using Orion.WebViewer.Curve.Business;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viewer.Tests.Integration
{
    /// <summary>
    /// This is a test class for CurveProviderTest and is intended
    /// to contain all CurveProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class CurveProviderTest
    {
        readonly CurveProvider _curveProvider;

        public CurveProviderTest()
        {
            _curveProvider = new CurveProvider();
        }

        /// <summary>
        /// A test for GetCurvesCount
        /// </summary>
        [TestMethod]
        public void GetCurvesCountTest()
        {
            const bool Enabled = true; 
            const string MarketName = "";
            const string PricingStructureType = "";
            const string Id = "";
            const int MaximumRows = 30; 
            const int StartRowIndex = 0; 
            const int NotExpected = 0; 

            int actual = _curveProvider.GetCurvesCount(Enabled, MarketName, PricingStructureType, Id, MaximumRows, StartRowIndex);
            Assert.AreNotEqual(NotExpected, actual);
        }

        /// <summary>
        /// A test for GetCurves
        /// </summary>
        [TestMethod]
        public void GetCurvesTest()
        {
            const bool Enabled = true;
            const string MarketName = "";
            const string PricingStructureType = "";
            const string Id = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;
            const int NotExpected = 0;

            IEnumerable<Curve> actual = _curveProvider.GetCurves(Enabled, MarketName, PricingStructureType, Id, MaximumRows, StartRowIndex);
            Assert.AreNotEqual(NotExpected, actual.Count());
        }

        /// <summary>
        /// test don't GetCurves
        /// </summary>
        [TestMethod]
        public void DontGetCurvesTest()
        {
            const bool Enabled = false;
            const string MarketName = "";
            const string PricingStructureType = "";
            const string Id = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;

            var actual = _curveProvider.GetCurves(Enabled, MarketName, PricingStructureType, Id, MaximumRows, StartRowIndex);
            Assert.AreEqual(0, actual.Count());
        }

        /// <summary>
        /// test don't GetCurvesCount
        /// </summary>
        [TestMethod]
        public void DontGetCurvesCountTest()
        {
            const bool Enabled = false;
            const string MarketName = "";
            const string PricingStructureType = "";
            const string Id = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;

            int actual = _curveProvider.GetCurvesCount(Enabled, MarketName, PricingStructureType, Id, MaximumRows, StartRowIndex);
            Assert.AreEqual(0, actual);
        }

        /// <summary>
        /// A test for GetCurve
        /// </summary>
        [TestMethod]
        public void GetCurveTest()
        {
            const bool Enabled = true;
            const string MarketName = "";
            const string PricingStructureType = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;

            // Load some curves so that we can get the Id
            IEnumerable<Curve> curves = _curveProvider.GetCurves(Enabled, MarketName, PricingStructureType, string.Empty, MaximumRows, StartRowIndex);
            string curveId = curves.First().Id;

            // now load a curve directly
            Curve curve = _curveProvider.GetCurve(curveId);
            Assert.AreEqual(curveId, curve.Id);

            // also load a curve indirectly
            curves = _curveProvider.GetCurves(true, string.Empty, string.Empty, curveId, 30, 0);
            Assert.AreEqual(curveId, curves.Single().Id);
        }

        /// <summary>
        /// Don't get a Curve
        /// </summary>
        [TestMethod]
        public void DontGetCurveTest()
        {
            // now load a curve
            Curve curve = _curveProvider.GetCurve(string.Empty);
            Assert.IsNull(curve);
        }

        /// <summary>
        /// A test for GetCurves
        /// </summary>
        [TestMethod]
        public void GetSomeCurvesTest()
        {
            const bool Enabled = true;
            const string MarketName = "";
            const string PricingStructureType = "DiscountCurve";
            const string Id = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;
            const int NotExpected = 0;

            var actual = _curveProvider.GetCurves(Enabled, MarketName, PricingStructureType, Id, MaximumRows, StartRowIndex);
            Assert.AreNotEqual(NotExpected, actual.Count());
        }

        /// <summary>
        /// A test for GetCurve
        /// </summary>
        [TestMethod]
        public void GetCurveSurfaceTest()
        {
            const bool Enabled = true;
            const string MarketName = "";
            const string PricingStructureType = "";
            const int MaximumRows = 30;
            const int StartRowIndex = 0;

            // Load some curves so that we can get the Id
            IEnumerable<Curve> curves = _curveProvider.GetCurves(Enabled, MarketName, PricingStructureType, string.Empty, MaximumRows, StartRowIndex);
            string curveId = curves.First().Id;

            // now load a curve surface
            Surface surface = _curveProvider.GetCurveSurface(curveId);
            Assert.IsNotNull(surface);
        }

        /// <summary>
        /// A test for GetCurve
        /// </summary>
        [TestMethod]
        public void DontGetCurveSurfaceTest()
        {
            Surface surface = _curveProvider.GetCurveSurface(string.Empty);
            Assert.IsNull(surface);
        }
    }
}