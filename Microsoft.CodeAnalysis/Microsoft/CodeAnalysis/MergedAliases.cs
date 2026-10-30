using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class MergedAliases
{
	public ArrayBuilder<string> AliasesOpt;

	public ArrayBuilder<string> RecursiveAliasesOpt;

	internal void Merge(MetadataReference reference)
	{
		if (reference.Properties.HasRecursiveAliases)
		{
			if (RecursiveAliasesOpt == null)
			{
				RecursiveAliasesOpt = ArrayBuilder<string>.GetInstance();
				RecursiveAliasesOpt.AddRange(reference.Properties.Aliases);
				return;
			}
		}
		else if (AliasesOpt == null)
		{
			AliasesOpt = ArrayBuilder<string>.GetInstance();
			AliasesOpt.AddRange(reference.Properties.Aliases);
			return;
		}
		Merge(reference.Properties.HasRecursiveAliases ? RecursiveAliasesOpt : AliasesOpt, reference.Properties.Aliases);
	}

	internal static void Merge(ArrayBuilder<string> aliases, ImmutableArray<string> newAliases)
	{
		if ((aliases.Count == 0) ^ newAliases.IsEmpty)
		{
			AddNonIncluded(aliases, MetadataReferenceProperties.GlobalAlias);
		}
		AddNonIncluded(aliases, newAliases);
	}

	internal static ImmutableArray<string> Merge(ImmutableArray<string> aliasesOpt, ImmutableArray<string> newAliases)
	{
		if (aliasesOpt.IsDefault)
		{
			return newAliases;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(aliasesOpt.Length);
		instance.AddRange(aliasesOpt);
		Merge(instance, newAliases);
		return instance.ToImmutableAndFree();
	}

	private static void AddNonIncluded(ArrayBuilder<string> builder, string item)
	{
		if (!builder.Contains(item))
		{
			builder.Add(item);
		}
	}

	private static void AddNonIncluded(ArrayBuilder<string> builder, ImmutableArray<string> items)
	{
		int count = builder.Count;
		foreach (string item in items)
		{
			if (builder.IndexOf(item, 0, count) < 0)
			{
				builder.Add(item);
			}
		}
	}
}
