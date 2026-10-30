using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavOnClosePageException : NavBaseException
{
	private string additionalMessage;

	private const string AdditionalMessageString = "AdditionalMessage";

	public string AdditionalMessage => additionalMessage;

	private NavOnClosePageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		additionalMessage = info.GetString("AdditionalMessage");
	}

	public NavOnClosePageException()
	{
	}

	public NavOnClosePageException(string message)
		: base(message)
	{
	}

	public NavOnClosePageException(string message, Exception innerException)
		: base(message, innerException)
	{
		if (innerException == null)
		{
			throw new ArgumentNullException("innerException");
		}
		if (innerException is NavOnClosePageException ex)
		{
			additionalMessage = ex.additionalMessage;
		}
		else
		{
			additionalMessage = innerException.Message;
		}
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("AdditionalMessage", additionalMessage);
		base.GetObjectData(info, context);
	}
}
