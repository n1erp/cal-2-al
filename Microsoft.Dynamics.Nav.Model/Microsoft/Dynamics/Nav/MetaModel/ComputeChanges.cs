using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class ComputeChanges
{
	[DebuggerDisplay("{Line} {Marked} {Code}")]
	private class CodeLineInfo
	{
		internal int Line { get; set; }

		internal bool Marked { get; set; }

		internal string Code { get; set; }
	}

	private readonly ComputeChangesOptions options;

	private readonly IElement originalRootElement;

	private const string lineStartPattern = "\\[LineStart\\(\\d+\\)\\]";

	private static readonly Regex lineStartRegex = new Regex("^\\[LineStart\\(\\d+\\)\\]$", RegexOptions.Compiled);

	internal static readonly ComputeChangesOptions DefaultOptions = new ComputeChangesOptions();

	public ComputeChanges(IElement originalRootElement)
		: this(originalRootElement, DefaultOptions)
	{
	}

	public ComputeChanges(IElement originalRootElement, ComputeChangesOptions options)
	{
		if (originalRootElement == null)
		{
			throw new ArgumentNullException("originalRootElement");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		this.originalRootElement = originalRootElement;
		this.options = options;
	}

	public IElement Compute(IElement modifiedRootElement)
	{
		IElement element = GenericElement.Create(ElementType.Modification);
		element.SetStringProperty(PropertyType.Name, originalRootElement.CreateDisplayElementKey());
		element.SetStringProperty(PropertyType.Target, originalRootElement.CreateDisplayElementKey());
		if (originalRootElement.TryGetStringProperty(PropertyType.PageType, out var value))
		{
			element.SetStringProperty(PropertyType.TargetPageType, value);
		}
		if (modifiedRootElement == null)
		{
			element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.Deletion.ToString());
			return element;
		}
		if (originalRootElement.ElementKey != modifiedRootElement.ElementKey)
		{
			throw new GenericElementException(NonLocalizable.ModelChanges_AppObjsMustHaveSameTypeAndId);
		}
		ComputeElementChanges(originalRootElement, modifiedRootElement, element);
		if (originalRootElement.ElementTypeInfo.ElementType == ElementType.Report)
		{
			ComputeRdlChanges(originalRootElement, modifiedRootElement, element);
			ComputeWordLayoutChanges(originalRootElement, modifiedRootElement, element);
		}
		if (!element.GetSubElements().Any())
		{
			return null;
		}
		return element;
	}

	private void ComputeElementChanges(IElement original, IElement modified, IElement delta)
	{
		if (original.ElementTypeInfo.Relations != null)
		{
			foreach (ElementRelation item in options.IgnoreDocumentationChanges ? original.ElementTypeInfo.Relations.Values.Where((ElementRelation r) => r.Child.ElementType != ElementType.Documentation) : original.ElementTypeInfo.Relations.Values)
			{
				List<Tuple<int, int>> originalSequence = new List<Tuple<int, int>>();
				List<Tuple<int, int>> modifiedSequence = new List<Tuple<int, int>>();
				List<Tuple<int, int, int>> matchSequence = new List<Tuple<int, int, int>>();
				int num = 0;
				int num2 = 0;
				int matchOriginal = 0;
				int matchModified = 0;
				int num3 = 0;
				IElement element = null;
				IElement element2 = null;
				foreach (IElement originalElement in original.GetSubElements(item.ChildType))
				{
					IElement element3 = modified.GetSubElements(item.ChildType).FirstOrDefault((IElement element13) => element13.ElementKeyHash == originalElement.ElementKeyHash);
					if (element3 != null)
					{
						originalSequence.Add(new Tuple<int, int>(++num, originalElement.ElementKeyHash));
						ComputeElementChanges(originalElement, element3, delta);
						continue;
					}
					IElement element4 = GenericElement.Create(ElementType.Change);
					element4.SetStringProperty(PropertyType.ChangeType, ChangeTypes.Deletion.ToString());
					if (!options.DevBaseCompatibility)
					{
						element4.AddSubElement(originalElement.Clone());
						ElementRelation value = ElementTypeInfo.Lookup.GetValue(ElementType.Change).Relations.GetValue(originalElement.ElementTypeInfo.ElementType);
						if (!string.IsNullOrEmpty(value.ContainerName))
						{
							element4.SetStringProperty(PropertyType.AddedChildren, value.ContainerName);
						}
					}
					if (originalElement.ElementTypeInfo.Category != CategoryType.Object)
					{
						element4.SetStringProperty(PropertyType.Target, originalElement.CreateDisplayElementKey());
					}
					delta.AddSubElement(element4);
				}
				List<IElement> list = new List<IElement>();
				foreach (IElement modifiedElement in modified.GetSubElements(item.ChildType))
				{
					IElement element5 = original.GetSubElements(item.ChildType).FirstOrDefault((IElement element13) => element13.ElementKeyHash == modifiedElement.ElementKeyHash);
					if (element5 != null)
					{
						modifiedSequence.Add(new Tuple<int, int>(++num2, modifiedElement.ElementKeyHash));
						Tuple<int, int> tuple = originalSequence.Find((Tuple<int, int> s) => s.Item2 == modifiedElement.ElementKeyHash);
						if (matchSequence.Count > 0 && tuple != null && matchModified + num3 == num2 && matchOriginal + num3 == tuple.Item1)
						{
							matchSequence.Remove(matchSequence.Find((Tuple<int, int, int> t) => t.Item1 == matchOriginal && t.Item2 == matchModified));
							matchSequence.Add(new Tuple<int, int, int>(matchOriginal, matchModified, ++num3));
						}
						else
						{
							matchOriginal = tuple.Item1;
							matchModified = num2;
							num3 = 1;
							matchSequence.Add(new Tuple<int, int, int>(matchOriginal, matchModified, num3));
						}
						if (element != null)
						{
							element = null;
						}
						element2 = element5;
						if (options.ExportToNewSyntax && element5.TryGetInt32Property(PropertyType.Level, out var value2))
						{
							if (!value2.HasValue)
							{
								value2 = 0;
							}
							if (list.Count > value2)
							{
								list = list.GetRange(0, value2.Value);
							}
							list.Add(element5);
						}
					}
					else if (element == null)
					{
						IElement element6 = GenericElement.Create(ElementTypeInfo.Lookup.GetValue(ElementType.Change));
						element6.AddSubElement(modifiedElement.Clone());
						element6.SetStringProperty(PropertyType.ChangeType, ChangeTypes.Insertion.ToString());
						if (modified.ElementTypeInfo.Category != CategoryType.Object)
						{
							element6.SetStringProperty(PropertyType.Target, modified.CreateDisplayElementKey());
						}
						if (options.ExportToNewSyntax && modifiedElement.TryGetInt32Property(PropertyType.Level, out var value3) && value3.HasValue)
						{
							if (value3.Value >= list.Count)
							{
								if (list.Count == 0)
								{
									element6.SetStringProperty(PropertyType.InsertAfter, "AddFirst:" + GetDisplayElementKey(element2));
								}
								else
								{
									IElement element7 = list[value3.Value - 1];
									element6.SetStringProperty(PropertyType.InsertAfter, "AddFirst:" + GetDisplayElementKey(element7));
								}
							}
							else
							{
								IElement element8 = list[value3.Value];
								element6.SetStringProperty(PropertyType.InsertAfter, "AddAfter:" + GetDisplayElementKey(element8));
							}
						}
						else if (element2 != null)
						{
							element6.SetStringProperty(PropertyType.InsertAfter, element2.CreateDisplayElementKey(includeParent: false));
						}
						ElementRelation value4 = ElementTypeInfo.Lookup.GetValue(ElementType.Change).Relations.GetValue(modifiedElement.ElementTypeInfo.ElementType);
						if (!string.IsNullOrEmpty(value4.ContainerName))
						{
							element6.SetStringProperty(PropertyType.AddedChildren, value4.ContainerName);
						}
						delta.AddSubElement(element6);
						element = element6;
					}
					else
					{
						element.AddSubElement(modifiedElement.Clone());
					}
				}
				if (matchSequence.Count <= 1)
				{
					continue;
				}
				for (int i = 0; i < matchSequence.Count; i++)
				{
					for (int num4 = i + 1; num4 < matchSequence.Count; num4++)
					{
						if (matchSequence[i].Item1 <= matchSequence[num4].Item1)
						{
							continue;
						}
						IElement element9 = GenericElement.Create(ElementTypeInfo.Lookup.GetValue(ElementType.Change));
						element9.SetStringProperty(PropertyType.ChangeType, ChangeTypes.Move.ToString());
						IElement element10 = original.GetSubElements(item.ChildType).First((IElement b) => b.ElementKeyHash == originalSequence.Find((Tuple<int, int> s) => s.Item1 == matchSequence[i].Item1).Item2);
						element9.SetStringProperty(PropertyType.Target, element10.CreateDisplayElementKey());
						int index = modifiedSequence.FindIndex((Tuple<int, int> s) => s.Item1 == matchSequence[i].Item2);
						if (index > 0)
						{
							IElement element11 = modified.GetSubElements(item.ChildType).First((IElement b) => b.ElementKeyHash == modifiedSequence[index - 1].Item2);
							element9.SetStringProperty(PropertyType.InsertAfter, element11.CreateDisplayElementKey(includeParent: false));
						}
						if (matchSequence[i].Item3 > 1)
						{
							StringBuilder stringBuilder = new StringBuilder();
							int k;
							for (k = 1; k < matchSequence[i].Item3; k++)
							{
								IElement element12 = modified.GetSubElements(item.ChildType).First((IElement b) => b.ElementKeyHash == modifiedSequence[index + k].Item2);
								stringBuilder.Append(element12.CreateDisplayElementKey(includeParent: false));
								stringBuilder.Append(',');
							}
							stringBuilder.Length--;
							element9.SetStringProperty(PropertyType.IncludeSiblings, stringBuilder.ToString());
						}
						delta.AddSubElement(element9);
						break;
					}
				}
			}
		}
		ComputePropertyChanges(original, modified, delta);
		ComputeCodeChanges(original, modified, delta);
	}

	private static string GetDisplayElementKey(IElement element)
	{
		if (element == null)
		{
			return string.Empty;
		}
		string text = element.CreateDisplayElementKey(includeParent: false);
		if (IsControlArea(element))
		{
			int startIndex = text.IndexOf("(Control", StringComparison.Ordinal);
			text = element.GetStringProperty(PropertyType.ContainerType) + text.Substring(startIndex);
		}
		if (IsActionArea(element))
		{
			int startIndex2 = text.IndexOf("(Action", StringComparison.Ordinal);
			text = element.GetStringProperty(PropertyType.ActionContainerType) + text.Substring(startIndex2);
		}
		return text;
	}

	private static bool IsActionArea(IElement element)
	{
		if (element.GetPropertySetState(PropertyType.ActionContainerType))
		{
			return string.Equals(element.GetStringProperty(PropertyType.ActionType), "ActionContainer", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsControlArea(IElement element)
	{
		if (element.GetPropertySetState(PropertyType.ContainerType))
		{
			return string.Equals(element.GetStringProperty(PropertyType.ControlType), "Container", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static void ComputePropertyChanges(IElement original, IElement modified, IElement delta)
	{
		foreach (PropertyType property in original.GetProperties())
		{
			string propertyValue2;
			if (modified.GetPropertySetState(property))
			{
				if (IsLineStartAttributeChange(property, original, out var propertyValue))
				{
					continue;
				}
				string stringProperty = modified.GetStringProperty(property);
				propertyValue = SanitizePropertyValue(property, propertyValue);
				stringProperty = SanitizePropertyValue(property, stringProperty);
				if (propertyValue != stringProperty)
				{
					IElement element = GenericElement.Create(ElementType.Change);
					element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyModification.ToString());
					if (modified.ElementTypeInfo.Category != CategoryType.Object)
					{
						element.SetStringProperty(PropertyType.Target, original.CreateDisplayElementKey());
					}
					element.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(property));
					element.SetStringProperty(PropertyType.OriginalValue, propertyValue);
					element.SetStringProperty(PropertyType.ModifiedValue, stringProperty);
					delta.AddSubElement(element);
				}
			}
			else if (!IsLineStartAttributeChange(property, original, out propertyValue2))
			{
				propertyValue2 = SanitizePropertyValue(property, propertyValue2);
				IElement element2 = GenericElement.Create(ElementType.Change);
				element2.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyDeletion.ToString());
				if (modified.ElementTypeInfo.Category != CategoryType.Object)
				{
					element2.SetStringProperty(PropertyType.Target, original.CreateDisplayElementKey());
				}
				element2.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(property));
				element2.SetStringProperty(PropertyType.OriginalValue, propertyValue2);
				delta.AddSubElement(element2);
			}
		}
		foreach (PropertyType property2 in modified.GetProperties())
		{
			if (!original.GetPropertySetState(property2) && !IsLineStartAttributeChange(property2, modified, out var propertyValue3))
			{
				propertyValue3 = SanitizePropertyValue(property2, propertyValue3);
				IElement element3 = GenericElement.Create(ElementType.Change);
				element3.SetStringProperty(PropertyType.ChangeType, ChangeTypes.PropertyInsertion.ToString());
				if (modified.ElementTypeInfo.Category != CategoryType.Object)
				{
					element3.SetStringProperty(PropertyType.Target, original.CreateDisplayElementKey());
				}
				element3.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(property2));
				element3.SetStringProperty(PropertyType.ModifiedValue, propertyValue3);
				delta.AddSubElement(element3);
			}
		}
	}

	private static bool IsLineStartAttributeChange(PropertyType propertyType, IElement element, out string propertyValue)
	{
		propertyValue = element.GetStringProperty(propertyType);
		if (propertyType == PropertyType.Attributes)
		{
			return lineStartRegex.IsMatch(propertyValue);
		}
		return false;
	}

	private static string SanitizePropertyValue(PropertyType propertyType, string propertyValue)
	{
		if (propertyType == PropertyType.Attributes)
		{
			return Regex.Replace(propertyValue, "\\[LineStart\\(\\d+\\)\\]", string.Empty, RegexOptions.Compiled);
		}
		return propertyValue;
	}

	private void ComputeCodeChanges(IElement original, IElement modified, IElement delta)
	{
		if (original.GetCodeLinesCount() != modified.GetCodeLinesCount() || !CodeLinesHelper.CompareLines(original.GetCodeLines(), modified.GetCodeLines()))
		{
			ComputeTextChanges(original.GetCodeLines().ToArray(), original, modified.GetCodeLines().ToArray(), delta, ChangeTypes.CodeModification);
		}
	}

	private void ComputeTextChanges(string[] originalLines, IElement original, string[] modifiedLines, IElement delta, ChangeTypes changeType)
	{
		IElement element = GenericElement.Create(ElementType.Change);
		element.SetStringProperty(PropertyType.ChangeType, changeType.ToString());
		if (original.ElementTypeInfo.Category != CategoryType.Object)
		{
			element.SetStringProperty(PropertyType.Target, original.CreateDisplayElementKey());
		}
		bool flag = options.UseCodeCompression && changeType != ChangeTypes.WordLayoutModification;
		string[] originalCode = originalLines;
		string[] modifiedCode = modifiedLines;
		string[] left = modifiedCode;
		CompressCodelines(ref originalCode, ref modifiedCode, flag);
		if (flag)
		{
			if (!CodeLinesHelper.MergeCode(originalCode, originalLines, CodeLinesHelper.ExpandCompressed(modifiedCode, originalCode).ToList(), out var mergedCode) || !CodeLinesHelper.CompareLines(left, mergedCode))
			{
				originalCode = originalLines;
				if (!CodeLinesHelper.MergeCode(originalCode, originalLines, CodeLinesHelper.ExpandCompressed(modifiedCode, originalCode).ToList(), out mergedCode) || !CodeLinesHelper.CompareLines(left, mergedCode))
				{
					throw new GenericElementException(NonLocalizable.ModelChanges_CodeCompressionCheck, original.ElementKey);
				}
			}
		}
		AddCodeElement(element, ElementType.OriginalCode, originalCode);
		AddCodeElement(element, ElementType.ModifiedCode, modifiedCode);
		delta.AddSubElement(element);
	}

	private void ComputeRdlChanges(IElement original, IElement modified, IElement delta)
	{
		if (original.GetRdlDataLinesCount() != modified.GetRdlDataLinesCount() || !CodeLinesHelper.CompareLines(original.GetRdlDataLines(), modified.GetRdlDataLines()))
		{
			ComputeTextChanges(original.GetRdlDataLines().ToArray(), original, modified.GetRdlDataLines().ToArray(), delta, ChangeTypes.RdlModification);
		}
	}

	private void ComputeWordLayoutChanges(IElement original, IElement modified, IElement delta)
	{
		if (original.GetWordLayoutLinesCount() != modified.GetWordLayoutLinesCount() || !CodeLinesHelper.CompareLines(original.GetWordLayoutLines(), modified.GetWordLayoutLines()))
		{
			ComputeTextChanges(original.GetWordLayoutLines().ToArray(), original, modified.GetWordLayoutLines().ToArray(), delta, ChangeTypes.WordLayoutModification);
		}
	}

	internal static void AddCodeElement(IElement targetElement, ElementType type, IEnumerable<string> codeLines)
	{
		IElement element = GenericElement.Create(type);
		element.SetCodeLines(codeLines);
		targetElement.AddSubElement(element);
	}

	private static void CompressCodelines(ref string[] originalCode, ref string[] modifiedCode, bool compressOriginal)
	{
		int line = 0;
		List<CodeLineInfo> list = originalCode.Select((string oc) => new CodeLineInfo
		{
			Line = ++line,
			Code = oc,
			Marked = true
		}).ToList();
		line = 0;
		var m = modifiedCode.Select((string mc) => new
		{
			Line = ++line,
			Code = mc
		}).ToList();
		List<int> list2 = new List<int>();
		if (m.Count == 0)
		{
			return;
		}
		int modifiedIndex = 0;
		int startIndex = 0;
		int num;
		while (modifiedIndex < m.Count)
		{
			if ((num = list.FindIndex(startIndex, (CodeLineInfo oc) => oc.Marked && oc.Code == m[modifiedIndex].Code)) >= 0)
			{
				startIndex = num;
				int num2 = modifiedIndex;
				int num3 = 0;
				do
				{
					num3++;
				}
				while (++modifiedIndex != m.Count && ++num < list.Count && list[num].Code == m[modifiedIndex].Code);
				if (num3 > 2)
				{
					list2.Add(list[startIndex].Line);
					m.RemoveRange(num2, num3);
					m.Insert(num2, new
					{
						Line = 0,
						Code = string.Format(CultureInfo.InvariantCulture, "#{0}..{1}", list[startIndex].Line, list[startIndex].Line + num3 - 1)
					});
					for (int num4 = startIndex; num4 < startIndex + num3; num4++)
					{
						list[num4].Marked = false;
					}
					modifiedIndex = num2;
					startIndex = 0;
				}
				else
				{
					modifiedIndex -= num3;
					startIndex++;
				}
			}
			else
			{
				modifiedIndex++;
				startIndex = 0;
			}
		}
		modifiedCode = m.Select(rc => rc.Code).ToArray();
		if (!compressOriginal)
		{
			return;
		}
		num = 0;
		bool flag = false;
		if (list2.Count <= 0)
		{
			return;
		}
		while (num < list.Count)
		{
			startIndex = num;
			bool marked = list[num].Marked;
			int num5 = 0;
			do
			{
				num5++;
			}
			while (++num < list.Count && marked == list[num].Marked && !list2.Contains(list[num].Line));
			startIndex += 3;
			num5 -= 6;
			if (num5 > 2)
			{
				list.Insert(startIndex, new CodeLineInfo
				{
					Line = 0,
					Code = string.Format(CultureInfo.InvariantCulture, "#{0}..{1}", list[startIndex].Line, list[startIndex].Line + num5 - 1)
				});
				list.RemoveRange(startIndex + 1, num5);
				num = startIndex + 3;
				flag = true;
			}
		}
		if (flag)
		{
			originalCode = list.Select((CodeLineInfo bc) => bc.Code).ToArray();
		}
	}
}
