using System;
using System.Reflection;
using System.Resources;

namespace Orion.Analytics.Tests.Helpers
{
    static class ResourceHelper
    {
        public static string ReadResourceValue(string file, string key)
        {
            //value for our return value
            string resourceValue;
            try
            {
                // specify your resource file name 
                string resourceFile = file;
                ResourceManager resourceManager = new ResourceManager(resourceFile, Assembly.GetExecutingAssembly());
                resourceValue = resourceManager.GetString(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                resourceValue = string.Empty;
            }
            return resourceValue;
        }

    }
}