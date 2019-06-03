using System;

namespace Orion.Equity.VolatilityCalculator
{
    [Serializable]
    public class Dividend
    {
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
    }


}
