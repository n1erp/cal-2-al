using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceLocalVariableSymbol : SourceVariableSymbol
{
	public override SymbolKind Kind => SymbolKind.LocalVariable;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public static SourceLocalVariableSymbol Create(Symbol owner, VariableDeclarationSyntax syntax)
	{
		string valueText = syntax.Name.Identifier.ValueText;
		ImmutableArray<Location> immutableArray = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceLocalVariableSymbol(owner, syntax, syntax.Name.Identifier, valueText, immutableArray);
	}

	private SourceLocalVariableSymbol(Symbol containingSymbol, VariableDeclarationSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
		: base(containingSymbol, syntax, identifier, name, locations)
	{
	}
}
