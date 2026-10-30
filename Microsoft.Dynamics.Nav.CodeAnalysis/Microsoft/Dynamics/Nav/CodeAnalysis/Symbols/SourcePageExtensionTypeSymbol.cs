using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourcePageExtensionTypeSymbol : PageExtensionTypeSymbol
{
	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.PageProperties;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal override ApplicationObjectTypeSymbol Target => Members.Target;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal PageExtensionObjectMembers Members { get; }

	internal SourcePageExtensionTypeSymbol(ModuleSymbol containingSymbol, PageExtensionSyntax declaration)
		: base(containingSymbol)
	{
		Members = new PageExtensionObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsPageExtensionMember);
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
		return new ApplicationObjectCodeEmitter(new PageExtensionCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new PageExtensionMetadataEmitter(this, moduleBuilder);
	}
}
