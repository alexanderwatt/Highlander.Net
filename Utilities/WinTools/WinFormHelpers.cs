using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace Orion.UI.WinTools
{
    public static class WinFormHelper
    {
        private static string _InitFormText = null;
        public static void SetAppFormTitle(Form form, string envName)
        {
            if (_InitFormText == null)
                _InitFormText = form.Text;
            string applFileVersion = "1.0.0.0";
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                string[] parts = assembly.FullName.Split(',');
                foreach (object attr in assembly.GetCustomAttributes(true))
                    if (attr is AssemblyFileVersionAttribute)
                        applFileVersion = ((AssemblyFileVersionAttribute)attr).Version;
            }
            else
            {
                // entry assembly is unmanaged - get Win32 details
                ProcessModule pm = Process.GetCurrentProcess().MainModule;
                applFileVersion = pm.FileVersionInfo.FileVersion;
            }
            if (envName != null)
                form.Text = String.Format("{0} V{1} ({2})", _InitFormText, applFileVersion, envName);
            else
                form.Text = String.Format("{0} V{1}", _InitFormText, applFileVersion);
        }

    }
}
