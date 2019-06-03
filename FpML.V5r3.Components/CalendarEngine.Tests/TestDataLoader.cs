using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using National.QRSC.Constants;
using National.QRSC.FpML.V47;
using National.QRSC.FpML.V47.Codes;
using National.QRSC.Runtime.Common;
using National.QRSC.Utility.Expressions;
using National.QRSC.Utility.Helpers;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.NamedValues;
using National.QRSC.Utility.Serialisation;

namespace Orion.CalendarEngine.Tests
{
    public static class TestDataLoader
    {
        private const string ResourcePath = "CalendarEngine.Tests.ConfigurationData";
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
                throw new InvalidOperationException(String.Format("No {0} files found!", typeof(T).Name));

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                T curve = XmlSerializerHelper.DeserializeFromString<T>(file.Value);
                //string nvs = ResourceHelper.GetResource(assembly, file.Key.Replace(".xml", ".nvs"));
                //NamedValueSet itemProps = new NamedValueSet(nvs);
                // strip assempbly prefix
                string itemName = file.Key.Substring(ResourcePath.Length + 1);
                // strip file extension
                itemName = itemName.Substring(0, itemName.Length - 4);
                logger.LogDebug("  {0} ...", itemName);
                client.SaveObject<T>(curve, itemName, null, TimeSpan.MaxValue);
            } // foreach file
            logger.LogDebug("Loaded {0} {1} files...", chosenFiles.Count(), typeof(T).Name);
        }

        public static void Load(ILogger logger, ICoreCache client)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            LoadFiles<DateRules>(logger, client, assembly, "DateRules");
            LoadHolidays(logger, client, assembly);
            //LoadFiles<Trade>(logger, client, assembly, "Calypso.Trade");
            //LoadFiles<Trade>(logger, client, assembly, "Murex.Trade");

            //LoadGWMLTrades(logger, client, assembly, "Calypso");
            //LoadGWMLTrades(logger, client, assembly, "Murex");
        }

        private static void LoadHolidays(ILogger logger, ICoreCache client, Assembly assembly)
        {
            const string FileName = "Holidays.xml";
            string holidaysXml = ResourceHelper.GetResourceWithPartialName(assembly, FileName);
            // load daterules from file
            LocationCalendarYears calendarYears = XmlSerializerHelper.DeserializeFromString<LocationCalendarYears>(holidaysXml);
            foreach (LocationCalendarYear locationCalendarYear in calendarYears.LocationCalendarYear)
            {
                string name;
                NamedValueSet itemProps = CreateProperties(locationCalendarYear, out name);
                client.SaveObject<LocationCalendarYear>(locationCalendarYear, name, itemProps, TimeSpan.MaxValue);
            }
            logger.LogDebug("Loaded holidays.");
        }

        private static NamedValueSet CreateProperties(LocationCalendarYear locationCalendarYear, out string name)
        {
            NamedValueSet itemProps = new NamedValueSet();
            itemProps.Set("Year", locationCalendarYear.Year);
            itemProps.Set("BusinessCenter", locationCalendarYear.BusinessCenter);
            itemProps.Set("RDMLocation", locationCalendarYear.RDMLocation);
            itemProps.Set(CurveProp.DataGroup, "Highlander.Configuration.RDMHolidays");
            itemProps.Set("SourceSystem", "RDM");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", "RDMHolidays");
            string itemName = string.Format("Highlander.ReferenceData.RDMHolidays.{0}.{1}", locationCalendarYear.BusinessCenter, locationCalendarYear.Year);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            name = itemName;
            return itemProps;
        }
    }
}
