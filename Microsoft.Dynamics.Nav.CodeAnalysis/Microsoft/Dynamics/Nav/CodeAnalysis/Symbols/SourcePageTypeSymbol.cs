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
internal sealed class SourcePageTypeSymbol : PageTypeSymbol
{
	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.PageProperties;

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal PageObjectMembers Members { get; }

	public override PageTypeKind PageType => this.GetEnumPropertyValueOrDefault<PageTypeKind>(PropertyKind.PageType);

	public override TableTypeSymbol RelatedTable
	{
		get
		{
			PropertySymbol property = GetProperty(PropertyKind.SourceTable);
			if (!(property != null) || property.Value == null)
			{
				return null;
			}
			return (TableTypeSymbol)property.Value;
		}
	}

	internal SourcePageTypeSymbol(ModuleSymbol containingSymbol, PageSyntax declaration)
		: base(containingSymbol)
	{
		Members = new PageObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsPageMember);
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
		return new ApplicationObjectCodeEmitter(new PageCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new PageMetadataEmitter(this, moduleBuilder);
	}
}
