using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundPermissionPropertyValue : BoundPropertyValue
{
	private const int ToIndirectPermission = 32;

	private uint[] indirectPermissionValues;

	public ImmutableArray<BoundPermissionExpression> Value { get; private set; }

	internal override object ValueObject => ValueText;

	internal override string ValueText => Syntax.ToString();

	internal uint[] IndirectPermissionValues
	{
		get
		{
			if (indirectPermissionValues == null)
			{
				indirectPermissionValues = GetIndirectPermissionValues();
			}
			return indirectPermissionValues;
		}
	}

	public BoundPermissionPropertyValue(SyntaxNode syntax, ImmutableArray<BoundPermissionExpression> value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PermissionPropertyValue, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPermissionPropertyValue(this);
	}

	public BoundPermissionPropertyValue Update(ImmutableArray<BoundPermissionExpression> value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundPermissionPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	private uint[] GetIndirectPermissionValues()
	{
		uint[] array = new uint[Value.Length * 2 + 2];
		for (int i = 0; i < Value.Length; i++)
		{
			BoundPermissionExpression boundPermissionExpression = Value[i];
			DebugAssertHelper.Assert(boundPermissionExpression.Target.Type.NavTypeKind == NavTypeKind.Record);
			DebugAssertHelper.Assert(boundPermissionExpression.Permission.IsIntegral);
			uint permissionObjectId = (uint)boundPermissionExpression.GetPermissionObjectId();
			uint num = boundPermissionExpression.Permission.UInt32Value * 32;
			array[i * 2] = permissionObjectId;
			array[i * 2 + 1] = num;
		}
		array[array.Length - 2] = 0u;
		array[array.Length - 1] = 0u;
		return array;
	}
}
