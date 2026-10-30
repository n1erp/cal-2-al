using System;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Resources;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlFieldAndChanges
{
	private readonly NCLMetaField oldField;

	private readonly FieldChangeTypes changes;

	private readonly NCLMetaField newField;

	public NCLMetaField OldField => oldField;

	public FieldChangeTypes Changes => changes;

	public NCLMetaField NewField => newField;

	public bool NoChange => changes == FieldChangeTypes.None;

	public bool Excessive => (changes & FieldChangeTypes.Excessive) != 0;

	public bool Missing => (changes & FieldChangeTypes.Missing) != 0;

	public bool NameChanged => (changes & FieldChangeTypes.Name) != 0;

	public bool AutoIncrementChanged => (changes & FieldChangeTypes.AutoIncrement) != 0;

	public bool DataTypeChanged => (changes & FieldChangeTypes.DataType) != 0;

	public bool SqlDataTypeChanged => (changes & FieldChangeTypes.SqlDataType) != 0;

	public bool CompressionChanged => (changes & FieldChangeTypes.Compression) != 0;

	public bool LengthChanged => (changes & FieldChangeTypes.Length) != 0;

	public bool SortingChanged => (changes & FieldChangeTypes.Sorting) != 0;

	public bool IsUnsafeChange
	{
		get
		{
			if (!Excessive && !DataTypeChanged && !SqlDataTypeChanged && (!LengthChanged || oldField.FieldDefinedLength <= newField.FieldDefinedLength))
			{
				if (oldField != null && oldField.FieldIsPartOfPrimaryKey)
				{
					return !newField.FieldIsPartOfPrimaryKey;
				}
				return false;
			}
			return true;
		}
	}

	public string FieldChangeTypeDescription
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = changes.ToString().Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				FieldChangeTypes result = (Enum.TryParse<FieldChangeTypes>(text, out result) ? result : FieldChangeTypes.None);
				string text2 = SqlFieldAndChangeDescriptions.ResourceManager.GetString(result.ToString());
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append((!string.IsNullOrEmpty(text2)) ? text2 : text);
			}
			return stringBuilder.ToString();
		}
	}

	public NavSqlFieldAndChanges(NCLMetaField oldField, FieldChangeTypes changeTypes, NCLMetaField newField)
	{
		this.oldField = oldField;
		changes = changeTypes;
		this.newField = newField;
	}
}
