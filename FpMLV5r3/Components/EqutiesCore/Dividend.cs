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

#region Usings

using System;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.Equities
{
    /// <summary>
    /// Cents or index points
    /// </summary>
    public enum Units
    {
        /// <summary>
        /// 
        /// </summary>
        Cents = 0,
        /// <summary>
        /// 
        /// </summary>
        Dollars = 1,
        /// <summary>
        /// 
        /// </summary>
        Points = 2
    }

    /// <summary>
    /// Represents a Dividend
    /// </summary>
    [Serializable]
    public class Dividend
    {
        /// <summary>
        /// Gets or sets the ex date.
        /// </summary>
        /// <value>The ex date.</value>
        public DateTime ExDate { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal Amount { get; set; }


        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }

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
            PriceUnits = Units.Cents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDate">The ex date.</param>
        /// <param name="amt">The amt.</param>
        /// <param name="units"></param>
        public Dividend(DateTime exDate, decimal amt, Units units)
        {
            ExDate = exDate;
            Amount = amt;
            PriceUnits = units;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDate">The ex date.</param>
        /// <param name="amt">The amt.</param>
        public Dividend(DateTime exDate, decimal amt)
        {
            PriceUnits = Units.Cents;
            ExDate = exDate;
            Amount = amt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDivDate">The ex div date.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentInCents">The payment in cents.</param>
        /// <param name="currencyCode">The currency code.</param>
        public Dividend(DateTime exDivDate, DateTime paymentDate, double paymentInCents, string currencyCode)
        {
            InputValidator.NotNull("Ex Div Date", exDivDate, true);
            InputValidator.NotNull("Payment Date", paymentDate, true);
            InputValidator.IsMissingField("Currency Code", currencyCode, true);
            if (paymentDate < exDivDate)
            {
                throw new InvalidValueException(
                    $"Payment date {paymentDate} is not greater or equal to ExDivDate {exDivDate}");
            }
            ExDivDate = exDivDate;
            PaymentDate = paymentDate;
            PaymentAmountInCents = paymentInCents;
            CurrencyCode = currencyCode;
        }
    }
}
