using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicMemberAccess : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Receiver);

	public BoundExpression Receiver { get; }

	public ImmutableArray<TypeSymbol> TypeArgumentsOpt { get; }

	public string Name { get; }

	public bool Invoked { get; }

	public bool Indexed { get; }

	public BoundDynamicMemberAccess(SyntaxNode syntax, BoundExpression receiver, ImmutableArray<TypeSymbol> typeArgumentsOpt, string name, bool invoked, bool indexed, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicMemberAccess, syntax, type, hasErrors || receiver.HasErrors())
	{
		Receiver = receiver;
		TypeArgumentsOpt = typeArgumentsOpt;
		Name = name;
		Invoked = invoked;
		Indexed = indexed;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicMemberAccess(this);
	}

	public BoundDynamicMemberAccess Update(BoundExpression receiver, ImmutableArray<TypeSymbol> typeArgumentsOpt, string name, bool invoked, bool indexed, TypeSymbol type)
	{
		if (receiver != Receiver || typeArgumentsOpt != TypeArgumentsOpt || name != Name || invoked != Invoked || indexed != Indexed || type != Type)
		{
			return new BoundDynamicMemberAccess(Syntax, receiver, typeArgumentsOpt, name, invoked, indexed, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
