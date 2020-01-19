/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Base Model Controller class from which all controllers/models should be extended
    /// </summary>
    [Serializable]
    public abstract class ModelControllerBase<TD, TR> : IModelController<TD, TR>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>The model parameters.</value>
        public TD ModelData { get; protected set; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public abstract IList<string> Metrics { get; set; }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public abstract TR Calculate(TD modelData);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="analyticResults">The analytic results.</param>
        /// <returns></returns>
        protected abstract TR GetValue<T>(T analyticResults);

        ///<summary>
        ///</summary>
        protected ModelControllerBase()
        {
            Id = string.Empty;
        }
    }
}