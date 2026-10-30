using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SourceParameterSymbol : ParameterSymbol
{
	private SymbolCompletionState state;

	private readonly ImmutableArray<Location> locations;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<ParameterSyntax>(locations);

	internal override bool RequiresCompletion => true;

	public override bool IsVar { get; }

	public override bool IsParams => false;

	public override string Name { get; }

	public override string MetadataName { get; }

	public override TypeSymbol Type { get; }

	public override int Ordinal { get; }

	internal static SourceParameterSymbol Create(Symbol owner, TypeSymbol parameterType, ParameterSyntax syntax, int ordinal, bool isVar, SyntaxToken identifier)
	{
		string valueText = identifier.ValueText;
		ImmutableArray<Location> immutableArray = ImmutableArray.Create((Location)new SourceLocation(identifier));
		DebugAssertHelper.Assert(syntax != null);
		return new SourceParameterSymbol(owner, parameterType, ordinal, isVar, valueText, immutableArray);
	}

	private SourceParameterSymbol(Symbol owner, TypeSymbol parameterType, int ordinal, bool isVar, string name, ImmutableArray<Location> locations)
	{
		DebugAssertHelper.Assert((object)owner != null);
		ContainingSymbol = owner;
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.Method || owner.Kind == SymbolKind.Event);
		Type = parameterType;
		Ordinal = ordinal;
		IsVar = isVar;
		Name = name.UnquoteIdentifier();
		MetadataName = name.GetLowercasedMetadataNameForParameter();
		this.locations = locations;
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		state.DefaultForceComplete(this);
	}
}
