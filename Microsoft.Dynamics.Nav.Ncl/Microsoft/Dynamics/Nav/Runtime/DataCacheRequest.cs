using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataCacheRequest : ISizableObject, IEquatable<DataCacheRequest>
{
	internal class NonInheritedEqualityComparer : IEqualityComparer<DataCacheRequest>
	{
		public static readonly NonInheritedEqualityComparer Instance = new NonInheritedEqualityComparer();

		private NonInheritedEqualityComparer()
		{
		}

		public int GetHashCode(DataCacheRequest request)
		{
			return request.NonInheritedGetHashCode();
		}

		public bool Equals(DataCacheRequest left, DataCacheRequest right)
		{
			return left?.NonInheritedEquals(right) ?? (right == null);
		}
	}

	private readonly NCLMetaApplicationObject metaApplicationObject;

	private readonly int companyToken;

	private readonly bool cacheable;

	private readonly FiltersAndMarks filtersAndMarks;

	private readonly TableFilterDictionary globalAndSecurityFilters;

	private readonly SecurityFiltering flowFieldSecurityFiltering;

	private bool nonInheritedHashCodeCalculated;

	private int nonInheritedHashCode;

	private bool hashCodeCalculated;

	private int hashCode;

	public int ApproximateByteSize => SizableObject.GetApproximateByteSize(filtersAndMarks) + SizableObject.GetApproximateByteSize(9) + 24;

	public NCLMetaApplicationObject MetaApplicationObject
	{
		[DebuggerStepThrough]
		get
		{
			return metaApplicationObject;
		}
	}

	public int CompanyToken
	{
		[DebuggerStepThrough]
		get
		{
			return companyToken;
		}
	}

	public bool Cacheable
	{
		[DebuggerStepThrough]
		get
		{
			return cacheable;
		}
	}

	public FiltersAndMarks FiltersAndMarks
	{
		[DebuggerStepThrough]
		get
		{
			return filtersAndMarks;
		}
	}

	public TableFilterDictionary GlobalAndSecurityFilters
	{
		[DebuggerStepThrough]
		get
		{
			return globalAndSecurityFilters;
		}
	}

	public SecurityFiltering FlowFieldSecurityFiltering
	{
		[DebuggerStepThrough]
		get
		{
			return flowFieldSecurityFiltering;
		}
	}

	public virtual bool ResultCanBeFoundInPrivateNonTransactionalCache => false;

	public virtual bool ResultCanBeStoredInPrivateNonTransactionalCache => ResultCanBeFoundInPrivateNonTransactionalCache;

	public DataCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering)
	{
		if (metaApplicationObject == null)
		{
			throw new ArgumentNullException("metaApplicationObject");
		}
		if (filtersAndMarks == null)
		{
			throw new ArgumentNullException("filtersAndMarks");
		}
		this.metaApplicationObject = metaApplicationObject;
		this.companyToken = companyToken;
		this.cacheable = cacheable;
		this.filtersAndMarks = filtersAndMarks;
		this.globalAndSecurityFilters = globalAndSecurityFilters;
		this.flowFieldSecurityFiltering = flowFieldSecurityFiltering;
	}

	public int NonInheritedGetHashCode()
	{
		if (!nonInheritedHashCodeCalculated)
		{
			nonInheritedHashCode = filtersAndMarks.GetHashCode();
			int h = ((globalAndSecurityFilters != null) ? globalAndSecurityFilters.GetHashCode() : int.MaxValue);
			nonInheritedHashCode = HashCodeHelper.CombineHashCodes(nonInheritedHashCode, h);
			nonInheritedHashCodeCalculated = true;
		}
		return nonInheritedHashCode;
	}

	public sealed override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			hashCode = CalculateHashCode();
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	protected virtual int CalculateHashCode()
	{
		return NonInheritedGetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as DataCacheRequest);
	}

	public virtual bool Equals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null || GetHashCode() != other.GetHashCode())
		{
			return false;
		}
		return NonInheritedEquals(other);
	}

	public virtual bool CanUseResultSetCache()
	{
		if (Cacheable && MetaApplicationObject.ApplicationObjectType != ObjectType.Query && !MetaApplicationObject.Stale)
		{
			return !FlowFieldsHelper.AnyFiltersOnFlowFields(FiltersAndMarks);
		}
		return false;
	}

	internal bool NonInheritedEquals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null || NonInheritedGetHashCode() != other.NonInheritedGetHashCode())
		{
			return false;
		}
		if (MetaApplicationObject.MetadataAppGroup != other.MetaApplicationObject.MetadataAppGroup)
		{
			return false;
		}
		return filtersAndMarks == null == (other.filtersAndMarks == null) && (filtersAndMarks == null || (filtersAndMarks.Equals(other.FiltersAndMarks) && globalAndSecurityFilters == null == (other.globalAndSecurityFilters == null))) && (globalAndSecurityFilters == null || globalAndSecurityFilters.Equals(other.globalAndSecurityFilters));
	}
}
