using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundClassType : BoundExpression, IStaticTypeReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => Type;

	public LanguageClassTypeSymbol ClassSymbol { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	ITypeSymbol IStaticTypeReferenceExpression.StaticType => ClassSymbol;

	public BoundClassType(SyntaxNode syntax, LanguageClassTypeSymbol classSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ClassType, syntax, type, hasErrors)
	{
		ClassSymbol = classSymbol;
	}

	public BoundClassType(SyntaxNode syntax, LanguageClassTypeSymbol classSymbol, TypeSymbol type)
		: base(BoundKind.ClassType, syntax, type)
	{
		ClassSymbol = classSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitClassType(this);
	}

	public BoundClassType Update(LanguageClassTypeSymbol classSymbol, TypeSymbol type)
	{
		if (classSymbol != ClassSymbol || type != Type)
		{
			return new BoundClassType(Syntax, classSymbol, type, HasErrors)
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
