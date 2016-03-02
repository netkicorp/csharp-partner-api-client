using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Netki
{

    public interface IRequestor
    {
        Tuple<HttpStatusCode, string> HandleRegularHTTPRequest(string uri, string method, string data = null);
        string ProcessRequest(string apiKey, string partnerId, string uri, string method, string data);
    }

	public class Requestor : IRequestor
	{
		public Requestor ()
		{
		}

        public Tuple<HttpStatusCode, string> HandleRegularHTTPRequest(string uri, string method, string data = null)
        {

            string responseString = null;

            WebRequest request = WebRequest.Create(uri);
            request.Method = method;
            if (data != null)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            WebResponse response;
            try
            {
                response = request.GetResponse();
            } catch (WebException e)
            {
                response = e.Response;
            }
            
            if (((HttpWebResponse)response).ContentLength > 0)
            {
                Stream responseDataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseDataStream);
                responseString = reader.ReadToEnd();
                reader.Close();
                responseDataStream.Close();
                response.Close();
            }

            return new Tuple<HttpStatusCode, string>(((HttpWebResponse)response).StatusCode, responseString);
        }

        public string ProcessRequest(string apiKey, string partnerId, string uri, string method, string data) {

			List<string> supportedMethods = new List<string>() {"GET", "POST", "PUT", "DELETE"};
			if (!supportedMethods.Contains(method)) {
				throw new Exception (string.Format ("Unsupported HTTP Method: {0}", method));
			}

			WebRequest request = WebRequest.Create (uri);
			request.Method = method;
            request.ContentType = "application/json";
			request.Headers.Set ("Authorization", apiKey);
			request.Headers.Set ("X-Partner-ID", partnerId);

			if (data != null) {
				byte[] byteArray = Encoding.UTF8.GetBytes(data);
				request.ContentLength = byteArray.Length;
				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
			}

			HttpStatusCode statusCode;
            WebResponse response;
            try {
                response = request.GetResponse();
            } catch(WebException e)
            {
                response = e.Response;
            }

			statusCode = ((HttpWebResponse)response).StatusCode;

			if (method == "DELETE" && statusCode == HttpStatusCode.NoContent) {
				return "";
			}

			Stream responseDataStream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (responseDataStream);
			string responseString = reader.ReadToEnd();
			reader.Close();
			responseDataStream.Close();
			response.Close();

			JObject retData = JObject.Parse (responseString);
			if (statusCode >= HttpStatusCode.MultipleChoices || !retData["success"].ToObject<bool>()) {

				string errorMessage = retData["message"].ToString();

				if (retData["failures"] != null) {
					List<string> failures = new List<string>();
					foreach(JObject failure in retData["failures"]) {
						failures.Add(failure["message"].ToString());
					}

					errorMessage = string.Format ("{0} [FAILURES: {1}]", retData.GetValue ("message"), String.Join (", ", failures));
				}

                throw new Exception(errorMessage);
            }

			return retData.ToString();
		}
	}
}

