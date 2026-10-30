using System;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class MetadataHashTuple : IEquatable<MetadataHashTuple>
{
	internal NavText MetadataHash { get; }

	internal int SchemaHash { get; }

	private MetadataHashTuple(NavText metadataHash, int schemaHash)
	{
		MetadataHash = metadataHash ?? NavText.Empty;
		SchemaHash = schemaHash;
	}

	public static MetadataHashTuple Create(NavText metadataHash, int schemaHash)
	{
		return new MetadataHashTuple(metadataHash, schemaHash);
	}

	public static MetadataHashTuple Create(NavText metadataHash)
	{
		return new MetadataHashTuple(metadataHash, 0);
	}

	public static MetadataHashTuple Create(string metadataHash, int schemaHash)
	{
		return new MetadataHashTuple(NavText.Create(metadataHash), schemaHash);
	}

	public bool Equals(MetadataHashTuple other)
	{
		if (other == null)
		{
			return false;
		}
		if (SchemaHash == 0 || other.SchemaHash == 0)
		{
			return string.Equals(MetadataHash, other.MetadataHash, StringComparison.Ordinal);
		}
		return SchemaHash == other.SchemaHash;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as MetadataHashTuple);
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes(MetadataHash.GetHashCode(), SchemaHash);
	}

	public static bool operator ==(MetadataHashTuple objA, MetadataHashTuple objB)
	{
		return object.Equals(objA, objB);
	}

	public static bool operator !=(MetadataHashTuple objA, MetadataHashTuple objB)
	{
		return !object.Equals(objA, objB);
	}
}
