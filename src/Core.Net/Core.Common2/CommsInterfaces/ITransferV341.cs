using Highlander.Grpc.Contracts;
using Highlander.Grpc.Session;

namespace Highlander.Core.Common.CommsInterfaces
{
    public interface ITransferV341
    {
        void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body);
        void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body);
        void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body);
        void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body);
        void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body);
        void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body);
        void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body);
    }
}
