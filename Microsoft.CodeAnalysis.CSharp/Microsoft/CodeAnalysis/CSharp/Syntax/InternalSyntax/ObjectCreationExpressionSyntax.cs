using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ObjectCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly TypeSyntax type;

	internal readonly ArgumentListSyntax argumentList;

	internal readonly InitializerExpressionSyntax initializer;

	public SyntaxToken NewKeyword => newKeyword;

	public TypeSyntax Type => type;

	public ArgumentListSyntax ArgumentList => argumentList;

	public InitializerExpressionSyntax Initializer => initializer;

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)newKeyword, 
			1 => type, 
			2 => argumentList, 
			3 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitObjectCreationExpression(this);
	}

	public ObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || type != Type || argumentList != ArgumentList || initializer != Initializer)
		{
			ObjectCreationExpressionSyntax objectCreationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(newKeyword, type, argumentList, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return objectCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ObjectCreationExpressionSyntax(Kind, newKeyword, type, argumentList, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ObjectCreationExpressionSyntax(Kind, newKeyword, type, argumentList, initializer, GetDiagnostics(), annotations);
	}

	internal ObjectCreationExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			newKeyword = syntaxToken;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
		ArgumentListSyntax argumentListSyntax = (ArgumentListSyntax)reader.ReadValue();
		if (argumentListSyntax != null)
		{
			AdjustFlagsAndWidth(argumentListSyntax);
			argumentList = argumentListSyntax;
		}
		InitializerExpressionSyntax initializerExpressionSyntax = (InitializerExpressionSyntax)reader.ReadValue();
		if (initializerExpressionSyntax != null)
		{
			AdjustFlagsAndWidth(initializerExpressionSyntax);
			initializer = initializerExpressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(newKeyword);
		writer.WriteValue(type);
		writer.WriteValue(argumentList);
		writer.WriteValue(initializer);
	}

	static ObjectCreationExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ObjectCreationExpressionSyntax), (ObjectReader r) => new ObjectCreationExpressionSyntax(r));
	}
}
