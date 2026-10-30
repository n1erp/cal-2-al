using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class VersionChecker
{
	internal static void CheckPropertyModificationAvailability(ModificationKind modificationKind, PropertyTypeInfo info, PropertySyntax propertySyntax, DiagnosticBag diagnostics)
	{
		if (info == null)
		{
			return;
		}
		VersionCompatibility versionCompatibility = ((modificationKind == ModificationKind.Extensions) ? info.Kind.GetExtensionModificationCompatibility() : info.Kind.GetCustomizationModificationCompatibility());
		if (versionCompatibility != null && !(propertySyntax?.SyntaxTree.Options == null))
		{
			Version runtimeVersion = propertySyntax.SyntaxTree.Options.RuntimeVersion;
			if (!versionCompatibility.IsSupported(runtimeVersion))
			{
				diagnostics.Add(ErrorCode.ERR_PropertyCustomizationNotSupportedByRuntime, propertySyntax.Name.GetLocation(), info.Name, runtimeVersion.ToMajorMinorString(), modificationKind.ToString().ToLower(CultureInfo.InvariantCulture), versionCompatibility);
			}
		}
	}

	internal static void CheckPropertyEnumValueAvailability(EnumPropertyTypeInfo info, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		if (info == null)
		{
			return;
		}
		string propertyValueText = propertyValueSyntax.GetPropertyValueText();
		VersionCompatibility propertyEnumAvailability = info.Kind.GetPropertyEnumAvailability(propertyValueText);
		if (propertyEnumAvailability != null && !(propertyValueSyntax?.SyntaxTree.Options == null))
		{
			Version runtimeVersion = propertyValueSyntax.SyntaxTree.Options.RuntimeVersion;
			if (!propertyEnumAvailability.IsSupported(runtimeVersion))
			{
				diagnostics.Add(ErrorCode.ERR_EnumPropertyValueNotSupportedByRuntime, propertyValueSyntax.GetLocation(), propertyValueText, info.Name, runtimeVersion.ToMajorMinorString(), propertyEnumAvailability);
			}
		}
	}

	private static VersionCompatibility GetPropertyEnumAvailability(this PropertyKind kind, string enumPropertyValue)
	{
		if (kind == PropertyKind.Importance)
		{
			if (Enum.TryParse<ImportanceKind>(enumPropertyValue, out var result) && result == ImportanceKind.Optional)
			{
				return VersionCompatibility.LowerThanSpring2019;
			}
			return null;
		}
		return null;
	}

	private static VersionCompatibility GetExtensionModificationCompatibility(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.TableRelation:
		case PropertyKind.AssistEdit:
		case PropertyKind.IndentationColumn:
		case PropertyKind.IndentationControls:
			return VersionCompatibility.Spring2019OrGreater;
		case PropertyKind.BlankZero:
		case PropertyKind.Editable:
			return VersionCompatibility.Fall2018CU2OrGreater;
		case PropertyKind.ShowMandatory:
			return VersionCompatibility.Fall2018CU4OrGreater;
		default:
			return null;
		}
	}

	private static VersionCompatibility GetCustomizationModificationCompatibility(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.PromotedActionCategories:
		case PropertyKind.Importance:
		case PropertyKind.QuickEntry:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.PromotedOnly:
		case PropertyKind.PromotedCategory:
			return VersionCompatibility.Spring2019OrGreater;
		default:
			return null;
		}
	}
}
