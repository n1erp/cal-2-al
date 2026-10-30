using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundReportDataItemAccess : BoundExpression, IReportDataItemAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => ReportDataItemSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public ReportDataItemSymbol ReportDataItemSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IReportDataItemAccess.Instance => ReceiverOpt;

	IReportDataItemSymbol IReportDataItemAccess.DataItemSymbol => ReportDataItemSymbol;

	protected override OperationKind ExpressionKind => OperationKind.ReportDataItemAccess;

	public BoundReportDataItemAccess(SyntaxNode syntax, BoundExpression receiverOpt, ReportDataItemSymbol reportDataItemSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ReportDataItemAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		ReportDataItemSymbol = reportDataItemSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitReportDataItemAccess(this);
	}

	public BoundReportDataItemAccess Update(BoundExpression receiverOpt, ReportDataItemSymbol reportDataItemSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || reportDataItemSymbol != ReportDataItemSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundReportDataItemAccess(Syntax, receiverOpt, reportDataItemSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitReportDataItemAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitReportDataItemAccess(this, argument);
	}
}
