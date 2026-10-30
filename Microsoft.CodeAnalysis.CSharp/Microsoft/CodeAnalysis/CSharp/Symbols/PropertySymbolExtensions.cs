namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class PropertySymbolExtensions
{
	public static MethodSymbol GetOwnOrInheritedGetMethod(this PropertySymbol property)
	{
		while ((object)property != null)
		{
			MethodSymbol getMethod = property.GetMethod;
			if ((object)getMethod != null)
			{
				return getMethod;
			}
			property = property.OverriddenProperty;
		}
		return null;
	}

	public static MethodSymbol GetOwnOrInheritedSetMethod(this PropertySymbol property)
	{
		while ((object)property != null)
		{
			MethodSymbol setMethod = property.SetMethod;
			if ((object)setMethod != null)
			{
				return setMethod;
			}
			property = property.OverriddenProperty;
		}
		return null;
	}

	public static bool CanCallMethodsDirectly(this PropertySymbol property)
	{
		if (property.MustCallMethodsDirectly)
		{
			return true;
		}
		if (property.IsIndexedProperty)
		{
			if (property.IsIndexer)
			{
				return property.HasRefOrOutParameter();
			}
			return true;
		}
		return false;
	}

	public static bool HasRefOrOutParameter(this PropertySymbol property)
	{
		foreach (ParameterSymbol parameter in property.Parameters)
		{
			if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
			{
				return true;
			}
		}
		return false;
	}
}
