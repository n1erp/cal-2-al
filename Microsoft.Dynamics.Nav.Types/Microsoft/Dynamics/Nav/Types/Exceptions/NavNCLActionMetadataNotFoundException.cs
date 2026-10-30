using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLActionMetadataNotFoundException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLActionMetadataNotFoundException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLActionMetadataNotFoundException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLActionMetadataNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLActionMetadataNotFoundException(CultureInfo culture, int pageId, int actionId)
		: base(CreateMessage(culture, pageId, actionId), null)
	{
	}

	public NavNCLActionMetadataNotFoundException(CultureInfo culture, int pageId, string actionName)
		: base(CreateMessage(culture, pageId, actionName), null)
	{
	}

	protected NavNCLActionMetadataNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, int pageId, int actionId)
	{
		return string.Format(culture, Lang.ActionMetadataNotFound, pageId, "ID", actionId);
	}

	private static string CreateMessage(CultureInfo culture, int pageId, string actionName)
	{
		return string.Format(culture, Lang.ActionMetadataNotFound, pageId, "name", actionName);
	}
}
