using System.Linq;

namespace Microsoft.Dynamics.Nav.MetaModel;

public abstract class ElementVisitor
{
	public IElement RootElement { get; private set; }

	protected ElementVisitor(IElement root)
	{
		RootElement = root;
	}

	public void Visit()
	{
		InternalVisitElement(RootElement);
	}

	private void InternalVisitElement(IElement element)
	{
		element = VisitElement(element);
		if (element != null)
		{
			IElement[] array = element.GetSubElements().ToArray();
			foreach (IElement element2 in array)
			{
				InternalVisitElement(element2);
			}
		}
	}

	protected virtual IElement VisitElement(IElement element)
	{
		return element;
	}
}
