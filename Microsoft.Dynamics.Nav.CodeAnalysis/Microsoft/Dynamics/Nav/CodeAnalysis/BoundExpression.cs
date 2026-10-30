using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundExpression : BoundNode, IOperation
{
	public virtual ConstantValue ConstantValue => null;

	public virtual LookupResultKind ResultKind => LookupResultKind.Viable;

	public virtual Symbol ExpressionSymbol => null;

	public virtual object Display => Type;

	internal bool IsAssignable => Binder.CheckValueKind(this, Binder.BindValueKind.Assignment);

	internal virtual TypeSymbol MemberType => Type;

	public TypeSymbol Type { get; private set; }

	protected abstract OperationKind ExpressionKind { get; }

	ITypeSymbol IOperation.Type => Type;

	OperationKind IOperation.Kind => ExpressionKind;

	bool IOperation.IsInvalid => HasErrors;

	Optional<object> IOperation.ConstantValue
	{
		get
		{
			ConstantValue constantValue = ConstantValue;
			if (!(constantValue != null))
			{
				return default;
			}
			return new Optional<object>(constantValue.Value);
		}
	}

	SyntaxNode IOperation.Syntax => Syntax;

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		Type = type;
	}

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax)
	{
		Type = type;
	}

	public abstract void Accept(OperationVisitor visitor);

	public abstract TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);
}
