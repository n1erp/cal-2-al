using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavMediaValueBase : NavValue, IDisposable
{
	private int parentTableId;

	protected NavGuid Key { get; }

	protected int FieldNo { get; set; }

	protected int ParentCompanyToken { get; set; }

	protected NavRecord ParentRecord { get; private set; }

	public Guid ALMediaId => Key.Value;

	public bool ALHasValue
	{
		get
		{
			if (!IsZeroOrEmpty)
			{
				return MediaExists(Key.Value);
			}
			return false;
		}
	}

	public override NavValue InnerValue => Key;

	public override int GetBytesSize => Key.GetBytesSize;

	public override bool IsZeroOrEmpty => Key.IsZeroOrEmpty;

	public override object ValueAsObject => Key.ValueAsObject;

	public int ParentId => parentTableId;

	protected NavMediaValueBase(Guid initValue, int parentId)
	{
		Key = new NavGuid(initValue);
		parentTableId = parentId;
	}

	protected NavMediaValueBase(string initValue)
	{
		Key = new NavGuid(initValue);
	}

	protected NavMediaValueBase(byte[] initValue)
	{
		if (initValue == null || initValue.Length == 0)
		{
			Key = new NavGuid(Guid.NewGuid());
		}
		else
		{
			Key = new NavGuid(NavMediaHelper.CreateGuidFromBytes(initValue, 0, initValue.Length));
		}
	}

	protected NavMediaValueBase()
	{
		Key = new NavGuid(Guid.NewGuid());
	}

	protected NavMediaValueBase(Guid initValue)
	{
		Key = new NavGuid(initValue);
	}

	protected NavMediaValueBase(NavMediaValueBase other)
	{
		Key = other.Key;
		parentTableId = other.parentTableId;
		ParentCompanyToken = other.ParentCompanyToken;
		FieldNo = other.FieldNo;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public bool Equals(NavMediaValueBase other)
	{
		if (other != null)
		{
			return Key.Equals(other.Key);
		}
		return false;
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (other.NavType != NavType)
		{
			return ThrowUnableToCompareException(other);
		}
		return Key.Equals((other as NavMediaValueBase)?.Key);
	}

	public override bool Equals(object obj)
	{
		if (obj is NavValue other)
		{
			return Equals(other);
		}
		if (obj is Guid)
		{
			return Key.Value == (Guid)obj;
		}
		return ThrowUnableToCompareException(obj);
	}

	public override int CompareTo(NavValue other)
	{
		if (other.NclType == NclType)
		{
			return Key.Value.CompareTo(other.ToGuid());
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	protected void SaveValueToTableField(Guid value)
	{
		if (ParentRecord == null)
		{
			return;
		}
		if (ParentRecord.IsDisposed)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000E8", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Media parent record is disposed. SessionID:{0}, Table ID:{1}, field ID:{2}, media ID:{3}, constructed in sessionId:{4}", NavCurrentThread.Session.Id, parentTableId, FieldNo, value, ParentRecord.RequiredSessionId);
			throw new NavNCLRecordNotOpenedException();
		}
		try
		{
			NCLMetaField fieldByNo = ParentRecord.MetaTable.GetFieldByNo(FieldNo);
			if (fieldByNo.FieldNavType.Equals(NavType.Media))
			{
				ParentRecord.SetFieldValueSafe(FieldNo, NavType.Media, new NavMedia(value));
			}
			else
			{
				ParentRecord.SetFieldValueSafe(FieldNo, NavType.MediaSet, new NavMediaSet(value));
			}
		}
		catch (ObjectDisposedException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000E9", Category.Runtime, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Media parent record got disposed. Session ID = {0}, Table ID = {1}, field ID = {2}, media ID = {3}", NavCurrentThread.Session.Id, parentTableId, FieldNo, value)));
			throw new NavNCLUnexpectedClrException(Lang.UnexpectedClrExceptionThrown, ex);
		}
		catch (NullReferenceException ex2)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000EB", Category.Runtime, ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Media parent record got disposed. SessionID:{0}, Table ID:{1}, field ID:{2}, media ID:{3}, constructed in sessionId:{4}", NavCurrentThread.Session.Id, parentTableId, FieldNo, value, ParentRecord.RequiredSessionId)));
			throw new NavNCLUnexpectedClrException(Lang.UnexpectedClrExceptionThrown, ex2);
		}
	}

	public override byte[] GetBytes()
	{
		return Key.GetBytes();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			ParentRecord = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public override string ToString()
	{
		return Key.ToString();
	}

	public override Guid ToGuid()
	{
		return Key.Value;
	}

	internal void SetOwnerRecordInformation(NavRecord parentRecord, int companyNameToken, int tableId, int fieldNo)
	{
		ParentRecord = parentRecord;
		ParentCompanyToken = companyNameToken;
		parentTableId = tableId;
		FieldNo = fieldNo;
	}

	internal abstract bool MediaExists(Guid mediaId);

	protected string RecordCompanyName()
	{
		if (ParentRecord != null)
		{
			return ParentRecord.CompanyName;
		}
		string text = string.Empty;
		if (ParentCompanyToken != -1)
		{
			text = NavCurrentThread.Session.Database.GetCompanyNameFromToken(ParentCompanyToken);
		}
		if (string.IsNullOrEmpty(text))
		{
			text = NavCurrentThread.Session.CompanyName;
		}
		return text;
	}
}
