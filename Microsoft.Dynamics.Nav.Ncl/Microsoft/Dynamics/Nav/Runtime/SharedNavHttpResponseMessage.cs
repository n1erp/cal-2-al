using System.Net.Http;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SharedNavHttpResponseMessage : TreeObject, ITreeSharedObject, ITreeObject
{
	private NavHttpContent contentProxy;

	private NavHttpHeaders headersProxy;

	private HttpResponseMessage message;

	internal HttpResponseMessage Message
	{
		get
		{
			if (message != null && message.Content != contentProxy.Content)
			{
				message.Content = contentProxy.Content;
			}
			return message;
		}
		set
		{
			if (message != value)
			{
				message?.Dispose();
				message = value;
				if (value == null)
				{
					contentProxy?.Dispose();
					contentProxy = null;
					headersProxy = null;
				}
				else
				{
					headersProxy = new NavHttpHeaders(value.Headers);
					UpdateMessageContentProxy(value);
				}
			}
		}
	}

	internal bool IsBlockedByEnvironment { get; set; }

	internal NavHttpHeaders MessageHeaders => headersProxy;

	internal NavHttpContent MessageContent => contentProxy;

	internal SharedNavHttpResponseMessage()
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
	}

	private void UpdateMessageContentProxy(HttpResponseMessage value)
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
			message?.Dispose();
		}
		base.Dispose(disposing);
	}
}
