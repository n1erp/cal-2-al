using System;
using System.Net.Http;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavHttpRequestMessage : NavComplexValue, ITreeObjectReference, ITreeObject
{
	public NavHttpContent ALContent
	{
		get
		{
			return Content;
		}
		set
		{
			Content = value;
		}
	}

	public string ALMethod
	{
		get
		{
			return Method;
		}
		set
		{
			Method = value;
		}
	}

	public string ALGetRequestUri => GetRequestUri;

	private SharedNavHttpRequestMessage Target
	{
		get
		{
			SharedNavHttpRequestMessage sharedNavHttpRequestMessage = (SharedNavHttpRequestMessage)Tree.GetReferenceTarget();
			if (sharedNavHttpRequestMessage == null)
			{
				sharedNavHttpRequestMessage = new SharedNavHttpRequestMessage();
				Tree.SetReferenceTarget(sharedNavHttpRequestMessage);
			}
			return sharedNavHttpRequestMessage;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	internal HttpRequestMessage RequestMessage => Target.Message;

	public override NavNclType NclType => NavNclType.NavHttpRequestMessage;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	private NavHttpContent Content
	{
		get
		{
			return Target.MessageContent;
		}
		set
		{
			Target.MessageContent = value;
		}
	}

	private string Method
	{
		get
		{
			return RequestMessage.Method.Method;
		}
		set
		{
			RequestMessage.Method = new HttpMethod(value);
		}
	}

	private string GetRequestUri => RequestMessage.RequestUri?.AbsoluteUri ?? string.Empty;

	public void ALClear()
	{
		Clear();
	}

	public bool ALSetRequestUri(DataError errorLevel, string value)
	{
		return SetRequestUri(errorLevel, value);
	}

	public bool ALGetHeaders(DataError errorLevel, ByRef<NavHttpHeaders> headersReference)
	{
		return GetHeaders(errorLevel, headersReference);
	}

	public void ALAssign(NavHttpRequestMessage other)
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

	public NavHttpRequestMessage(ITreeObject parent)
		: base(parent)
	{
		Target.Message = new HttpRequestMessage();
	}

	public override void Clear()
	{
		Target = null;
		Target.Message = new HttpRequestMessage();
	}

	public override int GetHashCode()
	{
		return RequestMessage?.GetHashCode() ?? 0;
	}

	private bool SetRequestUri(DataError errorLevel, string value)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpUri, () =>
		{
			RequestMessage.RequestUri = new Uri(value);
		});
	}

	private bool GetHeaders(DataError errorLevel, ByRef<NavHttpHeaders> headersReference)
	{
		return TrappableHttpOperationExecutor.Execute(errorLevel, TrappableHttpOperationExecutor.ExecutionOrigin.HttpContent, () =>
		{
			headersReference.Value = Target.MessageHeaders;
		});
	}
}
