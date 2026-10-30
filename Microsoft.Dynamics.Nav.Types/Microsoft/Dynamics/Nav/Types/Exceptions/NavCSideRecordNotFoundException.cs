using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideRecordNotFoundException : NavCSideDataException
{
	private byte[] bookmark;

	public byte[] Bookmark => bookmark;

	public NavCSideRecordNotFoundException()
	{
	}

	public NavCSideRecordNotFoundException(int errorCode)
		: this(errorCode, string.Empty)
	{
	}

	public NavCSideRecordNotFoundException(string message)
		: base(message)
	{
	}

	public NavCSideRecordNotFoundException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavCSideRecordNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavCSideRecordNotFoundException(int errorCode, CultureInfo culture, string message, byte[] bookmark, params DiagnosticParameter[] args)
		: base(errorCode, culture, message, args)
	{
		this.bookmark = bookmark;
	}

	protected NavCSideRecordNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		bookmark = (byte[])info.GetValue("bookmark", typeof(byte[]));
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("bookmark", bookmark);
		base.GetObjectData(info, context);
	}

	protected override void UpdateCloneCore(NavBaseException exceptionClone)
	{
		base.UpdateCloneCore(exceptionClone);
		(exceptionClone as NavCSideRecordNotFoundException).bookmark = bookmark;
	}

	public static NavCSideRecordNotFoundException Create(int errorCode, string tableCaption, string primaryKeyValues, byte[] bookmark, string navTenantId = null)
	{
		return new NavCSideRecordNotFoundException(errorCode, CultureInfo.CurrentCulture, Lang.RecordNotFoundError, bookmark, DiagnosticParameter.SystemMetadata(tableCaption), DiagnosticParameter.CustomerContent(primaryKeyValues))
		{
			NavTenantId = navTenantId
		};
	}
}
