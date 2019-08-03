/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.ServiceModel;

namespace Core.Common
{
    public class CustomClientBase<I> : ClientBase<I>, IDisposable where I : class
    {
        public CustomClientBase(AddressBinding addressBinding)
            : base(addressBinding.Binding, addressBinding.Address)
        { }

        protected virtual void Dispose(bool disposing)
        {
            // no managed or unmanaged resources to clean up
        }
        public void Dispose()
        {
            try
            {
                Dispose(true);
                if (State != CommunicationState.Faulted)
                    Close();
            }
            catch (CommunicationObjectFaultedException)
            {
                Abort();
            }
        }
    }
}
