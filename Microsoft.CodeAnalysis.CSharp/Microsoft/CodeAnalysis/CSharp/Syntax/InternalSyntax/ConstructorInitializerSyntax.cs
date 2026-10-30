using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConstructorInitializerSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken colonToken;

	internal readonly SyntaxToken thisOrBaseKeyword;

	internal readonly ArgumentListSyntax argumentList;

	public SyntaxToken ColonToken => colonToken;

	public SyntaxToken ThisOrBaseKeyword => thisOrBaseKeyword;

	public ArgumentListSyntax ArgumentList => argumentList;

	internal ConstructorInitializerSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(thisOrBaseKeyword);
		this.thisOrBaseKeyword = thisOrBaseKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ConstructorInitializerSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(thisOrBaseKeyword);
		this.thisOrBaseKeyword = thisOrBaseKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ConstructorInitializerSyntax(SyntaxKind kind, SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(thisOrBaseKeyword);
		this.thisOrBaseKeyword = thisOrBaseKeyword;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)colonToken, 
			1 => thisOrBaseKeyword, 
			2 => argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstructorInitializer(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstructorInitializer(this);
	}

	public ConstructorInitializerSyntax Update(SyntaxToken colonToken, SyntaxToken thisOrBaseKeyword, ArgumentListSyntax argumentList)
	{
		if (colonToken != ColonToken || thisOrBaseKeyword != ThisOrBaseKeyword || argumentList != ArgumentList)
		{
			ConstructorInitializerSyntax constructorInitializerSyntax = SyntaxFactory.ConstructorInitializer(Kind, colonToken, thisOrBaseKeyword, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				constructorInitializerSyntax = constructorInitializerSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				constructorInitializerSyntax = constructorInitializerSyntax.WithAnnotationsGreen(annotations);
			}
			return constructorInitializerSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ConstructorInitializerSyntax(Kind, colonToken, thisOrBaseKeyword, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ConstructorInitializerSyntax(Kind, colonToken, thisOrBaseKeyword, argumentList, GetDiagnostics(), annotations);
	}

	internal ConstructorInitializerSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			colonToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			thisOrBaseKeyword = syntaxToken2;
		}
		ArgumentListSyntax argumentListSyntax = (ArgumentListSyntax)reader.ReadValue();
		if (argumentListSyntax != null)
		{
			AdjustFlagsAndWidth(argumentListSyntax);
			argumentList = argumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(colonToken);
		writer.WriteValue(thisOrBaseKeyword);
		writer.WriteValue(argumentList);
	}

	static ConstructorInitializerSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ConstructorInitializerSyntax), (ObjectReader r) => new ConstructorInitializerSyntax(r));
	}
}
