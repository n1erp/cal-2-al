using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SubstitutedErrorTypeSymbol : ErrorTypeSymbol
{
	private readonly ErrorTypeSymbol _originalDefinition;

	private int _hashCode;

	public override NamedTypeSymbol OriginalDefinition => _originalDefinition;

	internal override bool MangleName => _originalDefinition.MangleName;

	internal override DiagnosticInfo ErrorInfo => _originalDefinition.ErrorInfo;

	public override int Arity => _originalDefinition.Arity;

	public override string Name => _originalDefinition.Name;

	public override ImmutableArray<Location> Locations => _originalDefinition.Locations;

	public override ImmutableArray<Symbol> CandidateSymbols => _originalDefinition.CandidateSymbols;

	internal override LookupResultKind ResultKind => _originalDefinition.ResultKind;

	protected SubstitutedErrorTypeSymbol(ErrorTypeSymbol originalDefinition)
	{
		_originalDefinition = originalDefinition;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		return _originalDefinition.GetUseSiteDiagnostic();
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = this.ComputeHashCode();
		}
		return _hashCode;
	}
}
