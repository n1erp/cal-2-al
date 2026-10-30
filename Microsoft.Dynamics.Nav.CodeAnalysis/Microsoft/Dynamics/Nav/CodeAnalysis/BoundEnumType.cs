using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundEnumType : BoundExpression
{
	public override Symbol ExpressionSymbol => EnumType;

	public EnumTypeSymbol EnumType { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.EnumTypeAccess;

	public BoundEnumType(SyntaxNode syntax, EnumTypeSymbol enumType, TypeSymbol type, bool hasErrors)
		: base(BoundKind.EnumType, syntax, type, hasErrors)
	{
		EnumType = enumType;
	}

	public BoundEnumType(SyntaxNode syntax, EnumTypeSymbol enumType, TypeSymbol type)
		: base(BoundKind.EnumType, syntax, type)
	{
		EnumType = enumType;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitEnumType(this);
	}

	public BoundEnumType Update(EnumTypeSymbol enumType, TypeSymbol type)
	{
		if (enumType != EnumType || type != Type)
		{
			return new BoundEnumType(Syntax, enumType, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEnumTypeAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEnumTypeAccess(this, argument);
	}
}
