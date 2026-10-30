using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.System.Collections.Immutable;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	private static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(ResourceType));

	internal static Type ResourceType { get; } = typeof(FxResources.System.Collections.Immutable.SR);

	internal static string Arg_KeyNotFoundWithKey => GetResourceString("Arg_KeyNotFoundWithKey", null);

	internal static string ArrayInitializedStateNotEqual => GetResourceString("ArrayInitializedStateNotEqual", null);

	internal static string ArrayLengthsNotEqual => GetResourceString("ArrayLengthsNotEqual", null);

	internal static string CannotFindOldValue => GetResourceString("CannotFindOldValue", null);

	internal static string CapacityMustBeGreaterThanOrEqualToCount => GetResourceString("CapacityMustBeGreaterThanOrEqualToCount", null);

	internal static string CapacityMustEqualCountOnMove => GetResourceString("CapacityMustEqualCountOnMove", null);

	internal static string CollectionModifiedDuringEnumeration => GetResourceString("CollectionModifiedDuringEnumeration", null);

	internal static string DuplicateKey => GetResourceString("DuplicateKey", null);

	internal static string InvalidEmptyOperation => GetResourceString("InvalidEmptyOperation", null);

	internal static string InvalidOperationOnDefaultArray => GetResourceString("InvalidOperationOnDefaultArray", null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, new object[1] { p1 });
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, new object[2] { p1, p2 });
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, new object[3] { p1, p2, p3 });
	}
}
