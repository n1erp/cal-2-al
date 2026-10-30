using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.DiaSymReader;

internal abstract class SymUnmanagedWriter : IDisposable
{
	public abstract int DocumentTableCapacity { get; set; }

	public abstract void Dispose();

	public abstract IEnumerable<ArraySegment<byte>> GetUnderlyingData();

	public abstract void WriteTo(Stream stream);

	public abstract int DefineDocument(string name, Guid language, Guid vendor, Guid type, Guid algorithmId, byte[] checksum, byte[] source);

	public abstract void DefineSequencePoints(int documentIndex, int count, int[] offsets, int[] startLines, int[] startColumns, int[] endLines, int[] endColumns);

	public abstract void OpenMethod(int methodToken);

	public abstract void CloseMethod();

	public abstract void OpenScope(int startOffset);

	public abstract void CloseScope(int endOffset);

	public abstract void DefineLocalVariable(int index, string name, int attributes, int localSignatureToken);

	public abstract bool DefineLocalConstant(string name, object value, int constantSignatureToken);

	public abstract void UsingNamespace(string importString);

	public abstract void SetAsyncInfo(int moveNextMethodToken, int kickoffMethodToken, int catchHandlerOffset, int[] yieldOffsets, int[] resumeOffsets);

	public abstract void DefineCustomMetadata(byte[] metadata);

	public abstract void SetEntryPoint(int entryMethodToken);

	public abstract void UpdateSignature(Guid guid, uint stamp, int age);

	public abstract void GetSignature(out Guid guid, out uint stamp, out int age);

	public abstract void SetSourceServerData(byte[] data);

	public abstract void SetSourceLinkData(byte[] data);

	public abstract void OpenTokensToSourceSpansMap();

	public abstract void MapTokenToSourceSpan(int token, int documentIndex, int startLine, int startColumn, int endLine, int endColumn);

	public abstract void CloseTokensToSourceSpansMap();
}
