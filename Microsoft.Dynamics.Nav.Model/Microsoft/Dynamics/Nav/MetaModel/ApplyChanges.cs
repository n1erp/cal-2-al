using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Compiler;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.Compiler;
using Microsoft.Dynamics.Nav.Model.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public sealed class ApplyChanges
{
	private readonly ApplyChangesOptions options;

	private readonly IElement targetRootElement;

	private IElement conflictRootElement;

	private readonly List<IElement> processedChanges = new List<IElement>();

	private const string ContainerTypeString = "Container";

	private const string GroupTypeString = "Group";

	private static readonly ApplyChangesOptions DefaultOptions = new ApplyChangesOptions
	{
		InlineCodeConflicts = true,
		ConflictSeverityLevel = ConflictSeverityType.Medium
	};

	public ApplyChanges(IElement targetRootElement)
		: this(targetRootElement, DefaultOptions)
	{
	}

	public ApplyChanges(IElement targetRootElement, ApplyChangesOptions options)
	{
		if (targetRootElement == null)
		{
			throw new ArgumentNullException("targetRootElement");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		this.targetRootElement = targetRootElement;
		this.options = options;
	}

	private IElement ApplyDoubleInsert(IElement deltaInsertRootElement, out IElement conflict)
	{
		IElement element = targetRootElement.Clone();
		ComputeChangesOptions computeChangesOptions = new ComputeChangesOptions
		{
			DevBaseCompatibility = options.DevBaseCompatibility,
			UseCodeCompression = false
		};
		conflict = new ComputeChanges(deltaInsertRootElement, computeChangesOptions).Compute(targetRootElement);
		if (conflict != null && !options.DevBaseCompatibility && options.InlineCodeConflicts)
		{
			foreach (IElement item in from c in conflict.GetSubElements(ElementType.Change)
				where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.CodeModification.ToString()
				select c)
			{
				string changeTarget = ElementKeyHelper.DisplayElementKeyToElementKey(item.GetStringProperty(PropertyType.Target));
				IElement element2 = element.GetElements().First((IElement e) => e.ElementKey == changeTarget);
				IElement element3 = deltaInsertRootElement.GetElements().First((IElement e) => e.ElementKey == changeTarget);
				CodeLinesHelper.MergeCode(new List<string>(), element2.GetCodeLines().ToList(), element3.GetCodeLines().ToList(), out var mergedCode);
				element2.SetCodeLines(mergedCode);
				item.SetStringProperty(PropertyType.ConflictHint, NonLocalizable.ConflictHint_InlineCodeConflict);
				item.ClearSubElements(ElementType.ModifiedCode);
				item.ClearSubElements(ElementType.TargetCode);
			}
		}
		return element;
	}

	public IElement Apply(IElement deltaRootElement, out IElement conflict)
	{
		if (deltaRootElement == null)
		{
			throw new ArgumentNullException("deltaRootElement");
		}
		if (!deltaRootElement.GetPropertySetState(PropertyType.Target))
		{
			return ApplyDoubleInsert(deltaRootElement, out conflict);
		}
		if (targetRootElement.GetApplicationObjectId() != deltaRootElement.GetApplicationObjectId())
		{
			throw new GenericElementException(NonLocalizable.ModelChanges_AppObjsMustHaveSameTypeAndId);
		}
		conflict = null;
		conflictRootElement = null;
		processedChanges.Clear();
		if (deltaRootElement.GetPropertySetState(PropertyType.ChangeType) && deltaRootElement.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Deletion.ToString())
		{
			return null;
		}
		IElement element = targetRootElement.Clone();
		ApplyElementChanges(element, deltaRootElement);
		ApplyRdlChanges(deltaRootElement, element);
		ApplyWordLayoutChanges(deltaRootElement, element);
		HandleUnprocessedChanges(element, deltaRootElement);
		if (conflictRootElement != null)
		{
			conflict = conflictRootElement;
			conflict.SetStringProperty(PropertyType.Name, deltaRootElement.GetStringProperty(PropertyType.Name));
			conflict.SetStringProperty(PropertyType.Target, deltaRootElement.GetStringProperty(PropertyType.Target));
		}
		return element;
	}

	private void ApplyRdlChanges(IElement deltaRootElement, IElement resultElement)
	{
		IElement element = deltaRootElement.GetSubElements(ElementType.Change).FirstOrDefault((IElement e) => e.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.RdlModification.ToString());
		if (element != null)
		{
			processedChanges.Add(element);
			IElement firstSubElement = element.GetFirstSubElement(ElementType.OriginalCode);
			IElement firstSubElement2 = element.GetFirstSubElement(ElementType.ModifiedCode);
			if (CodeLinesHelper.MergeCode(firstSubElement.GetCodeLines().ToList(), resultElement.GetRdlDataLines().ToList(), CodeLinesHelper.ExpandCompressed(firstSubElement2.GetCodeLines(), firstSubElement.GetCodeLines()).ToList(), out var mergedCode))
			{
				resultElement.SetRdlDataLines(mergedCode);
			}
			else
			{
				AddCodeConflict(element.GetStringProperty(PropertyType.Target), ChangeTypes.RdlModification, firstSubElement.GetCodeLines(), firstSubElement2.GetCodeLines(), resultElement.GetRdlDataLines());
			}
		}
	}

	private void ApplyWordLayoutChanges(IElement deltaRootElement, IElement resultElement)
	{
		IElement element = deltaRootElement.GetSubElements(ElementType.Change).FirstOrDefault((IElement e) => e.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.WordLayoutModification.ToString());
		if (element != null)
		{
			processedChanges.Add(element);
			IElement firstSubElement = element.GetFirstSubElement(ElementType.OriginalCode);
			List<string> list = CodeLinesHelper.ExpandCompressed(element.GetFirstSubElement(ElementType.ModifiedCode).GetCodeLines(), firstSubElement.GetCodeLines()).ToList();
			if (firstSubElement.GetCodeLinesCount() == resultElement.GetWordLayoutLinesCount() && CodeLinesHelper.CompareLines(firstSubElement.GetCodeLines(), resultElement.GetWordLayoutLines()))
			{
				resultElement.SetWordLayoutLines(list);
			}
			else
			{
				AddCodeConflict(element.GetStringProperty(PropertyType.Target), ChangeTypes.WordLayoutModification, firstSubElement.GetCodeLines(), list, resultElement.GetWordLayoutLines());
			}
		}
	}

	private void ApplyElementChanges(IElement resultElement, IElement deltaElement)
	{
		foreach (IElement item in from c in deltaElement.GetSubElements(ElementType.Change)
			let ct = c.GetStringProperty(PropertyType.ChangeType)
			where (ct == ChangeTypes.PropertyDeletion.ToString() || ct == ChangeTypes.PropertyModification.ToString()) && ((!c.GetPropertySetState(PropertyType.Target) && resultElement.Parent == null) || ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target)) == resultElement.ElementKey)
			select c)
		{
			processedChanges.Add(item);
			PropertyType propertyType = FindAndCheckPropertyFromName(resultElement, item.GetStringProperty(PropertyType.Property));
			string stringProperty = resultElement.GetStringProperty(propertyType);
			if (!options.SkipOriginalValueCheck && stringProperty != item.GetStringProperty(PropertyType.OriginalValue))
			{
				if (stringProperty != item.GetStringProperty(PropertyType.ModifiedValue))
				{
					AddPropertyConflict(propertyType, item, resultElement, NonLocalizable.ConflictHint_PropertyChangedInBothTargetAndModified, ConflictSeverityType.Medium);
				}
			}
			else if (item.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.PropertyDeletion.ToString())
			{
				resultElement.ClearProperty(propertyType);
			}
			else
			{
				resultElement.SetStringProperty(propertyType, item.GetStringProperty(PropertyType.ModifiedValue));
			}
		}
		foreach (IElement item2 in from c in deltaElement.GetSubElements(ElementType.Change)
			where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.PropertyInsertion.ToString() && ((!c.GetPropertySetState(PropertyType.Target) && resultElement.Parent == null) || ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target)) == resultElement.ElementKey)
			select c)
		{
			processedChanges.Add(item2);
			PropertyType propertyType2 = FindAndCheckPropertyFromName(resultElement, item2.GetStringProperty(PropertyType.Property));
			if (!options.SkipOriginalValueCheck && resultElement.GetPropertySetState(propertyType2) && resultElement.GetStringProperty(propertyType2) != item2.GetStringProperty(PropertyType.ModifiedValue))
			{
				AddPropertyConflict(propertyType2, item2, resultElement, NonLocalizable.ConflictHint_PropertyInsertedInBothTargetAndModified, ConflictSeverityType.Medium);
			}
			else
			{
				resultElement.SetStringProperty(propertyType2, item2.GetStringProperty(PropertyType.ModifiedValue));
			}
		}
		if (resultElement.ElementTypeInfo.HasCodeLines)
		{
			var anon = (from c in deltaElement.GetSubElements(ElementType.Change)
				where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.CodeModification.ToString() && ((!c.GetPropertySetState(PropertyType.Target) && resultElement.Parent == null) || ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target)) == resultElement.ElementKey)
				select new
				{
					Target = c.GetStringProperty(PropertyType.Target),
					OriginalCode = c.GetFirstSubElement(ElementType.OriginalCode),
					ModifiedCode = c.GetFirstSubElement(ElementType.ModifiedCode),
					Change = c
				}).FirstOrDefault();
			if (anon != null)
			{
				processedChanges.Add(anon.Change);
				MergeCode(resultElement, anon.OriginalCode, anon.ModifiedCode, anon.Target);
			}
		}
		if (resultElement.ElementTypeInfo.Relations != null)
		{
			foreach (ElementRelation relation in resultElement.ElementTypeInfo.Relations.Values)
			{
				foreach (var item3 in from c in deltaElement.GetSubElements(ElementType.Change)
					where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Move.ToString()
					let targetKey = ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target))
					join se in resultElement.GetSubElements(relation.ChildType) on targetKey equals se.ElementKey
					select new
					{
						Change = c,
						Target = se
					})
				{
					processedChanges.Add(item3.Change);
					IElement element = null;
					if (item3.Change.GetPropertySetState(PropertyType.InsertAfter))
					{
						element = FindSubElementOfTypeByElementKeyNoParent(resultElement, relation.ChildType, item3.Change.GetStringProperty(PropertyType.InsertAfter));
						if (element == null)
						{
							AddConflict(item3.Change, NonLocalizable.ConflictHint_UnprocessedMove, CalcConflictSeverityForOrderingConflict(relation));
							continue;
						}
					}
					resultElement.MoveSubElement(item3.Target, element);
					if (!item3.Change.GetPropertySetState(PropertyType.IncludeSiblings))
					{
						continue;
					}
					element = item3.Target;
					foreach (string item4 in ElementKeyHelper.DisplayProvider.SplitElementKeyList(item3.Change.GetStringProperty(PropertyType.IncludeSiblings)))
					{
						IElement element2 = FindSubElementOfTypeByElementKeyNoParent(resultElement, relation.ChildType, item4);
						if (element2 == null)
						{
							AddConflict(item3.Change, NonLocalizable.ConflictHint_UnprocessedMoveSibling, CalcConflictSeverityForOrderingConflict(relation));
							continue;
						}
						resultElement.MoveSubElement(element2, element);
						element = element2;
					}
				}
				foreach (var item5 in from c in deltaElement.GetSubElements(ElementType.Change)
					where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Deletion.ToString()
					let targetKey = ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target))
					join se in resultElement.GetSubElements(relation.ChildType) on targetKey equals se.ElementKey
					select new
					{
						SubElement = se,
						Change = c
					})
				{
					if (!options.DevBaseCompatibility)
					{
						IElement element3 = item5.Change.GetSubElements(relation.ChildType).Single();
						if (!CompareElements(item5.SubElement.Clone(), element3.Clone()))
						{
							AddConflict(item5.Change, NonLocalizable.ConflictHint_DeleteOfModifiedTarget, ConflictSeverityType.Medium);
							continue;
						}
					}
					processedChanges.Add(item5.Change);
					resultElement.RemoveSubElement(item5.SubElement);
				}
				foreach (IElement item6 in from c in deltaElement.GetSubElements(ElementType.Change)
					where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Insertion.ToString() && c.GetSubElementsCount(relation.ChildType) > 0 && ((!c.GetPropertySetState(PropertyType.Target) && resultElement.Parent == null) || ElementKeyHelper.DisplayElementKeyToElementKey(c.GetStringProperty(PropertyType.Target)) == resultElement.ElementKey)
					select c)
				{
					processedChanges.Add(item6);
					IElement element4 = null;
					if (item6.GetPropertySetState(PropertyType.InsertAfter))
					{
						element4 = FindSubElementOfTypeByElementKeyNoParent(resultElement, relation.ChildType, item6.GetStringProperty(PropertyType.InsertAfter));
						if (element4 == null)
						{
							AddConflict(item6, NonLocalizable.ConflictHint_InsertAfterTargetNotFound, CalcConflictSeverityForOrderingConflict(relation));
						}
						if (element4 == null)
						{
							element4 = resultElement.GetSubElements(relation.ChildType).LastOrDefault();
						}
					}
					foreach (IElement subElement in item6.GetSubElements(relation.ChildType))
					{
						IElement element5 = FindSubElementOfTypeByElementKeyNoParent(resultElement, relation.ChildType, subElement.CreateDisplayElementKey(includeParent: false));
						IElement element6 = subElement.Clone();
						if (element5 == null)
						{
							resultElement.InsertSubElement(element6, element4);
							element4 = element6;
						}
						else
						{
							if (CompareElements(element5.Clone(), element6))
							{
								continue;
							}
							if (element5.ElementTypeInfo.Category == CategoryType.Trigger && options.InlineCodeConflicts)
							{
								CodeLinesHelper.MergeCode(new List<string>(), element5.GetCodeLines().ToList(), subElement.GetCodeLines().ToList(), out var mergedCode, GetMergeOutputMode(element5, out var ignoreConflict));
								element5.SetCodeLines(mergedCode);
								foreach (IElement subElement2 in element6.GetSubElements(ElementType.Variable))
								{
									IElement left = subElement2.Clone();
									IElement element7 = FindSubElementOfTypeByElementKeyNoParent(element5, ElementType.Variable, subElement2.CreateDisplayElementKey(includeParent: false));
									if (element7 == null || !CompareElements(left, element7.Clone()))
									{
										element5.AddSubElement(subElement2.Clone());
									}
								}
								if (!ignoreConflict)
								{
									AddCodeConflict(element5.CreateDisplayElementKey());
								}
							}
							else
							{
								AddConflict(item6, NonLocalizable.ConflictHint_DoubleInsertOfElement, ConflictSeverityType.Medium);
							}
						}
					}
				}
				foreach (IElement subElement3 in resultElement.GetSubElements(relation.ChildType))
				{
					ApplyElementChanges(subElement3, deltaElement);
				}
			}
		}
		if (options.AutoAlignControlIndent)
		{
			ValidateControlIndent(resultElement);
		}
	}

	private static void ValidateControlIndent(IElement resultElement)
	{
		string value = ((resultElement != null && resultElement.SupportsPropertyType(PropertyType.ControlType)) ? resultElement.GetStringProperty(PropertyType.ControlType) : null);
		if (resultElement.ElementTypeInfo.ElementType != ElementType.Control || "Container".Equals(value, StringComparison.OrdinalIgnoreCase) || !IsOnPage(resultElement))
		{
			return;
		}
		List<IElement> list = resultElement.Parent.GetSubElements(ElementType.Control).ToList();
		int num = list.IndexOf(resultElement);
		if (num < 0)
		{
			return;
		}
		int? elementLevel = GetElementLevel(resultElement);
		if (!elementLevel.HasValue)
		{
			return;
		}
		IElement element = ((num > 0) ? list[num - 1] : null);
		if (element != null && element.ElementTypeInfo.ElementType != ElementType.Control)
		{
			element = null;
		}
		IElement element2 = ((num < list.Count - 1) ? list[num + 1] : null);
		if (element2 != null && element2.ElementTypeInfo.ElementType != ElementType.Control)
		{
			element2 = null;
		}
		int? num2 = GetElementLevel(element);
		if (element != null && !num2.HasValue)
		{
			num2 = 0;
		}
		int? elementLevel2 = GetElementLevel(element2);
		string value2 = element?.GetStringProperty(PropertyType.ControlType);
		int num3 = 1;
		int val;
		if (num2.HasValue)
		{
			val = num2.Value;
			if ("Container".Equals(value2, StringComparison.OrdinalIgnoreCase) || "Group".Equals(value2, StringComparison.OrdinalIgnoreCase))
			{
				val = num2.Value + 1;
			}
		}
		else
		{
			val = 0;
		}
		if (elementLevel2.HasValue)
		{
			num3 = elementLevel2.Value;
			if ("Container".Equals(value, StringComparison.OrdinalIgnoreCase) || "Group".Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				num3 = elementLevel2.Value - 1;
			}
			if (num3 < 1)
			{
				num3 = 1;
			}
		}
		int value3 = Math.Min(val, Math.Max(num3, elementLevel.Value));
		resultElement.SetInt32Property(PropertyType.Level, value3);
	}

	private static int? GetElementLevel(IElement element)
	{
		return element?.GetInt32Property(PropertyType.Level);
	}

	private static bool IsOnPage(IElement element)
	{
		bool result = false;
		while (element != null)
		{
			if (element.ElementTypeInfo.ElementType == ElementType.Page)
			{
				result = true;
				break;
			}
			element = element.Parent;
		}
		return result;
	}

	private void MergeCode(IElement resultElement, IElement originalCode, IElement modifiedCode, string targetKey)
	{
		bool commentedOut;
		if (CodeLinesHelper.MergeCode(originalCode.GetCodeLines().ToList(), resultElement.GetCodeLines().ToList(), CodeLinesHelper.ExpandCompressed(modifiedCode.GetCodeLines(), originalCode.GetCodeLines()).ToList(), out var mergedCode, GetMergeOutputMode(resultElement, out var ignoreConflict)))
		{
			resultElement.SetCodeLines(MakeCodeImportable(mergedCode.ToArray(), resultElement.ElementTypeInfo.ElementType, out commentedOut));
			if (commentedOut && !ignoreConflict)
			{
				AddCodeConflict(targetKey, NonLocalizable.ConflictHint_CodeIsMergedButIsOutCommented);
			}
		}
		else if (!options.DevBaseCompatibility && options.InlineCodeConflicts)
		{
			resultElement.SetCodeLines(MakeCodeImportable(mergedCode.ToArray(), resultElement.ElementTypeInfo.ElementType, out commentedOut));
			if (!ignoreConflict)
			{
				AddCodeConflict(targetKey);
			}
		}
		else
		{
			AddCodeConflict(targetKey, ChangeTypes.CodeModification, originalCode.GetCodeLines(), modifiedCode.GetCodeLines(), resultElement.GetCodeLines());
		}
	}

	private static PropertyType FindAndCheckPropertyFromName(IElement element, string propertyName)
	{
		if (!element.ElementTypeInfo.Properties.TryGetValue(propertyName, out var value))
		{
			throw new GenericElementException(NonLocalizable.ModelChanges_UnknownOrUnsupportedProperty, propertyName, element.ElementTypeInfo.Name);
		}
		return value.Property.PropertyType;
	}

	private static IElement FindSubElementOfTypeByElementKeyNoParent(IElement parent, ElementType childType, string elementKeyNoParent)
	{
		elementKeyNoParent = ElementKeyHelper.DisplayElementKeyToElementKey(elementKeyNoParent);
		return parent.GetSubElements(childType).FirstOrDefault((IElement e) => e.ElementKeyNoParent == elementKeyNoParent);
	}

	private void AddConflict(IElement conflict, ConflictSeverityType conflictSeverity)
	{
		if (conflictSeverity >= options.ConflictSeverityLevel)
		{
			if (conflictRootElement == null)
			{
				conflictRootElement = GenericElement.Create(ElementType.Modification);
			}
			conflictRootElement.AddSubElement(conflict);
		}
	}

	private void AddConflict(IElement change, string hint, ConflictSeverityType conflictSeverity)
	{
		IElement element = change.Clone();
		element.SetStringProperty(PropertyType.ConflictHint, hint);
		AddConflict(element, conflictSeverity);
	}

	private void AddCodeConflict(string target, string hint = null)
	{
		IElement element = GenericElement.Create(ElementType.Change);
		element.SetStringProperty(PropertyType.Target, target);
		element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.CodeModification.ToString());
		element.SetStringProperty(PropertyType.ConflictHint, hint ?? NonLocalizable.ConflictHint_InlineCodeConflict);
		AddConflict(element, ConflictSeverityType.High);
	}

	private void AddCodeConflict(string targetKey, ChangeTypes changeType, IEnumerable<string> original, IEnumerable<string> modified, IEnumerable<string> target)
	{
		IElement element = GenericElement.Create(ElementType.Change);
		element.SetStringProperty(PropertyType.Target, targetKey);
		element.SetStringProperty(PropertyType.ChangeType, changeType.ToString());
		IElement element2 = GenericElement.Create(ElementType.OriginalCode);
		element2.SetCodeLines(original);
		element.AddSubElement(element2);
		element2 = GenericElement.Create(ElementType.ModifiedCode);
		element2.SetCodeLines(modified);
		element.AddSubElement(element2);
		element2 = GenericElement.Create(ElementType.TargetCode);
		element2.SetCodeLines(target);
		element.AddSubElement(element2);
		AddConflict(element, ConflictSeverityType.High);
	}

	private void AddPropertyConflict(PropertyType propertyType, IElement change, IElement target, string hint, ConflictSeverityType conflictSeverity)
	{
		IElement element = change.Clone();
		element.SetStringProperty(PropertyType.TargetValue, target.GetStringProperty(propertyType));
		element.SetStringProperty(PropertyType.ConflictHint, hint);
		AddConflict(element, conflictSeverity);
	}

	private bool CompareElements(IElement left, IElement right)
	{
		return new ComputeChanges(left, new ComputeChangesOptions
		{
			DevBaseCompatibility = options.DevBaseCompatibility
		}).Compute(right) == null;
	}

	private MergeOutputMode GetMergeOutputMode(IElement element, out bool ignoreConflict)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Documentation)
		{
			ignoreConflict = true;
			switch (options.DocumentationConflict)
			{
			case DocumentationConflictAction.ModifiedFirst:
				return MergeOutputMode.ModifiedFirst;
			case DocumentationConflictAction.TargetFirst:
				return MergeOutputMode.TargetFirst;
			}
		}
		ignoreConflict = false;
		return MergeOutputMode.Normal;
	}

	private IEnumerable<string> MakeCodeImportable(string[] code, ElementType triggerType, out bool commentedOut)
	{
		commentedOut = false;
		if (options.SkipCodeValidation || triggerType == ElementType.Documentation)
		{
			return code;
		}
		try
		{
			Parser.ParseFunctionBody(code);
			return code;
		}
		catch (ALParseException)
		{
			string[] array = new string[code.Length + 2];
			array[0] = NonLocalizable.UnimportableCode_Line1;
			array[1] = NonLocalizable.UnimportableCode_Line2;
			for (int i = 0; i < code.Length; i++)
			{
				array[i + 2] = "//" + code[i];
			}
			commentedOut = true;
			return array;
		}
	}

	private void HandleUnprocessedChanges(IElement resultRootElement, IElement deltaRootElement)
	{
		foreach (IElement item in from c in deltaRootElement.GetSubElements(ElementType.Change).Except(processedChanges)
			where c.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Deletion.ToString()
			select c)
		{
			processedChanges.Add(item);
		}
		foreach (IElement item2 in deltaRootElement.GetSubElements(ElementType.Change).Except(processedChanges))
		{
			if (item2.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.CodeModification.ToString())
			{
				IElementKeySegment[] array = ElementKeyHelper.GetElementKeySegments(ElementKeyHelper.DisplayElementKeyToElementKey(item2.GetStringProperty(PropertyType.Target))).ToArray();
				if (array.Length != 0 && ElementTypeInfo.Lookup.TryGetValue(array[0].ElementType, out var value) && value.Category == CategoryType.Trigger)
				{
					IElement element = resultRootElement.FindElement(array.Skip(1));
					if (element != null)
					{
						IElement element2 = GenericElement.Create(value.ElementType);
						element.AddSubElement(element2);
						MergeCode(element2, item2.GetFirstSubElement(ElementType.OriginalCode), item2.GetFirstSubElement(ElementType.ModifiedCode), element2.CreateDisplayElementKey());
						continue;
					}
				}
			}
			AddConflict(item2, NonLocalizable.ConflictHint_Unprocessed, ConflictSeverityType.Medium);
		}
	}

	private static ConflictSeverityType CalcConflictSeverityForOrderingConflict(ElementRelation relation)
	{
		ElementType childType = relation.ChildType;
		if (childType == ElementType.Parameter || childType == ElementType.Control)
		{
			return ConflictSeverityType.Medium;
		}
		return ConflictSeverityType.Low;
	}
}
