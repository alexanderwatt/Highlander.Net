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

using System;
using System.Drawing;

namespace Highlander.Core.Common
{
    public enum V131EnvId
    {
        Undefined,
        UTT_UnitTest,
        DEV_Development,
        SIT_SystemTest,
        STG_StagingLive,
        PRD_Production
    }

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

        // V1.3
        public static V131EnvId ToV131EnvId(EnvId env)
        {
            return env switch
            {
                EnvId.Undefined => V131EnvId.Undefined,
                EnvId.Utt_UnitTest => V131EnvId.UTT_UnitTest,
                EnvId.Dev_Development => V131EnvId.DEV_Development,
                EnvId.Sit_SystemTest => V131EnvId.SIT_SystemTest,
                EnvId.Stg_StagingLive => V131EnvId.STG_StagingLive,
                EnvId.Prd_Production => V131EnvId.PRD_Production,
                _ => throw new ArgumentException($"Unknown EnvId: {env}")
            };
        }
        public static EnvId ToEnvId(V131EnvId env)
        {
            return env switch
            {
                V131EnvId.Undefined => EnvId.Undefined,
                V131EnvId.UTT_UnitTest => EnvId.Utt_UnitTest,
                V131EnvId.DEV_Development => EnvId.Dev_Development,
                V131EnvId.SIT_SystemTest => EnvId.Sit_SystemTest,
                V131EnvId.STG_StagingLive => EnvId.Stg_StagingLive,
                V131EnvId.PRD_Production => EnvId.Prd_Production,
                _ => throw new ArgumentException($"Unknown V131EnvId: {env}")
            };
        }
    }
}
