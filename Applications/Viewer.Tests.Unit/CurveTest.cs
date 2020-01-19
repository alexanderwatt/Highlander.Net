using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.WebViewer.Curve.Business;

namespace Viewer.Tests.Unit
{
    [TestClass]
    public class CurveTest
    {
        private readonly CurveProvider _curveProvider = CurveProviderTest.CreateCurveProvider();

        [TestMethod]
        public void CurveEmptyTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveEmptyFpml);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveEmptyFpml);
            Assert.AreEqual(0, curve.Points.Count);
            Assert.AreNotEqual(0, curve.Fpml.Length);
        }

        [TestMethod]
        public void CurveInputZeroDiscountNoIdsTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveInputDiscountZeroNoIds);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveInputDiscountZeroNoIds);
            // check row count
            Assert.AreEqual(25, curve.Points.Count);
            // check column count
            Assert.AreEqual(5, curve.Points[0].Values.Count);
            Assert.AreEqual(5, curve.Titles.Count);
            // no IDs so don't expect the inputs or zeros to show
            Assert.IsNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
            Assert.IsNull(curve.Points[5].Values[2]);
            Assert.IsNotNull(curve.Points[5].Values[3]);
            Assert.IsNull(curve.Points[5].Values[4]);

            // check graphing properties
            Assert.AreEqual(25, curve.XValues.Count);
            Assert.AreNotEqual(0, curve.XValueName.Length);
            Assert.AreEqual(25, curve.Values.Count);
            Assert.AreNotEqual(0, curve.ValueName.Length);
        }

        [TestMethod]
        public void CurveInputZeroDiscountIdsTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveInputDiscountZeroIds);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveInputDiscountZeroIds);
            // check row count
            Assert.AreEqual(39, curve.Points.Count);
            // check column count
            Assert.AreEqual(5, curve.Points[0].Values.Count);
            Assert.AreEqual(5, curve.Titles.Count);
            // Has IDs so all columns show (for some rows)
            Assert.IsNotNull(curve.Points[13].Values[0]);
            Assert.IsNotNull(curve.Points[13].Values[1]);
            Assert.IsNotNull(curve.Points[13].Values[2]);
            Assert.IsNotNull(curve.Points[13].Values[3]);
            Assert.IsNotNull(curve.Points[13].Values[4]);
        }

        [TestMethod]
        public void CurveInputDiscountTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveInputDiscount);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveInputDiscount);
            // check row count
            Assert.AreEqual(27, curve.Points.Count);
            // check column count
            Assert.AreEqual(4, curve.Points[0].Values.Count);
            Assert.AreEqual(4, curve.Titles.Count);
            // no IDs so don't expect the inputs or zeros to show
            Assert.IsNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
            Assert.IsNotNull(curve.Points[5].Values[2]);
            Assert.IsNull(curve.Points[5].Values[3]);
        }

        [TestMethod]
        public void CurveInputTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveInput);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveInput);
            // check row count
            Assert.AreEqual(28, curve.Points.Count);
            // check column count
            Assert.AreEqual(2, curve.Points[0].Values.Count);
            Assert.AreEqual(2, curve.Titles.Count);
            // no IDs so don't expect the inputs or zeros to show
            Assert.IsNotNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
        }

        [TestMethod]
        public void CurveDiscountTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveDiscount);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveDiscount);
            // check row count
            Assert.AreEqual(39, curve.Points.Count);
            // check column count
            Assert.AreEqual(3, curve.Points[0].Values.Count);
            Assert.AreEqual(3, curve.Titles.Count);
            // no IDs so don't expect the inputs or zeros to show
            Assert.IsNotNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
            Assert.IsNotNull(curve.Points[5].Values[2]);
        }

        [TestMethod]
        public void CurveDiscountZeroTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveDiscountZero);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveDiscountZero);
            // check row count
            Assert.AreEqual(37, curve.Points.Count);
            // check column count
            Assert.AreEqual(4, curve.Points[0].Values.Count);
            Assert.AreEqual(4, curve.Titles.Count);
            // no IDs so don't expect the inputs or zeros to show
            Assert.IsNotNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
            Assert.IsNotNull(curve.Points[5].Values[2]);
            Assert.IsNotNull(curve.Points[5].Values[3]);
        }
        
        [TestMethod]
        public void CurveFxTest()
        {
            var curve = _curveProvider.GetCurve(CurveProviderTest.CurveFx);

            Assert.AreEqual(curve.Id, CurveProviderTest.CurveFx);
            // check row count
            Assert.AreEqual(24, curve.Points.Count);
            // check column count
            Assert.AreEqual(4, curve.Points[0].Values.Count);
            Assert.AreEqual(4, curve.Titles.Count);
            // Has IDs expect all columns
            Assert.IsNotNull(curve.Points[5].Values[0]);
            Assert.IsNotNull(curve.Points[5].Values[1]);
            Assert.IsNotNull(curve.Points[5].Values[2]);
            Assert.IsNotNull(curve.Points[5].Values[3]);
        }

        [TestMethod]
        public void SurfaceTest()
        {
            Surface surface = _curveProvider.GetCurveSurface(CurveProviderTest.Surface);

            // check row count
            Assert.AreEqual(20, surface.Rows.Count);
            // check strikes count
            Assert.AreEqual(7, surface.Strikes.Count());
        }
    }
}
