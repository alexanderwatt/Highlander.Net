using Highlander.Core.Common;
using Highlander.Grpc.Contracts;
using V131EnvId = Highlander.Grpc.Session.V131EnvId;
using System;

namespace Highlander.Grpc
{
    public static class Extensions
    {
        public static V341ItemKind ToV341ItemKind(this ItemKind itemKind)
        {
            switch (itemKind)
            {
                case ItemKind.Undefined: return V341ItemKind.Undefined;
                case ItemKind.Object: return V341ItemKind.Object;
                case ItemKind.System: return V341ItemKind.System;
                case ItemKind.Signal: return V341ItemKind.Event;
                case ItemKind.Debug: return V341ItemKind.Debug;
                case ItemKind.Local: return V341ItemKind.Local;
                default: throw new ArgumentException($"Unknown ItemKind: {itemKind}");
            }
        }
        public static ItemKind ToItemKind(this V341ItemKind itemKind)
        {
            switch (itemKind)
            {
                case V341ItemKind.Undefined: return ItemKind.Undefined;
                case V341ItemKind.Object: return ItemKind.Object;
                case V341ItemKind.System: return ItemKind.System;
                case V341ItemKind.Event: return ItemKind.Signal;
                case V341ItemKind.Debug: return ItemKind.Debug;
                case V341ItemKind.Local: return ItemKind.Local;
                default: throw new ArgumentException($"Unknown V341ItemKind: {itemKind}");
            }
        }

        public static V131EnvId ToV131EnvId(this EnvId env)
        {
            return env switch
            {
                EnvId.Undefined => V131EnvId.Undefined,
                EnvId.Utt_UnitTest => V131EnvId.UttUnitTest,
                EnvId.Dev_Development => V131EnvId.DevDevelopment,
                EnvId.Sit_SystemTest => V131EnvId.SitSystemTest,
                EnvId.Stg_StagingLive => V131EnvId.StgStagingLive,
                EnvId.Prd_Production => V131EnvId.PrdProduction,
                _ => throw new ArgumentException($"Unknown EnvId: {env}")
            };
        }
        public static EnvId ToEnvId(this V131EnvId env)
        {
            return env switch
            {
                V131EnvId.Undefined => EnvId.Undefined,
                V131EnvId.UttUnitTest => EnvId.Utt_UnitTest,
                V131EnvId.DevDevelopment => EnvId.Dev_Development,
                V131EnvId.SitSystemTest => EnvId.Sit_SystemTest,
                V131EnvId.StgStagingLive => EnvId.Stg_StagingLive,
                V131EnvId.PrdProduction => EnvId.Prd_Production,
                _ => throw new ArgumentException($"Unknown V131EnvId: {env}")
            };
        }
    }
}
