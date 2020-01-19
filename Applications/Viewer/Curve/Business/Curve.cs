using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Common;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using nab.QDS.FpML.V47;
using Orion.Constants;

namespace Orion.WebViewer.Curve.Business
{
    public class Curve
    {
        public string PricingStructureType { get; private set; }
        public DateTime BuildDateTime { get; private set; }
        public DateTime? BaseDate { get; private set; }
        public string MarketName { get; private set; }
        public string CurveName { get; private set; }
        public string Algorithm { get; private set; }
        public string Currency { get; private set; }
        public string Id { get; private set; }
        public string Domain { get; private set; }

        private Market MarketStructure { get; set; }
        private PointBaseCollection _points;

        internal Curve(ICoreItem item)
        {
            NamedValueSet properties = item.AppProps;

            // These fields could potentially not exist
            PricingStructureType = properties.GetString(CurveProp.PricingStructureType, false);
            BuildDateTime = new DateTime(item.Created.Ticks, DateTimeKind.Local);
            BaseDate = properties.GetValue(CurveProp.BaseDate, BuildDateTime.Date);
            string market = properties.GetString(CurveProp.Market, false);
            MarketName = properties.GetString(CurveProp.MarketAndDate, market);
            Algorithm = properties.GetString(CurveProp.Algorithm, false);
            Currency = properties.GetString(CurveProp.Currency1, false);
            Id = item.Name;

            // Find the part of the ID after the curve type.
            CurveName = properties.GetString(CurveProp.CurveName, false);
            if (string.IsNullOrEmpty(CurveName) && Id.Contains(PricingStructureType))
            {
                int startPosition = Id.IndexOf(PricingStructureType, StringComparison.Ordinal);
                CurveName = Id.Substring(startPosition + PricingStructureType.Length + 1);
            }
            Domain = properties.GetString("Domain", false);
            MarketStructure = (Market)item.Data;
        }

        public string Fpml
        {
            get
            {
                return XmlSerializerHelper.SerializeToString(MarketStructure);
            }
        }

        public PointBaseCollection Points 
        { 
            get {
                return _points ??
                       (_points =
                        MarketStructure.Items1 == null
                            ? new PointsSurfaceCollection()
                            : PointBaseCollection.Factory(MarketStructure.Items1[0]));
            }
        }

        public BindingList<DateTime> XValues
        {
            get 
            {
                return new BindingList<DateTime>(_points.XValues); 
            }
        }

        public string XValueName
        {
            get { return _points.Titles[0]; }
        }

        public BindingList<double> Values
        {
            get
            {
                return new BindingList<double>(_points.YValues);
            }
        }

        public List<string> Titles
        {
            get 
            {
                return _points.Titles;
            }
        }

        public string ValueName
        {
            get { return _points.Titles[1]; }
        }

        public Surface Surface
        {
            get
            {
                return MarketStructure.Items1 == null ? new Surface() : new Surface(MarketStructure.Items1[0]);
            }
        }
    }
}