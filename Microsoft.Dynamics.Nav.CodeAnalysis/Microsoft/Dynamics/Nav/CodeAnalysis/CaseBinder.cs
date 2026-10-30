using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class CaseBinder : Binder
{
	private readonly CaseStatementSyntax caseSyntax;

	private TypeSymbol caseType;

	public CaseBinder(Binder enclosing, CaseStatementSyntax syntax)
		: base(enclosing)
	{
		DebugAssertHelper.Assert(syntax != null);
		caseSyntax = syntax;
	}

	internal override BoundCaseStatement BindCaseParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		DebugAssertHelper.Assert(caseSyntax != null);
		bool hasErrors = false;
		BoundExpression boundExpression = BindExpression(caseSyntax.Expression, diagnostics);
		boundExpression = ApplyFromDotNetConversionIfNeeded(boundExpression);
		if (boundExpression.Type.IsArray())
		{
			Binder.Error(diagnostics, ErrorCode.ERR_ArrayNotValidForCase, caseSyntax.Expression);
			hasErrors = true;
		}
		if (boundExpression.Kind == BoundKind.EnumType && boundExpression.ConstantValue == null)
		{
			diagnostics.Add(ErrorCode.ERR_ExpectedOptionValue, boundExpression.Syntax.Location);
			hasErrors = true;
		}
		caseType = boundExpression.Type;
		ImmutableArray<BoundCaseLine> caseLines = ImmutableArray<BoundCaseLine>.Empty;
		if (!caseSyntax.CaseLines.IsEmpty())
		{
			int count = caseSyntax.CaseLines.Count;
			ArrayBuilder<BoundCaseLine> instance = ArrayBuilder<BoundCaseLine>.GetInstance(count);
			for (int i = 0; i < count; i++)
			{
				instance.Add(BindCaseLine(caseSyntax.CaseLines[i], diagnostics));
			}
			caseLines = instance.ToImmutableAndFree();
		}
		BoundStatementList caseElse = null;
		if (caseSyntax.CaseElse != null)
		{
			caseElse = BindStatementList(caseSyntax.CaseElse.ElseStatements, diagnostics);
		}
		return new BoundCaseStatement(caseSyntax, boundExpression, caseLines, caseElse, hasErrors);
	}

	private BoundCaseLine BindCaseLine(CaseLineSyntax caseLine, DiagnosticBag diagnostics)
	{
		bool hasErrors = false;
		int count = caseLine.Expressions.Count;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(count);
		for (int i = 0; i < count; i++)
		{
			BoundExpression expr = BindValue(caseLine.Expressions[i], diagnostics, BindValueKind.RValue);
			expr = ApplyCaseConversion(expr, diagnostics);
			instance.Add(expr);
		}
		ImmutableArray<BoundExpression> expressions = instance.ToImmutableAndFree();
		return new BoundCaseLine(caseLine, expressions, BindStatement(caseLine.Statement, diagnostics), hasErrors);
	}

	private BoundExpression ApplyCaseConversion(BoundExpression expr, DiagnosticBag diagnostics)
	{
		if (expr.Type != caseType)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			TypeSymbol type = expr.Type;
			TypeSymbol destination = caseType;
			Conversion conversion = Conversions.ClassifyConversionFromExpression(expr, type, destination, ref useSiteDiagnostics);
			diagnostics.Add(expr.Syntax, useSiteDiagnostics);
			if (!conversion.IsImplicit || !conversion.IsValid)
			{
				GenerateImplicitConversionError(diagnostics, expr.Syntax, conversion, expr, caseType);
				expr = new BoundConversion(expr.Syntax, expr, conversion, null, caseType, null, hasErrors: true);
			}
			else
			{
				expr = CreateConversion(expr.Syntax, expr, conversion, isCast: false, caseType, null, diagnostics);
			}
		}
		return expr;
	}
}
