using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UnboundLambda : BoundExpression
{
	public override object Display => MessageID.Localize();

	public MessageID MessageID => Data.MessageID;

	public bool HasSignature => Data.HasSignature;

	public bool HasExplicitlyTypedParameterList => Data.HasExplicitlyTypedParameterList;

	public int ParameterCount => Data.ParameterCount;

	public bool IsAsync => Data.IsAsync;

	public UnboundLambdaState Data { get; }

	public UnboundLambda(CSharpSyntaxNode syntax, Binder binder, ImmutableArray<RefKind> refKinds, ImmutableArray<TypeSymbol> types, ImmutableArray<string> names, bool isAsync, bool hasErrors = false)
		: base(BoundKind.UnboundLambda, syntax, null, hasErrors || (!types.IsDefault && types.Any(SymbolKind.ErrorType)))
	{
		Data = new PlainUnboundLambdaState(this, binder, names, types, refKinds, isAsync);
	}

	public BoundLambda Bind(NamedTypeSymbol delegateType)
	{
		return Data.Bind(delegateType);
	}

	public BoundLambda BindForErrorRecovery()
	{
		return Data.BindForErrorRecovery();
	}

	public BoundLambda BindForReturnTypeInference(NamedTypeSymbol delegateType)
	{
		return Data.BindForReturnTypeInference(delegateType);
	}

	public TypeSymbol InferReturnType(NamedTypeSymbol delegateType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return Data.InferReturnType(delegateType, ref useSiteDiagnostics);
	}

	public RefKind RefKind(int index)
	{
		return Data.RefKind(index);
	}

	public void GenerateAnonymousFunctionConversionError(DiagnosticBag diagnostics, TypeSymbol targetType)
	{
		Data.GenerateAnonymousFunctionConversionError(diagnostics, targetType);
	}

	public bool GenerateSummaryErrors(DiagnosticBag diagnostics)
	{
		return Data.GenerateSummaryErrors(diagnostics);
	}

	public TypeSymbol ParameterType(int index)
	{
		return Data.ParameterType(index);
	}

	public Location ParameterLocation(int index)
	{
		return Data.ParameterLocation(index);
	}

	public string ParameterName(int index)
	{
		return Data.ParameterName(index);
	}

	public UnboundLambda(SyntaxNode syntax, UnboundLambdaState data, bool hasErrors)
		: base(BoundKind.UnboundLambda, syntax, null, hasErrors)
	{
		Data = data;
	}

	public UnboundLambda(SyntaxNode syntax, UnboundLambdaState data)
		: base(BoundKind.UnboundLambda, syntax, null)
	{
		Data = data;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnboundLambda(this);
	}

	public UnboundLambda Update(UnboundLambdaState data)
	{
		if (data != Data)
		{
			return new UnboundLambda(Syntax, data, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
