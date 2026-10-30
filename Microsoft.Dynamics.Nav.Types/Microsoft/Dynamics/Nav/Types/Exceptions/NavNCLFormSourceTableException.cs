using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFormSourceTableException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFormSourceTableException()
	{
	}

	public NavNCLFormSourceTableException(string name, string objectId)
		: base(CreateMessage(name, objectId))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFormSourceTableException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLFormSourceTableException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNCLFormSourceTableException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string name, string objectId)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		return string.Format(CultureInfo.CurrentCulture, Lang.FormSourceTableUndefined, name, objectId);
	}
}
