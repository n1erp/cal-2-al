using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.DiaSymReader;

internal sealed class SymUnmanagedWriterImpl : SymUnmanagedWriter
{
	private static object s_zeroInt32 = 0;

	private ISymUnmanagedWriter5 _symWriter;

	private readonly ComMemoryStream _pdbStream;

	private readonly List<ISymUnmanagedDocumentWriter> _documentWriters;

	private readonly string _symWriterModuleName;

	private bool _disposed;

	public override int DocumentTableCapacity
	{
		get
		{
			return _documentWriters.Capacity;
		}
		set
		{
			if (value > _documentWriters.Count)
			{
				_documentWriters.Capacity = value;
			}
		}
	}

	internal SymUnmanagedWriterImpl(ComMemoryStream pdbStream, ISymUnmanagedWriter5 symWriter, string symWriterModuleName)
	{
		_pdbStream = pdbStream;
		_symWriter = symWriter;
		_documentWriters = new List<ISymUnmanagedDocumentWriter>();
		_symWriterModuleName = symWriterModuleName;
	}

	private ISymUnmanagedWriter5 GetSymWriter()
	{
		return _symWriter ?? throw _disposed ? new ObjectDisposedException("SymUnmanagedWriterImpl") : new InvalidOperationException();
	}

	private ISymUnmanagedWriter8 GetSymWriter8()
	{
		if (!(GetSymWriter() is ISymUnmanagedWriter8 result))
		{
			throw PdbWritingException(new NotSupportedException());
		}
		return result;
	}

	private Exception PdbWritingException(Exception inner)
	{
		return new SymUnmanagedWriterException(inner, _symWriterModuleName);
	}

	public override void WriteTo(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		CloseSymWriter();
		try
		{
			_pdbStream.CopyTo(stream);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void Dispose()
	{
		DisposeImpl();
		GC.SuppressFinalize(this);
	}

	~SymUnmanagedWriterImpl()
	{
		DisposeImpl();
	}

	private void DisposeImpl()
	{
		try
		{
			CloseSymWriter();
		}
		catch
		{
		}
		_disposed = true;
	}

	private void CloseSymWriter()
	{
		ISymUnmanagedWriter5 symUnmanagedWriter = Interlocked.Exchange(ref _symWriter, null);
		if (symUnmanagedWriter == null)
		{
			return;
		}
		try
		{
			symUnmanagedWriter.Close();
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
		finally
		{
			_documentWriters.Clear();
		}
	}

	public override IEnumerable<ArraySegment<byte>> GetUnderlyingData()
	{
		GetSymWriter().Commit();
		return _pdbStream.GetChunks();
	}

	public override int DefineDocument(string name, Guid language, Guid vendor, Guid type, Guid algorithmId, byte[] checksum, byte[] source)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		int count = _documentWriters.Count;
		ISymUnmanagedDocumentWriter symUnmanagedDocumentWriter;
		try
		{
			symUnmanagedDocumentWriter = symWriter.DefineDocument(name, ref language, ref vendor, ref type);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
		_documentWriters.Add(symUnmanagedDocumentWriter);
		if (algorithmId != default(Guid) && checksum.Length != 0)
		{
			try
			{
				symUnmanagedDocumentWriter.SetCheckSum(algorithmId, (uint)checksum.Length, checksum);
			}
			catch (Exception inner2)
			{
				throw PdbWritingException(inner2);
			}
		}
		if (source != null)
		{
			try
			{
				symUnmanagedDocumentWriter.SetSource((uint)source.Length, source);
			}
			catch (Exception inner3)
			{
				throw PdbWritingException(inner3);
			}
		}
		return count;
	}

	public override void DefineSequencePoints(int documentIndex, int count, int[] offsets, int[] startLines, int[] startColumns, int[] endLines, int[] endColumns)
	{
		if (documentIndex < 0 || documentIndex >= _documentWriters.Count)
		{
			throw new ArgumentOutOfRangeException("documentIndex");
		}
		if (offsets == null)
		{
			throw new ArgumentNullException("offsets");
		}
		if (startLines == null)
		{
			throw new ArgumentNullException("startLines");
		}
		if (startColumns == null)
		{
			throw new ArgumentNullException("startColumns");
		}
		if (endLines == null)
		{
			throw new ArgumentNullException("endLines");
		}
		if (endColumns == null)
		{
			throw new ArgumentNullException("endColumns");
		}
		if (count < 0 || count > startLines.Length || count > startColumns.Length || count > endLines.Length || count > endColumns.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.DefineSequencePoints(_documentWriters[documentIndex], count, offsets, startLines, startColumns, endLines, endColumns);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void OpenMethod(int methodToken)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.OpenMethod((uint)methodToken);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void CloseMethod()
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.CloseMethod();
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void OpenScope(int startOffset)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.OpenScope(startOffset);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void CloseScope(int endOffset)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.CloseScope(endOffset);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void DefineLocalVariable(int index, string name, int attributes, int localSignatureToken)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.DefineLocalVariable2(name, attributes, localSignatureToken, 1u, index, 0u, 0u, 0u, 0u);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override bool DefineLocalConstant(string name, object value, int constantSignatureToken)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		if (value != null)
		{
			if (value is string text)
			{
				string value2 = text;
				return DefineLocalStringConstant(symWriter, name, value2, constantSignatureToken);
			}
			if (value is DateTime dateTime)
			{
				DateTime date = dateTime;
				try
				{
					symWriter.DefineConstant2(name, new VariantStructure(date), constantSignatureToken);
				}
				catch (Exception inner)
				{
					throw PdbWritingException(inner);
				}
				return true;
			}
		}
		try
		{
			DefineLocalConstantImpl(symWriter, name, value ?? s_zeroInt32, constantSignatureToken);
		}
		catch (Exception inner2)
		{
			throw PdbWritingException(inner2);
		}
		return true;
	}

	private unsafe void DefineLocalConstantImpl(ISymUnmanagedWriter5 symWriter, string name, object value, int constantSignatureToken)
	{
		VariantStructure value2 = default;
		Marshal.GetNativeVariantForObject(value, new IntPtr(&value2));
		symWriter.DefineConstant2(name, value2, constantSignatureToken);
	}

	private bool DefineLocalStringConstant(ISymUnmanagedWriter5 symWriter, string name, string value, int constantSignatureToken)
	{
		int num;
		if (!IsValidUnicodeString(value))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			num = bytes.Length;
			value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}
		else
		{
			num = Encoding.UTF8.GetByteCount(value);
		}
		num++;
		if (num > 2032)
		{
			return false;
		}
		try
		{
			DefineLocalConstantImpl(symWriter, name, value, constantSignatureToken);
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
		return true;
	}

	private static bool IsValidUnicodeString(string str)
	{
		int num = 0;
		while (num < str.Length)
		{
			char c = str[num++];
			if (char.IsHighSurrogate(c))
			{
				if (num >= str.Length || !char.IsLowSurrogate(str[num]))
				{
					return false;
				}
				num++;
			}
			else if (char.IsLowSurrogate(c))
			{
				return false;
			}
		}
		return true;
	}

	public override void UsingNamespace(string importString)
	{
		if (importString == null)
		{
			throw new ArgumentNullException("importString");
		}
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.UsingNamespace(importString);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void SetAsyncInfo(int moveNextMethodToken, int kickoffMethodToken, int catchHandlerOffset, int[] yieldOffsets, int[] resumeOffsets)
	{
		if (yieldOffsets == null)
		{
			throw new ArgumentNullException("yieldOffsets");
		}
		if (resumeOffsets == null)
		{
			throw new ArgumentNullException("resumeOffsets");
		}
		if (yieldOffsets.Length != resumeOffsets.Length)
		{
			throw new ArgumentOutOfRangeException("yieldOffsets");
		}
		if (!(GetSymWriter() is ISymUnmanagedAsyncMethodPropertiesWriter symUnmanagedAsyncMethodPropertiesWriter))
		{
			return;
		}
		int num = yieldOffsets.Length;
		if (num > 0)
		{
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = moveNextMethodToken;
			}
			try
			{
				symUnmanagedAsyncMethodPropertiesWriter.DefineAsyncStepInfo(num, yieldOffsets, resumeOffsets, array);
			}
			catch (Exception inner)
			{
				throw PdbWritingException(inner);
			}
		}
		try
		{
			if (catchHandlerOffset >= 0)
			{
				symUnmanagedAsyncMethodPropertiesWriter.DefineCatchHandlerILOffset(catchHandlerOffset);
			}
			symUnmanagedAsyncMethodPropertiesWriter.DefineKickoffMethod(kickoffMethodToken);
		}
		catch (Exception inner2)
		{
			throw PdbWritingException(inner2);
		}
	}

	public unsafe override void DefineCustomMetadata(byte[] metadata)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		if (metadata.Length == 0)
		{
			return;
		}
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			fixed (byte* data = metadata)
			{
				symWriter.SetSymAttribute(0u, "MD2", metadata.Length, data);
			}
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void SetEntryPoint(int entryMethodToken)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.SetUserEntryPoint(entryMethodToken);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void UpdateSignature(Guid guid, uint stamp, int age)
	{
		ISymUnmanagedWriter8 symWriter = GetSymWriter8();
		try
		{
			symWriter.UpdateSignature(guid, stamp, age);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public unsafe override void SetSourceServerData(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			return;
		}
		ISymUnmanagedWriter8 symWriter = GetSymWriter8();
		try
		{
			fixed (byte* data2 = data)
			{
				symWriter.SetSourceServerData(data2, data.Length);
			}
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public unsafe override void SetSourceLinkData(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			return;
		}
		ISymUnmanagedWriter8 symWriter = GetSymWriter8();
		try
		{
			fixed (byte* data2 = data)
			{
				symWriter.SetSourceLinkData(data2, data.Length);
			}
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void OpenTokensToSourceSpansMap()
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.OpenMapTokensToSourceSpans();
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void MapTokenToSourceSpan(int token, int documentIndex, int startLine, int startColumn, int endLine, int endColumn)
	{
		if (documentIndex < 0 || documentIndex >= _documentWriters.Count)
		{
			throw new ArgumentOutOfRangeException("documentIndex");
		}
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.MapTokenToSourceSpan(token, _documentWriters[documentIndex], startLine, startColumn, endLine, endColumn);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public override void CloseTokensToSourceSpansMap()
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		try
		{
			symWriter.CloseMapTokensToSourceSpans();
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
	}

	public unsafe override void GetSignature(out Guid guid, out uint stamp, out int age)
	{
		ISymUnmanagedWriter5 symWriter = GetSymWriter();
		ImageDebugDirectory debugDirectory = default;
		uint dataCountPtr;
		try
		{
			symWriter.GetDebugInfo(ref debugDirectory, 0u, out dataCountPtr, null);
		}
		catch (Exception inner)
		{
			throw PdbWritingException(inner);
		}
		byte[] array = new byte[dataCountPtr];
		fixed (byte* data = array)
		{
			try
			{
				symWriter.GetDebugInfo(ref debugDirectory, dataCountPtr, out dataCountPtr, data);
			}
			catch (Exception inner2)
			{
				throw PdbWritingException(inner2);
			}
		}
		byte[] array2 = new byte[16];
		Buffer.BlockCopy(array, 4, array2, 0, array2.Length);
		guid = new Guid(array2);
		((IPdbWriter)symWriter).GetSignatureAge(out stamp, out age);
	}
}
