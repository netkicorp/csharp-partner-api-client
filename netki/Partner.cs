namespace Netki
{
    public class Partner : BaseObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        private IRequestor requestor { get; set; }

		public Partner ()
		{
		}

        public Partner(IRequestor requestor)
        {
            this.requestor = requestor;
        }

		public Partner(string id, string name) {
			Id = id;
			Name = name;
			requestor = new Requestor();
		}

		public void Delete() {
			requestor.ProcessRequest (
				apiKey,
				partnerId,
				string.Format ("{0}/v1/admin/partner/{1}", apiUrl, Name),
				"DELETE",
				null
			);
		}
	}
}

