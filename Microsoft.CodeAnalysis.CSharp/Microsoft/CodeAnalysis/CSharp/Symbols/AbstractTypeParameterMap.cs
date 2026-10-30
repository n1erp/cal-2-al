using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class AbstractTypeParameterMap : AbstractTypeMap
{
	protected readonly SmallDictionary<TypeParameterSymbol, TypeWithModifiers> Mapping;

	protected AbstractTypeParameterMap(SmallDictionary<TypeParameterSymbol, TypeWithModifiers> mapping)
	{
		Mapping = mapping;
	}

	protected sealed override TypeWithModifiers SubstituteTypeParameter(TypeParameterSymbol typeParameter)
	{
		if (Mapping.TryGetValue(typeParameter, out var value))
		{
			return value;
		}
		return new TypeWithModifiers(typeParameter);
	}

	private string GetDebuggerDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder("[");
		stringBuilder.Append(GetType().Name);
		foreach (KeyValuePair<TypeParameterSymbol, TypeWithModifiers> item in Mapping)
		{
			stringBuilder.Append(" ").Append(item.Key).Append(":")
				.Append(item.Value.Type);
		}
		return stringBuilder.Append("]").ToString();
	}
}
