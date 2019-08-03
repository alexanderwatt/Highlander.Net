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

using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace Orion.Util.WinTools
{
    public static class WinFormHelper
    {
        private static string _initFormText;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        /// <param name="envName"></param>
        public static void SetAppFormTitle(Form form, string envName)
        {
            if (_initFormText == null)
                _initFormText = form.Text;
            string applFileVersion = "1.0.0.0";
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                //string[] parts = assembly.FullName.Split(',');
                foreach (object attr in assembly.GetCustomAttributes(true))
                    if (attr is AssemblyFileVersionAttribute attribute)
                        applFileVersion = attribute.Version;
            }
            else
            {
                // entry assembly is unmanaged - get Win32 details
                ProcessModule pm = Process.GetCurrentProcess().MainModule;
                applFileVersion = pm.FileVersionInfo.FileVersion;
            }
            form.Text = envName != null ? $"{_initFormText} V{applFileVersion} ({envName})" : $"{_initFormText} V{applFileVersion}";
        }
    }
}
