using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideDuplicateKeyException : NavCSideException
{
	private const int DuplicateKeyErrorCode = 85132273;

	public string SqlExceptionMessage { get; private set; }

	public bool UniqueIndexViolation { get; private set; }

	public NavCSideDuplicateKeyException()
		: base(85132273)
	{
	}

	public NavCSideDuplicateKeyException(string message)
		: base(85132273, message)
	{
	}

	public NavCSideDuplicateKeyException(string message, Exception innerException)
		: base(85132273, message, innerException)
	{
	}

	public NavCSideDuplicateKeyException(string message, Exception innerException, string sqlExceptionMessage, bool uniqueIndexViolation, bool remappable)
		: base(85132273, message, innerException, remappable)
	{
		SqlExceptionMessage = sqlExceptionMessage;
		UniqueIndexViolation = uniqueIndexViolation;
	}

	public NavCSideDuplicateKeyException(Exception innerException, bool remappable, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(85132273, innerException, remappable, culture, message, args)
	{
	}

	public NavCSideDuplicateKeyException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(85132273, culture, message, args)
	{
	}

	protected NavCSideDuplicateKeyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		SqlExceptionMessage = info.GetString("SqlExceptionMessage");
		UniqueIndexViolation = info.GetBoolean("UniqueIndexViolation");
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("SqlExceptionMessage", SqlExceptionMessage);
		info.AddValue("UniqueIndexViolation", UniqueIndexViolation);
		base.GetObjectData(info, context);
	}

	public static NavCSideDuplicateKeyException Create(string tableCaptions, string fieldsAndValues, bool remappable = true)
	{
		return new NavCSideDuplicateKeyException(null, remappable, CultureInfo.CurrentCulture, Lang.DuplicateKeyError, DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(fieldsAndValues));
	}

	public static NavCSideDuplicateKeyException CreateUniqueConstraint(string tableCaptions, string fieldsAndValues)
	{
		return new NavCSideDuplicateKeyException(null, false, CultureInfo.CurrentCulture, Lang.UniqueIndexError, DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(fieldsAndValues));
	}

	public static NavCSideDuplicateKeyException CreateRemappable(string tableCaptions, string fieldsAndValues, Exception innerException, bool remappable)
	{
		return new NavCSideDuplicateKeyException(innerException, remappable, CultureInfo.CurrentCulture, Lang.DuplicateKeyError, DiagnosticParameter.SystemMetadata(tableCaptions), DiagnosticParameter.CustomerContent(fieldsAndValues));
	}
}
