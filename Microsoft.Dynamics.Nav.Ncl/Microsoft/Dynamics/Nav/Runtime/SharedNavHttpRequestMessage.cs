using System.Net.Http;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SharedNavHttpRequestMessage : TreeObject, ITreeSharedObject, ITreeObject
{
	private HttpRequestMessage requestMessage;

	private NavHttpContent contentProxy;

	private NavHttpHeaders headersProxy;

	internal HttpRequestMessage Message
	{
		get
		{
			if (requestMessage != null && requestMessage.Content != contentProxy.Content)
			{
				requestMessage.Content = contentProxy.Content;
			}
			return requestMessage;
		}
		set
		{
			if (requestMessage != value)
			{
				requestMessage?.Dispose();
				requestMessage = value;
				if (value == null)
				{
					contentProxy?.Dispose();
					contentProxy = null;
					headersProxy = null;
				}
				else
				{
					headersProxy = new NavHttpHeaders(value.Headers);
					UpdateMessageContentWrapper(value);
				}
			}
		}
	}

	internal NavHttpHeaders MessageHeaders => headersProxy;

	internal NavHttpContent MessageContent
	{
		get
		{
			return contentProxy;
		}
		set
		{
			contentProxy.ALAssign(value);
		}
	}

	internal SharedNavHttpRequestMessage()
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
	}

	private void UpdateMessageContentWrapper(HttpRequestMessage value)
	{
		NavHttpContent other = ((value.Content != null) ? new NavHttpContent(this, value.Content) : new NavHttpContent((ITreeObject)this, (HttpContent)null));
		if (contentProxy == null)
		{
			contentProxy = other;
		}
		else
		{
			contentProxy.ALAssign(other);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			contentProxy?.Dispose();
			requestMessage?.Dispose();
		}
		base.Dispose(disposing);
	}
}
