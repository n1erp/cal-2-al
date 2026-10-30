using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableUserWindowsSidMustBeUniqueException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableUserWindowsSidMustBeUniqueException()
	{
	}

	public NavNCLUserTableUserWindowsSidMustBeUniqueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableUserWindowsSidMustBeUniqueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableUserWindowsSidMustBeUniqueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableUserWindowsSidMustBeUniqueException Create()
	{
		return new NavNCLUserTableUserWindowsSidMustBeUniqueException(string.Format(CultureInfo.CurrentCulture, Lang.UserWindowsSidMustBeUnique, ProductNames.ProductNameShort));
	}
}
