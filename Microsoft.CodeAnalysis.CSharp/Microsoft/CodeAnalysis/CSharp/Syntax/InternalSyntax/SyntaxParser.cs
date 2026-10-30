using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class SyntaxParser : IDisposable
{
	protected struct ResetPoint
	{
		internal readonly int ResetCount;

		internal readonly LexerMode Mode;

		internal readonly int Position;

		internal readonly GreenNode PrevTokenTrailingTrivia;

		internal ResetPoint(int resetCount, LexerMode mode, int position, GreenNode prevTokenTrailingTrivia)
		{
			ResetCount = resetCount;
			Mode = mode;
			Position = position;
			PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
		}
	}

	protected readonly Lexer lexer;

	private readonly bool _isIncremental;

	private readonly bool _allowModeReset;

	protected readonly CancellationToken cancellationToken;

	private LexerMode _mode;

	private Blender _firstBlender;

	private BlendedNode _currentNode;

	private SyntaxToken _currentToken;

	private ArrayElement<SyntaxToken>[] _lexedTokens;

	private GreenNode _prevTokenTrailingTrivia;

	private int _firstToken;

	private int _tokenOffset;

	private int _tokenCount;

	private int _resetCount;

	private int _resetStart;

	private static readonly ObjectPool<BlendedNode[]> s_blendedNodesPool = new ObjectPool<BlendedNode[]>(() => new BlendedNode[32], 2);

	private BlendedNode[] _blendedTokens;

	protected bool IsIncremental => _isIncremental;

	public CSharpParseOptions Options => lexer.Options;

	public bool IsScript => Options.Kind == SourceCodeKind.Script;

	protected LexerMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if (_mode != value)
			{
				_mode = value;
				_currentToken = null;
				_currentNode = default;
				_tokenCount = _tokenOffset;
			}
		}
	}

	protected Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode CurrentNode
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node = _currentNode.Node;
			if (node != null)
			{
				return node;
			}
			ReadCurrentNode();
			return _currentNode.Node;
		}
	}

	protected SyntaxKind CurrentNodeKind => CurrentNode?.Kind() ?? SyntaxKind.None;

	protected SyntaxToken CurrentToken => _currentToken ?? (_currentToken = FetchCurrentToken());

	internal DirectiveStack Directives => lexer.Directives;

	private int CurrentTokenPosition => _firstToken + _tokenOffset;

	protected SyntaxParser(Lexer lexer, LexerMode mode, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, IEnumerable<TextChangeRange> changes, bool allowModeReset, bool preLexIfNotIncremental = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		this.lexer = lexer;
		_mode = mode;
		_allowModeReset = allowModeReset;
		this.cancellationToken = cancellationToken;
		_currentNode = default;
		_isIncremental = oldTree != null;
		if (IsIncremental | allowModeReset)
		{
			_firstBlender = new Blender(lexer, oldTree, changes);
			_blendedTokens = s_blendedNodesPool.Allocate();
		}
		else
		{
			_firstBlender = default;
			_lexedTokens = new ArrayElement<SyntaxToken>[32];
		}
		if (preLexIfNotIncremental && !IsIncremental && !cancellationToken.CanBeCanceled)
		{
			PreLex();
		}
	}

	public void Dispose()
	{
		BlendedNode[] blendedTokens = _blendedTokens;
		if (blendedTokens != null)
		{
			_blendedTokens = null;
			if (blendedTokens.Length < 4096)
			{
				Array.Clear(blendedTokens, 0, blendedTokens.Length);
				s_blendedNodesPool.Free(blendedTokens);
			}
		}
	}

	protected void ReInitialize()
	{
		_firstToken = 0;
		_tokenOffset = 0;
		_tokenCount = 0;
		_resetCount = 0;
		_resetStart = 0;
		_currentToken = null;
		_prevTokenTrailingTrivia = null;
		if (IsIncremental || _allowModeReset)
		{
			_firstBlender = new Blender(lexer, null, null);
		}
	}

	private void PreLex()
	{
		int num = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
		_lexedTokens = new ArrayElement<SyntaxToken>[num];
		Lexer lexer = this.lexer;
		LexerMode mode = _mode;
		for (int i = 0; i < num; i++)
		{
			SyntaxToken syntaxToken = lexer.Lex(mode);
			AddLexedToken(syntaxToken);
			if (syntaxToken.Kind == SyntaxKind.EndOfFileToken)
			{
				break;
			}
		}
	}

	protected ResetPoint GetResetPoint()
	{
		int currentTokenPosition = CurrentTokenPosition;
		if (_resetCount == 0)
		{
			_resetStart = currentTokenPosition;
		}
		_resetCount++;
		return new ResetPoint(_resetCount, _mode, currentTokenPosition, _prevTokenTrailingTrivia);
	}

	protected void Reset(ref ResetPoint point)
	{
		_mode = point.Mode;
		int tokenOffset = point.Position - _firstToken;
		_tokenOffset = tokenOffset;
		_currentToken = null;
		_currentNode = default;
		_prevTokenTrailingTrivia = point.PrevTokenTrailingTrivia;
		if (_blendedTokens == null)
		{
			return;
		}
		for (int i = _tokenOffset; i < _tokenCount; i++)
		{
			if (_blendedTokens[i].Token == null)
			{
				_tokenCount = i;
				if (_tokenCount == _tokenOffset)
				{
					FetchCurrentToken();
				}
				break;
			}
		}
	}

	protected void Release(ref ResetPoint point)
	{
		_resetCount--;
		if (_resetCount == 0)
		{
			_resetStart = -1;
		}
	}

	private void ReadCurrentNode()
	{
		if (_tokenOffset == 0)
		{
			_currentNode = _firstBlender.ReadNode(_mode);
		}
		else
		{
			_currentNode = _blendedTokens[_tokenOffset - 1].Blender.ReadNode(_mode);
		}
	}

	protected GreenNode EatNode()
	{
		GreenNode green = CurrentNode.Green;
		if (_tokenOffset >= _blendedTokens.Length)
		{
			AddTokenSlot();
		}
		_blendedTokens[_tokenOffset++] = _currentNode;
		_tokenCount = _tokenOffset;
		_currentNode = default;
		_currentToken = null;
		return green;
	}

	private SyntaxToken FetchCurrentToken()
	{
		if (_tokenOffset >= _tokenCount)
		{
			AddNewToken();
		}
		if (_blendedTokens != null)
		{
			return _blendedTokens[_tokenOffset].Token;
		}
		return _lexedTokens[_tokenOffset];
	}

	private void AddNewToken()
	{
		if (_blendedTokens != null)
		{
			if (_tokenCount > 0)
			{
				AddToken(_blendedTokens[_tokenCount - 1].Blender.ReadToken(_mode));
			}
			else if (_currentNode.Token != null)
			{
				AddToken(in _currentNode);
			}
			else
			{
				AddToken(_firstBlender.ReadToken(_mode));
			}
		}
		else
		{
			AddLexedToken(lexer.Lex(_mode));
		}
	}

	private void AddToken(in BlendedNode tokenResult)
	{
		if (_tokenCount >= _blendedTokens.Length)
		{
			AddTokenSlot();
		}
		_blendedTokens[_tokenCount] = tokenResult;
		_tokenCount++;
	}

	private void AddLexedToken(SyntaxToken token)
	{
		if (_tokenCount >= _lexedTokens.Length)
		{
			AddLexedTokenSlot();
		}
		_lexedTokens[_tokenCount].Value = token;
		_tokenCount++;
	}

	private void AddTokenSlot()
	{
		if (_tokenOffset > _blendedTokens.Length >> 1 && (_resetStart == -1 || _resetStart > _firstToken))
		{
			int num = ((_resetStart == -1) ? _tokenOffset : (_resetStart - _firstToken));
			int num2 = _tokenCount - num;
			_firstBlender = _blendedTokens[num - 1].Blender;
			if (num2 > 0)
			{
				Array.Copy(_blendedTokens, num, _blendedTokens, 0, num2);
			}
			_firstToken += num;
			_tokenCount -= num;
			_tokenOffset -= num;
		}
		else
		{
			_ = _blendedTokens;
			Array.Resize(ref _blendedTokens, _blendedTokens.Length * 2);
		}
	}

	private void AddLexedTokenSlot()
	{
		if (_tokenOffset > _lexedTokens.Length >> 1 && (_resetStart == -1 || _resetStart > _firstToken))
		{
			int num = ((_resetStart == -1) ? _tokenOffset : (_resetStart - _firstToken));
			int num2 = _tokenCount - num;
			if (num2 > 0)
			{
				Array.Copy(_lexedTokens, num, _lexedTokens, 0, num2);
			}
			_firstToken += num;
			_tokenCount -= num;
			_tokenOffset -= num;
		}
		else
		{
			ArrayElement<SyntaxToken>[] array = new ArrayElement<SyntaxToken>[_lexedTokens.Length * 2];
			Array.Copy(_lexedTokens, array, _lexedTokens.Length);
			_lexedTokens = array;
		}
	}

	protected SyntaxToken PeekToken(int n)
	{
		while (_tokenOffset + n >= _tokenCount)
		{
			AddNewToken();
		}
		if (_blendedTokens != null)
		{
			return _blendedTokens[_tokenOffset + n].Token;
		}
		return _lexedTokens[_tokenOffset + n];
	}

	protected SyntaxToken EatToken()
	{
		SyntaxToken currentToken = CurrentToken;
		MoveToNextToken();
		return currentToken;
	}

	private void MoveToNextToken()
	{
		_prevTokenTrailingTrivia = _currentToken.GetTrailingTrivia();
		_currentToken = null;
		if (_blendedTokens != null)
		{
			_currentNode = default;
		}
		_tokenOffset++;
	}

	protected void ForceEndOfFile()
	{
		_currentToken = SyntaxFactory.Token(SyntaxKind.EndOfFileToken);
	}

	protected SyntaxToken EatToken(SyntaxKind kind)
	{
		SyntaxToken currentToken = CurrentToken;
		if (currentToken.Kind == kind)
		{
			MoveToNextToken();
			return currentToken;
		}
		return CreateMissingToken(kind, CurrentToken.Kind, reportError: true);
	}

	protected SyntaxToken EatTokenAsKind(SyntaxKind expected)
	{
		SyntaxToken currentToken = CurrentToken;
		if (currentToken.Kind == expected)
		{
			MoveToNextToken();
			return currentToken;
		}
		SyntaxToken node = CreateMissingToken(expected, CurrentToken.Kind, reportError: true);
		return AddTrailingSkippedSyntax(node, EatToken());
	}

	private SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
	{
		SyntaxToken syntaxToken = SyntaxFactory.MissingToken(expected);
		if (reportError)
		{
			syntaxToken = WithAdditionalDiagnostics(syntaxToken, GetExpectedTokenError(expected, actual));
		}
		return syntaxToken;
	}

	private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
	{
		SyntaxToken syntaxToken = SyntaxFactory.MissingToken(expected);
		if (reportError)
		{
			syntaxToken = AddError(syntaxToken, code);
		}
		return syntaxToken;
	}

	protected SyntaxToken EatToken(SyntaxKind kind, bool reportError)
	{
		if (reportError)
		{
			return EatToken(kind);
		}
		if (CurrentToken.Kind != kind)
		{
			return SyntaxFactory.MissingToken(kind);
		}
		return EatToken();
	}

	protected SyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
	{
		if (CurrentToken.Kind != kind)
		{
			return CreateMissingToken(kind, code, reportError);
		}
		return EatToken();
	}

	protected SyntaxToken EatTokenWithPrejudice(SyntaxKind kind)
	{
		SyntaxToken syntaxToken = CurrentToken;
		if (syntaxToken.Kind != kind)
		{
			syntaxToken = WithAdditionalDiagnostics(syntaxToken, GetExpectedTokenError(kind, syntaxToken.Kind));
		}
		MoveToNextToken();
		return syntaxToken;
	}

	protected SyntaxToken EatTokenWithPrejudice(ErrorCode errorCode, params object[] args)
	{
		SyntaxToken syntaxToken = EatToken();
		return WithAdditionalDiagnostics(syntaxToken, MakeError(syntaxToken.GetLeadingTriviaWidth(), syntaxToken.Width, errorCode, args));
	}

	protected SyntaxToken EatContextualToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
	{
		if (CurrentToken.ContextualKind != kind)
		{
			return CreateMissingToken(kind, code, reportError);
		}
		return ConvertToKeyword(EatToken());
	}

	protected SyntaxToken EatContextualToken(SyntaxKind kind, bool reportError = true)
	{
		SyntaxKind contextualKind = CurrentToken.ContextualKind;
		if (contextualKind != kind)
		{
			return CreateMissingToken(kind, contextualKind, reportError);
		}
		return ConvertToKeyword(EatToken());
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
	{
		ErrorCode expectedTokenErrorCode = GetExpectedTokenErrorCode(expected, actual);
		if (expectedTokenErrorCode == ErrorCode.ERR_SyntaxError || expectedTokenErrorCode == ErrorCode.ERR_IdentifierExpectedKW)
		{
			return new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode, SyntaxFacts.GetText(expected), SyntaxFacts.GetText(actual));
		}
		return new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode);
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
	{
		GetDiagnosticSpanForMissingToken(out var offset, out var width);
		return GetExpectedTokenError(expected, actual, offset, width);
	}

	private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual)
	{
		switch (expected)
		{
		case SyntaxKind.IdentifierToken:
			if (SyntaxFacts.IsReservedKeyword(actual))
			{
				return ErrorCode.ERR_IdentifierExpectedKW;
			}
			return ErrorCode.ERR_IdentifierExpected;
		case SyntaxKind.SemicolonToken:
			return ErrorCode.ERR_SemicolonExpected;
		case SyntaxKind.CloseParenToken:
			return ErrorCode.ERR_CloseParenExpected;
		case SyntaxKind.OpenBraceToken:
			return ErrorCode.ERR_LbraceExpected;
		case SyntaxKind.CloseBraceToken:
			return ErrorCode.ERR_RbraceExpected;
		default:
			return ErrorCode.ERR_SyntaxError;
		}
	}

	protected void GetDiagnosticSpanForMissingToken(out int offset, out int width)
	{
		GreenNode prevTokenTrailingTrivia = _prevTokenTrailingTrivia;
		if (prevTokenTrailingTrivia != null && new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(prevTokenTrailingTrivia).Any(8539))
		{
			offset = -prevTokenTrailingTrivia.FullWidth;
			width = 0;
		}
		else
		{
			SyntaxToken currentToken = CurrentToken;
			offset = currentToken.GetLeadingTriviaWidth();
			width = currentToken.Width;
		}
	}

	protected virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics) where TNode : GreenNode
	{
		DiagnosticInfo[] diagnostics2 = node.GetDiagnostics();
		int num = diagnostics2.Length;
		if (num == 0)
		{
			return node.WithDiagnosticsGreen(diagnostics);
		}
		DiagnosticInfo[] array = new DiagnosticInfo[diagnostics2.Length + diagnostics.Length];
		diagnostics2.CopyTo(array, 0);
		diagnostics.CopyTo(array, num);
		return node.WithDiagnosticsGreen(array);
	}

	protected TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : GreenNode
	{
		return AddError(node, code, Array.Empty<object>());
	}

	protected TNode AddError<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : GreenNode
	{
		if (!node.IsMissing)
		{
			return WithAdditionalDiagnostics(node, MakeError(node, code, args));
		}
		int offset;
		int width;
		if (node is SyntaxToken { ContainsSkippedText: not false } syntaxToken)
		{
			offset = syntaxToken.GetLeadingTriviaWidth();
			width = 0;
			bool flag = false;
			foreach (CSharpSyntaxNode trailingTrivium in syntaxToken.TrailingTrivia)
			{
				if (trailingTrivium.Kind == SyntaxKind.SkippedTokensTrivia)
				{
					flag = true;
					width += trailingTrivium.Width;
					continue;
				}
				if (flag)
				{
					break;
				}
				offset += trailingTrivium.Width;
			}
		}
		else
		{
			GetDiagnosticSpanForMissingToken(out offset, out width);
		}
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
	}

	protected TNode AddError<TNode>(TNode node, int offset, int length, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		return WithAdditionalDiagnostics(node, MakeError(offset, length, code, args));
	}

	protected TNode AddError<TNode>(TNode node, CSharpSyntaxNode location, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		FindOffset(node, location, out var offset);
		return WithAdditionalDiagnostics(node, MakeError(offset, location.Width, code, args));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : CSharpSyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code, args));
	}

	protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : CSharpSyntaxNode
	{
		GetOffsetAndWidthForLastToken(node, out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code));
	}

	protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		GetOffsetAndWidthForLastToken(node, out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
	}

	private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : CSharpSyntaxNode
	{
		SyntaxToken lastNonmissingToken = node.GetLastNonmissingToken();
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

	protected static SyntaxDiagnosticInfo MakeError(GreenNode node, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(node.GetLeadingTriviaWidth(), node.Width, code, args);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	protected TNode AddLeadingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : CSharpSyntaxNode
	{
		SyntaxToken syntaxToken = (node as SyntaxToken) ?? node.GetFirstToken();
		SyntaxToken newToken = AddSkippedSyntax(syntaxToken, skippedSyntax, trailing: false);
		return SyntaxFirstTokenReplacer.Replace(node, syntaxToken, newToken, skippedSyntax.FullWidth);
	}

	protected void AddTrailingSkippedSyntax(SyntaxListBuilder list, GreenNode skippedSyntax)
	{
		list[list.Count - 1] = AddTrailingSkippedSyntax((CSharpSyntaxNode)list[list.Count - 1], skippedSyntax);
	}

	protected void AddTrailingSkippedSyntax<TNode>(SyntaxListBuilder<TNode> list, GreenNode skippedSyntax) where TNode : CSharpSyntaxNode
	{
		list[list.Count - 1] = AddTrailingSkippedSyntax(list[list.Count - 1], skippedSyntax);
	}

	protected TNode AddTrailingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : CSharpSyntaxNode
	{
		if (node is SyntaxToken target)
		{
			return (TNode)(CSharpSyntaxNode)AddSkippedSyntax(target, skippedSyntax, trailing: true);
		}
		SyntaxToken lastToken = node.GetLastToken();
		SyntaxToken newToken = AddSkippedSyntax(lastToken, skippedSyntax, trailing: true);
		return SyntaxLastTokenReplacer.Replace(node, newToken);
	}

	internal SyntaxToken AddSkippedSyntax(SyntaxToken target, GreenNode skippedSyntax, bool trailing)
	{
		SyntaxListBuilder syntaxListBuilder = new SyntaxListBuilder(4);
		SyntaxDiagnosticInfo syntaxDiagnosticInfo = null;
		int num = 0;
		int num2 = 0;
		foreach (GreenNode item in skippedSyntax.EnumerateNodes())
		{
			if (item is SyntaxToken syntaxToken)
			{
				syntaxListBuilder.Add(syntaxToken.GetLeadingTrivia());
				if (syntaxToken.Width > 0)
				{
					SyntaxToken syntaxToken2 = syntaxToken.TokenWithLeadingTrivia(null).TokenWithTrailingTrivia(null);
					int leadingTriviaWidth = syntaxToken.GetLeadingTriviaWidth();
					if (leadingTriviaWidth > 0)
					{
						DiagnosticInfo[] diagnostics = syntaxToken2.GetDiagnostics();
						for (int i = 0; i < diagnostics.Length; i++)
						{
							SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = (SyntaxDiagnosticInfo)diagnostics[i];
							diagnostics[i] = new SyntaxDiagnosticInfo(syntaxDiagnosticInfo2.Offset - leadingTriviaWidth, syntaxDiagnosticInfo2.Width, (ErrorCode)syntaxDiagnosticInfo2.Code, syntaxDiagnosticInfo2.Arguments);
						}
					}
					syntaxListBuilder.Add(SyntaxFactory.SkippedTokensTrivia(syntaxToken2));
				}
				else
				{
					SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = (SyntaxDiagnosticInfo)syntaxToken.GetDiagnostics().FirstOrDefault();
					if (syntaxDiagnosticInfo3 != null)
					{
						syntaxDiagnosticInfo = syntaxDiagnosticInfo3;
						num = num2;
					}
				}
				syntaxListBuilder.Add(syntaxToken.GetTrailingTrivia());
				num2 += syntaxToken.FullWidth;
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
		GreenNode greenNode = syntaxListBuilder.ToListNode();
		int num4;
		if (trailing)
		{
			GreenNode trailingTrivia = target.GetTrailingTrivia();
			num4 = target.FullWidth;
			target = target.TokenWithTrailingTrivia(SyntaxList.Concat(trailingTrivia, greenNode));
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
			GreenNode leadingTrivia = target.GetLeadingTrivia();
			target = target.TokenWithLeadingTrivia(SyntaxList.Concat(greenNode, leadingTrivia));
			num4 = 0;
		}
		if (syntaxDiagnosticInfo != null)
		{
			int offset = num4 + num + syntaxDiagnosticInfo.Offset;
			target = WithAdditionalDiagnostics(target, new SyntaxDiagnosticInfo(offset, syntaxDiagnosticInfo.Width, (ErrorCode)syntaxDiagnosticInfo.Code, syntaxDiagnosticInfo.Arguments));
		}
		return target;
	}

	private bool FindOffset(GreenNode root, CSharpSyntaxNode location, out int offset)
	{
		int num = 0;
		offset = 0;
		if (root != null)
		{
			int i = 0;
			for (int slotCount = root.SlotCount; i < slotCount; i++)
			{
				GreenNode slot = root.GetSlot(i);
				if (slot != null)
				{
					if (slot == location)
					{
						offset = num;
						return true;
					}
					if (FindOffset(slot, location, out offset))
					{
						offset += slot.GetLeadingTriviaWidth() + num;
						return true;
					}
					num += slot.FullWidth;
				}
			}
		}
		return false;
	}

	protected static SyntaxToken ConvertToKeyword(SyntaxToken token)
	{
		if (token.Kind != token.ContextualKind)
		{
			SyntaxToken syntaxToken = (token.IsMissing ? SyntaxFactory.MissingToken(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node) : SyntaxFactory.Token(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node));
			DiagnosticInfo[] diagnostics = token.GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				syntaxToken = syntaxToken.WithDiagnosticsGreen(diagnostics);
			}
			return syntaxToken;
		}
		return token;
	}

	protected TNode CheckFeatureAvailability<TNode>(TNode node, MessageID feature, bool forceWarning = false) where TNode : GreenNode
	{
		LanguageVersion languageVersion = Options.LanguageVersion;
		if (feature == MessageID.IDS_FeatureModuleAttrLoc)
		{
			if (languageVersion < LanguageVersion.CSharp2)
			{
				return AddError(node, ErrorCode.WRN_NonECMAFeature, feature.Localize());
			}
			return node;
		}
		if (IsFeatureEnabled(feature))
		{
			return node;
		}
		LocalizableErrorArgument localizableErrorArgument = feature.Localize();
		LanguageVersion version = feature.RequiredVersion();
		if (forceWarning)
		{
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = new SyntaxDiagnosticInfo(languageVersion.GetErrorCode(), localizableErrorArgument, new CSharpRequiredLanguageVersion(version));
			return AddError(node, ErrorCode.WRN_ErrorOverride, syntaxDiagnosticInfo, syntaxDiagnosticInfo.Code);
		}
		return AddError(node, languageVersion.GetErrorCode(), localizableErrorArgument, new CSharpRequiredLanguageVersion(version));
	}

	protected bool IsFeatureEnabled(MessageID feature)
	{
		return Options.IsFeatureEnabled(feature);
	}

	protected bool IsMakingProgress(ref int lastTokenPosition)
	{
		int currentTokenPosition = CurrentTokenPosition;
		if (currentTokenPosition > lastTokenPosition)
		{
			lastTokenPosition = currentTokenPosition;
			return true;
		}
		return false;
	}
}
