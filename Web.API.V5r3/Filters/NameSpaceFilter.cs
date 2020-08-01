using Autofac.Integration.WebApi;
using Highlander.Core.Interface.V5r3;
using Highlander.Web.API.V5r3.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Highlander.Web.API.V5r3.Filters
{
    public class NameSpaceFilter : IAutofacActionFilter
    {
        private readonly CacheManager cacheManager;
        private readonly PricingCache cache;

        public NameSpaceFilter(CacheManager cacheManager, PricingCache cache)
        {
            this.cacheManager = cacheManager;
            this.cache = cache;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var nameSpace = actionContext.ActionArguments.SingleOrDefault(a => a.Key == Constants.Constants.NameSpaceKey);

            if (nameSpace.Equals(default(KeyValuePair<string, object>)))
                return Task.CompletedTask;

            cache.NameSpace = nameSpace.Value.ToString();
            cacheManager.LoadConfig(nameSpace.Value.ToString());
            return Task.CompletedTask;
        }
    }
}