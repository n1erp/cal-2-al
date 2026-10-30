using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal abstract class ParserBase
{
	internal enum PostSkipAction
	{
		Continue,
		Abort
	}

	private readonly Lexer lexer;

	private Blender firstBlender;

	private BlendedNode currentNode;

	private InternalSyntaxToken currentToken;

	private ArrayElement<InternalSyntaxToken>[] lexedTokens;

	private int tokenOffset;

	private int tokenCount;

	private LexerMode lexerMode;

	private BlendedNode[] blendedTokens;

	private const int InitialTokenArraySize = 32;

	internal LexerMode LexerMode
	{
		get
		{
			return lexerMode;
		}
		set
		{
			lexerMode = value;
			ContextAwareSyntaxFactory.LexerMode = value;
		}
	}

	protected ContextAwareSyntaxFactory ContextAwareSyntaxFactory { get; }

	private bool ForceLex
	{
		get
		{
			if (blendedTokens != null && tokenOffset < blendedTokens.Length)
			{
				LexerMode lexerMode = blendedTokens[tokenOffset].Blender.LexerMode;
				if (lexerMode != LexerMode.Default && LexerMode != LexerMode.Default && lexerMode != 0)
				{
					return lexerMode != LexerMode;
				}
				return false;
			}
			return false;
		}
	}

	protected internal InternalSyntaxListPool Pool { get; } = new InternalSyntaxListPool();

	protected CancellationToken CancellationToken { get; }

	internal bool IsIncremental { get; }

	protected internal bool IsEndOfFile => CurrentToken.Kind == SyntaxKind.EndOfFileToken;

	protected SyntaxNode CurrentNode
	{
		get
		{
			DebugAssertHelper.Assert(blendedTokens != null);
			SyntaxNode node = currentNode.Node;
			if (node != null)
			{
				return node;
			}
			ReadCurrentNode();
			return currentNode.Node;
		}
	}

	protected SyntaxKind CurrentNodeKind => CurrentNode?.Kind ?? SyntaxKind.None;

	internal InternalSyntaxToken CurrentToken => currentToken ?? (currentToken = FetchCurrentToken());

	protected ParserBase(Lexer lexer, LexerMode mode, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes, bool allowModeReset, bool preLexIfNotIncremental = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		ContextAwareSyntaxFactory = new ContextAwareSyntaxFactory();
		CancellationToken = cancellationToken;
		this.lexer = lexer;
		LexerMode = mode;
		currentNode = default;
		IsIncremental = oldTree != null;
		if (IsIncremental | allowModeReset)
		{
			firstBlender = new Blender(lexer, oldTree, changes);
			blendedTokens = new BlendedNode[32];
		}
		else
		{
			firstBlender = default;
			lexedTokens = new ArrayElement<InternalSyntaxToken>[32];
		}
		if (preLexIfNotIncremental && !IsIncremental && !cancellationToken.CanBeCanceled)
		{
			PreLex();
		}
	}

	private void PreLex()
	{
		int num = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
		lexedTokens = new ArrayElement<InternalSyntaxToken>[num];
		Lexer lexer = this.lexer;
		LexerMode lexerMode = LexerMode;
		for (int i = 0; i < num; i++)
		{
			InternalSyntaxToken internalSyntaxToken = lexer.Lex(lexerMode);
			AddLexedToken(internalSyntaxToken);
			if (internalSyntaxToken.Kind == SyntaxKind.EndOfFileToken)
			{
				break;
			}
		}
	}

	private void AddLexedToken(InternalSyntaxToken token)
	{
		DebugAssertHelper.Assert(token != null);
		if (tokenCount >= lexedTokens.Length)
		{
			AddLexedTokenSlot();
		}
		lexedTokens[tokenCount].Value = token;
		tokenCount++;
	}

	private void AddLexedTokenSlot()
	{
		if (tokenOffset > lexedTokens.Length >> 1)
		{
			int num = tokenOffset;
			int num2 = tokenCount - num;
			DebugAssertHelper.Assert(num > 0);
			if (num2 > 0)
			{
				Array.Copy(lexedTokens, num, lexedTokens, 0, num2);
			}
			tokenCount -= num;
			tokenOffset -= num;
		}
		else
		{
			ArrayElement<InternalSyntaxToken>[] destinationArray = new ArrayElement<InternalSyntaxToken>[lexedTokens.Length * 2];
			Array.Copy(lexedTokens, destinationArray, lexedTokens.Length);
			lexedTokens = destinationArray;
		}
	}

	protected internal bool IsTerminator()
	{
		if (IsEndOfFile)
		{
			return true;
		}
		return false;
	}

	private void ReadCurrentNode()
	{
		if (tokenOffset == 0)
		{
			currentNode = firstBlender.ReadNode(LexerMode);
		}
		else
		{
			currentNode = blendedTokens[tokenOffset - 1].Blender.ReadNode(LexerMode, ForceLex);
		}
	}

	protected InternalSyntaxNode EatNode()
	{
		DebugAssertHelper.Assert(blendedTokens != null);
		InternalSyntaxNode node = CurrentNode.Node;
		if (tokenOffset >= blendedTokens.Length)
		{
			AddTokenSlot();
		}
		blendedTokens[tokenOffset++] = currentNode;
		tokenCount = tokenOffset;
		currentNode = default;
		currentToken = null;
		return node;
	}

	private InternalSyntaxToken FetchCurrentToken()
	{
		bool forceLex = ForceLex;
		if ((tokenOffset >= tokenCount) | forceLex)
		{
			AddNewToken(forceLex);
		}
		if (blendedTokens != null)
		{
			return blendedTokens[tokenOffset].Token;
		}
		return lexedTokens[tokenOffset];
	}

	private void AddNewToken(bool forceLex = false)
	{
		if (blendedTokens != null)
		{
			if (tokenCount > 0)
			{
				AddToken(blendedTokens[tokenCount - 1].Blender.ReadToken(LexerMode, forceLex));
			}
			else if (currentNode.Token != null)
			{
				AddToken(currentNode);
			}
			else
			{
				AddToken(firstBlender.ReadToken(LexerMode, forceLex));
			}
		}
		else
		{
			AddLexedToken(lexer.Lex(LexerMode));
		}
	}

	private void AddToken(BlendedNode tokenResult)
	{
		DebugAssertHelper.Assert(tokenResult.Token != null);
		if (tokenCount >= blendedTokens.Length)
		{
			AddTokenSlot();
		}
		blendedTokens[tokenCount] = tokenResult;
		tokenCount++;
	}

	private void AddTokenSlot()
	{
		if (tokenOffset > blendedTokens.Length >> 1)
		{
			int num = tokenOffset;
			int num2 = tokenCount - num;
			DebugAssertHelper.Assert(num > 0);
			firstBlender = blendedTokens[num - 1].Blender;
			if (num2 > 0)
			{
				Array.Copy(blendedTokens, num, blendedTokens, 0, num2);
			}
			tokenCount -= num;
			tokenOffset -= num;
		}
		else
		{
			Array.Resize(ref blendedTokens, blendedTokens.Length * 2);
		}
	}

	protected InternalSyntaxToken EatToken()
	{
		InternalSyntaxToken result = CurrentToken;
		MoveToNextToken();
		return result;
	}

	private void MoveToNextToken()
	{
		currentToken = null;
		if (blendedTokens != null)
		{
			currentNode = default;
		}
		tokenOffset++;
	}

	protected InternalSyntaxToken EatToken(IList<SyntaxKind> kinds, ErrorCode errorCode, params object[] args)
	{
		DebugAssertHelper.Assert(kinds != null && kinds.Count > 0);
		InternalSyntaxToken ct = CurrentToken;
		if (!kinds.Any((SyntaxKind k) => k == ct.Kind))
		{
			return CreateMissingToken(kinds[0], errorCode, args);
		}
		MoveToNextToken();
		return ct;
	}

	protected InternalSyntaxToken EatToken(SyntaxKind kind)
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		if (internalSyntaxToken.Kind == kind)
		{
			MoveToNextToken();
			return internalSyntaxToken;
		}
		return CreateMissingToken(kind, CurrentToken.Kind, reportError: true);
	}

	protected InternalSyntaxToken EatToken(SyntaxKind kind, bool reportError)
	{
		if (reportError)
		{
			return EatToken(kind);
		}
		if (CurrentToken.Kind != kind)
		{
			return MissingToken(kind);
		}
		return EatToken();
	}

	protected InternalSyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
	{
		DebugAssertHelper.Assert(kind.IsAnyToken());
		if (CurrentToken.Kind != kind)
		{
			return CreateMissingToken(kind, code, reportError);
		}
		return EatToken();
	}

	protected InternalSyntaxToken EatTokenWithPrejudice(SyntaxKind expectedKind, bool checkAllowedIdentifiers = false)
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		DebugAssertHelper.Assert(expectedKind.IsAnyToken());
		if (internalSyntaxToken.Kind != expectedKind && (!checkAllowedIdentifiers || !expectedKind.IsTokenIdentifier() || !internalSyntaxToken.Kind.IsTokenIdentifier()))
		{
			internalSyntaxToken = WithAdditionalDiagnostics(internalSyntaxToken, GetExpectedTokenError(expectedKind, internalSyntaxToken.Kind));
		}
		MoveToNextToken();
		return internalSyntaxToken;
	}

	protected InternalSyntaxToken EatTokenWithPrejudice(ErrorCode errorCode, params object[] args)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken();
		return WithAdditionalDiagnostics(internalSyntaxToken, MakeError(internalSyntaxToken.GetLeadingTriviaWidth(), internalSyntaxToken.Width, errorCode, args));
	}

	protected InternalSyntaxToken EatTokenIfKind(SyntaxKind kind)
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		if (internalSyntaxToken.Kind == kind)
		{
			MoveToNextToken();
			return internalSyntaxToken;
		}
		return null;
	}

	protected InternalSyntaxToken EatTokenIfCondition(Func<SyntaxKind, bool> condition)
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		if (condition(internalSyntaxToken.Kind))
		{
			MoveToNextToken();
			return internalSyntaxToken;
		}
		return null;
	}

	internal static InternalSyntaxToken MissingToken(SyntaxKind kind)
	{
		return InternalSyntaxToken.CreateMissing(kind, null, null);
	}

	protected internal InternalSyntaxToken ParseIdentifierToken(bool allowMissingIdentifier = false)
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind.IsTokenIdentifier())
		{
			InternalSyntaxToken internalSyntaxToken = EatToken();
			if (kind != SyntaxKind.IdentifierToken)
			{
				internalSyntaxToken = InternalSyntaxFactory.Identifier(internalSyntaxToken);
			}
			return internalSyntaxToken;
		}
		if (allowMissingIdentifier)
		{
			return null;
		}
		InternalSyntaxToken node = CreateMissingIdentifierToken();
		return AddError(node, ErrorCode.ERR_IdentifierExpected);
	}

	internal virtual bool IsTokenIdentifier(SyntaxKind kind)
	{
		return SyntaxKind.IdentifierToken == kind;
	}

	protected InternalSyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
	{
		InternalSyntaxToken internalSyntaxToken = InternalSyntaxFactory.MissingToken(expected);
		if (reportError)
		{
			internalSyntaxToken = WithAdditionalDiagnostics(internalSyntaxToken, GetExpectedTokenError(expected, actual));
		}
		return internalSyntaxToken;
	}

	protected static InternalIdentifierNameSyntax CreateMissingIdentifierName()
	{
		return InternalSyntaxFactory.IdentifierName(CreateMissingIdentifierToken());
	}

	protected static InternalSyntaxToken CreateMissingIdentifierToken()
	{
		return InternalSyntaxFactory.MissingToken(SyntaxKind.IdentifierToken);
	}

	protected static InternalSyntaxToken CreateMissingStringLiteralToken()
	{
		return InternalSyntaxFactory.MissingToken(SyntaxKind.StringLiteralToken);
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
	{
		ErrorCode expectedTokenErrorCode = GetExpectedTokenErrorCode(expected, actual);
		if (expectedTokenErrorCode == ErrorCode.ERR_SyntaxError || expectedTokenErrorCode == ErrorCode.ERR_IdentifierExpectedKW)
		{
			return new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode, expected.GetText(), actual.GetText());
		}
		return new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode);
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
	{
		GetDiagnosticSpanForCurrentToken(out var offset, out var width);
		return GetExpectedTokenError(expected, actual, offset, width);
	}

	private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual)
	{
		switch (expected)
		{
		case SyntaxKind.IdentifierToken:
			if (actual.IsKeyword())
			{
				return ErrorCode.ERR_IdentifierExpectedKW;
			}
			return ErrorCode.ERR_IdentifierExpected;
		case SyntaxKind.Int32LiteralToken:
			return ErrorCode.ERR_IntegerLiteralExpected;
		case SyntaxKind.StringLiteralToken:
			return ErrorCode.ERR_ExpectedStringLiteral;
		default:
			return ErrorCode.ERR_SyntaxError;
		}
	}

	protected internal void GetDiagnosticSpanForCurrentToken(out int offset, out int width)
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		offset = internalSyntaxToken.GetLeadingTriviaWidth();
		width = internalSyntaxToken.Width;
	}

	protected PostSkipAction SkipBadSeparatedListTokensWithExpectedKind<T, TNode>(ref T startToken, InternalSeparatedSyntaxListBuilder<TNode> list, Func<ParserBase, bool> isNotExpectedFunction, Func<ParserBase, bool> abortFunction, SyntaxKind expected) where T : InternalSyntaxNode where TNode : InternalSyntaxNode
	{
		PostSkipAction result = SkipBadListTokensWithExpectedKindHelper(list.UnderlyingBuilder, isNotExpectedFunction, abortFunction, expected, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
		}
		return result;
	}

	protected PostSkipAction SkipBadListTokensWithExpectedKindHelper(InternalSyntaxListBuilder list, Func<ParserBase, bool> isNotExpectedFunction, Func<ParserBase, bool> abortFunction, SyntaxKind expected, out InternalSyntaxNode trailingTrivia)
	{
		if (list.Count == 0)
		{
			return SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, out trailingTrivia);
		}
		PostSkipAction result = SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, out var trailingTrivia2);
		if (trailingTrivia2 != null)
		{
			list[list.Count - 1] = AddTrailingSkippedSyntax(list[list.Count - 1], trailingTrivia2);
		}
		trailingTrivia = null;
		return result;
	}

	protected internal PostSkipAction SkipBadTokensWithExpectedKind(Func<ParserBase, bool> isNotExpectedFunction, Func<ParserBase, bool> abortFunction, SyntaxKind expected, out InternalSyntaxNode trailingTrivia)
	{
		InternalSyntaxListBuilder internalSyntaxListBuilder = Pool.Allocate();
		try
		{
			bool flag = true;
			PostSkipAction result = PostSkipAction.Continue;
			while (isNotExpectedFunction(this))
			{
				if (abortFunction(this))
				{
					result = PostSkipAction.Abort;
					break;
				}
				InternalSyntaxToken item = ((flag && !CurrentToken.ContainsDiagnostics) ? EatTokenWithPrejudice(expected, expected == SyntaxKind.IdentifierToken) : EatToken());
				flag = false;
				internalSyntaxListBuilder.Add(item);
			}
			trailingTrivia = ((internalSyntaxListBuilder.Count > 0) ? internalSyntaxListBuilder.ToListNode() : null);
			return result;
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	protected internal void SkipBadTokens(Func<ParserBase, bool> abortFunction, Func<ParserBase, InternalSyntaxToken, InternalSyntaxToken> errorFunction, out InternalSyntaxNode trailingTrivia)
	{
		InternalSyntaxListBuilder internalSyntaxListBuilder = Pool.Allocate();
		try
		{
			while (!abortFunction(this))
			{
				InternalSyntaxToken internalSyntaxToken = EatToken();
				if (internalSyntaxListBuilder.Count == 0 && errorFunction != null)
				{
					internalSyntaxToken = errorFunction(this, internalSyntaxToken);
				}
				internalSyntaxListBuilder.Add(internalSyntaxToken);
			}
			trailingTrivia = ((internalSyntaxListBuilder.Count > 0) ? internalSyntaxListBuilder.ToListNode() : null);
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	protected internal virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics) where TNode : InternalSyntaxNode
	{
		DiagnosticInfo[] diagnostics2 = node.GetDiagnostics();
		int num = diagnostics2.Length;
		if (num == 0)
		{
			return node.WithInternalDiagnostics(diagnostics);
		}
		DiagnosticInfo[] array = new DiagnosticInfo[diagnostics2.Length + diagnostics.Length];
		diagnostics2.CopyTo(array, 0);
		diagnostics.CopyTo(array, num);
		return node.WithInternalDiagnostics(array);
	}

	private InternalSyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
	{
		InternalSyntaxToken internalSyntaxToken = InternalSyntaxFactory.MissingToken(expected);
		if (reportError)
		{
			internalSyntaxToken = AddError(internalSyntaxToken, code);
		}
		return internalSyntaxToken;
	}

	private InternalSyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, params object[] args)
	{
		InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(expected);
		return AddError(node, code, args);
	}

	protected internal InternalSyntaxToken ConvertToMissingWithTrailingTrivia(InternalSyntaxToken token, SyntaxKind expectedKind)
	{
		InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(expectedKind);
		return AddTrailingSkippedSyntax(node, token);
	}

	protected internal TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : InternalSyntaxNode
	{
		return AddError(node, code, SpecializedCollections.EmptyObjects);
	}

	protected internal TNode AddError<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : InternalSyntaxNode
	{
		if (!node.IsMissing)
		{
			return WithAdditionalDiagnostics(node, MakeError(node, code, args));
		}
		GetDiagnosticSpanForCurrentToken(out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : InternalSyntaxNode
	{
		InternalSyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : InternalSyntaxNode
	{
		InternalSyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code, args));
	}

	protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : InternalSyntaxNode
	{
		GetOffsetAndWidthForLastToken(node, out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code));
	}

	protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : InternalSyntaxNode
	{
		GetOffsetAndWidthForLastToken(node, out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
	}

	private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : InternalSyntaxNode
	{
		InternalSyntaxToken lastNonmissingToken = node.GetLastNonmissingToken();
		offset = node.FullWidth;
		width = 0;
		if (lastNonmissingToken != null)
		{
			offset -= lastNonmissingToken.FullWidth;
			offset += lastNonmissingToken.GetLeadingTriviaWidth();
			width += lastNonmissingToken.Width;
		}
	}

	protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(offset, width, code);
	}

	protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(offset, width, code, args);
	}

	protected static SyntaxDiagnosticInfo MakeError(InternalSyntaxNode node, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(node.GetLeadingTriviaWidth(), node.Width, code, args);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	protected TNode AddLeadingSkippedSyntax<TNode>(TNode node, InternalSyntaxNode skippedSyntax) where TNode : InternalSyntaxNode
	{
		if (skippedSyntax == null)
		{
			return node;
		}
		InternalSyntaxToken internalSyntaxToken = (node as InternalSyntaxToken) ?? node.GetFirstToken();
		InternalSyntaxToken newToken = AddSkippedSyntax(internalSyntaxToken, skippedSyntax, trailing: false);
		return InternalSyntaxFirstTokenReplacer.Replace(node, internalSyntaxToken, newToken, skippedSyntax.FullWidth);
	}

	protected internal TNode AddTrailingSkippedSyntax<TNode>(TNode node, InternalSyntaxNode skippedSyntax) where TNode : InternalSyntaxNode
	{
		if (skippedSyntax == null)
		{
			return node;
		}
		if (node is InternalSyntaxToken target)
		{
			return (TNode)(InternalSyntaxNode)AddSkippedSyntax(target, skippedSyntax, trailing: true);
		}
		InternalSyntaxToken lastToken = node.GetLastToken();
		InternalSyntaxToken newToken = AddSkippedSyntax(lastToken, skippedSyntax, trailing: true);
		return InternalSyntaxLastTokenReplacer.Replace(node, newToken);
	}

	internal InternalSyntaxToken AddSkippedSyntax(InternalSyntaxToken target, InternalSyntaxNode skippedSyntax, bool trailing)
	{
		InternalSyntaxListBuilder internalSyntaxListBuilder = new InternalSyntaxListBuilder(4);
		SyntaxDiagnosticInfo syntaxDiagnosticInfo = null;
		int num = 0;
		int num2 = 0;
		foreach (InternalSyntaxNode item in skippedSyntax.EnumerateNodes())
		{
			if (item is InternalSyntaxToken internalSyntaxToken)
			{
				internalSyntaxListBuilder.Add(internalSyntaxToken.GetLeadingTrivia());
				if (internalSyntaxToken.Width > 0)
				{
					InternalSyntaxToken internalSyntaxToken2 = (InternalSyntaxToken)internalSyntaxToken.WithLeadingTrivia(null).WithTrailingTrivia(null);
					int leadingTriviaWidth = internalSyntaxToken.GetLeadingTriviaWidth();
					if (leadingTriviaWidth > 0)
					{
						DiagnosticInfo[] diagnostics = internalSyntaxToken2.GetDiagnostics();
						for (int i = 0; i < diagnostics.Length; i++)
						{
							SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = (SyntaxDiagnosticInfo)diagnostics[i];
							diagnostics[i] = new SyntaxDiagnosticInfo(syntaxDiagnosticInfo2.Offset - leadingTriviaWidth, syntaxDiagnosticInfo2.Width, (ErrorCode)syntaxDiagnosticInfo2.Code, syntaxDiagnosticInfo2.Arguments);
						}
					}
					internalSyntaxListBuilder.Add(InternalSyntaxFactory.SkippedTokensTrivia(internalSyntaxToken2));
				}
				else
				{
					SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = (SyntaxDiagnosticInfo)internalSyntaxToken.GetDiagnostics().FirstOrDefault();
					if (syntaxDiagnosticInfo3 != null)
					{
						syntaxDiagnosticInfo = syntaxDiagnosticInfo3;
						num = num2;
					}
				}
				internalSyntaxListBuilder.Add(internalSyntaxToken.GetTrailingTrivia());
				num2 += internalSyntaxToken.FullWidth;
			}
			else if (item.ContainsDiagnostics && syntaxDiagnosticInfo == null)
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo4 = (SyntaxDiagnosticInfo)item.GetDiagnostics().FirstOrDefault();
				if (syntaxDiagnosticInfo4 != null)
				{
					syntaxDiagnosticInfo = syntaxDiagnosticInfo4;
					num = num2;
				}
			}
		}
		int num3 = num2;
		InternalSyntaxNode internalSyntaxNode = internalSyntaxListBuilder.ToListNode();
		int num4;
		if (trailing)
		{
			InternalSyntaxNode trailingTrivia = target.GetTrailingTrivia();
			num4 = target.FullWidth;
			target = (InternalSyntaxToken)target.WithTrailingTrivia(InternalSyntaxList.Concat(trailingTrivia, internalSyntaxNode));
		}
		else
		{
			if (num3 > 0)
			{
				DiagnosticInfo[] diagnostics2 = target.GetDiagnostics();
				for (int j = 0; j < diagnostics2.Length; j++)
				{
					SyntaxDiagnosticInfo syntaxDiagnosticInfo5 = (SyntaxDiagnosticInfo)diagnostics2[j];
					diagnostics2[j] = new SyntaxDiagnosticInfo(syntaxDiagnosticInfo5.Offset + num3, syntaxDiagnosticInfo5.Width, (ErrorCode)syntaxDiagnosticInfo5.Code, syntaxDiagnosticInfo5.Arguments);
				}
			}
			InternalSyntaxNode leadingTrivia = target.GetLeadingTrivia();
			target = (InternalSyntaxToken)target.WithLeadingTrivia(InternalSyntaxList.Concat(internalSyntaxNode, leadingTrivia));
			num4 = 0;
		}
		if (syntaxDiagnosticInfo != null)
		{
			int offset = num4 + num + syntaxDiagnosticInfo.Offset;
			target = WithAdditionalDiagnostics(target, new SyntaxDiagnosticInfo(offset, syntaxDiagnosticInfo.Width, (ErrorCode)syntaxDiagnosticInfo.Code, syntaxDiagnosticInfo.Arguments));
		}
		return target;
	}

	protected TNode CheckFeatureAvailability<TNode>(TNode node, Feature feature) where TNode : InternalSyntaxNode
	{
		if (IsFeatureEnabled(feature))
		{
			return node;
		}
		string text = feature.Localize();
		Version arg = feature.RequiredVersion();
		return AddError(node, ErrorCode.ERR_FeatureNotAvailable, text, lexer.Options.RuntimeVersion, string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.VersionOrGreater, arg));
	}

	protected bool IsFeatureEnabled(Feature feature)
	{
		return lexer.Options.IsFeatureEnabled(feature);
	}
}
