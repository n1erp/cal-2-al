using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFieldInfo : BoundExpression
{
	public FieldSymbol Field { get; }

	public MethodSymbol GetFieldFromHandle { get; }

	public BoundFieldInfo(SyntaxNode syntax, FieldSymbol field, MethodSymbol getFieldFromHandle, TypeSymbol type, bool hasErrors)
		: base(BoundKind.FieldInfo, syntax, type, hasErrors)
	{
		Field = field;
		GetFieldFromHandle = getFieldFromHandle;
	}

	public BoundFieldInfo(SyntaxNode syntax, FieldSymbol field, MethodSymbol getFieldFromHandle, TypeSymbol type)
		: base(BoundKind.FieldInfo, syntax, type)
	{
		Field = field;
		GetFieldFromHandle = getFieldFromHandle;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFieldInfo(this);
	}

	public BoundFieldInfo Update(FieldSymbol field, MethodSymbol getFieldFromHandle, TypeSymbol type)
	{
		if (field != Field || getFieldFromHandle != GetFieldFromHandle || type != Type)
		{
			return new BoundFieldInfo(Syntax, field, getFieldFromHandle, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
