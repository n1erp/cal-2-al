using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavHttpClient : NavComplexValue, ITreeObjectReference, ITreeObject
{
	private IHttpClientRequestFilterService clientRequestFilterService;

	public string ALGetBaseAddress => HttpClient.BaseAddress?.ToString() ?? string.Empty;

	public NavHttpHeaders ALDefaultRequestHeaders => Target.DefaultRequestHeaders;

	public NavDuration ALTimeout
	{
		get
		{
			return Timeout;
		}
		set
		{
			Timeout = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavHttpClient;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	internal HttpClient HttpClient => Target.Client;

	internal SharedNavHttpClient Target
	{
		get
		{
			SharedNavHttpClient sharedNavHttpClient = (SharedNavHttpClient)Tree.GetReferenceTarget();
			if (sharedNavHttpClient == null)
			{
				sharedNavHttpClient = new SharedNavHttpClient();
				Tree.SetReferenceTarget(sharedNavHttpClient);
			}
			return sharedNavHttpClient;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	private NavDuration Timeout
	{
		get
		{
			return new NavDuration(HttpClient.Timeout);
		}
		set
		{
			TimeSpan timeSpan = value;
			if (timeSpan.TotalMilliseconds > ServerUserSettings.Instance.NavHttpClientMaxTimeout.Value.TotalMilliseconds)
			{
				throw new NavNclHttpClientTimeoutTooLargeException(string.Format(CultureInfo.CurrentCulture, Lang.NavNclHttpClientInvalidTimeout, timeSpan.Minutes, ServerUserSettings.Instance.NavHttpClientMaxTimeout.Value.Minutes));
			}
			HttpClient.Timeout = value;
		}
	}

	public void ALClear()
	{
		Clear();
	}

	public bool ALSetBaseAddress(DataError errorLevel, string value)
	{
		return SetBaseAddress(errorLevel, value);
	}

	public bool ALGet(DataError errorLevel, string path, ByRef<NavHttpResponseMessage> response)
	{
		return Get(errorLevel, path, response);
	}

	public bool ALDelete(DataError errorLevel, string path, ByRef<NavHttpResponseMessage> response)
	{
		return Delete(errorLevel, path, response);
	}

	public bool ALPut(DataError errorLevel, string path, NavHttpContent content, ByRef<NavHttpResponseMessage> response)
	{
		return Put(errorLevel, path, content, response);
	}

	public bool ALPost(DataError errorLevel, string path, NavHttpContent content, ByRef<NavHttpResponseMessage> response)
	{
		return Post(errorLevel, path, content, response);
	}

	public bool ALSend(DataError errorLevel, NavHttpRequestMessage request, ByRef<NavHttpResponseMessage> response)
	{
		return Send(errorLevel, request, response);
	}

	public void ALAddCertificate(DataError errorLevel, string certificateContent, string password = null)
	{
		AddCertificate(errorLevel, certificateContent, password);
	}

	public bool ALUseDefaultNetworkWindowsAuthentication(DataError errorLevel)
	{
		return UseDefaultNetworkWindowsAuthentication(errorLevel);
	}

	public bool ALUseWindowsAuthentication(DataError errorLevel, string userName, string password, string domain = null)
	{
		return UseWindowsAuthentication(errorLevel, userName, password, domain);
	}

	public void ALAssign(NavHttpClient other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (this != other && Target != other.Target)
		{
			Target = other.Target;
		}
	}

	public NavHttpClient(ITreeObject parent)
		: this(parent, null, null)
	{
	}

	public NavHttpClient(ITreeObject parent, HttpMessageHandler wrappedHandler, IHttpClientRequestFilterService clientRequestFilterService)
		: base(parent)
	{
		InitializeToDefault(wrappedHandler, clientRequestFilterService);
	}

	public override void Clear()
	{
		Target = null;
		InitializeToDefault();
	}

	public override int GetHashCode()
	{
		return HttpClient.GetHashCode();
	}

	private void InitializeToDefault(HttpMessageHandler wrappedHandler = null, IHttpClientRequestFilterService mockClientRequestFilterService = null)
	{
		Target.CreateClient(wrappedHandler);
		clientRequestFilterService = mockClientRequestFilterService ?? new HttpClientRequestFilterService();
	}

	private bool AddCertificate(DataError errorLevel, string certificateContent, string password)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpClient, () =>
		{
			byte[] rawData = Convert.FromBase64String(certificateContent);
			X509Certificate2 certificate = ((password == null) ? new X509Certificate2(rawData) : new X509Certificate2(rawData, password));
			Target.AddCertificate(certificate);
		});
	}

	private bool UseDefaultNetworkWindowsAuthentication(DataError errorLevel)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpClient, () =>
		{
			Target.UseDefaultNetworkWindowsAuthentication();
		});
	}

	private bool UseWindowsAuthentication(DataError errorLevel, string userName, string password, string domain)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpClient, () =>
		{
			Target.UseWindowsAuthentication(userName, password, domain);
		});
	}

	private bool SetBaseAddress(DataError errorLevel, string value)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			HttpClient.BaseAddress = new Uri(value);
		});
	}

	private bool Get(DataError errorLevel, string path, ByRef<NavHttpResponseMessage> response)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
			return Send(errorLevel, requestMessage, response);
		});
	}

	private bool Delete(DataError errorLevel, string path, ByRef<NavHttpResponseMessage> response)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, path);
			return Send(errorLevel, requestMessage, response);
		});
	}

	private bool Put(DataError errorLevel, string path, NavHttpContent content, ByRef<NavHttpResponseMessage> response)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, path)
			{
				Content = content.Content
			};
			return Send(errorLevel, requestMessage, response);
		});
	}

	private bool Post(DataError errorLevel, string path, NavHttpContent content, ByRef<NavHttpResponseMessage> response)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, path)
			{
				Content = content.Content
			};
			return Send(errorLevel, requestMessage, response);
		});
	}

	private bool Send(DataError errorLevel, NavHttpRequestMessage request, ByRef<NavHttpResponseMessage> response)
	{
		return Send(errorLevel, request.RequestMessage, response);
	}

	private bool Send(DataError errorLevel, HttpRequestMessage requestMessage, ByRef<NavHttpResponseMessage> response)
	{
		if (ApplyRequestBlocking(errorLevel, requestMessage, response))
		{
			return false;
		}
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpClient, () =>
		{
			NavThread.RunExternalAction(() =>
			{
				HttpResponseMessage response2 = HttpClient.SendAsync(requestMessage).WaitThrowingOriginalException();
				response.Value.ALAssign(new NavHttpResponseMessage(this, response2));
			});
		});
	}

	private bool ApplyRequestBlocking(DataError errorLevel, HttpRequestMessage requestMessage, ByRef<NavHttpResponseMessage> response)
	{
		if (!clientRequestFilterService.IsRequestFilteringEnabled)
		{
			return false;
		}
		if (clientRequestFilterService.ApplyRequestFiltering(NavCurrentThread.Session.CurrentMethodScope))
		{
			InitializeResponseMessageWithRequestBlockedStatus(requestMessage, response);
			if (errorLevel == DataError.TrapError)
			{
				return true;
			}
			throw new NavNclHttpRequestException(GetRequestBlockedErrorMessage());
		}
		return false;
	}

	private void InitializeResponseMessageWithRequestBlockedStatus(HttpRequestMessage requestMessage, ByRef<NavHttpResponseMessage> response)
	{
		HttpResponseMessage response2 = new HttpResponseMessage
		{
			StatusCode = HttpStatusCode.BadRequest,
			RequestMessage = requestMessage,
			ReasonPhrase = GetRequestBlockedErrorMessage()
		};
		response.Value.ALAssign(new NavHttpResponseMessage(this, response2, blockedByEnvironment: true));
	}

	private static string GetRequestBlockedErrorMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.HttpClientRequestWasBlockedByTheRuntime);
	}
}
