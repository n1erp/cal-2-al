using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LockBinder : LockOrUsingBinder
{
	private readonly LockStatementSyntax _syntax;

	protected override ExpressionSyntax TargetExpressionSyntax => _syntax.Expression;

	public LockBinder(Binder enclosing, LockStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	internal override BoundStatement BindLockStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
		BoundExpression boundExpression = BindTargetExpression(diagnostics, originalBinder);
		TypeSymbol type = boundExpression.Type;
		bool hasErrors = false;
		if ((object)type == null)
		{
			if (boundExpression.ConstantValue != ConstantValue.Null || Compilation.FeatureStrictEnabled)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_LockNeedsReference, targetExpressionSyntax, boundExpression.Display);
				hasErrors = true;
			}
		}
		else if (!type.IsReferenceType && (type.IsValueType || Compilation.FeatureStrictEnabled))
		{
			Binder.Error(diagnostics, ErrorCode.ERR_LockNeedsReference, targetExpressionSyntax, type);
			hasErrors = true;
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(_syntax.Statement, diagnostics);
		return new BoundLockStatement(_syntax, boundExpression, body, hasErrors);
	}
}
