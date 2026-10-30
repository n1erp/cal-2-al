using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLTestCodeUnitMethodArgumentException : NavNCLException
{
	public NavNCLTestCodeUnitMethodArgumentException()
	{
	}

	public NavNCLTestCodeUnitMethodArgumentException(string message)
		: base(message)
	{
	}

	public NavNCLTestCodeUnitMethodArgumentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTestCodeUnitMethodArgumentException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLTestCodeUnitMethodArgumentException CreateNotSupportedRecord(CultureInfo culture, int codeUnitId, string codeUnitName, int tableId, string tableName)
	{
		return new NavNCLTestCodeUnitMethodArgumentException(string.Format(culture, Lang.TestCodeUnitsDoNotSupportRecordParameter, codeUnitId, codeUnitName, tableId, tableName));
	}
}
