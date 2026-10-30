using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectXmlMetadataLoader : INCLObjectXmlMetadataLoader
{
	private readonly NavDatabase appDatabase;

	private readonly Func<int, NCLMetaTable> systemTableRetriever;

	private readonly INavAppMetadataRetriever metadataRetriever;

	private readonly INCLObjectMetadataCache cache;

	public NCLObjectXmlMetadataLoader(Func<int, NCLMetaTable> systemTableRetriever, NavDatabase applicationDatabase, INavAppMetadataRetriever metadataRetriever, INCLObjectMetadataCache cache)
	{
		this.systemTableRetriever = systemTableRetriever;
		appDatabase = applicationDatabase;
		this.metadataRetriever = metadataRetriever;
		this.cache = cache ?? new NullNCLObjectMetadataCache();
	}

	public NCLObjectXmlMetadata GetMetaObjectAsXmlDocument(ApplicationObjectId objectId, NavAppGroup appGroup)
	{
		NavAppRuntimeMetadata objectOwner = appGroup.GetObjectOwner(objectId.ObjectType, objectId.ObjectNumber);
		if (objectOwner != null)
		{
			NavAppObjectMetadataSummary objectSummary = objectOwner.GetObjectSummary(objectId);
			NavAppObjectMetadataBlob objectMetadataBlob = metadataRetriever.GetObjectMetadataBlob(objectId, objectOwner.PackageId, (NavAppObjectMetadataBlobType)0);
			if (objectMetadataBlob == null)
			{
				throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentUICulture, objectId.ObjectType, objectId.ObjectNumber, null);
			}
			XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
			xmlDocument.LoadXml(objectMetadataBlob.Content);
			return new NCLObjectXmlMetadata(xmlDocument, objectMetadataBlob.SqlTimestamp, NavText.Create(objectSummary.MetadataHash?.HexString));
		}
		NCLObjectXmlMetadata metaObjectXmlMetadataFromApplicationDatabase = GetMetaObjectXmlMetadataFromApplicationDatabase(objectId);
		XmlDocument document = metaObjectXmlMetadataFromApplicationDatabase.Document;
		return new NCLObjectXmlMetadata(document, metaObjectXmlMetadataFromApplicationDatabase.Timestamp, metaObjectXmlMetadataFromApplicationDatabase.MetadataHash);
	}

	public NCLObjectXmlMetadata GetUnmodifiedBaseObjectXml(ApplicationObjectId objectId)
	{
		return GetMetaObjectXmlMetadataFromApplicationDatabase(objectId);
	}

	public Tuple<XmlDocument, int> GetMetaObjectAsXmlDocument(ObjectType navObjectType, string filter, bool returnFirst)
	{
		Tuple<ReadOnlyRecordBuffer, int> objectMetadataRecordFromApplicationDatabase = GetObjectMetadataRecordFromApplicationDatabase(navObjectType, filter, returnFirst);
		if (objectMetadataRecordFromApplicationDatabase == null)
		{
			return null;
		}
		NCLObjectXmlMetadata objectXmlMetadataFromObjectMetadataRecord = GetObjectXmlMetadataFromObjectMetadataRecord(objectMetadataRecordFromApplicationDatabase.Item1);
		return Tuple.Create(objectXmlMetadataFromObjectMetadataRecord.Document, objectMetadataRecordFromApplicationDatabase.Item2);
	}

	public NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> GetExtensionDeltasForAppObject(ApplicationObjectId objectId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> result = null;
		if (runtimeAppMetadata.ObjectMetadataSummaries.Any((NavAppObjectMetadataSummary s) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)s.Format == 2 && s.ObjectType == objectId.ObjectType && s.ObjectId == objectId.ObjectNumber;
		}))
		{
			result = metadataRetriever.GetObjectExtensionDeltas(objectId, runtimeAppMetadata);
		}
		return result;
	}

	private Tuple<ReadOnlyRecordBuffer, int> GetObjectMetadataRecordFromApplicationDatabase(ObjectType navObjectType, string filter, bool returnFirst)
	{
		NCLMetaTable nCLMetaTable = systemTableRetriever(2000000071);
		NCLMetaField fieldByNo = nCLMetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = nCLMetaTable.GetFieldByNo(6);
		FilterExpression left = new UnaryFilterExpression(FilterExpressionType.Equal, NavOption.Create(fieldByNo.FieldOptionMetadata, (int)navObjectType), fieldByNo.ExpressionContext);
		FilterExpression right = FilterExpressionParser.Parse(filter, fieldByNo2.ExpressionContext);
		FilterExpression filter2 = FilterExpression.And(left, right);
		byte findMethod = (byte)(returnFirst ? 91 : 93);
		NCLMetaTable table = nCLMetaTable;
		ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindTop1(appDatabase, table, 0, findMethod, locking: false, onlyCurrentKeyFieldsNeeded: false, nCLMetaTable.PrimaryKey.DefinedSortingFields, filter2);
		if (readOnlyRecordBuffer != null)
		{
			return Tuple.Create(readOnlyRecordBuffer, readOnlyRecordBuffer[fieldByNo2.FieldIndex].ToInt32());
		}
		if (navObjectType == ObjectType.MenuSuite)
		{
			return null;
		}
		throw RecordImplementationHelper.GetRecordNotFoundException(nCLMetaTable, new NavValue[0]);
	}

	private NCLObjectXmlMetadata GetObjectXmlMetadataFromObjectMetadataRecord(ReadOnlyRecordBuffer objectMetadataRecord)
	{
		NCLMetaTable objectMetadataTable = objectMetadataRecord.MetaApplicationObject as NCLMetaTable;
		if (objectMetadataTable == null || objectMetadataTable.ObjectId != 2000000071)
		{
			throw new InvalidOperationException();
		}
		NCLMetaField fieldByNo = objectMetadataTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = objectMetadataTable.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = objectMetadataTable.GetFieldByNo(27);
		NCLMetaField fieldByNo4 = objectMetadataTable.GetFieldByNo(0);
		NCLMetaField fieldByNo5 = objectMetadataTable.GetFieldByNo(30);
		NCLMetaField fieldByNo6 = objectMetadataTable.GetFieldByNo(36);
		NCLObjectMetadataCacheKey nCLObjectMetadataCacheKey = new NCLObjectMetadataCacheKey((ObjectType)objectMetadataRecord[fieldByNo.FieldIndex].ToInt32(), objectMetadataRecord[fieldByNo2.FieldIndex].ToInt32(), objectMetadataRecord[fieldByNo3.FieldIndex].ToInt32(), objectMetadataRecord[fieldByNo4.FieldIndex].ToInt64(), (NavText)objectMetadataRecord[fieldByNo5.FieldIndex]);
		NCLMetaApplicationObject.VerifyMetadataVersion(nCLObjectMetadataCacheKey.ObjectId, nCLObjectMetadataCacheKey.ObjectType, nCLObjectMetadataCacheKey.ObjectVersion);
		XmlDocument orAdd = cache.GetOrAdd(nCLObjectMetadataCacheKey, (NavBLOB blob) => GetMetaObjectAsXmlDocumentFromStream(blob.GetNewStream()), (NCLObjectMetadataCacheKey k) => GetMetadataBlobFromApplicationDatabase(k, objectMetadataTable, objectMetadataRecord));
		return new NCLObjectXmlMetadata(orAdd, nCLObjectMetadataCacheKey.Timestamp, nCLObjectMetadataCacheKey.Hash, (NavInteger)objectMetadataRecord[fieldByNo6.FieldIndex]);
	}

	private NavBLOB GetMetadataBlobFromApplicationDatabase(NCLObjectMetadataCacheKey key, NCLMetaTable objectMetadataTable, ReadOnlyRecordBuffer objectMetadataRecord)
	{
		NCLMetaField fieldByNo = objectMetadataTable.GetFieldByNo(9);
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(objectMetadataRecord);
		int blob = NavSqlBlobCommand.GetBlob(objectMetadataTable, appDatabase, 0, new FieldList(fieldByNo), mutableRecordBuffer, locking: false);
		if (blob == 85132272)
		{
			throw new NavMetadataNotFoundException(key.ObjectType, key.ObjectId);
		}
		return (NavBLOB)mutableRecordBuffer[fieldByNo.FieldIndex];
	}

	private NCLObjectXmlMetadata GetMetaObjectXmlMetadataFromApplicationDatabase(ApplicationObjectId objectId)
	{
		ReadOnlyRecordBuffer readOnlyRecordBuffer = NavGlobal.NCLCodeLoader.GetBaseApplicationObjectRecord(objectId);
		if (readOnlyRecordBuffer == null)
		{
			readOnlyRecordBuffer = GetObjectMetadataRecordFromApplicationDatabase(objectId);
		}
		return GetObjectXmlMetadataFromObjectMetadataRecord(readOnlyRecordBuffer);
	}

	private ReadOnlyRecordBuffer GetObjectMetadataRecordFromApplicationDatabase(ApplicationObjectId objectId)
	{
		NCLMetaTable nCLMetaTable = systemTableRetriever(2000000071);
		NCLMetaField fieldByNo = nCLMetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = nCLMetaTable.GetFieldByNo(6);
		NavValue[] array = new NavValue[nCLMetaTable.FieldCount];
		array[fieldByNo.FieldIndex] = NavOption.Create(fieldByNo.FieldOptionMetadata, (int)objectId.ObjectType);
		array[fieldByNo2.FieldIndex] = NavInteger.Create(objectId.ObjectNumber);
		MutableRecordBuffer inputRecord = new MutableRecordBuffer(new ReadOnlyRecordBuffer(nCLMetaTable, array));
		NCLMetaTable table = nCLMetaTable;
		ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindEqual(appDatabase, table, 0, 61, locking: false, inputRecord, null, nCLMetaTable.PrimaryKey.DefinedSortingFields);
		if (readOnlyRecordBuffer == null)
		{
			throw new NavMetadataNotFoundException(objectId.ObjectType, objectId.ObjectNumber);
		}
		return readOnlyRecordBuffer;
	}

	public static XmlDocument GetMetaObjectAsXmlDocumentFromStream(Stream stream)
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
		return document;
	}
}
