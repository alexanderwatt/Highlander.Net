namespace FpML.V5r3.Confirmation
{
    public static class InterestRateStreamHelper
    {
        public static void SetPayerAndReceiver(InterestRateStream stream, string payer, string receiver)
        {
            stream.payerPartyReference = PartyReferenceFactory.Create(payer);
            stream.receiverPartyReference = PartyReferenceFactory.Create(receiver);
        }
    }
}