using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace com.ThirdPartyAPIs.Amadeus.Flight
{
    public class Fare_PriceUpsellWithoutPNR_response_old
    {

        // Matches SOAP Envelope
        [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public class Envelope
        {
            [XmlElement("Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public Header? Header { get; set; }

            [XmlElement("Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public Body? Body { get; set; }
        }

        public class Header
        {
            // preserve unknown header children (e.g. wsa, awsse)
            [XmlAnyElement]
            public XmlElement[]? Any { get; set; }
        }

        public class Body
        {
            // If a Fault is returned, it will be deserialized here
            [XmlElement("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
            public SoapFault? Fault { get; set; }

            // Capture the concrete response element (Fare_PriceUpsellWithoutPNRReply) if present.
            // Use XmlAnyElement to tolerate namespace/name variations.
            [XmlAnyElement]
            public XmlElement[]? Any { get; set; }

            // Helper to find the first element with the given local name
            public XmlElement? FindElementByLocalName(string localName)
            {
                if (Any == null) return null;
                foreach (var el in Any)
                {
                    if (el.LocalName == localName) return el;
                }
                return null;
            }
        }

        public class SoapFault
        {
            [XmlElement("faultcode")]
            public string? FaultCode { get; set; }

            [XmlElement("faultstring")]
            public string? FaultString { get; set; }

            [XmlAnyElement]
            public XmlElement[]? Detail { get; set; }
        }

        // Optional typed wrapper for the business response; inner structure captured as XmlAnyElement
        [XmlRoot("Fare_PriceUpsellWithoutPNRReply", Namespace = "http://xml.amadeus.com/")]
        public class Fare_PriceUpsellWithoutPNRReply
        {
            // keep flexible: capture children so you can parse required nodes later
            [XmlAnyElement]
            public XmlElement[]? Any { get; set; }
        }
    }
}
