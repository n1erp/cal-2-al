namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TupleVirtualElementFieldSymbol : TupleElementFieldSymbol
{
	private readonly string _name;

	private readonly bool _cannotUse;

	public override string Name => _name;

	internal override int? TypeLayoutOffset => null;

	public override Symbol AssociatedSymbol => null;

	public override bool IsVirtualTupleField => true;

	public TupleVirtualElementFieldSymbol(TupleTypeSymbol container, FieldSymbol underlyingField, string name, int tupleElementIndex, Location location, bool cannotUse, bool isImplicitlyDeclared, TupleElementFieldSymbol correspondingDefaultFieldOpt)
		: base(container, underlyingField, tupleElementIndex, location, isImplicitlyDeclared, correspondingDefaultFieldOpt)
	{
		_name = name;
		_cannotUse = cannotUse;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_cannotUse)
		{
			return new CSDiagnosticInfo(ErrorCode.ERR_TupleInferredNamesNotAvailable, _name, new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureInferredTupleNames.RequiredVersion()));
		}
		return base.GetUseSiteDiagnostic();
	}
}
