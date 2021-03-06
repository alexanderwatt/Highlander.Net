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
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Core.Common;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Metadata.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Curves
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CurveBase : PricingStructureBase, ICurve
    {

        /// <summary>
        /// The namespace
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Holds the algorithm  information.
        /// </summary>
        public PricingStructureAlgorithmsHolder Holder { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        protected CurveBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected CurveBase(ILogger logger, ICoreCache cache, string nameSpace, PricingStructureIdentifier curveIdentifier)
        {
            PricingStructureIdentifier = curveIdentifier;
            NameSpace = nameSpace;
            if (cache != null)
            {
                Holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveIdentifier.PricingStructureType, curveIdentifier.Algorithm);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="pricingStructure"></param>
        protected CurveBase(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> pricingStructure, PricingStructureIdentifier curveIdentifier)
            : base(pricingStructure)
        {
            PricingStructureIdentifier = curveIdentifier;
            NameSpace = nameSpace;
            if (cache != null)
            {
                Holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveIdentifier.PricingStructureType, curveIdentifier.Algorithm);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveAlgorithm">The curve algorithm.</param>
        protected CurveBase(string nameSpace, PricingStructureIdentifier curveIdentifier, Algorithm curveAlgorithm)
        {
            PricingStructureIdentifier = curveIdentifier;
            NameSpace = nameSpace;
            Holder = new PricingStructureAlgorithmsHolder(curveAlgorithm);
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public abstract QuotedAssetSet GetQuotedAssetSet();

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public abstract TermCurve GetTermCurve();

        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measure type to determine which one.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="values">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public abstract Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, decimal[] values, string measureType);

        /// <summary>
        /// Creates the basic rate curve risk set.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public abstract List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetValue(DateTime baseDate, DateTime targetDate)
        {
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            return (double)GetValue(point).Value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetValue(DateTime targetDate)
        {
            IPoint point = new DateTimePoint1D(GetBaseDate(), targetDate);
            return (double)GetValue(point).Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, Decimal> GetInputs()
        {
            Pair<PricingStructure, PricingStructureValuation> fpml = GetFpMLData();
            if (!(fpml.Second is YieldCurveValuation yieldCurve))
            {
                return new Dictionary<string, decimal>();
            }
            BasicAssetValuation[] inputs = yieldCurve.inputs.assetQuote;
            Asset[] instrumentSet = yieldCurve.inputs.instrumentSet.Items;
            IEnumerable<KeyValuePair<string, decimal>> results
                        = from a in instrumentSet
                          join i in inputs on a.id equals i.objectReference.href
                          where i.quote[0].measureType.Value == "MarketQuote"
                          select new KeyValuePair<string, Decimal>(a.id, i.quote[0].value);
            IDictionary<string, decimal> data = results.ToDictionary(i => i.Key, i => i.Value);
            return data;
        }

        /// <summary>
        /// Gets the asset quotations.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, decimal> GetAssetQuotations()
        {
            Asset[] assetSet = GetQuotedAssetSet().instrumentSet.Items;
            IDictionary<string, decimal> result = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase);
            int index = 0;
            foreach (BasicAssetValuation asset in GetQuotedAssetSet().assetQuote)
            {
                decimal value = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", asset.quote).value;
                result.Add(assetSet[index].id, value);
                index++;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected IBusinessCalendar ExtractFixingCalendar(ICoreCache cache, string nameSpace, NamedValueSet properties)
        {
            string[] fixingCalendars = properties.GetArray<string>("FixingCenters");
            IBusinessCalendar fixingCalendar = null;
            if (fixingCalendars != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingCalendars, nameSpace);
            }
            return fixingCalendar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected IBusinessCalendar ExtractPaymentCalendar(ICoreCache cache, string nameSpace, NamedValueSet properties)
        {
            string[] paymentCalendars = properties.GetArray<string>("PaymentCenters");
            IBusinessCalendar paymentCalendar = null;
            if (paymentCalendars != null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentCalendars, nameSpace);
            }
            return paymentCalendar;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public abstract void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar);

        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract override string GetAlgorithm();

        #region Save method

        public string SaveToFile(string filename)
        {
            var assembler = GetFpMLData();
            var market = new Market();
            var ps = assembler.First;
            market.id = ps.name;
            market.name = ps.id;
            market.Items = new[] { ps };
            market.Items1 = new[] {assembler.Second};
            XmlSerializerHelper.SerializeToFile(market, filename);
            return filename;
        }

        #endregion
    }
}