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

#region Usings

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Factory;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// 
    /// </summary>
    public class FxDerivedCurve : CurveBase, IFxCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public DateTime SpotDate { get; }

        /// <summary>
        /// 
        /// </summary>
        public FxCurve FxCurve1 { get; }

        /// <summary>
        /// 
        /// </summary>
        public FxCurve FxCurve2 { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency1 { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency2 { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FxDerivedCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="curve1">The currency1 data. This should be against USD. 
        /// This must also be the currency1 curve for the derived fx curve.</param>
        /// <param name="curve2">The currency2 data. This should be against USD.</param>
        /// <param name="newFxCurveProperties">The new FxCurve properties .</param>
        public FxDerivedCurve(ILogger logger, ICoreCache cache, string nameSpace, FxCurve curve1, FxCurve curve2, NamedValueSet newFxCurveProperties)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Fx, newFxCurveProperties); 
            FxCurve1 = curve1;
            FxCurve2 = curve2;
            Currency1 = curve1.GetQuotedCurrencyPair().currency1.Value != "USD" ? curve1.GetQuotedCurrencyPair().currency1.Value : curve1.GetQuotedCurrencyPair().currency2.Value;
            Currency2 = curve2.GetQuotedCurrencyPair().currency1.Value != "USD" ? curve2.GetQuotedCurrencyPair().currency1.Value : curve2.GetQuotedCurrencyPair().currency2.Value;
            SpotDate = GetSpotDate(logger, cache, nameSpace, null, null, FxCurve1.GetBaseDate());//Create the new identifier.
            PricingStructureIdentifier = new FxCurveIdentifier(newFxCurveProperties);
        }

        #endregion

        #region Value Override

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            double currency1Value = FxCurve1.Value(point);
            double currency2Value = FxCurve2.Value(point);
            
            double value = currency1Value / currency2Value;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IValue GetValue(IPoint pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return ((FxCurveIdentifier)PricingStructureIdentifier).Algorithm;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public decimal GetSpotRate()
        {
            var spotRate1 = (double)FxCurve1.GetSpotRate();
            var spotRate2 = (double)FxCurve2.GetSpotRate();
            return (decimal)(spotRate1 / spotRate2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DateTime GetSpotDate()
        {
            return SpotDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public DateTime GetSpotDate(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            BasicAssetValuation bav = BasicAssetValuationHelper.Create(BasicQuotationHelper.Create(0, "MarketQuote"));
            string identifier = Currency1 + Currency2 + "-FxSpot-SP";//Currency2 would normally be USD.
            var priceableAsset = (IPriceableFxAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, identifier, baseDate, bav, fixingCalendar, rollCalendar);
            if (priceableAsset!=null)
            {
                return priceableAsset.GetRiskMaturityDate();
            }
            return FxCurve1.GetSpotDate();//TODO Default to the first curve. Should merge the two!
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public double GetForward(DateTime valuationDate, DateTime targetDate)
        {
            double value = 0d;

            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var point = new DateTimePoint1D(valuationDate, targetDate);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var point1 = new DateTimePoint1D(GetBaseDate(), targetDate);
                double value1 = Value(point1);
                var point2 = new DateTimePoint1D(GetBaseDate(), valuationDate);
                double value2 = Value(point2);
                value = value1 / value2;
                return value;
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="isBaseCurve"></param>
        /// <param name="derivedCurveProperties"></param>
        /// <returns></returns>
        public IRateCurve GenerateRateCurve(IRateCurve referenceCurve, bool isBaseCurve, NamedValueSet derivedCurveProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="values"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public override Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, Decimal[] values, String measureType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the basic rate curve risk set.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return ((FxCurveValuation)PricingStructureValuation).spotRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TermCurve GetTermCurve()
        {
            return ((FxCurveValuation)PricingStructureValuation).fxForwardCurve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
