using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct AnonymousTypeKey : IEquatable<AnonymousTypeKey>
{
	internal readonly bool IsDelegate;

	internal readonly ImmutableArray<AnonymousTypeKeyField> Fields;

	internal AnonymousTypeKey(ImmutableArray<AnonymousTypeKeyField> fields, bool isDelegate = false)
	{
		IsDelegate = isDelegate;
		Fields = fields;
	}

	public bool Equals(AnonymousTypeKey other)
	{
		if (IsDelegate == other.IsDelegate)
		{
			return Fields.SequenceEqual(other.Fields);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals((AnonymousTypeKey)obj);
	}

	public override int GetHashCode()
	{
		bool isDelegate = IsDelegate;
		return Hash.Combine(isDelegate.GetHashCode(), Hash.CombineValues(Fields));
	}

	private string GetDebuggerDisplay()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		for (int i = 0; i < Fields.Length; i++)
		{
			if (i > 0)
			{
				builder.Append("|");
			}
			builder.Append(Fields[i]);
		}
		return instance.ToStringAndFree();
	}
}
