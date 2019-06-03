#region Using directives

using System;
using System.IO;
using System.Reflection;

#endregion

namespace Orion.CurveEngine
{
    ///<summary>
    /// Information about the CurveGenerator assembly.
    ///</summary>
    public static class Information
    {
        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public  static string GetVersionInfo()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var fileInfo = new FileInfo(uri.LocalPath);
            DateTime buildTime = fileInfo.LastWriteTime;
            var formattedInfo = String.Format("Version: 1.0 Built on : {0}, {1}", buildTime.ToShortDateString(), buildTime.ToShortTimeString());
            return formattedInfo;
        }
    }
}