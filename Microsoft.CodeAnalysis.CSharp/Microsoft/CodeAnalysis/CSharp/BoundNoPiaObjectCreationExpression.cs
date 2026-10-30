using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNoPiaObjectCreationExpression : BoundExpression
{
	public string GuidString { get; }

	public BoundObjectInitializerExpressionBase InitializerExpressionOpt { get; }

	public BoundNoPiaObjectCreationExpression(SyntaxNode syntax, string guidString, BoundObjectInitializerExpressionBase initializerExpressionOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NoPiaObjectCreationExpression, syntax, type, hasErrors || initializerExpressionOpt.HasErrors())
	{
		GuidString = guidString;
		InitializerExpressionOpt = initializerExpressionOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNoPiaObjectCreationExpression(this);
	}

	public BoundNoPiaObjectCreationExpression Update(string guidString, BoundObjectInitializerExpressionBase initializerExpressionOpt, TypeSymbol type)
	{
		if (guidString != GuidString || initializerExpressionOpt != InitializerExpressionOpt || type != Type)
		{
			return new BoundNoPiaObjectCreationExpression(Syntax, guidString, initializerExpressionOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
