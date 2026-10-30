using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideException : NavErrorCodeException
{
	private const string RemappableFieldName = "remappable";

	private readonly bool remappable = true;

	public bool Remappable
	{
		[DebuggerStepThrough]
		get
		{
			return remappable;
		}
	}

	public NavCSideException()
	{
	}

	public NavCSideException(int errorCode)
		: this(errorCode, string.Format(CultureInfo.CurrentCulture, "Error code: {0}", errorCode))
	{
	}

	public NavCSideException(string message)
		: base(message)
	{
	}

	public NavCSideException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavCSideException(int errorCode, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(errorCode, culture, message, args)
	{
	}

	public NavCSideException(int errorCode, string message, Exception innerException)
		: this(errorCode, message, innerException, remappable: false)
	{
	}

	public NavCSideException(int errorCode, string message, Exception innerException, bool remappable)
		: base(errorCode, message, innerException)
	{
		this.remappable = remappable;
	}

	public NavCSideException(int errorCode, Exception innerException, bool remappable, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(errorCode, innerException, culture, message, args)
	{
		this.remappable = remappable;
	}

	public NavCSideException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavCSideException(string message, string diagnosticsMessage, Exception innerException)
		: base(message, diagnosticsMessage, innerException)
	{
	}

	protected NavCSideException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		remappable = info.GetBoolean("remappable");
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("remappable", remappable);
		base.GetObjectData(info, context);
	}

	public static NavCSideException CreateAutoIncrementFieldError(NavErrorCodeException originalException, string fieldCaption, string tableCaptions, string primaryKeyFieldsAndValues, bool remappable)
	{
		return new NavCSideException(originalException.ErrorCode, originalException, remappable, CultureInfo.CurrentCulture, Lang.CannotModifyAutoIncrementFieldError, DiagnosticParameter.SystemMetadata(fieldCaption), DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(primaryKeyFieldsAndValues));
	}

	public static NavCSideException CreateForInvalidValueInField(NavErrorCodeException originalException, string tableCaptions, string primaryKeyFieldsAndValues, bool remappable)
	{
		return new NavCSideException(originalException.ErrorCode, originalException, remappable, CultureInfo.CurrentCulture, Lang.BadCodeDataError, DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(primaryKeyFieldsAndValues), DiagnosticParameter.CustomerContent(originalException.Message));
	}

	public static NavCSideException CreateAndLogRecordChangedInDatabase(int errorCode, string dataChangedErrorMessage, string tableCaptions, string fieldsAndValues, string sessionContext)
	{
		NavCSideException exception = new NavCSideException(errorCode, CultureInfo.CurrentCulture, dataChangedErrorMessage, DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(fieldsAndValues));
		return NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("00001ZK", Category.Sql, exception, Verbosity.Warning, null, sessionContext) as NavCSideException;
	}
}
