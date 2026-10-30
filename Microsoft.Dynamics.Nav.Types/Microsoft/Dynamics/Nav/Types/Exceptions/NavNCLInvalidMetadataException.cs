using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidMetadataException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidMetadataException()
	{
	}

	public NavNCLInvalidMetadataException(int objectId)
		: base(CreateMessage(objectId))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidMetadataException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNCLInvalidMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(int objectId)
	{
		return string.Format(CultureInfo.InvariantCulture, "Metadata for specified system part: {0} is not valid.", objectId);
	}
}
