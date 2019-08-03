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

#region Using directives

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Analytics.Interpolations.Points;
using Orion.Identifiers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.LinearAlgebra;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public abstract class ExpiryTermStrikeVolatilitySurface : PricingStructureBase, IStrikeVolatilitySurface
    {
        #region Private Fields

        ///<summary>
        /// Gets and sets the algorithm.
        ///</summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// An auxilliary structure used to track indexes within the volatility surface.
        /// This should make lookups faster than the default linear search.
        /// </summary>
        private readonly SortedList<ExpiryTenorStrikeKey, int> _matrixIndexHelper;

        /// <summary>
        /// Auxilliary counts of the inputs
        /// </summary>
        private int _matrixRowCount;
        private int _matrixColumnCount;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="surfaceId">The id to use with this matrix</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="expiryTenors">An array of expiry definitions</param>
        /// <paparam name="strike">An array of strike descriptions</paparam>
        /// <param name="strikes">The strike array.</param>
        /// <param name="volSurface">A 2d array of volatilities.
        /// This must be equal to (expiry.Count x (1 &lt;= y &lt;= term.Count) x strike.Count </param>
        /// <param name="cache">The cache</param>
        protected ExpiryTermStrikeVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, Double[] strikes, Double[,] volSurface, 
            VolatilitySurfaceIdentifier surfaceId)
        {
            Algorithm = surfaceId.Algorithm;
            PricingStructureIdentifier = surfaceId;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            var points = ProcessRawSurface(expiryTenors, strikes, volSurface, surfaceId.StrikeQuoteUnits, surfaceId.UnderlyingAssetReference);
            PricingStructure = CreateVolatilityRepresentation(surfaceId);
            PricingStructureValuation = CreateDataPoints(points, surfaceId);
            var expiries = new double[expiryTenors.Length];
            var index = 0;
            foreach (var term in expiryTenors)//TODO include business day holidays and roll conventions.
            {
                expiries[index] = PeriodHelper.Parse(term).ToYearFraction();
                index++;
            }
            // Record the row/column sizes of the inputs
            _matrixRowCount = expiryTenors.Length;
            // Columns includes expiry and term (tenor) if it exists.
            _matrixColumnCount = strikes.Length + 1;
            // Generate an interpolator to use
            Interpolator = new VolSurfaceInterpolator(expiries, strikes, new Matrix(volSurface), curveInterpolationMethod, true);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="logger">THe logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="properties">The properties.</param>
        protected ExpiryTermStrikeVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] expiryTenors, Double[] strikes, Double[,] volSurface)
        {       
            var surfaceId = new VolatilitySurfaceIdentifier(properties);
            Algorithm = surfaceId.Algorithm;
            PricingStructureIdentifier = surfaceId;
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            var points = ProcessRawSurface(expiryTenors, strikes, volSurface, surfaceId.StrikeQuoteUnits, surfaceId.UnderlyingAssetReference);
            PricingStructure = CreateVolatilityRepresentation(surfaceId);
            PricingStructureValuation = CreateDataPoints(points, surfaceId);
            var expiries = new double[expiryTenors.Length];
            var index = 0;
            foreach (var term in expiryTenors)//TODO include business day holidays and roll conventions.
            {
                expiries[index] = PeriodHelper.Parse(term).ToYearFraction();
                index++;
            }
            // Record the row/column sizes of the inputs
            _matrixRowCount = expiryTenors.Length;
            // Columns includes expiry and term (tenor) if it exists.
            _matrixColumnCount = strikes.Length + 1;
            // Generate an interpolator to use
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            Interpolator = new VolSurfaceInterpolator(expiries, strikes, new Matrix(volSurface), curveInterpolationMethod, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="properties"></param>
        /// <param name="expiries"></param>
        /// <param name="vols"></param>
        /// <param name="inputInstruments"></param>
        /// <param name="inputSwapRates"></param>
        /// <param name="inputBlackVolRates"></param>
        protected ExpiryTermStrikeVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, DateTime[] expiries, double[] vols,
            string[] inputInstruments, double[] inputSwapRates, double[] inputBlackVolRates)
        {
            Algorithm = PropertyHelper.ExtractAlgorithm(properties);
            PricingStructureIdentifier = new VolatilitySurfaceIdentifier(properties);
            var surfaceId = (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
            var expiryLength = expiries.Length;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength];
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                // extract the current expiry
                var expiryKeyPart = expiries[expiryIndex];
                var key = new ExpiryTenorStrikeKey(expiryKeyPart.ToString(CultureInfo.InvariantCulture), 0);
                _matrixIndexHelper.Add(key, pointIndex);
                // Add the value to the points array (dataPoints entry in the matrix)
                var coordinates = new PricingDataPointCoordinate[1];
                coordinates[0] = new PricingDataPointCoordinate{expiration = new TimeDimension[1]};
                coordinates[0].expiration[0] = new TimeDimension { Items = new object[] { expiries[expiryIndex] } };
                var pt = new PricingStructurePoint
                {
                    value = (decimal)vols[expiryIndex],
                    valueSpecified = true,
                    coordinate = coordinates,
                };
                points[pointIndex++] = pt;
            }
            // Record the row/column sizes of the inputs
            _matrixRowCount = expiries.Length;
            _matrixColumnCount = 1;
            PricingStructure = CreateVolatilityRepresentation(surfaceId);
            PricingStructureValuation = CreateDataPoints(points, surfaceId);
            if (inputInstruments != null)
            {
                int inputCount = inputInstruments.GetUpperBound(0);
                var assetQuotes = new List<BasicAssetValuation>();
                var assetSet = new List<Asset>();
                var itemsList = new List<ItemsChoiceType19>();
                for (int i = 0; i <= inputCount; i++)
                {
                    var assetProperties = new PriceableAssetProperties(inputInstruments[i]);
                    assetSet.Add(new SimpleIRSwap { id = inputInstruments[i], term = assetProperties.TermTenor });
                    var rateQuote = BasicQuotationHelper.Create((decimal) inputSwapRates[i], AssetMeasureEnum.MarketQuote, PriceQuoteUnitsEnum.DecimalRate);
                    var volQuote = BasicQuotationHelper.Create((decimal)inputBlackVolRates[i], AssetMeasureEnum.Volatility, PriceQuoteUnitsEnum.LogNormalVolatility);
                    assetQuotes.Add(new BasicAssetValuation
                                        {
                                            objectReference = new AnyAssetReference {href = inputInstruments[i]},
                                            definitionRef = assetProperties.TermTenor.ToString(),
                                            quote = new[] { rateQuote, volQuote }
                                        });
                    itemsList.Add(ItemsChoiceType19.simpleIrSwap);//TODO NOt actually correct.
                }
                var instrumentSet = new InstrumentSet { Items = assetSet.ToArray(), ItemsElementName = itemsList.ToArray()};
                ((VolatilityMatrix) PricingStructureValuation).inputs
                    = new QuotedAssetSet { assetQuote = assetQuotes.ToArray(), instrumentSet = instrumentSet };
            }
            // Generate an interpolator to use
            double[] expiryTerms = expiries.Select(a => (surfaceId.BaseDate - a).TotalDays / 365d).ToArray();
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            Interpolator = new VolSurfaceInterpolator(expiryTerms, new double[] { 1 }, new Matrix(vols), curveInterpolationMethod, true);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        protected ExpiryTermStrikeVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            var data = (VolatilityMatrix)fpmlData.Second;
            Algorithm = "Linear";//Add as a property in the id.
            //Creates the property collection. This should be backward compatable with V1.
            var surfaceId = new VolatilitySurfaceIdentifier(properties);
            PricingStructureIdentifier = surfaceId;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            Interpolator = new VolSurfaceInterpolator(data.dataPoints, curveInterpolationMethod, true, PricingStructureIdentifier.BaseDate);
            SetFpmlData(fpmlData);
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        #endregion

        #region Helpers

        private static VolatilityRepresentation CreateVolatilityRepresentation(VolatilitySurfaceIdentifier surfaceId)
        {
            return new VolatilityRepresentation
            {
                name = surfaceId.Name,
                id = surfaceId.Id,
                currency = surfaceId.Currency,
                asset = new AnyAssetReference { href = surfaceId.Instrument },
            };
        }

        private static VolatilityMatrix CreateDataPoints(PricingStructurePoint[] points, VolatilitySurfaceIdentifier surfaceId)
        {
            var datapoints = new MultiDimensionalPricingData
            {
                point = points,
                businessCenter = surfaceId.BusinessCenter,
                timing = surfaceId.QuoteTiming,
                currency = surfaceId.Currency,
                cashflowType = surfaceId.CashflowType,
                informationSource = surfaceId.InformationSources,
                measureType = surfaceId.MeasureType,
                quoteUnits = surfaceId.QuoteUnits
            };
            if (surfaceId.ExpiryTime != null)
            {
                datapoints.expiryTime = (DateTime)surfaceId.ExpiryTime;
                datapoints.expiryTimeSpecified = true;
            }
            if (surfaceId.ValuationDate != null)
            {
                datapoints.valuationDate = (DateTime)surfaceId.ValuationDate;
                datapoints.valuationDateSpecified = true;
            }
            else
            {
                datapoints.valuationDate = surfaceId.BaseDate;
                datapoints.valuationDateSpecified = true;
            }
            if (surfaceId.Time != null)
            {
                datapoints.time = (DateTime)surfaceId.Time;
                datapoints.timeSpecified = true;
            }
            if (surfaceId.QuotationSide != null)
            {
                datapoints.side = (QuotationSideEnum)surfaceId.QuotationSide;
                datapoints.sideSpecified = true;
            }
            var pricingStructureValuation = new VolatilityMatrix
            {
                dataPoints = datapoints,
                objectReference = new AnyAssetReference { href = surfaceId.Instrument },
                baseDate = new IdentifiedDate { Value = surfaceId.BaseDate },
                buildDateTime = surfaceId.BuildDateTime,
                buildDateTimeSpecified = true,
                definitionRef = surfaceId.CapFrequency
            };
            return pricingStructureValuation;
        }

        #endregion

        #region IPricingStructure Members

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        private void SetFpmlData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        /// <summary>
        /// Gets the VolatilityMatrix.
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            return (VolatilityMatrix)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the VolatilityRepresentation.
        /// </summary>
        /// <returns></returns>
        public VolatilityRepresentation GetVolatilityRepresentation()
        {
            return (VolatilityRepresentation)PricingStructure;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public override IValue GetValue(IPoint pt)
        {
            return new DoubleValue("dummy", Value(pt), pt);
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
//            var result =Interpolator.GetDiscreteSpace().GetClosestValues(pt);

            //var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            //var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));

            IList<IValue> result = new List<IValue>();

            return result;
        }

        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        #endregion

        #region IPointFunction Members

        /// <summary>
        /// This method uses the supplied Interpolation function to provide a value for a given point
        /// </summary>
        /// <param name="point">A point to get a value for</param>
        /// <returns></returns>
        public override double Value(IPoint point)
        {
            return Interpolator.Value(point);
        }

        #endregion

        #region Public Convenience Methods

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public object[,] GetSurface()
        {
            // Assign the matrix size
            var rawSurface = new object[_matrixRowCount + 1, _matrixColumnCount];
            var addTenor = _matrixIndexHelper.Keys[0].Tenor != null;
            // Zeroth row will be the column headers
            for (int column = 0; column < _matrixColumnCount; column++)
            {
                if (column == 0)
                    rawSurface[0, column] = "Option Expiry";
                else if (column == 1 && addTenor)
                    rawSurface[0, column] = "Tenor";
                else if (column > 0 && !addTenor)
                    rawSurface[0, column] = _matrixIndexHelper.Keys[column - 1].Strike.ToString(CultureInfo.InvariantCulture);
                else if (column > 1 && addTenor)
                    rawSurface[0, column] = _matrixIndexHelper.Keys[column - 2].Strike.ToString(CultureInfo.InvariantCulture);
            }
            // Add the data rows to the matrix
            var idx = 0;
            for (var row = 1; row < _matrixRowCount + 1; row++)
            {
                for (var column = 0; column < _matrixColumnCount; column++)
                {
                    if (column == 0)
                        rawSurface[row, column] = _matrixIndexHelper.Keys[idx].Expiry.ToString();
                    else if (column == 1 && addTenor)
                        rawSurface[row, column] = _matrixIndexHelper.Keys[idx].Tenor.ToString();
                    else if (column > 0 && !addTenor)
                        rawSurface[row, column] = GetVolatilityMatrix().dataPoints.point[_matrixIndexHelper.Values[idx++]].value;
                    else if (column > 1 && addTenor)
                    {
                        rawSurface[row, column] = GetVolatilityMatrix().dataPoints.point[_matrixIndexHelper.Values[idx++]].value;
                    }
                }
            }
            return rawSurface;
        }

        #endregion

        #region Process and Value

        /// <summary>
        /// Record the volatilities in the key helper and counts
        /// </summary>
        private void ProcessVolatilityRepresentation()
        {
            var row = 0;
            var column = 0;
            var columnExtra = 0;
            var strikeCheck = decimal.MinValue;
            var strike = decimal.MinValue;
            var dataPoints = GetVolatilityMatrix().dataPoints;
            var points = dataPoints.point;
            for (var idx = 0; idx < points.Length; idx++)
            {
                var tenor = points[idx].coordinate[0].term != null ? (Period)points[idx].coordinate[0].term[0].Items[0] : null;
                var strikeField = points[idx].coordinate[0].strike;
                if (strikeField != null)
                {
                    strike = strikeField[0];
                }
                if (idx == 0)
                {
                    strikeCheck = strike;
                    // Generate an interpolator to use
                    columnExtra = tenor == null ? 1 : 2;
                }
                else
                {
                    column++;
                    if (strike == strikeCheck)
                    {
                        row++;
                        column = 0;
                    }
                }
                if (points[idx].coordinate[0].expiration[0].Items[0] is Period expiry)
                {
                    //var expiry = (Period)points[idx].coordinate[0].expiration[0].Items[0];
                    _matrixIndexHelper.Add(new ExpiryTenorStrikeKey(expiry, null, strike), idx);
                }
                var expiryAsDate = points[idx].coordinate[0].expiration[0].Items[0] is DateTime;
                if (expiryAsDate && dataPoints.valuationDateSpecified)
                {
                    var periodInDays = ((DateTime)points[idx].coordinate[0].expiration[0].Items[0] - dataPoints.valuationDate).Days;
                    expiry = new Period { period = PeriodEnum.D, periodMultiplier = periodInDays.ToString(CultureInfo.InvariantCulture), periodSpecified = true };
                    _matrixIndexHelper.Add(new ExpiryTenorStrikeKey(expiry, null, strike), idx);
                }              
            }
            _matrixColumnCount = column + columnExtra + 1;
            _matrixRowCount = row + 1;
        }

        /// <summary>
        /// Generate an array of PricingStructurePoint from a set of input arrays
        /// The array can then be added to the Matrix
        /// </summary>
        /// <param name="expiry">Expiry values to use</param>
        /// <param name="strike">Strike values to use</param>
        /// <param name="volatility">An array of volatility values</param>
        /// <param name="strikeQuoteUnits">The strike quote units.</param>
        /// <param name="underlyingAssetReference">The underlying asset.</param>
        /// <returns></returns>
        private PricingStructurePoint[] ProcessRawSurface(String[] expiry, Double[] strike, double[,] volatility, PriceQuoteUnits strikeQuoteUnits, AssetReference underlyingAssetReference)
        {
            var expiryLength = expiry.Length;
            var strikeLength = strike.Length;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength * strikeLength];
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                // extract the current expiry
                var expiryKeyPart = expiry[expiryIndex];
                for (var strikeIndex = 0; strikeIndex < strikeLength; strikeIndex++)
                {                   
                    // Extract the strike to use in the helper key
                    var strikeKeyPart = strike[strikeIndex];
                    // extract the current tenor (term) of null if there are no tenors
                    // Extract the row,column indexed volatility
                    var vol = (decimal)volatility[expiryIndex, strikeIndex];
                    var key = new ExpiryTenorStrikeKey(expiryKeyPart, strikeKeyPart);
                    _matrixIndexHelper.Add(key, pointIndex);
                    // Add the value to the points array (dataPoints entry in the matrix)
                    var coordinates = new PricingDataPointCoordinate[1];
                    coordinates[0] = PricingDataPointCoordinateFactory.Create(expiry[expiryIndex], null, (Decimal)strike[strikeIndex]);
                    var pt = new PricingStructurePoint
                                 {
                                     value = vol,
                                     valueSpecified = true,
                                     coordinate = coordinates,
                                     underlyingAssetReference = underlyingAssetReference,
                                     quoteUnits = strikeQuoteUnits
                                 };
                    points[pointIndex++] = pt;
                }
            }
            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dimension1"></param>
        /// <param name="dimension2"></param>
        /// <returns></returns>
        public double GetValue(double dimension1, double dimension2)
        {
            IPoint point = new Point2D(dimension1, dimension2);
            return Interpolator.Value(point); 
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="expirationAsDate"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public double GetValueByExpiryDateAndStrike(DateTime valuationDate, DateTime expirationAsDate, double strike)
        {
            var value = 0d;

            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var time = new DateTimePoint1D(valuationDate, expirationAsDate);
                IPoint point = new Point2D(time.GetX(), strike);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var time = new DateTimePoint1D(GetBaseDate(), expirationAsDate);
                IPoint point = new Point2D(time.GetX(), strike);
                value = Value(point);
                return value;
            }

            return value;
        }

        ///<summary>
        ///</summary>
        ///<param name="term"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public double GetValueByExpiryTermAndStrike(string term, double strike)
        {
            var expiryTerm = PeriodHelper.Parse(term).ToYearFraction();
            IPoint point = new Point2D(expiryTerm, strike);
            return Interpolator.Value(point);
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// A class to model an expiry/term key pair.
        /// If a surface has only an expiry dimension then the term (tenor) will
        /// default to null.
        /// The class implements the IComparer&lt;ExpiryTenorpairKey&gt;.Compare() method
        /// to keep track of indexes and their keys.
        /// </summary>
        private class ExpiryTenorStrikeKey : IComparer<ExpiryTenorStrikeKey>
        {
            #region Properties

            public Period Expiry { get; }
            public Period Tenor { get; }
            public decimal Strike { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor for use as a Comparer
            /// </summary>
            public ExpiryTenorStrikeKey()
                : this(null, 0)
            {
            }

            /// <summary>
            /// Use the Expiry and Tenor to create a key.
            /// Expiry or tenor can be null but not both.
            /// </summary>
            /// <param name="expiry">Expiry key part</param>
            /// <param name="strike">Strike key part</param>
            public ExpiryTenorStrikeKey(string expiry, Double strike)
            {
                try
                {
                    Expiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
                }
                catch (System.Exception)
                {
                    Expiry = null;
                }
                try
                {
                    Tenor = null;
                }
                catch (System.Exception)
                {
                    Tenor = null;
                }

                try
                {
                    Strike = Convert.ToDecimal(strike);
                }
                catch (System.Exception)
                {
                    Strike = 0;
                }
            }

            /// <summary>
            /// Create a key from a Coordinate
            /// </summary>
            /// <param name="expiry"></param>
            /// <param name="tenor"></param>
            /// <param name="strike"></param>
            public ExpiryTenorStrikeKey(Period expiry, Period tenor, decimal strike)
            {
                Expiry = expiry;
                Tenor = tenor;
                Strike = strike;
            }

            #endregion

            #region IComparer<ExpiryTenorStrikeKey> Members

            /// <summary>
            /// if x &lt; y return -1
            /// if x &gt; y return  1
            /// if x = y    return  0
            /// </summary>
            /// <param name="x">The key - x</param>
            /// <param name="y">The key - y</param>
            /// <returns>An indication of the relative positions of x and y</returns>
            public int Compare(ExpiryTenorStrikeKey x, ExpiryTenorStrikeKey y)
            {
                // Test x and y as intervals Expiry before Tenor?
                var xExpiryAsInterval = x.Expiry;
                var xTenorAsInterval = x.Tenor;
                var yExpiryAsInterval = y.Expiry;
                var yTenorAsInterval = y.Tenor;
                var xStrike = x.Strike;
                var yStrike = y.Strike;

                // Test equality
                if (IntervalHelper.Less(xExpiryAsInterval, yExpiryAsInterval))
                    return -1;
                if (IntervalHelper.Greater(xExpiryAsInterval, yExpiryAsInterval))
                    return 1;
                if (IntervalHelper.Less(xTenorAsInterval, yTenorAsInterval))
                    return -1;
                if (IntervalHelper.Greater(xTenorAsInterval, yTenorAsInterval))
                    return 1;
                if (xStrike < yStrike)
                    return -1;
                return xStrike > yStrike ? 1 : 0;
            }

            #endregion
        }

        #endregion
    }
}
