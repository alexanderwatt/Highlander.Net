namespace FpML.V5r10.Reporting.Helpers
{
    public static class AmountScheduleHelper
    {
        public static AmountSchedule Create(Currency value)
        {
            var amountSchedule = new AmountSchedule {currency = value};
            return amountSchedule;
        }

        public static AmountSchedule Create(Schedule schedule, Currency currency)
        {
            var result = new AmountSchedule
                                        {
                                            currency = currency,
                                            initialValue = schedule.initialValue,
                                            step = schedule.step
                                        };
            return result;
        }
    }
    public static class NonNegativeAmountScheduleHelper
    {
        public static NonNegativeAmountSchedule Create(Currency value)
        {
            var amountSchedule = new NonNegativeAmountSchedule { currency = value };
            return amountSchedule;
        }

        public static NonNegativeAmountSchedule Create(NonNegativeSchedule schedule, Currency currency)
        {
            var result = new NonNegativeAmountSchedule
            {
                currency = currency,
                initialValue = schedule.initialValue,
                step = schedule.step
            };
            return result;
        }
    }
}