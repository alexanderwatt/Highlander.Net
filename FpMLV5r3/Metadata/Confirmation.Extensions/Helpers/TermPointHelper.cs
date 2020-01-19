#region Using directives

using System;

#endregion

namespace nab.QDS.FpML.V47
{

    public class TermPointHelper
    {
        public static DateTime GetDate(TermPoint termPoint)
        {
            DateTime date = XsdClassesFieldResolver.TimeDimension_GetDate(termPoint.term);

            return date;
        }
    }
}