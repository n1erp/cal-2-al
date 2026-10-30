using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTypeExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => (Symbol)(((object)AliasOpt) ?? ((object)Type));

	public override LookupResultKind ResultKind
	{
		get
		{
			if (Type.OriginalDefinition is ErrorTypeSymbol errorTypeSymbol)
			{
				return errorTypeSymbol.ResultKind;
			}
			return LookupResultKind.Viable;
		}
	}

	public AliasSymbol AliasOpt { get; }

	public bool InferredType { get; }

	public BoundTypeExpression BoundContainingTypeOpt { get; }

	public BoundTypeExpression(SyntaxNode syntax, AliasSymbol aliasOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, aliasOpt, inferredType: false, null, type, hasErrors)
	{
	}

	public BoundTypeExpression(SyntaxNode syntax, AliasSymbol aliasOpt, TypeSymbol type)
		: this(syntax, aliasOpt, inferredType: false, null, type)
	{
	}

	public BoundTypeExpression(SyntaxNode syntax, AliasSymbol aliasOpt, bool inferredType, TypeSymbol type, bool hasErrors = false)
		: this(syntax, aliasOpt, inferredType, null, type, hasErrors)
	{
	}

	public BoundTypeExpression(SyntaxNode syntax, AliasSymbol aliasOpt, bool inferredType, BoundTypeExpression boundContainingTypeOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TypeExpression, syntax, type, hasErrors || boundContainingTypeOpt.HasErrors())
	{
		AliasOpt = aliasOpt;
		InferredType = inferredType;
		BoundContainingTypeOpt = boundContainingTypeOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTypeExpression(this);
	}

	public BoundTypeExpression Update(AliasSymbol aliasOpt, bool inferredType, BoundTypeExpression boundContainingTypeOpt, TypeSymbol type)
	{
		if (aliasOpt != AliasOpt || inferredType != InferredType || boundContainingTypeOpt != BoundContainingTypeOpt || type != Type)
		{
			return new BoundTypeExpression(Syntax, aliasOpt, inferredType, boundContainingTypeOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
