using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundModuleVersionIdString : BoundExpression
{
	public BoundModuleVersionIdString(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ModuleVersionIdString, syntax, type, hasErrors)
	{
	}

	public BoundModuleVersionIdString(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ModuleVersionIdString, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitModuleVersionIdString(this);
	}

	public BoundModuleVersionIdString Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundModuleVersionIdString(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
