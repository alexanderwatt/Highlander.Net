using System;
using System.Collections.Generic;

namespace Orion.WebViewer.Curve.Business
{
    public class CurvePoint
    {
        public CurvePoint(DateTime x, double y, List<object> values)
        {
            X = x;
            Y = y;
            Values = values;
        }

        public CurvePoint(DateTime x, double strike, double value, List<object> values)
        {
            X = x;
            Y = strike;
            Z = value;
            Values = values;
        }

        public List<object> Values
        {
            get;
            private set;
        }

        public DateTime X
        {
            get; 
            private set;
        }

        public double Y
        {
            get;
            private set;
        }

        public double Z
        {
            get;
            private set;
        }
    }
}
