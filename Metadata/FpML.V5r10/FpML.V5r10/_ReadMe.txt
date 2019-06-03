FpML 5.10 Recommendation 

February 12, 2018 - Build 5

Documentation, schemas, and examples can be found at:
http://www.fpml.org/spec/fpml-5-10-5-rec-1/

Feature         Schema File         Generated code
Confirmations   Confirmation.xsd    Confirmation_xmldsig-core-schema.cs
Reporting       Reporting.xsd       Reporting_xmldsig-core-schema.cs
Transparency    Transparency.xsd    Transparency_xmldsig-core-schema.cs
RecordKeeping   RecordKeeping.xsd   RecordKeeping_xmldsig-core-schema.cs
Code lists      CodeList.xsd        Codelist.cs

*Note:
To re-generate the source code (eg. if the schemas are updated), run the
xsdgen.bat command in this project. After generation, some manual edits
are required to fix errors in the generated source code. The author is not
aware of the cause of the errors, but has identified the fixes required.
Unfortunately, these errors are only discovered at runtime, either when a
XmlSerializer is created, or when serialisation or deserialisation is attempted.

The replacements are:

    Search text                                 Replace text
1.  RoutingId[][]                               RoutingId[]
2.  SettlementPeriodsReference[][]              SettlementPeriodsReference[]
3.  DailyInterestCalculation[][]                DailyInterestCalculation[]
4.  SPKIDataType[][]							SPKIDataType[]

Add to Confirmation.Trade item field.

        [System.Xml.Serialization.XmlElementAttribute("bondTransaction", typeof(BondTransaction))]
		[System.Xml.Serialization.XmlElementAttribute("equityTransaction", typeof(EquityTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("futureTransaction", typeof(FutureTransaction))]
        [System.Xml.Serialization.XmlElementAttribute("propertyTransaction", typeof(PropertyTransaction))]

Add to Repporting.ItemChoiceType15

		/// <remarks/>
        equityTransaction,
        /// <remarks/>
        futureTransaction,
        /// <remarks/>
        propertyTransaction,
        /// <remarks/>
        bondTransaction,

Add to Confirmation also.

Update the FpML.V5r10.ConfigData.FpMLTradeLoader with the addtional trade types
Update the extensions and regenerate.