using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Core.Server;
using Highlander.Grpc.Contracts;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Contracts.TransferV341;

namespace Highlander.GrpcService.Services
{
    public class TransferReceiverV341 : TransferV341Base
    {
        private readonly ILogger<TransferReceiverV341> _logger;
        private readonly CacheEngine cacheEngine;

        internal TransferReceiverV341(
            ILogger<TransferReceiverV341> logger,
            CacheEngine cacheEngine
        )
        {
            _logger = logger;
            this.cacheEngine = cacheEngine;
        }

        public override Task<TransferV341AnswerMultipleItemsReply> TransferV341AnswerMultipleItems(TransferV341AnswerMultipleItemsRequest request, ServerCallContext context)
        {
            return cacheEngine.PerformSyncRecvAnswerMultipleItems(request.Body)
            return base.TransferV341AnswerMultipleItems(request, context);
        }

        public override Task<TransferV341SelectMultipleItemsReply> TransferV341SelectMultipleItems(TransferV341SelectMultipleItemsRequest request, ServerCallContext context)
        {
            cacheEngine.PerformSyncRecvSelectMultipleItems(new PackageSelectMultipleItems(request.Header.SessionIdGuid, new PackageHeader(request.Header), new PackageSelectItemsQuery(request.Body)));
            return Task.FromResult(new TransferV341SelectMultipleItemsReply());
        }

        public override Task<TransferV341CompletionResultReply> TransferV341CompletionResult(TransferV341CompletionResultRequest request, ServerCallContext context)
        {
            cacheEngine.PerformSyncRecvCompletionResult(new PackageCompletionResult(request.Header.SessionIdGuid, new PackageHeader(request.Header), request.Body.Success, request.Body.Result, request.Body.Message));
            return Task.FromResult(new TransferV341CompletionResultReply());
        }

        public override Task<TransferV341CreateSubscriptionReply> TransferV341CreateSubscription(TransferV341CreateSubscriptionRequest request, ServerCallContext context)
        {
            cacheEngine.PerformSyncRecvCreateSubscription(new PackageCreateSubscription(
                        request.Header.SessionIdGuid, new PackageHeader(request.Header), new PackageSubscriptionQuery(request.Body), request.Body.ExpiryTime.ToDateTimeOffset()));
            return Task.FromResult(new TransferV341CreateSubscriptionReply());
        }

        public override Task<TransferV341ExtendSubscriptionReply> TransferV341ExtendSubscription(TransferV341ExtendSubscriptionRequest request, ServerCallContext context)
        {
            cacheEngine.UpdateConnectionState(request.Header.SessionIdGuid, connection.ContractName, connection.ReplyAddress);
            connection.DispatchAsyncRecvExtendSubscription(
                new PackageExtendSubscription(
                    connection.ClientId, new PackageHeader(header), body.SubscriptionId, body.ExpiryTime));
            return base.TransferV341ExtendSubscription(request, context);
        }

        public override Task<TransferV341CancelSubscriptionReply> TransferV341CancelSubscription(TransferV341CancelSubscriptionRequest request, ServerCallContext context)
        {
            return base.TransferV341CancelSubscription(request, context);
        }

        public override Task<TransferV341NotifyMultipleItemsReply> TransferV341NotifyMultipleItems(TransferV341NotifyMultipleItemsRequest request, ServerCallContext context)
        {
            return base.TransferV341NotifyMultipleItems(request, context);
        }
    }
}
