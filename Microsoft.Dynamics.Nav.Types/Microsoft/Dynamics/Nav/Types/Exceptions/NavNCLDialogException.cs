using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDialogException : NavNCLException
{
	private const string AutomationIdToken = "AutomationId";

	public string AutomationId { get; set; }

	public NavNCLDialogException()
	{
	}

	public NavNCLDialogException(string message)
		: base(message)
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDialogException(string message, Exception innerException)
		: base(message, innerException)
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	protected NavNCLDialogException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		AutomationId = info.GetString("AutomationId");
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("AutomationId", AutomationId);
	}

	public static NavNCLDialogException Create(Guid automationId, string message, string diagnosticsMessage)
	{
		NavNCLDialogException ex = (string.IsNullOrEmpty(message) ? new NavNCLDialogException
		{
			SuppressMessage = true
		} : new NavNCLDialogException(message)
		{
			DiagnosticsMessage = diagnosticsMessage
		});
		ex.AutomationId = automationId.ToString();
		return ex;
	}

	protected override void UpdateCloneCore(NavBaseException exceptionClone)
	{
		base.UpdateCloneCore(exceptionClone);
		((NavNCLDialogException)exceptionClone).AutomationId = AutomationId;
	}
}
