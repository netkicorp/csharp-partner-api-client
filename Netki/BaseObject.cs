namespace Netki
{
	public class BaseObject
	{
		protected string apiKey;
		protected string apiUrl;
		protected string partnerId;

		public BaseObject ()
		{
		}

		public void SetApiOpts(string apiUrl, string apiKey, string partnerId) {
			this.apiKey = apiKey;
			this.apiUrl = apiUrl;
			this.partnerId = partnerId;
		}

        public string GetApiKey() { return apiKey; }
        public string GetApiUrl() { return apiUrl; }
        public string GetPartnerId() { return partnerId; }

	}
}

