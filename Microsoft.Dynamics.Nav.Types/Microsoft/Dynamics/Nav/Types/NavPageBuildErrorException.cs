using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavPageBuildErrorException : NavBuilderException
{
	public NavPageBuildErrorException(int pageId, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.PageBuildError, pageId) + ((innerException != null) ? (Environment.NewLine + innerException.Message) : string.Empty), innerException)
	{
		if (innerException != null)
		{
			DiagnosticsMessage = string.Format(CultureInfo.CurrentCulture, Lang.PageBuildError, pageId) + Environment.NewLine + ((innerException is NavBaseException ex) ? ex.MessageWithoutPrivateInformation : "<redacted>");
		}
	}

	public NavPageBuildErrorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavPageBuildErrorException()
	{
	}

	private NavPageBuildErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavPageBuildErrorException(string message)
		: base(message)
	{
	}
}
