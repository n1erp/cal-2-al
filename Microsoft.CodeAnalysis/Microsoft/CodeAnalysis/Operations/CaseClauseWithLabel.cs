namespace Microsoft.CodeAnalysis.Operations;

internal abstract class CaseClauseWithLabel : CaseClause
{
	public sealed override ILabelSymbol Label { get; }

	protected CaseClauseWithLabel(ILabelSymbol label, CaseKind caseKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(caseKind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Label = label;
	}
}
