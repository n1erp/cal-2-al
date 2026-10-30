using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundMultipleLocalDeclarations : BoundStatement
{
	public ImmutableArray<BoundLocalDeclaration> LocalDeclarations { get; }

	public BoundMultipleLocalDeclarations(SyntaxNode syntax, ImmutableArray<BoundLocalDeclaration> localDeclarations, bool hasErrors = false)
		: base(BoundKind.MultipleLocalDeclarations, syntax, hasErrors || localDeclarations.HasErrors())
	{
		LocalDeclarations = localDeclarations;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMultipleLocalDeclarations(this);
	}

	public BoundMultipleLocalDeclarations Update(ImmutableArray<BoundLocalDeclaration> localDeclarations)
	{
		if (localDeclarations != LocalDeclarations)
		{
			return new BoundMultipleLocalDeclarations(Syntax, localDeclarations, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
