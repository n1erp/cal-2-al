using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

public static class NavMediaHelper
{
	private const string TenantPrefix = "Tenant ";

	private const string MediaSetDescriptionStatement = "SELECT MediaSet.[Media ID], Media.Description, Media.[Company Name] FROM [{0}].[{1}].[{2}Media Set] as MediaSet WITH(READUNCOMMITTED) LEFT OUTER JOIN[{0}].[{1}].[{2}Media] as Media WITH(READUNCOMMITTED) on MediaSet.[Media ID] = Media.ID{5} where {4}MediaSet.[ID] = @0{3} order by MediaSet.[Media Index]";

	private const string MediaSetDescriptionCompanySuffix = " AND (MediaSet.[Company Name] = @1 or MediaSet.[Company Name] = ' ')";

	private const int ColumnIndexMediaId = 0;

	private const int ColumnIndexMediaDescription = 1;

	private const int ColumnIndexMediaCompany = 2;

	private const string LoadOrderedMediaSetStatement = "SELECT [Media ID] FROM [{0}].[{1}].[{2}Media Set] WITH(READUNCOMMITTED) where {3}[ID] = @0{4} order by [Media Index]";

	private const string LoadOrderedMediaSetCompanySuffix = " AND ([Company Name] = @1 or [Company Name] = ' ')";

	internal static int GetMediaSetTableId(int parentId)
	{
		if (parentId >= 1 && SystemTables.ApplicationDatabaseTables.Contains(parentId))
		{
			return 2000000180;
		}
		return 2000000183;
	}

	internal static int GetMediaTableId(int parentId)
	{
		if (parentId >= 1 && SystemTables.ApplicationDatabaseTables.Contains(parentId))
		{
			return 2000000181;
		}
		return 2000000184;
	}

	public static NavMediaDescriptionCollection BuildMediaDescriptionCollection(int parentId, Guid referenceValue, bool referenceIsSet)
	{
		NavMediaDescriptionCollection navMediaDescriptionCollection = new NavMediaDescriptionCollection();
		if (referenceValue.Equals(Guid.Empty))
		{
			return navMediaDescriptionCollection;
		}
		string companyName = string.Empty;
		int num = (referenceIsSet ? GetMediaSetTableId(parentId) : GetMediaTableId(parentId));
		if (!SystemTables.ApplicationDatabaseTables.Contains(num) && (parentId <= 0 || NavGlobal.NCLMetadata.GetMetaTableById(parentId, -1, requireCompiled: false).DataPerCompany))
		{
			companyName = NavCurrentThread.Session.CompanyName;
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { num }))
		{
			if (!referenceIsSet)
			{
				string mediaDescription = GetMediaDescription(parentId, referenceValue, companyName);
				if (mediaDescription == null && num == 2000000184)
				{
					using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000181 }))
					{
						mediaDescription = GetMediaDescription(2000000182, referenceValue, string.Empty);
					}
				}
				if (mediaDescription != null)
				{
					NavMediaDescription item = new NavMediaDescription
					{
						MediaId = referenceValue,
						Description = mediaDescription
					};
					navMediaDescriptionCollection.MediaDescriptions.Add(item);
				}
			}
			else
			{
				NavMediaDescription item2 = new NavMediaDescription
				{
					MediaId = referenceValue,
					Description = string.Empty
				};
				navMediaDescriptionCollection.Add(item2);
				if (SerializeMediaSet(referenceValue, companyName, num, navMediaDescriptionCollection) == 0)
				{
					int num2 = 0;
					if (num == 2000000183)
					{
						using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000180 }))
						{
							num2 = SerializeMediaSet(referenceValue, string.Empty, 2000000180, navMediaDescriptionCollection);
						}
					}
					if (num2 == 0)
					{
						navMediaDescriptionCollection.Clear();
					}
				}
			}
		}
		return navMediaDescriptionCollection;
	}

	private static int SerializeMediaSet(Guid referenceValue, string companyName, int systemTableId, NavMediaDescriptionCollection navMediaDescriptionCollection)
	{
		bool includeCompanyFilter = false;
		NavDatabase mediaDatabase;
		string text;
		string tablePrefix;
		if (SystemTables.ApplicationDatabaseTables.Contains(systemTableId))
		{
			mediaDatabase = NavCurrentThread.Session.AppDatabase;
			text = "MediaSetDescriptionApp";
			tablePrefix = string.Empty;
		}
		else
		{
			mediaDatabase = NavCurrentThread.Session.Database;
			text = (string.IsNullOrEmpty(companyName) ? "MediaSetDescriptionTenant" : "MediaSetDescriptionCompanyTenant");
			tablePrefix = "Tenant ";
			includeCompanyFilter = !string.IsNullOrEmpty(companyName);
		}
		int recordsInCollection = 0;
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(mediaDatabase, TransactionType.Browse);
		string companyIdWhere = string.Empty;
		CompanyId? companyId = null;
		string companyIdJoinClause = string.Empty;
		if (!SystemTables.ApplicationDatabaseTables.Contains(systemTableId))
		{
			companyId = mediaDatabase.GetCompanyIdForSystemTables(systemTableId);
			if (companyId.HasValue)
			{
				text += "ID";
				companyIdWhere = "MediaSet.\"$companyId\" = @2 AND ";
				companyIdJoinClause = " AND MediaSet.\"$companyId\" = Media.\"$companyId\"";
			}
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, mediaDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT MediaSet.[Media ID], Media.Description, Media.[Company Name] FROM [{0}].[{1}].[{2}Media Set] as MediaSet WITH(READUNCOMMITTED) LEFT OUTER JOIN[{0}].[{1}].[{2}Media] as Media WITH(READUNCOMMITTED) on MediaSet.[Media ID] = Media.ID{5} where {4}MediaSet.[ID] = @0{3} order by MediaSet.[Media Index]", mediaDatabase.DatabaseName, "dbo", tablePrefix, includeCompanyFilter ? " AND (MediaSet.[Company Name] = @1 or MediaSet.[Company Name] = ' ')" : string.Empty, companyIdWhere, companyIdJoinClause), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			if (includeCompanyFilter)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 30));
			}
			if (!string.IsNullOrWhiteSpace(companyIdWhere))
			{
				cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = referenceValue;
			int index = 1;
			if (includeCompanyFilter)
			{
				cmd.Command.Parameters[index++].Value = companyName;
			}
			if (!string.IsNullOrWhiteSpace(companyIdWhere) && companyId.HasValue)
			{
				cmd.Command.Parameters[index].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			using (cmd.ExecuteReader())
			{
				while (cmd.ReadNextRow())
				{
					bool flag = true;
					NavMediaDescription navMediaDescriptionFromReader = GetNavMediaDescriptionFromReader(cmd.Reader);
					if (includeCompanyFilter)
					{
						string navMediaCompanyFromReader = GetNavMediaCompanyFromReader(cmd.Reader);
						flag = string.IsNullOrEmpty(navMediaCompanyFromReader) || navMediaCompanyFromReader.Equals(NavCurrentThread.Session.CompanyName, StringComparison.CurrentCultureIgnoreCase);
					}
					if (flag && navMediaDescriptionFromReader != null)
					{
						navMediaDescriptionCollection.Add(navMediaDescriptionFromReader);
					}
				}
			}
			recordsInCollection = cmd.RecordsFetchedThisExecution;
			return cmd;
		});
		return recordsInCollection;
	}

	internal static IList<Guid> LoadMediaInSet(int parentId, Guid referenceValue)
	{
		if (referenceValue == Guid.Empty)
		{
			return new List<Guid>();
		}
		string companyName = string.Empty;
		int mediaSetTableId = GetMediaSetTableId(parentId);
		bool flag = SystemTables.ApplicationDatabaseTables.Contains(mediaSetTableId);
		if (!flag && (parentId <= 0 || NavGlobal.NCLMetadata.GetMetaTableById(parentId, -1, requireCompiled: false).DataPerCompany))
		{
			companyName = NavCurrentThread.Session.CompanyName;
		}
		NavCurrentThread.Session.DataAccessSource.TenantDataAccess.DataProvider.FlushBufferedWrites(NavCurrentThread.Session.NCLMetadata.GetMetaTableById(mediaSetTableId));
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { mediaSetTableId }))
		{
			return LoadOrderedMediaSet(referenceValue, companyName, flag);
		}
	}

	private static IList<Guid> LoadOrderedMediaSet(Guid referenceValue, string companyName, bool isApplicationDatabaseTable)
	{
		NavDatabase mediaDatabase = NavCurrentThread.Session.Database;
		bool includeCompanyFilter = string.IsNullOrEmpty(companyName);
		string text = (includeCompanyFilter ? "LoadOrderedMediaSetCompanyTenant" : "LoadOrderedMediaSetTenant");
		string tablePrefix = "Tenant ";
		List<Guid> mediaList = new List<Guid>();
		if (isApplicationDatabaseTable)
		{
			mediaDatabase = NavCurrentThread.Session.AppDatabase;
			text = "LoadOrderedMediaSetApp";
			tablePrefix = string.Empty;
			includeCompanyFilter = false;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(mediaDatabase, TransactionType.Browse);
		string companyIdWhere = string.Empty;
		CompanyId? companyId = null;
		companyId = mediaDatabase.GetCompanyIdForSystemTables(isApplicationDatabaseTable ? 2000000180 : 2000000183);
		if (companyId.HasValue)
		{
			text += "ID";
			companyIdWhere = "\"$companyId\" = @2 AND ";
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(text, mediaDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Media ID] FROM [{0}].[{1}].[{2}Media Set] WITH(READUNCOMMITTED) where {3}[ID] = @0{4} order by [Media Index]", mediaDatabase.DatabaseName, "dbo", tablePrefix, companyIdWhere, includeCompanyFilter ? " AND ([Company Name] = @1 or [Company Name] = ' ')" : string.Empty), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
			if (includeCompanyFilter)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 30));
			}
			if (companyId.HasValue)
			{
				cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
			}
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = referenceValue;
			int index = 1;
			if (includeCompanyFilter)
			{
				cmd.Command.Parameters[index++].Value = companyName;
			}
			if (companyId.HasValue)
			{
				cmd.Command.Parameters[index].Value = companyId.Value;
			}
		}, (NavSqlCommand cmd) =>
		{
			using (cmd.ExecuteReader())
			{
				while (cmd.ReadNextRow())
				{
					Guid guid = cmd.Reader.GetGuid(0);
					if (guid != Guid.Empty)
					{
						mediaList.Add(guid);
					}
				}
				return cmd;
			}
		});
		return mediaList;
	}

	private static string GetNavMediaCompanyFromReader(SqlDataReader reader)
	{
		return GetStringValueFromReader(reader, 2);
	}

	private static NavMediaDescription GetNavMediaDescriptionFromReader(SqlDataReader reader)
	{
		if (!reader.HasRows)
		{
			return null;
		}
		Guid guid;
		try
		{
			if (reader.IsDBNull(0))
			{
				return null;
			}
			guid = reader.GetGuid(0);
		}
		catch (InvalidOperationException)
		{
			return null;
		}
		catch (InvalidCastException)
		{
			return null;
		}
		string stringValueFromReader = GetStringValueFromReader(reader, 1);
		if (stringValueFromReader == null)
		{
			return null;
		}
		return new NavMediaDescription
		{
			MediaId = guid,
			Description = stringValueFromReader
		};
	}

	private static string GetStringValueFromReader(SqlDataReader reader, int index)
	{
		try
		{
			return reader.IsDBNull(index) ? null : reader.GetString(index);
		}
		catch (SqlNullValueException)
		{
			return null;
		}
	}

	public static byte[] GetMediaContent(Guid mediaId, ref bool allowCache, ref string mimeType)
	{
		if (mediaId.Equals(Guid.Empty))
		{
			allowCache = false;
			mimeType = string.Empty;
			return new byte[0];
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[2] { 2000000181, 2000000184 }))
		{
			using NavRecord navRecord = GetMediaContentRecord(mediaId);
			if (navRecord == null)
			{
				allowCache = false;
				mimeType = string.Empty;
				return new byte[0];
			}
			string text = (NavText)navRecord.GetFieldValue(7);
			if (!string.IsNullOrEmpty(text) && !text.Equals(NavCurrentThread.Session.CompanyName, StringComparison.CurrentCultureIgnoreCase))
			{
				allowCache = false;
				mimeType = string.Empty;
				return new byte[0];
			}
			navRecord.ALCalcFields(DataError.ThrowError, 3);
			NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(3);
			mimeType = (NavText)navRecord.GetFieldValue(4);
			allowCache = !(NavBoolean)navRecord.GetFieldValue(10);
			return navBLOB.GetBytes();
		}
	}

	internal static NavRecord GetMediaContentRecord(Guid mediaId)
	{
		NavRecord globalRecordInstance = NavCurrentThread.Session.GetGlobalRecordInstance(2000000184);
		if (!globalRecordInstance.ALGet(DataError.TrapError, new NavGuid(mediaId)))
		{
			globalRecordInstance.Dispose();
			globalRecordInstance = NavCurrentThread.Session.GetGlobalRecordInstance(2000000181);
			if (!globalRecordInstance.ALGet(DataError.TrapError, new NavGuid(mediaId)))
			{
				globalRecordInstance.Dispose();
				return null;
			}
		}
		return globalRecordInstance;
	}

	private static string GetMediaDescription(int parentId, Guid mediaId, string companyName)
	{
		if (mediaId.Equals(Guid.Empty))
		{
			return string.Empty;
		}
		int mediaTableId = GetMediaTableId(parentId);
		using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaTableId);
		SetCompanyfilter(navRecord, 7, companyName);
		if (!navRecord.ALGet(DataError.TrapError, new NavGuid(mediaId)))
		{
			return null;
		}
		return (NavText)navRecord.GetFieldValue(2);
	}

	internal static bool IsCompanyValid(NavRecord mediaRecord, bool isMediaSet = false)
	{
		string text = (NavText)mediaRecord.GetFieldValue(isMediaSet ? 3 : 7);
		if (!string.IsNullOrEmpty(text))
		{
			return text.Equals(NavCurrentThread.Session.CompanyName, StringComparison.CurrentCultureIgnoreCase);
		}
		return true;
	}

	public static byte[] GetScaledMedia(Guid mediaId, int width, int height, ref bool allowCache, ref string mimeType)
	{
		if (mediaId.Equals(Guid.Empty))
		{
			allowCache = false;
			mimeType = string.Empty;
			return new byte[0];
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[3] { 2000000181, 2000000184, 2000000185 }))
		{
			using NavRecord navRecord = GetMediaContentRecord(mediaId);
			if (navRecord == null)
			{
				allowCache = false;
				mimeType = string.Empty;
				return new byte[0];
			}
			allowCache = !(NavBoolean)navRecord.GetFieldValue(10);
			return GetScaledMedia(navRecord, mediaId, width, height, out mimeType);
		}
	}

	private static byte[] GetBuiltInThumbnail(string sourceExtension, int width, out string thumbnailMimeType)
	{
		thumbnailMimeType = "image/png";
		string extension = NavMimeMapping.MapExtensionToKnownThumbnailType(sourceExtension);
		if (width == 0)
		{
			width = 360;
		}
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string text = executingAssembly.GetManifestResourceNames().FirstOrDefault((string name) => name.EndsWith("thumbnail" + extension + "." + width + ".png", StringComparison.OrdinalIgnoreCase));
		if (string.IsNullOrEmpty(text))
		{
			NavMediaImage navMediaImage = NavMediaDocument.PictureFromMimetype(NavMimeMapping.GetMimeMapping(sourceExtension));
			thumbnailMimeType = navMediaImage.MimeType;
			return navMediaImage.Bytes();
		}
		using MemoryStream memoryStream = MemoryStreamManager.GetStream();
		using Stream stream = executingAssembly.GetManifestResourceStream(text);
		if (stream == null)
		{
			return new byte[0];
		}
		stream.CopyTo(memoryStream);
		return memoryStream.GetBuffer();
	}

	private static byte[] GetScaledMedia(NavRecord mediaRecord, Guid mediaId, int width, int height, out string mimeType)
	{
		string text = ((NavText)mediaRecord.GetFieldValue(4)).ToString();
		string extensionForBuiltinMimeType = NavMimeMapping.GetExtensionForBuiltinMimeType(text);
		if (!string.IsNullOrEmpty(extensionForBuiltinMimeType))
		{
			return GetBuiltInThumbnail(extensionForBuiltinMimeType, width, out mimeType);
		}
		if (width == 0 && height == 0)
		{
			bool allowCache = false;
			mimeType = text;
			return GetMediaContent(mediaId, ref allowCache, ref mimeType);
		}
		NavBLOB navBLOB;
		using (NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(2000000185))
		{
			navRecord.ALSetFilter(2, "%1", new NavGuid(mediaId));
			navRecord.ALSetFilter(6, "%1", NavInteger.Create(width));
			if (height != 0)
			{
				navRecord.ALSetFilter(5, "%1", NavInteger.Create(height));
			}
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				navRecord.ALCalcFields(DataError.ThrowError, 3);
				navBLOB = (NavBLOB)navRecord.GetFieldValue(3);
				mimeType = (NavText)navRecord.GetFieldValue(4);
				return navBLOB.GetBytes();
			}
		}
		mediaRecord.ALCalcFields(DataError.ThrowError, 3);
		navBLOB = (NavBLOB)mediaRecord.GetFieldValue(3);
		NavMediaBase navMediaBase = NavMediaFactory.ProcessMediaObject(navBLOB.GetBytes(), (NavText)mediaRecord.GetFieldValue(4));
		NavMediaThumbnail navMediaThumbnail = ((height == 0) ? navMediaBase.Scale(width) : navMediaBase.Scale(width, height));
		mimeType = NavMediaImage.GetMimeType(navMediaThumbnail.imageFormat);
		InsertThumbnail(mediaRecord, mediaId, navMediaThumbnail.Data, navMediaThumbnail.Width, navMediaThumbnail.Height, mimeType);
		return navMediaThumbnail.Data;
	}

	internal static void InsertThumbnail(NavRecord parent, Guid mediaId, byte[] thumbnail, int width, int height, string mimeType, bool embedded = false)
	{
		NavSession navSession = parent?.Session ?? NavCurrentThread.Session;
		using (new MaximizedPermissionScope(navSession, new int[1] { 2000000185 }))
		{
			using NavRecord navRecord = navSession.GetGlobalRecordInstance(2000000185);
			navRecord.ALInit();
			navRecord.SetFieldValue(1, new NavGuid(Guid.NewGuid()));
			navRecord.SetFieldValue(2, new NavGuid(mediaId));
			navRecord.SetFieldValue(3, new NavBLOB(thumbnail));
			navRecord.SetFieldValue(6, NavInteger.Create(width));
			navRecord.SetFieldValue(5, NavInteger.Create(height));
			navRecord.SetFieldValue(8, NavBoolean.Create(embedded));
			navRecord.SetFieldValue(4, new NavText(navRecord.GetFieldValue(4).NavDefinedLengthMetadata, mimeType));
			NavText navText = (NavText)(parent?.GetFieldValue(7));
			string value = (((object)navText != null) ? ((string)navText) : navSession.CompanyName);
			navRecord.SetFieldValue(7, new NavText(navRecord.GetFieldValue(7).NavDefinedLengthMetadata, value));
			navRecord.ALInsert(DataError.ThrowError);
		}
	}

	public static IEnumerable<Guid> FindMediaByDescription(int parentId, string description)
	{
		int mediaTableId = GetMediaTableId(parentId);
		List<Guid> list = new List<Guid>();
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { mediaTableId }))
		{
			using NavRecord navRecord = NavCurrentThread.Session.GetGlobalRecordInstance(mediaTableId);
			navRecord.ALSetFilter(2, "%1", new NavText(navRecord.GetFieldValue(2).NavDefinedLengthMetadata, description));
			if (mediaTableId == 2000000184)
			{
				SetCompanyfilter(navRecord, 7, NavCurrentThread.Session.CompanyName);
			}
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					list.Add(navRecord.GetFieldValue(1).ToGuid());
				}
				while (navRecord.ALNext() != 0);
			}
		}
		return list;
	}

	internal static Guid CreateGuidFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length < startIndex + length || startIndex < 0 || length < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Media);
		}
		byte[] array = new byte[16];
		Array.Copy(data, startIndex, array, 0, 16);
		NavGuid navGuid = new NavGuid(array);
		return navGuid.Value;
	}

	internal static void SetCompanyfilter(NavRecord record, int companyField, string recordCompanyName)
	{
		if (!string.IsNullOrEmpty(recordCompanyName))
		{
			record.ALSetFilter(companyField, "%1|''", new NavText(recordCompanyName));
		}
	}
}
