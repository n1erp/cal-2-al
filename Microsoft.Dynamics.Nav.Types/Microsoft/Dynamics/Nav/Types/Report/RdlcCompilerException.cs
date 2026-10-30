using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Report;

[Serializable]
[CLSCompliant(false)]
public class RdlcCompilerException : NavErrorCodeException
{
	public string UnmanagedMessage { get; private set; }

	public RdlcCompilerException(int errorCode, string message)
		: base(errorCode, message)
	{
		UnmanagedMessage = message;
	}

	public static RdlcCompilerException Create(int errorCode, Exception exception = null, string parameter = null)
	{
		string message = null;
		switch (errorCode)
		{
		case 707:
			AddExceptionMessage(exception, Lang.RdlcCompilerError707, ref parameter, ref message);
			break;
		case 724:
			AddExceptionMessage(exception, Lang.RdlcCompilerError724, ref parameter, ref message);
			break;
		case 725:
			message = AddParameterMessage(Lang.RdlcCompilerError725, parameter);
			break;
		case 726:
			message = AddParameterMessage(Lang.RdlcCompilerError726, parameter);
			break;
		case 727:
			message = AddParameterMessage(Lang.RdlcCompilerError727, parameter);
			break;
		case 728:
			message = AddParameterMessage(Lang.RdlcCompilerError728, parameter);
			break;
		case 734:
			message = AddParameterMessage(Lang.RdlcCompilerError734, parameter);
			break;
		case 743:
			message = AddParameterMessage(Lang.RdlcCompilerError743, parameter);
			break;
		}
		return new RdlcCompilerException(errorCode, message, exception)
		{
			UnmanagedMessage = parameter
		};
	}

	private static void AddExceptionMessage(Exception exception, string formatString, ref string parameter, ref string message)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		parameter = exception.Message;
		message = string.Format(CultureInfo.CurrentCulture, formatString, exception.Message);
	}

	private static string AddParameterMessage(string formatString, string parameter)
	{
		if (parameter == null)
		{
			throw new ArgumentNullException("parameter");
		}
		return string.Format(CultureInfo.CurrentCulture, formatString, parameter);
	}

	public RdlcCompilerException(Exception innerException)
		: base(innerException.Message, innerException)
	{
	}

	public RdlcCompilerException(int errorCode, Exception innerException)
		: base(errorCode, innerException.Message, innerException)
	{
	}

	private RdlcCompilerException(int errorCode, string message, Exception innerException)
		: base(errorCode, message, innerException)
	{
	}

	public RdlcCompilerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public RdlcCompilerException(string message)
		: base(message)
	{
	}

	public RdlcCompilerException()
	{
	}

	protected RdlcCompilerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("ErrorMessage", UnmanagedMessage);
		base.GetObjectData(info, context);
	}
}
