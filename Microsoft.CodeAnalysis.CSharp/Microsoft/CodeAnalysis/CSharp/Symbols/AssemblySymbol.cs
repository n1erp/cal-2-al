using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class AssemblySymbol : Symbol, IAssemblySymbolInternal, IAssemblySymbol, ISymbol, IEquatable<ISymbol>
{
	private AssemblySymbol _corLibrary;

	private static readonly char[] s_nestedTypeNameSeparators = new char[1] { '+' };

	internal AssemblySymbol CorLibrary => _corLibrary;

	public override string Name => Identity.Name;

	public abstract AssemblyIdentity Identity { get; }

	public abstract Version AssemblyVersionPattern { get; }

	internal Machine Machine => Modules[0].Machine;

	internal bool Bit32Required => Modules[0].Bit32Required;

	public abstract NamespaceSymbol GlobalNamespace { get; }

	public abstract ImmutableArray<ModuleSymbol> Modules { get; }

	public sealed override SymbolKind Kind => SymbolKind.Assembly;

	public sealed override AssemblySymbol ContainingAssembly => null;

	internal abstract bool IsMissing { get; }

	public sealed override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public sealed override bool IsStatic => false;

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	public sealed override bool IsExtern => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public virtual bool IsInteractive => false;

	public sealed override Symbol ContainingSymbol => null;

	internal virtual bool KeepLookingForDeclaredSpecialTypes
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal abstract bool IsLinked { get; }

	public abstract ICollection<string> TypeNames { get; }

	public abstract ICollection<string> NamespaceNames { get; }

	public abstract bool MightContainExtensionMethods { get; }

	internal static TypeSymbol DynamicType => DynamicTypeSymbol.Instance;

	internal NamedTypeSymbol ObjectType => GetSpecialType(SpecialType.System_Object);

	internal abstract ImmutableArray<byte> PublicKey { get; }

	INamespaceSymbol IAssemblySymbol.GlobalNamespace => GlobalNamespace;

	IEnumerable<IModuleSymbol> IAssemblySymbol.Modules => Modules;

	internal void SetCorLibrary(AssemblySymbol corLibrary)
	{
		_corLibrary = corLibrary;
	}

	internal NamespaceSymbol GetAssemblyNamespace(NamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol.IsGlobalNamespace)
		{
			return GlobalNamespace;
		}
		NamespaceSymbol containingNamespace = namespaceSymbol.ContainingNamespace;
		if ((object)containingNamespace == null)
		{
			return GlobalNamespace;
		}
		if (namespaceSymbol.NamespaceKind == NamespaceKind.Assembly && namespaceSymbol.ContainingAssembly == this)
		{
			return namespaceSymbol;
		}
		NamespaceSymbol assemblyNamespace = GetAssemblyNamespace(containingNamespace);
		if ((object)assemblyNamespace == containingNamespace)
		{
			return namespaceSymbol;
		}
		return assemblyNamespace?.GetNestedNamespace(namespaceSymbol.Name);
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAssembly(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitAssembly(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAssembly(this);
	}

	internal AssemblySymbol()
	{
	}

	internal NamedTypeSymbol LookupTopLevelMetadataType(ref MetadataTypeName emittedName, bool digThroughForwardedTypes)
	{
		return LookupTopLevelMetadataTypeWithCycleDetection(ref emittedName, null, digThroughForwardedTypes);
	}

	internal abstract NamedTypeSymbol LookupTopLevelMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies, bool digThroughForwardedTypes);

	public NamedTypeSymbol ResolveForwardedType(string fullyQualifiedMetadataName)
	{
		if (fullyQualifiedMetadataName == null)
		{
			throw new ArgumentNullException("fullyQualifiedMetadataName");
		}
		MetadataTypeName emittedName = MetadataTypeName.FromFullName(fullyQualifiedMetadataName);
		return TryLookupForwardedMetadataType(ref emittedName);
	}

	internal NamedTypeSymbol TryLookupForwardedMetadataType(ref MetadataTypeName emittedName)
	{
		return TryLookupForwardedMetadataTypeWithCycleDetection(ref emittedName, null);
	}

	internal virtual NamedTypeSymbol TryLookupForwardedMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies)
	{
		return null;
	}

	internal ErrorTypeSymbol CreateCycleInTypeForwarderErrorTypeSymbol(ref MetadataTypeName emittedName)
	{
		DiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_CycleInTypeForwarder, emittedName.FullName, Name);
		return new MissingMetadataTypeSymbol.TopLevelWithCustomErrorInfo(Modules[0], ref emittedName, errorInfo);
	}

	internal ErrorTypeSymbol CreateMultipleForwardingErrorTypeSymbol(ref MetadataTypeName emittedName, ModuleSymbol forwardingModule, AssemblySymbol destination1, AssemblySymbol destination2)
	{
		CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_TypeForwardedToMultipleAssemblies, forwardingModule, this, emittedName.FullName, destination1, destination2);
		return new MissingMetadataTypeSymbol.TopLevelWithCustomErrorInfo(forwardingModule, ref emittedName, errorInfo);
	}

	internal abstract NamedTypeSymbol GetDeclaredSpecialType(SpecialType type);

	internal virtual void RegisterDeclaredSpecialType(NamedTypeSymbol corType)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal abstract ImmutableArray<AssemblySymbol> GetNoPiaResolutionAssemblies();

	internal abstract void SetNoPiaResolutionAssemblies(ImmutableArray<AssemblySymbol> assemblies);

	internal abstract ImmutableArray<AssemblySymbol> GetLinkedReferencedAssemblies();

	internal abstract void SetLinkedReferencedAssemblies(ImmutableArray<AssemblySymbol> assemblies);

	internal abstract IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName);

	internal abstract bool AreInternalsVisibleToThisAssembly(AssemblySymbol other);

	internal virtual bool GetGuidString(out string guidString)
	{
		return GetGuidStringDefaultImplementation(out guidString);
	}

	internal NamedTypeSymbol GetSpecialType(SpecialType type)
	{
		return CorLibrary.GetDeclaredSpecialType(type);
	}

	internal NamedTypeSymbol GetPrimitiveType(PrimitiveTypeCode type)
	{
		return GetSpecialType(SpecialTypes.GetTypeFromMetadataName(type));
	}

	public NamedTypeSymbol GetTypeByMetadataName(string fullyQualifiedMetadataName)
	{
		if (fullyQualifiedMetadataName == null)
		{
			throw new ArgumentNullException("fullyQualifiedMetadataName");
		}
		(AssemblySymbol, AssemblySymbol) conflicts;
		return GetTypeByMetadataName(fullyQualifiedMetadataName, includeReferences: false, isWellKnownType: false, out conflicts);
	}

	internal NamedTypeSymbol GetTypeByMetadataName(string metadataName, bool includeReferences, bool isWellKnownType, out (AssemblySymbol, AssemblySymbol) conflicts, bool useCLSCompliantNameArityEncoding = false, DiagnosticBag warnings = null, bool ignoreCorLibraryDuplicatedTypes = false)
	{
		NamedTypeSymbol namedTypeSymbol;
		if (metadataName.IndexOf('+') >= 0)
		{
			string[] array = metadataName.Split(s_nestedTypeNameSeparators);
			MetadataTypeName metadataName2 = MetadataTypeName.FromFullName(array[0], useCLSCompliantNameArityEncoding);
			namedTypeSymbol = GetTopLevelTypeByMetadataName(ref metadataName2, null, includeReferences, isWellKnownType, out conflicts, warnings, ignoreCorLibraryDuplicatedTypes);
			int num = 1;
			while ((object)namedTypeSymbol != null && !namedTypeSymbol.IsErrorType() && num < array.Length)
			{
				metadataName2 = MetadataTypeName.FromTypeName(array[num]);
				NamedTypeSymbol namedTypeSymbol2 = namedTypeSymbol.LookupMetadataType(ref metadataName2);
				namedTypeSymbol = ((!isWellKnownType || IsValidWellKnownType(namedTypeSymbol2)) ? namedTypeSymbol2 : null);
				num++;
			}
		}
		else
		{
			MetadataTypeName metadataName2 = MetadataTypeName.FromFullName(metadataName, useCLSCompliantNameArityEncoding);
			namedTypeSymbol = GetTopLevelTypeByMetadataName(ref metadataName2, null, includeReferences, isWellKnownType, out conflicts, warnings, ignoreCorLibraryDuplicatedTypes);
		}
		if ((object)namedTypeSymbol != null && !namedTypeSymbol.IsErrorType())
		{
			return namedTypeSymbol;
		}
		return null;
	}

	internal TypeSymbol GetTypeByReflectionType(Type type, bool includeReferences)
	{
		System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsArray)
		{
			TypeSymbol typeByReflectionType = GetTypeByReflectionType(typeInfo.GetElementType(), includeReferences);
			if ((object)typeByReflectionType == null)
			{
				return null;
			}
			int arrayRank = typeInfo.GetArrayRank();
			return ArrayTypeSymbol.CreateCSharpArray(this, typeByReflectionType, ImmutableArray<CustomModifier>.Empty, arrayRank);
		}
		if (typeInfo.IsPointer)
		{
			TypeSymbol typeByReflectionType2 = GetTypeByReflectionType(typeInfo.GetElementType(), includeReferences);
			if ((object)typeByReflectionType2 == null)
			{
				return null;
			}
			return new PointerTypeSymbol(typeByReflectionType2);
		}
		if ((object)typeInfo.DeclaringType != null)
		{
			Type[] genericTypeArguments = typeInfo.GenericTypeArguments;
			int currentTypeArgument = 0;
			System.Reflection.TypeInfo typeInfo2 = (typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition().GetTypeInfo() : typeInfo);
			ArrayBuilder<System.Reflection.TypeInfo> instance = ArrayBuilder<System.Reflection.TypeInfo>.GetInstance();
			while (true)
			{
				instance.Add(typeInfo2);
				if ((object)typeInfo2.DeclaringType == null)
				{
					break;
				}
				typeInfo2 = typeInfo2.DeclaringType.GetTypeInfo();
			}
			int num = instance.Count - 1;
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)GetTypeByReflectionType(instance[num].AsType(), includeReferences);
			if ((object)namedTypeSymbol == null)
			{
				return null;
			}
			while (--num >= 0)
			{
				int forcedArity = instance[num].GenericTypeParameters.Length - instance[num + 1].GenericTypeParameters.Length;
				MetadataTypeName emittedTypeName = MetadataTypeName.FromTypeName(instance[num].Name, useCLSCompliantNameArityEncoding: false, forcedArity);
				namedTypeSymbol = namedTypeSymbol.LookupMetadataType(ref emittedTypeName);
				if ((object)namedTypeSymbol == null || namedTypeSymbol.IsErrorType())
				{
					return null;
				}
				namedTypeSymbol = ApplyGenericArguments(namedTypeSymbol, genericTypeArguments, ref currentTypeArgument, includeReferences);
				if ((object)namedTypeSymbol == null)
				{
					return null;
				}
			}
			instance.Free();
			return namedTypeSymbol;
		}
		AssemblyIdentity assemblyOpt = AssemblyIdentity.FromAssemblyDefinition(typeInfo.Assembly);
		MetadataTypeName metadataName = MetadataTypeName.FromNamespaceAndTypeName(typeInfo.Namespace ?? string.Empty, typeInfo.Name, useCLSCompliantNameArityEncoding: false, typeInfo.GenericTypeArguments.Length);
		NamedTypeSymbol topLevelTypeByMetadataName = GetTopLevelTypeByMetadataName(ref metadataName, assemblyOpt, includeReferences, isWellKnownType: false, out var _);
		if ((object)topLevelTypeByMetadataName == null || topLevelTypeByMetadataName.IsErrorType())
		{
			return null;
		}
		int currentTypeArgument2 = 0;
		Type[] genericTypeArguments2 = typeInfo.GenericTypeArguments;
		return ApplyGenericArguments(topLevelTypeByMetadataName, genericTypeArguments2, ref currentTypeArgument2, includeReferences);
	}

	private NamedTypeSymbol ApplyGenericArguments(NamedTypeSymbol symbol, Type[] typeArguments, ref int currentTypeArgument, bool includeReferences)
	{
		if (typeArguments.Length - currentTypeArgument == 0)
		{
			return symbol;
		}
		TypeWithModifiers[] array = new TypeWithModifiers[symbol.TypeArgumentsNoUseSiteDiagnostics.Length];
		for (int i = 0; i < array.Length; i++)
		{
			TypeSymbol typeByReflectionType = GetTypeByReflectionType(typeArguments[currentTypeArgument++], includeReferences);
			if ((object)typeByReflectionType == null)
			{
				return null;
			}
			array[i] = new TypeWithModifiers(typeByReflectionType);
		}
		return symbol.ConstructIfGeneric(array.AsImmutableOrNull());
	}

	internal NamedTypeSymbol GetTopLevelTypeByMetadataName(ref MetadataTypeName metadataName, AssemblyIdentity assemblyOpt, bool includeReferences, bool isWellKnownType, out (AssemblySymbol, AssemblySymbol) conflicts, DiagnosticBag warnings = null, bool ignoreCorLibraryDuplicatedTypes = false)
	{
		conflicts = default;
		NamedTypeSymbol namedTypeSymbol = GetTopLevelTypeByMetadataName(this, ref metadataName, assemblyOpt);
		if (isWellKnownType && !IsValidWellKnownType(namedTypeSymbol))
		{
			namedTypeSymbol = null;
		}
		if ((object)namedTypeSymbol != null || !includeReferences)
		{
			return namedTypeSymbol;
		}
		ArrayBuilder<AssemblySymbol> instance = ArrayBuilder<AssemblySymbol>.GetInstance();
		if (assemblyOpt != null)
		{
			instance.AddRange(DeclaringCompilation.GetBoundReferenceManager().ReferencedAssemblies);
		}
		else
		{
			DeclaringCompilation.GetUnaliasedReferencedAssemblies(instance);
		}
		foreach (AssemblySymbol item in instance)
		{
			NamedTypeSymbol namedTypeSymbol2 = GetTopLevelTypeByMetadataName(item, ref metadataName, assemblyOpt);
			if (isWellKnownType && !IsValidWellKnownType(namedTypeSymbol2))
			{
				namedTypeSymbol2 = null;
			}
			if ((object)namedTypeSymbol2 == null || namedTypeSymbol2.IsHiddenByCodeAnalysisEmbeddedAttribute())
			{
				continue;
			}
			if ((object)namedTypeSymbol != null)
			{
				if (ignoreCorLibraryDuplicatedTypes)
				{
					if (IsInCorLib(namedTypeSymbol2))
					{
						continue;
					}
					if (IsInCorLib(namedTypeSymbol))
					{
						namedTypeSymbol = namedTypeSymbol2;
						continue;
					}
				}
				if (warnings == null)
				{
					conflicts = (namedTypeSymbol.ContainingAssembly, namedTypeSymbol2.ContainingAssembly);
					namedTypeSymbol = null;
				}
				else
				{
					warnings.Add(ErrorCode.WRN_MultiplePredefTypes, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingAssembly);
				}
				break;
			}
			namedTypeSymbol = namedTypeSymbol2;
		}
		instance.Free();
		return namedTypeSymbol;
	}

	private bool IsInCorLib(NamedTypeSymbol type)
	{
		return (object)type.ContainingAssembly == CorLibrary;
	}

	private bool IsValidWellKnownType(NamedTypeSymbol result)
	{
		if ((object)result == null || result.TypeKind == TypeKind.Error)
		{
			return false;
		}
		if (result.DeclaredAccessibility != Accessibility.Public)
		{
			return Symbol.IsSymbolAccessible(result, this);
		}
		return true;
	}

	private static NamedTypeSymbol GetTopLevelTypeByMetadataName(AssemblySymbol assembly, ref MetadataTypeName metadataName, AssemblyIdentity assemblyOpt)
	{
		NamedTypeSymbol namedTypeSymbol = assembly.LookupTopLevelMetadataType(ref metadataName, digThroughForwardedTypes: false);
		if (!IsAcceptableMatchForGetTypeByMetadataName(namedTypeSymbol))
		{
			return null;
		}
		if (assemblyOpt != null && !assemblyOpt.Equals(assembly.Identity))
		{
			return null;
		}
		return namedTypeSymbol;
	}

	private static bool IsAcceptableMatchForGetTypeByMetadataName(NamedTypeSymbol candidate)
	{
		if (candidate.Kind == SymbolKind.ErrorType)
		{
			return !(candidate is MissingMetadataTypeSymbol);
		}
		return true;
	}

	internal virtual Symbol GetDeclaredSpecialTypeMember(SpecialMember member)
	{
		return null;
	}

	internal virtual Symbol GetSpecialTypeMember(SpecialMember member)
	{
		return CorLibrary.GetDeclaredSpecialTypeMember(member);
	}

	public abstract AssemblyMetadata GetMetadata();

	INamedTypeSymbol IAssemblySymbol.ResolveForwardedType(string fullyQualifiedMetadataName)
	{
		return ResolveForwardedType(fullyQualifiedMetadataName);
	}

	bool IAssemblySymbol.GivesAccessTo(IAssemblySymbol assemblyWantingAccess)
	{
		if (object.Equals(this, assemblyWantingAccess))
		{
			return true;
		}
		IEnumerable<ImmutableArray<byte>> internalsVisibleToPublicKeys = GetInternalsVisibleToPublicKeys(assemblyWantingAccess.Name);
		if (internalsVisibleToPublicKeys.Any() && assemblyWantingAccess.IsNetModule())
		{
			return true;
		}
		foreach (ImmutableArray<byte> item in internalsVisibleToPublicKeys)
		{
			IVTConclusion iVTConclusion = Identity.PerformIVTCheck(assemblyWantingAccess.Identity.PublicKey, item);
			if (iVTConclusion == IVTConclusion.Match || iVTConclusion == IVTConclusion.OneSignedOneNot)
			{
				return true;
			}
		}
		return false;
	}

	INamedTypeSymbol IAssemblySymbol.GetTypeByMetadataName(string metadataName)
	{
		return GetTypeByMetadataName(metadataName);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAssembly(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAssembly(this);
	}
}
