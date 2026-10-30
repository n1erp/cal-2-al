using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

public struct CommandLineAnalyzerReference(string path) : IEquatable<CommandLineAnalyzerReference>
{
	public string FilePath { get; } = path;

	public override bool Equals(object obj)
	{
		if (obj is CommandLineAnalyzerReference)
		{
			return base.Equals((CommandLineAnalyzerReference)obj);
		}
		return false;
	}

	public bool Equals(CommandLineAnalyzerReference other)
	{
		return string.Compare(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase) == 0;
	}

	public static bool operator ==(CommandLineAnalyzerReference left, CommandLineAnalyzerReference right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(CommandLineAnalyzerReference left, CommandLineAnalyzerReference right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(FilePath.GetHashCode(), 0);
	}
}
