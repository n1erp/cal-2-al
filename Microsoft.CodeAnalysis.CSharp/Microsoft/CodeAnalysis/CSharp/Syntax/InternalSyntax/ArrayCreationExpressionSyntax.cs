using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArrayCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly ArrayTypeSyntax type;

	internal readonly InitializerExpressionSyntax initializer;

	public SyntaxToken NewKeyword => newKeyword;

	public ArrayTypeSyntax Type => type;

	public InitializerExpressionSyntax Initializer => initializer;

	internal ArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
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
			2 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArrayCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrayCreationExpression(this);
	}

	public ArrayCreationExpressionSyntax Update(SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || type != Type || initializer != Initializer)
		{
			ArrayCreationExpressionSyntax arrayCreationExpressionSyntax = SyntaxFactory.ArrayCreationExpression(newKeyword, type, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				arrayCreationExpressionSyntax = arrayCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				arrayCreationExpressionSyntax = arrayCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return arrayCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ArrayCreationExpressionSyntax(Kind, newKeyword, type, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ArrayCreationExpressionSyntax(Kind, newKeyword, type, initializer, GetDiagnostics(), annotations);
	}

	internal ArrayCreationExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			newKeyword = syntaxToken;
		}
		ArrayTypeSyntax arrayTypeSyntax = (ArrayTypeSyntax)reader.ReadValue();
		if (arrayTypeSyntax != null)
		{
			AdjustFlagsAndWidth(arrayTypeSyntax);
			type = arrayTypeSyntax;
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
		writer.WriteValue(initializer);
	}

	static ArrayCreationExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ArrayCreationExpressionSyntax), (ObjectReader r) => new ArrayCreationExpressionSyntax(r));
	}
}
