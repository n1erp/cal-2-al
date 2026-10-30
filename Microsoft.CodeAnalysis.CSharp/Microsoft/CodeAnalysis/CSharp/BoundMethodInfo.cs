using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundMethodInfo : BoundExpression
{
	public MethodSymbol Method { get; }

	public MethodSymbol GetMethodFromHandle { get; }

	public BoundMethodInfo(SyntaxNode syntax, MethodSymbol method, MethodSymbol getMethodFromHandle, TypeSymbol type, bool hasErrors)
		: base(BoundKind.MethodInfo, syntax, type, hasErrors)
	{
		Method = method;
		GetMethodFromHandle = getMethodFromHandle;
	}

	public BoundMethodInfo(SyntaxNode syntax, MethodSymbol method, MethodSymbol getMethodFromHandle, TypeSymbol type)
		: base(BoundKind.MethodInfo, syntax, type)
	{
		Method = method;
		GetMethodFromHandle = getMethodFromHandle;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMethodInfo(this);
	}

	public BoundMethodInfo Update(MethodSymbol method, MethodSymbol getMethodFromHandle, TypeSymbol type)
	{
		if (method != Method || getMethodFromHandle != GetMethodFromHandle || type != Type)
		{
			return new BoundMethodInfo(Syntax, method, getMethodFromHandle, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
