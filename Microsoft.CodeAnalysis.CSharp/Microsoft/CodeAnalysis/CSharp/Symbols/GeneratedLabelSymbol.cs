using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class GeneratedLabelSymbol : LabelSymbol
{
	private readonly string _name;

	public override string Name => _name;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsImplicitlyDeclared => true;

	public GeneratedLabelSymbol(string name)
	{
		_name = LabelName(name);
	}

	private static string LabelName(string name)
	{
		return name;
	}
}
