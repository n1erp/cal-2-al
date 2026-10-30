using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientBuilderDisabledColumnException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientBuilderDisabledColumnException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientBuilderDisabledColumnException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientBuilderDisabledColumnException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavClientBuilderDisabledColumnException(CultureInfo culture, string columnName, string caption)
		: base(CreateMessage(culture, columnName, caption))
	{
	}

	protected NavClientBuilderDisabledColumnException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string columnName, string caption)
	{
		return string.Format(culture, "The form's metadata is not valid. The reference to the field '{0}' from the control '{1}' could not be resolved.", columnName, caption);
	}
}
