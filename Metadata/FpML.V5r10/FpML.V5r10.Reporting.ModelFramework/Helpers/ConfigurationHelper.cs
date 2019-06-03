using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Orion.Util.Helpers;


namespace FpML.V5r10.Reporting.ModelFramework.Helpers
{
    /// <summary>
    /// Helper for configuration
    /// </summary>
    public static class ConfigurationHelper
    {
        private static readonly IDictionary<string, System.Configuration.Configuration> _configurations = new Dictionary<string, System.Configuration.Configuration>();

        private static string _defaultCallingAssemblyConfigFilename = GetReferencingAssemblyConfigFilename();
        private static string _defaultCallingAssemblyConfigName = GetReferencingAssemblyName();

        /// <summary>
        /// Gets the name of the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyName()
        {
            StackTrace st = new StackTrace();
            return st.GetFrame(3).GetMethod().DeclaringType?.Assembly.GetName().Name;
        }

        /// <summary>
        /// Gets the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyConfigFilename()
        {
            StackTrace st = new StackTrace();
            return $"{st.GetFrame(3).GetMethod().DeclaringType?.Assembly.GetName().Name}.dll.config";
        }

        /// <summary>
        /// Gets the referencing assembly location.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyLocation()
        {
            StackTrace st = new StackTrace();
            var newLoc = ReflectionHelper.GetAssemblyCodeBaseLocation(st.GetFrame(3).GetMethod().DeclaringType?.Assembly);
            return newLoc;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="fullConfigurationFilePath">The full configuration file path.</param>
        /// <returns></returns>
        public static Configuration GetConfiguration(string fullConfigurationFilePath)
        {
            Configuration configuration;
            if (_configurations != null && _configurations.ContainsKey(fullConfigurationFilePath))
            {
                configuration = _configurations[fullConfigurationFilePath];
            }
            else
            {
                lock (_configurations)
                {
                    ExeConfigurationFileMap map =
                        new ExeConfigurationFileMap
                        {
                            ExeConfigFilename = fullConfigurationFilePath
                        };
                    configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                    _configurations.Add(fullConfigurationFilePath, configuration);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static Configuration GetConfiguration()
        {
            _defaultCallingAssemblyConfigName = GetReferencingAssemblyName();
            _defaultCallingAssemblyConfigFilename = GetReferencingAssemblyConfigFilename();
            string defaultCallingAssemblyLocation = GetReferencingAssemblyLocation();
            string keyName = $"{_defaultCallingAssemblyConfigName}Location";
            string overrideLocation = GetConfigurationKeyVal(keyName);
            string targetLocation = overrideLocation.Length > 0 ? overrideLocation : defaultCallingAssemblyLocation;
            string fullConfigurationFilePath =
                $@"{targetLocation}\{_defaultCallingAssemblyConfigFilename}";
            Configuration configuration;
            lock (_configurations)
            {
                if (_configurations.ContainsKey(fullConfigurationFilePath))
                {
                    configuration = _configurations[fullConfigurationFilePath];
                }
                else
                {
                    lock (_configurations)
                    {
                        ExeConfigurationFileMap map =
                            new ExeConfigurationFileMap
                            {
                                ExeConfigFilename = fullConfigurationFilePath
                            };
                        configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                        _configurations.Add(fullConfigurationFilePath, configuration);
                    }
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
        public static string GetAppSettingsSectionEntry(System.Configuration.Configuration configuration, string sectionName, string keyName)
        {
            string result = string.Empty;
            ConfigurationSection configSection = configuration.GetSection(sectionName);
            if (configSection != null)
            {
                AppSettingsSection section = (AppSettingsSection)configuration.GetSection(sectionName);
                if (section?.Settings?[keyName] == null) return result;
                var item = section.Settings[keyName];
                result = item.Value;
            }
            return result;
        }

        internal static string GetConfigurationKeyVal(string key)
        {
            string retval = string.Empty;
            string keyVal = key;
            if (ConfigurationManager.AppSettings[keyVal] != null)
                retval = ConfigurationManager.AppSettings[keyVal];
            return retval;
        }
    }
}
