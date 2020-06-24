/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using Highlander.Core.Common;
using Highlander.Utilities.Logging;

#endregion

namespace Highlander.Configuration.Data.V5r3
{
    public static class LoadBasicConfigDataHelper
    {
        public static void Load(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            //Load basic config
            PricingStructureLoader.LoadPricingStructures(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadInstrumentsConfig(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadDateRules(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadNewHolidayDates(logger, targetClient, nameSpace);
            ConfigDataLoader.LoadPricingStructureAlgorithm(logger, targetClient, nameSpace);
            MarketLoader.Load(logger, targetClient, nameSpace);
        }
    }
}
