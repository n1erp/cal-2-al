using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal sealed class ManagedResource
{
	private readonly Func<Stream> _streamProvider;

	private readonly IFileReference _fileReference;

	private readonly uint _offset;

	private readonly string _name;

	private readonly bool _isPublic;

	public IFileReference ExternalFile => _fileReference;

	public uint Offset => _offset;

	public IEnumerable<ICustomAttribute> Attributes => SpecializedCollections.EmptyEnumerable<ICustomAttribute>();

	public bool IsPublic => _isPublic;

	public string Name => _name;

	internal ManagedResource(string name, bool isPublic, Func<Stream> streamProvider, IFileReference fileReference, uint offset)
	{
		_streamProvider = streamProvider;
		_name = name;
		_fileReference = fileReference;
		_offset = offset;
		_isPublic = isPublic;
	}

	public void WriteData(BlobBuilder resourceWriter)
	{
		if (_fileReference != null)
		{
			return;
		}
		try
		{
			using Stream stream = _streamProvider();
			if (stream == null)
			{
				throw new InvalidOperationException(CodeAnalysisResources.ResourceStreamProviderShouldReturnNonNullStream);
			}
			int num = (int)(stream.Length - stream.Position);
			resourceWriter.WriteInt32(num);
			int num2 = resourceWriter.TryWriteBytes(stream, num);
			if (num2 != num)
			{
				throw new EndOfStreamException(string.Format(CultureInfo.CurrentUICulture, CodeAnalysisResources.ResourceStreamEndedUnexpectedly, num2, num));
			}
			resourceWriter.Align(8);
		}
		catch (Exception inner)
		{
			throw new ResourceException(_name, inner);
		}
	}
}
