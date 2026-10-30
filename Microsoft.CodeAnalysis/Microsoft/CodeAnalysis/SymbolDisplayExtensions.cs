using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

public static class SymbolDisplayExtensions
{
	public static string ToDisplayString(this ImmutableArray<SymbolDisplayPart> parts)
	{
		if (parts.IsDefault)
		{
			throw new ArgumentException("parts");
		}
		if (parts.Length == 0)
		{
			return string.Empty;
		}
		if (parts.Length == 1)
		{
			return parts[0].ToString();
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			foreach (SymbolDisplayPart item in parts)
			{
				builder.Append(item);
			}
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IncludesOption(this SymbolDisplayCompilerInternalOptions options, SymbolDisplayCompilerInternalOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayGenericsOptions options, SymbolDisplayGenericsOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayMemberOptions options, SymbolDisplayMemberOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayMiscellaneousOptions options, SymbolDisplayMiscellaneousOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayParameterOptions options, SymbolDisplayParameterOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayKindOptions options, SymbolDisplayKindOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayLocalOptions options, SymbolDisplayLocalOptions flag)
	{
		return (options & flag) == flag;
	}
}
