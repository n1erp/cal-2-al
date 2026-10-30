using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Roslyn.Utilities;

internal static class ImmutableInt32ArrayInterop
{
	[StructLayout(LayoutKind.Explicit)]
	private struct ArrayUnion
	{
		[FieldOffset(0)]
		internal int[] MutableArray;

		[FieldOffset(0)]
		internal ImmutableArray<int> ImmutableArray;
	}

	internal static int[] DangerousGetUnderlyingArray(this ImmutableArray<int> array)
	{
		ArrayUnion arrayUnion = new ArrayUnion
		{
			ImmutableArray = array
		};
		return arrayUnion.MutableArray;
	}

	internal static ImmutableArray<int> DangerousCreateFromUnderlyingArray(ref int[] array)
	{
		ArrayUnion arrayUnion = new ArrayUnion
		{
			MutableArray = array
		};
		array = null;
		return arrayUnion.ImmutableArray;
	}
}
