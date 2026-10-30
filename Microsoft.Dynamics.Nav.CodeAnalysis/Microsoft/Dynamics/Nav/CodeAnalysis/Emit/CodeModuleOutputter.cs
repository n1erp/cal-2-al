using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Intended spelling")]
public abstract class CodeModuleOutputter : ModuleOutputter
{
	private readonly object mutex = new object();

	public EmitOptions EmitOptions { get; set; }

	protected CodeModuleOutputter(EmitOptions emitOptions = null)
	{
		EmitOptions = emitOptions ?? EmitOptions.Default;
	}

	public sealed override void AddApplicationObject(IApplicationObjectTypeSymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode)
	{
		(byte[], string) codeAndMetadata = GetCodeAndMetadata(code, metadata);
		if (EmitOptions.ConcurrentEmit)
		{
			lock (mutex)
			{
				AddApplicationObject(symbol, codeAndMetadata.Item1, codeAndMetadata.Item2, debugCode);
				return;
			}
		}
		AddApplicationObject(symbol, codeAndMetadata.Item1, codeAndMetadata.Item2, debugCode);
	}

	public sealed override void AddProfileObject(ISymbol symbol, CompilationUnitSyntax code, Stream metadata, string debugCode)
	{
		(byte[], string) codeAndMetadata = GetCodeAndMetadata(code, metadata);
		if (EmitOptions.ConcurrentEmit)
		{
			lock (mutex)
			{
				AddProfileObject(symbol, codeAndMetadata.Item1, codeAndMetadata.Item2, debugCode);
				return;
			}
		}
		AddProfileObject(symbol, codeAndMetadata.Item1, codeAndMetadata.Item2, debugCode);
	}

	public abstract void AddApplicationObject(IApplicationObjectTypeSymbol symbol, byte[] code, string metadata, string debugCode);

	public abstract void AddProfileObject(ISymbol symbol, byte[] code, string metadata, string debugCode);

	private (byte[] Code, string MetadataString) GetCodeAndMetadata(CompilationUnitSyntax compilationUnit, Stream metadata)
	{
		string item = ((metadata == null) ? null : new StreamReader(metadata).ReadToEnd());
		if (compilationUnit == null)
		{
			return (Code: Array.Empty<byte>(), MetadataString: item);
		}
		if (EmitOptions.EmitCodeAsAst)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				compilationUnit.SerializeTo(memoryStream);
				return (Code: memoryStream.ToArray(), MetadataString: item);
			}
		}
		return (Code: Encoding.UTF8.GetBytes(compilationUnit.ToFullString()), MetadataString: item);
	}
}
