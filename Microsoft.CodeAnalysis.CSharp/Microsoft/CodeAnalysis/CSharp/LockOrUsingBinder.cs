using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class LockOrUsingBinder : LocalScopeBinder
{
	private class ExpressionAndDiagnostics
	{
		public readonly BoundExpression Expression;

		public readonly ImmutableArray<Diagnostic> Diagnostics;

		public ExpressionAndDiagnostics(BoundExpression expression, ImmutableArray<Diagnostic> diagnostics)
		{
			Expression = expression;
			Diagnostics = diagnostics;
		}
	}

	private ImmutableHashSet<Symbol> _lazyLockedOrDisposedVariables;

	private ExpressionAndDiagnostics _lazyExpressionAndDiagnostics;

	protected abstract ExpressionSyntax TargetExpressionSyntax { get; }

	internal sealed override ImmutableHashSet<Symbol> LockedOrDisposedVariables
	{
		get
		{
			if (_lazyLockedOrDisposedVariables == null)
			{
				ImmutableHashSet<Symbol> immutableHashSet = Next.LockedOrDisposedVariables;
				ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
				if (targetExpressionSyntax != null && targetExpressionSyntax.Kind() == SyntaxKind.IdentifierName)
				{
					BoundExpression boundExpression = BindTargetExpression(null, GetBinder(targetExpressionSyntax.Parent));
					switch (boundExpression.Kind)
					{
					case BoundKind.Local:
						immutableHashSet = immutableHashSet.Add(((BoundLocal)boundExpression).LocalSymbol);
						break;
					case BoundKind.Parameter:
						immutableHashSet = immutableHashSet.Add(((BoundParameter)boundExpression).ParameterSymbol);
						break;
					}
				}
				Interlocked.CompareExchange(ref _lazyLockedOrDisposedVariables, immutableHashSet, null);
			}
			return _lazyLockedOrDisposedVariables;
		}
	}

	internal LockOrUsingBinder(Binder enclosing)
		: base(enclosing)
	{
	}

	protected BoundExpression BindTargetExpression(DiagnosticBag diagnostics, Binder originalBinder)
	{
		if (_lazyExpressionAndDiagnostics == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression expression = originalBinder.BindValue(TargetExpressionSyntax, instance, BindValueKind.RValueOrMethodGroup);
			Interlocked.CompareExchange(ref _lazyExpressionAndDiagnostics, new ExpressionAndDiagnostics(expression, instance.ToReadOnlyAndFree()), null);
		}
		diagnostics?.AddRange(_lazyExpressionAndDiagnostics.Diagnostics);
		return _lazyExpressionAndDiagnostics.Expression;
	}
}
