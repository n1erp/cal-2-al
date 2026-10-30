using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLStringLengthExceededException : NavNCLEvaluateException
{
	private const int MaxValueMessageLength = 200;

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLStringLengthExceededException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLStringLengthExceededException(string message)
		: base(message)
	{
	}

	public NavNCLStringLengthExceededException(int maxLength)
		: base(CreateMessage(maxLength))
	{
	}

	public NavNCLStringLengthExceededException(int maxLength, int length, string value)
		: this(CultureInfo.CurrentCulture, Lang._00095_50007, DiagnosticParameter.SystemMetadata(maxLength), DiagnosticParameter.SystemMetadata(length), DiagnosticParameter.CustomerContent(GetTruncatedString(value)))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLStringLengthExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNCLStringLengthExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLStringLengthExceededException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	private static string CreateMessage(int maxLength)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50006, maxLength);
	}

	private static string GetTruncatedString(string value)
	{
		if (value.Length >= 200)
		{
			return value.Substring(0, 200) + "...";
		}
		return value;
	}
}
