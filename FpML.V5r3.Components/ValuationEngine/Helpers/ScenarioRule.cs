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

using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

namespace Orion.ValuationEngine.Helpers
{
    public class CachedSummary
    {
        public readonly string UniqueId;
        public readonly NamedValueSet Properties;
        public readonly ValuationReport Summary;
        public CachedSummary(string uniqueId, NamedValueSet properties, ValuationReport summary)
        {
            UniqueId = uniqueId;
            Properties = properties;
            Summary = summary;
        }
    }
}
