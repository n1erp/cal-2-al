using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundSystemOptionType : BoundExpression, IStaticTypeReferenceExpression, IOperation
{
	public SystemOptionTypeSymbol OptionTypeSymbol { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	ITypeSymbol IStaticTypeReferenceExpression.StaticType => OptionTypeSymbol;

	public BoundSystemOptionType(SyntaxNode syntax, SystemOptionTypeSymbol optionTypeSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.SystemOptionType, syntax, type, hasErrors)
	{
		OptionTypeSymbol = optionTypeSymbol;
	}

	public BoundSystemOptionType(SyntaxNode syntax, SystemOptionTypeSymbol optionTypeSymbol, TypeSymbol type)
		: base(BoundKind.SystemOptionType, syntax, type)
	{
		OptionTypeSymbol = optionTypeSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSystemOptionType(this);
	}

	public BoundSystemOptionType Update(SystemOptionTypeSymbol optionTypeSymbol, TypeSymbol type)
	{
		if (optionTypeSymbol != OptionTypeSymbol || type != Type)
		{
			return new BoundSystemOptionType(Syntax, optionTypeSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
