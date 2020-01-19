#region Using directives

using System;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class RoundingHelper
    {
        public static Decimal Round(Decimal valueToRound, Rounding rounding)
        {
            int decimals = int.Parse(rounding.precision);

            Decimal result;

            switch(rounding.roundingDirection)
            {
                case RoundingDirectionEnum.Up:
                    {
                        throw new NotImplementedException();
                    }
                case RoundingDirectionEnum.Down:
                    {
                        throw new NotImplementedException();
                    }
                case RoundingDirectionEnum.Nearest:
                    {
                        result = Decimal.Round(valueToRound, decimals);

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result; 
        }
        
    }
}