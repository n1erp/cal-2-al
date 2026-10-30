using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SharedNavHttpClient : TreeObject, ITreeSharedObject, ITreeObject
{
	private class NavHttpClientHandler : DelegatingHandler
	{
		public NavHttpClientHandler(HttpMessageHandler innerHandler = null)
			: base(innerHandler ?? new WebRequestHandler())
		{
		}

		public void AddClientCertificate(X509Certificate value)
		{
			GetHttpClientHandler().ClientCertificates.Add(value);
		}

		public void UseDefaultNetworkWindowsAuthentication()
		{
			GetHttpClientHandler().Credentials = CredentialCache.DefaultNetworkCredentials;
		}

		public void UseWindowsAuthentication(string userName, string password, string domain)
		{
			GetHttpClientHandler().Credentials = new NetworkCredential(userName, password, domain ?? string.Empty);
		}

		internal HttpClientHandler GetHttpClientHandler()
		{
			HttpClientHandler httpClientHandler = GetHttpClientHandler(this);
			if (httpClientHandler == null)
			{
				throw new NavNCLInvalidOperationException();
			}
			return httpClientHandler;
			static HttpClientHandler GetHttpClientHandler(DelegatingHandler handler)
			{
				HttpMessageHandler httpMessageHandler = handler.InnerHandler;
				if (httpMessageHandler is HttpClientHandler result)
				{
					return result;
				}
				if (httpMessageHandler is DelegatingHandler handler2)
				{
					return GetHttpClientHandler(handler2);
				}
				return null;
			}
		}
	}

	private HttpClient httpClient;

	private NavHttpClientHandler handler;

	private NavHttpHeaders defaultRequestHeaders;

	internal HttpClient Client => httpClient;

	internal NavHttpHeaders DefaultRequestHeaders => defaultRequestHeaders;

	internal SharedNavHttpClient()
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
	}

	internal void CreateClient(HttpMessageHandler wrappedHandler = null)
	{
		httpClient?.Dispose();
		handler?.Dispose();
		NavHttpClientHandler requestHandler = GetRequestHandler(wrappedHandler);
		if (!ServerUserSettings.Instance.ServicesCertificateValidationEnabled.Value)
		{
			requestHandler.GetHttpClientHandler().ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true;
		}
		httpClient = new HttpClient(requestHandler)
		{
			MaxResponseContentBufferSize = ServerUserSettings.Instance.NavHttpClientMaxResponseContentSize.ValueInBytes
		};
		defaultRequestHeaders = new NavHttpHeaders(httpClient.DefaultRequestHeaders);
	}

	internal void AddCertificate(X509Certificate certificate)
	{
		handler.AddClientCertificate(certificate);
	}

	internal void UseDefaultNetworkWindowsAuthentication()
	{
		handler.UseDefaultNetworkWindowsAuthentication();
	}

	internal void UseWindowsAuthentication(string userName, string password, string domain)
	{
		handler.UseWindowsAuthentication(userName, password, domain);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			httpClient?.Dispose();
			handler?.Dispose();
		}
		base.Dispose(disposing);
	}

	private NavHttpClientHandler GetRequestHandler(HttpMessageHandler wrappedHandler)
	{
		handler = new NavHttpClientHandler(wrappedHandler);
		return handler;
	}
}
