#region Using directives

using System;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class SwaptionFactory
    {
        public static Swaption Create(Swap swap, decimal premiumAmount, DateTime expirationDate)
        {
            var swaption = new Swaption {swap = swap, premium = new[] {new Payment()}};
            swaption.premium[0].paymentAmount = MoneyHelper.GetAmount(premiumAmount);
            var europeanExercise = new EuropeanExercise
                                       {
                                           expirationDate = new AdjustableOrRelativeDate()
                                       };
            var adjustableDate = new AdjustableDate
                                     {
                                         unadjustedDate = new IdentifiedDate
                                                              {Value = expirationDate}
                                     };
            europeanExercise.expirationDate.Item = adjustableDate;
            XsdClassesFieldResolver.Swaption_SetEuropeanExercise(swaption, europeanExercise);           
            return swaption;
        }

        public static Swaption Create(Swap swap, Money premium, string premiumPayer, string premiumReceiver,
                                      AdjustableDate paymentDate,
                                      AdjustableDate expirationDate,
                                      DateTime earliestExerciseTime, DateTime expirationTime, bool automaticExercise)
        {
            var swaption = new Swaption {swap = swap, premium = new[] {new Payment()}};
            swaption.buyerPartyReference = PartyOrTradeSideReferenceHelper.ToPartyOrTradeSideReference(premiumPayer);
            swaption.sellerPartyReference = PartyOrTradeSideReferenceHelper.ToPartyOrTradeSideReference(premiumReceiver);
            swaption.premium[0].paymentAmount = premium;
            swaption.premium[0].paymentDate = paymentDate;
            swaption.premium[0].payerPartyReference = PartyOrAccountReferenceFactory.Create(premiumPayer);
            swaption.premium[0].receiverPartyReference = PartyOrAccountReferenceFactory.Create(premiumReceiver);
            var europeanExercise = new EuropeanExercise
                                       {
                                           expirationDate = new AdjustableOrRelativeDate(),
                                           earliestExerciseTime = BusinessCenterTimeHelper.Create(earliestExerciseTime),
                                           expirationTime = BusinessCenterTimeHelper.Create(expirationTime)
                                       };
            europeanExercise.expirationDate.Item = expirationDate;
            swaption.exerciseProcedure = new ExerciseProcedure();
            if (automaticExercise)
            {
                XsdClassesFieldResolver.ExerciseProcedure_SetAutomaticExercise(swaption.exerciseProcedure, new AutomaticExercise());
            }
            else//manual exercise
            {
                XsdClassesFieldResolver.ExerciseProcedure_SetManualExercise(swaption.exerciseProcedure, new ManualExercise());
            }
            XsdClassesFieldResolver.Swaption_SetEuropeanExercise(swaption, europeanExercise);
            return swaption;
        }

        public static Swaption Create(Swap swap, Payment[] premium, EuropeanExercise exercise, bool automaticExercise)
        {
            var swaption = new Swaption { swap = swap, premium = premium };
            var europeanExercise = exercise;
            swaption.exerciseProcedure = new ExerciseProcedure();
            if (automaticExercise)
            {
                XsdClassesFieldResolver.ExerciseProcedure_SetAutomaticExercise(swaption.exerciseProcedure, new AutomaticExercise());
            }
            else//manual exercise
            {
                XsdClassesFieldResolver.ExerciseProcedure_SetManualExercise(swaption.exerciseProcedure, new ManualExercise());
            }
            XsdClassesFieldResolver.Swaption_SetEuropeanExercise(swaption, europeanExercise);
            return swaption;
        }
    }
}