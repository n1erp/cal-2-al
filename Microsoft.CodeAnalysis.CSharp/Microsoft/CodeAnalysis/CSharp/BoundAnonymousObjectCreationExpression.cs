using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAnonymousObjectCreationExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => Constructor;

	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(Arguments);

	public MethodSymbol Constructor { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<BoundAnonymousPropertyDeclaration> Declarations { get; }

	public BoundAnonymousObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<BoundAnonymousPropertyDeclaration> declarations, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AnonymousObjectCreationExpression, syntax, type, hasErrors || arguments.HasErrors() || declarations.HasErrors())
	{
		Constructor = constructor;
		Arguments = arguments;
		Declarations = declarations;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAnonymousObjectCreationExpression(this);
	}

	public BoundAnonymousObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<BoundAnonymousPropertyDeclaration> declarations, TypeSymbol type)
	{
		if (constructor != Constructor || arguments != Arguments || declarations != Declarations || type != Type)
		{
			return new BoundAnonymousObjectCreationExpression(Syntax, constructor, arguments, declarations, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
