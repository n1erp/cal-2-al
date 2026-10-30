using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface IAlWriterState
{
	string Indent { get; }

	int LinePosition { get; }

	bool PendingEmptyLine { get; }

	bool DataClassificationDefaulting { get; }

	IDotNetDeclarationExtractor DotNetExtractor { get; }

	IEnumDeclarationExtractor EnumExtractor { get; }

	ITranslationExtractor TranslationExtractor { get; }

	ITypeResolver TypeResolver { get; }

	IFileNameExporter FileNameExporter { get; }

	Version TargetRuntimeVersion { get; }
}
