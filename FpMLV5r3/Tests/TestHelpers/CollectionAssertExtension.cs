﻿/*
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.TestHelpers.V5r3
{
    public static class CollectionAssertExtension
    {
        public static void IsEmpty(Array item1)
        {
            Assert.IsTrue(item1 == null || item1.GetLength(0) == 0);
        }
    }
}
