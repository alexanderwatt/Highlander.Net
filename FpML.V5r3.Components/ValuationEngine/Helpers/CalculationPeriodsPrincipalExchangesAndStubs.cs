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
using System.Collections.Generic;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class CalculationPeriodsPrincipalExchangesAndStubs
    {
        public List<CalculationPeriod> CalculationPeriods = new List<CalculationPeriod>();
        public CalculationPeriod InitialStubCalculationPeriod;
        public CalculationPeriod FinalStubCalculationPeriod;

        /// <summary>
        /// List of principle exchange cashflows (initial, final and intermediate).
        /// </summary>
        public List<PrincipalExchange>  IntermediatePrincipalExchanges = new List<PrincipalExchange>();
        public PrincipalExchange InitialPrincipalExchange;
        public PrincipalExchange FinalPrincipalExchange;

        public void Add(PrincipalExchange intermediatePrincipalExchange)
        {
            IntermediatePrincipalExchanges.Add(intermediatePrincipalExchange);
        }

        public List<PrincipalExchange> GetAllPrincipalExchanges()
        {
            var result = new List<PrincipalExchange>(IntermediatePrincipalExchanges);
            if (null != InitialPrincipalExchange)
            {
                result.Insert(0, InitialPrincipalExchange);
            }
            if (null != FinalPrincipalExchange)
            {
                result.Add(FinalPrincipalExchange);
            }
            return result;
        }

        public bool IsFirstRegularPeriod(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod == CalculationPeriods[0];
        }

        public bool IsLastRegularPeriod(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod == CalculationPeriods[CalculationPeriods.Count - 1];
        }

        public List<CalculationPeriod> GetRegularAndStubPeriods()
        {
            var result = new List<CalculationPeriod>(CalculationPeriods);
            if (HasInitialStub)
            {
                result.Insert(0, InitialStubCalculationPeriod);   
            }
            if (HasFinalStub)
            {
                result.Add(FinalStubCalculationPeriod);
            }
            return result;
        }

        public bool HasInitialStub => null != InitialStubCalculationPeriod;

        public bool HasFinalStub => null != FinalStubCalculationPeriod;

        public DateTime FirstRegularPeriodUnadjustedStartDate => CalculationPeriods[0].unadjustedStartDate;

        public DateTime LastRegularPeriodUnadjustedEndDate => CalculationPeriods[CalculationPeriods.Count - 1].unadjustedEndDate;

        public void Add(CalculationPeriod calculationPeriod)
        {
            CalculationPeriods.Add(calculationPeriod); 
        }

        public void Remove(CalculationPeriod calculationPeriod)
        {
            CalculationPeriods.Remove(calculationPeriod); 
        }

        public void InsertFirst(CalculationPeriod calculationPeriod)
        {
            CalculationPeriods.Insert(0, calculationPeriod); 
        }

        public void CreateLongFinalStub()
        {
            CalculationPeriod lastRegularPeriod = CalculationPeriods[CalculationPeriods.Count - 1];
            CalculationPeriod shortFinalStub = FinalStubCalculationPeriod;
            if (lastRegularPeriod.unadjustedEndDate != shortFinalStub.unadjustedStartDate)
            {
                throw new System.Exception();
            }
            var longFinalStub = new CalculationPeriod
                                    {
                                        unadjustedStartDateSpecified = true,
                                        unadjustedEndDateSpecified = true,
                                        unadjustedStartDate = lastRegularPeriod.unadjustedStartDate,
                                        unadjustedEndDate = shortFinalStub.unadjustedEndDate
                                    };
            FinalStubCalculationPeriod = longFinalStub;
            // Remove last regular period (after it was merged with short stub) 
            //
            CalculationPeriods.Remove(lastRegularPeriod);
        }

        public void CreateLongInitialStub()
        {
            var shortInitialStub = InitialStubCalculationPeriod;
            var firstRegularPeriod = CalculationPeriods[0];
            if (shortInitialStub.unadjustedEndDate != firstRegularPeriod.unadjustedStartDate)
            {
                throw new System.Exception();
            }
            var longInitialStub = new CalculationPeriod
                                      {
                                          unadjustedStartDateSpecified = true,
                                          unadjustedEndDateSpecified = true,
                                          unadjustedStartDate = shortInitialStub.unadjustedStartDate,
                                          unadjustedEndDate = firstRegularPeriod.unadjustedEndDate
                                      };
            InitialStubCalculationPeriod = longInitialStub;
            // Remove first regular period (after it was merged with short stub) 
            //
            CalculationPeriods.Remove(firstRegularPeriod);
        }
    }
}