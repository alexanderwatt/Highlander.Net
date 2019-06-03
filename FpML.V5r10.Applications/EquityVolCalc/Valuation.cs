using System;

namespace Orion.Equity.VolatilityCalculator
{
    [Serializable]
    public class Valuation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        public Valuation()
        {
            PriceUnits = Units.Cents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="price">The price.</param>
        /// <param name="units">The units.</param>
        public Valuation(DateTime date, decimal price, Units units)
        {
            Date = date;
            Price = price;
            PriceUnits = units;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="price">The price.</param>
        public Valuation(DateTime date, decimal price)
        {
            PriceUnits = Units.Cents;
            Date = date;
            Price = price;            
        }

        /// <summary>
        /// Gets or sets a value indicating whether [ex date].
        /// </summary>
        /// <value><c>true</c> if [ex date]; otherwise, <c>false</c>.</value>
        public bool ExDate { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>The price.</value>
        public decimal Price { get; set; }


        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }
    }

}
