using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLControlException : NavBaseException
{
	private const string StatementName = "Statement";

	private NavControlStatement ctrlStatement;

	public NavControlStatement ControlStatement
	{
		get
		{
			return ctrlStatement;
		}
		protected set
		{
			ctrlStatement = value;
		}
	}

	public NavNCLControlException()
	{
		ControlStatement = NavControlStatement.None;
	}

	public NavNCLControlException(string message)
		: base(message)
	{
		ControlStatement = NavControlStatement.None;
	}

	public NavNCLControlException(NavControlStatement controlStatement)
		: base(Lang.UnsupportedMethodsInTriggers)
	{
		ControlStatement = controlStatement;
	}

	public NavNCLControlException(string message, Exception innerException)
		: base(message, innerException)
	{
		ControlStatement = NavControlStatement.None;
	}

	protected NavNCLControlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		ControlStatement = (NavControlStatement)info.GetInt32("Statement");
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Statement", ControlStatement);
		base.GetObjectData(info, context);
	}

	protected override void UpdateCloneCore(NavBaseException exceptionClone)
	{
		base.UpdateCloneCore(exceptionClone);
		(exceptionClone as NavNCLControlException).ControlStatement = ctrlStatement;
	}
}
