using System;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpScriptCompilationInfo : ScriptCompilationInfo
{
	public new CSharpCompilation PreviousScriptCompilation { get; }

	internal override Compilation CommonPreviousScriptCompilation => PreviousScriptCompilation;

	internal CSharpScriptCompilationInfo(CSharpCompilation previousCompilationOpt, Type returnType, Type globalsType)
		: base(returnType, globalsType)
	{
		PreviousScriptCompilation = previousCompilationOpt;
	}

	public CSharpScriptCompilationInfo WithPreviousScriptCompilation(CSharpCompilation compilation)
	{
		if (compilation != PreviousScriptCompilation)
		{
			return new CSharpScriptCompilationInfo(compilation, ReturnTypeOpt, GlobalsType);
		}
		return this;
	}

	internal override ScriptCompilationInfo CommonWithPreviousScriptCompilation(Compilation compilation)
	{
		return WithPreviousScriptCompilation((CSharpCompilation)compilation);
	}
}
