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
using Highlander.Grpc.Session;

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

        // V1.3
        public static V131EnvId ToV131EnvId(EnvId env)
        {
            switch (env)
            {
                case EnvId.Undefined: return V131EnvId.Undefined;
                case EnvId.Utt_UnitTest: return V131EnvId.UttUnitTest;
                case EnvId.Dev_Development: return V131EnvId.DevDevelopment;
                case EnvId.Sit_SystemTest: return V131EnvId.SitSystemTest;
                case EnvId.Stg_StagingLive: return V131EnvId.StgStagingLive;
                case EnvId.Prd_Production: return V131EnvId.PrdProduction;
                default: throw new ArgumentException($"Unknown EnvId: {env}");
            }
        }
        public static EnvId ToEnvId(V131EnvId env)
        {
            switch (env)
            {
                case V131EnvId.Undefined: return EnvId.Undefined;
                case V131EnvId.UttUnitTest: return EnvId.Utt_UnitTest;
                case V131EnvId.DevDevelopment: return EnvId.Dev_Development;
                case V131EnvId.SitSystemTest: return EnvId.Sit_SystemTest;
                case V131EnvId.StgStagingLive: return EnvId.Stg_StagingLive;
                case V131EnvId.PrdProduction: return EnvId.Prd_Production;
                default: throw new ArgumentException($"Unknown V131EnvId: {env}");
            }
        }
    }
}
