using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public interface IEnumDeclarationExtractor
{
	bool HasDeclarations { get; }

	void AddEnumDeclaration(int id, string name, bool extensible, string optionString, string optionCaptionML);

	EnumTypeDefinition[] GetDeclarations();
}
