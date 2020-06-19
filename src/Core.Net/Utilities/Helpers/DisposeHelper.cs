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
using System.Diagnostics;

namespace Highlander.Utilities.Helpers
{
    public static class DisposeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        public static void SafeDispose<T>(ref T target) where T : class, IDisposable
        {
            try
            {
                target?.Dispose();
            }
            catch(Exception excp)
            {
                Debug.WriteLine($"DisposeHelper.SafeDispose<{typeof(T).Name}>: {excp.GetType().Name}");
            }
            target = null;
        }

        public static void SafeDispose<T>(T target) where T : class, IDisposable
        {
            try
            {
                target?.Dispose();
            }
            catch (Exception excp)
            {
                Debug.WriteLine($"DisposeHelper.SafeDispose<{typeof(T).Name}>: {excp.GetType().Name}");
            }
            target = null;
        }
    }
}
