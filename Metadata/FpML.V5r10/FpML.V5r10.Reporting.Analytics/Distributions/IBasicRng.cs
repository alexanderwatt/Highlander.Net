#region Using directives

using System;

#endregion

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// A marker interface that extends <see cref="ICloneable"/> with
    /// <see cref="IDisposable"/> and <see cref="IContinousRng"/>.
    /// </summary>
    public interface IBasicRng : IContinousRng, ICloneable, IDisposable
    {
    }
}