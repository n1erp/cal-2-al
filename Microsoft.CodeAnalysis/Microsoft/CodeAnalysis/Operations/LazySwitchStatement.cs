using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazySwitchStatement : BaseSwitchStatement, ISwitchOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	private readonly Lazy<ImmutableArray<ISwitchCaseOperation>> _lazyCases;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public override ImmutableArray<ISwitchCaseOperation> Cases => Operation.SetParentOperation(_lazyCases.Value, this);

	public LazySwitchStatement(ImmutableArray<ILocalSymbol> locals, Lazy<IOperation> value, Lazy<ImmutableArray<ISwitchCaseOperation>> cases, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
		_lazyCases = cases;
	}
}
