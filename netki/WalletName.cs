using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Netki
{
	public class WalletName : BaseObject
	{
		// WalletName Data
		public string Id { get; set; }
		public string DomainName  { get; set; }
		public string Name { get; set; }
		public string ExternalId { get; set; }
        private IRequestor requestor = new Requestor();

        private Dictionary<string, string> wallets = new Dictionary<string, string>();

		// Empty Constructor
		public WalletName(IRequestor requestor = null)
		{
            if(requestor != null)
            {
                this.requestor = requestor;
            }
		}

		public List<string> GetUsedCurrencies() {
			return new List<string>(wallets.Keys);
		}

		public string GetWalletAddress(string currency) {
			if (wallets.ContainsKey (currency)) {
				return wallets[currency];
			}
			return null;
		}

		public void SetCurrencyAddress(string currency, string walletAddress) {
			wallets[currency] = walletAddress;
		}

		public void RemoveCurrencyAddress(string currency) {
			if(wallets.ContainsKey(currency)) {
				wallets.Remove(currency);
			}
		}

		public void Save() {
			
            // TODO: Define data in dictionary literal
			Dictionary<string, object> fullRequest = new Dictionary<string, object> ();
			Dictionary<object, object> requestObj = new Dictionary<object, object> ();

			// Create JSON Request Object
			requestObj["name"] = Name;
			requestObj["domain_name"] = DomainName;
			requestObj["external_id"] = ExternalId;
            requestObj["wallets"] = new List<object>();

			if (Id != null) {
				requestObj["id"] = Id;
			}
            foreach(var currency in wallets.Keys)
            {
                ((List<object>)requestObj["wallets"]).Add(new Dictionary<string, string> {
                    {"currency", currency },
                    {"wallet_address", wallets[currency] }
                });
            }

			fullRequest["wallet_names"] = new object[] { requestObj };

			if (Id != null) {
				string respJsonString = requestor.ProcessRequest (
					apiKey, 
					partnerId, 
					string.Format ("{0}/v1/partner/walletname", apiUrl), 
					"PUT", 
					JsonConvert.SerializeObject(fullRequest)
				);
			} else {
				string respJsonString = requestor.ProcessRequest (
					apiKey, 
					partnerId, 
					string.Format ("{0}/v1/partner/walletname", apiUrl), 
					"POST", 
					JsonConvert.SerializeObject(fullRequest)
				);

                JObject respObj = JObject.Parse(respJsonString);
                if (respObj["wallet_names"] != null)
                {
                    JArray walletNames = (JArray)respObj["wallet_names"].ToObject(typeof(JArray));
                    foreach (JObject wn in walletNames.Children())
                    {
                        if (wn["domain_name"].ToString() == DomainName && wn["name"].ToString() == Name)
                        {
                            Id = wn["id"].ToString();
                        }
                    }
                }
            }
		}

		public void Delete() {
			
			if (Id == null) {
				throw new Exception("Unable to Delete Object that Does Not Exist Remotely");
			}

			Dictionary<string, object> fullRequest = new Dictionary<string, object> ();
			Dictionary<object, object> requestObj = new Dictionary<object, object> ();

			// Create JSON Request Object
			requestObj["domain_name"] = DomainName;
			requestObj["id"] = Id;
			fullRequest["wallet_names"] = new object[] {requestObj};

			requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format("{0}/v1/partner/walletname", apiUrl),
				"DELETE",
				JsonConvert.SerializeObject(fullRequest)
			);

		}
	}
}

