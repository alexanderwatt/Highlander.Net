using System;
using System.Collections.Generic;
using Orion.EquityCollarPricer.Exception;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Represents a list of dividends
    /// </summary>
    public class DividendList: List<Dividend>
    {
        /// <summary>
        /// Gets the dividends.
        /// </summary>
        /// <value>The dividends.</value>
        public Dividend[] Dividends
        {
            get
            {
                var dividends = new Dividend[Count];
                if (Count > 0)
                {
                    CopyTo(dividends, 0);
                }
                return dividends;
            }
        }

        /// <summary>
        /// Gets the ex div dates.
        /// </summary>
        /// <value>The ex div dates.</value>
        public DateTime[] ExDivDates
        {
            get 
            {
                return DividendHelper.GetDividendsProperty<DateTime>(this, "ExDivDate");
            }
        }

        /// <summary>
        /// Gets the payment dates.
        /// </summary>
        /// <value>The payment dates.</value>
        public DateTime[] PaymentDates
        {
            get
            {
                return DividendHelper.GetDividendsProperty<DateTime>(this, "PaymentDate");
            }
        }

        /// <summary>
        /// Gets the payment amounts in cents.
        /// </summary>
        /// <value>The payment amounts in cents.</value>
        public Double[] PaymentAmountsInCents
        {
            get
            {
                return DividendHelper.GetDividendsProperty<Double>(this, "PaymentAmountInCents");
            }
        }

        /// <summary>
        /// Adds the specified dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        public new void Add(Dividend dividend)
        {
            AddDividend(dividend);
        }

        /// <summary>
        /// Removes the specified dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        public new void Remove(Dividend dividend)
        {
            RemoveDividend(dividend);
        }

        /// <summary>
        /// Adds the dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        private void AddDividend(Dividend dividend)
        {
            Dividend match = DividendHelper.FindDividend(dividend, this);

            if (match == null)
            {
                if (Count == 0)
                {
                    base.Add(dividend);
                }
                else
                {
                    List<Dividend> divsFound = FindAll(
                        dividendItem => (dividendItem.ExDivDate < dividend.ExDivDate)
                        );

                    Insert(divsFound.Count != 0 ? divsFound.Count : 0, dividend);
                }
            }
            else
            {
                throw new DuplicateNotAllowedException(string.Format("A dividend with ExDiv date {0},  already exists in this list", dividend.ExDivDate));
            }
        }

        private void RemoveDividend(Dividend dividend)
        {
            Dividend match = DividendHelper.FindDividend(dividend, this);
            if (match != null)
            {
                base.Remove(match);
            }
        }
 
    }
}
