using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TablePositionResolver : TableFilterResolver
{
	protected override string Description => Lang.PositionExpression;

	public TablePositionResolver(NCLMetaTable table, string input)
		: base(table, input)
	{
	}

	protected override void ValidateFilterType(NCLMetaFilterType filterType)
	{
		if (filterType != NCLMetaFilterType.Const)
		{
			throw GetInvalidExpressionException();
		}
	}
}
