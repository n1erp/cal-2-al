using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDeclarationPattern : BoundPattern
{
	public Symbol Variable { get; }

	public BoundExpression VariableAccess { get; }

	public BoundTypeExpression DeclaredType { get; }

	public bool IsVar { get; }

	public BoundDeclarationPattern(SyntaxNode syntax, LocalSymbol localSymbol, BoundTypeExpression declaredType, bool isVar, bool hasErrors = false)
		: this(syntax, localSymbol, (localSymbol == null) ? ((BoundExpression)new BoundDiscardExpression(syntax, declaredType.Type)) : ((BoundExpression)new BoundLocal(syntax, localSymbol, null, declaredType.Type)), declaredType, isVar, hasErrors)
	{
	}

	public BoundDeclarationPattern(SyntaxNode syntax, Symbol variable, BoundExpression variableAccess, BoundTypeExpression declaredType, bool isVar, bool hasErrors = false)
		: base(BoundKind.DeclarationPattern, syntax, hasErrors || variableAccess.HasErrors() || declaredType.HasErrors())
	{
		Variable = variable;
		VariableAccess = variableAccess;
		DeclaredType = declaredType;
		IsVar = isVar;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeclarationPattern(this);
	}

	public BoundDeclarationPattern Update(Symbol variable, BoundExpression variableAccess, BoundTypeExpression declaredType, bool isVar)
	{
		if (variable != Variable || variableAccess != VariableAccess || declaredType != DeclaredType || isVar != IsVar)
		{
			return new BoundDeclarationPattern(Syntax, variable, variableAccess, declaredType, isVar, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
