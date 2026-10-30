using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class GenericElement : IElement
{
	private static readonly List<string> emptyStringEnumerator = new List<string>();

	private static readonly List<IElement> emptyElementEnumerator = new List<IElement>();

	private IElement parent;

	private readonly ElementTypeInfo elementTypeInfo;

	private LineData codelines;

	private LineData rdllines;

	private LineData wordLayoutLines;

	private Dictionary<PropertyType, IModelType> properties;

	private List<IElement> subElements;

	private int id;

	private string elementKey;

	private string elementKeyNoParent;

	private int elementKeyHash;

	internal string DebugDisplay
	{
		get
		{
			string text = new StringBuilder(1024).ToString();
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return ElementKey;
		}
	}

	public string DisplayName
	{
		get
		{
			if (!TryGetStringProperty(ElementTypeInfo.NameProperty, out var value) || string.IsNullOrEmpty(value))
			{
				return "<" + ElementTypeInfo.Name + ">";
			}
			return ElementTypeInfo.Name + "<" + value + ">";
		}
	}

	public ElementTypeInfo ElementTypeInfo => elementTypeInfo;

	public IElement Parent
	{
		get
		{
			return parent;
		}
		set
		{
			if (Parent != null && value != null)
			{
				throw new GenericElementException(NonLocalizable.GenericElement_ParentIsAlreadyAssigned);
			}
			if (Parent != null && value == null && Parent.GetSubElements().Any((IElement e) => e == this))
			{
				throw new GenericElementException(NonLocalizable.GenericElement_ParentCanNotBeSetToNull);
			}
			ClearElementKey();
			parent = value;
		}
	}

	public int Id => id;

	public virtual string Name
	{
		get
		{
			TryGetStringProperty(ElementTypeInfo.NameProperty, out var value);
			return value;
		}
	}

	public virtual string FullName
	{
		get
		{
			if (Parent != null)
			{
				return Parent.FullName + "." + DisplayName;
			}
			return DisplayName;
		}
	}

	public string ElementKey
	{
		get
		{
			if (elementKey != null)
			{
				return elementKey;
			}
			elementKey = ElementKeyNoParent;
			if (Parent != null && Parent.Parent != null)
			{
				elementKey = ElementKeyHelper.CombineElementKeySegmentStrings(Parent.ElementKey, elementKey);
			}
			return elementKey;
		}
	}

	public string ElementKeyNoParent => elementKeyNoParent ?? (elementKeyNoParent = ElementKeyHelper.CreateElementKey(this));

	public int ElementKeyHash
	{
		get
		{
			if (elementKeyHash == 0)
			{
				return elementKeyHash = ElementKey.GetHashCode();
			}
			return elementKeyHash;
		}
	}

	private GenericElement(ElementTypeInfo elementTypeInfo)
	{
		if (elementTypeInfo == null)
		{
			throw new ArgumentNullException("elementTypeInfo");
		}
		this.elementTypeInfo = elementTypeInfo;
	}

	public static IElement Create(ElementTypeInfo elementTypeInfo)
	{
		return new GenericElement(elementTypeInfo);
	}

	public static IElement Create(ElementType type)
	{
		return new GenericElement(ElementTypeInfo.Lookup.GetValue(type));
	}

	[Conditional("DEBUG")]
	private void GetDebugDisplay(StringBuilder sb, int indent = 0)
	{
		string value = new string(' ', indent);
		sb.Append(value);
		sb.AppendLine(ElementKey);
		foreach (IElement subElement in GetSubElements())
		{
			_ = subElement is GenericElement;
		}
		foreach (PropertyType property in GetProperties())
		{
			sb.Append(value);
			sb.Append("  ");
			sb.AppendLine(property.ToString() + " = " + this.GetStringProperty(property));
		}
	}

	public override string ToString()
	{
		if (TryGetStringProperty(ElementTypeInfo.NameProperty, out var value))
		{
			if (string.IsNullOrEmpty(value))
			{
				value = "<Blank>";
			}
			return "(" + elementTypeInfo.Name + ": " + value + ")";
		}
		return elementTypeInfo.Name;
	}

	private void ClearElementKey()
	{
		elementKey = null;
		elementKeyNoParent = null;
		elementKeyHash = 0;
	}

	public int GetCodeLinesCount()
	{
		if (codelines != null)
		{
			return codelines.Count;
		}
		return 0;
	}

	public IEnumerable<string> GetCodeLines()
	{
		if (codelines != null)
		{
			return codelines.GetLines();
		}
		return emptyStringEnumerator;
	}

	public Stream GetCodeLinesStream()
	{
		if (codelines != null)
		{
			return codelines.GetStream();
		}
		return null;
	}

	public void SetCodeLines(IEnumerable<string> newCodeLines)
	{
		if (newCodeLines == null)
		{
			codelines = null;
			return;
		}
		codelines = new LineData();
		codelines.SetLines(newCodeLines);
	}

	public void SetCodeLines(IElement element)
	{
		if (element is GenericElement genericElement)
		{
			codelines = genericElement.codelines;
		}
		else
		{
			codelines.SetLines(element.GetCodeLines());
		}
	}

	public int GetRdlDataLinesCount()
	{
		if (rdllines != null)
		{
			return rdllines.Count;
		}
		return 0;
	}

	public IEnumerable<string> GetRdlDataLines()
	{
		if (rdllines != null)
		{
			return rdllines.GetLines();
		}
		return emptyStringEnumerator;
	}

	public void SetRdlDataLines(IEnumerable<string> rdlDataLines)
	{
		if (rdlDataLines == null)
		{
			rdllines = null;
			return;
		}
		rdllines = new LineData();
		rdllines.SetLines(rdlDataLines);
	}

	public int GetWordLayoutLinesCount()
	{
		if (wordLayoutLines != null)
		{
			return wordLayoutLines.Count;
		}
		return 0;
	}

	public IEnumerable<string> GetWordLayoutLines()
	{
		if (wordLayoutLines != null)
		{
			return wordLayoutLines.GetLines();
		}
		return emptyStringEnumerator;
	}

	public void SetWordLayoutLines(IEnumerable<string> lines)
	{
		if (lines == null)
		{
			wordLayoutLines = null;
			return;
		}
		wordLayoutLines = new LineData();
		wordLayoutLines.SetLines(lines);
	}

	public int GetPropertiesCount()
	{
		int num = ((Id != 0) ? 1 : 0);
		if (properties == null)
		{
			return num;
		}
		return properties.Count + num;
	}

	public IEnumerable<PropertyType> GetProperties()
	{
		if (Id != 0)
		{
			yield return ElementTypeInfo.IdProperty;
		}
		if (properties == null)
		{
			yield break;
		}
		foreach (PropertyType key in properties.Keys)
		{
			yield return key;
		}
	}

	public bool GetPropertySetState(PropertyType propertyType)
	{
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			return id != 0;
		}
		if (properties != null)
		{
			return properties.ContainsKey(propertyType);
		}
		return false;
	}

	public void ClearProperty(PropertyType propertyType)
	{
		ClearElementKey();
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			id = 0;
		}
		else if (properties != null)
		{
			properties.Remove(propertyType);
		}
	}

	public bool TryGetInt32Property(PropertyType propertyType, out int? value)
	{
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			if (id != 0)
			{
				value = id;
				return true;
			}
			value = null;
			ElementProperty value2;
			return ElementTypeInfo.Properties.TryGetValue(propertyType, out value2);
		}
		if (!TryGetStringProperty(propertyType, out var value3))
		{
			value = null;
			return false;
		}
		if (value3 == null)
		{
			value = null;
			return true;
		}
		if (!int.TryParse(value3, out var result))
		{
			value = 0;
			return false;
		}
		value = result;
		return true;
	}

	public bool TryGetStringProperty(PropertyType propertyType, out string value)
	{
		IModelType value2;
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			if (id != 0)
			{
				value = id.ToString(CultureInfo.InvariantCulture);
				return true;
			}
		}
		else if (properties != null && properties.TryGetValue(propertyType, out value2))
		{
			value = value2.ValueAsString();
			return true;
		}
		value = null;
		ElementProperty value3;
		return ElementTypeInfo.Properties.TryGetValue(propertyType, out value3);
	}

	public void SetInt32Property(PropertyType propertyType, int value)
	{
		ClearElementKey();
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			id = value;
		}
		else
		{
			SetStringProperty(propertyType, value.ToString(CultureInfo.InvariantCulture));
		}
	}

	public void SetStringProperty(PropertyType propertyType, string value)
	{
		ClearElementKey();
		if (ElementTypeInfo.IdProperty == propertyType)
		{
			id = int.Parse(value, CultureInfo.InvariantCulture);
			return;
		}
		if (!ElementTypeInfo.Properties.TryGetValue(propertyType, out var value2))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error assigning value to element {0} property {1}. Value '{2}' can't be assigned since the property is not supported.", FullName, propertyType.ToString(), value));
		}
		if (properties == null)
		{
			properties = new Dictionary<PropertyType, IModelType>();
		}
		properties[propertyType] = ModelTypeCreator.InvariantTypeCreatorFromString.Create(value2.Property.DataType, value);
	}

	public int GetSubElementsCount()
	{
		if (subElements == null)
		{
			return 0;
		}
		return subElements.Count;
	}

	public int GetSubElementsCount(ElementType subtype)
	{
		if (subElements == null)
		{
			return 0;
		}
		return GetSubElements(subtype).Count();
	}

	public IEnumerable<IElement> GetSubElements()
	{
		if (subElements == null)
		{
			return emptyElementEnumerator;
		}
		return subElements;
	}

	public IEnumerable<IElement> GetSubElements(ElementType subtype)
	{
		if (subElements == null)
		{
			return emptyElementEnumerator;
		}
		return subElements.Where((IElement element) => element.ElementTypeInfo.ElementType == subtype);
	}

	public IElement GetFirstSubElement(ElementType subtype)
	{
		if (subElements == null)
		{
			return null;
		}
		return subElements.FirstOrDefault((IElement element) => element.ElementTypeInfo.ElementType == subtype);
	}

	public void ClearSubElements(ElementType subtype)
	{
		if (subElements == null)
		{
			return;
		}
		for (int num = subElements.Count - 1; num >= 0; num--)
		{
			IElement element = subElements[num];
			if (element.ElementTypeInfo.ElementType == subtype)
			{
				subElements.RemoveAt(num);
				element.Parent = null;
			}
		}
		if (subElements.Count == 0)
		{
			subElements = null;
		}
	}

	public void AddSubElement(IElement subElement)
	{
		if (subElement == null)
		{
			throw new ArgumentNullException("subElement");
		}
		CheckValidSubElement(subElement);
		if (subElements == null)
		{
			subElements = new List<IElement>();
		}
		subElement.Parent = this;
		subElements.Add(subElement);
	}

	public void InsertSubElement(IElement subElement, IElement insertAfterElement)
	{
		if (subElement == null)
		{
			throw new ArgumentNullException("subElement");
		}
		CheckValidSubElement(subElement);
		if (subElements == null)
		{
			subElements = new List<IElement>();
		}
		if (insertAfterElement != null)
		{
			int num = subElements.IndexOf(insertAfterElement);
			if (num < 0)
			{
				throw new GenericElementException(NonLocalizable.GenericElement_InsertAfterNonSubElement);
			}
			subElement.Parent = this;
			subElements.Insert(num + 1, subElement);
		}
		else
		{
			subElement.Parent = this;
			subElements.Insert(0, subElement);
		}
	}

	public void MoveSubElement(IElement subElement, IElement insertAfterElement)
	{
		if (subElement == null)
		{
			throw new ArgumentNullException("subElement");
		}
		RemoveSubElement(subElement);
		if (insertAfterElement == null)
		{
			subElements.Insert(0, subElement);
		}
		else
		{
			int num = subElements.IndexOf(insertAfterElement);
			if (num < 0)
			{
				throw new GenericElementException(NonLocalizable.GenericElement_InsertAfterNonSubElement);
			}
			subElements.Insert(num + 1, subElement);
		}
		subElement.Parent = this;
	}

	public void RemoveSubElement(IElement subElement)
	{
		if (subElement == null)
		{
			throw new ArgumentNullException("subElement");
		}
		int index = CheckHasSubElement(subElement);
		subElements.RemoveAt(index);
		subElement.Parent = null;
	}

	public IElement Clone()
	{
		GenericElement clone = new GenericElement(ElementTypeInfo)
		{
			id = id
		};
		if (properties != null)
		{
			clone.properties = new Dictionary<PropertyType, IModelType>();
			properties.ForEach((KeyValuePair<PropertyType, IModelType> pair) =>
			{
				clone.properties.Add(pair.Key, pair.Value);
			});
		}
		if (subElements != null)
		{
			clone.subElements = new List<IElement>();
			subElements.ForEach((IElement e) =>
			{
				clone.AddSubElement(e.Clone());
			});
		}
		clone.codelines = codelines;
		clone.rdllines = rdllines;
		clone.wordLayoutLines = wordLayoutLines;
		return clone;
	}

	private int CheckHasSubElement(IElement subElement)
	{
		int result;
		if (subElements == null || (result = subElements.IndexOf(subElement)) < 0)
		{
			throw new GenericElementException(NonLocalizable.GenericElement_SubElementNotFound, subElement.ElementKey);
		}
		return result;
	}

	private void CheckValidSubElement(IElement subElement)
	{
		if (ElementTypeInfo.Relations != null && !ElementTypeInfo.Relations.TryGetValue(subElement.ElementTypeInfo.ElementType, out var _))
		{
			throw new GenericElementException(NonLocalizable.GenericElement_ValidSubElement, subElement.ElementTypeInfo.Name, ElementTypeInfo.Name);
		}
	}
}
