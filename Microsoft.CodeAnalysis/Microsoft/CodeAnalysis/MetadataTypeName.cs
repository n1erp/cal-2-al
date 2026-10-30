using System;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal struct MetadataTypeName
{
	public struct Key : IEquatable<Key>
	{
		private readonly string _namespaceOrFullyQualifiedName;

		private readonly string _typeName;

		private readonly byte _useCLSCompliantNameArityEncoding;

		private readonly short _forcedArity;

		private bool HasFullyQualifiedName => _typeName == null;

		internal Key(MetadataTypeName mdTypeName)
		{
			if (mdTypeName.IsNull)
			{
				_namespaceOrFullyQualifiedName = null;
				_typeName = null;
				_useCLSCompliantNameArityEncoding = 0;
				_forcedArity = 0;
				return;
			}
			if (mdTypeName._fullName != null)
			{
				_namespaceOrFullyQualifiedName = mdTypeName._fullName;
				_typeName = null;
			}
			else
			{
				_namespaceOrFullyQualifiedName = mdTypeName._namespaceName;
				_typeName = mdTypeName._typeName;
			}
			_useCLSCompliantNameArityEncoding = (byte)(mdTypeName.UseCLSCompliantNameArityEncoding ? 1 : 0);
			_forcedArity = mdTypeName._forcedArity;
		}

		public bool Equals(Key other)
		{
			if (_useCLSCompliantNameArityEncoding == other._useCLSCompliantNameArityEncoding && _forcedArity == other._forcedArity)
			{
				return EqualNames(ref other);
			}
			return false;
		}

		private bool EqualNames(ref Key other)
		{
			if (_typeName == other._typeName)
			{
				return _namespaceOrFullyQualifiedName == other._namespaceOrFullyQualifiedName;
			}
			if (HasFullyQualifiedName)
			{
				return MetadataHelpers.SplitNameEqualsFullyQualifiedName(other._namespaceOrFullyQualifiedName, other._typeName, _namespaceOrFullyQualifiedName);
			}
			if (other.HasFullyQualifiedName)
			{
				return MetadataHelpers.SplitNameEqualsFullyQualifiedName(_namespaceOrFullyQualifiedName, _typeName, other._namespaceOrFullyQualifiedName);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Key)
			{
				return Equals((Key)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(GetHashCodeName(), Hash.Combine(_useCLSCompliantNameArityEncoding != 0, _forcedArity));
		}

		private int GetHashCodeName()
		{
			int num = Hash.GetFNVHashCode(_namespaceOrFullyQualifiedName);
			if (!HasFullyQualifiedName)
			{
				num = Hash.CombineFNVHash(num, '.');
				num = Hash.CombineFNVHash(num, _typeName);
			}
			return num;
		}
	}

	private string _fullName;

	private string _namespaceName;

	private string _typeName;

	private string _unmangledTypeName;

	private short _inferredArity;

	private short _forcedArity;

	private bool _useCLSCompliantNameArityEncoding;

	private ImmutableArray<string> _namespaceSegments;

	public string FullName
	{
		get
		{
			if (_fullName == null)
			{
				_fullName = MetadataHelpers.BuildQualifiedName(_namespaceName, _typeName);
			}
			return _fullName;
		}
	}

	public string NamespaceName
	{
		get
		{
			if (_namespaceName == null)
			{
				_typeName = MetadataHelpers.SplitQualifiedName(_fullName, out _namespaceName);
			}
			return _namespaceName;
		}
	}

	public string TypeName
	{
		get
		{
			if (_typeName == null)
			{
				_typeName = MetadataHelpers.SplitQualifiedName(_fullName, out _namespaceName);
			}
			return _typeName;
		}
	}

	public string UnmangledTypeName
	{
		get
		{
			if (_unmangledTypeName == null)
			{
				_unmangledTypeName = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(TypeName, out _inferredArity);
			}
			return _unmangledTypeName;
		}
	}

	public int InferredArity
	{
		get
		{
			if (_inferredArity == -1)
			{
				_unmangledTypeName = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(TypeName, out _inferredArity);
			}
			return _inferredArity;
		}
	}

	public bool IsMangled => InferredArity > 0;

	public bool UseCLSCompliantNameArityEncoding => _useCLSCompliantNameArityEncoding;

	public int ForcedArity => _forcedArity;

	public ImmutableArray<string> NamespaceSegments
	{
		get
		{
			if (_namespaceSegments.IsDefault)
			{
				_namespaceSegments = MetadataHelpers.SplitQualifiedName(NamespaceName);
			}
			return _namespaceSegments;
		}
	}

	public bool IsNull
	{
		get
		{
			if (_typeName == null)
			{
				return _fullName == null;
			}
			return false;
		}
	}

	public static MetadataTypeName FromFullName(string fullName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default;
		result._fullName = fullName;
		result._namespaceName = null;
		result._typeName = null;
		result._unmangledTypeName = null;
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = default;
		return result;
	}

	public static MetadataTypeName FromNamespaceAndTypeName(string namespaceName, string typeName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default;
		result._fullName = null;
		result._namespaceName = namespaceName;
		result._typeName = typeName;
		result._unmangledTypeName = null;
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = default;
		return result;
	}

	public static MetadataTypeName FromTypeName(string typeName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default;
		result._fullName = typeName;
		result._namespaceName = string.Empty;
		result._typeName = typeName;
		result._unmangledTypeName = null;
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = ImmutableArray<string>.Empty;
		return result;
	}

	public override string ToString()
	{
		if (IsNull)
		{
			return "{Null}";
		}
		return $"{{{NamespaceName},{TypeName},{UseCLSCompliantNameArityEncoding.ToString()},{_forcedArity.ToString()}}}";
	}

	public Key ToKey()
	{
		return new Key(this);
	}
}
