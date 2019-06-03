using System.Collections.Generic;
using System.Diagnostics;
using Orion.Util.Helpers;


namespace Orion.ModelFramework.Helpers
{
    /// <summary>
    /// Helper for configuration
    /// </summary>
    static public class ConfigurationHelper
    {
        static private readonly IDictionary<string, System.Configuration.Configuration> _configurations = new Dictionary<string, System.Configuration.Configuration>();

        private static string _defaultCallingAssemblyConfigFilename = GetReferencingAssemblyConfigFilename();
        private static string _defaultCallingAssemblyConfigName = GetReferencingAssemblyName();

        /// <summary>
        /// Gets the name of the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyName()
        {
            StackTrace st = new StackTrace();
            return st.GetFrame(3).GetMethod().DeclaringType.Assembly.GetName().Name;
        }

        /// <summary>
        /// Gets the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyConfigFilename()
        {
            StackTrace st = new StackTrace();
            return string.Format("{0}.dll.config", st.GetFrame(3).GetMethod().DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// Gets the referencing assembly location.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyLocation()
        {
            StackTrace st = new StackTrace();

            var newLoc = ReflectionHelper.GetAssemblyCodeBaseLocation(st.GetFrame(3).GetMethod().DeclaringType.Assembly);

            return newLoc;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="fullConfigurationFilePath">The full configuration file path.</param>
        /// <returns></returns>
        static public System.Configuration.Configuration GetConfiguration(string fullConfigurationFilePath)
        {
            System.Configuration.Configuration configuration = null;
            if (ConfigurationHelper._configurations.ContainsKey(fullConfigurationFilePath))
            {
                configuration = ConfigurationHelper._configurations[fullConfigurationFilePath];
            }
            else
            {
                lock (ConfigurationHelper._configurations)
                {
                    System.Configuration.ExeConfigurationFileMap map = new System.Configuration.ExeConfigurationFileMap();
                    map.ExeConfigFilename = fullConfigurationFilePath;
                    configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, System.Configuration.ConfigurationUserLevel.None);
                    ConfigurationHelper._configurations.Add(fullConfigurationFilePath, configuration);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        static public System.Configuration.Configuration GetConfiguration()
        {
            ConfigurationHelper._defaultCallingAssemblyConfigName = ConfigurationHelper.GetReferencingAssemblyName();
            ConfigurationHelper._defaultCallingAssemblyConfigFilename = ConfigurationHelper.GetReferencingAssemblyConfigFilename();
            string defaultCallingAssemblyLocation = ConfigurationHelper.GetReferencingAssemblyLocation();

            string keyName = string.Format("{0}Location", ConfigurationHelper._defaultCallingAssemblyConfigName);
            string overrideLocation = ConfigurationHelper.GetConfigurationKeyVal(keyName);
            string targetLocation = overrideLocation.Length > 0 ? overrideLocation : defaultCallingAssemblyLocation;
            string fullConfigurationFilePath = string.Format(@"{0}\{1}", targetLocation, ConfigurationHelper._defaultCallingAssemblyConfigFilename);

            System.Configuration.Configuration configuration = null;
            if (ConfigurationHelper._configurations.ContainsKey(fullConfigurationFilePath))
            {
                configuration = ConfigurationHelper._configurations[fullConfigurationFilePath];
            }
            else
            {
                lock (ConfigurationHelper._configurations)
                {
                    System.Configuration.ExeConfigurationFileMap map = new System.Configuration.ExeConfigurationFileMap();
                    map.ExeConfigFilename = fullConfigurationFilePath;
                    configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, System.Configuration.ConfigurationUserLevel.None);
                    ConfigurationHelper._configurations.Add(fullConfigurationFilePath, configuration);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Gets the app settings section entry.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        static public string GetAppSettingsSectionEntry(System.Configuration.Configuration configuration, string sectionName, string keyName)
        {
            string result = string.Empty;
            System.Configuration.ConfigurationSection configSection = configuration.GetSection(sectionName);
            if (configSection != null)
            {
                System.Configuration.AppSettingsSection section = (System.Configuration.AppSettingsSection)configuration.GetSection(sectionName);
                System.Configuration.KeyValueConfigurationElement item = null;
                if (section != null)
                {
                    if (section.Settings != null)
                    {
                        if (section.Settings[keyName] != null)
                        {
                            item = section.Settings[keyName];
                            result = item.Value;
                        }
                    }
                }
            }
            return result;
        }

        static internal string GetConfigurationKeyVal(string key)
        {
            string retval = string.Empty;
            string keyVal = key;
            if (System.Configuration.ConfigurationManager.AppSettings[keyVal] != null)
                retval = System.Configuration.ConfigurationManager.AppSettings[keyVal];

            return retval;
        }
    }
}
