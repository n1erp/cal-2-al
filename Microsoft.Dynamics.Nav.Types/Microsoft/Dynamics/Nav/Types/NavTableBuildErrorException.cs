using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavTableBuildErrorException : NavBuilderException
{
	public NavTableBuildErrorException(int tableId, Exception innerException)
		: base(string.Format(CultureInfo.CurrentCulture, Lang.TableBuildError, tableId), innerException)
	{
	}

	public NavTableBuildErrorException()
	{
	}

	private NavTableBuildErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavTableBuildErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public NavTableBuildErrorException(string message)
		: base(message)
	{
	}
}
