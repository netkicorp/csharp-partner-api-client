# Netki C# Partner Library

This is the Netki Partner library written in C#. It allows you to use the Netki Partner API to CRUD all of your partner data:

* Wallet Names
* Domains
* Partners

# Example
```cs

using Netki.Netkiclient;

namespace Example {

	public class Example {

		static void Main(string[] args) {

			string partnerId = "XXXXXXXXXXXXXXXXXXXX";
            string apiKey = "XXXXXXXXXXXXXXXXXXXXXXXXX";

            // Create NetkiClient
            var client = new NetkiClient(partnerId, apiKey, "https://api.netki.com");
            
            // Get All Available Domains
            List<Domain> domains = client.GetDomains();

            // Create a Domain
			var newTestDomain = client.CreateDomain("testdomain1.com");
			
			// Get Partners
			var partners = client.GetPartners();

            // Create Partner
			var newPartner = client.CreatePartner("My Test Partner");

            // Create a Domain Belonging to a Partner
			var partnerTestDomain = client.CreateDomain("partnertestdomain.com", newPartner);
			
			// Delete Partner Domain
			partnerTestDomain.Delete();
			
			// Delete Partner
			newPartner.Delete();

            // Get All WalletNames
			var walletNames = client.GetWalletNames();
			
			// Create a New WalletName on domain testdomain1.com with name testwallet (Full WalletName: testwallet.testdomain1.com)
			var walletName = client.CreateWalletName("testdomain1.com", "testwallet", "externalId");
			
			// Setup WalletName Currency for Bitcoin (btc) 
			walletName.SetCurrencyAddress ("btc", "1CpLXM15vjULK3ZPGUTDMUcGATGR9xGitv");
			
			// Change WalletName Name
			walletName.Name = "otherwallet";
			
	        // Change WalletName ExternalID
	        walletName.ExternalId = "newExternalId";
			
			// Save WalletName
			walletName.Save();
			
			// Get All WalletNames Belonging to testdomain1.com
			var walletNames2 = client.GetWalletNames("testdomain1.com");

            // Setup WalletName Currency for Litecoin (ltc)
			walletName.SetCurrencyAddress ("ltc", "LLfcbGQuv3HBPxzeEUsCEbTc5QQn79TWxE");
			
			// Save WalletName
			walletName.Save ();

            // Delete WalletName
			walletName.Delete();
			
			// Delete Domain
			newTestDomain.Delete();

		}

	}
}
```
