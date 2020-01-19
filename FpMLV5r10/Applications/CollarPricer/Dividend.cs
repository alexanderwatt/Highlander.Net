using System;
using Orion.EquityCollarPricer.Exception;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Represents a Dividend
    /// </summary>
    [Serializable]
    public class Dividend
    {
        /// <summary>
        /// Gets or sets the ex div date.
        /// </summary>
        /// <value>The ex div date.</value>
        public DateTime ExDivDate { get; set; }

        /// <summary>
        /// Gets or sets the pay date.
        /// </summary>
        /// <value>The pay date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        /// <value>The payment amount.</value>
        public double PaymentAmountInCents { get; set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        /// <value>The currency code.</value>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        public Dividend()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDivDate">The ex div date.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentInCents">The payment in cents.</param>
        /// <param name="currencyCode">The currency code.</param>
        public Dividend(DateTime exDivDate, DateTime paymentDate, Double paymentInCents, string currencyCode)
        {
            InputValidator.NotNull("Ex Div Date", exDivDate, true);
            InputValidator.NotNull("Payment Date", paymentDate, true);
            InputValidator.IsMissingField("Currency Code", currencyCode, true);

            if (paymentDate < exDivDate)
            {
                throw new InvalidValueException(string.Format("Payment date {0} is not greater or equal to ExDivDate {1}", paymentDate, exDivDate));
            }

            ExDivDate = exDivDate;
            PaymentDate = paymentDate;
            PaymentAmountInCents = paymentInCents;
            CurrencyCode = currencyCode;
        }
    }
}
