using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundEventAccess : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => EventSymbol;

	public BoundExpression ReceiverOpt { get; }

	public EventSymbol EventSymbol { get; }

	public bool IsUsableAsField { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundEventAccess(SyntaxNode syntax, BoundExpression receiverOpt, EventSymbol eventSymbol, bool isUsableAsField, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.EventAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		EventSymbol = eventSymbol;
		IsUsableAsField = isUsableAsField;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitEventAccess(this);
	}

	public BoundEventAccess Update(BoundExpression receiverOpt, EventSymbol eventSymbol, bool isUsableAsField, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || eventSymbol != EventSymbol || isUsableAsField != IsUsableAsField || resultKind != ResultKind || type != Type)
		{
			return new BoundEventAccess(Syntax, receiverOpt, eventSymbol, isUsableAsField, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
