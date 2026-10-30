using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavMediaSet : NavMediaValueBase, ICloneable
{
	private List<Guid> mediaInSet = new List<Guid>();

	private NavGuid cachedKey;

	public int ALCount => CountMediaInSet();

	public override NavNclType NclType => NavNclType.NavMediaSet;

	public static NavMediaSet Default => new NavMediaSet(Guid.Empty);

	public override object ClientObject => NavMediaHelper.BuildMediaDescriptionCollection(ParentId, (Guid)Key.ClientObject, referenceIsSet: true);

	public NavMediaSet()
	{
	}

	public NavMediaSet(Guid initValue)
		: base(initValue)
	{
	}

	public NavMediaSet(Guid initValue, int parentId)
		: base(initValue, parentId)
	{
	}

	public NavMediaSet(NavMediaSet other)
		: base(other)
	{
	}

	public NavMediaSet(string initValue)
		: base(initValue)
	{
	}

	public NavMediaSet(byte[] initValue)
		: base(initValue)
	{
	}

	public static NavMediaSet CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavMediaSet(NavMediaHelper.CreateGuidFromBytes(data, startIndex, length));
	}

	public static NavMediaSet CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is string initValue)
		{
			return new NavMediaSet(initValue);
		}
		NavStringValue navStringValue = value as NavStringValue;
		if (navStringValue != null)
		{
			return new NavMediaSet(navStringValue);
		}
		if (value is Guid initValue2)
		{
			return new NavMediaSet(initValue2);
		}
		if (value is NavGuid navGuid)
		{
			return new NavMediaSet(navGuid.Value);
		}
		if (value is NSMediaSetWrapper nSMediaSetWrapper)
		{
			NavMediaDescription navMediaDescription = nSMediaSetWrapper.MediaCollection.FirstOrDefault();
			if (navMediaDescription == null || navMediaDescription.MediaId.Equals(Guid.Empty))
			{
				return new NavMediaSet(Guid.Empty);
			}
			return new NavMediaSet(navMediaDescription.MediaId);
		}
		if (value is byte[] initValue3)
		{
			return new NavMediaSet(initValue3);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavMediaSet), value);
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
				throw new NavNCLArgumentException("Stream", "Import", 1);
			}
			if (!string.IsNullOrEmpty(fileName))
			{
				if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(fileName))
				{
					throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedUpload);
				}
				fileName = Path.GetFileName(fileName);
				if (string.IsNullOrEmpty(mimeType))
				{
					mimeType = NavMimeMapping.GetMimeMapping(fileName);
				}
			}
			NavSession session = NavCurrentThread.Session;
			Guid guid;
			using (new MaximizedPermissionScope(session, new int[2]
			{
				NavMediaHelper.GetMediaTableId(ParentId),
				NavMediaHelper.GetMediaSetTableId(ParentId)
			}))
			{
				Guid mediaId;
				using (NavMediaImport navMediaImport = new NavMediaImport(session, RecordCompanyName()))
				{
					mediaId = navMediaImport.ImportMediaObject(ParentId, stream.InternalStream, fileName, description, mimeType);
				}
				guid = AddMediaToSet(Key.Value, mediaId);
			}
			if (!Key.Equals(new NavGuid(guid)))
			{
				SaveValueToTableField(guid);
			}
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
		if (string.IsNullOrEmpty(mimeType))
		{
			mimeType = NavMimeMapping.GetMimeMapping(fileName);
		}
		if (!FileTypeFilter.IsFileTypeAllowedByConfiguration(fileName))
		{
			throw new NavNCLFileLoadException(Lang.FileTypeNotAllowedUpload);
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

	public int ALExport(DataError errorLevel, string fileBaseName)
	{
		if (string.IsNullOrEmpty(fileBaseName))
		{
			if (errorLevel == DataError.TrapError)
			{
				return 0;
			}
			throw new NavNCLMissingFileNameException();
		}
		return NavMediaExport.ExportMediaSetObjects(errorLevel, ParentId, ParentCompanyToken, Key.Value, fileBaseName);
	}

	public bool ALInsert(DataError errorLevel, Guid mediaId)
	{
		Guid guid;
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[2]
		{
			NavMediaHelper.GetMediaTableId(ParentId),
			NavMediaHelper.GetMediaSetTableId(ParentId)
		}))
		{
			NavMedia navMedia = new NavMedia(mediaId);
			if (!navMedia.MediaExists())
			{
				if (errorLevel == DataError.ThrowError)
				{
					throw new NavMediaNotFoundException();
				}
				return false;
			}
			if (MediaCacheContains(mediaId))
			{
				return false;
			}
			guid = AddMediaToSet(Key.Value, mediaId);
		}
		if (!Key.Equals(new NavGuid(guid)))
		{
			SaveValueToTableField(guid);
		}
		return true;
	}

	public bool ALRemove(DataError errorLevel, Guid mediaId)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { NavMediaHelper.GetMediaSetTableId(ParentId) }))
		{
			bool flag = RemoveMediaRecord(mediaId);
			if (flag)
			{
				UpdateMediaCache(addToCache: false, mediaId);
			}
			return flag;
		}
	}

	public Guid ALItem(int index)
	{
		if (index == 0)
		{
			throw new NavNCLIndexOutOfBoundsException(string.Format(CultureInfo.CurrentCulture, Lang.ParameterMustBePositive, "index"));
		}
		index--;
		if (index < 0)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang.ParameterMustBePositive, "index"));
		}
		if (!mediaInSet.Any())
		{
			UpdateMediaCache();
		}
		if (index >= mediaInSet.Count)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang._00095_50170, "index"));
		}
		return mediaInSet[index];
	}

	public object Clone()
	{
		return new NavMediaSet(this);
	}

	private int CountMediaInSet()
	{
		if (Key.IsZeroOrEmpty)
		{
			return 0;
		}
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(ParentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaSetTableId);
		navRecord.ALSetRange(1, Key);
		NavMediaHelper.SetCompanyfilter(navRecord, 3, RecordCompanyName());
		return navRecord.ALFind(DataError.TrapError, "-") ? navRecord.ALCount : 0;
	}

	internal override bool MediaExists(Guid mediaId)
	{
		if (mediaId == Guid.Empty)
		{
			return false;
		}
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(ParentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaSetTableId);
		if (ParentCompanyToken != -1)
		{
			string companyNameFromToken = NavCurrentThread.Session.Database.GetCompanyNameFromToken(ParentCompanyToken);
			NavMediaHelper.SetCompanyfilter(navRecord, 3, companyNameFromToken);
		}
		return navRecord.ALGet(DataError.TrapError, new NavGuid(mediaId));
	}

	internal bool RemoveMediaRecord(Guid mediaId)
	{
		if (mediaId == Guid.Empty)
		{
			return false;
		}
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(ParentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaSetTableId);
		if (ParentCompanyToken != -1)
		{
			string companyNameFromToken = NavCurrentThread.Session.Database.GetCompanyNameFromToken(ParentCompanyToken);
			NavMediaHelper.SetCompanyfilter(navRecord, 3, companyNameFromToken);
		}
		navRecord.ALSetFilter(1, Key.ToString());
		navRecord.ALSetFilter(2, mediaId.ToString());
		return navRecord.ALFindFirst(DataError.TrapError) && navRecord.ALDelete(DataError.TrapError);
	}

	internal IList<Guid> GetContainedMedia()
	{
		if (!Key.IsZeroOrEmpty)
		{
			return NavMediaHelper.LoadMediaInSet(ParentId, Key.ToGuid());
		}
		return new List<Guid>();
	}

	private void UpdateMediaCache()
	{
		mediaInSet.Clear();
		mediaInSet.AddRange(GetContainedMedia());
		cachedKey = Key;
	}

	private void UpdateMediaCache(bool addToCache, Guid mediaId)
	{
		if (!mediaInSet.Any() || Key.IsZeroOrEmpty || !Key.Equals(cachedKey))
		{
			UpdateMediaCache();
		}
		if (addToCache)
		{
			if (!mediaInSet.Contains(mediaId))
			{
				mediaInSet.Add(mediaId);
			}
		}
		else
		{
			mediaInSet.Remove(mediaId);
		}
	}

	private bool MediaCacheContains(Guid mediaId)
	{
		if (!mediaInSet.Any() || Key.IsZeroOrEmpty || !Key.Equals(cachedKey))
		{
			UpdateMediaCache();
		}
		return mediaInSet.Contains(mediaId);
	}

	internal Guid AddMediaToSet(Guid setId, Guid mediaId)
	{
		if (setId == Guid.Empty)
		{
			setId = Guid.NewGuid();
		}
		NavGuid navGuid = new NavGuid(setId);
		NavMedia navMedia = new NavMedia(mediaId);
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(ParentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaSetTableId);
		if (navRecord.ALGet(DataError.TrapError, navGuid, navMedia))
		{
			return setId;
		}
		navRecord.ALInit();
		navRecord.SetFieldValue(1, navGuid);
		navRecord.SetFieldValue(2, navMedia);
		navRecord.SetFieldValue(4, NavBigInteger.Create(DateTime.UtcNow.Ticks));
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(ParentId);
		if (metaTableById.DataPerCompany)
		{
			navRecord.SetFieldValue(3, new NavText(navRecord.GetFieldValue(3).NavDefinedLengthMetadata, RecordCompanyName()));
		}
		navRecord.ALInsert(DataError.ThrowError);
		UpdateMediaCache(addToCache: true, mediaId);
		return setId;
	}
}
