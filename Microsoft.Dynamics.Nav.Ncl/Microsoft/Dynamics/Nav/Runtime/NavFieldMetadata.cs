using System;
using System.Globalization;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFieldMetadata : INavFieldMetadata, INavValueMetadata
{
	private readonly NavValueMetadata valueMetadata;

	private readonly SqlDataType sqlDataType;

	private NavValue emptyValue;

	public int NavDefinedLengthMetadata => valueMetadata.NavDefinedLengthMetadata;

	public virtual NCLOptionMetadata NavOptionMetadata => valueMetadata.NavOptionMetadata;

	public NavNclType NclType => valueMetadata.NclType;

	public NavType NavType => valueMetadata.NavType;

	public int ColumnIndex => 0;

	public string DescriptiveName
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public string DescriptiveParentName
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public virtual FilterExpressionContext ExpressionContext
	{
		get
		{
			switch (valueMetadata.NclType)
			{
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavText:
			case NavNclType.NavCode:
				return FilterExpressionContext.CreateForTextField(this, CultureInfo.InvariantCulture, CompareOptions.Ordinal, canBeOptimized: false);
			default:
				return FilterExpressionContext.CreateForNonTextField(this);
			}
		}
	}

	public FieldClass FieldClass => FieldClass.Normal;

	public SqlDataType SqlDataType => sqlDataType;

	public NavValue InitValue => EmptyValue;

	public NavValue EmptyValue
	{
		get
		{
			if (NclType == NavNclType.NavBlob)
			{
				return NavBLOB.Default();
			}
			return emptyValue ?? (emptyValue = NavValue.GetDefaultNavValue(this));
		}
	}

	internal static NavFieldMetadata DefaultMetadata(NavValueMetadata defaultValueMetadata, SqlDataType sqlDataType = SqlDataType.Default)
	{
		return new NavFieldMetadata(defaultValueMetadata, sqlDataType);
	}

	protected NavFieldMetadata(NavValueMetadata valueMetadata, SqlDataType sqlDataType)
	{
		this.valueMetadata = valueMetadata;
		this.sqlDataType = sqlDataType;
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes((int)sqlDataType, valueMetadata.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is NavFieldMetadata navFieldMetadata))
		{
			return false;
		}
		if (valueMetadata.Equals(navFieldMetadata.valueMetadata))
		{
			return sqlDataType == navFieldMetadata.sqlDataType;
		}
		return false;
	}
}
