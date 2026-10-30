using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundPermissionExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => Target.Type;

	public SyntaxKind ObjectType { get; private set; }

	public BoundExpression Target { get; private set; }

	public ConstantValue Permission { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	internal string GetPermissionString()
	{
		string text = Syntax.ToString();
		text = text.Substring(text.IndexOf('=') + 1).Trim().ToLowerInvariant();
		string permissionValueObjectTypeName = ObjectType.GetPermissionValueObjectTypeName();
		string name = ((BoundApplicationObjectAccess)Target).ApplicationObjectTypeSymbol.Name;
		return string.Format(CultureInfo.InvariantCulture, "{0} {1}={2}", permissionValueObjectTypeName, name, text);
	}

	internal int GetPermissionObjectId()
	{
		if (ObjectType == SyntaxKind.SystemKeyword)
		{
			string stringValue = ((BoundLiteral)Target).ConstantValue.StringValue;
			return SystemObjects.SystemNameAndObjectsDictionary[stringValue];
		}
		return ((BoundApplicationObjectAccess)Target).ApplicationObjectTypeSymbol.Id;
	}

	public BoundPermissionExpression(SyntaxNode syntax, SyntaxKind objectType, BoundExpression target, ConstantValue permission, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PermissionExpression, syntax, type, hasErrors || target.HasErrors())
	{
		ObjectType = objectType;
		Target = target;
		Permission = permission;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPermissionExpression(this);
	}

	public BoundPermissionExpression Update(SyntaxKind objectType, BoundExpression target, ConstantValue permission, TypeSymbol type)
	{
		if (objectType != ObjectType || target != Target || permission != Permission || type != Type)
		{
			return new BoundPermissionExpression(Syntax, objectType, target, permission, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
