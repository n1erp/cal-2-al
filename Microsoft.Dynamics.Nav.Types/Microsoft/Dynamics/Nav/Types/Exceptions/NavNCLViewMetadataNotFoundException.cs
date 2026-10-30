using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLViewMetadataNotFoundException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLViewMetadataNotFoundException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLViewMetadataNotFoundException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLViewMetadataNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLViewMetadataNotFoundException(CultureInfo culture, int pageId, string viewName)
		: base(CreateMessage(culture, pageId, viewName), null)
	{
	}

	protected NavNCLViewMetadataNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, int pageId, string viewName)
	{
		return string.Format(culture, Lang.ViewMetadataNotFound, pageId, viewName);
	}
}
