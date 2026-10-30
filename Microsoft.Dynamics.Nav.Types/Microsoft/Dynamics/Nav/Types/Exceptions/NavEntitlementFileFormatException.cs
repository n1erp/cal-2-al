using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavEntitlementFileFormatException : NavException
{
	[Obsolete("Do not use this constructor. Use a factory method instead")]
	public NavEntitlementFileFormatException()
	{
	}

	public NavEntitlementFileFormatException(string message)
		: base(message)
	{
	}

	public NavEntitlementFileFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Do not use this constructor. Use a factory method instead")]
	protected NavEntitlementFileFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavEntitlementFileFormatException CreateForWrongHeader(string expectedHeader, string actualHeader)
	{
		return new NavEntitlementFileFormatException(string.Format(CultureInfo.CurrentCulture, Lang.EntitlementFileWrongHeader, actualHeader, expectedHeader));
	}

	public static NavEntitlementFileFormatException CreateForWrongLine(string line)
	{
		return new NavEntitlementFileFormatException(string.Format(CultureInfo.CurrentCulture, Lang.EntitlementFileWronglyFormattedLine, line));
	}

	public static NavEntitlementFileFormatException CreateForWrongLine(string line, Exception innerException)
	{
		return new NavEntitlementFileFormatException(string.Format(CultureInfo.CurrentCulture, Lang.EntitlementFileWronglyFormattedLine, line), innerException);
	}
}
