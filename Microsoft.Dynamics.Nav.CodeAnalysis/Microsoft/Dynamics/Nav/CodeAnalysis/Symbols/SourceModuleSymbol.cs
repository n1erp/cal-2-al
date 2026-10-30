using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceModuleSymbol : ModuleSymbol
{
	private class DeclaredObjects
	{
		private ImmutableArray<ObjectTypeSymbol> lazyObjects;

		internal ImmutableDictionary<SymbolKind, ImmutableArray<ObjectTypeSymbol>> ObjectByKind { get; }

		internal DeclaredObjects(ImmutableArray<ObjectTypeSymbol> codeunits, ImmutableArray<ObjectTypeSymbol> tables, ImmutableArray<ObjectTypeSymbol> tableExtensions, ImmutableArray<ObjectTypeSymbol> pages, ImmutableArray<ObjectTypeSymbol> pageExtensions, ImmutableArray<ObjectTypeSymbol> pageCustomizations, ImmutableArray<ObjectTypeSymbol> reports, ImmutableArray<ObjectTypeSymbol> xmlPorts, ImmutableArray<ObjectTypeSymbol> queries, ImmutableArray<ObjectTypeSymbol> profiles, ImmutableArray<ObjectTypeSymbol> controlAddIns, ImmutableArray<ObjectTypeSymbol> dotNetPackages, ImmutableArray<ObjectTypeSymbol> enumTypes, ImmutableArray<ObjectTypeSymbol> enumExtensionTypes)
		{
			PooledDictionary<SymbolKind, ImmutableArray<ObjectTypeSymbol>> instance = PooledDictionary<SymbolKind, ImmutableArray<ObjectTypeSymbol>>.GetInstance();
			instance.Add(SymbolKind.Codeunit, codeunits);
			instance.Add(SymbolKind.Table, tables);
			instance.Add(SymbolKind.TableExtension, tableExtensions);
			instance.Add(SymbolKind.Page, pages);
			instance.Add(SymbolKind.PageExtension, pageExtensions);
			instance.Add(SymbolKind.PageCustomization, pageCustomizations);
			instance.Add(SymbolKind.Report, reports);
			instance.Add(SymbolKind.XmlPort, xmlPorts);
			instance.Add(SymbolKind.Query, queries);
			instance.Add(SymbolKind.Profile, profiles);
			instance.Add(SymbolKind.ControlAddIn, controlAddIns);
			instance.Add(SymbolKind.DotNetPackage, dotNetPackages);
			instance.Add(SymbolKind.Enum, enumTypes);
			instance.Add(SymbolKind.EnumExtension, enumExtensionTypes);
			AddDotNetTypeDeclarations(instance, dotNetPackages);
			ObjectByKind = instance.ToImmutableDictionaryAndFree();
		}

		private static void AddDotNetTypeDeclarations(PooledDictionary<SymbolKind, ImmutableArray<ObjectTypeSymbol>> builder, ImmutableArray<ObjectTypeSymbol> dotNetPackages)
		{
			ArrayBuilder<ObjectTypeSymbol> instance = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
			foreach (DotNetPackageTypeSymbol item in dotNetPackages)
			{
				instance.AddRange(item.FlattenedTypeDeclarations);
			}
			builder.Add(SymbolKind.DotNetTypeDeclaration, instance.ToImmutableAndFree());
		}

		internal IEnumerable<Symbol> GetObjects()
		{
			if (lazyObjects != null)
			{
				return lazyObjects;
			}
			ArrayBuilder<ObjectTypeSymbol> instance = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
			foreach (ImmutableArray<ObjectTypeSymbol> value in ObjectByKind.Values)
			{
				instance.AddRange(value);
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyObjects, instance.ToImmutableAndFree(), default);
			return lazyObjects;
		}
	}

	private readonly Compilation compilation;

	private SymbolCompletionState state;

	private bool flattenedMembersIsSorted;

	private ImmutableArray<Symbol> lazyMembersFlattened;

	private readonly DeclarationTable sources;

	private ImmutableArray<Location> locations;

	private DeclaredObjects lazyObjects;

	private Dictionary<string, ImmutableArray<Symbol>> lazyMembersDictionary;

	private ImmutableDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>> lazyObjectsById;

	private ImmutableDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> lazyObjectsByName;

	public override string PublisherName { get; }

	public override Version Version { get; }

	public override Guid AppId { get; }

	internal override IDotNetResolver DotNetResolver { get; }

	internal override IReferenceResolver ReferenceResolver => compilation.ReferenceManager;

	internal override Compilation DeclaringCompilation => compilation;

	public override SymbolKind Kind => SymbolKind.Module;

	public override string Name { get; }

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (locations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref locations, ((IEnumerable<Location>)(from n in sources.AllRootObjectCollectionsUnordered()
					select n.Location)).ToImmutableArray(), default);
			}
			return locations;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal SourceModuleSymbol(Compilation compilation, DeclarationTable declarations, string moduleName, string publisherName, Version version, Guid appId, ImmutableArray<IModuleSymbol> referenceModules)
		: base(referenceModules)
	{
		this.compilation = compilation;
		sources = declarations;
		Name = moduleName;
		PublisherName = publisherName;
		Version = version;
		DotNetResolver = compilation.DotNetResolverFactory.CreateResolver();
		AppId = appId;
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Variables:
				GetMembersByName();
				break;
			case CompletionPart.ObjectsCompleted:
			{
				ImmutableArray<Symbol> members = GetMembers();
				bool flag = true;
				if (DeclaringCompilation.Options.ConcurrentBuild)
				{
					ParallelOptions parallelOptions = (cancellationToken.CanBeCanceled ? new ParallelOptions
					{
						CancellationToken = cancellationToken
					} : Compilation.DefaultParallelOptions);
					Parallel.For(0, members.Length, parallelOptions, UICultureUtilities.WithCurrentUICulture((int i) =>
					{
						Symbol member = members[i];
						ForceCompleteMemberByLocation(locationOpt, member, cancellationToken);
					}));
					foreach (Symbol item in members)
					{
						if (!item.HasComplete(CompletionPart.All))
						{
							flag = false;
							break;
						}
					}
				}
				else
				{
					foreach (Symbol item2 in members)
					{
						ForceCompleteMemberByLocation(locationOpt, item2, cancellationToken);
						flag = flag && item2.HasComplete(CompletionPart.All);
					}
				}
				if (flag)
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					try
					{
						CheckObjects(instance);
						CheckDuplicateMemberIdsInExtensions<TableTypeSymbol, TableExtensionTypeSymbol>(SymbolKind.TableExtension, CheckDuplicateMembersIdsForExtendedTargetTable, instance);
						CheckDuplicateMemberIdsInPageExtensionsAndCustomizations(CheckDuplicateMembersIdsForExtendedTargetPage, instance);
						CheckDuplicateMemberIdsInExtensions<EnumTypeSymbol, EnumExtensionTypeSymbol>(SymbolKind.EnumExtension, CheckDuplicateMembersIdsForExtendedTargetEnum, instance);
						if (state.NotePartComplete(CompletionPart.ObjectsCompleted))
						{
							if (!instance.IsEmptyWithoutResolution)
							{
								DeclaringCompilation.DeclarationDiagnostics.AddRange(instance);
							}
							DeclaringCompilation.SymbolDeclaredEvent(this);
						}
					}
					finally
					{
						instance.Free();
					}
					break;
				}
				CompletionPart part = ((!(locationOpt == null)) ? CompletionPart.Variables : CompletionPart.ModuleSymbolAll);
				state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				DebugAssertHelper.Assert((nextIncompletePart & CompletionPart.ModuleSymbolAll) == 0);
				state.NotePartComplete((CompletionPart)131023);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	private static void ForceCompleteMemberByLocation(SourceLocation locationOpt, Symbol member, CancellationToken cancellationToken)
	{
		if (IsMemberCandidateForForceComplete(locationOpt, member, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			member.ForceComplete(locationOpt, cancellationToken);
		}
	}

	private static bool IsMemberCandidateForForceComplete(SourceLocation locationOpt, Symbol member, CancellationToken cancellationToken)
	{
		if (member.Kind != SymbolKind.DotNetTypeDeclaration)
		{
			if (!(locationOpt == null))
			{
				return member.IsDefinedInSourceTree(locationOpt.SourceTree, locationOpt.SourceSpan, cancellationToken);
			}
			return true;
		}
		return false;
	}

	private static void CheckDuplicateMembersIdsForExtendedTargetPage(ImmutableArray<PageExtensionBaseTypeSymbol> pageExtensionsForTarget, PageTypeSymbol target, DiagnosticBag diagnostics)
	{
		PooledList<Symbol> instance = PooledList<Symbol>.GetInstance();
		foreach (PageExtensionBaseTypeSymbol item in pageExtensionsForTarget)
		{
			instance.AddRange(item.GetMembers());
			instance.AddRange(item.FlattenedViews);
			if (item.Kind == SymbolKind.PageExtension)
			{
				PageExtensionTypeSymbol pageExtensionTypeSymbol = (PageExtensionTypeSymbol)item;
				instance.AddRange(pageExtensionTypeSymbol.FlattenedActions);
				instance.AddRange(pageExtensionTypeSymbol.FlattenedControls);
			}
		}
		instance.AddRange(target.GetMembers());
		instance.AddRange(target.FlattenedActions);
		instance.AddRange(target.FlattenedControls);
		instance.AddRange(target.FlattenedViews);
		ImmutableArray<Symbol> members = instance.Where((Symbol m) => !m.IsActionArea() && !m.IsControlArea()).ToImmutableArray();
		instance.Free();
		ObjectMembers<PageExtensionTypeSymbol>.CheckMembersForDuplicates(members, diagnostics, SymbolKind.Action, ErrorCode.ERR_DuplicateMemberName, ErrorCode.ERR_DuplicateMemberIdFromHash, crossModule: true);
		ObjectMembers<PageExtensionTypeSymbol>.CheckMembersForDuplicates(members, diagnostics, SymbolKind.Control, ErrorCode.ERR_DuplicateMemberName, ErrorCode.ERR_DuplicateMemberIdFromHash, crossModule: true);
		ObjectMembers<PageExtensionTypeSymbol>.CheckMembersForDuplicates(members, diagnostics, SymbolKind.View, ErrorCode.ERR_DuplicateMemberName, ErrorCode.ERR_DuplicateMemberIdFromHash, crossModule: true);
	}

	private static void CheckDuplicateMembersIdsForExtendedTargetEnum(ImmutableArray<EnumExtensionTypeSymbol> extensionsForTarget, EnumTypeSymbol target, DiagnosticBag diagnostics)
	{
		PooledList<EnumValueSymbol> instance = PooledList<EnumValueSymbol>.GetInstance();
		try
		{
			foreach (EnumExtensionTypeSymbol item in extensionsForTarget)
			{
				instance.AddRange(item.Values);
			}
			instance.AddRange(target.Values);
			ObjectMembers<EnumExtensionTypeSymbol>.CheckMembersForDuplicates(((IEnumerable<Symbol>)instance).ToImmutableArray(), diagnostics, SymbolKind.EnumValue, ErrorCode.ERR_DuplicateMemberName, ErrorCode.ERR_DuplicateMemberId, crossModule: true);
		}
		finally
		{
			instance.Free();
		}
	}

	private void CheckDuplicateMemberIdsInExtensions<TBase, TExt>(SymbolKind extensionKind, Action<ImmutableArray<TExt>, TBase, DiagnosticBag> typeSpecificChecks, DiagnosticBag diagnostics) where TBase : ApplicationObjectTypeSymbol where TExt : ApplicationObjectExtensionTypeSymbol
	{
		PooledHashSet<int> instance = PooledHashSet<int>.GetInstance();
		try
		{
			IEnumerable<TExt> extensionsInModule = GetDeclaredObjects().ObjectByKind[extensionKind].Cast<TExt>();
			CheckDuplicateMemberIdsInExtensionsForSymbolOfKind(instance, extensionsInModule, extensionKind, typeSpecificChecks, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private void CheckDuplicateMemberIdsInPageExtensionsAndCustomizations(Action<ImmutableArray<PageExtensionBaseTypeSymbol>, PageTypeSymbol, DiagnosticBag> typeSpecificChecks, DiagnosticBag diagnostics)
	{
		PooledHashSet<int> instance = PooledHashSet<int>.GetInstance();
		try
		{
			IEnumerable<PageExtensionBaseTypeSymbol> extensionsInModule = GetDeclaredObjects().ObjectByKind[SymbolKind.PageExtension].Cast<PageExtensionBaseTypeSymbol>();
			CheckDuplicateMemberIdsInExtensionsForSymbolOfKind(instance, extensionsInModule, SymbolKind.PageExtension, typeSpecificChecks, diagnostics);
			IEnumerable<PageExtensionBaseTypeSymbol> extensionsInModule2 = GetDeclaredObjects().ObjectByKind[SymbolKind.PageCustomization].Cast<PageExtensionBaseTypeSymbol>();
			CheckDuplicateMemberIdsInExtensionsForSymbolOfKind(instance, extensionsInModule2, null, typeSpecificChecks, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private void CheckDuplicateMemberIdsInExtensionsForSymbolOfKind<TBase, TExt>(PooledHashSet<int> extendedIds, IEnumerable<TExt> extensionsInModule, SymbolKind? extensionKind, Action<ImmutableArray<TExt>, TBase, DiagnosticBag> typeSpecificChecks, DiagnosticBag diagnostics) where TBase : ApplicationObjectTypeSymbol where TExt : ApplicationObjectExtensionTypeSymbol
	{
		foreach (TExt item in extensionsInModule)
		{
			TBase val = item.Target as TBase;
			if (val != null && !extendedIds.Contains(val.Id))
			{
				extendedIds.Add(val.Id);
				VerifyExtensionsAndCustomizations(val, extensionKind, typeSpecificChecks, diagnostics);
			}
		}
	}

	private void VerifyExtensionsAndCustomizations<TBase, TExt>(TBase target, SymbolKind? symbolKind, Action<ImmutableArray<TExt>, TBase, DiagnosticBag> typeSpecificChecks, DiagnosticBag diagnostics) where TBase : ApplicationObjectTypeSymbol where TExt : ApplicationObjectExtensionTypeSymbol
	{
		ImmutableArray<TExt> immutableArray = DeclaringCompilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target).Cast<TExt>().ToImmutableArray();
		if (symbolKind.HasValue)
		{
			CheckExtensionObjectRestrictions(target, immutableArray, symbolKind.Value, diagnostics);
		}
		typeSpecificChecks(immutableArray, target, diagnostics);
	}

	private void CheckExtensionObjectRestrictions<T>(ApplicationObjectTypeSymbol baseObject, ImmutableArray<T> extensionObjects, SymbolKind symbolKind, DiagnosticBag diagnostics) where T : ApplicationObjectTypeSymbol
	{
		ImmutableArray<T> immutableArray = extensionObjects.WhereAsArray((T e) => e.ContainingModule == this && e.Kind == symbolKind);
		int length = immutableArray.Length;
		bool flag = !compilation.Options.AllowExtensionAndTargetInSameModule && baseObject.ContainingModule == this && length > 0;
		if ((length > 1) | flag)
		{
			foreach (T item in immutableArray)
			{
				diagnostics.Add(ErrorCode.ERR_ExtensionCannotBeDeclared, item.GetLocation(), item.Name, baseObject.Name);
			}
		}
	}

	private static void CheckDuplicateMembersIdsForExtendedTargetTable(ImmutableArray<TableExtensionTypeSymbol> tableExtensionsForTarget, TableTypeSymbol target, DiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (TableExtensionTypeSymbol item in tableExtensionsForTarget)
		{
			instance.AddRange(item.GetMembers());
		}
		instance.AddRange(target.GetMembers());
		ImmutableArray<Symbol> immutableArray = instance.ToImmutableAndFree();
		if (!ObjectMembers<TableExtensionTypeSymbol>.CheckMembersForDuplicates(immutableArray, diagnostics, SymbolKind.Field, ErrorCode.ERR_DuplicateFieldName, ErrorCode.ERR_DuplicateFieldId))
		{
			ObjectMembers<TableExtensionTypeSymbol>.CheckMemberGroupsForDuplicateNames(immutableArray.Where((Symbol m) => m.Kind == SymbolKind.Field || m.Kind == SymbolKind.Method), diagnostics, (Symbol m) => m.Kind, ErrorCode.WRN_ERR_DuplicateMemberName);
		}
	}

	private void CheckObjects(DiagnosticBag diagnostics)
	{
		DeclaredObjects declaredObjects = GetDeclaredObjects();
		IEnumerable<ObjectTypeSymbol> objects = declaredObjects.GetObjects().Cast<ObjectTypeSymbol>();
		CheckObjects(objects, diagnostics);
		CheckObjectUniqueness(declaredObjects, diagnostics);
	}

	private void CheckObjectUniqueness(DeclaredObjects objects, DiagnosticBag diagnostics)
	{
		foreach (SymbolKind key in objects.ObjectByKind.Keys)
		{
			if (IsSubjectToUniquenessConstraints(key))
			{
				CheckUniquenessWithinType(objects.ObjectByKind[key], diagnostics);
			}
		}
	}

	private static bool IsSubjectToUniquenessConstraints(SymbolKind kind)
	{
		return kind != SymbolKind.DotNetPackage;
	}

	private void CheckObjects(IEnumerable<ObjectTypeSymbol> objects, DiagnosticBag diagnostics)
	{
		foreach (ObjectTypeSymbol @object in objects)
		{
			ISymbolWithId symbolWithId = @object as ISymbolWithId;
			CheckSymbolWithIdIsWithinAllowedRange(symbolWithId, diagnostics);
			CheckObjectCompilationScope(@object, diagnostics);
		}
	}

	private void CheckObjectCompilationScope(ObjectTypeSymbol applicationObject, DiagnosticBag diagnostics)
	{
		if (compilation.Options.Target.IsPersonalizationTarget() && !applicationObject.IsPersonalizationObject())
		{
			ReportInvalidSymbolUse(applicationObject, compilation.Options.Target, diagnostics);
		}
		else if (!applicationObject.CanCompileWithTarget(compilation.Options.Target))
		{
			ReportInvalidSymbolUse(applicationObject, compilation.Options.Target, diagnostics);
		}
	}

	private static void ReportInvalidSymbolUse(ObjectTypeSymbol applicationObject, CompilationTarget target, DiagnosticBag diagnostics)
	{
		ObjectSyntax objectSyntax = applicationObject.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ObjectSyntax;
		diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, objectSyntax?.Name?.GetLocation() ?? objectSyntax?.Location ?? Location.None, applicationObject.Name, target.ToString());
	}

	private void CheckSymbolWithIdIsWithinAllowedRange(ISymbolWithId symbolWithId, DiagnosticBag diagnostics)
	{
		if (symbolWithId != null)
		{
			int id = symbolWithId.Id;
			IdSpaces idSpaces = DeclaringCompilation.Options.IdSpaces;
			if (!idSpaces.IsObjectIdAllowedForApplicationObject(id, symbolWithId.Kind))
			{
				diagnostics.Add(ErrorCode.ERR_ApplicationObjectIdentifierOutOfBounds, (symbolWithId.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ApplicationObjectSyntax)?.ObjectId.GetLocation() ?? Location.None, symbolWithId.Id, idSpaces.AllowedRanges);
			}
		}
	}

	private void CheckUniquenessWithinType(ImmutableArray<ObjectTypeSymbol> symbols, DiagnosticBag diagnostics)
	{
		CheckForDuplicateNames(symbols, diagnostics);
		ObjectTypeSymbol objectTypeSymbol = symbols.FirstOrDefault();
		if ((object)objectTypeSymbol == null || objectTypeSymbol.Kind != SymbolKind.PageCustomization)
		{
			CheckForDuplicateIds(symbols.OfType<ISymbolWithId>().ToImmutableArray(), diagnostics);
		}
	}

	private void CheckForDuplicateNames(ImmutableArray<ObjectTypeSymbol> symbols, DiagnosticBag diagnostics)
	{
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			foreach (ObjectTypeSymbol item in symbols)
			{
				if (instance.Contains(item.Name))
				{
					if (!instance2.Contains(item.Name))
					{
						instance2.Add(item.Name);
					}
				}
				else
				{
					instance.Add(item.Name);
				}
				foreach (IModuleSymbol referenceModule in ReferenceModules)
				{
					if (referenceModule.GetObjectSymbolByName(item.Kind, item.Name) != null)
					{
						instance2.Add(item.Name);
					}
				}
			}
			foreach (string name in instance2)
			{
				foreach (ObjectTypeSymbol item2 in symbols.Where((ObjectTypeSymbol s) => SemanticFacts.IsSameName(name, s.Name)))
				{
					ReportDuplicateNameErrorForSymbol(diagnostics, item2);
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private static void ReportDuplicateNameErrorForSymbol(DiagnosticBag diagnostics, ObjectTypeSymbol symbol)
	{
		SyntaxNode firstSyntax = symbol.GetFirstSyntax();
		if (firstSyntax.IsKind(SyntaxKind.DotNetTypeDeclaration))
		{
			DotNetTypeDeclarationSyntax dotNetTypeDeclarationSyntax = (DotNetTypeDeclarationSyntax)symbol.GetFirstSyntax();
			Location location = dotNetTypeDeclarationSyntax?.TypeAlias?.Location ?? dotNetTypeDeclarationSyntax.TypeName.Location;
			diagnostics.Add(ErrorCode.ERR_DuplicateDotNetTypeAlias, location ?? Location.None, symbol.Name);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_AppObjDuplicateName, (firstSyntax as ObjectSyntax)?.Name.GetLocation() ?? Location.None, symbol.Kind, symbol.Name);
		}
	}

	private void CheckForDuplicateIds(ImmutableArray<ISymbolWithId> symbols, DiagnosticBag diagnostics)
	{
		PooledHashSet<int> instance = PooledHashSet<int>.GetInstance();
		PooledHashSet<int> instance2 = PooledHashSet<int>.GetInstance();
		try
		{
			foreach (ISymbolWithId item in symbols)
			{
				if (instance.Contains(item.Id))
				{
					if (!instance2.Contains(item.Id))
					{
						instance2.Add(item.Id);
					}
				}
				else
				{
					instance.Add(item.Id);
				}
				foreach (IModuleSymbol referenceModule in ReferenceModules)
				{
					if (referenceModule.GetObjectSymbolById(item.Kind, item.Id) != null)
					{
						instance2.Add(item.Id);
					}
				}
			}
			foreach (int id in instance2)
			{
				foreach (ApplicationObjectTypeSymbol item2 in from ApplicationObjectTypeSymbol s in symbols
					where id == s.Id
					select s)
				{
					diagnostics.Add(ErrorCode.ERR_AppObjDuplicateId, (item2.GetFirstSyntax() as ApplicationObjectSyntax)?.ObjectId.GetLocation() ?? Location.None, item2.Kind, item2.Id);
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
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

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (flattenedMembersIsSorted)
		{
			return lazyMembersFlattened;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length > 1)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembersFlattened, immutableArray, default);
		}
		flattenedMembersIsSorted = true;
		return immutableArray;
	}

	public override ImmutableArray<Symbol> GetMembers(string memberName)
	{
		if (GetMembersByName().TryGetValue(memberName, out var value))
		{
			return value;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	private DeclaredObjects GetDeclaredObjects()
	{
		DeclaredObjects declaredObjects = lazyObjects;
		if (declaredObjects != null)
		{
			return declaredObjects;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			declaredObjects = BuildDeclaredMembers(instance);
			DeclaredObjects declaredObjects2 = Interlocked.CompareExchange(ref lazyObjects, declaredObjects, null);
			if (declaredObjects2 != null)
			{
				return declaredObjects2;
			}
			AddDeclarationDiagnostics(instance);
			return declaredObjects;
		}
		finally
		{
			instance.Free();
		}
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetMembersByName()
	{
		if (state.HasComplete(CompletionPart.Variables))
		{
			return lazyMembersDictionary;
		}
		return GetMembersByNameSlow();
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetMembersByNameSlow()
	{
		if (lazyMembersDictionary == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				Dictionary<string, ImmutableArray<Symbol>> value = GetDeclaredObjects().GetObjects().ToDictionaryOfArrays((Symbol s) => s.Name);
				if (Interlocked.CompareExchange(ref lazyMembersDictionary, value, null) == null)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.Variables);
				}
			}
			finally
			{
				instance.Free();
			}
		}
		state.SpinWaitComplete(CompletionPart.Variables, default);
		return lazyMembersDictionary;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private DeclaredObjects BuildDeclaredMembers(DiagnosticBag diagnostics)
	{
		ArrayBuilder<ObjectTypeSymbol> instance = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance2 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance3 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance4 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance5 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance6 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance7 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance8 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance9 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance10 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance11 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance12 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance13 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		ArrayBuilder<ObjectTypeSymbol> instance14 = ArrayBuilder<ObjectTypeSymbol>.GetInstance();
		foreach (SingleObjectDeclaration child in sources.MergedRoot.Children)
		{
			if (lazyObjects != null)
			{
				break;
			}
			switch (child.Kind)
			{
			case DeclarationKind.Codeunit:
				instance.Add(new SourceCodeunitTypeSymbol(this, child.SyntaxReference.GetSyntax() as CodeunitSyntax));
				break;
			case DeclarationKind.Table:
				instance2.Add(new SourceTableTypeSymbol(this, child.SyntaxReference.GetSyntax() as TableSyntax));
				break;
			case DeclarationKind.TableExtension:
				instance3.Add(new SourceTableExtensionTypeSymbol(this, child.SyntaxReference.GetSyntax() as TableExtensionSyntax));
				break;
			case DeclarationKind.Page:
				instance4.Add(new SourcePageTypeSymbol(this, child.SyntaxReference.GetSyntax() as PageSyntax));
				break;
			case DeclarationKind.PageExtension:
				instance5.Add(new SourcePageExtensionTypeSymbol(this, child.SyntaxReference.GetSyntax() as PageExtensionSyntax));
				break;
			case DeclarationKind.PageCustomization:
				instance6.Add(new SourcePageCustomizationTypeSymbol(this, child.SyntaxReference.GetSyntax() as PageCustomizationSyntax));
				break;
			case DeclarationKind.XmlPort:
				instance8.Add(new SourceXmlPortTypeSymbol(this, child.SyntaxReference.GetSyntax() as XmlPortSyntax));
				break;
			case DeclarationKind.Report:
				instance7.Add(new SourceReportTypeSymbol(this, child.SyntaxReference.GetSyntax() as ReportSyntax));
				break;
			case DeclarationKind.Profile:
				instance10.Add(new SourceProfileTypeSymbol(this, child.SyntaxReference.GetSyntax() as ProfileSyntax));
				break;
			case DeclarationKind.Query:
				instance9.Add(new SourceQueryTypeSymbol(this, child.SyntaxReference.GetSyntax() as QuerySyntax));
				break;
			case DeclarationKind.ControlAddIn:
				instance11.Add(new SourceControlAddInTypeSymbol(this, child.SyntaxReference.GetSyntax() as ControlAddInSyntax));
				break;
			case DeclarationKind.DotNetPackage:
				instance12.Add(new SourceDotNetPackageTypeSymbol(this, child.SyntaxReference.GetSyntax() as DotNetPackageSyntax));
				break;
			case DeclarationKind.EnumType:
				instance13.Add(SourceEnumTypeSymbol.Create(this, child.SyntaxReference.GetSyntax() as EnumTypeSyntax));
				break;
			case DeclarationKind.EnumExtensionType:
				instance14.Add(SourceEnumExtensionTypeSymbol.Create(this, child.SyntaxReference.GetSyntax() as EnumExtensionTypeSyntax));
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(child.Kind);
			}
		}
		if (lazyObjects != null)
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
			instance4.Free();
			instance5.Free();
			instance6.Free();
			instance7.Free();
			instance8.Free();
			instance9.Free();
			instance10.Free();
			instance11.Free();
			instance12.Free();
			instance13.Free();
			instance14.Free();
			return null;
		}
		return new DeclaredObjects(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), instance3.ToImmutableAndFree(), instance4.ToImmutableAndFree(), instance5.ToImmutableAndFree(), instance6.ToImmutableAndFree(), instance7.ToImmutableAndFree(), instance8.ToImmutableAndFree(), instance9.ToImmutableAndFree(), instance10.ToImmutableAndFree(), instance11.ToImmutableAndFree(), instance12.ToImmutableAndFree(), instance13.ToImmutableAndFree(), instance14.ToImmutableAndFree());
	}

	public override ImmutableArray<ISymbol> GetDeclaredObjectSymbols()
	{
		return ((IEnumerable<ISymbol>)GetDeclaredObjects()?.GetObjects()).ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;
	}

	public override ImmutableArray<ISymbol> GetObjectSymbols(SymbolKind kind)
	{
		if (!GetDeclaredObjects().ObjectByKind.TryGetValue(kind, out var value))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return value.CastArray<ISymbol>();
	}

	public override ISymbolWithId GetObjectSymbolById(SymbolKind kind, int id)
	{
		if (lazyObjectsById == null)
		{
			PooledDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>> instance = PooledDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>>.GetInstance();
			foreach (SymbolKind allApplicationObjectSymbolKind in SymbolKindExtensions.AllApplicationObjectSymbolKinds)
			{
				IEnumerable<ISymbolWithId> source = GetObjectSymbols(allApplicationObjectSymbolKind).Cast<ISymbolWithId>();
				PooledDictionary<int, ISymbolWithId> dictionary = PooledDictionary<int, ISymbolWithId>.GetInstance();
				source.Do((ISymbolWithId s) =>
				{
					if (s.Id != 0 && !dictionary.ContainsKey(s.Id))
					{
						dictionary.Add(s.Id, s);
					}
				});
				instance.Add(allApplicationObjectSymbolKind, dictionary.ToImmutableDictionaryAndFree());
			}
			Interlocked.CompareExchange(ref lazyObjectsById, instance.ToImmutableDictionaryAndFree(), null);
		}
		if (!lazyObjectsById.TryGetValue(kind, out var value))
		{
			return null;
		}
		if (!value.TryGetValue(id, out var value2))
		{
			return null;
		}
		return value2;
	}

	public override ISymbol GetObjectSymbolByName(SymbolKind kind, string name)
	{
		if (name == null)
		{
			return null;
		}
		if (lazyObjectsByName == null)
		{
			PooledDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> instance = PooledDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>>.GetInstance();
			foreach (SymbolKind allRootObjectSymbolKind in SymbolKindExtensions.AllRootObjectSymbolKinds)
			{
				ImmutableArray<ISymbol> objectSymbols = GetObjectSymbols(allRootObjectSymbolKind);
				PooledNameSymbolDictionary instance2 = PooledNameSymbolDictionary.GetInstance();
				foreach (ISymbol item in objectSymbols)
				{
					if (!string.IsNullOrEmpty(item.Name) && !instance2.ContainsKey(item.Name))
					{
						instance2.Add(item.Name, item);
					}
				}
				instance.Add(allRootObjectSymbolKind, instance2.ToImmutableDictionaryAndFree());
			}
			Interlocked.CompareExchange(ref lazyObjectsByName, instance.ToImmutableDictionaryAndFree(), null);
		}
		return ModuleSymbol.GetObjectSymbolByName(lazyObjectsByName, kind, name);
	}
}
