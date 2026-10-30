namespace Microsoft.Dynamics.Nav.Runtime;

internal static class CueStyleHelper
{
	internal static bool UseCodeunitTriggerForControl(int controlId, NavFormMetadataHelper metadataHelper)
	{
		if (!metadataHelper.TryGetControlDefinitionById(controlId, out var result))
		{
			return false;
		}
		return string.IsNullOrEmpty(result.StyleExpr);
	}

	internal static bool TryGetValueAsDecimal(NavRecord navRecord, int fieldNo, out NavDecimal valueAsDecimal)
	{
		NavValue fieldValue = navRecord.GetFieldValue(fieldNo);
		valueAsDecimal = fieldValue as NavDecimal;
		if ((object)valueAsDecimal == null)
		{
			if (!(fieldValue is NavInteger navInteger))
			{
				return false;
			}
			valueAsDecimal = NavDecimal.Create(navInteger.Value);
		}
		return true;
	}
}
