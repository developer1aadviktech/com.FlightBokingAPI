using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace com.ThirdPartyAPIs.Amadeus.Flight
{
    public class Fare_GetFareFamilyDescription__old
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

            private Fare_GetFareFamilyDescriptionReply fare_GetFareFamilyDescriptionReplyField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
            public Fare_GetFareFamilyDescriptionReply Fare_GetFareFamilyDescriptionReply
            {
                get
                {
                    return this.fare_GetFareFamilyDescriptionReplyField;
                }
                set
                {
                    this.fare_GetFareFamilyDescriptionReplyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A", IsNullable = false)]
        public partial class Fare_GetFareFamilyDescriptionReply
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroup fareFamilyDescriptionGroupField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroup fareFamilyDescriptionGroup
            {
                get
                {
                    return this.fareFamilyDescriptionGroupField;
                }
                set
                {
                    this.fareFamilyDescriptionGroupField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroup
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformation referenceInformationField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformation fareInformationField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformation carrierInformationField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescription freeFlowDescriptionField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformation[] ocFeeInformationField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamily errorInformationFareFamilyField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformation referenceInformation
            {
                get
                {
                    return this.referenceInformationField;
                }
                set
                {
                    this.referenceInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformation fareInformation
            {
                get
                {
                    return this.fareInformationField;
                }
                set
                {
                    this.fareInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformation carrierInformation
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

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescription freeFlowDescription
            {
                get
                {
                    return this.freeFlowDescriptionField;
                }
                set
                {
                    this.freeFlowDescriptionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("ocFeeInformation")]
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformation[] ocFeeInformation
            {
                get
                {
                    return this.ocFeeInformationField;
                }
                set
                {
                    this.ocFeeInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamily errorInformationFareFamily
            {
                get
                {
                    return this.errorInformationFareFamilyField;
                }
                set
                {
                    this.errorInformationFareFamilyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformation
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformationItemNumberDetails itemNumberDetailsField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformationItemNumberDetails itemNumberDetails
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupReferenceInformationItemNumberDetails
        {

            private byte numberField;

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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformation
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformationDiscountDetails discountDetailsField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformationDiscountDetails discountDetails
            {
                get
                {
                    return this.discountDetailsField;
                }
                set
                {
                    this.discountDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFareInformationDiscountDetails
        {

            private string rateCategoryField;

            /// <remarks/>
            public string rateCategory
            {
                get
                {
                    return this.rateCategoryField;
                }
                set
                {
                    this.rateCategoryField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformation
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformationCompanyIdentification companyIdentificationField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformationCompanyIdentification companyIdentification
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupCarrierInformationCompanyIdentification
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescription
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescriptionFreeTextDetails freeTextDetailsField;

            private string freeTextField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescriptionFreeTextDetails freeTextDetails
            {
                get
                {
                    return this.freeTextDetailsField;
                }
                set
                {
                    this.freeTextDetailsField = value;
                }
            }

            /// <remarks/>
            public string freeText
            {
                get
                {
                    return this.freeTextField;
                }
                set
                {
                    this.freeTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupFreeFlowDescriptionFreeTextDetails
        {

            private string textSubjectQualifierField;

            private string informationTypeField;

            private string sourceField;

            private byte encodingField;

            /// <remarks/>
            public string textSubjectQualifier
            {
                get
                {
                    return this.textSubjectQualifierField;
                }
                set
                {
                    this.textSubjectQualifierField = value;
                }
            }

            /// <remarks/>
            public string informationType
            {
                get
                {
                    return this.informationTypeField;
                }
                set
                {
                    this.informationTypeField = value;
                }
            }

            /// <remarks/>
            public string source
            {
                get
                {
                    return this.sourceField;
                }
                set
                {
                    this.sourceField = value;
                }
            }

            /// <remarks/>
            public byte encoding
            {
                get
                {
                    return this.encodingField;
                }
                set
                {
                    this.encodingField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformation
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescription feeDescriptionField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetails serviceDetailsField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescription feeFreeFlowDescriptionField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescription feeDescription
            {
                get
                {
                    return this.feeDescriptionField;
                }
                set
                {
                    this.feeDescriptionField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetails serviceDetails
            {
                get
                {
                    return this.serviceDetailsField;
                }
                set
                {
                    this.serviceDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescription feeFreeFlowDescription
            {
                get
                {
                    return this.feeFreeFlowDescriptionField;
                }
                set
                {
                    this.feeFreeFlowDescriptionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescription
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataTypeInformation dataTypeInformationField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataInformation dataInformationField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataTypeInformation dataTypeInformation
            {
                get
                {
                    return this.dataTypeInformationField;
                }
                set
                {
                    this.dataTypeInformationField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataInformation dataInformation
            {
                get
                {
                    return this.dataInformationField;
                }
                set
                {
                    this.dataInformationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataTypeInformation
        {

            private string typeField;

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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeDescriptionDataInformation
        {

            private string indicatorField;

            /// <remarks/>
            public string indicator
            {
                get
                {
                    return this.indicatorField;
                }
                set
                {
                    this.indicatorField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetails
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetailsSpecialRequirementsInfo specialRequirementsInfoField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetailsSpecialRequirementsInfo specialRequirementsInfo
            {
                get
                {
                    return this.specialRequirementsInfoField;
                }
                set
                {
                    this.specialRequirementsInfoField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationServiceDetailsSpecialRequirementsInfo
        {

            private string ssrCodeField;

            private string airlineCodeField;

            private string serviceTypeField;

            private string otherServiceTypeField;

            /// <remarks/>
            public string ssrCode
            {
                get
                {
                    return this.ssrCodeField;
                }
                set
                {
                    this.ssrCodeField = value;
                }
            }

            /// <remarks/>
            public string airlineCode
            {
                get
                {
                    return this.airlineCodeField;
                }
                set
                {
                    this.airlineCodeField = value;
                }
            }

            /// <remarks/>
            public string serviceType
            {
                get
                {
                    return this.serviceTypeField;
                }
                set
                {
                    this.serviceTypeField = value;
                }
            }

            /// <remarks/>
            public string otherServiceType
            {
                get
                {
                    return this.otherServiceTypeField;
                }
                set
                {
                    this.otherServiceTypeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescription
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescriptionFreeTextDetails freeTextDetailsField;

            private string freeTextField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescriptionFreeTextDetails freeTextDetails
            {
                get
                {
                    return this.freeTextDetailsField;
                }
                set
                {
                    this.freeTextDetailsField = value;
                }
            }

            /// <remarks/>
            public string freeText
            {
                get
                {
                    return this.freeTextField;
                }
                set
                {
                    this.freeTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupOcFeeInformationFeeFreeFlowDescriptionFreeTextDetails
        {

            private byte textSubjectQualifierField;

            private string sourceField;

            private byte encodingField;

            /// <remarks/>
            public byte textSubjectQualifier
            {
                get
                {
                    return this.textSubjectQualifierField;
                }
                set
                {
                    this.textSubjectQualifierField = value;
                }
            }

            /// <remarks/>
            public string source
            {
                get
                {
                    return this.sourceField;
                }
                set
                {
                    this.sourceField = value;
                }
            }

            /// <remarks/>
            public byte encoding
            {
                get
                {
                    return this.encodingField;
                }
                set
                {
                    this.encodingField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamily
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetails errorOrWarningCodeDetailsField;

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescription errorWarningDescriptionField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetails errorOrWarningCodeDetails
            {
                get
                {
                    return this.errorOrWarningCodeDetailsField;
                }
                set
                {
                    this.errorOrWarningCodeDetailsField = value;
                }
            }

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescription errorWarningDescription
            {
                get
                {
                    return this.errorWarningDescriptionField;
                }
                set
                {
                    this.errorWarningDescriptionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetails
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetailsErrorDetails errorDetailsField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetailsErrorDetails errorDetails
            {
                get
                {
                    return this.errorDetailsField;
                }
                set
                {
                    this.errorDetailsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorOrWarningCodeDetailsErrorDetails
        {

            private ushort errorCodeField;

            /// <remarks/>
            public ushort errorCode
            {
                get
                {
                    return this.errorCodeField;
                }
                set
                {
                    this.errorCodeField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescription
        {

            private Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescriptionFreeTextDetails freeTextDetailsField;

            private string freeTextField;

            /// <remarks/>
            public Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescriptionFreeTextDetails freeTextDetails
            {
                get
                {
                    return this.freeTextDetailsField;
                }
                set
                {
                    this.freeTextDetailsField = value;
                }
            }

            /// <remarks/>
            public string freeText
            {
                get
                {
                    return this.freeTextField;
                }
                set
                {
                    this.freeTextField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://xml.amadeus.com/TFQFRR_18_1_1A")]
        public partial class Fare_GetFareFamilyDescriptionReplyFareFamilyDescriptionGroupErrorInformationFareFamilyErrorWarningDescriptionFreeTextDetails
        {

            private byte textSubjectQualifierField;

            private string sourceField;

            private byte encodingField;

            /// <remarks/>
            public byte textSubjectQualifier
            {
                get
                {
                    return this.textSubjectQualifierField;
                }
                set
                {
                    this.textSubjectQualifierField = value;
                }
            }

            /// <remarks/>
            public string source
            {
                get
                {
                    return this.sourceField;
                }
                set
                {
                    this.sourceField = value;
                }
            }

            /// <remarks/>
            public byte encoding
            {
                get
                {
                    return this.encodingField;
                }
                set
                {
                    this.encodingField = value;
                }
            }
        }


    }
    public class Fare_GetFareFamilyDescription_old
    {
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
        //[System.SerializableAttribute()]
        //[System.ComponentModel.DesignerCategoryAttribute("code")]
        //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        [Serializable]
        [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public partial class EnvelopeBody
        {
            // ⚠️ Amadeus usually sends this WITHOUT namespace
            [XmlElement("Fare_GetFareFamilyDescriptionReply")]
            public Amadeus_wsdl.Fare_GetFareFamilyDescriptionReply
                Fare_GetFareFamilyDescriptionReply
            { get; set; }


            //private Amadeus_wsdl.Fare_GetFareFamilyDescriptionReply fare_GetFareFamilyDescriptionReplyField;

            ///// <remarks/>
            //[System.Xml.Serialization.XmlElementAttribute(Namespace = "http://xml.amadeus.com/FMPTBR_23_4_1A")]
            //public Amadeus_wsdl.Fare_GetFareFamilyDescriptionReply fare_GetFareFamilyDescriptionReplyReply
            //{
            //    get
            //    {
            //        return this.fare_GetFareFamilyDescriptionReplyField;
            //    }
            //    set
            //    {
            //        //ErrorGroupType17
            //        this.fare_GetFareFamilyDescriptionReplyField = value;
            //    }
            //}

            //private Amadeus_wsdl.Fare_PriceUpsellWithoutPNRReplyFareList[] fare_PriceUpsellWithoutPNRReplyField;

            ///// <remarks/>
            //[System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://xml.amadeus.com/TIUNRR_23_1_1A")]
            //[System.Xml.Serialization.XmlArrayItemAttribute("fareList", IsNullable = false)]
            //public Amadeus_wsdl.Fare_PriceUpsellWithoutPNRReplyFareList[] Fare_PriceUpsellWithoutPNRReply
            //{
            //    get
            //    {
            //        return this.fare_PriceUpsellWithoutPNRReplyField;
            //    }
            //    set
            //    {
            //        this.fare_PriceUpsellWithoutPNRReplyField = value;
            //    }
            //}

        }

    }
}
