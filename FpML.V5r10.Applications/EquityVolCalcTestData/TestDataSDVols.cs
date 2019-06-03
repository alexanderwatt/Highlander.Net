using System;
using Orion.Util.Serialisation;

namespace Orion.EquitiesVolCalc.TestData
{
    public static class TestDataHelper
    {
        public static EquityVolCalcTestData.Stock GetStock(string stockName)
        {
            string fileName = $"{stockName}_SD_Vol_20090909";
            string file = Properties.Resources.ResourceManager.GetString(fileName);
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentException($"stockName {stockName} not found in resources");
            }
            EquityVolCalcTestData.Stock stock = XmlSerializerHelper.DeserializeFromString<EquityVolCalcTestData.Stock>(file);
            return stock;
        }
    }
}
