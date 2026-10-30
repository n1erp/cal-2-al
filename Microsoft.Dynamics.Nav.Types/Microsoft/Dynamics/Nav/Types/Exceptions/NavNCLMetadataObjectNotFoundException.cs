using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMetadataObjectNotFoundException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataObjectNotFoundException()
	{
	}

	public NavNCLMetadataObjectNotFoundException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataObjectNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMetadataObjectNotFoundException(CultureInfo culture, ObjectType objectType, int objectId, Exception innerException)
		: base(CreateMessage(culture, objectType, objectId), innerException)
	{
	}

	protected NavNCLMetadataObjectNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, ObjectType objectType, int objectId)
	{
		return string.Format(culture, Lang._00095_01008, objectType, objectId);
	}
}
