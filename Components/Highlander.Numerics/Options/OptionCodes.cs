/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.Numerics.Options
{
    /// <summary>
    /// This enum is used with NabLib.
    /// </summary>
    public enum OptionCode
    {
        ZZZZ,   //00 //blank
        VEUC,   //01 //vanilla call
        VEUP,   //02 //vanilla put
        BDIC,   //03 //single barrier down and in call
        BDIP,   //04 //single barrier down and in put
        BUIC,   //05 //single barrier up and in call
        BUIP,   //06 //single barrier up and in put
        B2IC,   //07 //double barrier knock in call
        B2IP,   //08 //double barrier knock in put
        BDOC,   //09 //single barrier down and out call
        BDOP,   //10 //single barrier down and out put
        BUOC,   //11 //single barrier up and out call
        BUOP,   //12 //single barrier up and out put
        B2OC,   //13 //double barrier knock out call
        B2OP,   //14 //double barrier knock out put
        SDOC,   //15 //stepped barrier down and out call
        SDOP,   //16 //stepped barrier down and out put
        SUOC,   //17 //stepped barrier up and out call
        SUOP,   //18 //stepped barrier up and out put
        SDIC,   //19 //stepped barrier down and in call
        SDIP,   //20 //stepped barrier down and in put
        SUIC,   //21 //stepped barrier up and in call
        SUIP,   //22 //stepped barrier up and in put
        RBNP,   //23 //double no touch cash payoff
        RBAP,   //24 //double no touch asset payoff
        NTAC,   //25 //single no touch asset upper barrier
        NTAP,   //26 //single no touch asset lower barrier
        NTNC,   //27 //single no touch cash upper barrier
        NTNP,   //28 //single no touch cash lower barrier
        OTAC,   //29 //single no touch asset upper barrier
        OTAP,   //30 //single no touch asset lower barrier
        OTNC,   //31 //single no touch cash upper barrier
        OTNP,   //32 //single no touch cash lower barrier
        EDAC,   //33 //digital above strike asset payoff
        EDAP,   //34 //digital below strike asset payoff
        EDNC,   //35 //digital above strike cash payoff
        EDNP,   //36 //digital above strike cash payoff
        RINP,   //37 //double one touch cash payoff
        RIAP,   //38 //double one touch asset payoff
        FRWD,   //39 //forward
        FXSP,   //40 //spot
        CPCC,   //41 //currency protection call
        CPCP,   //42 //currency protection put
        CASH,   //43 //cash 
        ASST    //44 //asset
    }
}