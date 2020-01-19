/*
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
using Highlander.Utilities.Serialisation;

namespace Highlander.EquitiesCalculator.TestData.V5r3
{
    public static class TestDataHelper
    {
        public static EquityCalculator.TestData.V5r3.Stock GetStock(string stockName)
        {
            string fileName = $"{stockName}_SD_Vol_20090909";
            string file = Properties.Resources.ResourceManager.GetString(fileName);
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentException($"stockName {stockName} not found in resources");
            }
            var stock = XmlSerializerHelper.DeserializeFromString<EquityCalculator.TestData.V5r3.Stock>(file);
            return stock;
        }
    }
}
