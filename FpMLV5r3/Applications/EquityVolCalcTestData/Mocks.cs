/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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

namespace Highlander.EquitiesCalculator.TestData.V5r3
{
    [Serializable]
    public class Stock
    {
        public string Name { get; set; }
        public string AssetId { get; set; }
        public VolatilitySurface VolatilitySurface { get; set; }
    }

    [Serializable]
    public class VolatilitySurface
    {        
        public string StockId { get; set; }
        public DateTime Date { get; set; }
        public bool IsComplete { get; set; }
        public List<ForwardExpiry> Expiries { get; set; }

        public void AddExpiry(ForwardExpiry expiry, bool nodePoint)
        {
            expiry.NodalPoint = nodePoint;
            if (Expiries == null)
            {
                Expiries = new List<ForwardExpiry>();
            }
            Expiries.Add(expiry);
        }
    }

    [Serializable]
    public class ForwardExpiry
    {        
        public DateTime ExpiryDate { get; set; }
        public decimal FwdPrice { get; set; }
        public decimal InterestRate { get; set; }
        public bool NodalPoint { get; set; }
        public List<double> RawStrikePrices { get; set; }

        public List<Strike> Strikes { get; set; }

        public void AddStrike(Strike strike, bool nodePoint)
        {
            NodalPoint = nodePoint;
            if (Strikes == null)
            {
                Strikes = new List<Strike>();                
            }
            Strikes.Add(strike);
        }
    }

    [Serializable]
    public class Strike
    {
        public OptionPosition Call { get; set; }
        public OptionPosition Put { get; set; }
        public double Moneyness { get; set; }
        public double StrikePrice { get; set; }
        public string PriceUnits { get; set; }
        public VolatilityPoint Volatility { get; set; }
        public bool VolatilityHasBeenSet { get; set; }
        public string StrikeInterpolationModel { get; set; }
        public string TenorInterpolationModel { get; set; }
    }

    [Serializable]
    public class OptionPosition
    {
        public string ContractId { get; set; }
        public double ContractPrice { get; set; }
        public string Type { get; set; }
        public VolatilityPoint Volatility { get; set; }
    }

    [Serializable]
    public class VolatilityPoint
    {
        public decimal Value { get; set; }
        public string State { get; set; }
    }
}
