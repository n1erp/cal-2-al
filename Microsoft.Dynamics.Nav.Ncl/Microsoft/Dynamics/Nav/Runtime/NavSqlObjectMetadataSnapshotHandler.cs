using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlObjectMetadataSnapshotHandler
{
	private static readonly Lazy<DataContractSerializer> serializer = new Lazy<DataContractSerializer>(() => new DataContractSerializer(typeof(MetaTable)));

	internal static bool GetTableMetadataFromTenant(NavDatabase tenantDatabase, int tableId, out NCLMetaTable table, bool databaseConvertedFromNav2013, bool lockExclusive = true)
	{
		NavBLOB rawMetadata;
		return GetTableMetadataFromTenant(tenantDatabase, tableId, out table, databaseConvertedFromNav2013, out rawMetadata, lockExclusive);
	}

	internal static bool GetTableMetadataFromTenant(NavDatabase tenantDatabase, int tableId, out NCLMetaTable table, bool databaseConvertedFromNav2013, out NavBLOB rawMetadata, bool lockExclusive = true)
	{
		return GetTableMetadataFromTenant(tenantDatabase, tableId, useBaseMetadata: false, out table, databaseConvertedFromNav2013, out rawMetadata, lockExclusive);
	}

	internal static bool GetBaseTableMetadataFromTenant(NavDatabase tenantDatabase, int tableId, out NCLMetaTable table, bool databaseConvertedFromNav2013, out NavBLOB rawMetadata)
	{
		return GetTableMetadataFromTenant(tenantDatabase, tableId, useBaseMetadata: true, out table, databaseConvertedFromNav2013, out rawMetadata);
	}

	internal static void GetTableMetadataFromAppDatabase(int tableId, out NCLMetaTable table, out NavBLOB rawMetadata, out NavText newHash, out int schemaHash)
	{
		Tuple<XmlDocument, NavText, NCLMetaTable, int> tableMetadataForUnmodifiedBaseTable;
		using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
		{
			tableMetadataForUnmodifiedBaseTable = GetTableMetadataForUnmodifiedBaseTable(tableId);
		}
		XmlDocument item = tableMetadataForUnmodifiedBaseTable.Item1;
		newHash = tableMetadataForUnmodifiedBaseTable.Item2;
		table = tableMetadataForUnmodifiedBaseTable.Item3;
		schemaHash = tableMetadataForUnmodifiedBaseTable.Item4;
		rawMetadata = new NavBLOB(Encoding.UTF8, item.OuterXml)
		{
			IsDirty = true
		};
	}

	private static Tuple<XmlDocument, NavText, NCLMetaTable, int> GetTableMetadataForUnmodifiedBaseTable(int tableId)
	{
		ApplicationObjectId objectId = new ApplicationObjectId(ObjectType.Table, tableId);
		NCLObjectXmlMetadata unmodifiedBaseObjectXml = NavGlobal.NCLObjectXmlMetadataLoader.GetUnmodifiedBaseObjectXml(objectId);
		NCLMetaApplicationObjectLoader loader = NCLMetaApplicationObjectLoader.CreateFromNavGlobal();
		MetaTable metaTable = MetaTable.CreateMetaTableFromXml(unmodifiedBaseObjectXml.Document.DocumentElement, 0, 0L);
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(loader, tableId, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(metaTable, databaseConvertedFromNav2013: false);
		XmlDocument metadataInLatestFormat = GetMetadataInLatestFormat(metaTable);
		return Tuple.Create(metadataInLatestFormat, unmodifiedBaseObjectXml.MetadataHash, nCLMetaTable, unmodifiedBaseObjectXml.SchemaHash);
	}

	internal static MetaTable GetMetaTableFromStream(Stream stream, int versionNo)
	{
		XmlDocument document = XmlHelper.SafeXmlDocument();
		long position = stream.Position;
		try
		{
			document.Load(XmlHelper.SafeXmlReader(stream));
		}
		catch (XmlException)
		{
			stream.Position = position;
			XmlMetadataCleaner.CleanForInvalidCharacters(stream, ref document);
		}
		return GetMetaTableFromMetadata(document.DocumentElement, versionNo);
	}

	internal static MetaTable GetMetaTableFromMetadata(XmlElement root, int versionNo)
	{
		if (versionNo == 2)
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlTextWriter w = new XmlTextWriter(stringWriter);
			root.WriteTo(w);
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(stringWriter.ToString()))
			{
				Position = 0L
			};
			XmlDictionaryReaderQuotas max = XmlDictionaryReaderQuotas.Max;
			XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, max);
			return (MetaTable)serializer.Value.ReadObject(reader, verifyObjectName: true);
		}
		return MetaTable.CreateMetaTableFromXml(root, 0, 0L);
	}

	private static bool GetTableMetadataFromTenant(NavDatabase tenantDatabase, int tableId, bool useBaseMetadata, out NCLMetaTable table, bool databaseConvertedFromNav2013, out NavBLOB rawMetadata, bool lockExclusive = true)
	{
		using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
		{
			int fieldNo = (useBaseMetadata ? 33 : 9);
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
			NCLMetaField fieldByNo = metaTableById.GetFieldByNo(fieldNo);
			NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(3);
			NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(6);
			NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(39);
			NavValue[] array = new NavValue[metaTableById.FieldCount];
			array[fieldByNo2.FieldIndex] = NavOption.Create(fieldByNo2.FieldOptionMetadata, 1);
			array[fieldByNo3.FieldIndex] = NavInteger.Create(tableId);
			array[fieldByNo.FieldIndex] = new NavBLOB();
			MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array));
			if (NavSqlBlobCommand.GetBlob(metaTableById, tenantDatabase, 0, new FieldList(fieldByNo), mutableRecordBuffer, lockExclusive) == 85132272)
			{
				table = null;
				rawMetadata = null;
				return false;
			}
			NavInteger tableBlobVersion;
			if (tenantDatabase.TenantDatabaseProperties != null && tenantDatabase.TenantDatabaseProperties.VersionNo >= 1075)
			{
				FilterExpression[] expressions = new FilterExpression[2]
				{
					new UnaryFilterExpression(FilterExpressionType.Equal, array[fieldByNo2.FieldIndex], fieldByNo2.ExpressionContext),
					new UnaryFilterExpression(FilterExpressionType.Equal, array[fieldByNo3.FieldIndex], fieldByNo3.ExpressionContext)
				};
				FilterExpression filter = FilterExpression.And(expressions);
				bool locking = lockExclusive;
				NCLMetaTable table2 = metaTableById;
				ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindTop1(tenantDatabase, table2, 0, 91, locking, onlyCurrentKeyFieldsNeeded: false, metaTableById.PrimaryKey.DefinedSortingFields, filter);
				tableBlobVersion = (NavInteger)readOnlyRecordBuffer[fieldByNo4.FieldIndex];
			}
			else
			{
				tableBlobVersion = NavInteger.Create(1);
			}
			NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer[fieldByNo.FieldIndex];
			if (navBLOB == null)
			{
				table = null;
				rawMetadata = null;
				return false;
			}
			MetaTable metaTableFromMetadataBlob = GetMetaTableFromMetadataBlob(navBLOB, tableBlobVersion);
			XmlDocument metadataInLatestFormat = GetMetadataInLatestFormat(metaTableFromMetadataBlob);
			rawMetadata = new NavBLOB(Encoding.UTF8.GetBytes(metadataInLatestFormat.OuterXml))
			{
				IsDirty = navBLOB.IsDirty
			};
			table = NCLMetaTable.CreateEmptyNCLMetaTable(null, tableId, NavAppGroup.BaseGroup);
			table.AssignFromMetaTable(metaTableFromMetadataBlob, databaseConvertedFromNav2013);
			return true;
		}
	}

	private static MetaTable GetMetaTableFromMetadataBlob(NavBLOB blob, NavInteger tableBlobVersion)
	{
		XmlDocument document = XmlHelper.SafeXmlDocument();
		try
		{
			document.Load(XmlHelper.SafeXmlReader(blob.GetNewStream()));
		}
		catch (XmlException)
		{
			try
			{
				XmlMetadataCleaner.CleanForInvalidCharacters(blob.GetNewStream(), ref document);
			}
			catch (XmlException)
			{
				string message = Encoding.Default.GetString(blob.GetBytes());
				LogWriterHelper.LogEvent(EventLogEntryType.Error, message);
				throw;
			}
		}
		return GetMetaTableFromMetadata(document.DocumentElement, tableBlobVersion);
	}

	internal static void SaveNewTableMetadataToTenantSingleTable(NavDatabase tenantDatabase, NCLMetaTable newTable, MetadataHashTuple newHashTuple, NavBLOB rawMetadata)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(9);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(33);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo5 = metaTableById.GetFieldByNo(10);
		NCLMetaField fieldByNo6 = metaTableById.GetFieldByNo(11);
		NCLMetaField fieldByNo7 = metaTableById.GetFieldByNo(30);
		NCLMetaField fieldByNo8 = metaTableById.GetFieldByNo(36);
		NCLMetaField fieldByNo9 = metaTableById.GetFieldByNo(39);
		NCLMetaField fieldByNo10 = metaTableById.GetFieldByNo(40);
		NavOption navOption = NavOption.Create(fieldByNo3.FieldOptionMetadata, 1);
		NavValue[] array = new NavValue[metaTableById.FieldCount];
		array[fieldByNo3.FieldIndex] = navOption;
		array[fieldByNo4.FieldIndex] = NavInteger.Create(newTable.TableId);
		array[fieldByNo.FieldIndex] = rawMetadata;
		array[fieldByNo2.FieldIndex] = rawMetadata;
		array[fieldByNo5.FieldIndex] = new NavText(fieldByNo5.FieldDefinedLength, newTable.TableName);
		array[fieldByNo6.FieldIndex] = NavBoolean.Create(newTable.DataPerCompany);
		array[fieldByNo7.FieldIndex] = newHashTuple.MetadataHash;
		array[fieldByNo8.FieldIndex] = newHashTuple.MetadataHash;
		array[fieldByNo9.FieldIndex] = NavInteger.Create(2);
		array[fieldByNo10.FieldIndex] = NavInteger.Create(newHashTuple.SchemaHash);
		MutableRecordBuffer recordBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array));
		NavValue identityValue = null;
		NavSqlDmlCommand.Insert(tenantDatabase, metaTableById, 0, recordBuffer, ref identityValue, out var _);
	}

	internal static void SaveTableMetadataToTenantDatabase(NavDatabase tenantDatabase, IEnumerable<NavSqlTableAndChange> tablesAndChanges)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(9);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(33);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo5 = metaTableById.GetFieldByNo(10);
		NCLMetaField fieldByNo6 = metaTableById.GetFieldByNo(11);
		NCLMetaField fieldByNo7 = metaTableById.GetFieldByNo(30);
		NCLMetaField fieldByNo8 = metaTableById.GetFieldByNo(36);
		NCLMetaField fieldByNo9 = metaTableById.GetFieldByNo(39);
		NCLMetaField fieldByNo10 = metaTableById.GetFieldByNo(40);
		NavOption navOption = NavOption.Create(fieldByNo3.FieldOptionMetadata, 1);
		foreach (NavSqlTableAndChange tablesAndChange in tablesAndChanges)
		{
			NavValue[] array = new NavValue[metaTableById.FieldCount];
			array[fieldByNo3.FieldIndex] = navOption;
			if (tablesAndChange.Excessive)
			{
				array[fieldByNo4.FieldIndex] = NavInteger.Create(tablesAndChange.OldTable.TableId);
				NavSqlDmlCommand.Delete(tenantDatabase, metaTableById, 0, new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array)), recordConsistentWithCurrentTransaction: true);
				continue;
			}
			array[fieldByNo4.FieldIndex] = NavInteger.Create(tablesAndChange.NewTable.TableId);
			MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array));
			mutableRecordBuffer[fieldByNo.FieldIndex] = tablesAndChange.NewMetadata;
			mutableRecordBuffer[fieldByNo2.FieldIndex] = tablesAndChange.BaseMetadata ?? new NavBLOB();
			mutableRecordBuffer[fieldByNo5.FieldIndex] = new NavText(fieldByNo5.FieldDefinedLength, tablesAndChange.NewTable.TableName);
			mutableRecordBuffer[fieldByNo6.FieldIndex] = NavBoolean.Create(tablesAndChange.NewTable.DataPerCompany);
			mutableRecordBuffer[fieldByNo7.FieldIndex] = tablesAndChange.NewHash;
			mutableRecordBuffer[fieldByNo8.FieldIndex] = tablesAndChange.BaseHash ?? NavText.Empty;
			mutableRecordBuffer[fieldByNo9.FieldIndex] = NavInteger.Create(2);
			mutableRecordBuffer[fieldByNo10.FieldIndex] = NavInteger.Create(tablesAndChange.SchemaHash);
			NavBigInteger timestampValue;
			if (tablesAndChange.Changed)
			{
				NavSqlDmlCommand.Modify(tenantDatabase, 0, mutableRecordBuffer, recordHasBeenReadWithLockInsideThisTransaction: true, out timestampValue);
				continue;
			}
			array[fieldByNo4.FieldIndex] = NavInteger.Create(tablesAndChange.NewTable.TableId);
			NavValue identityValue = null;
			NavSqlDmlCommand.Insert(tenantDatabase, metaTableById, 0, mutableRecordBuffer, ref identityValue, out timestampValue);
		}
	}

	private static XmlDocument GetMetadataInLatestFormat(MetaTable mt)
	{
		mt.SkipValidatingEvents = true;
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild();
		try
		{
			serializer.Value.WriteObject(xmlWriter, mt);
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Cannot serialize the MetaTable"), innerException);
		}
		xmlWriter.Close();
		return xmlDocument;
	}
}
