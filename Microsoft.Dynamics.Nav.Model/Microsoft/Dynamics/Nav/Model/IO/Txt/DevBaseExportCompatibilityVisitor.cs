using System;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

internal class DevBaseExportCompatibilityVisitor : ElementVisitor
{
	public DevBaseExportCompatibilityVisitor(IElement root)
		: base(root)
	{
	}

	protected override IElement VisitElement(IElement element)
	{
		base.VisitElement(element);
		if (element.ElementTypeInfo.ElementType == ElementType.Change)
		{
			element.ChangePropertyType(PropertyType.ConflictHint, PropertyType.ResidualHint);
			if (element.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.RdlModification.ToString())
			{
				IElement firstSubElement = element.GetFirstSubElement(ElementType.OriginalCode);
				if (firstSubElement != null)
				{
					element.SetStringProperty(PropertyType.BaselineValue, string.Join(Environment.NewLine, firstSubElement.GetCodeLines()));
					element.RemoveSubElement(firstSubElement);
				}
				IElement firstSubElement2 = element.GetFirstSubElement(ElementType.ModifiedCode);
				if (firstSubElement2 != null)
				{
					element.SetStringProperty(PropertyType.ResultingValue, string.Join(Environment.NewLine, firstSubElement2.GetCodeLines()));
					element.RemoveSubElement(firstSubElement2);
				}
				IElement firstSubElement3 = element.GetFirstSubElement(ElementType.TargetCode);
				if (firstSubElement3 != null)
				{
					element.SetStringProperty(PropertyType.NewBaselineValue, string.Join(Environment.NewLine, firstSubElement3.GetCodeLines()));
					element.RemoveSubElement(firstSubElement3);
				}
				if (firstSubElement == null)
				{
					element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyInsertion.ToString());
				}
				else if (firstSubElement2 == null)
				{
					element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyDeletion.ToString());
				}
				else
				{
					element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyModification.ToString());
				}
				element.SetStringProperty(PropertyType.Property, PropertyType.RdlData.ToString());
				return element;
			}
			element.ChangePropertyType(PropertyType.OriginalValue, PropertyType.BaselineValue);
			element.ChangePropertyType(PropertyType.ModifiedValue, PropertyType.ResultingValue);
			element.ChangePropertyType(PropertyType.TargetValue, PropertyType.NewBaselineValue);
			if (element.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.CodeModification.ToString())
			{
				ChangeElementType(element, ElementType.OriginalCode, ElementType.BaselineCode);
				ChangeElementType(element, ElementType.ModifiedCode, ElementType.ResultingCode);
				ChangeElementType(element, ElementType.TargetCode, ElementType.NewBaselineCode);
			}
			else if (element.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.PropertyModification.ToString() && EnumNameLookup<PropertyType>.StringToEnum(element.GetStringProperty(PropertyType.Property)) == PropertyType.TextConstString)
			{
				element.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(PropertyType.DataType));
				element.SetStringProperty(PropertyType.BaselineValue, Token.TextConst.ToString() + Environment.NewLine + element.GetStringProperty(PropertyType.BaselineValue));
				element.SetStringProperty(PropertyType.ResultingValue, Token.TextConst.ToString() + Environment.NewLine + element.GetStringProperty(PropertyType.ResultingValue));
			}
		}
		return element;
	}

	internal static void ChangeElementType(IElement element, ElementType sourceType, ElementType destinationType)
	{
		IElement firstSubElement = element.GetFirstSubElement(sourceType);
		if (firstSubElement != null)
		{
			IElement element2 = GenericElement.Create(destinationType);
			element2.SetCodeLines(firstSubElement.GetCodeLines());
			element.AddSubElement(element2);
			element.RemoveSubElement(firstSubElement);
		}
	}
}
