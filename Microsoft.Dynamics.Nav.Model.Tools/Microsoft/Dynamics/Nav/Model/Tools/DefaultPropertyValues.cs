using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class DefaultPropertyValues
{
	internal static string GetDefaultPropertyValue(PropertyType type)
	{
		return type switch
		{
			PropertyType.TableType => "Normal", 
			PropertyType.LinkedObject => "No", 
			PropertyType.PasteIsValid => "Yes", 
			PropertyType.DataPerCompany => "Yes", 
			PropertyType.Enabled => "Yes", 
			PropertyType.FieldClass => "Normal", 
			PropertyType.Editable => "Yes", 
			PropertyType.NotBlank => "No", 
			PropertyType.Numeric => "No", 
			PropertyType.DateFormula => "No", 
			PropertyType.ValidateTableRelation => "Yes", 
			PropertyType.TestTableRelation => "Yes", 
			PropertyType.ExtendedDataType => "None", 
			PropertyType.ObsoleteState => "No", 
			PropertyType.Unique => "No", 
			_ => null, 
		};
	}
}
