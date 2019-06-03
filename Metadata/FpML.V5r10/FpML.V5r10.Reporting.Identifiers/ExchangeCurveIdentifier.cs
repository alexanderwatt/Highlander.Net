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
    public class ExchangeCurveIdentifier : PricingStructureIdentifier
    {
        ///<summary>
        /// Used for inflation curves.
        ///</summary>
        public String Exchange { get; set; }

        ///<summary>
        /// The reference curve type and name e.g RateCurve.AUD-BBR-BBSW-1M
        ///</summary>
        public String Code { get; set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public ExchangeCurveIdentifier(NamedValueSet properties)
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
        public ExchangeCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime, string algorithm)
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
        public ExchangeCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : this(pricingStructureType, curveName, buildDateTime, "Default")
        {}

        /// <summary>
        /// An id for a ratecurve.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="baseDate"></param>
        public ExchangeCurveIdentifier(string curveId, DateTime baseDate) 
            : base(curveId)
        {
            BaseDate = baseDate;
            SetProperties(PricingStructureType, CurveName);
        }

        private void SetProperties()
        {
            if (Properties != null)
            {
                Exchange = Properties.GetString("Exchange", true);
                Code = Properties.GetString("ContractCode", true);
            }
        }

        private void SetProperties(PricingStructureTypeEnum pricingStructureType, string curveName)
        {
            if (pricingStructureType == PricingStructureTypeEnum.ExchangeTradedCurve)
            {
                var curveId = curveName.Split('-');
                if (curveId.Length > 2)
                {
                    Currency = CurrencyHelper.Parse(curveId[0]);
                    Exchange = curveId[1];
                    Code = curveId[2];
                }
            }
        }
    }
}