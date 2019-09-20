/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

#endregion

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Retrieves resources
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadResourceValue(string file, string key)
        {
            //value for our return value
            string resourceValue;
            try
            {
                // specify your resource file name 
                string resourceFile = file;
                var resourceManager = new ResourceManager(resourceFile, Assembly.GetExecutingAssembly());
                resourceValue = resourceManager.GetString(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                resourceValue = string.Empty;
            }
            return resourceValue;
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        public static string GetResource(Assembly assembly, string resourceName)
        {
            // ReSharper disable AssignNullToNotNullAttribute
            using (TextReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            // ReSharper restore AssignNullToNotNullAttribute
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="shortResourceName">Short name of the resource.</param>
        /// <param name="deriveNamespaceFromThisType">Type of the derive namespace from this.</param>
        /// <returns></returns>
        public static string GetResource(Assembly assembly, string shortResourceName, Type deriveNamespaceFromThisType)
        {
            string resourceName = deriveNamespaceFromThisType.Namespace + "." + shortResourceName;
            return GetResource(assembly, resourceName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="partialResourceName"></param>
        /// <returns></returns>
        public static string GetResourceWithPartialName(Assembly assembly, string partialResourceName)
        {
            return (from resourceName in assembly.GetManifestResourceNames() where resourceName.Contains(partialResourceName) select GetResource(assembly, resourceName)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="partialResourceName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetResources(Assembly assembly, string partialResourceName)
        {
            string[] files = assembly.GetManifestResourceNames();
            Dictionary<string, string> chosenFiles
                = (from f in files
                  where f.Contains(partialResourceName)
                  select new KeyValuePair<string, string>(f, GetResource(assembly, f))).ToDictionary(a=>a.Key, a=>a.Value);

            return chosenFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetResources(Assembly assembly, string prefix, string suffix)
        {
            string[] files = assembly.GetManifestResourceNames();
            var chosenFiles
                = (from f in files
                  where f.StartsWith(prefix) && f.EndsWith(suffix)
                   select new KeyValuePair<string, string>(f, GetResource(assembly, f))).ToDictionary(a => a.Key, a => a.Value);
            return chosenFiles;
        }
    }
}
