using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public struct RuntimeBuildOptions : IEquatable<RuntimeBuildOptions>
{
	public bool IncludeAlCode { get; }

	public RuntimeBuildOptions(bool includeAlCode = false)
	{
		IncludeAlCode = includeAlCode;
	}

	public bool Equals(RuntimeBuildOptions other)
	{
		return IncludeAlCode == other.IncludeAlCode;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is RuntimeBuildOptions other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return IncludeAlCode.GetHashCode();
	}

	public static bool operator ==(RuntimeBuildOptions a, RuntimeBuildOptions b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(RuntimeBuildOptions a, RuntimeBuildOptions b)
	{
		return !(a == b);
	}
}
