using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class ObjectMetadataEmitter
{
	internal static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
	{
		Indent = true,
		Encoding = Encoding.UTF8,
		ConformanceLevel = ConformanceLevel.Document
	};

	protected internal ModuleBuilder ModuleBuilder { get; }

	protected ObjectMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		ModuleBuilder = moduleBuilder;
	}

	internal abstract void EmitMetadata(DiagnosticBag diagnostics);

	protected void WriteProperties(XmlWriter xmlWriter, Symbol declaringSymbol, IEnumerable<PropertySymbol> properties, bool shouldOutputDefaultValues)
	{
		ObjectMetadataEmitHelper.WriteProperties(xmlWriter, declaringSymbol, properties, WritePropertyValueMetadata, shouldOutputDefaultValues);
	}

	private void WritePropertyValueMetadata(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (!TryUseSpecialPropertyValueMetadataEmitter(xmlWriter, property))
		{
			ObjectMetadataEmitHelper.WritePropertyValueMetadata(xmlWriter, property);
		}
	}

	protected virtual bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		return false;
	}
}
internal abstract class ObjectMetadataEmitter<T> : ObjectMetadataEmitter where T : ObjectTypeSymbol
{
	protected internal T Symbol { get; }

	protected internal IFileSystem FileSystem { get; }

	protected ObjectMetadataEmitter(T symbol, ModuleBuilder moduleBuilder)
		: base(moduleBuilder)
	{
		Symbol = symbol;
		FileSystem = symbol.DeclaringCompilation.FileSystem;
	}

	internal void EmitMetadata(Action<XmlWriter> content)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, ObjectMetadataEmitter.WriterSettings))
		{
			xmlWriter.WriteStartDocument();
			content(xmlWriter);
			xmlWriter.WriteEndDocument();
		}
		memoryStream.Position = 0L;
		SetObjectMetadata(Symbol, memoryStream);
	}

	protected virtual void SetObjectMetadata(IObjectTypeSymbol symbol, MemoryStream outputStream)
	{
		ModuleBuilder.SetApplicationObjectMetadata(symbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>(), outputStream);
	}
}
