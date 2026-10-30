using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageControlFieldDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	public override int TableId => 2000000192;

	public PageControlFieldDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
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
			2 => GetControlsOnPage((NavInteger)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetControlsOnPage(NavInteger pageNo, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		MasterPage page = MetadataProvider.GetMasterPageForDesigner(pageNo);
		int tableNo = page.PageProperties.SourceObject.SourceTable;
		MetaTable metaTable = null;
		IEnumerable<(int, ControlDefinition)> source = page.FindAll((ElementDefinition ed) => ed is ControlDefinition).Cast<ControlDefinition>().Select((ControlDefinition cd, int i) => (sequence: i, control: cd));
		IOrderedEnumerable<(int, ControlDefinition)> orderedEnumerable = ((sortOrder == SortOrder.Ascending) ? source.OrderBy(((int sequence, ControlDefinition control) c) => c.control.ID) : source.OrderByDescending(((int sequence, ControlDefinition control) c) => c.control.ID));
		foreach (var pageControl in orderedEnumerable)
		{
			if (pageControl.Item2.ID > high || pageControl.Item2.ID < low)
			{
				if ((sortOrder == SortOrder.Ascending && pageControl.Item2.ID > high) || (sortOrder == SortOrder.Descending && pageControl.Item2.ID < low))
				{
					break;
				}
				continue;
			}
			string value = string.Empty;
			string value2 = string.Empty;
			if (int.TryParse(pageControl.Item2.DataColumnName, out var result))
			{
				if (tableNo > 0)
				{
					if (metaTable == null)
					{
						metaTable = MetadataProvider.GetTableMetadata(tableNo);
					}
					MetaField fieldsById = metaTable.GetFieldsById(result);
					if (fieldsById != null)
					{
						value = ((fieldsById.Type == NavType.Option) ? fieldsById.OptionString : string.Empty);
						value2 = fieldsById.Name;
					}
				}
			}
			else
			{
				DataFieldDefinition dataFieldDefinition = page.Expressions.FirstOrDefault((DataFieldDefinition x) => x.Name == pageControl.Item2.DataColumnName);
				if (dataFieldDefinition != null)
				{
					value2 = dataFieldDefinition.SourceExpression;
					if (dataFieldDefinition.Datatype == DataType.Option)
					{
						value = dataFieldDefinition.OptionString;
					}
				}
			}
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, pageNo, NavInteger.Create(pageControl.Item2.ID), NavText.Create(pageControl.Item2.Name), NavInteger.Create(tableNo), NavInteger.Create(result), NavText.Create(pageControl.Item2.Enabled), NavText.Create(pageControl.Item2.Editable), NavText.Create(pageControl.Item2.Visible), NavText.Create(value2), NavText.Create(value), NavInteger.Create(pageControl.Item1));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}
}
