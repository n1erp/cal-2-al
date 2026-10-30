using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AliasQualifiedNameSyntax : NameSyntax
{
	internal readonly IdentifierNameSyntax alias;

	internal readonly SyntaxToken colonColonToken;

	internal readonly SimpleNameSyntax name;

	public IdentifierNameSyntax Alias => alias;

	public SyntaxToken ColonColonToken => colonColonToken;

	public SimpleNameSyntax Name => name;

	internal AliasQualifiedNameSyntax(SyntaxKind kind, IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(alias);
		this.alias = alias;
		AdjustFlagsAndWidth(colonColonToken);
		this.colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal AliasQualifiedNameSyntax(SyntaxKind kind, IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(alias);
		this.alias = alias;
		AdjustFlagsAndWidth(colonColonToken);
		this.colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal AliasQualifiedNameSyntax(SyntaxKind kind, IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(alias);
		this.alias = alias;
		AdjustFlagsAndWidth(colonColonToken);
		this.colonColonToken = colonColonToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)alias, 
			1 => colonColonToken, 
			2 => name, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAliasQualifiedName(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAliasQualifiedName(this);
	}

	public AliasQualifiedNameSyntax Update(IdentifierNameSyntax alias, SyntaxToken colonColonToken, SimpleNameSyntax name)
	{
		if (alias != Alias || colonColonToken != ColonColonToken || name != Name)
		{
			AliasQualifiedNameSyntax aliasQualifiedNameSyntax = SyntaxFactory.AliasQualifiedName(alias, colonColonToken, name);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				aliasQualifiedNameSyntax = aliasQualifiedNameSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				aliasQualifiedNameSyntax = aliasQualifiedNameSyntax.WithAnnotationsGreen(annotations);
			}
			return aliasQualifiedNameSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AliasQualifiedNameSyntax(Kind, alias, colonColonToken, name, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AliasQualifiedNameSyntax(Kind, alias, colonColonToken, name, GetDiagnostics(), annotations);
	}

	internal AliasQualifiedNameSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)reader.ReadValue();
		if (identifierNameSyntax != null)
		{
			AdjustFlagsAndWidth(identifierNameSyntax);
			alias = identifierNameSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			colonColonToken = syntaxToken;
		}
		SimpleNameSyntax simpleNameSyntax = (SimpleNameSyntax)reader.ReadValue();
		if (simpleNameSyntax != null)
		{
			AdjustFlagsAndWidth(simpleNameSyntax);
			name = simpleNameSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(alias);
		writer.WriteValue(colonColonToken);
		writer.WriteValue(name);
	}

	static AliasQualifiedNameSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AliasQualifiedNameSyntax), (ObjectReader r) => new AliasQualifiedNameSyntax(r));
	}
}
