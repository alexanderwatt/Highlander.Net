/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using FpML.V5r10.EquityVolatilityCalculator.Helpers;
using FpML.V5r10.Reporting.Analytics.Equities;
using FpML.V5r10.Reporting.Analytics.Interpolations.Spaces;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    /// <summary>
    /// Stock class which implements the IStock interface
    /// </summary>
    [Serializable]
    public class Stock : IStock
    {
        private VolatilitySurface _volatilitySurface;

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
        /// Gets the forward.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <returns></returns>
        public double GetForward(DateTime baseDate, DateTime expiryDate )
        {
            if (baseDate >= expiryDate)
                return Convert.ToDouble(Spot);
            int n = Dividends.Count;
            var times = new double[n];
            var divs = new double[n];
            int maturity = expiryDate.Subtract(baseDate).Days;
            DividendHelper.DividendsToArray(baseDate,Dividends,ref times,ref divs);
            var divCurve =new DiscreteCurve(times,divs);            
            double[] ratetimes = RateCurve.GetYearsArray();
            double[] rateamts = RateCurve.RateArray;          
            double pvdivs = EquityAnalytics.GetPVDivs(baseDate, expiryDate, divCurve, ratetimes, rateamts, RateCurve.InterpolationType, RateCurve.RateType);                                              
            decimal df = RateCurve.GetDf(maturity);
            double adjSpot = Convert.ToDouble(Spot) - pvdivs;        
            double fwd = 1.0/Convert.ToDouble(df)*adjSpot;
            return fwd;
        }

        /// <summary>
        /// Calcs the forwards.
        /// </summary>
        public void CalcForwards()
        {
            ForwardExpiry[] fwdExps = VolatilitySurface.Expiries;            
            foreach (ForwardExpiry fwdExp in fwdExps)
            {                              
                DateTime exp = fwdExp.ExpiryDate;
                DateTime today = Date;
                fwdExp.FwdPrice = Convert.ToDecimal(GetForward(today, exp));                
            }
        }   
    }
}
