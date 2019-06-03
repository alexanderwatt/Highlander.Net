using System;
using National.QRSC.Contracts;
using nab.QDS.Core.Common;
using nab.QDS.Util.Logging;
using National.QRSC.Workflow;
using National.QRSC.Workflow.TradeValuation;
using National.QRSC.Workflow.CurveGeneration;

namespace National.QRSC.Grid.Handler
{
    public static class RequestHandler
    {
        private static void HandleSpecificRequest<R, W>(ILogger logger, ICoreCache cache, string requestItemName)
            where R : RequestBase
            where W : WorkstepBase<RequestBase, HandlerResponse>, IRequestHandler<RequestBase, HandlerResponse>, new()
        {
            R request = cache.LoadObject<R>(requestItemName);
            if (request == null)
                throw new ApplicationException(String.Format("Request not found (type={0},name='{1}')", typeof(R).Name, requestItemName));

            // publish 'initial' status
            HandlerResponse response = new HandlerResponse()
            {
                RequestId = request.RequestId,
                RequesterId = request.RequesterId,
                Status = RequestStatusEnum.Commencing,
                CommenceTime = DateTimeOffset.Now.ToString("o")
            };
            cache.SaveObject<HandlerResponse>(response);
            try
            {
                using (IRequestHandler<RequestBase, HandlerResponse> handler = new W() as IRequestHandler<RequestBase, HandlerResponse>)
                {
                    handler.InitialiseRequest(logger, cache);
                    handler.ProcessRequest(request, response);
                }
            }
            catch (OperationCanceledException cancelExcp)
            {
                response.Status = RequestStatusEnum.Cancelled;
                response.CancelReason = cancelExcp.Message;
            }
            catch (Exception outerExcp)
            {
                response.Status = RequestStatusEnum.Faulted;
                response.FaultDetail = new ExceptionDetail(outerExcp);
                logger.Log(outerExcp);
            }
            // publish 'completed' status
            cache.SaveObject<HandlerResponse>(response);
        }

        public static int HandleRequest(ILogger logger, ICoreCache cache, Guid requestId, string hostInstance)
        {
            int result = 0;

            if (requestId == Guid.Empty)
                throw new ArgumentNullException("requestId");

            string requestItemName = (new AssignedWorkflowRequest()
            {
                RequestId = requestId.ToString(),
                WorkerHostComputer = Environment.MachineName,
                WorkerHostInstance = hostInstance
            }).NetworkKey;
            AssignedWorkflowRequest request = cache.LoadObject<AssignedWorkflowRequest>(requestItemName);
            if (request == null)
                throw new ArgumentException("Cannot find request name: ", requestItemName);

            logger.LogDebug("---------- request header details");
            logger.LogDebug("Id       : {0}", requestId);
            logger.LogDebug("Computer : {0}", request.WorkerHostComputer);
            logger.LogDebug("Instance : {0}", request.WorkerHostInstance);
            logger.LogDebug("Data Type: {0}", request.RequestDataType);
            logger.LogDebug("Item Name: {0}", request.RequestItemName);
            logger.LogDebug("---------- request handler executing");
            try
            {
                if (request.RequestDataType.Equals(typeof(TradeValuationRequest).FullName))
                {
                    HandleSpecificRequest<TradeValuationRequest, WFCalculateTradeValuation>(logger, cache, request.RequestItemName);
                }
                else if (request.RequestDataType.Equals(typeof(PortfolioValuationRequest).FullName))
                {
                    HandleSpecificRequest<PortfolioValuationRequest, WFCalculatePortfolioValuation>(logger, cache, request.RequestItemName);
                }
                else if (request.RequestDataType.Equals(typeof(OrdinaryCurveGenRequest).FullName))
                {
                    HandleSpecificRequest<OrdinaryCurveGenRequest, WFGenerateOrdinaryCurve>(logger, cache, request.RequestItemName);
                }
                else if (request.RequestDataType.Equals(typeof(StressedCurveGenRequest).FullName))
                {
                    HandleSpecificRequest<StressedCurveGenRequest, WFGenerateStressedCurve>(logger, cache, request.RequestItemName);
                }
                // diagnostic
                else if (request.RequestDataType.Equals(typeof(PingHandlerRequest).FullName))
                {
                    HandleSpecificRequest<PingHandlerRequest, WFCalculatePingHandler>(logger, cache, request.RequestItemName);
                }
                else
                    throw new NotSupportedException(String.Format("Unsupported RequestDataType: '{0}'", request.RequestDataType));

                // success
                result = 1;
                logger.LogDebug("---------- request handler completed");
            }
            catch (Exception outerExcp)
            {
                logger.LogDebug("---------- request handler failed");
                logger.Log(outerExcp);
            }
            return result;
        }

    }
}
