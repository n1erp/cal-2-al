using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTableForm : NavForm
{
	public NavTableForm(ITreeObject parent, MasterPage masterPage, MetaTable metaTable)
		: base(parent, masterPage)
	{
		RepeaterDefinition repeaterDefinition = masterPage.ContentArea.Find((ElementDefinition elementDefinition) => elementDefinition is RepeaterDefinition) as RepeaterDefinition;
		foreach (ControlDefinition control in repeaterDefinition.Controls)
		{
			MetaField metaField = metaTable.Fields.Find((MetaField f) => f.Id == control.ID);
			if (metaField.Type == NavType.Decimal)
			{
				int fieldId = metaField.Id;
				int controlId = control.ID;
				RegisterSourceExpression(control.FormatColumnName, control.FormatColumnName, typeof(NavOemText), SourceExpressionType.Property, () => ALCompiler.ConvertToDotNetFormatString(GetDecimalString(SourceTable, fieldId, controlId)), null);
			}
		}
	}

	protected override string GetAutoFormatString(int controlId)
	{
		return string.Empty;
	}

	protected override string GetDecimalPlaces(int controlId)
	{
		return string.Empty;
	}
}
