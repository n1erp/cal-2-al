using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxToken : InternalSyntaxNode
{
	protected class InternalSyntaxTokenWithTrivia : InternalSyntaxToken
	{
		private readonly InternalSyntaxNode leadingTrivia;

		private readonly InternalSyntaxNode trailingTrivia;

		internal new InternalSyntaxList<InternalSyntaxNode> LeadingTrivia => new InternalSyntaxList<InternalSyntaxNode>(GetLeadingTrivia());

		internal new InternalSyntaxList<InternalSyntaxNode> TrailingTrivia => new InternalSyntaxList<InternalSyntaxNode>(GetTrailingTrivia());

		internal InternalSyntaxTokenWithTrivia(SyntaxKind kind, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode = LexerMode.Default)
			: base(kind, mode)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		internal InternalSyntaxTokenWithTrivia(SyntaxKind kind, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, diagnostics, annotations)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		public override InternalSyntaxNode GetLeadingTrivia()
		{
			return leadingTrivia;
		}

		public override InternalSyntaxNode GetTrailingTrivia()
		{
			return trailingTrivia;
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new InternalSyntaxTokenWithTrivia(Kind, leadingTrivia, trailingTrivia, diagnostics, GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new InternalSyntaxTokenWithTrivia(Kind, leadingTrivia, trailingTrivia, GetDiagnostics(), annotations);
		}
	}

	internal class InternalSyntaxTokenWithValue<T> : InternalSyntaxToken
	{
		protected readonly string TextField;

		protected readonly T ValueField;

		internal override string Text => TextField;

		internal override object Value => ValueField;

		internal InternalSyntaxTokenWithValue(SyntaxKind kind, string text, T value, LexerMode mode)
			: base(kind, text.Length, mode)
		{
			TextField = text;
			ValueField = value;
		}

		internal InternalSyntaxTokenWithValue(SyntaxKind kind, string text, T value)
			: base(kind, text.Length)
		{
			TextField = text;
			ValueField = value;
		}

		internal InternalSyntaxTokenWithValue(SyntaxKind kind, string text, T value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text.Length, diagnostics, annotations)
		{
			TextField = text;
			ValueField = value;
		}

		public override InternalSyntaxNode WithLeadingTriviaBase(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, TextField, (T)Value, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		public override InternalSyntaxNode WithTrailingTriviaBase(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, TextField, (T)Value, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, TextField, (T)Value, trivia, GetTrailingTrivia(), GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, TextField, (T)Value, GetLeadingTrivia(), trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new InternalSyntaxTokenWithValue<T>(Kind, TextField, ValueField, diagnostics, GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new InternalSyntaxTokenWithValue<T>(Kind, TextField, ValueField, GetDiagnostics(), annotations);
		}
	}

	internal class InternalSyntaxTokenWithValueAndTrivia<T> : InternalSyntaxTokenWithValue<T>
	{
		private readonly InternalSyntaxNode leadingTrivia;

		private readonly InternalSyntaxNode trailingTrivia;

		internal InternalSyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode = LexerMode.Default)
			: base(kind, text, value, mode)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		internal InternalSyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text, value, diagnostics, annotations)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		public override InternalSyntaxNode GetLeadingTrivia()
		{
			return leadingTrivia;
		}

		public override InternalSyntaxNode GetTrailingTrivia()
		{
			return trailingTrivia;
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxTokenWithValueAndTrivia<T>));
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, Text, (T)Value, leadingTrivia, trailingTrivia, GetDiagnostics(), annotations);
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxTokenWithValueAndTrivia<T>));
			return new InternalSyntaxTokenWithValueAndTrivia<T>(Kind, Text, (T)Value, leadingTrivia, trailingTrivia, diagnostics, GetAnnotations());
		}
	}

	internal class InternalSyntaxIdentifier : InternalSyntaxToken
	{
		private readonly string textField;

		private readonly string valueText;

		internal override string Text => textField;

		internal override object Value => textField;

		public override string ValueText => valueText;

		internal InternalSyntaxIdentifier(string text, string valueText, LexerMode mode)
			: base(SyntaxKind.IdentifierToken, text.Length, mode)
		{
			textField = text;
			this.valueText = valueText;
		}

		internal InternalSyntaxIdentifier(string text, string valueText)
			: base(SyntaxKind.IdentifierToken, text.Length)
		{
			textField = text;
			this.valueText = valueText;
		}

		internal InternalSyntaxIdentifier(string text, string valueText, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(SyntaxKind.IdentifierToken, text.Length, diagnostics, annotations)
		{
			textField = text;
			this.valueText = valueText;
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxIdentifierWithTrivia(textField, textField, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxIdentifierWithTrivia(textField, textField, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxIdentifier));
			return new InternalSyntaxIdentifier(Text, ValueText, GetDiagnostics(), annotations);
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxIdentifier));
			return new InternalSyntaxIdentifier(Text, ValueText, diagnostics, GetAnnotations());
		}
	}

	internal class InternalSyntaxIdentifierWithTrivia : InternalSyntaxIdentifier
	{
		private readonly InternalSyntaxNode leadingTrivia;

		private readonly InternalSyntaxNode trailingTrivia;

		internal InternalSyntaxIdentifierWithTrivia(string text, string valueText, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode)
			: base(text, valueText, mode)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		internal InternalSyntaxIdentifierWithTrivia(string text, string valueText, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(text, valueText, diagnostics, annotations)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		public override InternalSyntaxNode GetLeadingTrivia()
		{
			return leadingTrivia;
		}

		public override InternalSyntaxNode GetTrailingTrivia()
		{
			return trailingTrivia;
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxIdentifierWithTrivia(Text, Text, trivia, trailingTrivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxIdentifierWithTrivia(Text, Text, leadingTrivia, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxIdentifierWithTrivia));
			return new InternalSyntaxIdentifierWithTrivia(Text, ValueText, leadingTrivia, trailingTrivia, GetDiagnostics(), annotations);
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxIdentifierWithTrivia));
			return new InternalSyntaxIdentifierWithTrivia(Text, ValueText, leadingTrivia, trailingTrivia, diagnostics, GetAnnotations());
		}
	}

	internal class InternalSyntaxKeyword : InternalSyntaxToken
	{
		private readonly string textField;

		internal override string Text => textField;

		internal override object Value => textField;

		public override string ValueText => textField;

		internal InternalSyntaxKeyword(SyntaxKind kind, string text, LexerMode mode = LexerMode.Default)
			: base(kind, text.Length, mode)
		{
			DebugAssertHelper.Assert(kind.IsKeyword() || kind.IsPropertyKeyword());
			textField = text;
		}

		internal InternalSyntaxKeyword(SyntaxKind kind, string text, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text.Length, diagnostics, annotations)
		{
			DebugAssertHelper.Assert(kind.IsKeyword() || kind.IsPropertyKeyword());
			textField = text;
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxKeywordWithTrivia(Kind, textField, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxKeywordWithTrivia(Kind, textField, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxKeyword));
			return new InternalSyntaxKeyword(Kind, Text, GetDiagnostics(), annotations);
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxKeyword));
			return new InternalSyntaxKeyword(Kind, Text, diagnostics, GetAnnotations());
		}
	}

	internal class InternalSyntaxKeywordWithTrivia : InternalSyntaxKeyword
	{
		private readonly InternalSyntaxNode leadingTrivia;

		private readonly InternalSyntaxNode trailingTrivia;

		internal InternalSyntaxKeywordWithTrivia(SyntaxKind kind, string text, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode = LexerMode.Default)
			: base(kind, text, mode)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		internal InternalSyntaxKeywordWithTrivia(SyntaxKind kind, string text, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text, diagnostics, annotations)
		{
			if (leadingTrivia != null)
			{
				AdjustFlagsAndWidth(leadingTrivia);
				this.leadingTrivia = leadingTrivia;
			}
			if (trailingTrivia != null)
			{
				AdjustFlagsAndWidth(trailingTrivia);
				this.trailingTrivia = trailingTrivia;
			}
		}

		public override InternalSyntaxNode GetLeadingTrivia()
		{
			return leadingTrivia;
		}

		public override InternalSyntaxNode GetTrailingTrivia()
		{
			return trailingTrivia;
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxKeywordWithTrivia(Kind, Text, trivia, trailingTrivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new InternalSyntaxKeywordWithTrivia(Kind, Text, leadingTrivia, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxKeywordWithTrivia));
			return new InternalSyntaxKeywordWithTrivia(Kind, Text, leadingTrivia, trailingTrivia, GetDiagnostics(), annotations);
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxKeywordWithTrivia));
			return new InternalSyntaxKeywordWithTrivia(Kind, Text, leadingTrivia, trailingTrivia, diagnostics, GetAnnotations());
		}
	}

	protected class MissingTokenWithTrivia : InternalSyntaxTokenWithTrivia
	{
		internal override string Text => string.Empty;

		internal override object Value
		{
			get
			{
				SyntaxKind kind = Kind;
				if (kind == SyntaxKind.IdentifierToken)
				{
					return string.Empty;
				}
				return null;
			}
		}

		internal MissingTokenWithTrivia(SyntaxKind kind, InternalSyntaxNode leading, InternalSyntaxNode trailing)
			: base(kind, leading, trailing)
		{
			Flags &= ~NodeFlags.IsNotMissing;
		}

		internal MissingTokenWithTrivia(SyntaxKind kind, InternalSyntaxNode leading, InternalSyntaxNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, leading, trailing, diagnostics, annotations)
		{
			Flags &= ~NodeFlags.IsNotMissing;
		}

		internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
		{
			return new MissingTokenWithTrivia(Kind, trivia, GetTrailingTrivia(), GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
		{
			return new MissingTokenWithTrivia(Kind, GetLeadingTrivia(), trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new MissingTokenWithTrivia(Kind, GetLeadingTrivia(), GetTrailingTrivia(), diagnostics, GetAnnotations());
		}

		internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new MissingTokenWithTrivia(Kind, GetLeadingTrivia(), GetTrailingTrivia(), GetDiagnostics(), annotations);
		}
	}

	internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.RDivToken;

	internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.EndOfFileToken;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCache_Default;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCache_Code;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCache_Object;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCache_Property;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCache_Expression;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingSpace_Default;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingSpace_Code;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingSpace_Object;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingSpace_Property;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingSpace_Expression;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingCrLf_Default;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingCrLf_Code;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingCrLf_Object;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingCrLf_Property;

	private static readonly ArrayElement<InternalSyntaxToken>[] tokenCacheTrailingCrLf_Expression;

	public override bool IsToken => true;

	internal virtual string Text => Kind.GetText();

	internal virtual object Value => Text;

	internal InternalSyntaxList<InternalSyntaxNode> LeadingTrivia => new InternalSyntaxList<InternalSyntaxNode>(GetLeadingTrivia());

	internal InternalSyntaxList<InternalSyntaxNode> TrailingTrivia => new InternalSyntaxList<InternalSyntaxNode>(GetTrailingTrivia());

	public virtual string ValueText => Text;

	public override string ToString()
	{
		return Text;
	}

	public override int GetLeadingTriviaWidth()
	{
		return GetLeadingTrivia()?.FullWidth ?? 0;
	}

	public override int GetTrailingTriviaWidth()
	{
		return GetTrailingTrivia()?.FullWidth ?? 0;
	}

	[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Must be fixed")]
	protected InternalSyntaxToken(SyntaxKind kind)
		: base(kind)
	{
		FullWidth = Text.Length;
		Flags |= NodeFlags.IsNotMissing;
	}

	protected InternalSyntaxToken(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
		FullWidth = Text.Length;
		Flags |= NodeFlags.IsNotMissing;
	}

	internal InternalSyntaxToken(SyntaxKind kind, int fullWidth, LexerMode mode)
		: base(kind, fullWidth, mode)
	{
		Flags |= NodeFlags.IsNotMissing;
	}

	internal InternalSyntaxToken(SyntaxKind kind, int fullWidth)
		: base(kind, fullWidth)
	{
		Flags |= NodeFlags.IsNotMissing;
	}

	[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Must be fixed")]
	internal InternalSyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics)
		: base(kind, diagnostics)
	{
		FullWidth = Text.Length;
		Flags |= NodeFlags.IsNotMissing;
	}

	[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Must be fixed")]
	internal InternalSyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		FullWidth = Text.Length;
		Flags |= NodeFlags.IsNotMissing;
	}

	internal InternalSyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics)
		: base(kind, diagnostics, fullWidth)
	{
		Flags |= NodeFlags.IsNotMissing;
	}

	internal InternalSyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations, fullWidth)
	{
		Flags |= NodeFlags.IsNotMissing;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		throw new NotSupportedException();
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		throw new NotSupportedException();
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		throw new NotSupportedException();
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		throw new NotSupportedException();
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxToken));
		return new InternalSyntaxToken(Kind, FullWidth, GetDiagnostics(), annotations);
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		DebugAssertHelper.Assert(GetType() == typeof(InternalSyntaxToken));
		return new InternalSyntaxToken(Kind, FullWidth, diagnostics, GetAnnotations());
	}

	protected internal override void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		if (leading)
		{
			GetLeadingTrivia()?.WriteTo(writer, leading: true, trailing: true);
		}
		writer.Write(Text);
		if (trailing)
		{
			GetTrailingTrivia()?.WriteTo(writer, leading: true, trailing: true);
		}
	}

	public override object GetValue()
	{
		return Value;
	}

	public override string GetValueText()
	{
		return ValueText;
	}

	public override InternalSyntaxNode WithLeadingTriviaBase(InternalSyntaxNode trivia)
	{
		return WithLeadingTrivia(trivia);
	}

	public override InternalSyntaxNode WithTrailingTriviaBase(InternalSyntaxNode trivia)
	{
		return WithTrailingTrivia(trivia);
	}

	internal override InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
	{
		return new InternalSyntaxTokenWithTrivia(Kind, trivia, GetTrailingTrivia(), GetDiagnostics(), GetAnnotations());
	}

	internal override InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
	{
		return new InternalSyntaxTokenWithTrivia(Kind, GetLeadingTrivia(), trivia, GetDiagnostics(), GetAnnotations());
	}

	internal static InternalSyntaxToken WithValue<T>(SyntaxKind kind, string text, T value, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia)
	{
		return new InternalSyntaxTokenWithValueAndTrivia<T>(kind, text, value, leadingTrivia, trailingTrivia);
	}

	internal static InternalSyntaxToken WithValue<T>(SyntaxKind kind, string text, T value, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode)
	{
		return new InternalSyntaxTokenWithValueAndTrivia<T>(kind, text, value, leadingTrivia, trailingTrivia, mode);
	}

	internal static InternalSyntaxToken Identifier(string text, LexerMode mode = LexerMode.Default)
	{
		return new InternalSyntaxIdentifier(text, text, mode);
	}

	internal static InternalSyntaxToken Identifier(InternalSyntaxNode leading, string text, InternalSyntaxNode trailing, LexerMode mode)
	{
		if (leading == null && trailing == null)
		{
			return Identifier(text, mode);
		}
		return new InternalSyntaxIdentifierWithTrivia(text, text, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Keyword(SyntaxKind kind)
	{
		string text = kind.GetText();
		return new InternalSyntaxKeyword(kind, text);
	}

	static InternalSyntaxToken()
	{
		tokenCache_Default = new ArrayElement<InternalSyntaxToken>[163];
		tokenCache_Code = new ArrayElement<InternalSyntaxToken>[163];
		tokenCache_Object = new ArrayElement<InternalSyntaxToken>[163];
		tokenCache_Property = new ArrayElement<InternalSyntaxToken>[163];
		tokenCache_Expression = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingSpace_Default = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingSpace_Code = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingSpace_Object = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingSpace_Property = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingSpace_Expression = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingCrLf_Default = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingCrLf_Code = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingCrLf_Object = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingCrLf_Property = new ArrayElement<InternalSyntaxToken>[163];
		tokenCacheTrailingCrLf_Expression = new ArrayElement<InternalSyntaxToken>[163];
		for (SyntaxKind syntaxKind = SyntaxKind.RDivToken; syntaxKind <= SyntaxKind.EndOfFileToken; syntaxKind++)
		{
			if (syntaxKind.IsKeyword() || syntaxKind.IsPropertyKeyword())
			{
				tokenCache_Default[(int)syntaxKind].Value = new InternalSyntaxKeyword(syntaxKind, syntaxKind.GetText());
				tokenCache_Object[(int)syntaxKind].Value = new InternalSyntaxKeyword(syntaxKind, syntaxKind.GetText(), LexerMode.Object);
				tokenCache_Code[(int)syntaxKind].Value = new InternalSyntaxKeyword(syntaxKind, syntaxKind.GetText(), LexerMode.Code);
				tokenCache_Property[(int)syntaxKind].Value = new InternalSyntaxKeyword(syntaxKind, syntaxKind.GetText(), LexerMode.Property);
				tokenCache_Expression[(int)syntaxKind].Value = new InternalSyntaxKeyword(syntaxKind, syntaxKind.GetText(), LexerMode.Expression);
				tokenCacheTrailingSpace_Default[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.Space);
				tokenCacheTrailingSpace_Object[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.Space, LexerMode.Object);
				tokenCacheTrailingSpace_Code[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.Space, LexerMode.Code);
				tokenCacheTrailingSpace_Property[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.Space, LexerMode.Property);
				tokenCacheTrailingSpace_Expression[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.Space, LexerMode.Expression);
				tokenCacheTrailingCrLf_Default[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.CarriageReturnLineFeed);
				tokenCacheTrailingCrLf_Object[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Object);
				tokenCacheTrailingCrLf_Code[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Code);
				tokenCacheTrailingCrLf_Property[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Property);
				tokenCacheTrailingCrLf_Expression[(int)syntaxKind].Value = new InternalSyntaxKeywordWithTrivia(syntaxKind, syntaxKind.GetText(), null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Expression);
			}
			else
			{
				tokenCache_Default[(int)syntaxKind].Value = new InternalSyntaxToken(syntaxKind, LexerMode.Default);
				tokenCache_Object[(int)syntaxKind].Value = new InternalSyntaxToken(syntaxKind, LexerMode.Object);
				tokenCache_Code[(int)syntaxKind].Value = new InternalSyntaxToken(syntaxKind, LexerMode.Code);
				tokenCache_Property[(int)syntaxKind].Value = new InternalSyntaxToken(syntaxKind, LexerMode.Property);
				tokenCache_Expression[(int)syntaxKind].Value = new InternalSyntaxToken(syntaxKind, LexerMode.Expression);
				tokenCacheTrailingSpace_Default[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.Space);
				tokenCacheTrailingSpace_Object[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.Space, LexerMode.Object);
				tokenCacheTrailingSpace_Code[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.Space, LexerMode.Code);
				tokenCacheTrailingSpace_Property[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.Space, LexerMode.Property);
				tokenCacheTrailingSpace_Expression[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.Space, LexerMode.Expression);
				tokenCacheTrailingCrLf_Default[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.CarriageReturnLineFeed);
				tokenCacheTrailingCrLf_Object[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Object);
				tokenCacheTrailingCrLf_Code[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Code);
				tokenCacheTrailingCrLf_Property[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Property);
				tokenCacheTrailingCrLf_Expression[(int)syntaxKind].Value = new InternalSyntaxTokenWithTrivia(syntaxKind, null, InternalSyntaxFactory.CarriageReturnLineFeed, LexerMode.Expression);
			}
		}
	}

	internal static InternalSyntaxToken Keyword(SyntaxKind kind, string text, LexerMode mode = LexerMode.Default)
	{
		if (string.CompareOrdinal(kind.GetText(), text) == 0)
		{
			switch (mode)
			{
			case LexerMode.Default:
				return tokenCache_Default[(int)kind];
			case LexerMode.Object:
				return tokenCache_Object[(int)kind];
			case LexerMode.Code:
				return tokenCache_Code[(int)kind];
			case LexerMode.Property:
				return tokenCache_Property[(int)kind];
			case LexerMode.Expression:
				return tokenCache_Expression[(int)kind];
			}
		}
		return new InternalSyntaxKeyword(kind, text, mode);
	}

	internal static InternalSyntaxToken Keyword(SyntaxKind kind, InternalSyntaxNode leading, string text, InternalSyntaxNode trailing, LexerMode mode)
	{
		if (leading == null)
		{
			if (trailing == null)
			{
				return Keyword(kind, text, mode);
			}
			if (trailing == InternalSyntaxFactory.Space && string.CompareOrdinal(kind.GetText(), text) == 0)
			{
				switch (mode)
				{
				case LexerMode.Default:
					return tokenCacheTrailingSpace_Default[(int)kind];
				case LexerMode.Object:
					return tokenCacheTrailingSpace_Object[(int)kind];
				case LexerMode.Code:
					return tokenCacheTrailingSpace_Code[(int)kind];
				case LexerMode.Property:
					return tokenCacheTrailingSpace_Property[(int)kind];
				case LexerMode.Expression:
					return tokenCacheTrailingSpace_Expression[(int)kind];
				}
			}
			if (trailing == InternalSyntaxFactory.CarriageReturnLineFeed && string.CompareOrdinal(kind.GetText(), text) == 0)
			{
				switch (mode)
				{
				case LexerMode.Default:
					return tokenCacheTrailingCrLf_Default[(int)kind];
				case LexerMode.Object:
					return tokenCacheTrailingCrLf_Object[(int)kind];
				case LexerMode.Code:
					return tokenCacheTrailingCrLf_Code[(int)kind];
				case LexerMode.Property:
					return tokenCacheTrailingCrLf_Property[(int)kind];
				case LexerMode.Expression:
					return tokenCacheTrailingCrLf_Expression[(int)kind];
				}
			}
		}
		return new InternalSyntaxKeywordWithTrivia(kind, text, leading, trailing, mode);
	}

	internal static InternalSyntaxToken CreateMissing(SyntaxKind kind, InternalSyntaxNode leading, InternalSyntaxNode trailing)
	{
		return new MissingTokenWithTrivia(kind, leading, trailing);
	}

	internal static InternalSyntaxToken Create(SyntaxKind kind, LexerMode mode = LexerMode.Default)
	{
		if (kind >= SyntaxKind.RDivToken && kind < SyntaxKind.EndOfFileToken)
		{
			switch (mode)
			{
			case LexerMode.Default:
				return tokenCache_Default[(int)kind];
			case LexerMode.Object:
				return tokenCache_Object[(int)kind];
			case LexerMode.Code:
				return tokenCache_Code[(int)kind];
			case LexerMode.Property:
				return tokenCache_Property[(int)kind];
			case LexerMode.Expression:
				return tokenCache_Expression[(int)kind];
			}
		}
		return new InternalSyntaxToken(kind, mode);
	}

	internal static InternalSyntaxToken Create(SyntaxKind kind, InternalSyntaxNode leadingTrivia, InternalSyntaxNode trailingTrivia, LexerMode mode = LexerMode.Default)
	{
		if (leadingTrivia == null)
		{
			if (trailingTrivia == null)
			{
				return Create(kind, mode);
			}
			if (kind >= SyntaxKind.RDivToken && kind < SyntaxKind.EndOfFileToken)
			{
				if (trailingTrivia == InternalSyntaxFactory.Space)
				{
					switch (mode)
					{
					case LexerMode.Default:
						return tokenCacheTrailingSpace_Default[(int)kind];
					case LexerMode.Object:
						return tokenCacheTrailingSpace_Object[(int)kind];
					case LexerMode.Code:
						return tokenCacheTrailingSpace_Code[(int)kind];
					case LexerMode.Property:
						return tokenCacheTrailingSpace_Property[(int)kind];
					case LexerMode.Expression:
						return tokenCacheTrailingSpace_Expression[(int)kind];
					}
				}
				if (trailingTrivia == InternalSyntaxFactory.CarriageReturnLineFeed)
				{
					switch (mode)
					{
					case LexerMode.Default:
						return tokenCacheTrailingCrLf_Default[(int)kind];
					case LexerMode.Object:
						return tokenCacheTrailingCrLf_Object[(int)kind];
					case LexerMode.Code:
						return tokenCacheTrailingCrLf_Code[(int)kind];
					case LexerMode.Property:
						return tokenCacheTrailingCrLf_Property[(int)kind];
					case LexerMode.Expression:
						return tokenCacheTrailingCrLf_Expression[(int)kind];
					}
				}
			}
		}
		return new InternalSyntaxTokenWithTrivia(kind, leadingTrivia, trailingTrivia, mode);
	}
}
