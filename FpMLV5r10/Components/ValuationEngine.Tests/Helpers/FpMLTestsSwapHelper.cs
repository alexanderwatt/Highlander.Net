#region Using directives

using System.Reflection;
using System.Xml;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;
//using FpML.V5r3.Confirmation;
using FpML.V5r10.Reporting;

#endregion

namespace Orion.ValuationEngine.Tests.Helpers
{
    public static class FpMLTestsSwapHelper
    {
        private const string Swap01VanillaSwapExample = "interest_rate_derivatives.ird-ex01-vanilla-swap.xml";
        private const string Swap02StubAmortExample = "interest_rate_derivatives.ird-ex02-stub-amort-swap.xml";
        private const string Swap03AUDExample = "interest_rate_derivatives.ird-ex03-compound-swap.xml";
        private const string Swap04StepupFeeSwapExample = "interest_rate_derivatives.ird-ex04-arrears-stepup-fee-swap.xml";
        private const string Swap05LongStubExample = "interest_rate_derivatives.ird-ex05-long-stub-swap.xml";
        private const string Swap06XccySwapExample = "interest_rate_derivatives.ird-ex06-xccy-swap.xml";
        private const string Swap07FXForwardExample = "fx_derivatives.fx-ex03-fx-fwd.xml";
        private const string Swap08FXForwardExample = "fx_derivatives.fx-ex01-fx-spot.xml";
        private const string Swap09OISExample = "interest_rate_derivatives.ird-ex07-ois-swap.xml";
        private const string Swap10FRAExample = "interest_rate_derivatives.ird-ex08-fra.xml";
        private const string Bullet28BulletPaymentsExample = "interest_rate_derivatives.ird-ex28-bullet-payments.xml";
        private const string Td01SimpleTermDepositExample = "fx_derivatives.td-ex02-term-deposit-w-settlement-etc.xml";
        private const string Td02TermDepositWSettlementExample = "fx_derivatives.td-ex01-simple-term-deposit.xml";
        private const string Cap22CapExample = "interest_rate_derivatives.ird-ex22-cap.xml";
        private const string Floor23CapExample = "interest_rate_derivatives.ird-ex23-floor.xml";
        private const string Collar24CapExample = "interest_rate_derivatives.ird-ex24-collar.xml";
        private const string FX08FXSwapExample = "fx_derivatives.fx-ex08-fx-swap.xml";
        private const string FX09FXSwapExample = "fx_derivatives.fx-ex09-euro-opt.xml";
        private const string Swaption09IRSwaptionExample = "interest_rate_derivatives.ird-ex09-euro-swaption-explicit.xml";
        private const string Swaption10IRSwaptionExample = "interest_rate_derivatives.ird-ex10-euro-swaption-relative.xml";

        public static Swaption GetIrSwaptionExampleObject()
        {
            return GetSwaptionObject(Swaption09IRSwaptionExample);
        }

        public static Swaption GetIrSwaption2ExampleObject()
        {
            return GetSwaptionObject(Swaption10IRSwaptionExample);
        }

        public static FxSwap GetFxSwapExampleObject()
        {
            return GetFxSwapObject(FX08FXSwapExample);
        }

        public static FxOption GetFxOptionLegExampleObject()
        {
            return GetFxOptionLegObject(FX09FXSwapExample);
        }

        public static CapFloor GetCapExampleObject()
        {
            return GetCapFloorObject(Cap22CapExample);
        }

        public static CapFloor GetFloorExampleObject()
        {
            return GetCapFloorObject(Floor23CapExample);
        }

        public static CapFloor GetCollarExampleObject()
        {
            return GetCapFloorObject(Collar24CapExample);
        }

        public static TermDeposit GetSimpleTermDepositExampleObject()
        {
            return GetTermDepositObject(Td01SimpleTermDepositExample);
        }

        public static TermDeposit GetTermDepositExampleObject()
        {
            return GetTermDepositObject(Td02TermDepositWSettlementExample);
        }

        public static Trade GetTrade01ExampleObject()
        {
            return GetTradeObject(Swap01VanillaSwapExample);
        }

        public static Trade GetTrade02ExampleObject()
        {
            return GetTradeObject(Swap02StubAmortExample);
        }

        public static Trade GetTrade03ExampleObject()
        {
            return GetTradeObject(Swap03AUDExample);
        }

        public static Trade GetTrade04ExampleObject()
        {
            return GetTradeObject(Swap04StepupFeeSwapExample);
        }

        public static Trade GetTrade05ExampleObject()
        {
            return GetTradeObject(Swap05LongStubExample);
        }

        public static Trade GetTrade06ExampleObject()
        {
            return GetTradeObject(Swap06XccySwapExample);
        }

        public static Trade GetTrade07ExampleObject()
        {
            return GetTradeObject(Swap07FXForwardExample);
        }

        public static Trade GetTrade08ExampleObject()
        {
            return GetTradeObject(Swap08FXForwardExample);
        }

        public static Trade GetTrade09ExampleObject()
        {
            return GetTradeObject(Swap09OISExample);
        }

        public static Trade GetTrade10ExampleObject()
        {
            return GetTradeObject(Swap10FRAExample);
        }

        public static Trade GetTradeBulletExampleObject()
        {
            return GetTradeObject(Bullet28BulletPaymentsExample);
        }

        public static BulletPayment GetBulletPaymentExampleObject()
        {
            return GetBulletPaymentObject(Bullet28BulletPaymentsExample);
        }

        public static Swap GetSwap01ExampleObject()
        {
            return GetSwapObject(Swap01VanillaSwapExample);
        }

        public static Swap GetSwap04ExampleObject()
        {
            return GetSwapObject(Swap04StepupFeeSwapExample);
        }
        
        public static Swap GetSwap05ExampleObject()
        {
            return GetSwapObject(Swap05LongStubExample);
        }

        public static Swap GetSwap06ExampleObject()
        {
            return GetSwapObject(Swap06XccySwapExample);
        }

        public static FxSingleLeg GetSwap07ExampleObject()
        {
            return GetFxSingleLegObject(Swap07FXForwardExample);
        }

        public static FxSingleLeg GetSwap08ExampleObject()
        {
            return GetFxSingleLegObject(Swap08FXForwardExample);
        }

        public static Swap GetSwap03AUDExampleObject()
        {
            return GetSwapObject(Swap03AUDExample);
        }

        public static Swap GetSwap02StubAmort()
        {
            return GetSwapObject(Swap02StubAmortExample);
        }

        public static BulletPayment GetBulletPaymentObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            //xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/confirmation");
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode paymentNode = xmlDocument.SelectSingleNode("//fpml:bulletPayment", xmlNamespaceManager);
            var payment = XmlSerializerHelper.DeserializeNode<BulletPayment>(paymentNode);
            payment.id = "TestTrade";
            return payment;
        }

        public static FxSwap GetFxSwapObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode paymentNode = xmlDocument.SelectSingleNode("//fpml:fxSwap", xmlNamespaceManager);
            var payment = XmlSerializerHelper.DeserializeNode<FxSwap>(paymentNode);
            payment.id = "TestTrade";
            return payment;
        }

        public static FxOption GetFxOptionLegObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode paymentNode = xmlDocument.SelectSingleNode("//fpml:fxOption", xmlNamespaceManager);
            var payment = XmlSerializerHelper.DeserializeNode<FxOption>(paymentNode);
            payment.id = "TestTrade";
            return payment;
        }

        public static CapFloor GetCapFloorObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode paymentNode = xmlDocument.SelectSingleNode("//fpml:capFloor", xmlNamespaceManager);
            var payment = XmlSerializerHelper.DeserializeNode<CapFloor>(paymentNode);
            payment.id = "TestTrade";
            return payment;
        }

        public static TermDeposit GetTermDepositObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode paymentNode = xmlDocument.SelectSingleNode("//fpml:termDeposit", xmlNamespaceManager);
            var payment = XmlSerializerHelper.DeserializeNode<TermDeposit>(paymentNode);
            payment.id = "TestTrade";
            return payment;
        }

        public static Swap GetSwapObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode swapNode = xmlDocument.SelectSingleNode("//fpml:swap", xmlNamespaceManager);
            var swap = XmlSerializerHelper.DeserializeNode<Swap>(swapNode);
            swap.id = "TestTrade";
            return swap;
        }

        public static Swaption GetSwaptionObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode swaptionNode = xmlDocument.SelectSingleNode("//fpml:swaption", xmlNamespaceManager);
            var swap = XmlSerializerHelper.DeserializeNode<Swaption>(swaptionNode);
            swap.id = "TestTrade";
            return swap;
        }

        public static Trade GetTradeObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var doc = XmlSerializerHelper.DeserializeFromString<Document>(resourceAsString);
            var dataDoc = (DataDocument)doc;
            var trade = dataDoc.trade[0];
            trade.id = "TestTrade";
            return trade;
        }

        public static FxSingleLeg GetFxSingleLegObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode swapNode = xmlDocument.SelectSingleNode("//fpml:fxSingleLeg", xmlNamespaceManager);
            var swap = XmlSerializerHelper.DeserializeNode<FxSingleLeg>(swapNode);
            swap.id = "TestTrade";
            return swap;
        }
    }

    public static class FpMLTestsFraHelper
    {
        private const string FRA08FRAExample = "interest_rate_derivatives.ird-ex08-fra.xml";

        public static Fra GetFra08ExampleObject()
        {
            return GetFraObject(FRA08FRAExample);
        }

        public static Fra GetFraObject(string resourceName)
        {
            string resourceAsString = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resourceAsString);
            XmlNameTable xmlNameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            xmlNamespaceManager.AddNamespace("fpml", "http://www.fpml.org/FpML-5/reporting");
            XmlNode swapNode = xmlDocument.SelectSingleNode("//fpml:fra", xmlNamespaceManager);
            var fra = XmlSerializerHelper.DeserializeNode<Fra>(swapNode);
            return fra;
        }
    }
}