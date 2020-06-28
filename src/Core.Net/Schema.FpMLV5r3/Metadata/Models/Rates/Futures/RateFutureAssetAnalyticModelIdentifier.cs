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

namespace Highlander.Reporting.Models.V5r3.Rates.Futures
{
    public enum RateFutureAssetAnalyticModelIdentifier
    {
        //Old approach where exchange future is its own class
        ZB,
        IR,
        IB,
        ED,
        ER,
        RA,
        L,
        ES,
        EY,
        HR,
        BAX,
        W,
        B,
        CER,
        LME
    }

    public enum ExchangeIdentifierEnum
    {
        //New approach using an exchange class and not a futures class.
        XCME,
        IEPA,
        XLME,
        XASX,
        XCBT
    }
}
