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

using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class InformationSourceHelper
    {
        public static InformationSource Create(string provider, string rateSource, string rateSourcePage)
        {
            var informationSource = new InformationSource
                                        {
                                            rateSource = InformationProviderHelper.Create(provider),
                                            rateSourcePage = RateSourcePageHelper.Create(rateSourcePage)
                                        };
            return informationSource;
        }

        public static InformationSource Create(string provider)
        {
            var informationSource = new InformationSource
                                        {
                                            rateSource = InformationProviderHelper.Create(provider)
                                        };
            return informationSource;
        }

        public static InformationSource[] CreateArray(string provider)
        {
            var informationSource = new InformationSource
            {
                rateSource = InformationProviderHelper.Create(provider)
            };
            var result = new[] {informationSource};
            return result;
        }

        public static InformationSource[] Create(InformationSource informationSource)
        {
            var informationSources = new InformationSource[1];
            informationSources[0] = informationSource;
            return informationSources;
        }

        public static InformationSource Copy(InformationSource baseSource)
        {
            InformationSource result = null;
            if (baseSource != null)
            {
                result = new InformationSource();
                if(baseSource.rateSource != null)
                {
                    result.rateSource = InformationProviderHelper.Create(baseSource.rateSource.Value);
                }
                if(baseSource.rateSourcePage != null)
                {
                    result.rateSourcePage = RateSourcePageHelper.Create(baseSource.rateSourcePage.Value);
                }
                if(baseSource.rateSourcePageHeading != null)
                {
                    result.rateSourcePageHeading = baseSource.rateSourcePageHeading;
                }               
            }
            return result;
        }

        public static List<InformationSource> Copy(InformationSource[] baseSources)
        {
            List<InformationSource> result = null;
            if (baseSources != null)
            {
                result = baseSources.Select(Copy).ToList();
            }
            return result;
        }
    }
}