#region Using directives

using System;
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The CommodityCurveIdentifier.
    /// </summary>
    public class CommodityCurveIdentifier : PricingStructureIdentifier
    {

        /// <summary>
        /// The CommodityAsset.
        /// </summary>
        public string CommodityAsset { get; private set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public CommodityCurveIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        /// <summary>
        /// The CommodityCurveIdentifier.
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="curveName"></param>
        /// <param name="buildDateTime"></param>
        public CommodityCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : base(pricingStructureType, curveName, buildDateTime)
        {
            CommodityAsset = CurveName.Split('-')[1];
        }

        /// <summary>
        /// The CommodityCurveIdentifier.
        /// </summary>
        /// <param name="curveId"></param>
        public CommodityCurveIdentifier(string curveId)
            : base(curveId)
        {
            var comcurveId = CurveName.Split('-');
            if (comcurveId.Length != 2)
            {
            }
            else
            {
                CommodityAsset = CurveName.Split('-')[1];
            }
        }

        private void SetProperties()
        {
            CommodityAsset = PropertyHelper.ExtractCommodityAsset(Properties);
        }
    }
}