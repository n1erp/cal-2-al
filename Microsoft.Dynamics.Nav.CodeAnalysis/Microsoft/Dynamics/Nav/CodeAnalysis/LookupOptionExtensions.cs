using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class LookupOptionExtensions
{
	internal static bool AreValid(this LookupOptions options)
	{
		if (options == LookupOptions.Default)
		{
			return true;
		}
		LookupOptions lookupOptions = LookupOptions.MustBeInstance | LookupOptions.MustNotBeInstance;
		if ((options & lookupOptions) == lookupOptions)
		{
			return false;
		}
		return OnlyOneBitSet(options & LookupOptions.AllMethods);
	}

	internal static void ThrowIfInvalid(this LookupOptions options)
	{
		if (!options.AreValid())
		{
			throw new ArgumentException("CSharpResources.LookupOptionsHasInvalidCombo");
		}
	}

	private static bool OnlyOneBitSet(LookupOptions o)
	{
		return (o & (o - 1)) == 0;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "options")]
	internal static bool CanConsiderMembers(this LookupOptions options)
	{
		return true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "options")]
	internal static bool CanConsiderLocals(this LookupOptions options)
	{
		return true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "options")]
	internal static bool CanConsiderGlobals(this LookupOptions options)
	{
		return true;
	}

	internal static bool IsMemberAccess(this LookupOptions options)
	{
		return (options & LookupOptions.MemberAccess) != 0;
	}
}
