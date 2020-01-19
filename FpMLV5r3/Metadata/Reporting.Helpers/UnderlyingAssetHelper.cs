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

#region Using directives

#endregion

using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
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
                        string errorMessage =
                            $"Unable to create UnderlyingAsset from the following marketInstrumentType : '{marketInstrumentType}'.";
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
            {
                startTerm = instrumentTerm;
                Period temp = PeriodHelper.Parse(endTerm);
                simpleFra.startTerm = PeriodHelper.Parse(startTerm);
                simpleFra.endTerm = simpleFra.startTerm.Sum(temp);
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

            //DateTime adjustedExpirationDate = SfeDatesHelper.GetAdjustedExpirationDate(unadjustedExpirationDate);
            //            future.maturity = unadjustedExpirationDate;
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