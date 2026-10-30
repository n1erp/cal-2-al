using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavQueryForm : NavForm
{
	private NCLMetaQuery nclMetaQuery;

	private readonly Dictionary<int, string> fieldDecimalPlaces = new Dictionary<int, string>();

	internal int[] FormatFields => fieldDecimalPlaces.Keys.ToArray();

	internal Dictionary<int, string> FieldDecimalPlaces => fieldDecimalPlaces;

	internal NCLMetaQuery NclMetaQuery => nclMetaQuery;

	public NavQueryForm(ITreeObject parent, MasterPage masterPage, NCLMetaTable nclMetaTable)
		: base(parent, masterPage, nclMetaTable)
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		CallInitializeComponentExtensionMethod();
		InitializeForm();
	}

	protected override void OnMetadataLoaded()
	{
		CallOnMetadataLoadedExtensionMethod();
		if (nclMetaQuery == null)
		{
			nclMetaQuery = NavGlobal.NCLMetadata.GetMetaQueryById(MasterPage.ID);
		}
		if (InitSourceExpressions() == 0)
		{
			return;
		}
		RepeaterDefinition repeater = Repeater;
		foreach (ControlBaseDefinition control2 in repeater.Controls)
		{
			ControlDefinition control = control2 as ControlDefinition;
			if (control == null)
			{
				continue;
			}
			NCLMetaField nCLMetaField = Array.Find(QueryMetaTable.AllFields, (NCLMetaField f) => f.ColumnIndex == control.ID && f.FieldNavType == NavType.Decimal);
			if (nCLMetaField != null)
			{
				int fieldId = nCLMetaField.ColumnIndex;
				RegisterSourceExpression(control.FormatColumnName, control.FormatColumnName, typeof(NavOemText), SourceExpressionType.Property, () => GetQueryFormatString(fieldId), null);
			}
		}
	}

	protected override string GetAutoFormatString(int controlId)
	{
		return string.Empty;
	}

	protected override string GetDecimalPlaces(int controlId)
	{
		if (!fieldDecimalPlaces.TryGetValue(controlId, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	internal NavValue GetQueryFormatString(int controlId)
	{
		if (!fieldDecimalPlaces.TryGetValue(controlId, out var value))
		{
			return NavOemText.Empty;
		}
		return ALCompiler.ConvertToDotNetFormatString(value);
	}

	private int InitSourceExpressions()
	{
		IEnumerable<NCLMetaQueryColumn> enumerable = nclMetaQuery.QueryDefinition.DefinedColumns.Where((NCLMetaQueryColumn column) => column.NavType == NavType.Decimal && column.ColumnType == QueryColumnType.Normal);
		Dictionary<int, NavRecord> dictionary = new Dictionary<int, NavRecord>();
		try
		{
			foreach (NCLMetaQueryColumn item in enumerable)
			{
				if (!dictionary.TryGetValue(item.SourceTableField.Parent.TableId, out var value))
				{
					value = item.SourceTableField.Parent.CreateObjectInstance(this);
					try
					{
						dictionary.Add(value.TableID, value);
					}
					catch
					{
						value.Dispose();
						throw;
					}
				}
				string decimalPlaces = value.GetDecimalPlaces(item.SourceTableField.FieldNo);
				fieldDecimalPlaces.Add(item.ColumnIndex + 1, decimalPlaces ?? string.Empty);
			}
		}
		finally
		{
			dictionary.ForEach((KeyValuePair<int, NavRecord> x) =>
			{
				x.Value.Dispose();
			});
			dictionary.Clear();
		}
		return fieldDecimalPlaces.Count;
	}
}
