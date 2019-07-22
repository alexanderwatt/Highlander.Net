/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Orion.Analytics.Counterparty
{

    /// <summary>
    /// This is used to specify the start date of the transaction, the
    /// created date (may be earlier than start date for forward starting), term etc
    /// 
    /// EndDate = Utilities.DateUtility.getEndDate(StartDate, trade.Tenor) OR maturity
    /// MaxMonths = Utilities.DateUtility.DifferenceInMonths(StartDate, EndDate)
    /// TotalMonths = Utilities.DateUtility.DifferenceInMonths(CreateDate, EndDate)
    /// TotalMonths = Length of forward Start(Delaymonths) plus term of Trade(Maxmonths) 
    /// 
    /// </summary>
    public class DateMonths
    {
        ///<summary>
        ///</summary>
        public DateTime StartDate { get; set; } //transaction start date?
        ///<summary>
        ///</summary>
        public DateTime CreatedDate { get; set; } //eval date?
        ///<summary>
        ///</summary>
        public int Maxmonths { get; set; }
        ///<summary>
        ///</summary>
        public int DelayMonths { get; set; }
        ///<summary>
        ///</summary>
        public int Totalmonths { get; set; }
    }
}