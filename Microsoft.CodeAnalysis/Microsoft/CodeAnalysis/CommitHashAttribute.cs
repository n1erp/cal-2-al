using System;

namespace Microsoft.CodeAnalysis;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
internal sealed class CommitHashAttribute : Attribute
{
	internal readonly string Hash;

	public CommitHashAttribute(string hash)
	{
		Hash = hash;
	}
}
