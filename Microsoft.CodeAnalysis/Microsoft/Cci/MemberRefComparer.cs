using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal sealed class MemberRefComparer : IEqualityComparer<ITypeMemberReference>
{
	private readonly MetadataWriter _metadataWriter;

	internal MemberRefComparer(MetadataWriter metadataWriter)
	{
		_metadataWriter = metadataWriter;
	}

	public bool Equals(ITypeMemberReference x, ITypeMemberReference y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.GetContainingType(_metadataWriter.Context) != y.GetContainingType(_metadataWriter.Context) && _metadataWriter.GetMemberReferenceParent(x) != _metadataWriter.GetMemberReferenceParent(y))
		{
			return false;
		}
		if (x.Name != y.Name)
		{
			return false;
		}
		IFieldReference fieldReference = x as IFieldReference;
		IFieldReference fieldReference2 = y as IFieldReference;
		if (fieldReference != null && fieldReference2 != null)
		{
			return _metadataWriter.GetFieldSignatureIndex(fieldReference) == _metadataWriter.GetFieldSignatureIndex(fieldReference2);
		}
		IMethodReference methodReference = x as IMethodReference;
		IMethodReference methodReference2 = y as IMethodReference;
		if (methodReference != null && methodReference2 != null)
		{
			return _metadataWriter.GetMethodSignatureHandle(methodReference) == _metadataWriter.GetMethodSignatureHandle(methodReference2);
		}
		return false;
	}

	public int GetHashCode(ITypeMemberReference memberRef)
	{
		int num = Hash.Combine(memberRef.Name, _metadataWriter.GetMemberReferenceParent(memberRef).GetHashCode());
		if (memberRef is IFieldReference fieldReference)
		{
			num = Hash.Combine(num, _metadataWriter.GetFieldSignatureIndex(fieldReference).GetHashCode());
		}
		else if (memberRef is IMethodReference methodReference)
		{
			num = Hash.Combine(num, _metadataWriter.GetMethodSignatureHandle(methodReference).GetHashCode());
		}
		return num;
	}
}
