using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

using National.QRSC.AnalyticModels.CreditMetrics;
using National.QRSC.Business.Helpers;

using NUnit.Framework;


using nabCap.QR.Schemas.FpML;


namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class ExcelClientIntegrationTest
    {
        IDictionary<StreamFields, List<object>> _PayLegStreamData;
        IDictionary<StreamFields, List<object>> _ReceiveLegStreamData;
        string _excelDiagnosticsFilePath = "ExcelSheetDiagnostics.txt";
        string _excelInputsFilePath = "ExcelSheetInputs.txt";
        string _excelPayStreamFieldsFilePath = "ClientPayStreamFields.txt";
        string _excelReceiveStreamFieldsFilePath = "ClientReceiveStreamFields.txt";
        string _argContainerResultsSerializedForm;
        string _argContainerInputsSerializedForm;
        
        [SetUp]
        public void Initialisation()
        {
            _argContainerResultsSerializedForm = LoadXMLFileToString(_excelDiagnosticsFilePath);
            _argContainerInputsSerializedForm = LoadXMLFileToString(_excelInputsFilePath);
        }

        ///// <summary>
        ///// Note: The data in the "ClientPayStreamFields.txt" is a cut and paste
        ///// of the data contained between the "<StreamDataSet>" tags under the
        ///// "PAY.STREAM.FIELDS" section of diagnostics data in the "Diagnostics" tab
        ///// column C of the spreedsheet 
        ///// with the following modifications:
        ///// (1) Namespace reference in "<StreamDataSet>" removed
        ///// 
        ///// Note: The data in the "ClientReceiveStreamFields.txt" is a cut and paste
        ///// of the data contained between the "<StreamDataSet>" tags under the
        ///// "RECEIVE.STREAM.FIELDS" section of diagnostics data in the "Diagnostics" tab
        ///// column C of the spreedsheet 
        ///// with the following modifications:
        ///// (1) Namespace reference in "<StreamDataSet>" removed
        ///// 
        ///// Note: The data in the "ExcelSheetInputs.txt" file is a cut and
        /////  paste of the data held in the "Diagnostics" tab column A with the following
        /////  modifications made:
        /////  (1) Add: missing "<itemlist>" at top.
        /////  
        /////  Note: The data in the "ExcelSheetDiagnostics.txt" file is a cut and
        /////  paster of the data held in the "Diagnostics" tab column C with the following
        /////  modifications made:
        /////  (1) replace 
        /////  "<soap:Body><QRUResponse xmlns="http://nabCap.com.au/"><QRUResult>"
        /////  with "<itemlist>"
        /////  (2) replace all occurances of: xmlns=""
        /////      with: xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
        ///// </summary>
        //[Test]
        //public void TestROEFromClientCall()
        //{
        //    //(SETUP) load data section
        //    DateTime evalDate = DateTime.Now; //set the eval date (should match date of diagnostics file data)
        //    //now deserialize the diagnostics data from the excel client into an argument container
        //    ArgumentContainer<string, object> result = ArgumentSerializer.Deserialize(_argContainerResultsSerializedForm);

        //    ArgumentContainer<string, object> inputs = ArgumentSerializer.Deserialize(_argContainerInputsSerializedForm);

        //    National.QRSC.AnalyticModels.Tests.Models.Credit.StreamDataSet payStreamData =
        //   XmlSerializerHelper.DeserializeFromFile<National.QRSC.AnalyticModels.Tests.Models.Credit.StreamDataSet>(_excelPayStreamFieldsFilePath);

        //    National.QRSC.AnalyticModels.Tests.Models.Credit.StreamDataSet receiveStreamData =
        //     XmlSerializerHelper.DeserializeFromFile<National.QRSC.AnalyticModels.Tests.Models.Credit.StreamDataSet>(_excelReceiveStreamFieldsFilePath);
            
        //    //setup pay leg data
        //    _PayLegStreamData = new SortedDictionary<StreamFields, List<object>>();
        //    _PayLegStreamData.Add(StreamFields.PaymentDate,
        //                       ConvertToList<DateTime>(payStreamData.PaymentDate));
        //    _PayLegStreamData.Add(StreamFields.Notional,
        //                            ConvertToList<decimal>(payStreamData.Notional));
        //    _PayLegStreamData.Add(StreamFields.AdjustedStartDate,
        //                            ConvertToList<DateTime>(payStreamData.AdjustedStartDate));
        //    _PayLegStreamData.Add(StreamFields.AdjustedEndDate,
        //                            ConvertToList<DateTime>(payStreamData.AdjustedEndDate));
        //    _PayLegStreamData.Add(StreamFields.CouponYearFraction,
        //                            ConvertToList<decimal>(payStreamData.CouponYearFraction));
        //    _PayLegStreamData.Add(StreamFields.DayCountConvention,
        //                        ConvertToList<string>(payStreamData.DayCountConvention));
        //    _PayLegStreamData.Add(StreamFields.DiscountingType,
        //                            ConvertToList<string>(payStreamData.DiscountingType));
        //    _PayLegStreamData.Add(StreamFields.Currency,
        //                            ConvertToList<string>(payStreamData.Currency));
        //    _PayLegStreamData.Add(StreamFields.DateAdjustmentConvention,
        //                            ConvertToList<string>(payStreamData.DateAdjustmentConvention));
            
        //    if (payStreamData.StreamType.Equals("FLOATING"))
        //    {
        //    _PayLegStreamData.Add(StreamFields.RateObservationSpecified,
        //                        ConvertToList<Boolean>(payStreamData.RateObservationSpecified));
        //    _PayLegStreamData.Add(StreamFields.ObservedRate,
        //                            ConvertToList<decimal>(payStreamData.ObservedRate));
        //    _PayLegStreamData.Add(StreamFields.ResetDate,
        //                            ConvertToList<DateTime>(payStreamData.ResetDate));
        //    _PayLegStreamData.Add(StreamFields.Margin,
        //                        ConvertToList<decimal>(payStreamData.Margin));
        //    _PayLegStreamData.Add(StreamFields.Rate,
        //                        ConvertToList<decimal>(payStreamData.Rate));
        //    _PayLegStreamData.Add(StreamFields.RateIndexName,
        //                        ConvertToList<string>(payStreamData.RateIndexName));
        //    }
        //    //setup receive leg data
        //    _ReceiveLegStreamData = new SortedDictionary<StreamFields, List<object>>();
        //    _ReceiveLegStreamData.Add(StreamFields.PaymentDate,
        //                           ConvertToList<DateTime>(receiveStreamData.PaymentDate));
        //    _ReceiveLegStreamData.Add(StreamFields.Rate,
        //                        ConvertToList<decimal>(receiveStreamData.Rate));
        //    _ReceiveLegStreamData.Add(StreamFields.Notional,
        //                        ConvertToList<decimal>(receiveStreamData.Notional));
        //    _ReceiveLegStreamData.Add(StreamFields.AdjustedStartDate,
        //                       ConvertToList<DateTime>(receiveStreamData.AdjustedStartDate));
        //    _ReceiveLegStreamData.Add(StreamFields.AdjustedEndDate,
        //                       ConvertToList<DateTime>(receiveStreamData.AdjustedEndDate));
        //    _ReceiveLegStreamData.Add(StreamFields.CouponYearFraction,
        //                       ConvertToList<decimal>(receiveStreamData.CouponYearFraction));
        //    _ReceiveLegStreamData.Add(StreamFields.DayCountConvention,
        //                       ConvertToList<string>(receiveStreamData.DayCountConvention));
        //    _ReceiveLegStreamData.Add(StreamFields.DiscountingType,
        //                       ConvertToList<string>(receiveStreamData.DiscountingType));
        //    _ReceiveLegStreamData.Add(StreamFields.Currency,
        //                       ConvertToList<string>(receiveStreamData.Currency));
        //    _ReceiveLegStreamData.Add(StreamFields.DateAdjustmentConvention,
        //                       ConvertToList<string>(receiveStreamData.DateAdjustmentConvention));

        //    if (receiveStreamData.StreamType.Equals("FLOATING"))
        //    {
        //        _ReceiveLegStreamData.Add(StreamFields.RateObservationSpecified,
        //                            ConvertToList<Boolean>(receiveStreamData.RateObservationSpecified));
        //        _ReceiveLegStreamData.Add(StreamFields.ObservedRate,
        //                                ConvertToList<decimal>(receiveStreamData.ObservedRate));
        //        _ReceiveLegStreamData.Add(StreamFields.ResetDate,
        //                                ConvertToList<DateTime>(receiveStreamData.ResetDate));
        //        _ReceiveLegStreamData.Add(StreamFields.Margin,
        //                            ConvertToList<decimal>(receiveStreamData.Margin));
        //        _ReceiveLegStreamData.Add(StreamFields.Rate,
        //                            ConvertToList<decimal>(receiveStreamData.Rate));
        //        _ReceiveLegStreamData.Add(StreamFields.RateIndexName,
        //                            ConvertToList<string>(receiveStreamData.RateIndexName));
        //    }
        //    //(A) work out required time buckets
        //    int[] timeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };

        //    //get the PayTerms->CouponPeriod
        //    object[] objContainer = (object[]) inputs["PayTerms"];
        //    object[] objFields = (object[]) objContainer[1];
        //    string timeBucketTenor = (string)objFields[5];
        //    decimal notional = Convert.ToDecimal((double)objFields[1]);
        //    string ccy = (string)objFields[2]; //this is the base party ccy taken as deal base ccy

        //    decimal fxXchangeBaseUSD = 0.65M;

        //    objContainer = (object[])inputs["SwapTerms"];
        //    objFields = (object[])objContainer[1];
        //    DateTime effectiveDate = (DateTime)objFields[1];
        //    DateTime terminationDate = (DateTime)objFields[2];

        //    objContainer = (object[])inputs["RegulatoryCapital"];
        //    objFields = (object[])objContainer[1];    
        //    string capitalType = (string)objFields[0];
        //    string transactionProductType = (string)objFields[1];
        //    string regCapCounterpartyType = (string)objFields[2];

        //    objContainer = (object[])inputs["Counterparty"];
        //    objFields = (object[])objContainer[1];
        //    string lgdCounterpartyType = (string)objFields[0];
        //    string lendingCategory = (string)objFields[1];
        //    int counterpartyRatingID = Convert.ToInt32((string)objFields[2]);

        //    objContainer = (object[])inputs["BoundaryRider"];
        //    objFields = (object[])objContainer[1];
        //    string region = (string)objFields[2];
        //    ProductName brProductName = (ProductName) Enum.Parse(typeof(ProductName),(string)objFields[1],true);
        //    string brCcy = (string)objFields[5];

        //    //setup timebuckets for EE,StatProv and ROE calls
        //    ROEParameters parameters = new ROEParameters();
        //    parameters.BaseCalculationDate = DateTime.Today;
        //    parameters.CounterpartyRatingID = counterpartyRatingID; //Note: must be eCRS 1-23, 98, 99
        //    parameters.LendingCategory = lendingCategory; //LGD lending category
        //    parameters.LGDCounterpartyType = lgdCounterpartyType; //LGD counterparty type
        //    parameters.RegCapCounterpartyType = regCapCounterpartyType; //used to determine risk weights in reg cap calc (Basel I only) 
        //    parameters.Region = region;
        //    parameters.CapitalType = capitalType;
        //    parameters.Notional = notional;
        //    parameters.Margin = 0.0m;
        //    parameters.TransactionStartDate = effectiveDate;
        //    parameters.TransactionMaturityDate = terminationDate;
        //    parameters.TransactionProductType = transactionProductType;
        //    parameters.TransactionCurrency = ccy;
        //    parameters.FrequencyOfFuturePoints = 3; /* used as multiplier for future point set in final ROE calc */
        //    parameters.DayCountConvention = "ACT365";
        //    parameters.TraceMode = false;

        //    DateTime[] timeBucketDates = { };
        //    timeBuckets = GetDayIntervalOffsets(parameters.TransactionStartDate, //the start date for the timebuckets
        //                                        parameters.TransactionMaturityDate, //the maturity date
        //                                        timeBucketTenor, //string tenor info
        //                                        out timeBucketDates);

        //    parameters.TimeBuckets = timeBuckets;

        //    //(B) get NPVs and DFs
        //    DateTime[] payStreamPaymentDates = (DateTime[])result["PAY.STREAM.FIELDS.PAYMENTDATE"];
        //    Decimal[] payStreamNPVs = (Decimal[])result["PAY.STREAM.FIELDS.NPV"];
        //    Decimal[] payStreamExpectedValues = (Decimal[])result["PAY.STREAM.FIELDS.EXPECTEDVALUE"];
        //    Decimal[] payStreamDFs = (Decimal[])result["PAY.STREAM.FIELDS.PAYMENTDISCOUNTFACTOR"];
        //    DateTime[] recStreamPaymentDates = (DateTime[])result["RECEIVE.STREAM.FIELDS.PAYMENTDATE"];
        //    Decimal[] recStreamNPVs = (Decimal[])result["RECEIVE.STREAM.FIELDS.NPV"];
        //    Decimal[] recStreamExpectedValues = (Decimal[])result["RECEIVE.STREAM.FIELDS.EXPECTEDVALUE"];
        //    Decimal[] recStreamDFs = (Decimal[])result["RECEIVE.STREAM.FIELDS.PAYMENTDISCOUNTFACTOR"];

        //    //net the NPVs and DFs
        //    DateTime[] combinedPaymentDates = CombinePaymentDates(payStreamPaymentDates,recStreamPaymentDates);
        //    Decimal[] nettedExpectedValue = NetLegAmounts(combinedPaymentDates, payStreamPaymentDates, payStreamExpectedValues, recStreamPaymentDates, recStreamExpectedValues);
        //    //TODO: convert nettedExpectedValue to base ccy

        //    //get DFs for timeBucketDates
        //    IDictionary<int,decimal> curve = GetDFCurve(payStreamPaymentDates, payStreamDFs, parameters.TransactionStartDate);
        //    Decimal[] dfs = GetDiscountFactors(timeBuckets, curve);

        //    //(C) Make EE call
        //    EEMetrics[] metrics = new EEMetrics[] { EEMetrics.EE, EEMetrics.EE_SQ, EEMetrics.MTM, EEMetrics.Term, EEMetrics.IsNode };
        //    IEEParameters eeInput = new EEParameters();
        //    eeInput.PayLegStreamData = _PayLegStreamData;
        //    eeInput.ReceiveLegStreamData = _ReceiveLegStreamData;
        //    eeInput.PayLegStreamFixed = false; //need to set which is floating (PAY=FLOAT)
        //    eeInput.ReceiveLegStreamFixed = true; //need to set which is fixed (RECEIVE=FIXED)
        //    eeInput.SimulationMethod = SimulationMethod.RiderSim;
        //    eeInput.CalculationTimeBuckets = timeBuckets;
        //    eeInput.BoundaryRiderProductName = brProductName;
        //    eeInput.BasePartyPays = true;
        //    eeInput.EvaluationDate = evalDate;
        //    eeInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
        //    eeInput.CalculationBaseCurrency = brCcy;
        //    EEAnalytic eeCall = new EEAnalytic();
        //    IEEResult eeResult = new EEResult();
        //    eeResult = eeCall.Calculate<IEEResult, EEResult>(eeInput, metrics);
        //    List<decimal> eeList = new List<decimal>();
        //    List<decimal> mtmList = new List<decimal>();
        //    for (int i = 0; i < eeResult.IsNode.Length; i++)
        //    {
        //        //use the isNode boolean value to determine if the node matches the timebucket node from input
        //        if (eeResult.IsNode[i] == true)
        //        {
        //            eeList.Add(eeResult.EE[i]);
        //            mtmList.Add(eeResult.MTM[i]);
        //        }
        //    }         

        //    //(D) Calculate Stat Prov
        //    StatProvMetrics[] spMetrics = new StatProvMetrics[] { StatProvMetrics.StatProv };
        //    IStatProvParameters statprovInput = new StatProvParameters();
        //    statprovInput.timeBuckets = timeBuckets;
        //    statprovInput.discountFactors = CMF.ROECalculator.GetCostOfCapitalDFs(timeBuckets,
        //                                            0.11M); //based on 11% pa cost capital
        //    statprovInput.epe = eeList.ToArray(); //EE vector
        //    statprovInput.FrequencyOfFuturePoints = parameters.FrequencyOfFuturePoints; 
        //    statprovInput.CounterpartyRatingID = parameters.CounterpartyRatingID;
        //    statprovInput.LendingCategory = parameters.LendingCategory;
        //    statprovInput.LGDCounterpartyType = parameters.LGDCounterpartyType;
        //    StatProvAnalytic statprovCall = new StatProvAnalytic();
        //    IStatProvResult spResult = new StatProvResult();
        //    spResult = statprovCall.Calculate<IStatProvResult, StatProvResult>(statprovInput, spMetrics);
        //    parameters.StatisticalProvisions = spResult.StatProv;
        //    parameters.FXExchRateStatProvCcyToTransCcy = fxXchangeBaseUSD;

        //    //(E) Calculate ROE
        //    ROEMetrics[] modelMetrics = new[] { ROEMetrics.ROE }; // specify metrics to calculate
           
        //    var cashflowDates = new List<DateTime>();
        //    foreach (var dayOffset in parameters.TimeBuckets)
        //    {
        //        cashflowDates.Add(parameters.TransactionStartDate.AddDays(dayOffset));
        //    }
        //    parameters.CashflowDates = combinedPaymentDates;
        //    parameters.CashflowAmounts = nettedExpectedValue;
        //    parameters.Costs = CreateEmptyArray(timeBuckets);
        //    parameters.DiscountFactors = dfs;
        //    parameters.NPVs = mtmList.ToArray(); //use EE mtms
        //    var model = new ROEAnalytic();
        //    var modelResult = model.Calculate<IROEResult, ROEResult>(parameters, modelMetrics);

        //    Assert.IsTrue(true);
        //}

        //get dfs
        static private Decimal[] GetDiscountFactors(int[] timeBuckets, IDictionary<int,decimal> curve)
        {
            Decimal[] result = new Decimal[timeBuckets.Length];
            for (int i = 0; i < timeBuckets.Length; i++)
            {
                result[i] = LinearInterpolation(timeBuckets[i] ,curve);
            }
            return result;
        }

        //work out df curve
        static private IDictionary<int, decimal> GetDFCurve(DateTime[] dfDates, Decimal[] dfs, DateTime startDate)
        {
            IDictionary<int, decimal> result = new SortedDictionary<int, decimal>();

            for(int i=0; i < dfDates.Length; i++)
            {
                DateTime date = dfDates[i];
                TimeSpan ts = date - startDate;
                int offset = ts.Days;
                result.Add(offset, dfs[i]);
            }
            return result;
        }

        //combine dates
        static private DateTime[] CombinePaymentDates(DateTime[] pay, DateTime[] receive)
        {
            List<DateTime> dateList = new List<DateTime>();
            int maxLength = System.Math.Max(pay.Length, receive.Length); 

            for (int i = 0; i < maxLength; i++)
            {
                if ((i < pay.Length) && (i < receive.Length))
                {
                    if (pay[i] == receive[i]) dateList.Add(pay[i]);
                    else
                    {
                        dateList.Add(pay[i]);
                        dateList.Add(receive[i]);
                    }
                }
                else {
                    if (i < pay.Length) dateList.Add(pay[i]);
                    if (i < receive.Length) dateList.Add(receive[i]);
                }
            }
            return dateList.ToArray();
        }

        //used to net the amts on the swap legs
        static private Decimal[] NetLegAmounts(DateTime[] paymentDates, DateTime[] payPaymentDates, Decimal[] payAmount, DateTime[] receivePaymentDates, Decimal[] receiveAmount)
        {
            List<DateTime> payerDates = new List<DateTime>(payPaymentDates);
            List<DateTime> recieverDates = new List<DateTime>(receivePaymentDates);
            List<Decimal> nettedValues = new List<Decimal>();
            for (int index = 0; index < paymentDates.Length; index++)
            {
                Decimal value1 = 0.0m;
                Decimal value2 = 0.0m;
                int matchIndex = payerDates.FindIndex(item => DateTime.Compare(item, paymentDates[index]) == 0);
                if (matchIndex > -1)
                {
                    value1 = payAmount[matchIndex];
                }

                matchIndex = recieverDates.FindIndex(item => DateTime.Compare(item, paymentDates[index]) == 0);
                if (matchIndex > -1)
                {
                    value2 = receiveAmount[matchIndex];
                }

                nettedValues.Add(value1 + value2);
            }
            return nettedValues.ToArray();
        }

        private List<object> ConvertToList<T>(T[] array)
        {
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private List<object> ConvertDateTimeToList(Object objArray)
        {
            DateTime[] array = (DateTime[])objArray;
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private List<object> ConvertDateTimeToStringList(Object objArray)
        {
            DateTime[] array = (DateTime[])objArray;
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i].ToString("d", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            }
            return result;
        }

        private List<object> ConvertDecimalToList(Object objArray)
        {
            Decimal[] array = (Decimal[])objArray;
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private List<object> ConvertIntToList(Object objArray)
        {
            int[] array = (int[])objArray;
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private List<object> ConvertStringToList(Object objArray)
        {
            String[] array = (String[])objArray;
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private static string LoadXMLFileToString(string filePath)
        {
            StringBuilder result = new StringBuilder();
            string input = null;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                TextReader r = new StreamReader(fs);
                while ((input = r.ReadLine()) != null)
                {
                    result.Append(input);
                }
            }
            return result.ToString();
        }

        //use this to set up time buckets for trade
        private static int[] GetDayIntervalOffsets(DateTime referenceDate, DateTime maturityDate, string intervalString, out DateTime[] dates)
        {
            List<int> result = new List<int>();
            List<DateTime> datesList = new List<DateTime>();

            DateTime baseDate = new DateTime(referenceDate.Year, referenceDate.Month, referenceDate.Day);
            const int cZeroDaysOffset = 0;
            DateTime offSetDate = baseDate;

            if (offSetDate.CompareTo(maturityDate) <= 0)
            {
                result.Add(cZeroDaysOffset);
                datesList.Add(offSetDate);
            }

            Interval interval = IntervalHelper.Parse(intervalString);

            DateTime newDate = DateHelper.AddPeriod(offSetDate, interval, 1);
            while (newDate.CompareTo(maturityDate) <= 0)
            {
                int noOfDaysOffset = (newDate - offSetDate).Days;
                result.Add(result[result.Count - 1] + noOfDaysOffset);
                offSetDate = newDate;
                datesList.Add(offSetDate);
                newDate = DateHelper.AddPeriod(offSetDate, interval, 1);
            }
            dates = datesList.ToArray();
            return result.ToArray();
        }


        static private Decimal[] CreateEmptyArray(int[] timeBuckets)
        {
            Decimal[] result = new Decimal[timeBuckets.Length];
            for (int i = 0; i < timeBuckets.Length; i++)
            {
                result[i] = 0.0M;
            }
            return result;
        }

        /// <summary>
        /// Function interpolates curve linearly assuming that the collection is already sorted
        /// </summary>
        /// <param name="t1"> Point to evaluate</param>
        /// <param name="curve"> curve data dictionary</param>
        /// <returns></returns>
        public static decimal LinearInterpolation(int t1, IDictionary<int, decimal> curve)
        {
            if (t1 <= GetCurveElement(0, curve).Key)
            {  
                return GetCurveElement(0, curve).Value; //lower curve bound
            }
            else if (t1 >= GetCurveElement(curve.Count - 1, curve).Key)
            {    
                return GetCurveElement(curve.Count - 1, curve).Value; //upper curve bound
            }
            else
            {
                //check if t1 exactly matches a curve point (ie no interpolation necessary)

                decimal x1;
                decimal y1;

                decimal x2;
                decimal y2;

                for (int j = 1; j < curve.Count; j++)
                {
                    if (t1 <= GetCurveElement(j, curve).Key)
                    {
                        x2 = GetCurveElement(j, curve).Key / 365.0M;
                        y2 = GetCurveElement(j, curve).Value;

                        x1 = GetCurveElement(j - 1, curve).Key / 365.0M;
                        y1 = GetCurveElement(j - 1, curve).Value;

                        decimal k = (y2 - y1) / (x2 - x1);
                        decimal a = y1 - k * x1;
                        decimal val = k * t1 / 365.0M + a; 
                        return val;
                    }
                }
            }

            return 0;

        }

        public static KeyValuePair<int, decimal> GetCurveElement(int i, IDictionary<int, decimal> curve)
        {
            int j = 0;
            foreach (KeyValuePair<int, decimal> temp in curve)
            {
                if (j == i)
                {
                    return new KeyValuePair<int, decimal>(temp.Key, temp.Value);
                }
                ++j;
            }
            throw new IndexOutOfRangeException();
        }
    }

    public class StreamDataSet
    {
        public string StreamType { get; set; }
        public string PaymentFlowType { get; set; }
        public decimal[] Rate { get; set; }
        public bool[] PaymentDayIsAdjusted { get; set; }
        public bool[] EndDayIsAdjusted { get; set; }
        public bool[] StartDayIsAdjusted { get; set; }
        public DateTime[] UnadjustedStartDate { get; set; }
        public DateTime[] AdjustedStartDate { get; set; }
        public DateTime[] UnadjustedEndDate { get; set; }
        public DateTime[] AdjustedEndDate { get; set; }
        public DateTime[] CouponStartDate { get; set; }
        public decimal[] CouponYearFraction { get; set; }
        public string[] DayCountConvention { get; set; }
        public string[] DiscountingType { get; set; }
        public string[] Currency { get; set; }
        public string[] BusinessCenters { get; set; }
        public string[] DateAdjustmentConvention { get; set; }
        public string[] BucketedDatesList { get; set; }
        public bool[] IsRealised { get; set; }
        public bool[] PaymentDateIncluded { get; set; }
        public DateTime[] PaymentDate { get; set; }
        public DateTime[] UnadjustedPaymentDate { get; set; }
        public decimal[] Notional { get; set; }
        public string[] DiscountCurveName { get; set; }
        public bool[] UseObservedRate { get; set; }
        public bool[] RequiresReset { get; set; }
        public string[] ForwardCurveName { get; set; }
        public bool[] RateObservationSpecified { get; set; }
        public decimal[] ObservedRate { get; set; }
        public DateTime[] ResetDate { get; set; }
        public string[] ResetRelativeTo { get; set; }
        public decimal[] Margin { get; set; }
        public string[] RateIndexName { get; set; }
        public decimal[] PaymentDiscountFactor { get; set; }
        public decimal[] ExpectedValue { get; set; }
        public decimal[] NPV { get; set; }
        public string[] PaymentType { get; set; }
        public DateTime[] PrincipalExchangeDates { get; set; }
        public string[] PrincipalExchangePaymentType { get; set; }
        public decimal[] PrincipalExchangeAmount { get; set; }
    }
}
