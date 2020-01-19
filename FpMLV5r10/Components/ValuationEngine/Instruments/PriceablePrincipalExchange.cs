#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Helpers;
using Orion.Analytics.Helpers;
using FpML.V5r10.Reporting;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    public enum PrincipalExchangeTypes { Initial, Final, Intermediate };

    [Serializable]
    public class PriceablePrincipalExchange : PriceableCashflow, IPriceableInstrumentController<PrincipalExchange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceablePrincipalExchange"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="payerIsBaseParty">A flag determining if the sign on the mamount is relative to the base party.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="adjustedPaymentDate">The adjusted payment date.</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceablePrincipalExchange
            (
            string id
            , bool payerIsBaseParty
            , Decimal amount
            , string currency
            , DateTime adjustedPaymentDate
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBaseParty, MoneyHelper.GetAmount(amount, currency), DateTypesHelper.ToAdjustableOrAdjustedDate(adjustedPaymentDate),
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()), false, paymentCalendar)
        {}

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public PrincipalExchange Build()
        {
            var money = MoneyHelper.Mul(PaymentAmount, PayerIsBaseParty);
            var px = PrincipalExchangeHelper.Create(PaymentDate, money.amount);
            px.id = Id;
            //px.
            px.adjustedPrincipalExchangeDate = PaymentDate;
            px.adjustedPrincipalExchangeDateSpecified = true;         
            if (CalculationPerfomedIndicator)
            {
                px.discountFactor = PaymentDiscountFactor;
                var npv = System.Math.Abs(ForecastAmount.amount) * PaymentDiscountFactor;
                px.discountFactorSpecified = true;
                px.presentValuePrincipalExchangeAmount = MoneyHelper.Mul(npv, PaymentAmount.currency.Value, PayerIsBaseParty);
            }
            return px;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        #endregion
    }
}
