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

using Highlander.Codes.V5r3;
using Highlander.Utilities.NamedValues;

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    public class TradeHelper
    {
        public static bool IsImplementedProductType(NamedValueSet tradeProps)
        {
            var productType = ProductTypeSimpleScheme.ParseEnumString(tradeProps.GetValue<string>(TradeProp.ProductType));
            return IsImplementedProductType(productType);
        }

        public static bool IsImplementedProductType(ProductTypeSimpleEnum productType)
        {
            var isValid = false;
            switch (productType)
            {
                case ProductTypeSimpleEnum.InterestRateSwap:
                case ProductTypeSimpleEnum.AssetSwap:
                case ProductTypeSimpleEnum.CrossCurrencySwap:
                case ProductTypeSimpleEnum.FRA:
                case ProductTypeSimpleEnum.FxSpot:
                case ProductTypeSimpleEnum.FxForward:
                case ProductTypeSimpleEnum.FxSwap:
                case ProductTypeSimpleEnum.BulletPayment:
                case ProductTypeSimpleEnum.TermDeposit:
                case ProductTypeSimpleEnum.CapFloor:
                case ProductTypeSimpleEnum.InterestRateSwaption:
                case ProductTypeSimpleEnum.FxOption:
                case ProductTypeSimpleEnum.BondTransaction:
                case ProductTypeSimpleEnum.EquityTransaction:
                case ProductTypeSimpleEnum.FutureTransaction:
                case ProductTypeSimpleEnum.PropertyTransaction:
                case ProductTypeSimpleEnum.CommodityForward:
                    isValid = true;
                    break;
            }
            return isValid;
        }
    }
}
