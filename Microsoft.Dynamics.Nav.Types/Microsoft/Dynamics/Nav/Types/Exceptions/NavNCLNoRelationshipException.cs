using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLNoRelationshipException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNoRelationshipException()
	{
	}

	public NavNCLNoRelationshipException(string fieldCaption, string tableCaption)
		: base(CreateMessage(fieldCaption, tableCaption))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNoRelationshipException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNoRelationshipException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNCLNoRelationshipException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string fieldCaption, string tableCaption)
	{
		if (string.IsNullOrEmpty(fieldCaption))
		{
			throw new ArgumentNullException("fieldCaption");
		}
		if (string.IsNullOrEmpty(tableCaption))
		{
			throw new ArgumentNullException("tableCaption");
		}
		return string.Format(CultureInfo.CurrentCulture, Lang.NoRelationshipExists, fieldCaption, tableCaption);
	}
}
