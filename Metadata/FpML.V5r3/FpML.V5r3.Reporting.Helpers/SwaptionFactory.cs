/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class SwaptionFactory
    {
        public static Swaption Create(Swap swap, decimal premiumAmount, DateTime expirationDate)
        {
            var swaption = new Swaption {swap = swap, premium = new[] {new Payment()}};
            swaption.premium[0].paymentAmount = MoneyHelper.GetNonNegativeAmount(premiumAmount);
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
            XsdClassesFieldResolver.SwaptionSetEuropeanExercise(swaption, europeanExercise);           
            return swaption;
        }

        public static Swaption Create(Swap swap, NonNegativeMoney premium, string premiumPayer, string premiumReceiver,
                                      AdjustableOrAdjustedDate paymentDate,
                                      AdjustableDate expirationDate,
                                      DateTime earliestExerciseTime, DateTime expirationTime, bool automaticExercise)
        {
            var swaption = new Swaption
                {
                    swap = swap,
                    premium = new[] {new Payment()},
                    buyerPartyReference = PartyReferenceHelper.Parse(premiumPayer),
                    sellerPartyReference = PartyReferenceHelper.Parse(premiumReceiver)
                };
            swaption.premium[0].paymentAmount = premium;
            swaption.premium[0].paymentDate = paymentDate;
            swaption.premium[0].payerPartyReference = PartyReferenceHelper.Parse(premiumPayer);
            swaption.premium[0].receiverPartyReference = PartyReferenceHelper.Parse(premiumReceiver);
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
                XsdClassesFieldResolver.ExerciseProcedureSetAutomaticExercise(swaption.exerciseProcedure, new AutomaticExercise());
            }
            else//manual exercise
            {
                XsdClassesFieldResolver.ExerciseProcedureSetManualExercise(swaption.exerciseProcedure, new ManualExercise());
            }
            XsdClassesFieldResolver.SwaptionSetEuropeanExercise(swaption, europeanExercise);
            return swaption;
        }

        public static Swaption Create(Swap swap, Payment[] premium, EuropeanExercise exercise, bool automaticExercise)
        {
            var swaption = new Swaption { swap = swap, premium = premium };
            var europeanExercise = exercise;
            swaption.exerciseProcedure = new ExerciseProcedure();
            if (automaticExercise)
            {
                XsdClassesFieldResolver.ExerciseProcedureSetAutomaticExercise(swaption.exerciseProcedure, new AutomaticExercise());
            }
            else//manual exercise
            {
                XsdClassesFieldResolver.ExerciseProcedureSetManualExercise(swaption.exerciseProcedure, new ManualExercise());
            }
            XsdClassesFieldResolver.SwaptionSetEuropeanExercise(swaption, europeanExercise);
            return swaption;
        }
    }
}