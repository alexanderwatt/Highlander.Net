#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
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