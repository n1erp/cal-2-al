using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArrayLengthMismatchException : NavNCLException
{
	public NavNCLArrayLengthMismatchException()
		: base(Lang._00095_50084)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLArrayLengthMismatchException(string message)
		: base(message)
	{
	}

	public NavNCLArrayLengthMismatchException(int length, int destinationArrayLength)
		: base(CreateMessage(length, destinationArrayLength))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLArrayLengthMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArrayLengthMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(int length, int destinationArrayLength)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.DestinationLengthMismatch, length, destinationArrayLength);
	}
}
