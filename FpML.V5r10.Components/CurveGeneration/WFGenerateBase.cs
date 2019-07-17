/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Contracts;

//using Orion.StoreHelper;

namespace Orion.Workflow.CurveGeneration
{
    public abstract class WFGenerateCurveBase : WorkstepBase<RequestBase, HandlerResponse>
    {
        //public string NameSpace { get; set; }

        protected ICoreItem LoadAndCheckMarketItem(ICoreCache cache, string nameSpace, string curveUniqueId)//TODO use the propper loader.
        {
            ICoreItem marketItem = cache.LoadItem<Market>(nameSpace + "." + curveUniqueId);
            if (marketItem == null)
                throw new ApplicationException("Curve '" + curveUniqueId + "' not found!");
            var market = (Market)marketItem.Data;
            if ((market.Items == null) || (market.Items.Length < 1) || (market.Items[0] == null))
                throw new ApplicationException("Curve '" + curveUniqueId + "' contains no PricingStructure!");
            if ((market.Items1 == null) || (market.Items1.Length < 1) || (market.Items1[0] == null))
                throw new ApplicationException("Curve '" + curveUniqueId + "' contains no PricingStructureValuation!");
            return marketItem;
        }
    }
}
