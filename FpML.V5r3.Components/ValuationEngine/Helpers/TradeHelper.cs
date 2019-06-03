using FpML.V5r3.Codes;
using Orion.Util.NamedValues;

namespace Orion.ValuationEngine.Helpers
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
