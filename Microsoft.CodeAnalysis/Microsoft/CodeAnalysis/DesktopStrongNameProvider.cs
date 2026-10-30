using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Interop;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public class DesktopStrongNameProvider : StrongNameProvider
{
	internal sealed class TempFileStream : Stream
	{
		private readonly string _path;

		private readonly Stream _stream;

		public string Path => _path;

		public override bool CanRead => _stream.CanRead;

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanWrite => _stream.CanWrite;

		public override long Length => _stream.Length;

		public override long Position
		{
			get
			{
				return _stream.Position;
			}
			set
			{
				_stream.Position = value;
			}
		}

		public TempFileStream(string path, Stream stream)
		{
			_path = path;
			_stream = stream;
		}

		public void DisposeUnderlyingStream()
		{
			_stream.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_stream.Dispose();
			try
			{
				File.Delete(_path);
			}
			catch
			{
			}
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}
	}

	internal sealed class ClrStrongNameMissingException : Exception
	{
	}

	private readonly ImmutableArray<string> _keyFileSearchPaths;

	private readonly string _tempPath;

	internal Func<IClrStrongName> TestStrongNameInterfaceFactory;

	internal StrongNameFileSystem FileSystem { get; }

	internal override SigningCapability Capability => SigningCapability.SignsStream;

	public DesktopStrongNameProvider(ImmutableArray<string> keyFileSearchPaths)
		: this(keyFileSearchPaths, null, null)
	{
	}

	public DesktopStrongNameProvider(ImmutableArray<string> keyFileSearchPaths = default(ImmutableArray<string>), string tempPath = null)
		: this(keyFileSearchPaths, tempPath, null)
	{
	}

	internal DesktopStrongNameProvider(ImmutableArray<string> keyFileSearchPaths, string tempPath, StrongNameFileSystem strongNameFileSystem)
	{
		if (!keyFileSearchPaths.IsDefault && keyFileSearchPaths.Any((string path) => !PathUtilities.IsAbsolute(path)))
		{
			throw new ArgumentException(CodeAnalysisResources.AbsolutePathExpected, "keyFileSearchPaths");
		}
		FileSystem = strongNameFileSystem ?? StrongNameFileSystem.Instance;
		_keyFileSearchPaths = keyFileSearchPaths.NullToEmpty();
		_tempPath = tempPath;
	}

	internal override Stream CreateInputStream()
	{
		Func<string, Stream> factory = (string lPath) => new TempFileStream(lPath, new FileStream(lPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
		string path = Path.Combine(_tempPath ?? Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		return FileUtilities.CreateFileStreamChecked(factory, path);
	}

	internal override StrongNameKeys CreateKeys(string keyFilePath, string keyContainerName, CommonMessageProvider messageProvider)
	{
		ImmutableArray<byte> keyPair = default;
		ImmutableArray<byte> publicKey = default;
		string keyContainerName2 = null;
		if (!string.IsNullOrEmpty(keyFilePath))
		{
			return CommonCreateKeys(FileSystem, keyFilePath, _keyFileSearchPaths, messageProvider);
		}
		if (!string.IsNullOrEmpty(keyContainerName))
		{
			try
			{
				ReadKeysFromContainer(keyContainerName, out publicKey);
				keyContainerName2 = keyContainerName;
			}
			catch (ClrStrongNameMissingException)
			{
				return new StrongNameKeys(StrongNameKeys.GetContainerError(messageProvider, keyContainerName, new CodeAnalysisResourcesLocalizableErrorArgument("AssemblySigningNotSupported")));
			}
			catch (Exception ex2)
			{
				return new StrongNameKeys(StrongNameKeys.GetContainerError(messageProvider, keyContainerName, ex2.Message));
			}
		}
		return new StrongNameKeys(keyPair, publicKey, null, keyContainerName2, keyFilePath);
	}

	internal virtual void ReadKeysFromContainer(string keyContainer, out ImmutableArray<byte> publicKey)
	{
		try
		{
			publicKey = GetPublicKey(keyContainer);
		}
		catch (ClrStrongNameMissingException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message);
		}
	}

	internal override void SignStream(StrongNameKeys keys, Stream inputStream, Stream outputStream)
	{
		TempFileStream tempFileStream = (TempFileStream)inputStream;
		string path = tempFileStream.Path;
		tempFileStream.DisposeUnderlyingStream();
		if (keys.KeyContainer != null)
		{
			Sign(path, keys.KeyContainer);
		}
		else
		{
			Sign(path, keys.KeyPair);
		}
		using FileStream fileStream = new FileStream(path, FileMode.Open);
		fileStream.CopyTo(outputStream);
	}

	internal IClrStrongName GetStrongNameInterface()
	{
		IClrStrongName clrStrongName = TestStrongNameInterfaceFactory?.Invoke();
		if (clrStrongName != null)
		{
			return clrStrongName;
		}
		try
		{
			return ClrStrongName.GetInstance();
		}
		catch (MarshalDirectiveException) when (PathUtilities.IsUnixLikePlatform)
		{
			throw new ClrStrongNameMissingException();
		}
	}

	internal ImmutableArray<byte> GetPublicKey(string keyContainer)
	{
		IClrStrongName strongNameInterface = GetStrongNameInterface();
		strongNameInterface.StrongNameGetPublicKey(keyContainer, (IntPtr)0, 0, out var ppbPublicKeyBlob, out var pcbPublicKeyBlob);
		byte[] array = new byte[pcbPublicKeyBlob];
		Marshal.Copy(ppbPublicKeyBlob, array, 0, pcbPublicKeyBlob);
		strongNameInterface.StrongNameFreeBuffer(ppbPublicKeyBlob);
		return array.AsImmutableOrNull();
	}

	private void Sign(string filePath, string keyName)
	{
		try
		{
			GetStrongNameInterface().StrongNameSignatureGeneration(filePath, keyName, IntPtr.Zero, 0, null, out var _);
		}
		catch (ClrStrongNameMissingException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	private unsafe void Sign(string filePath, ImmutableArray<byte> keyPair)
	{
		try
		{
			IClrStrongName strongNameInterface = GetStrongNameInterface();
			fixed (byte* ptr = keyPair.ToArray())
			{
				strongNameInterface.StrongNameSignatureGeneration(filePath, null, (IntPtr)ptr, keyPair.Length, null, out var _);
			}
		}
		catch (ClrStrongNameMissingException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(_keyFileSearchPaths, StringComparer.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || (object)GetType() != obj.GetType())
		{
			return false;
		}
		DesktopStrongNameProvider desktopStrongNameProvider = (DesktopStrongNameProvider)obj;
		if (FileSystem != desktopStrongNameProvider.FileSystem)
		{
			return false;
		}
		if (!_keyFileSearchPaths.SequenceEqual(desktopStrongNameProvider._keyFileSearchPaths, StringComparer.Ordinal))
		{
			return false;
		}
		return string.Equals(_tempPath, desktopStrongNameProvider._tempPath, StringComparison.Ordinal);
	}
}
