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


namespace Highlander.Reporting.Models.V5r3.Property.Lease
{
    public interface ILeaseStreamParameters : ILeaseAssetParameters
    {
        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted fla.</value>
        bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        decimal NPV { get; set; }

        /// <summary>
        /// Gets/Sets the amounts to be used for break even rate.
        /// </summary>
        /// <value>The amounts.</value>
        decimal[] Amounts { get; set; }

        /// <summary>
        /// Gets or sets the target npv for solvers.
        /// </summary>
        /// <value>The target npv.</value>
        decimal TargetNPV { get; set; }
    }
}