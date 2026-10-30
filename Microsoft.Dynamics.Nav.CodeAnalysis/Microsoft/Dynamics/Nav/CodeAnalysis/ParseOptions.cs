using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class ParseOptions : IEquatable<ParseOptions>
{
	public Version RuntimeVersion { get; }

	public static ParseOptions Default { get; } = new ParseOptions(Microsoft.Dynamics.Nav.CodeAnalysis.RuntimeVersion.CurrentRelease);

	public ParseOptions(Version runtimeVersion)
	{
		RuntimeVersion = runtimeVersion;
	}

	public ParseOptions WithRuntimeVersion(Version runtimeVersion)
	{
		if (runtimeVersion.Equals(RuntimeVersion))
		{
			return this;
		}
		return new ParseOptions(runtimeVersion);
	}

	internal bool IsFeatureEnabled(Feature feature)
	{
		return RuntimeVersion >= feature.RequiredVersion();
	}

	public override bool Equals(object obj)
	{
		ParseOptions parseOptions = obj as ParseOptions;
		if (parseOptions == null)
		{
			return false;
		}
		return Equals(parseOptions);
	}

	public bool Equals(ParseOptions other)
	{
		if (other == null)
		{
			return false;
		}
		return RuntimeVersion == other.RuntimeVersion;
	}

	public override int GetHashCode()
	{
		if (!(RuntimeVersion == null))
		{
			return RuntimeVersion.GetHashCode();
		}
		return 0;
	}

	public static bool operator ==(ParseOptions first, ParseOptions second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(ParseOptions first, ParseOptions second)
	{
		return !(first == second);
	}
}
