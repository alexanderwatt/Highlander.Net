namespace Highlander.Build
{
    public partial class BuildConst
    {
        public const string BuildEnv = "DEV";
#if DEBUG
        public const string BuildCfg = "Debug";
#else
        public const string BuildCfg = "Release";
#endif
    }
}
