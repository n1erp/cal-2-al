using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFilterPageBuilder : NavComplexValue, ICloneable
{
	public const string DataItemTableViewIndexFormat = "DataItem{0}TableView";

	private readonly OrderedDictionary recordLookup = new OrderedDictionary();

	private readonly List<NavRecordRef> indexedRecordList = new List<NavRecordRef>();

	private readonly ITreeObject scope;

	private bool instanceHasRun;

	private string pageCaption;

	public int ALCount => recordLookup.Count;

	public string ALPageCaption
	{
		get
		{
			if (string.IsNullOrEmpty(pageCaption))
			{
				return Lang.FilterPageBuilderCaption;
			}
			return pageCaption;
		}
		set
		{
			pageCaption = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavFilterPageBuilder;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => recordLookup.Count == 0;

	public NavText ALAddTable(DataError errorLevel, string name, int tableId)
	{
		return ALAddTable(errorLevel, new NavText(name), tableId);
	}

	public NavText ALAddTable(DataError errorLevel, NavText name, int tableId)
	{
		try
		{
			FilterControlInformation filterControlInformation = AddTable(name, tableId);
			return new NavText(filterControlInformation.VariableName);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return NavText.Empty;
		}
	}

	public NavText ALAddRecord(DataError errorLevel, string name, NavRecord record)
	{
		return ALAddRecord(errorLevel, new NavText(name), record);
	}

	public NavText ALAddRecord(DataError errorLevel, NavText name, NavRecord record)
	{
		try
		{
			FilterControlInformation filterControlInformation = AddTable(name, record);
			return new NavText(filterControlInformation.VariableName);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return NavText.Empty;
		}
	}

	public NavText ALAddRecordRef(DataError errorLevel, string name, NavRecordRef recordRef)
	{
		return ALAddRecordRef(errorLevel, new NavText(name), recordRef);
	}

	public NavText ALAddRecordRef(DataError errorLevel, NavText name, NavRecordRef recordRef)
	{
		try
		{
			FilterControlInformation filterControlInformation = AddTable(name, recordRef);
			return new NavText(filterControlInformation.VariableName);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return NavText.Empty;
		}
	}

	public bool ALAddField(DataError errorLevel, string name, int fieldNo)
	{
		return ALAddField(errorLevel, new NavText(name), fieldNo);
	}

	public bool ALAddField(DataError errorLevel, NavText name, int fieldNo)
	{
		return ALAddField(errorLevel, name, fieldNo, null);
	}

	public bool ALAddField(DataError errorLevel, string name, int fieldNo, string defaultFilter)
	{
		return ALAddField(errorLevel, new NavText(name), fieldNo, new NavText(defaultFilter));
	}

	public bool ALAddField(DataError errorLevel, string name, int fieldNo, NavText defaultFilter)
	{
		return ALAddField(errorLevel, new NavText(name), fieldNo, defaultFilter);
	}

	public bool ALAddField(DataError errorLevel, NavText name, int fieldNo, string defaultFilter)
	{
		return ALAddField(errorLevel, name, fieldNo, new NavText(defaultFilter));
	}

	public bool ALAddField(DataError errorLevel, NavText name, int fieldNo, NavText defaultFilter)
	{
		return ALAddFieldNo(errorLevel, name, fieldNo, defaultFilter);
	}

	internal bool ALAddFieldNo(DataError errorLevel, NavText name, int fieldNo, NavText defaultFilter)
	{
		try
		{
			return AddFilter(name, fieldNo, defaultFilter);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public NavText ALGetView(NavText name)
	{
		return ALGetView(name, useCaptions: true);
	}

	public NavText ALGetView(string name)
	{
		return ALGetView(new NavText(name), useCaptions: true);
	}

	public NavText ALGetView(string name, bool useCaptions)
	{
		return ALGetView(new NavText(name), useCaptions);
	}

	public NavText ALGetView(NavText name, bool useCaptions)
	{
		FilterControlInformation filterControlInformation = FindRecord(name);
		if (filterControlInformation != null)
		{
			return new NavText(int.MaxValue, filterControlInformation.RecordReference.ALGetView(useCaptions));
		}
		return NavText.Empty;
	}

	public bool ALSetView(DataError errorLevel, string name, string view)
	{
		return ALSetView(errorLevel, new NavText(name), new NavText(view));
	}

	public bool ALSetView(DataError errorLevel, string name, NavText view)
	{
		return ALSetView(errorLevel, new NavText(name), view);
	}

	public bool ALSetView(DataError errorLevel, NavText name, string view)
	{
		return ALSetView(errorLevel, name, new NavText(view));
	}

	public bool ALSetView(DataError errorLevel, NavText name, NavText view)
	{
		try
		{
			if (string.IsNullOrEmpty(view))
			{
				throw new NavNCLArgumentException(Lang.FilterPageInvalidView);
			}
			FilterControlInformation filterControlInformation = FindFilterControlInformation(name);
			filterControlInformation.RecordReference.ALSetView(view);
			return true;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public bool ALRunModal(DataError errorLevel)
	{
		try
		{
			return RunModal(scope);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public void ALAssign(NavFilterPageBuilder other)
	{
		Clone(other, scope);
	}

	public NavText ALName(int index)
	{
		if (index < 1 || index > recordLookup.Count)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang.FilterControlIndexOutOfRange, index, 1, recordLookup.Count));
		}
		FilterControlInformation filterControlInformation = (FilterControlInformation)recordLookup[index - 1];
		if (filterControlInformation != null)
		{
			return new NavText(filterControlInformation.VariableName);
		}
		return NavText.Empty;
	}

	public NavFilterPageBuilder(ITreeObject parent)
		: base(parent)
	{
		scope = parent;
	}

	public NavFilterPageBuilder(NavFilterPageBuilder other, ITreeObject parent = null)
		: base(parent ?? other.scope)
	{
		scope = parent ?? other.scope;
		Clone(other, scope);
	}

	private void Clone(NavFilterPageBuilder other, ITreeObject parent)
	{
		indexedRecordList.Clear();
		recordLookup.Clear();
		foreach (NavRecordRef indexedRecord in other.indexedRecordList)
		{
			indexedRecordList.Add(indexedRecord.Clone(parent));
		}
		OrderedDictionary orderedDictionary = other.recordLookup.AsReadOnly();
		foreach (DictionaryEntry item in orderedDictionary)
		{
			string key = (string)item.Key;
			FilterControlInformation filterControlInformation = (FilterControlInformation)item.Value;
			recordLookup.Add(key, filterControlInformation.Clone(parent));
		}
	}

	internal FilterControlInformation AddTable(string name, int tableNo)
	{
		if (tableNo < 1)
		{
			throw new NavNCLArgumentOutOfRangeException(Lang.FilterPageTableIdMustBePositive);
		}
		FilterControlInformation filterControlInformation = FindRecord(name);
		if (filterControlInformation != null)
		{
			ValidateExistingTableNumber(filterControlInformation, tableNo);
			return filterControlInformation;
		}
		NavRecordRef navRecordRef = new NavRecordRef(scope, SecurityFiltering.Validated);
		navRecordRef.ALOpen(tableNo);
		return AddFilterControlInformation(name, navRecordRef);
	}

	internal FilterControlInformation AddTable(string name, NavRecord record)
	{
		FilterControlInformation filterControlInformation = FindRecord(name);
		if (filterControlInformation != null)
		{
			ValidateExistingTableNumber(filterControlInformation, record.TableID);
			return filterControlInformation;
		}
		NavRecordRef navRecordRef = new NavRecordRef(scope, SecurityFiltering.Validated);
		navRecordRef.ALGetTable(record);
		return AddFilterControlInformation(name, navRecordRef);
	}

	internal FilterControlInformation AddTable(string name, NavRecordRef recordRef)
	{
		FilterControlInformation filterControlInformation = FindRecord(name);
		if (filterControlInformation != null)
		{
			ValidateExistingTableNumber(filterControlInformation, recordRef.ALNumber);
			return filterControlInformation;
		}
		return AddFilterControlInformation(name, recordRef);
	}

	private static void ValidateExistingTableNumber(FilterControlInformation info, int tableNo)
	{
		int aLNumber = info.RecordReference.ALNumber;
		if (aLNumber != tableNo)
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.FilterPageControlNameRedefinition, info.VariableName));
		}
	}

	private FilterControlInformation AddFilterControlInformation(string name, NavRecordRef recordRef)
	{
		indexedRecordList.Add(recordRef);
		FilterControlInformation filterControlInformation = new FilterControlInformation(name, recordRef);
		recordLookup.Add(name, filterControlInformation);
		return filterControlInformation;
	}

	private bool AddFilter(string name, int fieldNo, string defaultFilter)
	{
		FilterControlInformation filterControlInformation = FindFilterControlInformation(name);
		if (filterControlInformation != null)
		{
			if (defaultFilter != null)
			{
				NavFieldRef navFieldRef = filterControlInformation.RecordReference.ALField(scope, fieldNo);
				navFieldRef.ALSetFilter(defaultFilter);
			}
			filterControlInformation.AddFilterField(fieldNo);
			return true;
		}
		return false;
	}

	private FilterControlInformation FindFilterControlInformation(string name)
	{
		FilterControlInformation filterControlInformation = FindRecord(name);
		if (filterControlInformation == null)
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.FilterControlNotFound, name));
		}
		return filterControlInformation;
	}

	internal FilterControlInformation FindRecord(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new NavNCLArgumentException(Lang.FilterPageInvalidTableName);
		}
		if (recordLookup.Contains(name))
		{
			return (FilterControlInformation)recordLookup[name];
		}
		return null;
	}

	internal bool RunModal(ITreeObject parent)
	{
		if (recordLookup.Count < 1)
		{
			return false;
		}
		if (instanceHasRun)
		{
			ResetFilters();
		}
		PageDefinition pageDefinition = BuildFilterPageDefinition(9598, ALPageCaption, recordLookup);
		MasterPage filterPageForRecord = NavCurrentThread.Session.MetadataProvider.GetFilterPageForRecord(pageDefinition, indexedRecordList);
		FormResult formResult;
		using (FilterPage filterPage = new FilterPage(parent, filterPageForRecord, indexedRecordList))
		{
			formResult = filterPage.RunModal();
			instanceHasRun |= formResult == FormResult.OK;
		}
		return formResult == FormResult.OK;
	}

	private void ResetFilters()
	{
		foreach (DictionaryEntry item in recordLookup)
		{
			((FilterControlInformation)item.Value).ResetView();
		}
	}

	internal static PageDefinition BuildFilterPageDefinition(int pageId, string caption, OrderedDictionary records)
	{
		PageDefinition pageDefinition = new PageDefinition
		{
			ID = pageId,
			Name = caption,
			Caption = caption,
			Properties = new PageProperties
			{
				SourceObject = new SourceObjectDefinition(),
				PageType = PageType.FilterPage,
				ControlGUID = Guid.NewGuid().ToString()
			},
			Content = new ContentDefinition
			{
				Containers = new List<ControlContainerDefinition>()
			}
		};
		ControlContainerDefinition controlContainerDefinition = new ControlContainerDefinition
		{
			ContainerType = ControlContainerType.RequestPageFilters,
			Controls = new List<ControlBaseDefinition>()
		};
		pageDefinition.Content.Containers.Add(controlContainerDefinition);
		pageDefinition.Expressions = new List<DataFieldDefinition>();
		int num = 1;
		int num2 = 0;
		ICollection keys = records.Keys;
		foreach (object item3 in keys)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "DataItem{0}TableView", num2++);
			ControlGroupDefinition controlGroupDefinition = new ControlGroupDefinition
			{
				ID = num++,
				ControlGUID = Guid.NewGuid().ToString(),
				Caption = (string)item3,
				Controls = new List<ControlBaseDefinition>()
			};
			controlContainerDefinition.Controls.Add(controlGroupDefinition);
			string text2 = ((FilterControlInformation)records[item3]).FilterFields();
			FilterControlDefinition item = new FilterControlDefinition
			{
				ID = num++,
				ControlGUID = Guid.NewGuid().ToString(),
				DataColumnName = text,
				FilterTableID = ((FilterControlInformation)records[item3]).RecordReference.ALNumber,
				ReqFilterFields = text2,
				Sorting = !string.IsNullOrEmpty(text2)
			};
			controlGroupDefinition.Controls.Add(item);
			DataFieldDefinition item2 = new DataFieldDefinition
			{
				Name = text,
				SourceExpression = text,
				ExpressionType = SourceExpressionType.DataItem,
				Datatype = DataType.Binary
			};
			pageDefinition.Expressions.Add(item2);
		}
		return pageDefinition;
	}

	public override void Clear()
	{
		foreach (FilterControlInformation item in recordLookup.Cast<DictionaryEntry>().Select((DictionaryEntry dictionaryEntry) =>
		{
			DictionaryEntry dictionaryEntry2 = dictionaryEntry;
			return (FilterControlInformation)dictionaryEntry2.Value;
		}))
		{
			item.Clear();
		}
		recordLookup.Clear();
		indexedRecordList.Clear();
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public object Clone()
	{
		return new NavFilterPageBuilder(this);
	}

	public override string ToString(bool invariant)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ICollection keys = recordLookup.Keys;
		bool flag = true;
		foreach (string item in keys)
		{
			if (!flag)
			{
				stringBuilder.Append(',');
			}
			else
			{
				flag = false;
			}
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}
}
