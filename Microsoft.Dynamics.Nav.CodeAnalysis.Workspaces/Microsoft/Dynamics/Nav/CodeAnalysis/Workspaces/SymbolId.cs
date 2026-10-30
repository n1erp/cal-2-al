using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SymbolId
{
	private class ListPool<T> : ObjectPool<List<T>>
	{
		public ListPool()
			: base((Factory)(() => new List<T>(10)), 10)
		{
		}

		public void ClearAndFree(List<T> list)
		{
			list.Clear();
			Free(list);
		}
	}

	private struct Generator(StringBuilder builder)
	{
		private readonly StringBuilder builder = builder;

		public bool Visit(ISymbol symbol)
		{
			if (symbol == null || symbol.Kind == SymbolKind.Module)
			{
				return false;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.ArrayType:
				VisitArrayType((ArrayTypeSymbol)symbol);
				break;
			case SymbolKind.Method:
				VisitMethod((IMethodSymbol)symbol);
				break;
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
			case SymbolKind.Class:
			case SymbolKind.Table:
			case SymbolKind.Codeunit:
			case SymbolKind.Page:
			case SymbolKind.Report:
			case SymbolKind.Query:
			case SymbolKind.XmlPort:
			case SymbolKind.PageExtension:
			case SymbolKind.TableExtension:
			case SymbolKind.ControlAddIn:
			case SymbolKind.DotNetTypeDeclaration:
			case SymbolKind.PageCustomization:
			case SymbolKind.Enum:
			case SymbolKind.EnumExtension:
			case SymbolKind.Event:
			case SymbolKind.Parameter:
			case SymbolKind.Field:
			case SymbolKind.Option:
			case SymbolKind.Property:
			case SymbolKind.Key:
			case SymbolKind.FieldGroup:
			case SymbolKind.OptionType:
			case SymbolKind.Control:
			case SymbolKind.Action:
			case SymbolKind.EnumValue:
			case SymbolKind.View:
			case SymbolKind.TestPage:
			case SymbolKind.TestField:
			case SymbolKind.TestAction:
			case SymbolKind.TestRequestPage:
			case SymbolKind.TestPart:
			case SymbolKind.TestFilter:
			case SymbolKind.TestFilterField:
			case SymbolKind.Change:
			case SymbolKind.QueryDataItem:
			case SymbolKind.QueryFilter:
			case SymbolKind.QueryColumn:
			case SymbolKind.ReportDataItem:
			case SymbolKind.ReportColumn:
			case SymbolKind.RequestPage:
			case SymbolKind.XmlPortNode:
			case SymbolKind.DotNetAssembly:
			case SymbolKind.DotNetPackage:
			case SymbolKind.DotNet:
				VisitSymbol(symbol);
				break;
			case SymbolKind.GlobalVariable:
			case SymbolKind.LocalVariable:
				VisitVariable((VariableSymbol)symbol);
				break;
			case SymbolKind.ReturnValue:
				VisitReturnValue((ReturnValueSymbol)symbol);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
			return true;
		}

		private void EncodeSymbolKind(ISymbol symbol)
		{
			builder.Append(':');
			builder.Append(symbol.Kind);
			builder.Append(':');
		}

		private void VisitSymbol(ISymbol symbol)
		{
			if (Visit(symbol.ContainingSymbol))
			{
				builder.Append(".");
			}
			EncodeSymbolKind(symbol);
			builder.Append(EncodeName(symbol.Name));
		}

		private void VisitMethod(IMethodSymbol symbol)
		{
			if (Visit(symbol.ContainingSymbol))
			{
				builder.Append(".");
			}
			EncodeSymbolKind(symbol);
			builder.Append(EncodeName(symbol.Name));
		}

		private void VisitArrayType(ArrayTypeSymbol symbol)
		{
			Visit(symbol.ElementType);
			builder.Append("[");
			for (int i = 0; i < symbol.Dimensions.Length; i++)
			{
				builder.Append(symbol.Dimensions[i]);
				if (i > 0)
				{
					builder.Append(",");
				}
			}
			builder.Append("]");
		}

		private void VisitVariable(VariableSymbol symbol)
		{
			if (Visit(symbol.ContainingSymbol))
			{
				builder.Append(".");
			}
			EncodeSymbolKind(symbol);
			builder.Append(symbol.Name);
			EncodeOccurrence(symbol);
		}

		private void VisitReturnValue(ReturnValueSymbol symbol)
		{
			EncodeSymbolKind(symbol);
			builder.Append(EncodeName(symbol.Name));
		}

		private void EncodeOccurrence(ISymbol symbol)
		{
			int interiorSymbolOccurrence = GetInteriorSymbolOccurrence(symbol);
			if (interiorSymbolOccurrence > 0)
			{
				builder.Append('`');
				builder.Append(interiorSymbolOccurrence);
			}
		}
	}

	private struct Parser
	{
		private readonly string id;

		private readonly Compilation compilation;

		private int index;

		private static readonly char[] nameDelimiters = new char[7] { '.', '(', ')', '[', ']', ',', '`' };

		private Parser(string id, int index, Compilation compilation)
		{
			this.id = id;
			this.compilation = compilation;
			this.index = index;
		}

		public static bool Parse(string id, Compilation compilation, List<ISymbol> results)
		{
			if (id == null)
			{
				return false;
			}
			results.Clear();
			new Parser(id, 0, compilation).ParseSymbolId(results);
			return results.Count > 0;
		}

		private void ParseSymbolId(List<ISymbol> results)
		{
			if (IsNamePrefix())
			{
				ParseNamedSymbol(results);
			}
			if (PeekNextChar() == '[')
			{
				ParseArrayTypes(results);
			}
		}

		private void ParseNamedSymbol(List<ISymbol> results)
		{
			List<ISymbol> list = symbolListPool.Allocate();
			try
			{
				list.Add(compilation.CompiledModule);
				if ((object)compilation.CompiledModule != NavCorLib.Instance)
				{
					list.Add(NavCorLib.Instance);
				}
				list.AddRange(compilation.ReferenceManager.GetLoadedModules());
				while (true)
				{
					ParsePrefixedName(out var prefix, out var name);
					switch (prefix)
					{
					case SymbolKind.Method:
						GetMatchingMethods(list, name, results);
						break;
					case SymbolKind.Property:
						GetMatchingProperties(list, name, results);
						break;
					case SymbolKind.OptionType:
						GetMatchingOptionTypeSymbols(list, name, results);
						break;
					case SymbolKind.Option:
						GetMatchingOptionSymbols(list, name, results);
						break;
					case SymbolKind.Field:
					case SymbolKind.Control:
					case SymbolKind.Action:
					case SymbolKind.TestField:
					case SymbolKind.TestAction:
						GetMatchingSymbols(list, name, prefix, results);
						break;
					case SymbolKind.ErrorType:
						GetErrorTypes(list, name, results);
						break;
					case SymbolKind.Parameter:
						GetMatchingParameterSymbols(list, name, results);
						break;
					case SymbolKind.GlobalVariable:
					case SymbolKind.LocalVariable:
						GetMatchingInteriorSymbols(list, prefix, name, results);
						break;
					case SymbolKind.Module:
					case SymbolKind.NamedType:
					case SymbolKind.Class:
					case SymbolKind.Table:
					case SymbolKind.Codeunit:
					case SymbolKind.Page:
					case SymbolKind.Report:
					case SymbolKind.XmlPort:
					case SymbolKind.PageExtension:
					case SymbolKind.TableExtension:
					case SymbolKind.PageCustomization:
					case SymbolKind.TestPage:
						GetMatchingContainerSymbols(list, prefix, name, results);
						break;
					default:
						throw ExceptionUtilities.UnexpectedValue(prefix);
					}
					if (PeekNextChar() != '.')
					{
						break;
					}
					index++;
					list.Clear();
					list.AddRange(results);
					results.Clear();
				}
				if (PeekNextChar() == ':')
				{
					index++;
					results.Clear();
				}
			}
			finally
			{
				symbolListPool.ClearAndFree(list);
			}
		}

		private bool IsNamePrefix()
		{
			if (PeekNextChar() == ':')
			{
				string text = string.Empty;
				int num = index + 1;
				do
				{
					text += ((num < id.Length) ? id[num] : '\0');
					num++;
				}
				while (num < id.Length && id[num] != ':');
				if (Enum.TryParse<SymbolKind>(text, out var result))
				{
					return IsNamePrefix(result);
				}
			}
			return false;
		}

		private static bool IsNamePrefix(SymbolKind kind)
		{
			switch (kind)
			{
			case SymbolKind.Module:
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
			case SymbolKind.Class:
			case SymbolKind.Table:
			case SymbolKind.Codeunit:
			case SymbolKind.Page:
			case SymbolKind.GlobalVariable:
			case SymbolKind.Method:
			case SymbolKind.LocalVariable:
			case SymbolKind.Parameter:
			case SymbolKind.Field:
			case SymbolKind.Option:
			case SymbolKind.Property:
			case SymbolKind.OptionType:
			case SymbolKind.Control:
				return true;
			default:
				return false;
			}
		}

		private SymbolKind ParseNamePrefix()
		{
			string text = string.Empty;
			if (PeekNextChar() == ':')
			{
				index++;
				char c;
				do
				{
					c = PeekNextChar();
					if (c != ':')
					{
						text += PeekNextChar();
					}
					index++;
				}
				while (c != ':' && c != '\0');
			}
			if (!Enum.TryParse<SymbolKind>(text, out var result))
			{
				return SymbolKind.Undefined;
			}
			return result;
		}

		private bool ParsePrefixedName(out SymbolKind prefix, out string name)
		{
			prefix = ParseNamePrefix();
			if (prefix != SymbolKind.Undefined)
			{
				name = ParseName();
				return true;
			}
			name = null;
			return false;
		}

		private void ParseArrayTypes(List<ISymbol> symbols)
		{
			while (PeekNextChar() == '[')
			{
				IList<int> dimensions = ParseArrayBounds();
				ConstructArray(symbols, dimensions);
			}
		}

		private static void ConstructArray(List<ISymbol> symbols, IList<int> dimensions)
		{
			int num = 0;
			while (num < symbols.Count)
			{
				TypeSymbol typeSymbol = symbols[num] as TypeSymbol;
				if (typeSymbol != null)
				{
					symbols[num] = ArrayTypeSymbol.Create(typeSymbol, dimensions.ToImmutableArray());
					num++;
				}
				else
				{
					symbols.RemoveAt(num);
				}
			}
		}

		private IList<int> ParseArrayBounds()
		{
			IList<int> list = new List<int>();
			index++;
			while (true)
			{
				if (char.IsDigit(PeekNextChar()))
				{
					list.Add(ParseIntegerLiteral());
				}
				if (PeekNextChar() != ',')
				{
					break;
				}
				index++;
			}
			if (PeekNextChar() == ']')
			{
				index++;
			}
			return list;
		}

		private static void GetErrorTypes(IReadOnlyList<ISymbol> containers, string memberName, List<ISymbol> results)
		{
			EnumerateContainersAndExecuteAction(containers, (ContainerSymbol container) =>
			{
				TypeSymbol typeSymbol = container as TypeSymbol;
				if (typeSymbol != null)
				{
					results.Add(Compilation.CreateErrorTypeSymbol(typeSymbol, memberName));
				}
			});
		}

		private static void GetMatchingMethods(IReadOnlyList<ISymbol> containers, string name, List<ISymbol> results)
		{
			foreach (ISymbol container in containers)
			{
				TypeSymbol typeSymbol = container as TypeSymbol;
				if (!(typeSymbol != null))
				{
					continue;
				}
				foreach (Symbol member in typeSymbol.GetMembers(name))
				{
					if (member is IMethodSymbol item)
					{
						results.Add(item);
					}
				}
			}
		}

		private static void GetMatchingProperties(IReadOnlyList<ISymbol> containers, string name, List<ISymbol> results)
		{
			EnumerateContainersGetMembersAndExecuteAction(containers, name, (Symbol symbol) =>
			{
				if (!symbol.Properties.IsDefaultOrEmpty)
				{
					results.AddRange(symbol.Properties);
				}
				else if (symbol.Kind == SymbolKind.Property)
				{
					results.Add(symbol);
				}
			});
		}

		private static void GetMatchingSymbols(IReadOnlyList<ISymbol> containers, string name, SymbolKind kind, List<ISymbol> results)
		{
			EnumerateContainersGetMembersAndExecuteAction(containers, name, (Symbol symbol) =>
			{
				if (symbol.Kind == kind)
				{
					results.Add(symbol);
				}
			});
		}

		private static void GetMatchingContainerSymbols(List<ISymbol> containers, SymbolKind prefix, string name, List<ISymbol> results)
		{
			if (prefix == SymbolKind.OptionType)
			{
				results.AddRange(containers);
				return;
			}
			EnumerateContainersAndExecuteAction(containers, (ContainerSymbol container) =>
			{
				results.AddRange(from m in container.GetMembers(name)
					where m.Kind == prefix
					select m);
			});
		}

		private static void GetMatchingOptionTypeSymbols(List<ISymbol> containers, string name, List<ISymbol> results)
		{
			EnumerateContainersAndExecuteAction(containers, (ContainerSymbol container) =>
			{
				Symbol symbol = container.GetMembers(name).FirstOrDefault((Symbol m) => m.Kind == SymbolKind.OptionType);
				if (symbol != null)
				{
					results.Add(symbol);
				}
			});
		}

		private static void GetMatchingOptionSymbols(List<ISymbol> containers, string name, List<ISymbol> results)
		{
			EnumerateContainersAndExecuteAction(containers, (ContainerSymbol container) =>
			{
				foreach (Symbol item in from m in container.GetMembers()
					where m.Kind == SymbolKind.OptionType
					select m)
				{
					Symbol symbol = (item as TypeSymbol).GetMembers(name).FirstOrDefault();
					if (symbol != null)
					{
						results.Add(symbol);
					}
				}
			});
		}

		private static void GetMatchingParameterSymbols(IReadOnlyList<ISymbol> containers, string name, List<ISymbol> symbols)
		{
			for (int i = 0; i < containers.Count; i++)
			{
				if (containers[i] is IMethodSymbol { Parameters: var parameters })
				{
					IParameterSymbol parameterSymbol = parameters.FirstOrDefault((IParameterSymbol p) => p.Name == name);
					if (parameterSymbol != null)
					{
						symbols.Add(parameterSymbol);
					}
				}
			}
		}

		private void GetMatchingInteriorSymbols(IReadOnlyList<ISymbol> containers, SymbolKind kind, string name, List<ISymbol> symbols)
		{
			int occurrence = 0;
			if (PeekNextChar() == '`')
			{
				index++;
				occurrence = ParseIntegerLiteral();
			}
			for (int i = 0; i < containers.Count; i++)
			{
				ISymbol matchingInteriorSymbol = GetMatchingInteriorSymbol(containers[i], kind, name, occurrence);
				if (matchingInteriorSymbol != null)
				{
					symbols.Add(matchingInteriorSymbol);
				}
			}
		}

		private static void EnumerateContainersAndExecuteAction(IEnumerable<ISymbol> containers, Action<ContainerSymbol> action)
		{
			foreach (ISymbol container in containers)
			{
				ContainerSymbol containerSymbol = container as ContainerSymbol;
				if (containerSymbol != null)
				{
					action(containerSymbol);
				}
			}
		}

		private static void EnumerateContainersGetMembersAndExecuteAction(IEnumerable<ISymbol> containers, string name, Action<Symbol> action)
		{
			EnumerateContainersAndExecuteAction(containers, (ContainerSymbol container) =>
			{
				foreach (Symbol member in container.GetMembers(name))
				{
					action(member);
				}
			});
		}

		private char PeekNextChar(int offset = 0)
		{
			if (index + offset < id.Length)
			{
				return id[index + offset];
			}
			return '\0';
		}

		private string ParseName()
		{
			int num = id.IndexOfAny(nameDelimiters, index);
			string name;
			if (num >= 0)
			{
				name = id.Substring(index, num - index);
				index = num;
			}
			else
			{
				name = id.Substring(index);
				index = id.Length;
			}
			return DecodeName(name);
		}

		private static string DecodeName(string name)
		{
			if (name.IndexOf('\t') >= 0)
			{
				return name.Replace('\t', '.');
			}
			return name;
		}

		private int ParseIntegerLiteral()
		{
			int num = 0;
			while (index < id.Length && char.IsDigit(id[index]))
			{
				num = num * 10 + (id[index] - 48);
				index++;
			}
			return num;
		}
	}

	private const char symbolKindSeparator = ':';

	private static readonly ListPool<ISymbol> symbolListPool = new ListPool<ISymbol>();

	private const char DotReplacer = '\t';

	internal const char OccurenceDelimiter = '`';

	public static string CreateId(ISymbol symbol)
	{
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		StringBuilder stringBuilder = new StringBuilder();
		new Generator(stringBuilder).Visit(symbol);
		return stringBuilder.ToString();
	}

	public static ISymbol GetFirstSymbolForId(string id, Compilation compilation)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		List<ISymbol> list = symbolListPool.Allocate();
		try
		{
			Parser.Parse(id, compilation, list);
			return (list.Count == 0) ? null : list[0];
		}
		finally
		{
			symbolListPool.ClearAndFree(list);
		}
	}

	private static string EncodeName(string name)
	{
		if (name.IndexOf('.') >= 0)
		{
			return name.Replace('.', '\t');
		}
		return name;
	}

	private static ISymbol GetMatchingInteriorSymbol(ISymbol containingSymbol, SymbolKind kind, string name, int occurrence)
	{
		List<ISymbol> list = symbolListPool.Allocate();
		try
		{
			int num = 0;
			GetInteriorSymbols(containingSymbol, list);
			foreach (ISymbol item in list)
			{
				if (item.Kind == kind && string.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (occurrence == num)
					{
						return item;
					}
					num++;
				}
			}
			return null;
		}
		finally
		{
			symbolListPool.ClearAndFree(list);
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbols")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "containingSymbol")]
	private static void GetInteriorSymbols(ISymbol containingSymbol, List<ISymbol> symbols)
	{
		Compilation declaringCompilation = containingSymbol.GetContainingSymbolOfKind<ModuleSymbol>(SymbolKind.Module).DeclaringCompilation;
		if (declaringCompilation == null)
		{
			return;
		}
		foreach (SyntaxReference declaringSyntaxReference in containingSymbol.DeclaringSyntaxReferences)
		{
			if (declaringCompilation.SyntaxTrees.Contains(declaringSyntaxReference.SyntaxTree))
			{
				SyntaxNode syntax = declaringSyntaxReference.GetSyntax();
				GetDeclaredSymbols(declaringCompilation.GetSemanticModel(syntax.SyntaxTree), syntax, symbols);
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private static void GetDeclaredSymbols(SemanticModel model, SyntaxNode root, List<ISymbol> symbols)
	{
		foreach (SyntaxNode item in root.DescendantNodes())
		{
			ISymbol declaredSymbol = model.GetDeclaredSymbol(item);
			if (declaredSymbol != null)
			{
				symbols.Add(declaredSymbol);
			}
		}
	}

	private static int GetInteriorSymbolOccurrence(ISymbol symbol)
	{
		List<ISymbol> list = symbolListPool.Allocate();
		try
		{
			int num = 0;
			GetInteriorSymbols(symbol.ContainingSymbol, list);
			foreach (ISymbol item in list)
			{
				if (item.Kind == symbol.Kind && item.Name == symbol.Name)
				{
					if (item.Equals(symbol))
					{
						return num;
					}
					if (item.DeclaringSyntaxReferences.Length > 0 && symbol.DeclaringSyntaxReferences.Length > 0 && item.DeclaringSyntaxReferences[0].Span == symbol.DeclaringSyntaxReferences[0].Span)
					{
						return num;
					}
					num++;
				}
			}
			return num;
		}
		finally
		{
			symbolListPool.ClearAndFree(list);
		}
	}
}
