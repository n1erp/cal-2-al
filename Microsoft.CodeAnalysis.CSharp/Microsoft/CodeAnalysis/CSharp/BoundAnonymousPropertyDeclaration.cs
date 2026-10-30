using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAnonymousPropertyDeclaration : BoundExpression
{
	public override Symbol ExpressionSymbol => Property;

	public PropertySymbol Property { get; }

	public BoundAnonymousPropertyDeclaration(SyntaxNode syntax, PropertySymbol property, TypeSymbol type, bool hasErrors)
		: base(BoundKind.AnonymousPropertyDeclaration, syntax, type, hasErrors)
	{
		Property = property;
	}

	public BoundAnonymousPropertyDeclaration(SyntaxNode syntax, PropertySymbol property, TypeSymbol type)
		: base(BoundKind.AnonymousPropertyDeclaration, syntax, type)
	{
		Property = property;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAnonymousPropertyDeclaration(this);
	}

	public BoundAnonymousPropertyDeclaration Update(PropertySymbol property, TypeSymbol type)
	{
		if (property != Property || type != Type)
		{
			return new BoundAnonymousPropertyDeclaration(Syntax, property, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
