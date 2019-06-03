#region Using directives

using System;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class UnderlyingAssetHelper
    {
        public static UnderlyingAsset Create(string marketInstrumentId)
        {
            string[] partsOfMarketInstrumentId = marketInstrumentId.Split('-');

            string marketInstrumentType = partsOfMarketInstrumentId[1];//AUD-Deposit-1M => Deposit, AUD-IRFuture-Z8 -> Future, etc

            switch (marketInstrumentType)
            {
                case "DEP":
                case "Deposit":
                    {
                        return CreateDeposit(marketInstrumentId);
                    }
                case "Fra":
                case "SimpleFra":
                    {
                        return CreateSimpleFra(marketInstrumentId);
                    }
                case "Swap":
                case "IRSwap":
                case "SimpleIRSwap":
                    {
                        return CreateSimpleIRSwap(marketInstrumentId);
                    }
                case "IRFuture":
                case "Future":
                    {
                        return CreateFuture(marketInstrumentId);
                    }
                default:
                    {
                        string errorMessage = String.Format("Unable to create UnderlyingAsset from the following marketInstrumentType : '{0}'.", marketInstrumentType);
                        throw new System.Exception(errorMessage);
                    }
            }
        }

        private static UnderlyingAsset CreateDeposit(string marketInstrumentId)
        {
            var deposit = new Deposit();
            string[] slicedInstrumentId = marketInstrumentId.Split('-');
            string instrumentCurrency = slicedInstrumentId[0];
            string instrumentTerm = slicedInstrumentId[2];
            deposit.currency = new IdentifiedCurrency { Value = instrumentCurrency };
            deposit.term = PeriodHelper.Parse(instrumentTerm);
            deposit.instrumentId = new[] { new InstrumentId() };
            deposit.instrumentId[0].Value = marketInstrumentId;
            return deposit;
        }

        private static UnderlyingAsset CreateSimpleFra(string marketInstrumentId)
        {
            var simpleFra = new SimpleFra();
            string[] slicedInstrumentId = marketInstrumentId.Split('-');
            string instrumentCurrency = slicedInstrumentId[0];
            string instrumentTerm = slicedInstrumentId[2];
            simpleFra.currency = new IdentifiedCurrency { Value = instrumentCurrency };
            string startTerm;
            string endTerm = slicedInstrumentId[3];
            if (endTerm != null)
            {
                startTerm = instrumentTerm;
                Period temp = PeriodHelper.Parse(endTerm);
                simpleFra.startTerm = PeriodHelper.Parse(startTerm);
                simpleFra.endTerm = simpleFra.startTerm.Sum(temp);
            }
            else
            {
                string[] slicedTerm = instrumentTerm.Split("vxVX".ToCharArray());//TODO fix this for index tenors.
                startTerm = slicedTerm[0];
                endTerm = slicedTerm[1];
                simpleFra.startTerm = PeriodHelper.Parse(startTerm);
                simpleFra.endTerm = PeriodHelper.Parse(endTerm);
            }           
            simpleFra.instrumentId = new[] { new InstrumentId() };
            simpleFra.instrumentId[0].Value = marketInstrumentId;
            return simpleFra;
        }

        private static UnderlyingAsset CreateSimpleIRSwap(string marketInstrumentId)
        {
            var simpleIRSwap = new SimpleIRSwap();
            string[] slicedInstrumentId = marketInstrumentId.Split('-');
            string instrumentCurrency = slicedInstrumentId[0];
            string instrumentTerm = slicedInstrumentId[2];
            simpleIRSwap.currency = new IdentifiedCurrency { Value = instrumentCurrency };
            simpleIRSwap.term = PeriodHelper.Parse(instrumentTerm);
            simpleIRSwap.instrumentId = new[] { new InstrumentId() };
            simpleIRSwap.instrumentId[0].Value = marketInstrumentId;
            return simpleIRSwap;
        }


        private static UnderlyingAsset CreateFuture(string marketInstrumentId)
        {
            var future = new Future();
            //future.multiplier = "10000";
            string[] slicedInstrumentId = marketInstrumentId.Split('-');
            string instrumentCurrency = slicedInstrumentId[0];
            //            string instrumentTerm = slicedInstrumentId[2];

            //DateTime adjustedExpirationDate = SfeDatesHelper.GetAdjustedExpirationDate(undajustedExpirationDate);
            //            future.maturity = undajustedExpirationDate;
            //            future.maturitySpecified = true;

            future.currency = new IdentifiedCurrency { Value = instrumentCurrency };
            future.instrumentId = new[] { new InstrumentId() };
            future.instrumentId[0].Value = marketInstrumentId;
            return future;
        }

    }

//    public static class UnderlyingAssetHelper
//    {
//        public static UnderlyingAsset Create(string marketInstrumentId)
//        {
//            string[] partsOfMarketInstrumentId = marketInstrumentId.Split('-');

//            string marketInstrumentType = partsOfMarketInstrumentId[0];//AUD-Deposit-1M => Deposit, AUD-Future-Z8 -> Future, etc

//            switch(marketInstrumentType)
//            {
//                case "Deposit":
//                {
//                    return CreateDeposit(marketInstrumentId);
//                }

//                case "SimpleFra":
//                {
//                    return CreateSimpleFra(marketInstrumentId);
//                }

//                case "SimpleIRSwap":
//                {
//                    return CreateSimpleIRSwap(marketInstrumentId);
//                }

//                case "Future":
//                {
//                    return CreateFuture(marketInstrumentId);
//                }

//                default:
//                    {
//                        string errorMessage = String.Format("Unable to create UnderlyingAsset from the following marketInstrumentType : '{0}'.", marketInstrumentType);
//                        throw new Exception(errorMessage);
//                    }
//            }
//        }

//        private static UnderlyingAsset CreateDeposit(string marketInstrumentId)
//        {
//            var deposit = new Deposit();

//            string[] slicedInstrumentId = marketInstrumentId.Split('-');
//            string instrumentCurrency = slicedInstrumentId[1];
//            string instrumentTerm = slicedInstrumentId[2];

//            deposit.currency = new IdentifiedCurrency { Value = instrumentCurrency };
//            deposit.term = PeriodHelper.Parse(instrumentTerm);
//            deposit.instrumentId = new[] { new InstrumentId() };
//            deposit.instrumentId[0].Value = marketInstrumentId;

//            return deposit;
//        }

//        private static UnderlyingAsset CreateSimpleFra(string marketInstrumentId)
//        {
//            var simpleFra = new SimpleFra();

//            string[] slicedInstrumentId = marketInstrumentId.Split('-');
//            string instrumentCurrency = slicedInstrumentId[1];
//            string instrumentTerm = slicedInstrumentId[2];


//            simpleFra.currency = new IdentifiedCurrency { Value = instrumentCurrency };

//            string[] slicedTerm = instrumentTerm.Split("vxVX".ToCharArray());
//            string startTerm = slicedTerm[0];
//            string endTerm = slicedTerm[1];

//            simpleFra.startTerm = PeriodHelper.Parse(startTerm);
//            simpleFra.endTerm = PeriodHelper.Parse(endTerm);
//            simpleFra.instrumentId = new[] { new InstrumentId() };
//            simpleFra.instrumentId[0].Value = marketInstrumentId;
        
//            return simpleFra;
//        }    
        
//        private static UnderlyingAsset CreateSimpleIRSwap(string marketInstrumentId)
//        {
//            var simpleIRSwap = new SimpleIRSwap();
   
          


//            string[] slicedInstrumentId = marketInstrumentId.Split('-');
//            string instrumentCurrency = slicedInstrumentId[1];
//            string instrumentTerm = slicedInstrumentId[2];


//            simpleIRSwap.currency = new IdentifiedCurrency { Value = instrumentCurrency };
//            simpleIRSwap.term = PeriodHelper.Parse(instrumentTerm);

//            simpleIRSwap.instrumentId = new[] { new InstrumentId() };
//            simpleIRSwap.instrumentId[0].Value = marketInstrumentId;


//            return simpleIRSwap;
//        }


//        private static UnderlyingAsset CreateFuture(string marketInstrumentId)
//        {
//            var future = new Future();
//            //future.multiplier = "10000";

//            string[] slicedInstrumentId = marketInstrumentId.Split('-');
//            string instrumentCurrency = slicedInstrumentId[1];
////            string instrumentTerm = slicedInstrumentId[2];

//            //DateTime adjustedExpirationDate = SfeDatesHelper.GetAdjustedExpirationDate(undajustedExpirationDate);
////            future.maturity = undajustedExpirationDate;
////            future.maturitySpecified = true;

//            future.currency = new IdentifiedCurrency { Value = instrumentCurrency };


//            future.instrumentId = new[] { new InstrumentId() };
//            future.instrumentId[0].Value = marketInstrumentId;

//            return future;
//        }
      



       

       

//    }
}