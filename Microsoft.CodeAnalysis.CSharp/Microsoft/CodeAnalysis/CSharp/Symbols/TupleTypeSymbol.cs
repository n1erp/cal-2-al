using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TupleTypeSymbol : WrappedNamedTypeSymbol
{
	private readonly ImmutableArray<Location> _locations;

	private readonly ImmutableArray<Location> _elementLocations;

	private readonly ImmutableArray<string> _elementNames;

	private readonly ImmutableArray<bool> _errorPositions;

	private readonly ImmutableArray<TypeSymbol> _elementTypes;

	private ImmutableArray<Symbol> _lazyMembers;

	private ImmutableArray<FieldSymbol> _lazyDefaultElementFields;

	private SmallDictionary<Symbol, Symbol> _lazyUnderlyingDefinitionToMemberMap;

	internal const int RestPosition = 8;

	internal const int RestIndex = 7;

	internal const string TupleTypeName = "ValueTuple";

	internal const string RestFieldName = "Rest";

	private static readonly WellKnownType[] tupleTypes = new WellKnownType[8]
	{
		WellKnownType.System_ValueTuple_T1,
		WellKnownType.System_ValueTuple_T2,
		WellKnownType.System_ValueTuple_T3,
		WellKnownType.System_ValueTuple_T4,
		WellKnownType.System_ValueTuple_T5,
		WellKnownType.System_ValueTuple_T6,
		WellKnownType.System_ValueTuple_T7,
		WellKnownType.System_ValueTuple_TRest
	};

	private static readonly WellKnownMember[] tupleCtors = new WellKnownMember[8]
	{
		WellKnownMember.System_ValueTuple_T1__ctor,
		WellKnownMember.System_ValueTuple_T2__ctor,
		WellKnownMember.System_ValueTuple_T3__ctor,
		WellKnownMember.System_ValueTuple_T4__ctor,
		WellKnownMember.System_ValueTuple_T5__ctor,
		WellKnownMember.System_ValueTuple_T6__ctor,
		WellKnownMember.System_ValueTuple_T7__ctor,
		WellKnownMember.System_ValueTuple_TRest__ctor
	};

	private static readonly WellKnownMember[][] tupleMembers = new WellKnownMember[8][]
	{
		new WellKnownMember[1] { WellKnownMember.System_ValueTuple_T1__Item1 },
		new WellKnownMember[2]
		{
			WellKnownMember.System_ValueTuple_T2__Item1,
			WellKnownMember.System_ValueTuple_T2__Item2
		},
		new WellKnownMember[3]
		{
			WellKnownMember.System_ValueTuple_T3__Item1,
			WellKnownMember.System_ValueTuple_T3__Item2,
			WellKnownMember.System_ValueTuple_T3__Item3
		},
		new WellKnownMember[4]
		{
			WellKnownMember.System_ValueTuple_T4__Item1,
			WellKnownMember.System_ValueTuple_T4__Item2,
			WellKnownMember.System_ValueTuple_T4__Item3,
			WellKnownMember.System_ValueTuple_T4__Item4
		},
		new WellKnownMember[5]
		{
			WellKnownMember.System_ValueTuple_T5__Item1,
			WellKnownMember.System_ValueTuple_T5__Item2,
			WellKnownMember.System_ValueTuple_T5__Item3,
			WellKnownMember.System_ValueTuple_T5__Item4,
			WellKnownMember.System_ValueTuple_T5__Item5
		},
		new WellKnownMember[6]
		{
			WellKnownMember.System_ValueTuple_T6__Item1,
			WellKnownMember.System_ValueTuple_T6__Item2,
			WellKnownMember.System_ValueTuple_T6__Item3,
			WellKnownMember.System_ValueTuple_T6__Item4,
			WellKnownMember.System_ValueTuple_T6__Item5,
			WellKnownMember.System_ValueTuple_T6__Item6
		},
		new WellKnownMember[7]
		{
			WellKnownMember.System_ValueTuple_T7__Item1,
			WellKnownMember.System_ValueTuple_T7__Item2,
			WellKnownMember.System_ValueTuple_T7__Item3,
			WellKnownMember.System_ValueTuple_T7__Item4,
			WellKnownMember.System_ValueTuple_T7__Item5,
			WellKnownMember.System_ValueTuple_T7__Item6,
			WellKnownMember.System_ValueTuple_T7__Item7
		},
		new WellKnownMember[8]
		{
			WellKnownMember.System_ValueTuple_TRest__Item1,
			WellKnownMember.System_ValueTuple_TRest__Item2,
			WellKnownMember.System_ValueTuple_TRest__Item3,
			WellKnownMember.System_ValueTuple_TRest__Item4,
			WellKnownMember.System_ValueTuple_TRest__Item5,
			WellKnownMember.System_ValueTuple_TRest__Item6,
			WellKnownMember.System_ValueTuple_TRest__Item7,
			WellKnownMember.System_ValueTuple_TRest__Rest
		}
	};

	public override NamedTypeSymbol TupleUnderlyingType => _underlyingType;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _underlyingType.BaseTypeNoUseSiteDiagnostics;

	internal sealed override bool IsManagedType => _underlyingType.IsManagedType;

	public override bool IsTupleType => true;

	public override bool IsImplicitlyDeclared => false;

	public override ImmutableArray<TypeSymbol> TupleElementTypes => _elementTypes;

	public override ImmutableArray<string> TupleElementNames => _elementNames;

	public override ImmutableArray<FieldSymbol> TupleElements
	{
		get
		{
			if (_lazyDefaultElementFields.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyDefaultElementFields, CollectTupleElementFields());
			}
			return _lazyDefaultElementFields;
		}
	}

	internal SmallDictionary<Symbol, Symbol> UnderlyingDefinitionToMemberMap => _lazyUnderlyingDefinitionToMemberMap ?? (_lazyUnderlyingDefinitionToMemberMap = ComputeDefinitionToMemberMap());

	public override NamedTypeSymbol EnumUnderlyingType => _underlyingType.EnumUnderlyingType;

	public override SymbolKind Kind => SymbolKind.NamedType;

	public override TypeKind TypeKind => TypeKind.Struct;

	public override Symbol ContainingSymbol => _underlyingType.ContainingSymbol;

	public override ImmutableArray<Location> Locations => _locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<CSharpSyntaxNode>(_locations);

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (_underlyingType.IsErrorType())
			{
				return Accessibility.Public;
			}
			return _underlyingType.DeclaredAccessibility;
		}
	}

	public override bool IsStatic => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal override bool HasTypeArgumentsCustomModifiers => false;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => ImmutableArray<TypeSymbol>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name => string.Empty;

	internal override bool MangleName => false;

	public override IEnumerable<string> MemberNames
	{
		get
		{
			PooledHashSet<string> set = PooledHashSet<string>.GetInstance();
			foreach (Symbol member in GetMembers())
			{
				string name = member.Name;
				if (set.Add(name))
				{
					yield return name;
				}
			}
			set.Free();
		}
	}

	internal override bool HasSpecialName => false;

	internal override bool IsComImport => false;

	internal override NamedTypeSymbol ComImportCoClass => null;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool ShouldAddWinRTMembers => false;

	public override bool IsSerializable => _underlyingType.IsSerializable;

	internal override TypeLayout Layout => _underlyingType.Layout;

	internal override CharSet MarshallingCharSet => _underlyingType.MarshallingCharSet;

	internal override bool HasDeclarativeSecurity => _underlyingType.HasDeclarativeSecurity;

	internal override bool IsInterface => false;

	private TupleTypeSymbol(Location locationOpt, NamedTypeSymbol underlyingType, ImmutableArray<Location> elementLocations, ImmutableArray<string> elementNames, ImmutableArray<TypeSymbol> elementTypes, ImmutableArray<bool> errorPositions)
		: this((locationOpt == null) ? ImmutableArray<Location>.Empty : ImmutableArray.Create(locationOpt), underlyingType, elementLocations, elementNames, elementTypes, errorPositions)
	{
	}

	private TupleTypeSymbol(ImmutableArray<Location> locations, NamedTypeSymbol underlyingType, ImmutableArray<Location> elementLocations, ImmutableArray<string> elementNames, ImmutableArray<TypeSymbol> elementTypes, ImmutableArray<bool> errorPositions)
		: base(underlyingType)
	{
		_elementLocations = elementLocations;
		_elementNames = elementNames;
		_elementTypes = elementTypes;
		_locations = locations;
		_errorPositions = errorPositions;
	}

	internal static NamedTypeSymbol Create(Location locationOpt, ImmutableArray<TypeSymbol> elementTypes, ImmutableArray<Location> elementLocations, ImmutableArray<string> elementNames, CSharpCompilation compilation, bool shouldCheckConstraints, ImmutableArray<bool> errorPositions, CSharpSyntaxNode syntax = null, DiagnosticBag diagnostics = null)
	{
		if (elementTypes.Length <= 1)
		{
			throw ExceptionUtilities.Unreachable;
		}
		NamedTypeSymbol tupleUnderlyingType = GetTupleUnderlyingType(elementTypes, syntax, compilation, diagnostics);
		if (diagnostics != null && ((SourceModuleSymbol)compilation.SourceModule).AnyReferencedAssembliesAreLinked)
		{
			EmbeddedTypesManager.IsValidEmbeddableType(tupleUnderlyingType, syntax, diagnostics);
		}
		TupleTypeSymbol tupleTypeSymbol = Create(tupleUnderlyingType, elementNames, errorPositions, locationOpt, elementLocations);
		if (shouldCheckConstraints && diagnostics != null)
		{
			tupleTypeSymbol.CheckConstraints(compilation.Conversions, syntax, elementLocations, compilation, diagnostics);
		}
		return tupleTypeSymbol;
	}

	public static TupleTypeSymbol Create(NamedTypeSymbol tupleCompatibleType, ImmutableArray<string> elementNames = default(ImmutableArray<string>), ImmutableArray<bool> errorPositions = default(ImmutableArray<bool>), Location locationOpt = null, ImmutableArray<Location> elementLocations = default(ImmutableArray<Location>))
	{
		return Create((locationOpt == null) ? ImmutableArray<Location>.Empty : ImmutableArray.Create(locationOpt), tupleCompatibleType, elementLocations, elementNames, errorPositions);
	}

	public static TupleTypeSymbol Create(ImmutableArray<Location> locations, NamedTypeSymbol tupleCompatibleType, ImmutableArray<Location> elementLocations, ImmutableArray<string> elementNames, ImmutableArray<bool> errorPositions)
	{
		ImmutableArray<TypeSymbol> elementTypes;
		if (tupleCompatibleType.Arity == 8)
		{
			tupleCompatibleType = EnsureRestExtensionsAreTuples(tupleCompatibleType);
			ImmutableArray<TypeSymbol> tupleElementTypes = tupleCompatibleType.TypeArgumentsNoUseSiteDiagnostics[7].TupleElementTypes;
			ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(7 + tupleElementTypes.Length);
			instance.AddRange(tupleCompatibleType.TypeArgumentsNoUseSiteDiagnostics, 7);
			instance.AddRange(tupleElementTypes);
			elementTypes = instance.ToImmutableAndFree();
		}
		else
		{
			elementTypes = tupleCompatibleType.TypeArgumentsNoUseSiteDiagnostics;
		}
		return new TupleTypeSymbol(locations, tupleCompatibleType, elementLocations, elementNames, elementTypes, errorPositions);
	}

	private static NamedTypeSymbol EnsureRestExtensionsAreTuples(NamedTypeSymbol tupleCompatibleType)
	{
		if (!tupleCompatibleType.TypeArgumentsNoUseSiteDiagnostics[7].IsTupleType)
		{
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
			NamedTypeSymbol namedTypeSymbol = tupleCompatibleType;
			do
			{
				instance.Add(namedTypeSymbol);
				namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[7];
			}
			while (namedTypeSymbol.Arity == 8);
			if (!namedTypeSymbol.IsTupleType)
			{
				instance.Add(namedTypeSymbol);
			}
			tupleCompatibleType = instance.Pop();
			ArrayBuilder<TypeWithModifiers> instance2 = ArrayBuilder<TypeWithModifiers>.GetInstance(8);
			do
			{
				TupleTypeSymbol extensionTuple = Create(tupleCompatibleType);
				tupleCompatibleType = instance.Pop();
				tupleCompatibleType = ReplaceRestExtensionType(tupleCompatibleType, instance2, extensionTuple);
			}
			while (instance.Count != 0);
			instance2.Free();
			instance.Free();
		}
		return tupleCompatibleType;
	}

	private static NamedTypeSymbol ReplaceRestExtensionType(NamedTypeSymbol tupleCompatibleType, ArrayBuilder<TypeWithModifiers> typeArgumentsBuilder, TupleTypeSymbol extensionTuple)
	{
		bool hasTypeArgumentsCustomModifiers = tupleCompatibleType.HasTypeArgumentsCustomModifiers;
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = tupleCompatibleType.TypeArgumentsNoUseSiteDiagnostics;
		typeArgumentsBuilder.Clear();
		for (int i = 0; i < 7; i++)
		{
			typeArgumentsBuilder.Add(new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[i], hasTypeArgumentsCustomModifiers ? tupleCompatibleType.GetTypeArgumentCustomModifiers(i) : ImmutableArray<CustomModifier>.Empty));
		}
		typeArgumentsBuilder.Add(new TypeWithModifiers(extensionTuple, hasTypeArgumentsCustomModifiers ? tupleCompatibleType.GetTypeArgumentCustomModifiers(7) : ImmutableArray<CustomModifier>.Empty));
		tupleCompatibleType = tupleCompatibleType.ConstructedFrom.Construct(typeArgumentsBuilder.ToImmutable(), unbound: false);
		return tupleCompatibleType;
	}

	internal TupleTypeSymbol WithUnderlyingType(NamedTypeSymbol newUnderlyingType)
	{
		return Create(_locations, newUnderlyingType, _elementLocations, _elementNames, _errorPositions);
	}

	internal TupleTypeSymbol WithElementNames(ImmutableArray<string> newElementNames, Location newLocation, ImmutableArray<Location> newElementLocations)
	{
		return new TupleTypeSymbol(newLocation, _underlyingType, newElementLocations, newElementNames, _elementTypes, default);
	}

	internal static void GetUnderlyingTypeChain(NamedTypeSymbol underlyingTupleType, ArrayBuilder<NamedTypeSymbol> underlyingTupleTypeChain)
	{
		NamedTypeSymbol namedTypeSymbol = underlyingTupleType;
		while (true)
		{
			underlyingTupleTypeChain.Add(namedTypeSymbol);
			if (namedTypeSymbol.Arity == 8)
			{
				namedTypeSymbol = namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[7].TupleUnderlyingType;
				continue;
			}
			break;
		}
	}

	internal static void AddElementTypes(NamedTypeSymbol underlyingTupleType, ArrayBuilder<TypeSymbol> tupleElementTypes)
	{
		NamedTypeSymbol namedTypeSymbol = underlyingTupleType;
		while (true)
		{
			if (namedTypeSymbol.IsTupleType)
			{
				tupleElementTypes.AddRange(namedTypeSymbol.TupleElementTypes);
				break;
			}
			int length = Math.Min(namedTypeSymbol.Arity, 7);
			tupleElementTypes.AddRange(namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics, length);
			if (namedTypeSymbol.Arity == 8)
			{
				namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[7];
				continue;
			}
			break;
		}
	}

	private static NamedTypeSymbol GetNestedTupleUnderlyingType(NamedTypeSymbol topLevelUnderlyingType, int depth)
	{
		NamedTypeSymbol namedTypeSymbol = topLevelUnderlyingType;
		for (int i = 0; i < depth; i++)
		{
			namedTypeSymbol = namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[7].TupleUnderlyingType;
		}
		return namedTypeSymbol;
	}

	private static int NumberOfValueTuples(int numElements, out int remainder)
	{
		remainder = (numElements - 1) % 7 + 1;
		return (numElements - 1) / 7 + 1;
	}

	private static NamedTypeSymbol GetTupleUnderlyingType(ImmutableArray<TypeSymbol> elementTypes, CSharpSyntaxNode syntax, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		int num = NumberOfValueTuples(elementTypes.Length, out var remainder);
		NamedTypeSymbol wellKnownType = compilation.GetWellKnownType(GetTupleType(remainder));
		if (diagnostics != null && syntax != null)
		{
			ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType);
		}
		NamedTypeSymbol namedTypeSymbol = wellKnownType.Construct(ImmutableArray.Create(elementTypes, (num - 1) * 7, remainder));
		int num2 = num - 1;
		if (num2 > 0)
		{
			NamedTypeSymbol wellKnownType2 = compilation.GetWellKnownType(GetTupleType(8));
			if (diagnostics != null && syntax != null)
			{
				ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType2);
			}
			do
			{
				ImmutableArray<TypeSymbol> typeArguments = ImmutableArray.Create(elementTypes, (num2 - 1) * 7, 7).Add(namedTypeSymbol);
				namedTypeSymbol = wellKnownType2.Construct(typeArguments);
				num2--;
			}
			while (num2 > 0);
		}
		return namedTypeSymbol;
	}

	private static void ReportUseSiteAndObsoleteDiagnostics(CSharpSyntaxNode syntax, DiagnosticBag diagnostics, NamedTypeSymbol firstTupleType)
	{
		Binder.ReportUseSiteDiagnostics(firstTupleType, diagnostics, syntax);
		Binder.ReportDiagnosticsIfObsoleteInternal(diagnostics, firstTupleType, syntax, firstTupleType.ContainingType, BinderFlags.None);
	}

	internal static void VerifyTupleTypePresent(int cardinality, CSharpSyntaxNode syntax, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		int num = NumberOfValueTuples(cardinality, out var remainder);
		NamedTypeSymbol wellKnownType = compilation.GetWellKnownType(GetTupleType(remainder));
		ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType);
		if (num > 1)
		{
			NamedTypeSymbol wellKnownType2 = compilation.GetWellKnownType(GetTupleType(8));
			ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType2);
		}
	}

	internal static void ReportNamesMismatchesIfAny(TypeSymbol destination, BoundTupleLiteral literal, DiagnosticBag diagnostics)
	{
		ImmutableArray<string> argumentNamesOpt = literal.ArgumentNamesOpt;
		if (argumentNamesOpt.IsDefault)
		{
			return;
		}
		ImmutableArray<bool> inferredNamesOpt = literal.InferredNamesOpt;
		bool isDefault = inferredNamesOpt.IsDefault;
		ImmutableArray<string> tupleElementNames = destination.TupleElementNames;
		int length = argumentNamesOpt.Length;
		bool isDefault2 = tupleElementNames.IsDefault;
		for (int i = 0; i < length; i++)
		{
			string text = argumentNamesOpt[i];
			bool flag = !isDefault && inferredNamesOpt[i];
			if (text != null && !flag && (isDefault2 || string.CompareOrdinal(tupleElementNames[i], text) != 0))
			{
				diagnostics.Add(ErrorCode.WRN_TupleLiteralNameMismatch, literal.Arguments[i].Syntax.Parent.Location, text, destination);
			}
		}
	}

	private static WellKnownType GetTupleType(int arity)
	{
		if (arity > 8)
		{
			throw ExceptionUtilities.Unreachable;
		}
		return tupleTypes[arity - 1];
	}

	internal static WellKnownMember GetTupleCtor(int arity)
	{
		if (arity > 8)
		{
			throw ExceptionUtilities.Unreachable;
		}
		return tupleCtors[arity - 1];
	}

	internal static WellKnownMember GetTupleTypeMember(int arity, int position)
	{
		return tupleMembers[arity - 1][position - 1];
	}

	internal static string TupleMemberName(int position)
	{
		return "Item" + position;
	}

	private static bool IsElementNameForbidden(string name)
	{
		switch (name)
		{
		case "CompareTo":
		case "Deconstruct":
		case "Equals":
		case "GetHashCode":
		case "Rest":
		case "ToString":
			return true;
		default:
			return false;
		}
	}

	internal static int IsElementNameReserved(string name)
	{
		if (IsElementNameForbidden(name))
		{
			return 0;
		}
		return MatchesCanonicalElementName(name);
	}

	private static int MatchesCanonicalElementName(string name)
	{
		if (name.StartsWith("Item", StringComparison.Ordinal) && int.TryParse(name.Substring(4), out var result) && result > 0 && string.Equals(name, TupleMemberName(result), StringComparison.Ordinal))
		{
			return result;
		}
		return -1;
	}

	internal static Symbol GetWellKnownMemberInType(NamedTypeSymbol type, WellKnownMember relativeMember)
	{
		MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(relativeMember);
		return CSharpCompilation.GetRuntimeMember(type, ref descriptor, CSharpCompilation.SpecialMembersSignatureComparer.Instance, null);
	}

	internal static Symbol GetWellKnownMemberInType(NamedTypeSymbol type, WellKnownMember relativeMember, DiagnosticBag diagnostics, SyntaxNode syntax)
	{
		Symbol wellKnownMemberInType = GetWellKnownMemberInType(type, relativeMember);
		if ((object)wellKnownMemberInType == null)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(relativeMember);
			Binder.Error(diagnostics, ErrorCode.ERR_PredefinedTypeMemberNotFoundInAssembly, syntax, descriptor.Name, type, type.ContainingAssembly);
		}
		else
		{
			DiagnosticInfo useSiteDiagnostic = wellKnownMemberInType.GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null && useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
			{
				diagnostics.Add(useSiteDiagnostic, syntax.GetLocation());
			}
		}
		return wellKnownMemberInType;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return _underlyingType.InterfacesNoUseSiteDiagnostics(basesBeingResolved);
	}

	private ImmutableArray<FieldSymbol> CollectTupleElementFields()
	{
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(_elementTypes.Length, null);
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind != SymbolKind.Field)
			{
				continue;
			}
			FieldSymbol fieldSymbol = (FieldSymbol)member;
			int tupleElementIndex = fieldSymbol.TupleElementIndex;
			if (tupleElementIndex >= 0)
			{
				FieldSymbol fieldSymbol2 = instance[tupleElementIndex];
				if ((object)fieldSymbol2 == null || fieldSymbol2.IsDefaultTupleElement)
				{
					instance[tupleElementIndex] = fieldSymbol;
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_lazyMembers.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _lazyMembers, CreateMembers());
		}
		return _lazyMembers;
	}

	private ImmutableArray<Symbol> CreateMembers()
	{
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance(_elementTypes.Length, fillWithValue: false);
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(Math.Max(_elementTypes.Length, _underlyingType.OriginalDefinition.GetMembers().Length));
		NamedTypeSymbol namedTypeSymbol = _underlyingType;
		int num = 0;
		ArrayBuilder<FieldSymbol> instance3 = ArrayBuilder<FieldSymbol>.GetInstance(namedTypeSymbol.Arity);
		CollectTargetTupleFields(namedTypeSymbol, instance3);
		ImmutableArray<Symbol> members = namedTypeSymbol.OriginalDefinition.GetMembers();
		while (true)
		{
			foreach (Symbol item in members)
			{
				switch (item.Kind)
				{
				case SymbolKind.Method:
					if (num == 0)
					{
						instance2.Add(new TupleMethodSymbol(this, ((MethodSymbol)item).AsMember(namedTypeSymbol)));
					}
					break;
				case SymbolKind.Field:
				{
					FieldSymbol fieldSymbol = (FieldSymbol)item;
					int num2 = instance3.IndexOf(fieldSymbol, Roslyn.Utilities.ReferenceEqualityComparer.Instance);
					if (num2 >= 0)
					{
						if (num != 0)
						{
							num2 += 7 * num;
						}
						string text = (_elementNames.IsDefault ? null : _elementNames[num2]);
						Location location = (_elementLocations.IsDefault ? null : _elementLocations[num2]);
						string text2 = TupleMemberName(num2 + 1);
						bool flag = text != text2;
						FieldSymbol underlyingField = fieldSymbol.AsMember(namedTypeSymbol);
						TupleElementFieldSymbol tupleElementFieldSymbol = ((num == 0) ? new TupleElementFieldSymbol(this, underlyingField, num2, location, flag, null) : new TupleVirtualElementFieldSymbol(this, underlyingField, text2, num2, location, cannotUse: false, flag, null));
						instance2.Add(tupleElementFieldSymbol);
						if (flag && !string.IsNullOrEmpty(text))
						{
							bool cannotUse = !_errorPositions.IsDefault && _errorPositions[num2];
							instance2.Add(new TupleVirtualElementFieldSymbol(this, underlyingField, text, num2, location, cannotUse, isImplicitlyDeclared: false, tupleElementFieldSymbol));
						}
						instance[num2] = true;
					}
					else if (num == 0)
					{
						instance2.Add(new TupleFieldSymbol(this, fieldSymbol.AsMember(namedTypeSymbol), -instance2.Count - 1));
					}
					break;
				}
				case SymbolKind.Property:
					if (num == 0)
					{
						instance2.Add(new TuplePropertySymbol(this, ((PropertySymbol)item).AsMember(namedTypeSymbol)));
					}
					break;
				case SymbolKind.Event:
					if (num == 0)
					{
						instance2.Add(new TupleEventSymbol(this, ((EventSymbol)item).AsMember(namedTypeSymbol)));
					}
					break;
				default:
					if (num == 0)
					{
						throw ExceptionUtilities.UnexpectedValue(item.Kind);
					}
					break;
				case SymbolKind.NamedType:
					break;
				}
			}
			if (namedTypeSymbol.Arity != 8)
			{
				break;
			}
			namedTypeSymbol = namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[7].TupleUnderlyingType;
			num++;
			if (namedTypeSymbol.Arity != 8)
			{
				members = namedTypeSymbol.OriginalDefinition.GetMembers();
				instance3.Clear();
				CollectTargetTupleFields(namedTypeSymbol, instance3);
			}
		}
		instance3.Free();
		for (int i = 0; i < instance.Count; i++)
		{
			if (!instance[i])
			{
				int num3 = NumberOfValueTuples(i + 1, out var remainder);
				NamedTypeSymbol originalDefinition = GetNestedTupleUnderlyingType(_underlyingType, num3 - 1).OriginalDefinition;
				CSDiagnosticInfo useSiteDiagnosticInfo = (originalDefinition.IsErrorType() ? null : new CSDiagnosticInfo(ErrorCode.ERR_PredefinedTypeMemberNotFoundInAssembly, TupleMemberName(remainder), originalDefinition, originalDefinition.ContainingAssembly));
				string text3 = (_elementNames.IsDefault ? null : _elementNames[i]);
				Location location2 = (_elementLocations.IsDefault ? null : _elementLocations[i]);
				string text4 = TupleMemberName(i + 1);
				bool flag2 = text3 != text4;
				TupleErrorFieldSymbol tupleErrorFieldSymbol = new TupleErrorFieldSymbol(this, text4, i, flag2 ? null : location2, _elementTypes[i], useSiteDiagnosticInfo, flag2, null);
				instance2.Add(tupleErrorFieldSymbol);
				if (flag2 && !string.IsNullOrEmpty(text3))
				{
					instance2.Add(new TupleErrorFieldSymbol(this, text3, i, location2, _elementTypes[i], useSiteDiagnosticInfo, isImplicitlyDeclared: false, tupleErrorFieldSymbol));
				}
			}
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
	}

	private static void CollectTargetTupleFields(NamedTypeSymbol underlying, ArrayBuilder<FieldSymbol> fieldsForElements)
	{
		underlying = underlying.OriginalDefinition;
		int num = Math.Min(underlying.Arity, 7);
		for (int i = 0; i < num; i++)
		{
			WellKnownMember tupleTypeMember = GetTupleTypeMember(underlying.Arity, i + 1);
			fieldsForElements.Add((FieldSymbol)GetWellKnownMemberInType(underlying, tupleTypeMember));
		}
	}

	private SmallDictionary<Symbol, Symbol> ComputeDefinitionToMemberMap()
	{
		SmallDictionary<Symbol, Symbol> smallDictionary = new SmallDictionary<Symbol, Symbol>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		_ = _underlyingType.OriginalDefinition;
		ImmutableArray<Symbol> members = GetMembers();
		for (int num = members.Length - 1; num >= 0; num--)
		{
			Symbol symbol = members[num];
			switch (symbol.Kind)
			{
			case SymbolKind.Method:
				smallDictionary.Add(((MethodSymbol)symbol).TupleUnderlyingMethod.OriginalDefinition, symbol);
				break;
			case SymbolKind.Field:
			{
				FieldSymbol tupleUnderlyingField = ((FieldSymbol)symbol).TupleUnderlyingField;
				if ((object)tupleUnderlyingField != null)
				{
					smallDictionary[tupleUnderlyingField.OriginalDefinition] = symbol;
				}
				break;
			}
			case SymbolKind.Property:
				smallDictionary.Add(((PropertySymbol)symbol).TupleUnderlyingProperty.OriginalDefinition, symbol);
				break;
			case SymbolKind.Event:
			{
				EventSymbol tupleUnderlyingEvent = ((EventSymbol)symbol).TupleUnderlyingEvent;
				FieldSymbol associatedField = tupleUnderlyingEvent.AssociatedField;
				if ((object)associatedField != null)
				{
					smallDictionary.Add(associatedField.OriginalDefinition, new TupleFieldSymbol(this, associatedField, -num - 1));
				}
				smallDictionary.Add(tupleUnderlyingEvent.OriginalDefinition, symbol);
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}
		return smallDictionary;
	}

	public TMember GetTupleMemberSymbolForUnderlyingMember<TMember>(TMember underlyingMemberOpt) where TMember : Symbol
	{
		if ((object)underlyingMemberOpt == null)
		{
			return null;
		}
		Symbol originalDefinition = underlyingMemberOpt.OriginalDefinition;
		if (originalDefinition.ContainingType == _underlyingType.OriginalDefinition && UnderlyingDefinitionToMemberMap.TryGetValue(originalDefinition, out var value))
		{
			return (TMember)value;
		}
		return null;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return GetMembers().WhereAsArray((Symbol m) => m.Name == name);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingType.GetAttributes();
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((comparison & TypeCompareKind.IgnoreTupleNames) != 0)
		{
			if ((object)t2 != null && t2.IsTupleType)
			{
				t2 = t2.TupleUnderlyingType;
			}
			return _underlyingType.Equals(t2, comparison);
		}
		return Equals(t2 as TupleTypeSymbol, comparison);
	}

	internal bool Equals(TupleTypeSymbol other)
	{
		return Equals(other, TypeCompareKind.ConsiderEverything);
	}

	private bool Equals(TupleTypeSymbol other, TypeCompareKind comparison)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other == null || !other._underlyingType.Equals(_underlyingType, comparison))
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreTupleNames) == 0)
		{
			if (_elementNames.IsDefault)
			{
				if (!other._elementNames.IsDefault)
				{
					return false;
				}
			}
			else
			{
				if (other._elementNames.IsDefault)
				{
					return false;
				}
				if (!_elementNames.SequenceEqual(other._elementNames))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return _underlyingType.GetHashCode();
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		return _underlyingType.GetUseSiteDiagnostic();
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		return _underlyingType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes);
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return AttributeUsageInfo.Null;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		ImmutableArray<Symbol> earlyAttributeDecodingMembers = _underlyingType.GetEarlyAttributeDecodingMembers();
		if (earlyAttributeDecodingMembers.IsEmpty)
		{
			return earlyAttributeDecodingMembers;
		}
		return earlyAttributeDecodingMembers.SelectAsArray((Symbol u, TupleTypeSymbol tuple) => tuple.GetTupleMemberSymbolForUnderlyingMember(u), this).WhereAsArray((Symbol m) => (object)m != null);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		ImmutableArray<Symbol> earlyAttributeDecodingMembers = _underlyingType.GetEarlyAttributeDecodingMembers(name);
		if (earlyAttributeDecodingMembers.IsEmpty)
		{
			return earlyAttributeDecodingMembers;
		}
		return earlyAttributeDecodingMembers.SelectAsArray((Symbol u, TupleTypeSymbol tuple) => tuple.GetTupleMemberSymbolForUnderlyingMember(u), this).WhereAsArray((Symbol m) => (object)m != null);
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return _underlyingType.GetDeclaredBaseType(basesBeingResolved);
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return _underlyingType.GetDeclaredInterfaces(basesBeingResolved);
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public static TypeSymbol TransformToTupleIfCompatible(TypeSymbol target)
	{
		if (!target.IsErrorType() && target.IsTupleCompatible())
		{
			return Create((NamedTypeSymbol)target);
		}
		return target;
	}
}
