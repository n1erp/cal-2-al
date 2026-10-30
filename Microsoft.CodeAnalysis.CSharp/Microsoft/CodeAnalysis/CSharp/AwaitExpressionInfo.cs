using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public struct AwaitExpressionInfo : IEquatable<AwaitExpressionInfo>
{
	public IMethodSymbol GetAwaiterMethod { get; }

	public IPropertySymbol IsCompletedProperty { get; }

	public IMethodSymbol GetResultMethod { get; }

	public bool IsDynamic { get; }

	internal AwaitExpressionInfo(IMethodSymbol getAwaiterMethod, IPropertySymbol isCompletedProperty, IMethodSymbol getResultMethod, bool isDynamic)
	{
		GetAwaiterMethod = getAwaiterMethod;
		IsCompletedProperty = isCompletedProperty;
		GetResultMethod = getResultMethod;
		IsDynamic = isDynamic;
	}

	public override bool Equals(object obj)
	{
		if (obj is AwaitExpressionInfo)
		{
			return Equals((AwaitExpressionInfo)obj);
		}
		return false;
	}

	public bool Equals(AwaitExpressionInfo other)
	{
		if (object.Equals(GetAwaiterMethod, other.GetAwaiterMethod) && object.Equals(IsCompletedProperty, other.IsCompletedProperty) && object.Equals(GetResultMethod, other.GetResultMethod))
		{
			return IsDynamic == other.IsDynamic;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(GetAwaiterMethod, Hash.Combine(IsCompletedProperty, Hash.Combine(GetResultMethod, IsDynamic.GetHashCode())));
	}
}
