using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

namespace FpML.V5r3.TestHelpers
{
    public static class TestDataLoader
    {
        private const string ResourcePath = "TestHelpers.TestData";
        //public static string GetExpectedResults(string resourceFileName)
        //{
        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    return ResourceHelper.GetResource(assembly, ResourcePath + ".ExpectedResults." + resourceFileName);
        //}

        private static void LoadFiles<T>(
            ILogger logger, 
            ICoreCache client,
            Assembly assembly,
            string filenamePrefix) where T : class
        {
            logger.LogDebug("Loading {0} files...", typeof(T).Name);
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, ResourcePath + "." + filenamePrefix, "xml");
            if (chosenFiles.Count == 0)
                throw new InvalidOperationException($"No {typeof(T).Name} files found!");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                var curve = XmlSerializerHelper.DeserializeFromString<T>(file.Value);
                string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                var itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject(curve, itemName, itemProps, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count, typeof(T).Name);
        }

        public static void Loadqqq(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            LoadFiles<LocationCalendarYear>(logger, client, assembly, "Holidays");
            //LoadFiles<Trade>(logger, client, assembly, "Calypso.Trade");
            //LoadFiles<Trade>(logger, client, assembly, "Murex.Trade");

        }

        public static void LoadHolidays(ILogger logger, ICoreCache localCache, Assembly assembly,
            string filenamePrefix)
        {
            string holidaysXml = ResourceHelper.GetResourceWithPartialName(assembly, filenamePrefix);

            // load daterules from file
            var calendarYears = XmlSerializerHelper.DeserializeFromString<LocationCalendarYears>(holidaysXml);

            foreach (LocationCalendarYear locationCalendarYear in calendarYears.LocationCalendarYear)
            {
                string name;
                NamedValueSet itemProps = CreateProperties(locationCalendarYear, out name);
                logger.LogDebug("  {0} ...", name);
                localCache.SaveObject(locationCalendarYear, name, itemProps, TimeSpan.MaxValue);
            }

            //const string BcFileName = "BusinessCenterDateRules.xml";
            //string bcDateRulesXml = ResourceHelper.GetResourceWithPartialName(assembly, BcFileName);
            //var bcDateRules = XmlSerializerHelper.DeserializeFromString<DateRules>(bcDateRulesXml);
            //localCache.SaveObject<DateRules>(bcDateRules, "Orion.Configuration.DateRules.BusinessCenterDateRules", null, TimeSpan.MaxValue);
        }


        private static NamedValueSet CreateProperties(LocationCalendarYear locationCalendarYear, out string name)
        {
            var itemProps = new NamedValueSet();
            itemProps.Set("Year", locationCalendarYear.Year);
            itemProps.Set("BusinessCenter", locationCalendarYear.BusinessCenter);
            itemProps.Set("RDMLocation", locationCalendarYear.RDMLocation);
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration.RDMHolidays");
            itemProps.Set("SourceSystem", "RDM");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", "RDMHolidays");
            itemProps.Set("Schema", "FpML.V5r3");
            string itemName =
                $"Orion.ReferenceData.RDMHolidays.{locationCalendarYear.BusinessCenter}.{locationCalendarYear.Year}";
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            name = itemName;
            return itemProps;
        }

    }
}
