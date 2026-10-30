using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedMethodBaseSymbol : SourceMemberMethodSymbol
{
	protected readonly MethodSymbol BaseMethod;

	private readonly string _name;

	private ImmutableArray<TypeParameterSymbol> _typeParameters;

	private ImmutableArray<ParameterSymbol> _parameters;

	private TypeSymbol _iteratorElementType;

	internal TypeMap TypeMap { get; private set; }

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	internal override int ParameterCount => Parameters.Length;

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_parameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _parameters, MakeParameters(), default);
			}
			return _parameters;
		}
	}

	protected virtual ImmutableArray<TypeSymbol> ExtraSynthesizedRefParameters => default;

	protected virtual ImmutableArray<ParameterSymbol> BaseMethodParameters => BaseMethod.Parameters;

	public sealed override RefKind RefKind => BaseMethod.RefKind;

	public sealed override TypeSymbol ReturnType => TypeMap.SubstituteType(BaseMethod.OriginalDefinition.ReturnType).Type;

	public sealed override bool IsVararg => BaseMethod.IsVararg;

	public sealed override string Name => _name;

	public sealed override bool IsImplicitlyDeclared => true;

	internal override bool IsExpressionBodied => false;

	internal override TypeSymbol IteratorElementType
	{
		get
		{
			if (_iteratorElementType == null)
			{
				_iteratorElementType = TypeMap.SubstituteType(BaseMethod.IteratorElementType).Type;
			}
			return _iteratorElementType;
		}
		set
		{
			_iteratorElementType = value;
		}
	}

	protected SynthesizedMethodBaseSymbol(NamedTypeSymbol containingType, MethodSymbol baseMethod, SyntaxReference syntaxReference, Location location, string name, DeclarationModifiers declarationModifiers)
		: base(containingType, syntaxReference, location)
	{
		BaseMethod = baseMethod;
		_name = name;
		MakeFlags(MethodKind.Ordinary, declarationModifiers, baseMethod.ReturnsVoid, isExtensionMethod: false);
	}

	protected void AssignTypeMapAndTypeParameters(TypeMap typeMap, ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		TypeMap = typeMap;
		_typeParameters = typeParameters;
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (!ContainingType.IsImplicitlyDeclared)
		{
			CSharpCompilation declaringCompilation = DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
	}

	private ImmutableArray<ParameterSymbol> MakeParameters()
	{
		int num = 0;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		foreach (ParameterSymbol baseMethodParameter in BaseMethodParameters)
		{
			instance.Add(SynthesizedParameterSymbol.Create(this, TypeMap.SubstituteType(baseMethodParameter.OriginalDefinition.Type).Type, num++, baseMethodParameter.RefKind, baseMethodParameter.Name));
		}
		ImmutableArray<TypeSymbol> extraSynthesizedRefParameters = ExtraSynthesizedRefParameters;
		if (!extraSynthesizedRefParameters.IsDefaultOrEmpty)
		{
			foreach (TypeSymbol item in extraSynthesizedRefParameters)
			{
				instance.Add(SynthesizedParameterSymbol.Create(this, TypeMap.SubstituteType(item).Type, num++, RefKind.Ref));
			}
		}
		return instance.ToImmutableAndFree();
	}
}
