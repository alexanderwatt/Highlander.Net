/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using Orion.Constants;
using Orion.ModelFramework.Identifiers;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The PropertyIdentifier.
    /// </summary>
    public class LeaseIdentifier : Identifier, ILeaseIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string LeaseType{ get; set; }

        /// <summary>
        ///  An id for a bond.
        /// </summary>
        /// <param name="referencePropertyId">The reference Property Id. </param>
        /// <param name="tenant">The tenant.</param>
        /// <param name="leaseType">The lease Type</param>
        /// <param name="leaseId">The lease Id</param>
        public LeaseIdentifier(string referencePropertyId, string tenant, string leaseType, string leaseId)
            : base(BuildUniqueId(referencePropertyId, tenant, leaseId, leaseType))
        {
            LeaseType = leaseType;
            Id = BuildId(referencePropertyId, tenant, leaseId, leaseType);
        }

        private static string BuildUniqueId(string referencePropertyId, string tenant, string leaseType, string leaseId)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Property + ".Lease." + referencePropertyId + "." + tenant + "." + leaseType + "." + leaseId;
        }

        public static string BuildId(string referencePropertyId, string tenant, string leaseType, string leaseId)
        {
            return referencePropertyId + ".Lease." + tenant + "." + leaseType + "." + leaseId;
        }
    }
}