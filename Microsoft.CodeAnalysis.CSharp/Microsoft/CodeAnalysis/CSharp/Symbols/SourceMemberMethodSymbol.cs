using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberMethodSymbol : SourceMethodSymbol, IAttributeTargetSymbol
{
	protected struct Flags
	{
		private const int MethodKindOffset = 0;

		private const int DeclarationModifiersOffset = 5;

		private const int MethodKindMask = 31;

		private const int DeclarationModifiersMask = 8388607;

		private const int ReturnsVoidBit = 134217728;

		private const int IsExtensionMethodBit = 268435456;

		private const int IsMetadataVirtualIgnoringInterfaceChangesBit = 536870912;

		private const int IsMetadataVirtualBit = 1073741824;

		private const int IsMetadataVirtualLockedBit = int.MinValue;

		private int _flags;

		private bool _returnsVoid;

		public bool ReturnsVoid
		{
			get
			{
				return _returnsVoid;
			}
			set
			{
				_returnsVoid = value;
			}
		}

		public MethodKind MethodKind => (MethodKind)(_flags & 0x1F);

		public bool IsExtensionMethod => (_flags & 0x10000000) != 0;

		public bool IsMetadataVirtualLocked => (_flags & int.MinValue) != 0;

		public DeclarationModifiers DeclarationModifiers => (DeclarationModifiers)((_flags >> 5) & 0x7FFFFF);

		private static bool ModifiersRequireMetadataVirtual(DeclarationModifiers modifiers)
		{
			return (modifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Virtual | DeclarationModifiers.Override)) != 0;
		}

		public Flags(MethodKind methodKind, DeclarationModifiers declarationModifiers, bool returnsVoid, bool isExtensionMethod, bool isMetadataVirtualIgnoringModifiers = false)
		{
			bool num = isMetadataVirtualIgnoringModifiers || ModifiersRequireMetadataVirtual(declarationModifiers);
			int num2 = (int)(methodKind & (MethodKind)0x1F);
			int num3 = (int)(declarationModifiers & (DeclarationModifiers.All | DeclarationModifiers.Unset)) << 5;
			int num4 = (isExtensionMethod ? 268435456 : 0);
			int num5 = (num ? 536870912 : 0);
			int num6 = (num ? 1073741824 : 0);
			_flags = num2 | num3 | num4 | num5 | num6;
			_returnsVoid = returnsVoid;
		}

		public bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
		{
			if (ignoreInterfaceImplementationChanges)
			{
				return (_flags & 0x20000000) != 0;
			}
			if (!IsMetadataVirtualLocked)
			{
				ThreadSafeFlagOperations.Set(ref _flags, int.MinValue);
			}
			return (_flags & 0x40000000) != 0;
		}

		public void EnsureMetadataVirtual()
		{
			if ((_flags & 0x40000000) == 0)
			{
				ThreadSafeFlagOperations.Set(ref _flags, 1073741824);
			}
		}
	}

	protected SymbolCompletionState state;

	protected Flags flags;

	private readonly NamedTypeSymbol _containingType;

	private ParameterSymbol _lazyThisParameter;

	private TypeSymbol _iteratorElementType;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private CustomAttributesBag<CSharpAttributeData> _lazyReturnTypeCustomAttributesBag;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	protected readonly SyntaxReference syntaxReferenceOpt;

	protected ImmutableArray<Location> locations;

	protected string lazyDocComment;

	private ImmutableArray<Diagnostic> _cachedDiagnostics;

	internal ImmutableArray<Diagnostic> Diagnostics => _cachedDiagnostics;

	protected virtual object MethodChecksLockObject => syntaxReferenceOpt;

	public sealed override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override Symbol AssociatedSymbol => null;

	public override bool ReturnsVoid => flags.ReturnsVoid;

	public sealed override MethodKind MethodKind => flags.MethodKind;

	public override bool IsExtensionMethod => flags.IsExtensionMethod;

	private bool IsMetadataVirtualLocked => flags.IsMetadataVirtualLocked;

	protected DeclarationModifiers DeclarationModifiers => flags.DeclarationModifiers;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(DeclarationModifiers);

	public sealed override bool IsExtern => (DeclarationModifiers & DeclarationModifiers.Extern) != 0;

	public sealed override bool IsSealed => (DeclarationModifiers & DeclarationModifiers.Sealed) != 0;

	public sealed override bool IsAbstract => (DeclarationModifiers & DeclarationModifiers.Abstract) != 0;

	public sealed override bool IsOverride => (DeclarationModifiers & DeclarationModifiers.Override) != 0;

	internal bool IsPartial => (DeclarationModifiers & DeclarationModifiers.Partial) != 0;

	public sealed override bool IsVirtual => (DeclarationModifiers & DeclarationModifiers.Virtual) != 0;

	internal bool IsNew => (DeclarationModifiers & DeclarationModifiers.New) != 0;

	public sealed override bool IsStatic => (DeclarationModifiers & DeclarationModifiers.Static) != 0;

	internal bool IsUnsafe => (DeclarationModifiers & DeclarationModifiers.Unsafe) != 0;

	public sealed override bool IsAsync => (DeclarationModifiers & DeclarationModifiers.Async) != 0;

	internal sealed override Microsoft.Cci.CallingConvention CallingConvention
	{
		get
		{
			Microsoft.Cci.CallingConvention callingConvention = (IsVararg ? Microsoft.Cci.CallingConvention.ExtraArguments : Microsoft.Cci.CallingConvention.Default);
			if (IsGenericMethod)
			{
				callingConvention |= Microsoft.Cci.CallingConvention.Generic;
			}
			if (!IsStatic)
			{
				callingConvention |= Microsoft.Cci.CallingConvention.HasThis;
			}
			return callingConvention;
		}
	}

	internal (BlockSyntax, ArrowExpressionClauseSyntax) Bodies
	{
		get
		{
			CSharpSyntaxNode syntaxNode = SyntaxNode;
			if (syntaxNode != null)
			{
				if (syntaxNode is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
				{
					BaseMethodDeclarationSyntax baseMethodDeclarationSyntax2 = baseMethodDeclarationSyntax;
					return (baseMethodDeclarationSyntax2.Body, baseMethodDeclarationSyntax2.ExpressionBody);
				}
				if (syntaxNode is AccessorDeclarationSyntax accessorDeclarationSyntax)
				{
					AccessorDeclarationSyntax accessorDeclarationSyntax2 = accessorDeclarationSyntax;
					return (accessorDeclarationSyntax2.Body, accessorDeclarationSyntax2.ExpressionBody);
				}
				if (syntaxNode is ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
				{
					ArrowExpressionClauseSyntax item = arrowExpressionClauseSyntax;
					return (null, item);
				}
				if (syntaxNode is BlockSyntax item2)
				{
					return (item2, null);
				}
			}
			return (null, null);
		}
	}

	internal SyntaxReference SyntaxRef => syntaxReferenceOpt;

	internal CSharpSyntaxNode SyntaxNode
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
			}
			return null;
		}
	}

	internal SyntaxTree SyntaxTree
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return syntaxReferenceOpt.SyntaxTree;
			}
			return null;
		}
	}

	public override ImmutableArray<Location> Locations => locations;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if (syntaxReferenceOpt != null)
			{
				return ImmutableArray.Create(syntaxReferenceOpt);
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override ImmutableArray<TypeSymbol> TypeArguments => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	public sealed override int Arity => TypeParameters.Length;

	internal override TypeSymbol IteratorElementType
	{
		get
		{
			return _iteratorElementType;
		}
		set
		{
			Interlocked.CompareExchange(ref _iteratorElementType, value, null);
		}
	}

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal sealed override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			LazyMethodChecks();
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	internal sealed override bool RequiresCompletion => true;

	protected virtual SourceMemberMethodSymbol BoundAttributesSource => null;

	protected virtual IAttributeTargetSymbol AttributeOwner => this;

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributeOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Method;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			switch (MethodKind)
			{
			case MethodKind.Constructor:
			case MethodKind.Destructor:
			case MethodKind.StaticConstructor:
				return AttributeLocation.Method;
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.PropertySet:
				return AttributeLocation.Method | AttributeLocation.Parameter | AttributeLocation.Return;
			default:
				return AttributeLocation.Method | AttributeLocation.Return;
			}
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!(ContainingType is SourceMemberContainerTypeSymbol { AnyMemberHasAttributes: not false }))
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((CommonMethodEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			if (syntaxReferenceOpt == null)
			{
				return null;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	public sealed override bool HidesBaseMethodsByName => false;

	internal override bool HasRuntimeSpecialName
	{
		get
		{
			if (!base.HasRuntimeSpecialName)
			{
				return IsVtableGapInterfaceMethod();
			}
			return true;
		}
	}

	internal sealed override bool HasSpecialName
	{
		get
		{
			switch (MethodKind)
			{
			case MethodKind.Constructor:
			case MethodKind.Conversion:
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.UserDefinedOperator:
			case MethodKind.PropertyGet:
			case MethodKind.PropertySet:
			case MethodKind.StaticConstructor:
				return true;
			default:
				if (IsVtableGapInterfaceMethod())
				{
					return true;
				}
				return GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;
			}
		}
	}

	internal sealed override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal sealed override bool RequiresSecurityObject => GetDecodedWellKnownAttributeData()?.HasDynamicSecurityMethodAttribute ?? false;

	internal sealed override bool HasDeclarativeSecurity => GetDecodedWellKnownAttributeData()?.HasDeclarativeSecurity ?? false;

	internal sealed override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => GetDecodedReturnTypeWellKnownAttributeData()?.MarshallingInformation;

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			MethodImplAttributes methodImplAttributes = GetDecodedWellKnownAttributeData()?.MethodImplAttributes ?? MethodImplAttributes.IL;
			if (ContainingType.IsComImport && MethodKind == MethodKind.Constructor)
			{
				methodImplAttributes |= (MethodImplAttributes)0x1003;
			}
			return methodImplAttributes;
		}
	}

	internal abstract bool IsExpressionBodied { get; }

	internal ImmutableArray<Diagnostic> SetDiagnostics(ImmutableArray<Diagnostic> newSet, out bool diagsWritten)
	{
		diagsWritten = ImmutableInterlocked.InterlockedInitialize(ref _cachedDiagnostics, newSet);
		return _cachedDiagnostics;
	}

	protected SourceMemberMethodSymbol(NamedTypeSymbol containingType, SyntaxReference syntaxReferenceOpt, Location location)
		: this(containingType, syntaxReferenceOpt, ImmutableArray.Create(location))
	{
	}

	protected SourceMemberMethodSymbol(NamedTypeSymbol containingType, SyntaxReference syntaxReferenceOpt, ImmutableArray<Location> locations)
	{
		_containingType = containingType;
		this.syntaxReferenceOpt = syntaxReferenceOpt;
		this.locations = locations;
	}

	protected void CheckEffectiveAccessibility(TypeSymbol returnType, ImmutableArray<ParameterSymbol> parameters, DiagnosticBag diagnostics)
	{
		if (DeclaredAccessibility <= Accessibility.Private)
		{
			return;
		}
		ErrorCode code = ((MethodKind == MethodKind.Conversion || MethodKind == MethodKind.UserDefinedOperator) ? ErrorCode.ERR_BadVisOpReturn : ErrorCode.ERR_BadVisReturnType);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!this.IsNoMoreVisibleThan(returnType, ref useSiteDiagnostics))
		{
			diagnostics.Add(code, Locations[0], this, returnType);
		}
		code = ((MethodKind == MethodKind.Conversion || MethodKind == MethodKind.UserDefinedOperator) ? ErrorCode.ERR_BadVisOpParam : ErrorCode.ERR_BadVisParamType);
		foreach (ParameterSymbol item in parameters)
		{
			if (!item.Type.IsAtLeastAsVisibleAs(this, ref useSiteDiagnostics))
			{
				diagnostics.Add(code, Locations[0], this, item.Type);
			}
		}
		diagnostics.Add(Locations[0], useSiteDiagnostics);
	}

	protected void MakeFlags(MethodKind methodKind, DeclarationModifiers declarationModifiers, bool returnsVoid, bool isExtensionMethod, bool isMetadataVirtualIgnoringModifiers = false)
	{
		flags = new Flags(methodKind, declarationModifiers, returnsVoid, isExtensionMethod, isMetadataVirtualIgnoringModifiers);
	}

	protected void SetReturnsVoid(bool returnsVoid)
	{
		flags.ReturnsVoid = returnsVoid;
	}

	protected abstract void MethodChecks(DiagnosticBag diagnostics);

	protected void LazyMethodChecks()
	{
		if (state.HasComplete(CompletionPart.StartMemberChecks))
		{
			return;
		}
		lock (MethodChecksLockObject)
		{
			if (state.NotePartComplete(CompletionPart.SynthesizedExplicitImplementations))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					MethodChecks(instance);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					state.NotePartComplete(CompletionPart.StartMemberChecks);
					instance.Free();
				}
			}
		}
	}

	protected virtual void LazyAsyncMethodChecks(CancellationToken cancellationToken)
	{
		state.NotePartComplete(CompletionPart.Members);
		state.NotePartComplete(CompletionPart.TypeMembers);
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		if (!IsOverride)
		{
			return IsMetadataVirtual(ignoreInterfaceImplementationChanges);
		}
		return this.RequiresExplicitOverride();
	}

	internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return flags.IsMetadataVirtual(ignoreInterfaceImplementationChanges);
	}

	internal void EnsureMetadataVirtual()
	{
		flags.EnsureMetadataVirtual();
	}

	internal Binder TryGetInMethodBinder(BinderFactory binderFactoryOpt = null)
	{
		CSharpSyntaxNode cSharpSyntaxNode = null;
		CSharpSyntaxNode syntaxNode = SyntaxNode;
		if (syntaxNode != null)
		{
			if (!(syntaxNode is ConstructorDeclarationSyntax constructorDeclarationSyntax))
			{
				if (!(syntaxNode is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax))
				{
					if (!(syntaxNode is AccessorDeclarationSyntax accessorDeclarationSyntax))
					{
						if (syntaxNode is ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
						{
							cSharpSyntaxNode = arrowExpressionClauseSyntax;
						}
					}
					else
					{
						AccessorDeclarationSyntax accessorDeclarationSyntax2 = accessorDeclarationSyntax;
						cSharpSyntaxNode = (CSharpSyntaxNode)(((object)accessorDeclarationSyntax2.Body) ?? ((object)accessorDeclarationSyntax2.ExpressionBody));
					}
				}
				else
				{
					BaseMethodDeclarationSyntax baseMethodDeclarationSyntax2 = baseMethodDeclarationSyntax;
					cSharpSyntaxNode = (CSharpSyntaxNode)(((object)baseMethodDeclarationSyntax2.Body) ?? ((object)baseMethodDeclarationSyntax2.ExpressionBody));
				}
			}
			else
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax2 = constructorDeclarationSyntax;
				cSharpSyntaxNode = (CSharpSyntaxNode)(constructorDeclarationSyntax2.Initializer ?? ((object)constructorDeclarationSyntax2.Body) ?? ((object)constructorDeclarationSyntax2.ExpressionBody));
			}
		}
		if (cSharpSyntaxNode == null)
		{
			return null;
		}
		return (binderFactoryOpt ?? DeclaringCompilation.GetBinderFactory(cSharpSyntaxNode.SyntaxTree)).GetBinder(cSharpSyntaxNode);
	}

	internal ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, BinderFlags additionalFlags = BinderFlags.None)
	{
		Binder binder = TryGetInMethodBinder(binderFactoryOpt);
		if (binder != null)
		{
			return new ExecutableCodeBinder(SyntaxNode, this, binder.WithAdditionalFlags(additionalFlags));
		}
		return null;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref lazyDocComment);
	}

	internal sealed override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		thisParameter = _lazyThisParameter;
		if ((object)thisParameter != null || IsStatic)
		{
			return true;
		}
		Interlocked.CompareExchange(ref _lazyThisParameter, new ThisParameterSymbol(this), null);
		thisParameter = _lazyThisParameter;
		return true;
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.ReturnTypeAttributes:
				GetReturnTypeAttributes();
				break;
			case CompletionPart.Type:
				_ = ReturnType;
				state.NotePartComplete(CompletionPart.Type);
				break;
			case CompletionPart.Parameters:
				foreach (ParameterSymbol parameter in Parameters)
				{
					parameter.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.Parameters);
				break;
			case CompletionPart.TypeParameters:
				foreach (TypeParameterSymbol typeParameter in TypeParameters)
				{
					typeParameter.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.TypeParameters);
				break;
			case CompletionPart.Members:
			case CompletionPart.TypeMembers:
				LazyAsyncMethodChecks(cancellationToken);
				break;
			case CompletionPart.SynthesizedExplicitImplementations:
			case CompletionPart.StartMemberChecks:
			{
				LazyMethodChecks();
				CompletionPart part = CompletionPart.MethodSymbolAll;
				state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompleted);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
	{
		return GetAttributeDeclarations();
	}

	internal CommonMethodEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonMethodEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	protected CommonMethodWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonMethodWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal CommonReturnTypeWellKnownAttributeData GetDecodedReturnTypeWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyReturnTypeCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetReturnTypeAttributesBag();
		}
		return (CommonReturnTypeWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		return GetAttributesBag(ref _lazyCustomAttributesBag, forReturnType: false);
	}

	private CustomAttributesBag<CSharpAttributeData> GetReturnTypeAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyReturnTypeCustomAttributesBag = _lazyReturnTypeCustomAttributesBag;
		if (lazyReturnTypeCustomAttributesBag != null && lazyReturnTypeCustomAttributesBag.IsSealed)
		{
			return lazyReturnTypeCustomAttributesBag;
		}
		return GetAttributesBag(ref _lazyReturnTypeCustomAttributesBag, forReturnType: true);
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag(ref CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag, bool forReturnType)
	{
		SourceMemberMethodSymbol boundAttributesSource = BoundAttributesSource;
		bool flag;
		if ((object)boundAttributesSource == null)
		{
			flag = ((!forReturnType) ? LoadAndValidateAttributes(GetAttributeDeclarations(), ref lazyCustomAttributesBag) : LoadAndValidateAttributes(GetReturnTypeAttributeDeclarations(), ref lazyCustomAttributesBag, AttributeLocation.Return));
		}
		else
		{
			CustomAttributesBag<CSharpAttributeData> value = (forReturnType ? boundAttributesSource.GetReturnTypeAttributesBag() : boundAttributesSource.GetAttributesBag());
			flag = Interlocked.CompareExchange(ref lazyCustomAttributesBag, value, null) == null;
		}
		if (flag)
		{
			CompletionPart part = ((!forReturnType) ? CompletionPart.Attributes : CompletionPart.ReturnTypeAttributes);
			state.NotePartComplete(part);
		}
		return lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return GetReturnTypeAttributesBag().Attributes;
	}

	internal override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (arguments.SymbolPart == AttributeLocation.None)
		{
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ConditionalAttribute))
			{
				CSharpAttributeData attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out var generatedDiagnostics);
				if (!attribute.HasErrors)
				{
					string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
					arguments.GetOrCreateData<CommonMethodEarlyWellKnownAttributeData>().AddConditionalSymbol(constructorArgument);
					if (!generatedDiagnostics)
					{
						return attribute;
					}
				}
				return null;
			}
			if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out var attributeData, out var obsoleteData))
			{
				if (obsoleteData != null)
				{
					arguments.GetOrCreateData<CommonMethodEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
				}
				return attributeData;
			}
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.ConditionalSymbols ?? ImmutableArray<string>.Empty;
	}

	internal override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.SymbolPart == AttributeLocation.None)
		{
			DecodeWellKnownAttributeAppliedToMethod(ref arguments);
		}
		else
		{
			DecodeWellKnownAttributeAppliedToReturnValue(ref arguments);
		}
	}

	private void DecodeWellKnownAttributeAppliedToMethod(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.PreserveSigAttribute))
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().SetPreserveSignature(arguments.Index);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.MethodImplAttribute))
		{
			AttributeData.DecodeMethodImplAttribute<CommonMethodWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>(ref arguments, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DllImportAttribute))
		{
			DecodeDllImportAttribute(ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.ConditionalAttribute))
		{
			ValidateConditionalAttribute(attribute, arguments.AttributeSyntaxOpt, arguments.Diagnostics);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.SuppressUnmanagedCodeSecurityAttribute))
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().HasSuppressUnmanagedCodeSecurityAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DynamicSecurityMethodAttribute))
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().HasDynamicSecurityMethodAttribute = true;
		}
		else
		{
			if (VerifyObsoleteAttributeAppliedToMethod(ref arguments, AttributeDescription.ObsoleteAttribute) || VerifyObsoleteAttributeAppliedToMethod(ref arguments, AttributeDescription.DeprecatedAttribute))
			{
				return;
			}
			if (attribute.IsTargetAttribute(this, AttributeDescription.IsReadOnlyAttribute))
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsReadOnlyAttribute.FullName);
			}
			else if (attribute.IsTargetAttribute(this, AttributeDescription.IsUnmanagedAttribute))
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsUnmanagedAttribute.FullName);
			}
			else if (attribute.IsTargetAttribute(this, AttributeDescription.IsByRefLikeAttribute))
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsByRefLikeAttribute.FullName);
			}
			else if (attribute.IsTargetAttribute(this, AttributeDescription.CaseSensitiveExtensionAttribute))
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitExtension, arguments.AttributeSyntaxOpt.Location);
			}
			else if (attribute.IsTargetAttribute(this, AttributeDescription.SecurityCriticalAttribute) || attribute.IsTargetAttribute(this, AttributeDescription.SecuritySafeCriticalAttribute))
			{
				if (IsAsync)
				{
					arguments.Diagnostics.Add(ErrorCode.ERR_SecurityCriticalOrSecuritySafeCriticalOnAsync, arguments.AttributeSyntaxOpt.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName());
				}
			}
			else
			{
				CSharpCompilation declaringCompilation = DeclaringCompilation;
				if (attribute.IsSecurityAttribute(declaringCompilation))
				{
					attribute.DecodeSecurityAttribute<CommonMethodWellKnownAttributeData>(this, declaringCompilation, ref arguments);
				}
			}
		}
	}

	private bool VerifyObsoleteAttributeAppliedToMethod(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, AttributeDescription description)
	{
		if (arguments.Attribute.IsTargetAttribute(this, description))
		{
			if (this.IsAccessor())
			{
				AttributeUsageInfo attributeUsageInfo = arguments.Attribute.AttributeClass.GetAttributeUsageInfo();
				arguments.Diagnostics.Add(ErrorCode.ERR_AttributeNotOnAccessor, arguments.AttributeSyntaxOpt.Name.Location, description.FullName, attributeUsageInfo.GetValidTargetsErrorArgument());
			}
			return true;
		}
		return false;
	}

	private void ValidateConditionalAttribute(CSharpAttributeData attribute, AttributeSyntax node, DiagnosticBag diagnostics)
	{
		if (this.IsAccessor())
		{
			AttributeUsageInfo attributeUsageInfo = attribute.AttributeClass.GetAttributeUsageInfo();
			diagnostics.Add(ErrorCode.ERR_AttributeNotOnAccessor, node.Name.Location, node.GetErrorDisplayName(), attributeUsageInfo.GetValidTargetsErrorArgument());
			return;
		}
		if (ContainingType.IsInterfaceType())
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnInterfaceMethod, node.Location);
			return;
		}
		if (IsOverride)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnOverride, node.Location, this);
			return;
		}
		if (!CanBeReferencedByName || MethodKind == MethodKind.Destructor)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnSpecialMethod, node.Location, this);
			return;
		}
		if (!ReturnsVoid)
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalMustReturnVoid, node.Location, this);
			return;
		}
		if (HasAnyOutParameter())
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalWithOutParam, node.Location, this);
			return;
		}
		string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (constructorArgument == null || !SyntaxFacts.IsValidIdentifier(constructorArgument))
		{
			CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(0, node);
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, attributeArgumentSyntax.Location, node.GetErrorDisplayName());
		}
	}

	private bool HasAnyOutParameter()
	{
		foreach (ParameterSymbol parameter in Parameters)
		{
			if (parameter.RefKind == RefKind.Out)
			{
				return true;
			}
		}
		return false;
	}

	private void DecodeWellKnownAttributeAppliedToReturnValue(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.MarshalAsAttribute))
		{
			MarshalAsAttributeDecoder<CommonReturnTypeWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>.Decode(ref arguments, AttributeTargets.ReturnValue, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DynamicAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsUnmanagedAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsUnmanagedAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsReadOnlyAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsReadOnlyAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsByRefLikeAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsByRefLikeAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
		}
	}

	private void DecodeDllImportAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		bool flag = false;
		if (!IsExtern || !IsStatic)
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_DllImportOnInvalidMethod, arguments.AttributeSyntaxOpt.Name.Location);
			flag = true;
		}
		if (IsGenericMethod || ((object)_containingType != null && _containingType.IsGenericType))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_DllImportOnGenericMethod, arguments.AttributeSyntaxOpt.Name.Location);
			flag = true;
		}
		string text = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (!MetadataHelpers.IsValidMetadataIdentifier(text))
		{
			CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(0, arguments.AttributeSyntaxOpt);
			arguments.Diagnostics.Add(ErrorCode.ERR_InvalidAttributeArgument, attributeArgumentSyntax.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName());
			flag = true;
			text = null;
		}
		CharSet charSet = GetEffectiveDefaultMarshallingCharSet() ?? CharSet.None;
		string text2 = null;
		bool preserveSig = true;
		System.Runtime.InteropServices.CallingConvention callingConvention = System.Runtime.InteropServices.CallingConvention.Winapi;
		bool setLastError = false;
		bool exactSpelling = false;
		bool? useBestFit = null;
		bool? throwOnUnmappable = null;
		int num = 1;
		foreach (KeyValuePair<string, TypedConstant> commonNamedArgument in attribute.CommonNamedArguments)
		{
			switch (commonNamedArgument.Key)
			{
			case "EntryPoint":
				text2 = commonNamedArgument.Value.Value as string;
				if (!MetadataHelpers.IsValidMetadataIdentifier(text2))
				{
					arguments.Diagnostics.Add(ErrorCode.ERR_InvalidNamedArgument, arguments.AttributeSyntaxOpt.ArgumentList.Arguments[num].Location, commonNamedArgument.Key);
					flag = true;
					text2 = null;
				}
				break;
			case "CharSet":
				charSet = commonNamedArgument.Value.DecodeValue<CharSet>(SpecialType.System_Enum);
				break;
			case "SetLastError":
				setLastError = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "ExactSpelling":
				exactSpelling = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "PreserveSig":
				preserveSig = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "CallingConvention":
				callingConvention = commonNamedArgument.Value.DecodeValue<System.Runtime.InteropServices.CallingConvention>(SpecialType.System_Enum);
				break;
			case "BestFitMapping":
				useBestFit = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			case "ThrowOnUnmappableChar":
				throwOnUnmappable = commonNamedArgument.Value.DecodeValue<bool>(SpecialType.System_Boolean);
				break;
			}
			num++;
		}
		if (!flag)
		{
			arguments.GetOrCreateData<CommonMethodWellKnownAttributeData>().SetDllImport(arguments.Index, text, text2, DllImportData.MakeFlags(exactSpelling, charSet, setLastError, callingConvention, useBestFit, throwOnUnmappable), preserveSig);
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		if (symbolPart != AttributeLocation.Return && _containingType.IsComImport && _containingType.TypeKind == TypeKind.Class)
		{
			MethodKind methodKind = MethodKind;
			if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
			{
				if (!IsImplicitlyDeclared)
				{
					diagnostics.Add(ErrorCode.ERR_ComImportWithUserCtor, Locations[0]);
				}
			}
			else if (!IsAbstract && !IsExtern)
			{
				diagnostics.Add(ErrorCode.ERR_ComImportWithImpl, Locations[0], this, _containingType);
			}
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	private bool IsVtableGapInterfaceMethod()
	{
		if (ContainingType.IsInterface)
		{
			return ModuleExtensions.GetVTableGapSize(MetadataName) > 0;
		}
		return false;
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		CustomAttributesBag<CSharpAttributeData> attributesBag = GetAttributesBag();
		CommonMethodWellKnownAttributeData commonMethodWellKnownAttributeData = (CommonMethodWellKnownAttributeData)attributesBag.DecodedWellKnownAttributeData;
		if (commonMethodWellKnownAttributeData != null)
		{
			SecurityWellKnownAttributeData securityInformation = commonMethodWellKnownAttributeData.SecurityInformation;
			if (securityInformation != null)
			{
				return securityInformation.GetSecurityAttributes(attributesBag.Attributes);
			}
		}
		return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
	}

	public sealed override DllImportData GetDllImportData()
	{
		return GetDecodedWellKnownAttributeData()?.DllImportPlatformInvokeData;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (IsAsync || IsIterator)
		{
			CSharpCompilation declaringCompilation = DeclaringCompilation;
			if (moduleBuilder.CompilationState.TryGetStateMachineType(this, out var stateMachineType))
			{
				WellKnownMember constructor = (IsAsync ? WellKnownMember.System_Runtime_CompilerServices_AsyncStateMachineAttribute__ctor : WellKnownMember.System_Runtime_CompilerServices_IteratorStateMachineAttribute__ctor);
				TypedConstant item = new TypedConstant(declaringCompilation.GetWellKnownType(WellKnownType.System_Type), TypedConstantKind.Type, stateMachineType.GetUnboundGenericTypeOrSelf());
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(constructor, ImmutableArray.Create(item)));
			}
			if (IsAsync)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDebuggerStepThroughAttribute());
			}
		}
	}

	protected void CheckModifiersForBody(Location location, DiagnosticBag diagnostics)
	{
		if (_containingType.IsInterface)
		{
			diagnostics.Add(ErrorCode.ERR_InterfaceMemberHasBody, location, this);
		}
		else if (IsExtern && !IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
		}
		else if (IsAbstract && !IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractHasBody, location, this);
		}
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		var (blockSyntax, arrowExpressionClauseSyntax) = Bodies;
		CSharpSyntaxNode cSharpSyntaxNode;
		if (blockSyntax != null && blockSyntax.Span.Contains(localPosition))
		{
			cSharpSyntaxNode = blockSyntax;
		}
		else
		{
			if (arrowExpressionClauseSyntax == null || !arrowExpressionClauseSyntax.Span.Contains(localPosition))
			{
				return -1;
			}
			cSharpSyntaxNode = arrowExpressionClauseSyntax;
		}
		return localPosition - cSharpSyntaxNode.SpanStart;
	}
}
