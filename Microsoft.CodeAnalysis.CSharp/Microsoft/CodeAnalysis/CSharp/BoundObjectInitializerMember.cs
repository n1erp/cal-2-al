using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundObjectInitializerMember : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => MemberSymbol;

	public Symbol MemberSymbol { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public bool Expanded { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public TypeSymbol ReceiverType { get; }

	public Binder BinderOpt { get; }

	public BoundObjectInitializerMember(SyntaxNode syntax, Symbol memberSymbol, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, LookupResultKind resultKind, TypeSymbol receiverType, Binder binderOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectInitializerMember, syntax, type, hasErrors || arguments.HasErrors())
	{
		MemberSymbol = memberSymbol;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		_ResultKind = resultKind;
		ReceiverType = receiverType;
		BinderOpt = binderOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectInitializerMember(this);
	}

	public BoundObjectInitializerMember Update(Symbol memberSymbol, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, LookupResultKind resultKind, TypeSymbol receiverType, Binder binderOpt, TypeSymbol type)
	{
		if (memberSymbol != MemberSymbol || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || resultKind != ResultKind || receiverType != ReceiverType || binderOpt != BinderOpt || type != Type)
		{
			return new BoundObjectInitializerMember(Syntax, memberSymbol, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, resultKind, receiverType, binderOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
