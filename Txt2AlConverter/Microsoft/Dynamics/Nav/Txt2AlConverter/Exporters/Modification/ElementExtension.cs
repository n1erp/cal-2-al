using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

public static class ElementExtension
{
	public static ChangeTypeKind GetChangeType(this IElement element)
	{
		return AsChangeTypeKinds(element.GetStringProperty(PropertyType.ChangeType));
	}

	public static string GetChangeTypeText(this IElement element)
	{
		return element.GetChangeType() switch
		{
			ChangeTypeKind.Insertion => "Insertion", 
			ChangeTypeKind.Deletion => "Deletion", 
			ChangeTypeKind.PropertyInsertion => "Property Insertion", 
			ChangeTypeKind.PropertyModification => "Property Modification", 
			ChangeTypeKind.PropertyDeletion => "Property Deletion", 
			ChangeTypeKind.CodeModification => "Code Modification", 
			ChangeTypeKind.Move => "Move", 
			_ => string.Empty, 
		};
	}

	private static ChangeTypeKind AsChangeTypeKinds(string s)
	{
		return s?.ToLower() switch
		{
			"insertion" => ChangeTypeKind.Insertion, 
			"deletion" => ChangeTypeKind.Deletion, 
			"propertyinsertion" => ChangeTypeKind.PropertyInsertion, 
			"propertymodification" => ChangeTypeKind.PropertyModification, 
			"propertydeletion" => ChangeTypeKind.PropertyDeletion, 
			"codemodification" => ChangeTypeKind.CodeModification, 
			"move" => ChangeTypeKind.Move, 
			_ => ChangeTypeKind.Unkown, 
		};
	}
}
