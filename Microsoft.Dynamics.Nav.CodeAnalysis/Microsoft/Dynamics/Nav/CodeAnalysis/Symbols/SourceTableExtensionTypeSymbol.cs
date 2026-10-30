using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceTableExtensionTypeSymbol : TableExtensionTypeSymbol
{
	private ImmutableArray<FieldSymbol> lazyAddedFields;

	private ImmutableArray<SourceChangeModifyFieldSymbol> lazyModifiedFields;

	private ImmutableArray<KeySymbol> lazyAddedKeys;

	private ImmutableArray<SourceChangeAddFieldGroupSymbol> lazyAddLastFieldGroups;

	public override int Id => Members.Id;

	public override string Name => Members.Name;

	internal override bool RequiresCompletion => true;

	public override SymbolCompilationScope CompilationScope => SymbolCompilationScope.Extension;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => Members.GetGlobalVariables();

	public override ImmutableArray<MethodSymbol> Triggers => Members.GetTriggers();

	internal override ApplicationObjectTypeSymbol Target => Members.Target;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Members.DeclaringSyntaxReferences;

	public override ImmutableArray<Location> Locations => Members.Locations;

	public override ImmutableArray<PropertySymbol> Properties => Members.GetProperties();

	internal TableExtensionObjectMembers Members { get; }

	internal SourceTableExtensionTypeSymbol(ModuleSymbol containingSymbol, TableExtensionSyntax declaration)
		: base(containingSymbol)
	{
		Members = new TableExtensionObjectMembers(this, declaration);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		Members.ForceComplete(locationOpt, cancellationToken, SymbolKindExtensions.IsTableExtensionMember);
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
		return new ApplicationObjectCodeEmitter(new TableExtensionCodeBuilder(this, moduleBuilder));
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new TableExtensionMetadataEmitter(this, moduleBuilder);
	}

	internal override ImmutableArray<FieldSymbol> GetAddedFields()
	{
		if (lazyAddedFields != null)
		{
			return lazyAddedFields;
		}
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance();
		foreach (Symbol member in Members.GetMembers())
		{
			if (member.Kind == SymbolKind.Field)
			{
				instance.Add((SourceFieldSymbol)member);
			}
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyAddedFields, instance.ToImmutableAndFree(), default);
		return lazyAddedFields;
	}

	internal ImmutableArray<SourceChangeModifyFieldSymbol> ModifiedFields()
	{
		if (lazyModifiedFields != null)
		{
			return lazyModifiedFields;
		}
		ArrayBuilder<SourceChangeModifyFieldSymbol> instance = ArrayBuilder<SourceChangeModifyFieldSymbol>.GetInstance();
		foreach (Symbol member in Members.GetMembers())
		{
			if (member.Kind == SymbolKind.Change && ((ChangeSymbol)member).ChangeTargetKind == SymbolKind.Field)
			{
				instance.Add((SourceChangeModifyFieldSymbol)member);
			}
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyModifiedFields, instance.ToImmutableAndFree(), default);
		return lazyModifiedFields;
	}

	internal override ImmutableArray<KeySymbol> GetAddedKeys()
	{
		if (lazyAddedKeys != null)
		{
			return lazyAddedKeys;
		}
		ArrayBuilder<KeySymbol> instance = ArrayBuilder<KeySymbol>.GetInstance();
		foreach (Symbol member in Members.GetMembers())
		{
			if (member.Kind == SymbolKind.Key)
			{
				instance.Add((KeySymbol)member);
			}
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyAddedKeys, instance.ToImmutableAndFree(), default);
		return lazyAddedKeys;
	}

	internal ImmutableArray<SourceChangeAddFieldGroupSymbol> GetChangedFieldGroups()
	{
		if (lazyAddLastFieldGroups != null)
		{
			return lazyAddLastFieldGroups;
		}
		ArrayBuilder<SourceChangeAddFieldGroupSymbol> instance = ArrayBuilder<SourceChangeAddFieldGroupSymbol>.GetInstance();
		foreach (Symbol member in Members.GetMembers())
		{
			if (member.Kind == SymbolKind.Change)
			{
				ChangeSymbol changeSymbol = (ChangeSymbol)member;
				if (changeSymbol.ChangeKind == ChangeKind.AddLast && changeSymbol.ChangeTargetKind == SymbolKind.FieldGroup)
				{
					instance.Add((SourceChangeAddFieldGroupSymbol)member);
				}
			}
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyAddLastFieldGroups, instance.ToImmutableAndFree(), default);
		return lazyAddLastFieldGroups;
	}
}
