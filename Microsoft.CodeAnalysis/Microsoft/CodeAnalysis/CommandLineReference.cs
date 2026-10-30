using System;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{Reference,nq}")]
public struct CommandLineReference(string reference, MetadataReferenceProperties properties) : IEquatable<CommandLineReference>
{
	private readonly string _reference = reference;

	private readonly MetadataReferenceProperties _properties = properties;

	public string Reference => _reference;

	public MetadataReferenceProperties Properties => _properties;

	public override bool Equals(object obj)
	{
		if (obj is CommandLineReference)
		{
			return base.Equals((CommandLineReference)obj);
		}
		return false;
	}

	public bool Equals(CommandLineReference other)
	{
		if (_reference == other._reference)
		{
			return _properties.Equals(other._properties);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_reference, _properties.GetHashCode());
	}
}
