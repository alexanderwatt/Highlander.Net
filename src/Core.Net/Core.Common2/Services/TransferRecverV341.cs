using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Grpc.Contracts;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Contracts.TransferV341;

namespace Highlander.Core.Common.Services
{
    public class TransferReceiverV341 : TransferV341Base
    {
        private readonly ILogger<TransferReceiverV341> _logger;
        public TransferReceiverV341(ILogger<TransferReceiverV341> logger)
        {
            _logger = logger;
        }

        public override Task<TransferV341AnswerMultipleItemsReply> TransferV341AnswerMultipleItems(TransferV341AnswerMultipleItemsRequest request, ServerCallContext context)
        {
            return base.TransferV341AnswerMultipleItems(request, context);
        }

        public override Task<TransferV341SelectMultipleItemsReply> TransferV341SelectMultipleItems(TransferV341SelectMultipleItemsRequest request, ServerCallContext context)
        {
            return base.TransferV341SelectMultipleItems(request, context);
        }

        public override Task<TransferV341CompletionResultReply> TransferV341CompletionResult(TransferV341CompletionResultRequest request, ServerCallContext context)
        {
            return base.TransferV341CompletionResult(request, context);
        }

        public override Task<TransferV341CreateSubscriptionReply> TransferV341CreateSubscription(TransferV341CreateSubscriptionRequest request, ServerCallContext context)
        {
            return base.TransferV341CreateSubscription(request, context);
        }

        public override Task<TransferV341ExtendSubscriptionReply> TransferV341ExtendSubscription(TransferV341ExtendSubscriptionRequest request, ServerCallContext context)
        {
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
