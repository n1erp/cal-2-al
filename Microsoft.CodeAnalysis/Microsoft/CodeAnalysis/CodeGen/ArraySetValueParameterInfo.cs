using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ArraySetValueParameterInfo : ArrayMethodParameterInfo
{
	private readonly IArrayTypeReference _arrayType;

	internal ArraySetValueParameterInfo(ushort index, IArrayTypeReference arrayType)
		: base(index)
	{
		_arrayType = arrayType;
	}

	public override ITypeReference GetType(EmitContext context)
	{
		return _arrayType.GetElementType(context);
	}
}
