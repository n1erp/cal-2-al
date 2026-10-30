using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavNCLTableConstraintBaseException : NavNCLException
{
	protected NavNCLTableConstraintBaseException()
	{
	}

	protected NavNCLTableConstraintBaseException(string message)
		: base(message)
	{
	}

	protected NavNCLTableConstraintBaseException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNCLTableConstraintBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTableConstraintBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
