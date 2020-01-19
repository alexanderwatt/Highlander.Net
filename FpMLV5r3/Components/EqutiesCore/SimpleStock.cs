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

using System.Collections.Generic;
using System.Linq;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.Equities
{
    /// <summary>
    /// Represents an Stock
    /// </summary>
    public class SimpleStock
    {
        private TransactionDetail _transaction;

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the dividends.
        /// </summary>
        /// <value>The dividends.</value>
        public DividendList Dividends { get; }

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>The transaction.</value>
        public TransactionDetail Transaction
        {
            get => _transaction;
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
        public WingCurvature[] WingCurvature { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStock"/> class.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="name">The name.</param>
        /// <param name="dividends">The dividends.</param>
        /// <param name="wingCurvature">The wing curvature.</param>
        public SimpleStock(string stockId, string name, DividendList dividends, WingCurvature[] wingCurvature)
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
        private static void ValidateInput(string stockId, string name, List<Dividend> dividends, IEnumerable<WingCurvature> wingCurvature)
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
