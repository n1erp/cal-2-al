using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArgumentOutOfRangeException : NavNCLException
{
	public NavNCLArgumentOutOfRangeException()
	{
	}

	public NavNCLArgumentOutOfRangeException(string message)
		: base(message)
	{
	}

	public NavNCLArgumentOutOfRangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArgumentOutOfRangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLArgumentOutOfRangeException CreateSourceArrayIndexMismatch(CultureInfo culture, int index, int maxIndex)
	{
		return new NavNCLArgumentOutOfRangeException(string.Format(culture, Lang.SourceIndexMismatch, index, maxIndex));
	}

	public static NavNCLArgumentOutOfRangeException CreateDestinationArrayIndexMismatch(CultureInfo culture, int index, int length, int maxIndex)
	{
		return new NavNCLArgumentOutOfRangeException(string.Format(culture, Lang.DestinationIndexMismatch, index, length, maxIndex));
	}

	public static NavNCLArgumentOutOfRangeException CreateForFile(CultureInfo culture, string message)
	{
		return new NavNCLArgumentOutOfRangeException(string.Format(culture, Lang._00095_50170, message));
	}
}
