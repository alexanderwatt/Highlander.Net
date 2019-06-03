using System;

namespace Orion.Analytics.Counterparty
{
    ///// <summary>
    ///// Input class (for ROE calcs)
    ///// </summary>
    //public class TradeInfo : ITradeInfo
    //{
    //    public string Region { get; set; }
    //    public string BusinessUnit { get; set; }
    //    public string PrimaryCurrency { get; set; }
    //    public string SettleCurrency { get; set; }
    //    public string Product { get; set; } //eg "FXFWD"
    //    public string ProductCategory { get; set; }
    //    public string ProductSubCategory { get; set; }
    //    public decimal FaceValue { get; set; }
    //    public decimal Revenue { get; set; }
    //    public decimal Tenor { get; set; }
    //    public decimal Margin { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EvalDate { get; set; }
    //    public DateTime MaturityDate { get; set; }
    //    public DateTime SettlementDate { get; set; }
    //    public decimal MTM { get; set; }
    //    public decimal MaxPCE { get; set; }
    //    public decimal[] PCEProfile { get; set; }
    //    public string BoughtSold { get; set; }
    //    public string FRAPayment { get; set; }
    //    public string CashPhys { get; set; }
    //    public decimal iRevenue { get; set; }
    //    public string CounterpartyName { get; set; }
    //    public string CounterpartyType { get; set; }
    //    public string LendingCategory { get; set; }
    //    public int ECRSRating { get; set; }
    //    public bool IsLendingTrade { get; set; }
    //    public bool IsMarketTrade { get; set; }
    //    public string Basis { get; set; }
    //}

    /////<summary>
    /////</summary>
    //public enum RevenueDistribType
    //{
    //    ///<summary>
    //    ///</summary>
    //    NA, 
    //    ///<summary>
    //    ///</summary>
    //    FlatProfile, 
    //    ///<summary>
    //    ///</summary>
    //    FiftyPercentUpFront
    //}

    /////<summary>
    /////</summary>
    //public enum MTMProfileType
    //{
    //    ///<summary>
    //    ///</summary>
    //    NA, 
    //    ///<summary>
    //    ///</summary>
    //    Flat, 
    //    ///<summary>
    //    ///</summary>
    //    Increasing, 
    //    ///<summary>
    //    ///</summary>
    //    Decreasing
    //};

    ///// <summary>
    ///// Input class for ROE calcs
    ///// </summary>
    //public class ROEDerivedFields
    //{
    //    ///<summary>
    //    ///</summary>
    //    public decimal BasisConvert { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal B2LGDDefault { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal Confidence { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal Correlation { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal CostOfCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal DiscountRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal EquityTransferPrice { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal ExchangeRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal FrankingRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal LendVariable { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal? LGDDefault { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal LiquidityCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal MarketRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal OpRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal PreTaxHurdleRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal? ProtectionB2Lgd { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal ProtectionExchangeRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal? ProtectionLgd { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal RegionExchangeRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal RenewalPercent { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal Reversion { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public bool Revolving { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal RiskCapitalScale { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal RxmCi { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal SecuritisationMultiplier { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int TabRows { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal TargetEquityRatio { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal TargetLiquidityRatio { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal TaxRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string TxnProduct { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal IntRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal IrVol { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal FxVol { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public RevenueDistribType RevType { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal VariableCost { get; set; } 
    //}

    ///// <summary>
    ///// Principle Profile input class for ROE calcs
    ///// 
    ///// Principal Profile (cashflow) was added as a function to allow for transactions that
    ///// have fixed amortisation schedules where ‘average utilisation’ is not a good enough indicator
    ///// 
    ///// </summary>
    //public class ROECashflow
    //{
    //    ///<summary>
    //    ///</summary>
    //    public int Period { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal PointCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal PointRevenue { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal Principal { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public decimal TaxBenefit { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public DateTime PointDate { get; set; }
    //}

    ///// <summary>
    ///// Output class for ROE calcs
    ///// </summary>
    //public class DerivResultFields
    //{
    //    ///<summary>
    //    ///</summary>
    //    public double Limit { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NpvStatProvision { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NpvRevenue { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double TotalRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NpvEva { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RAROCRenamed { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public bool ErrorFlag { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RegEquity { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ROE { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double CreditRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double CumulativeEdf { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int Maxmonths { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int Totalmonths { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Nopat { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ReqMargin { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2Nopat { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2Crc { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2Trc { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2ROE { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2CreditRwa { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2OpRiskRwa { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2TotalRwa { get; set; }
    //}

    ///// <summary>
    ///// Output class for ROE calcs
    ///// 
    ///// Defines a row of the calculation result profile
    ///// </summary>
    //public class DerivCalcProfile
    //{
    //    ///<summary>
    //    ///</summary>
    //    public int PeriodNumber { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double PrincipalProfile { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public DateTime PeriodEndDate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RiskCapitalProfile { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RevenueProfile { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double CostProfile { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RORC { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ExpectedLossProfile { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double AfterTaxfactor { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RequiredReturn { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double PerperiodEVA { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NpvEva { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NpvRevenue { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NPVRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double NPVRegEquity { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RORE { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Nopat { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RegEquity { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double UnDrawnAmt { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Adjppl { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2NPVRegEquity { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2RORE { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2RegEquity { get; set; }
    //}

    ///// <summary>
    ///// Credit Protection Input
    ///// 
    ///// Capital constraints placed upon us by Group Treasury has resulted in using bought credit
    ///// protection as a means of managing Capital consumption, hence functionality that allowed 
    ///// this product to be recognised within the Pricing Model was required
    ///// 
    ///// The Bought Credit Protection details are provided by nabCapital Portfolio Management at 
    ///// inception and quarterly updates are provided to monitor the hedges
    ///// 
    ///// It is assumed that the Credit Protection starts and ends at the same time as the underlying
    ///// facility being hedged
    ///// 
    ///// 
    ///// </summary>
    //public class CreditPInput
    //{
    //    ///<summary>
    //    ///</summary>
    //    public string BoughtProtection { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string CreditHedge { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ProtectionAmount { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ProtectionCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string ProtectionCcy { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ProtectionLGD { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double ProtectionB2LGD { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string ProtectionRating { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string ProtectionRatAgncy { get; set; }
    //}

    /////<summary>
    /////</summary>
    //public class RoeInput
    //{
    //    ///<summary>
    //    ///</summary>
    //    public string Counterparty { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string CommitmentCd { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Revolving { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double TargetEqRatio { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string BankGuar { get; set; }
    //}

    /// <summary>
    /// This is used to specify the start date of the transaction, the
    /// created date (may be earlier than start date for forward starting), term etc
    /// 
    /// EndDate = Utilities.DateUtility.getEndDate(StartDate, trade.Tenor) OR maturity
    /// MaxMonths = Utilities.DateUtility.DifferenceInMonths(StartDate, EndDate)
    /// TotalMonths = Utilities.DateUtility.DifferenceInMonths(CreateDate, EndDate)
    /// TotalMonths = Length of forward Start(Delaymonths) plus term of Trade(Maxmonths) 
    /// 
    /// </summary>
    public class DateMonths
    {
        ///<summary>
        ///</summary>
        public DateTime StartDate { get; set; } //transaction start date?
        ///<summary>
        ///</summary>
        public DateTime CreatedDate { get; set; } //eval date?
        ///<summary>
        ///</summary>
        public int Maxmonths { get; set; }
        ///<summary>
        ///</summary>
        public int DelayMonths { get; set; }
        ///<summary>
        ///</summary>
        public int Totalmonths { get; set; }
    }

    ///// <summary>
    ///// Input class for ROE calcs
    ///// 
    ///// Summary of input fields:
    ///// 
    ///// CalcType:	Switch: Normal Calc = CALC, Required Margin = TARGET
    ///// BasisConvert:	Scaling factor - converts to ACT/365; 1 = already 365 otherwise 365/360?
    ///// BoughtSold:	Used for options - indicates bought/sold option
    ///// CapWeighting:	Risk Capital Scaling Factor - used to calculate Credit Risk Capital - fixed GV
    ///// CashPhys:	Mainly for swaptions - cash or physical settled - physical - value swaption & swap
    ///// Ccy1:	Main ccy of transaction
    ///// CoC:	Cost of Capital = 11.5% currently (fixed GV)
    ///// Concentration:	Scaling factor relating to concentration or diversification benefits - 1 default
    ///// Confidence:	No. of std devs required by RC calculation to ensure we meet our ratings - fixed GV
    ///// Correlation:	Correlation of deal with overall portfolio - fixed GV
    ///// Disc:	Monthly Discount Factor - (1+Drate)^(-1/12)
    ///// Drate:	Market Discount Rate
    ///// EquityXfer:	RORC - fixed GV
    ///// FeeLimit:	Fee calculated of a limit = "facility fee" + "line service fee" + "commitment fee"
    ///// FeeUndrawn:	Undrawn commitment fee
    ///// FeeUsage:	Fee based on drawn down amount of loan = "activation fee" + "issuing fee" - FTP GV
    ///// FixedCost:	Upfront fixed cost related to transaction - fixed GV*"a percentage" dependent on N/R
    ///// FRAPayment:	In arrears or in advance - not lending
    ///// FxCorr:	Correlation required for cross-currency swaps
    ///// FxVol:	Volatility required for expected exposure of FX products
    ///// Hurdle:	Hurdle rate currently 14.5% after tax
    ///// IntRate:
    ///// iRevenue:	Gross revenue (treasury products) in $ or points
    ///// IrVol:	Same as FXVol but for IR products
    ///// Limit:	Limit of deal
    ///// LiquidityCost:	Cost of holding liquid assets - fixed GV
    ///// LLied:	Loss in Event of Default - linked to "Region" and "Lending Category" - fixed GV
    ///// Margin:	margin over reference rate - lending
    ///// Maturity:	maturity of deal - mths or years - not linked to lending
    ///// MaxMonths:	months to maturity
    ///// MktRiskCapital:	RC for market risk - derivatives only
    ///// MRCF:	Flag (0 or 1) used for telling whether CRC required for transaction
    ///// nStates:	no. of rows in cdp matrix - no. of ratings
    ///// nYears:	no. of columns in cdp matrix - currently fixed at 25 years
    ///// OpRiskCapital:	Operational Risk Capital - fixed percentage GV
    ///// PpdCost:	Monthly Cost - derivatives
    ///// Product:	="LOAN" for lending products
    ///// ProductType:	Number version of product - 1 for lending, 2 for deposits, 3 for markets - fixed value
    ///// Ptype:	Profile Type - can either be "LOAN" or "SWAP" or NA
    ///// Rating:	CRS, S&P or Moody's actual rating of company
    ///// Rating_Agency:	N = NAB, M = Moody's, S = S&P's
    ///// RevenueType:	Coming through as $ or points or %
    ///// Reversion:	Mean reversion for derivative exposure fixed GV
    ///// RevTreat:	Revenue Treatment (in arrears, in advance, at maturity) - not lending
    ///// RxmCI:	RXM Confidence Interval - fixed global variable
    ///// TabRows:	Linked to "Edit Profile" button; no. of rows added to profile
    ///// TaxEffect:	After tax factor
    ///// Tenor:	Tenor of underlying - expressed in days
    ///// Tim:	Returns the life of the product for average calculations
    ///// TLIED:	Treasury loss in event of default
    ///// TxnProduct:	= Product value above
    ///// UfCcy:	Other Upfront fees ($)
    ///// UfPct:	Application/Establishment Fee (%)
    ///// Utilisation:	Utilisation of deal (-1 got a profile - ignore)
    ///// VariableCost:	% of monthly revenue - monthly cost for loans - fixed GV
    ///// PointFS:	Columns of input profile - Revenue, costs, tax benefit
    ///// </summary>
    //public class DerivROEInput
    //{
    //    ///<summary>
    //    ///</summary>
    //    public DerivROEInput()
    //    {
    //        RoeData = new RoeInput();
    //        CreditProtection = new CreditPInput();
    //        Months = new DateMonths();
    //        UseProvidedPCE = false;
    //    }

    //    ///<summary>
    //    ///</summary>
    //    public string CalcType { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double BasisConvert { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Boughtsold { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double CapWeighting { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string CashPhys { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string CCy1 { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double CoC { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Concentration { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Confidence { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Correlation { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Drate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double EquityXfer { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Feelimit { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double FeeUndrawn { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double FeeUsage { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double FixedCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string FRAPayment { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double FxCorr { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double FxVol { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Hurdle { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double IntRate { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double IRevenue { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double IrVol { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Limit { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double LiquidityCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Lied { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double B2LGD { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Margin { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Maturity { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double MktRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int MRCF { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double MTM { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double OpRiskCapital { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double PpdCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Product { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int ProductType { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Ptype { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Rating { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string RatingAgency { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string RevenueType { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Reversion { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string RevTreat { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RxmCI { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public int TabRows { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Taxeff { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Tenor { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double Tim { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string TxnProduct { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double UfCcy { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double UfPct { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public string Utilisation { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double VariableCost { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double[,] PointFs { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double[] Ppl { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double[] CPFlow { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public bool AuditDump { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public bool Production { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public DateMonths Months { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public RoeInput RoeData { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public CreditPInput CreditProtection { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double SecuritisationMultiplier { get; set; }
    //    //the following have been added (DB)
    //    ///<summary>
    //    ///</summary>
    //    public bool UseProvidedPCE { get; set; }
    //    ///<summary>
    //    ///</summary>
    //    public double RxmPCE { get; set; } //max pce
    //    ///<summary>
    //    ///</summary>
    //    public double[] PceProfile { get; set; } //pce profile
    //}
}