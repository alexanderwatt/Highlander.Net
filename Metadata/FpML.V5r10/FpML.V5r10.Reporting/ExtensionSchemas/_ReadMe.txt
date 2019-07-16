This assembly contains C# classes generated* from
FpML 5.3 March 29, 2012 (build 4) XML schemas.

Documentation, schemas, and examples can be found at:
http://www.fpml.org/spec/fpml-5-3-4-lcwd-1/

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
4.	TradeIdentifier[][]?
5.	byte[][] sPKISexpField?

