namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedExplicitImplementationForwardingMethod : SynthesizedImplementationMethod
{
	private readonly MethodSymbol _implementingMethod;

	internal override bool SynthesizesLoweredBoundBody => true;

	public MethodSymbol ImplementingMethod => _implementingMethod;

	public override MethodKind MethodKind
	{
		get
		{
			if (!_implementingMethod.IsAccessor())
			{
				return MethodKind.ExplicitInterfaceImplementation;
			}
			return _implementingMethod.MethodKind;
		}
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = OriginalDefinition;
		try
		{
			MethodSymbol methodToInvoke = (IsGenericMethod ? ImplementingMethod.Construct(TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>()) : ImplementingMethod);
			syntheticBoundNodeFactory.CloseMethod(MethodBodySynthesizer.ConstructSingleInvocationMethodBody(syntheticBoundNodeFactory, methodToInvoke, useBaseReference: false));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}

	public SynthesizedExplicitImplementationForwardingMethod(MethodSymbol interfaceMethod, MethodSymbol implementingMethod, NamedTypeSymbol implementingType)
		: base(interfaceMethod, implementingType, null, generateDebugInfo: false)
	{
		_implementingMethod = implementingMethod;
	}
}
