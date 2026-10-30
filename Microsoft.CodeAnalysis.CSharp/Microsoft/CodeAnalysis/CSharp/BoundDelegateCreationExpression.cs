using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDelegateCreationExpression : BoundExpression
{
	public BoundExpression Argument { get; }

	public MethodSymbol MethodOpt { get; }

	public bool IsExtensionMethod { get; }

	public BoundDelegateCreationExpression(SyntaxNode syntax, BoundExpression argument, MethodSymbol methodOpt, bool isExtensionMethod, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DelegateCreationExpression, syntax, type, hasErrors || argument.HasErrors())
	{
		Argument = argument;
		MethodOpt = methodOpt;
		IsExtensionMethod = isExtensionMethod;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDelegateCreationExpression(this);
	}

	public BoundDelegateCreationExpression Update(BoundExpression argument, MethodSymbol methodOpt, bool isExtensionMethod, TypeSymbol type)
	{
		if (argument != Argument || methodOpt != MethodOpt || isExtensionMethod != IsExtensionMethod || type != Type)
		{
			return new BoundDelegateCreationExpression(Syntax, argument, methodOpt, isExtensionMethod, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
