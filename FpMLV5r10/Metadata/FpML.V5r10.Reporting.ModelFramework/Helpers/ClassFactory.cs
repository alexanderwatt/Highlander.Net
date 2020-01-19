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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;

namespace FpML.V5r10.Reporting.ModelFramework.Helpers
{
    /// <summary>
    /// Factory for creating an instance of the supplied generic Type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClassFactory<T>
    {
        #region Static Members
        private static readonly IDictionary<string, HybridDictionary> AssemblyAssetRegistry
            = new Dictionary<string, HybridDictionary>();
        private static readonly IDictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
        private static readonly IDictionary<string, List<Type>> LoadedAssemblyTypes = new Dictionary<string, List<Type>>();
        private static readonly string DefaultCallingAssemblyName = GetReferencingAssembly();
        private static readonly List<string> RegisteredAssemblies = new List<string>();

        /// <summary>
        /// Initializes the <see cref="ClassFactory&lt;T&gt;"/> class.
        /// </summary>
        static ClassFactory()
        {
            string callingAssembly = DefaultCallingAssemblyName;
            Register(callingAssembly);
        }


        /// <summary>
        /// Gets the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssembly()
        {
            StackTrace st = new StackTrace();
            return st.GetFrame(3).GetMethod().DeclaringType?.Assembly.GetName().Name;
        }

        /// <summary>
        /// Registers the specified assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        private static void Register(string assemblyName)
        {
            if (RegisteredAssemblies.Contains(assemblyName))
            {
                return;
            }

            // Add all classes that implement the IScenario interface from the default assembly
            RegisterAssembly(assemblyName);

            RegisteredAssemblies.Add(assemblyName);
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        private static Assembly GetAssembly(string assemblyName)
        {
            Assembly assembly;
            lock (LoadedAssemblies)
            {
                IDictionary<string, Assembly> assemblies = LoadedAssemblies;
                if (!assemblies.ContainsKey(assemblyName))
                {
                    assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assemblyName, assembly);
                }
                else
                {
                    assembly = assemblies[assemblyName];
                }
            }
            return assembly;
        }

        /// <summary>
        /// Loads the assembly types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        private static void LoadAssemblyTypes(Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name;
            lock (LoadedAssemblyTypes)
            {
                IDictionary<string, List<Type>> loadedAssemblyTypes = LoadedAssemblyTypes;
                List<Type> assemblyTypes;
                if (!loadedAssemblyTypes.ContainsKey(assemblyName))
                {
                    assemblyTypes = new List<Type>(assembly.GetTypes());
                    loadedAssemblyTypes.Add(assemblyName, assemblyTypes);
                    RegisterAssemblyTypes(assemblyTypes);

                }
                else
                {
                    assemblyTypes = loadedAssemblyTypes[assemblyName];
                    RegisterAssemblyTypes(assemblyTypes);
                }
            }
        }

        /// <summary>
        /// Registers the assembly types.
        /// </summary>
        /// <param name="assemblyTypes">The assembly types.</param>
        private static void RegisterAssemblyTypes(List<Type> assemblyTypes)
        {
            Type theTypeImInterestedIn = typeof(T);

            foreach (Type assemblyType in assemblyTypes)
            {
                string name = assemblyType.Name;
                string fullName = $"{assemblyType.Namespace}.{assemblyType.Name}";
                Type[] extendedTypes = { };
                if (theTypeImInterestedIn.IsGenericType)
                {
                    extendedTypes = assemblyType.GetInterfaces();
                }
                else
                {
                    Type extendedType = assemblyType.GetInterface(theTypeImInterestedIn.Name);
                    if (extendedType != null)
                    {
                        extendedTypes = new[] { extendedType };
                    }
                }
                var baseTypes = extendedTypes.Length > 0 ? new List<Type>(extendedTypes) : GetBaseTypeList(assemblyType);
                if (baseTypes.Contains(theTypeImInterestedIn))
                {
                    string assemblyName = assemblyType.Assembly.GetName().Name;
                    HybridDictionary assetRegistry = new HybridDictionary();
                    if (AssemblyAssetRegistry.ContainsKey(assemblyName))
                    {
                        assetRegistry = AssemblyAssetRegistry[assemblyName];
                        assetRegistry[name] = new AssetRegistryValue(assemblyName, assemblyType.FullName);
                        assetRegistry[fullName] = new AssetRegistryValue(assemblyName, assemblyType.FullName);
                    }
                    else
                    {
                        assetRegistry[name] = new AssetRegistryValue(assemblyName, assemblyType.FullName);
                        assetRegistry[fullName] = new AssetRegistryValue(assemblyName, assemblyType.FullName);
                        AssemblyAssetRegistry.Add(assemblyName, assetRegistry);
                    }
                }
            }
        }

        /// <summary>
        /// Register any implementations of the IScenario interface
        /// that are in the specified assembly
        /// </summary>
        /// <param name="assemblyName">The assembly name to use when invoking</param>
        public static void RegisterAssembly(string assemblyName)
        {
            Assembly assembly = GetAssembly(assemblyName);
            LoadAssemblyTypes(assembly);
        }

        /// <summary>
        /// Casts the type of the instance to.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="convertToTypeName">Name of the convert to type.</param>
        /// <returns></returns>
        public static object CastInstanceToType(object instance, string assemblyName, string nameSpace, string convertToTypeName)
        {
            Type theTypeToConvertTo = instance.GetType();
            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                if (assetRegistry.Contains(convertToTypeName))
                {
                    AssetRegistryValue registryValue = (AssetRegistryValue)assetRegistry[convertToTypeName];
                    Assembly assembly = GetAssembly(registryValue.Assembly);
                    convertToTypeName = $"{nameSpace}.{convertToTypeName}";
                    theTypeToConvertTo = assembly.GetType(convertToTypeName);
                }
            }
            return Convert.ChangeType(instance, theTypeToConvertTo);
        }

        /// <summary>
        /// Remove a class from the Pricing model Factory
        /// </summary>
        /// <param name="className">The name of the class to unregister</param>
        public static void UnregisterAssets(string className)
        {
            foreach (string assemblyName in AssemblyAssetRegistry.Keys)
            {
                UnregisterAssets(assemblyName, className);
            }
        }

        /// <summary>
        /// Gets the type of the asset.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Type GetAssetType(string assemblyName, string name)
        {
            Type theType=null;
            Register(assemblyName);

            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                if (assetRegistry.Contains(name))
                {
                    AssetRegistryValue registryValue = (AssetRegistryValue)assetRegistry[name];
                    Assembly assembly = GetAssembly(registryValue.Assembly);
                    theType = assembly.GetType(name);
                }
            }
            return theType;
        }


        /// <summary>
        /// Unregisters the assets.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public static void UnregisterAssets(string assemblyName, string className)
        {
            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                if (assetRegistry.Contains(className))
                {
                    assetRegistry.Remove(className);
                }
            }
        }

        /// <summary>
        /// Empty this factory of all registered classes
        /// </summary>
        public static void UnregisterAllAssets()
        {
            foreach (string assemblyName in AssemblyAssetRegistry.Keys)
            {
                UnregisterAllAssets(assemblyName);
            }
        }

        /// <summary>
        /// Unregisters all assets.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        public static void UnregisterAllAssets(string assemblyName)
        {
            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                assetRegistry.Clear();
            }
        }

        /// <summary>
        /// List all currently registered scenario classes
        /// </summary>
        /// <returns></returns>
        public static string[] ListAssets()
        {
            List<string> allAssets = new List<string>();
            foreach (string assemblyName in AssemblyAssetRegistry.Keys)
            {
                List<string> assemblyAssets = ListAssets(assemblyName);
                allAssets.AddRange(assemblyAssets);
            }
            string[] allAssetsArray = new string[allAssets.Count];
            allAssets.CopyTo(allAssetsArray, 0);
            return allAssetsArray;
        }

        /// <summary>
        /// Lists the assets.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public static List<string> ListAssets(string assemblyName)
        {
            List<string> allAssets = new List<string>();
            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];

                string[] assetArray = new string[assetRegistry.Keys.Count];
                assetRegistry.Keys.CopyTo(assetArray, 0);
                allAssets = new List<string>(assetArray);
            }
            return allAssets;
        }


        /// <summary>
        /// Gets the base type list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static List<Type> GetBaseTypeList(Type type)
        {
            if (type != null)
            {
                // Recursive method call
                List<Type> baseTypes = GetBaseTypeList(type.BaseType);
                if (baseTypes.Count <= 0)
                {
                    List<Type> types = new List<Type> {type};
                    return types;
                }
                baseTypes.Add(type);
                return baseTypes;
            }
            return (new List<Type>());
        }

        /// <summary>
        /// Creates the specified assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T Create(string assemblyName, string name)
        {
            Register(assemblyName);
            object typeToCreate = null;
            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                if (assetRegistry.Contains(name))
                {
                    AssetRegistryValue registryValue = (AssetRegistryValue)assetRegistry[name];
                    Assembly assembly = GetAssembly(registryValue.Assembly);
                    typeToCreate = assembly.CreateInstance(registryValue.FullName);
                }
            }
            return (T)typeToCreate;
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T Create(string name)
        {
            string assemblyName = DefaultCallingAssemblyName;
            Register(assemblyName);
            object typeToCreate = null;

            if (AssemblyAssetRegistry.ContainsKey(assemblyName))
            {
                HybridDictionary assetRegistry = AssemblyAssetRegistry[assemblyName];
                if (assetRegistry.Contains(name))
                {
                    AssetRegistryValue registryValue = (AssetRegistryValue)assetRegistry[name];
                    Assembly assembly = GetAssembly(registryValue.Assembly);
                    typeToCreate = assembly.CreateInstance(registryValue.FullName);
                }
            }
            return (T)typeToCreate;
        }

        #endregion
    }

    #region Inner Classes
    class AssetRegistryValue
    {
        public string Assembly { get; }

        public string FullName { get; }

        public AssetRegistryValue(string assembly, string name)
        {
            Assembly = assembly;
            FullName = name;
        }
    }

    #endregion
}
