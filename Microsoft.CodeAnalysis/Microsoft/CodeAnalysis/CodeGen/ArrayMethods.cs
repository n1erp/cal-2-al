using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal class ArrayMethods
{
	private enum ArrayMethodKind : byte
	{
		GET,
		SET,
		ADDRESS,
		CTOR
	}

	private sealed class ArrayConstructor : ArrayMethod
	{
		public override string Name => ".ctor";

		public ArrayConstructor(IArrayTypeReference arrayType)
			: base(arrayType)
		{
		}

		public override ITypeReference GetType(EmitContext context)
		{
			return context.Module.GetPlatformType(PlatformType.SystemVoid, context);
		}
	}

	private sealed class ArrayGet : ArrayMethod
	{
		public override string Name => "Get";

		public ArrayGet(IArrayTypeReference arrayType)
			: base(arrayType)
		{
		}

		public override ITypeReference GetType(EmitContext context)
		{
			return arrayType.GetElementType(context);
		}
	}

	private sealed class ArrayAddress : ArrayMethod
	{
		public override bool ReturnValueIsByRef => true;

		public override string Name => "Address";

		public ArrayAddress(IArrayTypeReference arrayType)
			: base(arrayType)
		{
		}

		public override ITypeReference GetType(EmitContext context)
		{
			return arrayType.GetElementType(context);
		}
	}

	private sealed class ArraySet : ArrayMethod
	{
		public override string Name => "Set";

		public ArraySet(IArrayTypeReference arrayType)
			: base(arrayType)
		{
		}

		public override ITypeReference GetType(EmitContext context)
		{
			return context.Module.GetPlatformType(PlatformType.SystemVoid, context);
		}

		protected override ImmutableArray<ArrayMethodParameterInfo> MakeParameters()
		{
			int rank = arrayType.Rank;
			ArrayBuilder<ArrayMethodParameterInfo> instance = ArrayBuilder<ArrayMethodParameterInfo>.GetInstance(rank + 1);
			for (int i = 0; i < rank; i++)
			{
				instance.Add(ArrayMethodParameterInfo.GetIndexParameter((ushort)i));
			}
			instance.Add(new ArraySetValueParameterInfo((ushort)rank, arrayType));
			return instance.ToImmutableAndFree();
		}
	}

	private readonly ConcurrentDictionary<(byte methodKind, IArrayTypeReference arrayType), ArrayMethod> _dict = new ConcurrentDictionary<(byte, IArrayTypeReference), ArrayMethod>();

	public ArrayMethod GetArrayConstructor(IArrayTypeReference arrayType)
	{
		return GetArrayMethod(arrayType, ArrayMethodKind.CTOR);
	}

	public ArrayMethod GetArrayGet(IArrayTypeReference arrayType)
	{
		return GetArrayMethod(arrayType, ArrayMethodKind.GET);
	}

	public ArrayMethod GetArraySet(IArrayTypeReference arrayType)
	{
		return GetArrayMethod(arrayType, ArrayMethodKind.SET);
	}

	public ArrayMethod GetArrayAddress(IArrayTypeReference arrayType)
	{
		return GetArrayMethod(arrayType, ArrayMethodKind.ADDRESS);
	}

	private ArrayMethod GetArrayMethod(IArrayTypeReference arrayType, ArrayMethodKind id)
	{
		(byte, IArrayTypeReference) key = ((byte)id, arrayType);
		ConcurrentDictionary<(byte, IArrayTypeReference), ArrayMethod> dict = _dict;
		if (!dict.TryGetValue(key, out var value))
		{
			value = MakeArrayMethod(arrayType, id);
			return dict.GetOrAdd(key, value);
		}
		return value;
	}

	private static ArrayMethod MakeArrayMethod(IArrayTypeReference arrayType, ArrayMethodKind id)
	{
		return id switch
		{
			ArrayMethodKind.CTOR => (ArrayMethod)new ArrayConstructor(arrayType), 
			ArrayMethodKind.GET => new ArrayGet(arrayType), 
			ArrayMethodKind.SET => new ArraySet(arrayType), 
			ArrayMethodKind.ADDRESS => new ArrayAddress(arrayType), 
			_ => throw ExceptionUtilities.UnexpectedValue(id), 
		};
	}
}
