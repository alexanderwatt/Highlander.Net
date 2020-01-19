using System;
using System.Globalization;
using Core.V34;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Orion.WebViewer
{
    public static class Common
    {
        internal static readonly CoreClientFactory Factory = new CoreClientFactory(Reference<ILogger>.Create(new TraceLogger(true)));

        public static string FormatDate(object dateItem)
        {
            var date = (DateTime?)dateItem;
            if (date.HasValue)
            {
                return date.Value.ToString(Resources.UserInteface.DateFormat, CultureInfo.CurrentCulture);
            }
            return "";
        }

        public static string FormatDateTime(object dateItem)
        {
            var date = (DateTime?)dateItem;
            if (date.HasValue)
            {
                return date.Value.ToString(Resources.UserInteface.DateTimeFormat, CultureInfo.CurrentCulture);
            }
            return "";
        }
    }
}