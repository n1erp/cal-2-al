using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInstrumentationPayloadRoot : BoundExpression
{
	public int AnalysisKind { get; }

	public BoundInstrumentationPayloadRoot(SyntaxNode syntax, int analysisKind, TypeSymbol type, bool hasErrors)
		: base(BoundKind.InstrumentationPayloadRoot, syntax, type, hasErrors)
	{
		AnalysisKind = analysisKind;
	}

	public BoundInstrumentationPayloadRoot(SyntaxNode syntax, int analysisKind, TypeSymbol type)
		: base(BoundKind.InstrumentationPayloadRoot, syntax, type)
	{
		AnalysisKind = analysisKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInstrumentationPayloadRoot(this);
	}

	public BoundInstrumentationPayloadRoot Update(int analysisKind, TypeSymbol type)
	{
		if (analysisKind != AnalysisKind || type != Type)
		{
			return new BoundInstrumentationPayloadRoot(Syntax, analysisKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
