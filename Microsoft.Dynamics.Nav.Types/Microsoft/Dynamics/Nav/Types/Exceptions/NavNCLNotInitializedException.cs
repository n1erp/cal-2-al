using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLNotInitializedException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotInitializedException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotInitializedException(string message)
		: base(message)
	{
	}

	public NavNCLNotInitializedException(Type type)
		: base(CreateMessage(type))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotInitializedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLNotInitializedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(Type type)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50052, type);
	}
}
