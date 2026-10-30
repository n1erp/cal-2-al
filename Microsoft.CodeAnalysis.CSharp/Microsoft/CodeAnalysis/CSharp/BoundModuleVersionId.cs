using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundModuleVersionId : BoundExpression
{
	public BoundModuleVersionId(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ModuleVersionId, syntax, type, hasErrors)
	{
	}

	public BoundModuleVersionId(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ModuleVersionId, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitModuleVersionId(this);
	}

	public BoundModuleVersionId Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundModuleVersionId(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
