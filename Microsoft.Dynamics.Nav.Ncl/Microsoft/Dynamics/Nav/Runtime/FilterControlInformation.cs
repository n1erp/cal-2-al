using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FilterControlInformation
{
	private readonly List<int> fields = new List<int>();

	private string defaultView;

	public NavRecordRef RecordReference { get; set; }

	public string VariableName { get; set; }

	public IList<int> Fields => fields;

	public FilterControlInformation(string name, NavRecordRef navRecord)
	{
		VariableName = name;
		RecordReference = navRecord;
		defaultView = navRecord.ALGetView();
	}

	private FilterControlInformation(ITreeObject parent, FilterControlInformation other)
	{
		VariableName = other.VariableName;
		RecordReference = other.RecordReference.Clone(parent);
		defaultView = other.defaultView;
		fields.AddRange(other.fields);
	}

	public void AddFilterField(int fieldNumber)
	{
		if (!fields.Contains(fieldNumber))
		{
			fields.Add(fieldNumber);
		}
	}

	public string FilterFields()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (int field in fields)
		{
			if (!flag)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(field.ToString(NavCurrentThread.Session.RuntimeLanguage));
			flag = false;
		}
		return stringBuilder.ToString();
	}

	public void ResetView()
	{
		if (!string.IsNullOrEmpty(defaultView))
		{
			RecordReference.ALSetView(defaultView);
		}
	}

	public FilterControlInformation Clone(ITreeObject parent)
	{
		return new FilterControlInformation(parent, this);
	}

	public void Clear()
	{
		VariableName = string.Empty;
		if (RecordReference != null)
		{
			RecordReference.Clear();
		}
		defaultView = string.Empty;
		fields.Clear();
	}
}
