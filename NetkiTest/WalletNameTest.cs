using System;
using System.Collections.Generic;

using NUnit.Framework;
using Netki;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetkiTest
{

    [TestFixture]
    class WalletNameTest
    {

        Mock<IRequestor> mockRequestor = new Mock<IRequestor>();

        [Test]
        public void WalletNameAccessorsTest()
        {
            WalletName walletName = new WalletName();
            Assert.IsNull(walletName.Id);
            Assert.IsNull(walletName.Name);
            Assert.IsNull(walletName.DomainName);
            Assert.IsNull(walletName.ExternalId);

            // Validate Empty Getter Returns
            Assert.AreEqual(0, walletName.GetUsedCurrencies().Count);
            Assert.IsNull(walletName.GetWalletAddress("btc"));

            // Set Currency Address
            walletName.SetCurrencyAddress("btc", "1btcaddress");
            Assert.AreEqual("1btcaddress", walletName.GetWalletAddress("btc"));

            // Remove Currency Address
            walletName.RemoveCurrencyAddress("btc");
            Assert.IsNull(walletName.GetWalletAddress("btc"));
        }

        [Test]
        public void TestSaveNewMatchingReturnData()
        {
            Dictionary<string, string> retWallet = new Dictionary<string, string>();
            retWallet.Add("name", "wallet");
            retWallet.Add("domain_name", "domain.com");
            retWallet.Add("id", "new_id");

            List<object> retList = new List<object>();
            retList.Add(retWallet);

            Dictionary<string, object> retData = new Dictionary<string, object>();
            retData.Add("wallet_names", retList);

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname", "POST", It.IsAny<string>())).Returns(JsonConvert.SerializeObject(retData));

            WalletName walletName = new WalletName(mockRequestor.Object);
            walletName.SetApiOpts("https://server", "api_key", "partner_id");
            walletName.DomainName = "domain.com";
            walletName.Name = "wallet";
            walletName.ExternalId = "external_id";
            walletName.SetCurrencyAddress("btc", "1btcaddress");

            walletName.Save();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname", "POST", It.IsAny<string>()));

            // Validate ID
            Assert.AreEqual("new_id", walletName.Id);
        }

        [Test]
        public void TestSaveNewNoMatchReturnData()
        {
            Dictionary<string, string> retWallet = new Dictionary<string, string>();
            retWallet.Add("name", "wrongwallet");
            retWallet.Add("domain_name", "domain.com");
            retWallet.Add("id", "new_id");

            List<object> retList = new List<object>();
            retList.Add(retWallet);

            Dictionary<string, object> retData = new Dictionary<string, object>();
            retData.Add("wallet_names", retList);

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname", "POST", It.IsAny<string>())).Returns(JsonConvert.SerializeObject(retData));

            WalletName walletName = new WalletName(mockRequestor.Object);
            walletName.SetApiOpts("https://server", "api_key", "partner_id");
            walletName.DomainName = "domain.com";
            walletName.Name = "wallet";
            walletName.ExternalId = "external_id";
            walletName.SetCurrencyAddress("btc", "1btcaddress");

            walletName.Save();

            // Validate Call
            string callData = JObject.Parse("{'wallet_names': [{'name':'wallet', 'domain':'domain.com', 'external_id':'external_id', 'wallets':[{'currency':'btc', 'wallet_address':'1btcadddress'}]}]}").ToString();
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname", "POST", It.IsAny<string>()));

            // Validate ID
            Assert.IsNull(walletName.Id);
        }

        [Test]
        public void TestSaveExisting()
        {
            Dictionary<string, string> retWallet = new Dictionary<string, string>();
            retWallet.Add("name", "wallet");
            retWallet.Add("domain_name", "domain.com");
            retWallet.Add("id", "new_id");
            
            List<object> retList = new List<object>();
            retList.Add(retWallet);

            Dictionary<string, object> retData = new Dictionary<string, object>();
            retData.Add("wallet_names", retList);

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname", "PUT", It.IsAny<string>())).Returns(JsonConvert.SerializeObject(retData));

            WalletName walletName = new WalletName(mockRequestor.Object);
            walletName.SetApiOpts("https://server", "api_key", "partner_id");
            walletName.DomainName = "domain.com";
            walletName.Name = "wallet";
            walletName.ExternalId = "external_id";
            walletName.Id = "id";
            walletName.SetCurrencyAddress("btc", "1btcaddress");

            walletName.Save();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname", "PUT", It.IsAny<string>()));

            // Validate ID
            Assert.AreEqual("id", walletName.Id);
        }

        [Test]
        public void TestDeleteMissingId()
        {
            WalletName walletName = new WalletName();
            try
            {
                walletName.Delete();
                Assert.IsFalse(true);
            } catch (Exception e) {
                Assert.AreEqual("Unable to Delete Object that Does Not Exist Remotely", e.Message);
            }

        }

        [Test]
        public void TestDelete()
        {
            WalletName walletName = new WalletName(mockRequestor.Object);
            walletName.DomainName = "domain.com";
            walletName.Id = "id";
            walletName.SetApiOpts("https://server", "api_key", "partner_id");

            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), "https://server/v1/partner/walletname", "DELETE", It.IsAny<string>()));
            walletName.Delete();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/partner/walletname", "DELETE", It.IsAny<string>()));

        }
    }
}
