using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableCannotBatchModifyFieldException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableCannotBatchModifyFieldException()
	{
	}

	public NavNCLUserTableCannotBatchModifyFieldException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableCannotBatchModifyFieldException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableCannotBatchModifyFieldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableCannotBatchModifyFieldException Create(string fieldName)
	{
		return new NavNCLUserTableCannotBatchModifyFieldException(string.Format(CultureInfo.CurrentCulture, Lang.FieldCanNotBeBatchModified, fieldName));
	}
}
