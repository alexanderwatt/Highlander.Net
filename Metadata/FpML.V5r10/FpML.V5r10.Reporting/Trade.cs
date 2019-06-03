using System;

namespace FpML.V5r10.Reporting
{
    public partial class Trade
    {
        public ItemChoiceType15 GetTradeTypeFromItem()
        {
            if (Item == null)
                throw new ArgumentNullException("Item");
            Type type = Item.GetType();
            if (type == typeof(BondOption)) return ItemChoiceType15.bondOption;
            if (type == typeof(Swap)) return ItemChoiceType15.swap;
            if (type == typeof(Fra)) return ItemChoiceType15.fra;
            if (type == typeof(FxSingleLeg)) return ItemChoiceType15.fxSingleLeg;
            if (type == typeof(FxSwap)) return ItemChoiceType15.fxSwap;
            if (type == typeof(FxOption)) return ItemChoiceType15.fxOption;
            if (type == typeof(Swaption)) return ItemChoiceType15.swaption;
            if (type == typeof(CapFloor)) return ItemChoiceType15.capFloor;
            if (type == typeof(TermDeposit)) return ItemChoiceType15.termDeposit;
            if (type == typeof(BulletPayment)) return ItemChoiceType15.bulletPayment;
            if (type == typeof(BondTransaction)) return ItemChoiceType15.bondTransaction;
            if (type == typeof(EquityTransaction)) return ItemChoiceType15.equityTransaction;
            if (type == typeof(FutureTransaction)) return ItemChoiceType15.futureTransaction;
            //if (type == typeof(xxx)) return ItemChoiceType16.xxx;
            throw new ArgumentException("Unknown product type: " + type.Name);
        }
    }
}
