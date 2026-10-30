using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class LocalCacheSymbolReferenceLoader : ISymbolReferenceLoader
{
	private readonly SymbolReferenceAnalyzer analyzer;

	public LocalCacheSymbolReferenceLoader(string cachePath)
	{
		analyzer = new SymbolReferenceAnalyzer(cachePath);
	}

	public ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		return ReadFromNavAppPackage(reference, diagnostics, (NavAppPackageReader reader) => SymbolReferenceJsonReader.ReadModule(reader.ReadSymbolReferenceFile(), diagnostics));
	}

	public IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		return ReadFromNavAppPackage(reference, diagnostics, (NavAppManifest manifest) => manifest?.GetAllReferences()) ?? Enumerable.Empty<SymbolReferenceSpecification>();
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private T ReadFromNavAppPackage<T>(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, Func<NavAppManifest, T> function)
	{
		if (function == null)
		{
			return default;
		}
		string text = analyzer.FindMatchingPackageFile(reference, diagnostics, out var matchingManifest);
		if (text == null || matchingManifest == null)
		{
			return default;
		}
		try
		{
			return function(matchingManifest);
		}
		catch (Exception)
		{
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1023, text));
			return default;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private T ReadFromNavAppPackage<T>(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, Func<NavAppPackageReader, T> function)
	{
		if (function == null)
		{
			return default;
		}
		string text = analyzer.FindMatchingPackageFile(reference, diagnostics, out var _);
		if (text == null)
		{
			return default;
		}
		try
		{
			using FileStream stream = File.OpenRead(text);
			using NavAppPackageReader arg = NavAppPackageReader.Create(stream);
			return function(arg);
		}
		catch (Exception)
		{
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1023, text));
			return default;
		}
	}
}
