#region Using directives

using System;

#endregion

namespace FpML.V5r3.Confirmation
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