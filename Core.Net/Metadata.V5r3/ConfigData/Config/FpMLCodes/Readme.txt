A number of data elements defined in FpML are restricted to holding one of a limited set of possible values, e.g. currency, business centers, etc. Such restricted sets of values are frequently referred to as domains.

Domains in FpML that are expected to change over time are coded using a strategy that has been defined by the Architecture Working Group, referred to as 'Coding Schemes'. 

Each Coding Scheme is associated with a URI. Coding Schemes can be categorized as one of the following:

* An external coding Scheme, which has a well-known URI. In this case the URI is assigned by an external body, and may or may not have its own versioning, date syntax and semantics.
 
* An external coding Scheme, which does not have a well-known URI. In this case FpML assigns a URI as a proxy to refer to the concept of the external Scheme.  URI will not be versioned.

* An FpML-defined coding Scheme. In this case the Scheme is fully under FpML control and the URI will change reflecting newer versions and revisions as the scheme evolves and changes.

A coding scheme provides alternate identifiers for one identity. It is not used to identify things other than the identity of the thing that contains it.


The "codelist.zip" file contains the following coding Scheme XML files:

FpML Schemes (represented in XML)
================================
 asset-class 
 asset-measure
 broker-confirmation-type
 bullion-delivery-location
 business-center
 business-process
 cashflow-type
 cdx-index-annex-source
 clearance-system
 clearing-status
 collateral-dispute-resolution-method-code
 collateral-dispute-resolution-method-reason
 collateral-dispute-resolution-reason - missing
 collateral-interest-response-reason
 collateral-margin-call-response-reason
 collateral-margin-call-response-reason-code - missing
 collateral-response-reason
 collateral-response-reason-code - missing
 collateral-retraction-reason
 collateral-retraction-reason-code - missing
 collateral-substitution-response-reason
 collateral-type
 commodity-business-calendar
 commodity-coal-product-source
 commodity-coal-product-type
 commodity-coal-quality-adjustments
 commodity-coal-transportation-equipment
 commodity-code
 commodity-expire-relative-to-event
 commodity-frequency-type
 commodity-fx-type
 commodity-market-disruption
 commodity-market-disruption-fallback
 commodity-pay-relative-to-event
 commodity-quantity-frequency
 commodity-reference-price
 compounding-frequency
 confirmation-method
 confirmation-type  - missing
 contractual-definitions
 contractual-supplement
 counterparty-types
 coupon-type
 credit-matrix-transaction-type
 credit-seniority
 credit-seniority-trading
 credit-support-agreement-type
 cut-name
 day-count-fraction
 derivative-calculation-method
 designated-priority
 determination-method
 entity-type
 event-status
 execution-type
 execution-venue-type
 facility-type
 floating-rate-index
 governing-law
 index-annex-source - missing
 inflation-index-description
 inflation-index-source
 inflation-main-publication
 information-provider
 interpolation-method
 local-jurisdiction
 loan-type - missing
 market-disruption
 master-agreement-type
 master-agreement-version
 master-confirmation-annex-type
 master-confirmation-type
 matrix-type
 mortgage-sector
 org-type-category
 originating-event
 party-role
 party-role-type
 perturbation-type
 position-change-type
 position-status
 position-update-reason-code
 price-quote-units
 pricing-input-type
 product-type-simple
 query-parameter-operator
 quote-timing
 reason-code
 regulator
 reporting-currency-type
 reporting-purpose
 reporting-role
 requested-action
 resource-type
 restructuring
 routing-id-code - missing
 scheduled-date-type
 service-advisory-category
 service-processing-cycle
 service-processing-event
 service-processing-step
 service-status
 set-of-schemes
 settled-entity-matrix-source
 settlement-method
 settlement-price-default-election
 settlement-price-source
 settlement-rate-option
 spread-schedule-type
 terminating-event
 trade-cashflows-status
 valuation-set-detail - missing

External Schemes (represented in Specification, not in XML)
===========================================================
 creditRatingScheme - Contains a code representing the credit rating agencies
 industryClassificationScheme - Contains a code representing the party's industry sector classification

 countryScheme - provides codes for country
 currencyScheme - provides codes for currency
 entityIdScheme - qualifier for using entity identifiers
 entityNameScheme - specifies entity names used
 exchangeIdScheme - specifies a set of exchange identifiers
 floatingRateIndexScheme - allows floating rate index code definitions
 instrumentIdScheme - specifies a set of instrument identifiers
 partyIdScheme - provides codes for identification of parties
 routingIdCodeScheme - The specification of the routing id code, which can be used to determine the coding convention for the settlement



Additional Information
======================
The Coding Schemes follow the content model defined by the CodeList.xsd schema file.

The FpML coding Schemes have been validated using Xerces J v 2.4.1.

For more information about Code Lists, visit:
http://www.genericode.org/
http://www.idealliance.org/proceedings/xml04/abstracts/paper86.html 