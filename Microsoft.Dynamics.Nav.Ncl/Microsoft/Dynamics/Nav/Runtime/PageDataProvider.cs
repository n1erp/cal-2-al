using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000138;

	public PageDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int fieldNo = keyField.FieldNo;
		if (fieldNo == 1)
		{
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.Page, range, sortOrder, needNames: false))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					PageDefinition pageDefinition = MetadataProvider.GetPageDefinition(item.Key);
					string text = MultiLanguage.GetLanguageValue(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)), pageDefinition.CaptionML);
					if (string.IsNullOrWhiteSpace(text))
					{
						text = pageDefinition.Name;
					}
					List<NavValue> list = new List<NavValue>
					{
						VirtualDataProvider.VirtualTimeStamp,
						NavInteger.Create(item.Key),
						GetTruncatedTextValue(2, pageDefinition.Name),
						GetTruncatedTextValue(3, text)
					};
					list.AddRange(GetValuesOnPageProperties(pageDefinition.Properties));
					list.AddRange(GetValuesOnSourceObjectProperties(pageDefinition.Properties));
					readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, list.ToArray());
				}
				catch (NavMetadataNotFoundException)
				{
					continue;
				}
				if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
				{
					yield return readOnlyRecordBuffer;
				}
			}
			yield break;
		}
		throw new NotSupportedException();
	}

	private IEnumerable<NavValue> GetValuesOnPageProperties(PageProperties properties)
	{
		List<NavValue> list = new List<NavValue>();
		if (properties == null)
		{
			list.Add(NavBoolean.Default);
			list.Add(NavInteger.Default);
			list.Add(NavInteger.Default);
			list.Add(NavText.Empty);
			list.Add(NavBoolean.Default);
			list.Add(NavText.Empty);
			list.Add(NavText.Empty);
			list.Add(NavText.Empty);
			list.Add(NavText.Empty);
			list.Add(NavText.Empty);
		}
		else
		{
			list.Add(NavBoolean.Create(properties.Editable));
			list.Add(GetOptionValue(5, (int)properties.PageType));
			list.Add(NavInteger.Create(properties.CardFormID));
			list.Add(string.IsNullOrEmpty(properties.DataCaptionExpr) ? NavText.Empty : GetTruncatedTextValue(7, properties.DataCaptionExpr));
			list.Add(NavBoolean.Create(properties.RefreshOnActivate));
			list.Add(string.IsNullOrEmpty(properties.APIPublisher) ? NavText.Empty : GetTruncatedTextValue(9, properties.APIPublisher));
			list.Add(string.IsNullOrEmpty(properties.APIGroup) ? NavText.Empty : GetTruncatedTextValue(10, properties.APIGroup));
			list.Add(string.IsNullOrEmpty(properties.APIVersion) ? NavText.Empty : GetTruncatedTextValue(11, properties.APIVersion));
			list.Add(string.IsNullOrEmpty(properties.EntitySetName) ? NavText.Empty : GetTruncatedTextValue(12, properties.EntitySetName));
			list.Add(string.IsNullOrEmpty(properties.EntityName) ? NavText.Empty : GetTruncatedTextValue(13, properties.EntityName));
		}
		return list;
	}

	private IEnumerable<NavValue> GetValuesOnSourceObjectProperties(PageProperties properties)
	{
		List<NavValue> list = new List<NavValue>();
		if (properties == null || properties.SourceObject == null)
		{
			list.Add(NavInteger.Default);
			list.Add(NavText.Empty);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavText.Empty);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
			list.Add(NavBoolean.Default);
		}
		else
		{
			list.Add(NavInteger.Create(properties.SourceObject.SourceTable));
			list.Add(FormatViewDefinition(properties.SourceObject.SourceTableView));
			list.Add(NavBoolean.Create(properties.SourceObject.InsertAllowed));
			list.Add(NavBoolean.Create(properties.SourceObject.ModifyAllowed));
			list.Add(NavBoolean.Create(properties.SourceObject.DeleteAllowed));
			list.Add(NavBoolean.Create(properties.SourceObject.DelayedInsert));
			list.Add(NavBoolean.Create(properties.SourceObject.ShowFilter));
			list.Add(NavBoolean.Create(properties.SourceObject.MultipleNewLines));
			list.Add(NavBoolean.Create(properties.SourceObject.SaveValues));
			list.Add(NavBoolean.Create(properties.SourceObject.AutoSplitKey));
			list.Add(string.IsNullOrEmpty(properties.SourceObject.DataCaptionFields) ? NavText.Empty : GetTruncatedTextValue(24, properties.SourceObject.DataCaptionFields));
			list.Add(NavBoolean.Create(properties.SourceObject.SourceTableTemporary));
			list.Add(NavBoolean.Create(properties.SourceObject.LinksAllowed));
			list.Add(NavBoolean.Create(properties.ChangeTrackingAllowed));
			list.Add(NavBoolean.Create(properties.SourceObject.PopulateAllFields));
		}
		return list;
	}

	private NavText FormatViewDefinition(ViewDefinition view)
	{
		string fieldValue = GenerateSourceTableViewString(view);
		return GetTruncatedTextValue(15, fieldValue);
	}

	public static string GenerateSourceTableViewString(ViewDefinition view)
	{
		if (view == null || view.Sorting == null)
		{
			return NavText.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (view.Sorting.KeyFields.Length > 0)
		{
			stringBuilder.Append("SORTING(").Append(view.Sorting.KeyFields).Append(") ");
		}
		if (!view.Sorting.Ascending)
		{
			stringBuilder.Append("ORDER(").Append('1').Append(") ");
		}
		if (view.TableFilters != null && view.TableFilters.Count > 0)
		{
			StringBuilder filterExpression = new StringBuilder();
			filterExpression.Join(",", view.TableFilters, (FilterDefinition f) =>
			{
				filterExpression.Append("Field").Append(f.FieldID).Append('=')
					.Append(f.FilterType.ToString())
					.Append('(')
					.Append(f.FilterValue)
					.Append(')');
			});
			if (filterExpression.Length > 0)
			{
				stringBuilder.Append("WHERE(").Append(filterExpression).Append(')');
			}
		}
		return stringBuilder.ToString();
	}
}
