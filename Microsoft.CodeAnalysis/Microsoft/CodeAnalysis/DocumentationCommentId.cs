using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

public static class DocumentationCommentId
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
			base.Free(list);
		}

		[Obsolete("Do not use Free, Use ClearAndFree instead.", true)]
		public new void Free(List<T> list)
		{
			throw new NotSupportedException();
		}
	}

	private class DeclarationGenerator : SymbolVisitor
	{
		private class Generator : SymbolVisitor<bool>
		{
			private readonly StringBuilder _builder;

			private ReferenceGenerator _referenceGenerator;

			public Generator(StringBuilder builder)
			{
				_builder = builder;
			}

			private ReferenceGenerator GetReferenceGenerator(ISymbol typeParameterContext)
			{
				if (_referenceGenerator == null || _referenceGenerator.TypeParameterContext != typeParameterContext)
				{
					_referenceGenerator = new ReferenceGenerator(_builder, typeParameterContext);
				}
				return _referenceGenerator;
			}

			public override bool DefaultVisit(ISymbol symbol)
			{
				throw new InvalidOperationException("Cannot generated a documentation comment id for symbol.");
			}

			public override bool VisitEvent(IEventSymbol symbol)
			{
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
				}
				_builder.Append(EncodeName(symbol.Name));
				return true;
			}

			public override bool VisitField(IFieldSymbol symbol)
			{
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
				}
				_builder.Append(EncodeName(symbol.Name));
				return true;
			}

			public override bool VisitProperty(IPropertySymbol symbol)
			{
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
				}
				string name = EncodePropertyName(symbol.Name);
				_builder.Append(EncodeName(name));
				AppendParameters(symbol.Parameters);
				return true;
			}

			public override bool VisitMethod(IMethodSymbol symbol)
			{
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
					_builder.Append(EncodeName(symbol.Name));
				}
				if (symbol.TypeParameters.Length > 0)
				{
					_builder.Append("``");
					_builder.Append(symbol.TypeParameters.Length);
				}
				AppendParameters(symbol.Parameters);
				if (!symbol.ReturnsVoid)
				{
					_builder.Append("~");
					GetReferenceGenerator(symbol).Visit(symbol.ReturnType);
				}
				return true;
			}

			private void AppendParameters(ImmutableArray<IParameterSymbol> parameters)
			{
				if (parameters.Length <= 0)
				{
					return;
				}
				_builder.Append("(");
				int i = 0;
				for (int length = parameters.Length; i < length; i++)
				{
					if (i > 0)
					{
						_builder.Append(",");
					}
					IParameterSymbol parameterSymbol = parameters[i];
					GetReferenceGenerator(parameterSymbol.ContainingSymbol).Visit(parameterSymbol.Type);
					if (parameterSymbol.RefKind != RefKind.None)
					{
						_builder.Append("@");
					}
				}
				_builder.Append(")");
			}

			public override bool VisitNamespace(INamespaceSymbol symbol)
			{
				if (symbol.IsGlobalNamespace)
				{
					return false;
				}
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
				}
				_builder.Append(EncodeName(symbol.Name));
				return true;
			}

			public override bool VisitNamedType(INamedTypeSymbol symbol)
			{
				if (Visit(symbol.ContainingSymbol))
				{
					_builder.Append(".");
				}
				_builder.Append(EncodeName(symbol.Name));
				if (symbol.TypeParameters.Length > 0)
				{
					_builder.Append("`");
					_builder.Append(symbol.TypeParameters.Length);
				}
				return true;
			}
		}

		private readonly StringBuilder _builder;

		private readonly Generator _generator;

		public DeclarationGenerator(StringBuilder builder)
		{
			_builder = builder;
			_generator = new Generator(builder);
		}

		public override void DefaultVisit(ISymbol symbol)
		{
			throw new InvalidOperationException("Cannot generated a documentation comment id for symbol.");
		}

		public override void VisitEvent(IEventSymbol symbol)
		{
			_builder.Append("E:");
			_generator.Visit(symbol);
		}

		public override void VisitField(IFieldSymbol symbol)
		{
			_builder.Append("F:");
			_generator.Visit(symbol);
		}

		public override void VisitProperty(IPropertySymbol symbol)
		{
			_builder.Append("P:");
			_generator.Visit(symbol);
		}

		public override void VisitMethod(IMethodSymbol symbol)
		{
			_builder.Append("M:");
			_generator.Visit(symbol);
		}

		public override void VisitNamespace(INamespaceSymbol symbol)
		{
			_builder.Append("N:");
			_generator.Visit(symbol);
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			_builder.Append("T:");
			_generator.Visit(symbol);
		}
	}

	private class ReferenceGenerator : SymbolVisitor<bool>
	{
		private readonly StringBuilder _builder;

		private readonly ISymbol _typeParameterContext;

		public ISymbol TypeParameterContext => _typeParameterContext;

		public ReferenceGenerator(StringBuilder builder, ISymbol typeParameterContext)
		{
			_builder = builder;
			_typeParameterContext = typeParameterContext;
		}

		private void BuildDottedName(ISymbol symbol)
		{
			if (Visit(symbol.ContainingSymbol))
			{
				_builder.Append(".");
			}
			_builder.Append(EncodeName(symbol.Name));
		}

		public override bool VisitAlias(IAliasSymbol symbol)
		{
			return symbol.Target.Accept(this);
		}

		public override bool VisitNamespace(INamespaceSymbol symbol)
		{
			if (symbol.IsGlobalNamespace)
			{
				return false;
			}
			BuildDottedName(symbol);
			return true;
		}

		public override bool VisitNamedType(INamedTypeSymbol symbol)
		{
			BuildDottedName(symbol);
			if (symbol.IsGenericType)
			{
				if (symbol.OriginalDefinition == symbol)
				{
					_builder.Append("`");
					_builder.Append(symbol.TypeParameters.Length);
				}
				else if (symbol.TypeArguments.Length > 0)
				{
					_builder.Append("{");
					int i = 0;
					for (int length = symbol.TypeArguments.Length; i < length; i++)
					{
						if (i > 0)
						{
							_builder.Append(",");
						}
						Visit(symbol.TypeArguments[i]);
					}
					_builder.Append("}");
				}
			}
			return true;
		}

		public override bool VisitArrayType(IArrayTypeSymbol symbol)
		{
			Visit(symbol.ElementType);
			_builder.Append("[");
			int i = 0;
			for (int rank = symbol.Rank; i < rank; i++)
			{
				if (i > 0)
				{
					_builder.Append(",");
				}
			}
			_builder.Append("]");
			return true;
		}

		public override bool VisitPointerType(IPointerTypeSymbol symbol)
		{
			Visit(symbol.PointedAtType);
			_builder.Append("*");
			return true;
		}

		public override bool VisitTypeParameter(ITypeParameterSymbol symbol)
		{
			if (!IsInScope(symbol))
			{
				new DeclarationGenerator(_builder).Visit(symbol.ContainingSymbol);
				_builder.Append(":");
			}
			if (symbol.DeclaringMethod != null)
			{
				_builder.Append("``");
				_builder.Append(symbol.Ordinal);
			}
			else
			{
				int totalTypeParameterCount = GetTotalTypeParameterCount(symbol.ContainingSymbol?.ContainingSymbol as INamedTypeSymbol);
				_builder.Append("`");
				_builder.Append(totalTypeParameterCount + symbol.Ordinal);
			}
			return true;
		}

		private bool IsInScope(ITypeParameterSymbol typeParameterSymbol)
		{
			ISymbol containingSymbol = typeParameterSymbol.ContainingSymbol;
			for (ISymbol symbol = _typeParameterContext; symbol != null; symbol = symbol.ContainingSymbol)
			{
				if (symbol == containingSymbol)
				{
					return true;
				}
			}
			return false;
		}
	}

	private static class Parser
	{
		[StructLayout(LayoutKind.Auto)]
		private struct ParameterInfo(ITypeSymbol type, bool isRefOrOut)
		{
			internal readonly ITypeSymbol Type = type;

			internal readonly bool IsRefOrOut = isRefOrOut;
		}

		private static readonly ListPool<ParameterInfo> s_parameterListPool = new ListPool<ParameterInfo>();

		private static readonly char[] s_nameDelimiters = new char[14]
		{
			':', '.', '(', ')', '{', '}', '[', ']', ',', '\'',
			'@', '*', '`', '~'
		};

		public static bool ParseDeclaredSymbolId(string id, Compilation compilation, List<ISymbol> results)
		{
			if (id == null)
			{
				return false;
			}
			if (id.Length < 2)
			{
				return false;
			}
			int index = 0;
			results.Clear();
			ParseDeclaredId(id, ref index, compilation, results);
			return results.Count > 0;
		}

		public static bool ParseReferencedSymbolId(string id, Compilation compilation, List<ISymbol> results)
		{
			if (id == null)
			{
				return false;
			}
			int index = 0;
			results.Clear();
			ParseTypeSymbol(id, ref index, compilation, null, results);
			return results.Count > 0;
		}

		private static void ParseDeclaredId(string id, ref int index, Compilation compilation, List<ISymbol> results)
		{
			SymbolKind symbolKind;
			switch (PeekNextChar(id, index))
			{
			default:
				return;
			case 'E':
				symbolKind = SymbolKind.Event;
				break;
			case 'F':
				symbolKind = SymbolKind.Field;
				break;
			case 'M':
				symbolKind = SymbolKind.Method;
				break;
			case 'N':
				symbolKind = SymbolKind.Namespace;
				break;
			case 'P':
				symbolKind = SymbolKind.Property;
				break;
			case 'T':
				symbolKind = SymbolKind.NamedType;
				break;
			}
			index++;
			if (PeekNextChar(id, index) == ':')
			{
				index++;
			}
			List<INamespaceOrTypeSymbol> list = s_namespaceOrTypeListPool.Allocate();
			try
			{
				list.Add(compilation.GlobalNamespace);
				string memberName;
				int num;
				while (true)
				{
					memberName = ParseName(id, ref index);
					num = 0;
					if (PeekNextChar(id, index) == '`')
					{
						index++;
						if (PeekNextChar(id, index) == '`')
						{
							index++;
						}
						num = ReadNextInteger(id, ref index);
					}
					if (PeekNextChar(id, index) != '.')
					{
						break;
					}
					index++;
					if (num > 0)
					{
						GetMatchingTypes(list, memberName, num, results);
					}
					else if (symbolKind == SymbolKind.Namespace)
					{
						GetMatchingNamespaces(list, memberName, results);
					}
					else
					{
						GetMatchingNamespaceOrTypes(list, memberName, results);
					}
					if (results.Count == 0)
					{
						return;
					}
					list.Clear();
					list.AddRange(results.OfType<INamespaceOrTypeSymbol>());
					results.Clear();
				}
				switch (symbolKind)
				{
				case SymbolKind.Method:
					GetMatchingMethods(id, ref index, list, memberName, num, compilation, results);
					break;
				case SymbolKind.NamedType:
					GetMatchingTypes(list, memberName, num, results);
					break;
				case SymbolKind.Property:
					GetMatchingProperties(id, ref index, list, memberName, compilation, results);
					break;
				case SymbolKind.Event:
					GetMatchingEvents(list, memberName, results);
					break;
				case SymbolKind.Field:
					GetMatchingFields(list, memberName, results);
					break;
				case SymbolKind.Namespace:
					GetMatchingNamespaces(list, memberName, results);
					break;
				case SymbolKind.Label:
				case SymbolKind.Local:
				case SymbolKind.NetModule:
				case SymbolKind.Parameter:
				case SymbolKind.PointerType:
					break;
				}
			}
			finally
			{
				s_namespaceOrTypeListPool.ClearAndFree(list);
			}
		}

		private static ITypeSymbol ParseTypeSymbol(string id, ref int index, Compilation compilation, ISymbol typeParameterContext)
		{
			List<ISymbol> list = s_symbolListPool.Allocate();
			try
			{
				ParseTypeSymbol(id, ref index, compilation, typeParameterContext, list);
				if (list.Count == 0)
				{
					return null;
				}
				return (ITypeSymbol)list[0];
			}
			finally
			{
				s_symbolListPool.ClearAndFree(list);
			}
		}

		private static void ParseTypeSymbol(string id, ref int index, Compilation compilation, ISymbol typeParameterContext, List<ISymbol> results)
		{
			char c = PeekNextChar(id, index);
			if ((c == 'M' || c == 'T') && PeekNextChar(id, index + 1) == ':')
			{
				List<ISymbol> list = s_symbolListPool.Allocate();
				try
				{
					ParseDeclaredId(id, ref index, compilation, list);
					if (list.Count == 0)
					{
						return;
					}
					if (PeekNextChar(id, index) == ':')
					{
						index++;
						int num = index;
						{
							foreach (ISymbol item in list)
							{
								index = num;
								ParseTypeSymbol(id, ref index, compilation, item, results);
							}
							return;
						}
					}
					results.AddRange(list.OfType<ITypeSymbol>());
					return;
				}
				finally
				{
					s_symbolListPool.ClearAndFree(list);
				}
			}
			if (c == '`')
			{
				ParseTypeParameterSymbol(id, ref index, typeParameterContext, results);
			}
			else
			{
				ParseNamedTypeSymbol(id, ref index, compilation, typeParameterContext, results);
			}
			int num2 = index;
			int num3 = index;
			for (int i = 0; i < results.Count; i++)
			{
				index = num2;
				ITypeSymbol typeSymbol = (ITypeSymbol)results[i];
				while (true)
				{
					if (PeekNextChar(id, index) == '[')
					{
						int rank = ParseArrayBounds(id, ref index);
						typeSymbol = compilation.CreateArrayTypeSymbol(typeSymbol, rank);
						continue;
					}
					if (PeekNextChar(id, index) != '*')
					{
						break;
					}
					index++;
					typeSymbol = compilation.CreatePointerTypeSymbol(typeSymbol);
				}
				results[i] = typeSymbol;
				num3 = index;
			}
			index = num3;
		}

		private static void ParseTypeParameterSymbol(string id, ref int index, ISymbol typeParameterContext, List<ISymbol> results)
		{
			index++;
			if (PeekNextChar(id, index) == '`')
			{
				index++;
				int num = ReadNextInteger(id, ref index);
				if (typeParameterContext is IMethodSymbol { TypeParameters: { Length: var length } } methodSymbol && length > 0 && num < length)
				{
					results.Add(methodSymbol.TypeParameters[num]);
				}
			}
			else
			{
				int n = ReadNextInteger(id, ref index);
				INamedTypeSymbol namedTypeSymbol = ((typeParameterContext is IMethodSymbol methodSymbol2) ? methodSymbol2.ContainingType : (typeParameterContext as INamedTypeSymbol));
				if (namedTypeSymbol != null)
				{
					results.Add(GetNthTypeParameter(namedTypeSymbol, n));
				}
			}
		}

		private static void ParseNamedTypeSymbol(string id, ref int index, Compilation compilation, ISymbol typeParameterContext, List<ISymbol> results)
		{
			List<INamespaceOrTypeSymbol> list = s_namespaceOrTypeListPool.Allocate();
			try
			{
				list.Add(compilation.GlobalNamespace);
				while (true)
				{
					string memberName = ParseName(id, ref index);
					List<ITypeSymbol> list2 = null;
					int num = 0;
					if (PeekNextChar(id, index) == '{')
					{
						list2 = new List<ITypeSymbol>();
						if (!ParseTypeArguments(id, ref index, compilation, typeParameterContext, list2))
						{
							continue;
						}
						num = list2.Count;
					}
					else if (PeekNextChar(id, index) == '`')
					{
						index++;
						num = ReadNextInteger(id, ref index);
					}
					if (num != 0 || PeekNextChar(id, index) != '.')
					{
						GetMatchingTypes(list, memberName, num, results);
						if (num != 0 && list2 != null && list2.Count != 0)
						{
							ITypeSymbol[] typeArguments = list2.ToArray();
							for (int i = 0; i < results.Count; i++)
							{
								results[i] = ((INamedTypeSymbol)results[i]).Construct(typeArguments);
							}
						}
					}
					else
					{
						GetMatchingNamespaceOrTypes(list, memberName, results);
					}
					if (PeekNextChar(id, index) == '.')
					{
						index++;
						list.Clear();
						CopyTo(results, list);
						results.Clear();
						continue;
					}
					break;
				}
			}
			finally
			{
				s_namespaceOrTypeListPool.ClearAndFree(list);
			}
		}

		private static int ParseArrayBounds(string id, ref int index)
		{
			index++;
			int num = 0;
			while (true)
			{
				if (char.IsDigit(PeekNextChar(id, index)))
				{
					ReadNextInteger(id, ref index);
				}
				if (PeekNextChar(id, index) == ':')
				{
					index++;
					if (char.IsDigit(PeekNextChar(id, index)))
					{
						ReadNextInteger(id, ref index);
					}
				}
				num++;
				if (PeekNextChar(id, index) != ',')
				{
					break;
				}
				index++;
			}
			if (PeekNextChar(id, index) == ']')
			{
				index++;
			}
			return num;
		}

		private static bool ParseTypeArguments(string id, ref int index, Compilation compilation, ISymbol typeParameterContext, List<ITypeSymbol> typeArguments)
		{
			index++;
			while (true)
			{
				ITypeSymbol typeSymbol = ParseTypeSymbol(id, ref index, compilation, typeParameterContext);
				if (typeSymbol == null)
				{
					return false;
				}
				typeArguments.Add(typeSymbol);
				if (PeekNextChar(id, index) != ',')
				{
					break;
				}
				index++;
			}
			if (PeekNextChar(id, index) == '}')
			{
				index++;
			}
			return true;
		}

		private static void GetMatchingTypes(List<INamespaceOrTypeSymbol> containers, string memberName, int arity, List<ISymbol> results)
		{
			int i = 0;
			for (int count = containers.Count; i < count; i++)
			{
				GetMatchingTypes(containers[i], memberName, arity, results);
			}
		}

		private static void GetMatchingTypes(INamespaceOrTypeSymbol container, string memberName, int arity, List<ISymbol> results)
		{
			foreach (ISymbol member in container.GetMembers(memberName))
			{
				if (member.Kind == SymbolKind.NamedType)
				{
					INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)member;
					if (namedTypeSymbol.Arity == arity)
					{
						results.Add(namedTypeSymbol);
					}
				}
			}
		}

		private static void GetMatchingNamespaceOrTypes(List<INamespaceOrTypeSymbol> containers, string memberName, List<ISymbol> results)
		{
			int i = 0;
			for (int count = containers.Count; i < count; i++)
			{
				GetMatchingNamespaceOrTypes(containers[i], memberName, results);
			}
		}

		private static void GetMatchingNamespaceOrTypes(INamespaceOrTypeSymbol container, string memberName, List<ISymbol> results)
		{
			foreach (ISymbol member in container.GetMembers(memberName))
			{
				if (member.Kind == SymbolKind.Namespace || (member.Kind == SymbolKind.NamedType && ((INamedTypeSymbol)member).Arity == 0))
				{
					results.Add(member);
				}
			}
		}

		private static void GetMatchingNamespaces(List<INamespaceOrTypeSymbol> containers, string memberName, List<ISymbol> results)
		{
			int i = 0;
			for (int count = containers.Count; i < count; i++)
			{
				GetMatchingNamespaces(containers[i], memberName, results);
			}
		}

		private static void GetMatchingNamespaces(INamespaceOrTypeSymbol container, string memberName, List<ISymbol> results)
		{
			foreach (ISymbol member in container.GetMembers(memberName))
			{
				if (member.Kind == SymbolKind.Namespace)
				{
					results.Add(member);
				}
			}
		}

		private static void GetMatchingMethods(string id, ref int index, List<INamespaceOrTypeSymbol> containers, string memberName, int arity, Compilation compilation, List<ISymbol> results)
		{
			List<ParameterInfo> list = s_parameterListPool.Allocate();
			try
			{
				int num = index;
				int num2 = index;
				int i = 0;
				for (int count = containers.Count; i < count; i++)
				{
					foreach (ISymbol member in containers[i].GetMembers(memberName))
					{
						index = num;
						if (!(member is IMethodSymbol methodSymbol) || methodSymbol.Arity != arity)
						{
							continue;
						}
						list?.Clear();
						if (PeekNextChar(id, index) == '(' && !ParseParameterList(id, ref index, compilation, methodSymbol, list))
						{
							continue;
						}
						if (list == null)
						{
							if (methodSymbol.Parameters.Length != 0)
							{
								continue;
							}
						}
						else if (!AllParametersMatch(methodSymbol.Parameters, list))
						{
							continue;
						}
						if (PeekNextChar(id, index) == '~')
						{
							index++;
							ITypeSymbol typeSymbol = ParseTypeSymbol(id, ref index, compilation, methodSymbol);
							if (typeSymbol != null && methodSymbol.ReturnType.Equals(typeSymbol))
							{
								results.Add(methodSymbol);
								num2 = index;
							}
						}
						else
						{
							results.Add(methodSymbol);
							num2 = index;
						}
					}
				}
				index = num2;
			}
			finally
			{
				s_parameterListPool.ClearAndFree(list);
			}
		}

		private static void GetMatchingProperties(string id, ref int index, List<INamespaceOrTypeSymbol> containers, string memberName, Compilation compilation, List<ISymbol> results)
		{
			int num = index;
			int num2 = index;
			List<ParameterInfo> list = null;
			try
			{
				int i = 0;
				for (int count = containers.Count; i < count; i++)
				{
					memberName = DecodePropertyName(memberName, compilation.Language);
					foreach (ISymbol member in containers[i].GetMembers(memberName))
					{
						index = num;
						if (!(member is IPropertySymbol propertySymbol))
						{
							continue;
						}
						if (PeekNextChar(id, index) == '(')
						{
							if (list == null)
							{
								list = s_parameterListPool.Allocate();
							}
							else
							{
								list.Clear();
							}
							if (ParseParameterList(id, ref index, compilation, propertySymbol.ContainingSymbol, list) && AllParametersMatch(propertySymbol.Parameters, list))
							{
								results.Add(propertySymbol);
								num2 = index;
							}
						}
						else if (propertySymbol.Parameters.Length == 0)
						{
							results.Add(propertySymbol);
							num2 = index;
						}
					}
				}
				index = num2;
			}
			finally
			{
				if (list != null)
				{
					s_parameterListPool.ClearAndFree(list);
				}
			}
		}

		private static void GetMatchingFields(List<INamespaceOrTypeSymbol> containers, string memberName, List<ISymbol> results)
		{
			int i = 0;
			for (int count = containers.Count; i < count; i++)
			{
				foreach (ISymbol member in containers[i].GetMembers(memberName))
				{
					if (member.Kind == SymbolKind.Field)
					{
						results.Add(member);
					}
				}
			}
		}

		private static void GetMatchingEvents(List<INamespaceOrTypeSymbol> containers, string memberName, List<ISymbol> results)
		{
			int i = 0;
			for (int count = containers.Count; i < count; i++)
			{
				foreach (ISymbol member in containers[i].GetMembers(memberName))
				{
					if (member.Kind == SymbolKind.Event)
					{
						results.Add(member);
					}
				}
			}
		}

		private static bool AllParametersMatch(ImmutableArray<IParameterSymbol> symbolParameters, List<ParameterInfo> expectedParameters)
		{
			if (symbolParameters.Length != expectedParameters.Count)
			{
				return false;
			}
			for (int i = 0; i < expectedParameters.Count; i++)
			{
				if (!ParameterMatches(symbolParameters[i], expectedParameters[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static bool ParameterMatches(IParameterSymbol symbol, ParameterInfo parameterInfo)
		{
			if (symbol.RefKind == RefKind.None == parameterInfo.IsRefOrOut)
			{
				return false;
			}
			ITypeSymbol type = parameterInfo.Type;
			if (type != null)
			{
				return symbol.Type.Equals(type);
			}
			return false;
		}

		private static ITypeParameterSymbol GetNthTypeParameter(INamedTypeSymbol typeSymbol, int n)
		{
			int typeParameterCount = GetTypeParameterCount(typeSymbol.ContainingType);
			if (n < typeParameterCount)
			{
				return GetNthTypeParameter(typeSymbol.ContainingType, n);
			}
			int index = n - typeParameterCount;
			return typeSymbol.TypeParameters[index];
		}

		private static int GetTypeParameterCount(INamedTypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
			{
				return 0;
			}
			return typeSymbol.TypeParameters.Length + GetTypeParameterCount(typeSymbol.ContainingType);
		}

		private static bool ParseParameterList(string id, ref int index, Compilation compilation, ISymbol typeParameterContext, List<ParameterInfo> parameters)
		{
			index++;
			if (PeekNextChar(id, index) == ')')
			{
				index++;
				return true;
			}
			ParameterInfo? parameterInfo = ParseParameter(id, ref index, compilation, typeParameterContext);
			if (!parameterInfo.HasValue)
			{
				return false;
			}
			parameters.Add(parameterInfo.Value);
			while (PeekNextChar(id, index) == ',')
			{
				index++;
				parameterInfo = ParseParameter(id, ref index, compilation, typeParameterContext);
				if (!parameterInfo.HasValue)
				{
					return false;
				}
				parameters.Add(parameterInfo.Value);
			}
			if (PeekNextChar(id, index) == ')')
			{
				index++;
			}
			return true;
		}

		private static ParameterInfo? ParseParameter(string id, ref int index, Compilation compilation, ISymbol typeParameterContext)
		{
			bool isRefOrOut = false;
			ITypeSymbol typeSymbol = ParseTypeSymbol(id, ref index, compilation, typeParameterContext);
			if (typeSymbol == null)
			{
				return null;
			}
			if (PeekNextChar(id, index) == '@')
			{
				index++;
				isRefOrOut = true;
			}
			return new ParameterInfo(typeSymbol, isRefOrOut);
		}

		private static char PeekNextChar(string id, int index)
		{
			if (index < id.Length)
			{
				return id[index];
			}
			return '\0';
		}

		private static string ParseName(string id, ref int index)
		{
			int num = id.IndexOfAny(s_nameDelimiters, index);
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
			if (name.IndexOf('#') >= 0)
			{
				return name.Replace('#', '.');
			}
			return name;
		}

		private static int ReadNextInteger(string id, ref int index)
		{
			int num = 0;
			while (index < id.Length && char.IsDigit(id[index]))
			{
				num = num * 10 + (id[index] - 48);
				index++;
			}
			return num;
		}

		private static void CopyTo<TSource, TDestination>(List<TSource> source, List<TDestination> destination) where TSource : class where TDestination : class
		{
			if (destination.Count + source.Count > destination.Capacity)
			{
				destination.Capacity = destination.Count + source.Count;
			}
			int i = 0;
			for (int count = source.Count; i < count; i++)
			{
				destination.Add((TDestination)(object)source[i]);
			}
		}
	}

	private static readonly ListPool<ISymbol> s_symbolListPool = new ListPool<ISymbol>();

	private static readonly ListPool<INamespaceOrTypeSymbol> s_namespaceOrTypeListPool = new ListPool<INamespaceOrTypeSymbol>();

	public static string CreateDeclarationId(ISymbol symbol)
	{
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		StringBuilder stringBuilder = new StringBuilder();
		new DeclarationGenerator(stringBuilder).Visit(symbol);
		return stringBuilder.ToString();
	}

	public static string CreateReferenceId(ISymbol symbol)
	{
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if (symbol is INamespaceSymbol)
		{
			return CreateDeclarationId(symbol);
		}
		StringBuilder stringBuilder = new StringBuilder();
		new ReferenceGenerator(stringBuilder, null).Visit(symbol);
		return stringBuilder.ToString();
	}

	public static ImmutableArray<ISymbol> GetSymbolsForDeclarationId(string id, Compilation compilation)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		List<ISymbol> list = s_symbolListPool.Allocate();
		try
		{
			Parser.ParseDeclaredSymbolId(id, compilation, list);
			return list.ToImmutableArray();
		}
		finally
		{
			s_symbolListPool.ClearAndFree(list);
		}
	}

	private static bool TryGetSymbolsForDeclarationId(string id, Compilation compilation, List<ISymbol> results)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		return Parser.ParseDeclaredSymbolId(id, compilation, results);
	}

	public static ISymbol GetFirstSymbolForDeclarationId(string id, Compilation compilation)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		List<ISymbol> list = s_symbolListPool.Allocate();
		try
		{
			Parser.ParseDeclaredSymbolId(id, compilation, list);
			return (list.Count == 0) ? null : list[0];
		}
		finally
		{
			s_symbolListPool.ClearAndFree(list);
		}
	}

	public static ImmutableArray<ISymbol> GetSymbolsForReferenceId(string id, Compilation compilation)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		List<ISymbol> list = s_symbolListPool.Allocate();
		try
		{
			TryGetSymbolsForReferenceId(id, compilation, list);
			return list.ToImmutableArray();
		}
		finally
		{
			s_symbolListPool.ClearAndFree(list);
		}
	}

	private static bool TryGetSymbolsForReferenceId(string id, Compilation compilation, List<ISymbol> results)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		if (id.Length > 1 && id[0] == 'N' && id[1] == ':')
		{
			return TryGetSymbolsForDeclarationId(id, compilation, results);
		}
		return Parser.ParseReferencedSymbolId(id, compilation, results);
	}

	public static ISymbol GetFirstSymbolForReferenceId(string id, Compilation compilation)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (id.Length > 1 && id[0] == 'N' && id[1] == ':')
		{
			return GetFirstSymbolForDeclarationId(id, compilation);
		}
		List<ISymbol> list = s_symbolListPool.Allocate();
		try
		{
			Parser.ParseReferencedSymbolId(id, compilation, list);
			return (list.Count == 0) ? null : list[0];
		}
		finally
		{
			s_symbolListPool.ClearAndFree(list);
		}
	}

	private static int GetTotalTypeParameterCount(INamedTypeSymbol symbol)
	{
		int num = 0;
		while (symbol != null)
		{
			num += symbol.TypeParameters.Length;
			symbol = symbol.ContainingSymbol as INamedTypeSymbol;
		}
		return num;
	}

	private static string EncodeName(string name)
	{
		if (name.IndexOf('.') >= 0)
		{
			return name.Replace('.', '#');
		}
		return name;
	}

	private static string EncodePropertyName(string name)
	{
		if (name == "this[]")
		{
			name = "Item";
		}
		else if (name.EndsWith(".this[]"))
		{
			name = name.Substring(0, name.Length - 6) + "Item";
		}
		return name;
	}

	private static string DecodePropertyName(string name, string language)
	{
		if (language == "C#")
		{
			if (name == "Item")
			{
				name = "this[]";
			}
			else if (name.EndsWith(".Item"))
			{
				name = name.Substring(0, name.Length - 4) + "this[]";
			}
		}
		return name;
	}
}
