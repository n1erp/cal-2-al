using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class SizableObject
{
	internal const int ObjectInstanceByteOverhead = 24;

	internal static int GetApproximateByteSize(params ISizableObject[] sizableObjects)
	{
		if (sizableObjects == null)
		{
			return 0;
		}
		int num = 0;
		foreach (ISizableObject sizableObject in sizableObjects)
		{
			if (sizableObject != null)
			{
				num += sizableObject.ApproximateByteSize;
			}
		}
		return num;
	}

	internal static int GetApproximateByteSize(params NavValue[] navValues)
	{
		if (navValues == null)
		{
			return 0;
		}
		int num = 0;
		foreach (NavValue navValue in navValues)
		{
			if (navValue != null)
			{
				num += ApproximateByteSizeForNavValue(navValue) + 24;
			}
		}
		return num;
	}

	internal static int GetApproximateByteSize(int numberOfObjectFields)
	{
		return numberOfObjectFields * 8;
	}

	internal unsafe static int ApproximateByteSizeForNclType(NavNclType nclType, int definedLength = 0)
	{
		switch (nclType)
		{
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavGuid:
			return sizeof(Guid);
		case NavNclType.NavBigInteger:
		case NavNclType.NavDuration:
			return 8;
		case NavNclType.NavBlob:
			return 4;
		case NavNclType.NavBoolean:
			return 1;
		case NavNclType.NavByte:
			return 1;
		case NavNclType.NavDecimal:
			return sizeof(Decimal18);
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			return 4;
		case NavNclType.NavRecordId:
			return 448;
		case NavNclType.NavDate:
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			return sizeof(DateTime);
		case NavNclType.NavTableFilter:
			return 504;
		case NavNclType.NavOemText:
		case NavNclType.NavOemCode:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return definedLength * 2;
		case NavNclType.NavDateFormula:
			return 32;
		default:
			throw new NotSupportedException();
		}
	}

	private static int ApproximateByteSizeForNavValue(NavValue navValue)
	{
		switch (navValue.NclType)
		{
		case NavNclType.NavOemText:
		case NavNclType.NavOemCode:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return ApproximateByteSizeForNclType(navValue.NclType, ((NavStringValue)navValue).Value.Length);
		default:
			return ApproximateByteSizeForNclType(navValue.NclType);
		}
	}
}
