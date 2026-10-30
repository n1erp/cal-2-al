using System;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavMedia : NavMediaValueBase
{
	private static readonly Lazy<INavMediaStorageHandler> StorageHandler = new Lazy<INavMediaStorageHandler>(NavMediaStorageHandlerFactory.CreateStorageHandler);

	internal const int DefaultMinutesToExpire = 30;

	public override NavNclType NclType => NavNclType.NavMedia;

	public override object ClientObject => NavMediaHelper.BuildMediaDescriptionCollection(ParentId, (Guid)Key.ClientObject, referenceIsSet: false);

	public static NavMedia Default => new NavMedia(Guid.Empty);

	public NavMedia()
	{
	}

	public NavMedia(Guid initValue)
		: base(initValue)
	{
	}

	public NavMedia(Guid initValue, int parentId)
		: base(initValue, parentId)
	{
	}

	public NavMedia(string initValue)
		: base(initValue)
	{
	}

	public NavMedia(byte[] initValue)
		: base(initValue)
	{
	}

	public NavMedia(NavMedia other)
		: base(other)
	{
	}

	public static NavMedia CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavMedia(NavMediaHelper.CreateGuidFromBytes(data, startIndex, length));
	}

	public static NavMedia CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is string initValue)
		{
			return new NavMedia(initValue);
		}
		NavStringValue navStringValue = value as NavStringValue;
		if (navStringValue != null)
		{
			return new NavMedia(navStringValue);
		}
		if (value is Guid initValue2)
		{
			return new NavMedia(initValue2);
		}
		if (value is NavGuid navGuid)
		{
			return new NavMedia(navGuid.Value);
		}
		if (value is NSMediaWrapper nSMediaWrapper)
		{
			NavMediaDescription navMediaDescription = nSMediaWrapper.MediaCollection.FirstOrDefault();
			if (navMediaDescription != null)
			{
				return new NavMedia(navMediaDescription.MediaId);
			}
			return new NavMedia(Guid.Empty);
		}
		if (value is byte[] initValue3)
		{
			return new NavMedia(initValue3);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavMedia), value);
	}

	public Guid ALImport(DataError errorLevel, NavStream stream, string description)
	{
		return ALImport(errorLevel, stream, description, string.Empty);
	}

	public Guid ALImport(DataError errorLevel, NavStream stream, string description, string mimeType)
	{
		return ALImport(errorLevel, stream, description, mimeType, string.Empty);
	}

	public Guid ALImport(DataError errorLevel, NavStream stream, string description, string mimeType, string fileName)
	{
		try
		{
			if (stream == null)
			{
				throw new NavNCLArgumentException("Stream", "ImportStream", 1);
			}
			if (!string.IsNullOrEmpty(fileName))
			{
				if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(fileName))
				{
					throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedUpload);
				}
				if (string.IsNullOrEmpty(mimeType))
				{
					mimeType = NavMimeMapping.GetMimeMapping(fileName);
				}
			}
			NavSession session = NavCurrentThread.Session;
			Guid guid;
			using (new MaximizedPermissionScope(session, new int[1] { NavMediaHelper.GetMediaTableId(ParentId) }))
			{
				using NavMediaImport navMediaImport = new NavMediaImport(session, RecordCompanyName());
				guid = navMediaImport.ImportMediaObject(ParentId, stream.InternalStream, fileName, description, mimeType);
			}
			SaveValueToTableField(guid);
			return guid;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return Guid.Empty;
		}
	}

	public Guid ALImport(DataError errorLevel, string fileName, string description)
	{
		return ALImport(errorLevel, fileName, description, string.Empty);
	}

	public Guid ALImport(DataError errorLevel, string fileName, string description, string mimeType)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new NavNCLInvalidPathException();
		}
		using NavScope navScope = new NavScope(NavCurrentThread.Session);
		using NavFile navFile = new NavFile(navScope);
		navFile.ALTextMode = false;
		try
		{
			navFile.ALOpen(fileName);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return Guid.Empty;
		}
		using NavInStream stream = navFile.ALCreateInStream(navScope);
		return ALImport(errorLevel, stream, description, mimeType, fileName);
	}

	public static Guid ALImportWithUrlAccess(NavStream stream, string fileName)
	{
		return ALImportWithUrlAccess(stream, fileName, 30);
	}

	public static Guid ALImportWithUrlAccess(NavStream stream, string fileName, int minutesToExpire)
	{
		if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(fileName))
		{
			throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedUpload);
		}
		if (stream == null)
		{
			throw new NavNCLArgumentException("Stream", "ImportWithUrlAccess", 1);
		}
		if (minutesToExpire < 1)
		{
			throw new NavNCLArgumentException("MinutesToExpire", "ImportWithUrlAccess", 3);
		}
		return StorageHandler.Value.ImportWithUrlAccess(stream, fileName, minutesToExpire);
	}

	public bool ALExport(DataError errorLevel, string fileName)
	{
		return NavMediaExport.ExportMediaObjectToFile(errorLevel, Key.Value, fileName, -1);
	}

	public bool ALExport(DataError errorLevel, NavStream stream)
	{
		try
		{
			if (stream == null)
			{
				throw new NavNCLArgumentException("Stream", "ExportStream", 1);
			}
			if (stream.InternalStream == null)
			{
				throw new NavNCLArgumentException("Stream provider", "ExportStream", 1);
			}
			return NavMediaExport.ExportMediaObject(stream.InternalStream, Key.Value);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public static string ALGetDocumentUrl(Guid documentId)
	{
		return StorageHandler.Value.GetDocumentUrl(documentId).ToString();
	}

	public static void CleanupExpiredFiles(NavTenant tenant)
	{
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant t, NavSession s) =>
		{
			StorageHandler.Value.CleanupExpiredFiles(t);
		}), false, false, false, (LanguageSetting)null);
	}

	public object Clone()
	{
		return new NavMedia(this);
	}

	internal override bool MediaExists(Guid mediaId)
	{
		if (mediaId == Guid.Empty)
		{
			return false;
		}
		int mediaTableId = NavMediaHelper.GetMediaTableId(ParentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaTableId);
		if (ParentCompanyToken != -1)
		{
			string companyNameFromToken = NavCurrentThread.Session.Database.GetCompanyNameFromToken(ParentCompanyToken);
			NavMediaHelper.SetCompanyfilter(navRecord, 7, companyNameFromToken);
		}
		return navRecord.ALGet(DataError.TrapError, new NavGuid(mediaId));
	}

	internal bool MediaExists()
	{
		return MediaExists(Key.ToGuid());
	}
}
