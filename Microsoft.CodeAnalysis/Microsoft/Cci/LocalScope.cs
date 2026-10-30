using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Microsoft.Cci;

internal struct LocalScope
{
	public readonly int StartOffset;

	public readonly int EndOffset;

	private readonly ImmutableArray<ILocalDefinition> _constants;

	private readonly ImmutableArray<ILocalDefinition> _locals;

	public int Length => EndOffset - StartOffset;

	public ImmutableArray<ILocalDefinition> Constants => _constants.NullToEmpty();

	public ImmutableArray<ILocalDefinition> Variables => _locals.NullToEmpty();

	internal LocalScope(int offset, int endOffset, ImmutableArray<ILocalDefinition> constants, ImmutableArray<ILocalDefinition> locals)
	{
		StartOffset = offset;
		EndOffset = endOffset;
		_constants = constants;
		_locals = locals;
	}
}
