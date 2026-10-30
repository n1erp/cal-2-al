using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundQueryDataItemAccess : BoundExpression, IQueryDataItemAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => QueryDataItemSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public QueryDataItemSymbol QueryDataItemSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IQueryDataItemAccess.Instance => ReceiverOpt;

	IQueryDataItemSymbol IQueryDataItemAccess.DataItemSymbol => QueryDataItemSymbol;

	protected override OperationKind ExpressionKind => OperationKind.QueryDataItemAccess;

	public BoundQueryDataItemAccess(SyntaxNode syntax, BoundExpression receiverOpt, QueryDataItemSymbol queryDataItemSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.QueryDataItemAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		QueryDataItemSymbol = queryDataItemSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitQueryDataItemAccess(this);
	}

	public BoundQueryDataItemAccess Update(BoundExpression receiverOpt, QueryDataItemSymbol queryDataItemSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || queryDataItemSymbol != QueryDataItemSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundQueryDataItemAccess(Syntax, receiverOpt, queryDataItemSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitQueryDataItemAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitQueryDataItemAccess(this, argument);
	}
}
