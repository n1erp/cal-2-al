using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class CompilationReference : MetadataReference, IEquatable<CompilationReference>
{
	public Compilation Compilation => CompilationCore;

	internal abstract Compilation CompilationCore { get; }

	public override string Display => Compilation.AssemblyName;

	internal CompilationReference(MetadataReferenceProperties properties)
		: base(properties)
	{
	}

	internal static MetadataReferenceProperties GetProperties(Compilation compilation, ImmutableArray<string> aliases, bool embedInteropTypes)
	{
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (compilation.IsSubmission)
		{
			throw new NotSupportedException(CodeAnalysisResources.CannotCreateReferenceToSubmission);
		}
		if (compilation.Options.OutputKind == OutputKind.NetModule)
		{
			throw new NotSupportedException(CodeAnalysisResources.CannotCreateReferenceToModule);
		}
		return new MetadataReferenceProperties(MetadataImageKind.Assembly, aliases, embedInteropTypes);
	}

	public new CompilationReference WithAliases(IEnumerable<string> aliases)
	{
		return WithAliases(ImmutableArray.CreateRange(aliases));
	}

	public new CompilationReference WithAliases(ImmutableArray<string> aliases)
	{
		return WithProperties(Properties.WithAliases(aliases));
	}

	public new CompilationReference WithEmbedInteropTypes(bool value)
	{
		return WithProperties(Properties.WithEmbedInteropTypes(value));
	}

	public new CompilationReference WithProperties(MetadataReferenceProperties properties)
	{
		if (properties == Properties)
		{
			return this;
		}
		if (properties.Kind == MetadataImageKind.Module)
		{
			throw new ArgumentException(CodeAnalysisResources.CannotCreateReferenceToModule);
		}
		return WithPropertiesImpl(properties);
	}

	internal sealed override MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties)
	{
		if (properties.Kind == MetadataImageKind.Module)
		{
			throw new NotSupportedException(CodeAnalysisResources.CannotCreateReferenceToModule);
		}
		return WithPropertiesImpl(properties);
	}

	internal abstract CompilationReference WithPropertiesImpl(MetadataReferenceProperties properties);

	public bool Equals(CompilationReference other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Compilation, other.Compilation))
		{
			return object.Equals(Properties, other.Properties);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as CompilationReference);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Compilation.GetHashCode(), Properties.GetHashCode());
	}
}
