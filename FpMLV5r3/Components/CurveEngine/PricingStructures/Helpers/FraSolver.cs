﻿/*
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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Core.Common;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Assets.Rates.Cash;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;
using DayCounterHelper = Highlander.Reporting.Analytics.V5r3.DayCounters.DayCounterHelper;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Helpers
{
    /// <summary>
    /// This class is responsible for finding rate adjustments
    /// should be made to cash rate to get the equivalent FRA 
    /// rates that could be fed into Calypso
    /// </summary>
    public class FraSolver
    {

        #region Const Variables

        /// <summary>
        /// The value of a shock is 1 basis point
        /// </summary>
        private const decimal ShockValue = 0.0001m;

        #endregion

        #region Constructor

        ///<summary>
        ///</summary>
        public FraSolver(List<decimal> initialRates)
        {
            InitialRates = initialRates;
        }

        /// <summary>
        /// public constructs, Fra Rate class acts as a
        /// container which holds all the necessary information
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="cache">cache</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="fixingCalendar">fixingCalendar</param>
        /// <param name="rollCalendar">rollCalendar</param>
        /// <param name="properties">curve properties</param>
        /// <param name="instruments">list of instruments</param>
        /// <param name="values">value of each instrument</param>
        /// <param name="initialFraRates">initial guesses for fra rate</param>
        /// <param name="shockedInstrumentIndices">array of shocked instrument indices</param>
        /// <param name="initialRates"></param>
        public FraSolver(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, 
            List<string> instruments,
            IEnumerable<decimal> values,
            IEnumerable<decimal> initialFraRates, 
            ICollection<int> shockedInstrumentIndices, 
            List<decimal> initialRates)
        {
            //SetInstruments(GetInstrumentIds(instruments));
            Instruments = instruments;
            Properties = properties.Clone();
            SetRates(values);
            FixingCalendar = fixingCalendar;
            RollCalendar = rollCalendar;
            //initially set the rates value to the initial curve rates
            SetRateAdjustment(shockedInstrumentIndices.Count);
            SetFraGuesses(initialFraRates);
            Currency = Properties.GetString(CurveProp.Currency1, true);
            SetDayCounter(cache, nameSpace, Currency);
            SetShockedInstrumentIndices(shockedInstrumentIndices);
            SetPropertiesOfShockedCurves(properties, shockedInstrumentIndices.Count);
            SetInitialCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, Properties, Instruments.ToArray(), Rates.ToArray());
            SetFraDates(cache, nameSpace, _initialCurve, ShockedInstrumentsIndices);
            SetInitalFraRates(_initialCurve, FraStartDates, FraEndDates );
            InitialRates = initialRates;
        }

        #endregion

        #region Business Logic

        ///<summary>
        ///</summary>
        public List<decimal> InitialRates { get; }

        ///<summary>
        ///</summary>
        public List<decimal> InitialFraRates { get; private set; }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public List<decimal> CalculateEquivalentFraValues(ILogger logger, ICoreCache cache, string nameSpace)//ICoreCache!
        {
            var shockedCurves = new List<RateCurve>();
            var ratesAdj = new List<decimal>();
            var updatedRates = new List<decimal>(Rates.ToArray());
            var fraValues = CalculateFraValuesFromCurve(_initialCurve, FraStartDates, FraEndDates);
            int len = ShockedInstrumentsIndices.Count;
            _differences = new List<decimal>();
            for (int i = 0; i < len; ++i )      
                SetDifferences(fraValues.ToArray(),FraGuesses.ToArray());
            while (System.Math.Abs(SumOfDifferences(_differences)) > 0.000001m)
            {
                //1. Create shocked Curves
                shockedCurves.Clear();
                shockedCurves = SetShockedCurves(logger, cache, nameSpace, ShockedCurvesProperties, Instruments, updatedRates,
                                                 ShockedInstrumentsIndices);

                //2. Calculate rates Adjustments
                ratesAdj.Clear();
                ratesAdj = CalculateRateAdjustments(_initialCurve, shockedCurves,
                                                    FraStartDates, FraEndDates, _differences);
                //3. Reset Rates
                UpdateRateAdjustments(RateAdjustmens, ratesAdj);
                updatedRates = UpdateRates(Rates, ShockedInstrumentsIndices, RateAdjustmens);

                //4. Update the curve
                _initialCurve = CreateCurve(logger, cache, nameSpace, Properties, Instruments.ToArray(), updatedRates.ToArray());

                //5.Calculate Differences
                _differences.Clear();
                fraValues.Clear();
                fraValues = CalculateFraValuesFromCurve(_initialCurve, FraStartDates, FraEndDates);

                SetDifferences(fraValues.ToArray(), FraGuesses.ToArray());
            }

            return RateAdjustmens;
        }

        /// <summary>
        /// Create initial Curve
        /// store it in _initialCurve
        /// </summary>
        private void SetInitialCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
                                     NamedValueSet properties, string[] instruments, decimal[] rates)
        {
            _initialCurve = PricingStructureFactory.CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, rates, null) as RateCurve;
        }

        private void SetFraDates(ICoreCache cache, string nameSpace, RateCurve initialCurve, IEnumerable<int> instrumentIndicies) 
                                    
        {
            FraStartDates = new List<DateTime>();
            FraEndDates = new List<DateTime>();
            foreach (int i in instrumentIndicies)
            {
                CheckDepositInstrument(i);
                var rateAssetController = (PriceableDeposit)initialCurve.PriceableRateAssets[i];
                DateTime startDate = CalculateFraStartTime(rateAssetController);
                FraStartDates.Add(startDate);
                var deposit = rateAssetController;
                //BusinessDayConventionEnum adjustment = deposit.BusinessDayAdjustments.businessDayConvention;
                BusinessCenters bs = deposit.SpotDateOffset.businessCenters;
                var calendar = BusinessCenterHelper.ToBusinessCalendar(cache, bs, nameSpace);
                DateTime endDate = CalculateFraEndTime(calendar, rateAssetController);
                FraEndDates.Add(endDate);
            }
        }

        private void SetInitalFraRates(RateCurve curve, List<DateTime> fraStartDate,  List<DateTime> fraEndDate)
        {
            InitialFraRates = CalculateFraValuesFromCurve(curve, fraStartDate, fraEndDate);
        }

        private static List<RateCurve> SetShockedCurves(ILogger logger, ICoreCache cache, string nameSpace, IList<NamedValueSet> properties, List<string> instruments,
                                                 List<decimal> rates, IList<int> indexOfShockedInstruments)
        {
            if (properties.Count != indexOfShockedInstruments.Count)
                throw new ArgumentException(
                    "The number of properties and index of shocked instruments are not the same.");
            var shockedCurves = new List<RateCurve>();
            int len = indexOfShockedInstruments.Count;
            for(int i = 0; i < len; ++i)
            {
                RateCurve shockedCurve = CreateShockedCurve(logger, cache, nameSpace, properties[i], instruments.ToArray(),
                                                            rates.ToArray(), indexOfShockedInstruments[i]);
                shockedCurves.Add(shockedCurve);
            }
            return shockedCurves;
        }

        /// <summary>
        /// Create a shocked curve
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="properties"></param>
        /// <param name="instruments"></param>
        /// <param name="rates"></param>
        /// <param name="indexOfShockedInstrument"></param>
        /// <returns></returns>
        public static RateCurve CreateShockedCurve(ILogger logger, ICoreCache cache,
            string nameSpace,
            NamedValueSet properties,
            string[] instruments,
            decimal[] rates,
            int indexOfShockedInstrument)
        {
            int len = instruments.Length;
            var additional = new decimal[len];
            additional.Initialize();
            var newRates = new decimal[rates.Length];
            rates.CopyTo(newRates, 0);
            // check the index
            if( indexOfShockedInstrument < 0 || indexOfShockedInstrument >= len )
                throw new ArgumentException("The index of shocked instrument is not within the range");
            // bump the rate one basis point
            newRates[indexOfShockedInstrument] += ShockValue;
            var qas = AssetHelper.Parse(instruments, newRates, additional);
            return new RateCurve(logger, cache, nameSpace, properties, qas, null, null);
        }


        private static RateCurve CreateCurve(ILogger logger, ICoreCache cache, 
            string nameSpace,
            NamedValueSet properties,
            string[] instruments,
            decimal[] rates)
        {
            var additional = new decimal[instruments.Length];
            additional.Initialize();
            var qas = AssetHelper.Parse(instruments, rates, additional);
            return new RateCurve(logger, cache, nameSpace, properties, qas, null, null);
        }


        /// <summary>
        /// Calculate the fra value from the given curve 
        /// </summary>
        /// <param name="rateCurve">rate curve</param>
        /// <param name="fraStartDates">List of fra start dates</param>
        /// <param name="fraEndDates">List of fra end dates</param>
        /// <returns></returns>
        public List<decimal> CalculateFraValuesFromCurve(RateCurve rateCurve, 
                                                         List<DateTime> fraStartDates,
                                                         List<DateTime> fraEndDates)
        {
            var calculatedFraRates = new List<decimal>();
            int len = fraStartDates.Count;
            for(int i=0; i < len; ++i )
            {
                decimal df1 = Convert.ToDecimal(rateCurve.GetDiscountFactor(fraStartDates[i]));
                decimal df2 = Convert.ToDecimal(rateCurve.GetDiscountFactor(fraEndDates[i]));
                double period = DayCounter.YearFraction(fraStartDates[i], fraEndDates[i]);

                decimal fraRate = CalculateForwardRate(df1, df2, (decimal)period);
                calculatedFraRates.Add(fraRate);
            }         
            return calculatedFraRates;
        }

        /// <summary>
        /// Calculating FRA
        /// </summary>
        /// <param name="firstDiscountFactor">df1</param>
        /// <param name="secondDiscountFactor">df2</param>
        /// <param name="timeFraction">alpha</param>
        /// <returns></returns>
        private static decimal CalculateForwardRate(decimal firstDiscountFactor,
                                             decimal secondDiscountFactor,
                                             decimal timeFraction)
        {
            return (firstDiscountFactor - secondDiscountFactor)/(secondDiscountFactor*timeFraction);         
        }


        private List<decimal> CalculateRateAdjustments(RateCurve initialCurve,
                                                       List<RateCurve> curves,
                                                       List<DateTime> fraStartDates,
                                                       List<DateTime> fraEndDates,                              
                                                       IList<decimal> differences)
        {
            Matrix partialDerivatives = CalculatePartialDerivatives(initialCurve, curves, fraStartDates, fraEndDates);
            Matrix partialDerivativesInverse = CalculateInverseOfPartialDerivatives(partialDerivatives);
            int row = partialDerivativesInverse.ColumnCount;
            if( row != differences.Count )
                throw new System.Exception();
            var diff = new Matrix(row, 1);
            for(int i= 0; i <row; ++i)
                diff.SetValue(Convert.ToDouble(differences[i]), i, 0);
            var adjustments = partialDerivativesInverse*diff;
            var adj = new List<decimal>();
            for( int i =0; i < row; ++i)
                adj.Add(Convert.ToDecimal(adjustments[i, 0]));
            return adj;
        }

        

        /// <summary>
        /// Update the rate, only the shocked instruments
        /// rate will be updated
        /// 
        /// Pre-condition: ShockedInstrumentIndices and
        /// adjustments should be of same length
        /// </summary>
        /// <param name="oldRates"></param>
        /// <param name="shockedInstrumentIndices"></param>
        /// <param name="adjustments"></param>
        /// <returns></returns>
        private static List<decimal> UpdateRates(List<decimal> oldRates, 
                                          IList<int> shockedInstrumentIndices, 
                                          IList<decimal> adjustments)
        {
            if( shockedInstrumentIndices.Count != adjustments.Count )
                throw new System.Exception("The number of shocked instruments and the number of " 
                                    +  "adjustments are not the same");      
            int len = shockedInstrumentIndices.Count;
            var updatedRates = new List<decimal>(oldRates.ToArray());
            for(int i = 0; i < len; ++i)
            {
                int index = shockedInstrumentIndices[i];
                //oldRates[index] += adjustments[i];           ///100.0m;
                updatedRates[index ] = oldRates[index] + adjustments[i];
            }
            return updatedRates;
        }


        private static void UpdateRateAdjustments(IList<decimal> oldAdjustmentRates, IList<decimal> newAdjustments)
        {
            if( oldAdjustmentRates.Count != newAdjustments.Count)
            {
                throw new ArgumentException("The number of old adjustment rates and new adjustments are not the same.");
            }
            int len = oldAdjustmentRates.Count;
            for (int i = 0; i < len; ++i)
                oldAdjustmentRates[i] += -1.0m*newAdjustments[i]/10000.0m;
        }

        /// <summary>
        /// Calculate the matrix of partial derivatives from the
        /// List of shocked curves
        /// </summary>
        /// <param name="shockedCurves"></param>
        /// <param name="fraStartDates">List of fra start dates</param>
        /// <param name="fraEndDates">List of fra end dates</param>
        /// <param name="initialCurve"></param>
        /// <returns></returns>
        public Matrix CalculatePartialDerivatives(RateCurve initialCurve,
                                                         List<RateCurve> shockedCurves,
                                                         List<DateTime> fraStartDates,
                                                         List<DateTime> fraEndDates)
        {
            int len = fraStartDates.Count;
            var partialDerivative = new Matrix(len, len);
            List<decimal> initialFraRates = CalculateFraValuesFromCurve(initialCurve, fraStartDates, fraEndDates);
            for (int i = 0; i < shockedCurves.Count; ++i )
            {
                List<decimal> fraRates = CalculateFraValuesFromCurve(shockedCurves[i],
                                                                     fraStartDates,
                                                                     fraEndDates);
                for(int j =0 ; j < len; ++j)
                {
                    partialDerivative.SetValue(Convert.ToDouble(fraRates[j] - initialFraRates[j]) * 10000, j, i);
                }
            }
            return partialDerivative;
        }


        private static Matrix CalculateInverseOfPartialDerivatives(Matrix partialDerivativeMatrix)
        {
            return partialDerivativeMatrix.Inverse();
        }
       
        #endregion

        #region Validation Methods

        /// <summary>
        /// PreCondition: the initialCurve must have been 
        /// assigned
        /// </summary>
        /// <param name="index"></param>
        private void CheckDepositInstrument(int index)
        {
            if( _initialCurve == null)
                throw new System.Exception("The initial curve has not been constructed.");        
            IPriceableRateAssetController rateAssetController = _initialCurve.PriceableRateAssets[index];
            if(!rateAssetController.Id.ToUpper().Contains("DEPOSIT"))
                throw new ArgumentException("The specified asset is not CASH asset.");

        }


        //private void CheckShockedProperties()
        //{
        //    if (ShockedInstrumentsIndices.Count <= 0)
        //        throw new ArgumentException("No Cash Instruments have been specified.");


        //    if (ShockedCurvesProperties.Count <= 0)
        //        throw new Exception("Properties of the shocked curves have been setup.");
        //}


        #endregion

        #region Helper Functions

        public static void GetFraGuesses(List<object> guesses, ref List<decimal> Guesses, ref List<int> indicies)
        {
            int len = guesses.Count;
            for (int i = 0; i < len; ++i)
            {
                if (guesses[i] != null)
                {
                    if (guesses[i].GetType() == Type.GetType("System.Decimal") ||
                        guesses[i].GetType() == Type.GetType("System.Double"))
                    {
                        Guesses.Add(Convert.ToDecimal(guesses[i]));
                        indicies.Add(i);
                    }
                }
            }
        }

        public static List<T> GetObjects<T>(object[,] table, int column)
        {
            var objects = new List<T>();
            // Get the number of rows to search through
            int maxRows = table.GetUpperBound(0) + 1;
            for (int row = 1; row < maxRows; row++)
            {
                var result = table[row, column];
                objects.Add((T)result);
            }
            return objects;
        }

        public static int FindHeader(object[,] table, string search)
        {
            int col = 0;
            bool found = false;
            // Get the number of cols to search through
            int maxCols = table.GetUpperBound(1) + 1;
            for (int c = 0; c < maxCols; ++c)
            {
                if (table[0, c].ToString().ToUpper() == search.ToUpper())
                {
                    col = c;
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new System.Exception("The header " + search + " is not found");
            return col;
        }

        ///<summary>
        ///</summary>
        ///<param name="curve"></param>
        ///<param name="index"></param>
        ///<returns></returns>
        public IPriceableRateAssetController FindShockedInstrument(RateCurve curve, int index)
        {
            List<IPriceableRateAssetController> rateAssetController = curve.PriceableRateAssets;
            return rateAssetController[index];     
        }

        /// <summary>
        /// Calculate the start date of the FRA which 
        /// will replace the cash instrument
        /// </summary>
        /// <param name="assetController"></param>
        /// <returns></returns>
        public DateTime CalculateFraStartTime(IPriceableRateAssetController assetController)
        {
            return assetController.GetRiskMaturityDate();
        } 


        /// <summary>
        /// Calculate the end date of the FRA which 
        /// will replace the cash instrument
        /// </summary>
        /// <param name="calendar"></param>
        /// <param name="assetController"></param>
        /// <returns></returns>
        public DateTime CalculateFraEndTime(IBusinessCalendar calendar, IPriceableRateAssetController assetController)//, 
            //string currency, string rollConvention)
        {                    
            var deposit = (PriceableDeposit) assetController;
            BusinessDayConventionEnum adjustment = deposit.BusinessDayAdjustments.businessDayConvention;
            return DatePeriodHelper.AddPeriod(assetController.GetRiskMaturityDate(),
                                                                             "3M", calendar, adjustment.ToString(), null);

        }

        internal static string BusinessCentersAsString(BusinessCenters businessCenters)
        {
            return string.Join("-", businessCenters.businessCenter.Select(bc => bc.Value).ToArray());
        }


        /// <summary>
        /// PreCondition, check the initialCurveProperties
        /// </summary>
        /// <param name="initialCurveProperties"></param>
        /// <param name="numOfShockedInstruments"></param>
        private void SetPropertiesOfShockedCurves(NamedValueSet initialCurveProperties, int numOfShockedInstruments)
        {
            ShockedCurvesProperties = new List<NamedValueSet>();
            for (int i = 0; i < numOfShockedInstruments; ++i )
            {
                NamedValueSet newProperties = initialCurveProperties.Clone();
                string curveName = initialCurveProperties.GetString(CurveProp.CurveName, false);
                if (!string.IsNullOrEmpty(curveName))
                {
                    newProperties.Set(CurveProp.CurveName, curveName + "ShockedCurve-" + i);
                }
                string id = initialCurveProperties.GetString("Identifier", false);
                if (!string.IsNullOrEmpty(id))
                {
                    newProperties.Set(CurveProp.CurveName, id + "ShockedCurve-" + i);
                }
                ShockedCurvesProperties.Add(newProperties);
            }
        }

        ///// <summary>
        ///// Create Simple Fra Node Struct 
        ///// from the config file
        ///// </summary>
        ///// <param name="currency"></param>
        ///// <returns></returns>
        //private static NQPH.SimpleFraNodeStruct CreateSimpleFraNodeStruct(string currency)
        //{
        //    Assembly pa = Assembly.Load("Highlander.priceableAssets");
        //    var resource = ResourceHelper.GetResource(pa, "Highlander.PriceableAssets.Configuration." + "Assets.config");
        //    XmlDocument config = new XmlDocument();
        //    config.LoadXml(resource);

        //    string xPathFormat = String.Format("//f:Instruments/f:Instrument[@Currency='{0}']", currency);
        //    var maturityTenorFilter = new XmlConfigurationSingleAttributeFilter("MaturityTenor", string.Empty);

        //    return CreateNodeStructFromAsset<NQPH.SimpleFraNodeStruct>(config, xPathFormat, "Fra", maturityTenorFilter);
        //}

       
        private static decimal SumOfDifferences(IEnumerable<decimal> differences)
        {
            return differences.Sum();
        }

        #endregion

        #region Set or Get Private Data Memeber

        ///<summary>
        ///</summary>
        public string Currency
        {
            get; set;
        }

        //private void SetInstruments(IEnumerable<string> instruments)
        //{
        //    Instruments = new List<string>(instruments);
        //}

        //private static IEnumerable<string> GetInstrumentIds(object[] instruments)
        //{
        //    var sInstruments = new string[instruments.Length];
        //    for(int i=0; i< instruments.Length; ++i)
        //        sInstruments[i] = instruments[i].ToString();
        //    return sInstruments;
        //}

        private void SetShockedInstrumentIndices(IEnumerable<int> shockedIndices)
        {
            ShockedInstrumentsIndices = new List<int>(shockedIndices);
        }

        private void SetFraGuesses(IEnumerable<decimal> fraGuesses)
        {
            FraGuesses = new List<decimal>(fraGuesses);
        }


        private void SetRates(IEnumerable<decimal> rates)
        {
            Rates = new List<decimal>(rates);
        }

        private void SetRateAdjustment(int size)
        {
            RateAdjustmens = new List<decimal>(size); 
            for(int i=0;  i < size ; ++i )
                RateAdjustmens.Add(0.0m);
        }

        private void SetDifferences(decimal[] fraValues, decimal[] initialGuesses)
        {
            if( fraValues.Length != initialGuesses.Length )
                throw new ArgumentException("The number of fra values and initial Fra Guesses are not the same.");
            int len = fraValues.Length;
            _differences = new List<decimal>(len);
            for (int i = 0; i < len; ++i)
                _differences.Add((fraValues[i] - initialGuesses[i])*10000);
        }

        private void SetDayCounter(ICoreCache cache, string nameSpace, string currency)
        {
            Instrument instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, currency, "ZeroRate");
            if (instrument.InstrumentNodeItem is ZeroRateNodeStruct zeroRateNode)
            {
                DayCounter = DayCounterHelper.Parse(zeroRateNode.DayCountFraction.Value);
            }
            if (DayCounter == null)
            {
                throw new InvalidOperationException($"DayCounter is invalid for '{currency}'");
            }
        }

        ///<summary>
        ///</summary>
        public List<string> Instruments { get; }

        ///<summary>
        ///</summary>
        public List<decimal> FraGuesses { get; private set; }

        ///<summary>
        ///</summary>
        public List<decimal> Rates { get; private set; }

        ///<summary>
        ///</summary>
        public List<int> ShockedInstrumentsIndices { get; private set; }

        ///<summary>
        ///</summary>
        public List<NamedValueSet> ShockedCurvesProperties { get; private set; }

        ///<summary>
        ///</summary>
        public List<DateTime> FraStartDates { get; private set; }

        ///<summary>
        ///</summary>
        public List<DateTime> FraEndDates { get; private set; }

        ///<summary>
        ///</summary>
        public NamedValueSet Properties { get; }

        ///<summary>
        ///</summary>
        public List<decimal> RateAdjustmens { get; private set; }

        private IDayCounter DayCounter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IBusinessCalendar FixingCalendar { get; }

        /// <summary>
        /// 
        /// </summary>
        public IBusinessCalendar RollCalendar { get; }

        #endregion  

        #region Private Data Members

        /// <summary>
        /// The initial curve id
        /// </summary>
        private RateCurve _initialCurve;

        /// <summary>
        /// List of differences between initial fra guess
        /// and calculated fra values after adjustments
        /// </summary>
        private List<decimal> _differences;

        #endregion
    }
}