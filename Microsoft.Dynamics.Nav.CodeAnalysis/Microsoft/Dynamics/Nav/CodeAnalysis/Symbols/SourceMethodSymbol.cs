using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceMethodSymbol : SourceMethodOrTriggerSymbol
{
	private readonly bool isOverload;

	private int lazyId;

	private string lazyMetadataName;

	public override string Name { get; }

	public override string MetadataName
	{
		get
		{
			if (lazyMetadataName != null)
			{
				return lazyMetadataName;
			}
			string text = GetSyntax().Name.Identifier.Text.MangleIdentifierName();
			if (isOverload)
			{
				text += this.GetOverloadedMethodMetadataNameSuffix();
			}
			Interlocked.CompareExchange(ref lazyMetadataName, text, null);
			return lazyMetadataName;
		}
	}

	public override MethodKind MethodKind
	{
		get
		{
			if (GetSyntax().Body != null)
			{
				return MethodKind.Method;
			}
			return MethodKind.DeclareMethod;
		}
	}

	public override int Id
	{
		get
		{
			if (lazyId == 0)
			{
				Interlocked.CompareExchange(ref lazyId, CalculateMethodId(isOverload), 0);
			}
			return lazyId;
		}
	}

	public override bool IsLocal => GetSyntax().LocalKeyword != default(SyntaxToken);

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (!IsLocal)
			{
				return Accessibility.Public;
			}
			return Accessibility.Private;
		}
	}

	public SourceMethodSymbol(ObjectTypeSymbol containingSymbol, MethodDeclarationSyntax methodDeclarationSyntax, bool isOverload)
		: base(containingSymbol, methodDeclarationSyntax)
	{
		Name = methodDeclarationSyntax.Name.Unquoted();
		this.isOverload = isOverload;
	}

	internal new MethodDeclarationSyntax GetSyntax()
	{
		return (MethodDeclarationSyntax)base.GetSyntax();
	}
}
