using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

public interface IElement
{
	ElementTypeInfo ElementTypeInfo { get; }

	IElement Parent { get; set; }

	int Id { get; }

	string Name { get; }

	string FullName { get; }

	string ElementKey { get; }

	string ElementKeyNoParent { get; }

	int ElementKeyHash { get; }

	int GetCodeLinesCount();

	IEnumerable<string> GetCodeLines();

	Stream GetCodeLinesStream();

	void SetCodeLines(IEnumerable<string> newCodeLines);

	int GetRdlDataLinesCount();

	IEnumerable<string> GetRdlDataLines();

	void SetWordLayoutLines(IEnumerable<string> lines);

	int GetWordLayoutLinesCount();

	IEnumerable<string> GetWordLayoutLines();

	void SetRdlDataLines(IEnumerable<string> rdlDataLines);

	int GetPropertiesCount();

	IEnumerable<PropertyType> GetProperties();

	bool GetPropertySetState(PropertyType propertyType);

	void ClearProperty(PropertyType propertyType);

	bool TryGetInt32Property(PropertyType propertyType, out int? value);

	bool TryGetStringProperty(PropertyType propertyType, out string value);

	void SetInt32Property(PropertyType propertyType, int value);

	void SetStringProperty(PropertyType propertyType, string value);

	int GetSubElementsCount();

	int GetSubElementsCount(ElementType subtype);

	IEnumerable<IElement> GetSubElements();

	IEnumerable<IElement> GetSubElements(ElementType subtype);

	IElement GetFirstSubElement(ElementType subtype);

	void ClearSubElements(ElementType subtype);

	void AddSubElement(IElement subElement);

	void InsertSubElement(IElement subElement, IElement insertAfterElement);

	void MoveSubElement(IElement subElement, IElement insertAfterElement);

	void RemoveSubElement(IElement subElement);

	IElement Clone();
}
