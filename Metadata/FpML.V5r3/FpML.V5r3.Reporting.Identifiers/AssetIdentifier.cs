using System;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Constants;
using Orion.ModelFramework.Identifiers;

namespace Orion.Identifiers
{
    /// <summary>
    /// The Identifier.
    /// </summary>
    public class AssetIdentifier : Identifier, IAssetIdentifier
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? MaturityDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AssetTypesEnum AssetType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? Coupon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? MarketQuote { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? Other { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? Strike { get; set; }

        /// <summary>
        ///  An id for a trade.
        /// </summary>
        /// <param name="other">THe spread. May be zero.</param>
        /// <param name="properties">The properties. These need to include:
        ///  SourceSystem, Id and Trade date.</param>
        /// <param name="marketQuote">The market quote. This could be a rate or a volatility.</param>
        public AssetIdentifier(Decimal? marketQuote, Decimal? other, NamedValueSet properties)
            : base(properties)
        {
            MarketQuote = marketQuote;
            Other = other;
            Strike = PropertyHelper.ExtractDecimalProperty("Strike", properties) ?? other;
            SetProperties(properties);
        }

        private void SetProperties(NamedValueSet properties)
        {
            try
            {
                var baseDate = PropertyHelper.ExtractDateTimeProperty(CurveProp.BaseDate, properties);
                if (baseDate != null)
                {
                    BaseDate = (DateTime)baseDate;
                }
                Id = PropertyHelper.ExtractStringProperty("AssetId", properties);
                //AssetType property - make sure it exists.
                var assetType = PropertyHelper.ExtractStringProperty("AssetType", properties);
                if (assetType != null)
                {
                    AssetType = EnumHelper.Parse<AssetTypesEnum>(assetType);
                }
                Coupon = PropertyHelper.ExtractDecimalProperty("Coupon", properties);
                MaturityDate = PropertyHelper.ExtractDateTimeProperty("Maturity", properties);  
                UniqueIdentifier = properties.GetValue<string>(CurveProp.UniqueIdentifier) ?? BuildUniqueId();
                PropertyHelper.Update(Properties, CurveProp.UniqueIdentifier, UniqueIdentifier);
            }
            catch
            {
                throw new Exception("Invalid assetid.");
            }
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string BuildUniqueId()
        {
            switch (AssetType)
            {
                case AssetTypesEnum.Bond:
                    if (MaturityDate != null)
                        return $"{Id}.{Coupon}.{((DateTime) MaturityDate).ToShortDateString()}";
                    break;
                case AssetTypesEnum.IRFutureOption:
                case AssetTypesEnum.IRPutFutureOption:
                case AssetTypesEnum.IRCallFutureOption:
                case AssetTypesEnum.IRCap:
                case AssetTypesEnum.IRFloor:
                case AssetTypesEnum.Caplet:
                case AssetTypesEnum.Floorlet:
                    if (Strike != null)
                        return $"{Id}-{Strike}-{BaseDate.ToShortDateString()}";
                    break;
                case AssetTypesEnum.CommoditySpot:
                case AssetTypesEnum.CommodityForward:
                case AssetTypesEnum.CommodityFuture: return $"{Id}.{BaseDate.ToShortDateString()}";
                default:
                    if (MarketQuote != null)
                        return $"{Id}-{MarketQuote}-{BaseDate.ToShortDateString()}";
                    break;
            }
            return Id;
        }
    }
}