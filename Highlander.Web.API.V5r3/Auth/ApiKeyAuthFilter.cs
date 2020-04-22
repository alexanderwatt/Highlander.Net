using Autofac.Integration.WebApi;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Highlander.Web.API.V5r3.Auth
{
    public class ApiKeyAuthFilter : IAutofacAuthorizationFilter
    {
            private readonly string _header;
            private readonly string _privateKey;

            public ApiKeyAuthFilter(string header, string privateKey)
            {
                _header = header;
                _privateKey = privateKey;
            }

            public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                var hasRequiredHeader = actionContext.Request.Headers.Any(requestHeader => string.Equals(requestHeader.Key, _header, StringComparison.OrdinalIgnoreCase) && requestHeader.Value.Any(val => val == _privateKey));
                if (!hasRequiredHeader)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access Denied for supplied API Key");

                return Task.CompletedTask;
            }
        }
    }