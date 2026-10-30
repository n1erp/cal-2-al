using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal class ArrayMethodParameterInfo : IParameterTypeInformation, IParameterListEntry
{
	private readonly ushort _index;

	private static readonly ArrayMethodParameterInfo s_index0 = new ArrayMethodParameterInfo(0);

	private static readonly ArrayMethodParameterInfo s_index1 = new ArrayMethodParameterInfo(1);

	private static readonly ArrayMethodParameterInfo s_index2 = new ArrayMethodParameterInfo(2);

	private static readonly ArrayMethodParameterInfo s_index3 = new ArrayMethodParameterInfo(3);

	public ImmutableArray<ICustomModifier> RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public ImmutableArray<ICustomModifier> CustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public bool IsByReference => false;

	public ushort Index => _index;

	protected ArrayMethodParameterInfo(ushort index)
	{
		_index = index;
	}

	public static ArrayMethodParameterInfo GetIndexParameter(ushort index)
	{
		return index switch
		{
			0 => s_index0, 
			1 => s_index1, 
			2 => s_index2, 
			3 => s_index3, 
			_ => new ArrayMethodParameterInfo(index), 
		};
	}

	public virtual ITypeReference GetType(EmitContext context)
	{
		return context.Module.GetPlatformType(PlatformType.SystemInt32, context);
	}
}
