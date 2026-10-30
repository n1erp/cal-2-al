using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCall : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => Method;

	public ImmutableArray<MethodSymbol> OriginalMethodsOpt { get; private set; }

	public BoundExpression ReceiverOpt { get; }

	public MethodSymbol Method { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public bool IsDelegateCall { get; }

	public bool Expanded { get; }

	public bool InvokedAsExtensionMethod { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public Binder BinderOpt { get; }

	public static BoundCall ErrorCall(SyntaxNode node, BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> namedArguments, ImmutableArray<RefKind> refKinds, bool isDelegateCall, bool invokedAsExtensionMethod, ImmutableArray<MethodSymbol> originalMethods, LookupResultKind resultKind, Binder binder)
	{
		if (!originalMethods.IsEmpty)
		{
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		return new BoundCall(node, receiverOpt, method, arguments, namedArguments, refKinds, isDelegateCall, expanded: false, invokedAsExtensionMethod, default, resultKind, binder, method.ReturnType, hasErrors: true)
		{
			OriginalMethodsOpt = originalMethods
		};
	}

	public BoundCall Update(BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments)
	{
		return Update(receiverOpt, method, arguments, ArgumentNamesOpt, ArgumentRefKindsOpt, IsDelegateCall, Expanded, InvokedAsExtensionMethod, ArgsToParamsOpt, ResultKind, BinderOpt, Type);
	}

	public static BoundCall Synthesized(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method)
	{
		return Synthesized(syntax, receiverOpt, method, ImmutableArray<BoundExpression>.Empty);
	}

	public static BoundCall Synthesized(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method, BoundExpression arg0)
	{
		return Synthesized(syntax, receiverOpt, method, ImmutableArray.Create(arg0));
	}

	public static BoundCall Synthesized(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method, BoundExpression arg0, BoundExpression arg1)
	{
		return Synthesized(syntax, receiverOpt, method, ImmutableArray.Create(arg0, arg1));
	}

	public static BoundCall Synthesized(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments)
	{
		return new BoundCall(syntax, receiverOpt, method, arguments, default, method.ParameterRefKinds, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default, LookupResultKind.Viable, null, method.ReturnType, method.OriginalDefinition is ErrorMethodSymbol)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundCall(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool isDelegateCall, bool expanded, bool invokedAsExtensionMethod, ImmutableArray<int> argsToParamsOpt, LookupResultKind resultKind, Binder binderOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Call, syntax, type, hasErrors || receiverOpt.HasErrors() || arguments.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Method = method;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		IsDelegateCall = isDelegateCall;
		Expanded = expanded;
		InvokedAsExtensionMethod = invokedAsExtensionMethod;
		ArgsToParamsOpt = argsToParamsOpt;
		_ResultKind = resultKind;
		BinderOpt = binderOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCall(this);
	}

	public BoundCall Update(BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool isDelegateCall, bool expanded, bool invokedAsExtensionMethod, ImmutableArray<int> argsToParamsOpt, LookupResultKind resultKind, Binder binderOpt, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || method != Method || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || isDelegateCall != IsDelegateCall || expanded != Expanded || invokedAsExtensionMethod != InvokedAsExtensionMethod || argsToParamsOpt != ArgsToParamsOpt || resultKind != ResultKind || binderOpt != BinderOpt || type != Type)
		{
			return new BoundCall(Syntax, receiverOpt, method, arguments, argumentNamesOpt, argumentRefKindsOpt, isDelegateCall, expanded, invokedAsExtensionMethod, argsToParamsOpt, resultKind, binderOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
