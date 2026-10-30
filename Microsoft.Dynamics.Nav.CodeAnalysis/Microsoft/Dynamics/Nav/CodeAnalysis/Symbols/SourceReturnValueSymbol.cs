using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceReturnValueSymbol : ReturnValueSymbol
{
	private readonly ReturnValueSyntax syntax;

	private readonly string name;

	public override Symbol ContainingSymbol { get; }

	public override string Name => name;

	public override string MetadataName { get; }

	public override bool IsNamed => name != null;

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(syntax.GetReference());

	public static SourceReturnValueSymbol Create(Symbol owner, TypeSymbol returnValueType, ReturnValueSyntax syntax)
	{
		string text = syntax.Name?.Identifier.ValueText;
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)((syntax.Name != null) ? new SourceLocation(syntax.Name.Identifier) : new SourceLocation(syntax.DataType)));
		return new SourceReturnValueSymbol(owner, returnValueType, syntax, text, locations);
	}

	private SourceReturnValueSymbol(Symbol containingSymbol, TypeSymbol returnValueType, ReturnValueSyntax syntax, string name, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		Type = returnValueType;
		this.syntax = syntax;
		this.name = name?.UnquoteIdentifier();
		MetadataName = name?.GetLowercasedMetadataName();
		Locations = locations;
	}
}
