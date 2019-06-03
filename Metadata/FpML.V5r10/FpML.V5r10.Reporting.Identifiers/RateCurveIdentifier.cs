#region Using directives

using System;
using FpML.V5r10.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class RateCurveIdentifier : PricingStructureIdentifier
    {
        ///<summary>
        /// The forecast rate index.
        ///</summary>
        public ForecastRateIndex ForecastRateIndex { get; set; }

        ///<summary>
        /// Used for inflation curves.
        ///</summary>
        public Period InflationLag { get; set; }

        ///<summary>
        /// The reference curve type and name e.g RateCurve.AUD-BBR-BBSW-1M
        ///</summary>
        public String ReferenceCurveTypeAndName { get; set; }

        ///<summary>
        /// The reference curve unique id e.g Orion.Market.QR_LIVE.RateCurve.AUD-BBR-BBSW-1M
        ///</summary>
        public String ReferenceCurveUniqueId { get; set; }

        ///<summary>
        /// The reference curve type and name e.g FxCurve.AUD-USD
        ///</summary>
        public String ReferenceFxCurveTypeAndName { get; set; }

        ///<summary>
        /// The reference curve unique id e.g Orion.Market.QR_LIVE.FxCurve.AUD-USD
        ///</summary>
        public String ReferenceFxCurveUniqueId { get; set; }

        ///<summary>
        /// The reference curve type and name e.g RateCurve.AUD-BBR-BBSW-1M
        ///</summary>
        public String ReferenceCurrency2CurveTypeAndName { get; set; }

        ///<summary>
        /// The reference curve unique id e.g Orion.Market.QR_LIVE.RateCurve.AUD-BBR-BBSW-1M
        ///</summary>
        public String ReferenceCurrency2CurveId { get; set; }

        /// <summary>
        /// The CreditSeniority.
        /// </summary>
        public CreditSeniority CreditSeniority { get; private set; }

        /// <summary>
        /// The CreditInstrumentId.
        /// </summary>
        public InstrumentId CreditInstrumentId { get; private set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public RateCurveIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="pricingStructureType">The pricing strucutre type.</param>
        ///<param name="curveName">The curve name.</param>
        ///<param name="buildDateTime">The build date time.</param>
        ///<param name="algorithm">The algorithm.</param>
        public RateCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime, string algorithm)
            : base(pricingStructureType, curveName, buildDateTime, algorithm)
        {
            SetProperties(PricingStructureType, CurveName);
        }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="pricingStructureType"></param>
        ///<param name="curveName"></param>
        ///<param name="buildDateTime"></param>
        public RateCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : this(pricingStructureType, curveName, buildDateTime, "Default")
        {}

        /// <summary>
        /// An id for a ratecurve.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="baseDate"></param>
        public RateCurveIdentifier(string curveId, DateTime baseDate) 
            : base(curveId)
        {
            BaseDate = baseDate;
            SetProperties(PricingStructureType, CurveName);
        }

        private void SetProperties()
        {
            switch (PricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.RateBasisCurve:
                    ForecastRateIndex = ForecastRateIndexHelper.Parse(Index, IndexTenor);
                    break;
                case PricingStructureTypeEnum.RateSpreadCurve:
                    ForecastRateIndex = ForecastRateIndexHelper.Parse(Index, IndexTenor);
                    ReferenceCurveTypeAndName = PropertyHelper.ExtractReferenceCurveName(Properties);
                    ReferenceCurveUniqueId = PropertyHelper.ExtractReferenceCurveUniqueId(Properties);
                    break;
                case PricingStructureTypeEnum.DiscountCurve:
                    string creditInstrumentId = PropertyHelper.ExtractCreditInstrumentId(Properties);
                    string creditSeniority = PropertyHelper.ExtractCreditSeniority(Properties);
                    CreditInstrumentId = InstrumentIdHelper.Parse(creditInstrumentId);
                    CreditSeniority = CreditSeniorityHelper.Parse(creditSeniority);
                    break;
                case PricingStructureTypeEnum.RateXccyCurve:
                    //ForecastRateIndex = ForecastRateIndexHelper.Parse(Index, IndexTenor);
                    string discountInstrumentId = PropertyHelper.ExtractCreditInstrumentId(Properties);
                    string discountSeniority = PropertyHelper.ExtractCreditSeniority(Properties);
                    CreditInstrumentId = InstrumentIdHelper.Parse(discountInstrumentId);
                    CreditSeniority = CreditSeniorityHelper.Parse(discountSeniority);
                    ReferenceCurveTypeAndName = PropertyHelper.ExtractReferenceCurveName(Properties);
                    ReferenceCurveUniqueId = PropertyHelper.ExtractReferenceCurveUniqueId(Properties);
                    ReferenceFxCurveTypeAndName = PropertyHelper.ExtractReferenceFxCurveName(Properties);
                    ReferenceFxCurveUniqueId = PropertyHelper.ExtractReferenceFxCurveUniqueId(Properties);
                    ReferenceCurrency2CurveTypeAndName = PropertyHelper.ExtractReferenceCurrency2CurveName(Properties);
                    ReferenceCurrency2CurveId = PropertyHelper.ExtractReferenceCurrency2CurveId(Properties);
                    break;
                case PricingStructureTypeEnum.InflationCurve:
                    ForecastRateIndex = ForecastRateIndexHelper.Parse(Index, IndexTenor);
                    var inflationLag = PropertyHelper.ExtractInflationLag(Properties);
                    if (inflationLag != "Unknown")
                    {
                        InflationLag = PeriodHelper.Parse(inflationLag);
                    }
                    break;
                case PricingStructureTypeEnum.XccySpreadCurve:
                    PricingStructureType = PricingStructureTypeEnum.RateSpreadCurve;
                    break;
            }
        }

        private void SetProperties(PricingStructureTypeEnum pricingStructureType, string curveName)
        {
            if (pricingStructureType == PricingStructureTypeEnum.RateCurve ||
                pricingStructureType == PricingStructureTypeEnum.RateBasisCurve ||
                pricingStructureType == PricingStructureTypeEnum.RateSpreadCurve)
            {
                var rateCurveId = curveName.Split('-');
                var indexTenor = rateCurveId[rateCurveId.Length - 1];
                var indexName = rateCurveId[0];
                for (var i = 1; i < rateCurveId.Length - 1; i++)
                {
                    indexName = indexName + '-' + rateCurveId[i];
                }
                ForecastRateIndex = ForecastRateIndexHelper.Parse(indexName, indexTenor);
            }
            if (pricingStructureType == PricingStructureTypeEnum.DiscountCurve ||
                pricingStructureType == PricingStructureTypeEnum.RateXccyCurve)
            {
                var rateCurveId = CurveName.Split('-');
                var subordination = rateCurveId[rateCurveId.Length - 1];
                var indexName = rateCurveId[0];
                for (var i = 1; i < rateCurveId.Length - 1; i++)
                {
                    indexName = indexName + '-' + rateCurveId[i];
                }
                CreditInstrumentId = InstrumentIdHelper.Parse(indexName);
                CreditSeniority = CreditSeniorityHelper.Parse(subordination);
            }
            if (pricingStructureType == PricingStructureTypeEnum.InflationCurve)
            {
                var rateCurveId = CurveName.Split('-');
                var indexTenor = rateCurveId[rateCurveId.Length - 1];
                var indexName = rateCurveId[0];
                for (var i = 1; i < rateCurveId.Length - 1; i++)
                {
                    indexName = indexName + '-' + rateCurveId[i];
                }
                ForecastRateIndex = ForecastRateIndexHelper.Parse(indexName, indexTenor);
            }
        }
    }
}