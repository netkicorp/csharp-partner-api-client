using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Netki
{
	public class NetkiClient
	{

		private string partnerId;
		private string apiKey;
		private string apiUrl = "https://api.netki.com";
        private IRequestor requestor = new Requestor();

		public NetkiClient (string partnerId, string apiKey, string apiUrl, IRequestor requestor = null)
		{
			this.partnerId = partnerId;
			this.apiKey = apiKey;
			if (apiUrl != null && apiUrl != "") {
				this.apiUrl = apiUrl;
			}
            
            if(requestor != null)
            {
                this.requestor = requestor;
            }
		}

		public List<WalletName> GetWalletNames() {
			return this.GetWalletNames(null, null);
		}

		public List<WalletName> GetWalletNames(string domainName, string externalId = null) {

			List<WalletName> results = new List<WalletName> ();

			List<string> args = new List<string>();
			if (domainName != null && domainName != "") {
				args.Add (string.Format ("domain_name={0}", domainName));
			}

			if (externalId != null && externalId != "") {
				args.Add (string.Format ("external_id={0}", externalId));
			}

			string uri = string.Format ("{0}/v1/partner/walletname", this.apiUrl);
			if (args.Count > 0) {
				uri = string.Format("{0}?{1}", uri, string.Join("&", args.ToArray()));
			}

			string respStr = requestor.ProcessRequest (
				                 apiKey,
				                 partnerId,
				                 uri,
				                 "GET",
				                 null
			                 );

			JObject respJson = JObject.Parse (respStr);
			if (respJson ["wallet_name_count"].ToObject<int>() == 0) {
				return results;
			}

			foreach (var data in respJson["wallet_names"].Children()) {
				WalletName wn = new WalletName ();
				wn.Id = data["id"].ToString();
				wn.DomainName = data["domain_name"].ToString();
				wn.Name = data["name"].ToString();
				wn.ExternalId = data["external_id"].ToString();

				foreach (var wallet in data["wallets"].Children()) {
					wn.SetCurrencyAddress (wallet ["currency"].ToString(), wallet ["wallet_address"].ToString());
				}

				wn.SetApiOpts (apiUrl, apiKey, partnerId);
				results.Add (wn);
			}

			return results;
		}

		public WalletName CreateWalletName(string domainName, string name, string externalId) {
			WalletName wn = new WalletName ();
			wn.DomainName = domainName;
			wn.Name = name;
			wn.ExternalId = externalId;
			wn.SetApiOpts(apiUrl, apiKey, partnerId);
			return wn;
		}

		/*
		 * Partner Operations
		 */
		public Partner CreatePartner(string partnerName) {

            string responseStr = requestor.ProcessRequest (
				                     apiKey,
				                     partnerId,
				                     string.Format ("{0}/v1/admin/partner/{1}", apiUrl, partnerName),
				                     "POST",
				                     null
			                     );

			JObject data = JObject.Parse(responseStr);
			Partner partner = new Partner(data ["partner"]["id"].ToString(), data ["partner"]["name"].ToString());
			partner.SetApiOpts(apiUrl, apiKey, partnerId);
			return partner;
		}

		public List<Partner> GetPartners() {

			List<Partner> partners = new List<Partner>();

            string responseStr = requestor.ProcessRequest (
				                     apiKey,
				                     partnerId,
				                     string.Format ("{0}/v1/admin/partner", apiUrl),
				                     "GET",
				                     null
			                     );

			JObject data = JObject.Parse (responseStr);
			if (data ["partners"] == null) {
				return partners;
			}

			foreach (var partner in data["partners"].Children()) {
				Partner p = new Partner (partner["id"].ToString(), partner["name"].ToString());
				p.SetApiOpts (apiUrl, apiKey, partnerId);
				partners.Add (p);
			}

			return partners;
		}

		/*
		 * Domains Operations
		 */
		public Domain CreateDomain(string domainName, Partner partner = null) {
			string submitData = null;

			if (partner != null) {
				Dictionary<string, string> subDict = new Dictionary<string, string> ();
				subDict.Add ("partner_id", partner.Id);
				submitData = JObject.FromObject (subDict).ToString ();
			}

            string responseStr = requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format ("{0}/v1/partner/domain/{1}", apiUrl, domainName),
				"POST",
				submitData
			);

			JObject data = JObject.Parse (responseStr);
			Domain domain = new Domain(domainName);
			domain.SetApiOpts(apiUrl, apiKey, partnerId);
			domain.Status = data["status"].ToString ();
			foreach (var nsObj in data["nameservers"].Children()) {
				domain.Nameservers.Add(nsObj.ToString ());
			}
			return domain;
		}

		public List<Domain> GetDomains() {

			List<Domain> domains = new List<Domain>();

			string responseStr = requestor.ProcessRequest (
				                     apiKey,
				                     partnerId,
				                     string.Format ("{0}/api/domain", apiUrl),
				                     "GET",
				                     null
			);

			JObject data = JObject.Parse (responseStr);
			if (data["domains"] == null) {
				return domains;
			}

			foreach (var domain in data["domains"].Children()) {
				Domain d = new Domain(domain["domain_name"].ToString(), requestor);
				d.SetApiOpts(apiUrl, apiKey, partnerId);
				d.LoadStatus();
				d.LoadDnssecDetails();
                domains.Add(d);
			}

			return domains;
		}

		static void Main(string[] args) {

//			string partnerId = "XXXXXXXXXXXXXXXXXXXX";
//            string apiKey = "XXXXXXXXXXXXXXXXXXXXXXXXX";
//
//            var client = new NetkiClient(partnerId, apiKey, "http://localhost:5000");
//            List<Domain> domains = client.GetDomains();
//
//			foreach (var domain in domains) {
//				if (domain.Name == "mgdtestdomain1.com" || domain.Name == "mgdpartnertestdomain.com") {
//					domain.Delete();
//				}
//			}
//				
//			foreach (var partner in client.GetPartners()) {
//				if (partner.Name == "SubPartner 75") {
//					partner.Delete ();
//				}
//			}
//
//			var newTestDomain = client.CreateDomain("mgdtestdomain1.com");
//			var domains2 = client.GetDomains();
//			var partners = client.GetPartners();
//
//			var newPartner = client.CreatePartner("SubPartner 75");
//			var partners2 = client.GetPartners();
//
//			var partnerTestDomain = client.CreateDomain("mgdpartnertestdomain.com", newPartner);
//			var domains3 = client.GetDomains();
//
//			partnerTestDomain.Delete();
//			newPartner.Delete();
//			var partners3 = client.GetPartners();
//
//			// Test Wallets
//			var walletNames = client.GetWalletNames();
//			var walletName = client.CreateWalletName("mgdtestdomain1.com", "testwallet", "externalId");
//			walletName.SetCurrencyAddress ("btc", "1btcaddress");
//			walletName.Save();
//			var walletNames2 = client.GetWalletNames("mgdtestdomain1.com");
//
//			walletName.SetCurrencyAddress ("ltc", "LtcAddress1");
//			walletName.Save ();
//			var walletNames3 = client.GetWalletNames("mgdtestdomain1.com");
//
//			walletName.Delete();
//			newTestDomain.Delete();
//			var domains4 = client.GetDomains();

        }
	}
}

