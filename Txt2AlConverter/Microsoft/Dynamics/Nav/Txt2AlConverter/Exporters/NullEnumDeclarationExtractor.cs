using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

public sealed class NullEnumDeclarationExtractor : IEnumDeclarationExtractor
{
	public static IEnumDeclarationExtractor Instance { get; } = new NullEnumDeclarationExtractor();

	public bool HasDeclarations { get; }

	private NullEnumDeclarationExtractor()
	{
	}

	public EnumTypeDefinition[] GetDeclarations()
	{
		return Array.Empty<EnumTypeDefinition>();
	}

	public void AddEnumDeclaration(int id, string name, bool extensible, string optionString, string optionCaptionML)
	{
	}
}
