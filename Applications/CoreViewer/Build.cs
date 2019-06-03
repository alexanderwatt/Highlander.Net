namespace CoreViewer.Build
{
    public partial class CoreConst
    {
        public const string BuildEnv = "DEV";
#if DEBUG
        public const string BuildCfg = "Debug";
#else
        public const string BuildCfg = "Release";
#endif
    }
}
