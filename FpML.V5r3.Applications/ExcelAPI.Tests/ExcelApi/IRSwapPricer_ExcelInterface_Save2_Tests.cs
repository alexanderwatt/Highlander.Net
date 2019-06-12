#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Constants;
//using Orion.CurveEngine.Tests.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
//using Orion.ModelFramework;
//using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ValuationEngine.Pricers;
//using Orion.CurveEngine.Tests;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    public partial class ExcelAPITests
    {       
        [TestMethod]
        public void Vanilla()
        {
            DateTime valuationDate = DateTime.Today;
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB( valuationDate);
            var payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            var receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg1PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg2PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valReportAsString = irSwapPricer.Save2(Engine.Logger, Engine.Cache, Engine.NameSpace, CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                          payFixed, null, receiveFloat, null, 
                                                          payCFRangeItemList, receiveCFRangeItemList,
                                                          leg1PrincipalExchangeCashflowList, leg2PrincipalExchangeCashflowList,
                                                          leg1BulletPaymentList, leg2BulletPaymentList);
            Debug.Print("ValReport:" + Environment.NewLine + valReportAsString);
        }
        
        private List<InputCashflowRangeItem> GetVanillaCashflows(SwapLegParametersRange_Old leg, ValuationRange valuationRange)
        {
            var result = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, leg, valuationRange);          
            return result;
        }

        private static List<InputCashflowRangeItem> AppendNCashflowsAtTheEnd(List<InputCashflowRangeItem> result, int cashflowsToAppend)
        {
            if (0 == cashflowsToAppend)
            {
                return result;
            }
            InputCashflowRangeItem lastItem = result[result.Count - 1];
            InputCashflowRangeItem newItem = BinarySerializerHelper.Clone(lastItem);           
            newItem.StartDate = lastItem.EndDate;
            newItem.EndDate = newItem.StartDate + (lastItem.EndDate - lastItem.StartDate);
            newItem.PaymentDate = lastItem.PaymentDate + (newItem.EndDate - newItem.StartDate);
            result.Add(newItem);
            return AppendNCashflowsAtTheEnd(result, --cashflowsToAppend);
        }

        private static List<InputCashflowRangeItem> RemoveNCashflows(List<InputCashflowRangeItem> result, int cashflowsToRemove, bool fromTail)
        {
            if (0 == cashflowsToRemove)
            {
                return result;
            }

            if (fromTail)
            {
                result.RemoveAt(result.Count - 1);
            }
            else
            {
                result.RemoveAt(0);
            }

            return RemoveNCashflows(result, --cashflowsToRemove, fromTail);
        }

        private static List<InputCashflowRangeItem> ModifyNotionalNCashflows(List<InputCashflowRangeItem> result, int cashflowsToModify, bool fromTail, decimal coeff)
        {
            if (fromTail)
            {
                for (int i = result.Count - 1; i > result.Count - cashflowsToModify - 1; --i)
                {
                    result[i].NotionalAmount *= (double)coeff;
                }
            }
            else
            {
                for(int i = 0; i < cashflowsToModify; ++i)
                {
                    result[i].NotionalAmount *= (double)coeff;
                }
            }

            return result;
        }

        private static List<InputCashflowRangeItem> ModifyFixedRateNCashflows(List<InputCashflowRangeItem> result, int cashflowsToModify, bool fromTail, decimal coeff)
        {
            if (fromTail)
            {
                for (int i = result.Count - 1; i > result.Count - cashflowsToModify - 1; --i)
                {
                    result[i].Rate += (double)coeff;
                }
            }
            else
            {
                for(int i = 0; i < cashflowsToModify; ++i)
                {
                    result[i].Rate += (double)coeff;
                }
            }

            return result;
        }

        private static List<InputCashflowRangeItem> ModifySpreadNCashflows(List<InputCashflowRangeItem> result, int cashflowsToModify, bool fromTail, decimal coeff)
        {
            if (fromTail)
            {
                for (int i = result.Count - 1; i > result.Count - cashflowsToModify - 1; --i)
                {
                    result[i].Spread += (double)coeff;
                }
            }
            else
            {
                for(int i = 0; i < cashflowsToModify; ++i)
                {
                    result[i].Spread += (double)coeff;
                }
            }

            return result;
        }

        private static List<InputCashflowRangeItem> ModifyPaymentDateNCashflows(List<InputCashflowRangeItem> result, int cashflowsToModify, bool fromTail, int paymentDateShift)
        {
            if (fromTail)
            {
                for (int i = result.Count - 1; i > result.Count - cashflowsToModify - 1; --i)
                {
                    result[i].PaymentDate = result[i].PaymentDate.AddDays(paymentDateShift);
                }
            }
            else
            {
                for(int i = 0; i < cashflowsToModify; ++i)
                {
                    result[i].PaymentDate = result[i].PaymentDate.AddDays(paymentDateShift);
                }
            }

            return result;
        }

        private static List<InputCashflowRangeItem> ModifyStartOrEndDateNCashflows(List<InputCashflowRangeItem> result, int cashflowsToModify, bool fromTail, int dateShift, bool startDate)
        {
            if (fromTail)
            {
                for (int i = result.Count - 1; i > result.Count - cashflowsToModify - 1; --i)
                {
                    if (startDate)
                    {
                        result[i].StartDate = result[i].StartDate.AddDays(dateShift);
                    }
                    else
                    {
                        result[i].EndDate = result[i].EndDate.AddDays(dateShift);
                    }
                }
            }
            else
            {
                for(int i = 0; i < cashflowsToModify; ++i)
                {
                    if (startDate)
                    {
                        result[i].StartDate = result[i].StartDate.AddDays(dateShift);
                    }
                    else
                    {
                        result[i].EndDate = result[i].EndDate.AddDays(dateShift);
                    }
                }
            }

            return result;
        }


        private struct AddOrRemoveCashflows
        {
            public AddOrRemoveCashflows(int fromPayer, int fromReceiver)
            {
                FromPayer = fromPayer;
                FromReceiver = fromReceiver;
            }

            public readonly int FromPayer;
            public readonly int FromReceiver;
        }

        private struct AddOrRemoveCashflowsResult
        {
            public AddOrRemoveCashflowsResult(AddOrRemoveCashflows first, AddOrRemoveCashflows second, int result)
            {
                First = first;
                Second = second;
                Result = result;
            }

            public readonly AddOrRemoveCashflows First;
            public readonly AddOrRemoveCashflows Second;
            public readonly int Result;
        }

        [TestMethod]
        public void AddNThCashflowsAtTheEnd()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            var inputAndOutput = new List<AddOrRemoveCashflowsResult>
                {
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(1, 0), new AddOrRemoveCashflows(0, 0), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 1), new AddOrRemoveCashflows(0, 0), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 0), new AddOrRemoveCashflows(0, 0), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 2), new AddOrRemoveCashflows(0, 0), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 0), new AddOrRemoveCashflows(1, 0), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 2), new AddOrRemoveCashflows(0, 1), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 1), new AddOrRemoveCashflows(1, 1), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(1, 2), new AddOrRemoveCashflows(1, 1), +1)
                };
            foreach (AddOrRemoveCashflowsResult result in inputAndOutput)
            {
                var payCFRangeItemListNMoreCashflows1 = AppendNCashflowsAtTheEnd(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer);
                var receiveCFRangeItemListNMoreCashflows1 = AppendNCashflowsAtTheEnd(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver);
                Pair<ValuationResultRange, Swap> resultMoreCashflows1 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                   payFixed, null, receiveFloat, null,
                                                                                                   payCFRangeItemListNMoreCashflows1, receiveCFRangeItemListNMoreCashflows1,
                                                                                                   new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                   new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                var payCFRangeItemListNMoreCashflows2 = AppendNCashflowsAtTheEnd(GetVanillaCashflows(payFixed, valuationRange), result.Second.FromPayer);
                var receiveCFRangeItemListNMoreCashflows2 = AppendNCashflowsAtTheEnd(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver);
                Pair<ValuationResultRange, Swap> resultMoreCashflows2 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                   payFixed, null, receiveFloat, null,
                                                                                                   payCFRangeItemListNMoreCashflows2, receiveCFRangeItemListNMoreCashflows2,
                                                                                                   new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                   new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                if (result.Result < 0)
                {
                    Assert.IsTrue(resultMoreCashflows1.First.PresentValue < resultMoreCashflows2.First.SwapPresentValue);
                }
                else
                {
                    Assert.IsTrue(resultMoreCashflows1.First.SwapPresentValue > resultMoreCashflows2.First.SwapPresentValue);
                }
            }
        }

        //[Ignore]
        [TestMethod]
        public void RemoveNThCashflows()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            var inputAndOutput = new List<AddOrRemoveCashflowsResult>
                {
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(1, 0), new AddOrRemoveCashflows(0, 0), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 1), new AddOrRemoveCashflows(0, 0), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 0), new AddOrRemoveCashflows(0, 0), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 2), new AddOrRemoveCashflows(0, 0), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 0), new AddOrRemoveCashflows(1, 0), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(0, 2), new AddOrRemoveCashflows(0, 1), -1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(2, 1), new AddOrRemoveCashflows(1, 1), +1),
                    new AddOrRemoveCashflowsResult(new AddOrRemoveCashflows(1, 2), new AddOrRemoveCashflows(1, 1), -1)
                };
            foreach (bool fromTail in new[] { true, false })
            {
                foreach (AddOrRemoveCashflowsResult result in inputAndOutput)
                {
                    var payCFRangeItemListNLessCashflows1 = RemoveNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer, fromTail);
                    var receiveCFRangeItemListNLessCashflows1 = RemoveNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver, fromTail);
                    Pair<ValuationResultRange, Swap> resultLessCashflows1 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange, 
                                                                                                       payFixed, null, receiveFloat, null,
                                                                                                       payCFRangeItemListNLessCashflows1, receiveCFRangeItemListNLessCashflows1,
                                                                                                       new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                       new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                    var payCFRangeItemListNLessCashflows2 = RemoveNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.Second.FromPayer, fromTail);
                    var receiveCFRangeItemListNLessCashflows2 = RemoveNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver, fromTail);
                    Pair<ValuationResultRange, Swap> resultLessCashflows2 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange, 
                                                                                                       payFixed, null, receiveFloat, null,
                                                                                                       payCFRangeItemListNLessCashflows2, receiveCFRangeItemListNLessCashflows2,
                                                                                                       new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                       new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                    Assert.IsTrue(resultLessCashflows1.First.FutureValue != resultLessCashflows2.First.FutureValue);
                }
            }
        }


        private struct ModifyNotionalCashflows
        {
            public ModifyNotionalCashflows(int fromPayer, decimal payerChangeCoeff, int fromReceiver, decimal receiverChangeCoeff)
            {
                FromPayer = fromPayer;
                PayerChangeCoeff = payerChangeCoeff;
                FromReceiver = fromReceiver;
                ReceiverChangeCoeff = receiverChangeCoeff;
            }

            public readonly int FromPayer;
            public readonly decimal PayerChangeCoeff;

            public readonly int FromReceiver;
            public readonly decimal ReceiverChangeCoeff;
        }

        private struct ModifyNotionalCashflowsResult
        {
            public ModifyNotionalCashflowsResult(ModifyNotionalCashflows first, ModifyNotionalCashflows second, int result)
            {
                First = first;
                Second = second;
                _result = result;
            }

            public ModifyNotionalCashflows First;
            public ModifyNotionalCashflows Second;
            private readonly int _result;
        }

        private struct ModifyFixedRateCashflows
        {
            public ModifyFixedRateCashflows(int fromPayer, decimal payerChangeCoeff, int fromReceiver, decimal receiverChangeCoeff)
            {
                FromPayer = fromPayer;
                PayerChangeCoeff = payerChangeCoeff;
                _fromReceiver = fromReceiver;
                _receiverChangeCoeff = receiverChangeCoeff;
            }

            public readonly int FromPayer;
            public readonly decimal PayerChangeCoeff;

            private readonly int _fromReceiver;
            private readonly decimal _receiverChangeCoeff;
        }

        private struct ModifyFixedRateCashflowsResult
        {
            public ModifyFixedRateCashflowsResult(ModifyFixedRateCashflows first, ModifyFixedRateCashflows second, int result)
            {
                First = first;
                Second = second;
                Result = result;
            }

            public readonly ModifyFixedRateCashflows First;
            public ModifyFixedRateCashflows Second;
            public readonly int Result;
        }

        private struct ModifySpreadCashflows
        {
            public ModifySpreadCashflows(int fromPayer, decimal payerChangeCoeff, int fromReceiver, decimal receiverChangeCoeff)
            {
                FromPayer = fromPayer;
                PayerChangeCoeff = payerChangeCoeff;
                FromReceiver = fromReceiver;
                ReceiverChangeCoeff = receiverChangeCoeff;
            }

            public readonly int FromPayer;
            public readonly decimal PayerChangeCoeff;
            public readonly int FromReceiver;
            public readonly decimal ReceiverChangeCoeff;
        }

        private struct ModifySpreadCashflowsResult
        {
            public ModifySpreadCashflowsResult(ModifySpreadCashflows first, ModifySpreadCashflows second, int result)
            {
                First = first;
                Second = second;
                Result = result;
            }

            public ModifySpreadCashflows First;
            public ModifySpreadCashflows Second;
            public readonly int Result;
        }

        private struct ModifyPaymentDateCashflows
        {
            public ModifyPaymentDateCashflows(int fromPayer, int payerDaysShift, int fromReceiver, int receiverDaysShift)
            {
                FromPayer = fromPayer;
                PayerDaysShift = payerDaysShift;
                FromReceiver = fromReceiver;
                ReceiverDaysShift = receiverDaysShift;
            }

            public readonly int FromPayer;
            public readonly int PayerDaysShift;

            public readonly int FromReceiver;
            public readonly int ReceiverDaysShift;
        }

        private struct ModifyPaymentDateCashflowsResult
        {
            public ModifyPaymentDateCashflowsResult(ModifyPaymentDateCashflows first, ModifyPaymentDateCashflows second, int result)
            {
                First = first;
                Second = second;
            }

            public ModifyPaymentDateCashflows First;
            public ModifyPaymentDateCashflows Second;
        }

        private struct ModifyStartOrEndDateCashflows
        {
            public ModifyStartOrEndDateCashflows(int fromPayer, int payerDaysShift, int fromReceiver, int receiverDaysShift)
            {
                FromPayer = fromPayer;
                PayerDaysShift = payerDaysShift;
                FromReceiver = fromReceiver;
                ReceiverDaysShift = receiverDaysShift;
            }

            public readonly int FromPayer;
            public readonly int PayerDaysShift;

            public readonly int FromReceiver;
            public readonly int ReceiverDaysShift;
        }

        private struct ModifyStartOrEndDateCashflowsResult
        {
            public ModifyStartOrEndDateCashflowsResult(ModifyStartOrEndDateCashflows first, ModifyStartOrEndDateCashflows second, int result)
            {
                First = first;
                Second = second;
            }

            public ModifyStartOrEndDateCashflows First;
            public ModifyStartOrEndDateCashflows Second;
        }


        [TestMethod]
        public void ModifyNotional()
        {
            InterestRateSwapPricer irSwapPricer = new InterestRateSwapPricer();
            DateTime valuationDate = DateTime.Today;
            TradeRange tradeRange = new TradeRange
            {
                Id = "TradeId_12345",
                TradeDate = valuationDate
            };
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<ModifyNotionalCashflowsResult> inputAndOutput = new List<ModifyNotionalCashflowsResult>
            {
                new ModifyNotionalCashflowsResult(new ModifyNotionalCashflows(1, 0.9m, 0, 1.0m),
                    new ModifyNotionalCashflows(0, 0.9m, 0, 1.0m),
                    +1),
                new ModifyNotionalCashflowsResult(new ModifyNotionalCashflows(2, 0.9m, 0, 1.0m),
                    new ModifyNotionalCashflows(0, 0.9m, 0, 1.0m),
                    +1),
                new ModifyNotionalCashflowsResult(new ModifyNotionalCashflows(2, 0.9m, 0, 1.0m),
                    new ModifyNotionalCashflows(1, 0.9m, 0, 1.0m),
                    +1),
                new ModifyNotionalCashflowsResult(new ModifyNotionalCashflows(0, 0.9m, 0, 1.0m),
                    new ModifyNotionalCashflows(1, 0.9m, 0, 1.0m),
                    -1)
            };
            //less notional on pay side - PV is higher
            //
            //
            foreach(bool fromPayerTail in new[]{true, false})
            {
                foreach(bool fromReceiverTail in new[]{true, false})
                {
                    foreach (ModifyNotionalCashflowsResult result in inputAndOutput)
                    {
                        var payCFRangeItemCashflows1 = ModifyNotionalNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer, fromPayerTail, result.First.PayerChangeCoeff);
                        var receiveCFRangeItemCashflows1 = ModifyNotionalNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver, fromReceiverTail, result.First.ReceiverChangeCoeff);

                        string valReportAsString1 = irSwapPricer.Save2(Engine.Logger, Engine.Cache, Engine.NameSpace, CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                        payFixed, null, receiveFloat, null,
                                                                        payCFRangeItemCashflows1, receiveCFRangeItemCashflows1,
                                                                        new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                        new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        Debug.Print("ValReport:" + Environment.NewLine + valReportAsString1);                      
                        var payCFRangeItemListCashflows2 = ModifyNotionalNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.Second.FromPayer, fromPayerTail, result.Second.PayerChangeCoeff);
                        var receiveCFRangeItemListCashflows2 = ModifyNotionalNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver, fromReceiverTail, result.Second.ReceiverChangeCoeff);
                        string valReportAsString2 = irSwapPricer.Save2(Engine.Logger, Engine.Cache, Engine.NameSpace, CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                       payFixed, null, receiveFloat, null,
                                                                       payCFRangeItemListCashflows2, receiveCFRangeItemListCashflows2,
                                                                       new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                       new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        Debug.Print("ValReport:" + Environment.NewLine + valReportAsString2);
                    }
                    GC.Collect();
                }
            }
        }

        [TestMethod]
        public void ModifyFixedRate()
        {
            DateTime valuationDate = DateTime.Today;
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate); //RateCurveExcelInterfaceTests.ExcelInterface_CreateAUDCurveFromDepostSwapsFuturesFras_WithDates(valuationDate, valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            var inputAndOutput = new List<ModifyFixedRateCashflowsResult>
                {
                    new ModifyFixedRateCashflowsResult(new ModifyFixedRateCashflows(2, +0.001m, 0, 0.0m),
                                                       new ModifyFixedRateCashflows(1, +0.001m, 0, 0.0m),
                                                       -1)
                };
            foreach(bool fromPayerTail in new[]{true, false})
            {
                {
                    foreach (ModifyFixedRateCashflowsResult result in inputAndOutput)
                    {
                        var payCFRangeItemCashflows1 = ModifyFixedRateNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer, fromPayerTail, result.First.PayerChangeCoeff);
                        var receiveCFRangeItemCashflows1 = GetVanillaCashflows(receiveFloat, valuationRange);
                        string valReportAsString2 = irSwapPricer.Save2(Engine.Logger, Engine.Cache, Engine.NameSpace, CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                                       payFixed, null, receiveFloat, null,
                                                                       payCFRangeItemCashflows1, receiveCFRangeItemCashflows1,
                                                                       new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                       new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        Debug.Print("ValReport:" + Environment.NewLine + valReportAsString2);
                        Debug.Print("Result:" + result.Result);
                    }
                }
            }
        }

        [TestMethod]
        public void ModifySpread()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            var inputAndOutput = new List<ModifySpreadCashflowsResult>
            {
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 1, +0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 0, +0.001m),
                    +1),
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 2, +0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 0, +0.001m),
                    +1),
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 2, +0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 1, +0.001m),
                    +1),
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 1, -0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 0, -0.001m),
                    -1),
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 2, -0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 0, -0.001m),
                    -1),
                new ModifySpreadCashflowsResult(new ModifySpreadCashflows(0, 0.0m, 2, -0.001m),
                    new ModifySpreadCashflows(0, 0.0m, 1, -0.001m),
                    -1)
            };
            //increase spread - PV is UP (more receiving)
            {
                foreach (bool fromReceiverTail in new[] { true, false })
                {
                    foreach (ModifySpreadCashflowsResult result in inputAndOutput)
                    {
                        var payCFRangeItemCashflows1 = GetVanillaCashflows(payFixed, valuationRange);
                        var receiveCFRangeItemCashflows1 = ModifySpreadNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver, fromReceiverTail, result.First.ReceiverChangeCoeff);
                        Pair<ValuationResultRange, Swap> resultLessCashflows1 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                           payFixed, null, receiveFloat, null,
                                                                                                           payCFRangeItemCashflows1, receiveCFRangeItemCashflows1,
                                                                                                           new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                           new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        var payCFRangeItemCashflows2 = GetVanillaCashflows(payFixed, valuationRange);                      
                        var receiveCFRangeItemCashflows2 = ModifySpreadNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver, fromReceiverTail, result.Second.ReceiverChangeCoeff);
                        Pair<ValuationResultRange, Swap> resultLessCashflows2 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                           payFixed, null, receiveFloat, null,
                                                                                                           payCFRangeItemCashflows2, receiveCFRangeItemCashflows2,
                                                                                                           new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                           new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        if (result.Result < 0)
                        {
                            Assert.IsTrue(resultLessCashflows1.First.SwapPresentValue < resultLessCashflows2.First.SwapPresentValue);
                        }
                        else
                        {
                            Assert.IsTrue(resultLessCashflows1.First.SwapPresentValue > resultLessCashflows2.First.SwapPresentValue);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ModifyPaymentDate()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<ModifyPaymentDateCashflowsResult> inputAndOutput = new List<ModifyPaymentDateCashflowsResult>
            {
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(1, 10, 0, 0),
                    new ModifyPaymentDateCashflows(0, 10, 0, 0),
                    +1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(1, -1, 0, 0),
                    new ModifyPaymentDateCashflows(0, 10, 0, 0),
                    -1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(2, 10, 0, 0),
                    new ModifyPaymentDateCashflows(0, 10, 0, 0),
                    +1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(2, -1, 0, 0),
                    new ModifyPaymentDateCashflows(0, 10, 0, 0),
                    -1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(2, 10, 0, 0),
                    new ModifyPaymentDateCashflows(1, 10, 0, 0),
                    +1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(2, -1, 0, 0),
                    new ModifyPaymentDateCashflows(1, -1, 0, 0),
                    -1),
                new ModifyPaymentDateCashflowsResult(new ModifyPaymentDateCashflows(0, 0, 2, 10),
                    new ModifyPaymentDateCashflows(0, 0, 0, 10),
                    -1)
            };
            //move payment date further into future spread - PV is UP (less receiving)
            foreach (bool fromPayerTail in new[] { true, false })
            {
                foreach (bool fromReceiverTail in new[] { true, false })
                {
                    foreach (ModifyPaymentDateCashflowsResult result in inputAndOutput)
                    {
                        var payCFRangeItemCashflows1 = ModifyPaymentDateNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer, fromPayerTail, result.First.PayerDaysShift);
                        var receiveCFRangeItemCashflows1 = ModifyPaymentDateNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver, fromReceiverTail, result.First.ReceiverDaysShift);
                        Pair<ValuationResultRange, Swap> resultLessCashflows1 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange, 
                                                                                                           payFixed, null, receiveFloat, null,
                                                                                                           payCFRangeItemCashflows1, receiveCFRangeItemCashflows1,
                                                                                                           new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                           new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        var payCFRangeItemCashflows2 = ModifyPaymentDateNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.Second.FromPayer, fromPayerTail, result.Second.PayerDaysShift);
                        var receiveCFRangeItemCashflows2 = ModifyPaymentDateNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver, fromReceiverTail, result.Second.ReceiverDaysShift);
                        Pair<ValuationResultRange, Swap> resultLessCashflows2 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange, 
                                                                                                           payFixed, null, receiveFloat, null,
                                                                                                           payCFRangeItemCashflows2, receiveCFRangeItemCashflows2,
                                                                                                           new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                           new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                        Assert.IsTrue(resultLessCashflows1.First.FutureValue == resultLessCashflows2.First.FutureValue);
                        Assert.IsTrue(resultLessCashflows1.First.PayLegFutureValue == resultLessCashflows2.First.PayLegFutureValue);
                        Assert.IsTrue(resultLessCashflows1.First.ReceiveLegFutureValue == resultLessCashflows2.First.ReceiveLegFutureValue);
                    }
                }
            }
        }

        //[Ignore]
        [TestMethod]
        public void ModifyStartOrEndDate()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<ModifyStartOrEndDateCashflowsResult> inputAndOutput = new List<ModifyStartOrEndDateCashflowsResult>
            {
                new ModifyStartOrEndDateCashflowsResult(new ModifyStartOrEndDateCashflows(1, 10, 0, 0),
                    new ModifyStartOrEndDateCashflows(0, 10, 0, 0),
                    +1),
                new ModifyStartOrEndDateCashflowsResult(new ModifyStartOrEndDateCashflows(2, 10, 0, 0),
                    new ModifyStartOrEndDateCashflows(0, 10, 0, 0),
                    +1),
                new ModifyStartOrEndDateCashflowsResult(new ModifyStartOrEndDateCashflows(2, 10, 0, 0),
                    new ModifyStartOrEndDateCashflows(1, 10, 0, 0),
                    +1),
                new ModifyStartOrEndDateCashflowsResult(new ModifyStartOrEndDateCashflows(0, 0, 1, 10),
                    new ModifyStartOrEndDateCashflows(0, 0, 0, 0),
                    -1)
            };
            //move payment date further into future spread - PV is UP (less receiving)
            foreach (bool startDate in new[] { true, false })
            {
                foreach (bool fromPayerTail in new[] { true, false })
                {
                    foreach (bool fromReceiverTail in new[] { true, false })
                    {
                        foreach (ModifyStartOrEndDateCashflowsResult result in inputAndOutput)
                        {
                            var payCFRangeItemCashflows1 = ModifyStartOrEndDateNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.First.FromPayer, fromPayerTail, result.First.PayerDaysShift, startDate);
                            var receiveCFRangeItemCashflows1 = ModifyStartOrEndDateNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.First.FromReceiver, fromReceiverTail, result.First.ReceiverDaysShift, startDate);
                            Pair<ValuationResultRange, Swap> resultLessCashflows1 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                               payFixed, null, receiveFloat, null,
                                                                                                               payCFRangeItemCashflows1, receiveCFRangeItemCashflows1,
                                                                                                               new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                               new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                            var payCFRangeItemCashflows2 = ModifyStartOrEndDateNCashflows(GetVanillaCashflows(payFixed, valuationRange), result.Second.FromPayer, fromPayerTail, result.Second.PayerDaysShift, startDate);
                            var receiveCFRangeItemCashflows2 = ModifyStartOrEndDateNCashflows(GetVanillaCashflows(receiveFloat, valuationRange), result.Second.FromReceiver, fromReceiverTail, result.Second.ReceiverDaysShift, startDate);
                            Pair<ValuationResultRange, Swap> resultLessCashflows2 = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                               payFixed, null, receiveFloat, null,
                                                                                                               payCFRangeItemCashflows2, receiveCFRangeItemCashflows2,
                                                                                                               new List<InputPrincipalExchangeCashflowRangeItem>(), new List<InputPrincipalExchangeCashflowRangeItem>(),
                                                                                                               new List<AdditionalPaymentRangeItem>(), new List<AdditionalPaymentRangeItem>());
                            //  FV should stay unchanged
                            //
                            Assert.IsTrue(resultLessCashflows1.First.FutureValue != resultLessCashflows2.First.FutureValue);
                        }
                    }
                }
            }//start or end 
        }

        [TestMethod]
        public void VanillaInitialAndFinalPEBothStreams()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve( valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            //  Get price of vanilla swap
            //
            ValuationResultRange valuationResultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            var payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            var receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipleExchangeCashflowList = GetInitialAndFinalPEX(payFixed);
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipleExchangeCashflowList = GetInitialAndFinalPEX(receiveFloat);
            List<AdditionalPaymentRangeItem> leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            List<AdditionalPaymentRangeItem> leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            Pair<ValuationResultRange, Swap> nonVanillaPriceImpl = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                              payFixed, null, receiveFloat, null,
                                                                                              payCFRangeItemList, receiveCFRangeItemList,
                                                                                              leg1PrincipleExchangeCashflowList, leg2PrincipleExchangeCashflowList,
                                                                                              leg1BulletPaymentList, leg2BulletPaymentList);
            //  Results should be exactly the same (except PV of the legs)
            //
            Assert.AreEqual((double)valuationResultRange.SwapPresentValue, (double)nonVanillaPriceImpl.First.SwapPresentValue);
            Assert.AreEqual((double)valuationResultRange.FutureValue, (double)nonVanillaPriceImpl.First.FutureValue);
            Assert.AreEqual((double)valuationResultRange.PayLegPresentValue, (double)nonVanillaPriceImpl.First.PayLegPresentValue);
            Assert.AreEqual((double)valuationResultRange.ReceiveLegPresentValue, (double)nonVanillaPriceImpl.First.ReceiveLegPresentValue);
            Assert.AreEqual((double)valuationResultRange.PayLegFutureValue, (double)nonVanillaPriceImpl.First.PayLegFutureValue);
            Assert.AreEqual((double)valuationResultRange.ReceiveLegFutureValue, (double)nonVanillaPriceImpl.First.ReceiveLegFutureValue);
            // 2 PEX for each stream
            //
            foreach (InterestRateStream interestRateStream in nonVanillaPriceImpl.Second.swapStream)
            {
                Assert.AreEqual(2, interestRateStream.cashflows.principalExchange.Length);
            }
            // No payments
            //
            Assert.AreEqual(nonVanillaPriceImpl.Second.additionalPayment.Length, 0);
        }

        private static List<InputPrincipalExchangeCashflowRangeItem> GetInitialAndFinalPEX(SwapLegParametersRange_Old legParam)
        {
            var result = new List<InputPrincipalExchangeCashflowRangeItem>();
            var initial = new InputPrincipalExchangeCashflowRangeItem
                {
                    Amount = (double) legParam.NotionalAmount,
                    PaymentDate = legParam.EffectiveDate
                };
            result.Add(initial);
            var final = new InputPrincipalExchangeCashflowRangeItem
                {
                    Amount = (double) -legParam.NotionalAmount,
                    PaymentDate = legParam.MaturityDate
                };
            result.Add(final);
            return result;
        }

        [Ignore]
        [TestMethod]
        public void VanillaAdditionalPayments1()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            //  Get price of vanilla swap
            //
            ValuationResultRange valuationResultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            var payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            var receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            var leg1PrincipleExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg2PrincipleExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg1Payments = new List<Pair<DateTime, decimal>>
                {
                    new Pair<DateTime, decimal>(payFixed.EffectiveDate.AddDays(10), 100000),
                    new Pair<DateTime, decimal>(payFixed.EffectiveDate.AddDays(100), 100000)
                };
            List<AdditionalPaymentRangeItem> leg1BulletPaymentList = CreateBulletPayments(leg1Payments);//leg1 payer chooses to pay extra
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            Pair<ValuationResultRange, Swap> nonVanillaPriceImplTwoPaymentsByPayer = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                                payFixed, null, receiveFloat, null,
                                                                                                                payCFRangeItemList, receiveCFRangeItemList,
                                                                                                                leg1PrincipleExchangeCashflowList, leg2PrincipleExchangeCashflowList,
                                                                                                                leg1BulletPaymentList, leg2BulletPaymentList);
            //  Payments made by payer of the swap 
            //
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayer.First.SwapPresentValue < valuationResultRange.SwapPresentValue);
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayer.First.FutureValue < valuationResultRange.FutureValue);
            // NO PExs
            //
            foreach (InterestRateStream interestRateStream in nonVanillaPriceImplTwoPaymentsByPayer.Second.swapStream)
            {
                Assert.AreEqual(interestRateStream.cashflows.principalExchange.Length, 0);
            }
            // 2 payments
            //
            Assert.AreEqual(2, nonVanillaPriceImplTwoPaymentsByPayer.Second.additionalPayment.Length);
            var leg2Payments = new List<Pair<DateTime, decimal>>
                {
                    new Pair<DateTime, decimal>(receiveFloat.EffectiveDate.AddDays(10), 150000),
                    new Pair<DateTime, decimal>(receiveFloat.EffectiveDate.AddDays(100), 150000)
                };
            leg2BulletPaymentList = CreateBulletPayments(leg2Payments);
            //  NOW additional payments occur from both sides, but leg2 payer pays more
            //
            Pair<ValuationResultRange, Swap> nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                                                        payFixed, null, receiveFloat, null,
                                                                                                                                        payCFRangeItemList, receiveCFRangeItemList,
                                                                                                                                        leg1PrincipleExchangeCashflowList, leg2PrincipleExchangeCashflowList,
                                                                                                                                        leg1BulletPaymentList, leg2BulletPaymentList);
            //  Payments made by payer of the swap 
            //
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.First.SwapPresentValue > valuationResultRange.SwapPresentValue);
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.First.FutureValue > valuationResultRange.FutureValue);
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.First.SwapPresentValue > nonVanillaPriceImplTwoPaymentsByPayer.First.SwapPresentValue);
            Assert.IsTrue(nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.First.FutureValue > nonVanillaPriceImplTwoPaymentsByPayer.First.FutureValue);
            // NO PExs
            //
            foreach (InterestRateStream interestRateStream in nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.Second.swapStream)
            {
                Assert.AreEqual(interestRateStream.cashflows.principalExchange.Length, 0);
            }
            // 2 payments
            //
            Assert.AreEqual(4, nonVanillaPriceImplTwoPaymentsByPayerAndTwoPaymentsByReceiver.Second.additionalPayment.Length);
        }

        private static List<AdditionalPaymentRangeItem> CreateBulletPayments(IEnumerable<Pair<DateTime, decimal>> payments)
        {
            var result = new List<AdditionalPaymentRangeItem>();
            foreach (Pair<DateTime, decimal> pair in payments)
            {
                var paymentRangeItem = new AdditionalPaymentRangeItem
                    {
                        PaymentDate = pair.First,
                        Amount = (double) pair.Second
                    };
                result.Add(paymentRangeItem);
            }
            return result;
        }  
    }
}
