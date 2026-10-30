using System.Collections.Immutable;

namespace System.Reflection.Metadata.Ecma335;

public readonly struct ArrayShapeEncoder
{
	public BlobBuilder Builder { get; }

	public ArrayShapeEncoder(BlobBuilder builder)
	{
		Builder = builder;
	}

	public void Shape(int rank, ImmutableArray<int> sizes, ImmutableArray<int> lowerBounds)
	{
		if ((uint)(rank - 1) > 65534u)
		{
			Throw.ArgumentOutOfRange("rank");
		}
		if (sizes.IsDefault)
		{
			Throw.ArgumentNull("sizes");
		}
		Builder.WriteCompressedInteger(rank);
		if (sizes.Length > rank)
		{
			Throw.ArgumentOutOfRange("rank");
		}
		Builder.WriteCompressedInteger(sizes.Length);
		foreach (int item in sizes)
		{
			Builder.WriteCompressedInteger(item);
		}
		if (lowerBounds.IsDefault)
		{
			Builder.WriteCompressedInteger(rank);
			for (int i = 0; i < rank; i++)
			{
				Builder.WriteCompressedSignedInteger(0);
			}
			return;
		}
		if (lowerBounds.Length > rank)
		{
			Throw.ArgumentOutOfRange("rank");
		}
		Builder.WriteCompressedInteger(lowerBounds.Length);
		foreach (int item2 in lowerBounds)
		{
			Builder.WriteCompressedSignedInteger(item2);
		}
	}
}
