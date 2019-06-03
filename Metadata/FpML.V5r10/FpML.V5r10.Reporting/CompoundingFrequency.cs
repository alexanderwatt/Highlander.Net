using FpML.V5r10.Codes;

namespace FpML.V5r10.Reporting
{
    public partial class CompoundingFrequency
    {
        public CompoundingFrequencyEnum ToEnum()
        {
            return EnumParse.ToCompoundingFrequencyEnum(Value);
        }

        public static CompoundingFrequency Parse(string frequency)
        {
            var result = new CompoundingFrequency { Value = frequency };

            return result;
        }

        public static CompoundingFrequency Create(CompoundingFrequencyEnum frequency)
        {
            var result
                = new CompoundingFrequency
                {
                    Value = CompoundingFrequencyScheme.GetEnumString(frequency)
                };

            return result;
        }
    }
}
