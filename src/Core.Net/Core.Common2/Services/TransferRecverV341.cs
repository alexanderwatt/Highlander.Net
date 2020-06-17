using System;
using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Core.Common.CommsInterfaces;
using Highlander.Grpc.Contracts;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Contracts.TransferV341;

namespace Highlander.Core.Common.Services
{
    public class TransferReceiverV341 : TransferV341Base
    {
        private readonly ILogger<TransferReceiverV341> _logger;
        private readonly ITransferV341 transferV341Interface;

        public TransferReceiverV341(ILogger<TransferReceiverV341> logger, ITransferV341 transferV341Interface)
        {
            _logger = logger;
            this.transferV341Interface = transferV341Interface;
        }

        public override Task<TransferV341AnswerMultipleItemsReply> TransferV341AnswerMultipleItems(TransferV341AnswerMultipleItemsRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341AnswerMultipleItems(request.Header, request.Body);
            return Task.FromResult(new TransferV341AnswerMultipleItemsReply());
        }

        public override Task<TransferV341SelectMultipleItemsReply> TransferV341SelectMultipleItems(TransferV341SelectMultipleItemsRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341SelectMultipleItems(request.Header, request.Body);
            return Task.FromResult(new TransferV341SelectMultipleItemsReply());
        }

        public override Task<TransferV341CompletionResultReply> TransferV341CompletionResult(TransferV341CompletionResultRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341CompletionResult(request.Header, request.Body);
            return Task.FromResult(new TransferV341CompletionResultReply());
        }

        public override Task<TransferV341CreateSubscriptionReply> TransferV341CreateSubscription(TransferV341CreateSubscriptionRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341CreateSubscription(request.Header, request.Body);
            return Task.FromResult(new TransferV341CreateSubscriptionReply());
        }

        public override Task<TransferV341ExtendSubscriptionReply> TransferV341ExtendSubscription(TransferV341ExtendSubscriptionRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341ExtendSubscription(request.Header, request.Body);
            return Task.FromResult(new TransferV341ExtendSubscriptionReply());
        }

        public override Task<TransferV341CancelSubscriptionReply> TransferV341CancelSubscription(TransferV341CancelSubscriptionRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341CancelSubscription(request.Header, request.Body);
            return Task.FromResult(new TransferV341CancelSubscriptionReply());
        }

        public override Task<TransferV341NotifyMultipleItemsReply> TransferV341NotifyMultipleItems(TransferV341NotifyMultipleItemsRequest request, ServerCallContext context)
        {
            transferV341Interface.TransferV341NotifyMultipleItems(request.Header, request.Body);
            return Task.FromResult(new TransferV341NotifyMultipleItemsReply());
        }
    }
}
