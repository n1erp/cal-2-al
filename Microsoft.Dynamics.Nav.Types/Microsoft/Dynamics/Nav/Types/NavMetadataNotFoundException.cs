using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavMetadataNotFoundException : NavMetadataException
{
	public NavMetadataNotFoundException()
	{
	}

	public NavMetadataNotFoundException(ObjectType type, int id, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.MetadataNotFound, type, id), innerException)
	{
	}

	public NavMetadataNotFoundException(ObjectType type, int id)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.MetadataNotFound, type, id))
	{
	}

	private NavMetadataNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavMetadataNotFoundException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public NavMetadataNotFoundException(string message)
		: base(message)
	{
	}
}
