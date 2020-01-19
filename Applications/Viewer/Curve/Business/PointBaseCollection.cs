using System;
using System.Collections.Generic;
using System.Linq;
using nab.QDS.FpML.V47;

namespace Orion.WebViewer.Curve.Business
{
    public abstract class PointBaseCollection : List<CurvePoint>
    {
        #region Base Properties
        public string Tenor { get; private set; }

        public abstract List<string> Titles { get; }

        public IList<DateTime> XValues
        {
            get { return this.Select(a => a.X).ToList(); }
        }

        public IList<double> YValues
        {
            get { return this.Select(a => a.Y).ToList(); }
        }

        #endregion

        #region Base Methods
        public new void Add(CurvePoint curvePoint)
        {
            base.Add(curvePoint);
            if (curvePoint.Values.Count > 3)
            {
                Tenor = (string)curvePoint.Values[3];
            }
        }
        #endregion 

        #region Static Methods
        public static PointBaseCollection Factory(PricingStructureValuation pricingStructure)
        {
            Type pricingStructureType = pricingStructure.GetType();

            if (pricingStructureType == typeof(YieldCurveValuation))
            {
                return new PointYieldCollection(pricingStructure);
            }
            if (pricingStructureType == typeof(FxCurveValuation))
            {
                return new PointFxCollection(pricingStructure);
            }
            return new PointsSurfaceCollection();
        }
        #endregion
    }
}
