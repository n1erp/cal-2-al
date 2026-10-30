using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ISymbol : IEquatable<ISymbol>
{
	SymbolKind Kind { get; }

	string Name { get; }

	ImmutableArray<Location> Locations { get; }

	ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	ISymbol ContainingSymbol { get; }

	ITypeSymbol ContainingType { get; }

	SymbolCompilationScope CompilationScope { get; }

	ISymbol OriginalDefinition { get; }

	ImmutableArray<IPropertySymbol> Properties { get; }

	string ToDisplayString(SymbolDisplayFormat format = null);

	ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null);

	string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null);

	ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null);

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	string GetDocumentation();

	IPropertySymbol GetProperty(PropertyKind kind);
}
