using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLObjectNotFoundException : NavNCLException
{
	public NavNCLObjectNotFoundException()
		: base(Lang._00095_50050)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLObjectNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLObjectNotFoundException(ApplicationObjectId objectId)
		: base(CreateMessage(objectId))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLObjectNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLObjectNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(ApplicationObjectId objectId)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_01008, objectId.ObjectType, objectId.ObjectNumber);
	}
}
