using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLWrongSignatureOnCU1TriggerException : NavServerSessionBaseException
{
	public NavNCLWrongSignatureOnCU1TriggerException()
	{
	}

	public NavNCLWrongSignatureOnCU1TriggerException(string message)
		: base(message)
	{
	}

	public NavNCLWrongSignatureOnCU1TriggerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLWrongSignatureOnCU1TriggerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLWrongSignatureOnCU1TriggerException Create(CultureInfo culture, int triggerId, int codeunitId, string expectedSignature, Exception innerException)
	{
		return new NavNCLWrongSignatureOnCU1TriggerException(string.Format(culture, Lang.MethodSignatureHasChanged, triggerId, codeunitId, expectedSignature), innerException);
	}
}
