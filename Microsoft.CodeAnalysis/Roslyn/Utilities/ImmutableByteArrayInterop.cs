using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Roslyn.Utilities;

internal static class ImmutableByteArrayInterop
{
	[StructLayout(LayoutKind.Explicit)]
	private struct ArrayUnion
	{
		[FieldOffset(0)]
		internal byte[] MutableArray;

		[FieldOffset(0)]
		internal ImmutableArray<byte> ImmutableArray;
	}

	internal static byte[] DangerousGetUnderlyingArray(this ImmutableArray<byte> array)
	{
		ArrayUnion arrayUnion = new ArrayUnion
		{
			ImmutableArray = array
		};
		return arrayUnion.MutableArray;
	}

	internal static ImmutableArray<byte> DangerousCreateFromUnderlyingArray(ref byte[] array)
	{
		ArrayUnion arrayUnion = new ArrayUnion
		{
			MutableArray = array
		};
		array = null;
		return arrayUnion.ImmutableArray;
	}
}
