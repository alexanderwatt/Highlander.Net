﻿/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Highlander.Metadata.Common;
using Highlander.Reporting.V5r3;
using Highlander.TestHelpers.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Reporting.Tests.V5r3
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ReportingTests
    {
        public ReportingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private const string fpmlViewName = "Reporting";

        [TestMethod]
        public void TestReportingAutoDetectionAllTypes()
        {
            foreach (var concreteType in FpMLViewHelpers.GetFpMLTypes(true))
            {
                System.Diagnostics.Debug.Print("Type: {0} ...", concreteType.Name);
                var orig = Activator.CreateInstance(concreteType);
                // serialise/deserialise as Document and then specific type
                foreach (var serialiserType in new[] {
                    typeof(Document),
                    concreteType,
                })
                {
                    byte[] streamBytes;
                    {
                        XmlSerializer xs = new XmlSerializer(serialiserType);
                        using (MemoryStream ms = new MemoryStream())
                        using (XmlTextWriter xw = new XmlTextWriter(ms, Encoding.UTF8))
                        {
                            xs.Serialize(xw, orig);
                            streamBytes = ms.ToArray();
                        }
                        using (MemoryStream ms = new MemoryStream(streamBytes, false))
                        {
                            var copy = xs.Deserialize(ms);
                            Assert.IsNotNull(copy);
                            Assert.AreEqual<Type>(concreteType, copy.GetType());
                        }
                    }
                    // autodetect concrete type to deserialise from stream
                    Type autoDetectedType = FpMLViewHelpers.AutoDetectType(streamBytes);
                    using (MemoryStream ms = new MemoryStream(streamBytes, false))
                    {
                        XmlSerializer xs = new XmlSerializer(autoDetectedType);
                        var copy = xs.Deserialize(ms);
                        Assert.IsNotNull(copy);
                        Assert.AreEqual<Type>(concreteType, copy.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void TestReportingFpMLSamplesLoadAllValid()
        {
            //string fileSpec =  "rpt-ex20-matured-and-expired-trades.xml";
            string fileSpec = "*.xml";
            TimeSpan localTimeOffset = DateTimeOffset.Now.Offset;
            //Changed target from 11 to 10. Why did this have to happen?
            Assert.AreEqual(TimeSpan.FromHours(11), localTimeOffset);//TimeSpan.FromHours(11)
            var transformIncoming = new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap(), OutputDateTimeKind.UnspecifiedOrLocal, null);
            var transformOutgoing = new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap(), OutputDateTimeKind.UnspecifiedOrCustom, TimeSpan.FromHours(-5));
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                fpmlViewName, schemaSet, FpMLViewHelpers.AutoDetectType, fileSpec,
                transformIncoming, transformOutgoing,
                false, false, false);
            Assert.AreEqual(149, results.FilesProcessed.Count);
            Assert.AreEqual(0, results.DeserialisationErrors.Count);
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
        }

        [TestMethod]
        public void TestReportingFpMLSamplesLoadAndValidateAll()
        {
            //string fileSpec = "rpt-ex11-position-report-equity.xml";
            string fileSpec = "*.xml";
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                fpmlViewName, schemaSet, FpMLViewHelpers.AutoDetectType, fileSpec, 
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                true, false, false);
            Assert.AreEqual(149, results.FilesProcessed.Count);
            Assert.AreEqual(0, results.DeserialisationErrors.Count);
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
        }

        [TestMethod]
        public void TestReportingFpMLSamplesLoadErrors()
        {
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                fpmlViewName, schemaSet, FpMLViewHelpers.AutoDetectType, "*.error",
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                false, false, false);
            Assert.AreEqual(2, results.FilesProcessed.Count);
            Assert.AreEqual(1, results.DeserialisationErrors.Count);
        }

        [TestMethod]
        public void TestReportingFpMLSamplesRoundtripAll()
        {
            //string fileSpec = "rpt-ex11-position-report-equity.xml";
            string fileSpec = "*.xml";
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                fpmlViewName, schemaSet, FpMLViewHelpers.AutoDetectType, fileSpec,
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                true, true, true);
            Assert.AreEqual(149, results.FilesProcessed.Count);
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
            Assert.AreEqual(0, results.DeserialisationErrors.Count);
            Assert.AreEqual(0, results.InternalComparisonErrors.Count);
            Assert.AreEqual(0, results.ExternalComparisonErrors.Count);
            Assert.AreEqual(0, results.CopyValidationWarnings.Count);
            Assert.AreEqual(0, results.CopyValidationErrors.Count);
        }

        private Party[] GetTestFragment_Parties()
        {
            return new[]
            {
                        new Party { id="Party0", partyId = new[] { new PartyId() { partyIdScheme = "scheme/1.0", Value="Participant 0" } } },
                        new Party { id="Party1", partyId = new[] { new PartyId() { partyIdScheme = "scheme/1.0", Value="Participant 1" } } },
            };
        }

        private Account[] GetTestFragment_Accounts(int count)
        {
            var results = new List<Account>();
            if (count > 0)
            {
                results.Add(new Account
                {
                    id = "Account0",
                    accountBeneficiary = new PartyReference() { href = "Party0"},
                    accountId = new[] { new AccountId() { accountIdScheme = "scheme/1.0", Value="Account0_Id" } },
                    accountName = new[] { new AccountName() { accountNameScheme = "scheme/1.0", Value = "Account0_Name" } }
                });
            }
            if (count > 1)
            {
                results.Add(new Account
                {
                    id = "Account1",
                    accountBeneficiary = new PartyReference() { href = "Party1"},
                    accountId = new[] { new AccountId() { accountIdScheme = "scheme/1.0", Value="Account1_Id" } },
                    accountName = new[] { new AccountName() { accountNameScheme = "scheme/1.0", Value = "Account1_Name" } }
                });
            }
            return results.ToArray();
        }

        private T Roundtrip_OutThenIn<T>(T input, bool compare, bool validate)
        {
            string originalPath = Path.GetFullPath(@"..\..\step0original.xml");
            string internalPath = Path.GetFullPath(@"..\..\step1external.xml");
            string externalPath = Path.GetFullPath(@"..\..\step2internal.xml");
            IXmlTransformer transformIncoming = new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap());
            IXmlTransformer transformOutgoing = new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap());
            // emit original internal
            var xs = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(originalPath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, input);
            }
            // transform to external format
            transformOutgoing.Transform(originalPath, externalPath);
            // transform to internal format
            transformIncoming.Transform(externalPath, internalPath);
            if (compare)
            {
                // compare
                // load original into xml doc
                var docA = new XmlDocument();
                using (var fs = new FileStream(originalPath, FileMode.Open, FileAccess.Read))
                //using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    docA.Load(fs);
                }
                // load emitted stream into xml doc
                var docB = new XmlDocument();
                using (var fs = new FileStream(internalPath, FileMode.Open, FileAccess.Read))
                {
                    docB.Load(fs);
                }
                // compare
                XmlCompare.CompareXmlDocs(docA, docB);
            }
            // validate external
            if (validate)
            {
                const string schemaPath = @"..\..\..\..\..\FpMLV5r3\Metadata\Reporting\" + fpmlViewName + ".xsd";
                string schemaFullPath = Path.GetFullPath(schemaPath);
                Assert.IsTrue(File.Exists(schemaFullPath));
                // validate external xml
                int validationEvents = 0;
                var externalDoc = new XmlDocument();
                externalDoc.Schemas.Add(null, "file://" + schemaFullPath);
                externalDoc.Load(externalPath);
                externalDoc.Validate((o, e) =>
                {
                    validationEvents++;
                    System.Diagnostics.Debug.Print("Validation event: {0}", e.Message);
                });
                Assert.AreEqual(0, validationEvents);
            }
            // deserialise
            using (TextReader tr = new StreamReader(internalPath))
            {
                return (T)xs.Deserialize(tr);
            }
        }

        [TestMethod]
        public void TestReportingRoundtripOutThenInRequestMargin()
        {
            var orig = new RequestMargin
            {
                fpmlVersion = "5-3",
                header = new RequestMessageHeader
                {
                    messageId = new MessageId { messageIdScheme = "scheme/1.0", Value = "12345678" },
                    //inReplyTo = new MessageId() { messageIdScheme = "scheme/1.0", Value = "87654321" },
                    sentBy = new MessageAddress { messageAddressScheme = "scheme/1.0", Value = "trader@bigbank.com" },
                    creationTimestamp = DateTime.Now
                },
                correlationId = new CorrelationId { correlationIdScheme = "scheme/1.0", Value = "12345678" },
                party = GetTestFragment_Parties(),
                account = GetTestFragment_Accounts(2),
                assets = new Asset[]
                    {
                        new Bond(), // empty
                        new Bond { id = "id0" },
                        new Bond
                        {
                            instrumentId = new[]
                            {
                                new InstrumentId {  instrumentIdScheme = "CUSIP", Value = "789012EW0" }
                            }
                        },
                        new Loan(), // empty
                        new Loan { id = "id1" },
                        new Loan
                        {
                            instrumentId = new[]
                            {
                                new InstrumentId {  instrumentIdScheme = "CUSIP", Value = "789012EW0" }
                            }
                        },
                        new Cash(),
                        new Cash { id="cash01"},
                        new Cash { currency=new Currency { Value="USD" } },
                        new EquityAsset(),
                        new EquityAsset { id="equtiy01" },
                        new EquityAsset
                        {
                            instrumentId = new[]
                            {
                                new InstrumentId {  instrumentIdScheme = "Bloomberg", Value = "BHP.AX" }
                            }
                        },
                    },
            };
            var copy = Roundtrip_OutThenIn(orig, true, true);
            //using (var sr = new StreamReader(internalFullPath))
            //{
            //    string xmlText = sr.ReadToEnd();
            //    Assert.IsFalse(xmlText.Contains("<bond"));
            //    Assert.IsFalse(xmlText.Contains("<loan"));
            //    Assert.IsFalse(xmlText.Contains("<cash"));
            //    Assert.IsFalse(xmlText.Contains("<equity"));
            //    Assert.IsTrue(xmlText.Contains("<underlyingAsset"));
            //}

            //using (var sr = new StreamReader(externalFullPath))
            //{
            //    string xmlText = sr.ReadToEnd();
            //    Assert.IsTrue(xmlText.Contains("<bond"));
            //    Assert.IsTrue(xmlText.Contains("<loan"));
            //    Assert.IsTrue(xmlText.Contains("<cash"));
            //    Assert.IsTrue(xmlText.Contains("<equity"));
            //    Assert.IsFalse(xmlText.Contains("<underlyingAsset"));
            //}
        }

        [TestMethod]
        public void TestReportingRoundtripOutThenInValuationReport()
        {
            var orig = new ValuationReport()
            {
                fpmlVersion = "5-3",
                header = new NotificationMessageHeader()
                {
                    messageId = new MessageId() { messageIdScheme = "scheme/1.0", Value = "12345678" },
                    inReplyTo = new MessageId() { messageIdScheme = "scheme/1.0", Value = "87654321" },
                    sentBy = new MessageAddress() { messageAddressScheme = "scheme/1.0", Value = "trader@bigbank.com" },
                    creationTimestamp = DateTime.Now
                },
                correlationId = new CorrelationId() { correlationIdScheme = "scheme/1.0", Value = "12345678" },
                party = GetTestFragment_Parties(),
                account = GetTestFragment_Accounts(2),
                tradeValuationItem = new[] {
                        new TradeValuationItem()
                },
            };
            var copy = Roundtrip_OutThenIn(orig, true, true);
        }

        [TestMethod]
        public void TestReportingRoundtripOutThenInPositionReport()
        {
            // payment rules abstract type issue
            var orig = new PositionReport()
            {
                fpmlVersion = "5-3",
                position = new[]
                {
                    new ReportedPosition()
                    {
                        constituent = new PositionConstituent()
                        {
                            Item = new Trade()
                            {
                                collateral = new Collateral()
                                {
                                    independentAmount = new IndependentAmount()
                                    {
                                        paymentDetail = new[]
                                        {
                                            new PaymentDetail() { id = "id0", paymentRule = new PercentageRule() { paymentPercentSpecified = true, paymentPercent = 5.0M } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            PositionReport copy = Roundtrip_OutThenIn(orig, true, false);          
            // tests
            Trade trade = (Trade)copy.position[0].constituent.Item;
            PercentageRule rule = (PercentageRule)trade.collateral.independentAmount.paymentDetail[0].paymentRule;
            Assert.IsTrue(rule.paymentPercentSpecified);
            Assert.AreEqual(5.0M, rule.paymentPercent);
        }

        [TestMethod]
        public void TestReportingRoundtripDateTimes()
        {
            const string schemaPath = @"..\..\..\..\..\FpMLV5r3\Metadata\Reporting\Reporting.xsd";
            string schemaFullPath = Path.GetFullPath(schemaPath);
            Assert.IsTrue(File.Exists(schemaFullPath));
            string originalFullPath = Path.GetFullPath(@"..\..\testOriginalFragment.xml");
            string internalFullPath = Path.GetFullPath(@"..\..\testInternalFragment.xml");
            string outgoingFullPath = Path.GetFullPath(@"..\..\testOutgoingFragment.xml");
            string externalFullPath = Path.GetFullPath(@"..\..\testExternalFragment.xml");
            IXmlTransformer transformIncoming = new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap(), OutputDateTimeKind.UnspecifiedOrUniversal, null);
            IXmlTransformer transformOutgoing = new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap(), OutputDateTimeKind.UnspecifiedOrCustom, TimeSpan.FromHours(-5));
            XmlSerializer xs = new XmlSerializer(typeof(ExposureReport));
            // original FpML.org sample
            string originalFpMLText =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<exposureReport" +
                "    xmlns=\"http://www.fpml.org/FpML-5/reporting\"" +
                "    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                "    fpmlVersion=\"5-3\" >" +
                "  <asOfDate>2010-09-27</asOfDate>" +
                "  <asOfTime>20:08:10-05:00</asOfTime>" +
                "</exposureReport>";
            using (var sw = new StreamWriter(originalFullPath))
            {
                sw.WriteLine(originalFpMLText);
            }
            DateTime expectedAsOfDate = new DateTime(2010, 9, 27, 0, 0, 0);
            DateTime expectedAsOfTime = new DateTime(1, 1, 1, 1, 8, 10);//11 became 12 then DateTime(1, 1, 1, 12, 8, 10)
            {
                // original FpML.org sample
                // --- does not parse reliably due to variably local time zone offset of paser ---
                // asOfDate has an unspecified time zone
                // asOfTime has specific time zone (New York)
                ExposureReport orig;
                using (StringReader sr = new StringReader(originalFpMLText))
                {
                    orig = (ExposureReport)xs.Deserialize(sr);
                }
                Assert.AreEqual(DateTimeKind.Unspecified, orig.asOfDate.Value.Kind);
                Assert.IsTrue(orig.asOfTimeSpecified);
                Assert.AreEqual(DateTimeKind.Local, orig.asOfTime.Kind);
                // --- this test does not pass reliably due to variable local time zone offset of paser ---
                DateTime asOfDate = new DateTime(2010, 9, 27);
                Assert.AreEqual(asOfDate, orig.asOfDate.Value);
                DateTime asOfTime = new DateTime(1, 1, 2, 12, 8, 10);//TODO the time changed due to daylight saving??Why?
                Assert.AreEqual(asOfTime, orig.asOfTime);//2/01/0001 12:08:10 AM
            }
            {
                // modified FpML.org sample
                // --- this test does not pass reliably due to variable local time zone offset of paser ---
                // - asOfDate time zone modified to match asOfTime
                string externalXmlText =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<exposureReport" +
                "    xmlns=\"http://www.fpml.org/FpML-5/reporting\"" +
                "    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                "    fpmlVersion=\"5-3\" >" +
                "  <asOfDate>2010-09-27-5:00</asOfDate>" +
                "  <asOfTime>20:08:10-05:00</asOfTime>" +
                "</exposureReport>";
                ExposureReport orig;
                using (StringReader sr = new StringReader(externalXmlText))
                {
                    orig = (ExposureReport)xs.Deserialize(sr);
                }
                Assert.AreEqual(DateTimeKind.Local, orig.asOfDate.Value.Kind);
                Assert.IsTrue(orig.asOfTimeSpecified);
                Assert.AreEqual(DateTimeKind.Local, orig.asOfTime.Kind);
                // --- this test does not pass reliably due to variable local time zone offset of parser ---
                DateTime asOfDate = new DateTime(2010, 9, 27, 15, 0, 0); // Australia EST is +10:00
                Assert.AreEqual(asOfDate, orig.asOfDate.Value);
                DateTime asOfTime = new DateTime(1, 1, 2, 12, 8, 10);//11 became 12
                Assert.AreEqual(asOfTime, orig.asOfTime);
            }
            {
                // modified FpML.org sample
                // - asOfDate and asOfTime set to UTC
                string externalXmlText =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<exposureReport" +
                "    xmlns=\"http://www.fpml.org/FpML-5/reporting\"" +
                "    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                "    fpmlVersion=\"5-3\" >" +
                "  <asOfDate>2010-09-27Z</asOfDate>" +
                "  <asOfTime>20:08:10Z</asOfTime>" +
                "</exposureReport>";
                ExposureReport orig;
                using (StringReader sr = new StringReader(externalXmlText))
                {
                    orig = (ExposureReport)xs.Deserialize(sr);
                }
                Assert.AreEqual(DateTimeKind.Local, orig.asOfDate.Value.Kind);
                Assert.IsTrue(orig.asOfTimeSpecified);
                Assert.AreEqual(DateTimeKind.Utc, orig.asOfTime.Kind);//Should be DateTimeKind.Local
                // --- this test does not pass reliably due to variable local time zone offset of parser ---
                DateTime asOfDate = new DateTime(2010, 9, 27, 10, 0, 0); // Australia EST is +10:00
                Assert.AreEqual(asOfDate, orig.asOfDate.Value);
                DateTime asOfTime = new DateTime(1, 1, 1, 20, 8, 10);//6 was (1, 1, 2, 7, 8, 10)
                Assert.AreEqual(asOfTime, orig.asOfTime);
            }
            {
                // modified FpML.org sample
                // --- passes reliably because local time zone offset of parser is ignored ---
                // - asOfDate and asOfTime set to unspecified time zone
                string externalXmlText =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<exposureReport" +
                "    xmlns=\"http://www.fpml.org/FpML-5/reporting\"" +
                "    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                "    fpmlVersion=\"5-3\" >" +
                "  <asOfDate>2010-09-27</asOfDate>" +
                "  <asOfTime>20:08:10</asOfTime>" +
                "</exposureReport>";
                ExposureReport orig;
                using (StringReader sr = new StringReader(externalXmlText))
                {
                    orig = (ExposureReport)xs.Deserialize(sr);
                }
                Assert.AreEqual(DateTimeKind.Unspecified, orig.asOfDate.Value.Kind);
                Assert.IsTrue(orig.asOfTimeSpecified);
                Assert.AreEqual(DateTimeKind.Unspecified, orig.asOfTime.Kind);
                Assert.AreEqual(new DateTime(2010, 9, 27, 0, 0, 0), orig.asOfDate.Value);
                Assert.AreEqual(new DateTime(1, 1, 1, 20, 8, 10), orig.asOfTime);
            }
            // now that we have a reliably formatted external fpml fragment we can test the roundtrip
            // transform to internal format
            transformIncoming.Transform(originalFullPath, internalFullPath);
            // deserialise
            ExposureReport fpml;
            using (var sr = new FileStream(internalFullPath, FileMode.Open, FileAccess.Read))
            {
                fpml = (ExposureReport)xs.Deserialize(sr);
            }
            Assert.AreEqual(DateTimeKind.Unspecified, fpml.asOfDate.Value.Kind);
            Assert.IsTrue(fpml.asOfTimeSpecified);
            Assert.AreEqual(DateTimeKind.Utc, fpml.asOfTime.Kind);//DateTimeKind.Local
            Assert.AreEqual(expectedAsOfDate, fpml.asOfDate.Value);
            Assert.AreEqual(expectedAsOfTime, fpml.asOfTime);
            // emit test fragment
            using (var fs = new FileStream(outgoingFullPath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, fpml);
            }
            // transform to external format
            transformOutgoing.Transform(outgoingFullPath, externalFullPath);
            // compare external xml
            using (var originalStream = new FileStream(originalFullPath, FileMode.Open, FileAccess.Read))
            using (var externalStream = new FileStream(externalFullPath, FileMode.Open, FileAccess.Read))
            {
                XmlDocument originalDoc = new XmlDocument();
                originalDoc.Load(originalStream);
                XmlDocument emittedDoc = new XmlDocument();
                emittedDoc.Load(externalStream);
                // compare
                XmlCompare.CompareXmlDocs(originalDoc, emittedDoc);
            }
        }

        //private T LoadSpecificFile<T>(string subPath, string filename) where T : Document
        //{
        //    string fullFilename = SampleFilesPath + @"\"+subPath + @"\"+ filename;
        //    Type autoDetectedType = typeof(T);
        //    XmlSerializer xs = new XmlSerializer(autoDetectedType);
        //    object anyFpml;
        //    using (var fs = new FileStream(fullFilename, FileMode.Open, FileAccess.Read))
        //    {
        //        anyFpml = xs.Deserialize(fs);
        //        Assert.IsNotNull(anyFpml);
        //        Assert.IsTrue(anyFpml is Document);
        //        Assert.AreEqual<Type>(autoDetectedType, anyFpml.GetType());
        //    }
        //    T fpml = anyFpml as T;
        //    Assert.IsNotNull(fpml);
        //    return fpml;
        //}

        [TestMethod]
        public void TestReportingRoundtripOutThenInTerminatingEventsReport()
        {
            var orig = new TerminatingEventsReport()
            {
                fpmlVersion = "5-3",
                header = new NotificationMessageHeader()
                {
                    messageId = new MessageId() { messageIdScheme = "scheme/1.0", Value = "12345678" },
                    sentBy = new MessageAddress() { messageAddressScheme = "scheme/1.0", Value = "trader@bigbank.com" },
                    sendTo = new[] { new MessageAddress() { messageAddressScheme = "scheme/1.0", Value = "client@company.com" } },
                    creationTimestamp = DateTime.Now
                },
                correlationId = new CorrelationId() { correlationIdScheme = "scheme/1.0", Value = "12345678" },
                party = GetTestFragment_Parties(),
                account = GetTestFragment_Accounts(1),
            };
            var copy = Roundtrip_OutThenIn(orig, true, true);
        }
    }
}
