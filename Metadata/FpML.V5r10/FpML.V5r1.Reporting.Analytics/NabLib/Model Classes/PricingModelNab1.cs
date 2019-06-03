using System;
using System.Collections.Generic;
using nab.QR.PricingModel;
using nab.QR.Xml;
using System.Linq;

namespace nab.QR.PricingModelNab1
{
  public class PricingModelNab1 : IPricingModel 
  {
    public PricingResult Price(Product product, Market market, PricingParams parameters, ref object state)
    {
      PricingResult result = null;
      OptionProduct option = new OptionProduct();
      option.Init(product, market, parameters);

      OptionMarket optMarket = state as OptionMarket;
      if (optMarket == null)
      {
        optMarket = new OptionMarket();
        state = optMarket;
      }
      optMarket.InitParams(market, parameters);

      if (optMarket.IsBlackModel
       || OptionMappings.IsBlackOption(option.Code)
       || OptionMappings.IsPhysical(option.Code))
      {
        PricingModelBS model = new PricingModelBS();
        result = model.Price(option, optMarket);
      }
      else
      {
        PricingModelCN model = new PricingModelCN();
        optMarket.InitTermParams(market, parameters);
        if (optMarket.IsTermChanged(market, parameters))
          optMarket.InitTermRates(market, parameters);

        double optionTenor = (option.ExpiryDate - option.HorizonDate).Days / 365.0;
        if (optionTenor > optMarket.LastCalibratedTenor)
        {
          VolatilityModelNab1 volModel = new VolatilityModelNab1();
          volModel.CalibrateSurface(optMarket, null, optionTenor);
          volModel.SaveCalibration(optMarket, market);
          optMarket.LastCalibratedTenor = optionTenor;
        }
        result = model.Price(option, optMarket);
      }

      optMarket.Publish(result);
      option.Publish(result);

      result.ProductID = product.UniqueID;
      return result;
    }
  }
}
