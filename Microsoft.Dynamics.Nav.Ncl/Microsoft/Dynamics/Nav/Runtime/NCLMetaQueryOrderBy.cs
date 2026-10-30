using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQueryOrderBy : INavFieldMetadata, INavValueMetadata, INavSortingField
{
	private readonly SortOrder sortOrder;

	private readonly NCLMetaQueryColumn column;

	private readonly bool extraColumnAutomaticallyAddedForUniqueness;

	public SortOrder Order => sortOrder;

	public INavFieldMetadata Field => column;

	public NCLMetaQueryColumn Column => column;

	public bool ExtraColumnAutomaticallyAddedForUniqueness => extraColumnAutomaticallyAddedForUniqueness;

	public NavType NavType => column.NavType;

	public NavNclType NclType => column.NclType;

	public bool IsStringType => column.IsStringType;

	public int NavDefinedLengthMetadata => column.NavDefinedLengthMetadata;

	public NCLOptionMetadata NavOptionMetadata => column.NavOptionMetadata;

	public int ColumnIndex => column.ColumnIndex;

	public string DescriptiveName => column.DescriptiveName;

	public string DescriptiveParentName => column.DescriptiveParentName;

	public FilterExpressionContext ExpressionContext => column.ExpressionContext;

	public FieldClass FieldClass => column.FieldClass;

	public SqlDataType SqlDataType => column.SqlDataType;

	public NavValue InitValue => column.InitValue;

	public NavValue EmptyValue => column.EmptyValue;

	private NCLMetaQueryOrderBy(SortOrder sortOrder, NCLMetaQueryColumn column, bool extraColumnAutomaticallyAddedForUniqueness)
	{
		this.sortOrder = sortOrder;
		this.column = column;
		this.extraColumnAutomaticallyAddedForUniqueness = extraColumnAutomaticallyAddedForUniqueness;
	}

	internal static NCLMetaQueryOrderBy CreateMetaOrderByFromDesignMetadata(NCLMetaQueryDefinition parentQueryDefinition, MetaQueryOrderBy metaQueryOrderBy)
	{
		NCLMetaQueryColumn nCLMetaQueryColumn = parentQueryDefinition.DefinedColumns.FirstOrDefault((NCLMetaQueryColumn column) => column.Id == metaQueryOrderBy.QueryColumnId);
		return new NCLMetaQueryOrderBy((SortOrder)metaQueryOrderBy.Sorting, nCLMetaQueryColumn, extraColumnAutomaticallyAddedForUniqueness: false);
	}

	internal static NCLMetaQueryOrderBy Create(SortOrder sorting, NCLMetaQueryColumn column, bool extraColumnAutomaticallyAddedForUniqueness)
	{
		return new NCLMetaQueryOrderBy(sorting, column, extraColumnAutomaticallyAddedForUniqueness);
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes(Field.GetHashCode(), (int)sortOrder);
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is NCLMetaQueryOrderBy nCLMetaQueryOrderBy))
		{
			return false;
		}
		if (Order == nCLMetaQueryOrderBy.Order)
		{
			return Field.Equals(nCLMetaQueryOrderBy.Field);
		}
		return false;
	}
}
