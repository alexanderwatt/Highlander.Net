using Highlander.Configuration.Data.V5r3;
using Highlander.Core.Common;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using System.Collections.Generic;

namespace Highlander.Web.API.V5r3.Services
{
    public class CacheManager
    {
        private readonly Dictionary<string, bool> _loadedNameSpaces;
        private readonly Reference<ILogger> _logger;
        private readonly ICoreCache _cache;

        public CacheManager(Reference<ILogger> logger, ICoreCache cache)
        {
            _loadedNameSpaces = new Dictionary<string, bool>();
            _logger = logger;
            _cache = cache;
        }

        public bool CheckConfig(string nameSpace)
        {
            return _loadedNameSpaces.ContainsKey(nameSpace) && _loadedNameSpaces[nameSpace];
        }

        public void LoadConfig(string nameSpace)
        {
            if (CheckConfig(nameSpace))
                return;
            LoadConfigDataHelper.LoadConfigurationData(_logger.Target, _cache.Proxy, nameSpace);
            _loadedNameSpaces.Add(nameSpace, true);
        }
    }
}