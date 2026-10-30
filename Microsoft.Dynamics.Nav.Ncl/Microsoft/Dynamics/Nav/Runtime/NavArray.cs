using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavArray<T> : IDisposable, ITreeObject, IArrayAccess
{
	private TreeHandler tree;

	private T initValue;

	private ElementFactory<T> elementFactory;

	private int[] dimensions;

	private Array elements;

	private NavArray<T> parentArray;

	private ITreeObject parentOfElements;

	private int[] fixedDimensions;

	public TreeHandler Tree => tree;

	internal int DimensionCount => dimensions.Length;

	public int Length => dimensions[0];

	public int[] Dimensions => (int[])dimensions.Clone();

	public T this[params int[] indexes]
	{
		get
		{
			return GetElement(lazyCreate: true, indexes);
		}
		set
		{
			SetElement(value, indexes);
		}
	}

	object IArrayAccess.this[params int[] indexes]
	{
		get
		{
			return this[indexes];
		}
		set
		{
			T value2 = (T)value;
			this[indexes] = value2;
		}
	}

	int IArrayAccess.Length => Length;

	public NavArray(T initValue, params int[] dimensions)
	{
		tree = TreeHandler.DefaultHandler;
		this.initValue = initValue;
		SetDimensions(dimensions);
		AllocateArray();
	}

	public NavArray(ITreeObject parent, T initValue, params int[] dimensions)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
		this.initValue = initValue;
		SetDimensions(dimensions);
		AllocateArray();
	}

	public NavArray(ElementFactory<T> elementFactory, params int[] dimensions)
	{
		tree = TreeHandler.CreateTreeHandler(elementFactory.Parent, this);
		this.elementFactory = elementFactory;
		this.elementFactory.SetElementParent(this);
		SetDimensions(dimensions);
		SetParentArray(null);
		AllocateArray();
	}

	private NavArray(ITreeObject parent, NavArray<T> parentArray, int[] fixedDimensions)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
		SetParentArray(parentArray);
		elementFactory = parentArray.elementFactory;
		initValue = parentArray.initValue;
		this.fixedDimensions = fixedDimensions;
		dimensions = new int[parentArray.dimensions.Length - fixedDimensions.Length];
		Array.Copy(parentArray.dimensions, fixedDimensions.Length, dimensions, 0, dimensions.Length);
	}

	private NavArray(ITreeObject parent, params int[] dimensions)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
		SetParentArray(null);
		SetDimensions(dimensions);
	}

	private void SetParentArray(NavArray<T> newParentArray)
	{
		parentArray = newParentArray;
		if (parentArray != null)
		{
			parentOfElements = newParentArray.parentOfElements;
		}
		else
		{
			parentOfElements = this;
		}
	}

	private void AllocateArray()
	{
		if (elements == null)
		{
			elements = Array.CreateInstance(typeof(T), dimensions);
			if (elementFactory == null)
			{
				int[] loopDimensions = new int[dimensions.Length];
				SetElements(0, loopDimensions);
			}
		}
	}

	private void SetDimensions(int[] arrDimensions)
	{
		dimensions = new int[arrDimensions.Length];
		Array.Copy(arrDimensions, dimensions, arrDimensions.Length);
	}

	private T GetElement(bool lazyCreate, params int[] indexes)
	{
		try
		{
			if (parentArray != null)
			{
				return parentArray.GetElement(lazyCreate, ToParentindexes(indexes));
			}
			T val = GetValueOnElements(indexes);
			if ((val == null) & lazyCreate)
			{
				val = elementFactory.CreateElement();
				elements.SetValue(val, indexes);
			}
			return val;
		}
		catch (IndexOutOfRangeException)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
	}

	private void SetElement(T value, params int[] indexes)
	{
		try
		{
			if (parentArray != null)
			{
				parentArray.SetElement(value, ToParentindexes(indexes));
			}
			else
			{
				SetValueOnElements(indexes, value);
			}
		}
		catch (IndexOutOfRangeException)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
	}

	private void SetElements(int dimension, int[] loopDimensions)
	{
		for (int i = 0; i < dimensions[dimension]; i++)
		{
			loopDimensions[dimension] = i;
			if (dimension == dimensions.Length - 1)
			{
				SetElement((elementFactory == null) ? initValue : default(T), loopDimensions);
			}
			else
			{
				SetElements(dimension + 1, loopDimensions);
			}
		}
	}

	private T GetValueOnElements(int[] indexes)
	{
		if (indexes.Length != dimensions.Length)
		{
			throw new NavNCLArrayLengthMismatchException();
		}
		if (dimensions.Length != 1)
		{
			return (T)elements.GetValue(indexes);
		}
		return ((T[])elements)[indexes[0]];
	}

	private void SetValueOnElements(int[] indexes, T value)
	{
		if (indexes.Length != dimensions.Length)
		{
			throw new NavNCLArrayLengthMismatchException();
		}
		if (dimensions.Length == 1)
		{
			((T[])elements)[indexes[0]] = value;
		}
		else
		{
			elements.SetValue(value, indexes);
		}
	}

	private int[] ToParentindexes(int[] indexes)
	{
		int[] array = new int[fixedDimensions.Length + indexes.Length];
		Array.Copy(fixedDimensions, 0, array, 0, fixedDimensions.Length);
		Array.Copy(indexes, 0, array, fixedDimensions.Length, indexes.Length);
		return array;
	}

	public int ArrayLen(int dimension)
	{
		if (dimension < 0 || dimension >= dimensions.Length)
		{
			throw new NavNCLOutsidePermittedRangeException("ARRAYLEN", 2, dimension + 1, 1, dimensions.Length);
		}
		return dimensions[dimension];
	}

	public int ArrayLen()
	{
		int num = dimensions[0];
		for (int i = 1; i < dimensions.Length; i++)
		{
			num *= dimensions[i];
		}
		return num;
	}

	public NavArray<T> Clone(ITreeObject parentOfClone)
	{
		try
		{
			NavArray<T> navArray = ((elementFactory == null) ? new NavArray<T>(initValue, dimensions) : new NavArray<T>(elementFactory.Clone(parentOfClone), dimensions));
			int[] srcLoopindexes = new int[dimensions.Length];
			int[] dstLoopindexes = new int[dimensions.Length];
			CopyElements(navArray, 0, dstLoopindexes, srcLoopindexes);
			return navArray;
		}
		catch (NavNCLNotSupportedOperationException innerException)
		{
			throw new NavNCLCloneException(CultureInfo.CurrentCulture, typeof(T).ToString(), innerException);
		}
	}

	public NavArray<T> ALByValue(ITreeObject parentOfResult)
	{
		return Clone(parentOfResult);
	}

	public NavArray<T> ALByValue(ITreeObject parentOfResult, bool cloneAsTemporary)
	{
		try
		{
			NavArray<T> navArray = ((elementFactory == null) ? new NavArray<T>(initValue, dimensions) : new NavArray<T>(elementFactory.Clone(parentOfResult), dimensions));
			((NavRecordHandle.Factory)(object)navArray.elementFactory).Temporary = cloneAsTemporary;
			int[] srcLoopindexes = new int[dimensions.Length];
			int[] dstLoopindexes = new int[dimensions.Length];
			CopyElements(navArray, 0, dstLoopindexes, srcLoopindexes);
			return navArray;
		}
		catch (NavNCLNotSupportedOperationException innerException)
		{
			throw new NavNCLCloneException(CultureInfo.CurrentCulture, typeof(T).ToString(), innerException);
		}
	}

	public void Clear()
	{
		int[] loopDimensions = new int[dimensions.Length];
		ClearElements(0, loopDimensions);
	}

	public void Clear(params int[] indexes)
	{
		if (indexes.Length == dimensions.Length)
		{
			ClearElement(indexes);
		}
		else
		{
			GetSubArray(indexes).Clear();
		}
	}

	private void ClearElements(int dimension, int[] loopDimensions)
	{
		for (int i = 0; i < dimensions[dimension]; i++)
		{
			loopDimensions[dimension] = i;
			if (dimension == dimensions.Length - 1)
			{
				ClearElement(loopDimensions);
			}
			else
			{
				ClearElements(dimension + 1, loopDimensions);
			}
		}
	}

	private void ClearElement(params int[] indexes)
	{
		if (elementFactory != null)
		{
			elementFactory.ClearElement(GetElement(lazyCreate: false, indexes));
		}
		else
		{
			SetElement(initValue, indexes);
		}
	}

	public void ClearReference()
	{
		int[] loopDimensions = new int[dimensions.Length];
		ClearReferenceElements(0, loopDimensions);
	}

	public void ClearReference(params int[] indexes)
	{
		if (indexes.Length == dimensions.Length)
		{
			ClearReferenceElement(indexes);
		}
		else
		{
			GetSubArray(indexes).Clear();
		}
	}

	private void ClearReferenceElements(int dimension, int[] loopDimensions)
	{
		for (int i = 0; i < dimensions[dimension]; i++)
		{
			loopDimensions[dimension] = i;
			if (dimension == dimensions.Length - 1)
			{
				ClearReferenceElement(loopDimensions);
			}
			else
			{
				ClearReferenceElements(dimension + 1, loopDimensions);
			}
		}
	}

	private void ClearReferenceElement(params int[] indexes)
	{
		if (GetElement(lazyCreate: false, indexes) is NavCodeunitHandle navCodeunitHandle)
		{
			navCodeunitHandle.ClearReference();
		}
	}

	public NavArray<T> GetSubArray(params int[] indexes)
	{
		if (indexes.Length >= dimensions.Length)
		{
			throw new NavNCLArrayLengthMismatchException();
		}
		return new NavArray<T>(this, this, indexes);
	}

	public NavArray<T> GetSubArrayClone(ITreeObject parentOfClone, params int[] indexes)
	{
		if (indexes.Length >= dimensions.Length)
		{
			throw new NavNCLArrayLengthMismatchException();
		}
		int[] array = new int[dimensions.Length - indexes.Length];
		Array.Copy(dimensions, indexes.Length, array, 0, array.Length);
		NavArray<T> navArray = ((elementFactory == null) ? new NavArray<T>(initValue, array) : new NavArray<T>(elementFactory.Clone(parentOfClone), array));
		int[] array2 = new int[dimensions.Length];
		Array.Copy(indexes, array2, indexes.Length);
		CopyElements(navArray, 0, array, array2);
		return navArray;
	}

	private void CopyElements(NavArray<T> destination, int dstDimension, int[] dstLoopindexes, int[] srcLoopindexes)
	{
		int num = dimensions.Length - (destination.dimensions.Length - dstDimension);
		for (int i = 0; i < destination.dimensions[dstDimension]; i++)
		{
			dstLoopindexes[dstDimension] = i;
			srcLoopindexes[num] = i;
			if (dstDimension == destination.dimensions.Length - 1)
			{
				if (elementFactory == null)
				{
					destination.SetElement(GetElement(lazyCreate: false, srcLoopindexes), dstLoopindexes);
				}
				else
				{
					elementFactory.AssignElement(destination.GetElement(lazyCreate: true, srcLoopindexes), GetElement(lazyCreate: true, srcLoopindexes));
				}
			}
			else
			{
				CopyElements(destination, dstDimension + 1, dstLoopindexes, srcLoopindexes);
			}
		}
	}

	internal void CopyArray(int sourceIndex, NavArray<T> destinationArray, int destinationIndex, int length)
	{
		if (elementFactory != null)
		{
			while (length > 0)
			{
				destinationArray.elementFactory.AssignElement(destinationArray.GetElement(true, destinationIndex++), GetElement(true, sourceIndex++));
				length--;
			}
		}
		else if (elements == null || destinationArray.elements == null)
		{
			while (length > 0)
			{
				destinationArray.SetElement(GetElement(false, sourceIndex++), destinationIndex++);
				length--;
			}
		}
		else
		{
			Array.Copy(elements, sourceIndex, destinationArray.elements, destinationIndex, length);
		}
	}

	internal NavArray<T> AssignNewOptions(NCLOptionMetadata newOptionMetadata)
	{
		initValue = (T)(object)NavOption.Create(newOptionMetadata, 0);
		int[] loopDimensions = new int[dimensions.Length];
		AssignNewOptionsElements(0, loopDimensions, this, newOptionMetadata);
		return this;
	}

	private void AssignNewOptionsElements(int dimension, int[] loopDimensions, NavArray<T> array, NCLOptionMetadata newOptionMetadata)
	{
		for (int i = 0; i < dimensions[dimension]; i++)
		{
			loopDimensions[dimension] = i;
			if (dimension == dimensions.Length - 1)
			{
				AssignNewOptionsElement(array, newOptionMetadata, loopDimensions);
			}
			else
			{
				AssignNewOptionsElements(dimension + 1, loopDimensions, array, newOptionMetadata);
			}
		}
	}

	private void AssignNewOptionsElement(NavArray<T> array, NCLOptionMetadata newOptionMetadata, params int[] indexes)
	{
		NavOption navOption = (NavOption)array.elements.GetValue(indexes);
		if (navOption != null)
		{
			elements.SetValue(NavOption.Create(newOptionMetadata, navOption.Value), indexes);
		}
	}

	private void DisposeElement(params int[] indexes)
	{
		if (indexes.Length != dimensions.Length)
		{
			throw new NavNCLArrayLengthMismatchException();
		}
		object value = elements.GetValue(indexes);
		if (value is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	private void DisposeElements(int dimension, int[] loopDimensions)
	{
		for (int i = 0; i < dimensions[dimension]; i++)
		{
			loopDimensions[dimension] = i;
			if (dimension == dimensions.Length - 1)
			{
				DisposeElement(loopDimensions);
			}
			else
			{
				DisposeElements(dimension + 1, loopDimensions);
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (elements != null && parentArray == null && typeof(IDisposable).IsAssignableFrom(typeof(T)))
			{
				int[] loopDimensions = new int[dimensions.Length];
				DisposeElements(0, loopDimensions);
				elements = null;
			}
			tree.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
