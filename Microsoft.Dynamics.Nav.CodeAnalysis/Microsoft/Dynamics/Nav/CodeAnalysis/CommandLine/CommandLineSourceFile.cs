using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal struct CommandLineSourceFile
{
	public string Path { get; }

	internal CommandLineSourceFile(string path)
	{
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(path));
		Path = path;
	}

	public override bool Equals(object obj)
	{
		if (obj is CommandLineSourceFile)
		{
			return base.Equals((CommandLineSourceFile)obj);
		}
		return false;
	}

	public bool Equals(CommandLineSourceFile other)
	{
		return string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase) == 0;
	}

	public static bool operator ==(CommandLineSourceFile left, CommandLineSourceFile right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(CommandLineSourceFile left, CommandLineSourceFile right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Path.GetHashCode(), 0);
	}
}
