using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class GeneratedLabelSymbol : LabelSymbol
{
	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsImplicitlyDeclared => true;

	public GeneratedLabelSymbol(string name)
		: base(LabelName(name))
	{
	}

	private static string LabelName(string name)
	{
		return name;
	}
}
