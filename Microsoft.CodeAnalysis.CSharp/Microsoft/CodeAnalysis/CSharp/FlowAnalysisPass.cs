using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class FlowAnalysisPass
{
	public static BoundBlock Rewrite(MethodSymbol method, BoundBlock block, DiagnosticBag diagnostics, bool hasTrailingExpression, bool originalBodyNested)
	{
		CSharpCompilation declaringCompilation = method.DeclaringCompilation;
		if (method.ReturnsVoid || method.IsIterator || method.IsTaskReturningAsync(declaringCompilation))
		{
			if ((method.IsImplicitlyDeclared && !method.IsScriptInitializer) || Analyze(declaringCompilation, method, block, diagnostics))
			{
				block = AppendImplicitReturn(block, method, originalBodyNested);
			}
		}
		else if (Analyze(declaringCompilation, method, block, diagnostics))
		{
			TypeSymbol typeSymbol = (method as SynthesizedInteractiveInitializerMethod)?.ResultType;
			if (!hasTrailingExpression && (object)typeSymbol != null)
			{
				BoundDefaultExpression boundDefaultExpression = new BoundDefaultExpression(method.GetNonNullSyntaxNode(), typeSymbol);
				ImmutableArray<BoundStatement> statements = block.Statements.Add(new BoundReturnStatement(boundDefaultExpression.Syntax, RefKind.None, boundDefaultExpression));
				block = new BoundBlock(block.Syntax, ImmutableArray<LocalSymbol>.Empty, statements)
				{
					WasCompilerGenerated = true
				};
			}
			else if (method.Locations.Length == 1)
			{
				diagnostics.Add(ErrorCode.ERR_ReturnExpected, method.Locations[0], method);
			}
		}
		return block;
	}

	private static BoundBlock AppendImplicitReturn(BoundBlock body, MethodSymbol method, bool originalBodyNested)
	{
		if (originalBodyNested)
		{
			ImmutableArray<BoundStatement> statements = body.Statements;
			int length = statements.Length;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(length);
			instance.AddRange(statements, length - 1);
			instance.Add(AppendImplicitReturn((BoundBlock)statements[length - 1], method));
			return body.Update(body.Locals, ImmutableArray<LocalFunctionSymbol>.Empty, instance.ToImmutableAndFree());
		}
		return AppendImplicitReturn(body, method);
	}

	internal static BoundBlock AppendImplicitReturn(BoundBlock body, MethodSymbol method)
	{
		SyntaxNode syntax = body.Syntax;
		BoundStatement item = (method.IsIterator ? ((BoundStatement)BoundYieldBreakStatement.Synthesized(syntax)) : ((BoundStatement)BoundReturnStatement.Synthesized(syntax, RefKind.None, null)));
		return body.Update(body.Locals, body.LocalFunctions, body.Statements.Add(item));
	}

	private static bool Analyze(CSharpCompilation compilation, MethodSymbol method, BoundBlock block, DiagnosticBag diagnostics)
	{
		bool result = ControlFlowPass.Analyze(compilation, method, block, diagnostics);
		DataFlowPass.Analyze(compilation, method, block, diagnostics);
		return result;
	}
}
