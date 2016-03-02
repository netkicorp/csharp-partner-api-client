using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Netki
{
	public class Domain : BaseObject
	{
		public string Name { get; set; }
		public string Status { get; set; }
		public bool DelegationStatus { get; set; }
		public string DelegationMessage { get; set; }
		public int WalletNameCount { get; set; }

		public DateTime NextRoll { get; set; }
		public List<string> DsRecords { get; set; }
		public List<string> Nameservers { get; set; }
		public string PublicKeySigningKey { get; set; }

        private IRequestor requestor = new Requestor();

		public Domain(string name, IRequestor requestor = null)
		{
            Name = name;
            DsRecords = new List<string>();
            Nameservers = new List<string>();

            if (requestor != null)
            {
                this.requestor = requestor;
            }
		}

		public void Delete() {
			requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format ("{0}/v1/partner/domain/{1}", apiUrl, Name),
				"DELETE",
				null
			);
		}

		public void LoadStatus() {
			string responseStr = requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format ("{0}/v1/partner/domain/{1}", apiUrl, Name),
				"GET",
				null
			);

			JObject jsonObj = JObject.Parse (responseStr);
			Status = jsonObj["status"].ToString();
            DelegationStatus = jsonObj["delegation_status"].ToObject<bool>();
			DelegationMessage = jsonObj["delegation_message"].ToString();
            WalletNameCount = jsonObj["wallet_name_count"].ToObject<int>();
		}

		public void LoadDnssecDetails() {
			string responseStr = requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format ("{0}/v1/partner/domain/dnssec/{1}", apiUrl, Name),
				"GET",
				null
			);

			JObject jsonObj = JObject.Parse (responseStr);
			if (jsonObj["public_key_signing_key"] != null) {
				PublicKeySigningKey = jsonObj ["public_key_signing_key"].ToString();
			}

			if (jsonObj ["ds_records"] != null) {
				foreach (var dsRecord in jsonObj["ds_records"].Children()) {
					DsRecords.Add (dsRecord.ToString ());
				}
			}

			if (jsonObj ["nameservers"] != null) {
				foreach (var nameserver in jsonObj["nameservers"].Children()) {
					Nameservers.Add (nameserver.ToString ());
				}
			}

			if (jsonObj ["nextroll_date"] != null) {
				NextRoll = Convert.ToDateTime (jsonObj ["nextroll_date"].ToString ());
			}
		}
	}
}

