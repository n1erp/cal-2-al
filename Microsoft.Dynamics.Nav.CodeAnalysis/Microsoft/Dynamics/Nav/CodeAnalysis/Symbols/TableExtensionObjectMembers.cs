using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class TableExtensionObjectMembers : ExtensionObjectMembers<TableExtensionTypeSymbol, TableTypeSymbol>
{
	private TableTypeSymbol lazyTarget;

	private ImmutableArray<Symbol> lazyCodeMembers;

	private readonly Lazy<ImmutableArray<SourceChangeModifySymbol>> lazyModifySymbols;

	internal override TableTypeSymbol Target
	{
		get
		{
			if (lazyTarget != null)
			{
				return lazyTarget;
			}
			GetMembers();
			return lazyTarget;
		}
	}

	protected override SymbolKind TargetKind => SymbolKind.Table;

	internal new TableExtensionSyntax Declaration => (TableExtensionSyntax)base.Declaration;

	internal ImmutableArray<SourceChangeModifySymbol> ModifySymbols => lazyModifySymbols.Value;

	internal TableExtensionObjectMembers(TableExtensionTypeSymbol containingSymbol, TableExtensionSyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
		lazyModifySymbols = SetGetterForLazyChangeModifySymbols();
	}

	private Lazy<ImmutableArray<SourceChangeModifySymbol>> SetGetterForLazyChangeModifySymbols()
	{
		return new Lazy<ImmutableArray<SourceChangeModifySymbol>>(() => (from ChangeSymbol c in from s in GetMembers()
				where s.Kind == SymbolKind.Change
				select s
			where c.ChangeKind.IsModify()
			select c).Cast<SourceChangeModifySymbol>().ToImmutableArray());
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetTableExtensionTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		GetLazyTarget(diagnostics);
		TypeSymbol recordTypeSymbol = GetRecordTypeSymbol();
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, recordTypeSymbol, "Rec", isThis: true));
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, recordTypeSymbol, "xRec"));
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, NavCorLib.IntegerType, "CurrFieldNo", isThis: false, hasImplicitWith: true));
		if (Declaration.Fields != null)
		{
			foreach (FieldBaseSyntax field in Declaration.Fields.Fields)
			{
				switch (field.Kind)
				{
				case SyntaxKind.Field:
					members.Add(SourceFieldSymbol.Create(ContainingSymbol, (FieldSyntax)field));
					break;
				case SyntaxKind.FieldModification:
					members.Add(SourceChangeModifyFieldSymbol.Create(ContainingSymbol, (FieldModificationSyntax)field));
					break;
				}
			}
		}
		if (Declaration.Keys != null)
		{
			foreach (KeySyntax key in Declaration.Keys.Keys)
			{
				members.Add(SourceKeySymbol.Create(ContainingSymbol, key));
			}
		}
		if (Declaration.FieldGroups == null)
		{
			return;
		}
		foreach (FieldGroupChangeBaseSyntax change in Declaration.FieldGroups.Changes)
		{
			SyntaxKind kind = change.Kind;
			if (kind == SyntaxKind.FieldGroupAddChange)
			{
				members.Add(new SourceChangeAddFieldGroupSymbol(ContainingSymbol, (FieldGroupAddChangeSyntax)change));
				continue;
			}
			throw ExceptionUtilities.UnexpectedValue(change.Kind);
		}
	}

	private void GetLazyTarget(DiagnosticBag diagnostics)
	{
		if (!(lazyTarget != null) && Declaration.BaseObject != null)
		{
			Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Declaration.SyntaxTree).GetBinder(Declaration.BaseObject);
			TableTypeSymbol value = binder.GetApplicationObjectTypeSymbol(SymbolKind.Table, Declaration.BaseObject, diagnostics) as TableTypeSymbol;
			if (Interlocked.CompareExchange(ref lazyTarget, value, null) == null)
			{
				binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, lazyTarget, Declaration.BaseObject, isTableExtensionTarget: true);
			}
		}
	}

	private TypeSymbol GetRecordTypeSymbol()
	{
		if (lazyTarget == null)
		{
			return ContainingSymbol;
		}
		return ContainingSymbol.ContainingModule.GetRecord(lazyTarget, isTemporary: false);
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		if (!CheckTargetRelation(diagnostics))
		{
			return;
		}
		CheckExtensionTarget(diagnostics);
		CheckForMultipleModify(diagnostics);
		CheckForNoChange(diagnostics);
		ImmutableArray<Symbol> members = GetMembers();
		PooledList<SourceKeySymbol> instance = PooledList<SourceKeySymbol>.GetInstance();
		PooledList<SourceFieldSymbol> instance2 = PooledList<SourceFieldSymbol>.GetInstance();
		try
		{
			foreach (Symbol item in members)
			{
				switch (item.Kind)
				{
				case SymbolKind.Key:
					instance.Add((SourceKeySymbol)item);
					break;
				case SymbolKind.Field:
					instance2.Add((SourceFieldSymbol)item);
					break;
				}
			}
			TableObjectMembers.CheckForDuplicatedFieldsList(instance, diagnostics);
			CheckForPropertiesNotAllowedOnKeys(instance, diagnostics);
			CheckForPropertiesNotAllowedOnFields(instance2, diagnostics);
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private void CheckExtensionTarget(DiagnosticBag diagnostics)
	{
		TableTypeKind? enumPropertyValue = Target.GetEnumPropertyValue<TableTypeKind>(PropertyKind.TableType);
		if (enumPropertyValue.HasValue && enumPropertyValue.Value != TableTypeKind.Normal)
		{
			diagnostics.Add(ErrorCode.ERR_ExtensionTargetTypeCannotBeExtended, Declaration.BaseObject.Location, SymbolKind.Table, Declaration.BaseObject.Identifier, enumPropertyValue.Value);
		}
		if (SystemTables.IsSystemOrVirtualTable(Target))
		{
			diagnostics.Add(ErrorCode.ERR_SystemTableCannotBeExtended, Declaration.BaseObject.Location, Target.Name);
		}
	}

	private static void CheckForPropertiesNotAllowedOnKeys(IEnumerable<SourceKeySymbol> keys, DiagnosticBag diagnostics)
	{
		foreach (SourceKeySymbol key in keys)
		{
			foreach (PropertySymbol property in key.Properties)
			{
				PropertyKind propertyKind = property.PropertyKind;
				if (propertyKind == PropertyKind.Clustered || propertyKind == PropertyKind.Unique)
				{
					diagnostics.Add(ErrorCode.ERR_PropertyInvalidOnTableExtension, property.GetLocation(), property.Name);
				}
			}
		}
	}

	private static void CheckForPropertiesNotAllowedOnFields(IEnumerable<SourceFieldSymbol> fields, DiagnosticBag diagnostics)
	{
		foreach (SourceFieldSymbol field in fields)
		{
			foreach (PropertySymbol property in field.Properties)
			{
				PropertyKind propertyKind = property.PropertyKind;
				if ((uint)(propertyKind - 59) <= 1u)
				{
					diagnostics.Add(ErrorCode.ERR_PropertyInvalidOnTableExtension, property.GetLocation(), property.Name);
				}
			}
		}
	}

	private void CheckForNoChange(DiagnosticBag diagnostics)
	{
		ModifySymbols.Where((SourceChangeModifySymbol c) => c.Properties.IsEmpty && c.Triggers.IsEmpty).Do((SourceChangeModifySymbol c) =>
		{
			diagnostics.Add(ErrorCode.ERR_ModifyContainsNoChange, c.GetLocation(), c.Name);
		});
	}

	private void CheckForMultipleModify(DiagnosticBag diagnostics)
	{
		(from modified in ModifySymbols
			group modified by modified.Name.ToUpperInvariant() into grouping
			where grouping.Count() > 1
			select grouping).Do((IGrouping<string, SourceChangeModifySymbol> grouping) =>
		{
			grouping.Do((SourceChangeModifySymbol modification) =>
			{
				diagnostics.Add(ErrorCode.ERR_CannotModifyMultipleTimes, modification.GetNameLocation(), modification.Target?.Kind);
			});
		});
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != null)
		{
			return lazyCodeMembers;
		}
		ImmutableArray<Symbol> value = (from m in GetMembers()
			where m.Kind != SymbolKind.Change
			select m).ToImmutableArray();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, value, default);
		return lazyCodeMembers;
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidTableExtensionTrigger(name))
		{
			return TriggerEventDefinitions.IsValidTableTriggerEvent(name);
		}
		return true;
	}
}
