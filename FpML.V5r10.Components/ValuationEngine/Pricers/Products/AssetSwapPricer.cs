#region Usings

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.Models.Rates.Swap;
using Orion.Util.Helpers;
using Orion.Util.Logging;

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    public class AssetSwapPricer : InterestRateSwapPricer
    {
        /// <summary>
        /// 
        /// </summary>
        public Bond UnderlyingBond { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AssetSwapPricer()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="bondReference"></param>
        public AssetSwapPricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference, Bond bondReference)
            : base(logger, cache, nameSpace, legCalendars, swapFpML, basePartyReference, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="bondReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        public AssetSwapPricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars, 
            Swap swapFpML, string basePartyReference, Bond bondReference, Boolean forecastRateInterpolation)
            : base(logger, cache, nameSpace, legCalendars, swapFpML, basePartyReference, forecastRateInterpolation)
        {
            AnalyticsModel = new SimpleIRSwapInstrumentAnalytic();
            ProductType = ProductTypeSimpleEnum.AssetSwap;
            UnderlyingBond = bondReference;
        }

    }
}