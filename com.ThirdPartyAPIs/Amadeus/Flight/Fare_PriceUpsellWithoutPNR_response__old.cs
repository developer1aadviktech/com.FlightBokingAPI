using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace com.ThirdPartyAPIs.Amadeus.Flight
{
    public class Fare_PriceUpsellWithoutPNR_response__old
    {

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
        public partial class Envelope
        {

            private EnvelopeHeader headerField;

            private EnvelopeBody bodyField;

            /// <remarks/>
            public EnvelopeHeader Header
            {
                get
                {
                    return this.headerField;
                }
                set
                {
                    this.headerField = value;
                }
            }

            /// <remarks/>
            public EnvelopeBody Body
            {
                get
                {
                    return this.bodyField;
                }
                set
                {
                    this.bodyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public partial class EnvelopeHeader
        {

            private string toField;

            private From fromField;

            private string actionField;

            private string messageIDField;

            private RelatesTo relatesToField;

            private Session sessionField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
            public string To
            {
                get
                {
                    return this.toField;
                }
                set
                {
                    this.toField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
            public From From
            {
                get
                {
                    return this.fromField;
                }
                set
                {
                    this.fromField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
            public string Action
            {
                get
                {
                    return this.actionField;
                }
                set
                {
                    this.actionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
            public string MessageID
            {
                get
                {
                    return this.messageIDField;
                }
                set
                {
                    this.messageIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
            public RelatesTo RelatesTo
            {
                get
                {
                    return this.relatesToField;
                }
                set
                {
                    this.relatesToField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://xml.amadeus.com/2010/06/Session_v3")]
            public Session Session
            {
                get
                {
                    return this.sessionField;
                }
                set
                {
                    this.sessionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
        public partial class From
        {

            private string addressField;

            /// <remarks/>
            public string Address
            {
                get
                {
                    return this.addressField;
                }
                set
                {
                    this.addressField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
        public partial class RelatesTo
        {

            private string relationshipTypeField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string RelationshipType
            {
                get
                {
                    return this.relationshipTypeField;
                }
                set
                {
                    this.relationshipTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/2010/06/Session_v3")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://xml.amadeus.com/2010/06/Session_v3", IsNullable = false)]
        public partial class Session
        {

            private string sessionIdField;

            private byte sequenceNumberField;

            private string securityTokenField;

            private string transactionStatusCodeField;

            /// <remarks/>
            public string SessionId
            {
                get
                {
                    return this.sessionIdField;
                }
                set
                {
                    this.sessionIdField = value;
                }
            }

            /// <remarks/>
            public byte SequenceNumber
            {
                get
                {
                    return this.sequenceNumberField;
                }
                set
                {
                    this.sequenceNumberField = value;
                }
            }

            /// <remarks/>
            public string SecurityToken
            {
                get
                {
                    return this.securityTokenField;
                }
                set
                {
                    this.securityTokenField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string TransactionStatusCode
            {
                get
                {
                    return this.transactionStatusCodeField;
                }
                set
                {
                    this.transactionStatusCodeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public partial class EnvelopeBody
        {

            private Fare_PriceUpsellWithoutPNRReplyFareList[] fare_PriceUpsellWithoutPNRReplyField;

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
            [System.Xml.Serialization.XmlArrayItemAttribute("fareList", IsNullable = false)]
            public Fare_PriceUpsellWithoutPNRReplyFareList[] Fare_PriceUpsellWithoutPNRReply
            {
                get
                {
                    return this.fare_PriceUpsellWithoutPNRReplyField;
                }
                set
                {
                    this.fare_PriceUpsellWithoutPNRReplyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareList
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListPricingInformation pricingInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareReference fareReferenceField;

            private Fare_PriceUpsellWithoutPNRReplyFareListLastTktDate lastTktDateField;

            private Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrier validatingCarrierField;

            private Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReference paxSegReferenceField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformation fareDataInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListOfferReferences offerReferencesField;

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformation[] taxInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListBankerRates bankerRatesField;

            private string[] originDestinationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformation segmentInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListAttributeDetails[] otherPricingInfoField;

            private Fare_PriceUpsellWithoutPNRReplyFareListWarningInformation[] warningInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroup fareComponentDetailsGroupField;

            private object endFareListField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListPricingInformation pricingInformation
            {
                get
                {
                    return this.pricingInformationField;
                }
                set
                {
                    this.pricingInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareReference fareReference
            {
                get
                {
                    return this.fareReferenceField;
                }
                set
                {
                    this.fareReferenceField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListLastTktDate lastTktDate
            {
                get
                {
                    return this.lastTktDateField;
                }
                set
                {
                    this.lastTktDateField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrier validatingCarrier
            {
                get
                {
                    return this.validatingCarrierField;
                }
                set
                {
                    this.validatingCarrierField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReference paxSegReference
            {
                get
                {
                    return this.paxSegReferenceField;
                }
                set
                {
                    this.paxSegReferenceField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformation fareDataInformation
            {
                get
                {
                    return this.fareDataInformationField;
                }
                set
                {
                    this.fareDataInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListOfferReferences offerReferences
            {
                get
                {
                    return this.offerReferencesField;
                }
                set
                {
                    this.offerReferencesField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("taxInformation")]
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformation[] taxInformation
            {
                get
                {
                    return this.taxInformationField;
                }
                set
                {
                    this.taxInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListBankerRates bankerRates
            {
                get
                {
                    return this.bankerRatesField;
                }
                set
                {
                    this.bankerRatesField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("cityCode", IsNullable = false)]
            public string[] originDestination
            {
                get
                {
                    return this.originDestinationField;
                }
                set
                {
                    this.originDestinationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformation segmentInformation
            {
                get
                {
                    return this.segmentInformationField;
                }
                set
                {
                    this.segmentInformationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("attributeDetails", IsNullable = false)]
            public Fare_PriceUpsellWithoutPNRReplyFareListAttributeDetails[] otherPricingInfo
            {
                get
                {
                    return this.otherPricingInfoField;
                }
                set
                {
                    this.otherPricingInfoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("warningInformation")]
            public Fare_PriceUpsellWithoutPNRReplyFareListWarningInformation[] warningInformation
            {
                get
                {
                    return this.warningInformationField;
                }
                set
                {
                    this.warningInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroup fareComponentDetailsGroup
            {
                get
                {
                    return this.fareComponentDetailsGroupField;
                }
                set
                {
                    this.fareComponentDetailsGroupField = value;
                }
            }

            /// <remarks/>
            public object endFareList
            {
                get
                {
                    return this.endFareListField;
                }
                set
                {
                    this.endFareListField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListPricingInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListPricingInformationTstInformation tstInformationField;

            private byte fcmiField;

            private string bestFareTypeField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListPricingInformationTstInformation tstInformation
            {
                get
                {
                    return this.tstInformationField;
                }
                set
                {
                    this.tstInformationField = value;
                }
            }

            /// <remarks/>
            public byte fcmi
            {
                get
                {
                    return this.fcmiField;
                }
                set
                {
                    this.fcmiField = value;
                }
            }

            /// <remarks/>
            public string bestFareType
            {
                get
                {
                    return this.bestFareTypeField;
                }
                set
                {
                    this.bestFareTypeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListPricingInformationTstInformation
        {

            private string tstIndicatorField;

            /// <remarks/>
            public string tstIndicator
            {
                get
                {
                    return this.tstIndicatorField;
                }
                set
                {
                    this.tstIndicatorField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareReference
        {

            private string referenceTypeField;

            private byte uniqueReferenceField;

            /// <remarks/>
            public string referenceType
            {
                get
                {
                    return this.referenceTypeField;
                }
                set
                {
                    this.referenceTypeField = value;
                }
            }

            /// <remarks/>
            public byte uniqueReference
            {
                get
                {
                    return this.uniqueReferenceField;
                }
                set
                {
                    this.uniqueReferenceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListLastTktDate
        {

            private string businessSemanticField;

            private Fare_PriceUpsellWithoutPNRReplyFareListLastTktDateDateTime dateTimeField;

            /// <remarks/>
            public string businessSemantic
            {
                get
                {
                    return this.businessSemanticField;
                }
                set
                {
                    this.businessSemanticField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListLastTktDateDateTime dateTime
            {
                get
                {
                    return this.dateTimeField;
                }
                set
                {
                    this.dateTimeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListLastTktDateDateTime
        {

            private ushort yearField;

            private byte monthField;

            private byte dayField;

            /// <remarks/>
            public ushort year
            {
                get
                {
                    return this.yearField;
                }
                set
                {
                    this.yearField = value;
                }
            }

            /// <remarks/>
            public byte month
            {
                get
                {
                    return this.monthField;
                }
                set
                {
                    this.monthField = value;
                }
            }

            /// <remarks/>
            public byte day
            {
                get
                {
                    return this.dayField;
                }
                set
                {
                    this.dayField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrier
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrierCarrierInformation carrierInformationField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrierCarrierInformation carrierInformation
            {
                get
                {
                    return this.carrierInformationField;
                }
                set
                {
                    this.carrierInformationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListValidatingCarrierCarrierInformation
        {

            private string carrierCodeField;

            /// <remarks/>
            public string carrierCode
            {
                get
                {
                    return this.carrierCodeField;
                }
                set
                {
                    this.carrierCodeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReference
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReferenceRefDetails refDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReferenceRefDetails refDetails
            {
                get
                {
                    return this.refDetailsField;
                }
                set
                {
                    this.refDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListPaxSegReferenceRefDetails
        {

            private string refQualifierField;

            private byte refNumberField;

            /// <remarks/>
            public string refQualifier
            {
                get
                {
                    return this.refQualifierField;
                }
                set
                {
                    this.refQualifierField = value;
                }
            }

            /// <remarks/>
            public byte refNumber
            {
                get
                {
                    return this.refNumberField;
                }
                set
                {
                    this.refNumberField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataMainInformation fareDataMainInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataSupInformation[] fareDataSupInformationField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataMainInformation fareDataMainInformation
            {
                get
                {
                    return this.fareDataMainInformationField;
                }
                set
                {
                    this.fareDataMainInformationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("fareDataSupInformation")]
            public Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataSupInformation[] fareDataSupInformation
            {
                get
                {
                    return this.fareDataSupInformationField;
                }
                set
                {
                    this.fareDataSupInformationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataMainInformation
        {

            private string fareDataQualifierField;

            /// <remarks/>
            public string fareDataQualifier
            {
                get
                {
                    return this.fareDataQualifierField;
                }
                set
                {
                    this.fareDataQualifierField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareDataInformationFareDataSupInformation
        {

            private string fareDataQualifierField;

            private uint fareAmountField;

            private string fareCurrencyField;

            /// <remarks/>
            public string fareDataQualifier
            {
                get
                {
                    return this.fareDataQualifierField;
                }
                set
                {
                    this.fareDataQualifierField = value;
                }
            }

            /// <remarks/>
            public uint fareAmount
            {
                get
                {
                    return this.fareAmountField;
                }
                set
                {
                    this.fareAmountField = value;
                }
            }

            /// <remarks/>
            public string fareCurrency
            {
                get
                {
                    return this.fareCurrencyField;
                }
                set
                {
                    this.fareCurrencyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListOfferReferences
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListOfferReferencesOfferIdentifier offerIdentifierField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListOfferReferencesOfferIdentifier offerIdentifier
            {
                get
                {
                    return this.offerIdentifierField;
                }
                set
                {
                    this.offerIdentifierField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListOfferReferencesOfferIdentifier
        {

            private string uniqueOfferReferenceField;

            /// <remarks/>
            public string uniqueOfferReference
            {
                get
                {
                    return this.uniqueOfferReferenceField;
                }
                set
                {
                    this.uniqueOfferReferenceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetails taxDetailsField;

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetails amountDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetails taxDetails
            {
                get
                {
                    return this.taxDetailsField;
                }
                set
                {
                    this.taxDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetails amountDetails
            {
                get
                {
                    return this.amountDetailsField;
                }
                set
                {
                    this.amountDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetails
        {

            private byte taxQualifierField;

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxIdentification taxIdentificationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxType taxTypeField;

            private string taxNatureField;

            /// <remarks/>
            public byte taxQualifier
            {
                get
                {
                    return this.taxQualifierField;
                }
                set
                {
                    this.taxQualifierField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxIdentification taxIdentification
            {
                get
                {
                    return this.taxIdentificationField;
                }
                set
                {
                    this.taxIdentificationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxType taxType
            {
                get
                {
                    return this.taxTypeField;
                }
                set
                {
                    this.taxTypeField = value;
                }
            }

            /// <remarks/>
            public string taxNature
            {
                get
                {
                    return this.taxNatureField;
                }
                set
                {
                    this.taxNatureField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxIdentification
        {

            private string taxIdentifierField;

            /// <remarks/>
            public string taxIdentifier
            {
                get
                {
                    return this.taxIdentifierField;
                }
                set
                {
                    this.taxIdentifierField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationTaxDetailsTaxType
        {

            private string isoCountryField;

            /// <remarks/>
            public string isoCountry
            {
                get
                {
                    return this.isoCountryField;
                }
                set
                {
                    this.isoCountryField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetails
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetailsFareDataMainInformation fareDataMainInformationField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetailsFareDataMainInformation fareDataMainInformation
            {
                get
                {
                    return this.fareDataMainInformationField;
                }
                set
                {
                    this.fareDataMainInformationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListTaxInformationAmountDetailsFareDataMainInformation
        {

            private string fareDataQualifierField;

            private ushort fareAmountField;

            private string fareCurrencyField;

            /// <remarks/>
            public string fareDataQualifier
            {
                get
                {
                    return this.fareDataQualifierField;
                }
                set
                {
                    this.fareDataQualifierField = value;
                }
            }

            /// <remarks/>
            public ushort fareAmount
            {
                get
                {
                    return this.fareAmountField;
                }
                set
                {
                    this.fareAmountField = value;
                }
            }

            /// <remarks/>
            public string fareCurrency
            {
                get
                {
                    return this.fareCurrencyField;
                }
                set
                {
                    this.fareCurrencyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListBankerRates
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListBankerRatesFirstRateDetail firstRateDetailField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListBankerRatesFirstRateDetail firstRateDetail
            {
                get
                {
                    return this.firstRateDetailField;
                }
                set
                {
                    this.firstRateDetailField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListBankerRatesFirstRateDetail
        {

            private decimal amountField;

            /// <remarks/>
            public decimal amount
            {
                get
                {
                    return this.amountField;
                }
                set
                {
                    this.amountField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformation connexInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetails segDetailsField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifier fareQualifierField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroup cabinGroupField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformation[] validityInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformation bagAllowanceInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReference segmentReferenceField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformation sequenceInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationType flightProductInformationTypeField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformation connexInformation
            {
                get
                {
                    return this.connexInformationField;
                }
                set
                {
                    this.connexInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetails segDetails
            {
                get
                {
                    return this.segDetailsField;
                }
                set
                {
                    this.segDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifier fareQualifier
            {
                get
                {
                    return this.fareQualifierField;
                }
                set
                {
                    this.fareQualifierField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroup cabinGroup
            {
                get
                {
                    return this.cabinGroupField;
                }
                set
                {
                    this.cabinGroupField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("validityInformation")]
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformation[] validityInformation
            {
                get
                {
                    return this.validityInformationField;
                }
                set
                {
                    this.validityInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformation bagAllowanceInformation
            {
                get
                {
                    return this.bagAllowanceInformationField;
                }
                set
                {
                    this.bagAllowanceInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReference segmentReference
            {
                get
                {
                    return this.segmentReferenceField;
                }
                set
                {
                    this.segmentReferenceField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformation sequenceInformation
            {
                get
                {
                    return this.sequenceInformationField;
                }
                set
                {
                    this.sequenceInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationType flightProductInformationType
            {
                get
                {
                    return this.flightProductInformationTypeField;
                }
                set
                {
                    this.flightProductInformationTypeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformationConnecDetails connecDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformationConnecDetails connecDetails
            {
                get
                {
                    return this.connecDetailsField;
                }
                set
                {
                    this.connecDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationConnexInformationConnecDetails
        {

            private string connexTypeField;

            /// <remarks/>
            public string connexType
            {
                get
                {
                    return this.connexTypeField;
                }
                set
                {
                    this.connexTypeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetails
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetailsSegmentDetail segmentDetailField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetailsSegmentDetail segmentDetail
            {
                get
                {
                    return this.segmentDetailField;
                }
                set
                {
                    this.segmentDetailField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegDetailsSegmentDetail
        {

            private string identificationField;

            private string classOfServiceField;

            /// <remarks/>
            public string identification
            {
                get
                {
                    return this.identificationField;
                }
                set
                {
                    this.identificationField = value;
                }
            }

            /// <remarks/>
            public string classOfService
            {
                get
                {
                    return this.classOfServiceField;
                }
                set
                {
                    this.classOfServiceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifier
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifierFareBasisDetails fareBasisDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifierFareBasisDetails fareBasisDetails
            {
                get
                {
                    return this.fareBasisDetailsField;
                }
                set
                {
                    this.fareBasisDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFareQualifierFareBasisDetails
        {

            private string primaryCodeField;

            private string fareBasisCodeField;

            private string ticketDesignatorField;

            private string discTktDesignatorField;

            /// <remarks/>
            public string primaryCode
            {
                get
                {
                    return this.primaryCodeField;
                }
                set
                {
                    this.primaryCodeField = value;
                }
            }

            /// <remarks/>
            public string fareBasisCode
            {
                get
                {
                    return this.fareBasisCodeField;
                }
                set
                {
                    this.fareBasisCodeField = value;
                }
            }

            /// <remarks/>
            public string ticketDesignator
            {
                get
                {
                    return this.ticketDesignatorField;
                }
                set
                {
                    this.ticketDesignatorField = value;
                }
            }

            /// <remarks/>
            public string discTktDesignator
            {
                get
                {
                    return this.discTktDesignatorField;
                }
                set
                {
                    this.discTktDesignatorField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroup
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegment cabinSegmentField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegment cabinSegment
            {
                get
                {
                    return this.cabinSegmentField;
                }
                set
                {
                    this.cabinSegmentField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegment
        {

            private string productDetailsQualifierField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegmentBookingClassDetails bookingClassDetailsField;

            /// <remarks/>
            public string productDetailsQualifier
            {
                get
                {
                    return this.productDetailsQualifierField;
                }
                set
                {
                    this.productDetailsQualifierField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegmentBookingClassDetails bookingClassDetails
            {
                get
                {
                    return this.bookingClassDetailsField;
                }
                set
                {
                    this.bookingClassDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationCabinGroupCabinSegmentBookingClassDetails
        {

            private string designatorField;

            private string optionField;

            /// <remarks/>
            public string designator
            {
                get
                {
                    return this.designatorField;
                }
                set
                {
                    this.designatorField = value;
                }
            }

            /// <remarks/>
            public string option
            {
                get
                {
                    return this.optionField;
                }
                set
                {
                    this.optionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformation
        {

            private string businessSemanticField;

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformationDateTime dateTimeField;

            /// <remarks/>
            public string businessSemantic
            {
                get
                {
                    return this.businessSemanticField;
                }
                set
                {
                    this.businessSemanticField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformationDateTime dateTime
            {
                get
                {
                    return this.dateTimeField;
                }
                set
                {
                    this.dateTimeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationValidityInformationDateTime
        {

            private ushort yearField;

            private byte monthField;

            private byte dayField;

            /// <remarks/>
            public ushort year
            {
                get
                {
                    return this.yearField;
                }
                set
                {
                    this.yearField = value;
                }
            }

            /// <remarks/>
            public byte month
            {
                get
                {
                    return this.monthField;
                }
                set
                {
                    this.monthField = value;
                }
            }

            /// <remarks/>
            public byte day
            {
                get
                {
                    return this.dayField;
                }
                set
                {
                    this.dayField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformationBagAllowanceDetails bagAllowanceDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformationBagAllowanceDetails bagAllowanceDetails
            {
                get
                {
                    return this.bagAllowanceDetailsField;
                }
                set
                {
                    this.bagAllowanceDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationBagAllowanceInformationBagAllowanceDetails
        {

            private byte baggageWeightField;

            private string baggageTypeField;

            private string measureUnitField;

            /// <remarks/>
            public byte baggageWeight
            {
                get
                {
                    return this.baggageWeightField;
                }
                set
                {
                    this.baggageWeightField = value;
                }
            }

            /// <remarks/>
            public string baggageType
            {
                get
                {
                    return this.baggageTypeField;
                }
                set
                {
                    this.baggageTypeField = value;
                }
            }

            /// <remarks/>
            public string measureUnit
            {
                get
                {
                    return this.measureUnitField;
                }
                set
                {
                    this.measureUnitField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReference
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReferenceRefDetails refDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReferenceRefDetails refDetails
            {
                get
                {
                    return this.refDetailsField;
                }
                set
                {
                    this.refDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSegmentReferenceRefDetails
        {

            private string refQualifierField;

            private byte refNumberField;

            /// <remarks/>
            public string refQualifier
            {
                get
                {
                    return this.refQualifierField;
                }
                set
                {
                    this.refQualifierField = value;
                }
            }

            /// <remarks/>
            public byte refNumber
            {
                get
                {
                    return this.refNumberField;
                }
                set
                {
                    this.refNumberField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformationSequenceSection sequenceSectionField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformationSequenceSection sequenceSection
            {
                get
                {
                    return this.sequenceSectionField;
                }
                set
                {
                    this.sequenceSectionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationSequenceInformationSequenceSection
        {

            private byte sequenceNumberField;

            /// <remarks/>
            public byte sequenceNumber
            {
                get
                {
                    return this.sequenceNumberField;
                }
                set
                {
                    this.sequenceNumberField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationType
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationTypeCabinProduct cabinProductField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationTypeCabinProduct cabinProduct
            {
                get
                {
                    return this.cabinProductField;
                }
                set
                {
                    this.cabinProductField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListSegmentInformationFlightProductInformationTypeCabinProduct
        {

            private string rbdField;

            private string cabinField;

            private byte avlStatusField;

            /// <remarks/>
            public string rbd
            {
                get
                {
                    return this.rbdField;
                }
                set
                {
                    this.rbdField = value;
                }
            }

            /// <remarks/>
            public string cabin
            {
                get
                {
                    return this.cabinField;
                }
                set
                {
                    this.cabinField = value;
                }
            }

            /// <remarks/>
            public byte avlStatus
            {
                get
                {
                    return this.avlStatusField;
                }
                set
                {
                    this.avlStatusField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListAttributeDetails
        {

            private string attributeTypeField;

            private string attributeDescriptionField;

            /// <remarks/>
            public string attributeType
            {
                get
                {
                    return this.attributeTypeField;
                }
                set
                {
                    this.attributeTypeField = value;
                }
            }

            /// <remarks/>
            public string attributeDescription
            {
                get
                {
                    return this.attributeDescriptionField;
                }
                set
                {
                    this.attributeDescriptionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListWarningInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCode warningCodeField;

            private Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningText warningTextField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCode warningCode
            {
                get
                {
                    return this.warningCodeField;
                }
                set
                {
                    this.warningCodeField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningText warningText
            {
                get
                {
                    return this.warningTextField;
                }
                set
                {
                    this.warningTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCode
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCodeApplicationErrorDetail applicationErrorDetailField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCodeApplicationErrorDetail applicationErrorDetail
            {
                get
                {
                    return this.applicationErrorDetailField;
                }
                set
                {
                    this.applicationErrorDetailField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningCodeApplicationErrorDetail
        {

            private string applicationErrorCodeField;

            private string codeListQualifierField;

            private string codeListResponsibleAgencyField;

            /// <remarks/>
            public string applicationErrorCode
            {
                get
                {
                    return this.applicationErrorCodeField;
                }
                set
                {
                    this.applicationErrorCodeField = value;
                }
            }

            /// <remarks/>
            public string codeListQualifier
            {
                get
                {
                    return this.codeListQualifierField;
                }
                set
                {
                    this.codeListQualifierField = value;
                }
            }

            /// <remarks/>
            public string codeListResponsibleAgency
            {
                get
                {
                    return this.codeListResponsibleAgencyField;
                }
                set
                {
                    this.codeListResponsibleAgencyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListWarningInformationWarningText
        {

            private string errorFreeTextField;

            /// <remarks/>
            public string errorFreeText
            {
                get
                {
                    return this.errorFreeTextField;
                }
                set
                {
                    this.errorFreeTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroup
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentID fareComponentIDField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponent marketFareComponentField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformation monetaryInformationField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfo componentClassInfoField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupDiscountDetails[] fareQualifiersDetailField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyDetails fareFamilyDetailsField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwner fareFamilyOwnerField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroup couponDetailsGroupField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentID fareComponentID
            {
                get
                {
                    return this.fareComponentIDField;
                }
                set
                {
                    this.fareComponentIDField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponent marketFareComponent
            {
                get
                {
                    return this.marketFareComponentField;
                }
                set
                {
                    this.marketFareComponentField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformation monetaryInformation
            {
                get
                {
                    return this.monetaryInformationField;
                }
                set
                {
                    this.monetaryInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfo componentClassInfo
            {
                get
                {
                    return this.componentClassInfoField;
                }
                set
                {
                    this.componentClassInfoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("discountDetails", IsNullable = false)]
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupDiscountDetails[] fareQualifiersDetail
            {
                get
                {
                    return this.fareQualifiersDetailField;
                }
                set
                {
                    this.fareQualifiersDetailField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyDetails fareFamilyDetails
            {
                get
                {
                    return this.fareFamilyDetailsField;
                }
                set
                {
                    this.fareFamilyDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwner fareFamilyOwner
            {
                get
                {
                    return this.fareFamilyOwnerField;
                }
                set
                {
                    this.fareFamilyOwnerField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroup couponDetailsGroup
            {
                get
                {
                    return this.couponDetailsGroupField;
                }
                set
                {
                    this.couponDetailsGroupField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentID
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentIDItemNumberDetails itemNumberDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentIDItemNumberDetails itemNumberDetails
            {
                get
                {
                    return this.itemNumberDetailsField;
                }
                set
                {
                    this.itemNumberDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareComponentIDItemNumberDetails
        {

            private byte numberField;

            private string typeField;

            /// <remarks/>
            public byte number
            {
                get
                {
                    return this.numberField;
                }
                set
                {
                    this.numberField = value;
                }
            }

            /// <remarks/>
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponent
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentBoardPointDetails boardPointDetailsField;

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentOffpointDetails offpointDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentBoardPointDetails boardPointDetails
            {
                get
                {
                    return this.boardPointDetailsField;
                }
                set
                {
                    this.boardPointDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentOffpointDetails offpointDetails
            {
                get
                {
                    return this.offpointDetailsField;
                }
                set
                {
                    this.offpointDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentBoardPointDetails
        {

            private string trueLocationIdField;

            /// <remarks/>
            public string trueLocationId
            {
                get
                {
                    return this.trueLocationIdField;
                }
                set
                {
                    this.trueLocationIdField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMarketFareComponentOffpointDetails
        {

            private string trueLocationIdField;

            /// <remarks/>
            public string trueLocationId
            {
                get
                {
                    return this.trueLocationIdField;
                }
                set
                {
                    this.trueLocationIdField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformation
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformationMonetaryDetails monetaryDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformationMonetaryDetails monetaryDetails
            {
                get
                {
                    return this.monetaryDetailsField;
                }
                set
                {
                    this.monetaryDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupMonetaryInformationMonetaryDetails
        {

            private string typeQualifierField;

            private decimal amountField;

            private string currencyField;

            /// <remarks/>
            public string typeQualifier
            {
                get
                {
                    return this.typeQualifierField;
                }
                set
                {
                    this.typeQualifierField = value;
                }
            }

            /// <remarks/>
            public decimal amount
            {
                get
                {
                    return this.amountField;
                }
                set
                {
                    this.amountField = value;
                }
            }

            /// <remarks/>
            public string currency
            {
                get
                {
                    return this.currencyField;
                }
                set
                {
                    this.currencyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfo
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfoFareBasisDetails fareBasisDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfoFareBasisDetails fareBasisDetails
            {
                get
                {
                    return this.fareBasisDetailsField;
                }
                set
                {
                    this.fareBasisDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupComponentClassInfoFareBasisDetails
        {

            private string rateTariffClassField;

            private string otherRateTariffClassField;

            /// <remarks/>
            public string rateTariffClass
            {
                get
                {
                    return this.rateTariffClassField;
                }
                set
                {
                    this.rateTariffClassField = value;
                }
            }

            /// <remarks/>
            public string otherRateTariffClass
            {
                get
                {
                    return this.otherRateTariffClassField;
                }
                set
                {
                    this.otherRateTariffClassField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupDiscountDetails
        {

            private string fareQualifierField;

            /// <remarks/>
            public string fareQualifier
            {
                get
                {
                    return this.fareQualifierField;
                }
                set
                {
                    this.fareQualifierField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyDetails
        {

            private string fareFamilynameField;

            private ushort hierarchyField;

            /// <remarks/>
            public string fareFamilyname
            {
                get
                {
                    return this.fareFamilynameField;
                }
                set
                {
                    this.fareFamilynameField = value;
                }
            }

            /// <remarks/>
            public ushort hierarchy
            {
                get
                {
                    return this.hierarchyField;
                }
                set
                {
                    this.hierarchyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwner
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwnerCompanyIdentification companyIdentificationField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwnerCompanyIdentification companyIdentification
            {
                get
                {
                    return this.companyIdentificationField;
                }
                set
                {
                    this.companyIdentificationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupFareFamilyOwnerCompanyIdentification
        {

            private string otherCompanyField;

            /// <remarks/>
            public string otherCompany
            {
                get
                {
                    return this.otherCompanyField;
                }
                set
                {
                    this.otherCompanyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroup
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductId productIdField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductId productId
            {
                get
                {
                    return this.productIdField;
                }
                set
                {
                    this.productIdField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductId
        {

            private Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductIdReferenceDetails referenceDetailsField;

            /// <remarks/>
            public Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductIdReferenceDetails referenceDetails
            {
                get
                {
                    return this.referenceDetailsField;
                }
                set
                {
                    this.referenceDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        public partial class Fare_PriceUpsellWithoutPNRReplyFareListFareComponentDetailsGroupCouponDetailsGroupProductIdReferenceDetails
        {

            private string typeField;

            private byte valueField;

            /// <remarks/>
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            public byte value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A", IsNullable = false)]
        public partial class Fare_PriceUpsellWithoutPNRReply
        {

            private Fare_PriceUpsellWithoutPNRReplyFareList[] fareListField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("fareList")]
            public Fare_PriceUpsellWithoutPNRReplyFareList[] fareList
            {
                get
                {
                    return this.fareListField;
                }
                set
                {
                    this.fareListField = value;
                }
            }
        }


    }
}