
namespace Orion.ModelFramework.Trades
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IPartyIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The base party.
        ///</summary>
        string PartyName { get; set; }
    }
}