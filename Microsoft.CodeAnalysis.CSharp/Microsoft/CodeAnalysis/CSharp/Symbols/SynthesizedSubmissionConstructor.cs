using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedSubmissionConstructor : SynthesizedInstanceConstructor
{
	private readonly ImmutableArray<ParameterSymbol> _parameters;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	internal SynthesizedSubmissionConstructor(NamedTypeSymbol containingType, DiagnosticBag diagnostics)
		: base(containingType)
	{
		CSharpCompilation declaringCompilation = containingType.DeclaringCompilation;
		ArrayTypeSymbol arrayTypeSymbol = declaringCompilation.CreateArrayTypeSymbol(declaringCompilation.GetSpecialType(SpecialType.System_Object));
		DiagnosticInfo useSiteDiagnostic = arrayTypeSymbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			diagnostics.Add(useSiteDiagnostic, NoLocation.Singleton);
		}
		_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, arrayTypeSymbol, 0, RefKind.None, "submissionArray"));
	}
}
