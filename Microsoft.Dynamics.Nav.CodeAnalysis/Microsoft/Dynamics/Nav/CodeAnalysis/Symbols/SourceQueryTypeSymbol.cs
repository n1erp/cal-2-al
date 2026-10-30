using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SourceQueryTypeSymbol : QueryTypeSymbol
{
	internal QuerySyntax Syntax { get; }

	public QueryObjectMembers Members { get; set; }

	public override int Id => Members.Id;

	public override string Name => Members.Name;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	public override QueryTypeKind QueryType => this.GetEnumPropertyValueOrDefault<QueryTypeKind>(PropertyKind.QueryType);

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal override bool RequiresCompletion => true;

	public SourceQueryTypeSymbol(ModuleSymbol containingSymbol, QuerySyntax declaration)
		: base(containingSymbol)
	{
		Members = new QueryObjectMembers(this, declaration);
		Syntax = declaration;
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

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsQueryMember);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return Members.HasComplete(part);
	}

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return new ApplicationObjectCodeEmitter(new QueryCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new QueryMetadataEmitter(this, moduleBuilder);
	}
}
