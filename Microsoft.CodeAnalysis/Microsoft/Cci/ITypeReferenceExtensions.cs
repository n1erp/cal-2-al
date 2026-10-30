using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal static class ITypeReferenceExtensions
{
	internal static void GetConsolidatedTypeArguments(this ITypeReference typeReference, ArrayBuilder<ITypeReference> consolidatedTypeArguments, EmitContext context)
	{
		typeReference.AsNestedTypeReference?.GetContainingType(context).GetConsolidatedTypeArguments(consolidatedTypeArguments, context);
		IGenericTypeInstanceReference asGenericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
		if (asGenericTypeInstanceReference != null)
		{
			consolidatedTypeArguments.AddRange(asGenericTypeInstanceReference.GetGenericArguments(context));
		}
	}

	internal static ITypeReference GetUninstantiatedGenericType(this ITypeReference typeReference, EmitContext context)
	{
		IGenericTypeInstanceReference asGenericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
		if (asGenericTypeInstanceReference != null)
		{
			return asGenericTypeInstanceReference.GetGenericType(context);
		}
		ISpecializedNestedTypeReference asSpecializedNestedTypeReference = typeReference.AsSpecializedNestedTypeReference;
		if (asSpecializedNestedTypeReference != null)
		{
			return asSpecializedNestedTypeReference.GetUnspecializedVersion(context);
		}
		return typeReference;
	}

	internal static bool IsTypeSpecification(this ITypeReference typeReference)
	{
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		if (asNestedTypeReference != null)
		{
			if (asNestedTypeReference.AsSpecializedNestedTypeReference == null)
			{
				return asNestedTypeReference.AsGenericTypeInstanceReference != null;
			}
			return true;
		}
		return typeReference.AsNamespaceTypeReference == null;
	}
}
