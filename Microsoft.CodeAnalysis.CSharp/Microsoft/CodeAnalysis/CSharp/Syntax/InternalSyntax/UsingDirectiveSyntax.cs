using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UsingDirectiveSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken usingKeyword;

	internal readonly SyntaxToken staticKeyword;

	internal readonly NameEqualsSyntax alias;

	internal readonly NameSyntax name;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken UsingKeyword => usingKeyword;

	public SyntaxToken StaticKeyword => staticKeyword;

	public NameEqualsSyntax Alias => alias;

	public NameSyntax Name => name;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 5;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal UsingDirectiveSyntax(SyntaxKind kind, SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(usingKeyword);
		this.usingKeyword = usingKeyword;
		if (staticKeyword != null)
		{
			AdjustFlagsAndWidth(staticKeyword);
			this.staticKeyword = staticKeyword;
		}
		if (alias != null)
		{
			AdjustFlagsAndWidth(alias);
			this.alias = alias;
		}
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)usingKeyword, 
			1 => staticKeyword, 
			2 => alias, 
			3 => name, 
			4 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUsingDirective(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingDirective(this);
	}

	public UsingDirectiveSyntax Update(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		if (usingKeyword != UsingKeyword || staticKeyword != StaticKeyword || alias != Alias || name != Name || semicolonToken != SemicolonToken)
		{
			UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(usingKeyword, staticKeyword, alias, name, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				usingDirectiveSyntax = usingDirectiveSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				usingDirectiveSyntax = usingDirectiveSyntax.WithAnnotationsGreen(annotations);
			}
			return usingDirectiveSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new UsingDirectiveSyntax(Kind, usingKeyword, staticKeyword, alias, name, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new UsingDirectiveSyntax(Kind, usingKeyword, staticKeyword, alias, name, semicolonToken, GetDiagnostics(), annotations);
	}

	internal UsingDirectiveSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 5;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			usingKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			staticKeyword = syntaxToken2;
		}
		NameEqualsSyntax nameEqualsSyntax = (NameEqualsSyntax)reader.ReadValue();
		if (nameEqualsSyntax != null)
		{
			AdjustFlagsAndWidth(nameEqualsSyntax);
			alias = nameEqualsSyntax;
		}
		NameSyntax nameSyntax = (NameSyntax)reader.ReadValue();
		if (nameSyntax != null)
		{
			AdjustFlagsAndWidth(nameSyntax);
			name = nameSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			semicolonToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(usingKeyword);
		writer.WriteValue(staticKeyword);
		writer.WriteValue(alias);
		writer.WriteValue(name);
		writer.WriteValue(semicolonToken);
	}

	static UsingDirectiveSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(UsingDirectiveSyntax), (ObjectReader r) => new UsingDirectiveSyntax(r));
	}
}
