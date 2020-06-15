/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/
using System.Drawing;

namespace Highlander.Core.Common
{
    public static class CoreHelper
    {
        public static Color CoreStateColor(CoreStateEnum state)
        {
            switch (state)
            {
                case CoreStateEnum.Initial:
                    return Color.Gray;
                case CoreStateEnum.Connecting:
                    return Color.Yellow;
                case CoreStateEnum.Connected:
                    return Color.Green;
                case CoreStateEnum.Offline:
                    return Color.Orange;
                case CoreStateEnum.Disposed:
                    return Color.Gray;
                case CoreStateEnum.Faulted:
                    return Color.Red;
                default:
                    return Color.Gray;
            }
        }
        public static string MakeUniqueName(ItemKind itemKind, string appScope, string itemName)
        {
            return ((appScope ?? AppScopeNames.Legacy) + "." + itemKind + "." + itemName).ToLower();
        }
    }
}
