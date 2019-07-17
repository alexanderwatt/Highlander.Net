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
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace National.QRSC.ModelFramework.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class UnityHelper
    {
        private const string cUnitySectioName = "unity";
        static private IUnityContainer _unityContainer = null;
        private static IDictionary<string, IUnityContainer> _unityContainers = new Dictionary<string, IUnityContainer>();
        private static IDictionary<string, UnityConfigurationSection> _unityConfigurationSections = new Dictionary<string, UnityConfigurationSection>();

        static private object _lockObject= new object();

        static private UnityConfigurationSection _unityConfigSection = new UnityConfigurationSection();
        private static IDictionary<string, UnityContainerElement> _resolvedContainers = new Dictionary<string, UnityContainerElement>();

        /// <summary>
        /// Initializes the <see cref="ClassFactory&lt;T&gt;"/> class.
        /// </summary>
        static UnityHelper()
        {
            //UnityHelper.InitializeContainer();
        }

        /// <summary>
        /// Gets the name of the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyName()
        {
            StackTrace st = new StackTrace();
            return st.GetFrame(4).GetMethod().DeclaringType.Assembly.GetName().Name;
        }

        /// <summary>
        /// Gets the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyConfigFilename()
        {
            StackTrace st = new StackTrace();
            return string.Format("{0}.dll.config", st.GetFrame(4).GetMethod().DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// Gets the referencing assembly location.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyLocation()
        {
            StackTrace st = new StackTrace();

            var oldLoc = System.IO.Path.GetDirectoryName(st.GetFrame(4).GetMethod().DeclaringType.Assembly.Location);
            var newLoc = ReflectionHelper.GetAssemblyCodeBaseLocation(st.GetFrame(4).GetMethod().DeclaringType.Assembly);

            //return newLoc;
            return newLoc;
        }

        /// <summary>
        /// Resolves the container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        static public void ResolveContainer(string containerName)
        {
            UnityHelper.InitializeContainer();
            lock (UnityHelper._resolvedContainers)
            {
                IDictionary<string, UnityContainerElement> resolvedContainers = UnityHelper._resolvedContainers;
                if (!resolvedContainers.ContainsKey(containerName))
                {
                    Boolean bContainerFound = false;
                    UnityConfigurationSection unityConfigurationSection = null;
                    foreach (UnityConfigurationSection section in UnityHelper._unityConfigurationSections.Values)
                    {
                        if (section.Containers[containerName] != null)
                        {
                            bContainerFound = true;
                            unityConfigurationSection = section;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    UnityContainerElement unityContainerElement = null;
                    IUnityContainer unityContainer = null;
                    if (bContainerFound)
                    {
                        unityContainerElement = unityConfigurationSection.Containers[containerName];
                        unityContainer = new UnityContainer();
                        unityContainerElement.Configure(unityContainer);
                        UnityHelper._unityContainers.Add(containerName, unityContainer);
                        resolvedContainers.Add(containerName, unityContainerElement);
                    }
                    else
                    {
                        unityContainer = UnityHelper._unityContainers[containerName];
                        unityContainerElement = resolvedContainers[containerName];
                        unityContainerElement.Configure(unityContainer);
                    }
                }
            }
        }

        private static readonly IDictionary<string, UnityContainerElement> _containers = new Dictionary<string, UnityContainerElement>();

        internal static UnityContainerElement GetContainer(string containerName)
        {
            if (_containers.ContainsKey(containerName))
            {
                return _containers[containerName];
            }

            UnityContainerElement unityContainerElement = null;
            UnityHelper.InitializeContainer();
            lock (UnityHelper._resolvedContainers)
            {
                IDictionary<string, UnityContainerElement> resolvedContainers = UnityHelper._resolvedContainers;
                if (!resolvedContainers.ContainsKey(containerName))
                {
                    Boolean bContainerFound = false;
                    UnityConfigurationSection unityConfigurationSection = null;
                    foreach (UnityConfigurationSection section in UnityHelper._unityConfigurationSections.Values)
                    {
                        if (section.Containers[containerName] != null)
                        {
                            bContainerFound = true;
                            unityConfigurationSection = section;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //IUnityContainer unityContainer = null;
                    if (bContainerFound)
                    {
                        unityContainerElement = unityConfigurationSection.Containers[containerName];
                        resolvedContainers.Add(containerName, unityContainerElement);
                    }
                    else
                    {
                        //unityContainer = UnityHelper._unityContainers[containerName];
                        unityContainerElement = resolvedContainers[containerName];
                    }
                }
                else
                {
                    unityContainerElement = resolvedContainers[containerName];
                }
            }

            _containers.Add(containerName, unityContainerElement);

            return unityContainerElement;
        }

        /// <summary>
        /// Initializes the container.
        /// </summary>
        private static void InitializeContainer()
        {
            string defaultCallingAssemblyConfigFilename = UnityHelper.GetReferencingAssemblyConfigFilename();
            string defaultCallingAssemblyConfigName = UnityHelper.GetReferencingAssemblyName();
            string defaultCallingAssemblyLocation  = UnityHelper.GetReferencingAssemblyLocation();

            string keyName = string.Format("{0}Location", defaultCallingAssemblyConfigName);
            string overrideLocation = UnityHelper.GetConfigurationKeyVal(keyName);
            string targetLocation = overrideLocation.Length > 0 ? overrideLocation : defaultCallingAssemblyLocation;

            string fullConfigurationFilePath = defaultCallingAssemblyConfigFilename;
            if (targetLocation.Length > 0)
            {
                fullConfigurationFilePath = string.Format(@"{0}\{1}", targetLocation, defaultCallingAssemblyConfigFilename);
            }

            if (!UnityHelper._unityConfigurationSections.ContainsKey(fullConfigurationFilePath))
            {
                lock (UnityHelper._unityConfigurationSections)
                {
                    Configuration configuration = ConfigurationHelper.GetConfiguration(fullConfigurationFilePath);
                    UnityConfigurationSection unityConfigSection = (UnityConfigurationSection)configuration.GetSection(cUnitySectioName);
                    UnityHelper._unityConfigurationSections.Add(fullConfigurationFilePath, unityConfigSection);
                }
            }
        }

        /// <summary>
        /// Resolves the specified container name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        static public T Resolve<T>(string containerName)
        {
            UnityHelper.ResolveContainer(containerName);
            return UnityHelper._unityContainers[containerName].Resolve<T>();
        }

        /// <summary>
        /// Resolves all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        static public IEnumerable<T> ResolveAll<T>(string containerName)
        {
           UnityHelper.ResolveContainer(containerName);
           return UnityHelper._unityContainers[containerName].ResolveAll<T>();
        }

        /// <summary>
        /// Containers this instance.
        /// </summary>
        /// <returns></returns>
        static public IUnityContainer Container()
        {
            return UnityHelper._unityContainer;
        }

        /// <summary>
        /// Gets the configuration key val.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        static internal string GetConfigurationKeyVal(string key)
        {
            string retval = string.Empty;
            string keyVal = key;
            if (ConfigurationManager.AppSettings[keyVal] != null)
                retval = ConfigurationManager.AppSettings[keyVal];

            return retval;
        }
    }
}
