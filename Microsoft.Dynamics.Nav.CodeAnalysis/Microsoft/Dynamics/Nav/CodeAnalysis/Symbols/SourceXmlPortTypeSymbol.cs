using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceXmlPortTypeSymbol : XmlPortTypeSymbol
{
	internal ImmutableArray<XmlPortNodeSymbol> FlattenedNodes => Members.FlattenedNodes;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	internal RequestPageTypeSymbol RequestPage => Members.RequestPage;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GlobalVariables;

	internal XmlPortObjectMembers Members { get; }

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal SourceXmlPortTypeSymbol(SourceModuleSymbol containingSymbol, XmlPortSyntax declaration)
		: base(containingSymbol)
	{
		Members = new XmlPortObjectMembers(this, declaration);
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
		Members.ForceComplete(locationOpt, cancellationToken, (SymbolKind kind) => kind == SymbolKind.XmlPortNode);
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return Members.HasComplete(part);
	}

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return new ApplicationObjectCodeEmitter(new XmlPortCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new XmlPortMetadataEmitter(this, moduleBuilder);
	}
}
