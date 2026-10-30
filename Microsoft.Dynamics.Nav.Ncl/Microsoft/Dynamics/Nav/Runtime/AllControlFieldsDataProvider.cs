using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AllControlFieldsDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	private static RangeList objectTypeRangeList;

	public override int TableId => 2000000202;

	private RangeList ObjectTypeRangeList
	{
		get
		{
			if (objectTypeRangeList == null)
			{
				NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(TableId, -1, requireCompiled: false);
				NCLMetaField fieldByNo = metaTableById.GetFieldByNo(1);
				objectTypeRangeList = RangeList.Union(RangeList.Create(Range.CreateForSingleValue(NavOption.Create(fieldByNo.FieldOptionMetadata, 8), fieldByNo.ExpressionContext)), RangeList.Create(Range.CreateForSingleValue(NavOption.Create(fieldByNo.FieldOptionMetadata, 3), fieldByNo.ExpressionContext)), RangeList.Create(Range.CreateForSingleValue(NavOption.Create(fieldByNo.FieldOptionMetadata, 6), fieldByNo.ExpressionContext)));
			}
			return objectTypeRangeList;
		}
	}

	public AllControlFieldsDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
			return from value in GetObjectTypesWithinRange(range, ObjectTypeRangeList, sortOrder)
				select new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, NavOption.Create(keyField.FieldOptionMetadata, (int)value), null, null);
		case 2:
		{
			NavOption objectTypeOption = (NavOption)precedingKeyFieldValues[1];
			ObjectType objectType2 = (ObjectType)objectTypeOption.ToInt32();
			return GetObjectNumberAndInfoWithinRange(objectType2, range, sortOrder, needNames: false).Select((KeyValuePair<int, AllObjectSnapshotEntry> pair) =>
			{
				NavValue[] array = new NavValue[4]
				{
					VirtualDataProvider.VirtualTimeStamp,
					objectTypeOption,
					null,
					null
				};
				KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = pair;
				array[2] = NavInteger.Create(keyValuePair.Key);
				return new ReadOnlyRecordBuffer(array);
			});
		}
		case 3:
		{
			NavOption navOption = (NavOption)precedingKeyFieldValues[1];
			ObjectType objectType = (ObjectType)navOption.ToInt32();
			NavInteger objectId = (NavInteger)precedingKeyFieldValues[2];
			return objectType switch
			{
				ObjectType.Page => GetPageControls(navOption, objectId, range, sortOrder, nonPrimaryKeyFilters), 
				ObjectType.Report => GetReportRequestPageControls(navOption, objectId, range, sortOrder, nonPrimaryKeyFilters), 
				ObjectType.XmlPort => GetXmlPortRequestPageControls(navOption, objectId, range, sortOrder, nonPrimaryKeyFilters), 
				_ => throw new NotSupportedException(objectType.ToString()), 
			};
		}
		default:
			throw new NotSupportedException();
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetReportRequestPageControls(NavOption objectType, NavInteger objectId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		MasterPage requestPageForReport = MetadataProvider.GetRequestPageForReport(objectId);
		IEnumerable<ReadOnlyRecordBuffer> pageControls = GetPageControls(requestPageForReport, objectType, objectId, low, high, sortOrder, nonPrimaryKeyFilters);
		foreach (ReadOnlyRecordBuffer item in pageControls)
		{
			yield return item;
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetXmlPortRequestPageControls(NavOption objectType, NavInteger objectId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		MasterPage requestPageForXmlPort = MetadataProvider.GetRequestPageForXmlPort(objectId);
		IEnumerable<ReadOnlyRecordBuffer> pageControls = GetPageControls(requestPageForXmlPort, objectType, objectId, low, high, sortOrder, nonPrimaryKeyFilters);
		foreach (ReadOnlyRecordBuffer item in pageControls)
		{
			yield return item;
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetPageControls(NavOption objectType, NavInteger objectId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		MasterPage masterPageForDesigner = MetadataProvider.GetMasterPageForDesigner(objectId);
		IEnumerable<ReadOnlyRecordBuffer> pageControls = GetPageControls(masterPageForDesigner, objectType, objectId, low, high, sortOrder, nonPrimaryKeyFilters);
		foreach (ReadOnlyRecordBuffer item in pageControls)
		{
			yield return item;
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetPageControls(MasterPage page, NavValue objectType, NavValue objectId, int low, int high, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int tableNo = page.PageProperties.SourceObject.SourceTable;
		MetaTable metaTable = null;
		IEnumerable<ControlDefinition> source = page.FindAll((ElementDefinition ed) => ed is ControlDefinition).Cast<ControlDefinition>();
		IOrderedEnumerable<ControlDefinition> orderedEnumerable = ((sortOrder == SortOrder.Ascending) ? source.OrderBy((ControlDefinition c) => c.ID) : source.OrderByDescending((ControlDefinition c) => c.ID));
		foreach (ControlDefinition pageControl in orderedEnumerable)
		{
			if (pageControl.ID > high || pageControl.ID < low)
			{
				if ((sortOrder == SortOrder.Ascending && pageControl.ID > high) || (sortOrder == SortOrder.Descending && pageControl.ID < low))
				{
					break;
				}
				continue;
			}
			NavValue[] array = CreateRecord(objectType, objectId, pageControl);
			if (int.TryParse(pageControl.DataColumnName, out var result))
			{
				TryInitRecordFromField(array, tableNo, result, ref metaTable);
			}
			else
			{
				DataFieldDefinition dataFieldDefinition = page.Expressions.FirstOrDefault((DataFieldDefinition x) => x.Name == pageControl.DataColumnName);
				if (dataFieldDefinition != null)
				{
					if (!TryInitRecordFromField(array, dataFieldDefinition.TableNo, dataFieldDefinition.FieldNo, ref metaTable))
					{
						array[5] = GetTypeOptionValue(CommonTypeInformation.DataTypeToNavType(dataFieldDefinition.Datatype));
						array[6] = NavInteger.Create(dataFieldDefinition.DataLength);
						if (dataFieldDefinition.Datatype == DataType.Option)
						{
							array[7] = NavText.Create(dataFieldDefinition.OptionString);
							array[8] = GetMultiLanguageValue(pageControl.OptionCaptionML);
						}
					}
					array[12] = NavText.Create(dataFieldDefinition.SourceExpression);
				}
			}
			if (!string.IsNullOrEmpty(pageControl.CaptionML))
			{
				array[9] = GetMultiLanguageValue(pageControl.CaptionML);
			}
			if (!string.IsNullOrEmpty(pageControl.OptionCaptionML))
			{
				array[8] = GetMultiLanguageValue(pageControl.OptionCaptionML);
			}
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, array);
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}

	private static NavValue[] CreateRecord(NavValue objectType, NavValue objectId, ControlDefinition pageControl)
	{
		return new NavValue[13]
		{
			VirtualDataProvider.VirtualTimeStamp,
			objectType,
			objectId,
			NavInteger.Create(pageControl.ID),
			NavText.Create(pageControl.Name),
			null,
			null,
			NavText.Empty,
			NavText.Empty,
			NavText.Empty,
			NavInteger.Default,
			NavInteger.Default,
			NavText.Empty
		};
	}

	private bool TryInitRecordFromField(NavValue[] record, int tableNo, int fieldNo, ref MetaTable metaTable)
	{
		if (tableNo > 0)
		{
			if (metaTable == null)
			{
				metaTable = MetadataProvider.GetTableMetadata(tableNo);
			}
			MetaField fieldsById = metaTable.GetFieldsById(fieldNo);
			if (fieldsById != null)
			{
				InitRecordFromField(record, metaTable, fieldsById);
				return true;
			}
		}
		return false;
	}

	private void InitRecordFromField(NavValue[] record, MetaTable metaTable, MetaField metaField)
	{
		record[9] = GetMultiLanguageValue(metaField.CaptionML);
		record[12] = NavText.Create(metaField.Name);
		record[5] = GetTypeOptionValue(metaField.Type);
		record[6] = NavInteger.Create(metaField.Length);
		if (metaField.Type == NavType.Option)
		{
			record[7] = NavText.Create(metaField.OptionString);
			record[8] = GetMultiLanguageValue(metaField.OptionCaptionML);
		}
		record[10] = NavInteger.Create(metaTable.Id);
		record[11] = NavInteger.Create(metaField.Id);
	}

	private NavOption GetTypeOptionValue(NavType navType)
	{
		return GetOptionValue(5, (int)(navType - 1));
	}

	private static NavText GetMultiLanguageValue(string value)
	{
		return NavText.Create(MultiLanguage.GetLanguageValue(NavCurrentThread.Session.AbbreviatedLanguageName, null, value));
	}
}
