using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemArray
{
	public static int ALArrayLen(object array)
	{
		throw new NavNCLArgumentException(Lang.IllegalArrayLenParameter);
	}

	public static int ALArrayLen(object array, int dimension)
	{
		throw new NavNCLArgumentException(Lang.IllegalArrayLenParameter);
	}

	public static int ALCompressArray<T>(NavArray<T> array) where T : NavStringValue
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.ArrayLen() == 0)
		{
			return 0;
		}
		if (array.DimensionCount == 1)
		{
			int num = array.ArrayLen(0);
			int num2 = 0;
			NavStringValue navStringValue = null;
			for (int i = 0; i < num; i++)
			{
				NavStringValue navStringValue2 = array[new int[1] { i }];
				if (navStringValue2.Value.Length > 0)
				{
					if (num2 != i)
					{
						array[new int[1] { num2 }] = (T)navStringValue2;
						array[new int[1] { i }] = (T)navStringValue;
					}
					num2++;
				}
				else
				{
					navStringValue = navStringValue2;
				}
			}
			return num2;
		}
		throw new NotSupportedException(Lang.MultidimensionalArraysIsNotSupportedInCompressArray);
	}

	public static void ALCopyArray<T>(NavArray<T> destinationArray, NavArray<T> sourceArray, int sourceIndex)
	{
		ALCopyArray(sourceArray, sourceIndex - 1, destinationArray, 0, sourceArray.Length - (sourceIndex - 1));
	}

	public static void ALCopyArray<T>(NavArray<T> destinationArray, NavArray<T> sourceArray, int sourceIndex, int length)
	{
		ALCopyArray(sourceArray, sourceIndex - 1, destinationArray, 0, length);
	}

	public static void ALCopyArray<T>(NavArray<T> sourceArray, int sourceIndex, NavArray<T> destinationArray, int destinationIndex, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (sourceArray.Length > 0 && sourceArray.DimensionCount != 1)
		{
			throw new NavNCLArrayDimensionException(Lang.SourceArrayMustBeOneDimensional);
		}
		if (destinationArray.Length > 0 && destinationArray.DimensionCount != 1)
		{
			throw new NavNCLArrayDimensionException(Lang.DestinationArrayMustBeOneDimensional);
		}
		if (destinationArray.Length != length)
		{
			throw new NavNCLArrayLengthMismatchException(length, destinationArray.Length);
		}
		if (sourceIndex < 0 || sourceIndex + length > sourceArray.Length)
		{
			throw NavNCLArgumentOutOfRangeException.CreateSourceArrayIndexMismatch(CultureInfo.InvariantCulture, sourceIndex, sourceArray.Length);
		}
		if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
		{
			throw NavNCLArgumentOutOfRangeException.CreateDestinationArrayIndexMismatch(CultureInfo.InvariantCulture, destinationIndex, length, destinationArray.Length);
		}
		try
		{
			sourceArray.CopyArray(sourceIndex, destinationArray, destinationIndex, length);
		}
		catch (ArrayTypeMismatchException)
		{
			throw new NavNCLArrayTypeMismatchException(Lang.SourceAndDestinationIncompatible);
		}
		catch (RankException)
		{
			throw new NavNCLArrayRankException(Lang.SourceAndDestinationDifferentRanks);
		}
		catch (InvalidCastException)
		{
			throw new NavNCLArrayTypeMismatchException(Lang.SourceAndDestinationIncompatible);
		}
	}

	public static void ALCopyArray(object sourceArray, int sourceIndex, object destinationArray, int destinationIndex, int length)
	{
		if (!(sourceArray is Array))
		{
			throw new NavNCLArrayDimensionException(Lang.SourceArrayMustBeOneDimensional);
		}
		throw new NavNCLArrayDimensionException(Lang.DestinationArrayMustBeOneDimensional);
	}
}
