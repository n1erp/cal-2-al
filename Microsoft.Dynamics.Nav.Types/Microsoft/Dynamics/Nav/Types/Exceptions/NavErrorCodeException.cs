using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavErrorCodeException : NavException
{
	private int errorCode;

	public int ErrorCode
	{
		get
		{
			return errorCode;
		}
		protected set
		{
			errorCode = value;
		}
	}

	public int ErrorNumber => GetErrorNumber(errorCode);

	public ModuleId ModuleNumber => (ModuleId)GetModuleNumber(errorCode);

	protected NavErrorCodeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		errorCode = info.GetInt32("ErrorCode");
	}

	protected NavErrorCodeException(string message)
		: base(message)
	{
	}

	protected NavErrorCodeException(int errorCode, string message)
		: base(message)
	{
		this.errorCode = errorCode;
	}

	protected NavErrorCodeException(int errorCode, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
		this.errorCode = errorCode;
	}

	protected NavErrorCodeException(int errorCode, Exception inner, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(inner, culture, message, args)
	{
		this.errorCode = errorCode;
	}

	protected NavErrorCodeException(int errorCode, string message, Exception innerException)
		: base(message, innerException)
	{
		this.errorCode = errorCode;
	}

	protected NavErrorCodeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavErrorCodeException(string message, string diagnosticsMessage, Exception innerException)
		: base(message, diagnosticsMessage, innerException)
	{
	}

	protected NavErrorCodeException()
	{
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("ErrorCode", errorCode);
		base.GetObjectData(info, context);
	}

	public static int CalcErrorCode(int errorNumber, int moduleNumber)
	{
		return (moduleNumber << 16) + errorNumber;
	}

	private static int GetErrorNumber(int errorCode)
	{
		return errorCode & 0xFFFF;
	}

	private static int GetModuleNumber(int errorCode)
	{
		return (byte)(errorCode >> 16);
	}

	protected override void UpdateCloneCore(NavBaseException exceptionClone)
	{
		base.UpdateCloneCore(exceptionClone);
		(exceptionClone as NavErrorCodeException).errorCode = ErrorCode;
	}
}
