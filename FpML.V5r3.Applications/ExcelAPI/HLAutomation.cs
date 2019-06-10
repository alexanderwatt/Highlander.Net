/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace HLV5r3
{
    public abstract class UdfBase
    {
        [ComRegisterFunction]
        public static void ComRegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(
                GetClsIdSubKeyName(type, "Programmable"));
            // Solves an intermittent issue where Excel
            // reports that it cannot find mscoree.dll
            // Register the full path to mscoree.dll.
            var key = Registry.ClassesRoot.OpenSubKey(
                GetClsIdSubKeyName(type, "InprocServer32"), true);
            key?.SetValue("",
                $"{Environment.SystemDirectory}\\mscoree.dll",
                RegistryValueKind.String);
        }

        [ComUnregisterFunction]
        public static void ComUnregisterFunction(Type type)
        {
            // Adds the "Programmable" registry key under CLSID
            Registry.ClassesRoot.DeleteSubKey(
                GetClsIdSubKeyName(type, "Programmable"));
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="type"></param>
        //[ComRegisterFunction]
        //public static void RegisterFunction(Type type)
        //{
        //    Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
        //    RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
        //    key?.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="type"></param>
        //[ComUnregisterFunction]
        //public static void UnregisterFunction(Type type)
        //{
        //    Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        //}

        private static string GetClsIdSubKeyName(Type type, String subKeyName)
        {
            return $"CLSID\\{{{type.GUID.ToString().ToUpper()}}}\\{subKeyName}";
        }

        // Hiding these methods from Excel
        [ComVisible(false)]
        public override string ToString()
        {
            return base.ToString();
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
