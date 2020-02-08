using System;

namespace nab.QDS.FpML.V47
{
    public partial class Asset
    {
        public Period ToPeriod()
        {
            string[] nameParts = id.Split('-');
            if (nameParts.Length < 3)
            {
                throw new ArgumentException("UnderlyingAsset Id must have at least three parts separated by '-'");
            }
            string termPart = nameParts[2];
            return PeriodHelper.Parse(termPart);
        }
    }
}
