using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class UpgradeFieldDesignEqualityComparer : EqualityComparer<NCLMetaField>
{
	public override bool Equals(NCLMetaField x, NCLMetaField y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		if (x.FieldNo.Equals(y.FieldNo) && x.FieldName.Equals(y.FieldName) && x.FieldDefinedLength == y.FieldDefinedLength && x.FieldNclType == y.FieldNclType && x.SqlDataType == y.SqlDataType && x.FieldIsAutoIncrement == y.FieldIsAutoIncrement && x.FieldIsCompressed == y.FieldIsCompressed && x.FieldActive == y.FieldActive)
		{
			return x.FieldClass == y.FieldClass;
		}
		return false;
	}

	public override int GetHashCode(NCLMetaField obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return HashCodeHelper.CombineHashCodes(obj.FieldNo.GetHashCode(), obj.FieldName.GetHashCode(), obj.FieldDefinedLength.GetHashCode(), obj.FieldNclType.GetHashCode());
	}
}
