using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageTableFieldDataProvider : MetadataDataProvider
{
	public enum FieldStatus
	{
		New,
		Ready,
		Placed
	}

	private const int FirstFieldNo = 1;

	public override int TableId => 2000000171;

	public PageTableFieldDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => GetObjectNumberAndInfoWithinRange(ObjectType.Page, range, sortOrder, needNames: true).Select((KeyValuePair<int, AllObjectSnapshotEntry> pair) =>
			{
				NavValue[] array = new NavValue[3]
				{
					VirtualDataProvider.VirtualTimeStamp,
					null,
					null
				};
				KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = pair;
				array[1] = NavInteger.Create(keyValuePair.Key);
				return new ReadOnlyRecordBuffer(array);
			}), 
			2 => GetFieldsOnTable((NavInteger)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		int pageId = precedingKeyFieldValues[1].ToInt32();
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			return 0;
		}
		MasterPage pageDef = MetadataProvider.GetMasterPageForDesigner(pageId);
		int sourceTable = pageDef.PageProperties.SourceObject.SourceTable;
		int count = 0;
		if (sourceTable != 0)
		{
			NCLMetaTable metaTableById;
			try
			{
				metaTableById = NclMetadata.GetMetaTableById(sourceTable, -1, requireCompiled: false);
			}
			catch (NavMetadataNotFoundException)
			{
				return 0;
			}
			NCLMetaField[] allFields = metaTableById.AllFields;
			for (int i = 0; i < allFields.Length; i++)
			{
				if (ShowFieldInPageFieldList(allFields[i]))
				{
					int fieldNo = allFields[i].FieldNo;
					if (fieldNo >= low && fieldNo <= high)
					{
						count++;
					}
				}
			}
			allFields = metaTableById.DisabledFields;
			int num = 0;
			while (allFields != null && num < allFields.Length)
			{
				if (ShowFieldInPageFieldList(allFields[num]))
				{
					int fieldNo2 = allFields[num].FieldNo;
					if (fieldNo2 >= low && fieldNo2 <= high)
					{
						count++;
					}
				}
				num++;
			}
			List<PageTableFieldData> newDesignerFields = GetNewDesignerFields();
			foreach (PageTableFieldData item in newDesignerFields)
			{
				if (item.FieldNo >= low && item.FieldNo <= high)
				{
					count++;
				}
			}
		}
		Dictionary<string, ControlDefinition> pageControlsMapByAlIdentifier = new Dictionary<string, ControlDefinition>();
		IEnumerable<ControlDefinition> source = from ControlDefinition control in pageDef.FindAll((ElementDefinition ed) => ed is ControlDefinition)
			where control.DataColumnName != null
			select control;
		source.ForEach((ControlDefinition controlDefinition) =>
		{
			pageControlsMapByAlIdentifier[NavDesignerUtil.GetControlALIdentifier(controlDefinition, pageDef.Name)] = controlDefinition;
		});
		(from control in GetExpressionBoundFieldsFromPage(pageControlsMapByAlIdentifier, pageDef).ToList()
			where control.FieldNo >= low && control.FieldNo <= high
			select control).ForEach((PageTableFieldData control) =>
		{
			count++;
		});
		return count;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetFieldsOnTable(NavInteger pageNo, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		MasterPage page = MetadataProvider.GetMasterPageForDesigner(pageNo);
		page = MetadataProvider.RemoveItemsOnPageBasedOnLicenseAndApplicationArea(page, null);
		int sourceTable = page.PageProperties.SourceObject.SourceTable;
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		NCLMetaTable nCLMetaTable = null;
		if (sourceTable != 0)
		{
			try
			{
				nCLMetaTable = NclMetadata.GetMetaTableById(sourceTable, -1, requireCompiled: false);
			}
			catch (NavMetadataNotFoundException)
			{
				yield break;
			}
		}
		IEnumerable<ControlDefinition> enumerable = from ControlDefinition control in page.FindAll((ElementDefinition ed) => ed is ControlDefinition)
			where control.DataColumnName != null
			select control;
		Dictionary<string, List<ControlDefinition>> pageControlsMapByDatacolumn = new Dictionary<string, List<ControlDefinition>>();
		Dictionary<string, ControlDefinition> dictionary = new Dictionary<string, ControlDefinition>();
		foreach (ControlDefinition item in enumerable)
		{
			if (!pageControlsMapByDatacolumn.ContainsKey(item.DataColumnName))
			{
				pageControlsMapByDatacolumn.Add(item.DataColumnName, new List<ControlDefinition>());
			}
			pageControlsMapByDatacolumn[item.DataColumnName].Add(item);
			string controlALIdentifier = NavDesignerUtil.GetControlALIdentifier(item, page.Name);
			if (!string.IsNullOrEmpty(controlALIdentifier))
			{
				dictionary[controlALIdentifier] = item;
			}
		}
		List<PageTableFieldData> list = new List<PageTableFieldData>();
		if (nCLMetaTable != null)
		{
			AddNCLMetaFieldsToList(list, nCLMetaTable.AllFields);
			AddNCLMetaFieldsToList(list, nCLMetaTable.DisabledFields);
		}
		List<string> designerHiddenControlsAlIds = GetCurrentDesignerHiddenControlId(dictionary, page);
		List<PageTableFieldData> collection = GetExpressionBoundFieldsFromPage(dictionary, page).ToList();
		list.AddRange(collection);
		IOrderedEnumerable<PageTableFieldData> orderedEnumerable = ((sortOrder == SortOrder.Ascending) ? list.OrderBy((PageTableFieldData f) => f.FieldNo) : list.OrderByDescending((PageTableFieldData f) => f.FieldNo));
		foreach (PageTableFieldData item2 in orderedEnumerable)
		{
			if (item2.FieldNo > high || item2.FieldNo < low)
			{
				if ((sortOrder == SortOrder.Ascending && item2.FieldNo > high) || (sortOrder == SortOrder.Descending && item2.FieldNo < low))
				{
					break;
				}
				continue;
			}
			pageControlsMapByDatacolumn.TryGetValue(item2.DataColumnName, out var value);
			NavOption navOption = GetStatusOptionValue(value);
			string fieldValue = item2.Caption;
			if (NavCurrentThread.Session.CurrentDesignerLevel.In(DesignerLevels.Personalization, DesignerLevels.Configuration))
			{
				if ((item2.IsTableField && value == null) || (navOption.Value == 2 && !HasABoundControlHiddenByDesigner(value, designerHiddenControlsAlIds, page.Name)))
				{
					continue;
				}
				navOption = GetOptionValue(6, 1);
				string text = value?.FirstOrDefault()?.Caption;
				fieldValue = ((!string.IsNullOrEmpty(text)) ? text : item2.Caption);
			}
			else if (!item2.IsTableField)
			{
				if (navOption.Value == 2 && !HasABoundControlHiddenByDesigner(value, designerHiddenControlsAlIds, page.Name))
				{
					continue;
				}
				navOption = GetOptionValue(6, 1);
			}
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, pageNo, NavInteger.Create(item2.FieldNo), item2.FieldType, NavInteger.Create(item2.FieldLength), GetTruncatedTextValue(5, fieldValue), navOption, NavBoolean.Create(item2.IsTableField));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}

	private static bool HasABoundControlHiddenByDesigner(List<ControlDefinition> boundControlList, List<string> designerHiddenControlsAlIds, string pageName)
	{
		return designerHiddenControlsAlIds.Exists((string hc) => boundControlList.Exists((ControlDefinition bc) => NavDesignerUtil.GetControlALIdentifier(bc, pageName) == hc));
	}

	private static List<string> GetCurrentDesignerHiddenControlId(Dictionary<string, ControlDefinition> pageControlsMapByAlIdentifier, MasterPage page)
	{
		List<string> list = new List<string>();
		IEnumerable<NavDesignerPageControlModify> enumerable = NavCurrentThread.Session.CurrentDesignerExtension?.GetPageModifications(page.ID)?.GetPageControlChanges().OfType<NavDesignerPageControlModify>();
		if (enumerable != null)
		{
			list.AddRange(from modification in enumerable
				where pageControlsMapByAlIdentifier.ContainsKey(modification.ControlName) && (modification.GetProperty(PropertyKind.Visible)?.Value.ToString().Equals("False", StringComparison.OrdinalIgnoreCase) ?? false)
				select modification.ControlName);
		}
		return list;
	}

	private IEnumerable<PageTableFieldData> GetExpressionBoundFieldsFromPage(Dictionary<string, ControlDefinition> pageControlsMapByAlIdentifier, MasterPage page)
	{
		List<PageTableFieldData> list = new List<PageTableFieldData>();
		IEnumerable<ControlDefinition> enumerable = from c in pageControlsMapByAlIdentifier
			where !int.TryParse(c.Value.DataColumnName, out var _)
			select c.Value;
		Dictionary<string, DataFieldDefinition> dictionary = page.Expressions.ToDictionary((DataFieldDefinition ex) => ex.Name);
		foreach (ControlDefinition item2 in enumerable)
		{
			DataFieldDefinition dataFieldDefinition = (dictionary.ContainsKey(item2.DataColumnName) ? dictionary[item2.DataColumnName] : null);
			if (!Enum.TryParse<NavType>(dataFieldDefinition?.Datatype.ToString(), out var result))
			{
				result = NavType.None;
			}
			PageTableFieldData item = new PageTableFieldData
			{
				FieldNo = item2.ID,
				TableNo = 0,
				FieldType = GetTypeOptionValue(result),
				FieldLength = (dataFieldDefinition?.DataLength ?? 0),
				Caption = ((!string.IsNullOrEmpty(item2.Caption)) ? item2.Caption : (dataFieldDefinition?.SourceExpression ?? NavDesignerUtil.GetControlALIdentifier(item2, page.Name))),
				IsTableField = false,
				DataColumnName = item2.DataColumnName
			};
			list.Add(item);
		}
		return list;
	}

	private void AddNCLMetaFieldsToList(List<PageTableFieldData> pageTableFields, NCLMetaField[] metaFields)
	{
		if (metaFields == null || metaFields.Length == 0)
		{
			return;
		}
		foreach (NCLMetaField nCLMetaField in metaFields)
		{
			if (ShowFieldInPageFieldList(nCLMetaField))
			{
				PageTableFieldData item = new PageTableFieldData
				{
					FieldNo = nCLMetaField.FieldNo,
					TableNo = nCLMetaField.Parent.TableId,
					FieldType = GetTypeOptionValue(nCLMetaField.FieldNavType),
					FieldLength = nCLMetaField.FieldDefinedLength,
					Caption = nCLMetaField.FieldCaption,
					IsTableField = true,
					DataColumnName = nCLMetaField.FieldNo.ToString(CultureInfo.InvariantCulture)
				};
				pageTableFields.Add(item);
			}
		}
	}

	private NavOption GetStatusOptionValue(IEnumerable<ControlDefinition> boundControlList)
	{
		int num = 1;
		if (boundControlList != null)
		{
			num = (boundControlList.Any((ControlDefinition c) => IsControlPlaced(c.Visible)) ? 2 : num);
		}
		return GetOptionValue(6, num);
	}

	private static bool IsControlPlaced(string booleanText)
	{
		if (string.Compare(booleanText, "0", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(booleanText, "No", StringComparison.OrdinalIgnoreCase) != 0)
		{
			return string.Compare(booleanText, "False", StringComparison.OrdinalIgnoreCase) != 0;
		}
		return false;
	}

	private NavOption GetTypeOptionValue(NavType fieldType)
	{
		return GetOptionValue(3, (int)(fieldType - 1));
	}

	private List<PageTableFieldData> GetNewDesignerFields()
	{
		List<PageTableFieldData> list = new List<PageTableFieldData>();
		NavDesignerExtension currentDesignerExtension = NavCurrentThread.Session.CurrentDesignerExtension;
		if (currentDesignerExtension != null)
		{
			foreach (NavDesignerTableExtension value in currentDesignerExtension.Objects.TableExtensions.Values)
			{
				int objectId = NavGlobal.NCLMetadata.GetObjectId(ObjectType.Table, value.ExtendsTableName);
				foreach (NavDesignerTableField field in value.Fields)
				{
					list.Add(new PageTableFieldData
					{
						FieldNo = field.Number,
						TableNo = objectId,
						FieldType = GetTypeOptionValue(field.FieldType),
						FieldLength = field.Length,
						Caption = (field.GetCaption() ?? field.Name),
						DataColumnName = field.Name
					});
				}
			}
		}
		return list;
	}

	private static bool ShowFieldInPageFieldList(NCLMetaField field)
	{
		return field.FieldClass != FieldClass.FlowFilter;
	}
}
