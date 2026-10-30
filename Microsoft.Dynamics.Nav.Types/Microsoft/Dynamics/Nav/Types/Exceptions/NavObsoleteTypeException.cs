using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavObsoleteTypeException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavObsoleteTypeException()
	{
	}

	public NavObsoleteTypeException(string typeName)
		: base(CreateMessage(typeName))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavObsoleteTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavObsoleteTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string typeName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.ObsoleteType, typeName);
	}
}
