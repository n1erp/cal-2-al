using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis;

public static class ISymbolExtensions
{
	public static IMethodSymbol GetConstructedReducedFrom(this IMethodSymbol method)
	{
		if (method.MethodKind != MethodKind.ReducedExtension)
		{
			return null;
		}
		IMethodSymbol reducedFrom = method.ReducedFrom;
		if (!reducedFrom.IsGenericMethod)
		{
			return reducedFrom;
		}
		ITypeSymbol[] array = new ITypeSymbol[reducedFrom.TypeParameters.Length];
		int i = 0;
		for (int length = method.TypeParameters.Length; i < length; i++)
		{
			ITypeSymbol typeSymbol = method.TypeArguments[i];
			if (typeSymbol.Equals(method.TypeParameters[i]))
			{
				typeSymbol = method.TypeParameters[i].ReducedFrom;
			}
			array[method.TypeParameters[i].ReducedFrom.Ordinal] = typeSymbol;
		}
		int j = 0;
		for (int length2 = reducedFrom.TypeParameters.Length; j < length2; j++)
		{
			ITypeSymbol typeInferredDuringReduction = method.GetTypeInferredDuringReduction(reducedFrom.TypeParameters[j]);
			if (typeInferredDuringReduction != null)
			{
				array[j] = typeInferredDuringReduction;
			}
		}
		return reducedFrom.Construct(array);
	}

	internal static bool IsDefaultTupleElement(this IFieldSymbol field)
	{
		return field == field.CorrespondingTupleField;
	}

	internal static bool IsTupleElement(this IFieldSymbol field)
	{
		return field.CorrespondingTupleField != null;
	}

	internal static string ProvidedTupleElementNameOrNull(this IFieldSymbol field)
	{
		if (!field.IsTupleElement() || field.IsImplicitlyDeclared)
		{
			return null;
		}
		return field.Name;
	}

	internal static INamespaceSymbol GetNestedNamespace(this INamespaceSymbol container, string name)
	{
		foreach (INamespaceOrTypeSymbol member in container.GetMembers(name))
		{
			if (member.Kind == SymbolKind.Namespace)
			{
				return (INamespaceSymbol)member;
			}
		}
		return null;
	}

	internal static bool IsNetModule(this IAssemblySymbol assembly)
	{
		if (assembly is ISourceAssemblySymbol sourceAssemblySymbol)
		{
			return sourceAssemblySymbol.Compilation.Options.OutputKind.IsNetModule();
		}
		return false;
	}

	internal static IVTConclusion PerformIVTCheck(this AssemblyIdentity assemblyGrantingAccessIdentity, ImmutableArray<byte> assemblyWantingAccessKey, ImmutableArray<byte> grantedToPublicKey)
	{
		bool isStrongName = assemblyGrantingAccessIdentity.IsStrongName;
		bool flag = !grantedToPublicKey.IsDefaultOrEmpty;
		bool flag2 = !assemblyWantingAccessKey.IsDefaultOrEmpty;
		bool flag3 = (flag & flag2) && ByteSequenceComparer.Equals(grantedToPublicKey, assemblyWantingAccessKey);
		if (flag && !flag3)
		{
			return IVTConclusion.PublicKeyDoesntMatch;
		}
		if (!isStrongName & flag2)
		{
			return IVTConclusion.OneSignedOneNot;
		}
		return IVTConclusion.Match;
	}
}
