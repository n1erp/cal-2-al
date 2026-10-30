using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundMethodDefIndex : BoundExpression
{
	public MethodSymbol Method { get; }

	public BoundMethodDefIndex(SyntaxNode syntax, MethodSymbol method, TypeSymbol type, bool hasErrors)
		: base(BoundKind.MethodDefIndex, syntax, type, hasErrors)
	{
		Method = method;
	}

	public BoundMethodDefIndex(SyntaxNode syntax, MethodSymbol method, TypeSymbol type)
		: base(BoundKind.MethodDefIndex, syntax, type)
	{
		Method = method;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMethodDefIndex(this);
	}

	public BoundMethodDefIndex Update(MethodSymbol method, TypeSymbol type)
	{
		if (method != Method || type != Type)
		{
			return new BoundMethodDefIndex(Syntax, method, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
