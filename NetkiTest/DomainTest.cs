using System.Collections.Generic;
using System;

using NUnit.Framework;
using Netki;
using Moq;
using Newtonsoft.Json.Linq;

namespace NetkiTest
{
    [TestFixture]
    class DomainTest
    {

        private Dictionary<string, object> DomainStatusData;
        private Dictionary<string, object> DomainDnssecData;
        private Mock<IRequestor> mockRequestor = new Mock<IRequestor>();

        [TestFixtureSetUp]
        public void InitialSetup()
        {
            // Setup DomainStatusData
            DomainStatusData = new Dictionary<string, object>
            {
                {"status", "status" },
                {"delegation_status", true },
                {"delegation_message", "delegated" },
                {"wallet_name_count", 42 }
            };

            DomainDnssecData = new Dictionary<string, object>
            {
                {"public_key_signing_key", "PUBKEY" },
                {"nextroll_date",  new DateTime(1980, 6, 13, 1, 2, 3)},
                {"ds_records", new string[] {"DS1", "DS2"} },
                {"nameservers", new string[] {"ns1.domain.com", "ns2.domain.com"} }
            };

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/domain.com", "GET", null)).Returns(JObject.FromObject(DomainStatusData).ToString());
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/dnssec/domain.com", "GET", null)).Returns(JObject.FromObject(DomainDnssecData).ToString());
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/domain.com", "DELETE", null));
        }

        [Test]
        public void DomainCreate()
        {
            Domain domain = new Domain("domain.com");
            Assert.AreEqual("domain.com", domain.Name);
        }

        [Test]
        public void DomainDelete()
        {

            // Setup Domain
            Domain domain = new Domain("domain.com", mockRequestor.Object);
            domain.SetApiOpts("https://server", "api_key", "partner_id");

            domain.Delete();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/domain/domain.com", "DELETE", null));

        }

        [Test]
        public void LoadStatus()
        {

            // Setup Domain
            Domain domain = new Domain("domain.com", mockRequestor.Object);
            domain.SetApiOpts("https://server", "api_key", "partner_id");

            domain.LoadStatus();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/domain/domain.com", "GET", null));

            // Validate domain status data
            Assert.AreEqual("status", domain.Status);
            Assert.IsTrue(domain.DelegationStatus);
            Assert.AreEqual("delegated", domain.DelegationMessage);
            Assert.AreEqual(42, domain.WalletNameCount);
        }

        [Test]
        public void LoadDnssecDetails()
        {

            // Setup Domain
            Domain domain = new Domain("domain.com", mockRequestor.Object);
            domain.SetApiOpts("https://server", "api_key", "partner_id");

            domain.LoadDnssecDetails();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/domain/dnssec/domain.com", "GET", null));

            // Validate domain status data
            Assert.AreEqual("PUBKEY", domain.PublicKeySigningKey);

            Assert.AreEqual(2, domain.DsRecords.Count);
            Assert.IsTrue(domain.DsRecords.Contains("DS1"));
            Assert.IsTrue(domain.DsRecords.Contains("DS2"));

            Assert.AreEqual(2, domain.Nameservers.Count);
            Assert.IsTrue(domain.Nameservers.Contains("ns1.domain.com"));
            Assert.IsTrue(domain.Nameservers.Contains("ns2.domain.com"));

            Assert.AreEqual(new DateTime(1980, 6, 13, 1, 2, 3), domain.NextRoll);
        }
    }
}
