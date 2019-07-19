/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using FpML.V5r10.Codes;

namespace FpML.V5r10.Reporting.Helpers
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
                commodity.Item = ProductTypeHelper.ExchangeIdHelper.Parse(exchangeId);
            }
            else
            {
                commodity.Item = InformationSourceHelper.Create(informationSource);
            }
            return commodity;
        }
    }
}
