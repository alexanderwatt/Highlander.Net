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

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Equities;
using Highlander.Equity.Calculator.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Helpers;

namespace Highlander.Reporting.Analytics.V5r3.Equities 
{
    /// <summary>
    /// Stock class which implements the IStock interface
    /// </summary>
    [Serializable]
    public class Stock : IStock, IComparable
    {
        private VolatilitySurface _volatilitySurface;

        private TransactionDetail _transaction;

        /// <summary>
        /// Gets the asset id.
        /// </summary>
        /// <value>The asset id.</value>
        public string AssetId { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Stock"/> is liquid.
        /// </summary>
        /// <value><c>true</c> if liquid; otherwise, <c>false</c>.</value>
        public bool Liquid { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        public IVolatilitySurface VolatilitySurface
        {
            get => _volatilitySurface;
            set => _volatilitySurface = (VolatilitySurface)value;
        }

        /// <summary>
        /// Gets or sets the valuation.
        /// </summary>
        /// <value>The valuation.</value>
        public List<Valuation> Valuations { get; set; }


        /// <summary>
        /// Gets or sets the dividends.
        /// </summary>
        /// <value>The dividends.</value>
        public List<Dividend> Dividends { get; set; }

        /// <summary>
        /// Gets or sets the rate curve.
        /// </summary>
        /// <value>The rate curve.</value>
        public RateCurve RateCurve { get; set; }

        /// <summary>
        /// Gets or sets the spot.
        /// </summary>
        /// <value>The spot.</value>
        public decimal Spot { get; set; }

        /// <summary>
        /// [ERROR: Unknown property access] the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Dollars { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StockName { get; set; }

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
        /// Initializes a new instance of the <see cref="Stock"/> class.
        /// </summary>
        public Stock()
        {
            Dividends = new List<Dividend>();
            Valuations = new List<Valuation>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stock"/> class.
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="assetId">The asset id.</param>
        /// <param name="name">The name.</param>
        /// <param name="today"></param>
        /// <param name="rateCurve"></param>
        /// <param name="divCurve"></param>
        /// <example>
        ///     <code>
        ///     // Creates a BHP stock instance
        ///     IStock stock = new Stock("123", "BHP");
        ///     </code>
        /// </example>
        public Stock(DateTime today, decimal spot, string assetId, string name, RateCurve rateCurve, List<Dividend> divCurve)
        {
            Valuations = new List<Valuation>();
            InputValidator.IsMissingField("AssetId", assetId, true);
            InputValidator.IsMissingField("Name", name, true);
            AssetId = assetId;
            Name = name;
            RateCurve = rateCurve;
            Dividends = divCurve;
            Date = today;
            Spot = spot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockName"></param>
        /// <param name="dollars"></param>
        public Stock(string stockName, double dollars)
        {
            StockName = stockName;
            Dollars = dollars;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stock"/> class.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="name">The name.</param>
        /// <param name="dividends">The dividends.</param>
        /// <param name="wingCurvature">The wing curvature.</param>
        public Stock(string stockId, string name, List<Dividend> dividends, WingCurvature[] wingCurvature)
        {
            ValidateInput(stockId, name, dividends, wingCurvature);
            AssetId = stockId;
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

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <returns></returns>
        public double GetForward(DateTime baseDate, DateTime expiryDate)
        {
            if (baseDate >= expiryDate)
                return Convert.ToDouble(Spot);
            int n = Dividends.Count;
            var times = new double[n];
            var divs = new double[n];
            int maturity = expiryDate.Subtract(baseDate).Days;
            DividendHelper.DividendsToArray(baseDate, Dividends, ref times, ref divs);
            var divCurve = new DiscreteCurve(times, divs);
            double[] rateTimes = RateCurve.GetYearsArray();
            double[] rateAmounts = RateCurve.RateArray;
            double pvDividends = EquityAnalytics.GetPVDivs(baseDate, expiryDate, divCurve, rateTimes, rateAmounts, RateCurve.InterpolationType, RateCurve.RateType);
            decimal df = RateCurve.GetDf(maturity);
            double adjSpot = Convert.ToDouble(Spot) - pvDividends;
            double fwd = 1.0 / Convert.ToDouble(df) * adjSpot;
            return fwd;
        }

        /// <summary>
        /// Calculations the forwards.
        /// </summary>
        public void CalcForwards()
        {
            ForwardExpiry[] fwdExpiry = VolatilitySurface.Expiry;
            foreach (ForwardExpiry fwdExp in fwdExpiry)
            {
                DateTime exp = fwdExp.ExpiryDate;
                DateTime today = Date;
                fwdExp.FwdPrice = Convert.ToDecimal(GetForward(today, exp));
            }
        }

        int IComparable.CompareTo(object obj)
        {
            var c = (Stock)obj;
            if (Dollars < c.Dollars)
                return 1;
            if (Dollars == c.Dollars)
              return 0;
            return -1;
        }
    }
}
