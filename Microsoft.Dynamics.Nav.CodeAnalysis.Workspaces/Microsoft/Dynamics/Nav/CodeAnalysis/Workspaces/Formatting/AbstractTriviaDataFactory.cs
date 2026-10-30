using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class AbstractTriviaDataFactory
{
	protected abstract class AbstractComplexTrivia : TriviaDataWithList
	{
		public TreeData TreeInfo { get; }

		public string OriginalString { get; }

		public SyntaxToken Token1 { get; }

		public SyntaxToken Token2 { get; }

		public override bool TreatAsElastic { get; }

		public override bool IsWhitespaceOnlyTrivia => false;

		public override bool ContainsChanges => false;

		public AbstractComplexTrivia(OptionSet optionSet, TreeData treeInfo, SyntaxToken token1, SyntaxToken token2)
			: base(optionSet, null)
		{
			Contract.ThrowIfNull(treeInfo);
			Token1 = token1;
			Token2 = token2;
			TreatAsElastic = CommonFormattingHelpers.HasAnyWhitespaceElasticTrivia(token1, token2);
			TreeInfo = treeInfo;
			OriginalString = TreeInfo.GetTextBetween(token1, token2);
			ExtractLineAndSpace(OriginalString, out var lines, out var spaces);
			LineBreaks = lines;
			Spaces = spaces;
		}

		public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
		{
			if (!SecondTokenIsFirstTokenOnLine)
			{
				return this;
			}
			if (SecondTokenIsFirstTokenOnLine)
			{
				return this;
			}
			return Contract.FailWithReturn<TriviaData>("Can not reach here");
		}

		public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			Contract.ThrowIfFalse(line > 0);
			if (TreatAsElastic)
			{
				return CreateComplexTrivia(line, indentation);
			}
			if (!SecondTokenIsFirstTokenOnLine)
			{
				return CreateComplexTrivia(line, indentation);
			}
			if (SecondTokenIsFirstTokenOnLine)
			{
				if (LineBreaks < line)
				{
					return CreateComplexTrivia(line, indentation);
				}
				if (LineBreaks == line)
				{
					return WithIndentation(indentation, context, formattingRules, cancellationToken);
				}
				if (LineBreaks > line)
				{
					return this;
				}
			}
			return Contract.FailWithReturn<TriviaData>("Can not reach here");
		}

		public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			if (!SecondTokenIsFirstTokenOnLine)
			{
				return this;
			}
			if (Spaces == indentation)
			{
				return this;
			}
			TriviaList list = new TriviaList(Token1.TrailingTrivia, Token2.LeadingTrivia);
			Contract.ThrowIfFalse(list.Count > 0);
			if (ContainsSkippedTokensOrText(list))
			{
				return this;
			}
			TriviaDataWithList triviaData = Format(context, formattingRules, LineBreaks, indentation, cancellationToken);
			string text = CreateString(triviaData, cancellationToken);
			ExtractLineAndSpace(text, out var lines, out var spaces);
			return CreateComplexTrivia(lines, spaces, indentation);
		}

		protected abstract void ExtractLineAndSpace(string text, out int lines, out int spaces);

		protected abstract TriviaData CreateComplexTrivia(int line, int space);

		protected abstract TriviaData CreateComplexTrivia(int line, int space, int indentation);

		protected abstract TriviaDataWithList Format(FormattingContext context, ChainedFormattingRules formattingRules, int lines, int spaces, CancellationToken cancellationToken);

		protected abstract bool ContainsSkippedTokensOrText(TriviaList list);

		private string CreateString(TriviaDataWithList triviaData, CancellationToken cancellationToken)
		{
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			foreach (SyntaxTrivia trivia in triviaData.GetTriviaList(cancellationToken))
			{
				stringBuilder.Append(trivia.ToFullString());
			}
			return StringBuilderPool.ReturnAndFree(stringBuilder);
		}
	}

	protected class FormattedWhitespace : TriviaData
	{
		private readonly string newString;

		public override bool TreatAsElastic => false;

		public override bool IsWhitespaceOnlyTrivia => true;

		public override bool ContainsChanges => true;

		public FormattedWhitespace(OptionSet optionSet, int lineBreaks, int indentation, string language)
			: base(optionSet, language)
		{
			LineBreaks = Math.Max(0, lineBreaks);
			Spaces = Math.Max(0, indentation);
			newString = CreateString(OptionSet.GetOption(FormattingOptions.NewLine, language));
		}

		public override IEnumerable<TextChange> GetTextChanges(TextSpan textSpan)
		{
			return SpecializedCollections.SingletonEnumerable(new TextChange(textSpan, newString));
		}

		public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
		{
			throw new NotImplementedException();
		}

		public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public override void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue)
		{
			throw new NotImplementedException();
		}

		private string CreateString(string newLine)
		{
			if (SecondTokenIsFirstTokenOnLine)
			{
				StringBuilder stringBuilder = StringBuilderPool.Allocate();
				for (int i = 0; i < LineBreaks; i++)
				{
					stringBuilder.Append(newLine);
				}
				stringBuilder.AppendIndentationString(Spaces, OptionSet.GetOption(FormattingOptions.UseTabs, Language), OptionSet.GetOption(FormattingOptions.TabSize, Language));
				return StringBuilderPool.ReturnAndFree(stringBuilder);
			}
			return new string(' ', Spaces);
		}
	}

	protected class ModifiedWhitespace : Whitespace
	{
		private readonly Whitespace original;

		public override bool ContainsChanges => false;

		public ModifiedWhitespace(OptionSet optionSet, int lineBreaks, int indentation, bool elastic, string language)
			: base(optionSet, lineBreaks, indentation, elastic, language)
		{
			original = null;
		}

		public ModifiedWhitespace(OptionSet optionSet, Whitespace original, int lineBreaks, int indentation, bool elastic, string language)
			: base(optionSet, lineBreaks, indentation, elastic, language)
		{
			Contract.ThrowIfNull(original);
			this.original = original;
		}

		public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
		{
			if (original == null)
			{
				return base.WithSpace(space, context, formattingRules);
			}
			if (LineBreaks == original.LineBreaks && original.Spaces == space)
			{
				return original;
			}
			return base.WithSpace(space, context, formattingRules);
		}

		public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			if (original == null)
			{
				return base.WithLine(line, indentation, context, formattingRules, cancellationToken);
			}
			if (original.LineBreaks == line && original.Spaces == indentation)
			{
				return original;
			}
			return base.WithLine(line, indentation, context, formattingRules, cancellationToken);
		}

		public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			if (original == null)
			{
				return base.WithIndentation(indentation, context, formattingRules, cancellationToken);
			}
			if (LineBreaks == original.LineBreaks && original.Spaces == indentation)
			{
				return original;
			}
			return base.WithIndentation(indentation, context, formattingRules, cancellationToken);
		}

		public override void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue)
		{
			formattingResultApplier(tokenPairIndex, new FormattedWhitespace(OptionSet, LineBreaks, Spaces, Language));
		}
	}

	protected class Whitespace : TriviaData
	{
		public override bool TreatAsElastic { get; }

		public override bool IsWhitespaceOnlyTrivia => true;

		public override bool ContainsChanges => false;

		public Whitespace(OptionSet optionSet, int space, bool elastic, string language)
			: this(optionSet, 0, space, elastic, language)
		{
			Contract.ThrowIfFalse(space >= 0);
		}

		public Whitespace(OptionSet optionSet, int lineBreaks, int indentation, bool elastic, string language)
			: base(optionSet, language)
		{
			TreatAsElastic = elastic;
			LineBreaks = lineBreaks;
			Spaces = indentation;
		}

		public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
		{
			if (LineBreaks == 0 && Spaces == space)
			{
				return this;
			}
			return new ModifiedWhitespace(OptionSet, this, 0, space, elastic: false, Language);
		}

		public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			Contract.ThrowIfFalse(line > 0);
			if (LineBreaks == line && Spaces == indentation)
			{
				return this;
			}
			return new ModifiedWhitespace(OptionSet, this, line, indentation, elastic: false, Language);
		}

		public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			if (Spaces == indentation)
			{
				return this;
			}
			return new ModifiedWhitespace(OptionSet, this, LineBreaks, indentation, elastic: false, Language);
		}

		public override void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue)
		{
		}

		public override IEnumerable<TextChange> GetTextChanges(TextSpan span)
		{
			throw new NotImplementedException();
		}
	}

	private const int SpaceCacheSize = 10;

	private const int LineBreakCacheSize = 5;

	private const int IndentationLevelCacheSize = 20;

	private readonly Whitespace[] spaces = new Whitespace[10];

	[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
	private readonly Whitespace[,] whitespaces = new Whitespace[5, 20];

	protected readonly OptionSet OptionSet;

	protected readonly TreeData TreeInfo;

	protected AbstractTriviaDataFactory(TreeData treeInfo, OptionSet optionSet)
	{
		Contract.ThrowIfNull(treeInfo);
		Contract.ThrowIfNull(optionSet);
		TreeInfo = treeInfo;
		OptionSet = optionSet;
		for (int i = 0; i < 10; i++)
		{
			spaces[i] = new Whitespace(OptionSet, i, elastic: false, "AL");
		}
	}

	public abstract TriviaData CreateLeadingTrivia(SyntaxToken token);

	public abstract TriviaData CreateTrailingTrivia(SyntaxToken token);

	public abstract TriviaData Create(SyntaxToken token1, SyntaxToken token2);

	protected TriviaData GetSpaceTriviaData(int space, bool elastic = false)
	{
		Contract.ThrowIfFalse(space >= 0);
		if (elastic)
		{
			return new Whitespace(OptionSet, space, elastic: true, "AL");
		}
		if (space < 10)
		{
			return spaces[space];
		}
		return new Whitespace(OptionSet, space, elastic: false, "AL");
	}

	protected TriviaData GetWhitespaceTriviaData(int lineBreaks, int indentation, bool useTriviaAsItIs, bool elastic)
	{
		Contract.ThrowIfFalse(lineBreaks >= 0);
		Contract.ThrowIfFalse(indentation >= 0);
		if ((!elastic & useTriviaAsItIs) && lineBreaks > 0 && lineBreaks <= 5 && indentation % OptionSet.GetOption(FormattingOptions.IndentationSize, "AL") == 0)
		{
			int num = indentation / OptionSet.GetOption(FormattingOptions.IndentationSize, "AL");
			if (num < 20)
			{
				int num2 = lineBreaks - 1;
				EnsureWhitespaceTriviaInfo(num2, num);
				return whitespaces[num2, num];
			}
		}
		if (!useTriviaAsItIs)
		{
			return new ModifiedWhitespace(OptionSet, lineBreaks, indentation, elastic, "AL");
		}
		return new Whitespace(OptionSet, lineBreaks, indentation, elastic, "AL");
	}

	private void EnsureWhitespaceTriviaInfo(int lineIndex, int indentationLevel)
	{
		Contract.ThrowIfFalse(lineIndex >= 0 && lineIndex < 5);
		Contract.ThrowIfFalse(indentationLevel >= 0 && indentationLevel < whitespaces.Length / whitespaces.Rank);
		if (whitespaces[lineIndex, indentationLevel] == null)
		{
			int indentation = indentationLevel * OptionSet.GetOption(FormattingOptions.IndentationSize, "AL");
			Whitespace value = new Whitespace(OptionSet, lineIndex + 1, indentation, elastic: false, "AL");
			Interlocked.CompareExchange(ref whitespaces[lineIndex, indentationLevel], value, null);
		}
	}
}
