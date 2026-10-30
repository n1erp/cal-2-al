using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidPathException : NavNCLException
{
	public NavNCLInvalidPathException()
		: base(Lang._00095_50045)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidPathException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidPathException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidPathException(string message, params DiagnosticParameter[] args)
		: base(CultureInfo.CurrentCulture, message, args)
	{
	}

	protected NavNCLInvalidPathException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
