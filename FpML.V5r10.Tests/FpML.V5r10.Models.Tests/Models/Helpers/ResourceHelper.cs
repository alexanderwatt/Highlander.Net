using System;
using System.Reflection;
using System.Resources;

namespace Orion.V5r3.Models.Tests.Models.Helpers
{
    static class ResourceHelper
    {
        static public string ReadResourceValue(string file, string key)
        {
            //value for our return value
            string resourceValue = string.Empty;
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