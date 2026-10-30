using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal delegate DebugSourceDocument DebugDocumentProvider(string path, string basePath);
