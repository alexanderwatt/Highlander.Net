#region Usings

using FpML.V5r10.Reporting;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class FpMLFieldResolver
    {
        public static void TradeSetFxSwap(Trade trade, FxSwap swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.fxSwap;
        }

        public static void TradeSetFxLeg(Trade trade, FxSingleLeg fxLeg)
        {
            trade.Item = fxLeg;
            trade.ItemElementName = ItemChoiceType15.fxSingleLeg;
        }

        public static void TradeSetFxOptionLeg(Trade trade, FxOption fxLeg)
        {
            trade.Item = fxLeg;
            trade.ItemElementName = ItemChoiceType15.fxOption;
        }

        public static void TradeSetBulletPayment(Trade trade, BulletPayment payment)
        {
            trade.Item = payment;
            trade.ItemElementName = ItemChoiceType15.bulletPayment;
        }

        public static void TradeSetTermDeposit(Trade trade, TermDeposit deposit)
        {
            trade.Item = deposit;
            trade.ItemElementName = ItemChoiceType15.termDeposit;
        }

        public static void TradeSetCapFloor(Trade trade, CapFloor capFloor)
        {
            trade.Item = capFloor;
            trade.ItemElementName = ItemChoiceType15.capFloor;
        }

        //public static void TradeSetFxSimpleOptionLeg(Trade trade, FxOption fxOptionLeg)
        //{
        //    trade.Item = fxOptionLeg;
        //    trade.ItemElementName = ItemChoiceType15.fxOption;
        //}
    }
}
