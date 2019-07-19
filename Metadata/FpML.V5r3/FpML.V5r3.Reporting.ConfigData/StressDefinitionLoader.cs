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
using Core.Common;
using Metadata.Common;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    public static class StressDefinitionLoader
    {
        /// <summary>
        /// Makes the fx curve scenario rule.
        /// </summary>
        /// <param name="scenarioId">The scenario id.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="currency">currency.</param>
        /// <param name="stressId">The stress id.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private static ScenarioRule MakeFxCurveScenarioRule(string scenarioId, int priority, string currency, string stressId, string description)
        {
            // currency1 is mandatory, currency2 = USD
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));
            return new ScenarioRule
                       {
                ScenarioId = scenarioId,
                Category = ScenarioConst.FxPrefix,
                RuleId = "1",
                Disabled = false,
                Priority = priority,
                // base curve filter
                FilterExpr =
                    Expr.BoolAND(
                        Expr.IsEQU(CurveProp.PricingStructureType, "FxCurve"),
                        Expr.IsEQU(CurveProp.Currency1, currency))
                    .Serialise(),
                // output
                RuleDesc = description,
                StressId = stressId
            };

        }

        private static ScenarioRule MakeIrCurveScenarioRule(string scenarioId, int priority, string currency, string indexName, string indexTenor, string stressId, string description)
        {
            return new ScenarioRule
                       {
                ScenarioId = scenarioId,
                Category = ScenarioConst.IrPrefix,
                RuleId = "1",
                Disabled = false,
                Priority = priority,
                // base curve filter
                FilterExpr = Expr.BoolAND(
                    Expr.BoolOR(
                        Expr.IsEQU(CurveProp.PricingStructureType, "RateCurve"),
                        Expr.IsEQU(CurveProp.PricingStructureType, "RateBasisCurve"),
                        Expr.IsEQU(CurveProp.PricingStructureType, "RateSpreadCurve"),
                        Expr.IsEQU(CurveProp.PricingStructureType, "DiscountCurve")),
                    ((indexName != null) ? Expr.Contains(CurveProp.IndexName, indexName) : null),
                    ((indexTenor != null) ? Expr.IsEQU(CurveProp.IndexTenor, indexTenor) : null),
                    ((currency != null) ? Expr.IsEQU(CurveProp.Currency1, currency) : null)).Serialise(),
                // output
                RuleDesc = description,
                StressId = stressId
            };
        }

        private static StressRule MakeStressRule(string stressId, string ruleId, int priority, IExpression filterExpr, IExpression updateExpr)
        {
            return new StressRule
                       {
                StressId = stressId,
                RuleId = ruleId,
                Disabled = false,
                Priority = priority,
                // base curve filter
                FilterExpr = filterExpr.Serialise(),
                // output
                UpdateExpr = updateExpr.Serialise()
            };
        }

        // update expressions
        private static IExpression NoAdjustment()
        {
            return Expr.Prop("MarketQuote");
        }
        private static IExpression ParallelUp(decimal shift)
        {
            return Expr.NumADD(Expr.Prop("MarketQuote"), Expr.Const(shift));
        }
        private static IExpression ParallelDn(decimal shift)
        {
            return Expr.NumADD(Expr.Prop("MarketQuote"), Expr.Const(-shift));
        }
        private static IExpression PercentUp(decimal rate)
        {
            return Expr.NumMUL(Expr.Prop("MarketQuote"), Expr.Const(1.0M + rate));
        }
        private static IExpression PercentDn(decimal rate)
        {
            return Expr.NumMUL(Expr.Prop("MarketQuote"), Expr.Const(1.0M - rate));
        }

        public static void LoadScenarioDefinitions(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            // generate scenario definitions
            logger.LogDebug("Loading scenario definitions...");
            var scenarioRules = new List<ScenarioRule>
                                    {
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Up01pc, 1, "AUD",
                                                                StressConst.PercentUp01, "AUD+1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Up05pc, 1, "AUD",
                                                                StressConst.PercentUp05, "AUD+5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Up10pc, 1, "AUD",
                                                                StressConst.PercentUp10, "AUD+10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Dn01pc, 1, "AUD",
                                                                StressConst.PercentDn01, "AUD-1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Dn05pc, 1, "AUD",
                                                                StressConst.PercentDn05, "AUD-5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.AUD_Dn10pc, 1, "AUD",
                                                                StressConst.PercentDn10, "AUD-10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Up01pc, 1, "GBP",
                                                                StressConst.PercentUp01, "GBP+1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Up05pc, 1, "GBP",
                                                                StressConst.PercentUp05, "GBP+5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Up10pc, 1, "GBP",
                                                                StressConst.PercentUp10, "GBP+10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Dn01pc, 1, "GBP",
                                                                StressConst.PercentDn01, "GBP-1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Dn05pc, 1, "GBP",
                                                                StressConst.PercentDn05, "GBP-5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.GBP_Dn10pc, 1, "GBP",
                                                                StressConst.PercentDn10, "GBP-10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Up01pc, 1, "EUR",
                                                                StressConst.PercentUp01, "EUR+1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Up05pc, 1, "EUR",
                                                                StressConst.PercentUp05, "EUR+5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Up10pc, 1, "EUR",
                                                                StressConst.PercentUp10, "EUR+10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Dn01pc, 1, "EUR",
                                                                StressConst.PercentDn01, "EUR-1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Dn05pc, 1, "EUR",
                                                                StressConst.PercentDn05, "EUR-5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.EUR_Dn10pc, 1, "EUR",
                                                                StressConst.PercentDn10, "EUR-10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Up01pc, 1, "JPY",
                                                                StressConst.PercentUp01, "JPY+1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Up05pc, 1, "JPY",
                                                                StressConst.PercentUp05, "JPY+5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Up10pc, 1, "JPY",
                                                                StressConst.PercentUp10, "JPY+10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Dn01pc, 1, "JPY",
                                                                StressConst.PercentDn01, "JPY-1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Dn05pc, 1, "JPY",
                                                                StressConst.PercentDn05, "JPY-5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.JPY_Dn10pc, 1, "JPY",
                                                                StressConst.PercentDn10, "JPY-10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Up01pc, 1, "NZD",
                                                                StressConst.PercentUp01, "NZD+1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Up05pc, 1, "NZD",
                                                                StressConst.PercentUp05, "NZD+5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Up10pc, 1, "NZD",
                                                                StressConst.PercentUp10, "NZD+10%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Dn01pc, 1, "NZD",
                                                                StressConst.PercentDn01, "NZD-1%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Dn05pc, 1, "NZD",
                                                                StressConst.PercentDn05, "NZD-5%"),
                                        MakeFxCurveScenarioRule(ScenarioConst.NZD_Dn10pc, 1, "NZD",
                                                                StressConst.PercentDn10, "NZD-10%"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GlobalIRUp001bp, 1, null, null, null,
                                                                StressConst.ParallelUp001, "GlobalIR+1bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GlobalIRUp050bp, 1, null, null, null,
                                                                StressConst.ParallelUp050, "GlobalIR+50bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GlobalIRUp100bp, 1, null, null, null,
                                                                StressConst.ParallelUp100, "GlobalIR+100bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.AUDBBSWUp001bp, 1, "AUD", null, null,
                                                                StressConst.ParallelUp001, "AUDBBSW+1bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.AUDBBSWUp050bp, 1, "AUD", null, null,
                                                                StressConst.ParallelUp050, "AUDBBSW+50bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.AUDBBSWUp100bp, 1, "AUD", null, null,
                                                                StressConst.ParallelUp100, "AUDBBSW+100bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.AUDBBSWDn050bp, 1, "AUD", null, null,
                                                                StressConst.ParallelDn050, "AUDBBSW-50bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.USDLIBORUp001bp, 1, "USD", null, null,
                                                                StressConst.ParallelUp001, "USDLIB+1bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.USDLIBORUp050bp, 1, "USD", null, null,
                                                                StressConst.ParallelUp050, "USDLIB+50bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.USDLIBORUp100bp, 1, "USD", null, null,
                                                                StressConst.ParallelUp100, "USDLIB+100bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GBPLIBORUp001bp, 1, "GBP", null, null,
                                                                StressConst.ParallelUp001, "GBPLIB+1bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GBPLIBORUp050bp, 1, "GBP", null, null,
                                                                StressConst.ParallelUp050, "GBPLIB+50bp"),
                                        MakeIrCurveScenarioRule(ScenarioConst.GBPLIBORUp100bp, 1, "GBP", null, null,
                                                                StressConst.ParallelUp100, "GBPLIB+100bp")
                                    };
            // FX scenarios
            //scenarioRules.Add(MakeFxCurveScenarioRule(ScenarioConst.CurrentFx, 1, null, null, null, "Current FX"));
            // AUD
            // GBP
            // EUR
            // JPY
            // NZD
            // IR scenarios
            //scenarioRules.Add(MakeIrCurveScenarioRule(ScenarioConst.CurrentIR, 1, null, null, null, null, "Current IR"));

            // save scenario rules
            foreach (ScenarioRule scenarioRule in scenarioRules)
            {
                const string itemType = "ScenarioDefinition";
                string idSuffix = scenarioRule.ScenarioId; // +"." + scenarioRule.RuleId;
                logger.LogDebug("  Loading {0} ...", idSuffix);
                var scenarioProps = new NamedValueSet();
                scenarioProps.Set(ValueProp.Scenario, scenarioRule.ScenarioId);
                Pair<string, NamedValueSet> itemInfo = LoadConfigDataHelper.StandardConfigProps(scenarioProps, itemType, idSuffix, nameSpace);
                TimeSpan lifetime = scenarioRule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(scenarioRule, itemInfo.First, itemInfo.Second, false, lifetime);
            }
            logger.LogDebug("Loaded {0} scenario definitions", scenarioRules.Count);
        }

        public static void LoadStressDefinitions(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            // generate stress definitions
            // - ZeroStress: the curve definition with the market quotes adjusted by zero
            // - ParallelUp/Dn: the curve definition with the market quotes adjusted by +/- 100bp
            // - PercentUp/Dn: the curve definition with the market quotes adjusted by +/- 1, 5, and 10%
            logger.LogDebug("Loading stress definitions...");
            var stressRules = new List<StressRule>();
            {
                //IExpression excludedCurveFilter =
                //    Expr.BoolOR(
                //        Expr.IsEQU(CurveProp.PricingStructureType, "RateBasisCurve"),
                //        Expr.IsEQU(CurveProp.PricingStructureType, "RateSpreadCurve"));
                //IExpression anyOtherCurveFilter =
                //    Expr.BoolAND(
                //        Expr.IsNEQ(CurveProp.PricingStructureType, "RateBasisCurve"),
                //        Expr.IsNEQ(CurveProp.PricingStructureType, "RateSpreadCurve"));

                // build rules
                stressRules.Add(MakeStressRule("ZeroStress", "1", 1, Expr.ALL, NoAdjustment()));
                stressRules.Add(MakeStressRule(StressConst.ParallelUp001, "1", 1, Expr.ALL, ParallelUp(0.0001M)));
                stressRules.Add(MakeStressRule(StressConst.ParallelDn001, "1", 1, Expr.ALL, ParallelDn(0.0001M)));
                stressRules.Add(MakeStressRule(StressConst.ParallelUp050, "1", 1, Expr.ALL, ParallelUp(0.0050M)));
                stressRules.Add(MakeStressRule(StressConst.ParallelDn050, "1", 1, Expr.ALL, ParallelDn(0.0050M)));
                stressRules.Add(MakeStressRule(StressConst.ParallelUp100, "1", 1, Expr.ALL, ParallelUp(0.0100M)));
                stressRules.Add(MakeStressRule(StressConst.ParallelDn100, "1", 1, Expr.ALL, ParallelDn(0.0100M)));

                stressRules.Add(MakeStressRule(StressConst.PercentUp01, "1", 1, Expr.ALL, PercentUp(0.01M)));
                stressRules.Add(MakeStressRule(StressConst.PercentDn01, "1", 1, Expr.ALL, PercentDn(0.01M)));
                stressRules.Add(MakeStressRule(StressConst.PercentUp05, "1", 1, Expr.ALL, PercentUp(0.05M)));
                stressRules.Add(MakeStressRule(StressConst.PercentDn05, "1", 1, Expr.ALL, PercentDn(0.05M)));
                stressRules.Add(MakeStressRule(StressConst.PercentUp10, "1", 1, Expr.ALL, PercentUp(0.10M)));
                stressRules.Add(MakeStressRule(StressConst.PercentDn10, "1", 1, Expr.ALL, PercentDn(0.10M)));
            }

            // save stress rules
            foreach (StressRule stressRule in stressRules)
            {
                const string itemType = "StressDefinition";
                string idSuffix = stressRule.StressId + "." + stressRule.RuleId;
                logger.LogDebug("  Loading {0} ...", idSuffix);
                Pair<string, NamedValueSet> itemInfo = LoadConfigDataHelper.StandardConfigProps(null, itemType, idSuffix, nameSpace);
                TimeSpan lifetime =  stressRule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(stressRule, itemInfo.First, itemInfo.Second, false, lifetime);
            }

            logger.LogDebug("Loaded {0} stress definitions", stressRules.Count);
        }
    }
}
