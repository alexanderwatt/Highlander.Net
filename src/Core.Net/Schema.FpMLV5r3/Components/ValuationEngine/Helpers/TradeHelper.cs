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

using System;
using Highlander.Codes.V5r3;
using Highlander.Reporting.V5r3;
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

        public static bool IsImplementedTradeType(ItemChoiceType15 tradeType)
        {
            switch (tradeType)
            {
                case ItemChoiceType15.leaseTransaction:
                case ItemChoiceType15.swap:
                case ItemChoiceType15.fra:
                case ItemChoiceType15.fxSwap:
                case ItemChoiceType15.bulletPayment:
                case ItemChoiceType15.termDeposit:
                case ItemChoiceType15.capFloor:
                case ItemChoiceType15.swaption:
                case ItemChoiceType15.fxOption:
                case ItemChoiceType15.bondTransaction:
                case ItemChoiceType15.equityTransaction:
                case ItemChoiceType15.futureTransaction:
                case ItemChoiceType15.propertyTransaction:
                case ItemChoiceType15.commodityForward:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsImplementedProductType(ProductTypeSimpleEnum productType)
        {
            var isValid = false;
            switch (productType)
            {
                case ProductTypeSimpleEnum.LeaseTransaction:
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
                case ProductTypeSimpleEnum.Undefined:
                    break;
                case ProductTypeSimpleEnum.InflationSwap:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultSwap:
                    break;
                case ProductTypeSimpleEnum.TotalReturnSwap:
                    break;
                case ProductTypeSimpleEnum.VarianceSwap:
                    break;
                case ProductTypeSimpleEnum.EquityOption:
                    break;
                case ProductTypeSimpleEnum.BondOption:
                    break;
                case ProductTypeSimpleEnum.FxOptionStrategy:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultIndex:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultIndexTranche:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultBasket:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultBasketTranche:
                    break;
                case ProductTypeSimpleEnum.CreditDefaultOption:
                    break;
                case ProductTypeSimpleEnum.EquityForward:
                    break;
                case ProductTypeSimpleEnum.DividendSwap:
                    break;
                case ProductTypeSimpleEnum.ConvertibleBondOption:
                    break;
                case ProductTypeSimpleEnum._LAST_:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
            }
            return isValid;
        }
    }
}
