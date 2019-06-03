using System.Collections.Generic;
using System.Linq;

namespace nab.QDS.FpML.V47
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