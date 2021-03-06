﻿/*
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

#region Usings

using System;
using Highlander.Core.Common;
using Highlander.Utilities.Logging;

#endregion

namespace Highlander.Workflow
{
    public interface IRequestHandler<R, S> : IDisposable
    {
        void InitialiseRequest(ILogger logger, ICoreCache cache);
        void ProcessRequest(R request, S status);
        Type HandledRequestType { get; }
    }
}
