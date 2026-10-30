using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserPropertyTableNameIdentifierMustBeUniqueException : NavNCLTableConstraintBaseException
{
	public NavNCLUserPropertyTableNameIdentifierMustBeUniqueException()
	{
	}

	public NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	protected NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserPropertyTableNameIdentifierMustBeUniqueException Create(string nameIdentifier)
	{
		return new NavNCLUserPropertyTableNameIdentifierMustBeUniqueException(CultureInfo.CurrentCulture, "The specified name identifier '{0}' is already being used by another user.", DiagnosticParameter.CustomerContent(nameIdentifier));
	}
}
