using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class TriviaDataFactory : AbstractTriviaDataFactory
{
	private class Analyzer
	{
		internal struct AnalysisResult
		{
			internal int LineBreaks { get; set; }

			internal int Space { get; set; }

			internal int Tab { get; set; }

			internal bool HasTabAfterSpace { get; set; }

			internal bool HasUnknownWhitespace { get; set; }

			internal bool HasTrailingSpace { get; set; }

			internal bool HasSkippedTokens { get; set; }

			internal bool HasSkippedOrDisabledText { get; set; }

			internal bool HasConflictMarker { get; set; }

			internal bool HasComments { get; set; }

			internal bool HasPreprocessor { get; set; }

			internal bool TreatAsElastic { get; set; }
		}

		public static AnalysisResult Leading(SyntaxToken token)
		{
			AnalysisResult result = default;
			Analyze(token.LeadingTrivia, ref result);
			return result;
		}

		public static AnalysisResult Trailing(SyntaxToken token)
		{
			AnalysisResult result = default;
			Analyze(token.TrailingTrivia, ref result);
			return result;
		}

		public static AnalysisResult Between(SyntaxToken token1, SyntaxToken token2)
		{
			if (!token1.HasTrailingTrivia && !token2.HasLeadingTrivia)
			{
				return default;
			}
			AnalysisResult result = default;
			if (token1.IsMissing && token1.FullWidth() == 0)
			{
				SyntaxToken token3 = token1;
				while (!token3.IsKind(SyntaxKind.None))
				{
					SyntaxToken previousToken = token3.GetPreviousToken(includeZeroWidth: true);
					if (previousToken.FullWidth() == 0)
					{
						token3 = previousToken;
						continue;
					}
					if (previousToken.TrailingTrivia.Count > 0 && previousToken.TrailingTrivia.Last().Kind == SyntaxKind.EndOfLineTrivia)
					{
						result.LineBreaks = 1;
					}
					break;
				}
			}
			else
			{
				Analyze(token1.TrailingTrivia, ref result);
			}
			Analyze(token2.LeadingTrivia, ref result);
			return result;
		}

		private static void Analyze(SyntaxTriviaList list, ref AnalysisResult result)
		{
			if (list.Count == 0)
			{
				return;
			}
			foreach (SyntaxTrivia item in list)
			{
				if (item.Kind == SyntaxKind.WhiteSpaceTrivia)
				{
					AnalyzeWhitespacesInTrivia(item, ref result);
				}
				else if (item.Kind == SyntaxKind.EndOfLineTrivia)
				{
					AnalyzeLineBreak(item, ref result);
				}
				else if (item.IsRegularOrDocComment())
				{
					result.HasComments = true;
				}
				else if (item.Kind == SyntaxKind.SkippedTokensTrivia)
				{
					result.HasSkippedTokens = true;
				}
				else
				{
					result.HasPreprocessor = true;
				}
			}
		}

		private static void AnalyzeLineBreak(SyntaxTrivia trivia, ref AnalysisResult result)
		{
			if (result.Space > 0 || result.Tab > 0)
			{
				result.HasTrailingSpace = true;
			}
			result.LineBreaks++;
			result.HasTabAfterSpace = false;
			result.Space = 0;
			result.Tab = 0;
			result.TreatAsElastic |= trivia.IsElastic();
		}

		private static void AnalyzeWhitespacesInTrivia(SyntaxTrivia trivia, ref AnalysisResult result)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			string text = trivia.ToString();
			for (int i = 0; i < trivia.Width(); i++)
			{
				if (text[i] == ' ')
				{
					num++;
				}
				else if (text[i] == '\t')
				{
					if (result.Space > 0)
					{
						result.HasTabAfterSpace = true;
					}
					num2++;
				}
				else
				{
					num3++;
				}
			}
			result.Space += num;
			result.Tab += num2;
			result.HasUnknownWhitespace |= num3 > 0;
			result.TreatAsElastic |= trivia.IsElastic();
		}
	}

	private struct CodeShapeAnalyzer
	{
		private readonly FormattingContext context;

		private readonly OptionSet optionSet;

		private readonly TriviaList triviaList;

		private int indentation;

		private bool hasTrailingSpaces;

		private int lastLineBreakIndex;

		private bool touchedNoisyCharacterOnCurrentLine;

		private bool UseIndentation => lastLineBreakIndex >= 0;

		public static bool ShouldFormatMultiLine(FormattingContext context, bool firstTriviaInTree, TriviaList triviaList)
		{
			return new CodeShapeAnalyzer(context, firstTriviaInTree, triviaList).ShouldFormat();
		}

		public static bool ShouldFormatSingleLine(TriviaList list)
		{
			foreach (SyntaxTrivia item in list)
			{
				Contract.ThrowIfTrue(item.Kind == SyntaxKind.EndOfLineTrivia);
				Contract.ThrowIfTrue(item.Kind == SyntaxKind.SkippedTokensTrivia);
				if (item.IsElastic())
				{
					return true;
				}
				if (item.Kind == SyntaxKind.WhiteSpaceTrivia && item.ToString().IndexOf('\t') >= 0)
				{
					return true;
				}
				if (item.IsRegularOrDocComment())
				{
					return false;
				}
			}
			return true;
		}

		public static bool ContainsSkippedTokensOrText(TriviaList list)
		{
			foreach (SyntaxTrivia item in list)
			{
				if (item.Kind == SyntaxKind.SkippedTokensTrivia)
				{
					return true;
				}
			}
			return false;
		}

		private CodeShapeAnalyzer(FormattingContext context, bool firstTriviaInTree, TriviaList triviaList)
		{
			this.context = context;
			optionSet = context.OptionSet;
			this.triviaList = triviaList;
			indentation = 0;
			hasTrailingSpaces = false;
			lastLineBreakIndex = ((!firstTriviaInTree) ? (-1) : 0);
			touchedNoisyCharacterOnCurrentLine = false;
		}

		private bool OnElastic(SyntaxTrivia trivia)
		{
			return trivia.IsElastic();
		}

		private bool OnWhitespace(SyntaxTrivia trivia)
		{
			if (trivia.Kind != SyntaxKind.WhiteSpaceTrivia)
			{
				return false;
			}
			if (!UseIndentation || touchedNoisyCharacterOnCurrentLine)
			{
				hasTrailingSpaces = true;
				return false;
			}
			string text = trivia.ToString();
			if (text.IndexOf('\t') >= 0)
			{
				return true;
			}
			indentation += text.ConvertTabToSpace(optionSet.GetOption(FormattingOptions.TabSize, "AL"), indentation, text.Length);
			return false;
		}

		private bool OnEndOfLine(SyntaxTrivia trivia, int currentIndex)
		{
			if (trivia.Kind != SyntaxKind.EndOfLineTrivia)
			{
				return false;
			}
			if (hasTrailingSpaces)
			{
				return true;
			}
			if (indentation > 0 && !touchedNoisyCharacterOnCurrentLine)
			{
				return true;
			}
			ResetStateAfterNewLine(currentIndex);
			return false;
		}

		private void ResetStateAfterNewLine(int currentIndex)
		{
			indentation = 0;
			touchedNoisyCharacterOnCurrentLine = false;
			hasTrailingSpaces = false;
			lastLineBreakIndex = currentIndex;
		}

		private bool OnComment(SyntaxTrivia trivia, int currentIndex)
		{
			if (!trivia.IsRegularOrDocComment())
			{
				return false;
			}
			if (UseIndentation && indentation != context.GetBaseIndentation(trivia.SpanStart))
			{
				return true;
			}
			return false;
		}

		private bool OnSkippedTokensOrText(SyntaxTrivia trivia)
		{
			if (trivia.Kind != SyntaxKind.SkippedTokensTrivia)
			{
				return false;
			}
			return Contract.FailWithReturn<bool>("This can't happen");
		}

		private bool OnTouchedNoisyCharacter(SyntaxTrivia trivia)
		{
			if (trivia.IsElastic() || trivia.Kind == SyntaxKind.WhiteSpaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
			{
				return false;
			}
			touchedNoisyCharacterOnCurrentLine = true;
			hasTrailingSpaces = false;
			return false;
		}

		private bool ShouldFormat()
		{
			int num = -1;
			foreach (SyntaxTrivia trivia in triviaList)
			{
				num++;
				if (OnElastic(trivia) || OnWhitespace(trivia) || OnEndOfLine(trivia, num) || OnTouchedNoisyCharacter(trivia) || OnComment(trivia, num) || OnSkippedTokensOrText(trivia))
				{
					return true;
				}
			}
			return false;
		}
	}

	private class ComplexTrivia : AbstractComplexTrivia
	{
		public ComplexTrivia(OptionSet optionSet, TreeData treeInfo, SyntaxToken token1, SyntaxToken token2)
			: base(optionSet, treeInfo, token1, token2)
		{
		}

		public override void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue)
		{
			if (ShouldFormat(context))
			{
				formattingResultApplier(tokenPairIndex, Format(context, formattingRules, LineBreaks, Spaces, cancellationToken));
			}
		}

		public override List<SyntaxTrivia> GetTriviaList(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<TextChange> GetTextChanges(TextSpan span)
		{
			throw new NotImplementedException();
		}

		protected override void ExtractLineAndSpace(string text, out int lines, out int spaces)
		{
			text.ProcessTextBetweenTokens(TreeInfo, Token1, OptionSet.GetOption(FormattingOptions.TabSize, "AL"), out lines, out spaces);
		}

		protected override TriviaData CreateComplexTrivia(int line, int space)
		{
			return CreateModifiedComplexTrivia(line, space);
		}

		protected override TriviaData CreateComplexTrivia(int line, int space, int indentation)
		{
			return CreateModifiedComplexTrivia(line, space);
		}

		protected override TriviaDataWithList Format(FormattingContext context, ChainedFormattingRules formattingRules, int lines, int spaces, CancellationToken cancellationToken)
		{
			return new FormattedComplexTrivia(context, formattingRules, Token1, Token2, lines, spaces, OriginalString, cancellationToken);
		}

		protected override bool ContainsSkippedTokensOrText(TriviaList list)
		{
			return CodeShapeAnalyzer.ContainsSkippedTokensOrText(list);
		}

		private TriviaData CreateModifiedComplexTrivia(int line, int space)
		{
			return new ModifiedComplexTrivia(OptionSet, this, line, space);
		}

		private bool ShouldFormat(FormattingContext context)
		{
			SyntaxToken token = Token1;
			SyntaxToken token2 = Token2;
			int end = ((token2.Kind == SyntaxKind.None) ? token.Span.End : token2.Span.Start);
			TextSpan textSpan = TextSpan.FromBounds(token.Span.End, end);
			if (context.IsSpacingSuppressed(textSpan))
			{
				return false;
			}
			TriviaList triviaList = new TriviaList(token.TrailingTrivia, token2.LeadingTrivia);
			Contract.ThrowIfFalse(triviaList.Count > 0);
			if (ContainsSkippedTokensOrText(triviaList))
			{
				return false;
			}
			if (!SecondTokenIsFirstTokenOnLine)
			{
				return CodeShapeAnalyzer.ShouldFormatSingleLine(triviaList);
			}
			if (OptionSet.GetOption(FormattingOptions.UseTabs, "AL"))
			{
				return true;
			}
			bool firstTriviaInTree = Token1.Kind == SyntaxKind.None;
			return CodeShapeAnalyzer.ShouldFormatMultiLine(context, firstTriviaInTree, triviaList);
		}
	}

	private class FormattedComplexTrivia : TriviaDataWithList
	{
		private readonly TriviaFormatter formatter;

		private readonly IList<TextChange> textChanges;

		public override bool TreatAsElastic => false;

		public override bool IsWhitespaceOnlyTrivia => false;

		public override bool ContainsChanges => textChanges.Count > 0;

		public FormattedComplexTrivia(FormattingContext context, ChainedFormattingRules formattingRules, SyntaxToken token1, SyntaxToken token2, int lineBreaks, int spaces, string originalString, CancellationToken cancellationToken)
			: base(context.OptionSet, "AL")
		{
			Contract.ThrowIfNull(context);
			Contract.ThrowIfNull(formattingRules);
			Contract.ThrowIfNull(originalString);
			LineBreaks = Math.Max(0, lineBreaks);
			Spaces = Math.Max(0, spaces);
			formatter = new TriviaFormatter(context, formattingRules, token1, token2, originalString, LineBreaks, Spaces);
			textChanges = formatter.FormatToTextChanges(cancellationToken);
		}

		public override IEnumerable<TextChange> GetTextChanges(TextSpan span)
		{
			return textChanges;
		}

		public override List<SyntaxTrivia> GetTriviaList(CancellationToken cancellationToken)
		{
			return formatter.FormatToSyntaxTrivia(cancellationToken);
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
	}

	private class ModifiedComplexTrivia : TriviaDataWithList
	{
		private readonly ComplexTrivia original;

		public override bool ContainsChanges => false;

		public override bool TreatAsElastic => original.TreatAsElastic;

		public override bool IsWhitespaceOnlyTrivia => false;

		public ModifiedComplexTrivia(OptionSet optionSet, ComplexTrivia original, int lineBreaks, int space)
			: base(optionSet, "AL")
		{
			Contract.ThrowIfNull(original);
			this.original = original;
			LineBreaks = lineBreaks;
			Spaces = space;
		}

		public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
		{
			return original.WithSpace(space, context, formattingRules);
		}

		public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			return original.WithLine(line, indentation, context, formattingRules, cancellationToken);
		}

		public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
		{
			return original.WithIndentation(indentation, context, formattingRules, cancellationToken);
		}

		public override void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue)
		{
			Contract.ThrowIfFalse(SecondTokenIsFirstTokenOnLine);
			SyntaxToken token = original.Token1;
			SyntaxToken token2 = original.Token2;
			TriviaList list = new TriviaList(token.TrailingTrivia, token2.LeadingTrivia);
			Contract.ThrowIfFalse(list.Count > 0);
			if (!CodeShapeAnalyzer.ContainsSkippedTokensOrText(list))
			{
				formattingResultApplier(tokenPairIndex, new FormattedComplexTrivia(context, formattingRules, original.Token1, original.Token2, LineBreaks, Spaces, original.OriginalString, cancellationToken));
			}
		}

		public override List<SyntaxTrivia> GetTriviaList(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<TextChange> GetTextChanges(TextSpan span)
		{
			throw new NotImplementedException();
		}
	}

	public TriviaDataFactory(TreeData treeInfo, OptionSet optionSet)
		: base(treeInfo, optionSet)
	{
	}

	public override TriviaData CreateLeadingTrivia(SyntaxToken token)
	{
		if (!token.HasLeadingTrivia)
		{
			return GetSpaceTriviaData(0);
		}
		Analyzer.AnalysisResult result = Analyzer.Leading(token);
		TriviaData whitespaceOnlyTriviaInfo = GetWhitespaceOnlyTriviaInfo(default, token, result);
		if (whitespaceOnlyTriviaInfo != null)
		{
			return whitespaceOnlyTriviaInfo;
		}
		return new ComplexTrivia(OptionSet, TreeInfo, default, token);
	}

	public override TriviaData CreateTrailingTrivia(SyntaxToken token)
	{
		if (!token.HasTrailingTrivia)
		{
			return GetSpaceTriviaData(0);
		}
		Analyzer.AnalysisResult result = Analyzer.Trailing(token);
		TriviaData whitespaceOnlyTriviaInfo = GetWhitespaceOnlyTriviaInfo(token, default, result);
		if (whitespaceOnlyTriviaInfo != null)
		{
			return whitespaceOnlyTriviaInfo;
		}
		return new ComplexTrivia(OptionSet, TreeInfo, token, default);
	}

	public override TriviaData Create(SyntaxToken token1, SyntaxToken token2)
	{
		if (!token1.HasTrailingTrivia && !token2.HasLeadingTrivia)
		{
			return GetSpaceTriviaData(0);
		}
		Analyzer.AnalysisResult result = Analyzer.Between(token1, token2);
		TriviaData whitespaceOnlyTriviaInfo = GetWhitespaceOnlyTriviaInfo(token1, token2, result);
		if (whitespaceOnlyTriviaInfo != null)
		{
			return whitespaceOnlyTriviaInfo;
		}
		return new ComplexTrivia(OptionSet, TreeInfo, token1, token2);
	}

	private static bool IsCSharpWhitespace(char c)
	{
		if (!SyntaxFacts.IsWhitespace(c))
		{
			return SyntaxFacts.IsNewLine(c);
		}
		return true;
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private bool ContainsOnlyWhitespace(Analyzer.AnalysisResult result)
	{
		if (!result.HasComments && !result.HasPreprocessor && !result.HasSkippedTokens && !result.HasSkippedOrDisabledText)
		{
			return !result.HasConflictMarker;
		}
		return false;
	}

	private TriviaData GetWhitespaceOnlyTriviaInfo(SyntaxToken token1, SyntaxToken token2, Analyzer.AnalysisResult result)
	{
		if (!ContainsOnlyWhitespace(result))
		{
			return null;
		}
		int spaceOnSingleLine = GetSpaceOnSingleLine(result);
		Contract.ThrowIfFalse(spaceOnSingleLine >= -1);
		if (spaceOnSingleLine >= 0)
		{
			return GetSpaceTriviaData(spaceOnSingleLine, result.TreatAsElastic);
		}
		if (result.LineBreaks == 0 && result.Tab > 0)
		{
			int indentation = CalculateSpaces(token1, token2);
			return new ModifiedWhitespace(OptionSet, result.LineBreaks, indentation, result.TreatAsElastic, "AL");
		}
		(bool, int, int) lineBreaksAndIndentation = GetLineBreaksAndIndentation(result);
		var (useTriviaAsItIs, _, _) = lineBreaksAndIndentation;
		return GetWhitespaceTriviaData(lineBreaksAndIndentation.Item2, lineBreaksAndIndentation.Item3, useTriviaAsItIs, result.TreatAsElastic);
	}

	private int CalculateSpaces(SyntaxToken token1, SyntaxToken token2)
	{
		int initialColumn = ((token1.Kind != SyntaxKind.None) ? (TreeInfo.GetOriginalColumn(OptionSet.GetOption(FormattingOptions.TabSize, "AL"), token1) + token1.Span.Length) : 0);
		string textBetween = TreeInfo.GetTextBetween(token1, token2);
		return textBetween.ConvertTabToSpace(OptionSet.GetOption(FormattingOptions.TabSize, "AL"), initialColumn, textBetween.Length);
	}

	private (bool, int, int) GetLineBreaksAndIndentation(Analyzer.AnalysisResult result)
	{
		int item = result.Tab * OptionSet.GetOption(FormattingOptions.TabSize, "AL") + result.Space;
		if (result.HasTrailingSpace || result.HasUnknownWhitespace)
		{
			return ValueTuple.Create(item1: false, result.LineBreaks, item);
		}
		if (!OptionSet.GetOption(FormattingOptions.UseTabs, "AL"))
		{
			if (result.Tab > 0)
			{
				return ValueTuple.Create(item1: false, result.LineBreaks, item);
			}
			return ValueTuple.Create(item1: true, result.LineBreaks, item);
		}
		if (result.HasTabAfterSpace)
		{
			return ValueTuple.Create(item1: false, result.LineBreaks, item);
		}
		if (result.Space >= OptionSet.GetOption(FormattingOptions.TabSize, "AL"))
		{
			return ValueTuple.Create(item1: false, result.LineBreaks, item);
		}
		return ValueTuple.Create(item1: true, result.LineBreaks, item);
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private int GetSpaceOnSingleLine(Analyzer.AnalysisResult result)
	{
		if (result.HasTrailingSpace || result.HasUnknownWhitespace || result.LineBreaks > 0 || result.Tab > 0)
		{
			return -1;
		}
		return result.Space;
	}
}
