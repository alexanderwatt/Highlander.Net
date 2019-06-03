using System;

namespace FpML.V5r10.Reporting
{
    public partial class Period
    {
        public new string ToString()
        {
            string s = $"{periodMultiplier}{period}";
            return s;
        }

        public bool Equals(Period periodToCompare)
        {
            if (periodToCompare == null)
                return false;
            bool result
                = (period == periodToCompare.period
                   && double.Parse(periodMultiplier) == double.Parse(periodToCompare.periodMultiplier));
            return result;
        }
    }
}
