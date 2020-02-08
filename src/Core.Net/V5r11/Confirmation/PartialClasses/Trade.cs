using System;
using FpML.V5r3.Helpers;

namespace FpML.V5r3.Confirmation
{
    public partial class Trade
    {
        public TradeTypeEnum GetTradeTypeFromItem()
        {
            if (Item == null)
                throw new ArgumentNullException("Item");
            Type type = Item.GetType();
            if (type == typeof(BondOption)) return TradeTypeEnum.bondOption;
            if (type == typeof(Swap)) return TradeTypeEnum.swap;
            if (type == typeof(Fra)) return TradeTypeEnum.fra;
            if (type == typeof(FxSingleLeg)) return TradeTypeEnum.fxSingleLeg;
            if (type == typeof(FxSwap)) return TradeTypeEnum.fxSwap;
            if (type == typeof(FxOption)) return TradeTypeEnum.fxOption;
            if (type == typeof(Swaption)) return TradeTypeEnum.swaption;
            if (type == typeof(CapFloor)) return TradeTypeEnum.capFloor;
            if (type == typeof(TermDeposit)) return TradeTypeEnum.termDeposit;
            if (type == typeof(BulletPayment)) return TradeTypeEnum.bulletPayment;
            //if (type == typeof(xxx)) return ItemChoiceType16.xxx;
            throw new ArgumentException("Unknown product type: " + type.Name);
        }
    }
}
