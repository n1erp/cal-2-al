using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavTypeDebuggerView
{
	[DebuggerDisplay("{fieldValue}", Name = "{fieldName}")]
	public struct FieldInfo(string fieldName, string fieldValue, string fieldType, int fieldNo)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string fieldName = fieldName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string fieldValue = fieldValue;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string fieldType = fieldType;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int fieldNo = fieldNo;

		public string FieldName => fieldName;

		public string FieldValue => fieldValue;

		public string FieldType => fieldType;

		public int FieldNo => fieldNo;
	}

	[DebuggerDisplay("{sorting}", Name = "{key}")]
	public struct KeyInfo(string key, string sorting)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string key = key;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string sorting = sorting;

		public string Key => key;

		public string SortOrder => sorting;
	}

	[DebuggerDisplay("{filterValue}", Name = "{fieldName}")]
	public struct FilterInfo(string fieldName, string filterValue)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string filterValue = filterValue;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string fieldName = fieldName;

		public string Field => fieldName;

		public string Filter => filterValue;
	}

	public struct FilterGroupInfo(int filterGroup)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int filterGroup = filterGroup;

		public int FilterGroup => filterGroup;
	}

	public struct TableInfo(string tableName)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string tableName = tableName;

		public string TableName => tableName;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private NavRecord viewedObject;

	public void SetViewedObject(NavRecord navRecord)
	{
		viewedObject = navRecord;
	}

	public NavRecord GetViewedObject()
	{
		return viewedObject;
	}

	public FilterGroupInfo GetFilterGroupProperties()
	{
		return new FilterGroupInfo(viewedObject.ALFilterGroup);
	}

	public TableInfo GetTableInfo()
	{
		return new TableInfo(viewedObject.ALTableName);
	}

	public ReadOnlyCollection<FilterInfo> GetFiltersProperties()
	{
		List<FilterInfo> list = new List<FilterInfo>();
		foreach (NCLMetaField field in viewedObject.GetNCLMetaTable().Fields)
		{
			if (field.FieldIndex > 0 && !string.IsNullOrEmpty(viewedObject.ALGetFilter(field.FieldNo)))
			{
				list.Add(new FilterInfo(field.FieldName, viewedObject.ALGetFilter(field.FieldNo)));
			}
		}
		return list.AsReadOnly();
	}

	public ReadOnlyCollection<FieldInfo> GetFieldInfo()
	{
		List<FieldInfo> list = new List<FieldInfo>();
		foreach (NCLMetaField field in viewedObject.MetaTable.Fields)
		{
			if (field.FieldIndex > 0)
			{
				list.Add(new FieldInfo(field.FieldName, viewedObject.GetFieldValue(field.FieldNo).ToString(), field.FieldNavType.ToString(), field.FieldNo));
			}
		}
		return list.AsReadOnly();
	}

	public ReadOnlyCollection<KeyInfo> GetKeysProperties()
	{
		int keyCount = viewedObject.MetaTable.KeyCount;
		List<KeyInfo> list = new List<KeyInfo>();
		string text = (viewedObject.ALAscending ? Lang.SortingTypeAscending : Lang.SortingTypeDescending);
		for (int i = 0; i < keyCount; i++)
		{
			NCLMetaKey keyByIndex = viewedObject.MetaTable.GetKeyByIndex(i);
			StringBuilder stringBuilder = new StringBuilder(keyByIndex.GetKeyFieldByIndex(0).FieldName);
			for (int j = 1; j < keyByIndex.KeyFieldCount; j++)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ",{0}", keyByIndex.GetKeyFieldByIndex(j).FieldName);
			}
			list.Add(new KeyInfo(stringBuilder.ToString(), (keyByIndex.GetKeyName() == viewedObject.ALCurrentKey) ? text : string.Empty));
		}
		return list.AsReadOnly();
	}
}
