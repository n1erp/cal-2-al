using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.DiaSymReader;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("DCF7780D-BDE9-45DF-ACFE-21731A32000C")]
[SuppressUnmanagedCodeSecurity]
internal interface ISymUnmanagedWriter5
{
	ISymUnmanagedDocumentWriter DefineDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);

	void SetUserEntryPoint(int entryMethodToken);

	void OpenMethod(uint methodToken);

	void CloseMethod();

	uint OpenScope(int startOffset);

	void CloseScope(int endOffset);

	void SetScopeRange(uint scopeID, uint startOffset, uint endOffset);

	unsafe void DefineLocalVariable(string name, uint attributes, uint sig, byte* signature, uint addrKind, uint addr1, uint addr2, uint startOffset, uint endOffset);

	void DefineParameter(string name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3);

	unsafe void DefineField(uint parent, string name, uint attributes, uint sig, byte* signature, uint addrKind, uint addr1, uint addr2, uint addr3);

	unsafe void DefineGlobalVariable(string name, uint attributes, uint sig, byte* signature, uint addrKind, uint addr1, uint addr2, uint addr3);

	void Close();

	unsafe void SetSymAttribute(uint parent, string name, int length, byte* data);

	void OpenNamespace(string name);

	void CloseNamespace();

	void UsingNamespace(string fullName);

	void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, object endDoc, uint endLine, uint endColumn);

	void Initialize([MarshalAs(UnmanagedType.IUnknown)] object emitter, string filename, [MarshalAs(UnmanagedType.IUnknown)] object ptrIStream, bool fullBuild);

	unsafe void GetDebugInfo(ref ImageDebugDirectory debugDirectory, uint dataCount, out uint dataCountPtr, byte* data);

	void DefineSequencePoints(ISymUnmanagedDocumentWriter document, int count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] offsets, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] lines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] columns, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] endLines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] endColumns);

	void RemapToken(uint oldToken, uint newToken);

	void Initialize2([MarshalAs(UnmanagedType.IUnknown)] object emitter, string tempfilename, [MarshalAs(UnmanagedType.IUnknown)] object ptrIStream, bool fullBuild, string finalfilename);

	unsafe void DefineConstant(string name, object value, uint sig, byte* signature);

	void Abort();

	void DefineLocalVariable2(string name, int attributes, int localSignatureToken, uint addrKind, int index, uint addr2, uint addr3, uint startOffset, uint endOffset);

	void DefineGlobalVariable2(string name, int attributes, int sigToken, uint addrKind, uint addr1, uint addr2, uint addr3);

	void DefineConstant2([MarshalAs(UnmanagedType.LPWStr)] string name, VariantStructure value, int constantSignatureToken);

	void OpenMethod2(uint methodToken, int sectionIndex, int offsetRelativeOffset);

	void Commit();

	unsafe void GetDebugInfoWithPadding(ref ImageDebugDirectory debugDirectory, uint dataCount, out uint dataCountPtr, byte* data);

	void OpenMapTokensToSourceSpans();

	void CloseMapTokensToSourceSpans();

	void MapTokenToSourceSpan(int token, ISymUnmanagedDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn);
}
