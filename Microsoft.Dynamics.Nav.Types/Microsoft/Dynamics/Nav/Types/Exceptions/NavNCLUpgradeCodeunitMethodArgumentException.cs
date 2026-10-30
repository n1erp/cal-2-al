using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUpgradeCodeunitMethodArgumentException : NavNCLException
{
	public NavNCLUpgradeCodeunitMethodArgumentException()
	{
	}

	public NavNCLUpgradeCodeunitMethodArgumentException(string message)
		: base(message)
	{
	}

	public NavNCLUpgradeCodeunitMethodArgumentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUpgradeCodeunitMethodArgumentException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUpgradeCodeunitMethodArgumentException CreateNotSupportedRecord(CultureInfo culture, int codeunitId, string codeunitName, int tableId, string tableName)
	{
		return new NavNCLUpgradeCodeunitMethodArgumentException(string.Format(culture, Lang.UpgradeCodeunitsDoNotSupportRecordParameter, codeunitId, codeunitName, tableId, tableName));
	}
}
