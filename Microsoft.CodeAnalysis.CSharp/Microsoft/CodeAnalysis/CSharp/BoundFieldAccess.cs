using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFieldAccess : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => FieldSymbol;

	public BoundExpression ReceiverOpt { get; }

	public FieldSymbol FieldSymbol { get; }

	public ConstantValue ConstantValueOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public bool IsByValue { get; }

	public bool IsDeclaration { get; }

	public BoundFieldAccess(SyntaxNode syntax, BoundExpression receiver, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, bool hasErrors = false)
		: this(syntax, receiver, fieldSymbol, constantValueOpt, LookupResultKind.Viable, fieldSymbol.Type, hasErrors)
	{
	}

	public BoundFieldAccess(SyntaxNode syntax, BoundExpression receiver, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: this(syntax, receiver, fieldSymbol, constantValueOpt, resultKind, NeedsByValueFieldAccess(receiver, fieldSymbol), isDeclaration: false, type, hasErrors)
	{
	}

	public BoundFieldAccess(SyntaxNode syntax, BoundExpression receiver, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, bool isDeclaration, TypeSymbol type, bool hasErrors = false)
		: this(syntax, receiver, fieldSymbol, constantValueOpt, resultKind, NeedsByValueFieldAccess(receiver, fieldSymbol), isDeclaration, type, hasErrors)
	{
	}

	public BoundFieldAccess Update(BoundExpression receiver, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, TypeSymbol typeSymbol)
	{
		return Update(receiver, fieldSymbol, constantValueOpt, resultKind, IsByValue, IsDeclaration, typeSymbol);
	}

	private static bool NeedsByValueFieldAccess(BoundExpression receiver, FieldSymbol fieldSymbol)
	{
		if (fieldSymbol.IsStatic || !fieldSymbol.ContainingType.IsValueType || receiver == null)
		{
			return false;
		}
		return receiver.Kind switch
		{
			BoundKind.FieldAccess => ((BoundFieldAccess)receiver).IsByValue, 
			BoundKind.Local => !((BoundLocal)receiver).LocalSymbol.IsWritableVariable, 
			_ => false, 
		};
	}

	public BoundFieldAccess(SyntaxNode syntax, BoundExpression receiverOpt, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, bool isByValue, bool isDeclaration, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.FieldAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		FieldSymbol = fieldSymbol;
		ConstantValueOpt = constantValueOpt;
		_ResultKind = resultKind;
		IsByValue = isByValue;
		IsDeclaration = isDeclaration;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFieldAccess(this);
	}

	public BoundFieldAccess Update(BoundExpression receiverOpt, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, bool isByValue, bool isDeclaration, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || fieldSymbol != FieldSymbol || constantValueOpt != ConstantValueOpt || resultKind != ResultKind || isByValue != IsByValue || isDeclaration != IsDeclaration || type != Type)
		{
			return new BoundFieldAccess(Syntax, receiverOpt, fieldSymbol, constantValueOpt, resultKind, isByValue, isDeclaration, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
