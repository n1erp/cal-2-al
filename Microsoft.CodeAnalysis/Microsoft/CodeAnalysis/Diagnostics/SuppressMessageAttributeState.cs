using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class SuppressMessageAttributeState
{
	private class GlobalSuppressions
	{
		private readonly Dictionary<string, SuppressMessageInfo> _compilationWideSuppressions = new Dictionary<string, SuppressMessageInfo>();

		private readonly Dictionary<ISymbol, Dictionary<string, SuppressMessageInfo>> _globalSymbolSuppressions = new Dictionary<ISymbol, Dictionary<string, SuppressMessageInfo>>();

		public void AddCompilationWideSuppression(SuppressMessageInfo info)
		{
			AddOrUpdate(info, _compilationWideSuppressions);
		}

		public void AddGlobalSymbolSuppression(ISymbol symbol, SuppressMessageInfo info)
		{
			if (_globalSymbolSuppressions.TryGetValue(symbol, out var value))
			{
				AddOrUpdate(info, value);
				return;
			}
			value = new Dictionary<string, SuppressMessageInfo> { { info.Id, info } };
			_globalSymbolSuppressions.Add(symbol, value);
		}

		public bool HasCompilationWideSuppression(string id, out SuppressMessageInfo info)
		{
			return _compilationWideSuppressions.TryGetValue(id, out info);
		}

		public bool HasGlobalSymbolSuppression(ISymbol symbol, string id, out SuppressMessageInfo info)
		{
			if (_globalSymbolSuppressions.TryGetValue(symbol, out var value) && value.TryGetValue(id, out info))
			{
				return true;
			}
			info = default;
			return false;
		}
	}

	internal enum TargetScope
	{
		None,
		Module,
		Namespace,
		Resource,
		Type,
		Member
	}

	[StructLayout(LayoutKind.Auto)]
	private struct TargetSymbolResolver(Compilation compilation, TargetScope scope, string fullyQualifiedName)
	{
		[StructLayout(LayoutKind.Auto)]
		private struct TypeInfo
		{
			public readonly ITypeSymbol Type;

			public readonly int StartIndex;

			public bool IsBound => Type != null;

			private TypeInfo(ITypeSymbol type, int startIndex)
			{
				Type = type;
				StartIndex = startIndex;
			}

			public static TypeInfo Create(ITypeSymbol type)
			{
				return new TypeInfo(type, -1);
			}

			public static TypeInfo CreateUnbound(int startIndex)
			{
				return new TypeInfo(null, startIndex);
			}
		}

		[StructLayout(LayoutKind.Auto)]
		private struct ParameterInfo(TypeInfo type, bool isRefOrOut)
		{
			public readonly TypeInfo Type = type;

			public readonly bool IsRefOrOut = isRefOrOut;
		}

		private static readonly char[] s_nameDelimiters = new char[15]
		{
			':', '.', '+', '(', ')', '<', '>', '[', ']', '{',
			'}', ',', '&', '*', '`'
		};

		private static readonly string[] s_callingConventionStrings = new string[5] { "[vararg]", "[cdecl]", "[fastcall]", "[stdcall]", "[thiscall]" };

		private static readonly ParameterInfo[] s_noParameters = Array.Empty<ParameterInfo>();

		private readonly Compilation _compilation = compilation;

		private readonly TargetScope _scope = scope;

		private readonly string _name = fullyQualifiedName;

		private int _index = 0;

		private static string RemovePrefix(string id, string prefix)
		{
			if (id != null && prefix != null && id.StartsWith(prefix, StringComparison.Ordinal))
			{
				return id.Substring(prefix.Length);
			}
			return id;
		}

		public void Resolve(IList<ISymbol> results)
		{
			if (string.IsNullOrEmpty(_name))
			{
				return;
			}
			ImmutableArray<ISymbol> symbolsForDeclarationId = DocumentationCommentId.GetSymbolsForDeclarationId(RemovePrefix(_name, "~"), _compilation);
			if (symbolsForDeclarationId.Length > 0)
			{
				foreach (ISymbol item in symbolsForDeclarationId)
				{
					results.Add(item);
				}
				return;
			}
			bool flag = false;
			if (_name.Length >= 2 && _name[0] == 'e' && _name[1] == ':')
			{
				flag = true;
				_index = 2;
			}
			INamespaceOrTypeSymbol namespaceOrTypeSymbol = _compilation.GlobalNamespace;
			bool? flag2 = null;
			bool flag3;
			ImmutableArray<ISymbol> immutableArray;
			int? num;
			ParameterInfo[] array;
			while (true)
			{
				string text = ParseNextNameSegment();
				flag3 = false;
				if (text == "Item" && PeekNextChar() == '[')
				{
					flag3 = true;
					if (_compilation.Language == "C#")
					{
						text = "this[]";
					}
				}
				immutableArray = namespaceOrTypeSymbol.GetMembers(text);
				if (immutableArray.Length == 0)
				{
					return;
				}
				if (flag2.HasValue)
				{
					immutableArray = (flag2.Value ? immutableArray.Where((ISymbol s) => s.Kind == SymbolKind.NamedType).ToImmutableArray() : immutableArray.Where((ISymbol s) => s.Kind != SymbolKind.NamedType).ToImmutableArray());
					flag2 = null;
				}
				num = null;
				array = null;
				if (_scope != TargetScope.Namespace && PeekNextChar() == '`')
				{
					_index++;
					num = ReadNextInteger();
				}
				char c = PeekNextChar();
				if ((!flag3 && c == '(') || (flag3 && c == '['))
				{
					array = ParseParameterList();
					if (array != null)
					{
						break;
					}
					return;
				}
				if (c != '.' && c != '+')
				{
					break;
				}
				_index++;
				namespaceOrTypeSymbol = ((!(num > 0) && c != '+') ? GetFirstMatchingNamespaceOrType(immutableArray) : GetFirstMatchingNamedType(immutableArray, num ?? 0));
				if (namespaceOrTypeSymbol == null)
				{
					return;
				}
				if (namespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
				{
					flag2 = c == '+';
				}
			}
			if (_scope == TargetScope.Member && !flag3 && array != null)
			{
				TypeInfo? returnType = null;
				if (PeekNextChar() == ':')
				{
					_index++;
					returnType = ParseNamedType(null);
				}
				foreach (IMethodSymbol matchingMethod in GetMatchingMethods(immutableArray, num, array, returnType))
				{
					results.Add(matchingMethod);
				}
			}
			else
			{
				ISymbol symbol = _scope switch
				{
					TargetScope.Namespace => immutableArray.FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.Namespace), 
					TargetScope.Type => GetFirstMatchingNamedType(immutableArray, num ?? 0), 
					TargetScope.Member => (!flag3) ? ((!flag) ? immutableArray.FirstOrDefault((ISymbol s) => s.Kind != SymbolKind.Namespace && s.Kind != SymbolKind.NamedType) : immutableArray.FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.Event)) : GetFirstMatchingIndexer(immutableArray, array), 
					_ => throw ExceptionUtilities.UnexpectedValue(_scope), 
				};
				if (symbol != null)
				{
					results.Add(symbol);
				}
			}
		}

		private string ParseNextNameSegment()
		{
			if (PeekNextChar() == '#')
			{
				_index++;
				if (PeekNextChar() == '[')
				{
					string[] array = s_callingConventionStrings;
					foreach (string text in array)
					{
						if (text == _name.Substring(_index, text.Length))
						{
							_index += text.Length;
							break;
						}
					}
				}
			}
			int num = ((PeekNextChar() == '.') ? _name.IndexOfAny(s_nameDelimiters, _index + 1) : _name.IndexOfAny(s_nameDelimiters, _index));
			string result;
			if (num >= 0)
			{
				result = _name.Substring(_index, num - _index);
				_index = num;
			}
			else
			{
				result = _name.Substring(_index);
				_index = _name.Length;
			}
			return result;
		}

		private char PeekNextChar()
		{
			if (_index < _name.Length)
			{
				return _name[_index];
			}
			return '\0';
		}

		private int ReadNextInteger()
		{
			int num = 0;
			while (_index < _name.Length && char.IsDigit(_name[_index]))
			{
				num = num * 10 + (_name[_index] - 48);
				_index++;
			}
			return num;
		}

		private ParameterInfo[] ParseParameterList()
		{
			_index++;
			char c = PeekNextChar();
			if (c == ')' || c == ']')
			{
				_index++;
				return s_noParameters;
			}
			ArrayBuilder<ParameterInfo> arrayBuilder = new ArrayBuilder<ParameterInfo>();
			while (true)
			{
				ParameterInfo? parameterInfo = ParseParameter();
				if (parameterInfo.HasValue)
				{
					arrayBuilder.Add(parameterInfo.Value);
					if (PeekNextChar() != ',')
					{
						break;
					}
					_index++;
					continue;
				}
				arrayBuilder.Free();
				return null;
			}
			c = PeekNextChar();
			if (c == ')' || c == ']')
			{
				_index++;
				return arrayBuilder.ToArrayAndFree();
			}
			arrayBuilder.Free();
			return null;
		}

		private ParameterInfo? ParseParameter()
		{
			TypeInfo? typeInfo = ParseType(null);
			if (!typeInfo.HasValue)
			{
				return null;
			}
			bool flag = PeekNextChar() == '&';
			if (flag)
			{
				_index++;
			}
			return new ParameterInfo(typeInfo.Value, flag);
		}

		private TypeInfo? ParseType(ISymbol bindingContext)
		{
			IgnoreCustomModifierList();
			TypeInfo? result;
			if (PeekNextChar() == '!')
			{
				result = ParseIndexedTypeParameter(bindingContext);
			}
			else
			{
				result = ParseNamedType(bindingContext);
				if (bindingContext != null && result.HasValue && !result.Value.IsBound)
				{
					_index = result.Value.StartIndex;
					result = ParseNamedTypeParameter(bindingContext);
				}
			}
			if (!result.HasValue)
			{
				return null;
			}
			if (result.Value.IsBound)
			{
				ITypeSymbol typeSymbol = result.Value.Type;
				while (true)
				{
					IgnoreCustomModifierList();
					switch (PeekNextChar())
					{
					case '[':
						typeSymbol = ParseArrayType(typeSymbol);
						if (typeSymbol == null)
						{
							return null;
						}
						break;
					case '*':
						_index++;
						typeSymbol = _compilation.CreatePointerTypeSymbol(typeSymbol);
						break;
					default:
						return TypeInfo.Create(typeSymbol);
					}
				}
			}
			IgnorePointerAndArraySpecifiers();
			return result;
		}

		private void IgnoreCustomModifierList()
		{
			if (PeekNextChar() == '{')
			{
				while (_index < _name.Length && _name[_index] != '}')
				{
					_index++;
				}
			}
		}

		private void IgnorePointerAndArraySpecifiers()
		{
			bool flag = false;
			while (_index < _name.Length)
			{
				switch (PeekNextChar())
				{
				case '[':
					flag = true;
					break;
				case ']':
					if (!flag)
					{
						return;
					}
					flag = false;
					break;
				default:
					if (!flag)
					{
						return;
					}
					break;
				case '*':
					break;
				}
				_index++;
			}
		}

		private TypeInfo? ParseIndexedTypeParameter(ISymbol bindingContext)
		{
			int index = _index;
			_index++;
			if (PeekNextChar() == '!')
			{
				_index++;
				int num = ReadNextInteger();
				if (bindingContext is IMethodSymbol { TypeParameters: { Length: var length } } methodSymbol)
				{
					if (length > 0 && num < length)
					{
						return TypeInfo.Create(methodSymbol.TypeParameters[num]);
					}
					return null;
				}
				return TypeInfo.CreateUnbound(index);
			}
			int n = ReadNextInteger();
			if (bindingContext != null)
			{
				ITypeParameterSymbol nthTypeParameter = GetNthTypeParameter(bindingContext.ContainingType, n);
				if (nthTypeParameter != null)
				{
					return TypeInfo.Create(nthTypeParameter);
				}
				return null;
			}
			return TypeInfo.CreateUnbound(index);
		}

		private TypeInfo? ParseNamedTypeParameter(ISymbol bindingContext)
		{
			string text = ParseNextNameSegment();
			if (bindingContext is IMethodSymbol methodSymbol)
			{
				for (int i = 0; i < methodSymbol.TypeParameters.Length; i++)
				{
					if (methodSymbol.TypeParameters[i].Name == text)
					{
						return TypeInfo.Create(methodSymbol.TypeArguments[i]);
					}
				}
			}
			for (INamedTypeSymbol containingType = bindingContext.ContainingType; containingType != null; containingType = containingType.ContainingType)
			{
				for (int j = 0; j < containingType.TypeParameters.Length; j++)
				{
					if (containingType.TypeParameters[j].Name == text)
					{
						return TypeInfo.Create(containingType.TypeArguments[j]);
					}
				}
			}
			return null;
		}

		private TypeInfo? ParseNamedType(ISymbol bindingContext)
		{
			INamespaceOrTypeSymbol namespaceOrTypeSymbol = _compilation.GlobalNamespace;
			int index = _index;
			ImmutableArray<ISymbol> members;
			int num;
			TypeInfo[] array;
			while (true)
			{
				string name = ParseNextNameSegment();
				members = namespaceOrTypeSymbol.GetMembers(name);
				if (members.Length == 0)
				{
					return TypeInfo.CreateUnbound(index);
				}
				num = 0;
				array = null;
				if (PeekNextChar() == '`')
				{
					_index++;
					num = ReadNextInteger();
				}
				if (PeekNextChar() == '<')
				{
					array = ParseTypeArgumentList(bindingContext);
					if (array == null)
					{
						return null;
					}
					if (array.Any((TypeInfo a) => !a.IsBound))
					{
						return TypeInfo.CreateUnbound(index);
					}
				}
				char c = PeekNextChar();
				if (c != '.' && c != '+')
				{
					break;
				}
				_index++;
				namespaceOrTypeSymbol = ((num <= 0 && c != '+') ? GetFirstMatchingNamespaceOrType(members) : GetFirstMatchingNamedType(members, num));
				if (namespaceOrTypeSymbol == null)
				{
					return null;
				}
			}
			INamedTypeSymbol namedTypeSymbol = GetFirstMatchingNamedType(members, num);
			if (namedTypeSymbol == null)
			{
				return null;
			}
			if (array != null)
			{
				namedTypeSymbol = namedTypeSymbol.Construct(array.Select((TypeInfo t) => t.Type).ToArray());
			}
			return TypeInfo.Create(namedTypeSymbol);
		}

		private TypeInfo[] ParseTypeArgumentList(ISymbol bindingContext)
		{
			_index++;
			ArrayBuilder<TypeInfo> arrayBuilder = new ArrayBuilder<TypeInfo>();
			while (true)
			{
				TypeInfo? typeInfo = ParseType(bindingContext);
				if (!typeInfo.HasValue)
				{
					arrayBuilder.Free();
					return null;
				}
				arrayBuilder.Add(typeInfo.Value);
				if (PeekNextChar() != ',')
				{
					break;
				}
				_index++;
			}
			if (PeekNextChar() == '>')
			{
				_index++;
				return arrayBuilder.ToArrayAndFree();
			}
			arrayBuilder.Free();
			return null;
		}

		private ITypeSymbol ParseArrayType(ITypeSymbol typeSymbol)
		{
			_index++;
			int num = 1;
			while (true)
			{
				char c = PeekNextChar();
				switch (c)
				{
				case ',':
					num++;
					break;
				case ']':
					_index++;
					return _compilation.CreateArrayTypeSymbol(typeSymbol, num);
				default:
					if (!char.IsDigit(c) && c != '.')
					{
						return null;
					}
					break;
				}
				_index++;
			}
		}

		private ISymbol GetFirstMatchingIndexer(ImmutableArray<ISymbol> candidateMembers, ParameterInfo[] parameters)
		{
			foreach (ISymbol item in candidateMembers)
			{
				if (item is IPropertySymbol propertySymbol && AllParametersMatch(propertySymbol.Parameters, parameters))
				{
					return propertySymbol;
				}
			}
			return null;
		}

		private ImmutableArray<IMethodSymbol> GetMatchingMethods(ImmutableArray<ISymbol> candidateMembers, int? arity, ParameterInfo[] parameters, TypeInfo? returnType)
		{
			ArrayBuilder<IMethodSymbol> arrayBuilder = new ArrayBuilder<IMethodSymbol>();
			foreach (ISymbol item in candidateMembers)
			{
				if (!(item is IMethodSymbol methodSymbol) || (arity.HasValue && methodSymbol.Arity != arity) || !AllParametersMatch(methodSymbol.Parameters, parameters))
				{
					continue;
				}
				if (!returnType.HasValue)
				{
					arrayBuilder.Add(methodSymbol);
					continue;
				}
				ITypeSymbol typeSymbol = BindParameterOrReturnType(methodSymbol, returnType.Value);
				if (typeSymbol != null && methodSymbol.ReturnType.Equals(typeSymbol))
				{
					arrayBuilder.Add(methodSymbol);
				}
			}
			return arrayBuilder.ToImmutableAndFree();
		}

		private bool AllParametersMatch(ImmutableArray<IParameterSymbol> symbolParameters, ParameterInfo[] expectedParameters)
		{
			if (symbolParameters.Length != expectedParameters.Length)
			{
				return false;
			}
			for (int i = 0; i < expectedParameters.Length; i++)
			{
				if (!ParameterMatches(symbolParameters[i], expectedParameters[i]))
				{
					return false;
				}
			}
			return true;
		}

		private bool ParameterMatches(IParameterSymbol symbol, ParameterInfo parameterInfo)
		{
			if (symbol.RefKind == RefKind.None == parameterInfo.IsRefOrOut)
			{
				return false;
			}
			ITypeSymbol typeSymbol = BindParameterOrReturnType(symbol.ContainingSymbol, parameterInfo.Type);
			if (typeSymbol != null)
			{
				return symbol.Type.Equals(typeSymbol);
			}
			return false;
		}

		private ITypeSymbol BindParameterOrReturnType(ISymbol bindingContext, TypeInfo type)
		{
			if (type.IsBound)
			{
				return type.Type;
			}
			int index = _index;
			_index = type.StartIndex;
			TypeInfo? typeInfo = ParseType(bindingContext);
			_index = index;
			return typeInfo?.Type;
		}

		private static INamedTypeSymbol GetFirstMatchingNamedType(ImmutableArray<ISymbol> candidateMembers, int arity)
		{
			return (INamedTypeSymbol)candidateMembers.FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.NamedType && ((INamedTypeSymbol)s).Arity == arity);
		}

		private static INamespaceOrTypeSymbol GetFirstMatchingNamespaceOrType(ImmutableArray<ISymbol> candidateMembers)
		{
			return (INamespaceOrTypeSymbol)candidateMembers.FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.Namespace || s.Kind == SymbolKind.NamedType);
		}

		private static ITypeParameterSymbol GetNthTypeParameter(INamedTypeSymbol typeSymbol, int n)
		{
			int typeParameterCount = GetTypeParameterCount(typeSymbol.ContainingType);
			if (n < typeParameterCount)
			{
				return GetNthTypeParameter(typeSymbol.ContainingType, n);
			}
			return typeSymbol.TypeParameters[n - typeParameterCount];
		}

		private static int GetTypeParameterCount(INamedTypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
			{
				return 0;
			}
			return typeSymbol.TypeParameters.Length + GetTypeParameterCount(typeSymbol.ContainingType);
		}
	}

	private static readonly SmallDictionary<string, TargetScope> s_suppressMessageScopeTypes = new SmallDictionary<string, TargetScope>
	{
		{
			null,
			TargetScope.None
		},
		{
			"module",
			TargetScope.Module
		},
		{
			"namespace",
			TargetScope.Namespace
		},
		{
			"resource",
			TargetScope.Resource
		},
		{
			"type",
			TargetScope.Type
		},
		{
			"member",
			TargetScope.Member
		}
	};

	private readonly Compilation _compilation;

	private GlobalSuppressions _lazyGlobalSuppressions;

	private readonly ConcurrentDictionary<ISymbol, ImmutableDictionary<string, SuppressMessageInfo>> _localSuppressionsBySymbol;

	private ISymbol _lazySuppressMessageAttribute;

	private const string s_suppressionPrefix = "~";

	private ISymbol SuppressMessageAttribute
	{
		get
		{
			if (_lazySuppressMessageAttribute == null)
			{
				_lazySuppressMessageAttribute = _compilation.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute");
			}
			return _lazySuppressMessageAttribute;
		}
	}

	internal SuppressMessageAttributeState(Compilation compilation)
	{
		_compilation = compilation;
		_localSuppressionsBySymbol = new ConcurrentDictionary<ISymbol, ImmutableDictionary<string, SuppressMessageInfo>>();
	}

	public Diagnostic ApplySourceSuppressions(Diagnostic diagnostic, ISymbol symbolOpt = null)
	{
		if (diagnostic.IsSuppressed)
		{
			return diagnostic;
		}
		if (IsDiagnosticSuppressed(diagnostic, out var _, null))
		{
			diagnostic = diagnostic.WithIsSuppressed(isSuppressed: true);
		}
		return diagnostic;
	}

	public bool IsDiagnosticSuppressed(Diagnostic diagnostic, out AttributeData suppressingAttribute)
	{
		if (IsDiagnosticSuppressed(diagnostic, out var info, null))
		{
			suppressingAttribute = info.Attribute;
			return true;
		}
		suppressingAttribute = null;
		return false;
	}

	private bool IsDiagnosticSuppressed(Diagnostic diagnostic, out SuppressMessageInfo info, ISymbol symbolOpt = null)
	{
		if (symbolOpt != null && IsDiagnosticSuppressed(diagnostic.Id, symbolOpt, out info))
		{
			return true;
		}
		return IsDiagnosticSuppressed(diagnostic.Id, diagnostic.Location, out info);
	}

	private bool IsDiagnosticSuppressed(string id, ISymbol symbol, out SuppressMessageInfo info)
	{
		if (symbol.Kind == SymbolKind.Namespace)
		{
			info = default;
			return false;
		}
		if (symbol.Kind == SymbolKind.Method)
		{
			ISymbol associatedSymbol = ((IMethodSymbol)symbol).AssociatedSymbol;
			if (associatedSymbol != null && (IsDiagnosticLocallySuppressed(id, associatedSymbol, out info) || IsDiagnosticGloballySuppressed(id, associatedSymbol, out info)))
			{
				return true;
			}
		}
		if (IsDiagnosticLocallySuppressed(id, symbol, out info) || IsDiagnosticGloballySuppressed(id, symbol, out info))
		{
			return true;
		}
		ISymbol containingSymbol = symbol.ContainingSymbol;
		if (containingSymbol != null)
		{
			return IsDiagnosticSuppressed(id, containingSymbol, out info);
		}
		return false;
	}

	private bool IsDiagnosticSuppressed(string id, Location location, out SuppressMessageInfo info)
	{
		info = default;
		if (IsDiagnosticGloballySuppressed(id, null, out info))
		{
			return true;
		}
		if (location.IsInSource)
		{
			SemanticModel semanticModel = _compilation.GetSemanticModel(location.SourceTree);
			bool flag = true;
			for (SyntaxNode syntaxNode = location.SourceTree.GetRoot().FindNode(location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true); syntaxNode != null; syntaxNode = syntaxNode.Parent)
			{
				foreach (ISymbol item in semanticModel.GetDeclaredSymbolsForNode(syntaxNode))
				{
					if (item.Kind == SymbolKind.Namespace)
					{
						if (flag)
						{
							return IsDiagnosticGloballySuppressed(id, item, out info);
						}
						return false;
					}
					if (IsDiagnosticLocallySuppressed(id, item, out info) || IsDiagnosticGloballySuppressed(id, item, out info))
					{
						return true;
					}
					flag = false;
				}
			}
		}
		return false;
	}

	private bool IsDiagnosticGloballySuppressed(string id, ISymbol symbolOpt, out SuppressMessageInfo info)
	{
		DecodeGlobalSuppressMessageAttributes();
		if (!_lazyGlobalSuppressions.HasCompilationWideSuppression(id, out info))
		{
			if (symbolOpt != null)
			{
				return _lazyGlobalSuppressions.HasGlobalSymbolSuppression(symbolOpt, id, out info);
			}
			return false;
		}
		return true;
	}

	private bool IsDiagnosticLocallySuppressed(string id, ISymbol symbol, out SuppressMessageInfo info)
	{
		return _localSuppressionsBySymbol.GetOrAdd(symbol, DecodeLocalSuppressMessageAttributes).TryGetValue(id, out info);
	}

	private void DecodeGlobalSuppressMessageAttributes()
	{
		if (_lazyGlobalSuppressions != null)
		{
			return;
		}
		GlobalSuppressions globalSuppressions = new GlobalSuppressions();
		DecodeGlobalSuppressMessageAttributes(_compilation, _compilation.Assembly, globalSuppressions);
		foreach (IModuleSymbol module in _compilation.Assembly.Modules)
		{
			DecodeGlobalSuppressMessageAttributes(_compilation, module, globalSuppressions);
		}
		Interlocked.CompareExchange(ref _lazyGlobalSuppressions, globalSuppressions, null);
	}

	private ImmutableDictionary<string, SuppressMessageInfo> DecodeLocalSuppressMessageAttributes(ISymbol symbol)
	{
		IEnumerable<AttributeData> attributes = from a in symbol.GetAttributes()
			where a.AttributeClass == SuppressMessageAttribute
			select a;
		return DecodeLocalSuppressMessageAttributes(symbol, attributes);
	}

	private static ImmutableDictionary<string, SuppressMessageInfo> DecodeLocalSuppressMessageAttributes(ISymbol symbol, IEnumerable<AttributeData> attributes)
	{
		ImmutableDictionary<string, SuppressMessageInfo>.Builder builder = ImmutableDictionary.CreateBuilder<string, SuppressMessageInfo>();
		foreach (AttributeData attribute in attributes)
		{
			if (TryDecodeSuppressMessageAttributeData(attribute, out var info))
			{
				AddOrUpdate(info, builder);
			}
		}
		return builder.ToImmutable();
	}

	private static void AddOrUpdate(SuppressMessageInfo info, IDictionary<string, SuppressMessageInfo> builder)
	{
		if (!builder.TryGetValue(info.Id, out var _))
		{
			builder[info.Id] = info;
		}
	}

	private void DecodeGlobalSuppressMessageAttributes(Compilation compilation, ISymbol symbol, GlobalSuppressions globalSuppressions)
	{
		IEnumerable<AttributeData> attributes = from a in symbol.GetAttributes()
			where a.AttributeClass == SuppressMessageAttribute
			select a;
		DecodeGlobalSuppressMessageAttributes(compilation, symbol, globalSuppressions, attributes);
	}

	private static void DecodeGlobalSuppressMessageAttributes(Compilation compilation, ISymbol symbol, GlobalSuppressions globalSuppressions, IEnumerable<AttributeData> attributes)
	{
		foreach (AttributeData attribute in attributes)
		{
			if (!TryDecodeSuppressMessageAttributeData(attribute, out var info))
			{
				continue;
			}
			string key = ((info.Scope != null) ? info.Scope.ToLowerInvariant() : null);
			if (!s_suppressMessageScopeTypes.TryGetValue(key, out var value))
			{
				continue;
			}
			if ((value == TargetScope.Module || value == TargetScope.None) && info.Target == null)
			{
				globalSuppressions.AddCompilationWideSuppression(info);
			}
			else
			{
				if (info.Target == null)
				{
					continue;
				}
				foreach (ISymbol item in ResolveTargetSymbols(compilation, info.Target, value))
				{
					globalSuppressions.AddGlobalSymbolSuppression(item, info);
				}
			}
		}
	}

	internal static IEnumerable<ISymbol> ResolveTargetSymbols(Compilation compilation, string target, TargetScope scope)
	{
		if (scope == TargetScope.Namespace || (uint)(scope - 4) <= 1u)
		{
			List<ISymbol> list = new List<ISymbol>();
			new TargetSymbolResolver(compilation, scope, target).Resolve(list);
			return list;
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private static bool TryDecodeSuppressMessageAttributeData(AttributeData attribute, out SuppressMessageInfo info)
	{
		info = default;
		if (attribute.CommonConstructorArguments.Length < 2)
		{
			return false;
		}
		info.Id = attribute.CommonConstructorArguments[1].Value as string;
		if (info.Id == null)
		{
			return false;
		}
		int num = info.Id.IndexOf(':');
		if (num != -1)
		{
			info.Id = info.Id.Remove(num);
		}
		info.Scope = attribute.DecodeNamedArgument<string>("Scope", SpecialType.System_String);
		info.Target = attribute.DecodeNamedArgument<string>("Target", SpecialType.System_String);
		info.MessageId = attribute.DecodeNamedArgument<string>("MessageId", SpecialType.System_String);
		info.Attribute = attribute;
		return true;
	}
}
