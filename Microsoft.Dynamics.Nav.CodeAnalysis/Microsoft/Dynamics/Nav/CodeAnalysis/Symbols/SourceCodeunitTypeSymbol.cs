using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceCodeunitTypeSymbol : CodeunitTypeSymbol
{
	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal CodeunitObjectMembers Members { get; }

	internal SourceCodeunitTypeSymbol(ModuleSymbol containingSymbol, CodeunitSyntax declaration)
		: base(containingSymbol)
	{
		Members = new CodeunitObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken);
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

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return new ApplicationObjectCodeEmitter(new CodeunitCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}
}
