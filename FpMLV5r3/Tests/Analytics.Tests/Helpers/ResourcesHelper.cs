/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Reflection;
using System.Resources;

namespace Highlander.Analytics.Tests.V5r3.Helpers
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