using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLNumberOfArgumentsExceededException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNumberOfArgumentsExceededException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNumberOfArgumentsExceededException(string message)
		: base(message)
	{
	}

	public NavNCLNumberOfArgumentsExceededException(int maxArguments)
		: base(CreateMessage(maxArguments))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNumberOfArgumentsExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLNumberOfArgumentsExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(int maxArguments)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50015, maxArguments);
	}
}
