using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal class SymbolReferenceAnalyzer
{
	private readonly string cachePath;

	internal SymbolReferenceAnalyzer(string cachePath)
	{
		this.cachePath = cachePath;
	}

	internal IEnumerable<SymbolReferenceSpecification> FindMissingReferences(IEnumerable<SymbolReferenceSpecification> declaredReferences, out IList<Diagnostic> diagnostics)
	{
		diagnostics = new List<Diagnostic>();
		if (!declaredReferences.Any())
		{
			return Enumerable.Empty<SymbolReferenceSpecification>();
		}
		return from x in ResolveAllMatchingPackageFiles(declaredReferences, diagnostics)
			where !x.Value.Found
			select x.Key;
	}

	internal string FindMatchingPackageFile(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, out NavAppManifest matchingManifest)
	{
		matchingManifest = null;
		if (!TryGetPackagesFromCache(diagnostics, out var matchingFiles))
		{
			return null;
		}
		string text = null;
		Version version = new Version(0, 0);
		foreach (string item in matchingFiles)
		{
			NavAppManifest navAppManifest = ReadNavAppManifest(item, diagnostics);
			if (navAppManifest != null && reference.IsSatisfiedBy(navAppManifest.AppPublisher, navAppManifest.AppName, navAppManifest.AppVersion) && navAppManifest.AppVersion > version)
			{
				version = navAppManifest.AppVersion;
				text = item;
				matchingManifest = navAppManifest;
			}
		}
		VerifyPackageFound(reference, matchingManifest, text, diagnostics);
		return text;
	}

	internal IDictionary<SymbolReferenceSpecification, LocalCacheSymbolReferencePackage> ResolveAllMatchingPackageFiles(IEnumerable<SymbolReferenceSpecification> references, IList<Diagnostic> diagnostics)
	{
		Dictionary<SymbolReferenceSpecification, LocalCacheSymbolReferencePackage> dictionary = new Dictionary<SymbolReferenceSpecification, LocalCacheSymbolReferencePackage>();
		foreach (SymbolReferenceSpecification reference in references)
		{
			dictionary.Add(reference, LocalCacheSymbolReferencePackage.CreatePackageNotFound());
		}
		if (!TryGetPackagesFromCache(diagnostics, out var matchingFiles))
		{
			return dictionary;
		}
		IEnumerable<SymbolReferenceSpecification> enumerable = references.Where((SymbolReferenceSpecification x) => x.IsValid);
		foreach (string item in matchingFiles)
		{
			NavAppManifest navAppManifest = ReadNavAppManifest(item, diagnostics);
			if (navAppManifest == null)
			{
				continue;
			}
			foreach (SymbolReferenceSpecification item2 in enumerable)
			{
				if (item2.IsSatisfiedBy(navAppManifest.AppPublisher, navAppManifest.AppName, navAppManifest.AppVersion))
				{
					LocalCacheSymbolReferencePackage localCacheSymbolReferencePackage = dictionary[item2];
					if (navAppManifest.AppVersion > localCacheSymbolReferencePackage.Version)
					{
						dictionary[item2] = LocalCacheSymbolReferencePackage.CreatePackageFound(navAppManifest, item);
					}
				}
			}
		}
		foreach (KeyValuePair<SymbolReferenceSpecification, LocalCacheSymbolReferencePackage> item3 in dictionary)
		{
			VerifyPackageFound(item3.Key, item3.Value.Manifest, item3.Value.FilePath, diagnostics);
		}
		return dictionary;
	}

	private bool TryGetPackagesFromCache(IList<Diagnostic> diagnostics, out IEnumerable<string> matchingFiles)
	{
		matchingFiles = null;
		if (string.IsNullOrEmpty(cachePath))
		{
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1021));
			return false;
		}
		try
		{
			if (Directory.Exists(cachePath))
			{
				matchingFiles = Directory.EnumerateFiles(cachePath, "*.app");
				return true;
			}
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1045, cachePath));
			return false;
		}
		catch (Exception ex)
		{
			GlobalLogger.LogVerbose("An exception happened while reading the package cache: '{0}'. {1}", cachePath, ex.Message);
		}
		diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1023, cachePath));
		return false;
	}

	private void VerifyPackageFound(SymbolReferenceSpecification reference, NavAppManifest manifest, string filePath, IList<Diagnostic> diagnostics)
	{
		if (filePath == null)
		{
			string dependencyPartIsMissing = CodeAnalysisResources.DependencyPartIsMissing;
			string text = ((reference.Version == null) ? dependencyPartIsMissing : reference.Version.ToString());
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1022, reference.Publisher ?? dependencyPartIsMissing, reference.Name ?? dependencyPartIsMissing, text, cachePath));
		}
		else
		{
			reference.ValidateReferenceModuleSpecification(manifest.AppId, diagnostics);
		}
	}

	private static NavAppManifest ReadNavAppManifest(string filePath, IList<Diagnostic> diagnostics)
	{
		try
		{
			using FileStream stream = File.OpenRead(filePath);
			using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream);
			return navAppPackageReader.ReadNavAppManifest();
		}
		catch (Exception ex)
		{
			GlobalLogger.LogVerbose("An exception happened while reading the manifest from the package '{0}'. {1}", filePath, ex.Message);
			diagnostics.Add(Diagnostic.Create(MessageProvider.Instance, 1023, filePath));
			return null;
		}
	}
}
