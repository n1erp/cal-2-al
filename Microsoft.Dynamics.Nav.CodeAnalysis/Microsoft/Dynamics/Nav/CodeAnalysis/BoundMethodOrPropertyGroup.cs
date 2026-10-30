using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundMethodOrPropertyGroup : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public BoundExpression ReceiverOpt { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	protected BoundMethodOrPropertyGroup(BoundKind kind, SyntaxNode syntax, BoundExpression receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(kind, syntax, null, hasErrors)
	{
		ReceiverOpt = receiverOpt;
		_ResultKind = resultKind;
	}
}
