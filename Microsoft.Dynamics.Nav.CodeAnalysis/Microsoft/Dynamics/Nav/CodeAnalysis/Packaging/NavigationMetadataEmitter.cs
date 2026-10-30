using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal static class NavigationMetadataEmitter
{
	private class NavigationMetadataEntry
	{
		internal UsageCategoryKind UsageCategory { get; set; }

		internal int TargetId { get; set; }

		internal int AccessByObjectID { get; set; }

		internal string PermissionMask { get; set; }

		internal string ApplicationArea { get; set; }

		internal string PermissionValueObjectTypeName { get; set; }

		internal string CaptionML { get; set; }

		internal SymbolKind Kind { get; set; }

		internal int SourceTableId { get; set; }

		internal string AdditionalSearchTermsML { get; set; }

		public void ToXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Actions");
			xmlWriter.WriteAttributeString("xsi", "type", null, "ActionDefinition");
			xmlWriter.WriteAttributeString("PushAction", "RunObject");
			xmlWriter.WriteAttributeString("RunObjectType", MapKind());
			xmlWriter.WriteAttributeString("TargetID", TargetId.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("DepartmentCategory", MapUsageCategory());
			xmlWriter.WriteAttributeString("RunObjectSrcTable", SourceTableId.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ControlGUID", Guid.NewGuid().ToString("B"));
			if (!string.IsNullOrEmpty(PermissionValueObjectTypeName))
			{
				xmlWriter.WriteAttributeString("AccessByObjectType", PermissionValueObjectTypeName);
			}
			if (!string.IsNullOrEmpty(CaptionML))
			{
				xmlWriter.WriteAttributeString("CaptionML", CaptionML);
			}
			if (AccessByObjectID != 0)
			{
				xmlWriter.WriteAttributeString("AccessByObjectID", AccessByObjectID.ToString(CultureInfo.InvariantCulture));
			}
			if (!string.IsNullOrEmpty(PermissionMask))
			{
				xmlWriter.WriteAttributeString("AccessByPermissionMask", PermissionMask);
			}
			if (!string.IsNullOrEmpty(ApplicationArea))
			{
				xmlWriter.WriteAttributeString("ApplicationArea", ApplicationArea);
			}
			if (!string.IsNullOrEmpty(AdditionalSearchTermsML))
			{
				xmlWriter.WriteAttributeString("AdditionalSearchTerms", AdditionalSearchTermsML);
			}
			xmlWriter.WriteEndElement();
		}

		private string MapKind()
		{
			if (Kind == SymbolKind.Page || Kind == SymbolKind.Report)
			{
				return Kind.ToString();
			}
			return string.Empty;
		}

		private string MapUsageCategory()
		{
			if (UsageCategory == UsageCategoryKind.ReportsAndAnalysis)
			{
				return "Reports and Analysis";
			}
			if (UsageCategory == UsageCategoryKind.None)
			{
				return string.Empty;
			}
			return UsageCategory.ToString();
		}
	}

	internal static string ToXmlString(IEnumerable<ApplicationObjectTypeSymbol> applicationObjects)
	{
		IEnumerable<NavigationMetadataEntry> enumerable = applicationObjects.Where(CanGenerateNavigationMetadata).Select(CreateNavigationMetadataEntry);
		if (!enumerable.Any())
		{
			return string.Empty;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, ObjectMetadataEmitter.WriterSettings))
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("NavigationDefinition", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
			xmlWriter.WriteStartElement("ActionContainers");
			xmlWriter.WriteAttributeString("ActionContainerType", "Departments");
			xmlWriter.WriteStartElement("Actions");
			WriteActionGroupDefinitionAttributes(xmlWriter);
			foreach (NavigationMetadataEntry item in enumerable)
			{
				item.ToXml(xmlWriter);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private static void WriteActionGroupDefinitionAttributes(XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("xsi", "type", null, "ActionGroupDefinition");
		xmlWriter.WriteAttributeString("Caption", "Extension");
		xmlWriter.WriteAttributeString("ControlGUID", Guid.NewGuid().ToString("B"));
	}

	internal static bool CanGenerateNavigationMetadata(Symbol symbol)
	{
		if (symbol == null || (symbol.Kind != SymbolKind.Page && symbol.Kind != SymbolKind.Report))
		{
			return false;
		}
		UsageCategoryKind? simplePropertyValue = symbol.GetSimplePropertyValue<UsageCategoryKind>(PropertyKind.UsageCategory);
		if (simplePropertyValue.HasValue)
		{
			return simplePropertyValue.Value != UsageCategoryKind.None;
		}
		return false;
	}

	private static NavigationMetadataEntry CreateNavigationMetadataEntry(ApplicationObjectTypeSymbol symbol)
	{
		UsageCategoryKind? simplePropertyValue = symbol.GetSimplePropertyValue<UsageCategoryKind>(PropertyKind.UsageCategory);
		DebugAssertHelper.Assert(simplePropertyValue.HasValue);
		NavigationMetadataEntry navigationMetadataEntry = new NavigationMetadataEntry
		{
			UsageCategory = simplePropertyValue.Value,
			Kind = symbol.Kind,
			TargetId = symbol.Id,
			SourceTableId = (symbol.GetRelatedTableSymbol()?.Id ?? 0),
			CaptionML = GenerateCaptionMLMetadata(symbol),
			AdditionalSearchTermsML = GenerateAdditionalSearchTermsMLMetadata(symbol)
		};
		BoundIdentifierListPropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundIdentifierListPropertyValue>(PropertyKind.ApplicationArea);
		if (boundPropertyValue != null)
		{
			navigationMetadataEntry.ApplicationArea = boundPropertyValue.ValueText;
		}
		BoundPermissionPropertyValue boundPropertyValue2 = symbol.GetBoundPropertyValue<BoundPermissionPropertyValue>(PropertyKind.AccessByPermission);
		if (boundPropertyValue2 != null && boundPropertyValue2.Value.Length == 1)
		{
			BoundPermissionExpression boundPermissionExpression = boundPropertyValue2.Value[0];
			navigationMetadataEntry.PermissionMask = boundPermissionExpression.Permission.Int32Value.ToString(CultureInfo.InvariantCulture);
			navigationMetadataEntry.PermissionValueObjectTypeName = boundPermissionExpression.ObjectType.GetPermissionValueObjectTypeName();
			navigationMetadataEntry.AccessByObjectID = boundPermissionExpression.GetPermissionObjectId();
		}
		return navigationMetadataEntry;
	}

	private static string GenerateCaptionMLMetadata(ApplicationObjectTypeSymbol symbol)
	{
		return GenerateMLMetadata(symbol, PropertyKind.Caption, PropertyKind.CaptionML);
	}

	private static string GenerateAdditionalSearchTermsMLMetadata(ApplicationObjectTypeSymbol symbol)
	{
		return GenerateMLMetadata(symbol, PropertyKind.AdditionalSearchTerms, PropertyKind.AdditionalSearchTermsML);
	}

	private static string GenerateMLMetadata(ApplicationObjectTypeSymbol applicationObjectTypeSymbol, PropertyKind basePropertyKind, PropertyKind propertyKind)
	{
		PropertySymbol propertySymbol = applicationObjectTypeSymbol.GetPropertySymbol(propertyKind);
		if (propertySymbol == null)
		{
			propertySymbol = applicationObjectTypeSymbol.GetPropertySymbol(basePropertyKind);
		}
		if (propertySymbol != null && ObjectMetadataEmitHelper.TryGetCaptionMLPropertyValue(propertySymbol, out var propertyValue))
		{
			return propertyValue;
		}
		return string.Format(CultureInfo.InvariantCulture, "ENU={0}", applicationObjectTypeSymbol.Name);
	}
}
