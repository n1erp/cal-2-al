using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class TableObjectMembers : ApplicationObjectMembers<TableTypeSymbol>
{
	private const int MaxColumnsInIndex = 32;

	private const int MaxKeysInTable = 40;

	internal new TableSyntax Declaration => (TableSyntax)base.Declaration;

	internal TableObjectMembers(TableTypeSymbol containingSymbol, TableSyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetTableTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		RecordTypeSymbol record = ContainingSymbol.ContainingModule.GetRecord(ContainingSymbol, isTemporary: false);
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "Rec", isThis: true));
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, record, "xRec"));
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, NavCorLib.IntegerType, "CurrFieldNo", isThis: false, hasImplicitWith: true));
		if (Declaration.Fields != null)
		{
			foreach (FieldSyntax field in Declaration.Fields.Fields)
			{
				members.Add(SourceFieldSymbol.Create(ContainingSymbol, field));
			}
		}
		if (Declaration.Keys != null)
		{
			foreach (KeySyntax key in Declaration.Keys.Keys)
			{
				members.Add(SourceKeySymbol.Create(ContainingSymbol, key));
			}
		}
		if (Declaration.FieldGroups != null)
		{
			foreach (FieldGroupSyntax fieldGroup in Declaration.FieldGroups.FieldGroups)
			{
				members.Add(SourceFieldGroupSymbol.Create(ContainingSymbol, fieldGroup));
			}
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		ImmutableArray<Symbol> members = GetMembers();
		bool flag = ObjectMembers<TableTypeSymbol>.CheckMembersForDuplicates(members, diagnostics, SymbolKind.Field, ErrorCode.ERR_DuplicateFieldName, ErrorCode.ERR_DuplicateFieldId);
		ObjectMembers<TableTypeSymbol>.CheckMembersForDuplicateNames(members, diagnostics, (Symbol f) => f.Kind == SymbolKind.Key, ErrorCode.ERR_DuplicateKeyName);
		ObjectMembers<TableTypeSymbol>.CheckMembersForDuplicateNames(members, diagnostics, (Symbol f) => f.Kind == SymbolKind.FieldGroup, ErrorCode.ERR_DuplicateFieldGroupName);
		if (!flag)
		{
			ObjectMembers<TableTypeSymbol>.CheckMemberGroupsForDuplicateNames(members.Where((Symbol m) => m.Kind == SymbolKind.Field || m.Kind == SymbolKind.Method), diagnostics, (Symbol m) => m.Kind, ErrorCode.WRN_ERR_DuplicateMemberName);
		}
		CheckIfDefaultKeyCanBeDetermined(ContainingSymbol, members, Declaration.Fields, diagnostics);
		CheckIfReservedFieldIsUsed(members, diagnostics);
		CheckKeys(members.Where((Symbol k) => k.Kind == SymbolKind.Key).Cast<SourceKeySymbol>().ToImmutableArray(), ContainingSymbol, Name, diagnostics);
	}

	private static void CheckIfDefaultKeyCanBeDetermined(TableTypeSymbol containingTable, ImmutableArray<Symbol> members, FieldListSyntax declarationFields, DiagnosticBag diagnostics)
	{
		if (!members.Any((Symbol m) => m.Kind == SymbolKind.Field && ((FieldSymbol)m).CheckEnumPropertyValue(PropertyKind.FieldClass, FieldClassKind.Normal)))
		{
			diagnostics.Add(ErrorCode.ERR_TableHasToHaveAtLeastOneNormalField, declarationFields.Location);
		}
		else if (containingTable.GetPrimaryKey() == null)
		{
			diagnostics.Add(ErrorCode.ERR_CouldNotDetermineDefaultPrimaryKey, containingTable.GetLocation(), containingTable.Name);
		}
	}

	private static void CheckIfReservedFieldIsUsed(ImmutableArray<Symbol> members, DiagnosticBag diagnostics)
	{
		foreach (Symbol item in members)
		{
			if (item.Kind == SymbolKind.Field)
			{
				SourceFieldSymbol sourceFieldSymbol = (SourceFieldSymbol)item;
				if (sourceFieldSymbol.Id >= 2000000000)
				{
					diagnostics.Add(ErrorCode.WRN_TableUsingReservedFieldId, new SourceLocation(sourceFieldSymbol.DeclaringSyntax.No), sourceFieldSymbol.Name, sourceFieldSymbol.Id);
				}
				if (sourceFieldSymbol.Name == "SystemId")
				{
					diagnostics.Add(ErrorCode.WRN_TableUsingReservedFieldName, sourceFieldSymbol.DeclaringSyntax.Name.Location, sourceFieldSymbol.Name);
				}
			}
		}
	}

	private static void CheckKeys(ImmutableArray<SourceKeySymbol> keys, TableTypeSymbol containingTable, string tableName, DiagnosticBag diagnostics)
	{
		if (keys.Length == 0)
		{
			return;
		}
		if (keys.Length > 40)
		{
			diagnostics.Add(ErrorCode.ERR_TableTooManyKeys, containingTable.GetLocation(), tableName);
			return;
		}
		SourceKeySymbol sourceKeySymbol = keys[0];
		CheckPrimaryKey(sourceKeySymbol, tableName, diagnostics);
		CheckForDuplicatedFieldsList(keys, diagnostics);
		if (sourceKeySymbol.GetBooleanPropertyValue(PropertyKind.Unique) == true)
		{
			diagnostics.Add(ErrorCode.ERR_PrimaryKeyMustNotHaveUnique, sourceKeySymbol.Syntax.Location, sourceKeySymbol.Name, tableName);
		}
		bool? flag = sourceKeySymbol.GetBooleanPropertyValue(PropertyKind.Clustered);
		for (int i = 1; i < keys.Length; i++)
		{
			SourceKeySymbol sourceKeySymbol2 = keys[i];
			bool? booleanPropertyValue = sourceKeySymbol2.GetBooleanPropertyValue(PropertyKind.Clustered);
			if (flag.HasValue && flag.Value)
			{
				if (booleanPropertyValue.HasValue && booleanPropertyValue.Value)
				{
					Location location = (from p in sourceKeySymbol2.Syntax.PropertyList.Properties
						where p.Kind != SyntaxKind.EmptyProperty && string.Compare(((PropertySyntax)p).Name.ToString(), PropertyKind.Clustered.ToString(), StringComparison.OrdinalIgnoreCase) == 0
						select p.Location).FirstOrDefault();
					diagnostics.Add(ErrorCode.ERR_ClusteredKeyCanAppearOnce, location ?? sourceKeySymbol2.Syntax.Location, sourceKeySymbol2.Name, tableName);
					break;
				}
			}
			else
			{
				flag = booleanPropertyValue;
			}
			if (sourceKeySymbol2.Fields.Length >= sourceKeySymbol.Fields.Length)
			{
				if (sourceKeySymbol2.Fields.Take(sourceKeySymbol.Fields.Length).SequenceEqual(sourceKeySymbol.Fields, SymbolNameEqualityComparer.Instance))
				{
					diagnostics.Add(ErrorCode.ERR_PrimaryKeyAppendedToSecondaryKey, sourceKeySymbol2.Syntax.Location, sourceKeySymbol2.Name, tableName);
					break;
				}
				CheckKeyConstraints(sourceKeySymbol, sourceKeySymbol2, tableName, diagnostics);
				Binder.ValidateForDuplicateNames(sourceKeySymbol2.Syntax.Fields, diagnostics);
			}
		}
	}

	internal static void CheckForDuplicatedFieldsList(IList<SourceKeySymbol> keys, DiagnosticBag diagnostics)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			for (int j = i + 1; j < keys.Count; j++)
			{
				if (keys[i].Fields.Length != keys[j].Fields.Length)
				{
					continue;
				}
				bool flag = true;
				for (int k = 0; k < keys[i].Fields.Length; k++)
				{
					if (keys[i].Fields[k] != keys[j].Fields[k])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Location location = keys[i].Syntax.Fields.Node.GetLocation();
					diagnostics.Add(ErrorCode.ERR_DuplicatedKeyFields, location, keys[i].Name, keys[j].Name);
				}
			}
		}
	}

	private static void CheckPrimaryKey(SourceKeySymbol primaryKey, string tableName, DiagnosticBag diagnostics)
	{
		bool? booleanPropertyValue = primaryKey.GetBooleanPropertyValue(PropertyKind.Enabled);
		if (booleanPropertyValue.HasValue && !booleanPropertyValue.Value)
		{
			diagnostics.Add(ErrorCode.ERR_PrimaryKeyMustBeEnabled, primaryKey.Syntax.Location, primaryKey.Name, tableName);
		}
		bool? booleanPropertyValue2 = primaryKey.GetBooleanPropertyValue(PropertyKind.MaintainSqlIndex);
		if (booleanPropertyValue2.HasValue && !booleanPropertyValue2.Value)
		{
			diagnostics.Add(ErrorCode.ERR_PrimaryKeyMustHaveMaintainSqlIndexEnabled, primaryKey.Syntax.Location, primaryKey.Name, tableName);
		}
		CheckKeyConstraints(primaryKey, primaryKey, tableName, diagnostics);
		Binder.ValidateForDuplicateNames(primaryKey.Syntax.Fields, diagnostics);
	}

	private static void CheckKeyConstraints(SourceKeySymbol primaryKey, SourceKeySymbol secondaryKey, string tableName, DiagnosticBag diagnostics)
	{
		SourceKeySymbol key = ((primaryKey == secondaryKey) ? primaryKey : secondaryKey);
		CheckKeyFieldTypes(key, tableName, diagnostics);
		CheckKeyMaxFields(key, tableName, diagnostics);
	}

	private static void CheckKeyFieldTypes(SourceKeySymbol key, string tableName, DiagnosticBag diagnostics)
	{
		bool flag = key.GetEnumPropertyValue<ObsoleteStateKind>(PropertyKind.ObsoleteState) == ObsoleteStateKind.Removed;
		bool flag2 = key.GetBooleanPropertyValue(PropertyKind.Enabled) != false;
		for (int i = 0; i < key.Fields.Length; i++)
		{
			FieldSymbol fieldSymbol = key.Fields[i];
			IdentifierNameSyntax identifierNameSyntax = key.Syntax.Fields[i];
			if (!fieldSymbol.Type.IsValidKeyType())
			{
				diagnostics.Add(ErrorCode.ERR_FieldTypeCannotBeUsedInKey, identifierNameSyntax.Location, fieldSymbol.Name, tableName, fieldSymbol.Type.Name);
			}
			if ((!flag & flag2) && fieldSymbol.GetEnumPropertyValue<ObsoleteStateKind>(PropertyKind.ObsoleteState) == ObsoleteStateKind.Removed)
			{
				diagnostics.Add(ErrorCode.ERR_ObsoleteFieldCannotBeUsedInKey, identifierNameSyntax.Location, fieldSymbol.Name);
			}
		}
	}

	private static void CheckKeyMaxFields(SourceKeySymbol key, string tableName, DiagnosticBag diagnostics)
	{
		if (key.Fields.Length > 32)
		{
			diagnostics.Add(ErrorCode.ERR_TableKeyTooManyFields, key.Syntax.Location, key.Name, tableName);
		}
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidTableTrigger(name) && !TriggerEventDefinitions.IsValidTableTriggerEvent(name) && !TriggerDefinitions.IsValidFieldTrigger(name))
		{
			return TriggerDefinitions.IsValidFieldExtensionTrigger(name);
		}
		return true;
	}
}
