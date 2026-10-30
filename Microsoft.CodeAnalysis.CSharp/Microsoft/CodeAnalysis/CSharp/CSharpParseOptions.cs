using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpParseOptions : ParseOptions, IEquatable<CSharpParseOptions>
{
	private ImmutableDictionary<string, string> _features;

	public static CSharpParseOptions Default { get; } = new CSharpParseOptions();

	public LanguageVersion LanguageVersion { get; private set; }

	public LanguageVersion SpecifiedLanguageVersion { get; private set; }

	internal ImmutableArray<string> PreprocessorSymbols { get; private set; }

	public override IEnumerable<string> PreprocessorSymbolNames => PreprocessorSymbols;

	public override string Language => "C#";

	public override IReadOnlyDictionary<string, string> Features => _features;

	public CSharpParseOptions(LanguageVersion languageVersion = LanguageVersion.Default, DocumentationMode documentationMode = DocumentationMode.Parse, SourceCodeKind kind = SourceCodeKind.Regular, IEnumerable<string> preprocessorSymbols = null)
		: this(languageVersion, documentationMode, kind, preprocessorSymbols.ToImmutableArrayOrEmpty(), ImmutableDictionary<string, string>.Empty)
	{
	}

	internal CSharpParseOptions(LanguageVersion languageVersion, DocumentationMode documentationMode, SourceCodeKind kind, ImmutableArray<string> preprocessorSymbols, IReadOnlyDictionary<string, string> features)
		: base(kind, documentationMode)
	{
		SpecifiedLanguageVersion = languageVersion;
		LanguageVersion = languageVersion.MapSpecifiedToEffectiveVersion();
		PreprocessorSymbols = preprocessorSymbols.ToImmutableArrayOrEmpty();
		_features = features?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
	}

	private CSharpParseOptions(CSharpParseOptions other)
		: this(other.SpecifiedLanguageVersion, other.DocumentationMode, other.Kind, other.PreprocessorSymbols, other.Features)
	{
	}

	public new CSharpParseOptions WithKind(SourceCodeKind kind)
	{
		if (kind == SpecifiedKind)
		{
			return this;
		}
		SourceCodeKind kind2 = kind.MapSpecifiedToEffectiveKind();
		return new CSharpParseOptions(this)
		{
			SpecifiedKind = kind,
			Kind = kind2
		};
	}

	public CSharpParseOptions WithLanguageVersion(LanguageVersion version)
	{
		if (version == SpecifiedLanguageVersion)
		{
			return this;
		}
		LanguageVersion languageVersion = version.MapSpecifiedToEffectiveVersion();
		return new CSharpParseOptions(this)
		{
			SpecifiedLanguageVersion = version,
			LanguageVersion = languageVersion
		};
	}

	public CSharpParseOptions WithPreprocessorSymbols(IEnumerable<string> preprocessorSymbols)
	{
		return WithPreprocessorSymbols(preprocessorSymbols.AsImmutableOrNull());
	}

	public CSharpParseOptions WithPreprocessorSymbols(params string[] preprocessorSymbols)
	{
		return WithPreprocessorSymbols(ImmutableArray.Create(preprocessorSymbols));
	}

	public CSharpParseOptions WithPreprocessorSymbols(ImmutableArray<string> symbols)
	{
		if (symbols.IsDefault)
		{
			symbols = ImmutableArray<string>.Empty;
		}
		if (symbols.Equals(PreprocessorSymbols))
		{
			return this;
		}
		return new CSharpParseOptions(this)
		{
			PreprocessorSymbols = symbols
		};
	}

	public new CSharpParseOptions WithDocumentationMode(DocumentationMode documentationMode)
	{
		if (documentationMode == DocumentationMode)
		{
			return this;
		}
		return new CSharpParseOptions(this)
		{
			DocumentationMode = documentationMode
		};
	}

	public override ParseOptions CommonWithKind(SourceCodeKind kind)
	{
		return WithKind(kind);
	}

	protected override ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode)
	{
		return WithDocumentationMode(documentationMode);
	}

	protected override ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features)
	{
		return WithFeatures(features);
	}

	public new CSharpParseOptions WithFeatures(IEnumerable<KeyValuePair<string, string>> features)
	{
		ImmutableDictionary<string, string> features2 = features?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string>.Empty;
		return new CSharpParseOptions(this)
		{
			_features = features2
		};
	}

	internal override void ValidateOptions(ArrayBuilder<Diagnostic> builder)
	{
		ValidateOptions(builder, MessageProvider.Instance);
		if (!LanguageVersion.IsValid())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 8192, LanguageVersion.ToString()));
		}
		if (PreprocessorSymbols.IsDefaultOrEmpty)
		{
			return;
		}
		foreach (string preprocessorSymbol in PreprocessorSymbols)
		{
			if (preprocessorSymbol == null)
			{
				builder.Add(Diagnostic.Create(MessageProvider.Instance, 8301, "null"));
			}
			else if (!SyntaxFacts.IsValidIdentifier(preprocessorSymbol))
			{
				builder.Add(Diagnostic.Create(MessageProvider.Instance, 8301, preprocessorSymbol));
			}
		}
	}

	internal bool IsFeatureEnabled(MessageID feature)
	{
		LanguageVersion languageVersion = LanguageVersion;
		LanguageVersion languageVersion2 = feature.RequiredVersion();
		return languageVersion >= languageVersion2;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as CSharpParseOptions);
	}

	public bool Equals(CSharpParseOptions other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (!EqualsHelper(other))
		{
			return false;
		}
		return SpecifiedLanguageVersion == other.SpecifiedLanguageVersion;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(GetHashCodeHelper(), Hash.Combine((int)SpecifiedLanguageVersion, 0));
	}
}
