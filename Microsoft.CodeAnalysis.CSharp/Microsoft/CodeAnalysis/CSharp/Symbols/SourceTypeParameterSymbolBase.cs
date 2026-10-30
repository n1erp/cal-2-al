using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceTypeParameterSymbolBase : TypeParameterSymbol, IAttributeTargetSymbol
{
	private readonly ImmutableArray<SyntaxReference> _syntaxRefs;

	private readonly ImmutableArray<Location> _locations;

	private readonly string _name;

	private readonly short _ordinal;

	private SymbolCompletionState _state;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private TypeParameterBounds _lazyBounds = TypeParameterBounds.Unset;

	public override ImmutableArray<Location> Locations => _locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _syntaxRefs;

	internal ImmutableArray<SyntaxReference> SyntaxReferences => _syntaxRefs;

	public override int Ordinal => _ordinal;

	public override VarianceKind Variance => VarianceKind.None;

	public override string Name => _name;

	internal ImmutableArray<SyntaxList<AttributeListSyntax>> MergedAttributeDeclarationSyntaxLists
	{
		get
		{
			ArrayBuilder<SyntaxList<AttributeListSyntax>> instance = ArrayBuilder<SyntaxList<AttributeListSyntax>>.GetInstance();
			foreach (SyntaxReference syntaxRef in _syntaxRefs)
			{
				TypeParameterSyntax typeParameterSyntax = (TypeParameterSyntax)syntaxRef.GetSyntax();
				instance.Add(typeParameterSyntax.AttributeLists);
			}
			if (ContainingSymbol is SourceOrdinaryMethodSymbol { IsPartial: not false, SourcePartialImplementation: { TypeParameters: var typeParameters } })
			{
				SourceTypeParameterSymbolBase sourceTypeParameterSymbolBase = (SourceTypeParameterSymbolBase)typeParameters[_ordinal];
				instance.AddRange(sourceTypeParameterSymbolBase.MergedAttributeDeclarationSyntaxLists);
			}
			return instance.ToImmutableAndFree();
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.TypeParameter;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AttributeLocation.TypeParameter;

	protected abstract ImmutableArray<TypeParameterSymbol> ContainerTypeParameters { get; }

	protected SourceTypeParameterSymbolBase(string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
	{
		_name = name;
		_ordinal = (short)ordinal;
		_locations = locations;
		_syntaxRefs = syntaxRefs;
	}

	internal override ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.ConstraintTypes ?? ImmutableArray<TypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return GetBounds(inProgress)?.Interfaces ?? ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.EffectiveBaseClass;
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		TypeParameterBounds bounds = GetBounds(inProgress);
		if (bounds == null)
		{
			return GetDefaultBaseType();
		}
		return bounds.DeducedBaseType;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	internal virtual CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if (_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed)
		{
			bool flag;
			if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { SourcePartialDefinition: not null } sourceOrdinaryMethodSymbol))
			{
				flag = LoadAndValidateAttributes(OneOrMany.Create(MergedAttributeDeclarationSyntaxLists), ref _lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: false, (ContainingSymbol as LocalFunctionSymbol)?.SignatureBinder);
			}
			else
			{
				CustomAttributesBag<CSharpAttributeData> attributesBag = ((SourceTypeParameterSymbolBase)sourceOrdinaryMethodSymbol.SourcePartialDefinition.TypeParameters[_ordinal]).GetAttributesBag();
				flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, attributesBag, null) == null;
			}
			if (flag)
			{
				_state.NotePartComplete(CompletionPart.Attributes);
			}
		}
		return _lazyCustomAttributesBag;
	}

	internal override void EnsureAllConstraintsAreResolved()
	{
		if (_lazyBounds == TypeParameterBounds.Unset)
		{
			TypeParameterSymbol.EnsureAllConstraintsAreResolved(ContainerTypeParameters);
		}
	}

	private TypeParameterBounds GetBounds(ConsList<TypeParameterSymbol> inProgress)
	{
		if (_lazyBounds == TypeParameterBounds.Unset)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			TypeParameterBounds value = ResolveBounds(inProgress, instance);
			if (Interlocked.CompareExchange(ref _lazyBounds, value, TypeParameterBounds.Unset) == TypeParameterBounds.Unset)
			{
				CheckConstraintTypeConstraints(instance);
				CheckUnmanagedConstraint(instance);
				AddDeclarationDiagnostics(instance);
				_state.NotePartComplete(CompletionPart.Members);
			}
			instance.Free();
		}
		return _lazyBounds;
	}

	protected abstract TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, DiagnosticBag diagnostics);

	private void CheckConstraintTypeConstraints(DiagnosticBag diagnostics)
	{
		ImmutableArray<TypeSymbol> constraintTypesNoUseSiteDiagnostics = ConstraintTypesNoUseSiteDiagnostics;
		if (constraintTypesNoUseSiteDiagnostics.Length == 0)
		{
			return;
		}
		TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
		Location location = _locations[0];
		foreach (TypeSymbol item in constraintTypesNoUseSiteDiagnostics)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			item.AddUseSiteDiagnostics(ref useSiteDiagnostics);
			if (!diagnostics.Add(location, useSiteDiagnostics))
			{
				item.CheckAllConstraints(conversions, location, diagnostics);
			}
		}
	}

	private void CheckUnmanagedConstraint(DiagnosticBag diagnostics)
	{
		if (!HasUnmanagedTypeConstraint)
		{
			return;
		}
		Symbol containingSymbol = ContainingSymbol;
		if ((object)containingSymbol != null)
		{
			bool modifyCompilationForIsUnmanaged;
			if (!(containingSymbol is SourceOrdinaryMethodSymbol) && !(containingSymbol is SourceMemberContainerTypeSymbol))
			{
				if (!(containingSymbol is LocalFunctionSymbol))
				{
					goto IL_0034;
				}
				modifyCompilationForIsUnmanaged = false;
			}
			else
			{
				modifyCompilationForIsUnmanaged = true;
			}
			DeclaringCompilation.EnsureIsUnmanagedAttributeExists(diagnostics, this.GetNonNullSyntaxNode().Location, modifyCompilationForIsUnmanaged);
			return;
		}
		goto IL_0034;
		IL_0034:
		throw ExceptionUtilities.UnexpectedValue(ContainingSymbol);
	}

	private NamedTypeSymbol GetDefaultBaseType()
	{
		return ContainingAssembly.GetSpecialType(SpecialType.System_Object);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.Members:
				_ = ConstraintTypesNoUseSiteDiagnostics;
				break;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompleted);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (HasUnmanagedTypeConstraint)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsUnmanagedAttribute(this));
		}
	}
}
