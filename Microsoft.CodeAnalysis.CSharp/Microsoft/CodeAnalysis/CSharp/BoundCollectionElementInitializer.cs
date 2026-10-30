using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCollectionElementInitializer : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => AddMethod;

	public MethodSymbol AddMethod { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public BoundExpression ImplicitReceiverOpt { get; }

	public bool Expanded { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public bool InvokedAsExtensionMethod { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public Binder BinderOpt { get; }

	public BoundCollectionElementInitializer(SyntaxNode syntax, MethodSymbol addMethod, ImmutableArray<BoundExpression> arguments, BoundExpression implicitReceiverOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, bool invokedAsExtensionMethod, LookupResultKind resultKind, Binder binderOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CollectionElementInitializer, syntax, type, hasErrors || arguments.HasErrors() || implicitReceiverOpt.HasErrors())
	{
		AddMethod = addMethod;
		Arguments = arguments;
		ImplicitReceiverOpt = implicitReceiverOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		InvokedAsExtensionMethod = invokedAsExtensionMethod;
		_ResultKind = resultKind;
		BinderOpt = binderOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCollectionElementInitializer(this);
	}

	public BoundCollectionElementInitializer Update(MethodSymbol addMethod, ImmutableArray<BoundExpression> arguments, BoundExpression implicitReceiverOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, bool invokedAsExtensionMethod, LookupResultKind resultKind, Binder binderOpt, TypeSymbol type)
	{
		if (addMethod != AddMethod || arguments != Arguments || implicitReceiverOpt != ImplicitReceiverOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || invokedAsExtensionMethod != InvokedAsExtensionMethod || resultKind != ResultKind || binderOpt != BinderOpt || type != Type)
		{
			return new BoundCollectionElementInitializer(Syntax, addMethod, arguments, implicitReceiverOpt, expanded, argsToParamsOpt, invokedAsExtensionMethod, resultKind, binderOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
