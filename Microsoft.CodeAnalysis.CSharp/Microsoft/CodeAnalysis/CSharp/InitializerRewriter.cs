using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class InitializerRewriter
{
	internal static BoundTypeOrInstanceInitializers RewriteConstructor(ImmutableArray<BoundInitializer> boundInitializers, MethodSymbol method)
	{
		return new BoundTypeOrInstanceInitializers((method is SourceMemberMethodSymbol sourceMemberMethodSymbol) ? sourceMemberMethodSymbol.SyntaxNode : method.GetNonNullSyntaxNode(), boundInitializers.SelectAsArray(RewriteInitializersAsStatements));
	}

	internal static BoundTypeOrInstanceInitializers RewriteScriptInitializer(ImmutableArray<BoundInitializer> boundInitializers, SynthesizedInteractiveInitializerMethod method, out bool hasTrailingExpression)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(boundInitializers.Length);
		bool flag = (object)method.ResultType != null;
		BoundStatement boundStatement = null;
		BoundExpression boundExpression = null;
		foreach (BoundInitializer item in boundInitializers)
		{
			if (flag && item == boundInitializers.Last() && item.Kind == BoundKind.GlobalStatementInitializer && method.DeclaringCompilation.IsSubmissionSyntaxTree(item.SyntaxTree))
			{
				boundStatement = ((BoundGlobalStatementInitializer)item).Statement;
				BoundExpression trailingScriptExpression = GetTrailingScriptExpression(boundStatement);
				if (trailingScriptExpression != null && (object)trailingScriptExpression.Type != null && trailingScriptExpression.Type.SpecialType != SpecialType.System_Void)
				{
					boundExpression = trailingScriptExpression;
					continue;
				}
			}
			instance.Add(RewriteInitializersAsStatements(item));
		}
		if (flag && boundExpression != null)
		{
			instance.Add(new BoundReturnStatement(boundStatement.Syntax, RefKind.None, boundExpression));
			hasTrailingExpression = true;
		}
		else
		{
			hasTrailingExpression = false;
		}
		return new BoundTypeOrInstanceInitializers(method.GetNonNullSyntaxNode(), instance.ToImmutableAndFree());
	}

	internal static BoundExpression GetTrailingScriptExpression(BoundStatement statement)
	{
		if (statement.Kind != BoundKind.ExpressionStatement || !((ExpressionStatementSyntax)statement.Syntax).SemicolonToken.IsMissing)
		{
			return null;
		}
		return ((BoundExpressionStatement)statement).Expression;
	}

	private static BoundStatement RewriteFieldInitializer(BoundFieldEqualsValue fieldInit)
	{
		SyntaxNode syntax = fieldInit.Syntax;
		syntax = (syntax as EqualsValueClauseSyntax)?.Value ?? syntax;
		BoundThisReference receiver = (fieldInit.Field.IsStatic ? null : new BoundThisReference(syntax, fieldInit.Field.ContainingType));
		BoundStatement boundStatement = new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, new BoundFieldAccess(syntax, receiver, fieldInit.Field, null), fieldInit.Value, fieldInit.Field.Type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = (!fieldInit.Locals.IsEmpty || fieldInit.WasCompilerGenerated)
		};
		if (!fieldInit.Locals.IsEmpty)
		{
			boundStatement = new BoundBlock(syntax, fieldInit.Locals, ImmutableArray.Create(boundStatement))
			{
				WasCompilerGenerated = fieldInit.WasCompilerGenerated
			};
		}
		return boundStatement;
	}

	private static BoundStatement RewriteInitializersAsStatements(BoundInitializer initializer)
	{
		return initializer.Kind switch
		{
			BoundKind.FieldEqualsValue => RewriteFieldInitializer((BoundFieldEqualsValue)initializer), 
			BoundKind.GlobalStatementInitializer => ((BoundGlobalStatementInitializer)initializer).Statement, 
			_ => throw ExceptionUtilities.UnexpectedValue(initializer.Kind), 
		};
	}
}
