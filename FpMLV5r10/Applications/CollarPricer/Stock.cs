using System.Collections.Generic;
using System.Linq;
using Orion.EquityCollarPricer.Exception;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Represents an Stock
    /// </summary>
    public class Stock
    {
        private TransactionDetail _transaction;

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the dividends.
        /// </summary>
        /// <value>The dividends.</value>
        public DividendList Dividends { get; private set; }

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>The transaction.</value>
        public TransactionDetail Transaction
        {
            get { return _transaction; }
            set 
            {
                TransactionDetail.TransactionComplete(value);
                _transaction = value; 
            }

        }

        /// <summary>
        /// Gets the wing curvature.
        /// </summary>
        /// <value>The wing curvature.</value>
        public WingCurvature[] WingCurvature { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stock"/> class.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="name">The name.</param>
        /// <param name="dividends">The dividends.</param>
        /// <param name="wingCurvature">The wing curvature.</param>
        public Stock(string stockId, string name, DividendList dividends, WingCurvature[] wingCurvature)
        {
            ValidateInput(stockId, name, dividends, wingCurvature);
            Id = stockId;
            Name = name;
            Dividends = dividends;
            WingCurvature = wingCurvature;
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="name">The name.</param>
        /// <param name="dividends">The dividends.</param>
        /// <param name="wingCurvature">The wing curvature.</param>
        static private void ValidateInput(string stockId, string name, List<Dividend> dividends, IEnumerable<WingCurvature> wingCurvature)
        {
            var curvatureList = new List<WingCurvature>(wingCurvature);
            InputValidator.IsMissingField("Stock Id", stockId, true);
            InputValidator.IsMissingField("Name", name, true);
            InputValidator.ListNotEmpty("Dividends", dividends, true);
            InputValidator.ListNotEmpty("Wing Curvature", curvatureList, true);
            if (curvatureList.Any(curvature => !curvature.IsComplete))
            {
                throw new InvalidValueException("Wing Curvature is not complete");
            }
        }
    }
}
