using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SymbolDistinguisher
{
	private sealed class Description : IFormattable
	{
		private readonly SymbolDistinguisher _distinguisher;

		private readonly int _index;

		public Description(SymbolDistinguisher distinguisher, int index)
		{
			_distinguisher = distinguisher;
			_index = index;
		}

		private Symbol GetSymbol()
		{
			if (_index != 0)
			{
				return _distinguisher._symbol1;
			}
			return _distinguisher._symbol0;
		}

		public override bool Equals(object obj)
		{
			if (obj is Description description && _distinguisher._compilation == description._distinguisher._compilation)
			{
				return GetSymbol() == description.GetSymbol();
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = GetSymbol().GetHashCode();
			Compilation compilation = _distinguisher._compilation;
			if (compilation != null)
			{
				num = Hash.Combine(num, compilation.GetHashCode());
			}
			return num;
		}

		public override string ToString()
		{
			return _distinguisher.GetDescription(_index);
		}

		string IFormattable.ToString(string format, IFormatProvider formatProvider)
		{
			return ToString();
		}
	}

	private readonly Compilation _compilation;

	private readonly Symbol _symbol0;

	private readonly Symbol _symbol1;

	private ImmutableArray<string> _lazyDescriptions;

	public IFormattable First => new Description(this, 0);

	public IFormattable Second => new Description(this, 1);

	public SymbolDistinguisher(Compilation compilation, Symbol symbol0, Symbol symbol1)
	{
		_compilation = compilation;
		_symbol0 = symbol0;
		_symbol1 = symbol1;
	}

	[Conditional("DEBUG")]
	private static void CheckSymbolKind(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
		case SymbolKind.DynamicType:
		case SymbolKind.ErrorType:
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Parameter:
		case SymbolKind.PointerType:
		case SymbolKind.Property:
		case SymbolKind.TypeParameter:
			return;
		}
		throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
	}

	private void MakeDescriptions()
	{
		if (!_lazyDescriptions.IsDefault)
		{
			return;
		}
		string text = _symbol0.ToDisplayString();
		string text2 = _symbol1.ToDisplayString();
		if (text == text2)
		{
			Symbol symbol = UnwrapSymbol(_symbol0);
			Symbol symbol2 = UnwrapSymbol(_symbol1);
			string text3 = GetLocationString(_compilation, symbol);
			string text4 = GetLocationString(_compilation, symbol2);
			if (text3 == text4)
			{
				AssemblySymbol containingAssembly = symbol.ContainingAssembly;
				AssemblySymbol containingAssembly2 = symbol2.ContainingAssembly;
				if ((object)containingAssembly != null && (object)containingAssembly2 != null)
				{
					text3 = containingAssembly.Identity.ToString();
					text4 = containingAssembly2.Identity.ToString();
				}
			}
			if (text3 != text4)
			{
				if (text3 != null)
				{
					text = text + " [" + text3 + "]";
				}
				if (text4 != null)
				{
					text2 = text2 + " [" + text4 + "]";
				}
			}
		}
		if (_lazyDescriptions.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _lazyDescriptions, ImmutableArray.Create(text, text2));
		}
	}

	private static Symbol UnwrapSymbol(Symbol symbol)
	{
		while (true)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.Parameter:
				symbol = ((ParameterSymbol)symbol).Type;
				break;
			case SymbolKind.PointerType:
				symbol = ((PointerTypeSymbol)symbol).PointedAtType;
				break;
			case SymbolKind.ArrayType:
				symbol = ((ArrayTypeSymbol)symbol).ElementType;
				break;
			default:
				return symbol;
			}
		}
	}

	private static string GetLocationString(Compilation compilation, Symbol unwrappedSymbol)
	{
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = unwrappedSymbol.DeclaringSyntaxReferences;
		if (declaringSyntaxReferences.Length > 0)
		{
			SyntaxTree syntaxTree = declaringSyntaxReferences[0].SyntaxTree;
			TextSpan span = declaringSyntaxReferences[0].Span;
			string displayPath = syntaxTree.GetDisplayPath(span, compilation?.Options.SourceReferenceResolver);
			if (!string.IsNullOrEmpty(displayPath))
			{
				return $"{displayPath}({syntaxTree.GetDisplayLineNumber(span)})";
			}
		}
		AssemblySymbol containingAssembly = unwrappedSymbol.ContainingAssembly;
		if ((object)containingAssembly != null)
		{
			if (compilation != null && compilation.GetMetadataReference(containingAssembly) is PortableExecutableReference { FilePath: var filePath } && !string.IsNullOrEmpty(filePath))
			{
				return filePath;
			}
			return containingAssembly.Identity.ToString();
		}
		return null;
	}

	private string GetDescription(int index)
	{
		MakeDescriptions();
		return _lazyDescriptions[index];
	}
}
