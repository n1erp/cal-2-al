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
internal sealed class SourceReportTypeSymbol : ReportTypeSymbol
{
	public override int Id => Members.Id;

	public override string Name => Members.Name;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal ReportObjectMembers Members { get; }

	internal override bool RequiresCompletion => true;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GlobalVariables;

	public override ImmutableArray<ReportDataItemSymbol> FlattenedDataItems => Members.FlattenedDataItems;

	internal ImmutableArray<SourceReportColumnSymbol> FlattenedColumns => Members.FlattenedColumns;

	public override RequestPageTypeSymbol RequestPage => Members.RequestPage;

	public SourceReportTypeSymbol(ModuleSymbol containingSymbol, ReportSyntax declaration)
		: base(containingSymbol)
	{
		Members = new ReportObjectMembers(this, declaration);
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
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsReportMember);
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
		return new ApplicationObjectCodeEmitter(new ReportCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new ReportMetadataEmitter(this, moduleBuilder);
	}

	internal override PreMetadataEmitter GetPreMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new ReportLayoutsEmitter(this, moduleBuilder);
	}
}
