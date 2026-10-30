using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLocal : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => LocalSymbol;

	public LocalSymbol LocalSymbol { get; }

	public bool IsDeclaration { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundLocal(SyntaxNode syntax, LocalSymbol localSymbol, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors)
		: this(syntax, localSymbol, isDeclaration: false, constantValueOpt, type, hasErrors)
	{
	}

	public BoundLocal(SyntaxNode syntax, LocalSymbol localSymbol, ConstantValue constantValueOpt, TypeSymbol type)
		: this(syntax, localSymbol, isDeclaration: false, constantValueOpt, type)
	{
	}

	public BoundLocal Update(LocalSymbol localSymbol, ConstantValue constantValueOpt, TypeSymbol type)
	{
		return Update(localSymbol, IsDeclaration, constantValueOpt, type);
	}

	public BoundLocal(SyntaxNode syntax, LocalSymbol localSymbol, bool isDeclaration, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Local, syntax, type, hasErrors)
	{
		LocalSymbol = localSymbol;
		IsDeclaration = isDeclaration;
		ConstantValueOpt = constantValueOpt;
	}

	public BoundLocal(SyntaxNode syntax, LocalSymbol localSymbol, bool isDeclaration, ConstantValue constantValueOpt, TypeSymbol type)
		: base(BoundKind.Local, syntax, type)
	{
		LocalSymbol = localSymbol;
		IsDeclaration = isDeclaration;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLocal(this);
	}

	public BoundLocal Update(LocalSymbol localSymbol, bool isDeclaration, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (localSymbol != LocalSymbol || isDeclaration != IsDeclaration || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundLocal(Syntax, localSymbol, isDeclaration, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
