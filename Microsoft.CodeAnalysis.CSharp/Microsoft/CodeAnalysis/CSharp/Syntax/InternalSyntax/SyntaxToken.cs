using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxToken : CSharpSyntaxNode
{
	internal class MissingTokenWithTrivia : SyntaxTokenWithTrivia
	{
		public override string Text => string.Empty;

		public override object Value
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

		internal MissingTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing)
			: base(kind, leading, trailing)
		{
			flags &= ~NodeFlags.IsNotMissing;
		}

		internal MissingTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, leading, trailing, diagnostics, annotations)
		{
			flags &= ~NodeFlags.IsNotMissing;
		}

		internal MissingTokenWithTrivia(ObjectReader reader)
			: base(reader)
		{
			flags &= ~NodeFlags.IsNotMissing;
		}

		static MissingTokenWithTrivia()
		{
			ObjectBinder.RegisterTypeReader(typeof(MissingTokenWithTrivia), (ObjectReader r) => new MissingTokenWithTrivia(r));
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new MissingTokenWithTrivia(Kind, trivia, TrailingField, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new MissingTokenWithTrivia(Kind, LeadingField, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new MissingTokenWithTrivia(Kind, LeadingField, TrailingField, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new MissingTokenWithTrivia(Kind, LeadingField, TrailingField, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxIdentifier : SyntaxToken
	{
		protected readonly string TextField;

		public override string Text => TextField;

		public override object Value => TextField;

		public override string ValueText => TextField;

		static SyntaxIdentifier()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifier), (ObjectReader r) => new SyntaxIdentifier(r));
		}

		internal SyntaxIdentifier(string text)
			: base(SyntaxKind.IdentifierToken, text.Length)
		{
			TextField = text;
		}

		internal SyntaxIdentifier(string text, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(SyntaxKind.IdentifierToken, text.Length, diagnostics, annotations)
		{
			TextField = text;
		}

		internal SyntaxIdentifier(ObjectReader reader)
			: base(reader)
		{
			TextField = reader.ReadString();
			FullWidth = TextField.Length;
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteString(TextField);
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(Kind, TextField, TextField, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(Kind, TextField, TextField, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifier(Text, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifier(Text, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxIdentifierExtended : SyntaxIdentifier
	{
		protected readonly SyntaxKind contextualKind;

		protected readonly string valueText;

		public override SyntaxKind ContextualKind => contextualKind;

		public override string ValueText => valueText;

		public override object Value => valueText;

		internal SyntaxIdentifierExtended(SyntaxKind contextualKind, string text, string valueText)
			: base(text)
		{
			this.contextualKind = contextualKind;
			this.valueText = valueText;
		}

		internal SyntaxIdentifierExtended(SyntaxKind contextualKind, string text, string valueText, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(text, diagnostics, annotations)
		{
			this.contextualKind = contextualKind;
			this.valueText = valueText;
		}

		internal SyntaxIdentifierExtended(ObjectReader reader)
			: base(reader)
		{
			contextualKind = (SyntaxKind)reader.ReadInt16();
			valueText = reader.ReadString();
		}

		static SyntaxIdentifierExtended()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifierExtended), (ObjectReader r) => new SyntaxIdentifierExtended(r));
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteInt16((short)contextualKind);
			writer.WriteString(valueText);
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifierExtended(contextualKind, TextField, valueText, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifierExtended(contextualKind, TextField, valueText, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxIdentifierWithTrailingTrivia : SyntaxIdentifier
	{
		private readonly GreenNode _trailing;

		internal SyntaxIdentifierWithTrailingTrivia(string text, GreenNode trailing)
			: base(text)
		{
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxIdentifierWithTrailingTrivia(string text, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(text, diagnostics, annotations)
		{
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxIdentifierWithTrailingTrivia(ObjectReader reader)
			: base(reader)
		{
			GreenNode greenNode = (GreenNode)reader.ReadValue();
			if (greenNode != null)
			{
				AdjustFlagsAndWidth(greenNode);
				_trailing = greenNode;
			}
		}

		static SyntaxIdentifierWithTrailingTrivia()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifierWithTrailingTrivia), (ObjectReader r) => new SyntaxIdentifierWithTrailingTrivia(r));
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(_trailing);
		}

		public override GreenNode GetTrailingTrivia()
		{
			return _trailing;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(Kind, TextField, TextField, trivia, _trailing, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrailingTrivia(TextField, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifierWithTrailingTrivia(TextField, _trailing, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifierWithTrailingTrivia(TextField, _trailing, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxIdentifierWithTrivia : SyntaxIdentifierExtended
	{
		private readonly GreenNode _leading;

		private readonly GreenNode _trailing;

		internal SyntaxIdentifierWithTrivia(SyntaxKind contextualKind, string text, string valueText, GreenNode leading, GreenNode trailing)
			: base(contextualKind, text, valueText)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxIdentifierWithTrivia(SyntaxKind contextualKind, string text, string valueText, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(contextualKind, text, valueText, diagnostics, annotations)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxIdentifierWithTrivia(ObjectReader reader)
			: base(reader)
		{
			GreenNode greenNode = (GreenNode)reader.ReadValue();
			if (greenNode != null)
			{
				AdjustFlagsAndWidth(greenNode);
				_leading = greenNode;
			}
			GreenNode greenNode2 = (GreenNode)reader.ReadValue();
			if (greenNode2 != null)
			{
				_trailing = greenNode2;
				AdjustFlagsAndWidth(greenNode2);
			}
		}

		static SyntaxIdentifierWithTrivia()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifierWithTrivia), (ObjectReader r) => new SyntaxIdentifierWithTrivia(r));
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(_leading);
			writer.WriteValue(_trailing);
		}

		public override GreenNode GetLeadingTrivia()
		{
			return _leading;
		}

		public override GreenNode GetTrailingTrivia()
		{
			return _trailing;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, trivia, _trailing, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, _leading, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, _leading, _trailing, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifierWithTrivia(contextualKind, TextField, valueText, _leading, _trailing, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxTokenWithValue<T> : SyntaxToken
	{
		protected readonly string TextField;

		protected readonly T ValueField;

		public override string Text => TextField;

		public override object Value => ValueField;

		public override string ValueText => Convert.ToString(ValueField, CultureInfo.InvariantCulture);

		internal SyntaxTokenWithValue(SyntaxKind kind, string text, T value)
			: base(kind, text.Length)
		{
			TextField = text;
			ValueField = value;
		}

		internal SyntaxTokenWithValue(SyntaxKind kind, string text, T value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text.Length, diagnostics, annotations)
		{
			TextField = text;
			ValueField = value;
		}

		internal SyntaxTokenWithValue(ObjectReader reader)
			: base(reader)
		{
			TextField = reader.ReadString();
			FullWidth = TextField.Length;
			ValueField = (T)reader.ReadValue();
		}

		static SyntaxTokenWithValue()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithValue<T>), (ObjectReader r) => new SyntaxTokenWithValue<T>(r));
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteString(TextField);
			writer.WriteValue(ValueField);
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, trivia, null, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, null, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithValue<T>(Kind, TextField, ValueField, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithValue<T>(Kind, TextField, ValueField, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxTokenWithValueAndTrivia<T> : SyntaxTokenWithValue<T>
	{
		private readonly GreenNode _leading;

		private readonly GreenNode _trailing;

		static SyntaxTokenWithValueAndTrivia()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithValueAndTrivia<T>), (ObjectReader r) => new SyntaxTokenWithValueAndTrivia<T>(r));
		}

		internal SyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, GreenNode leading, GreenNode trailing)
			: base(kind, text, value)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text, value, diagnostics, annotations)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		internal SyntaxTokenWithValueAndTrivia(ObjectReader reader)
			: base(reader)
		{
			GreenNode greenNode = (GreenNode)reader.ReadValue();
			if (greenNode != null)
			{
				AdjustFlagsAndWidth(greenNode);
				_leading = greenNode;
			}
			GreenNode greenNode2 = (GreenNode)reader.ReadValue();
			if (greenNode2 != null)
			{
				AdjustFlagsAndWidth(greenNode2);
				_trailing = greenNode2;
			}
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(_leading);
			writer.WriteValue(_trailing);
		}

		public override GreenNode GetLeadingTrivia()
		{
			return _leading;
		}

		public override GreenNode GetTrailingTrivia()
		{
			return _trailing;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, trivia, _trailing, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, _leading, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, _leading, _trailing, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(Kind, TextField, ValueField, _leading, _trailing, GetDiagnostics(), annotations);
		}
	}

	internal class SyntaxTokenWithTrivia : SyntaxToken
	{
		protected readonly GreenNode LeadingField;

		protected readonly GreenNode TrailingField;

		static SyntaxTokenWithTrivia()
		{
			ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithTrivia), (ObjectReader r) => new SyntaxTokenWithTrivia(r));
		}

		internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing)
			: base(kind)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				LeadingField = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				TrailingField = trailing;
			}
		}

		internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, diagnostics, annotations)
		{
			if (leading != null)
			{
				AdjustFlagsAndWidth(leading);
				LeadingField = leading;
			}
			if (trailing != null)
			{
				AdjustFlagsAndWidth(trailing);
				TrailingField = trailing;
			}
		}

		internal SyntaxTokenWithTrivia(ObjectReader reader)
			: base(reader)
		{
			GreenNode greenNode = (GreenNode)reader.ReadValue();
			if (greenNode != null)
			{
				AdjustFlagsAndWidth(greenNode);
				LeadingField = greenNode;
			}
			GreenNode greenNode2 = (GreenNode)reader.ReadValue();
			if (greenNode2 != null)
			{
				AdjustFlagsAndWidth(greenNode2);
				TrailingField = greenNode2;
			}
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(LeadingField);
			writer.WriteValue(TrailingField);
		}

		public override GreenNode GetLeadingTrivia()
		{
			return LeadingField;
		}

		public override GreenNode GetTrailingTrivia()
		{
			return TrailingField;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(Kind, trivia, TrailingField, GetDiagnostics(), GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(Kind, LeadingField, trivia, GetDiagnostics(), GetAnnotations());
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithTrivia(Kind, LeadingField, TrailingField, diagnostics, GetAnnotations());
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithTrivia(Kind, LeadingField, TrailingField, GetDiagnostics(), annotations);
		}
	}

	internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.TildeToken;

	internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.EndOfFileToken;

	private static readonly ArrayElement<SyntaxToken>[] s_tokensWithNoTrivia;

	private static readonly ArrayElement<SyntaxToken>[] s_tokensWithElasticTrivia;

	private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingSpace;

	private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingCRLF;

	internal override bool ShouldReuseInSerialization
	{
		get
		{
			if (base.ShouldReuseInSerialization)
			{
				return FullWidth < 42;
			}
			return false;
		}
	}

	public override bool IsToken => true;

	public virtual SyntaxKind ContextualKind => Kind;

	public override int RawContextualKind => (int)ContextualKind;

	public virtual string Text => SyntaxFacts.GetText(Kind);

	public virtual object Value => Kind switch
	{
		SyntaxKind.TrueKeyword => (object)true, 
		SyntaxKind.FalseKeyword => false, 
		SyntaxKind.NullKeyword => null, 
		_ => Text, 
	};

	public virtual string ValueText => Text;

	public override int Width => Text.Length;

	internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> LeadingTrivia => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(GetLeadingTrivia());

	internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode> TrailingTrivia => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(GetTrailingTrivia());

	internal SyntaxToken(SyntaxKind kind)
		: base(kind)
	{
		FullWidth = Text.Length;
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics)
		: base(kind, diagnostics)
	{
		FullWidth = Text.Length;
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		FullWidth = Text.Length;
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(SyntaxKind kind, int fullWidth)
		: base(kind, fullWidth)
	{
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics)
		: base(kind, diagnostics, fullWidth)
	{
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations, fullWidth)
	{
		flags |= NodeFlags.IsNotMissing;
	}

	internal SyntaxToken(ObjectReader reader)
		: base(reader)
	{
		string text = Text;
		if (text != null)
		{
			FullWidth = text.Length;
		}
		flags |= NodeFlags.IsNotMissing;
	}

	internal override GreenNode GetSlot(int index)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal static SyntaxToken Create(SyntaxKind kind)
	{
		if ((int)kind > 8496)
		{
			if (!SyntaxFacts.IsAnyToken(kind))
			{
				throw new ArgumentException(string.Format(CSharpResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), "kind");
			}
			return CreateMissing(kind, null, null);
		}
		return s_tokensWithNoTrivia[(uint)kind].Value;
	}

	internal static SyntaxToken Create(SyntaxKind kind, GreenNode leading, GreenNode trailing)
	{
		if ((int)kind > 8496)
		{
			if (!SyntaxFacts.IsAnyToken(kind))
			{
				throw new ArgumentException(string.Format(CSharpResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), "kind");
			}
			return CreateMissing(kind, leading, trailing);
		}
		if (leading == null)
		{
			if (trailing == null)
			{
				return s_tokensWithNoTrivia[(uint)kind].Value;
			}
			if (trailing == SyntaxFactory.Space)
			{
				return s_tokensWithSingleTrailingSpace[(uint)kind].Value;
			}
			if (trailing == SyntaxFactory.CarriageReturnLineFeed)
			{
				return s_tokensWithSingleTrailingCRLF[(uint)kind].Value;
			}
		}
		if (leading == SyntaxFactory.ElasticZeroSpace && trailing == SyntaxFactory.ElasticZeroSpace)
		{
			return s_tokensWithElasticTrivia[(uint)kind].Value;
		}
		return new SyntaxTokenWithTrivia(kind, leading, trailing);
	}

	internal static SyntaxToken CreateMissing(SyntaxKind kind, GreenNode leading, GreenNode trailing)
	{
		return new MissingTokenWithTrivia(kind, leading, trailing);
	}

	static SyntaxToken()
	{
		s_tokensWithNoTrivia = new ArrayElement<SyntaxToken>[8497];
		s_tokensWithElasticTrivia = new ArrayElement<SyntaxToken>[8497];
		s_tokensWithSingleTrailingSpace = new ArrayElement<SyntaxToken>[8497];
		s_tokensWithSingleTrailingCRLF = new ArrayElement<SyntaxToken>[8497];
		ObjectBinder.RegisterTypeReader(typeof(SyntaxToken), (ObjectReader r) => new SyntaxToken(r));
		SyntaxKind syntaxKind = SyntaxKind.TildeToken;
		while ((int)syntaxKind <= 8496)
		{
			s_tokensWithNoTrivia[(uint)syntaxKind].Value = new SyntaxToken(syntaxKind);
			s_tokensWithElasticTrivia[(uint)syntaxKind].Value = new SyntaxTokenWithTrivia(syntaxKind, SyntaxFactory.ElasticZeroSpace, SyntaxFactory.ElasticZeroSpace);
			s_tokensWithSingleTrailingSpace[(uint)syntaxKind].Value = new SyntaxTokenWithTrivia(syntaxKind, null, SyntaxFactory.Space);
			s_tokensWithSingleTrailingCRLF[(uint)syntaxKind].Value = new SyntaxTokenWithTrivia(syntaxKind, null, SyntaxFactory.CarriageReturnLineFeed);
			syntaxKind++;
		}
	}

	internal static IEnumerable<SyntaxToken> GetWellKnownTokens()
	{
		ArrayElement<SyntaxToken>[] array = s_tokensWithNoTrivia;
		for (int i = 0; i < array.Length; i++)
		{
			ArrayElement<SyntaxToken> arrayElement = array[i];
			if (arrayElement.Value != null)
			{
				yield return arrayElement.Value;
			}
		}
		array = s_tokensWithElasticTrivia;
		for (int i = 0; i < array.Length; i++)
		{
			ArrayElement<SyntaxToken> arrayElement2 = array[i];
			if (arrayElement2.Value != null)
			{
				yield return arrayElement2.Value;
			}
		}
		array = s_tokensWithSingleTrailingSpace;
		for (int i = 0; i < array.Length; i++)
		{
			ArrayElement<SyntaxToken> arrayElement3 = array[i];
			if (arrayElement3.Value != null)
			{
				yield return arrayElement3.Value;
			}
		}
		array = s_tokensWithSingleTrailingCRLF;
		for (int i = 0; i < array.Length; i++)
		{
			ArrayElement<SyntaxToken> arrayElement4 = array[i];
			if (arrayElement4.Value != null)
			{
				yield return arrayElement4.Value;
			}
		}
	}

	internal static SyntaxToken Identifier(string text)
	{
		return new SyntaxIdentifier(text);
	}

	internal static SyntaxToken Identifier(GreenNode leading, string text, GreenNode trailing)
	{
		if (leading == null)
		{
			if (trailing == null)
			{
				return Identifier(text);
			}
			return new SyntaxIdentifierWithTrailingTrivia(text, trailing);
		}
		return new SyntaxIdentifierWithTrivia(SyntaxKind.IdentifierToken, text, text, leading, trailing);
	}

	internal static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode leading, string text, string valueText, GreenNode trailing)
	{
		if (contextualKind == SyntaxKind.IdentifierToken && valueText == text)
		{
			return Identifier(leading, text, trailing);
		}
		return new SyntaxIdentifierWithTrivia(contextualKind, text, valueText, leading, trailing);
	}

	internal static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value)
	{
		return new SyntaxTokenWithValue<T>(kind, text, value);
	}

	internal static SyntaxToken WithValue<T>(SyntaxKind kind, GreenNode leading, string text, T value, GreenNode trailing)
	{
		return new SyntaxTokenWithValueAndTrivia<T>(kind, text, value, leading, trailing);
	}

	internal static SyntaxToken StringLiteral(string text)
	{
		return new SyntaxTokenWithValue<string>(SyntaxKind.StringLiteralToken, text, text);
	}

	internal static SyntaxToken StringLiteral(CSharpSyntaxNode leading, string text, CSharpSyntaxNode trailing)
	{
		return new SyntaxTokenWithValueAndTrivia<string>(SyntaxKind.StringLiteralToken, text, text, leading, trailing);
	}

	public override string ToString()
	{
		return Text;
	}

	public override object GetValue()
	{
		return Value;
	}

	public override string GetValueText()
	{
		return ValueText;
	}

	public override int GetLeadingTriviaWidth()
	{
		return GetLeadingTrivia()?.FullWidth ?? 0;
	}

	public override int GetTrailingTriviaWidth()
	{
		return GetTrailingTrivia()?.FullWidth ?? 0;
	}

	public sealed override GreenNode WithLeadingTrivia(GreenNode trivia)
	{
		return TokenWithLeadingTrivia(trivia);
	}

	public virtual SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
	{
		return new SyntaxTokenWithTrivia(Kind, trivia, null, GetDiagnostics(), GetAnnotations());
	}

	public sealed override GreenNode WithTrailingTrivia(GreenNode trivia)
	{
		return TokenWithTrailingTrivia(trivia);
	}

	public virtual SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
	{
		return new SyntaxTokenWithTrivia(Kind, null, trivia, GetDiagnostics(), GetAnnotations());
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SyntaxToken(Kind, FullWidth, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SyntaxToken(Kind, FullWidth, GetDiagnostics(), annotations);
	}

	internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
	{
		if (ContainsDirectives)
		{
			stack = ApplyDirectivesToTrivia(GetLeadingTrivia(), stack);
			stack = ApplyDirectivesToTrivia(GetTrailingTrivia(), stack);
		}
		return stack;
	}

	private static DirectiveStack ApplyDirectivesToTrivia(GreenNode triviaList, DirectiveStack stack)
	{
		if (triviaList != null && triviaList.ContainsDirectives)
		{
			return CSharpSyntaxNode.ApplyDirectivesToListOrNode(triviaList, stack);
		}
		return stack;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitToken(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitToken(this);
	}

	protected override void WriteTokenTo(TextWriter writer, bool leading, bool trailing)
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

	public override bool IsEquivalentTo(GreenNode other)
	{
		if (!base.IsEquivalentTo(other))
		{
			return false;
		}
		SyntaxToken syntaxToken = (SyntaxToken)other;
		if (Text != syntaxToken.Text)
		{
			return false;
		}
		GreenNode leadingTrivia = GetLeadingTrivia();
		GreenNode leadingTrivia2 = syntaxToken.GetLeadingTrivia();
		if (leadingTrivia != leadingTrivia2)
		{
			if (leadingTrivia == null || leadingTrivia2 == null)
			{
				return false;
			}
			if (!leadingTrivia.IsEquivalentTo(leadingTrivia2))
			{
				return false;
			}
		}
		GreenNode trailingTrivia = GetTrailingTrivia();
		GreenNode trailingTrivia2 = syntaxToken.GetTrailingTrivia();
		if (trailingTrivia != trailingTrivia2)
		{
			if (trailingTrivia == null || trailingTrivia2 == null)
			{
				return false;
			}
			if (!trailingTrivia.IsEquivalentTo(trailingTrivia2))
			{
				return false;
			}
		}
		return true;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
