using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NamespaceDeclarationSyntax : MemberDeclarationSyntax
{
	internal readonly SyntaxToken namespaceKeyword;

	internal readonly NameSyntax name;

	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode externs;

	internal readonly GreenNode usings;

	internal readonly GreenNode members;

	internal readonly SyntaxToken closeBraceToken;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken NamespaceKeyword => namespaceKeyword;

	public NameSyntax Name => name;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> Externs => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax>(externs);

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> Usings => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax>(usings);

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>(members);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal NamespaceDeclarationSyntax(SyntaxKind kind, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode externs, GreenNode usings, GreenNode members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal NamespaceDeclarationSyntax(SyntaxKind kind, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode externs, GreenNode usings, GreenNode members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 8;
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal NamespaceDeclarationSyntax(SyntaxKind kind, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode externs, GreenNode usings, GreenNode members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => namespaceKeyword, 
			1 => name, 
			2 => openBraceToken, 
			3 => externs, 
			4 => usings, 
			5 => members, 
			6 => closeBraceToken, 
			7 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNamespaceDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNamespaceDeclaration(this);
	}

	public NamespaceDeclarationSyntax Update(SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (namespaceKeyword != NamespaceKeyword || name != Name || openBraceToken != OpenBraceToken || externs != Externs || usings != Usings || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = SyntaxFactory.NamespaceDeclaration(namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				namespaceDeclarationSyntax = namespaceDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				namespaceDeclarationSyntax = namespaceDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return namespaceDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new NamespaceDeclarationSyntax(Kind, namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new NamespaceDeclarationSyntax(Kind, namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken, GetDiagnostics(), annotations);
	}

	internal NamespaceDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 8;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			namespaceKeyword = syntaxToken;
		}
		NameSyntax nameSyntax = (NameSyntax)reader.ReadValue();
		if (nameSyntax != null)
		{
			AdjustFlagsAndWidth(nameSyntax);
			name = nameSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openBraceToken = syntaxToken2;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			externs = greenNode;
		}
		GreenNode greenNode2 = (GreenNode)reader.ReadValue();
		if (greenNode2 != null)
		{
			AdjustFlagsAndWidth(greenNode2);
			usings = greenNode2;
		}
		GreenNode greenNode3 = (GreenNode)reader.ReadValue();
		if (greenNode3 != null)
		{
			AdjustFlagsAndWidth(greenNode3);
			members = greenNode3;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeBraceToken = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			semicolonToken = syntaxToken4;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(namespaceKeyword);
		writer.WriteValue(name);
		writer.WriteValue(openBraceToken);
		writer.WriteValue(externs);
		writer.WriteValue(usings);
		writer.WriteValue(members);
		writer.WriteValue(closeBraceToken);
		writer.WriteValue(semicolonToken);
	}

	static NamespaceDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(NamespaceDeclarationSyntax), (ObjectReader r) => new NamespaceDeclarationSyntax(r));
	}
}
