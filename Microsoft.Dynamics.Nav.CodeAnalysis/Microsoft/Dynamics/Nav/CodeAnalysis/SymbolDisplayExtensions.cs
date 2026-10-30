using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class SymbolDisplayExtensions
{
	public static string ToDisplayString(this ImmutableArray<SymbolDisplayPart> parts)
	{
		if (parts.IsDefault)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "parts");
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

	internal static bool IncludesOption(this SymbolDisplayMethodOptions Options, SymbolDisplayMethodOptions flag)
	{
		return (Options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayParameterOptions Options, SymbolDisplayParameterOptions flag)
	{
		return (Options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayTypedSymbolOptions Options, SymbolDisplayTypedSymbolOptions flag)
	{
		return (Options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayVariableOptions Options, SymbolDisplayVariableOptions flag)
	{
		return (Options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayReturnValueOptions Options, SymbolDisplayReturnValueOptions flag)
	{
		return (Options & flag) == flag;
	}
}
