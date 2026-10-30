using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMetadataVersionMismatchException : NavNCLMetadataException
{
	public NavNCLMetadataVersionMismatchException()
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	public NavNCLMetadataVersionMismatchException(string message)
		: base(message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	public NavNCLMetadataVersionMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	public NavNCLMetadataVersionMismatchException(CultureInfo culture, ObjectType objectType, int objectId)
		: base(string.Format(culture, Lang._00095_50175, objectType, objectId, ProductNames.ProductNameClassicClient, ProductNames.ServerServiceDisplayName))
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	protected NavNCLMetadataVersionMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}
}
