using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceProfileTypeSymbol : ProfileTypeSymbol
{
	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal ProfileObjectMembers Members { get; }

	internal SourceProfileTypeSymbol(ModuleSymbol containingSymbol, ProfileSyntax declaration)
		: base(containingSymbol)
	{
		Members = new ProfileObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, (SymbolKind kind) => true);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return Members.HasComplete(part);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Members.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return Members.GetMembers(name);
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new ProfileMetadataEmitter(this, moduleBuilder);
	}
}
