using System;

namespace Newtonsoft.Json.Utilities;

internal struct TypeNameKey(string assemblyName, string typeName) : IEquatable<TypeNameKey>
{
	internal readonly string AssemblyName = assemblyName;

	internal readonly string TypeName = typeName;

	public override int GetHashCode()
	{
		return (AssemblyName?.GetHashCode() ?? 0) ^ (TypeName?.GetHashCode() ?? 0);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TypeNameKey))
		{
			return false;
		}
		return Equals((TypeNameKey)obj);
	}

	public bool Equals(TypeNameKey other)
	{
		if (AssemblyName == other.AssemblyName)
		{
			return TypeName == other.TypeName;
		}
		return false;
	}
}
