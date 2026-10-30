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
internal sealed class SourceTableTypeSymbol : TableTypeSymbol
{
	public override SymbolCompilationScope CompilationScope
	{
		get
		{
			if (!Scope.HasValue)
			{
				PropertySymbol propertySymbol = this.GetPropertySymbol(PropertyKind.Scope);
				if (propertySymbol != null)
				{
					Scope = propertySymbol.GetValueAsEnum<SymbolCompilationScope>();
				}
				else
				{
					Scope = SymbolCompilationScope.Extension;
				}
			}
			return Scope.Value;
		}
	}

	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override TableTypeKind TableType => this.GetEnumPropertyValueOrDefault<TableTypeKind>(PropertyKind.TableType);

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal TableObjectMembers Members { get; }

	internal SourceTableTypeSymbol(ModuleSymbol containingSymbol, TableSyntax declaration)
		: base(containingSymbol)
	{
		Members = new TableObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsTableMember);
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
		return new ApplicationObjectCodeEmitter(new TableCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new TableMetadataEmitter(this, moduleBuilder);
	}

	internal override ImmutableArray<FieldSymbol> GetFields()
	{
		return this.GetMembersOfKind<FieldSymbol>(SymbolKind.Field).ToImmutableArray();
	}

	internal override ImmutableArray<KeySymbol> GetKeys()
	{
		return this.GetMembersOfKind<KeySymbol>(SymbolKind.Key).ToImmutableArray();
	}

	internal override ImmutableArray<FieldGroupSymbol> GetFieldGroups()
	{
		return this.GetMembersOfKind<FieldGroupSymbol>(SymbolKind.FieldGroup).ToImmutableArray();
	}
}
