using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLambda : BoundExpression, IBoundLambdaOrFunction
{
	internal sealed class BlockReturns : BoundTreeWalker
	{
		private readonly ArrayBuilder<TypeSymbol> _types;

		private bool _hasReturnWithoutArgument;

		private RefKind refKind;

		private BlockReturns()
		{
			_types = ArrayBuilder<TypeSymbol>.GetInstance();
		}

		public static ImmutableArray<TypeSymbol> GetReturnTypes(BoundBlock block, out RefKind refKind, out int numberOfDistinctReturns)
		{
			BlockReturns blockReturns = new BlockReturns();
			blockReturns.Visit(block);
			refKind = blockReturns.refKind;
			ImmutableArray<TypeSymbol> result = blockReturns._types.ToImmutableAndFree();
			numberOfDistinctReturns = result.Length;
			if (blockReturns._hasReturnWithoutArgument)
			{
				numberOfDistinctReturns++;
			}
			return result;
		}

		public override BoundNode Visit(BoundNode node)
		{
			if (!(node is BoundExpression))
			{
				return base.Visit(node);
			}
			return null;
		}

		protected override BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			return null;
		}

		public override BoundNode VisitReturnStatement(BoundReturnStatement node)
		{
			if (node.RefKind != RefKind.None)
			{
				refKind = node.RefKind;
			}
			BoundExpression expressionOpt = node.ExpressionOpt;
			if (expressionOpt != null)
			{
				TypeSymbol type = expressionOpt.Type;
				if (!_types.Contains(type))
				{
					_types.Add(type);
				}
			}
			else
			{
				_hasReturnWithoutArgument = true;
			}
			return null;
		}
	}

	private readonly bool _inferredFromSingleType;

	private readonly RefKind _refKind;

	private readonly TypeSymbol _inferredReturnType;

	private readonly HashSet<DiagnosticInfo> _inferredReturnTypeUseSiteDiagnostics;

	public override Symbol ExpressionSymbol => Symbol;

	public override object Display => MessageID.Localize();

	public MessageID MessageID
	{
		get
		{
			if (Syntax.Kind() != SyntaxKind.AnonymousMethodExpression)
			{
				return MessageID.IDS_Lambda;
			}
			return MessageID.IDS_AnonMethod;
		}
	}

	public bool InferredFromSingleType => _inferredFromSingleType;

	public RefKind RefKind => _refKind;

	MethodSymbol IBoundLambdaOrFunction.Symbol => Symbol;

	SyntaxNode IBoundLambdaOrFunction.Syntax => Syntax;

	public LambdaSymbol Symbol { get; }

	public BoundBlock Body { get; }

	public ImmutableArray<Diagnostic> Diagnostics { get; }

	public Binder Binder { get; }

	public BoundLambda(SyntaxNode syntax, BoundBlock body, ImmutableArray<Diagnostic> diagnostics, Binder binder, TypeSymbol delegateType, bool inferReturnType)
		: this(syntax, (LambdaSymbol)binder.ContainingMemberOrLambda, body, diagnostics, binder, delegateType)
	{
		if (inferReturnType)
		{
			_inferredReturnType = InferReturnType(Body, Binder, delegateType, Symbol.IsAsync, ref _inferredReturnTypeUseSiteDiagnostics, out _refKind, out _inferredFromSingleType);
		}
	}

	public TypeSymbol InferredReturnType(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!_inferredReturnTypeUseSiteDiagnostics.IsNullOrEmpty())
		{
			if (useSiteDiagnostics == null)
			{
				useSiteDiagnostics = new HashSet<DiagnosticInfo>();
			}
			foreach (DiagnosticInfo inferredReturnTypeUseSiteDiagnostic in _inferredReturnTypeUseSiteDiagnostics)
			{
				useSiteDiagnostics.Add(inferredReturnTypeUseSiteDiagnostic);
			}
		}
		return _inferredReturnType;
	}

	private static TypeSymbol InferReturnType(BoundBlock block, Binder binder, TypeSymbol delegateType, bool isAsync, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out RefKind refKind, out bool inferredFromSingleType)
	{
		ImmutableArray<TypeSymbol> returnTypes = BlockReturns.GetReturnTypes(block, out refKind, out var numberOfDistinctReturns);
		inferredFromSingleType = numberOfDistinctReturns < 2;
		TypeSymbol typeSymbol;
		if (returnTypes.IsDefaultOrEmpty)
		{
			typeSymbol = null;
		}
		else
		{
			typeSymbol = ((returnTypes.Length != 1) ? BestTypeInferrer.InferBestType(returnTypes, binder.Conversions, ref useSiteDiagnostics) : returnTypes[0]);
		}
		if (!isAsync)
		{
			return typeSymbol;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		if (delegateType?.GetDelegateType()?.DelegateInvokeMethod?.ReturnType is NamedTypeSymbol { SpecialType: not SpecialType.System_Void } namedTypeSymbol2 && namedTypeSymbol2.IsCustomTaskType(out var _))
		{
			namedTypeSymbol = namedTypeSymbol2.ConstructedFrom;
		}
		if (returnTypes.IsEmpty)
		{
			if ((object)namedTypeSymbol == null || namedTypeSymbol.Arity != 0)
			{
				return binder.Compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task);
			}
			return namedTypeSymbol;
		}
		if ((object)typeSymbol == null || typeSymbol.SpecialType == SpecialType.System_Void)
		{
			return null;
		}
		return (((object)namedTypeSymbol != null && namedTypeSymbol.Arity == 1) ? namedTypeSymbol : binder.Compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T)).Construct(typeSymbol);
	}

	public BoundLambda(SyntaxNode syntax, LambdaSymbol symbol, BoundBlock body, ImmutableArray<Diagnostic> diagnostics, Binder binder, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Lambda, syntax, type, hasErrors || body.HasErrors())
	{
		Symbol = symbol;
		Body = body;
		Diagnostics = diagnostics;
		Binder = binder;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLambda(this);
	}

	public BoundLambda Update(LambdaSymbol symbol, BoundBlock body, ImmutableArray<Diagnostic> diagnostics, Binder binder, TypeSymbol type)
	{
		if (symbol != Symbol || body != Body || diagnostics != Diagnostics || binder != Binder || type != Type)
		{
			return new BoundLambda(Syntax, symbol, body, diagnostics, binder, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
