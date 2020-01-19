using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class InterestRateStreamHelper
    {
        public static void SetPayerAndReceiver(InterestRateStream stream, string payer, string receiver)
        {
            stream.payerPartyReference = PartyOrAccountReferenceFactory.Create(payer);
            stream.receiverPartyReference = PartyOrAccountReferenceFactory.Create(receiver);
        }
    }
}