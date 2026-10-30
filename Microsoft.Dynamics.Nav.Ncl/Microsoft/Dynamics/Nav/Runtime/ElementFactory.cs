using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class ElementFactory<T>
{
	private ITreeObject parentForArray;

	private ITreeObject parentForElements;

	protected ITreeObject ParentForArray => parentForArray;

	protected ITreeObject ParentForElements => parentForElements;

	public ITreeObject Parent => ParentForArray;

	protected ElementFactory(ITreeObject parentForArray)
	{
		if (parentForArray == null)
		{
			throw new ArgumentNullException("parentForArray");
		}
		this.parentForArray = parentForArray;
	}

	public abstract T CreateElement();

	public abstract void AssignElement(T destinationElement, T sourceElement);

	public void ClearElement(T element)
	{
		if (element is NavComplexValue navComplexValue)
		{
			navComplexValue.Clear();
		}
	}

	public void SetElementParent(ITreeObject elementParent)
	{
		parentForElements = elementParent;
	}

	public abstract ElementFactory<T> Clone(ITreeObject parentOfClone);
}
