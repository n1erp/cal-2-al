using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class TypeSymbol : NamespaceOrTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	private class InterfaceInfo
	{
		internal ImmutableArray<NamedTypeSymbol> allInterfaces;

		internal ImmutableHashSet<NamedTypeSymbol> interfacesAndTheirBaseInterfaces;

		private ConcurrentDictionary<Symbol, SymbolAndDiagnostics> _implementationForInterfaceMemberMap;

		internal Dictionary<Symbol, Symbol> explicitInterfaceImplementationMap;

		public ConcurrentDictionary<Symbol, SymbolAndDiagnostics> ImplementationForInterfaceMemberMap
		{
			get
			{
				ConcurrentDictionary<Symbol, SymbolAndDiagnostics> implementationForInterfaceMemberMap = _implementationForInterfaceMemberMap;
				if (implementationForInterfaceMemberMap != null)
				{
					return implementationForInterfaceMemberMap;
				}
				implementationForInterfaceMemberMap = new ConcurrentDictionary<Symbol, SymbolAndDiagnostics>(1, 1);
				return Interlocked.CompareExchange(ref _implementationForInterfaceMemberMap, implementationForInterfaceMemberMap, null) ?? implementationForInterfaceMemberMap;
			}
		}

		internal bool IsDefaultValue()
		{
			if (allInterfaces.IsDefault && interfacesAndTheirBaseInterfaces == null && _implementationForInterfaceMemberMap == null)
			{
				return explicitInterfaceImplementationMap == null;
			}
			return false;
		}
	}

	internal sealed class EqualsIgnoringComparer : EqualityComparer<TypeSymbol>
	{
		private readonly TypeCompareKind _comparison;

		public static EqualsIgnoringComparer InstanceIgnoringTupleNames { get; } = new EqualsIgnoringComparer(TypeCompareKind.IgnoreTupleNames);

		public EqualsIgnoringComparer(TypeCompareKind comparison)
		{
			_comparison = comparison;
		}

		public override int GetHashCode(TypeSymbol obj)
		{
			return obj?.GetHashCode() ?? 0;
		}

		public override bool Equals(TypeSymbol x, TypeSymbol y)
		{
			return x?.Equals(y, _comparison) ?? ((object)y == null);
		}
	}

	protected class SymbolAndDiagnostics
	{
		public static readonly SymbolAndDiagnostics Empty = new SymbolAndDiagnostics(null, ImmutableArray<Diagnostic>.Empty);

		public readonly Symbol Symbol;

		public readonly ImmutableArray<Diagnostic> Diagnostics;

		public SymbolAndDiagnostics(Symbol symbol, ImmutableArray<Diagnostic> diagnostics)
		{
			Symbol = symbol;
			Diagnostics = diagnostics;
		}
	}

	internal const string ImplicitTypeName = "<invalid-global-code>";

	private static readonly InterfaceInfo s_noInterfaces = new InterfaceInfo();

	private ImmutableHashSet<Symbol> _lazyAbstractMembers;

	private InterfaceInfo _lazyInterfaceInfo;

	internal static readonly EqualityComparer<TypeSymbol> EqualsIgnoringDynamicAndTupleNamesComparer = new EqualsIgnoringComparer(TypeCompareKind.IgnoreDynamicAndTupleNames);

	public new TypeSymbol OriginalDefinition => OriginalTypeSymbolDefinition;

	protected virtual TypeSymbol OriginalTypeSymbolDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalTypeSymbolDefinition;

	internal abstract NamedTypeSymbol BaseTypeNoUseSiteDiagnostics { get; }

	internal ImmutableArray<NamedTypeSymbol> AllInterfacesNoUseSiteDiagnostics => GetAllInterfaces();

	internal TypeSymbol EffectiveTypeNoUseSiteDiagnostics
	{
		get
		{
			if (!this.IsTypeParameter())
			{
				return this;
			}
			return ((TypeParameterSymbol)this).EffectiveBaseClassNoUseSiteDiagnostics;
		}
	}

	internal ImmutableHashSet<NamedTypeSymbol> InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics
	{
		get
		{
			InterfaceInfo interfaceInfo = GetInterfaceInfo();
			if (interfaceInfo == s_noInterfaces)
			{
				return ImmutableHashSet.Create<NamedTypeSymbol>();
			}
			if (interfaceInfo.interfacesAndTheirBaseInterfaces == null)
			{
				Interlocked.CompareExchange(ref interfaceInfo.interfacesAndTheirBaseInterfaces, MakeInterfacesAndTheirBaseInterfaces(InterfacesNoUseSiteDiagnostics()), null);
			}
			return interfaceInfo.interfacesAndTheirBaseInterfaces;
		}
	}

	public abstract bool IsReferenceType { get; }

	public abstract bool IsValueType { get; }

	public abstract TypeKind TypeKind { get; }

	public virtual SpecialType SpecialType => SpecialType.None;

	internal PrimitiveTypeCode PrimitiveTypeCode
	{
		get
		{
			if (!this.IsPointerType())
			{
				return SpecialTypes.GetTypeCode(SpecialType);
			}
			return PrimitiveTypeCode.Pointer;
		}
	}

	protected override int HighestPriorityUseSiteError => 648;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				return useSiteDiagnostic.Code == 648;
			}
			return false;
		}
	}

	public virtual bool IsAnonymousType => false;

	public virtual bool IsTupleType => false;

	public virtual NamedTypeSymbol TupleUnderlyingType => null;

	public virtual ImmutableArray<TypeSymbol> TupleElementTypes => default;

	public virtual ImmutableArray<string> TupleElementNames => default;

	public virtual ImmutableArray<FieldSymbol> TupleElements => default;

	internal abstract bool IsManagedType { get; }

	internal abstract bool IsByRefLikeType { get; }

	internal abstract bool IsReadOnly { get; }

	INamedTypeSymbol ITypeSymbol.BaseType => BaseTypeNoUseSiteDiagnostics;

	ImmutableArray<INamedTypeSymbol> ITypeSymbol.Interfaces => StaticCast<INamedTypeSymbol>.From(InterfacesNoUseSiteDiagnostics());

	ImmutableArray<INamedTypeSymbol> ITypeSymbol.AllInterfaces => StaticCast<INamedTypeSymbol>.From(AllInterfacesNoUseSiteDiagnostics);

	bool ITypeSymbol.IsReferenceType => IsReferenceType;

	bool ITypeSymbol.IsValueType => IsValueType;

	ITypeSymbol ITypeSymbol.OriginalDefinition => OriginalDefinition;

	TypeKind ITypeSymbol.TypeKind => TypeKind;

	bool ITypeSymbol.IsTupleType => IsTupleType;

	internal ImmutableHashSet<Symbol> AbstractMembers
	{
		get
		{
			if (_lazyAbstractMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyAbstractMembers, ComputeAbstractMembers(), null);
			}
			return _lazyAbstractMembers;
		}
	}

	private InterfaceInfo GetInterfaceInfo()
	{
		InterfaceInfo lazyInterfaceInfo = _lazyInterfaceInfo;
		if (lazyInterfaceInfo != null)
		{
			return lazyInterfaceInfo;
		}
		TypeSymbol typeSymbol = this;
		while ((object)typeSymbol != null)
		{
			if (!((typeSymbol.TypeKind == TypeKind.TypeParameter) ? ((TypeParameterSymbol)typeSymbol).EffectiveInterfacesNoUseSiteDiagnostics : typeSymbol.InterfacesNoUseSiteDiagnostics()).IsEmpty)
			{
				lazyInterfaceInfo = new InterfaceInfo();
				return Interlocked.CompareExchange(ref _lazyInterfaceInfo, lazyInterfaceInfo, null) ?? lazyInterfaceInfo;
			}
			typeSymbol = typeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		return _lazyInterfaceInfo = s_noInterfaces;
	}

	internal NamedTypeSymbol BaseTypeWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		baseTypeNoUseSiteDiagnostics?.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		return baseTypeNoUseSiteDiagnostics;
	}

	internal NamedTypeSymbol BaseTypeOriginalDefinition(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = BaseTypeNoUseSiteDiagnostics;
		if ((object)namedTypeSymbol != null)
		{
			namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
			namedTypeSymbol.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return namedTypeSymbol;
	}

	internal abstract ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null);

	internal ImmutableArray<NamedTypeSymbol> AllInterfacesWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<NamedTypeSymbol> allInterfacesNoUseSiteDiagnostics = AllInterfacesNoUseSiteDiagnostics;
		TypeSymbol typeSymbol = this;
		do
		{
			typeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		while ((object)typeSymbol != null);
		foreach (NamedTypeSymbol item in allInterfacesNoUseSiteDiagnostics)
		{
			item.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return allInterfacesNoUseSiteDiagnostics;
	}

	internal TypeSymbol EffectiveType(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!this.IsTypeParameter())
		{
			return this;
		}
		return ((TypeParameterSymbol)this).EffectiveBaseClass(ref useSiteDiagnostics);
	}

	internal bool IsDerivedFrom(TypeSymbol type, TypeCompareKind comparison, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)this == type)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		while ((object)namedTypeSymbol != null)
		{
			if (type.Equals(namedTypeSymbol, comparison))
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return false;
	}

	internal bool IsEqualToOrDerivedFrom(TypeSymbol type, TypeCompareKind comparison, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!Equals(type, comparison))
		{
			return IsDerivedFrom(type, comparison, ref useSiteDiagnostics);
		}
		return true;
	}

	internal virtual bool Equals(TypeSymbol t2, TypeCompareKind compareKind = TypeCompareKind.ConsiderEverything)
	{
		return (object)this == t2;
	}

	public sealed override bool Equals(object obj)
	{
		if (!(obj is TypeSymbol t))
		{
			return false;
		}
		return Equals(t);
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	protected virtual ImmutableArray<NamedTypeSymbol> GetAllInterfaces()
	{
		InterfaceInfo interfaceInfo = GetInterfaceInfo();
		if (interfaceInfo == s_noInterfaces)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		if (interfaceInfo.allInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref interfaceInfo.allInterfaces, MakeAllInterfaces());
		}
		return interfaceInfo.allInterfaces;
	}

	protected virtual ImmutableArray<NamedTypeSymbol> MakeAllInterfaces()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		HashSet<NamedTypeSymbol> visited = new HashSet<NamedTypeSymbol>(EqualsIgnoringComparer.InstanceIgnoringTupleNames);
		TypeSymbol typeSymbol = this;
		while ((object)typeSymbol != null)
		{
			ImmutableArray<NamedTypeSymbol> immutableArray = ((typeSymbol.TypeKind == TypeKind.TypeParameter) ? ((TypeParameterSymbol)typeSymbol).EffectiveInterfacesNoUseSiteDiagnostics : typeSymbol.InterfacesNoUseSiteDiagnostics());
			for (int num = immutableArray.Length - 1; num >= 0; num--)
			{
				AddAllInterfaces(immutableArray[num], visited, instance);
			}
			typeSymbol = typeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		instance.ReverseContents();
		return instance.ToImmutableAndFree();
	}

	private static void AddAllInterfaces(NamedTypeSymbol @interface, HashSet<NamedTypeSymbol> visited, ArrayBuilder<NamedTypeSymbol> result)
	{
		if (visited.Add(@interface))
		{
			ImmutableArray<NamedTypeSymbol> immutableArray = @interface.InterfacesNoUseSiteDiagnostics();
			for (int num = immutableArray.Length - 1; num >= 0; num--)
			{
				AddAllInterfaces(immutableArray[num], visited, result);
			}
			result.Add(@interface);
		}
	}

	private static ImmutableHashSet<NamedTypeSymbol> MakeInterfacesAndTheirBaseInterfaces(ImmutableArray<NamedTypeSymbol> declaredInterfaces)
	{
		HashSet<NamedTypeSymbol> hashSet = new HashSet<NamedTypeSymbol>();
		foreach (NamedTypeSymbol item in declaredInterfaces)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
				hashSet.UnionWith(item.AllInterfacesNoUseSiteDiagnostics);
			}
		}
		if (hashSet.Count != 0)
		{
			return ImmutableHashSet.CreateRange(hashSet);
		}
		return ImmutableHashSet.Create<NamedTypeSymbol>();
	}

	public Symbol FindImplementationForInterfaceMember(Symbol interfaceMember)
	{
		if ((object)interfaceMember == null)
		{
			throw new ArgumentNullException("interfaceMember");
		}
		return FindImplementationForInterfaceMemberWithDiagnostics(interfaceMember).Symbol;
	}

	internal TypeSymbol()
	{
	}

	internal abstract bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes);

	public virtual bool IsTupleCompatible(out int tupleCardinality)
	{
		tupleCardinality = 0;
		return false;
	}

	public bool IsTupleCompatible()
	{
		int tupleCardinality;
		return IsTupleCompatible(out tupleCardinality);
	}

	public bool IsTupleOrCompatibleWithTupleOfCardinality(int targetCardinality)
	{
		if (IsTupleType)
		{
			return TupleElementTypes.Length == targetCardinality;
		}
		if (IsTupleCompatible(out var tupleCardinality))
		{
			return tupleCardinality == targetCardinality;
		}
		return false;
	}

	ISymbol ITypeSymbol.FindImplementationForInterfaceMember(ISymbol interfaceMember)
	{
		if (!(interfaceMember is Symbol))
		{
			return null;
		}
		return FindImplementationForInterfaceMember((Symbol)interfaceMember);
	}

	protected SymbolAndDiagnostics FindImplementationForInterfaceMemberWithDiagnostics(Symbol interfaceMember)
	{
		if (this.IsInterfaceType())
		{
			return SymbolAndDiagnostics.Empty;
		}
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		if ((object)containingType == null || !containingType.IsInterface)
		{
			return SymbolAndDiagnostics.Empty;
		}
		SymbolKind kind = interfaceMember.Kind;
		if (kind == SymbolKind.Event || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			InterfaceInfo interfaceInfo = GetInterfaceInfo();
			if (interfaceInfo == s_noInterfaces)
			{
				return SymbolAndDiagnostics.Empty;
			}
			ConcurrentDictionary<Symbol, SymbolAndDiagnostics> implementationForInterfaceMemberMap = interfaceInfo.ImplementationForInterfaceMemberMap;
			if (implementationForInterfaceMemberMap.TryGetValue(interfaceMember, out var value))
			{
				return value;
			}
			value = ComputeImplementationAndDiagnosticsForInterfaceMember(interfaceMember);
			implementationForInterfaceMemberMap.TryAdd(interfaceMember, value);
			return value;
		}
		return SymbolAndDiagnostics.Empty;
	}

	private SymbolAndDiagnostics ComputeImplementationAndDiagnosticsForInterfaceMember(Symbol interfaceMember)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		return new SymbolAndDiagnostics(ComputeImplementationForInterfaceMember(interfaceMember, this, instance), instance.ToReadOnlyAndFree());
	}

	private static Symbol ComputeImplementationForInterfaceMember(Symbol interfaceMember, TypeSymbol implementingType, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		bool flag = false;
		bool dangerous_IsFromSomeCompilation = implementingType.Dangerous_IsFromSomeCompilation;
		Symbol implicitImpl = null;
		Symbol symbol = null;
		TypeSymbol typeSymbol = implementingType;
		while ((object)typeSymbol != null)
		{
			Symbol implementingMember = typeSymbol.GetExplicitImplementationForInterfaceMember(interfaceMember);
			if ((object)implementingMember != null)
			{
				return implementingMember;
			}
			if (IsExplicitlyImplementedViaAccessors(interfaceMember, typeSymbol, out implementingMember))
			{
				return implementingMember;
			}
			flag = flag || typeSymbol.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains(containingType);
			if (flag)
			{
				FindPotentialImplicitImplementationMemberDeclaredInType(interfaceMember, dangerous_IsFromSomeCompilation, typeSymbol, out var implicitImpl2, out var closeMismatch);
				if ((object)implicitImpl2 != null)
				{
					implicitImpl = implicitImpl2;
					break;
				}
				if ((object)symbol == null)
				{
					symbol = closeMismatch;
				}
			}
			typeSymbol = typeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		if (interfaceMember.IsAccessor())
		{
			CheckForImplementationOfCorrespondingPropertyOrEvent((MethodSymbol)interfaceMember, implementingType, dangerous_IsFromSomeCompilation, ref implicitImpl);
		}
		if ((object)implicitImpl != null)
		{
			ReportImplicitImplementationMatchDiagnostics(interfaceMember, implementingType, implicitImpl, diagnostics);
		}
		else if ((object)symbol != null)
		{
			ReportImplicitImplementationMismatchDiagnostics(interfaceMember, implementingType, symbol, diagnostics);
		}
		return implicitImpl;
	}

	private static bool IsExplicitlyImplementedViaAccessors(Symbol interfaceMember, TypeSymbol currType, out Symbol implementingMember)
	{
		MethodSymbol interfaceAccessor;
		MethodSymbol interfaceAccessor2;
		switch (interfaceMember.Kind)
		{
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)interfaceMember;
			interfaceAccessor = propertySymbol.GetMethod;
			interfaceAccessor2 = propertySymbol.SetMethod;
			break;
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)interfaceMember;
			interfaceAccessor = eventSymbol.AddMethod;
			interfaceAccessor2 = eventSymbol.RemoveMethod;
			break;
		}
		default:
			implementingMember = null;
			return false;
		}
		if ((TryGetExplicitImplementationAssociatedPropertyOrEvent(interfaceAccessor, currType, out var associated) | TryGetExplicitImplementationAssociatedPropertyOrEvent(interfaceAccessor2, currType, out var associated2)) && ((object)associated == null || (object)associated2 == null || associated == associated2))
		{
			implementingMember = associated ?? associated2;
			if ((object)implementingMember != null && implementingMember.Dangerous_IsFromSomeCompilation)
			{
				implementingMember = null;
			}
			return true;
		}
		implementingMember = null;
		return false;
	}

	private static bool TryGetExplicitImplementationAssociatedPropertyOrEvent(MethodSymbol interfaceAccessor, TypeSymbol currType, out Symbol associated)
	{
		if ((object)interfaceAccessor != null)
		{
			Symbol explicitImplementationForInterfaceMember = currType.GetExplicitImplementationForInterfaceMember(interfaceAccessor);
			if ((object)explicitImplementationForInterfaceMember != null)
			{
				associated = ((explicitImplementationForInterfaceMember.Kind == SymbolKind.Method) ? ((MethodSymbol)explicitImplementationForInterfaceMember).AssociatedSymbol : null);
				return true;
			}
		}
		associated = null;
		return false;
	}

	private static void CheckForImplementationOfCorrespondingPropertyOrEvent(MethodSymbol interfaceMethod, TypeSymbol implementingType, bool implementingTypeIsFromSomeCompilation, ref Symbol implicitImpl)
	{
		Symbol associatedSymbol = interfaceMethod.AssociatedSymbol;
		Symbol symbol = implementingType.FindImplementationForInterfaceMember(associatedSymbol);
		MethodSymbol methodSymbol = null;
		if ((object)symbol != null)
		{
			methodSymbol = interfaceMethod.MethodKind switch
			{
				MethodKind.PropertyGet => ((PropertySymbol)symbol).GetOwnOrInheritedGetMethod(), 
				MethodKind.PropertySet => ((PropertySymbol)symbol).GetOwnOrInheritedSetMethod(), 
				MethodKind.EventAdd => ((EventSymbol)symbol).GetOwnOrInheritedAddMethod(), 
				MethodKind.EventRemove => ((EventSymbol)symbol).GetOwnOrInheritedRemoveMethod(), 
				_ => throw ExceptionUtilities.UnexpectedValue(interfaceMethod.MethodKind), 
			};
		}
		if (methodSymbol == implicitImpl)
		{
			return;
		}
		if ((object)methodSymbol == null && (object)implicitImpl != null && implicitImpl.IsAccessor())
		{
			implicitImpl = null;
		}
		else if ((object)methodSymbol != null && ((object)implicitImpl == null || methodSymbol.ContainingType == implicitImpl.ContainingType))
		{
			MethodSymbol interfaceMember = new SignatureOnlyMethodSymbol(methodSymbol.Name, interfaceMethod.ContainingType, interfaceMethod.MethodKind, interfaceMethod.CallingConvention, interfaceMethod.TypeParameters, interfaceMethod.Parameters, interfaceMethod.RefKind, interfaceMethod.ReturnType, interfaceMethod.ReturnTypeCustomModifiers, interfaceMethod.RefCustomModifiers, interfaceMethod.ExplicitInterfaceImplementations);
			if (IsInterfaceMemberImplementation(methodSymbol, interfaceMember, implementingTypeIsFromSomeCompilation))
			{
				implicitImpl = methodSymbol;
			}
		}
	}

	private static void ReportImplicitImplementationMatchDiagnostics(Symbol interfaceMember, TypeSymbol implementingType, Symbol implicitImpl, DiagnosticBag diagnostics)
	{
		if (interfaceMember.Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)interfaceMember;
			bool flag = implicitImpl.IsAccessor();
			bool flag2 = methodSymbol.IsAccessor();
			if (flag2 && !flag && !methodSymbol.IsIndexedPropertyAccessor())
			{
				diagnostics.Add(ErrorCode.ERR_MethodImplementingAccessor, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
			}
			else if (!flag2 & flag)
			{
				diagnostics.Add(ErrorCode.ERR_AccessorImplementingMethod, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
			}
			else
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)implicitImpl;
				if (methodSymbol2.IsConditional)
				{
					diagnostics.Add(ErrorCode.ERR_InterfaceImplementedByConditional, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
				}
				else
				{
					ReportAnyMismatchedConstraints(methodSymbol, implementingType, methodSymbol2, diagnostics);
				}
			}
		}
		if (implicitImpl.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(implicitImpl, interfaceMember))
		{
			diagnostics.Add(ErrorCode.ERR_ImplBadTupleNames, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, interfaceMember);
		}
		if (implicitImpl.ContainingType.IsDefinition)
		{
			return;
		}
		foreach (Symbol member in implicitImpl.ContainingType.GetMembers(implicitImpl.Name))
		{
			if (member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic && !(member == implicitImpl) && MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(interfaceMember, member) && !member.IsAccessor())
			{
				diagnostics.Add(ErrorCode.WRN_MultipleRuntimeImplementationMatches, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, member), member, interfaceMember, implementingType);
			}
		}
	}

	private static void ReportImplicitImplementationMismatchDiagnostics(Symbol interfaceMember, TypeSymbol implementingType, Symbol closestMismatch, DiagnosticBag diagnostics)
	{
		Location location;
		if ((object)implementingType != null)
		{
			NamedTypeSymbol containingType = interfaceMember.ContainingType;
			location = (implementingType as SourceMemberContainerTypeSymbol)?.GetImplementsLocation(containingType) ?? implementingType.Locations[0];
		}
		else
		{
			location = implementingType.Locations[0];
		}
		if (closestMismatch.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberStatic, location, implementingType, interfaceMember, closestMismatch);
			return;
		}
		if (closestMismatch.DeclaredAccessibility != Accessibility.Public)
		{
			ErrorCode code = (interfaceMember.IsAccessor() ? ErrorCode.ERR_UnimplementedInterfaceAccessor : ErrorCode.ERR_CloseUnimplementedInterfaceMemberNotPublic);
			diagnostics.Add(code, location, implementingType, interfaceMember, closestMismatch);
			return;
		}
		RefKind refKind = RefKind.None;
		TypeSymbol typeSymbol;
		switch (interfaceMember.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)interfaceMember;
			refKind = methodSymbol.RefKind;
			typeSymbol = methodSymbol.ReturnType;
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)interfaceMember;
			refKind = propertySymbol.RefKind;
			typeSymbol = propertySymbol.Type;
			break;
		}
		case SymbolKind.Event:
			typeSymbol = ((EventSymbol)interfaceMember).Type;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(interfaceMember.Kind);
		}
		bool flag = false;
		switch (closestMismatch.Kind)
		{
		case SymbolKind.Method:
			flag = ((MethodSymbol)closestMismatch).RefKind != refKind;
			break;
		case SymbolKind.Property:
			flag = ((PropertySymbol)closestMismatch).RefKind != refKind;
			break;
		}
		DiagnosticInfo useSiteDiagnostic;
		if ((object)typeSymbol != null && (useSiteDiagnostic = typeSymbol.GetUseSiteDiagnostic()) != null && useSiteDiagnostic.DefaultSeverity == DiagnosticSeverity.Error)
		{
			diagnostics.Add(useSiteDiagnostic, location);
		}
		else if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberWrongRefReturn, location, implementingType, interfaceMember, closestMismatch);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberWrongReturnType, location, implementingType, interfaceMember, closestMismatch, typeSymbol);
		}
	}

	private static void ReportAnyMismatchedConstraints(MethodSymbol interfaceMethod, TypeSymbol implementingType, MethodSymbol implicitImpl, DiagnosticBag diagnostics)
	{
		int arity = interfaceMethod.Arity;
		if (arity <= 0)
		{
			return;
		}
		ImmutableArray<TypeParameterSymbol> typeParameters = interfaceMethod.TypeParameters;
		ImmutableArray<TypeParameterSymbol> typeParameters2 = implicitImpl.TypeParameters;
		ImmutableArray<TypeParameterSymbol> to = IndexedTypeParameterSymbol.Take(arity);
		TypeMap typeMap = new TypeMap(typeParameters, to, allowAlpha: true);
		TypeMap typeMap2 = new TypeMap(typeParameters2, to, allowAlpha: true);
		for (int i = 0; i < arity; i++)
		{
			TypeParameterSymbol typeParameterSymbol = typeParameters[i];
			TypeParameterSymbol typeParameterSymbol2 = typeParameters2[i];
			if (!MemberSignatureComparer.HaveSameConstraints(typeParameterSymbol, typeMap, typeParameterSymbol2, typeMap2))
			{
				diagnostics.Add(ErrorCode.ERR_ImplBadConstraints, GetImplicitImplementationDiagnosticLocation(interfaceMethod, implementingType, implicitImpl), typeParameterSymbol2.Name, implicitImpl, typeParameterSymbol.Name, interfaceMethod);
			}
		}
	}

	internal static Location GetImplicitImplementationDiagnosticLocation(Symbol interfaceMember, TypeSymbol implementingType, Symbol member)
	{
		if (member.ContainingType == implementingType)
		{
			return member.Locations[0];
		}
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		return (implementingType as SourceMemberContainerTypeSymbol)?.GetImplementsLocation(containingType) ?? implementingType.Locations[0];
	}

	private static void FindPotentialImplicitImplementationMemberDeclaredInType(Symbol interfaceMember, bool implementingTypeIsFromSomeCompilation, TypeSymbol currType, out Symbol implicitImpl, out Symbol closeMismatch)
	{
		implicitImpl = null;
		closeMismatch = null;
		foreach (Symbol member in currType.GetMembers(interfaceMember.Name))
		{
			if (member.Kind == interfaceMember.Kind)
			{
				if (IsInterfaceMemberImplementation(member, interfaceMember, implementingTypeIsFromSomeCompilation))
				{
					implicitImpl = member;
					break;
				}
				if ((((object)closeMismatch == null) & implementingTypeIsFromSomeCompilation) && MemberSignatureComparer.CSharpCloseImplicitImplementationComparer.Equals(interfaceMember, member))
				{
					closeMismatch = member;
				}
			}
		}
	}

	private static bool IsInterfaceMemberImplementation(Symbol candidateMember, Symbol interfaceMember, bool implementingTypeIsFromSomeCompilation)
	{
		if (candidateMember.DeclaredAccessibility != Accessibility.Public || candidateMember.IsStatic)
		{
			return false;
		}
		if (implementingTypeIsFromSomeCompilation)
		{
			return MemberSignatureComparer.CSharpImplicitImplementationComparer.Equals(interfaceMember, candidateMember);
		}
		return MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(interfaceMember, candidateMember);
	}

	private Symbol GetExplicitImplementationForInterfaceMember(Symbol interfaceMember)
	{
		InterfaceInfo interfaceInfo = GetInterfaceInfo();
		if (interfaceInfo == s_noInterfaces)
		{
			return null;
		}
		if (interfaceInfo.explicitInterfaceImplementationMap == null)
		{
			Interlocked.CompareExchange(ref interfaceInfo.explicitInterfaceImplementationMap, MakeExplicitInterfaceImplementationMap(), null);
		}
		interfaceInfo.explicitInterfaceImplementationMap.TryGetValue(interfaceMember, out var value);
		return value;
	}

	private Dictionary<Symbol, Symbol> MakeExplicitInterfaceImplementationMap()
	{
		Dictionary<Symbol, Symbol> dictionary = new Dictionary<Symbol, Symbol>();
		foreach (Symbol item in GetMembersUnordered())
		{
			foreach (Symbol explicitInterfaceImplementation in item.GetExplicitInterfaceImplementations())
			{
				if (!dictionary.ContainsKey(explicitInterfaceImplementation))
				{
					dictionary[explicitInterfaceImplementation] = item;
				}
			}
		}
		return dictionary;
	}

	private ImmutableHashSet<Symbol> ComputeAbstractMembers()
	{
		ImmutableHashSet<Symbol> immutableHashSet = ImmutableHashSet.Create<Symbol>();
		ImmutableHashSet<Symbol> immutableHashSet2 = ImmutableHashSet.Create<Symbol>();
		foreach (Symbol item in GetMembersUnordered())
		{
			if (IsAbstract && item.IsAbstract && item.Kind != SymbolKind.NamedType)
			{
				immutableHashSet = immutableHashSet.Add(item);
			}
			Symbol symbol = null;
			switch (item.Kind)
			{
			case SymbolKind.Method:
				symbol = ((MethodSymbol)item).OverriddenMethod;
				break;
			case SymbolKind.Property:
				symbol = ((PropertySymbol)item).OverriddenProperty;
				break;
			case SymbolKind.Event:
				symbol = ((EventSymbol)item).OverriddenEvent;
				break;
			}
			if ((object)symbol != null)
			{
				immutableHashSet2 = immutableHashSet2.Add(symbol);
			}
		}
		if ((object)BaseTypeNoUseSiteDiagnostics != null && BaseTypeNoUseSiteDiagnostics.IsAbstract)
		{
			foreach (Symbol abstractMember in BaseTypeNoUseSiteDiagnostics.AbstractMembers)
			{
				if (!immutableHashSet2.Contains(abstractMember))
				{
					immutableHashSet = immutableHashSet.Add(abstractMember);
				}
			}
		}
		return immutableHashSet;
	}

	[Obsolete("Use TypeWithModifiers.Is method.", true)]
	internal bool Equals(TypeWithModifiers other)
	{
		return other.Is(this);
	}
}
