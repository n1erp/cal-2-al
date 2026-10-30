using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PositionedFindCacheRequest : FindCacheRequest
{
	private readonly MutableRecordBuffer startingPosition;

	private readonly bool includeCurrent;

	public MutableRecordBuffer StartingPosition
	{
		[DebuggerStepThrough]
		get
		{
			return startingPosition;
		}
	}

	public bool IncludeCurrent
	{
		[DebuggerStepThrough]
		get
		{
			return includeCurrent;
		}
	}

	public override bool IsFromPosition
	{
		[DebuggerStepThrough]
		get
		{
			return true;
		}
	}

	public PositionedFindCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, MutableRecordBuffer startingPosition, bool includeCurrent, int topNumberOfRows = 0, int skipNumberOfRows = 0, int? timeout = null)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering, autoCalcFields, sortingFields, findType, topNumberOfRows, skipNumberOfRows, timeout)
	{
		this.startingPosition = startingPosition;
		this.includeCurrent = includeCurrent;
	}

	protected override int CalculateHashCode()
	{
		int num = 0;
		if (startingPosition != null && SortingFields != null)
		{
			for (int i = 0; i < SortingFields.Count; i++)
			{
				int h = StartingPosition[SortingFields[i].Field.ColumnIndex]?.GetHashCode() ?? int.MaxValue;
				num = HashCodeHelper.CombineHashCodes(num, h);
			}
		}
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), num, IncludeCurrent.GetHashCode());
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (!base.Equals(other))
		{
			return false;
		}
		PositionedFindCacheRequest positionedFindCacheRequest = other as PositionedFindCacheRequest;
		bool flag = positionedFindCacheRequest != null && StartingPosition == null == (positionedFindCacheRequest.StartingPosition == null) && IncludeCurrent == positionedFindCacheRequest.IncludeCurrent;
		if (flag && startingPosition != null)
		{
			for (int i = 0; i < SortingFields.Count; i++)
			{
				int columnIndex = SortingFields[i].Field.ColumnIndex;
				NavValue objA = StartingPosition[columnIndex];
				NavValue objB = positionedFindCacheRequest.StartingPosition[columnIndex];
				if (!object.Equals(objA, objB))
				{
					return false;
				}
			}
		}
		return flag;
	}
}
