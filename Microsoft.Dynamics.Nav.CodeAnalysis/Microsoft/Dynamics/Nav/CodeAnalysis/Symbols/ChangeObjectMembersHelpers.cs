using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class ChangeObjectMembersHelpers
{
	internal static void CheckForNoChange(ImmutableArray<ChangeModifySymbol> modifySymbols, ImmutableArray<ChangeMoveSymbol> moveSymbols, CompilationOptions options, DiagnosticBag diagnostics)
	{
		foreach (ChangeModifySymbol item in modifySymbols)
		{
			if (item.Properties.IsEmpty && item.Triggers.IsEmpty)
			{
				diagnostics.Add(ErrorCode.ERR_ModifyContainsNoChange, item.GetLocation(), item.Name);
			}
		}
		if (options.Target.IsPersonalizationTarget())
		{
			return;
		}
		foreach (ChangeMoveSymbol item2 in moveSymbols)
		{
			if (item2.MovedSymbols.IsEmpty)
			{
				diagnostics.Add(ErrorCode.ERR_MissingTargetsForMove, item2.GetLocation());
			}
		}
	}

	internal static void CheckForMultipleModify(ImmutableArray<ChangeModifySymbol> modifySymbols, DiagnosticBag diagnostics)
	{
		PooledList<ChangeModifySymbol> instance = PooledList<ChangeModifySymbol>.GetInstance();
		PooledList<ChangeModifySymbol> instance2 = PooledList<ChangeModifySymbol>.GetInstance();
		PooledList<ChangeModifySymbol> instance3 = PooledList<ChangeModifySymbol>.GetInstance();
		try
		{
			foreach (ChangeModifySymbol item in modifySymbols)
			{
				switch (item.ChangeTargetKind)
				{
				case SymbolKind.Action:
					instance.Add(item);
					break;
				case SymbolKind.Control:
					instance2.Add(item);
					break;
				case SymbolKind.View:
					instance3.Add(item);
					break;
				}
			}
			ObjectMembers<ObjectTypeSymbol>.CheckMembersForDuplicateNames(instance, (ChangeModifySymbol s) =>
			{
				diagnostics.Add(ErrorCode.ERR_CannotModifyMultipleTimes, s.GetNameLocation(), s.Target?.Kind);
			});
			ObjectMembers<ObjectTypeSymbol>.CheckMembersForDuplicateNames(instance2, (ChangeModifySymbol s) =>
			{
				diagnostics.Add(ErrorCode.ERR_CannotModifyMultipleTimes, s.GetNameLocation(), s.Target?.Kind);
			});
			ObjectMembers<ObjectTypeSymbol>.CheckMembersForDuplicateNames(instance3, (ChangeModifySymbol s) =>
			{
				diagnostics.Add(ErrorCode.ERR_CannotModifyMultipleTimes, s.GetNameLocation(), s.Target?.Kind);
			});
		}
		finally
		{
			instance2.Free();
			instance.Free();
			instance3.Free();
		}
	}

	internal static void CheckForMoveAcrossAreas(ImmutableArray<ChangeMoveSymbol> moveSymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeMoveSymbol item in moveSymbols)
		{
			if (!item.Anchor.IsKind(SymbolKind.Control))
			{
				continue;
			}
			ControlSymbol control = (ControlSymbol)item.Anchor;
			foreach (Symbol movedSymbol in item.MovedSymbols)
			{
				if (movedSymbol.IsKind(SymbolKind.Control))
				{
					ControlSymbol controlSymbol = (ControlSymbol)movedSymbol;
					AreaKind area = controlSymbol.GetArea();
					AreaKind area2 = control.GetArea();
					if (area != area2 || area == AreaKind.None)
					{
						diagnostics.Add(ErrorCode.ERR_CannotMoveSymbolsBetweenAreas, ChangeSymbolHelpers.GetMovedSymbolSyntax(item, controlSymbol)?.GetLocation(), controlSymbol.Name, area, area2);
					}
				}
			}
		}
	}

	internal static void CheckForMultipleMoveInSameMove(ImmutableArray<ChangeMoveSymbol> moveSymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeMoveSymbol item in moveSymbols)
		{
			foreach (ImmutableArray<Symbol> value in item.MovedSymbols.ToDictionaryOfArrays((Symbol m) => m.Name, SemanticFacts.NameEqualityComparer).Values)
			{
				if (value.Length > 1)
				{
					foreach (Symbol item2 in value)
					{
						Location location = ChangeSymbolHelpers.GetMovedSymbolSyntax(item, item2)?.GetLocation();
						diagnostics.Add(ErrorCode.ERR_CannotMoveMultipleTimes, location, item2.Kind.ToString().ToLowerInvariant());
					}
				}
			}
		}
	}

	internal static void CheckForMoveRelativeToItself(ImmutableArray<ChangeMoveSymbol> moveSymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeMoveSymbol item in moveSymbols)
		{
			foreach (Symbol movedSymbol in item.MovedSymbols)
			{
				if (SemanticFacts.IsSameName(movedSymbol.Name, item.Anchor?.Name))
				{
					diagnostics.Add(ErrorCode.ERR_CannotMoveRelativeToItself, item.GetLocation(), movedSymbol.Name);
				}
			}
		}
	}

	internal static void CheckForOnlyAdd(ImmutableArray<ChangeAddSymbol> addSymbols, ImmutableArray<ChangeModifySymbol> modifySymbols, ImmutableArray<ChangeMoveSymbol> moveSymbols, DiagnosticBag diagnostics)
	{
		foreach (ChangeAddSymbol item in addSymbols)
		{
			foreach (Symbol addedSymbol in item.AddedSymbols)
			{
				ImmutableArray<Symbol> immutableArray = ImmutableArray<Symbol>.Empty;
				ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
				try
				{
					foreach (ChangeMoveSymbol item2 in moveSymbols)
					{
						foreach (Symbol movedSymbol in item2.MovedSymbols)
						{
							if (SemanticFacts.IsSameName(movedSymbol.Name, addedSymbol.Name))
							{
								instance.Add(movedSymbol);
							}
						}
					}
					foreach (ChangeModifySymbol item3 in modifySymbols)
					{
						if (SemanticFacts.IsSameName(item3.Name, addedSymbol.Name))
						{
							instance.Add(item3);
						}
					}
					immutableArray = instance.ToImmutable();
				}
				finally
				{
					instance.Free();
				}
				foreach (Symbol item4 in immutableArray)
				{
					Location location = (item4 as ChangeModifySymbol)?.GetNameLocation() ?? item4.GetLocation();
					diagnostics.Add(ErrorCode.ERR_CannotMoveOrModifyInTheSameExtensionYouAdded, location);
				}
				if (immutableArray.Length > 0)
				{
					diagnostics.Add(ErrorCode.ERR_CannotMoveOrModifyInTheSameExtensionYouAdded, addedSymbol.GetLocation());
				}
			}
		}
	}

	internal static void CheckForMultipleAdd(ImmutableArray<ChangeAddSymbol> addSymbols, DiagnosticBag diagnostics)
	{
		PooledList<Symbol> instance = PooledList<Symbol>.GetInstance();
		try
		{
			GetSymbolAndAllChildren(addSymbols, instance);
			ObjectMembers<ObjectTypeSymbol>.CheckMembersForDuplicateNames(instance, (Symbol s) =>
			{
				diagnostics.Add(ErrorCode.ERR_CannotAddMultipleTimes, s.GetLocation(), s.Kind);
			});
		}
		finally
		{
			instance.Free();
		}
	}

	private static void GetSymbolAndAllChildren(ImmutableArray<ChangeAddSymbol> addSymbols, PooledList<Symbol> symbols)
	{
		foreach (ChangeAddSymbol item in addSymbols)
		{
			ImmutableArray<Symbol> addedSymbols = item.AddedSymbols;
			int count = symbols.Count;
			symbols.AddRange(addedSymbols);
			for (int i = count; i < symbols.Count; i++)
			{
				if (!(symbols[i] is ContainerSymbol containerSymbol))
				{
					continue;
				}
				foreach (Symbol member in containerSymbol.GetMembers())
				{
					if (member.Kind == SymbolKind.Control || member.Kind == SymbolKind.Action)
					{
						symbols.Add(member);
					}
				}
			}
		}
	}
}
