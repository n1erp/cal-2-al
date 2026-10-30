using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundQueryElementAccess : BoundExpression, IQueryElementAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => QueryElementSymbol;

	internal override TypeSymbol MemberType
	{
		get
		{
			if (Type.NavTypeKind == NavTypeKind.QueryFilter)
			{
				return ((QueryFilterSymbol)QueryElementSymbol).RelatedField?.Type ?? Type;
			}
			return Type;
		}
	}

	public BoundExpression ReceiverOpt { get; private set; }

	public QueryElementSymbol QueryElementSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IQueryElementAccess.Instance => ReceiverOpt;

	IQueryElementSymbol IQueryElementAccess.QueryElementSymbol => QueryElementSymbol;

	protected override OperationKind ExpressionKind => OperationKind.QueryElementAccess;

	public BoundQueryElementAccess(SyntaxNode syntax, BoundExpression receiverOpt, QueryElementSymbol queryElementSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.QueryElementAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		QueryElementSymbol = queryElementSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitQueryElementAccess(this);
	}

	public BoundQueryElementAccess Update(BoundExpression receiverOpt, QueryElementSymbol queryElementSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || queryElementSymbol != QueryElementSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundQueryElementAccess(Syntax, receiverOpt, queryElementSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitQueryColumnAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitQueryElementAccess(this, argument);
	}
}
