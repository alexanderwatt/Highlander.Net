#region Using directives

using System;
using System.Diagnostics;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The valuebase class.
    /// </summary>
    public abstract class ValueBase : IValue
    {
        /// <summary>
        /// The value base.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coord"></param>
        protected ValueBase(string name, object value, IPoint coord)
        {
            _name = name;
            _value = value;
            _coord = coord;
        }

        /// <summary>
        /// The name.
        /// </summary>
        public virtual string Name => _name;

        /// <summary>
        /// The name.
        /// </summary>
        protected string _name;

        /// <summary>
        /// The value.
        /// </summary>
        public virtual object Value => _value;

        /// <summary>
        /// The value.
        /// </summary>
        protected object _value;
        
        /// <summary>
        /// The coord.
        /// </summary>
        public virtual IPoint Coord => _coord;

        /// <summary>
        /// A coordinate.
        /// </summary>
        protected IPoint _coord;
    }

    /// <summary>
    /// This class wraps an FpML PricingStructurePoint. it holds a coordinate and a value
    /// </summary>
    public class VolatilityValue : ValueBase
    {
        readonly PricingStructurePoint _point;

        #region Constructor

        /// <summary>
        /// The volatiltiy value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coord"></param>
        public VolatilityValue(string name, object value, IPoint coord)
            : base(name, value, coord)
        {
            _point = new PricingStructurePoint
            {
                id = name,
                value = Convert.ToDecimal(value),
                valueSpecified = true,
                coordinate = new[] { ((Coordinate)coord).PricingDataCoordinate }
            };
        }

        /// <summary>
        /// The volatiltiy value.
        /// </summary>
        /// <param name="point"></param>
        public VolatilityValue(PricingStructurePoint point)
            : base(point.id, point.value, new Coordinate(point.coordinate[0]))
        {
            _point = point;
        }

        #endregion

        #region Properties

        /// <summary>
        /// A price point.
        /// </summary>
        public PricingStructurePoint PricePoint
        { get { return _point; } }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Equals override.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var match = false;
            if (obj.GetType() == typeof(VolatilityValue))
            {
                var pt = ((VolatilityValue)obj).PricePoint;

                match = _point.value == pt.value;

                if (match)
                {
                    match = _point.coordinate[0] == pt.coordinate[0];
                }

                if (match)
                {
                    match = _point.id == pt.id;
                }
            }
            return match;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// A double value.
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}, Coord = {Coord}")]
    public class DoubleValue : ValueBase
    {
        /// <summary>
        /// A double value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="coord"></param>
        public DoubleValue(string name, double value, IPoint coord)
        :base(name, value, coord)
        {
        }
    }

}
