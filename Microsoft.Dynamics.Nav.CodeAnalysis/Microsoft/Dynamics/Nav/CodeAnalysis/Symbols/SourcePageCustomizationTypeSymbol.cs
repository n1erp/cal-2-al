using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourcePageCustomizationTypeSymbol : PageCustomizationTypeSymbol
{
	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal override ApplicationObjectTypeSymbol Target => Members.Target;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal PageCustomizationObjectMembers Members { get; }

	internal SourcePageCustomizationTypeSymbol(ModuleSymbol containingSymbol, PageCustomizationSyntax declaration)
		: base(containingSymbol)
	{
		Members = new PageCustomizationObjectMembers(this, declaration);
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
		Members.ForceComplete(locationOpt, cancellationToken, (SymbolKind kind) => true);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return Members.HasComplete(part);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return Members.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return Members.GetCodeMembers(name);
	}

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return new ApplicationObjectCodeEmitter(new PageCustomizationCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new PageExtensionMetadataEmitter(this, moduleBuilder);
	}
}
