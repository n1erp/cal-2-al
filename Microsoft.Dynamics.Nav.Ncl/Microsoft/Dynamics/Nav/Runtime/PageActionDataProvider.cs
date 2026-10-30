using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageActionDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000143;

	public PageActionDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
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
			2 => GetActions((NavInteger)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetActions(NavInteger pageNo, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		int index = 0;
		int autoId = -1;
		int num = 0;
		PageDefinition item = MetadataProvider.GetPageDefinitionWithExtensions(pageNo).Item;
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		foreach (ActionContainerDefinition actionContainer in item.ActionContainers)
		{
			PageActionData orCreatePageActionData = GetOrCreatePageActionData(pageNo, ref autoId, ref index, num, 0, actionContainer);
			AddRowIfInRange(list, low, high, orCreatePageActionData);
			VisitActions(ref index, num + 1, low, high, list, pageNo, orCreatePageActionData.ActionId, actionContainer.Actions);
		}
		foreach (ReadOnlyRecordBuffer item2 in (sortOrder == SortOrder.Ascending) ? list.OrderBy((ReadOnlyRecordBuffer r) => r[2]) : list.OrderByDescending((ReadOnlyRecordBuffer r) => r[2]))
		{
			if (item2.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return item2;
			}
		}
	}

	private void VisitActions(ref int index, int indentation, int low, int high, List<ReadOnlyRecordBuffer> rows, NavInteger pageNo, int parentActionId, List<ActionBaseDefinition> actions)
	{
		if (actions == null)
		{
			return;
		}
		foreach (ActionBaseDefinition action in actions)
		{
			PageActionData orCreatePageActionData = GetOrCreatePageActionData(pageNo, ref index, indentation, parentActionId, action);
			AddRowIfInRange(rows, low, high, orCreatePageActionData);
			VisitActions(ref index, indentation + 1, low, high, rows, pageNo, action.ID, action.Actions);
		}
	}

	private void AddRowIfInRange(List<ReadOnlyRecordBuffer> rows, int low, int high, PageActionData pageActionData)
	{
		if (pageActionData != null && pageActionData.ActionIndex <= high && pageActionData.ActionIndex >= low)
		{
			AssignToolTipML(MultiLanguage.GetLanguageValue(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)), pageActionData.ToolTipML), out var toolTip, out var toolTip2, out var toolTip3, out var toolTip4);
			ReadOnlyRecordBuffer item = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(pageActionData.PageId), NavInteger.Create(pageActionData.ActionIndex), NavInteger.Create(pageActionData.ActionId), NavInteger.Create(pageActionData.Indentation), NavInteger.Create(pageActionData.ParentActionId), GetOptionValue(6, pageActionData.ActionType), GetOptionValue(7, pageActionData.ActionSubtype + 1), GetTruncatedTextValue(8, pageActionData.Name), GetTruncatedTextValue(9, MultiLanguage.GetLanguageValue(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)), pageActionData.CaptionML)), GetTruncatedTextValue(10, toolTip), GetTruncatedTextValue(11, toolTip2), GetTruncatedTextValue(12, toolTip3), GetTruncatedTextValue(13, toolTip4), NavBoolean.Create(pageActionData.Promoted), GetOptionValue(15, pageActionData.RunObjectType), NavInteger.Create(pageActionData.RunObjectId), GetOptionValue(17, pageActionData.RunPageMode + 1), NavInteger.Create(pageActionData.RunSourceTable), GetTruncatedTextValue(19, pageActionData.ApplicationArea));
			rows.Add(item);
		}
	}

	internal static void AssignToolTipML(string toolTip, out string toolTip1, out string toolTip2, out string toolTip3, out string toolTip4)
	{
		toolTip1 = string.Empty;
		toolTip2 = string.Empty;
		toolTip3 = string.Empty;
		toolTip4 = string.Empty;
		if (string.IsNullOrWhiteSpace(toolTip))
		{
			return;
		}
		int length = toolTip.Length;
		toolTip1 = toolTip.Substring(0, Math.Min(length, 250));
		length -= 250;
		if (length < 1)
		{
			return;
		}
		toolTip2 = toolTip.Substring(250, Math.Min(length, 250));
		length -= 250;
		if (length >= 1)
		{
			toolTip3 = toolTip.Substring(500, Math.Min(length, 250));
			length -= 250;
			if (length >= 1)
			{
				toolTip4 = toolTip.Substring(750, Math.Min(length, 250));
			}
		}
	}

	private static PageActionData GetOrCreatePageActionData(int pageId, ref int index, int indentation, int parentActionId, ActionBaseDefinition actionBase)
	{
		if (actionBase.PageActionData == null)
		{
			PageActionData pageActionData = new PageActionData
			{
				PageId = pageId,
				ActionIndex = index++,
				ActionId = actionBase.ID,
				Indentation = indentation,
				ParentActionId = parentActionId,
				ActionSubtype = -1,
				RunPageMode = -1,
				Name = actionBase.Name,
				CaptionML = actionBase.CaptionML
			};
			if (!(actionBase is ActionDefinition actionDefinition))
			{
				if (!(actionBase is SeparatorDefinition))
				{
					if (!(actionBase is ActionGroupDefinition actionGroupDefinition))
					{
						throw new NotImplementedException();
					}
					pageActionData.ActionType = 3;
					pageActionData.ToolTipML = actionGroupDefinition.ToolTipML;
				}
				else
				{
					pageActionData.ActionType = 2;
				}
			}
			else
			{
				pageActionData.ActionType = 1;
				pageActionData.ToolTipML = actionDefinition.ToolTipML;
				pageActionData.RunPageMode = (int)actionDefinition.RunPageMode;
				pageActionData.ApplicationArea = actionDefinition.ApplicationArea;
				pageActionData.Promoted = actionDefinition.Promoted;
				pageActionData.RunObjectType = (int)actionDefinition.RunObjectType;
				pageActionData.RunObjectId = actionDefinition.TargetID;
				pageActionData.RunSourceTable = actionDefinition.RunObjectSrcTable;
				pageActionData.RunPageMode = (int)actionDefinition.RunPageMode;
			}
			actionBase.PageActionData = pageActionData;
		}
		return actionBase.PageActionData;
	}

	private static PageActionData GetOrCreatePageActionData(int pageId, ref int autoId, ref int index, int indentation, int parentActionId, ActionContainerDefinition actionContainer)
	{
		if (actionContainer.PageActionData == null)
		{
			PageActionData pageActionData = new PageActionData
			{
				PageId = pageId,
				ActionIndex = index++,
				ActionId = ((actionContainer.ID > 0) ? actionContainer.ID : autoId--),
				Indentation = indentation,
				ParentActionId = parentActionId,
				ToolTipML = actionContainer.ToolTipML,
				ActionType = 0,
				ActionSubtype = (int)actionContainer.ActionContainerType,
				RunPageMode = -1
			};
			actionContainer.PageActionData = pageActionData;
		}
		return actionContainer.PageActionData;
	}
}
