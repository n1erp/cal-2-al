using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class TableMetadataEmitter : TableBaseMetadataEmitter<SourceTableTypeSymbol>
{
	internal TableMetadataEmitter(SourceTableTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("MetaTable", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", Symbol.Name);
			WriteProperties(xmlWriter, Symbol, Symbol.Properties, shouldOutputDefaultValues: true);
			WriteExtensionSourceAttributesIfNecessary(xmlWriter);
			WriteFields(xmlWriter);
			WriteKeys(xmlWriter, Symbol.GetKeys().Cast<SourceKeySymbol>());
			WriteMembers<SourceFieldGroupSymbol>(xmlWriter, "FieldGroups", SymbolKind.FieldGroup, WriteFieldGroupAttributeStrings);
			xmlWriter.WriteEndElement();
		});
	}

	private void WriteKeys(XmlWriter xmlWriter, IEnumerable<SourceKeySymbol> keys)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Keys", () =>
		{
			if (keys.IsEmpty())
			{
				WriteDefaultKeyProperties(xmlWriter);
				return;
			}
			foreach (SourceKeySymbol member in keys)
			{
				ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Key", () =>
				{
					WriteKeyAttributes(member, xmlWriter);
				});
			}
		});
	}

	internal void WriteFields(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Fields", () =>
		{
			ImmutableArray<FieldSymbol> fields = Symbol.GetFields();
			if (fields.Length > 0)
			{
				foreach (SourceFieldSymbol item in fields.Cast<SourceFieldSymbol>())
				{
					WriteField(xmlWriter, item);
				}
			}
		});
	}

	private static void WriteFieldGroupAttributeStrings(SourceFieldGroupSymbol sourceFieldGroupSymbol, XmlWriter xmlWriter)
	{
		string fieldPrefixedAttributeValues = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(sourceFieldGroupSymbol.Fields);
		xmlWriter.WriteAttributeString("GroupName", sourceFieldGroupSymbol.Name);
		xmlWriter.WriteAttributeString("GroupFields", fieldPrefixedAttributeValues);
	}

	private void WriteMembers<T>(XmlWriter xmlWriter, string groupsName, SymbolKind kind, Action<T, XmlWriter> writeAttributeStrings) where T : Symbol
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, groupsName, () =>
		{
			foreach (T member in Symbol.GetMembersOfKind<T>(kind))
			{
				ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, groupsName.Substring(0, groupsName.Length - 1), () =>
				{
					writeAttributeStrings(member, xmlWriter);
					WriteProperties(xmlWriter, member, member.Properties, shouldOutputDefaultValues: true);
				});
			}
		});
	}

	private void WriteDefaultKeyProperties(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Key", () =>
		{
			KeySymbol primaryKey = Symbol.GetPrimaryKey();
			DebugAssertHelper.Assert(primaryKey != null);
			DebugAssertHelper.Assert(!primaryKey.Fields.IsDefaultOrEmpty);
			FieldSymbol fieldSymbol = primaryKey.Fields[0];
			xmlWriter.WriteAttributeString("Enabled", "1");
			xmlWriter.WriteAttributeString("Key", "Field" + fieldSymbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("KeyName", primaryKey.Name);
			xmlWriter.WriteAttributeString("MaintainSQLIndex", "1");
			xmlWriter.WriteAttributeString("MaintainSIFTIndex", "1");
			xmlWriter.WriteAttributeString("Clustered", "1");
			WriteExtensionSourceAttributesIfNecessary(xmlWriter);
		});
	}
}
