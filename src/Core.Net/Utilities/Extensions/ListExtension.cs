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

using System.Collections.Generic;
using System.Linq;

namespace Highlander.Utilities.Extensions
{
    public static class ListExtension
    {
        public static void Resize<T>(this List<T> list, int size, T element = default)
        {
            var count = list.Count;
            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)     // Optimization
                    list.Capacity = size;
                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        // erases the contents without changing the size
        public static void Erase<T>(this List<T> list)
        {
            for (var i = 0; i < list.Count; i++)
                list[i] = default;
        }
    }
}
