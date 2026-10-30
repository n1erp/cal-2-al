using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFilterData
{
	private string dataItemName;

	private string fieldName;

	private string filterValue;

	private int? fieldId;

	private DataItem dataItem;

	private static readonly Regex tableNumberRegex = new Regex("^Table(\\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public string FilterValue => filterValue;

	public int FieldId
	{
		get
		{
			if (!fieldId.HasValue)
			{
				throw new InvalidOperationException("TODO: Add message");
			}
			return fieldId.Value;
		}
	}

	public DataItem DataItem => dataItem;

	private NavFilterData(string dataItemName, string fieldName, string filterValue)
	{
		this.dataItemName = dataItemName;
		this.fieldName = fieldName;
		this.filterValue = filterValue;
	}

	public static NavFilterData GetFilterData(string textualFilter)
	{
		string[] array = textualFilter.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 2)
		{
			throw new NavNCLInvalidFilterDataException(Lang.InvalidFilterString);
		}
		string text = Uri.UnescapeDataString(array[1]);
		string text2 = Uri.UnescapeDataString(array[0]);
		if (text[0] == '"' && text[text.Length - 1] == '"')
		{
			text = text.Substring(1, text.Length - 2);
		}
		string text3;
		string stringToUnescape;
		if (text2[text2.Length - 1] == '"')
		{
			text2 = text2.Substring(0, text2.Length - 1);
			int num = text2.LastIndexOf('"');
			if (num <= 0)
			{
				throw new NavNCLInvalidFilterDataException(Lang.InvalidFilterString);
			}
			text3 = text2.Substring(0, num - 1);
			stringToUnescape = text2.Substring(num + 1);
		}
		else
		{
			int num = text2.LastIndexOf('.');
			if (num <= 0)
			{
				throw new NavNCLInvalidFilterDataException(Lang.InvalidFilterString);
			}
			text3 = text2.Substring(0, num);
			stringToUnescape = text2.Substring(num + 1);
		}
		if (text3[0] == '"' && text3[text3.Length - 1] == '"')
		{
			text3 = text3.Substring(1, text3.Length - 2);
		}
		return new NavFilterData(Uri.UnescapeDataString(text3), Uri.UnescapeDataString(stringToUnescape), text);
	}

	public void ValidateForDataItemIterator(DataItemIterator dataItemIterator)
	{
		if (fieldId.HasValue && dataItem != null)
		{
			return;
		}
		Match match = tableNumberRegex.Match(dataItemName);
		int num = (match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : 0);
		ICollection<DataItem> dataItems = dataItemIterator.DataItems;
		foreach (DataItem item in dataItems)
		{
			if (string.Compare(item.MetaData.DataItemVarName, dataItemName, StringComparison.CurrentCultureIgnoreCase) == 0 || item.MetaData.DataItemTable == num)
			{
				dataItem = item;
				break;
			}
		}
		if (dataItem == null)
		{
			throw new NavNCLInvalidFilterDataException(Lang.InvalidFilterString);
		}
		try
		{
			fieldId = dataItem.Record.MetaTable.FindFieldMatch(fieldName).FieldNo;
		}
		catch (NavNCLFieldNotFoundException innerException)
		{
			throw new NavNCLInvalidFilterDataException(Lang.InvalidFilterString, innerException);
		}
	}
}
