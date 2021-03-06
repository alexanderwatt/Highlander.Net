#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class NotionalFactory
    {
        public static Notional Create(Money initialValue)
        {
            var result = new Notional
                                  {
                                      notionalStepSchedule = new NonNegativeAmountSchedule
                                                                 {
                                                                     initialValue = initialValue.amount,
                                                                     currency = initialValue.currency
                                                                 }
                                  };

            return result;
        }

        public static Notional Create(NonNegativeAmountSchedule schedule)
        {
            var result = new Notional
                             {
                                 notionalStepSchedule = schedule
            };

            return result;
        }
    }
}