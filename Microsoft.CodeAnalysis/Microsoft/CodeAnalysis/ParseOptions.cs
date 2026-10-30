using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class ParseOptions
{
	private readonly Lazy<ImmutableArray<Diagnostic>> _lazyErrors;

	public SourceCodeKind Kind { get; protected set; }

	public SourceCodeKind SpecifiedKind { get; protected set; }

	public DocumentationMode DocumentationMode { get; protected set; }

	public abstract string Language { get; }

	public ImmutableArray<Diagnostic> Errors => _lazyErrors.Value;

	public abstract IReadOnlyDictionary<string, string> Features { get; }

	public abstract IEnumerable<string> PreprocessorSymbolNames { get; }

	internal ParseOptions(SourceCodeKind kind, DocumentationMode documentationMode)
	{
		SpecifiedKind = kind;
		Kind = kind.MapSpecifiedToEffectiveKind();
		DocumentationMode = documentationMode;
		_lazyErrors = new Lazy<ImmutableArray<Diagnostic>>(() =>
		{
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
			ValidateOptions(instance);
			return instance.ToImmutableAndFree();
		});
	}

	public ParseOptions WithKind(SourceCodeKind kind)
	{
		return CommonWithKind(kind);
	}

	internal abstract void ValidateOptions(ArrayBuilder<Diagnostic> builder);

	internal void ValidateOptions(ArrayBuilder<Diagnostic> builder, CommonMessageProvider messageProvider)
	{
		if (!SpecifiedKind.IsValid())
		{
			builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_BadSourceCodeKind, Location.None, SpecifiedKind.ToString()));
		}
		if (!DocumentationMode.IsValid())
		{
			builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_BadDocumentationMode, Location.None, DocumentationMode.ToString()));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract ParseOptions CommonWithKind(SourceCodeKind kind);

	public ParseOptions WithDocumentationMode(DocumentationMode documentationMode)
	{
		return CommonWithDocumentationMode(documentationMode);
	}

	protected abstract ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode);

	public ParseOptions WithFeatures(IEnumerable<KeyValuePair<string, string>> features)
	{
		return CommonWithFeatures(features);
	}

	protected abstract ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features);

	public abstract override bool Equals(object obj);

	protected bool EqualsHelper(ParseOptions other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (SpecifiedKind == other.SpecifiedKind && DocumentationMode == other.DocumentationMode && Features.SequenceEqual(other.Features))
		{
			if (PreprocessorSymbolNames != null)
			{
				return PreprocessorSymbolNames.SequenceEqual(other.PreprocessorSymbolNames, StringComparer.Ordinal);
			}
			return other.PreprocessorSymbolNames == null;
		}
		return false;
	}

	public abstract override int GetHashCode();

	protected int GetHashCodeHelper()
	{
		return Hash.Combine((int)SpecifiedKind, Hash.Combine((int)DocumentationMode, Hash.Combine(HashFeatures(Features), Hash.Combine(Hash.CombineValues(PreprocessorSymbolNames, StringComparer.Ordinal), 0))));
	}

	private static int HashFeatures(IReadOnlyDictionary<string, string> features)
	{
		int num = 0;
		foreach (KeyValuePair<string, string> feature in features)
		{
			num = Hash.Combine(feature.Key.GetHashCode(), Hash.Combine(feature.Value.GetHashCode(), num));
		}
		return num;
	}

	public static bool operator ==(ParseOptions left, ParseOptions right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ParseOptions left, ParseOptions right)
	{
		return !object.Equals(left, right);
	}
}
