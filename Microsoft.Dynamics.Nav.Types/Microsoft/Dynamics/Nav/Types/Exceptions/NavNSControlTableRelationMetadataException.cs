using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSControlTableRelationMetadataException : NavException
{
	public NavNSControlTableRelationMetadataException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSControlTableRelationMetadataException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSControlTableRelationMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNSControlTableRelationMetadataException(CultureInfo culture, string fieldName, string tableName)
		: base(CreateMessage(culture, fieldName, tableName), null)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNSControlTableRelationMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string fieldName, string tableName)
	{
		return string.Format(culture, Lang.FieldMustBelongToPrimaryKey, fieldName, tableName);
	}
}
