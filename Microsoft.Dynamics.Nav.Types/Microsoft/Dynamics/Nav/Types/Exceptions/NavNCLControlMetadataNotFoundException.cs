using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLControlMetadataNotFoundException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLControlMetadataNotFoundException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLControlMetadataNotFoundException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLControlMetadataNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLControlMetadataNotFoundException(CultureInfo culture, int pageId, int controlId)
		: base(CreateMessage(culture, pageId, controlId), null)
	{
	}

	public NavNCLControlMetadataNotFoundException(CultureInfo culture, int pageId, string controlName)
		: base(CreateNameMessage(culture, pageId, controlName), null)
	{
	}

	protected NavNCLControlMetadataNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, int pageId, int controlId)
	{
		return string.Format(culture, Lang.ControlMetadataNotFound, pageId, "ID", controlId);
	}

	private static string CreateNameMessage(CultureInfo culture, int pageId, string controlName)
	{
		return string.Format(culture, Lang.ControlMetadataNotFound, pageId, "name", controlName);
	}
}
