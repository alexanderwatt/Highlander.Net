using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.DayCounters;
using Orion.ModelFramework;

namespace Orion.Analytics.Tests.Dates
{
    [TestClass]
    public class DayCounterHelperTest
    {
        [TestMethod]
        public void ParseTest()
        {
            IDayCounter dayCounter = DayCounterHelper.Parse("Act/360");

            Assert.IsNotNull(dayCounter);
        }
    }
}
