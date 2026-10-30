using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceRequestPageTypeSymbol : RequestPageTypeSymbol
{
	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	internal override bool RequiresCompletion => true;

	private RequestPageObjectMembers Members { get; }

	public SourceRequestPageTypeSymbol(ApplicationObjectTypeSymbol containingSymbol, RequestPageSyntax declaration)
		: base(containingSymbol)
	{
		Members = new RequestPageObjectMembers(this, declaration);
		Locations = ImmutableArray.Create((Location)new SourceLocation(declaration.RequestPageKeyword));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Members.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return Members.GetMembers(name);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, (SymbolKind kind) => kind == SymbolKind.Control || kind == SymbolKind.Action);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return Members.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return Members.GetCodeMembers(name);
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		throw new NotImplementedException();
	}
}
