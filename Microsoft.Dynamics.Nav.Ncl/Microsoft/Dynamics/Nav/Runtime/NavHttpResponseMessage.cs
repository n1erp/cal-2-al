using System;
using System.Net.Http;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavHttpResponseMessage : NavComplexValue, ITreeObjectReference, ITreeObject
{
	public NavHttpContent ALContent => Target.MessageContent;

	public NavHttpHeaders ALHeaders => Target.MessageHeaders;

	public bool ALIsSuccessStatusCode => IsSuccessStatusCode;

	public bool ALIsBlockedByEnvironment => IsBlockedByEnvironment;

	public string ALReasonPhrase => ReasonPhrase;

	public NavInteger ALHttpStatusCode => HttpStatusCode;

	private SharedNavHttpResponseMessage Target
	{
		get
		{
			SharedNavHttpResponseMessage sharedNavHttpResponseMessage = (SharedNavHttpResponseMessage)Tree.GetReferenceTarget();
			if (sharedNavHttpResponseMessage == null)
			{
				sharedNavHttpResponseMessage = new SharedNavHttpResponseMessage();
				Tree.SetReferenceTarget(sharedNavHttpResponseMessage);
			}
			return sharedNavHttpResponseMessage;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	internal HttpResponseMessage ResponseMessage => Target.Message;

	public override NavNclType NclType => NavNclType.NavHttpResponseMessage;

	private bool IsSuccessStatusCode => ResponseMessage.IsSuccessStatusCode;

	private bool IsBlockedByEnvironment => Target.IsBlockedByEnvironment;

	private string ReasonPhrase => ResponseMessage.ReasonPhrase;

	private NavInteger HttpStatusCode => NavInteger.Create((int)ResponseMessage.StatusCode);

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public void ALClear()
	{
		Clear();
	}

	public void ALAssign(NavHttpResponseMessage other)
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

	public NavHttpResponseMessage(ITreeObject parent)
		: this(parent, new HttpResponseMessage())
	{
	}

	internal NavHttpResponseMessage(ITreeObject parent, HttpResponseMessage response, bool blockedByEnvironment = false)
		: base(parent)
	{
		Initialize(response, blockedByEnvironment);
	}

	public override void Clear()
	{
		Target = null;
		Initialize(new HttpResponseMessage(), blockedByEnvironment: false);
	}

	public override int GetHashCode()
	{
		return ResponseMessage.GetHashCode();
	}

	private void Initialize(HttpResponseMessage message, bool blockedByEnvironment)
	{
		Target.Message = message;
		Target.IsBlockedByEnvironment = blockedByEnvironment;
	}
}
