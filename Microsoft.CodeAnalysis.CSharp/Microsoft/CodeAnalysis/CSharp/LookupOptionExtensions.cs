using System;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class LookupOptionExtensions
{
	internal static bool AreValid(this LookupOptions options)
	{
		if (options == LookupOptions.Default)
		{
			return true;
		}
		if ((options & LookupOptions.LabelsOnly) != 0)
		{
			return options == LookupOptions.LabelsOnly;
		}
		LookupOptions lookupOptions = LookupOptions.MustBeInstance | LookupOptions.MustNotBeInstance;
		if ((options & lookupOptions) == lookupOptions)
		{
			return false;
		}
		if ((options & (LookupOptions.MustNotBeNamespace | LookupOptions.MustNotBeMethodTypeParameter)) != 0 && (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) != 0)
		{
			return false;
		}
		return OnlyOneBitSet(options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly | LookupOptions.AllMethodsOnArityZero));
	}

	internal static void ThrowIfInvalid(this LookupOptions options)
	{
		if (!options.AreValid())
		{
			throw new ArgumentException(CSharpResources.LookupOptionsHasInvalidCombo);
		}
	}

	private static bool OnlyOneBitSet(LookupOptions o)
	{
		return (o & (o - 1)) == 0;
	}

	internal static bool CanConsiderMembers(this LookupOptions options)
	{
		return (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly | LookupOptions.LabelsOnly)) == 0;
	}

	internal static bool CanConsiderLocals(this LookupOptions options)
	{
		return (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly | LookupOptions.LabelsOnly)) == 0;
	}

	internal static bool CanConsiderTypes(this LookupOptions options)
	{
		return (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.MustBeInvocableIfMember | LookupOptions.MustBeInstance | LookupOptions.LabelsOnly)) == 0;
	}

	internal static bool CanConsiderNamespaces(this LookupOptions options)
	{
		return (options & (LookupOptions.MustBeInvocableIfMember | LookupOptions.MustBeInstance | LookupOptions.MustNotBeNamespace | LookupOptions.LabelsOnly)) == 0;
	}

	internal static bool IsAttributeTypeLookup(this LookupOptions options)
	{
		return (options & LookupOptions.AttributeTypeOnly) == LookupOptions.AttributeTypeOnly;
	}

	internal static bool IsVerbatimNameAttributeTypeLookup(this LookupOptions options)
	{
		return (options & LookupOptions.VerbatimNameAttributeTypeOnly) == LookupOptions.VerbatimNameAttributeTypeOnly;
	}
}
