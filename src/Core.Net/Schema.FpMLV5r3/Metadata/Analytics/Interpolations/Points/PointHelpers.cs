/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public static class PointHelpers
    {
        /// <summary>
        /// Maps from double arrays to an IPoint list.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="coords"></param>
        public static List<IPoint> Point1D(DateTime baseDate, IDictionary<DateTime, double> coords)
        {
            return coords.Select(keyValuePair => new Point1D((new DateTimePoint1D(baseDate, keyValuePair.Key).GetX()), keyValuePair.Value)).Cast<IPoint>().ToList();
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list.
        /// </summary>
        /// <param name="xCoords"></param>
        /// <param name="values"><c>double</c> The one dimensional function value.</param>
        public static List<IPoint> Point1D(IList<double> xCoords, IList<double> values)
        {
            if (xCoords.Count != values.Count)
                throw new ArgumentException(
                    "TODO: unequal number of elements for applying the helper");
            return xCoords.Select((t, i) => new Point1D(t, values[i])).Cast<IPoint>().ToList();
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="values"><c>double</c> The one dimensional function value.</param>
        public static List<IPoint> Point2D(double[] rows, double[] columns, Matrix values)
        {
            if ((rows.Length != values.RowCount) || (columns.Length != values.ColumnCount))
                throw new ArgumentException(
                    "TODO: unequal number of elements for applying the helper");
            var points = new List<IPoint>();
            for (var i = 0; i < rows.Length; i++)
            {
                points.AddRange(columns.Select((t, j) => new Point2D(rows[i], t, values[i, j])));
            }
            return points;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="termStrike">The term or strike of the underlying.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="strikeFlag">the flag which identifies which: term or strike.</param>
        public static double[] Create2DArray(string expiry, string termStrike, string generic, bool strikeFlag)
        {
            return Create2DArray(expiry, termStrike, generic, strikeFlag, 0.0);
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="termStrike">The term or strike of the underlying.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="strikeFlag">the flag which identifies which: term or strike.</param>
        /// <param name="value">the value for that coordinate.</param>
        public static double[] Create2DArray(string expiry, string termStrike, string generic, bool strikeFlag, double value)
        {
            var result = new double[3];
            var coords = CreatePoint(expiry, termStrike, generic, strikeFlag).Coords;
            result[0] = (double)coords[0];
            result[1] = (double)coords[1];
            result[2] = value;
            return result;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="term">The term or strike of the underlying.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="strike">the flag which identifies which: term or strike.</param>
        /// <param name="value">the value for that coordinate.</param>
        public static double[] Create3DArray(string expiry, string term, string strike, string generic, double value)
        {
            var result = new double[4];
            var coords = CreatePoint(expiry, term, strike).Coords;//TODO add generic.
            result[0] = (double)coords[0];
            result[1] = (double)coords[1];
            result[2] = (double)coords[2];
            result[3] = value;
            return result;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="termStrike">The term or strike of the underlying.</param>
        /// <param name="generic">A generic dimension</param>
        /// <param name="strikeFlag">the flag which identifies which: term or strike.</param>
        public static IPoint CreatePoint(string expiry, string termStrike, string generic, bool strikeFlag)
        {
            if (expiry == null || termStrike == null)
            {
                return null;
            }
            //TODO test for generic.
            return strikeFlag ? CreatePoint(expiry, null, termStrike) : CreatePoint(expiry, termStrike, null);
        }


        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="term">The term of the underlying.</param>
        /// <param name="strike">The strike.</param>
        public static GeneralPoint CreatePoint(string expiry, string term, double strike)
        {
            if (expiry == null)
            {
                return null;
            }
            var expiryTime = PeriodHelper.Parse(expiry).ToYearFraction();
            if (term == null)
            {
                return new GeneralPoint(new[] { expiryTime, strike, 0.0 });
            }
            var termTime = PeriodHelper.Parse(term).ToYearFraction();
            return new GeneralPoint(new[] { expiryTime, termTime, strike, 0.0 });
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// This method assumes simple time periods.
        /// </summary>
        /// <param name="expiry">the expiry of the underlying.</param>
        /// <param name="term">The term of the underlying.</param>
        /// <param name="strike">The strike.</param>
        public static GeneralPoint CreatePoint(string expiry, string term, string strike)
        {
            if (expiry == null)
            {
                return null;
            }
            var expiryTime = PeriodHelper.Parse(expiry).ToYearFraction();
            if (term == null)
            {
                return new GeneralPoint(new[]{expiryTime, double.Parse(strike), 0.0});
            }
            var termTime = PeriodHelper.Parse(term).ToYearFraction();
            return strike == null ? new GeneralPoint(new[] {expiryTime, termTime, 0.0}) : new GeneralPoint(new[]{expiryTime, termTime, double.Parse(strike), 0.0});
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="expiryYearFraction">the expiry of the underlying.</param>
        /// <param name="termYearFraction">The term of the underlying.</param>
        /// <param name="strike">the strike.</param>
        public static Point3D CreatePoint3D(double expiryYearFraction, double termYearFraction, decimal strike)
        {
            var point = new Point3D(expiryYearFraction, termYearFraction, (double)strike, 0.0);
            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static double[] To2DArray(PricingDataPointCoordinate pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            if (pt.expiration == null)
            {
                return null;
            }
            var point = Array.Empty<double>();
            var expiry = pt.expiration[0].Items[0] as Period;
            const double value = 0.0;
            if (expiry != null)
            {
                var expiryTime = expiry.ToYearFraction();
                if (pt.strike != null)
                {
                    point = new[]
                    {
                        expiryTime,
                        (double)pt.strike[0],
                        value
                    };
                }
                if (pt.term != null)
                {
                    if (pt.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new[]
                        {
                            expiryTime,
                            termTime,
                            value
                        };
                    }
                }
            }
            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static double[] ToArray(PricingDataPointCoordinate pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            if (pt.expiration == null)
            {
                return null;
            }
            if(pt.strike == null && pt.term == null)
            {
                return null;
            }
            const double value = 0.0;
            if (pt.expiration[0].Items[0] is Period expiry)
            {
                var expiryTime = expiry.ToYearFraction();
                var point = Array.Empty<double>();

                if (pt.term != null && pt.strike != null)
                {
                    var strike = (double)pt.strike[0];
                    if (pt.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new[]
                        {
                            expiryTime,
                            termTime,
                            strike,
                            value,
                        };
                    }
                }
                if (pt.term == null && pt.strike != null)
                {
                    var strike = (double)pt.strike[0];
                    point = new[]
                    {
                        expiryTime,
                        strike,
                        value
                    };
                }
                if (pt.term != null && pt.strike == null)
                {
                    if (pt.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new[]
                        {
                            expiryTime,
                            termTime,
                            value
                        };
                    }
                }
                return point;
            }

            return Array.Empty<double>();
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static double[] To2DArray(PricingStructurePoint pt)
        {
            return To2DArray(pt, null);
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        /// <param name="baseDate"></param>
        public static double[] To2DArray(PricingStructurePoint pt, DateTime? baseDate)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            var coordinate = pt.coordinate[0];
            if (coordinate.expiration == null)
            {
                return null;
            }
            double value = Convert.ToDouble(pt.value);
            var expiry = coordinate.expiration[0].Items[0] as Period;
            double expiryTime;
            if (expiry != null)
            {
                expiryTime = expiry.ToYearFraction();
            }
            else if (baseDate.HasValue && coordinate.expiration[0].Items[0] is DateTime)
            {
                var expiryDate = (DateTime)coordinate.expiration[0].Items[0];
                expiryTime = (expiryDate - baseDate.Value).TotalDays / 365d;
            }
            else
            {
                throw new InvalidCastException("Coordinate expiration must be a Period or DateTime");
            }
            double yDimension = (coordinate.term?[0].Items[0] as Period)?.ToYearFraction() ?? (coordinate.strike != null ? (double)coordinate.strike[0] : 0);
            var point = new[]
                            {
                                expiryTime,
                                yDimension,
                                value
                            };
            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static double[] To3DArray(PricingDataPointCoordinate pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            if (pt.expiration == null || pt.strike == null)
            {
                return null;
            }
            const double value = 0.0;
            var expiry = pt.expiration[0].Items[0] as Period;
            var strike = (double)pt.strike[0];
            var point = Array.Empty<double>();
            if (expiry != null)
            {
                var expiryTime = expiry.ToYearFraction();
                if (pt.term != null)
                {
                    if (pt.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new[]
                        {
                            expiryTime,
                            termTime,
                            strike,
                            value,
                        };
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static double[] To3DArray(PricingStructurePoint pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            var coordinate = pt.coordinate[0];
            if (coordinate.expiration == null || coordinate.strike == null)
            {
                return null;
            }
            var expiry = coordinate.expiration[0].Items[0] as Period;
            var strike = (double)coordinate.strike[0];
            var value = Convert.ToDouble(pt.value);
            var point = Array.Empty<double>();
            if (expiry != null)
            {
                var expiryTime = expiry.ToYearFraction();
                if (coordinate.term != null)
                {
                    if (coordinate.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new[]
                        {
                            expiryTime,
                            termTime,
                            strike,
                            value
                        };
                    }
                }
            }
            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static Point3D CreatePoint3D(PricingStructurePoint pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            var coordinate = pt.coordinate[0];
            if (coordinate.expiration == null || coordinate.strike == null)
            {
                return null;
            }
            var expiry = coordinate.expiration[0].Items[0] as Period;
            var strike = (double)coordinate.strike[0];
            var value = Convert.ToDouble(pt.value);
            Point3D point = null;
            if (expiry != null)
            {
                var expiryTime = expiry.ToYearFraction();
                if (coordinate.term?[0].Items[0] is Period term)
                {
                    var termTime = term.ToYearFraction();
                    point = new Point3D(expiryTime, termTime, strike, value);
                }
            }

            return point;
        }

        /// <summary>
        /// Maps from double arrays to an IPoint list. The number of values is equal to the multiplication of xCoords and yCoords.
        /// </summary>
        /// <param name="pt">A PricingStructurePoint.</param>
        public static Point CreatePoint(PricingStructurePoint pt)
        {
            //TODO trap the generic. Also add baseDate for carry calculations.
            var coordinate = pt.coordinate[0];
            if (coordinate.expiration == null || coordinate.strike == null)
            {
                return null;
            }
            var expiry = coordinate.expiration[0].Items[0] as Period;
            var strike = (double)coordinate.strike[0];           
            var value = Convert.ToDouble(pt.value);           
            Point point = null;
            if (expiry != null)
            {
                var expiryTime = expiry.ToYearFraction();
                if (coordinate.term != null)
                {
                    if (coordinate.term[0].Items[0] is Period term)
                    {
                        var termTime = term.ToYearFraction();
                        point = new Point3D(expiryTime, termTime, strike, value);
                    }
                }
                else
                {
                    point = new Point2D(expiryTime, strike, value);
                }
            }
            return point;
        }

        #region DoublePoint Conversions

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point2D"/>
        /// that represents the year fraction expiry and a sPoint2DStrike
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Point2D ToDoublePoint2D(VolatilityValue val)
        {
            var expiry = (Period)val.PricePoint.coordinate[0].expiration[0].Items[0];
            var strike = val.PricePoint.coordinate[0].strike[0];
            var value = Convert.ToDouble(val.Value);
            return new Point2D((double)strike, expiry.ToYearFraction(), value);
        }

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represent year fractions expiry and tenor, and a strike
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Point3D ToDoublePoint3D(VolatilityValue val)
        {
            var expiry = (Period)val.PricePoint.coordinate[0].expiration[0].Items[0];
            var tenor = (Period)val.PricePoint.coordinate[0].term[0].Items[0];
            var strike = val.PricePoint.coordinate[0].strike[0];
            var value = Convert.ToDouble(val.Value);
            return new Point3D((double)strike, tenor.ToYearFraction(), expiry.ToYearFraction(), value);
        }

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represents the year fraction expiry and a strike
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static Point2D ToDoublePoint2D(IPoint coordinate)
        {
            var expiry = (Period)((Coordinate)coordinate).PricingDataCoordinate.expiration[0].Items[0];
            var strike = ((Coordinate)coordinate).PricingDataCoordinate.strike[0];
            const double value = 0;
            return new Point2D((double)strike, expiry.ToYearFraction(), value);
        }

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represent year fractions expiry and tenor, and a strike
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public static Point3D ToDoublePoint3D(IPoint coordinates)
        {
            var expiry = (Period)((Coordinate)coordinates).PricingDataCoordinate.expiration[0].Items[0];
            var tenor = (Period)((Coordinate)coordinates).PricingDataCoordinate.term[0].Items[0];
            var strike = ((Coordinate)coordinates).PricingDataCoordinate.strike[0];
            const double value = 0;
            return new Point3D((double)strike, tenor.ToYearFraction(), expiry.ToYearFraction(), value);
        }

        #endregion
    }
}


