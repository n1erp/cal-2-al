using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLObjectHasAlreadyBeenRunException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLObjectHasAlreadyBeenRunException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLObjectHasAlreadyBeenRunException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLObjectHasAlreadyBeenRunException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLObjectHasAlreadyBeenRunException(CultureInfo culture, ApplicationObjectId applicationId)
		: base(CreateMessage(culture, applicationId))
	{
	}

	protected NavNCLObjectHasAlreadyBeenRunException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, ApplicationObjectId applicationId)
	{
		return string.Format(culture, Lang.ObjectHasAlreadyBeenRun, applicationId.ObjectType.ToString());
	}
}
