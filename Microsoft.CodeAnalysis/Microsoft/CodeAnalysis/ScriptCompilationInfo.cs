using System;

namespace Microsoft.CodeAnalysis;

public abstract class ScriptCompilationInfo
{
	internal Type ReturnTypeOpt { get; }

	public Type ReturnType => ReturnTypeOpt ?? typeof(object);

	public Type GlobalsType { get; }

	public Compilation PreviousScriptCompilation => CommonPreviousScriptCompilation;

	internal abstract Compilation CommonPreviousScriptCompilation { get; }

	internal ScriptCompilationInfo(Type returnType, Type globalsType)
	{
		ReturnTypeOpt = returnType;
		GlobalsType = globalsType;
	}

	public ScriptCompilationInfo WithPreviousScriptCompilation(Compilation compilation)
	{
		return CommonWithPreviousScriptCompilation(compilation);
	}

	internal abstract ScriptCompilationInfo CommonWithPreviousScriptCompilation(Compilation compilation);
}
