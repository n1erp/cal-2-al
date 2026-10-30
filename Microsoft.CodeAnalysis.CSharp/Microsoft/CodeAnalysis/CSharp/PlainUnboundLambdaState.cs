using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class PlainUnboundLambdaState : UnboundLambdaState
{
	private readonly ImmutableArray<string> _parameterNames;

	private readonly ImmutableArray<TypeSymbol> _parameterTypes;

	private readonly ImmutableArray<RefKind> _parameterRefKinds;

	private readonly bool _isAsync;

	public override bool HasSignature => !_parameterNames.IsDefault;

	public override bool HasExplicitlyTypedParameterList => !_parameterTypes.IsDefault;

	public override int ParameterCount
	{
		get
		{
			if (!_parameterNames.IsDefault)
			{
				return _parameterNames.Length;
			}
			return 0;
		}
	}

	public override bool IsAsync => _isAsync;

	public override MessageID MessageID
	{
		get
		{
			if (UnboundLambda.Syntax.Kind() != SyntaxKind.AnonymousMethodExpression)
			{
				return MessageID.IDS_Lambda;
			}
			return MessageID.IDS_AnonMethod;
		}
	}

	private CSharpSyntaxNode Body => UnboundLambda.Syntax.AnonymousFunctionBody();

	private bool IsExpressionLambda => Body.Kind() != SyntaxKind.Block;

	internal PlainUnboundLambdaState(UnboundLambda unboundLambda, Binder binder, ImmutableArray<string> parameterNames, ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, bool isAsync)
		: base(binder, unboundLambda)
	{
		_parameterNames = parameterNames;
		_parameterTypes = parameterTypes;
		_parameterRefKinds = parameterRefKinds;
		_isAsync = isAsync;
	}

	public override Location ParameterLocation(int index)
	{
		SyntaxNode syntax = UnboundLambda.Syntax;
		return syntax.Kind() switch
		{
			SyntaxKind.ParenthesizedLambdaExpression => ((ParenthesizedLambdaExpressionSyntax)syntax).ParameterList.Parameters[index].Identifier.GetLocation(), 
			SyntaxKind.AnonymousMethodExpression => ((AnonymousMethodExpressionSyntax)syntax).ParameterList.Parameters[index].Identifier.GetLocation(), 
			_ => ((SimpleLambdaExpressionSyntax)syntax).Parameter.Identifier.GetLocation(), 
		};
	}

	public override string ParameterName(int index)
	{
		return _parameterNames[index];
	}

	public override RefKind RefKind(int index)
	{
		if (!_parameterRefKinds.IsDefault)
		{
			return _parameterRefKinds[index];
		}
		return Microsoft.CodeAnalysis.RefKind.None;
	}

	public override TypeSymbol ParameterType(int index)
	{
		return _parameterTypes[index];
	}

	protected override BoundBlock BindLambdaBody(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics)
	{
		if (IsExpressionLambda)
		{
			return lambdaBodyBinder.BindLambdaExpressionAsBlock((ExpressionSyntax)Body, diagnostics);
		}
		return lambdaBodyBinder.BindEmbeddedBlock((BlockSyntax)Body, diagnostics);
	}
}
