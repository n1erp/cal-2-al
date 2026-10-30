using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavClassicClientCompatibilityException : NavClientBaseException
{
	private NavClassicClientCompatibilityException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClassicClientCompatibilityException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClassicClientCompatibilityException(string message)
		: base(message)
	{
	}

	public NavClassicClientCompatibilityException(FileVersionInfo clientVersion, FileVersionInfo rtcVersion)
		: base(CreateMessage(clientVersion, rtcVersion))
	{
	}

	private static string CreateMessage(FileVersionInfo clientVersion, FileVersionInfo rtcVersion)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.IncompatibleClassicClientVersion, ProductNames.ProductNameClassicClient, clientVersion.ProductVersion, rtcVersion.ProductVersion);
	}

	public NavClassicClientCompatibilityException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
