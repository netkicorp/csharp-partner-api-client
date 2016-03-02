using System;
using System.Collections.Generic;
using System.Net;

using NUnit.Framework;
using Netki;
using Newtonsoft.Json.Linq;

using HttpMock;

namespace NetkiTest
{

    class RequestorTest
    {
        // HandleRegularHttpRequest Testss
        [Test]
        public void HandleRegularHTTPRequest_Get()
        {
            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return("data").OK();

            Requestor requestor = new Requestor();
            Tuple <HttpStatusCode, string> result = requestor.HandleRegularHTTPRequest("http://localhost:9191/endpoint", "GET", null);

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));
            Assert.AreEqual(HttpStatusCode.OK, result.Item1);
            Assert.AreEqual("data", result.Item2);
        }

        [Test]
        public void HandleRegularHTTPRequest_GetNoData()
        {
            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).OK();

            Requestor requestor = new Requestor();
            Tuple<HttpStatusCode, string> result = requestor.HandleRegularHTTPRequest("http://localhost:9191/endpoint", "GET", null);

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));
            Assert.AreEqual(HttpStatusCode.OK, result.Item1);
            Assert.AreEqual(null, result.Item2);
        }

        [Test]
        public void HandleRegularHTTPRequest_GetNotFound()
        {
            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return("data").NotFound();

            Requestor requestor = new Requestor();
            Tuple<HttpStatusCode, string> result = requestor.HandleRegularHTTPRequest("http://localhost:9191/endpoint", "GET", null);

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));
            Assert.AreEqual(HttpStatusCode.NotFound, result.Item1);
            Assert.AreEqual("data", result.Item2);
        }

        [Test]
        public void HandleRegularHTTPRequest_Post()
        {
            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Post("/endpoint")).Return("data").OK();

            Requestor requestor = new Requestor();
            Tuple<HttpStatusCode, string> result = requestor.HandleRegularHTTPRequest("http://localhost:9191/endpoint", "POST", "submit data");

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Post("/endpoint"));
            Assert.AreEqual("submit data", reqVerify.GetBody());
            Assert.AreEqual(HttpStatusCode.OK, result.Item1);
            Assert.AreEqual("data", result.Item2);
        }

        // ProcessResquest Tests
        [Test]
        public void ProcessRequestGetSuccessGoRight()
        {
            Dictionary<string, bool> respData = new Dictionary<string, bool>
            {
                {"success", true }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return(JObject.FromObject(respData).ToString()).OK();

            Requestor requestor = new Requestor();
            string returnData = requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "GET", null);
            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));

            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));
            JObject assertData = JObject.Parse(returnData);
            Assert.AreEqual(true, assertData["success"].ToObject<bool>());

        }

        [Test]
        public void ProcessRequestDeleteNoContent()
        {
            Dictionary<string, bool> respData = new Dictionary<string, bool>
            {
                {"success", true }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Delete("/endpoint")).Return(JObject.FromObject(respData).ToString()).WithStatus(HttpStatusCode.NoContent);

            Requestor requestor = new Requestor();
            string returnData = requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "DELETE", null);

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Delete("/endpoint"));
            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));

            Assert.AreEqual("", returnData);

        }

        [Test]
        public void ProcessRequestPostSuccessGoRight()
        {
            Dictionary<string, bool> respData = new Dictionary<string, bool>
            {
                {"success", true }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Post("/endpoint")).Return(JObject.FromObject(respData).ToString()).OK();

            Requestor requestor = new Requestor();
            string returnData = requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "POST", "post data");

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Post("/endpoint"));
            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));
            Assert.AreEqual("post data", reqVerify.GetBody());

            JObject assertData = JObject.Parse(returnData);
            Assert.AreEqual(true, assertData["success"].ToObject<bool>());

        }

        [Test]
        public void ProcessRequestGetBasicError()
        {
            Dictionary<string, object> respData = new Dictionary<string, object>
            {
                {"success", false },
                {"message", "failure message" }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return(JObject.FromObject(respData).ToString()).OK();

            Requestor requestor = new Requestor();
            try
            {
                requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "GET", null);
                Assert.IsTrue(false);
            } catch (Exception e)
            {
                Assert.AreEqual("failure message", e.Message);
            }

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));

            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));

        }

        [Test]
        public void ProcessRequestGetBasicErrorNotFoundCode()
        {
            Dictionary<string, object> respData = new Dictionary<string, object>
            {
                {"success", true },
                {"message", "failure message" }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return(JObject.FromObject(respData).ToString()).NotFound();

            Requestor requestor = new Requestor();
            try
            {
                requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "GET", null);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.AreEqual("failure message", e.Message);
            }

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));

            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));

        }

        [Test]
        public void ProcessRequestGetFailureList()
        {
            Dictionary<string, object> respData = new Dictionary<string, object>
            {
                {"success", false },
                {"message", "failure message" },
                {"failures", new object[] {
                    new Dictionary<string, string> { { "message", "fail1" } },
                    new Dictionary<string, string> { { "message", "fail2" } },
                    new Dictionary<string, string> { { "message", "fail3" } }
                } }
            };

            IHttpServer server = HttpMockRepository.At("http://localhost:9191");
            server.Stub(x => x.Get("/endpoint")).Return(JObject.FromObject(respData).ToString()).OK();

            Requestor requestor = new Requestor();
            try
            {
                requestor.ProcessRequest("api_key", "partner_id", "http://localhost:9191/endpoint", "GET", null);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.AreEqual("failure message [FAILURES: fail1, fail2, fail3]", e.Message);
            }

            IRequestVerify reqVerify = server.AssertWasCalled(x => x.Get("/endpoint"));

            reqVerify.WithHeader("Authorization", Is.EqualTo("api_key"));
            reqVerify.WithHeader("X-Partner-ID", Is.EqualTo("partner_id"));

        }
    }
}
