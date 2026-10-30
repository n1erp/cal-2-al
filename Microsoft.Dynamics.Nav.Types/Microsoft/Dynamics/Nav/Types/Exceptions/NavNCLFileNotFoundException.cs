using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLFileNotFoundException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileNotFoundException()
	{
	}

	public NavNCLFileNotFoundException(string fileName)
		: base(CultureInfo.CurrentCulture, Lang._00095_50047, DiagnosticParameter.Euii(fileName ?? string.Empty))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFileNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLFileNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
