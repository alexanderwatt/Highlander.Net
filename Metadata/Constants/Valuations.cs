/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Orion.Constants
{
    ///<summary>
    ///</summary>
    public enum ValuationReportType
    {
        ///<summary>
        ///</summary>
        Summary,
        ///<summary>
        ///</summary>
        Full
    }

    public class ValueProp
    {
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string RealTimePricing = "RealTimePricing";
        public const string Metrics = "Metrics";
        public const string MarketName = "MarketName";
        public const string ReportingCurrency = "ReportingCurrency";
        public const string Aggregation = "Aggregation";
        public const string IrScenario = "IrScenario";
        public const string FxScenario = "FxScenario";
        public const string Scenario = "Scenario";
        public const string RequestId = "RequestId";
        public const string Requester = "Requester";
        public const string PortfolioId = "PortfolioId";
        public const string Status = "Status";
        public const string OwnerId = "OwnerId";
        public const string BaseDate = "BaseDate";
        public const string BaseParty = "BaseParty";
        public const string CalculationDateTime = "CalculationDateTime";
        // extra metrics
        public const string BreakEvenRate = "BreakEvenRate";
        public const string TradeCount = "TradeCount";
        public const string ErrorCount = "ErrorCount";
        public static string[] ExtraMetrics => new[] { BreakEvenRate, TradeCount, ErrorCount };
        public const int BreakEvenRateOffset = 0;
        public const int TradeCountOffset = 1;
        public const int ErrorCountOffset = 2;
    }

    public class ValueConst
    {
        public const string SumValue = "(sum)";
        public const string AggTypeSummary = "Sum";
        public const string AggTypeAverage = "Avg";
        public const string AggTypeStdDev = "StD";
    }

    public class StressConst
    {
        public const string ZeroStress = "ZeroStress";
        public const string ParallelUp001 = "ParallelUp1bp";
        public const string ParallelDn001 = "ParallelDn1bp";
        public const string ParallelUp050 = "ParallelUp50bp";
        public const string ParallelDn050 = "ParallelDn50bp";
        public const string ParallelUp100 = "ParallelUp100bp";
        public const string ParallelDn100 = "ParallelDn100bp";
        public const string PercentUp01 = "PercentUp1";
        public const string PercentDn01 = "PercentDn1";
        public const string PercentUp05 = "PercentUp5";
        public const string PercentDn05 = "PercentDn5";
        public const string PercentUp10 = "PercentUp10";
        public const string PercentDn10 = "PercentDn10";
        public static string[] AllStressIds => new[] {
            ZeroStress,
            ParallelUp001,
            ParallelDn001,
            ParallelUp050,
            ParallelDn050,
            ParallelUp100,
            ParallelDn100,
            PercentUp01,
            PercentDn01,
            PercentUp05,
            PercentDn05,
            PercentUp10,
            PercentDn10
        };
    }
    public class ScenarioConst
    {
        public const string FxPrefix = "Fx";
        public const string IrPrefix = "Ir";
        // FX
        public const string Unstressed = "Unstressed";
        // AUD
        public const string AUD_Up01pc = "AUD_Up01pc";
        public const string AUD_Up05pc = "AUD_Up05pc";
        public const string AUD_Up10pc = "AUD_Up10pc";
        public const string AUD_Dn01pc = "AUD_Dn01pc";
        public const string AUD_Dn05pc = "AUD_Dn05pc";
        public const string AUD_Dn10pc = "AUD_Dn10pc";
        // NZD
        public const string NZD_Up01pc = "NZD_Up01pc";
        public const string NZD_Up05pc = "NZD_Up05pc";
        public const string NZD_Up10pc = "NZD_Up10pc";
        public const string NZD_Dn01pc = "NZD_Dn01pc";
        public const string NZD_Dn05pc = "NZD_Dn05pc";
        public const string NZD_Dn10pc = "NZD_Dn10pc";
        // GBP
        public const string GBP_Up01pc = "GBP_Up01pc";
        public const string GBP_Up05pc = "GBP_Up05pc";
        public const string GBP_Up10pc = "GBP_Up10pc";
        public const string GBP_Dn01pc = "GBP_Dn01pc";
        public const string GBP_Dn05pc = "GBP_Dn05pc";
        public const string GBP_Dn10pc = "GBP_Dn10pc";
        // EUR
        public const string EUR_Up01pc = "EUR_Up01pc";
        public const string EUR_Up05pc = "EUR_Up05pc";
        public const string EUR_Up10pc = "EUR_Up10pc";
        public const string EUR_Dn01pc = "EUR_Dn01pc";
        public const string EUR_Dn05pc = "EUR_Dn05pc";
        public const string EUR_Dn10pc = "EUR_Dn10pc";
        // JPY
        public const string JPY_Up01pc = "JPY_Up01pc";
        public const string JPY_Up05pc = "JPY_Up05pc";
        public const string JPY_Up10pc = "JPY_Up10pc";
        public const string JPY_Dn01pc = "JPY_Dn01pc";
        public const string JPY_Dn05pc = "JPY_Dn05pc";
        public const string JPY_Dn10pc = "JPY_Dn10pc";

        // IR
        //public const string CurrentIR = "CurrentIR";
        public const string GlobalIRUp001bp = "GlobalIRUp001bp";
        public const string GlobalIRUp050bp = "GlobalIRUp050bp";
        public const string GlobalIRUp100bp = "GlobalIRUp100bp";
        public const string GlobalIRDn001bp = "GlobalIRDn001bp";
        public const string GlobalIRDn050bp = "GlobalIRDn050bp";
        public const string GlobalIRDn100bp = "GlobalIRDn100bp";
        public const string AUDBBSWUp001bp = "AUDBBSWUp001bp";
        public const string AUDBBSWUp050bp = "AUDBBSWUp050bp";
        public const string AUDBBSWUp100bp = "AUDBBSWUp100bp";
        public const string AUDBBSWDn001bp = "AUDBBSWDn001bp";
        public const string AUDBBSWDn050bp = "AUDBBSWDn050bp";
        public const string AUDBBSWDn100bp = "AUDBBSWDn100bp";
        public const string USDLIBORUp001bp = "USDLIBORUp001bp";
        public const string USDLIBORUp050bp = "USDLIBORUp050bp";
        public const string USDLIBORUp100bp = "USDLIBORUp100bp";
        public const string USDLIBORDn001bp = "USDLIBORDn001bp";
        public const string USDLIBORDn050bp = "USDLIBORDn050bp";
        public const string USDLIBORDn100bp = "USDLIBORDn100bp";
        public const string GBPLIBORUp001bp = "GBPLIBORUp001bp";
        public const string GBPLIBORUp050bp = "GBPLIBORUp050bp";
        public const string GBPLIBORUp100bp = "GBPLIBORUp100bp";
        public const string GBPLIBORDn001bp = "GBPLIBORDn001bp";
        public const string GBPLIBORDn050bp = "GBPLIBORDn050bp";
        public const string GBPLIBORDn100bp = "GBPLIBORDn100bp";

        public static string ScenarioId(string irScenario, string fxScenario)
        {
            return $"{IrPrefix}{irScenario ?? Unstressed}_{FxPrefix}{fxScenario ?? Unstressed}";
        }

        public static string[] AllIrScenarioIds => new[] {
            GlobalIRUp001bp,
            GlobalIRUp050bp,
            GlobalIRUp100bp,
            AUDBBSWUp001bp,
            AUDBBSWUp050bp,
            AUDBBSWUp100bp,
            AUDBBSWDn050bp,
            USDLIBORUp001bp,
            USDLIBORUp050bp,
            USDLIBORUp100bp,
            GBPLIBORUp001bp,
            GBPLIBORUp050bp,
            GBPLIBORUp100bp
        };

        public static string[] AllFxScenarioIds => new[] {
            AUD_Up01pc,
            AUD_Up05pc,
            AUD_Up10pc,
            AUD_Dn01pc,
            AUD_Dn05pc,
            AUD_Dn10pc,
            GBP_Up05pc,
            GBP_Dn05pc,
            EUR_Up05pc,
            EUR_Dn05pc,
            JPY_Up05pc,
            JPY_Dn05pc,
            NZD_Up05pc,
            NZD_Dn05pc
        };
    }
}
