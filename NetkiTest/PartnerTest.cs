using NUnit.Framework;
using Netki;
using Moq;

namespace NetkiTest
{

    [TestFixture]
    class PartnerTest
    {

        [Test]
        public void PartnerCreate()
        {
            Partner partner = new Partner("id", "name");
            Assert.AreEqual("id", partner.Id);
            Assert.AreEqual("name", partner.Name);
        }

        [Test]
        public void PartnerDelete()
        {
            // Setup Mocks
            Mock<IRequestor> mockRequestor = new Mock<IRequestor>();
            mockRequestor.Setup(m => m.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null));

            // Setup Partner
            Partner partner = new Partner(mockRequestor.Object);
            partner.Id = "id";
            partner.Name = "name";
            partner.SetApiOpts("https://server", "api_key", "partner_id");

            partner.Delete();

            // Validate Call
            mockRequestor.Verify(m => m.ProcessRequest("api_key", "partner_id", "https://server/v1/admin/partner/name", "DELETE", null));

        }
    }
}
