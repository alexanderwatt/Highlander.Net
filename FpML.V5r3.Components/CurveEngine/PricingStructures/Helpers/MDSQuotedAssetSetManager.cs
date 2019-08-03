/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System.Collections.Generic;
using System.Data;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Serialisation;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    class MarketDataSubscriberDataManager
    {
        public void Initialize(List<string> instrumentIds, List<string> fieldNames)
        {
            _dataTable.Columns.Add("InstrumentId", typeof(string));
            foreach (string fieldName in fieldNames)
            {
                _dataTable.Columns.Add(fieldName, typeof(decimal));
            }
            foreach (string instrumentId in instrumentIds)
            {
                DataRow dataRow = _dataTable.NewRow();
                dataRow["InstrumentId"] = instrumentId;

                _dataTable.Rows.Add(dataRow);
            }
            _dataTable.AcceptChanges();
        }
        
        private readonly DataTable _dataTable = new DataTable();

        private static QuotedAssetSet _lastQuotedAssetSet;

        public void AddQuotedAssetSet(QuotedAssetSet quotedAssetSet)
        {
            quotedAssetSet = BinarySerializerHelper.Clone(quotedAssetSet);

            //_lastQuotedAssetSet = quotedAssetSet;
            if (null == _lastQuotedAssetSet)
            {
                _lastQuotedAssetSet = quotedAssetSet;
            }
            else
            {
                MergeQuotedAssetSet(quotedAssetSet);
            }
        }


        //  Assumption - QAS contains one BAV with one BQ
        //
        private static void MergeQuotedAssetSet(QuotedAssetSet quotedAssetSet)
        {
            BasicAssetValuation bavNew = quotedAssetSet.assetQuote[0];
            BasicQuotation      bqNew =  bavNew.quote[0];
            string newInstrumentId = bavNew.objectReference.href;
            List<BasicAssetValuation> basicAssetValuationsOldWithTheSameIdAsNew = QuotedAssetSet.GetAssetQuote(_lastQuotedAssetSet, newInstrumentId);
            if (basicAssetValuationsOldWithTheSameIdAsNew.Count == 0)
            {
                //  Add
                //
                var temp = new List<BasicAssetValuation>(_lastQuotedAssetSet.assetQuote) {bavNew};
                _lastQuotedAssetSet.assetQuote = temp.ToArray();
            }
            else
            {
                //update
                BasicAssetValuation bavOld = basicAssetValuationsOldWithTheSameIdAsNew[0];
                bool bqUpdated = false;
                foreach (BasicQuotation bqOld in bavOld.quote)
                {
                    if (bqNew.timing.Value == bqOld.timing.Value)
                    {
                        bqOld.value = bqNew.value;

                        bqUpdated = true;
                        break;
                    }
                }
                if (!bqUpdated)
                {
                    //  Add
                    //
                    var tempBQ = new List<BasicQuotation>(bavOld.quote) {bqNew};
                    bavOld.quote = tempBQ.ToArray();
                }
            }
        }


        public DataTable GetDataTable()
        {
            if (null != _lastQuotedAssetSet)
            {
                foreach (DataRow dataRow in _dataTable.Rows)
                {
                    var instrumentId = (string)dataRow["InstrumentId"];
                    List<BasicAssetValuation> basicAssetValuations =
                        QuotedAssetSet.GetAssetQuote(_lastQuotedAssetSet, instrumentId);
                    if (basicAssetValuations.Count > 0)
                    {
                        BasicAssetValuation bav0 = basicAssetValuations[0];
                        foreach (DataColumn dataColumn in _dataTable.Columns)
                        {
                            string columnName = dataColumn.ColumnName;
                            BasicQuotation basicQuotation = BasicAssetValuationHelper.GetQuotationByTiming(bav0, columnName);
                            if (null != basicQuotation)
                            {
                                dataRow[columnName] = basicQuotation.value;
                            }
                        }
                    }
                }
            }
            _dataTable.AcceptChanges();
            DataTable copy = _dataTable.Copy();
            //  remove "InstrumentId" column
            //  
            copy.Columns.Remove("InstrumentId");
            copy.AcceptChanges();
            return copy;
        }
    }
}