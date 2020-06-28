using System;

namespace nab.QDS.FpML.V47
{
    public partial class Trade
    {
        public ItemChoiceType16 GetTradeTypeFromItem()
        {
            if (Item == null)
                throw new ArgumentNullException("Item");
            Type type = Item.GetType();
            if (type == typeof(BondOption)) return ItemChoiceType16.bondOption;
            if (type == typeof(Swap)) return ItemChoiceType16.swap;
            if (type == typeof(Fra)) return ItemChoiceType16.fra;
            if (type == typeof(FxLeg)) return ItemChoiceType16.fxSingleLeg;
            if (type == typeof(FxSwap)) return ItemChoiceType16.fxSwap;
            if (type == typeof(FxOptionLeg)) return ItemChoiceType16.fxSimpleOption;
            if (type == typeof(Swaption)) return ItemChoiceType16.swaption;
            if (type == typeof(CapFloor)) return ItemChoiceType16.capFloor;
            if (type == typeof(TermDeposit)) return ItemChoiceType16.termDeposit;
            if (type == typeof(BulletPayment)) return ItemChoiceType16.bulletPayment;
            //if (type == typeof(xxx)) return ItemChoiceType16.xxx;
            throw new ArgumentException("Unknown product type: " + type.Name);
        }
    }
}
