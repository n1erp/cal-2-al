using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSControlTableRelationMetadataMismatchException : NavException
{
	public NavNSControlTableRelationMetadataMismatchException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSControlTableRelationMetadataMismatchException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSControlTableRelationMetadataMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNSControlTableRelationMetadataMismatchException(CultureInfo culture, string controlType, string relatedFieldType, string fieldName, string tableName)
		: base(CreateMessage(culture, controlType, relatedFieldType, fieldName, tableName), null)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNSControlTableRelationMetadataMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string controlType, string relatedFieldType, string fieldName, string tableName)
	{
		return string.Format(culture, Lang.TableRelationMetadataMismatch, controlType, relatedFieldType, fieldName, tableName);
	}
}
