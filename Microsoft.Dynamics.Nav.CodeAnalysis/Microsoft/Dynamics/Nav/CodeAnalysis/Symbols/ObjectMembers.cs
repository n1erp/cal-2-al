using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal class ObjectMembers<T> : IObjectMembers where T : ObjectTypeSymbol
{
	private class DistincModuleComparer<TMember> : IEqualityComparer<TMember> where TMember : Symbol
	{
		public bool Equals(TMember x, TMember y)
		{
			return (object)x.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>() == y.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		}

		public int GetHashCode(TMember obj)
		{
			return obj.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>()?.GetHashCode() ?? 0;
		}
	}

	private class DeclaredMembers
	{
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		internal readonly ImmutableArray<Symbol> Members;

		internal DeclaredMembers(ImmutableArray<Symbol> members)
		{
			Members = members;
		}
	}

	private bool flattenedMembersIsSorted;

	private int lastMemberId;

	private ImmutableArray<VariableSymbol> lazyGlobalVariables;

	private DeclaredMembers lazyMembers;

	private Dictionary<string, ImmutableArray<Symbol>> lazyMembersDictionary;

	private Dictionary<string, ImmutableArray<Symbol>> lazyCodeMembersDictionary;

	private ImmutableArray<Symbol> lazyMembersFlattened;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private ImmutableSortedDictionary<PropertyKind, PropertySymbol> lazyPropertyByKind;

	private ImmutableArray<MethodSymbol> lazyTriggers;

	private SymbolCompletionState state;

	public string Name { get; }

	internal virtual ObjectSyntax Declaration { get; }

	internal T ContainingSymbol { get; }

	protected virtual bool MethodsMustHaveBody => true;

	public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public ImmutableArray<Location> Locations { get; }

	public CompletionPart NextIncompletePart => state.NextIncompletePart;

	internal ObjectMembers(T containingSymbol, ObjectSyntax declaration)
	{
		ContainingSymbol = containingSymbol;
		Declaration = declaration;
		DeclaringSyntaxReferences = ImmutableArray.Create(declaration.GetReference());
		Locations = ImmutableArray.Create(declaration.Name?.Location ?? declaration.Location);
		Name = ((declaration.Name != null) ? declaration.Name.Identifier.ValueText.UnquoteIdentifier() : string.Empty);
		lastMemberId = 0;
	}

	public ImmutableArray<VariableSymbol> GetGlobalVariables()
	{
		if (lazyGlobalVariables != null)
		{
			return lazyGlobalVariables;
		}
		ImmutableArray<VariableSymbol> value = (from s in GetMembers()
			where s.Kind == SymbolKind.GlobalVariable
			select s).Cast<VariableSymbol>().ToImmutableArray();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyGlobalVariables, value, default);
		return lazyGlobalVariables;
	}

	public ImmutableArray<Symbol> GetMembers()
	{
		if (flattenedMembersIsSorted)
		{
			return lazyMembersFlattened;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length > 1)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref lazyMembersFlattened, immutableArray);
		}
		flattenedMembersIsSorted = true;
		return immutableArray;
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetMembersByName()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return lazyMembersDictionary;
		}
		return GetMembersByNameSlow();
	}

	public ImmutableArray<Symbol> GetMembers(string name)
	{
		if (GetMembersByName().TryGetValue(name, out var value))
		{
			return value;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	public ImmutableArray<PropertySymbol> GetProperties()
	{
		LazyGetProperties();
		return lazyProperties;
	}

	public ImmutableArray<MethodSymbol> GetTriggers()
	{
		if (lazyTriggers != null)
		{
			return lazyTriggers;
		}
		ImmutableArray<MethodSymbol> value = (from MethodSymbol m in from s in GetMembers()
				where s.Kind == SymbolKind.Method
				select s
			where m.MethodKind == MethodKind.Trigger
			select m).ToImmutableArrayOrEmpty();
		ImmutableInterlocked.InterlockedExchange(ref lazyTriggers, value);
		return lazyTriggers;
	}

	public void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		if (HasComplete(CompletionPart.Variables))
		{
			return;
		}
		try
		{
			foreach (PropertySymbol property in GetProperties())
			{
				cancellationToken.ThrowIfCancellationRequested();
				property.ForceComplete(locationOpt, cancellationToken);
			}
		}
		finally
		{
			NotePartComplete(CompletionPart.Variables);
		}
	}

	public void ForceMembersCompleteAndCheckUniqueness(SourceLocation locationOpt, DiagnosticBag diagnostics, CancellationToken cancellationToken, Predicate<SymbolKind> excludeFromNameUniqueness = null)
	{
		ImmutableArray<Symbol> members = GetMembers();
		foreach (Symbol item in members)
		{
			cancellationToken.ThrowIfCancellationRequested();
			item.ForceComplete(locationOpt, cancellationToken);
		}
		PooledList<MethodSymbol> instance = PooledList<MethodSymbol>.GetInstance();
		try
		{
			foreach (KeyValuePair<string, ImmutableArray<Symbol>> item2 in GetMembersByName())
			{
				cancellationToken.ThrowIfCancellationRequested();
				instance.Clear();
				bool flag = false;
				foreach (Symbol item3 in item2.Value)
				{
					if ((excludeFromNameUniqueness != null && excludeFromNameUniqueness(item3.Kind)) || item3.IsTrigger())
					{
						continue;
					}
					if (IsNameRestrictedForTriggers(item3.Name))
					{
						diagnostics.Add(ErrorCode.WRN_ERR_MemberNameOnlyAllowedForTriggers, item3.Locations.IsEmpty ? Location.None : item3.Locations[0], item3.Name);
						continue;
					}
					bool flag2 = item3.Kind != SymbolKind.Method;
					if (!flag2)
					{
						MethodSymbol methodSymbol = (MethodSymbol)item3;
						flag2 = methodSymbol.IsEvent || methodSymbol.IsEventSubscriber();
					}
					if (flag2)
					{
						if (flag || instance.Count > 0)
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateMemberName, item3.Locations.IsEmpty ? Location.None : item3.Locations[0], item3.Name);
						}
						flag = true;
						continue;
					}
					if (flag)
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateMemberName, item3.Locations.IsEmpty ? Location.None : item3.Locations[0], item3.Name);
					}
					MethodSymbol methodSymbol2 = item3 as MethodSymbol;
					if (instance.Count > 0)
					{
						CheckOverloadUniqueness(instance, methodSymbol2, diagnostics);
					}
					instance.Add(methodSymbol2);
				}
			}
		}
		finally
		{
			instance.Free();
		}
		CheckMembersForDuplicateIds(members, diagnostics, (Symbol m) => m.Kind == SymbolKind.Method && !((MethodSymbol)m).IsLocal, ErrorCode.ERR_DuplicateMemberId);
	}

	private static void CheckOverloadUniqueness(PooledList<MethodSymbol> methods, MethodSymbol method, DiagnosticBag diagnostics)
	{
		foreach (MethodSymbol method2 in methods)
		{
			if (method2.ParameterCount != method.ParameterCount)
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < method.ParameterCount; i++)
			{
				if (!CompareOverloadParameter(method2.Parameters[i], method.Parameters[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				diagnostics.Add(ErrorCode.ERR_MemberAlreadyExists, method.GetLocation(), method.GetContainingSymbolOfType<ObjectTypeSymbol>(), method.Name);
			}
		}
	}

	public static bool CompareOverloadParameter(ParameterSymbol parameterA, ParameterSymbol parameterB)
	{
		if (parameterA.Type.NavTypeKind != parameterB.Type.NavTypeKind)
		{
			return false;
		}
		if (parameterA.Type.IsArray() != parameterB.Type.IsArray())
		{
			return false;
		}
		if (parameterA.Type.TypeCategory == TypeCategoryKind.ApplicationObject || (parameterA.Type.Kind == SymbolKind.Enum && parameterA.Type.NavTypeKind == NavTypeKind.Enum))
		{
			ApplicationObjectTypeSymbol obj = (parameterA.Type as ApplicationObjectTypeSymbol) ?? (parameterA.Type.OriginalDefinition as ApplicationObjectTypeSymbol);
			ApplicationObjectTypeSymbol applicationObjectTypeSymbol = (parameterB.Type as ApplicationObjectTypeSymbol) ?? (parameterB.Type.OriginalDefinition as ApplicationObjectTypeSymbol);
			if (obj.Id != applicationObjectTypeSymbol.Id)
			{
				return false;
			}
		}
		if (parameterA.Type.IsGenericType() && parameterB.Type.IsGenericType())
		{
			return parameterA.Type.Equals(parameterB.Type);
		}
		return true;
	}

	public static bool CheckMembersForDuplicates(ImmutableArray<Symbol> members, DiagnosticBag diagnostics, SymbolKind kind, ErrorCode errorCodeName, ErrorCode errorCodeId)
	{
		return CheckMembersForDuplicates(members, diagnostics, kind, errorCodeName, errorCodeId, crossModule: false);
	}

	public static bool CheckMembersForDuplicates(ImmutableArray<Symbol> members, DiagnosticBag diagnostics, SymbolKind kind, ErrorCode errorCodeName, ErrorCode errorCodeId, bool crossModule)
	{
		return CheckMembersForDuplicateNames(members, diagnostics, (Symbol f) => f.Kind == kind, errorCodeName, null, crossModule) | CheckMembersForDuplicateIds(members, diagnostics, (Symbol f) => f.Kind == kind, errorCodeId);
	}

	protected internal static void CheckMembersForDuplicateNames<TMember>(IEnumerable<TMember> symbols, Action<TMember> applyDiagnostics) where TMember : Symbol
	{
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			foreach (TMember symbol in symbols)
			{
				string name = symbol.Name;
				if (!instance.Add(name))
				{
					instance2.Add(name);
					applyDiagnostics(symbol);
				}
			}
			foreach (TMember symbol2 in symbols)
			{
				string name2 = symbol2.Name;
				if (instance2.Remove(name2))
				{
					applyDiagnostics(symbol2);
				}
				if (instance2.Count == 0)
				{
					break;
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	public PropertySymbol GetProperty(PropertyKind kind)
	{
		LazyGetProperties();
		lazyPropertyByKind.TryGetValue(kind, out var value);
		return value;
	}

	protected static bool CheckMembersForDuplicateIds<TMember>(ImmutableArray<TMember> members, DiagnosticBag diagnostics, Func<TMember, bool> filter, ErrorCode errorCode) where TMember : Symbol
	{
		bool result = false;
		PooledHashSet<int> instance = PooledHashSet<int>.GetInstance();
		PooledHashSet<int> instance2 = PooledHashSet<int>.GetInstance();
		try
		{
			IEnumerable<ISymbolWithId> enumerable = members.Where(filter).Cast<ISymbolWithId>();
			foreach (ISymbolWithId item in enumerable)
			{
				if (!instance.Add(item.Id))
				{
					instance2.Add(item.Id);
				}
			}
			foreach (int id in instance2)
			{
				IEnumerable<ISymbolWithId> enumerable2 = enumerable.Where((ISymbolWithId m) => m.Id == id);
				bool flag = true;
				ISymbolWithId symbolWithId = enumerable2.First();
				foreach (ISymbolWithId item2 in enumerable2.Skip(1))
				{
					if (!SemanticFacts.IsSameName(item2.Name, symbolWithId.Name))
					{
						flag = false;
						break;
					}
					symbolWithId = item2;
				}
				if (flag)
				{
					continue;
				}
				foreach (ISymbolWithId item3 in enumerable2)
				{
					if (!item3.Locations.IsEmpty)
					{
						result = true;
						if (item3.IsKind(SymbolKind.Field))
						{
							SourceFieldSymbol sourceFieldSymbol = (SourceFieldSymbol)item3;
							diagnostics.Add(errorCode, new SourceLocation(sourceFieldSymbol.DeclaringSyntax.No), item3.Id);
						}
						else
						{
							diagnostics.Add(errorCode, item3.Locations[0], item3.Id);
						}
					}
				}
			}
			return result;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	internal virtual int GetNextMemberId()
	{
		return Interlocked.Increment(ref lastMemberId);
	}

	protected virtual void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
	}

	protected virtual TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}

	protected virtual ImmutableArray<Symbol> GetBaseMembers()
	{
		if (!(ContainingSymbol.BaseType != null))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return ContainingSymbol.BaseType.GetMembers();
	}

	protected virtual void CheckMembers(DiagnosticBag diagnostics)
	{
		SyntaxNode firstSyntax = ContainingSymbol.GetFirstSyntax();
		DebugAssertHelper.Assert(firstSyntax != null, "Containing symbol must have syntax");
		ImmutableArray<Symbol> baseMembers = GetBaseMembers();
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		PooledList<MethodSymbol> instance2 = PooledList<MethodSymbol>.GetInstance();
		try
		{
			ImmutableArray<Symbol> members = GetMembers();
			PopulatePublicMethodNames(members, instance);
			PopulatePublicMethods(baseMembers, instance2, instance);
			foreach (Symbol item in members)
			{
				SyntaxNode firstSyntax2 = item.GetFirstSyntax();
				if (firstSyntax2 != null && !SyntaxFacts.IsSyntaxAllowed(firstSyntax, firstSyntax2.Kind))
				{
					diagnostics.Add(ErrorCode.ERR_NotAllowedSyntax, item.GetLocation(), item.Name, ContainingSymbol.Kind);
				}
				if (item.Kind == SymbolKind.Method)
				{
					CheckMethod((MethodSymbol)item, instance2, diagnostics);
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private void CheckMethod(MethodSymbol method, IList<MethodSymbol> baseMethods, DiagnosticBag diagnostics)
	{
		if (MethodsMustHaveBody && method.MethodKind == MethodKind.DeclareMethod)
		{
			diagnostics.Add(ErrorCode.ERR_MethodBodyMustBeSpecified, method.GetLocation(), method.Name);
		}
		else if (!MethodsMustHaveBody && method.MethodKind == MethodKind.Method)
		{
			diagnostics.Add(ErrorCode.ERR_MethodBodyNotAllowed, method.GetLocation(), method.Name);
		}
		foreach (MethodSymbol baseMethod in baseMethods)
		{
			if (method.HasMatchingSignature(baseMethod))
			{
				ObjectTypeSymbol containingSymbolOfType = baseMethod.GetContainingSymbolOfType<ObjectTypeSymbol>();
				if (containingSymbolOfType == null)
				{
					diagnostics.Add(ErrorCode.WRN_MemberAlreadyDeclaredInBase, method.GetLocation(), method.Name);
				}
				else
				{
					diagnostics.Add(ErrorCode.WRN_MemberAlreadyExists, method.GetLocation(), containingSymbolOfType, method.Name);
				}
				break;
			}
		}
	}

	protected virtual void CheckObject(DiagnosticBag diagnostics)
	{
	}

	internal ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> result = lazyMembersFlattened;
		if (result.IsDefault)
		{
			result = GetMembersByName().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref lazyMembersFlattened, result);
			result = lazyMembersFlattened;
		}
		return result;
	}

	internal static bool CheckMembersForDuplicateNames<TMember>(IEnumerable<TMember> members, DiagnosticBag diagnostics, Func<TMember, bool> filter, ErrorCode errorCode, Func<TMember, string> nameGetter = null, bool crossModule = false, ErrorCode warningCode = ErrorCode.Unknown, Func<IEnumerable<TMember>, TMember, bool> isWarning = null) where TMember : Symbol
	{
		bool result = false;
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			nameGetter = nameGetter ?? ((Func<TMember, string>)((TMember member) => member.Name));
			isWarning = isWarning ?? ((Func<IEnumerable<TMember>, TMember, bool>)((IEnumerable<TMember> all, TMember curr) => false));
			IEnumerable<TMember> enumerable = members.Where(filter);
			foreach (TMember item2 in enumerable)
			{
				string item = nameGetter(item2);
				if (!instance.Add(item))
				{
					instance2.Add(item);
				}
			}
			DistincModuleComparer<TMember> comparer = new DistincModuleComparer<TMember>();
			foreach (string name in instance2)
			{
				IEnumerable<TMember> enumerable2 = enumerable.Where((TMember f) => SemanticFacts.IsSameName(nameGetter(f), name));
				if (crossModule && enumerable2.Distinct(comparer).HasLessThan(2))
				{
					continue;
				}
				foreach (TMember item3 in enumerable2)
				{
					if (!item3.Locations.IsEmpty)
					{
						result = true;
						ErrorCode code = (isWarning(enumerable2, item3) ? warningCode : errorCode);
						diagnostics.Add(code, item3.Locations[0], nameGetter(item3));
					}
				}
			}
			return result;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	internal static bool CheckMemberGroupsForDuplicateNames<TMember, TKey>(IEnumerable<TMember> members, DiagnosticBag diagnostics, Func<TMember, TKey> groupBy, ErrorCode errorCode) where TMember : Symbol
	{
		bool result = false;
		IGrouping<TKey, TMember>[] array = members.GroupBy(groupBy).ToArray();
		PooledList<PooledNameComparisonHashSet> instance = PooledList<PooledNameComparisonHashSet>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array.Length != 0)
				{
					instance.Add(PooledNameComparisonHashSet.GetInstance());
					foreach (TMember item in array[i])
					{
						instance[i].Add(item.Name);
					}
				}
				else
				{
					instance.Add(null);
				}
			}
			for (int j = 0; j < instance.Count - 1; j++)
			{
				for (int k = j + 1; k < instance.Count; k++)
				{
					if (instance[j] != null && instance[k] != null)
					{
						IEnumerable<string> enumerable = instance[j].Intersect(instance[k]);
						if (enumerable.Any())
						{
							instance2.AddAll(enumerable);
						}
					}
				}
			}
			foreach (TMember member in members)
			{
				if (instance2.Contains(member.Name) && !member.Locations.IsEmpty)
				{
					result = true;
					diagnostics.Add(errorCode, member.Locations[0], member.Name);
				}
			}
			return result;
		}
		finally
		{
			instance2.Free();
			foreach (PooledNameComparisonHashSet item2 in instance)
			{
				item2?.Free();
			}
			instance.Free();
		}
	}

	internal virtual ImmutableArray<Symbol> GetCodeMembers()
	{
		return GetMembers();
	}

	internal virtual ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		if (lazyCodeMembersDictionary == null)
		{
			Dictionary<string, ImmutableArray<Symbol>> value = GetCodeMembers().ToDictionaryOfArrays((Symbol s) => s.Name, SemanticFacts.NameEqualityComparer);
			Interlocked.CompareExchange(ref lazyCodeMembersDictionary, value, null);
		}
		if (!lazyCodeMembersDictionary.TryGetValue(name, out var value2))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value2;
	}

	internal void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken, Predicate<SymbolKind> excludeFromNameUniqueness = null)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				break;
			case CompletionPart.Members:
				GetMembersByName();
				break;
			case CompletionPart.MembersCompleted:
				ForceCompleteMembers(locationOpt, cancellationToken, excludeFromNameUniqueness);
				break;
			case CompletionPart.None:
				return;
			default:
				NotePartComplete((CompletionPart)130863);
				break;
			}
			SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken, Predicate<SymbolKind> excludeFromNameUniqueness = null)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			CheckObject(instance);
			ForceMembersCompleteAndCheckUniqueness(locationOpt, instance, cancellationToken, excludeFromNameUniqueness);
			GetGlobalVariables();
			CheckTriggers(ContainingSymbol, instance);
			CheckMembers(instance);
			if (NotePartComplete(CompletionPart.MembersCompleted))
			{
				ContainingSymbol.AddDeclarationDiagnostics(instance);
				ContainingSymbol.DeclaringCompilation.SymbolDeclaredEvent(ContainingSymbol);
			}
		}
		finally
		{
			instance.Free();
		}
	}

	protected virtual bool IsNameRestrictedForTriggers(string name)
	{
		return false;
	}

	private void LazyGetProperties()
	{
		if (lazyPropertyByKind != null)
		{
			return;
		}
		lock (Declaration)
		{
			ImmutableArray<PropertySymbol> value = Declaration.PropertyList?.Properties.CreatePropertySymbols(ContainingSymbol) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
			ImmutableSortedDictionary<PropertyKind, PropertySymbol> value2 = lazyProperties.ToDictionaryDistinct((PropertySymbol p) => p.PropertyKind).ToImmutableSortedDictionary();
			Interlocked.CompareExchange(ref lazyPropertyByKind, value2, null);
		}
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetMembersByNameSlow()
	{
		if (lazyMembersDictionary == null)
		{
			GlobalLogger.LogVerbose("Binding the members for {0}.", ContainingSymbol);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				Dictionary<string, ImmutableArray<Symbol>> value = GetDeclaredMembers().Members.ToDictionaryOfArrays((Symbol s) => s.Name, SemanticFacts.NameEqualityComparer);
				if (Interlocked.CompareExchange(ref lazyMembersDictionary, value, null) == null)
				{
					ContainingSymbol.AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.Members);
				}
			}
			finally
			{
				instance.Free();
			}
		}
		state.SpinWaitComplete(CompletionPart.Members, default);
		return lazyMembersDictionary;
	}

	private DeclaredMembers GetDeclaredMembers()
	{
		DeclaredMembers declaredMembers = lazyMembers;
		if (declaredMembers != null)
		{
			return declaredMembers;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			declaredMembers = BuildDeclaredMembers(instance);
			DeclaredMembers declaredMembers2 = Interlocked.CompareExchange(ref lazyMembers, declaredMembers, null);
			if (declaredMembers2 != null)
			{
				return declaredMembers2;
			}
			ContainingSymbol.AddDeclarationDiagnostics(instance);
			return declaredMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	private DeclaredMembers BuildDeclaredMembers(DiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		ImmutableArray<Symbol> members;
		try
		{
			BuildDeclaredMembersCore(instance, diagnostics);
			if (Declaration != null)
			{
				PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
				try
				{
					foreach (MemberSyntax member in Declaration.Members)
					{
						if (lazyMembers != null)
						{
							return lazyMembers;
						}
						switch (member.Kind)
						{
						case SyntaxKind.VarSection:
							AddVariables((VarSectionSyntax)member, instance);
							break;
						case SyntaxKind.TriggerDeclaration:
							instance.Add(SourceTriggerSymbol.Create(ContainingSymbol, (TriggerDeclarationSyntax)member));
							break;
						case SyntaxKind.EventTriggerDeclaration:
							instance.Add(SourceDotNetEventTriggerSymbol.Create(ContainingSymbol, (EventTriggerDeclarationSyntax)member));
							break;
						case SyntaxKind.MethodDeclaration:
						{
							MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)member;
							string item = methodDeclarationSyntax.Name.Unquoted();
							bool isOverload = !instance2.Add(item);
							instance.Add(new SourceMethodSymbol(ContainingSymbol, methodDeclarationSyntax, isOverload));
							break;
						}
						case SyntaxKind.EventDeclaration:
							instance.Add(new SourceEventSymbol(ContainingSymbol, (EventDeclarationSyntax)member));
							break;
						}
					}
				}
				finally
				{
					instance2.Free();
				}
			}
		}
		finally
		{
			members = instance.ToImmutableAndFree();
		}
		if (lazyMembers != null)
		{
			return null;
		}
		return new DeclaredMembers(members);
	}

	private void AddVariables(VarSectionSyntax varSectionSyntax, ArrayBuilder<Symbol> members)
	{
		foreach (VariableDeclarationSyntax variable in varSectionSyntax.Variables)
		{
			members.Add(SourceGlobalVariableSymbol.Create(ContainingSymbol, variable));
		}
	}

	private void CheckTriggers(Symbol containingSymbol, DiagnosticBag diagnostics)
	{
		TriggerValidationHelper.CheckTriggers(containingSymbol, GetTriggers(), GetTriggerTypeInfo, diagnostics);
	}

	public bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	public bool NotePartComplete(CompletionPart part)
	{
		return state.NotePartComplete(part);
	}

	public void SpinWaitComplete(CompletionPart part, CancellationToken cancellationToken)
	{
		state.SpinWaitComplete(part, cancellationToken);
	}

	protected static void PopulatePublicMethodNames(IEnumerable<Symbol> members, PooledNameComparisonHashSet output)
	{
		foreach (Symbol member in members)
		{
			if (member.Kind == SymbolKind.Method && member.DeclaredAccessibility == Accessibility.Public)
			{
				output.Add(member.Name);
			}
		}
	}

	protected static void PopulatePublicMethods(IEnumerable<Symbol> symbols, PooledList<MethodSymbol> output, ISet<string> nameWhiteList = null)
	{
		foreach (Symbol symbol in symbols)
		{
			if (symbol.Kind == SymbolKind.Method && symbol.DeclaredAccessibility == Accessibility.Public && (nameWhiteList == null || nameWhiteList.Contains(symbol.Name)))
			{
				output.Add((MethodSymbol)symbol);
			}
		}
	}
}
