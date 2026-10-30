using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class MethodSymbol : Symbol, ITypeMemberReference, IReference, INamedEntity, IMethodReference, ISignature, IGenericMethodInstanceReference, ISpecializedMethodReference, ITypeDefinitionMember, IDefinition, IMethodDefinition, IMethodSymbolInternal, IMethodSymbol, ISymbol, IEquatable<ISymbol>
{
	internal const MethodSymbol None = null;

	internal static readonly Func<TypeSymbol, bool> TypeSymbolIsNullFunction = (TypeSymbol type) => (object)type == null;

	private ParameterSignature _lazyParameterSignature;

	IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference
	{
		get
		{
			if (!IsDefinition && IsGenericMethod)
			{
				return this;
			}
			return null;
		}
	}

	ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference
	{
		get
		{
			if (!IsDefinition && (!IsGenericMethod || PEModuleBuilder.IsGenericType(ContainingType)))
			{
				return this;
			}
			return null;
		}
	}

	string INamedEntity.Name => MetadataName;

	bool IMethodReference.AcceptsExtraArguments => IsVararg;

	ushort IMethodReference.GenericParameterCount => (ushort)Arity;

	bool IMethodReference.IsGeneric => IsGenericMethod;

	ushort ISignature.ParameterCount => (ushort)ParameterCount;

	ImmutableArray<IParameterTypeInformation> IMethodReference.ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	CallingConvention ISignature.CallingConvention => CallingConvention;

	ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => ReturnTypeCustomModifiers.As<ICustomModifier>();

	ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => RefCustomModifiers.As<ICustomModifier>();

	bool ISignature.ReturnValueIsByRef => RefKind.IsManagedReference();

	IMethodReference ISpecializedMethodReference.UnspecializedVersion => OriginalDefinition;

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
	{
		get
		{
			if (this is SynthesizedGlobalMethodSymbol synthesizedGlobalMethodSymbol)
			{
				return synthesizedGlobalMethodSymbol.ContainingPrivateImplementationDetailsType;
			}
			return ContainingType;
		}
	}

	TypeMemberVisibility ITypeDefinitionMember.Visibility => PEModuleBuilder.MemberVisibility(this);

	IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters
	{
		get
		{
			foreach (TypeParameterSymbol typeParameter in TypeParameters)
			{
				yield return typeParameter;
			}
		}
	}

	bool IMethodDefinition.HasDeclarativeSecurity => HasDeclarativeSecurity;

	IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes => GetSecurityInformation();

	bool IMethodDefinition.IsAbstract => IsAbstract;

	bool IMethodDefinition.IsAccessCheckedOnOverride => IsAccessCheckedOnOverride;

	internal virtual bool IsAccessCheckedOnOverride
	{
		get
		{
			Accessibility declaredAccessibility = DeclaredAccessibility;
			if ((declaredAccessibility == Accessibility.Private || declaredAccessibility == Accessibility.ProtectedAndInternal || declaredAccessibility == Accessibility.Internal) && ((IMethodDefinition)this).IsVirtual)
			{
				return !((IMethodDefinition)this).IsSealed;
			}
			return false;
		}
	}

	bool IMethodDefinition.IsConstructor => MethodKind == MethodKind.Constructor;

	bool IMethodDefinition.IsExternal => IsExternal;

	internal virtual bool IsExternal
	{
		get
		{
			if (!IsExtern)
			{
				if ((object)ContainingType != null)
				{
					return ContainingType.TypeKind == TypeKind.Delegate;
				}
				return false;
			}
			return true;
		}
	}

	bool IMethodDefinition.IsHiddenBySignature => !HidesBaseMethodsByName;

	bool IMethodDefinition.IsNewSlot => IsMetadataNewSlot();

	bool IMethodDefinition.IsPlatformInvoke => GetDllImportData() != null;

	IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData => GetDllImportData();

	bool IMethodDefinition.IsRuntimeSpecial => HasRuntimeSpecialName;

	internal virtual bool HasRuntimeSpecialName
	{
		get
		{
			if (MethodKind != MethodKind.Constructor)
			{
				return MethodKind == MethodKind.StaticConstructor;
			}
			return true;
		}
	}

	bool IMethodDefinition.IsSealed => IsMetadataFinal;

	internal virtual bool IsMetadataFinal
	{
		get
		{
			if (!IsSealed)
			{
				if (IsMetadataVirtual())
				{
					if (!IsVirtual && !IsOverride && !IsAbstract)
					{
						return MethodKind != MethodKind.Destructor;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	bool IMethodDefinition.IsSpecialName => HasSpecialName;

	bool IMethodDefinition.IsStatic => IsStatic;

	bool IMethodDefinition.IsVirtual => IsMetadataVirtual();

	ImmutableArray<IParameterDefinition> IMethodDefinition.Parameters => EnumerateDefinitionParameters();

	bool IMethodDefinition.RequiresSecurityObject => RequiresSecurityObject;

	bool IMethodDefinition.ReturnValueIsMarshalledExplicitly => ReturnValueIsMarshalledExplicitly;

	internal virtual bool ReturnValueIsMarshalledExplicitly => ReturnValueMarshallingInformation != null;

	IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation => ReturnValueMarshallingInformation;

	ImmutableArray<byte> IMethodDefinition.ReturnValueMarshallingDescriptor => ReturnValueMarshallingDescriptor;

	internal virtual ImmutableArray<byte> ReturnValueMarshallingDescriptor => default;

	INamespace IMethodDefinition.ContainingNamespace => ContainingNamespace;

	public new virtual MethodSymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public abstract MethodKind MethodKind { get; }

	public abstract int Arity { get; }

	public virtual bool IsGenericMethod => Arity != 0;

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	public abstract bool IsExtensionMethod { get; }

	internal abstract bool HasSpecialName { get; }

	internal abstract MethodImplAttributes ImplementationAttributes { get; }

	internal abstract bool HasDeclarativeSecurity { get; }

	internal abstract MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation { get; }

	internal abstract bool RequiresSecurityObject { get; }

	public abstract bool HidesBaseMethodsByName { get; }

	public abstract bool IsVararg { get; }

	public virtual bool IsCheckedBuiltin => false;

	public abstract bool ReturnsVoid { get; }

	public abstract bool IsAsync { get; }

	public bool ReturnsByRef => RefKind == RefKind.Ref;

	public bool ReturnsByRefReadonly => RefKind == RefKind.In;

	public abstract RefKind RefKind { get; }

	public abstract TypeSymbol ReturnType { get; }

	public abstract ImmutableArray<TypeSymbol> TypeArguments { get; }

	public abstract ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	internal ParameterSymbol ThisParameter
	{
		get
		{
			if (!TryGetThisParameter(out var thisParameter))
			{
				throw ExceptionUtilities.Unreachable;
			}
			return thisParameter;
		}
	}

	internal virtual int ParameterCount => Parameters.Length;

	public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

	public virtual MethodSymbol ConstructedFrom => this;

	internal virtual bool IsExplicitInterfaceImplementation => ExplicitInterfaceImplementations.Any();

	public abstract ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations { get; }

	public abstract ImmutableArray<CustomModifier> ReturnTypeCustomModifiers { get; }

	public abstract ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public abstract Symbol AssociatedSymbol { get; }

	public MethodSymbol OverriddenMethod
	{
		get
		{
			if (IsOverride && (object)ConstructedFrom == this)
			{
				if (IsDefinition)
				{
					return (MethodSymbol)OverriddenOrHiddenMembers.GetOverriddenMember();
				}
				return (MethodSymbol)OverriddenOrHiddenMembersResult.GetOverriddenMember(this, OriginalDefinition.OverriddenMethod);
			}
			return null;
		}
	}

	internal bool IsConditional
	{
		get
		{
			if (GetAppliedConditionalSymbols().Any())
			{
				return true;
			}
			if (IsOverride)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null)
				{
					return overriddenMethod.IsConditional;
				}
			}
			return false;
		}
	}

	internal virtual OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers => this.MakeOverriddenOrHiddenMembers();

	public sealed override SymbolKind Kind => SymbolKind.Method;

	internal bool IsScriptConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor)
			{
				return ContainingType.IsScriptClass;
			}
			return false;
		}
	}

	internal virtual bool IsScriptInitializer => false;

	internal bool IsImplicitConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor || MethodKind == MethodKind.StaticConstructor)
			{
				return IsImplicitlyDeclared;
			}
			return false;
		}
	}

	internal bool IsImplicitInstanceConstructor
	{
		get
		{
			if (MethodKind == MethodKind.Constructor)
			{
				return IsImplicitlyDeclared;
			}
			return false;
		}
	}

	internal bool IsSubmissionConstructor
	{
		get
		{
			if (IsScriptConstructor)
			{
				return ContainingAssembly.IsInteractive;
			}
			return false;
		}
	}

	internal bool IsSubmissionInitializer
	{
		get
		{
			if (IsScriptInitializer)
			{
				return ContainingAssembly.IsInteractive;
			}
			return false;
		}
	}

	internal bool IsEntryPointCandidate
	{
		get
		{
			if (this.IsPartialDefinition() && (object)PartialImplementationPart == null)
			{
				return false;
			}
			if (IsStatic)
			{
				return Name == "Main";
			}
			return false;
		}
	}

	internal virtual MethodSymbol CallsiteReducedFromMethod => null;

	public virtual MethodSymbol PartialImplementationPart => null;

	public virtual MethodSymbol PartialDefinitionPart => null;

	public virtual MethodSymbol ReducedFrom => null;

	public virtual TypeSymbol ReceiverType => ContainingType;

	internal ImmutableArray<TypeSymbol> ParameterTypes
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterTypes;
		}
	}

	internal ImmutableArray<RefKind> ParameterRefKinds
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterRefKinds;
		}
	}

	internal abstract CallingConvention CallingConvention { get; }

	internal virtual TypeMap TypeSubstitution => null;

	protected override int HighestPriorityUseSiteError => 570;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				if (useSiteDiagnostic.Code != 570)
				{
					return useSiteDiagnostic.Code == 7085;
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsIterator => (object)IteratorElementType != null;

	internal virtual TypeSymbol IteratorElementType
	{
		get
		{
			return null;
		}
		set
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal virtual bool SynthesizesLoweredBoundBody => false;

	internal abstract bool GenerateDebugInfo { get; }

	MethodKind IMethodSymbol.MethodKind => MethodKind switch
	{
		MethodKind.AnonymousFunction => MethodKind.AnonymousFunction, 
		MethodKind.Constructor => MethodKind.Constructor, 
		MethodKind.Conversion => MethodKind.Conversion, 
		MethodKind.DelegateInvoke => MethodKind.DelegateInvoke, 
		MethodKind.Destructor => MethodKind.Destructor, 
		MethodKind.EventAdd => MethodKind.EventAdd, 
		MethodKind.EventRemove => MethodKind.EventRemove, 
		MethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation, 
		MethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator, 
		MethodKind.BuiltinOperator => MethodKind.BuiltinOperator, 
		MethodKind.Ordinary => MethodKind.Ordinary, 
		MethodKind.PropertyGet => MethodKind.PropertyGet, 
		MethodKind.PropertySet => MethodKind.PropertySet, 
		MethodKind.ReducedExtension => MethodKind.ReducedExtension, 
		MethodKind.StaticConstructor => MethodKind.StaticConstructor, 
		MethodKind.LocalFunction => MethodKind.LocalFunction, 
		_ => throw ExceptionUtilities.UnexpectedValue(MethodKind), 
	};

	ITypeSymbol IMethodSymbol.ReturnType => ReturnType;

	ImmutableArray<ITypeSymbol> IMethodSymbol.TypeArguments => StaticCast<ITypeSymbol>.From(TypeArguments);

	ImmutableArray<ITypeParameterSymbol> IMethodSymbol.TypeParameters => StaticCast<ITypeParameterSymbol>.From(TypeParameters);

	ImmutableArray<IParameterSymbol> IMethodSymbol.Parameters => StaticCast<IParameterSymbol>.From(Parameters);

	IMethodSymbol IMethodSymbol.ConstructedFrom => ConstructedFrom;

	IMethodSymbol IMethodSymbol.OriginalDefinition => OriginalDefinition;

	IMethodSymbol IMethodSymbol.OverriddenMethod => OverriddenMethod;

	ITypeSymbol IMethodSymbol.ReceiverType => ReceiverType;

	IMethodSymbol IMethodSymbol.ReducedFrom => ReducedFrom;

	ImmutableArray<IMethodSymbol> IMethodSymbol.ExplicitInterfaceImplementations => ExplicitInterfaceImplementations.Cast<MethodSymbol, IMethodSymbol>();

	ISymbol IMethodSymbol.AssociatedSymbol => AssociatedSymbol;

	bool IMethodSymbol.IsGenericMethod => IsGenericMethod;

	bool IMethodSymbol.IsAsync => IsAsync;

	bool IMethodSymbol.HidesBaseMethodsByName => HidesBaseMethodsByName;

	ImmutableArray<CustomModifier> IMethodSymbol.ReturnTypeCustomModifiers => ReturnTypeCustomModifiers;

	ImmutableArray<CustomModifier> IMethodSymbol.RefCustomModifiers => RefCustomModifiers;

	IMethodSymbol IMethodSymbol.PartialImplementationPart => PartialImplementationPart;

	IMethodSymbol IMethodSymbol.PartialDefinitionPart => PartialDefinitionPart;

	INamedTypeSymbol IMethodSymbol.AssociatedAnonymousDelegate => null;

	public virtual bool IsTupleMethod => false;

	public virtual MethodSymbol TupleUnderlyingMethod => null;

	bool IMethodSymbolInternal.IsIterator => IsIterator;

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return ResolvedMethodImpl(context);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		if (this is SynthesizedGlobalMethodSymbol synthesizedGlobalMethodSymbol)
		{
			return synthesizedGlobalMethodSymbol.ContainingPrivateImplementationDetailsType;
		}
		if (!IsDefinition)
		{
			return ((PEModuleBuilder)context.Module).Translate(ContainingType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}
		return ContainingType;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		if (!IsDefinition)
		{
			if (IsGenericMethod)
			{
				visitor.Visit((IGenericMethodInstanceReference)this);
			}
			else
			{
				visitor.Visit((IMethodReference)this);
			}
			return;
		}
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)visitor.Context.Module;
		if (ContainingModule == pEModuleBuilder.SourceModule)
		{
			visitor.Visit((IMethodDefinition)this);
		}
		else
		{
			visitor.Visit((IMethodReference)this);
		}
	}

	IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
	{
		return ResolvedMethodImpl(context);
	}

	private IMethodDefinition ResolvedMethodImpl(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (IsDefinition && ContainingModule == pEModuleBuilder.SourceModule)
		{
			return this;
		}
		return null;
	}

	ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (IsDefinition && ContainingModule == pEModuleBuilder.SourceModule)
		{
			return StaticCast<IParameterTypeInformation>.From(EnumerateDefinitionParameters());
		}
		return pEModuleBuilder.Translate(Parameters);
	}

	private ImmutableArray<IParameterDefinition> EnumerateDefinitionParameters()
	{
		return StaticCast<IParameterDefinition>.From(Parameters);
	}

	ITypeReference ISignature.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(ReturnType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	IEnumerable<ITypeReference> IGenericMethodInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (TypeSymbol typeArgument in TypeArguments)
		{
			yield return moduleBeingBuilt.Translate(typeArgument, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}
	}

	IMethodReference IGenericMethodInstanceReference.GetGenericMethod(EmitContext context)
	{
		if (!PEModuleBuilder.IsGenericType(ContainingType))
		{
			return ((PEModuleBuilder)context.Module).Translate(OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics, null, needDeclaration: true);
		}
		return new SpecializedMethodReference(ConstructedFrom);
	}

	IMethodBody IMethodDefinition.GetBody(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).GetMethodBody(this);
	}

	internal abstract bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false);

	MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
	{
		return ImplementationAttributes;
	}

	internal abstract bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false);

	IEnumerable<ICustomAttribute> IMethodDefinition.GetReturnValueAttributes(EmitContext context)
	{
		ImmutableArray<CSharpAttributeData> returnTypeAttributes = GetReturnTypeAttributes();
		ArrayBuilder<SynthesizedAttributeData> attributes = null;
		AddSynthesizedReturnTypeAttributes((PEModuleBuilder)context.Module, ref attributes);
		return GetCustomAttributesToEmit(returnTypeAttributes, attributes, isReturnType: true, emittingAssemblyAttributesInNetModule: false);
	}

	public abstract DllImportData GetDllImportData();

	internal abstract IEnumerable<SecurityAttribute> GetSecurityInformation();

	internal virtual bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		thisParameter = null;
		return false;
	}

	public virtual ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal MethodSymbol GetLeastOverriddenMethod(NamedTypeSymbol accessingTypeOpt)
	{
		NamedTypeSymbol originalDefinition = (((object)accessingTypeOpt == null) ? ContainingType : accessingTypeOpt).OriginalDefinition;
		MethodSymbol methodSymbol = this;
		while (methodSymbol.IsOverride && !methodSymbol.HidesBaseMethodsByName)
		{
			MethodSymbol overriddenMethod = methodSymbol.OverriddenMethod;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if ((object)overriddenMethod == null || !AccessCheck.IsSymbolAccessible(overriddenMethod, originalDefinition, ref useSiteDiagnostics))
			{
				break;
			}
			methodSymbol = overriddenMethod;
		}
		return methodSymbol;
	}

	internal MethodSymbol GetConstructedLeastOverriddenMethod(NamedTypeSymbol accessingTypeOpt)
	{
		MethodSymbol leastOverriddenMethod = ConstructedFrom.GetLeastOverriddenMethod(accessingTypeOpt);
		if (!leastOverriddenMethod.IsGenericMethod)
		{
			return leastOverriddenMethod;
		}
		return leastOverriddenMethod.Construct(TypeArguments);
	}

	internal virtual bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		if (syntaxTree != null)
		{
			return CallsAreConditionallyOmitted(syntaxTree);
		}
		return false;
	}

	private bool CallsAreConditionallyOmitted(SyntaxTree syntaxTree)
	{
		if (IsConditional)
		{
			ImmutableArray<string> appliedConditionalSymbols = GetAppliedConditionalSymbols();
			if (syntaxTree.IsAnyPreprocessorSymbolDefined(appliedConditionalSymbols))
			{
				return false;
			}
			if (IsOverride)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null && overriddenMethod.IsConditional)
				{
					return overriddenMethod.CallsAreConditionallyOmitted(syntaxTree);
				}
			}
			return true;
		}
		return false;
	}

	internal abstract ImmutableArray<string> GetAppliedConditionalSymbols();

	internal static bool CanOverrideOrHide(MethodKind kind)
	{
		switch (kind)
		{
		case MethodKind.AnonymousFunction:
		case MethodKind.Constructor:
		case MethodKind.Destructor:
		case MethodKind.ExplicitInterfaceImplementation:
		case MethodKind.ReducedExtension:
		case MethodKind.StaticConstructor:
			return false;
		case MethodKind.Conversion:
		case MethodKind.DelegateInvoke:
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		case MethodKind.UserDefinedOperator:
		case MethodKind.Ordinary:
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
		case MethodKind.LocalFunction:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMethod(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitMethod(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitMethod(this);
	}

	public MethodSymbol ReduceExtensionMethod(TypeSymbol receiverType)
	{
		if ((object)receiverType == null)
		{
			throw new ArgumentNullException("receiverType");
		}
		if (!IsExtensionMethod || MethodKind == MethodKind.ReducedExtension)
		{
			return null;
		}
		return ReducedExtensionMethodSymbol.Create(this, receiverType, null);
	}

	public MethodSymbol ReduceExtensionMethod()
	{
		if (!IsExtensionMethod || MethodKind == MethodKind.ReducedExtension)
		{
			return null;
		}
		return ReducedExtensionMethodSymbol.Create(this);
	}

	public virtual TypeSymbol GetTypeInferredDuringReduction(TypeParameterSymbol reducedFromTypeParameter)
	{
		throw new InvalidOperationException();
	}

	public MethodSymbol Construct(params TypeSymbol[] typeArguments)
	{
		return Construct(ImmutableArray.Create(typeArguments));
	}

	public MethodSymbol Construct(ImmutableArray<TypeSymbol> typeArguments)
	{
		if ((object)this != ConstructedFrom || Arity == 0)
		{
			throw new InvalidOperationException();
		}
		if (typeArguments.IsDefault)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (typeArguments.Any(TypeSymbolIsNullFunction))
		{
			throw new ArgumentException(CSharpResources.TypeArgumentCannotBeNull, "typeArguments");
		}
		if (typeArguments.Length != Arity)
		{
			throw new ArgumentException(CSharpResources.WrongNumberOfTypeArguments, "typeArguments");
		}
		if (TypeParametersMatchTypeArguments(TypeParameters, typeArguments))
		{
			return this;
		}
		return new ConstructedMethodSymbol(this, typeArguments);
	}

	internal static bool TypeParametersMatchTypeArguments(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeSymbol> typeArguments)
	{
		int length = typeParameters.Length;
		for (int i = 0; i < length; i++)
		{
			if ((object)typeArguments[i] != typeParameters[i])
			{
				return false;
			}
		}
		return true;
	}

	internal MethodSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!(newOwner == ContainingSymbol))
		{
			return new SubstitutedMethodSymbol(newOwner, this);
		}
		return this;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (IsDefinition)
		{
			return base.GetUseSiteDiagnostic();
		}
		return OriginalDefinition.GetUseSiteDiagnostic();
	}

	internal bool CalculateUseSiteDiagnostic(ref DiagnosticInfo result)
	{
		if (DeriveUseSiteDiagnosticFromType(ref result, ReturnType) || DeriveUseSiteDiagnosticFromCustomModifiers(ref result, RefCustomModifiers) || DeriveUseSiteDiagnosticFromCustomModifiers(ref result, ReturnTypeCustomModifiers) || DeriveUseSiteDiagnosticFromParameters(ref result, Parameters))
		{
			return true;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			if (ReturnType.GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, RefCustomModifiers, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, ReturnTypeCustomModifiers, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, Parameters, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, TypeParameters, this, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal virtual void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal abstract int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree);

	ITypeSymbol IMethodSymbol.GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter)
	{
		return GetTypeInferredDuringReduction(reducedFromTypeParameter.EnsureCSharpSymbolOrNull<ITypeParameterSymbol, TypeParameterSymbol>("reducedFromTypeParameter"));
	}

	IMethodSymbol IMethodSymbol.ReduceExtensionMethod(ITypeSymbol receiverType)
	{
		return ReduceExtensionMethod(receiverType.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("receiverType"));
	}

	ImmutableArray<AttributeData> IMethodSymbol.GetReturnTypeAttributes()
	{
		return GetReturnTypeAttributes().Cast<CSharpAttributeData, AttributeData>();
	}

	internal virtual void AddSynthesizedReturnTypeAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		if (ReturnsByRefReadonly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		TypeSymbol returnType = ReturnType;
		if (returnType.ContainsDynamic() && declaringCompilation.HasDynamicEmitAttributes())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(returnType, ReturnTypeCustomModifiers.Length + RefCustomModifiers.Length, RefKind));
		}
		if (returnType.ContainsTupleNames() && declaringCompilation.HasTupleNamesAttributes)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(returnType));
		}
	}

	IMethodSymbol IMethodSymbol.Construct(params ITypeSymbol[] arguments)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			arguments[i].EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("typeArguments");
		}
		return Construct(arguments.Cast<TypeSymbol>().AsImmutable());
	}

	int IMethodSymbolInternal.CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return CalculateLocalSyntaxOffset(localPosition, localTree);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitMethod(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitMethod(this);
	}
}
