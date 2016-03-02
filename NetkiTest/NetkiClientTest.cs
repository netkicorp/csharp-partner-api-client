using System;
using System.Collections.Generic;
using System.Net;

using NUnit.Framework;
using Netki;
using Moq;
using Newtonsoft.Json.Linq;

namespace NetkiTest
{
    class NetkiClientTest
    {
        private Mock<IRequestor> mockRequestor = new Mock<IRequestor>();

        private string PartnerId = "partner_id";
        private string ApiKey = "api_key";
        private string ApiUrl = "https://server";

        [Test]
        public void NetkiInstantiation()
        {
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl);
        }

        [Test]
        public void GetWalletNamesNoWalletNames()
        {
            Dictionary<string, object> WalletNameGetResponse = new Dictionary<string, object>();
            WalletNameGetResponse.Add("wallet_name_count", 0);

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname", "GET", null)).Returns(JObject.FromObject(WalletNameGetResponse).ToString());

            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);
            List<WalletName> results = netki.GetWalletNames();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname", "GET", It.IsAny<string>()));

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void GetWalletNamesAllOptions()
        {

            var WalletNameObj = new Dictionary<string, object>
            {
                {"id", "id" },
                {"domain_name", "domain.com" },
                {"name", "wallet" },
                {"external_id", "external_id" },
                {"wallets", new Dictionary<string, string>[] {
                    new Dictionary<string, string>
                    {
                        {"currency", "btc" },
                        {"wallet_address", "1btcaddress" }
                    },
                    new Dictionary<string, string>
                    {
                        {"currency", "ltc" },
                        {"wallet_address", "Ltcaddress42" }
                    }
                } }
            };

            var GetResponse = new Dictionary<string, object>
            {
                { "wallet_name_count", 1 },
                { "wallet_names", new object[] { WalletNameObj } }
            };

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname?domain_name=domain.com&external_id=external_id", "GET", null)).Returns(JObject.FromObject(GetResponse).ToString());

            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);
            List<WalletName> results = netki.GetWalletNames("domain.com", "external_id");

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname?domain_name=domain.com&external_id=external_id", "GET", It.IsAny<string>()));

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("id", results[0].Id);
            Assert.AreEqual("domain.com", results[0].DomainName);
            Assert.AreEqual("wallet", results[0].Name);
            Assert.AreEqual("external_id", results[0].ExternalId);
            Assert.AreEqual("1btcaddress", results[0].GetWalletAddress("btc"));
            Assert.AreEqual("Ltcaddress42", results[0].GetWalletAddress("ltc"));
            Assert.Contains("btc", results[0].GetUsedCurrencies());
            Assert.Contains("ltc", results[0].GetUsedCurrencies());
        }

        [Test]
        public void CreateWalletName()
        {
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);
            WalletName walletName = netki.CreateWalletName("domain.com", "wallet", "external_id");

            Assert.AreEqual("domain.com", walletName.DomainName);
            Assert.AreEqual("wallet", walletName.Name);
            Assert.AreEqual("external_id", walletName.ExternalId);
            Assert.AreEqual(PartnerId, walletName.GetPartnerId());
            Assert.AreEqual(ApiKey, walletName.GetApiKey());
            Assert.AreEqual(ApiUrl, walletName.GetApiUrl());
        }

        [Test]
        public void CreatePartner()
        {
            var PostResponse = new Dictionary<string, object>
            {
                {"partner", new Dictionary<string, string>
                {
                    {"id", "partnerId" },
                    {"name", "My Partner" }
                }
                }
            };

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/admin/partner/My Partner", "POST", null)).Returns(JObject.FromObject(PostResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            Partner partner = netki.CreatePartner("My Partner");

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/admin/partner/My Partner", "POST", It.IsAny<string>()));

            Assert.AreEqual("partnerId", partner.Id);
            Assert.AreEqual("My Partner", partner.Name);
            Assert.AreEqual(PartnerId, partner.GetPartnerId());
            Assert.AreEqual(ApiKey, partner.GetApiKey());
            Assert.AreEqual(ApiUrl, partner.GetApiUrl());

        }

        [Test]
        public void GetPartnersNoPartners()
        {
            var GetResponse = new Dictionary<string, string>();
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/admin/partner", "GET", null)).Returns(JObject.FromObject(GetResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            var partners = netki.GetPartners();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/admin/partner", "GET", It.IsAny<string>()));

            Assert.AreEqual(0, partners.Count);
        }

        [Test]
        public void GetPartnersGoRight()
        {
            var GetResponse = new Dictionary<string, object>
            {
                {"partners", new List<object> { new Dictionary<string, string>
                {
                    {"id", "partner_id" },
                    {"name", "Test Partner" }
                } } }
            };

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/admin/partner", "GET", null)).Returns(JObject.FromObject(GetResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            var partners = netki.GetPartners();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/admin/partner", "GET", It.IsAny<string>()));

            Assert.AreEqual(1, partners.Count);
            Assert.AreEqual("partner_id", partners[0].Id);
            Assert.AreEqual("Test Partner", partners[0].Name);
            Assert.AreEqual(PartnerId, partners[0].GetPartnerId());
            Assert.AreEqual(ApiKey, partners[0].GetApiKey());
            Assert.AreEqual(ApiUrl, partners[0].GetApiUrl());
        }

        [Test]
        public void CreateDomainWithSubpartner()
        {
            var PostResponse = new Dictionary<string, object> {
                { "status", "completed"},
                { "nameservers", new string[] {
                    "ns1.domain.com",
                    "ns2.domain.com"
                }}
            };
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/domain.com", "POST", It.IsAny<string>())).Returns(JObject.FromObject(PostResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            Partner partner = new Partner();
            partner.Id = "SubPartnerId";
            Domain domain = netki.CreateDomain("domain.com", partner);

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/domain/domain.com", "POST", "{\r\n  \"partner_id\": \"SubPartnerId\"\r\n}"));

            Assert.AreEqual("domain.com", domain.Name);
            Assert.AreEqual("completed", domain.Status);
            Assert.Contains("ns1.domain.com", domain.Nameservers);
            Assert.Contains("ns2.domain.com", domain.Nameservers);
        }

        [Test]
        public void CreateDomainWithoutSubpartner()
        {
            var PostResponse = new Dictionary<string, object> {
                { "status", "completed"},
                { "nameservers", new string[] {
                    "ns1.domain.com",
                    "ns2.domain.com"
                }}
            };
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/domain.com", "POST", null)).Returns(JObject.FromObject(PostResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            Domain domain = netki.CreateDomain("domain.com");

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/domain/domain.com", "POST", null));

            Assert.AreEqual("domain.com", domain.Name);
            Assert.AreEqual("completed", domain.Status);
            Assert.Contains("ns1.domain.com", domain.Nameservers);
            Assert.Contains("ns2.domain.com", domain.Nameservers);
        }

        [Test]
        public void GetDomainsNull()
        {
            var GetResponse = new Dictionary<string, string>();
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/api/domain", "GET", null)).Returns(JObject.FromObject(GetResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            List<Domain> result = netki.GetDomains();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/api/domain", "GET", null));
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetDomains()
        {
            // Setup Domain Data Loads
            Dictionary<string, object> DomainStatusData = new Dictionary<string, object>
            {
                {"status", "status" },
                {"delegation_status", true },
                {"delegation_message", "delegated" },
                {"wallet_name_count", 42 }
            };

            Dictionary<string, object> DomainDnssecData = new Dictionary<string, object>
            {
                {"public_key_signing_key", "PUBKEY" },
                {"nextroll_date",  new DateTime(1980, 6, 13, 1, 2, 3)},
                {"ds_records", new string[] {"DS1", "DS2"} },
                {"nameservers", new string[] {"ns1.domain.com", "ns2.domain.com"} }
            };

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/domain.com", "GET", null)).Returns(JObject.FromObject(DomainStatusData).ToString());
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/domain/dnssec/domain.com", "GET", null)).Returns(JObject.FromObject(DomainDnssecData).ToString());

            // Setup GetDomain Response Data
            var GetResponse = new Dictionary<string, object>
            {
                {"domains", new object[] {  new Dictionary<string, string>
                {
                    {"domain_name", "domain.com" }
                } } }
            };
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/api/domain", "GET", null)).Returns(JObject.FromObject(GetResponse).ToString());
            NetkiClient netki = new NetkiClient(PartnerId, ApiKey, ApiUrl, mockRequestor.Object);

            List<Domain> result = netki.GetDomains();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/api/domain", "GET", null));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("domain.com", result[0].Name);
            Assert.AreEqual(PartnerId, result[0].GetPartnerId());
            Assert.AreEqual(ApiKey, result[0].GetApiKey());
            Assert.AreEqual(ApiUrl, result[0].GetApiUrl());
        }
    }
}
