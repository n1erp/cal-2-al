using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UsingStatementBinder : LockOrUsingBinder
{
	private readonly UsingStatementSyntax _syntax;

	protected override ExpressionSyntax TargetExpressionSyntax => _syntax.Expression;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public UsingStatementBinder(Binder enclosing, UsingStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
		VariableDeclarationSyntax declaration = _syntax.Declaration;
		if (targetExpressionSyntax != null)
		{
			ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
			ExpressionVariableFinder.FindExpressionVariables(this, instance, targetExpressionSyntax);
			return instance.ToImmutableAndFree();
		}
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance(declaration.Variables.Count);
		foreach (VariableDeclaratorSyntax variable in declaration.Variables)
		{
			instance2.Add(MakeLocal(declaration, variable, LocalDeclarationKind.UsingVariable));
			ExpressionVariableFinder.FindExpressionVariables(this, instance2, variable);
		}
		return instance2.ToImmutableAndFree();
	}

	internal override BoundStatement BindUsingStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
		VariableDeclarationSyntax declaration = _syntax.Declaration;
		bool hasErrors = false;
		BoundMultipleLocalDeclarations declarationsOpt = null;
		BoundExpression boundExpression = null;
		_ = Conversion.NoConversion;
		TypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_IDisposable);
		Conversion iDisposableConversion;
		if (targetExpressionSyntax != null)
		{
			boundExpression = BindTargetExpression(diagnostics, originalBinder);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			iDisposableConversion = originalBinder.Conversions.ClassifyImplicitConversionFromExpression(boundExpression, specialType, ref useSiteDiagnostics);
			diagnostics.Add(targetExpressionSyntax, useSiteDiagnostics);
			if (!iDisposableConversion.IsImplicit)
			{
				TypeSymbol type = boundExpression.Type;
				if ((object)type == null || !type.IsErrorType())
				{
					Binder.Error(diagnostics, ErrorCode.ERR_NoConvToIDisp, targetExpressionSyntax, boundExpression.Display);
				}
				hasErrors = true;
			}
		}
		else
		{
			originalBinder.BindForOrUsingOrFixedDeclarations(declaration, LocalDeclarationKind.UsingVariable, diagnostics, out var declarations);
			declarationsOpt = new BoundMultipleLocalDeclarations(declaration, declarations);
			TypeSymbol type2 = declarations[0].DeclaredType.Type;
			if (type2.IsDynamic())
			{
				iDisposableConversion = Conversion.ImplicitDynamic;
			}
			else
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics2 = null;
				iDisposableConversion = originalBinder.Conversions.ClassifyImplicitConversionFromType(type2, specialType, ref useSiteDiagnostics2);
				diagnostics.Add(declaration, useSiteDiagnostics2);
				if (!iDisposableConversion.IsImplicit)
				{
					if (!type2.IsErrorType())
					{
						Binder.Error(diagnostics, ErrorCode.ERR_NoConvToIDisp, declaration, type2);
					}
					hasErrors = true;
				}
			}
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(_syntax.Statement, diagnostics);
		return new BoundUsingStatement(_syntax, Locals, declarationsOpt, boundExpression, iDisposableConversion, body, hasErrors);
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
