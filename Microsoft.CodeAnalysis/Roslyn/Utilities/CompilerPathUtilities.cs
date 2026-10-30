using System;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class CompilerPathUtilities
{
	internal static void RequireAbsolutePath(string path, string argumentName)
	{
		if (path == null)
		{
			throw new ArgumentNullException(argumentName);
		}
		if (!PathUtilities.IsAbsolute(path))
		{
			throw new ArgumentException(CodeAnalysisResources.AbsolutePathExpected, argumentName);
		}
	}
}
