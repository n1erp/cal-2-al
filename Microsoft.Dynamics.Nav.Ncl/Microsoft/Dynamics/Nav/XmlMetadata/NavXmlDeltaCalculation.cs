using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlDeltaCalculation : IXmlDeltaCalculationRules
{
	private readonly IXmlDeltaCalculationRules rules;

	private const string PersonalizedFilter = "PersonalizedFilter";

	private const string UserFilter = "UserFilter";

	private const string UserFilterId = "@FieldId";

	private const string Separator = "; ";

	private const string NamespacePrefix = "def";

	private static Dictionary<string, string> elementIds;

	private IDictionary<string, XmlElement> sourceNodes;

	private XmlNamespaceManager namespaceManager;

	static NavXmlDeltaCalculation()
	{
		InitializeXPathExpressionsForElementIds();
	}

	public NavXmlDeltaCalculation(IXmlDeltaCalculationRules rules = null)
	{
		this.rules = rules ?? this;
	}

	private static void InitializeXPathExpressionsForElementIds()
	{
		elementIds = new Dictionary<string, string>();
		elementIds.Add(string.Format(CultureInfo.InvariantCulture, "{0}:{1}/{0}:{2}", "def", "PersonalizedFilter", "UserFilter"), "@FieldId");
		elementIds.Add(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", "def", "PersonalizedFilter"), string.Empty);
	}

	private void InitializeSourceDocument(XmlDocument source)
	{
		sourceNodes = NavXmlDeltaUtilities.GetControlElements(source);
		namespaceManager = new XmlNamespaceManager(source.NameTable);
		namespaceManager.AddNamespace("def", source.DocumentElement.NamespaceURI);
	}

	public NavXmlDelta Build(IXPathNavigable source, IXPathNavigable target)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (!(source is XmlDocument source2))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The type of parameter '{0}' is incorrect.", "source"));
		}
		if (!(target is XmlDocument xmlDocument))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The type of parameter '{0}' is incorrect.", "target"));
		}
		NavXmlDelta navXmlDelta = new NavXmlDelta();
		InitializeSourceDocument(source2);
		IDictionary<string, XmlElement> controlElements = NavXmlDeltaUtilities.GetControlElements(xmlDocument);
		CollectAddedElements(navXmlDelta, xmlDocument.ChildNodes);
		CollectDeletedElements(navXmlDelta, controlElements);
		CollectMovedElements(navXmlDelta, controlElements);
		CollectOrderedElements(navXmlDelta, controlElements);
		CollectUpdatedElements(navXmlDelta, controlElements);
		return navXmlDelta;
	}

	private void CollectAddedElements(NavXmlDelta delta, XmlNodeList targetNodes)
	{
		foreach (XmlNode targetNode in targetNodes)
		{
			if (NavXmlDeltaUtilities.IsNewNode(targetNode, sourceNodes))
			{
				NavXmlNode navXmlNode = NavXmlDeltaUtilities.ConvertXmlNodeToNavXmlNode(targetNode, sourceNodes, rules);
				if (navXmlNode != null)
				{
					NavXmlAdd navXmlAdd = new NavXmlAdd();
					navXmlAdd.Node = navXmlNode;
					navXmlAdd.Id = NavXmlDeltaUtilities.GetControlId(targetNode);
					navXmlAdd.Destination = NavXmlDeltaUtilities.GetControlId(targetNode.ParentNode);
					delta.Items.Add(navXmlAdd);
				}
			}
			else if (!rules.RemoveDisallowedFromExistingAddDeltas(targetNode))
			{
				CollectAddedElements(delta, targetNode.ChildNodes);
			}
		}
	}

	private void CollectDeletedElements(NavXmlDelta delta, IDictionary<string, XmlElement> targetNodes)
	{
		foreach (string key in sourceNodes.Keys)
		{
			if (!targetNodes.ContainsKey(key) && !rules.IsElementRestrictedFromDelete(key))
			{
				NavXmlDelete navXmlDelete = new NavXmlDelete();
				navXmlDelete.Id = key;
				delta.Items.Add(navXmlDelete);
			}
		}
	}

	private void CollectMovedElements(NavXmlDelta delta, IDictionary<string, XmlElement> targetNodes)
	{
		foreach (string key in targetNodes.Keys)
		{
			if (!sourceNodes.ContainsKey(key))
			{
				continue;
			}
			XmlElement element = targetNodes[key];
			XmlElement parentControlElement = NavXmlDeltaUtilities.GetParentControlElement(element);
			XmlElement element2 = sourceNodes[key];
			XmlElement parentControlElement2 = NavXmlDeltaUtilities.GetParentControlElement(element2);
			if (parentControlElement2 != null && parentControlElement != null)
			{
				string attribute = parentControlElement.GetAttribute("ControlGUID");
				string attribute2 = parentControlElement2.GetAttribute("ControlGUID");
				if (string.Compare(attribute, attribute2, StringComparison.OrdinalIgnoreCase) != 0)
				{
					NavXmlMove navXmlMove = new NavXmlMove();
					navXmlMove.Id = key;
					navXmlMove.Destination = attribute;
					delta.Items.Add(navXmlMove);
				}
			}
		}
	}

	private void CollectOrderedElements(NavXmlDelta delta, IDictionary<string, XmlElement> targetNodes)
	{
		foreach (string key in targetNodes.Keys)
		{
			XmlElement element = targetNodes[key];
			string[] childNodeOrder = NavXmlDeltaUtilities.GetChildNodeOrder(element);
			bool flag = false;
			if (sourceNodes.ContainsKey(key))
			{
				XmlElement element2 = sourceNodes[key];
				string[] childNodeOrder2 = NavXmlDeltaUtilities.GetChildNodeOrder(element2);
				flag = NavXmlDeltaUtilities.IsOrderChanged(childNodeOrder2, childNodeOrder);
			}
			else if (childNodeOrder.Length > 1)
			{
				flag = true;
			}
			if (flag)
			{
				NavXmlOrder navXmlOrder = new NavXmlOrder();
				navXmlOrder.Id = key;
				navXmlOrder.Sequence = NavXmlDeltaUtilities.ArrayToString(childNodeOrder, "; ");
				delta.Items.Add(navXmlOrder);
			}
		}
	}

	private void CollectUpdatedElements(NavXmlDelta delta, IDictionary<string, XmlElement> targetNodes)
	{
		foreach (string key in targetNodes.Keys)
		{
			if (!sourceNodes.ContainsKey(key))
			{
				continue;
			}
			XmlElement targetNode = targetNodes[key];
			XmlElement sourceNode = sourceNodes[key];
			CollectUpdatedAttributes(delta, targetNode, sourceNode, key);
			foreach (string key2 in elementIds.Keys)
			{
				string value = elementIds[key2];
				if (string.IsNullOrEmpty(value))
				{
					CollectUpdatesOnSingleInnerElements(delta, key2, targetNode, sourceNode);
				}
				else
				{
					CollectUpdatesOnInnerElements(delta, key2, targetNode, sourceNode);
				}
			}
		}
	}

	private void CollectUpdatesOnInnerElements(NavXmlDelta delta, string elementPath, XmlNode targetNode, XmlNode sourceNode)
	{
		string controlId = NavXmlDeltaUtilities.GetControlId(targetNode);
		string text = elementIds[elementPath];
		XmlNodeList xmlNodeList = targetNode.SelectNodes(elementPath, namespaceManager);
		XmlNodeList xmlNodeList2 = sourceNode.SelectNodes(elementPath, namespaceManager);
		if (xmlNodeList.Count > 0)
		{
			foreach (XmlNode item in xmlNodeList)
			{
				XmlNode elementId = item.SelectSingleNode(text);
				XmlNode xmlNode2 = NavXmlDeltaUtilities.FindNodeById(elementId, text, xmlNodeList2, namespaceManager);
				if (xmlNode2 != null)
				{
					CollectUpdatedAttributes(delta, (XmlElement)item, (XmlElement)xmlNode2, controlId, elementPath, elementId);
				}
				else
				{
					CollectAddedInnerElement(delta, controlId, item, elementPath, elementId);
				}
			}
		}
		foreach (XmlNode item2 in xmlNodeList2)
		{
			XmlNode elementId2 = item2.SelectSingleNode(text);
			XmlNode xmlNode4 = NavXmlDeltaUtilities.FindNodeById(elementId2, text, xmlNodeList, namespaceManager);
			if (xmlNode4 == null)
			{
				CollectDeletedInnerElement(delta, controlId, elementPath, elementId2);
			}
		}
	}

	private void CollectUpdatesOnSingleInnerElements(NavXmlDelta delta, string elementPath, XmlNode targetNode, XmlNode sourceNode)
	{
		string controlId = NavXmlDeltaUtilities.GetControlId(targetNode);
		XmlNode xmlNode = targetNode.SelectSingleNode(elementPath, namespaceManager);
		XmlNode xmlNode2 = sourceNode.SelectSingleNode(elementPath, namespaceManager);
		if (xmlNode != null && xmlNode2 != null)
		{
			CollectUpdatedAttributes(delta, (XmlElement)xmlNode, (XmlElement)xmlNode2, controlId, elementPath, null);
		}
		if (xmlNode != null && xmlNode2 == null)
		{
			CollectAddedInnerElement(delta, controlId, xmlNode, elementPath, null);
		}
		if (xmlNode2 != null && xmlNode == null)
		{
			CollectDeletedInnerElement(delta, controlId, elementPath, null);
		}
	}

	private void CollectUpdatedAttributes(NavXmlDelta delta, XmlElement targetNode, XmlElement sourceNode, string changeId)
	{
		CollectUpdatedAttributes(delta, targetNode, sourceNode, changeId, null, null);
	}

	private void CollectUpdatedAttributes(NavXmlDelta delta, XmlElement targetNode, XmlElement sourceNode, string changeId, string path, XmlNode elementId)
	{
		foreach (XmlAttribute attribute2 in targetNode.Attributes)
		{
			bool flag = !sourceNode.HasAttribute(attribute2.LocalName, attribute2.NamespaceURI);
			if (!flag)
			{
				string attribute = sourceNode.GetAttribute(attribute2.LocalName, attribute2.NamespaceURI);
				string value = attribute2.Value;
				flag = string.Compare(value, attribute, StringComparison.Ordinal) != 0;
			}
			if (flag)
			{
				NavXmlUpdate navXmlUpdate = new NavXmlUpdate();
				navXmlUpdate.Id = changeId;
				navXmlUpdate.Uri = attribute2.NamespaceURI;
				navXmlUpdate.Name = attribute2.LocalName;
				navXmlUpdate.Value = rules.ScrubAttributeValueBeforeUpdate(attribute2.Name, attribute2.Value);
				navXmlUpdate.ElementPath = path;
				if (elementId != null)
				{
					navXmlUpdate.ElementId = NavXmlDeltaUtilities.ConvertElementIdToNavXmlNode(elementId);
				}
				delta.Items.Add(navXmlUpdate);
			}
		}
		foreach (XmlAttribute attribute3 in sourceNode.Attributes)
		{
			if (!targetNode.HasAttribute(attribute3.LocalName, attribute3.NamespaceURI))
			{
				NavXmlUpdate navXmlUpdate2 = new NavXmlUpdate();
				navXmlUpdate2.Id = changeId;
				navXmlUpdate2.Uri = attribute3.NamespaceURI;
				navXmlUpdate2.Name = attribute3.LocalName;
				navXmlUpdate2.Value = string.Empty;
				navXmlUpdate2.ElementPath = path;
				if (elementId != null)
				{
					navXmlUpdate2.ElementId = NavXmlDeltaUtilities.ConvertElementIdToNavXmlNode(elementId);
				}
				delta.Items.Add(navXmlUpdate2);
			}
		}
	}

	public void Apply(IXPathNavigable document, NavXmlDelta delta)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (!(document is XmlDocument xmlDocument))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The type of parameter '{0}' is incorrect.", "document"));
		}
		if (delta == null)
		{
			throw new ArgumentNullException("delta");
		}
		IDictionary<string, XmlElement> controlElements = NavXmlDeltaUtilities.GetControlElements(xmlDocument);
		namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		namespaceManager.AddNamespace("def", xmlDocument.DocumentElement.NamespaceURI);
		foreach (NavXmlBase item in delta.Items)
		{
			if (item is NavXmlAdd add)
			{
				ApplyNodeAdd(add, xmlDocument, controlElements);
			}
			else if (item is NavXmlDelete delete)
			{
				ApplyNodeDelete(delete, controlElements);
			}
			else if (item is NavXmlMove move)
			{
				ApplyNodeMove(move, controlElements);
			}
			else if (item is NavXmlOrder order)
			{
				ApplyNodeOrder(order, controlElements);
			}
			else if (item is NavXmlUpdate update)
			{
				ApplyNodeUpdate(update, controlElements);
			}
		}
	}

	private void ApplyNodeAdd(NavXmlAdd add, XmlDocument document, IDictionary<string, XmlElement> resultNodes)
	{
		if (add == null || add.Node == null || add.Destination == null)
		{
			return;
		}
		XmlElement value = null;
		resultNodes.TryGetValue(add.Destination, out value);
		if (value == null)
		{
			return;
		}
		XmlElement newChild = ConvertAddNode(add.Node, document, resultNodes);
		if (!string.IsNullOrEmpty(add.ParentPath))
		{
			value = value.SelectSingleNode(add.ParentPath, namespaceManager) as XmlElement;
		}
		if (value != null)
		{
			XmlNode xmlNode = FindInnerElementNode(add.ElementId, add.ElementIdPath, value);
			if (xmlNode != null)
			{
				value.ReplaceChild(newChild, xmlNode);
			}
			else
			{
				value.AppendChild(newChild);
			}
		}
	}

	private void ApplyNodeDelete(NavXmlDelete delete, IDictionary<string, XmlElement> resultNodes)
	{
		XmlElement value = null;
		if (!string.IsNullOrEmpty(delete.Destination))
		{
			resultNodes.TryGetValue(delete.Destination, out value);
			if (value != null)
			{
				XmlNode xmlNode = FindInnerElementNode(delete.ElementId, delete.ElementPath, value);
				if (xmlNode != null && xmlNode.ParentNode != null)
				{
					xmlNode.ParentNode.RemoveChild(xmlNode);
				}
			}
		}
		else
		{
			resultNodes.TryGetValue(delete.Id, out value);
			if (value != null && value.ParentNode != null)
			{
				value.ParentNode.RemoveChild(value);
			}
		}
	}

	private void ApplyNodeMove(NavXmlMove move, IDictionary<string, XmlElement> resultNodes)
	{
		XmlElement value = null;
		XmlElement value2 = null;
		resultNodes.TryGetValue(move.Id, out value);
		resultNodes.TryGetValue(move.Destination, out value2);
		if (value != null && value2 != null)
		{
			XmlNode elementId = value.Attributes["ControlGUID"];
			XmlNode xmlNode = NavXmlDeltaUtilities.FindNodeById(elementId, "@ControlGUID", value2.ChildNodes, namespaceManager);
			if (xmlNode != null)
			{
				value2.ReplaceChild(value, xmlNode);
			}
			else
			{
				value2.AppendChild(value);
			}
		}
	}

	private static void ApplyNodeOrder(NavXmlOrder order, IDictionary<string, XmlElement> resultNodes)
	{
		XmlElement value = null;
		resultNodes.TryGetValue(order.Id, out value);
		if (value != null)
		{
			string[] order2 = NavXmlDeltaUtilities.StringToArray(order.Sequence, "; ");
			NavXmlDeltaUtilities.SetChildNodeOrder(value, order2);
		}
	}

	private void ApplyNodeUpdate(NavXmlUpdate update, IDictionary<string, XmlElement> resultNodes)
	{
		XmlElement value = null;
		resultNodes.TryGetValue(update.Id, out value);
		if (value == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(update.ElementPath))
		{
			value = FindInnerElementNode(update.ElementId, update.ElementPath, value) as XmlElement;
		}
		if (value != null)
		{
			if (!string.IsNullOrEmpty(update.Value))
			{
				value.SetAttribute(update.Name, update.Uri, update.Value);
			}
			else
			{
				value.Attributes.RemoveNamedItem(update.Name, update.Uri);
			}
		}
	}

	private void CollectAddedInnerElement(NavXmlDelta delta, string destinationId, XmlNode innerElement, string elementIdPath, XmlNode elementId)
	{
		NavXmlNode navXmlNode = NavXmlDeltaUtilities.ConvertXmlNodeToNavXmlNode(innerElement, sourceNodes, rules);
		if (navXmlNode != null)
		{
			NavXmlAdd navXmlAdd = new NavXmlAdd();
			navXmlAdd.Destination = destinationId;
			navXmlAdd.Node = navXmlNode;
			navXmlAdd.ParentPath = NavXmlDeltaUtilities.GetParentPath(innerElement, "def");
			navXmlAdd.ElementIdPath = elementIdPath;
			navXmlAdd.ElementId = NavXmlDeltaUtilities.ConvertElementIdToNavXmlNode(elementId);
			delta.Items.Add(navXmlAdd);
		}
	}

	private static void CollectDeletedInnerElement(NavXmlDelta delta, string destinationId, string elementPath, XmlNode elementId)
	{
		NavXmlDelete navXmlDelete = new NavXmlDelete();
		navXmlDelete.ElementId = NavXmlDeltaUtilities.ConvertElementIdToNavXmlNode(elementId);
		navXmlDelete.ElementPath = elementPath;
		navXmlDelete.Destination = destinationId;
		delta.Items.Add(navXmlDelete);
	}

	private static XmlElement ConvertAddNode(NavXmlNode addNode, XmlDocument document, IDictionary<string, XmlElement> resultNodes)
	{
		XmlElement xmlElement = document.CreateElement(addNode.Name, addNode.Uri);
		foreach (NavXmlNodeAttribute attribute in addNode.Attributes)
		{
			XmlAttribute xmlAttribute = document.CreateAttribute(attribute.Name, attribute.Uri);
			xmlAttribute.Value = attribute.Value;
			xmlElement.Attributes.Append(xmlAttribute);
		}
		foreach (NavXmlNode childNode in addNode.ChildNodes)
		{
			XmlElement newChild = ConvertAddNode(childNode, document, resultNodes);
			xmlElement.AppendChild(newChild);
		}
		if (addNode.Id != null && !resultNodes.ContainsKey(addNode.Id))
		{
			resultNodes.Add(addNode.Id, xmlElement);
		}
		return xmlElement;
	}

	private XmlNode FindInnerElementNode(NavXmlNode elementId, string elementPath, XmlNode parentElement)
	{
		if (elementPath == null)
		{
			return null;
		}
		if (elementId != null)
		{
			XmlNode elementId2 = NavXmlDeltaUtilities.ConvertElementIdToXmlNode(elementId, parentElement.OwnerDocument);
			XmlNodeList elementList = parentElement.SelectNodes(elementPath, namespaceManager);
			return NavXmlDeltaUtilities.FindNodeById(elementId2, elementIds[elementPath], elementList, namespaceManager);
		}
		return parentElement.SelectSingleNode(elementPath, namespaceManager);
	}

	public bool IsElementRestrictedFromDelete(string sourceNodeIdentifier)
	{
		return true;
	}

	public string ScrubAttributeValueBeforeUpdate(string attributeName, string attributeValue)
	{
		return attributeValue;
	}

	public bool IsAllowedAsAddDelta(string nodeName)
	{
		return false;
	}

	public bool RemoveDisallowedFromExistingAddDeltas(IXPathNavigable node)
	{
		return false;
	}
}
