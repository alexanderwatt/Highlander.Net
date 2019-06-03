using System;
using nab.QR.Xml;
using nab.QR.PricingModel;

namespace nab.QR.PricingModelNab1
{
  internal class OptionProduct
  {
    public OptionCode Code;
    public PositionType PositionType;
    public string LeftAsset;
    public string RightAsset;
    public string FixedAsset;
    public double LeftAmount;
    public double RightAmount;
    public double Rate;

    public OptionType? OptionType;
    public DateTime ExpiryDate;
    public DateTime SettlementDate;
    public bool IsDigital;

    public double BarrierLowerLevel;
    public double BarrierUpperLevel;
    public TriggerType? BarrierType;
    public DateTime BarrierStartDate;
    public DateTime BarrierEndDate;

    public string PremiumMarginAsset;
    public double PremiumMarginPoints;

    public DateTime HorizonDate;
    public DateTime SpotDate;

    public Spread Spot;
    public Spread Forward;
    public Spread Volatility;
    public Spread LeftDepo;
    public Spread RightDepo;
    public Spread LeftCC;
    public Spread RightCC;
    public Spread RightDF;
    public Spread LeftDF;

    public Spread MarketSpot;
    public Spread MarketForward;
    public Spread MarketVolatility;
    public Spread MarketLeftDepo;
    public Spread MarketRightDepo;
    public bool SpreadPricing;

    public void Init(Product product, Market market, PricingParams parameters)
    {
      Product newProduct = DynamicPeelOff(product, market);
      InitParams(this, parameters);
      InitProductValues(this, newProduct, market);
      InitMarketRates(this, newProduct, market);
      Code = MapOptionCode(this);
    }

    public void Publish(PricingResult result)
    {
      PositionType position = SpreadPricing ? PositionType : PositionType.None;

      result.Add(new PricingValue { Asset = "", Value = new PropertyValue(LeftAsset), Name = "Asset", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = "", Value = new PropertyValue(RightAsset), Name = "Numeraire", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Value = new PropertyValue(LeftAmount), Name = "Amount", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = RightAsset, Value = new PropertyValue(RightAmount), Name = "Amount", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Value = new PropertyValue(Rate), Name = "Rate", Unit = ValueUnit.Absolute });

      result.Add(new PricingValue { Asset = LeftAsset, Name = "Spot", Value = new PropertyValue(Spot[position]), Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Name = "Forward", Value = new PropertyValue(Forward[position]), Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Name = "Depo", Value = new PropertyValue(LeftDepo[position.Opposite()]), Unit = ValueUnit.Percent });
      result.Add(new PricingValue { Asset = RightAsset, Name = "Depo", Value = new PropertyValue(RightDepo[position]), Unit = ValueUnit.Percent });
      if (Volatility != null)
      {
        result.Add(new PricingValue { Asset = LeftAsset, Name = "Volatility", Value = new PropertyValue(Volatility[position]), Unit = ValueUnit.Percent });
      }

      result.Add(new PricingValue { Asset = LeftAsset, Name = "MarketSpot", Value = new PropertyValue(MarketSpot[position]), Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Name = "MarketForward", Value = new PropertyValue(MarketForward[position]), Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Name = "MarketDepo", Value = new PropertyValue(MarketLeftDepo[position.Opposite()]), Unit = ValueUnit.Percent });
      result.Add(new PricingValue { Asset = RightAsset, Name = "MarketDepo", Value = new PropertyValue(MarketRightDepo[position]), Unit = ValueUnit.Percent });

      if (MarketVolatility != null)
      {
        result.Add(new PricingValue { Asset = LeftAsset, Name = "MarketVolatility", Value = new PropertyValue(MarketVolatility[position]), Unit = ValueUnit.Percent });
      }

      result.Add(new PricingValue { Asset = LeftAsset, Name = "DF", Value = new PropertyValue(LeftDF[position.Opposite()]), Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = RightAsset, Name = "DF", Value = new PropertyValue(RightDF[position]), Unit = ValueUnit.Absolute });

    }

    private static void InitParams(OptionProduct result, PricingParams parameters)
    {
      result.SpreadPricing = parameters.Values.GetValue<bool>("SpreadPricing");
    }

    private static void InitProductValues(OptionProduct result, Product product, Market market)
    {
      // position
      result.PositionType = product.Position.PositionType;

      // asset (in terms how the market is quoted)
      ExchangeRate rate = null;
      if (product.Option != null && product.Option.Strike != null)
        rate = product.Option.Strike;
      else if (product.Triggers != null)
        rate = product.Triggers[0].LowerLevel != null ? product.Triggers[0].LowerLevel : product.Triggers[0].UpperLevel;
      else
        rate = product.Exchange.Rate;

      Xml.Curve fwd = market["FXFWD", rate.LeftAsset, rate.RightAsset];
      result.LeftAsset = fwd.LeftAsset;
      result.RightAsset = fwd.RightAsset;

      // amount
      if (product.Exchange != null)
      {
        result.IsDigital = false;
        result.FixedAsset = product.Exchange.GetFixedAsset();
        if (product.Exchange.Rate.LeftAsset == result.LeftAsset)
        {
          result.LeftAmount = product.Exchange.LeftAmount;
          result.RightAmount = product.Exchange.RightAmount;
        }
        else
        {
          result.LeftAmount = product.Exchange.RightAmount;
          result.RightAmount = product.Exchange.LeftAmount;
        }
        result.Rate = product.Exchange.Rate.Rate;
      }
      else
      {
        result.IsDigital = true;
        result.FixedAsset = product.Cash.Asset;
        if (product.Cash.Asset == result.LeftAsset)
        {
          result.LeftAmount = product.Cash.Amount;
          result.RightAmount = 0;
        }
        else
        {
          result.LeftAmount = 0;
          result.RightAmount = product.Cash.Amount;
        }
      }

      // option
      result.OptionType = null;
      if (product.Option != null)
      {
        if (product.Option.Type != Xml.OptionType.None)
        {
          result.OptionType = product.Option.Type;
          result.Rate = ExchangeRate.Flip(product.Option.Strike.Rate, product.Option.Strike.LeftAsset, result.LeftAsset);
        }
        result.ExpiryDate = product.Option.ExpiryDate;
      }

      // settlement
      result.SettlementDate = product.Settlement.SettlementDate;


      // barrier
      if (product.Triggers == null)
      {
        result.BarrierType = null;
        result.BarrierLowerLevel = 0;
        result.BarrierUpperLevel = 0;
      }
      else
      {
        result.BarrierStartDate = product.Triggers[0].StartDate;
        result.BarrierEndDate = product.Triggers[0].EndDate;
        result.BarrierType = product.Triggers[0].Type;

        bool flip = false;
        if (product.Triggers[0].LowerLevel != null)
        {
          result.BarrierLowerLevel = product.Triggers[0].LowerLevel.Rate;
          flip = result.LeftAsset != product.Triggers[0].LowerLevel.LeftAsset;
        }
        else
        {
          result.BarrierLowerLevel = 0;
        }

        if (product.Triggers[0].UpperLevel != null)
        {
          result.BarrierUpperLevel = product.Triggers[0].UpperLevel.Rate;
          flip = result.LeftAsset != product.Triggers[0].UpperLevel.LeftAsset;
        }
        else
        {
          result.BarrierUpperLevel = 0;
        }

        if (flip)
        {
          double lower = result.BarrierLowerLevel == 0 ? 0 : 1.0 / result.BarrierLowerLevel;
          double upper = result.BarrierUpperLevel == 0 ? 0 : 1.0 / result.BarrierUpperLevel;
          result.BarrierLowerLevel = upper;
          result.BarrierUpperLevel = lower;
        }
      }

      result.PremiumMarginAsset = null;
      result.PremiumMarginPoints = 0;
      if (product.Margin != null)
      {
        result.PremiumMarginAsset = product.Margin.Asset;
        result.PremiumMarginPoints = product.Margin.Amount;
      }
    }

    private static Product DynamicPeelOff(Product product, Market market)
    {
      return product;
    }

    private static void InitMarketRates(OptionProduct result, Product product, Market market)
    {
      Xml.Curve fwd = market["FXFWD", result.LeftAsset, result.RightAsset];
      result.HorizonDate = fwd.CurveDate;

      Xml.Rate spotRate = fwd.Rates.Find("SPOT");
      result.SpotDate = spotRate.Date;

      result.MarketSpot = new Spread { Mid = spotRate.Spread.Mid }.Round(6);  //spotRate.Spread.Round(6);

      Xml.Curve leftDF = market["FXDF", result.LeftAsset, result.LeftAsset];
      result.MarketLeftDepo = CurveInterpolationDF.GetFwdZero(leftDF, result.SpotDate, result.SettlementDate).Round(6);

      Xml.Curve rightDF = market["FXDF", result.RightAsset, result.RightAsset];
      result.MarketRightDepo = CurveInterpolationDF.GetFwdZero(rightDF, result.SpotDate, result.SettlementDate).Round(6);

      int leftDayCount = CurveInterpolationDF.GetDayCount(leftDF);
      int rightDayCount = CurveInterpolationDF.GetDayCount(rightDF);
      int days = (result.SettlementDate - result.SpotDate).Days;
      result.MarketForward = CurveFunctionFwd.CalculateFwd(result.MarketSpot, result.MarketLeftDepo, result.MarketRightDepo,
                                                           days, leftDayCount, rightDayCount).Round(6);

      bool isOption = result.OptionType != null || result.BarrierType != null;
      if (isOption)
        CalcVolatility(result, product, market, spotRate);

      if (product.Parameters.IsMember("Spot"))
        result.Spot = Spread.FromCSV(product.Parameters.GetValue<string>("Spot"));
      else
        result.Spot = result.MarketSpot.Clone();

      if (product.Parameters.IsMember("DepoLeft"))
        result.LeftDepo = Spread.FromCSV(product.Parameters.GetValue<string>("DepoLeft"));
      else
        result.LeftDepo = result.MarketLeftDepo.Clone() as Spread;

      if (product.Parameters.IsMember("DepoRight"))
        result.RightDepo = Spread.FromCSV(product.Parameters.GetValue<string>("DepoRight"));
      else
        result.RightDepo = result.MarketRightDepo.Clone() as Spread;

      result.Forward = CurveFunctionFwd.CalculateFwd(result.Spot, result.LeftDepo, result.RightDepo,
                                                     days, leftDayCount, rightDayCount).Round(6);
      if (isOption)
      {
        if (product.Parameters.IsMember("Volatility"))
          result.Volatility = Spread.FromCSV(product.Parameters.GetValue<string>("Volatility"));
        else
          result.Volatility = result.MarketVolatility.Clone() as Spread;
      }

      result.LeftCC = CurveInterpolationDF.ZeroToCC(result.LeftDepo, days, CurveInterpolationDF.GetDayCount(leftDF));
      result.RightCC = CurveInterpolationDF.ZeroToCC(result.RightDepo, days, CurveInterpolationDF.GetDayCount(rightDF));
      result.RightDF = result.RightCC.Exp(-days / 365.0);
      result.LeftDF = result.LeftCC.Exp(-days / 365.0);
    }

    private static void CalcVolatility(OptionProduct result, Product product, Market market, Xml.Rate spotRate)
    {
      result.MarketVolatility = new Spread();

      PropertyNode volParams = new PropertyNode();
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("ATM"));
      Xml.Curve volCurve = market["FXVOL", result.LeftAsset, result.RightAsset];

      if (result.BarrierType == null)
      {
        if (result.SpreadPricing)
        {
          Spread spread = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, result.ExpiryDate, volParams);
          double atmVol = VanillaExpiryVolatility.Calculate(result, market, PositionType.None);

          double vega = BlackScholes.VanillaVega(result.LeftAsset, result.RightAsset, result.ExpiryDate, PositionType.None,
                                                 result.SpotDate, result.SettlementDate, result.HorizonDate,
                                                 market, result.Rate, atmVol);
          vega /= spotRate.Spread[PositionType.None];

          double vSpread = (spread.Ask - spread.Bid) * 0.5 / vega;

          PositionType position = result.PositionType;
          PositionType opposite = position.Opposite();
          result.MarketVolatility[position] = atmVol - vSpread * position.Sign();
          result.MarketVolatility[opposite] = atmVol + vSpread * position.Sign();

          if (result.MarketVolatility[position] < 0)
            result.MarketVolatility[position] = 0;

          if (result.MarketVolatility[opposite] < 0)
            result.MarketVolatility[opposite] = 0;
        }
        else
        {
          result.MarketVolatility[PositionType.None] = VanillaExpiryVolatility.Calculate(result, market, PositionType.None);
        }
      }
      else
      {
        // barrier ATM only
        result.MarketVolatility[PositionType.None] = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, result.ExpiryDate, volParams).Mid;
      }
      result.MarketVolatility.Round(6);
    }

    private static OptionCode MapOptionCode(OptionProduct option)
    {
      OptionCode result = OptionCode.ZZZZ;

      if (option.OptionType == null && option.BarrierType == null)
      {

        if (option.LeftAmount != 0 && option.RightAmount != 0)
          result = option.SettlementDate == option.SpotDate ? OptionCode.FXSP : OptionCode.FRWD;
        else
          result = option.LeftAmount != 0 ? OptionCode.ASST : OptionCode.CASH;
      }
      else
      {
        TrrigerLevelType? levelType = null;
        TrrigerPeriodType? periodType = null;
        if (option.BarrierType != null)
        {
          if (option.BarrierLowerLevel != 0 && option.BarrierUpperLevel != 0)
            levelType = TrrigerLevelType.Both;
          else if (option.BarrierLowerLevel != 0)
            levelType = TrrigerLevelType.Lower;
          else
            levelType = TrrigerLevelType.Upper;

          periodType = option.BarrierStartDate > option.HorizonDate || option.BarrierEndDate < option.ExpiryDate ?
                       TrrigerPeriodType.Partial : TrrigerPeriodType.Continuous;
        }
        DigitalPayoffType? payoffType = null;
        if (option.IsDigital)
          payoffType = option.FixedAsset == option.LeftAsset ? DigitalPayoffType.Asset : DigitalPayoffType.Cash;

        result = OptionMappings.Map(option.OptionType, option.BarrierType, levelType, periodType, payoffType);

        if (option.PremiumMarginAsset != null)
        {
          if (result == OptionCode.VEUC)
            result = OptionCode.CPCC;
          else if (result == OptionCode.VEUP)
            result = OptionCode.CPCP;
        }
      }
      return result;
    }
  }
}
