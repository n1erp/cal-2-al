using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Dynamics.Nav.Client;

[Serializable]
[CLSCompliant(false)]
public sealed class NavClientClosingException : Exception
{
	public NavClientExecutionResult Reaction { get; set; }

	public Exception SideException { get; set; }

	public bool Fatal { get; set; }

	public bool ShowError
	{
		get
		{
			if (Reaction != null)
			{
				return Reaction.ShowError;
			}
			return false;
		}
	}

	public NavClientClosingException()
	{
	}

	public NavClientClosingException(string message)
		: base(message)
	{
	}

	public NavClientClosingException(string message, Exception innerException)
		: base(message, innerException)
	{
		Reaction = new NavClientExecutionResult(innerException, 0, showError: false);
		if (InnerException is NavClientClosingException ex)
		{
			Fatal = ex.Fatal;
			SideException = ex.SideException;
			Reaction = ex.Reaction;
		}
	}

	public NavClientClosingException(string message, Exception innerException, NavClientExecutionResult reaction)
		: this(message, innerException)
	{
		Reaction = reaction;
	}

	private NavClientClosingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		bool boolean = info.GetBoolean("Exception reaction show error.");
		int @int = info.GetInt32("Exception reaction return value.");
		Reaction = new NavClientExecutionResult(InnerException, @int, boolean);
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		int value = 0;
		bool value2 = false;
		if (Reaction != null)
		{
			value = Reaction.ReturnValue;
			value2 = Reaction.ShowError;
		}
		info.AddValue("Exception reaction return value.", value);
		info.AddValue("Exception reaction show error.", value2);
	}
}
