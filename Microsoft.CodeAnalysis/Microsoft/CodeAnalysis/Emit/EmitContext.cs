using System;

namespace Microsoft.CodeAnalysis.Emit;

internal struct EmitContext
{
	[Flags]
	private enum Flags
	{
		None = 0,
		MetadataOnly = 1,
		IncludePrivateMembers = 2
	}

	public readonly CommonPEModuleBuilder Module;

	public readonly SyntaxNode SyntaxNodeOpt;

	public readonly DiagnosticBag Diagnostics;

	private readonly Flags _flags;

	public bool IncludePrivateMembers => (_flags & Flags.IncludePrivateMembers) != 0;

	public bool MetadataOnly => (_flags & Flags.MetadataOnly) != 0;

	public bool IsRefAssembly
	{
		get
		{
			if (MetadataOnly)
			{
				return !IncludePrivateMembers;
			}
			return false;
		}
	}

	public EmitContext(CommonPEModuleBuilder module, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool metadataOnly, bool includePrivateMembers)
	{
		Module = module;
		SyntaxNodeOpt = syntaxNodeOpt;
		Diagnostics = diagnostics;
		Flags flags = Flags.None;
		if (metadataOnly)
		{
			flags |= Flags.MetadataOnly;
		}
		if (includePrivateMembers)
		{
			flags |= Flags.IncludePrivateMembers;
		}
		_flags = flags;
	}
}
