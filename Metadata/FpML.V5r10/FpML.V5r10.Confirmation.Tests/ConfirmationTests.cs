using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Metadata.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FpML.V5r10.Confirmation.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ConfirmationTests
    {
        public ConfirmationTests()
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

        private const string FPMLViewName = "Confirmation";

        [TestMethod]
        public void TestConfirmationAutoDetectionAllTypes()
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
                        var xs = new XmlSerializer(serialiserType);
                        using (var ms = new MemoryStream())
                        using (var xw = new XmlTextWriter(ms, Encoding.UTF8))
                        {
                            xs.Serialize(xw, orig);
                            streamBytes = ms.ToArray();
                        }
                        using (var ms = new MemoryStream(streamBytes, false))
                        {
                            var copy = xs.Deserialize(ms);
                            Assert.IsNotNull(copy);
                            Assert.AreEqual(concreteType, copy.GetType());
                        }
                    }
                    // autodetect concrete type to deserialise from stream
                    Type autoDetectedType = FpMLViewHelpers.AutoDetectType(streamBytes);
                    using (var ms = new MemoryStream(streamBytes, false))
                    {
                        var xs = new XmlSerializer(autoDetectedType);
                        var copy = xs.Deserialize(ms);
                        Assert.IsNotNull(copy);
                        Assert.AreEqual(concreteType, copy.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void TestConfirmationFpMLSamplesLoadAllValid()
        {
            const string fileSpec = "*.xml";
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                FPMLViewName, schemaSet, FpMLViewHelpers.AutoDetectType, fileSpec,
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                false, false, false);
            Assert.AreEqual(1493, results.FilesProcessed.Count);
            Assert.AreEqual(121, results.DeserialisationErrors.Count);//121 errors
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
        }

        [TestMethod]
        public void TestConfirmationFpMLSamplesLoadAndValidateAll()
        {
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                FPMLViewName, schemaSet, FpMLViewHelpers.AutoDetectType, "*.xml",
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                true, false, false);
            Assert.AreEqual(1493, results.FilesProcessed.Count);//593
            Assert.AreEqual(121, results.DeserialisationErrors.Count);//TODO should be 0
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
        }

        [TestMethod]
        public void TestConfirmationFpMLSamplesLoadErrors()
        {
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                FPMLViewName, schemaSet, FpMLViewHelpers.AutoDetectType, "*.error",
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                false, false, false);
            Assert.AreEqual(0, results.FilesProcessed.Count);//7
            Assert.AreEqual(0, results.DeserialisationErrors.Count);//4
        }

        [TestMethod]
        public void TestConfirmationFpMLSamplesRoundtripAll()
        {
            const string fileSpec = "*.xml";
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                FPMLViewName, schemaSet, FpMLViewHelpers.AutoDetectType, fileSpec,
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                true, true, true);
            Assert.AreEqual(1493, results.FilesProcessed.Count);
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
            Assert.AreEqual(0, results.OutgoingTransformErrors.Count);
            Assert.AreEqual(121, results.DeserialisationErrors.Count);//TODO should be 0
            Assert.AreEqual(360, results.InternalComparisonErrors.Count);//TODO should be 0
            Assert.AreEqual(360, results.ExternalComparisonErrors.Count);//TODO should be 0
            // Note: There are 4 files causing benign validation events. These are all 
            // due to un-necessary emission of the "breakFeeElection" element in the 
            // "equitySwapTransactionSupplement" element. It appears that the schema should
            // define the "breakFeeElection" element as minOccurs=0, but it does not, and
            // the consequence is that the element is emitted with the default value (FlatFee).
            // Removal of the extra element causes downstream errors so it remains as is.
            Assert.AreEqual(0, results.CopyValidationErrors.Count);//4
            Assert.AreEqual(0, results.CopyValidationWarnings.Count);
        }

        [TestMethod]
        public void TestConfirmationFpMLSamplesRoundtripFromQrml()
        {
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            var results = TestHelper.LoadFiles(
                FPMLViewName, schemaSet, FpMLViewHelpers.AutoDetectType, @"FromQRML\*.xml",
                new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap()),
                new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap()),
                true, true, true);
            Assert.AreEqual(1, results.FilesProcessed.Count);
            Assert.AreEqual(0, results.OrigValidationWarnings.Count);
            Assert.AreEqual(0, results.OrigValidationErrors.Count);
            Assert.AreEqual(0, results.IncomingTransformErrors.Count);
            Assert.AreEqual(0, results.OutgoingTransformErrors.Count);
            Assert.AreEqual(0, results.DeserialisationErrors.Count);
            Assert.AreEqual(0, results.InternalComparisonErrors.Count);
            Assert.AreEqual(0, results.ExternalComparisonErrors.Count);
            Assert.AreEqual(0, results.CopyValidationWarnings.Count);
            Assert.AreEqual(0, results.CopyValidationErrors.Count);
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
                const string schemaPath = @"..\..\..\..\..\Metadata\FpML.V5r10\FpML.V5r10\" + FPMLViewName + ".xsd";
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
        public void TestConfirmationRoundtripOutThenInConfirmationAgreedEquityOptionWithBermudanExerciseDates()
        {
            string originalPath = Path.GetFullPath(@"..\..\step0original.xml");
            string internalPath = Path.GetFullPath(@"..\..\step1external.xml");
            string externalPath = Path.GetFullPath(@"..\..\step2internal.xml");
            var xs = new XmlSerializer(typeof(ConfirmationAgreed));
            var orig = new ConfirmationAgreed()
            {
                fpmlVersion = "5-3",
                header = new ResponseMessageHeader
                {
                    messageId = new MessageId { messageIdScheme = "scheme/1.0", Value = "12345678" },
                    inReplyTo = new MessageId { messageIdScheme = "scheme/1.0", Value = "87654321" },
                    sentBy = new MessageAddress { messageAddressScheme = "scheme/1.0", Value = "trader@bigbank.com" },
                    creationTimestamp = DateTime.Now
                },
                correlationId = new CorrelationId { correlationIdScheme = "scheme/1.0", Value = "12345678" },
                party = new[] {
                        new Party { id="Party0", partyId = new[] { new PartyId() { partyIdScheme = "scheme/1.0", Value="Participant 0" } } },
                        new Party { id="Party1", partyId = new[] { new PartyId() { partyIdScheme = "scheme/1.0", Value="Participant 1" } } },
                    },
                account = new[] {
                        new Account
                        {
                            id = "Account0",
                            accountId = new[] { new AccountId() { accountIdScheme = "scheme/1.0", Value="Account0_Id" } },
                            ItemsElementName = new[] { ItemsChoiceType21.accountBeneficiary }, 
                            Items = new[] { new PartyReference() { href = "Party0"}}
                        },
                        new Account
                        {
                            id = "Account1",
                            accountId = new[] { new AccountId() { accountIdScheme = "scheme/1.0", Value="Account1_Id" }},
                            ItemsElementName = new[] { ItemsChoiceType21.accountBeneficiary },
                            Items = new[] { new PartyReference() { href = "Party1"}},
                        },
                    },
                validation = new[]
                    {
                        new Validation() { validationScheme = "scheme/1.0", Value = "Validation0" }
                    },
                trade = new Trade()
                {
                    tradeHeader = new TradeHeader
                    {
                        partyTradeIdentifier = new[] { new PartyTradeIdentifier() { Items = new object[] {
                                new PartyReference { href="Party0" }, 
                                new AccountReference { href="Account0"},
                                new TradeId() { tradeIdScheme = "scheme/1.0", Value = "Trade1234" },
                            } } },
                        tradeDate = new IdentifiedDate { Value = DateTime.Now.Date }
                    },
                    Item = new EquityOption()
                    {
                        equityPremium = new EquityPremium
                        {
                            payerPartyReference = new PartyReference { href = "Party0" },
                            payerAccountReference = new AccountReference { href = "Account0" },
                            receiverPartyReference = new PartyReference { href = "Party1" },
                            receiverAccountReference = new AccountReference { href = "Account1" }
                        },
                        optionEntitlement = 1.0M,
                        extraordinaryEvents = new ExtraordinaryEvents
                        {
                            mergerEvents = new EquityCorporateEvents(),
                            tenderOfferSpecified = true,
                            tenderOffer = true,
                            tenderOfferEvents = new EquityCorporateEvents(),
                            compositionOfCombinedConsiderationSpecified = true,
                            compositionOfCombinedConsideration = true,
                            indexAdjustmentEvents = new IndexAdjustmentEvents(),
                            Item = false // failureToDeliver
                        },
                        buyerPartyReference = new PartyReference { href = "Party0" },
                        buyerAccountReference = new AccountReference { href = "Account0" },
                        sellerPartyReference = new PartyReference { href = "Party1" },
                        sellerAccountReference = new AccountReference { href = "Account1" },
                        optionType = "Forward",
                        equityEffectiveDateSpecified = true,
                        equityEffectiveDate = DateTime.Now.Date,
                        underlyer = new Underlyer
                        {
                            Item = new SingleUnderlyer
                            {
                                Item = new EquityAsset { instrumentId = new[] { new InstrumentId { instrumentIdScheme = "scheme/1.0", Value = "BHP" } } }
                            }
                        },
                        equityExercise = new EquityExerciseValuationSettlement
                        {
                            settlementDate = new AdjustableOrRelativeDate
                            {
                                Item = new AdjustableDate
                                {
                                    unadjustedDate = new IdentifiedDate { Value = DateTime.Now.Date },
                                    dateAdjustments = new BusinessDayAdjustments
                                    {
                                        businessDayConvention = BusinessDayConventionEnum.NONE
                                    }
                                }
                            },
                            settlementCurrency = new Currency() { currencyScheme = "scheme/1.0", Value = "AUD" },
                            settlementPriceSource = new SettlementPriceSource() { settlementPriceSourceScheme = "scheme/1.0", Value = "Source0" },
                            settlementType = "Cash",
                            equityValuation = new EquityValuation(),
                            Items = new object[] {
                                    true, // automaticExercise
                                    new MakeWholeProvisions() { makeWholeDate = DateTime.Now.Date, recallSpread = 1.0M },
                                },
                            Item = new EquityBermudaExercise()
                            {
                                commencementDate = new AdjustableOrRelativeDate
                                    {
                                    Item = new AdjustableDate()
                                    {
                                        unadjustedDate = new IdentifiedDate { Value = DateTime.Now.Date },
                                        dateAdjustments = new BusinessDayAdjustments
                                            {
                                            businessDayConvention = BusinessDayConventionEnum.NONE
                                        }
                                    }
                                },
                                expirationDate = new AdjustableOrRelativeDate
                                    {
                                    Item = new AdjustableDate()
                                    {
                                        unadjustedDate = new IdentifiedDate() { Value = DateTime.Now.Date },
                                        dateAdjustments = new BusinessDayAdjustments
                                            {
                                            businessDayConvention = BusinessDayConventionEnum.NONE
                                        }
                                    }
                                },
                                bermudaExerciseDates = new[]
                                    {
                                        new DateTime(2002, 4, 21),
                                        new DateTime(2002, 5, 21),
                                        new DateTime(2002, 6, 21)
                                    }
                            },
                        }
                    }
                }
            };
            ConfirmationAgreed copy = Roundtrip_OutThenIn(orig, true, true);
            using (var sr = new StreamReader(externalPath))
            {
                string externalXml = sr.ReadToEnd();
                Assert.IsFalse(externalXml.Contains("<dateTime>"));
                Assert.IsTrue(externalXml.Contains("<date>"));
            }
            using (var sr = new StreamReader(internalPath))
            {
                string roundTripXml = sr.ReadToEnd();
                Assert.IsFalse(roundTripXml.Contains("<date>"));
                Assert.IsTrue(roundTripXml.Contains("<dateTime>"));
            }
            // tests
            Assert.IsNotNull(copy.trade);
            Assert.IsNotNull(copy.trade.Item);
            Assert.AreEqual(typeof(EquityOption), copy.trade.Item.GetType());
            var eo = (EquityOption)copy.trade.Item;
            Assert.IsNotNull(eo.equityExercise);
            Assert.IsNotNull(eo.equityExercise.Item);
            Assert.AreEqual(typeof(EquityBermudaExercise), eo.equityExercise.Item.GetType());
            var ebe = (EquityBermudaExercise)eo.equityExercise.Item;
            Assert.IsNotNull(ebe.bermudaExerciseDates);
            Assert.AreEqual(3, ebe.bermudaExerciseDates.Length);
            Assert.AreEqual(new DateTime(2002, 4, 21), ebe.bermudaExerciseDates[0]);
            Assert.AreEqual(new DateTime(2002, 5, 21), ebe.bermudaExerciseDates[1]);
            Assert.AreEqual(new DateTime(2002, 6, 21), ebe.bermudaExerciseDates[2]);
        }

        private bool StreamIsNABReceives(InterestRateStream stream)
        {
            return stream.receiverPartyReference.href.Equals("NAB");
        }

        [TestMethod]
        public void TestCheckStreamDirection()
        {
            var swap = new Swap
                {
                swapStream = new[]
                {
                    new InterestRateStream
                        {
                        payerPartyReference = new PartyReference { href="NAB" },
                        receiverPartyReference = new PartyReference { href = "client" }
                    },
                    new InterestRateStream
                        {
                        payerPartyReference = new PartyReference { href="client" },
                        receiverPartyReference = new PartyReference { href = "NAB" }
                    }
                }
            };

            Assert.IsFalse(StreamIsNABReceives(swap.swapStream[0]));
            Assert.IsTrue(StreamIsNABReceives(swap.swapStream[1]));

        }
    }
}
