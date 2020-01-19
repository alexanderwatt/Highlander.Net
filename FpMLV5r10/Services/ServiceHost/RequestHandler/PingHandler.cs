using System;
using System.Threading;
using National.QRSC.Contracts;
using National.QRSC.Workflow;
using nab.QDS.Util.Logging;
using nab.QDS.Core.Common;

namespace National.QRSC.Grid.Handler
{
    public class WFCalculatePingHandler : WorkstepBase<RequestBase, HandlerResponse>, IRequestHandler<RequestBase, HandlerResponse>
    {
        #region IRequestHandler<RequestBase,HandlerResponse> Members

        public void InitialiseRequest(ILogger logger, ICoreCache cache)
        {
            this.Initialise(new WorkContext(logger, cache, null));
        }

        public void ProcessRequest(RequestBase request, HandlerResponse response)
        {
            PingHandlerRequest pingRequest = request as PingHandlerRequest;
            response.Status = RequestStatusEnum.InProgress;
            _Context.Cache.SaveObject<HandlerResponse>(response);
            // sleep if required
            if (pingRequest.DelayPeriod != null)
            {
                _Context.Logger.LogDebug("PingHandlerRequest: Sleeping for {0}", pingRequest.DelayPeriod);
                Thread.Sleep(TimeSpan.Parse(pingRequest.DelayPeriod));
                _Context.Logger.LogDebug("PingHandlerRequest: Sleep complete.");
            }
            // throw error if required
            if (pingRequest.FaultMessage != null)
            {
                throw new Exception(pingRequest.FaultMessage);
            }
            response.Status = RequestStatusEnum.Completed;
            _Context.Cache.SaveObject<HandlerResponse>(response);
        }

        #endregion

        //old
        //protected override HandlerResponse OnExecute(RequestBase input)
        //{
        //    HandlerResponse response = new HandlerResponse()
        //    {
        //        RequestId = input.RequestId,
        //        RequesterId = input.RequesterId
        //    };
        //    ProcessRequest(input, response);
        //    return response;
        //}
    }
}
