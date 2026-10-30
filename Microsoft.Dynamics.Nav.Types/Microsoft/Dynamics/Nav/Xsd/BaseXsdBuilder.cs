using System;
using System.Diagnostics;
using System.Xml.Schema;

namespace Microsoft.Dynamics.Nav.Xsd;

public class BaseXsdBuilder
{
	public delegate void ComplexType(ComplexTypeBuilder type);

	public delegate void ElementType(ElementBuilder element);

	private string @namespace;

	public string Namespace
	{
		[DebuggerStepThrough]
		get
		{
			return @namespace;
		}
		[DebuggerStepThrough]
		set
		{
			@namespace = value;
		}
	}

	public BaseXsdBuilder(string theNamespace)
	{
		if (theNamespace != null)
		{
			@namespace = theNamespace;
		}
	}

	public BaseXsdBuilder(BaseXsdBuilder parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		@namespace = parent.@namespace;
	}

	public virtual XmlSchemaComplexType CreateType(string name, ComplexType complexDelegate)
	{
		ComplexTypeBuilder complexTypeBuilder = new ComplexTypeBuilder(this, name);
		complexDelegate?.Invoke(complexTypeBuilder);
		return complexTypeBuilder.Type;
	}

	public virtual XmlSchemaComplexType CreateType(string name, bool useLax, ComplexType complexDelegate)
	{
		ComplexTypeBuilder complexTypeBuilder = new ComplexTypeBuilder(this, name, useLax);
		complexDelegate?.Invoke(complexTypeBuilder);
		return complexTypeBuilder.Type;
	}

	public virtual XmlSchemaComplexType CreateType(ComplexType complexDelegate)
	{
		ComplexTypeBuilder complexTypeBuilder = new ComplexTypeBuilder(this);
		complexDelegate?.Invoke(complexTypeBuilder);
		return complexTypeBuilder.Type;
	}

	public virtual XmlSchemaElement CreateElement(string name, ElementNumber cardinality, ElementType elementDelegate)
	{
		ElementBuilder elementBuilder = new ElementBuilder(this, name);
		elementBuilder.Cardinality = cardinality;
		elementDelegate?.Invoke(elementBuilder);
		return elementBuilder.Element;
	}

	public virtual XmlSchemaElement CreateElement(string name, string namespaceUri, ElementNumber cardinality, ElementType elementDelegate)
	{
		ElementBuilder elementBuilder = new ElementBuilder(this, name, namespaceUri);
		elementBuilder.Cardinality = cardinality;
		elementDelegate?.Invoke(elementBuilder);
		return elementBuilder.Element;
	}
}
