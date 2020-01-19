using System;
using System.Collections.Generic;

namespace Orion.EquitiesVolCalc.TestData
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

        public void AddExpiry(ForwardExpiry expiry, bool nodelPoint)
        {
            expiry.NodalPoint = nodelPoint;
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

        public void AddStrike(Strike strike, bool nodelPoint)
        {
            NodalPoint = nodelPoint;
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
        public string StrikeInterpModel { get; set; }
        public string TenorInterpModel { get; set; }
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
