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
using System.Threading;

namespace Highlander.Utilities.RefCounting
{
    public class Reference<T> : IDisposable where T : class
    {
        public static Reference<T> Create(T target)
        {
            return new Reference<T>(new RefCounted<T>(target));
        }

        private RefCounted<T> _target;
        public T Target => _target.Target;

        private Reference(RefCounted<T> target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            // increment target ref count
            _target.AddRef();
        }
        private void Release()
        {
            // decrement target ref count once only
            var target = Interlocked.Exchange(ref _target, null);
            target?.Release();
        }
        public void Dispose()
        {
            Release();
        }
        ~Reference()
        {
            // ignore exceptions in finaliser
            try
            {
                Release();
            }
            catch (Exception) { }
        }
        public Reference<T> Clone()
        {
            return new Reference<T>(_target);
        }
        public int RefCount
        {
            get
            {
                RefCounted<T> target = _target;
                if (target != null)
                    return target.RefCount;
                return 0;
            }
        }
    }

    internal class RefCounted<T> where T : class
    {
        private T _target;
        public T Target => _target;

        public RefCounted(T target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }
        private int _refCount;
        public int RefCount => Interlocked.Add(ref _refCount, 0);

        public int AddRef()
        {
            return Interlocked.Increment(ref _refCount);
        }
        public int Release()
        {
            int refCount = Interlocked.Decrement(ref _refCount);
            if (refCount == 0)
            {
                var disposable = _target as IDisposable;
                _target = null;
                disposable?.Dispose();
            }
            return refCount;
        }
    }
}
