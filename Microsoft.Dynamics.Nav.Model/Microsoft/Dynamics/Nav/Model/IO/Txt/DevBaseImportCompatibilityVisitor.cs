using System;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

internal class DevBaseImportCompatibilityVisitor : ElementVisitor
{
	private static readonly string[] NewLineSplit = new string[1] { "\r\n" };

	public DevBaseImportCompatibilityVisitor(IElement root)
		: base(root)
	{
	}

	protected override IElement VisitElement(IElement element)
	{
		base.VisitElement(element);
		if (element.ElementTypeInfo.ElementType == ElementType.Change)
		{
			element.ChangePropertyType(PropertyType.ResidualHint, PropertyType.ConflictHint);
			if (element.GetStringProperty(PropertyType.Property) == PropertyType.Attributes.ToString())
			{
				ConvertAttributeChangeFromDevBase(element, PropertyType.ResultingValue);
				ConvertAttributeChangeFromDevBase(element, PropertyType.BaselineValue);
			}
			if (element.GetStringProperty(PropertyType.Property) == PropertyType.RdlData.ToString())
			{
				if (element.GetPropertySetState(PropertyType.BaselineValue))
				{
					ComputeChanges.AddCodeElement(element, ElementType.OriginalCode, element.GetStringProperty(PropertyType.BaselineValue).Split(NewLineSplit, StringSplitOptions.None));
					element.ClearProperty(PropertyType.BaselineValue);
				}
				if (element.GetPropertySetState(PropertyType.ResultingValue))
				{
					ComputeChanges.AddCodeElement(element, ElementType.ModifiedCode, element.GetStringProperty(PropertyType.ResultingValue).Split(NewLineSplit, StringSplitOptions.None));
					element.ClearProperty(PropertyType.ResultingValue);
				}
				if (element.GetPropertySetState(PropertyType.NewBaselineValue))
				{
					ComputeChanges.AddCodeElement(element, ElementType.TargetCode, element.GetStringProperty(PropertyType.NewBaselineValue).Split(NewLineSplit, StringSplitOptions.None));
					element.ClearProperty(PropertyType.NewBaselineValue);
				}
				element.ClearProperty(PropertyType.Property);
				element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.RdlModification.ToString());
				return element;
			}
			element.ChangePropertyType(PropertyType.BaselineValue, PropertyType.OriginalValue);
			element.ChangePropertyType(PropertyType.ResultingValue, PropertyType.ModifiedValue);
			element.ChangePropertyType(PropertyType.NewBaselineValue, PropertyType.TargetValue);
			string stringProperty = element.GetStringProperty(PropertyType.ChangeType);
			if (stringProperty == ChangeTypes.CodeModification.ToString())
			{
				DevBaseExportCompatibilityVisitor.ChangeElementType(element, ElementType.BaselineCode, ElementType.OriginalCode);
				DevBaseExportCompatibilityVisitor.ChangeElementType(element, ElementType.ResultingCode, ElementType.ModifiedCode);
				DevBaseExportCompatibilityVisitor.ChangeElementType(element, ElementType.NewBaselineCode, ElementType.TargetCode);
			}
			else if (stringProperty == ChangeTypes.PropertyModification.ToString() && EnumNameLookup<PropertyType>.StringToEnum(element.GetStringProperty(PropertyType.Property)) == PropertyType.DataType)
			{
				string stringProperty2 = element.GetStringProperty(PropertyType.OriginalValue);
				string stringProperty3 = element.GetStringProperty(PropertyType.ModifiedValue);
				if (stringProperty2.StartsWith(Token.TextConst.ToString(), StringComparison.Ordinal) && stringProperty3.StartsWith(Token.TextConst.ToString(), StringComparison.Ordinal))
				{
					element.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(PropertyType.TextConstString));
					element.SetStringProperty(PropertyType.OriginalValue, TrimToTextConstString(stringProperty2));
					element.SetStringProperty(PropertyType.ModifiedValue, TrimToTextConstString(stringProperty3));
				}
			}
		}
		return element;
	}

	private static void ConvertAttributeChangeFromDevBase(IElement element, PropertyType propertyType)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string stringProperty = element.GetStringProperty(propertyType);
		if (stringProperty != null)
		{
			string[] array = stringProperty.Split(NewLineSplit, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				stringBuilder.Append("[" + text + "]");
			}
			element.SetStringProperty(propertyType, stringBuilder.ToString());
		}
	}

	private static string TrimToTextConstString(string value)
	{
		int i;
		for (i = Token.TextConst.ToString().Length; i < value.Length && char.IsWhiteSpace(value[i]); i++)
		{
		}
		return value.Substring(i);
	}
}
