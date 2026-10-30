using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavFieldMetadata : INavValueMetadata
{
	int ColumnIndex { get; }

	FilterExpressionContext ExpressionContext { get; }

	FieldClass FieldClass { get; }

	SqlDataType SqlDataType { get; }

	NavValue InitValue { get; }

	NavValue EmptyValue { get; }

	string DescriptiveName { get; }

	string DescriptiveParentName { get; }
}
