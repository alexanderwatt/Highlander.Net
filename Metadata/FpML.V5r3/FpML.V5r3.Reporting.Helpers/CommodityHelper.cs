using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;

namespace FpML.V5r3.Reporting
{
    public class CommodityHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commodityType"></param>
        /// <param name="commodityDetails"></param>
        /// <param name="currency"></param>
        /// <param name="multiplier"></param>
        /// <param name="specifiedPrice"></param>
        /// <param name="deliveryDateRollConvention"></param>
        /// <param name="priceQuoteUnits"></param>
        /// <param name="deliveryDates"></param>
        /// <param name="exchangeId"></param>
        /// <param name="informationSource"></param>
        /// <returns></returns>
        public static Commodity Create(string commodityType, string commodityDetails, string currency, int multiplier, SpecifiedPriceEnum specifiedPrice,
            Offset deliveryDateRollConvention, PriceQuoteUnitsEnum priceQuoteUnits, DeliveryDatesEnum deliveryDates, string exchangeId, string informationSource)
        {
            var commodity = new Commodity
                {
                    currency = new Currency {Value = currency},
                    commodityBase = new CommodityBase {Value = commodityType},
                    commodityDetails = new CommodityDetails {Value = commodityDetails},
                    deliveryDateRollConvention = deliveryDateRollConvention,
                    Item1 = deliveryDates,
                    multiplier = multiplier,
                    multiplierSpecified = true,
                    specifiedPriceSpecified = true,
                    unit = new QuantityUnit(),
                    specifiedPrice = specifiedPrice
                };
            commodity.unit.Value = priceQuoteUnits.ToString();
            if (exchangeId != null)
            {
                commodity.Item = ExchangeIdHelper.Parse(exchangeId);
            }
            else
            {
                commodity.Item = InformationSourceHelper.Create(informationSource);
            }
            return commodity;
        }
    }
}
