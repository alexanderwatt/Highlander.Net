using System;
using nab.QDS.FpML.Codes;

namespace nab.QDS.FpML.V47
{
    public partial class CompoundingFrequency
    {
        public CompoundingFrequencyEnum ToEnum()
        {
            return EnumParse.ToCompoundingFrequencyEnum(this.Value);
        }

        public static CompoundingFrequency Parse(string frequency)
        {
            CompoundingFrequency result = new CompoundingFrequency { Value = frequency };

            return result;
        }

        public static CompoundingFrequency Create(CompoundingFrequencyEnum frequency)
        {
            CompoundingFrequency result
                = new CompoundingFrequency
                {
                    Value = CompoundingFrequencyScheme.GetEnumString(frequency)
                };

            return result;
        }
    }
}
