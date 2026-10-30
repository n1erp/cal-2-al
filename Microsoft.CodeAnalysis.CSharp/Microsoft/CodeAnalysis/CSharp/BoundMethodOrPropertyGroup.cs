using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundMethodOrPropertyGroup : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)ReceiverOpt);

	public BoundExpression ReceiverOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	protected BoundMethodOrPropertyGroup(BoundKind kind, SyntaxNode syntax, BoundExpression receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(kind, syntax, null, hasErrors)
	{
		ReceiverOpt = receiverOpt;
		_ResultKind = resultKind;
	}
}
