using System;
using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public class XmlFileResolver : XmlReferenceResolver
{
	private readonly string _baseDirectory;

	public static XmlFileResolver Default { get; } = new XmlFileResolver(null);

	public string BaseDirectory => _baseDirectory;

	public XmlFileResolver(string baseDirectory)
	{
		if (baseDirectory != null && PathUtilities.GetPathKind(baseDirectory) != PathKind.Absolute)
		{
			throw new ArgumentException(CodeAnalysisResources.AbsolutePathExpected, "baseDirectory");
		}
		_baseDirectory = baseDirectory;
	}

	public override string ResolveReference(string path, string baseFilePath)
	{
		if (baseFilePath != null)
		{
			string text = FileUtilities.ResolveRelativePath(path, baseFilePath, _baseDirectory);
			if (FileExists(text))
			{
				return FileUtilities.TryNormalizeAbsolutePath(text);
			}
		}
		if (_baseDirectory != null)
		{
			string text = FileUtilities.ResolveRelativePath(path, _baseDirectory);
			if (FileExists(text))
			{
				return FileUtilities.TryNormalizeAbsolutePath(text);
			}
		}
		return null;
	}

	public override Stream OpenRead(string resolvedPath)
	{
		CompilerPathUtilities.RequireAbsolutePath(resolvedPath, "resolvedPath");
		return FileUtilities.OpenRead(resolvedPath);
	}

	protected virtual bool FileExists(string resolvedPath)
	{
		return File.Exists(resolvedPath);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || (object)GetType() != obj.GetType())
		{
			return false;
		}
		XmlFileResolver xmlFileResolver = (XmlFileResolver)obj;
		return string.Equals(_baseDirectory, xmlFileResolver._baseDirectory, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		if (_baseDirectory == null)
		{
			return 0;
		}
		return StringComparer.Ordinal.GetHashCode(_baseDirectory);
	}
}
