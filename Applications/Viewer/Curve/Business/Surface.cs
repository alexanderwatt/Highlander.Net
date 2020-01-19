using System;
using System.Collections.Generic;
using System.Linq;
using nab.QDS.FpML.V47;

namespace Orion.WebViewer.Curve.Business
{
    public class Surface
    {
        public IEnumerable<double> Strikes { get; private set; }
        public List<PointBaseCollection> Rows { get; private set; }

        public Surface()
        {
        }

        public Surface(PricingStructureValuation pricingStructure)
        {
            Type pricingStructureType = pricingStructure.GetType();

            if (pricingStructureType == typeof(VolatilityMatrix))
            {
                PricingStructurePoint[] points = ((VolatilityMatrix)pricingStructure).dataPoints.point;
                Initialize(points);
            }
        }

        private void Initialize(IEnumerable<PricingStructurePoint> points)
        {
            Rows = new List<PointBaseCollection>();
            var curvePoints = new List<CurvePoint>();

            foreach (PricingStructurePoint point in points)
            {
                if (point.coordinate == null
                    || point.coordinate[0] == null
                    || point.coordinate[0].strike == null
                    || point.coordinate[0].strike.Length == 0
                    || point.coordinate[0].expiration == null
                    || point.coordinate[0].expiration.Length == 0
                    || point.coordinate[0].expiration[0].Items == null
                    || point.coordinate[0].expiration[0].Items.Length == 0)
                {
                    return;    
                }

                var values = new List<object>();

                PricingDataPointCoordinate coordinate = point.coordinate[0];
                var interval = (Period)coordinate.expiration[0].Items[0];

                DateTime date = interval.Add(DateTime.Today);
                values.Add(date);
                values.Add(coordinate.strike[0]);
                values.Add(point.value);
                values.Add(interval.periodMultiplier + interval.period);

                var curvePoint = new CurvePoint(date, (double)point.value, (double)coordinate.strike[0], values);
                curvePoints.Add(curvePoint);

                PointBaseCollection row = Rows.SingleOrDefault(a => a.Tenor == (string)curvePoint.Values[3]);
                if (row == null)
                {
                    row = new PointsSurfaceCollection();
                    Rows.Add(row);
                }
                row.Add(curvePoint);
            }

            Strikes = curvePoints.Select(a => a.Z).Distinct();
        }
    }
}
