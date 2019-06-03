#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using nab.QDS.FpML.V47;
using nab.QDS.FpML.Codes;

#endregion

namespace Orion.Configuration
{
    public static class ConfigDataLoader
    {
        private class ItemInfo
        {
            public string ItemName;
            public NamedValueSet ItemProps;
        }

        private static ItemInfo StandardConfigProps(string type, string idSuffix)
        {
            var itemProps = new NamedValueSet();
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration." + type);
            itemProps.Set("SourceSystem", "Orion");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", type);
            string itemName = "Orion.Configuration." + type + (idSuffix != null ? "." + idSuffix : null);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };
        }

        public static void LoadInstrumentsConfig(ILogger logger, ICoreCache targetClient)
        {
            string xml = GetXml("Orion.Configuration.Config.Instruments.xml");

            var assetSet = XmlSerializerHelper.DeserializeFromString<AssetSetConfiguration>(xml);
            foreach (Instrument instrument in assetSet.Instruments)
            {
                string id = instrument.Currency.Value + '-' + instrument.AssetType;
                if (instrument.ExtraItem != null)
                    id = id + "." + instrument.ExtraItem;

                ItemInfo itemInfo = StandardConfigProps("Instrument", instrument.AssetType + '.' + id);
                itemInfo.ItemProps.Set(CurveProp.Currency1, instrument.Currency.Value);
                itemInfo.ItemProps.Set("AssetType", instrument.AssetType);
                itemInfo.ItemProps.Set("AssetId", id);
                if (instrument.ExtraItem != null)
                    itemInfo.ItemProps.Set("ExtraItem", instrument.ExtraItem);
                targetClient.SaveObject(instrument, itemInfo.ItemName, itemInfo.ItemProps);
            }
            logger.LogDebug("Loaded instrument configs.");
        }

        private static ItemInfo HolidayDatesConfigProps(LocationCalendarYear locationCalendarYear)
        {
            var itemProps = new NamedValueSet();
            itemProps.Set("Year", locationCalendarYear.Year);
            itemProps.Set("BusinessCenter", locationCalendarYear.BusinessCenter);
            itemProps.Set("RDMLocation", locationCalendarYear.RDMLocation);
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration.RDMHolidays");
            itemProps.Set("SourceSystem", "RDM");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", "RDMHolidays");
            string itemName = string.Format("Orion.ReferenceData.RDMHolidays.{0}.{1}", locationCalendarYear.BusinessCenter, locationCalendarYear.Year);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };

        }

        private static ItemInfo BusinessCenterHolidayConfigProps(BusinessCenterCalendar locationCalendarYear)
        {
            var itemProps = new NamedValueSet();
            itemProps.Set("BusinessCenter", locationCalendarYear.BusinessCenter);
            itemProps.Set("Location", locationCalendarYear.Location);
            itemProps.Set(CurveProp.DataGroup, "Orion.Configuration.BusinessCenterHolidays");
            itemProps.Set("SourceSystem", "Orion");
            itemProps.Set("Function", "Configuration");
            itemProps.Set("Type", "BusinessCenterHolidays");
            string itemName = string.Format("Orion.ReferenceData.BusinessCenterHolidays.{0}", locationCalendarYear.BusinessCenter);
            itemProps.Set(CurveProp.UniqueIdentifier, itemName);
            return new ItemInfo { ItemName = itemName, ItemProps = itemProps };

        }

        /// <summary>
        /// Moving to the new algorithm name!
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="targetClient"></param>
        public static void LoadPricingStructureAlgorithm(ILogger logger, ICoreCache targetClient)
        {
            string xml = GetXml("Orion.Configuration.Config.PricingStructureAlgorithms.xml");

            var pricingStructureTypes = XmlSerializerHelper.DeserializeFromString<PricingStructureTypes>(xml);
            //ItemInfo itemInfo = StandardConfigProps("PricingStructureAlgorithms", null);
            //targetClient.SaveObject<PricingStructureTypes>(pricingStructureTypes, itemInfo.ItemName, itemInfo.ItemProps);
            //logger.LogDebug("Loaded pricing structure types.");

            foreach (var pst in pricingStructureTypes.PricingStructureType)
            {
                foreach (var algo in pst.Algorithms)
                {
                    var itemInfo = StandardConfigProps("Algorithm", pst.id + "." + algo.id);
                    targetClient.SaveObject(algo, itemInfo.ItemName, itemInfo.ItemProps);
                    logger.LogDebug("Loaded Algorithm: {0}", itemInfo.ItemName);
                }
            }
        }

        public static void LoadBoundaryRider(ILogger logger, ICoreCache targetClient)
        {
            string xml = GetXml("Orion.Configuration.Config.BoundaryRiderMappingVals.xml");

            var boundaryRiderMappings = XmlSerializerHelper.DeserializeFromString<BoundaryRiderMappings>(xml);
            ItemInfo itemInfo = StandardConfigProps("BoundaryRiderMappingVals", null);
            targetClient.SaveObject(boundaryRiderMappings, itemInfo.ItemName, itemInfo.ItemProps);
            logger.LogDebug("Loaded BoundaryRider mappings.");
        }

        public static void LoadHolidayDates(ILogger logger, ICoreCache targetClient)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.HolidayDates";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0) throw new InvalidOperationException("Missing Date Rules");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                // load daterules from file
                var calendarYears = XmlSerializerHelper.DeserializeFromString<LocationCalendarYears>(file.Value);
                foreach (LocationCalendarYear locationCalendarYear in calendarYears.LocationCalendarYear)
                {
                    ItemInfo itemInfo = HolidayDatesConfigProps(locationCalendarYear);
                    targetClient.SaveObject(locationCalendarYear, itemInfo.ItemName, itemInfo.ItemProps);
                }
            }
            logger.LogDebug("Loaded holiday dates.");
        }

        public static void LoadNewHolidayDates(ILogger logger, ICoreCache targetClient)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.HolidayDates";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0) throw new InvalidOperationException("Missing Date Rules");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                // load daterules from file
                var calendarYears = XmlSerializerHelper.DeserializeFromString<LocationCalendarYears>(file.Value);
                var businessCenterList = new Dictionary<string, BusinessCenterCalendar>();
                
                foreach (LocationCalendarYear locationCalendarYear in calendarYears.LocationCalendarYear)
                {
                    BusinessCenterCalendar result;
                    var contains = businessCenterList.TryGetValue(locationCalendarYear.BusinessCenter, out result);
                    if (!contains)
                    {
                        var bc = new BusinessCenterCalendar
                                     {
                                         BusinessCenter = locationCalendarYear.BusinessCenter,
                                         Location = locationCalendarYear.RDMLocation,
                                         LocationName = locationCalendarYear.LocationName,
                                         Holidays = locationCalendarYear.Holidays
                                     };
                        businessCenterList.Add(bc.BusinessCenter, bc);
                    }
                    else
                    {
                        if (locationCalendarYear.Holidays != null)// && result.Holidays != null)
                        {
                            var holidays = new List<DateRule>();
                            if (result.Holidays != null)
                            {
                                holidays = new List<DateRule>(result.Holidays);
                            }
                            holidays.AddRange(locationCalendarYear.Holidays);//Test if null
                            result.Holidays = holidays.ToArray();
                        }
                    }
                }
                foreach (var center in businessCenterList)
                {
                    ItemInfo itemInfo = BusinessCenterHolidayConfigProps(center.Value);
                    targetClient.SaveObject(center.Value, itemInfo.ItemName, itemInfo.ItemProps);
                    logger.LogDebug("Loaded business center holiday dates: {0}", itemInfo.ItemName);
                }
            }
            logger.LogDebug("Loaded business center holiday dates.");
        }

        public static void LoadDateRules(ILogger logger, ICoreCache targetClient)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.DateRules";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0) throw new InvalidOperationException("Missing Date Rules");

            foreach (KeyValuePair<string, string> file in chosenFiles)
            {
                // load daterules from file
                var dateRules = XmlSerializerHelper.DeserializeFromString<DateRules>(file.Value);
                string key = file.Key.Replace(".xml", "").Split('.').Last();
                ItemInfo itemInfo = StandardConfigProps("DateRules", key);
                targetClient.SaveObject(dateRules, itemInfo.ItemName, itemInfo.ItemProps);
            }
            logger.LogDebug("Loaded date rules.");
        }

        public static void LoadGwml(ILogger logger, ICoreCache targetClient)
        {
            string xml = GetXml("Orion.Configuration.Config.GwmlEnumMaps.xml");

            var enumMaps = XmlSerializerHelper.DeserializeFromString<EnumMaps>(xml);
            ItemInfo itemInfo = StandardConfigProps("GwmlEnumMaps", null);
            targetClient.SaveObject(enumMaps, itemInfo.ItemName, itemInfo.ItemProps);
            logger.LogDebug("Loaded GWML enum maps.");
        }

        public static void LoadFpml(ILogger logger, ICoreCache targetClient)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string prefix = "Orion.Configuration.Config.FpMLCodes";
            Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, prefix, "xml");
            if (chosenFiles.Count == 0) throw new InvalidOperationException("Missing FpML Code Lists");

            foreach (var file in chosenFiles)
            {
                var data = XmlSerializerHelper.DeserializeFromString<CodeListDocument>(file.Value);
                string classDefName = GetClassDefName(data.Identification.ShortName);

                // determine primary key
                string primaryKey = null;
                foreach (Key key in data.ColumnSet.Key)
                {
                    if (key.Id == "PrimaryKey")
                    {
                        if (primaryKey != null)
                            throw new ApplicationException("PrimaryKey defined more than once!");
                        primaryKey = key.ColumnRef[0].Ref;
                    }
                }
                if (primaryKey == null)
                    throw new ApplicationException("PrimaryKey is not defined!");

                // load rows
                IFpMLCodeScheme scheme = FpMLCodeSchemeFactory.CreateCodeScheme(classDefName);
                foreach (Row row in data.SimpleCodeList.Row)
                {
                    IFpMLCodeValue codeValue = scheme.CreateCodeValue(row);
                    if (scheme is DayCountFractionScheme)
                    {
                        var dcfValue = (DayCountFractionValue)codeValue;
                        if (dcfValue.HLClassName == null)
                            dcfValue.HLClassName = "DefaultClassName";
                    }
                    scheme.AddCodeValue(codeValue);
                }
                targetClient.SaveTypedObject(scheme.GetType(), scheme, scheme.GetItemName(null), scheme.GetItemProps());
            } // foreach file
            logger.LogDebug("Loaded FpML code lists.");
        }

        private static string GetXml(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string xml = ResourceHelper.GetResource(assembly, name);
            return xml;
        }

        private static string GetClassDefName(string schemeName)
        {
            if (!schemeName.EndsWith("Scheme"))
                throw new ApplicationException(
                    String.Format("Cannot derive class def name from '{0}'.", schemeName));

            string result = schemeName.Substring(0, (schemeName.Length - 6));
            result = result[0].ToString(CultureInfo.InvariantCulture).ToUpper() + result.Substring(1);
            return result;
        }
    }
}
