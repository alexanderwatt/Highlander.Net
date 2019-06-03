using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace Orion.Identifiers.Tests
{
    [TestClass]
    public class PropertyHelperTests
    {
        [TestMethod]
        public void ExtractPricingStructureTypeTest()
        {
            NamedValueSet namedValueSet = new NamedValueSet();
            namedValueSet.Set("PricingStructureType", "XccySpreadCurve");

            PricingStructureTypeEnum id = PropertyHelper.ExtractPricingStructureType(namedValueSet);

            Assert.AreEqual(PricingStructureTypeEnum.XccySpreadCurve, id);
        }
    }
}
