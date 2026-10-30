using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public struct ForEachStatementInfo : IEquatable<ForEachStatementInfo>
{
	public IMethodSymbol GetEnumeratorMethod { get; }

	public IMethodSymbol MoveNextMethod { get; }

	public IPropertySymbol CurrentProperty { get; }

	public IMethodSymbol DisposeMethod { get; }

	public ITypeSymbol ElementType { get; }

	public Conversion ElementConversion { get; }

	public Conversion CurrentConversion { get; }

	internal ForEachStatementInfo(IMethodSymbol getEnumeratorMethod, IMethodSymbol moveNextMethod, IPropertySymbol currentProperty, IMethodSymbol disposeMethod, ITypeSymbol elementType, Conversion elementConversion, Conversion currentConversion)
	{
		GetEnumeratorMethod = getEnumeratorMethod;
		MoveNextMethod = moveNextMethod;
		CurrentProperty = currentProperty;
		DisposeMethod = disposeMethod;
		ElementType = elementType;
		ElementConversion = elementConversion;
		CurrentConversion = currentConversion;
	}

	public override bool Equals(object obj)
	{
		if (obj is ForEachStatementInfo)
		{
			return Equals((ForEachStatementInfo)obj);
		}
		return false;
	}

	public bool Equals(ForEachStatementInfo other)
	{
		if (object.Equals(GetEnumeratorMethod, other.GetEnumeratorMethod) && object.Equals(MoveNextMethod, other.MoveNextMethod) && object.Equals(CurrentProperty, other.CurrentProperty) && object.Equals(DisposeMethod, other.DisposeMethod) && object.Equals(ElementType, other.ElementType) && ElementConversion == other.ElementConversion)
		{
			return CurrentConversion == other.CurrentConversion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(GetEnumeratorMethod, Hash.Combine(MoveNextMethod, Hash.Combine(CurrentProperty, Hash.Combine(DisposeMethod, Hash.Combine(ElementType, Hash.Combine(ElementConversion.GetHashCode(), CurrentConversion.GetHashCode()))))));
	}
}
