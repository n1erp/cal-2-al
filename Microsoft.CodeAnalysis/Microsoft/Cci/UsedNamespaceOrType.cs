using System;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal struct UsedNamespaceOrType : IEquatable<UsedNamespaceOrType>
{
	public readonly string AliasOpt;

	public readonly IAssemblyReference TargetAssemblyOpt;

	public readonly INamespace TargetNamespaceOpt;

	public readonly ITypeReference TargetTypeOpt;

	public readonly string TargetXmlNamespaceOpt;

	private UsedNamespaceOrType(string alias = null, IAssemblyReference targetAssembly = null, INamespace targetNamespace = null, ITypeReference targetType = null, string targetXmlNamespace = null)
	{
		AliasOpt = alias;
		TargetAssemblyOpt = targetAssembly;
		TargetNamespaceOpt = targetNamespace;
		TargetTypeOpt = targetType;
		TargetXmlNamespaceOpt = targetXmlNamespace;
	}

	internal static UsedNamespaceOrType CreateType(ITypeReference type, string aliasOpt = null)
	{
		return new UsedNamespaceOrType(aliasOpt, null, null, type);
	}

	internal static UsedNamespaceOrType CreateNamespace(INamespace @namespace, IAssemblyReference assemblyOpt = null, string aliasOpt = null)
	{
		return new UsedNamespaceOrType(aliasOpt, assemblyOpt, @namespace);
	}

	internal static UsedNamespaceOrType CreateExternAlias(string alias)
	{
		return new UsedNamespaceOrType(alias);
	}

	internal static UsedNamespaceOrType CreateXmlNamespace(string prefix, string xmlNamespace)
	{
		return new UsedNamespaceOrType(prefix, null, null, null, xmlNamespace);
	}

	public override bool Equals(object obj)
	{
		if (obj is UsedNamespaceOrType other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(UsedNamespaceOrType other)
	{
		if (AliasOpt == other.AliasOpt && object.Equals(TargetAssemblyOpt, other.TargetAssemblyOpt) && object.Equals(TargetNamespaceOpt, other.TargetNamespaceOpt) && object.Equals(TargetTypeOpt, other.TargetTypeOpt))
		{
			return TargetXmlNamespaceOpt == other.TargetXmlNamespaceOpt;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(AliasOpt, Hash.Combine((object)TargetAssemblyOpt, Hash.Combine((object)TargetNamespaceOpt, Hash.Combine((object)TargetTypeOpt, Hash.Combine(TargetXmlNamespaceOpt, 0)))));
	}
}
