using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class LanguageParser : SyntaxParser
{
	[Flags]
	internal enum TerminatorState
	{
		EndOfFile = 0,
		IsNamespaceMemberStartOrStop = 1,
		IsAttributeDeclarationTerminator = 2,
		IsPossibleAggregateClauseStartOrStop = 4,
		IsPossibleMemberStartOrStop = 8,
		IsEndOfReturnType = 0x10,
		IsEndOfParameterList = 0x20,
		IsEndOfFieldDeclaration = 0x40,
		IsPossibleEndOfVariableDeclaration = 0x80,
		IsEndOfTypeArgumentList = 0x100,
		IsPossibleStatementStartOrStop = 0x200,
		IsEndOfFixedStatement = 0x400,
		IsEndOfTryBlock = 0x800,
		IsEndOfCatchClause = 0x1000,
		IsEndOfilterClause = 0x2000,
		IsEndOfCatchBlock = 0x4000,
		IsEndOfDoWhileExpression = 0x8000,
		IsEndOfForStatementArgument = 0x10000,
		IsEndOfDeclarationClause = 0x20000,
		IsEndOfArgumentList = 0x40000,
		IsSwitchSectionStart = 0x80000,
		IsEndOfTypeParameterList = 0x100000,
		IsEndOfMethodSignature = 0x200000,
		IsEndOfNameInExplicitInterface = 0x400000
	}

	private struct NamespaceBodyBuilder(SyntaxListPool pool)
	{
		public SyntaxListBuilder<ExternAliasDirectiveSyntax> Externs = pool.Allocate<ExternAliasDirectiveSyntax>();

		public SyntaxListBuilder<UsingDirectiveSyntax> Usings = pool.Allocate<UsingDirectiveSyntax>();

		public SyntaxListBuilder<AttributeListSyntax> Attributes = pool.Allocate<AttributeListSyntax>();

		public SyntaxListBuilder<MemberDeclarationSyntax> Members = pool.Allocate<MemberDeclarationSyntax>();

		internal void Free(SyntaxListPool pool)
		{
			pool.Free(Members);
			pool.Free(Attributes);
			pool.Free(Usings);
			pool.Free(Externs);
		}
	}

	private enum NamespaceParts
	{
		None,
		ExternAliases,
		Usings,
		GlobalAttributes,
		MembersAndStatements
	}

	private enum PostSkipAction
	{
		Continue,
		Abort
	}

	[Flags]
	private enum VariableFlags
	{
		Fixed = 1,
		Const = 2,
		Local = 4
	}

	[Flags]
	private enum NameOptions
	{
		None = 0,
		InExpression = 1,
		InTypeList = 2,
		PossiblePattern = 4,
		AfterIs = 8,
		AfterCase = 0x10,
		AfterOut = 0x20,
		AfterTupleComma = 0x40,
		FirstElementOfPossibleTupleLiteral = 0x80
	}

	private enum ScanTypeArgumentListKind
	{
		NotTypeArgumentList,
		PossibleTypeArgumentList,
		DefiniteTypeArgumentList
	}

	private enum ScanTypeFlags
	{
		NotType,
		MustBeType,
		GenericTypeOrMethod,
		GenericTypeOrExpression,
		NonGenericTypeOrExpression,
		AliasQualifiedName,
		NullableType,
		PointerOrMultiplication,
		TupleType
	}

	private enum ParseTypeMode
	{
		Normal,
		Parameter,
		AfterIs,
		AfterCase,
		AfterOut,
		AfterTupleComma,
		AsExpression,
		ArrayCreation,
		FirstElementOfPossibleTupleLiteral
	}

	private enum Precedence : uint
	{
		Expression = 0u,
		Assignment = 1u,
		Lambda = Assignment,
		Ternary = 2u,
		Coalescing = 3u,
		ConditionalOr = 4u,
		ConditionalAnd = 5u,
		LogicalOr = 6u,
		LogicalXor = 7u,
		LogicalAnd = 8u,
		Equality = 9u,
		Relational = 10u,
		Shift = 11u,
		Additive = 12u,
		Mutiplicative = 13u,
		Unary = 14u,
		Cast = 15u,
		PointerIndirection = 16u,
		AddressOf = 17u,
		Primary_UNUSED = 18u
	}

	private new struct ResetPoint
	{
		internal SyntaxParser.ResetPoint BaseResetPoint;

		internal readonly TerminatorState TerminatorState;

		internal readonly bool IsInTry;

		internal readonly bool IsInAsync;

		internal readonly int QueryDepth;

		internal ResetPoint(SyntaxParser.ResetPoint resetPoint, TerminatorState terminatorState, bool isInTry, bool isInAsync, int queryDepth)
		{
			BaseResetPoint = resetPoint;
			TerminatorState = terminatorState;
			IsInTry = isInTry;
			IsInAsync = isInAsync;
			QueryDepth = queryDepth;
		}
	}

	private readonly SyntaxListPool _pool = new SyntaxListPool();

	private readonly SyntaxFactoryContext _syntaxFactoryContext;

	private readonly ContextAwareSyntax _syntaxFactory;

	private int _recursionDepth;

	private TerminatorState _termState;

	private bool _isInTry;

	private const int LastTerminatorState = 4194304;

	private bool IsCurrentTokenQueryContextualKeyword => IsTokenQueryContextualKeyword(CurrentToken);

	private bool IsStrict => Options.Features.ContainsKey("strict");

	[Obsolete("Use IsIncrementalAndFactoryContextMatches")]
	private new bool IsIncremental
	{
		get
		{
			throw new Exception("Use IsIncrementalAndFactoryContextMatches");
		}
	}

	private bool IsIncrementalAndFactoryContextMatches
	{
		get
		{
			if (!base.IsIncremental)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode currentNode = CurrentNode;
			if (currentNode != null)
			{
				return MatchesFactoryContext(currentNode.Green, _syntaxFactoryContext);
			}
			return false;
		}
	}

	private bool IsInAsync
	{
		get
		{
			return _syntaxFactoryContext.IsInAsync;
		}
		set
		{
			_syntaxFactoryContext.IsInAsync = value;
		}
	}

	private bool IsInQuery => _syntaxFactoryContext.IsInQuery;

	internal LanguageParser(Lexer lexer, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, IEnumerable<TextChangeRange> changes, LexerMode lexerMode = LexerMode.Syntax, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, lexerMode, oldTree, changes, allowModeReset: false, preLexIfNotIncremental: true, cancellationToken)
	{
		_syntaxFactoryContext = new SyntaxFactoryContext();
		_syntaxFactory = new ContextAwareSyntax(_syntaxFactoryContext);
	}

	private static bool IsSomeWord(SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return SyntaxFacts.IsKeywordKind(kind);
		}
		return true;
	}

	private bool IsTerminator()
	{
		if (CurrentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return true;
		}
		for (int num = 1; num <= 4194304; num <<= 1)
		{
			switch ((TerminatorState)((uint)_termState & (uint)num))
			{
			case TerminatorState.IsNamespaceMemberStartOrStop:
				if (IsNamespaceMemberStartOrStop())
				{
					return true;
				}
				break;
			case TerminatorState.IsAttributeDeclarationTerminator:
				if (IsAttributeDeclarationTerminator())
				{
					return true;
				}
				break;
			case TerminatorState.IsPossibleAggregateClauseStartOrStop:
				if (IsPossibleAggregateClauseStartOrStop())
				{
					return true;
				}
				break;
			case TerminatorState.IsPossibleMemberStartOrStop:
				if (IsPossibleMemberStartOrStop())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfReturnType:
				if (IsEndOfReturnType())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfParameterList:
				if (IsEndOfParameterList())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfFieldDeclaration:
				if (IsEndOfFieldDeclaration())
				{
					return true;
				}
				break;
			case TerminatorState.IsPossibleEndOfVariableDeclaration:
				if (IsPossibleEndOfVariableDeclaration())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfTypeArgumentList:
				if (IsEndOfTypeArgumentList())
				{
					return true;
				}
				break;
			case TerminatorState.IsPossibleStatementStartOrStop:
				if (IsPossibleStatementStartOrStop())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfFixedStatement:
				if (IsEndOfFixedStatement())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfTryBlock:
				if (IsEndOfTryBlock())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfCatchClause:
				if (IsEndOfCatchClause())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfilterClause:
				if (IsEndOfFilterClause())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfCatchBlock:
				if (IsEndOfCatchBlock())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfDoWhileExpression:
				if (IsEndOfDoWhileExpression())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfForStatementArgument:
				if (IsEndOfForStatementArgument())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfDeclarationClause:
				if (IsEndOfDeclarationClause())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfArgumentList:
				if (IsEndOfArgumentList())
				{
					return true;
				}
				break;
			case TerminatorState.IsSwitchSectionStart:
				if (IsPossibleSwitchSection())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfTypeParameterList:
				if (IsEndOfTypeParameterList())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfMethodSignature:
				if (IsEndOfMethodSignature())
				{
					return true;
				}
				break;
			case TerminatorState.IsEndOfNameInExplicitInterface:
				if (IsEndOfNameInExplicitInterface())
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	private static Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode GetOldParent(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
	{
		return node?.Parent;
	}

	internal CompilationUnitSyntax ParseCompilationUnit()
	{
		return ParseWithStackGuard(ParseCompilationUnitCore, () => SyntaxFactory.CompilationUnit(default, default, default, default, SyntaxFactory.Token(SyntaxKind.EndOfFileToken)));
	}

	internal CompilationUnitSyntax ParseCompilationUnitCore()
	{
		SyntaxToken openBrace = null;
		SyntaxListBuilder initialBadNodes = null;
		NamespaceBodyBuilder body = new NamespaceBodyBuilder(_pool);
		try
		{
			ParseNamespaceBody(ref openBrace, ref body, ref initialBadNodes, SyntaxKind.CompilationUnit);
			SyntaxToken endOfFileToken = EatToken(SyntaxKind.EndOfFileToken);
			CompilationUnitSyntax compilationUnitSyntax = _syntaxFactory.CompilationUnit(body.Externs, body.Usings, body.Attributes, body.Members, endOfFileToken);
			if (initialBadNodes != null)
			{
				compilationUnitSyntax = AddLeadingSkippedSyntax(compilationUnitSyntax, initialBadNodes.ToListNode());
				_pool.Free(initialBadNodes);
			}
			return compilationUnitSyntax;
		}
		finally
		{
			body.Free(_pool);
		}
	}

	internal TNode ParseWithStackGuard<TNode>(Func<TNode> parseFunc, Func<TNode> createEmptyNodeFunc) where TNode : CSharpSyntaxNode
	{
		try
		{
			return parseFunc();
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			return CreateForGlobalFailure(lexer.TextWindow.Position, createEmptyNodeFunc());
		}
	}

	private TNode CreateForGlobalFailure<TNode>(int position, TNode node) where TNode : CSharpSyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = new SyntaxListBuilder(1);
		syntaxListBuilder.Add(SyntaxFactory.BadToken(null, lexer.TextWindow.Text.ToString(), null));
		SkippedTokensTriviaSyntax skippedSyntax = _syntaxFactory.SkippedTokensTrivia(syntaxListBuilder.ToList<SyntaxToken>());
		node = AddLeadingSkippedSyntax(node, skippedSyntax);
		ForceEndOfFile();
		return AddError(node, position, 0, ErrorCode.ERR_InsufficientStack);
	}

	private NamespaceDeclarationSyntax ParseNamespaceDeclaration()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.NamespaceDeclaration)
		{
			return (NamespaceDeclarationSyntax)EatNode();
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.NamespaceKeyword);
		if (IsScript)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_NamespaceNotAllowedInScript);
		}
		NameSyntax name = ParseQualifiedName();
		SyntaxToken openBrace;
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken || IsPossibleNamespaceMemberDeclaration())
		{
			openBrace = EatToken(SyntaxKind.OpenBraceToken);
		}
		else
		{
			openBrace = EatTokenWithPrejudice(SyntaxKind.OpenBraceToken);
			openBrace = ConvertToMissingWithTrailingTrivia(openBrace, SyntaxKind.OpenBraceToken);
		}
		NamespaceBodyBuilder body = new NamespaceBodyBuilder(_pool);
		SyntaxListBuilder initialBadNodes = null;
		try
		{
			ParseNamespaceBody(ref openBrace, ref body, ref initialBadNodes, SyntaxKind.NamespaceDeclaration);
			SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			SyntaxToken semicolonToken = null;
			if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				semicolonToken = EatToken();
			}
			return _syntaxFactory.NamespaceDeclaration(syntaxToken, name, openBrace, body.Externs, body.Usings, body.Members, closeBraceToken, semicolonToken);
		}
		finally
		{
			body.Free(_pool);
		}
	}

	private static bool IsPossibleStartOfTypeDeclaration(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.SealedKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.AbstractKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.UnsafeKeyword:
			return true;
		default:
			return false;
		}
	}

	private void AddSkippedNamespaceText(ref SyntaxToken openBrace, ref NamespaceBodyBuilder body, ref SyntaxListBuilder initialBadNodes, CSharpSyntaxNode skippedSyntax)
	{
		if (body.Members.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Members, skippedSyntax);
			return;
		}
		if (body.Attributes.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Attributes, skippedSyntax);
			return;
		}
		if (body.Usings.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Usings, skippedSyntax);
			return;
		}
		if (body.Externs.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Externs, skippedSyntax);
			return;
		}
		if (openBrace != null)
		{
			openBrace = AddTrailingSkippedSyntax(openBrace, skippedSyntax);
			return;
		}
		if (initialBadNodes == null)
		{
			initialBadNodes = _pool.Allocate();
		}
		initialBadNodes.AddRange(skippedSyntax);
	}

	private void ParseNamespaceBody(ref SyntaxToken openBrace, ref NamespaceBodyBuilder body, ref SyntaxListBuilder initialBadNodes, SyntaxKind parentKind)
	{
		bool flag = openBrace == null;
		bool flag2 = flag && IsScript;
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsNamespaceMemberStartOrStop;
		NamespaceParts namespaceParts = NamespaceParts.None;
		SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers = _pool.Allocate<MemberDeclarationSyntax>();
		bool flag3 = true;
		try
		{
			while (true)
			{
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.NamespaceKeyword:
					AddIncompleteMembers(ref incompleteMembers, ref body);
					body.Members.Add(ParseNamespaceDeclaration());
					namespaceParts = NamespaceParts.MembersAndStatements;
					flag3 = true;
					continue;
				case SyntaxKind.CloseBraceToken:
					if (flag)
					{
						ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref body, ref initialBadNodes);
						SyntaxToken node3 = EatToken();
						node3 = AddError(node3, IsScript ? ErrorCode.ERR_GlobalDefinitionOrStatementExpected : ErrorCode.ERR_EOFExpected);
						AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, node3);
						flag3 = true;
						continue;
					}
					return;
				case SyntaxKind.EndOfFileToken:
					return;
				case SyntaxKind.ExternKeyword:
					if (!flag2 || ScanExternAliasDirective())
					{
						ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref body, ref initialBadNodes);
						ExternAliasDirectiveSyntax node2 = ParseExternAliasDirective();
						if (namespaceParts > NamespaceParts.ExternAliases)
						{
							node2 = AddErrorToFirstToken(node2, ErrorCode.ERR_ExternAfterElements);
							AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, node2);
						}
						else
						{
							body.Externs.Add(node2);
							namespaceParts = NamespaceParts.ExternAliases;
						}
						flag3 = true;
						continue;
					}
					break;
				case SyntaxKind.UsingKeyword:
					if (flag2 && PeekToken(1).Kind == SyntaxKind.OpenParenToken)
					{
						AddIncompleteMembers(ref incompleteMembers, ref body);
						body.Members.Add(_syntaxFactory.GlobalStatement(ParseUsingStatement()));
						namespaceParts = NamespaceParts.MembersAndStatements;
					}
					else
					{
						ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref body, ref initialBadNodes);
						UsingDirectiveSyntax node = ParseUsingDirective();
						if (namespaceParts > NamespaceParts.Usings)
						{
							node = AddError(node, ErrorCode.ERR_UsingAfterElements);
							AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, node);
						}
						else
						{
							body.Usings.Add(node);
							namespaceParts = NamespaceParts.Usings;
						}
					}
					flag3 = true;
					continue;
				case SyntaxKind.OpenBracketToken:
					if (IsPossibleGlobalAttributeDeclaration())
					{
						ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref body, ref initialBadNodes);
						AttributeListSyntax attributeListSyntax = ParseAttributeDeclaration();
						if (!flag || namespaceParts > NamespaceParts.GlobalAttributes)
						{
							attributeListSyntax = AddError(attributeListSyntax, attributeListSyntax.Target.Identifier, ErrorCode.ERR_GlobalAttributesNotFirst);
							AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, attributeListSyntax);
						}
						else
						{
							body.Attributes.Add(attributeListSyntax);
							namespaceParts = NamespaceParts.GlobalAttributes;
						}
						flag3 = true;
						continue;
					}
					break;
				}
				MemberDeclarationSyntax memberDeclarationSyntax = ParseMemberDeclarationOrStatement(parentKind);
				if (memberDeclarationSyntax == null)
				{
					ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref body, ref initialBadNodes);
					SyntaxToken syntaxToken = EatToken();
					if (flag3 && !syntaxToken.ContainsDiagnostics)
					{
						syntaxToken = AddError(syntaxToken, IsScript ? ErrorCode.ERR_GlobalDefinitionOrStatementExpected : ErrorCode.ERR_EOFExpected);
						flag3 = false;
					}
					AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, syntaxToken);
				}
				else if (memberDeclarationSyntax.Kind == SyntaxKind.IncompleteMember && namespaceParts < NamespaceParts.MembersAndStatements)
				{
					incompleteMembers.Add(memberDeclarationSyntax);
					flag3 = true;
				}
				else
				{
					AddIncompleteMembers(ref incompleteMembers, ref body);
					body.Members.Add(memberDeclarationSyntax);
					namespaceParts = NamespaceParts.MembersAndStatements;
					flag3 = true;
				}
			}
		}
		finally
		{
			_termState = termState;
			AddIncompleteMembers(ref incompleteMembers, ref body);
			_pool.Free(incompleteMembers);
		}
	}

	private static void AddIncompleteMembers(ref SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers, ref NamespaceBodyBuilder body)
	{
		if (incompleteMembers.Count > 0)
		{
			body.Members.AddRange(incompleteMembers);
			incompleteMembers.Clear();
		}
	}

	private void ReduceIncompleteMembers(ref SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers, ref SyntaxToken openBrace, ref NamespaceBodyBuilder body, ref SyntaxListBuilder initialBadNodes)
	{
		for (int i = 0; i < incompleteMembers.Count; i++)
		{
			AddSkippedNamespaceText(ref openBrace, ref body, ref initialBadNodes, incompleteMembers[i]);
		}
		incompleteMembers.Clear();
	}

	private bool IsPossibleNamespaceMemberDeclaration()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.NamespaceKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsPartialInNamespaceMemberDeclaration();
		default:
			return IsPossibleStartOfTypeDeclaration(CurrentToken.Kind);
		}
	}

	private bool IsPartialInNamespaceMemberDeclaration()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.PartialKeyword)
		{
			if (IsPartialType())
			{
				return true;
			}
			if (PeekToken(1).Kind == SyntaxKind.NamespaceKeyword)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEndOfNamespace()
	{
		return CurrentToken.Kind == SyntaxKind.CloseBraceToken;
	}

	public bool IsGobalAttributesTerminator()
	{
		if (!IsEndOfNamespace())
		{
			return IsPossibleNamespaceMemberDeclaration();
		}
		return true;
	}

	private bool IsNamespaceMemberStartOrStop()
	{
		if (!IsEndOfNamespace())
		{
			return IsPossibleNamespaceMemberDeclaration();
		}
		return true;
	}

	private bool ScanExternAliasDirective()
	{
		if (CurrentToken.Kind == SyntaxKind.ExternKeyword && PeekToken(1).Kind == SyntaxKind.IdentifierToken && PeekToken(1).ContextualKind == SyntaxKind.AliasKeyword && PeekToken(2).Kind == SyntaxKind.IdentifierToken)
		{
			return PeekToken(3).Kind == SyntaxKind.SemicolonToken;
		}
		return false;
	}

	private ExternAliasDirectiveSyntax ParseExternAliasDirective()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.ExternAliasDirective)
		{
			return (ExternAliasDirectiveSyntax)EatNode();
		}
		SyntaxToken node = EatToken(SyntaxKind.ExternKeyword);
		SyntaxToken aliasKeyword = EatContextualToken(SyntaxKind.AliasKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureExternAlias);
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.ExternAliasDirective(node, aliasKeyword, identifier, semicolonToken);
	}

	private NameEqualsSyntax ParseNameEquals()
	{
		return _syntaxFactory.NameEquals(_syntaxFactory.IdentifierName(ParseIdentifierToken()), EatToken(SyntaxKind.EqualsToken));
	}

	private UsingDirectiveSyntax ParseUsingDirective()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.UsingDirective)
		{
			return (UsingDirectiveSyntax)EatNode();
		}
		SyntaxToken usingKeyword = EatToken(SyntaxKind.UsingKeyword);
		SyntaxToken syntaxToken = null;
		if (CurrentToken.Kind == SyntaxKind.StaticKeyword)
		{
			syntaxToken = EatToken(SyntaxKind.StaticKeyword);
		}
		NameEqualsSyntax alias = (IsNamedAssignment() ? ParseNameEquals() : null);
		NameSyntax nameSyntax;
		SyntaxToken semicolonToken;
		if (IsPossibleNamespaceMemberDeclaration())
		{
			nameSyntax = WithAdditionalDiagnostics(CreateMissingIdentifierName(), GetExpectedTokenError(SyntaxKind.IdentifierToken, CurrentToken.Kind));
			semicolonToken = SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			nameSyntax = ParseQualifiedName();
			if (nameSyntax.IsMissing && PeekToken(1).Kind == SyntaxKind.SemicolonToken)
			{
				nameSyntax = AddTrailingSkippedSyntax(nameSyntax, EatToken());
			}
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		UsingDirectiveSyntax usingDirectiveSyntax = _syntaxFactory.UsingDirective(usingKeyword, syntaxToken, alias, nameSyntax, semicolonToken);
		if (syntaxToken != null)
		{
			usingDirectiveSyntax = CheckFeatureAvailability(usingDirectiveSyntax, MessageID.IDS_FeatureUsingStatic);
		}
		return usingDirectiveSyntax;
	}

	private bool IsPossibleGlobalAttributeDeclaration()
	{
		if (CurrentToken.Kind == SyntaxKind.OpenBracketToken && IsGlobalAttributeTarget(PeekToken(1)))
		{
			return PeekToken(2).Kind == SyntaxKind.ColonToken;
		}
		return false;
	}

	private static bool IsGlobalAttributeTarget(SyntaxToken token)
	{
		AttributeLocation attributeLocation = token.ToAttributeLocation();
		if ((uint)(attributeLocation - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	private bool IsPossibleAttributeDeclaration()
	{
		return CurrentToken.Kind == SyntaxKind.OpenBracketToken;
	}

	private void ParseAttributeDeclarations(SyntaxListBuilder list)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsAttributeDeclarationTerminator;
		while (IsPossibleAttributeDeclaration())
		{
			list.Add(ParseAttributeDeclaration());
		}
		_termState = termState;
	}

	private bool IsAttributeDeclarationTerminator()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			return IsPossibleAttributeDeclaration();
		}
		return true;
	}

	private AttributeListSyntax ParseAttributeDeclaration()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.AttributeList)
		{
			return (AttributeListSyntax)EatNode();
		}
		SyntaxToken openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
		AttributeTargetSpecifierSyntax attributeTargetSpecifierSyntax = null;
		if (IsSomeWord(CurrentToken.Kind) && PeekToken(1).Kind == SyntaxKind.ColonToken)
		{
			SyntaxToken identifier = SyntaxParser.ConvertToKeyword(EatToken());
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
			attributeTargetSpecifierSyntax = _syntaxFactory.AttributeTargetSpecifier(identifier, colonToken);
		}
		SeparatedSyntaxListBuilder<AttributeSyntax> builder = _pool.AllocateSeparated<AttributeSyntax>();
		try
		{
			if (attributeTargetSpecifierSyntax != null && attributeTargetSpecifierSyntax.Identifier.ToAttributeLocation() == AttributeLocation.Module)
			{
				attributeTargetSpecifierSyntax = CheckFeatureAvailability(attributeTargetSpecifierSyntax, MessageID.IDS_FeatureModuleAttrLoc);
			}
			ParseAttributes(builder);
			SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			return _syntaxFactory.AttributeList(openBracketToken, attributeTargetSpecifierSyntax, builder, closeBracketToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private void ParseAttributes(SeparatedSyntaxListBuilder<AttributeSyntax> nodes)
	{
		nodes.Add(ParseAttribute());
		while (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			if (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				nodes.AddSeparator(EatToken());
				if (CurrentToken.Kind == SyntaxKind.CloseBracketToken)
				{
					break;
				}
				nodes.Add(ParseAttribute());
			}
			else if (IsPossibleAttribute())
			{
				nodes.AddSeparator(EatToken(SyntaxKind.CommaToken));
				nodes.Add(ParseAttribute());
			}
			else if (SkipBadAttributeListTokens(nodes, SyntaxKind.IdentifierToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
	}

	private PostSkipAction SkipBadAttributeListTokens(SeparatedSyntaxListBuilder<AttributeSyntax> list, SyntaxKind expected)
	{
		SyntaxToken startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttribute(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.IsTerminator(), expected);
	}

	private bool IsPossibleAttribute()
	{
		return IsTrueIdentifier();
	}

	private AttributeSyntax ParseAttribute()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.Attribute)
		{
			return (AttributeSyntax)EatNode();
		}
		NameSyntax name = ParseQualifiedName();
		AttributeArgumentListSyntax argumentList = ParseAttributeArgumentList();
		return _syntaxFactory.Attribute(name, argumentList);
	}

	internal AttributeArgumentListSyntax ParseAttributeArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.AttributeArgumentList)
		{
			return (AttributeArgumentListSyntax)EatNode();
		}
		AttributeArgumentListSyntax result = null;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);
			SeparatedSyntaxListBuilder<AttributeArgumentSyntax> builder = _pool.AllocateSeparated<AttributeArgumentSyntax>();
			try
			{
				while (CurrentToken.Kind != SyntaxKind.CloseParenToken)
				{
					if (IsPossibleAttributeArgument() || CurrentToken.Kind == SyntaxKind.CommaToken)
					{
						builder.Add(ParseAttributeArgument());
						while (CurrentToken.Kind != SyntaxKind.CloseParenToken)
						{
							if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleAttributeArgument())
							{
								builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
								builder.Add(ParseAttributeArgument());
							}
							else if (SkipBadAttributeArgumentTokens(ref openParen, builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
							{
								break;
							}
						}
						break;
					}
					if (SkipBadAttributeArgumentTokens(ref openParen, builder, SyntaxKind.IdentifierToken) != PostSkipAction.Continue)
					{
						break;
					}
				}
				SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
				result = _syntaxFactory.AttributeArgumentList(openParen, builder, closeParenToken);
			}
			finally
			{
				_pool.Free(in builder);
			}
		}
		return result;
	}

	private PostSkipAction SkipBadAttributeArgumentTokens(ref SyntaxToken openParen, SeparatedSyntaxListBuilder<AttributeArgumentSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttributeArgument(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator(), expected);
	}

	private bool IsPossibleAttributeArgument()
	{
		return IsPossibleExpression();
	}

	private AttributeArgumentSyntax ParseAttributeArgument()
	{
		NameEqualsSyntax nameEquals = null;
		NameColonSyntax nameColon = null;
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			switch (PeekToken(1).Kind)
			{
			case SyntaxKind.EqualsToken:
			{
				SyntaxToken identifier = ParseIdentifierToken();
				SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
				nameEquals = _syntaxFactory.NameEquals(_syntaxFactory.IdentifierName(identifier), equalsToken);
				break;
			}
			case SyntaxKind.ColonToken:
			{
				IdentifierNameSyntax name = ParseIdentifierName();
				SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
				nameColon = _syntaxFactory.NameColon(name, colonToken);
				nameColon = CheckFeatureAvailability(nameColon, MessageID.IDS_FeatureNamedArgument);
				break;
			}
			}
		}
		return _syntaxFactory.AttributeArgument(nameEquals, nameColon, ParseExpressionCore());
	}

	private static DeclarationModifiers GetModifier(SyntaxToken token)
	{
		return GetModifier(token.Kind, token.ContextualKind);
	}

	internal static DeclarationModifiers GetModifier(SyntaxKind kind, SyntaxKind contextualKind)
	{
		switch (kind)
		{
		case SyntaxKind.PublicKeyword:
			return DeclarationModifiers.Public;
		case SyntaxKind.InternalKeyword:
			return DeclarationModifiers.Internal;
		case SyntaxKind.ProtectedKeyword:
			return DeclarationModifiers.Protected;
		case SyntaxKind.PrivateKeyword:
			return DeclarationModifiers.Private;
		case SyntaxKind.SealedKeyword:
			return DeclarationModifiers.Sealed;
		case SyntaxKind.AbstractKeyword:
			return DeclarationModifiers.Abstract;
		case SyntaxKind.StaticKeyword:
			return DeclarationModifiers.Static;
		case SyntaxKind.VirtualKeyword:
			return DeclarationModifiers.Virtual;
		case SyntaxKind.ExternKeyword:
			return DeclarationModifiers.Extern;
		case SyntaxKind.NewKeyword:
			return DeclarationModifiers.New;
		case SyntaxKind.OverrideKeyword:
			return DeclarationModifiers.Override;
		case SyntaxKind.ReadOnlyKeyword:
			return DeclarationModifiers.ReadOnly;
		case SyntaxKind.VolatileKeyword:
			return DeclarationModifiers.Volatile;
		case SyntaxKind.UnsafeKeyword:
			return DeclarationModifiers.Unsafe;
		case SyntaxKind.PartialKeyword:
			return DeclarationModifiers.Partial;
		case SyntaxKind.AsyncKeyword:
			return DeclarationModifiers.Async;
		case SyntaxKind.RefKeyword:
			return DeclarationModifiers.Ref;
		case SyntaxKind.IdentifierToken:
			switch (contextualKind)
			{
			case SyntaxKind.PartialKeyword:
				return DeclarationModifiers.Partial;
			case SyntaxKind.AsyncKeyword:
				return DeclarationModifiers.Async;
			}
			break;
		}
		return DeclarationModifiers.None;
	}

	private void ParseModifiers(SyntaxListBuilder tokens, bool forAccessors)
	{
		while (true)
		{
			SyntaxToken item;
			switch (GetModifier(CurrentToken))
			{
			case DeclarationModifiers.Partial:
			{
				SyntaxToken syntaxToken = PeekToken(1);
				bool flag = IsPartialType();
				bool flag2 = IsPartialMember();
				if (flag | flag2)
				{
					item = SyntaxParser.ConvertToKeyword(EatToken());
					item = CheckFeatureAvailability(item, flag ? MessageID.IDS_FeaturePartialTypes : MessageID.IDS_FeaturePartialMethod);
					break;
				}
				if (syntaxToken.Kind == SyntaxKind.NamespaceKeyword)
				{
					item = SyntaxParser.ConvertToKeyword(EatToken());
					break;
				}
				if (syntaxToken.Kind == SyntaxKind.EnumKeyword || syntaxToken.Kind == SyntaxKind.DelegateKeyword || (IsPossibleStartOfTypeDeclaration(syntaxToken.Kind) && GetModifier(syntaxToken) != DeclarationModifiers.None))
				{
					item = AddError(SyntaxParser.ConvertToKeyword(EatToken()), ErrorCode.ERR_PartialMisplaced);
					break;
				}
				return;
			}
			case DeclarationModifiers.Ref:
			{
				SyntaxToken syntaxToken2 = PeekToken(1);
				if (syntaxToken2.Kind == SyntaxKind.StructKeyword || (syntaxToken2.ContextualKind == SyntaxKind.PartialKeyword && PeekToken(2).Kind == SyntaxKind.StructKeyword))
				{
					item = EatToken();
					item = CheckFeatureAvailability(item, MessageID.IDS_FeatureRefStructs);
					break;
				}
				if (forAccessors && IsPossibleAccessorModifier())
				{
					item = EatToken();
					break;
				}
				return;
			}
			case DeclarationModifiers.Async:
				if (!ShouldAsyncBeTreatedAsModifier(parsingStatementNotDeclaration: false))
				{
					return;
				}
				item = SyntaxParser.ConvertToKeyword(EatToken());
				item = CheckFeatureAvailability(item, MessageID.IDS_FeatureAsync);
				break;
			default:
				item = EatToken();
				break;
			case DeclarationModifiers.None:
				return;
			}
			tokens.Add(item);
		}
	}

	private bool ShouldAsyncBeTreatedAsModifier(bool parsingStatementNotDeclaration)
	{
		if (IsNonContextualModifier(PeekToken(1)))
		{
			return true;
		}
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			if (!parsingStatementNotDeclaration && CurrentToken.ContextualKind == SyntaxKind.PartialKeyword)
			{
				EatToken();
			}
			if (!parsingStatementNotDeclaration)
			{
				SyntaxKind kind = CurrentToken.Kind;
				if (IsPossibleStartOfTypeDeclaration(kind) || kind == SyntaxKind.EventKeyword || ((kind == SyntaxKind.ExplicitKeyword || kind == SyntaxKind.ImplicitKeyword) && PeekToken(1).Kind == SyntaxKind.OperatorKeyword))
				{
					return true;
				}
			}
			if (ScanType() != ScanTypeFlags.NotType)
			{
				if (IsPossibleMemberName())
				{
					return true;
				}
				SyntaxKind kind2 = CurrentToken.Kind;
				switch (kind2)
				{
				case SyntaxKind.EndOfFileToken:
					return true;
				case SyntaxKind.CloseBraceToken:
					return true;
				default:
					if (SyntaxFacts.IsPredefinedType(CurrentToken.Kind))
					{
						return true;
					}
					if (IsNonContextualModifier(CurrentToken))
					{
						return true;
					}
					if (IsTypeDeclarationStart())
					{
						return true;
					}
					if (kind2 == SyntaxKind.NamespaceKeyword)
					{
						return true;
					}
					if (!parsingStatementNotDeclaration && kind2 == SyntaxKind.OperatorKeyword)
					{
						return true;
					}
					break;
				}
			}
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
		return false;
	}

	private static bool IsNonContextualModifier(SyntaxToken nextToken)
	{
		if (GetModifier(nextToken) != DeclarationModifiers.None)
		{
			return !SyntaxFacts.IsContextualKeyword(nextToken.ContextualKind);
		}
		return false;
	}

	private bool IsPartialType()
	{
		SyntaxKind kind = PeekToken(1).Kind;
		if (kind - 8374 <= (SyntaxKind)2)
		{
			return true;
		}
		return false;
	}

	private bool IsPartialMember()
	{
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			if (ScanType() == ScanTypeFlags.NotType)
			{
				return false;
			}
			return IsPossibleMemberName();
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool IsPossibleMemberName()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.ThisKeyword || kind == SyntaxKind.IdentifierToken)
		{
			return true;
		}
		return false;
	}

	private MemberDeclarationSyntax ParseTypeDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		CancellationToken cancellationToken = base.cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.ClassKeyword:
			CheckForVersionSpecificModifiers(modifiers, SyntaxKind.StaticKeyword, MessageID.IDS_FeatureStaticClasses);
			return ParseClassOrStructOrInterfaceDeclaration(attributes, modifiers);
		case SyntaxKind.StructKeyword:
			CheckForVersionSpecificModifiers(modifiers, SyntaxKind.ReadOnlyKeyword, MessageID.IDS_FeatureReadOnlyStructs);
			return ParseClassOrStructOrInterfaceDeclaration(attributes, modifiers);
		case SyntaxKind.InterfaceKeyword:
			return ParseClassOrStructOrInterfaceDeclaration(attributes, modifiers);
		case SyntaxKind.DelegateKeyword:
			return ParseDelegateDeclaration(attributes, modifiers);
		case SyntaxKind.EnumKeyword:
			return ParseEnumDeclaration(attributes, modifiers);
		default:
			throw ExceptionUtilities.UnexpectedValue(CurrentToken.Kind);
		}
	}

	private void CheckForVersionSpecificModifiers(SyntaxListBuilder modifiers, SyntaxKind kind, MessageID feature)
	{
		int i = 0;
		for (int count = modifiers.Count; i < count; i++)
		{
			if (modifiers[i].RawKind == (int)kind)
			{
				modifiers[i] = CheckFeatureAvailability(modifiers[i], feature);
			}
		}
	}

	private TypeDeclarationSyntax ParseClassOrStructOrInterfaceDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken syntaxToken = EatToken();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsPossibleAggregateClauseStartOrStop;
		SyntaxToken syntaxToken2 = ParseIdentifierToken();
		TypeParameterListSyntax typeParameterList = ParseTypeParameterList();
		_termState = termState;
		BaseListSyntax baseList = ParseBaseList();
		bool flag = true;
		SyntaxListBuilder<MemberDeclarationSyntax> syntaxListBuilder = default;
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder2 = default;
		try
		{
			if (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				syntaxListBuilder2 = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
				ParseTypeParameterConstraintClauses(syntaxListBuilder2);
			}
			SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
			if (syntaxToken2.IsMissing || openBrace.IsMissing)
			{
				flag = false;
			}
			if (flag)
			{
				syntaxListBuilder = _pool.Allocate<MemberDeclarationSyntax>();
				while (true)
				{
					SyntaxKind kind = CurrentToken.Kind;
					if (CanStartMember(kind))
					{
						TerminatorState termState2 = _termState;
						_termState |= TerminatorState.IsPossibleMemberStartOrStop;
						MemberDeclarationSyntax memberDeclarationSyntax = ParseMemberDeclarationOrStatement(syntaxToken.Kind);
						if (memberDeclarationSyntax != null)
						{
							syntaxListBuilder.Add(memberDeclarationSyntax);
						}
						else
						{
							SkipBadMemberListTokens(ref openBrace, syntaxListBuilder);
						}
						_termState = termState2;
					}
					else
					{
						if (kind == SyntaxKind.CloseBraceToken || kind == SyntaxKind.EndOfFileToken || IsTerminator())
						{
							break;
						}
						SkipBadMemberListTokens(ref openBrace, syntaxListBuilder);
					}
				}
			}
			SyntaxToken node;
			if (openBrace.IsMissing)
			{
				node = SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
				node = WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, CurrentToken.Kind));
			}
			else
			{
				node = EatToken(SyntaxKind.CloseBraceToken);
			}
			SyntaxToken semicolonToken = null;
			if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				semicolonToken = EatToken();
			}
			return syntaxToken.Kind switch
			{
				SyntaxKind.ClassKeyword => (TypeDeclarationSyntax)_syntaxFactory.ClassDeclaration(attributes, modifiers.ToList(), syntaxToken, syntaxToken2, typeParameterList, baseList, syntaxListBuilder2, openBrace, syntaxListBuilder, node, semicolonToken), 
				SyntaxKind.StructKeyword => _syntaxFactory.StructDeclaration(attributes, modifiers.ToList(), syntaxToken, syntaxToken2, typeParameterList, baseList, syntaxListBuilder2, openBrace, syntaxListBuilder, node, semicolonToken), 
				SyntaxKind.InterfaceKeyword => _syntaxFactory.InterfaceDeclaration(attributes, modifiers.ToList(), syntaxToken, syntaxToken2, typeParameterList, baseList, syntaxListBuilder2, openBrace, syntaxListBuilder, node, semicolonToken), 
				_ => throw ExceptionUtilities.UnexpectedValue(syntaxToken.Kind), 
			};
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
			if (!syntaxListBuilder2.IsNull)
			{
				_pool.Free(syntaxListBuilder2);
			}
		}
	}

	private void SkipBadMemberListTokens(ref SyntaxToken openBrace, SyntaxListBuilder members)
	{
		if (members.Count > 0)
		{
			GreenNode previousNode = members[members.Count - 1];
			SkipBadMemberListTokens(ref previousNode);
			members[members.Count - 1] = previousNode;
		}
		else
		{
			GreenNode previousNode2 = openBrace;
			SkipBadMemberListTokens(ref previousNode2);
			openBrace = (SyntaxToken)previousNode2;
		}
	}

	private void SkipBadMemberListTokens(ref GreenNode previousNode)
	{
		int num = 0;
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		try
		{
			bool flag = false;
			SyntaxToken syntaxToken = EatToken();
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_InvalidMemberDecl, syntaxToken.Text);
			syntaxListBuilder.Add(syntaxToken);
			while (!flag)
			{
				SyntaxKind kind = CurrentToken.Kind;
				if (CanStartMember(kind) && (kind != SyntaxKind.DelegateKeyword || (PeekToken(1).Kind != SyntaxKind.OpenBraceToken && PeekToken(1).Kind != SyntaxKind.OpenParenToken)))
				{
					flag = true;
					continue;
				}
				switch (kind)
				{
				case SyntaxKind.OpenBraceToken:
					num++;
					break;
				case SyntaxKind.CloseBraceToken:
					if (num-- == 0)
					{
						flag = true;
						continue;
					}
					break;
				case SyntaxKind.EndOfFileToken:
					flag = true;
					continue;
				}
				syntaxListBuilder.Add(EatToken());
			}
			previousNode = AddTrailingSkippedSyntax((CSharpSyntaxNode)previousNode, syntaxListBuilder.ToListNode());
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsPossibleMemberStartOrStop()
	{
		if (!IsPossibleMemberStart())
		{
			return CurrentToken.Kind == SyntaxKind.CloseBraceToken;
		}
		return true;
	}

	private bool IsPossibleAggregateClauseStartOrStop()
	{
		if (CurrentToken.Kind != SyntaxKind.ColonToken && CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return IsCurrentTokenWhereOfConstraintClause();
		}
		return true;
	}

	private BaseListSyntax ParseBaseList()
	{
		if (CurrentToken.Kind != SyntaxKind.ColonToken)
		{
			return null;
		}
		SyntaxToken colon = EatToken();
		SeparatedSyntaxListBuilder<BaseTypeSyntax> builder = _pool.AllocateSeparated<BaseTypeSyntax>();
		try
		{
			TypeSyntax type = ParseType();
			builder.Add(_syntaxFactory.SimpleBaseType(type));
			while (CurrentToken.Kind != SyntaxKind.OpenBraceToken && !IsCurrentTokenWhereOfConstraintClause())
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleType())
				{
					builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
					builder.Add(_syntaxFactory.SimpleBaseType(ParseType()));
				}
				else if (SkipBadBaseListTokens(ref colon, builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			return _syntaxFactory.BaseList(colon, builder);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private PostSkipAction SkipBadBaseListTokens(ref SyntaxToken colon, SeparatedSyntaxListBuilder<BaseTypeSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref colon, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttribute(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.OpenBraceToken || p.IsCurrentTokenWhereOfConstraintClause() || p.IsTerminator(), expected);
	}

	private bool IsCurrentTokenWhereOfConstraintClause()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword && PeekToken(1).Kind == SyntaxKind.IdentifierToken)
		{
			return PeekToken(2).Kind == SyntaxKind.ColonToken;
		}
		return false;
	}

	private void ParseTypeParameterConstraintClauses(SyntaxListBuilder list)
	{
		while (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
		{
			list.Add(ParseTypeParameterConstraintClause());
		}
	}

	private TypeParameterConstraintClauseSyntax ParseTypeParameterConstraintClause()
	{
		SyntaxToken whereKeyword = EatContextualToken(SyntaxKind.WhereKeyword);
		IdentifierNameSyntax name = ((!IsTrueIdentifier()) ? AddError(CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected) : ParseIdentifierName());
		SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		SeparatedSyntaxListBuilder<TypeParameterConstraintSyntax> builder = _pool.AllocateSeparated<TypeParameterConstraintSyntax>();
		try
		{
			if (CurrentToken.Kind == SyntaxKind.OpenBraceToken || IsCurrentTokenWhereOfConstraintClause())
			{
				builder.Add(_syntaxFactory.TypeConstraint(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TypeExpected)));
			}
			else
			{
				builder.Add(ParseTypeParameterConstraint());
				while (CurrentToken.Kind != SyntaxKind.OpenBraceToken && CurrentToken.Kind != SyntaxKind.EqualsGreaterThanToken && CurrentToken.ContextualKind != SyntaxKind.WhereKeyword)
				{
					if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleTypeParameterConstraint())
					{
						builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
						if (IsCurrentTokenWhereOfConstraintClause())
						{
							builder.Add(_syntaxFactory.TypeConstraint(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TypeExpected)));
							break;
						}
						builder.Add(ParseTypeParameterConstraint());
					}
					else if (SkipBadTypeParameterConstraintTokens(builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
					{
						break;
					}
				}
			}
			return _syntaxFactory.TypeParameterConstraintClause(whereKeyword, name, colonToken, builder);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private bool IsPossibleTypeParameterConstraint()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		default:
			return IsPredefinedType(CurrentToken.Kind);
		}
	}

	private TypeParameterConstraintSyntax ParseTypeParameterConstraint()
	{
		_ = CurrentToken.Kind;
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.NewKeyword:
		{
			SyntaxToken newKeyword = EatToken();
			SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			return _syntaxFactory.ConstructorConstraint(newKeyword, openParenToken, closeParenToken);
		}
		case SyntaxKind.StructKeyword:
		{
			SyntaxToken classOrStructKeyword2 = EatToken();
			return _syntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint, classOrStructKeyword2);
		}
		case SyntaxKind.ClassKeyword:
		{
			SyntaxToken classOrStructKeyword = EatToken();
			return _syntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint, classOrStructKeyword);
		}
		default:
		{
			TypeSyntax type = ParseType();
			return _syntaxFactory.TypeConstraint(type);
		}
		}
	}

	private PostSkipAction SkipBadTypeParameterConstraintTokens(SeparatedSyntaxListBuilder<TypeParameterConstraintSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => CurrentToken.Kind != SyntaxKind.CommaToken && !IsPossibleTypeParameterConstraint(), (LanguageParser p) => CurrentToken.Kind == SyntaxKind.OpenBraceToken || IsCurrentTokenWhereOfConstraintClause() || IsTerminator(), expected);
	}

	private bool IsPossibleMemberStart()
	{
		return CanStartMember(CurrentToken.Kind);
	}

	private static bool CanStartMember(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TildeToken:
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.BoolKeyword:
		case SyntaxKind.ByteKeyword:
		case SyntaxKind.SByteKeyword:
		case SyntaxKind.ShortKeyword:
		case SyntaxKind.UShortKeyword:
		case SyntaxKind.IntKeyword:
		case SyntaxKind.UIntKeyword:
		case SyntaxKind.LongKeyword:
		case SyntaxKind.ULongKeyword:
		case SyntaxKind.DoubleKeyword:
		case SyntaxKind.FloatKeyword:
		case SyntaxKind.DecimalKeyword:
		case SyntaxKind.StringKeyword:
		case SyntaxKind.CharKeyword:
		case SyntaxKind.VoidKeyword:
		case SyntaxKind.ObjectKeyword:
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.SealedKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.FixedKeyword:
		case SyntaxKind.VolatileKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.OverrideKeyword:
		case SyntaxKind.AbstractKeyword:
		case SyntaxKind.VirtualKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.UnsafeKeyword:
		case SyntaxKind.ExplicitKeyword:
		case SyntaxKind.ImplicitKeyword:
		case SyntaxKind.IdentifierToken:
			return true;
		default:
			return false;
		}
	}

	private bool IsTypeDeclarationStart()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind - 8374 <= (SyntaxKind)4)
		{
			return true;
		}
		return false;
	}

	private static bool CanReuseMemberDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax member)
	{
		switch (member?.Kind())
		{
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.EnumDeclaration:
		case SyntaxKind.DelegateDeclaration:
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
			return true;
		default:
			return false;
		}
	}

	private MemberDeclarationSyntax ParseMemberDeclarationOrStatement(SyntaxKind parentKind)
	{
		CancellationToken cancellationToken = base.cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		bool flag = parentKind == SyntaxKind.CompilationUnit && IsScript;
		bool flag2 = flag;
		if (IsIncrementalAndFactoryContextMatches && CanReuseMemberDeclaration(CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax))
		{
			return (MemberDeclarationSyntax)EatNode();
		}
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		SyntaxListBuilder modifiers = _pool.Allocate();
		TerminatorState termState = _termState;
		try
		{
			ParseAttributeDeclarations(syntaxListBuilder);
			if (syntaxListBuilder.Count > 0)
			{
				flag2 = false;
			}
			SyntaxKind kind;
			if (flag2)
			{
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.UnsafeKeyword:
					if (PeekToken(1).Kind == SyntaxKind.OpenBraceToken)
					{
						return _syntaxFactory.GlobalStatement(ParseUnsafeStatement());
					}
					break;
				case SyntaxKind.FixedKeyword:
					if (PeekToken(1).Kind == SyntaxKind.OpenParenToken)
					{
						return _syntaxFactory.GlobalStatement(ParseFixedStatement());
					}
					break;
				case SyntaxKind.DelegateKeyword:
					kind = PeekToken(1).Kind;
					if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBraceToken)
					{
						return _syntaxFactory.GlobalStatement(ParseExpressionStatement());
					}
					break;
				case SyntaxKind.NewKeyword:
					if (IsPossibleNewExpression())
					{
						return _syntaxFactory.GlobalStatement(ParseExpressionStatement());
					}
					break;
				}
			}
			ParseModifiers(modifiers, forAccessors: false);
			if (modifiers.Count > 0)
			{
				flag2 = false;
			}
			if (CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.OpenParenToken)
			{
				if (!flag)
				{
					return ParseConstructorDeclaration(syntaxListBuilder, modifiers);
				}
				if (!flag2)
				{
					SyntaxToken node = SyntaxFactory.MissingToken(SyntaxKind.VoidKeyword);
					node = AddError(node, ErrorCode.ERR_MemberNeedsType);
					PredefinedTypeSyntax type = _syntaxFactory.PredefinedType(node);
					SyntaxToken identifier = EatToken();
					return ParseMethodDeclaration(syntaxListBuilder, modifiers, type, null, identifier, null);
				}
			}
			if (!flag && CurrentToken.Kind == SyntaxKind.TildeToken)
			{
				return ParseDestructorDeclaration(syntaxListBuilder, modifiers);
			}
			if (CurrentToken.Kind == SyntaxKind.ConstKeyword)
			{
				return ParseConstantFieldDeclaration(syntaxListBuilder, modifiers, parentKind);
			}
			if (CurrentToken.Kind == SyntaxKind.EventKeyword)
			{
				return ParseEventDeclaration(syntaxListBuilder, modifiers, parentKind);
			}
			if (CurrentToken.Kind == SyntaxKind.FixedKeyword)
			{
				return ParseFixedSizeBufferDeclaration(syntaxListBuilder, modifiers, parentKind);
			}
			if (CurrentToken.Kind == SyntaxKind.ExplicitKeyword || CurrentToken.Kind == SyntaxKind.ImplicitKeyword || (CurrentToken.Kind == SyntaxKind.OperatorKeyword && !SyntaxFacts.IsAnyOverloadableOperator(PeekToken(1).Kind)))
			{
				return ParseConversionOperatorDeclaration(syntaxListBuilder, modifiers);
			}
			if (CurrentToken.Kind == SyntaxKind.NamespaceKeyword && parentKind == SyntaxKind.CompilationUnit)
			{
				if (syntaxListBuilder.Count > 0)
				{
					syntaxListBuilder[0] = AddError(syntaxListBuilder[0], ErrorCode.ERR_BadModifiersOnNamespace);
				}
				else
				{
					modifiers[0] = AddError(modifiers[0], ErrorCode.ERR_BadModifiersOnNamespace);
				}
				NamespaceDeclarationSyntax namespaceDeclarationSyntax = ParseNamespaceDeclaration();
				if (modifiers.Count > 0)
				{
					namespaceDeclarationSyntax = AddLeadingSkippedSyntax(namespaceDeclarationSyntax, modifiers.ToListNode());
				}
				if (syntaxListBuilder.Count > 0)
				{
					namespaceDeclarationSyntax = AddLeadingSkippedSyntax(namespaceDeclarationSyntax, syntaxListBuilder.ToListNode());
				}
				return namespaceDeclarationSyntax;
			}
			if (IsTypeDeclarationStart())
			{
				return ParseTypeDeclaration(syntaxListBuilder, modifiers);
			}
			if (flag2 && CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken && IsPossibleStatement(acceptAccessibilityMods: true))
			{
				TerminatorState termState2 = _termState;
				_termState |= TerminatorState.IsPossibleStatementStartOrStop;
				StatementSyntax statementSyntax = ParseStatementNoDeclaration(allowAnyExpression: true);
				_termState = termState2;
				if (statementSyntax != null)
				{
					return _syntaxFactory.GlobalStatement(statementSyntax);
				}
			}
			TypeSyntax type2 = ParseReturnType();
			bool flag3 = type2.Kind == SyntaxKind.RefType;
			if (GetModifier(CurrentToken) != DeclarationModifiers.None && CurrentToken.ContextualKind != SyntaxKind.PartialKeyword && CurrentToken.ContextualKind != SyntaxKind.AsyncKeyword && IsComplete(type2))
			{
				SyntaxToken currentToken = CurrentToken;
				type2 = AddError(type2, type2.FullWidth + currentToken.GetLeadingTriviaWidth(), currentToken.Width, ErrorCode.ERR_BadModifierLocation, currentToken.Text);
				return _syntaxFactory.IncompleteMember(syntaxListBuilder, modifiers.ToList(), type2);
			}
			ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt;
			SyntaxToken identifierOrThisOpt;
			TypeParameterListSyntax typeParameterListOpt;
			do
			{
				if (!flag3)
				{
					if (IsOperatorKeyword())
					{
						return ParseOperatorDeclaration(syntaxListBuilder, modifiers, type2);
					}
					if (IsFieldDeclaration(isEvent: false))
					{
						if (flag2)
						{
							_termState |= TerminatorState.IsPossibleStatementStartOrStop;
						}
						return ParseNormalFieldDeclaration(syntaxListBuilder, modifiers, type2, parentKind);
					}
				}
				ParseMemberName(out explicitInterfaceOpt, out identifierOrThisOpt, out typeParameterListOpt, isEvent: false);
				if (explicitInterfaceOpt == null && identifierOrThisOpt == null && typeParameterListOpt == null)
				{
					if (syntaxListBuilder.Count == 0 && modifiers.Count == 0 && type2.IsMissing && !flag3)
					{
						return null;
					}
					IncompleteMemberSyntax incompleteMemberSyntax = _syntaxFactory.IncompleteMember(syntaxListBuilder, modifiers.ToList(), type2.IsMissing ? null : type2);
					if (incompleteMemberSyntax.ContainsDiagnostics)
					{
						return incompleteMemberSyntax;
					}
					if (parentKind == SyntaxKind.NamespaceDeclaration || (parentKind == SyntaxKind.CompilationUnit && !IsScript))
					{
						return AddErrorToLastToken(incompleteMemberSyntax, ErrorCode.ERR_NamespaceUnexpected);
					}
					return AddError(incompleteMemberSyntax, incompleteMemberSyntax.FullWidth + CurrentToken.GetLeadingTriviaWidth(), CurrentToken.Width, ErrorCode.ERR_InvalidMemberDecl, CurrentToken.Text);
				}
			}
			while (!flag3 && identifierOrThisOpt != null && ((typeParameterListOpt != null && typeParameterListOpt.ContainsDiagnostics) || (CurrentToken.Kind != SyntaxKind.OpenParenToken && CurrentToken.Kind != SyntaxKind.OpenBraceToken && CurrentToken.Kind != SyntaxKind.EqualsGreaterThanToken)) && ReconsiderTypeAsAsyncModifier(ref modifiers, ref type2, ref explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt));
			if (identifierOrThisOpt.Kind == SyntaxKind.ThisKeyword)
			{
				return ParseIndexerDeclaration(syntaxListBuilder, modifiers, type2, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
			}
			kind = CurrentToken.Kind;
			if (kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.EqualsGreaterThanToken)
			{
				return ParsePropertyDeclaration(syntaxListBuilder, modifiers, type2, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
			}
			return ParseMethodDeclaration(syntaxListBuilder, modifiers, type2, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
		}
		finally
		{
			_pool.Free(modifiers);
			_pool.Free(syntaxListBuilder);
			_termState = termState;
		}
	}

	private bool ReconsiderTypeAsAsyncModifier(ref SyntaxListBuilder modifiers, ref TypeSyntax type, ref ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifierOrThisOpt, TypeParameterListSyntax typeParameterListOpt)
	{
		if (type.Kind != SyntaxKind.IdentifierName)
		{
			return false;
		}
		if (identifierOrThisOpt.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		SyntaxToken identifier = ((IdentifierNameSyntax)type).Identifier;
		SyntaxKind contextualKind = identifier.ContextualKind;
		if (contextualKind != SyntaxKind.AsyncKeyword || modifiers.Any((int)contextualKind))
		{
			return false;
		}
		modifiers.Add(SyntaxParser.ConvertToKeyword(identifier));
		SimpleNameSyntax simpleNameSyntax = ((typeParameterListOpt == null) ? ((SimpleNameSyntax)_syntaxFactory.IdentifierName(identifierOrThisOpt)) : ((SimpleNameSyntax)_syntaxFactory.GenericName(identifierOrThisOpt, TypeArgumentFromTypeParameters(typeParameterListOpt))));
		type = ((explicitInterfaceOpt == null) ? ((NameSyntax)simpleNameSyntax) : ((NameSyntax)_syntaxFactory.QualifiedName(explicitInterfaceOpt.Name, explicitInterfaceOpt.DotToken, simpleNameSyntax)));
		explicitInterfaceOpt = null;
		identifierOrThisOpt = null;
		typeParameterListOpt = null;
		return true;
	}

	private TypeArgumentListSyntax TypeArgumentFromTypeParameters(TypeParameterListSyntax typeParameterList)
	{
		SeparatedSyntaxListBuilder<TypeSyntax> item = _pool.AllocateSeparated<TypeSyntax>();
		foreach (GreenNode withSeparator in typeParameterList.Parameters.GetWithSeparators())
		{
			switch ((SyntaxKind)(ushort)withSeparator.RawKind)
			{
			case SyntaxKind.TypeParameter:
			{
				TypeParameterSyntax typeParameterSyntax = (TypeParameterSyntax)withSeparator;
				IdentifierNameSyntax node = _syntaxFactory.IdentifierName(typeParameterSyntax.Identifier);
				if (typeParameterSyntax.VarianceKeyword != null)
				{
					node = AddLeadingSkippedSyntax(node, typeParameterSyntax.VarianceKeyword);
				}
				if (typeParameterSyntax.AttributeLists.Node != null)
				{
					node = AddLeadingSkippedSyntax(node, typeParameterSyntax.AttributeLists.Node);
				}
				item.Add(node);
				break;
			}
			case SyntaxKind.CommaToken:
				item.AddSeparator((SyntaxToken)withSeparator);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(withSeparator.RawKind);
			}
		}
		TypeArgumentListSyntax result = _syntaxFactory.TypeArgumentList(typeParameterList.LessThanToken, item.ToList(), typeParameterList.GreaterThanToken);
		_pool.Free(in item);
		return result;
	}

	private bool IsFieldDeclaration(bool isEvent)
	{
		if (CurrentToken.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		switch (PeekToken(1).Kind)
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.ColonColonToken:
		case SyntaxKind.EqualsGreaterThanToken:
			return false;
		case SyntaxKind.OpenParenToken:
			return isEvent;
		default:
			return true;
		}
	}

	private bool IsOperatorKeyword()
	{
		if (CurrentToken.Kind != SyntaxKind.ImplicitKeyword && CurrentToken.Kind != SyntaxKind.ExplicitKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.OperatorKeyword;
		}
		return true;
	}

	public static bool IsComplete(CSharpSyntaxNode node)
	{
		if (node == null)
		{
			return false;
		}
		foreach (GreenNode item in node.ChildNodesAndTokens().Reverse())
		{
			if (!(item is SyntaxToken syntaxToken))
			{
				return IsComplete((CSharpSyntaxNode)item);
			}
			if (syntaxToken.IsMissing)
			{
				return false;
			}
			if (syntaxToken.Kind != SyntaxKind.None)
			{
				return true;
			}
		}
		return true;
	}

	private ConstructorDeclarationSyntax ParseConstructorDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken identifier = ParseIdentifierToken();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		try
		{
			ParameterListSyntax parameterList = ParseParenthesizedParameterList();
			ConstructorInitializerSyntax initializer = ((CurrentToken.Kind == SyntaxKind.ColonToken) ? ParseConstructorInitializer() : null);
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon, parseSemicolonAfterBlock: true, MessageID.IDS_FeatureExpressionBodiedDeOrConstructor);
			return _syntaxFactory.ConstructorDeclaration(attributes, modifiers.ToList(), identifier, parameterList, initializer, blockBody, expressionBody, semicolon);
		}
		finally
		{
			_termState = termState;
		}
	}

	private ConstructorInitializerSyntax ParseConstructorInitializer()
	{
		SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		bool reportError = true;
		SyntaxKind kind = ((CurrentToken.Kind == SyntaxKind.BaseKeyword) ? SyntaxKind.BaseConstructorInitializer : SyntaxKind.ThisConstructorInitializer);
		SyntaxToken thisOrBaseKeyword;
		if (CurrentToken.Kind == SyntaxKind.BaseKeyword || CurrentToken.Kind == SyntaxKind.ThisKeyword)
		{
			thisOrBaseKeyword = EatToken();
		}
		else
		{
			thisOrBaseKeyword = EatToken(SyntaxKind.ThisKeyword, ErrorCode.ERR_ThisOrBaseExpected);
			reportError = false;
		}
		ArgumentListSyntax argumentList;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			argumentList = ParseParenthesizedArgumentList();
		}
		else
		{
			SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken, reportError);
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken, reportError);
			argumentList = _syntaxFactory.ArgumentList(openParenToken, default, closeParenToken);
		}
		return _syntaxFactory.ConstructorInitializer(kind, colonToken, thisOrBaseKeyword, argumentList);
	}

	private DestructorDeclarationSyntax ParseDestructorDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken tildeToken = EatToken(SyntaxKind.TildeToken);
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon, parseSemicolonAfterBlock: true, MessageID.IDS_FeatureExpressionBodiedDeOrConstructor);
		ParameterListSyntax parameterList = _syntaxFactory.ParameterList(openParenToken, default, closeParenToken);
		return _syntaxFactory.DestructorDeclaration(attributes, modifiers.ToList(), tildeToken, identifier, parameterList, blockBody, expressionBody, semicolon);
	}

	private void ParseBlockAndExpressionBodiesWithSemicolon(out BlockSyntax blockBody, out ArrowExpressionClauseSyntax expressionBody, out SyntaxToken semicolon, bool parseSemicolonAfterBlock = true, MessageID requestedExpressionBodyFeature = MessageID.IDS_FeatureExpressionBodiedMethod)
	{
		if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			blockBody = null;
			expressionBody = null;
			semicolon = EatToken(SyntaxKind.SemicolonToken);
			return;
		}
		blockBody = null;
		expressionBody = null;
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			blockBody = ParseBlock(isMethodBody: true);
		}
		if (CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			expressionBody = ParseArrowExpressionClause();
			expressionBody = CheckFeatureAvailability(expressionBody, requestedExpressionBodyFeature);
		}
		semicolon = null;
		if (expressionBody != null || blockBody == null)
		{
			semicolon = EatToken(SyntaxKind.SemicolonToken);
		}
		else if (parseSemicolonAfterBlock && CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolon = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
		}
	}

	private void ParseBodyOrSemicolon(out BlockSyntax body, out SyntaxToken semicolon)
	{
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			body = ParseBlock(isMethodBody: true);
			semicolon = null;
			if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				semicolon = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
			}
		}
		else
		{
			semicolon = EatToken(SyntaxKind.SemicolonToken);
			body = null;
		}
	}

	private bool IsEndOfTypeParameterList()
	{
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return true;
		}
		if (CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			return true;
		}
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return true;
		}
		if (IsCurrentTokenWhereOfConstraintClause())
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfMethodSignature()
	{
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return CurrentToken.Kind == SyntaxKind.OpenBraceToken;
		}
		return true;
	}

	private bool IsEndOfNameInExplicitInterface()
	{
		if (CurrentToken.Kind != SyntaxKind.DotToken)
		{
			return CurrentToken.Kind == SyntaxKind.ColonColonToken;
		}
		return true;
	}

	private MethodDeclarationSyntax ParseMethodDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifier, TypeParameterListSyntax typeParameterList)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList();
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder = default;
		try
		{
			if (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				syntaxListBuilder = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
				ParseTypeParameterConstraintClauses(syntaxListBuilder);
			}
			else if (CurrentToken.Kind == SyntaxKind.ColonToken)
			{
				SyntaxToken currentToken = CurrentToken;
				ConstructorInitializerSyntax node = ParseConstructorInitializer();
				node = AddErrorToFirstToken(node, ErrorCode.ERR_UnexpectedToken, currentToken.Text);
				parameterListSyntax = AddTrailingSkippedSyntax(parameterListSyntax, node);
			}
			_termState = termState;
			IsInAsync = modifiers.Any(8435);
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
			IsInAsync = false;
			return _syntaxFactory.MethodDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, identifier, typeParameterList, parameterListSyntax, syntaxListBuilder, blockBody, expressionBody, semicolon);
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
		}
	}

	private TypeSyntax ParseReturnType()
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfReturnType;
		TypeSyntax result = ParseTypeOrVoid();
		_termState = termState;
		return result;
	}

	private bool IsEndOfReturnType()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private ConversionOperatorDeclarationSyntax ParseConversionOperatorDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken implicitOrExplicitKeyword = ((CurrentToken.Kind != SyntaxKind.ImplicitKeyword && CurrentToken.Kind != SyntaxKind.ExplicitKeyword) ? EatToken(SyntaxKind.ExplicitKeyword) : EatToken());
		SyntaxToken operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
		TypeSyntax type = ParseType();
		ParameterListSyntax parameterList = ParseParenthesizedParameterList();
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
		return _syntaxFactory.ConversionOperatorDeclaration(attributes, modifiers.ToList(), implicitOrExplicitKeyword, operatorKeyword, type, parameterList, blockBody, expressionBody, semicolon);
	}

	private OperatorDeclarationSyntax ParseOperatorDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type)
	{
		SyntaxToken operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
		SyntaxToken syntaxToken;
		int offset;
		int width;
		if (SyntaxFacts.IsAnyOverloadableOperator(CurrentToken.Kind))
		{
			syntaxToken = EatToken();
			offset = syntaxToken.GetLeadingTriviaWidth();
			width = syntaxToken.Width;
		}
		else if (CurrentToken.Kind == SyntaxKind.ImplicitKeyword || CurrentToken.Kind == SyntaxKind.ExplicitKeyword)
		{
			GetDiagnosticSpanForMissingToken(out offset, out width);
			syntaxToken = ConvertToMissingWithTrailingTrivia(EatToken(), SyntaxKind.PlusToken);
			if (type.IsMissing)
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_BadOperatorSyntax, SyntaxFacts.GetText(SyntaxKind.PlusToken));
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo);
			}
			else
			{
				type = AddError(type, ErrorCode.ERR_BadOperatorSyntax, SyntaxFacts.GetText(SyntaxKind.PlusToken));
			}
		}
		else
		{
			syntaxToken = EatToken();
			offset = syntaxToken.GetLeadingTriviaWidth();
			width = syntaxToken.Width;
		}
		SyntaxKind kind = syntaxToken.Kind;
		SyntaxToken currentToken = CurrentToken;
		if (syntaxToken.Kind == SyntaxKind.GreaterThanToken && currentToken.Kind == SyntaxKind.GreaterThanToken && syntaxToken.GetTrailingTriviaWidth() == 0 && currentToken.GetLeadingTriviaWidth() == 0)
		{
			SyntaxToken syntaxToken2 = EatToken();
			syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanToken, syntaxToken2.GetTrailingTrivia());
		}
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList();
		switch (parameterListSyntax.Parameters.Count)
		{
		case 1:
			if (syntaxToken.IsMissing || !SyntaxFacts.IsOverloadableUnaryOperator(kind))
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo4 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlUnaryOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo4);
			}
			break;
		case 2:
			if (syntaxToken.IsMissing || !SyntaxFacts.IsOverloadableBinaryOperator(kind))
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlBinaryOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo3);
			}
			break;
		default:
			if (syntaxToken.IsMissing)
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo2);
			}
			else if (SyntaxFacts.IsOverloadableBinaryOperator(kind))
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadBinOpArgs, SyntaxFacts.GetText(kind));
			}
			else
			{
				syntaxToken = ((!SyntaxFacts.IsOverloadableUnaryOperator(kind)) ? AddError(syntaxToken, ErrorCode.ERR_OvlOperatorExpected) : AddError(syntaxToken, ErrorCode.ERR_BadUnOpArgs, SyntaxFacts.GetText(kind)));
			}
			break;
		}
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
		if (kind != SyntaxKind.IsKeyword && !SyntaxFacts.IsOverloadableUnaryOperator(kind) && !SyntaxFacts.IsOverloadableBinaryOperator(kind))
		{
			syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.PlusToken);
		}
		return _syntaxFactory.OperatorDeclaration(attributes, modifiers.ToList(), type, operatorKeyword, syntaxToken, parameterListSyntax, blockBody, expressionBody, semicolon);
	}

	private IndexerDeclarationSyntax ParseIndexerDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken thisKeyword, TypeParameterListSyntax typeParameterList)
	{
		if (typeParameterList != null)
		{
			thisKeyword = AddTrailingSkippedSyntax(thisKeyword, typeParameterList);
			thisKeyword = AddError(thisKeyword, ErrorCode.ERR_UnexpectedGenericName);
		}
		BracketedParameterListSyntax parameterList = ParseBracketedParameterList();
		AccessorListSyntax accessorList = null;
		ArrowExpressionClauseSyntax expressionBody = null;
		SyntaxToken syntaxToken = null;
		if (CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			expressionBody = ParseArrowExpressionClause();
			expressionBody = CheckFeatureAvailability(expressionBody, MessageID.IDS_FeatureExpressionBodiedIndexer);
			syntaxToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			accessorList = ParseAccessorList(isEvent: false);
			if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				syntaxToken = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
			}
		}
		if (CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken && syntaxToken == null)
		{
			expressionBody = ParseArrowExpressionClause();
			expressionBody = CheckFeatureAvailability(expressionBody, MessageID.IDS_FeatureExpressionBodiedIndexer);
			syntaxToken = EatToken(SyntaxKind.SemicolonToken);
		}
		return _syntaxFactory.IndexerDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, thisKeyword, parameterList, accessorList, expressionBody, syntaxToken);
	}

	private PropertyDeclarationSyntax ParsePropertyDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifier, TypeParameterListSyntax typeParameterList)
	{
		if (typeParameterList != null)
		{
			identifier = AddTrailingSkippedSyntax(identifier, typeParameterList);
			identifier = AddError(identifier, ErrorCode.ERR_UnexpectedGenericName);
		}
		AccessorListSyntax accessorList = null;
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			accessorList = ParseAccessorList(isEvent: false);
		}
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = null;
		EqualsValueClauseSyntax equalsValueClauseSyntax = null;
		if (CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			arrowExpressionClauseSyntax = ParseArrowExpressionClause();
			arrowExpressionClauseSyntax = CheckFeatureAvailability(arrowExpressionClauseSyntax, MessageID.IDS_FeatureExpressionBodiedProperty);
		}
		else if (CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			ExpressionSyntax value = ParseVariableInitializer();
			equalsValueClauseSyntax = _syntaxFactory.EqualsValueClause(equalsToken, value);
			equalsValueClauseSyntax = CheckFeatureAvailability(equalsValueClauseSyntax, MessageID.IDS_FeatureAutoPropertyInitializer);
		}
		SyntaxToken semicolonToken = null;
		if (arrowExpressionClauseSyntax != null || equalsValueClauseSyntax != null)
		{
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolonToken = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
		}
		return _syntaxFactory.PropertyDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, identifier, accessorList, arrowExpressionClauseSyntax, equalsValueClauseSyntax, semicolonToken);
	}

	private AccessorListSyntax ParseAccessorList(bool isEvent)
	{
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors = default;
		if (!openBrace.IsMissing || !IsTerminator())
		{
			SyntaxListBuilder<AccessorDeclarationSyntax> syntaxListBuilder = _pool.Allocate<AccessorDeclarationSyntax>();
			try
			{
				while (CurrentToken.Kind != SyntaxKind.CloseBraceToken)
				{
					if (IsPossibleAccessor())
					{
						AccessorDeclarationSyntax node = ParseAccessorDeclaration(isEvent);
						syntaxListBuilder.Add(node);
					}
					else if (SkipBadAccessorListTokens(ref openBrace, syntaxListBuilder, isEvent ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected) == PostSkipAction.Abort)
					{
						break;
					}
				}
				accessors = syntaxListBuilder.ToList();
			}
			finally
			{
				_pool.Free(syntaxListBuilder);
			}
		}
		SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return _syntaxFactory.AccessorList(openBrace, accessors, closeBraceToken);
	}

	private ArrowExpressionClauseSyntax ParseArrowExpressionClause()
	{
		SyntaxToken arrowToken = EatToken(SyntaxKind.EqualsGreaterThanToken);
		return _syntaxFactory.ArrowExpressionClause(arrowToken, ParsePossibleRefExpression());
	}

	private ExpressionSyntax ParsePossibleRefExpression()
	{
		SyntaxToken syntaxToken = null;
		if (CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			syntaxToken = EatToken();
			syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureRefLocalsReturns);
		}
		ExpressionSyntax expressionSyntax = ParseExpressionCore();
		if (syntaxToken != null)
		{
			expressionSyntax = _syntaxFactory.RefExpression(syntaxToken, expressionSyntax);
		}
		return expressionSyntax;
	}

	private PostSkipAction SkipBadAccessorListTokens(ref SyntaxToken openBrace, SyntaxListBuilder<AccessorDeclarationSyntax> list, ErrorCode error)
	{
		return SkipBadListTokensWithErrorCode(ref openBrace, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CloseBraceToken && !p.IsPossibleAccessor(), (LanguageParser p) => p.IsTerminator(), error);
	}

	private bool IsPossibleAccessor()
	{
		if (CurrentToken.Kind != SyntaxKind.IdentifierToken && !IsPossibleAttributeDeclaration() && SyntaxFacts.GetAccessorDeclarationKind(CurrentToken.ContextualKind) == SyntaxKind.None && CurrentToken.Kind != SyntaxKind.OpenBraceToken && CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return IsPossibleAccessorModifier();
		}
		return true;
	}

	private bool IsPossibleAccessorModifier()
	{
		if (GetModifier(CurrentToken) == DeclarationModifiers.None)
		{
			return false;
		}
		int i;
		for (i = 1; GetModifier(PeekToken(i)) != DeclarationModifiers.None; i++)
		{
		}
		SyntaxToken syntaxToken = PeekToken(i);
		if (syntaxToken.Kind == SyntaxKind.CloseBraceToken || syntaxToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return true;
		}
		SyntaxKind contextualKind = syntaxToken.ContextualKind;
		if (contextualKind - 8417 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	private PostSkipAction SkipBadSeparatedListTokensWithExpectedKind<T, TNode>(ref T startToken, SeparatedSyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, SyntaxKind expected) where T : CSharpSyntaxNode where TNode : CSharpSyntaxNode
	{
		PostSkipAction result = SkipBadListTokensWithExpectedKindHelper(list.UnderlyingBuilder, isNotExpectedFunction, abortFunction, expected, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
		}
		return result;
	}

	private PostSkipAction SkipBadListTokensWithErrorCode<T, TNode>(ref T startToken, SyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode error) where T : CSharpSyntaxNode where TNode : CSharpSyntaxNode
	{
		PostSkipAction result = SkipBadListTokensWithErrorCodeHelper(list, isNotExpectedFunction, abortFunction, error, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
		}
		return result;
	}

	private PostSkipAction SkipBadListTokensWithExpectedKindHelper(SyntaxListBuilder list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, SyntaxKind expected, out GreenNode trailingTrivia)
	{
		if (list.Count == 0)
		{
			return SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, out trailingTrivia);
		}
		PostSkipAction result = SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, out var trailingTrivia2);
		if (trailingTrivia2 != null)
		{
			AddTrailingSkippedSyntax(list, trailingTrivia2);
		}
		trailingTrivia = null;
		return result;
	}

	private PostSkipAction SkipBadListTokensWithErrorCodeHelper<TNode>(SyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode error, out GreenNode trailingTrivia) where TNode : CSharpSyntaxNode
	{
		if (list.Count == 0)
		{
			return SkipBadTokensWithErrorCode(isNotExpectedFunction, abortFunction, error, out trailingTrivia);
		}
		PostSkipAction result = SkipBadTokensWithErrorCode(isNotExpectedFunction, abortFunction, error, out var trailingTrivia2);
		if (trailingTrivia2 != null)
		{
			AddTrailingSkippedSyntax(list, trailingTrivia2);
		}
		trailingTrivia = null;
		return result;
	}

	private PostSkipAction SkipBadTokensWithExpectedKind(Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, SyntaxKind expected, out GreenNode trailingTrivia)
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
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
				SyntaxToken item = ((flag && !CurrentToken.ContainsDiagnostics) ? EatTokenWithPrejudice(expected) : EatToken());
				flag = false;
				syntaxListBuilder.Add(item);
			}
			trailingTrivia = ((syntaxListBuilder.Count > 0) ? syntaxListBuilder.ToListNode() : null);
			return result;
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private PostSkipAction SkipBadTokensWithErrorCode(Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode errorCode, out GreenNode trailingTrivia)
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
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
				SyntaxToken item = ((flag && !CurrentToken.ContainsDiagnostics) ? EatTokenWithPrejudice(errorCode) : EatToken());
				flag = false;
				syntaxListBuilder.Add(item);
			}
			trailingTrivia = ((syntaxListBuilder.Count > 0) ? syntaxListBuilder.ToListNode() : null);
			return result;
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private AccessorDeclarationSyntax ParseAccessorDeclaration(bool isEvent)
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseAccessorDeclaration())
		{
			return (AccessorDeclarationSyntax)EatNode();
		}
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		SyntaxListBuilder syntaxListBuilder2 = _pool.Allocate();
		try
		{
			ParseAttributeDeclarations(syntaxListBuilder);
			ParseModifiers(syntaxListBuilder2, forAccessors: true);
			if (!isEvent && syntaxListBuilder2 != null && syntaxListBuilder2.Count > 0)
			{
				syntaxListBuilder2[0] = CheckFeatureAvailability(syntaxListBuilder2[0], MessageID.IDS_FeaturePropertyAccessorMods);
			}
			SyntaxToken syntaxToken = EatToken(SyntaxKind.IdentifierToken, isEvent ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected);
			SyntaxKind accessorKind = GetAccessorKind(syntaxToken);
			if (accessorKind == SyntaxKind.UnknownAccessorDeclaration)
			{
				if (!syntaxToken.IsMissing)
				{
					syntaxToken = AddError(syntaxToken, isEvent ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected);
				}
			}
			else
			{
				syntaxToken = SyntaxParser.ConvertToKeyword(syntaxToken);
			}
			BlockSyntax blockBody = null;
			ArrowExpressionClauseSyntax expressionBody = null;
			SyntaxToken semicolon = null;
			bool flag = CurrentToken.Kind == SyntaxKind.SemicolonToken;
			bool flag2 = CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken;
			if ((CurrentToken.Kind == SyntaxKind.OpenBraceToken) | flag2)
			{
				ParseBlockAndExpressionBodiesWithSemicolon(out blockBody, out expressionBody, out semicolon, parseSemicolonAfterBlock: true, MessageID.IDS_FeatureExpressionBodiedAccessor);
			}
			else if (flag)
			{
				semicolon = EatAccessorSemicolon();
				if (accessorKind == SyntaxKind.AddAccessorDeclaration || accessorKind == SyntaxKind.RemoveAccessorDeclaration)
				{
					semicolon = AddError(semicolon, ErrorCode.ERR_AddRemoveMustHaveBody);
				}
			}
			else if (accessorKind != SyntaxKind.UnknownAccessorDeclaration)
			{
				if (!IsTerminator())
				{
					blockBody = ParseBlock(isMethodBody: true, isAccessorBody: true);
				}
				else
				{
					semicolon = EatAccessorSemicolon();
				}
			}
			return _syntaxFactory.AccessorDeclaration(accessorKind, syntaxListBuilder, syntaxListBuilder2.ToList(), syntaxToken, blockBody, expressionBody, semicolon);
		}
		finally
		{
			_pool.Free(syntaxListBuilder2);
			_pool.Free(syntaxListBuilder);
		}
	}

	private SyntaxToken EatAccessorSemicolon()
	{
		return EatToken(SyntaxKind.SemicolonToken, IsFeatureEnabled(MessageID.IDS_FeatureExpressionBodiedAccessor) ? ErrorCode.ERR_SemiOrLBraceOrArrowExpected : ErrorCode.ERR_SemiOrLBraceExpected);
	}

	private SyntaxKind GetAccessorKind(SyntaxToken accessorName)
	{
		return accessorName.ContextualKind switch
		{
			SyntaxKind.GetKeyword => SyntaxKind.GetAccessorDeclaration, 
			SyntaxKind.SetKeyword => SyntaxKind.SetAccessorDeclaration, 
			SyntaxKind.AddKeyword => SyntaxKind.AddAccessorDeclaration, 
			SyntaxKind.RemoveKeyword => SyntaxKind.RemoveAccessorDeclaration, 
			_ => SyntaxKind.UnknownAccessorDeclaration, 
		};
	}

	private bool CanReuseAccessorDeclaration()
	{
		SyntaxKind currentNodeKind = CurrentNodeKind;
		if (currentNodeKind - 8896 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	internal ParameterListSyntax ParseParenthesizedParameterList()
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseParameterList(CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax))
		{
			return (ParameterListSyntax)EatNode();
		}
		SeparatedSyntaxListBuilder<ParameterSyntax> builder = _pool.AllocateSeparated<ParameterSyntax>();
		try
		{
			SyntaxKind openKind = SyntaxKind.OpenParenToken;
			SyntaxKind closeKind = SyntaxKind.CloseParenToken;
			ParseParameterList(out var open, builder, out var close, openKind, closeKind);
			return _syntaxFactory.ParameterList(open, builder, close);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	internal BracketedParameterListSyntax ParseBracketedParameterList()
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseBracketedParameterList(CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax))
		{
			return (BracketedParameterListSyntax)EatNode();
		}
		SeparatedSyntaxListBuilder<ParameterSyntax> builder = _pool.AllocateSeparated<ParameterSyntax>();
		try
		{
			SyntaxKind openKind = SyntaxKind.OpenBracketToken;
			SyntaxKind closeKind = SyntaxKind.CloseBracketToken;
			ParseParameterList(out var open, builder, out var close, openKind, closeKind);
			return _syntaxFactory.BracketedParameterList(open, builder, close);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private static bool CanReuseParameterList(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax list)
	{
		if (list == null)
		{
			return false;
		}
		if (list.OpenParenToken.IsMissing)
		{
			return false;
		}
		if (list.CloseParenToken.IsMissing)
		{
			return false;
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter in list.Parameters)
		{
			if (!CanReuseParameter(parameter))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CanReuseBracketedParameterList(Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax list)
	{
		if (list == null)
		{
			return false;
		}
		if (list.OpenBracketToken.IsMissing)
		{
			return false;
		}
		if (list.CloseBracketToken.IsMissing)
		{
			return false;
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter in list.Parameters)
		{
			if (!CanReuseParameter(parameter))
			{
				return false;
			}
		}
		return true;
	}

	private void ParseParameterList(out SyntaxToken open, SeparatedSyntaxListBuilder<ParameterSyntax> nodes, out SyntaxToken close, SyntaxKind openKind, SyntaxKind closeKind)
	{
		open = EatToken(openKind);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfParameterList;
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		SyntaxListBuilder syntaxListBuilder2 = _pool.Allocate();
		try
		{
			if (CurrentToken.Kind != closeKind)
			{
				do
				{
					if (!IsPossibleParameter() && CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						continue;
					}
					syntaxListBuilder.Clear();
					syntaxListBuilder2.Clear();
					ParameterSyntax node = ParseParameter(syntaxListBuilder, syntaxListBuilder2);
					nodes.Add(node);
					while (CurrentToken.Kind != closeKind)
					{
						if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleParameter())
						{
							nodes.AddSeparator(EatToken(SyntaxKind.CommaToken));
							syntaxListBuilder.Clear();
							syntaxListBuilder2.Clear();
							node = ParseParameter(syntaxListBuilder, syntaxListBuilder2);
							if (node.IsMissing && IsPossibleParameter())
							{
								node = AddTrailingSkippedSyntax(node, EatToken());
							}
							nodes.Add(node);
						}
						else if (SkipBadParameterListTokens(ref open, nodes, SyntaxKind.CommaToken, closeKind) == PostSkipAction.Abort)
						{
							break;
						}
					}
					break;
				}
				while (SkipBadParameterListTokens(ref open, nodes, SyntaxKind.IdentifierToken, closeKind) == PostSkipAction.Continue);
			}
			_termState = termState;
			close = EatToken(closeKind);
		}
		finally
		{
			_pool.Free(syntaxListBuilder2);
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsEndOfParameterList()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return CurrentToken.Kind == SyntaxKind.CloseBracketToken;
		}
		return true;
	}

	private PostSkipAction SkipBadParameterListTokens(ref SyntaxToken open, SeparatedSyntaxListBuilder<ParameterSyntax> list, SyntaxKind expected, SyntaxKind closeKind)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref open, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleParameter(), (LanguageParser p) => p.CurrentToken.Kind == closeKind || p.IsTerminator(), expected);
	}

	private bool IsPossibleParameter()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.ArgListKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		default:
			if (!IsParameterModifier(CurrentToken.Kind))
			{
				return IsPredefinedType(CurrentToken.Kind);
			}
			return true;
		}
	}

	private static bool CanReuseParameter(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter, SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		if (parameter == null)
		{
			return false;
		}
		if (attributes.Count != 0 || parameter.AttributeLists.Count != 0)
		{
			return false;
		}
		if ((modifiers != null && modifiers.Count != 0) || parameter.Modifiers.Count != 0)
		{
			return false;
		}
		return CanReuseParameter(parameter);
	}

	private static bool CanReuseParameter(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter)
	{
		if (parameter.Default != null)
		{
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode parent = parameter.Parent;
		if (parent != null)
		{
			if (parent.Kind() == SyntaxKind.SimpleLambdaExpression)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode parent2 = parent.Parent;
			if (parent2 != null && parent2.Kind() == SyntaxKind.ParenthesizedLambdaExpression)
			{
				return false;
			}
		}
		return true;
	}

	private ParameterSyntax ParseParameter(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseParameter(CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax, attributes, modifiers))
		{
			return (ParameterSyntax)EatNode();
		}
		ParseAttributeDeclarations(attributes);
		ParseParameterModifiers(modifiers);
		TypeSyntax type;
		SyntaxToken syntaxToken;
		if (CurrentToken.Kind != SyntaxKind.ArgListKeyword)
		{
			type = ParseType(ParseTypeMode.Parameter);
			syntaxToken = ParseIdentifierToken();
			if (CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind == SyntaxKind.CloseBracketToken)
			{
				SyntaxToken node = EatToken();
				SyntaxToken child = EatToken();
				node = AddError(node, ErrorCode.ERR_BadArraySyntax);
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, SyntaxList.List(node, child));
			}
		}
		else
		{
			type = null;
			syntaxToken = EatToken(SyntaxKind.ArgListKeyword);
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = null;
		if (CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			ExpressionSyntax value = ParseExpressionCore();
			equalsValueClauseSyntax = _syntaxFactory.EqualsValueClause(equalsToken, value);
			equalsValueClauseSyntax = CheckFeatureAvailability(equalsValueClauseSyntax, MessageID.IDS_FeatureOptionalParameter);
		}
		return _syntaxFactory.Parameter(attributes, modifiers.ToList(), type, syntaxToken, equalsValueClauseSyntax);
	}

	private static bool IsParameterModifier(SyntaxKind kind)
	{
		if (kind - 8360 <= (SyntaxKind)2 || kind == SyntaxKind.ParamsKeyword || kind == SyntaxKind.ThisKeyword)
		{
			return true;
		}
		return false;
	}

	private void ParseParameterModifiers(SyntaxListBuilder modifiers)
	{
		while (IsParameterModifier(CurrentToken.Kind))
		{
			SyntaxToken syntaxToken = EatToken();
			switch (syntaxToken.Kind)
			{
			case SyntaxKind.ThisKeyword:
				syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureExtensionMethod);
				break;
			case SyntaxKind.RefKeyword:
				if (CurrentToken.Kind == SyntaxKind.ThisKeyword)
				{
					syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureRefExtensionMethods);
				}
				break;
			case SyntaxKind.InKeyword:
				syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureReadOnlyReferences);
				if (CurrentToken.Kind == SyntaxKind.ThisKeyword)
				{
					syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureRefExtensionMethods);
				}
				break;
			}
			modifiers.Add(syntaxToken);
		}
	}

	private FieldDeclarationSyntax ParseFixedSizeBufferDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		SyntaxToken node = EatToken();
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureFixedBuffer);
		modifiers.Add(node);
		TypeSyntax type = ParseType();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFieldDeclaration;
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			ParseVariableDeclarators(type, VariableFlags.Fixed, builder, parentKind);
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, builder), semicolonToken);
		}
		finally
		{
			_termState = termState;
			_pool.Free(in builder);
		}
	}

	private MemberDeclarationSyntax ParseEventDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		SyntaxToken eventToken = EatToken();
		TypeSyntax type = ParseType();
		if (IsFieldDeclaration(isEvent: true))
		{
			return ParseEventFieldDeclaration(attributes, modifiers, eventToken, type, parentKind);
		}
		return ParseEventDeclarationWithAccessors(attributes, modifiers, eventToken, type);
	}

	private EventDeclarationSyntax ParseEventDeclarationWithAccessors(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxToken eventToken, TypeSyntax type)
	{
		ParseMemberName(out var explicitInterfaceOpt, out var identifierOrThisOpt, out var typeParameterListOpt, isEvent: true);
		if (explicitInterfaceOpt != null && identifierOrThisOpt == null)
		{
			SyntaxToken identifier = CreateMissingIdentifierToken();
			AccessorListSyntax accessorList = _syntaxFactory.AccessorList(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken), default, SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken));
			return _syntaxFactory.EventDeclaration(attributes, modifiers.ToList(), eventToken, type, explicitInterfaceOpt, identifier, accessorList);
		}
		SyntaxToken syntaxToken;
		if (identifierOrThisOpt == null)
		{
			syntaxToken = CreateMissingIdentifierToken();
		}
		else
		{
			syntaxToken = ((identifierOrThisOpt.Kind == SyntaxKind.IdentifierToken) ? identifierOrThisOpt : ConvertToMissingWithTrailingTrivia(identifierOrThisOpt, SyntaxKind.IdentifierToken));
		}
		if (syntaxToken.IsMissing && !type.IsMissing)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_IdentifierExpected);
		}
		if (typeParameterListOpt != null)
		{
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, typeParameterListOpt);
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_UnexpectedGenericName);
		}
		AccessorListSyntax accessorList2 = ParseAccessorList(isEvent: true);
		EventDeclarationSyntax decl = _syntaxFactory.EventDeclaration(attributes, modifiers.ToList(), eventToken, type, explicitInterfaceOpt, syntaxToken, accessorList2);
		return EatUnexpectedTrailingSemicolon(decl);
	}

	private TNode EatUnexpectedTrailingSemicolon<TNode>(TNode decl) where TNode : CSharpSyntaxNode
	{
		if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			SyntaxToken node = EatToken();
			node = AddError(node, ErrorCode.ERR_UnexpectedSemicolon);
			decl = AddTrailingSkippedSyntax(decl, node);
		}
		return decl;
	}

	private FieldDeclarationSyntax ParseNormalFieldDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, SyntaxKind parentKind)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFieldDeclaration;
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			ParseVariableDeclarators(type, (VariableFlags)0, builder, parentKind);
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, builder), semicolonToken);
		}
		finally
		{
			_termState = termState;
			_pool.Free(in builder);
		}
	}

	private EventFieldDeclarationSyntax ParseEventFieldDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxToken eventToken, TypeSyntax type, SyntaxKind parentKind)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFieldDeclaration;
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			ParseVariableDeclarators(type, (VariableFlags)0, builder, parentKind);
			if (CurrentToken.Kind == SyntaxKind.DotToken)
			{
				eventToken = AddError(eventToken, ErrorCode.ERR_ExplicitEventFieldImpl);
			}
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.EventFieldDeclaration(attributes, modifiers.ToList(), eventToken, _syntaxFactory.VariableDeclaration(type, builder), semicolonToken);
		}
		finally
		{
			_termState = termState;
			_pool.Free(in builder);
		}
	}

	private bool IsEndOfFieldDeclaration()
	{
		return CurrentToken.Kind == SyntaxKind.SemicolonToken;
	}

	private void ParseVariableDeclarators(TypeSyntax type, VariableFlags flags, SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> variables, SyntaxKind parentKind)
	{
		ParseVariableDeclarators(type, flags, variables, parentKind switch
		{
			SyntaxKind.CompilationUnit => IsScript, 
			SyntaxKind.NamespaceDeclaration => false, 
			_ => true, 
		}, allowLocalFunctions: false, default, out var _);
	}

	private void ParseVariableDeclarators(TypeSyntax type, VariableFlags flags, SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> variables, bool variableDeclarationsExpected, bool allowLocalFunctions, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, out LocalFunctionStatementSyntax localFunction)
	{
		variables.Add(ParseVariableDeclarator(type, flags, isFirst: true, allowLocalFunctions, mods, out localFunction));
		if (localFunction != null)
		{
			return;
		}
		while (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			if (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				variables.AddSeparator(EatToken(SyntaxKind.CommaToken));
				variables.Add(ParseVariableDeclarator(type, flags, isFirst: false, allowLocalFunctions: false, mods, out localFunction));
			}
			else if (!variableDeclarationsExpected || SkipBadVariableListTokens(variables, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
	}

	private PostSkipAction SkipBadVariableListTokens(SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p) => CurrentToken.Kind == SyntaxKind.SemicolonToken || IsTerminator(), expected);
	}

	private static SyntaxTokenList GetOriginalModifiers(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode decl)
	{
		if (decl != null)
		{
			switch (decl.Kind())
			{
			case SyntaxKind.FieldDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.MethodDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.ConstructorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.DestructorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.PropertyDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.EventFieldDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
			case SyntaxKind.AddAccessorDeclaration:
			case SyntaxKind.RemoveAccessorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.ClassDeclaration:
			case SyntaxKind.StructDeclaration:
			case SyntaxKind.InterfaceDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.DelegateDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax)decl).Modifiers;
			}
		}
		return default;
	}

	private static bool WasFirstVariable(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variable)
	{
		if (GetOldParent(variable) is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax { Variables: var variables })
		{
			return variables[0] == variable;
		}
		return false;
	}

	private static VariableFlags GetOriginalVariableFlags(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax old)
	{
		Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldParent = GetOldParent(old);
		SyntaxTokenList originalModifiers = GetOriginalModifiers(oldParent);
		VariableFlags variableFlags = (VariableFlags)0;
		if (originalModifiers.Any(SyntaxKind.FixedKeyword))
		{
			variableFlags |= VariableFlags.Fixed;
		}
		if (originalModifiers.Any(SyntaxKind.ConstKeyword))
		{
			variableFlags |= VariableFlags.Const;
		}
		if (oldParent != null && (oldParent.Kind() == SyntaxKind.VariableDeclaration || oldParent.Kind() == SyntaxKind.LocalDeclarationStatement))
		{
			variableFlags |= VariableFlags.Local;
		}
		return variableFlags;
	}

	private static bool CanReuseVariableDeclarator(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax old, VariableFlags flags, bool isFirst)
	{
		if (old == null)
		{
			return false;
		}
		SyntaxKind syntaxKind;
		if (flags == GetOriginalVariableFlags(old) && isFirst == WasFirstVariable(old) && old.Initializer == null && (syntaxKind = GetOldParent(old).Kind()) != SyntaxKind.VariableDeclaration)
		{
			return syntaxKind != SyntaxKind.LocalDeclarationStatement;
		}
		return false;
	}

	private VariableDeclaratorSyntax ParseVariableDeclarator(TypeSyntax parentType, VariableFlags flags, bool isFirst, bool allowLocalFunctions, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, out LocalFunctionStatementSyntax localFunction, bool isExpressionContext = false)
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseVariableDeclarator(CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax, flags, isFirst))
		{
			localFunction = null;
			return (VariableDeclaratorSyntax)EatNode();
		}
		if (!isExpressionContext)
		{
			ResetPoint state = GetResetPoint();
			try
			{
				SyntaxKind kind = CurrentToken.Kind;
				if (kind == SyntaxKind.IdentifierToken && !parentType.IsMissing && parentType.GetLastToken().TrailingTrivia.Any(8539))
				{
					GetDiagnosticSpanForMissingToken(out var offset, out var width);
					EatToken();
					kind = CurrentToken.Kind;
					bool flag = kind != SyntaxKind.EqualsToken && SyntaxFacts.IsBinaryExpressionOperatorToken(kind);
					if (((kind == SyntaxKind.DotToken || kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.MinusGreaterThanToken) | flag) && ((kind != SyntaxKind.OpenParenToken && kind != SyntaxKind.LessThanToken) || !IsLocalFunctionAfterIdentifier()))
					{
						SyntaxToken node = CreateMissingIdentifierToken();
						node = AddError(node, offset, width, ErrorCode.ERR_IdentifierExpected);
						localFunction = null;
						return _syntaxFactory.VariableDeclarator(node, null, null);
					}
				}
			}
			finally
			{
				Reset(ref state);
				Release(ref state);
			}
		}
		SyntaxToken syntaxToken = ParseIdentifierToken();
		BracketedArgumentListSyntax bracketedArgumentListSyntax = null;
		EqualsValueClauseSyntax initializer = null;
		TerminatorState termState = _termState;
		bool flag2 = (flags & VariableFlags.Fixed) != 0;
		bool flag3 = (flags & VariableFlags.Const) != 0;
		bool flag4 = (flags & VariableFlags.Local) != 0;
		if (!isFirst && IsTrueIdentifier())
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_MultiTypeInDeclaration);
		}
		SyntaxKind kind2 = CurrentToken.Kind;
		if (kind2 <= SyntaxKind.EqualsToken)
		{
			if (kind2 == SyntaxKind.OpenParenToken)
			{
				if (allowLocalFunctions & isFirst)
				{
					localFunction = TryParseLocalFunctionStatementBody(mods, parentType, syntaxToken);
					if (localFunction != null)
					{
						return null;
					}
				}
				_termState |= TerminatorState.IsPossibleEndOfVariableDeclaration;
				bracketedArgumentListSyntax = ParseBracketedArgumentList();
				_termState = termState;
				bracketedArgumentListSyntax = AddError(bracketedArgumentListSyntax, ErrorCode.ERR_BadVarDecl);
				goto IL_03ef;
			}
			if (kind2 == SyntaxKind.EqualsToken)
			{
				goto IL_01cf;
			}
		}
		else
		{
			if (kind2 == SyntaxKind.OpenBracketToken)
			{
				goto IL_02b1;
			}
			if (kind2 == SyntaxKind.LessThanToken && (allowLocalFunctions & isFirst))
			{
				localFunction = TryParseLocalFunctionStatementBody(mods, parentType, syntaxToken);
				if (localFunction != null)
				{
					return null;
				}
			}
		}
		goto IL_03bb;
		IL_01cf:
		if (flag2)
		{
			goto IL_03bb;
		}
		SyntaxToken equalsToken = EatToken();
		SyntaxToken syntaxToken2 = null;
		if (flag4 && !flag3 && CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			syntaxToken2 = EatToken();
			syntaxToken2 = CheckFeatureAvailability(syntaxToken2, MessageID.IDS_FeatureRefLocalsReturns);
		}
		ExpressionSyntax expressionSyntax = ParseVariableInitializer();
		if (syntaxToken2 != null)
		{
			expressionSyntax = _syntaxFactory.RefExpression(syntaxToken2, expressionSyntax);
		}
		initializer = _syntaxFactory.EqualsValueClause(equalsToken, expressionSyntax);
		goto IL_03ef;
		IL_02b1:
		_termState |= TerminatorState.IsPossibleEndOfVariableDeclaration;
		ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = ParseArrayRankSpecifier(isArrayCreation: false, flags == VariableFlags.Fixed, out var sawNonOmittedSize);
		_termState = termState;
		SyntaxToken openBracketToken = arrayRankSpecifierSyntax.OpenBracketToken;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes = arrayRankSpecifierSyntax.Sizes;
		SyntaxToken syntaxToken3 = arrayRankSpecifierSyntax.CloseBracketToken;
		if (flag2 && !sawNonOmittedSize)
		{
			syntaxToken3 = AddError(syntaxToken3, ErrorCode.ERR_ValueExpected);
		}
		SeparatedSyntaxListBuilder<ArgumentSyntax> builder = _pool.AllocateSeparated<ArgumentSyntax>();
		try
		{
			foreach (GreenNode withSeparator in sizes.GetWithSeparators())
			{
				if (withSeparator is ExpressionSyntax expression)
				{
					builder.Add(_syntaxFactory.Argument(null, null, expression));
				}
				else
				{
					builder.AddSeparator((SyntaxToken)withSeparator);
				}
			}
			bracketedArgumentListSyntax = _syntaxFactory.BracketedArgumentList(openBracketToken, builder, syntaxToken3);
			if (!flag2)
			{
				bracketedArgumentListSyntax = AddError(bracketedArgumentListSyntax, ErrorCode.ERR_CStyleArray);
				if (CurrentToken.Kind == SyntaxKind.EqualsToken)
				{
					goto IL_01cf;
				}
			}
		}
		finally
		{
			_pool.Free(in builder);
		}
		goto IL_03ef;
		IL_03ef:
		localFunction = null;
		return _syntaxFactory.VariableDeclarator(syntaxToken, bracketedArgumentListSyntax, initializer);
		IL_03bb:
		if (flag3)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_ConstValueRequired);
		}
		else if (flag2)
		{
			if (parentType.Kind != SyntaxKind.ArrayType)
			{
				goto IL_02b1;
			}
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_FixedDimsRequired);
		}
		goto IL_03ef;
	}

	private bool IsLocalFunctionAfterIdentifier()
	{
		ResetPoint state = GetResetPoint();
		try
		{
			ParseTypeParameterList();
			if (!ParseParenthesizedParameterList().IsMissing && (CurrentToken.Kind == SyntaxKind.OpenBraceToken || CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken || CurrentToken.ContextualKind == SyntaxKind.WhereKeyword))
			{
				return true;
			}
			return false;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool IsPossibleEndOfVariableDeclaration()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.SemicolonToken || kind == SyntaxKind.CommaToken)
		{
			return true;
		}
		return false;
	}

	private ExpressionSyntax ParseVariableInitializer()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.OpenBraceToken)
		{
			return ParseArrayInitializer();
		}
		return ParseExpressionCore();
	}

	private bool IsPossibleVariableInitializer()
	{
		if (CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private FieldDeclarationSyntax ParseConstantFieldDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		SyntaxToken item = EatToken(SyntaxKind.ConstKeyword);
		modifiers.Add(item);
		TypeSyntax type = ParseType();
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			ParseVariableDeclarators(type, VariableFlags.Const, builder, parentKind);
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, builder), semicolonToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private DelegateDeclarationSyntax ParseDelegateDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken delegateKeyword = EatToken(SyntaxKind.DelegateKeyword);
		TypeSyntax returnType = ParseReturnType();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		SyntaxToken identifier = ParseIdentifierToken();
		TypeParameterListSyntax typeParameterList = ParseTypeParameterList();
		ParameterListSyntax parameterList = ParseParenthesizedParameterList();
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder = default;
		try
		{
			if (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				syntaxListBuilder = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
				ParseTypeParameterConstraintClauses(syntaxListBuilder);
			}
			_termState = termState;
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.DelegateDeclaration(attributes, modifiers.ToList(), delegateKeyword, returnType, identifier, typeParameterList, parameterList, syntaxListBuilder, semicolonToken);
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
		}
	}

	private EnumDeclarationSyntax ParseEnumDeclaration(SyntaxListBuilder<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken enumKeyword = EatToken(SyntaxKind.EnumKeyword);
		SyntaxToken syntaxToken = ParseIdentifierToken();
		TypeParameterListSyntax typeParameterListSyntax = ParseTypeParameterList();
		if (typeParameterListSyntax != null)
		{
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, typeParameterListSyntax);
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_UnexpectedGenericName);
		}
		BaseListSyntax baseList = null;
		if (CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
			TypeSyntax type = ParseType();
			SeparatedSyntaxListBuilder<BaseTypeSyntax> builder = _pool.AllocateSeparated<BaseTypeSyntax>();
			builder.Add(_syntaxFactory.SimpleBaseType(type));
			baseList = _syntaxFactory.BaseList(colonToken, builder);
			_pool.Free(in builder);
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = default;
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		if (!openBrace.IsMissing)
		{
			SeparatedSyntaxListBuilder<EnumMemberDeclarationSyntax> item = _pool.AllocateSeparated<EnumMemberDeclarationSyntax>();
			try
			{
				ParseEnumMemberDeclarations(ref openBrace, item);
				members = item.ToList();
			}
			finally
			{
				_pool.Free(in item);
			}
		}
		SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		SyntaxToken semicolonToken = null;
		if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolonToken = EatToken();
		}
		return _syntaxFactory.EnumDeclaration(attributes, modifiers.ToList(), enumKeyword, syntaxToken, baseList, openBrace, members, closeBraceToken, semicolonToken);
	}

	private void ParseEnumMemberDeclarations(ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<EnumMemberDeclarationSyntax> members)
	{
		if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return;
		}
		while (true)
		{
			if (IsPossibleEnumMemberDeclaration() || CurrentToken.Kind == SyntaxKind.CommaToken || CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				members.Add(ParseEnumMemberDeclaration());
				while (true)
				{
					if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
					{
						return;
					}
					if (CurrentToken.Kind == SyntaxKind.CommaToken || CurrentToken.Kind == SyntaxKind.SemicolonToken || IsPossibleEnumMemberDeclaration())
					{
						if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
						{
							members.AddSeparator(EatTokenWithPrejudice(SyntaxKind.CommaToken));
						}
						else
						{
							members.AddSeparator(EatToken(SyntaxKind.CommaToken));
						}
						if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
						{
							return;
						}
						if (!IsPossibleEnumMemberDeclaration())
						{
							break;
						}
						members.Add(ParseEnumMemberDeclaration());
					}
					else if (SkipBadEnumMemberListTokens(ref openBrace, members, SyntaxKind.CommaToken) == PostSkipAction.Abort)
					{
						return;
					}
				}
			}
			else if (SkipBadEnumMemberListTokens(ref openBrace, members, SyntaxKind.IdentifierToken) != PostSkipAction.Continue)
			{
				break;
			}
		}
	}

	private PostSkipAction SkipBadEnumMemberListTokens(ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<EnumMemberDeclarationSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBrace, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.SemicolonToken && !p.IsPossibleEnumMemberDeclaration(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.IsTerminator(), expected);
	}

	private EnumMemberDeclarationSyntax ParseEnumMemberDeclaration()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.EnumMemberDeclaration)
		{
			return (EnumMemberDeclarationSyntax)EatNode();
		}
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		try
		{
			ParseAttributeDeclarations(syntaxListBuilder);
			SyntaxToken identifier = ParseIdentifierToken();
			EqualsValueClauseSyntax equalsValue = null;
			if (CurrentToken.Kind == SyntaxKind.EqualsToken)
			{
				SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
				ExpressionSyntax value = ((CurrentToken.Kind != SyntaxKind.CommaToken && CurrentToken.Kind != SyntaxKind.CloseBraceToken) ? ParseExpressionCore() : ParseIdentifierName(ErrorCode.ERR_ConstantExpected));
				equalsValue = _syntaxFactory.EqualsValueClause(equalsToken, value);
			}
			return _syntaxFactory.EnumMemberDeclaration(syntaxListBuilder, identifier, equalsValue);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsPossibleEnumMemberDeclaration()
	{
		if (CurrentToken.Kind != SyntaxKind.OpenBracketToken)
		{
			return IsTrueIdentifier();
		}
		return true;
	}

	private bool IsDotOrColonColon()
	{
		if (CurrentToken.Kind != SyntaxKind.DotToken)
		{
			return CurrentToken.Kind == SyntaxKind.ColonColonToken;
		}
		return true;
	}

	public NameSyntax ParseName()
	{
		return ParseQualifiedName();
	}

	private IdentifierNameSyntax CreateMissingIdentifierName()
	{
		return _syntaxFactory.IdentifierName(CreateMissingIdentifierToken());
	}

	private static SyntaxToken CreateMissingIdentifierToken()
	{
		return SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken);
	}

	private bool IsTrueIdentifier()
	{
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken && !IsCurrentTokenPartialKeywordOfPartialMethodOrType() && !IsCurrentTokenQueryKeywordInQuery() && !IsCurrentTokenWhereOfConstraintClause())
		{
			return true;
		}
		return false;
	}

	private bool IsTrueIdentifier(SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.IdentifierToken)
		{
			if (IsInQuery)
			{
				return !IsTokenQueryContextualKeyword(token);
			}
			return true;
		}
		return false;
	}

	private IdentifierNameSyntax ParseIdentifierName(ErrorCode code = ErrorCode.ERR_IdentifierExpected)
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.IdentifierName && !SyntaxFacts.IsContextualKeyword(((Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)CurrentNode).Identifier.Kind()))
		{
			return (IdentifierNameSyntax)EatNode();
		}
		return SyntaxFactory.IdentifierName(ParseIdentifierToken(code));
	}

	private SyntaxToken ParseIdentifierToken(ErrorCode code = ErrorCode.ERR_IdentifierExpected)
	{
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			if (IsCurrentTokenPartialKeywordOfPartialMethodOrType() || IsCurrentTokenQueryKeywordInQuery())
			{
				SyntaxToken node = CreateMissingIdentifierToken();
				return AddError(node, ErrorCode.ERR_InvalidExprTerm, CurrentToken.Text);
			}
			SyntaxToken syntaxToken = EatToken();
			if (IsInAsync && syntaxToken.ContextualKind == SyntaxKind.AwaitKeyword)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadAwaitAsIdentifier);
			}
			return syntaxToken;
		}
		SyntaxToken node2 = CreateMissingIdentifierToken();
		return AddError(node2, code);
	}

	private bool IsCurrentTokenQueryKeywordInQuery()
	{
		if (IsInQuery)
		{
			return IsCurrentTokenQueryContextualKeyword;
		}
		return false;
	}

	private bool IsCurrentTokenPartialKeywordOfPartialMethodOrType()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.PartialKeyword && (IsPartialType() || IsPartialMember()))
		{
			return true;
		}
		return false;
	}

	private TypeParameterListSyntax ParseTypeParameterList()
	{
		if (CurrentToken.Kind != SyntaxKind.LessThanToken)
		{
			return null;
		}
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfTypeParameterList;
		try
		{
			SeparatedSyntaxListBuilder<TypeParameterSyntax> builder = _pool.AllocateSeparated<TypeParameterSyntax>();
			SyntaxToken node = EatToken(SyntaxKind.LessThanToken);
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureGenerics);
			builder.Add(ParseTypeParameter());
			while (CurrentToken.Kind != SyntaxKind.GreaterThanToken && !IsCurrentTokenWhereOfConstraintClause())
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
					builder.Add(ParseTypeParameter());
				}
				else if (SkipBadTypeParameterListTokens(builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			SyntaxToken greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
			return _syntaxFactory.TypeParameterList(node, builder, greaterThanToken);
		}
		finally
		{
			_termState = termState;
		}
	}

	private PostSkipAction SkipBadTypeParameterListTokens(SeparatedSyntaxListBuilder<TypeParameterSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p) => CurrentToken.Kind == SyntaxKind.GreaterThanToken || IsTerminator(), expected);
	}

	private TypeParameterSyntax ParseTypeParameter()
	{
		if (IsCurrentTokenWhereOfConstraintClause())
		{
			return _syntaxFactory.TypeParameter(default, null, AddError(CreateMissingIdentifierToken(), ErrorCode.ERR_IdentifierExpected));
		}
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		try
		{
			if (CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind != SyntaxKind.CloseBracketToken)
			{
				TerminatorState termState = _termState;
				_termState = TerminatorState.IsEndOfTypeArgumentList;
				ParseAttributeDeclarations(syntaxListBuilder);
				_termState = termState;
			}
			SyntaxToken varianceKeyword = null;
			if (CurrentToken.Kind == SyntaxKind.InKeyword || CurrentToken.Kind == SyntaxKind.OutKeyword)
			{
				varianceKeyword = CheckFeatureAvailability(EatToken(), MessageID.IDS_FeatureTypeVariance);
			}
			return _syntaxFactory.TypeParameter(syntaxListBuilder, varianceKeyword, ParseIdentifierToken());
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private SimpleNameSyntax ParseSimpleName(NameOptions options = NameOptions.None)
	{
		IdentifierNameSyntax identifierNameSyntax = ParseIdentifierName();
		if (identifierNameSyntax.Identifier.IsMissing)
		{
			return identifierNameSyntax;
		}
		SimpleNameSyntax result = identifierNameSyntax;
		if (CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			ResetPoint state = GetResetPoint();
			ScanTypeArgumentListKind scanTypeArgumentListKind = ScanTypeArgumentList(options);
			Reset(ref state);
			Release(ref state);
			if (scanTypeArgumentListKind == ScanTypeArgumentListKind.DefiniteTypeArgumentList || (scanTypeArgumentListKind == ScanTypeArgumentListKind.PossibleTypeArgumentList && (options & NameOptions.InTypeList) != 0))
			{
				SeparatedSyntaxListBuilder<TypeSyntax> builder = _pool.AllocateSeparated<TypeSyntax>();
				ParseTypeArgumentList(out var open, builder, out var close);
				result = _syntaxFactory.GenericName(identifierNameSyntax.Identifier, _syntaxFactory.TypeArgumentList(open, builder, close));
				_pool.Free(in builder);
			}
		}
		return result;
	}

	private ScanTypeArgumentListKind ScanTypeArgumentList(NameOptions options)
	{
		if (CurrentToken.Kind != SyntaxKind.LessThanToken)
		{
			return ScanTypeArgumentListKind.NotTypeArgumentList;
		}
		if ((options & NameOptions.InExpression) == 0)
		{
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		SyntaxToken lastTokenOfList = null;
		if (ScanPossibleTypeArgumentList(ref lastTokenOfList, out var isDefinitelyTypeArgumentList) == ScanTypeFlags.NotType)
		{
			return ScanTypeArgumentListKind.NotTypeArgumentList;
		}
		if (isDefinitelyTypeArgumentList)
		{
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		switch ((ushort)CurrentToken.Kind)
		{
		case 8508:
			if ((options & (NameOptions.AfterIs | NameOptions.AfterCase | NameOptions.AfterOut)) != 0 || ((options & NameOptions.AfterTupleComma) != 0 && (PeekToken(1).Kind == SyntaxKind.CommaToken || PeekToken(1).Kind == SyntaxKind.CloseParenToken)) || ((options & NameOptions.FirstElementOfPossibleTupleLiteral) != 0 && PeekToken(1).Kind == SyntaxKind.CommaToken))
			{
				return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
			}
			return ScanTypeArgumentListKind.PossibleTypeArgumentList;
		case 8197:
		case 8200:
		case 8201:
		case 8206:
		case 8208:
		case 8209:
		case 8211:
		case 8212:
		case 8216:
		case 8218:
		case 8219:
		case 8267:
		case 8268:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case 8198:
		case 8207:
		case 8215:
		case 8260:
		case 8261:
		case 8270:
		case 8273:
		case 8363:
		case 8364:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case 8217:
			if ((options & NameOptions.AfterIs) != 0 && PeekToken(1).Kind != SyntaxKind.GreaterThanToken)
			{
				return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
			}
			break;
		case 8496:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		return ScanTypeArgumentListKind.PossibleTypeArgumentList;
	}

	private ScanTypeFlags ScanPossibleTypeArgumentList(ref SyntaxToken lastTokenOfList, out bool isDefinitelyTypeArgumentList)
	{
		isDefinitelyTypeArgumentList = false;
		if (CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			ScanTypeFlags result = ScanTypeFlags.GenericTypeOrExpression;
			do
			{
				lastTokenOfList = EatToken();
				if (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					lastTokenOfList = null;
					return ScanTypeFlags.NotType;
				}
				if (CurrentToken.Kind == SyntaxKind.GreaterThanToken)
				{
					lastTokenOfList = EatToken();
					return result;
				}
				switch (ScanType(out lastTokenOfList))
				{
				case ScanTypeFlags.NotType:
					lastTokenOfList = null;
					return ScanTypeFlags.NotType;
				case ScanTypeFlags.MustBeType:
					isDefinitelyTypeArgumentList = DetermineIfDefinitelyTypeArgumentList(isDefinitelyTypeArgumentList);
					result = ScanTypeFlags.GenericTypeOrMethod;
					break;
				case ScanTypeFlags.NullableType:
					isDefinitelyTypeArgumentList = DetermineIfDefinitelyTypeArgumentList(isDefinitelyTypeArgumentList);
					if (isDefinitelyTypeArgumentList)
					{
						result = ScanTypeFlags.GenericTypeOrMethod;
					}
					break;
				case ScanTypeFlags.GenericTypeOrExpression:
					if (!isDefinitelyTypeArgumentList)
					{
						isDefinitelyTypeArgumentList = CurrentToken.Kind == SyntaxKind.CommaToken;
						result = ScanTypeFlags.GenericTypeOrMethod;
					}
					break;
				case ScanTypeFlags.GenericTypeOrMethod:
					result = ScanTypeFlags.GenericTypeOrMethod;
					break;
				}
			}
			while (CurrentToken.Kind == SyntaxKind.CommaToken);
			if (CurrentToken.Kind != SyntaxKind.GreaterThanToken)
			{
				lastTokenOfList = null;
				return ScanTypeFlags.NotType;
			}
			lastTokenOfList = EatToken();
			return result;
		}
		return ScanTypeFlags.NonGenericTypeOrExpression;
	}

	private bool DetermineIfDefinitelyTypeArgumentList(bool isDefinitelyTypeArgumentList)
	{
		if (!isDefinitelyTypeArgumentList)
		{
			isDefinitelyTypeArgumentList = CurrentToken.Kind == SyntaxKind.CommaToken || CurrentToken.Kind == SyntaxKind.GreaterThanToken;
		}
		return isDefinitelyTypeArgumentList;
	}

	private void ParseTypeArgumentList(out SyntaxToken open, SeparatedSyntaxListBuilder<TypeSyntax> types, out SyntaxToken close)
	{
		open = EatToken(SyntaxKind.LessThanToken);
		open = CheckFeatureAvailability(open, MessageID.IDS_FeatureGenerics);
		if (IsOpenName())
		{
			OmittedTypeArgumentSyntax node = _syntaxFactory.OmittedTypeArgument(SyntaxFactory.Token(SyntaxKind.OmittedTypeArgumentToken));
			types.Add(node);
			while (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				types.AddSeparator(EatToken(SyntaxKind.CommaToken));
				types.Add(node);
			}
			close = EatToken(SyntaxKind.GreaterThanToken);
			return;
		}
		types.Add(ParseTypeArgument());
		while (CurrentToken.Kind != SyntaxKind.GreaterThanToken)
		{
			if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleType())
			{
				types.AddSeparator(EatToken(SyntaxKind.CommaToken));
				types.Add(ParseTypeArgument());
			}
			else if (SkipBadTypeArgumentListTokens(types, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
		close = EatToken(SyntaxKind.GreaterThanToken);
	}

	private PostSkipAction SkipBadTypeArgumentListTokens(SeparatedSyntaxListBuilder<TypeSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => CurrentToken.Kind != SyntaxKind.CommaToken && !IsPossibleType(), (LanguageParser p) => CurrentToken.Kind == SyntaxKind.GreaterThanToken || IsTerminator(), expected);
	}

	private TypeSyntax ParseTypeArgument()
	{
		SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
		try
		{
			if (CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind != SyntaxKind.CloseBracketToken)
			{
				TerminatorState termState = _termState;
				_termState = TerminatorState.IsEndOfTypeArgumentList;
				ParseAttributeDeclarations(syntaxListBuilder);
				_termState = termState;
			}
			SyntaxToken syntaxToken = null;
			if (CurrentToken.Kind == SyntaxKind.InKeyword || CurrentToken.Kind == SyntaxKind.OutKeyword)
			{
				syntaxToken = EatToken();
				syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureTypeVariance);
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_IllegalVarianceSyntax);
			}
			TypeSyntax typeSyntax = ParseType();
			SyntaxKind kind;
			if (typeSyntax.IsMissing && CurrentToken.Kind != SyntaxKind.CommaToken && CurrentToken.Kind != SyntaxKind.GreaterThanToken && ((kind = PeekToken(1).Kind) == SyntaxKind.CommaToken || kind == SyntaxKind.GreaterThanToken))
			{
				typeSyntax = AddTrailingSkippedSyntax(typeSyntax, EatToken());
			}
			if (syntaxToken != null)
			{
				typeSyntax = AddLeadingSkippedSyntax(typeSyntax, syntaxToken);
			}
			if (syntaxListBuilder.Count > 0)
			{
				typeSyntax = AddLeadingSkippedSyntax(typeSyntax, syntaxListBuilder.ToListNode());
				typeSyntax = AddError(typeSyntax, ErrorCode.ERR_TypeExpected);
			}
			return typeSyntax;
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsEndOfTypeArgumentList()
	{
		return CurrentToken.Kind == SyntaxKind.GreaterThanToken;
	}

	private bool IsOpenName()
	{
		bool result = true;
		int i;
		for (i = 0; PeekToken(i).Kind == SyntaxKind.CommaToken; i++)
		{
		}
		if (PeekToken(i).Kind != SyntaxKind.GreaterThanToken)
		{
			result = false;
		}
		return result;
	}

	private void ParseMemberName(out ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, out SyntaxToken identifierOrThisOpt, out TypeParameterListSyntax typeParameterListOpt, bool isEvent)
	{
		identifierOrThisOpt = null;
		explicitInterfaceOpt = null;
		typeParameterListOpt = null;
		if (!IsPossibleMemberName())
		{
			return;
		}
		NameSyntax nameSyntax = null;
		SyntaxToken syntaxToken = null;
		ResetPoint state = default;
		bool flag = false;
		try
		{
			while (true)
			{
				if (CurrentToken.Kind == SyntaxKind.ThisKeyword)
				{
					state = GetResetPoint();
					flag = true;
					identifierOrThisOpt = EatToken();
					typeParameterListOpt = ParseTypeParameterList();
					break;
				}
				ResetPoint state2 = GetResetPoint();
				bool flag2;
				try
				{
					ScanNamedTypePart();
					flag2 = !IsDotOrColonColon();
				}
				finally
				{
					Reset(ref state2);
					Release(ref state2);
				}
				if (flag2)
				{
					state = GetResetPoint();
					flag = true;
					if (syntaxToken != null && syntaxToken.Kind == SyntaxKind.ColonColonToken)
					{
						syntaxToken = AddError(syntaxToken, ErrorCode.ERR_AliasQualAsExpression);
						syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.DotToken);
					}
					identifierOrThisOpt = ParseIdentifierToken();
					typeParameterListOpt = ParseTypeParameterList();
					break;
				}
				TerminatorState termState = _termState;
				_termState |= TerminatorState.IsEndOfNameInExplicitInterface;
				if (nameSyntax == null)
				{
					nameSyntax = ParseSimpleName(NameOptions.InTypeList);
					syntaxToken = ((CurrentToken.Kind == SyntaxKind.ColonColonToken) ? EatToken() : EatToken(SyntaxKind.DotToken));
				}
				else
				{
					nameSyntax = ParseQualifiedNameRight(NameOptions.InTypeList, nameSyntax, syntaxToken);
					syntaxToken = ((CurrentToken.Kind == SyntaxKind.ColonColonToken) ? ConvertToMissingWithTrailingTrivia(EatToken(), SyntaxKind.DotToken) : EatToken(SyntaxKind.DotToken));
				}
				_termState = termState;
			}
			if (nameSyntax != null)
			{
				if (syntaxToken.Kind != SyntaxKind.DotToken)
				{
					syntaxToken = WithAdditionalDiagnostics(syntaxToken, GetExpectedTokenError(SyntaxKind.DotToken, syntaxToken.Kind, syntaxToken.GetLeadingTriviaWidth(), syntaxToken.Width));
					syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.DotToken);
				}
				if (isEvent && CurrentToken.Kind != SyntaxKind.OpenBraceToken)
				{
					explicitInterfaceOpt = _syntaxFactory.ExplicitInterfaceSpecifier(nameSyntax, AddError(syntaxToken, ErrorCode.ERR_ExplicitEventFieldImpl));
					Reset(ref state);
					identifierOrThisOpt = null;
					typeParameterListOpt = null;
				}
				else
				{
					explicitInterfaceOpt = _syntaxFactory.ExplicitInterfaceSpecifier(nameSyntax, syntaxToken);
				}
			}
		}
		finally
		{
			if (flag)
			{
				Release(ref state);
			}
		}
	}

	private NameSyntax ParseAliasQualifiedName(NameOptions allowedParts = NameOptions.None)
	{
		NameSyntax nameSyntax = ParseSimpleName(allowedParts);
		if (CurrentToken.Kind == SyntaxKind.ColonColonToken)
		{
			SyntaxToken separator = EatToken();
			nameSyntax = ParseQualifiedNameRight(allowedParts, nameSyntax, separator);
		}
		return nameSyntax;
	}

	private NameSyntax ParseQualifiedName(NameOptions options = NameOptions.None)
	{
		NameSyntax nameSyntax = ParseAliasQualifiedName(options);
		while (IsDotOrColonColon() && PeekToken(1).Kind != SyntaxKind.ThisKeyword)
		{
			SyntaxToken separator = EatToken();
			nameSyntax = ParseQualifiedNameRight(options, nameSyntax, separator);
		}
		return nameSyntax;
	}

	private NameSyntax ParseQualifiedNameRight(NameOptions options, NameSyntax left, SyntaxToken separator)
	{
		SimpleNameSyntax simpleNameSyntax = ParseSimpleName(options);
		if (separator.Kind == SyntaxKind.DotToken)
		{
			return _syntaxFactory.QualifiedName(left, separator, simpleNameSyntax);
		}
		if (separator.Kind == SyntaxKind.ColonColonToken)
		{
			if (left.Kind != SyntaxKind.IdentifierName)
			{
				separator = AddError(separator, ErrorCode.ERR_UnexpectedAliasedName, separator.ToString());
			}
			IdentifierNameSyntax identifierNameSyntax = left as IdentifierNameSyntax;
			if (identifierNameSyntax == null)
			{
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
				return _syntaxFactory.QualifiedName(left, separator, simpleNameSyntax);
			}
			if (identifierNameSyntax.Identifier.ContextualKind == SyntaxKind.GlobalKeyword)
			{
				identifierNameSyntax = _syntaxFactory.IdentifierName(SyntaxParser.ConvertToKeyword(identifierNameSyntax.Identifier));
			}
			identifierNameSyntax = CheckFeatureAvailability(identifierNameSyntax, MessageID.IDS_FeatureGlobalNamespace);
			return WithAdditionalDiagnostics(_syntaxFactory.AliasQualifiedName(identifierNameSyntax, separator, simpleNameSyntax), left.GetDiagnostics());
		}
		return left;
	}

	private SyntaxToken ConvertToMissingWithTrailingTrivia(SyntaxToken token, SyntaxKind expectedKind)
	{
		SyntaxToken node = SyntaxFactory.MissingToken(expectedKind);
		return AddTrailingSkippedSyntax(node, token);
	}

	private bool IsPossibleType()
	{
		if (!IsPredefinedType(CurrentToken.Kind))
		{
			return IsTrueIdentifier();
		}
		return true;
	}

	private bool IsPossibleName()
	{
		return IsTrueIdentifier();
	}

	private ScanTypeFlags ScanType()
	{
		SyntaxToken lastTokenOfType;
		return ScanType(out lastTokenOfType);
	}

	private ScanTypeFlags ScanType(out SyntaxToken lastTokenOfType)
	{
		return ScanType(ParseTypeMode.Normal, out lastTokenOfType);
	}

	private ScanTypeFlags ScanType(ParseTypeMode mode, out SyntaxToken lastTokenOfType)
	{
		ScanTypeFlags scanTypeFlags = ScanNonArrayType(mode, out lastTokenOfType);
		if (scanTypeFlags == ScanTypeFlags.NotType)
		{
			return scanTypeFlags;
		}
		while (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			EatToken();
			while (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				EatToken();
			}
			if (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
			{
				lastTokenOfType = null;
				return ScanTypeFlags.NotType;
			}
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTypeFlags.MustBeType;
		}
		return scanTypeFlags;
	}

	private void ScanNamedTypePart()
	{
		ScanNamedTypePart(out var _);
	}

	private ScanTypeFlags ScanNamedTypePart(out SyntaxToken lastTokenOfType)
	{
		if (CurrentToken.Kind != SyntaxKind.IdentifierToken || !IsTrueIdentifier())
		{
			lastTokenOfType = null;
			return ScanTypeFlags.NotType;
		}
		lastTokenOfType = EatToken();
		bool isDefinitelyTypeArgumentList;
		if (CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			return ScanPossibleTypeArgumentList(ref lastTokenOfType, out isDefinitelyTypeArgumentList);
		}
		return ScanTypeFlags.NonGenericTypeOrExpression;
	}

	private ScanTypeFlags ScanNonArrayType()
	{
		SyntaxToken lastTokenOfType;
		return ScanNonArrayType(ParseTypeMode.Normal, out lastTokenOfType);
	}

	private ScanTypeFlags ScanNonArrayType(ParseTypeMode mode, out SyntaxToken lastTokenOfType)
	{
		if (CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			EatToken();
			if (CurrentToken.Kind == SyntaxKind.ReadOnlyKeyword)
			{
				EatToken();
			}
		}
		ScanTypeFlags scanTypeFlags;
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			scanTypeFlags = ScanNamedTypePart(out lastTokenOfType);
			if (scanTypeFlags == ScanTypeFlags.NotType)
			{
				return ScanTypeFlags.NotType;
			}
			bool flag = CurrentToken.Kind == SyntaxKind.ColonColonToken;
			bool flag2 = true;
			while (IsDotOrColonColon())
			{
				if (!flag2 & flag)
				{
					flag = false;
				}
				lastTokenOfType = EatToken();
				scanTypeFlags = ScanNamedTypePart(out lastTokenOfType);
				if (scanTypeFlags == ScanTypeFlags.NotType)
				{
					return ScanTypeFlags.NotType;
				}
				flag2 = false;
			}
			if (flag)
			{
				scanTypeFlags = ScanTypeFlags.AliasQualifiedName;
			}
		}
		else if (IsPredefinedType(CurrentToken.Kind))
		{
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTypeFlags.MustBeType;
		}
		else
		{
			if (CurrentToken.Kind != SyntaxKind.OpenParenToken)
			{
				lastTokenOfType = null;
				return ScanTypeFlags.NotType;
			}
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTupleType(out lastTokenOfType);
			if (scanTypeFlags == ScanTypeFlags.NotType)
			{
				return ScanTypeFlags.NotType;
			}
		}
		if (CurrentToken.Kind == SyntaxKind.QuestionToken)
		{
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTypeFlags.NullableType;
		}
		if ((mode != ParseTypeMode.AfterTupleComma && mode != ParseTypeMode.FirstElementOfPossibleTupleLiteral) || PointerTypeModsFollowedByRankAndDimensionSpecifier())
		{
			while (CurrentToken.Kind == SyntaxKind.AsteriskToken)
			{
				lastTokenOfType = EatToken();
				switch (scanTypeFlags)
				{
				case ScanTypeFlags.GenericTypeOrExpression:
				case ScanTypeFlags.NonGenericTypeOrExpression:
					scanTypeFlags = ScanTypeFlags.PointerOrMultiplication;
					break;
				case ScanTypeFlags.GenericTypeOrMethod:
					scanTypeFlags = ScanTypeFlags.MustBeType;
					break;
				}
			}
		}
		return scanTypeFlags;
	}

	private ScanTypeFlags ScanTupleType(out SyntaxToken lastTokenOfType)
	{
		if (ScanType(out lastTokenOfType) != ScanTypeFlags.NotType)
		{
			if (IsTrueIdentifier())
			{
				lastTokenOfType = EatToken();
			}
			if (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				do
				{
					lastTokenOfType = EatToken();
					if (ScanType(out lastTokenOfType) == ScanTypeFlags.NotType)
					{
						lastTokenOfType = EatToken();
						return ScanTypeFlags.NotType;
					}
					if (IsTrueIdentifier())
					{
						lastTokenOfType = EatToken();
					}
				}
				while (CurrentToken.Kind == SyntaxKind.CommaToken);
				if (CurrentToken.Kind == SyntaxKind.CloseParenToken)
				{
					lastTokenOfType = EatToken();
					return ScanTypeFlags.TupleType;
				}
			}
		}
		lastTokenOfType = null;
		return ScanTypeFlags.NotType;
	}

	private static bool IsPredefinedType(SyntaxKind keyword)
	{
		return SyntaxFacts.IsPredefinedType(keyword);
	}

	public TypeSyntax ParseTypeName()
	{
		return ParseType();
	}

	private TypeSyntax ParseTypeOrVoid()
	{
		if (CurrentToken.Kind == SyntaxKind.VoidKeyword && PeekToken(1).Kind != SyntaxKind.AsteriskToken)
		{
			return _syntaxFactory.PredefinedType(EatToken());
		}
		return ParseType();
	}

	private bool IsTerm()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.EqualsGreaterThanToken:
		case SyntaxKind.StringKeyword:
		case SyntaxKind.TypeOfKeyword:
		case SyntaxKind.SizeOfKeyword:
		case SyntaxKind.NullKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.MakeRefKeyword:
		case SyntaxKind.RefTypeKeyword:
		case SyntaxKind.RefValueKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.BaseKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
		case SyntaxKind.InterpolatedStringStartToken:
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.InterpolatedStringToken:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		default:
			return false;
		}
	}

	private TypeSyntax ParseType(ParseTypeMode mode = ParseTypeMode.Normal, bool expectSizes = false)
	{
		if (mode == ParseTypeMode.Normal && !expectSizes && CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			SyntaxToken node = EatToken();
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureRefLocalsReturns);
			SyntaxToken readOnlyKeyword = null;
			if (CurrentToken.Kind == SyntaxKind.ReadOnlyKeyword)
			{
				readOnlyKeyword = EatToken();
				readOnlyKeyword = CheckFeatureAvailability(readOnlyKeyword, MessageID.IDS_FeatureReadOnlyReferences);
			}
			TypeSyntax type = ParseTypeCore(mode, expectSizes);
			return _syntaxFactory.RefType(node, readOnlyKeyword, type);
		}
		return ParseTypeCore(mode, expectSizes);
	}

	private TypeSyntax ParseTypeCore(ParseTypeMode mode, bool expectSizes)
	{
		bool flag = mode == ParseTypeMode.AsExpression || mode == ParseTypeMode.AfterIs;
		NameOptions options;
		switch (mode)
		{
		case ParseTypeMode.AfterIs:
			options = NameOptions.InExpression | NameOptions.PossiblePattern | NameOptions.AfterIs;
			break;
		case ParseTypeMode.AfterCase:
			options = NameOptions.InExpression | NameOptions.PossiblePattern | NameOptions.AfterCase;
			break;
		case ParseTypeMode.AfterOut:
			options = NameOptions.InExpression | NameOptions.AfterOut;
			break;
		case ParseTypeMode.AfterTupleComma:
			options = NameOptions.InExpression | NameOptions.AfterTupleComma;
			break;
		case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
			options = NameOptions.InExpression | NameOptions.FirstElementOfPossibleTupleLiteral;
			break;
		case ParseTypeMode.Normal:
		case ParseTypeMode.Parameter:
		case ParseTypeMode.AsExpression:
		case ParseTypeMode.ArrayCreation:
			options = NameOptions.None;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(mode);
		}
		TypeSyntax typeSyntax = ParseUnderlyingType(mode == ParseTypeMode.Parameter, options);
		if (CurrentToken.Kind == SyntaxKind.QuestionToken && ((mode != ParseTypeMode.AfterIs && mode != ParseTypeMode.AfterCase) || !IsTrueIdentifier(PeekToken(1))))
		{
			ResetPoint state = GetResetPoint();
			try
			{
				SyntaxToken node = EatToken();
				if (flag && (IsTerm() || IsPredefinedType(CurrentToken.Kind) || SyntaxFacts.IsAnyUnaryExpression(CurrentToken.Kind)))
				{
					Reset(ref state);
					return typeSyntax;
				}
				node = CheckFeatureAvailability(node, MessageID.IDS_FeatureNullable);
				typeSyntax = _syntaxFactory.NullableType(typeSyntax, node);
			}
			finally
			{
				Release(ref state);
			}
		}
		switch (mode)
		{
		case ParseTypeMode.AfterIs:
		case ParseTypeMode.AfterCase:
		case ParseTypeMode.AfterTupleComma:
		case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
			if (PointerTypeModsFollowedByRankAndDimensionSpecifier())
			{
				typeSyntax = ParsePointerTypeMods(typeSyntax);
			}
			break;
		case ParseTypeMode.Normal:
		case ParseTypeMode.Parameter:
		case ParseTypeMode.AfterOut:
		case ParseTypeMode.AsExpression:
		case ParseTypeMode.ArrayCreation:
			typeSyntax = ParsePointerTypeMods(typeSyntax);
			break;
		}
		if (IsPossibleRankAndDimensionSpecifier())
		{
			SyntaxListBuilder<ArrayRankSpecifierSyntax> syntaxListBuilder = _pool.Allocate<ArrayRankSpecifierSyntax>();
			try
			{
				while (IsPossibleRankAndDimensionSpecifier())
				{
					ArrayRankSpecifierSyntax node2 = ParseArrayRankSpecifier(mode == ParseTypeMode.ArrayCreation, expectSizes, out var _);
					syntaxListBuilder.Add(node2);
					expectSizes = false;
				}
				typeSyntax = _syntaxFactory.ArrayType(typeSyntax, syntaxListBuilder);
			}
			finally
			{
				_pool.Free(syntaxListBuilder);
			}
		}
		return typeSyntax;
	}

	private bool PointerTypeModsFollowedByRankAndDimensionSpecifier()
	{
		int num = 0;
		while (true)
		{
			switch (PeekToken(num).Kind)
			{
			case SyntaxKind.OpenBracketToken:
				return true;
			default:
				return false;
			case SyntaxKind.AsteriskToken:
				break;
			}
			num++;
		}
	}

	private bool IsPossibleRankAndDimensionSpecifier()
	{
		return CurrentToken.Kind == SyntaxKind.OpenBracketToken;
	}

	private ArrayRankSpecifierSyntax ParseArrayRankSpecifier(bool isArrayCreation, bool expectSizes, out bool sawNonOmittedSize)
	{
		sawNonOmittedSize = false;
		bool flag = false;
		SyntaxToken openBracket = EatToken(SyntaxKind.OpenBracketToken);
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
		try
		{
			OmittedArraySizeExpressionSyntax node = _syntaxFactory.OmittedArraySizeExpression(SyntaxFactory.Token(SyntaxKind.OmittedArraySizeExpressionToken));
			while (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					flag = true;
					builder.Add(node);
					builder.AddSeparator(EatToken());
				}
				else if (IsPossibleExpression())
				{
					ExpressionSyntax node2 = ParseExpressionCore();
					sawNonOmittedSize = true;
					if (!expectSizes)
					{
						node2 = AddError(node2, isArrayCreation ? ErrorCode.ERR_InvalidArray : ErrorCode.ERR_ArraySizeInDeclaration);
					}
					builder.Add(node2);
					if (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
					{
						builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
					}
				}
				else if (SkipBadArrayRankSpecifierTokens(ref openBracket, builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			if ((builder.Count & 1) == 0)
			{
				flag = true;
				builder.Add(node);
			}
			if (flag & sawNonOmittedSize)
			{
				for (int i = 0; i < builder.Count; i++)
				{
					if (builder[i].RawKind == 8654)
					{
						int width = builder[i].Width;
						int leadingTriviaWidth = builder[i].GetLeadingTriviaWidth();
						builder[i] = AddError(CreateMissingIdentifierName(), leadingTriviaWidth, width, ErrorCode.ERR_ValueExpected);
					}
				}
			}
			SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			return _syntaxFactory.ArrayRankSpecifier(openBracket, builder, closeBracketToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private TupleTypeSyntax ParseTupleType()
	{
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		SeparatedSyntaxListBuilder<TupleElementSyntax> builder = _pool.AllocateSeparated<TupleElementSyntax>();
		try
		{
			if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
			{
				TupleElementSyntax node = ParseTupleElement();
				builder.Add(node);
				while (CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					SyntaxToken separatorToken = EatToken(SyntaxKind.CommaToken);
					builder.AddSeparator(separatorToken);
					node = ParseTupleElement();
					builder.Add(node);
				}
			}
			if (builder.Count < 2)
			{
				if (builder.Count < 1)
				{
					builder.Add(_syntaxFactory.TupleElement(CreateMissingIdentifierName(), null));
				}
				builder.AddSeparator(SyntaxFactory.MissingToken(SyntaxKind.CommaToken));
				IdentifierNameSyntax type = AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TupleTooFewElements);
				builder.Add(_syntaxFactory.TupleElement(type, null));
			}
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			TupleTypeSyntax node2 = _syntaxFactory.TupleType(openParenToken, builder, closeParenToken);
			return CheckFeatureAvailability(node2, MessageID.IDS_FeatureTuples);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private TupleElementSyntax ParseTupleElement()
	{
		TypeSyntax type = ParseType();
		SyntaxToken identifier = null;
		if (IsTrueIdentifier())
		{
			identifier = ParseIdentifierToken();
		}
		return _syntaxFactory.TupleElement(type, identifier);
	}

	private PostSkipAction SkipBadArrayRankSpecifierTokens(ref SyntaxToken openBracket, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.IsTerminator(), expected);
	}

	private TypeSyntax ParseUnderlyingType(bool parentIsParameter, NameOptions options = NameOptions.None)
	{
		if (IsPredefinedType(CurrentToken.Kind))
		{
			SyntaxToken syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.VoidKeyword && CurrentToken.Kind != SyntaxKind.AsteriskToken)
			{
				syntaxToken = AddError(syntaxToken, parentIsParameter ? ErrorCode.ERR_NoVoidParameter : ErrorCode.ERR_NoVoidHere);
			}
			return _syntaxFactory.PredefinedType(syntaxToken);
		}
		if (IsTrueIdentifier())
		{
			return ParseQualifiedName(options);
		}
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return ParseTupleType();
		}
		IdentifierNameSyntax node = CreateMissingIdentifierName();
		return AddError(node, ErrorCode.ERR_TypeExpected);
	}

	private TypeSyntax ParsePointerTypeMods(TypeSyntax type)
	{
		while (CurrentToken.Kind == SyntaxKind.AsteriskToken)
		{
			type = _syntaxFactory.PointerType(type, EatToken());
		}
		return type;
	}

	public StatementSyntax ParseStatement()
	{
		return ParseWithStackGuard(() => ParseStatementCore() ?? ParseExpressionStatement(), () => SyntaxFactory.EmptyStatement(SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken)));
	}

	private StatementSyntax ParseStatementCore()
	{
		try
		{
			_recursionDepth++;
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			if (IsIncrementalAndFactoryContextMatches && CurrentNode is Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax)
			{
				return (StatementSyntax)EatNode();
			}
			StatementSyntax statementSyntax = ParseStatementNoDeclaration(allowAnyExpression: false);
			if (statementSyntax != null)
			{
				return statementSyntax;
			}
			return ParsePossibleDeclarationOrBadAwaitStatement();
		}
		finally
		{
			_recursionDepth--;
		}
	}

	private StatementSyntax ParsePossibleDeclarationOrBadAwaitStatement()
	{
		ResetPoint resetPointBeforeStatement = GetResetPoint();
		StatementSyntax result = ParsePossibleDeclarationOrBadAwaitStatement(ref resetPointBeforeStatement);
		Release(ref resetPointBeforeStatement);
		return result;
	}

	private StatementSyntax ParsePossibleDeclarationOrBadAwaitStatement(ref ResetPoint resetPointBeforeStatement)
	{
		bool flag = CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword;
		StatementSyntax statementSyntax = ParseLocalDeclarationStatement();
		if (statementSyntax == null)
		{
			Reset(ref resetPointBeforeStatement);
			return null;
		}
		if (!flag || !statementSyntax.ContainsDiagnostics)
		{
			return statementSyntax;
		}
		Reset(ref resetPointBeforeStatement);
		IsInAsync = true;
		statementSyntax = ParseStatementNoDeclaration(allowAnyExpression: false);
		IsInAsync = false;
		if (!statementSyntax.ContainsDiagnostics)
		{
			return statementSyntax;
		}
		Reset(ref resetPointBeforeStatement);
		return ParseLocalDeclarationStatement();
	}

	private StatementSyntax ParseStatementNoDeclaration(bool allowAnyExpression)
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.FixedKeyword:
			return ParseFixedStatement();
		case SyntaxKind.BreakKeyword:
			return ParseBreakStatement();
		case SyntaxKind.ContinueKeyword:
			return ParseContinueStatement();
		case SyntaxKind.TryKeyword:
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
			return ParseTryStatement();
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
			return ParseCheckedStatement();
		case SyntaxKind.ConstKeyword:
			return null;
		case SyntaxKind.DoKeyword:
			return ParseDoStatement();
		case SyntaxKind.ForKeyword:
			return ParseForOrForEachStatement();
		case SyntaxKind.ForEachKeyword:
			return ParseForEachStatement();
		case SyntaxKind.GotoKeyword:
			return ParseGotoStatement();
		case SyntaxKind.IfKeyword:
			return ParseIfStatement();
		case SyntaxKind.LockKeyword:
			return ParseLockStatement();
		case SyntaxKind.ReturnKeyword:
			return ParseReturnStatement();
		case SyntaxKind.SwitchKeyword:
			return ParseSwitchStatement();
		case SyntaxKind.ThrowKeyword:
			return ParseThrowStatement();
		case SyntaxKind.UnsafeKeyword:
			if (IsPossibleUnsafeStatement())
			{
				return ParseUnsafeStatement();
			}
			break;
		case SyntaxKind.UsingKeyword:
			return ParseUsingStatement();
		case SyntaxKind.WhileKeyword:
			return ParseWhileStatement();
		case SyntaxKind.OpenBraceToken:
			return ParseBlock();
		case SyntaxKind.SemicolonToken:
			return _syntaxFactory.EmptyStatement(EatToken());
		case SyntaxKind.IdentifierToken:
			if (IsPossibleLabeledStatement())
			{
				return ParseLabeledStatement();
			}
			if (IsPossibleYieldStatement())
			{
				return ParseYieldStatement();
			}
			if (IsPossibleAwaitExpressionStatement())
			{
				return ParseExpressionStatement();
			}
			if (IsQueryExpression(mayBeVariableDeclaration: true, allowAnyExpression))
			{
				return ParseExpressionStatement(ParseQueryExpression(Precedence.Expression));
			}
			break;
		}
		if (IsPossibleLocalDeclarationStatement(allowAnyExpression))
		{
			return null;
		}
		return ParseExpressionStatement();
	}

	private bool IsPossibleLabeledStatement()
	{
		if (PeekToken(1).Kind == SyntaxKind.ColonToken)
		{
			return IsTrueIdentifier();
		}
		return false;
	}

	private bool IsPossibleUnsafeStatement()
	{
		return PeekToken(1).Kind == SyntaxKind.OpenBraceToken;
	}

	private bool IsPossibleYieldStatement()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.YieldKeyword)
		{
			if (PeekToken(1).Kind != SyntaxKind.ReturnKeyword)
			{
				return PeekToken(1).Kind == SyntaxKind.BreakKeyword;
			}
			return true;
		}
		return false;
	}

	private bool IsPossibleLocalDeclarationStatement(bool allowAnyExpression)
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.RefKeyword || IsDeclarationModifier(kind) || (SyntaxFacts.IsPredefinedType(kind) && PeekToken(1).Kind != SyntaxKind.DotToken && PeekToken(1).Kind != SyntaxKind.OpenParenToken))
		{
			return true;
		}
		kind = CurrentToken.ContextualKind;
		if (IsAdditionalLocalFunctionModifier(kind) && (kind != SyntaxKind.AsyncKeyword || ShouldAsyncBeTreatedAsModifier(parsingStatementNotDeclaration: true)))
		{
			return true;
		}
		bool? flag = IsPossibleTypedIdentifierStart(CurrentToken, PeekToken(1), allowThisKeyword: false);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		if (kind == SyntaxKind.IdentifierToken)
		{
			SyntaxToken syntaxToken = PeekToken(1);
			if (syntaxToken.Kind == SyntaxKind.DotToken && syntaxToken.TrailingTrivia.Any(8539) && PeekToken(2).Kind == SyntaxKind.IdentifierToken && PeekToken(3).Kind == SyntaxKind.IdentifierToken)
			{
				SyntaxKind kind2 = PeekToken(4).Kind;
				if (kind2 != SyntaxKind.SemicolonToken && kind2 != SyntaxKind.EqualsToken && kind2 != SyntaxKind.CommaToken && kind2 != SyntaxKind.OpenParenToken && kind2 != SyntaxKind.LessThanToken)
				{
					return false;
				}
			}
		}
		ResetPoint state = GetResetPoint();
		try
		{
			ScanTypeFlags scanTypeFlags = ScanType();
			if (scanTypeFlags == ScanTypeFlags.MustBeType && CurrentToken.Kind != SyntaxKind.DotToken && CurrentToken.Kind != SyntaxKind.OpenParenToken)
			{
				return true;
			}
			if (scanTypeFlags == ScanTypeFlags.NotType || CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				return false;
			}
			if (allowAnyExpression)
			{
				switch (scanTypeFlags)
				{
				case ScanTypeFlags.PointerOrMultiplication:
					return false;
				case ScanTypeFlags.NullableType:
					return IsPossibleDeclarationStatementFollowingNullableType();
				}
			}
			return true;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool IsPossibleDeclarationStatementFollowingNullableType()
	{
		if (IsFieldDeclaration(isEvent: false))
		{
			return IsPossibleFieldDeclarationFollowingNullableType();
		}
		ParseMemberName(out var explicitInterfaceOpt, out var identifierOrThisOpt, out var typeParameterListOpt, isEvent: false);
		if (explicitInterfaceOpt == null && identifierOrThisOpt == null && typeParameterListOpt == null)
		{
			return false;
		}
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return true;
		}
		if (identifierOrThisOpt.Kind == SyntaxKind.ThisKeyword)
		{
			return false;
		}
		return IsPossibleMethodDeclarationFollowingNullableType();
	}

	private bool IsPossibleFieldDeclarationFollowingNullableType()
	{
		if (CurrentToken.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		EatToken();
		if (CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			TerminatorState termState = _termState;
			_termState |= TerminatorState.IsEndOfFieldDeclaration;
			EatToken();
			ParseVariableInitializer();
			_termState = termState;
		}
		if (CurrentToken.Kind != SyntaxKind.CommaToken)
		{
			return CurrentToken.Kind == SyntaxKind.SemicolonToken;
		}
		return true;
	}

	private bool IsPossibleMethodDeclarationFollowingNullableType()
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList();
		_termState = termState;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<GreenNode> withSeparators = parameterListSyntax.Parameters.GetWithSeparators();
		if (!parameterListSyntax.CloseParenToken.IsMissing)
		{
			if (CurrentToken.Kind == SyntaxKind.OpenBraceToken || CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				return true;
			}
			if (CurrentToken.Kind == SyntaxKind.ColonToken)
			{
				return false;
			}
		}
		if (withSeparators.Count == 0)
		{
			return false;
		}
		ParameterSyntax parameterSyntax = (ParameterSyntax)withSeparators[0];
		if (parameterSyntax.AttributeLists.Count > 0)
		{
			return true;
		}
		for (int i = 0; i < parameterSyntax.Modifiers.Count; i++)
		{
			if (parameterSyntax.Modifiers[i].Kind == SyntaxKind.ParamsKeyword)
			{
				return true;
			}
		}
		if (parameterSyntax.Type == null)
		{
			if (parameterSyntax.Identifier.Kind == SyntaxKind.ArgListKeyword)
			{
				return true;
			}
		}
		else if (parameterSyntax.Type.Kind == SyntaxKind.NullableType)
		{
			if (parameterSyntax.Modifiers.Count > 0)
			{
				return true;
			}
			if (!parameterSyntax.Identifier.IsMissing && ((withSeparators.Count >= 2 && !withSeparators[1].IsMissing) || (withSeparators.Count == 1 && !parameterListSyntax.CloseParenToken.IsMissing)))
			{
				return true;
			}
		}
		else
		{
			if (parameterSyntax.Type.Kind == SyntaxKind.IdentifierName && ((IdentifierNameSyntax)parameterSyntax.Type).Identifier.ContextualKind == SyntaxKind.FromKeyword)
			{
				return false;
			}
			if (!parameterSyntax.Identifier.IsMissing)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPossibleNewExpression()
	{
		SyntaxToken syntaxToken = PeekToken(1);
		SyntaxKind kind = syntaxToken.Kind;
		if (kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.OpenBracketToken)
		{
			return true;
		}
		if (SyntaxFacts.GetBaseTypeDeclarationKind(syntaxToken.Kind) != SyntaxKind.None)
		{
			return false;
		}
		switch (GetModifier(syntaxToken))
		{
		case DeclarationModifiers.Partial:
			if (SyntaxFacts.IsPredefinedType(PeekToken(2).Kind))
			{
				return false;
			}
			if (IsPossibleStartOfTypeDeclaration(PeekToken(2).Kind))
			{
				return false;
			}
			break;
		default:
			return false;
		case DeclarationModifiers.None:
			break;
		}
		bool? flag = IsPossibleTypedIdentifierStart(syntaxToken, PeekToken(2), allowThisKeyword: true);
		if (flag.HasValue)
		{
			return !flag.Value;
		}
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			ScanTypeFlags scanTypeFlags = ScanType();
			return !IsPossibleMemberName() || scanTypeFlags == ScanTypeFlags.NotType;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool? IsPossibleTypedIdentifierStart(SyntaxToken current, SyntaxToken next, bool allowThisKeyword)
	{
		if (IsTrueIdentifier(current))
		{
			switch (next.Kind)
			{
			case SyntaxKind.AsteriskToken:
			case SyntaxKind.OpenBracketToken:
			case SyntaxKind.LessThanToken:
			case SyntaxKind.DotToken:
			case SyntaxKind.QuestionToken:
			case SyntaxKind.ColonColonToken:
				return null;
			case SyntaxKind.OpenParenToken:
				if (current.IsVar())
				{
					return null;
				}
				return false;
			case SyntaxKind.IdentifierToken:
				return IsTrueIdentifier(next);
			case SyntaxKind.ThisKeyword:
				return allowThisKeyword;
			default:
				return false;
			}
		}
		return null;
	}

	private BlockSyntax ParseBlock(bool isMethodBody = false, bool isAccessorBody = false)
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.Block)
		{
			return (BlockSyntax)EatNode();
		}
		SyntaxToken syntaxToken = ((isAccessorBody && CurrentToken.Kind != SyntaxKind.OpenBraceToken) ? AddError(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken), IsFeatureEnabled(MessageID.IDS_FeatureExpressionBodiedAccessor) ? ErrorCode.ERR_SemiOrLBraceOrArrowExpected : ErrorCode.ERR_SemiOrLBraceExpected) : EatToken(SyntaxKind.OpenBraceToken));
		SyntaxListBuilder<StatementSyntax> syntaxListBuilder = _pool.Allocate<StatementSyntax>();
		try
		{
			CSharpSyntaxNode previousNode = syntaxToken;
			ParseStatements(ref previousNode, syntaxListBuilder, stopOnSwitchSections: false);
			syntaxToken = (SyntaxToken)previousNode;
			SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements = ((!isMethodBody || !IsLargeEnoughNonEmptyStatementList(syntaxListBuilder)) ? ((Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>)syntaxListBuilder) : new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>(SyntaxList.List(((SyntaxListBuilder)syntaxListBuilder).ToArray())));
			return _syntaxFactory.Block(syntaxToken, statements, closeBraceToken);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private static bool IsLargeEnoughNonEmptyStatementList(SyntaxListBuilder<StatementSyntax> statements)
	{
		if (statements.Count == 0)
		{
			return false;
		}
		if (statements.Count == 1)
		{
			return statements[0].Width > 60;
		}
		return true;
	}

	private void ParseStatements(ref CSharpSyntaxNode previousNode, SyntaxListBuilder<StatementSyntax> statements, bool stopOnSwitchSections)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsPossibleStatementStartOrStop;
		if (stopOnSwitchSections)
		{
			_termState |= TerminatorState.IsSwitchSectionStart;
		}
		while (CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken && (!stopOnSwitchSections || !IsPossibleSwitchSection()))
		{
			if (IsPossibleStatement(acceptAccessibilityMods: true))
			{
				StatementSyntax statementSyntax = ParseStatementCore();
				if (statementSyntax != null)
				{
					statements.Add(statementSyntax);
					continue;
				}
			}
			PostSkipAction postSkipAction = SkipBadStatementListTokens(statements, SyntaxKind.CloseBraceToken, out var trailingTrivia);
			if (trailingTrivia != null)
			{
				previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
			}
			if (postSkipAction == PostSkipAction.Abort)
			{
				break;
			}
		}
		_termState = termState;
	}

	private bool IsPossibleStatementStartOrStop()
	{
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return IsPossibleStatement(acceptAccessibilityMods: true);
		}
		return true;
	}

	private PostSkipAction SkipBadStatementListTokens(SyntaxListBuilder<StatementSyntax> statements, SyntaxKind expected, out GreenNode trailingTrivia)
	{
		return SkipBadListTokensWithExpectedKindHelper(statements, (LanguageParser p) => !p.IsPossibleStatement(acceptAccessibilityMods: false), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.IsTerminator(), expected, out trailingTrivia);
	}

	private bool IsPossibleStatement(bool acceptAccessibilityMods)
	{
		SyntaxKind kind = CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.SwitchKeyword:
		case SyntaxKind.TryKeyword:
		case SyntaxKind.LockKeyword:
		case SyntaxKind.GotoKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.ReturnKeyword:
		case SyntaxKind.ThrowKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.FixedKeyword:
		case SyntaxKind.VolatileKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.UsingKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
		case SyntaxKind.UnsafeKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
			return !_isInTry;
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
			return acceptAccessibilityMods;
		default:
			if (!IsPredefinedType(kind))
			{
				return IsPossibleExpression();
			}
			return true;
		}
	}

	private FixedStatementSyntax ParseFixedStatement()
	{
		SyntaxToken fixedKeyword = EatToken(SyntaxKind.FixedKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFixedStatement;
		VariableDeclarationSyntax declaration = ParseVariableDeclaration();
		_termState = termState;
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		return _syntaxFactory.FixedStatement(fixedKeyword, openParenToken, declaration, closeParenToken, statement);
	}

	private bool IsEndOfFixedStatement()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return CurrentToken.Kind == SyntaxKind.SemicolonToken;
		}
		return true;
	}

	private StatementSyntax ParseEmbeddedStatement()
	{
		StatementSyntax statementSyntax = ParseStatementCore() ?? SyntaxFactory.EmptyStatement(EatToken(SyntaxKind.SemicolonToken));
		SyntaxKind kind = statementSyntax.Kind;
		if (kind == SyntaxKind.ExpressionStatement && IsScript)
		{
			ExpressionStatementSyntax expressionStatementSyntax = (ExpressionStatementSyntax)statementSyntax;
			SyntaxToken semicolonToken = expressionStatementSyntax.SemicolonToken;
			if (semicolonToken.IsMissing && !semicolonToken.GetDiagnostics().Contains((DiagnosticInfo diagnosticInfo) => diagnosticInfo.Code == 1002))
			{
				semicolonToken = AddError(semicolonToken, ErrorCode.ERR_SemicolonExpected);
				statementSyntax = expressionStatementSyntax.Update(expressionStatementSyntax.Expression, semicolonToken);
			}
		}
		return statementSyntax;
	}

	private BreakStatementSyntax ParseBreakStatement()
	{
		SyntaxToken breakKeyword = EatToken(SyntaxKind.BreakKeyword);
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.BreakStatement(breakKeyword, semicolonToken);
	}

	private ContinueStatementSyntax ParseContinueStatement()
	{
		SyntaxToken continueKeyword = EatToken(SyntaxKind.ContinueKeyword);
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.ContinueStatement(continueKeyword, semicolonToken);
	}

	private TryStatementSyntax ParseTryStatement()
	{
		bool isInTry = _isInTry;
		_isInTry = true;
		SyntaxToken syntaxToken = EatToken(SyntaxKind.TryKeyword);
		BlockSyntax blockSyntax;
		if (syntaxToken.IsMissing)
		{
			blockSyntax = _syntaxFactory.Block(EatToken(SyntaxKind.OpenBraceToken), default, EatToken(SyntaxKind.CloseBraceToken));
		}
		else
		{
			TerminatorState termState = _termState;
			_termState |= TerminatorState.IsEndOfTryBlock;
			blockSyntax = ParseBlock();
			_termState = termState;
		}
		SyntaxListBuilder<CatchClauseSyntax> syntaxListBuilder = default;
		FinallyClauseSyntax finallyClauseSyntax = null;
		try
		{
			bool flag = false;
			if (CurrentToken.Kind == SyntaxKind.CatchKeyword)
			{
				flag = true;
				syntaxListBuilder = _pool.Allocate<CatchClauseSyntax>();
				while (CurrentToken.Kind == SyntaxKind.CatchKeyword)
				{
					syntaxListBuilder.Add(ParseCatchClause());
				}
			}
			if (CurrentToken.Kind == SyntaxKind.FinallyKeyword)
			{
				flag = true;
				SyntaxToken finallyKeyword = EatToken();
				BlockSyntax block = ParseBlock();
				finallyClauseSyntax = _syntaxFactory.FinallyClause(finallyKeyword, block);
			}
			if (!flag)
			{
				blockSyntax = AddErrorToLastToken(blockSyntax, ErrorCode.ERR_ExpectedEndTry);
				finallyClauseSyntax = _syntaxFactory.FinallyClause(SyntaxToken.CreateMissing(SyntaxKind.FinallyKeyword, null, null), _syntaxFactory.Block(SyntaxToken.CreateMissing(SyntaxKind.OpenBraceToken, null, null), default, SyntaxToken.CreateMissing(SyntaxKind.CloseBraceToken, null, null)));
			}
			_isInTry = isInTry;
			return _syntaxFactory.TryStatement(syntaxToken, blockSyntax, syntaxListBuilder, finallyClauseSyntax);
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
		}
	}

	private bool IsEndOfTryBlock()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.CatchKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.FinallyKeyword;
		}
		return true;
	}

	private CatchClauseSyntax ParseCatchClause()
	{
		SyntaxToken catchKeyword = EatToken();
		CatchDeclarationSyntax declaration = null;
		TerminatorState termState = _termState;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParenToken = EatToken();
			_termState |= TerminatorState.IsEndOfCatchClause;
			TypeSyntax type = ParseType();
			SyntaxToken identifier = null;
			if (IsTrueIdentifier())
			{
				identifier = ParseIdentifierToken();
			}
			_termState = termState;
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			declaration = _syntaxFactory.CatchDeclaration(openParenToken, type, identifier, closeParenToken);
		}
		CatchFilterClauseSyntax filter = null;
		SyntaxKind contextualKind = CurrentToken.ContextualKind;
		if (contextualKind == SyntaxKind.WhenKeyword || contextualKind == SyntaxKind.IfKeyword)
		{
			SyntaxToken node = EatContextualToken(SyntaxKind.WhenKeyword);
			if (contextualKind == SyntaxKind.IfKeyword)
			{
				node = AddTrailingSkippedSyntax(node, EatToken());
			}
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureExceptionFilter);
			_termState |= TerminatorState.IsEndOfilterClause;
			SyntaxToken openParenToken2 = EatToken(SyntaxKind.OpenParenToken);
			ExpressionSyntax filterExpression = ParseExpressionCore();
			_termState = termState;
			SyntaxToken closeParenToken2 = EatToken(SyntaxKind.CloseParenToken);
			filter = _syntaxFactory.CatchFilterClause(node, openParenToken2, filterExpression, closeParenToken2);
		}
		_termState |= TerminatorState.IsEndOfCatchBlock;
		BlockSyntax block = ParseBlock();
		_termState = termState;
		return _syntaxFactory.CatchClause(catchKeyword, declaration, filter, block);
	}

	private bool IsEndOfCatchClause()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.OpenBraceToken && CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.CatchKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.FinallyKeyword;
		}
		return true;
	}

	private bool IsEndOfFilterClause()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.OpenBraceToken && CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.CatchKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.FinallyKeyword;
		}
		return true;
	}

	private bool IsEndOfCatchBlock()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseBraceToken && CurrentToken.Kind != SyntaxKind.CatchKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.FinallyKeyword;
		}
		return true;
	}

	private StatementSyntax ParseCheckedStatement()
	{
		if (PeekToken(1).Kind == SyntaxKind.OpenParenToken)
		{
			return ParseExpressionStatement();
		}
		SyntaxToken syntaxToken = EatToken();
		BlockSyntax block = ParseBlock();
		return _syntaxFactory.CheckedStatement(SyntaxFacts.GetCheckStatement(syntaxToken.Kind), syntaxToken, block);
	}

	private DoStatementSyntax ParseDoStatement()
	{
		SyntaxToken doKeyword = EatToken(SyntaxKind.DoKeyword);
		StatementSyntax statement = ParseEmbeddedStatement();
		SyntaxToken whileKeyword = EatToken(SyntaxKind.WhileKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfDoWhileExpression;
		ExpressionSyntax condition = ParseExpressionCore();
		_termState = termState;
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.DoStatement(doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
	}

	private bool IsEndOfDoWhileExpression()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return CurrentToken.Kind == SyntaxKind.SemicolonToken;
		}
		return true;
	}

	private StatementSyntax ParseForOrForEachStatement()
	{
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			if (EatToken().Kind == SyntaxKind.OpenParenToken && ScanType() != ScanTypeFlags.NotType && EatToken().Kind == SyntaxKind.IdentifierToken && EatToken().Kind == SyntaxKind.InKeyword)
			{
				Reset(ref state);
				return ParseForEachStatement();
			}
			Reset(ref state);
			return ParseForStatement();
		}
		finally
		{
			Release(ref state);
		}
	}

	private ForStatementSyntax ParseForStatement()
	{
		SyntaxToken forKeyword = EatToken(SyntaxKind.ForKeyword);
		SyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfForStatementArgument;
		ResetPoint state = GetResetPoint();
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder2 = _pool.AllocateSeparated<ExpressionSyntax>();
		try
		{
			VariableDeclarationSyntax variableDeclarationSyntax = null;
			bool flag;
			if (CurrentToken.Kind == SyntaxKind.RefKeyword)
			{
				flag = true;
			}
			else
			{
				flag = !IsQueryExpression(mayBeVariableDeclaration: true, mayBeMemberDeclaration: false) && ScanType() != ScanTypeFlags.NotType && IsTrueIdentifier();
				Reset(ref state);
			}
			if (flag)
			{
				variableDeclarationSyntax = ParseVariableDeclaration();
				if (variableDeclarationSyntax.Type.Kind == SyntaxKind.RefType)
				{
					variableDeclarationSyntax = variableDeclarationSyntax.Update(CheckFeatureAvailability(variableDeclarationSyntax.Type, MessageID.IDS_FeatureRefFor), variableDeclarationSyntax.Variables);
				}
			}
			else if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				ParseForStatementExpressionList(ref startToken, builder);
			}
			SyntaxToken firstSemicolonToken = EatToken(SyntaxKind.SemicolonToken);
			ExpressionSyntax condition = null;
			if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				condition = ParseExpressionCore();
			}
			SyntaxToken startToken2 = EatToken(SyntaxKind.SemicolonToken);
			if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
			{
				ParseForStatementExpressionList(ref startToken2, builder2);
			}
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			StatementSyntax statement = ParseEmbeddedStatement();
			return _syntaxFactory.ForStatement(forKeyword, startToken, variableDeclarationSyntax, builder, firstSemicolonToken, condition, startToken2, builder2, closeParenToken, statement);
		}
		finally
		{
			_termState = termState;
			Release(ref state);
			_pool.Free(in builder2);
			_pool.Free(in builder);
		}
	}

	private bool IsEndOfForStatementArgument()
	{
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken && CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return CurrentToken.Kind == SyntaxKind.OpenBraceToken;
		}
		return true;
	}

	private void ParseForStatementExpressionList(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ExpressionSyntax> list)
	{
		if (CurrentToken.Kind == SyntaxKind.CloseParenToken || CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			return;
		}
		do
		{
			if (!IsPossibleExpression() && CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				continue;
			}
			list.Add(ParseExpressionCore());
			while (CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleExpression())
				{
					list.AddSeparator(EatToken(SyntaxKind.CommaToken));
					list.Add(ParseExpressionCore());
				}
				else if (SkipBadForStatementExpressionListTokens(ref startToken, list, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			break;
		}
		while (SkipBadForStatementExpressionListTokens(ref startToken, list, SyntaxKind.IdentifierToken) == PostSkipAction.Continue);
	}

	private PostSkipAction SkipBadForStatementExpressionListTokens(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private CommonForEachStatementSyntax ParseForEachStatement()
	{
		SyntaxToken forEachKeyword;
		if (CurrentToken.Kind == SyntaxKind.ForKeyword)
		{
			SyntaxToken node = EatToken();
			node = AddError(node, ErrorCode.ERR_SyntaxError, SyntaxFacts.GetText(SyntaxKind.ForEachKeyword), SyntaxFacts.GetText(SyntaxKind.ForKeyword));
			forEachKeyword = ConvertToMissingWithTrailingTrivia(node, SyntaxKind.ForEachKeyword);
		}
		else
		{
			forEachKeyword = EatToken(SyntaxKind.ForEachKeyword);
		}
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.Normal, MessageID.IDS_FeatureTuples, permitTupleDesignation: true);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.InKeyword, ErrorCode.ERR_InExpected);
		if (!IsValidForeachVariable(expressionSyntax))
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadForeachDecl);
		}
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		if (expressionSyntax is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			if (declarationExpressionSyntax.Type.Kind == SyntaxKind.RefType)
			{
				declarationExpressionSyntax = declarationExpressionSyntax.Update(CheckFeatureAvailability(declarationExpressionSyntax.Type, MessageID.IDS_FeatureRefForEach), declarationExpressionSyntax.Designation);
			}
			if (declarationExpressionSyntax.designation.Kind != SyntaxKind.ParenthesizedVariableDesignation)
			{
				SyntaxToken identifier;
				switch (declarationExpressionSyntax.designation.Kind)
				{
				case SyntaxKind.SingleVariableDesignation:
					identifier = ((SingleVariableDesignationSyntax)declarationExpressionSyntax.designation).identifier;
					break;
				case SyntaxKind.DiscardDesignation:
				{
					SyntaxToken underscoreToken = ((DiscardDesignationSyntax)declarationExpressionSyntax.designation).underscoreToken;
					identifier = SyntaxToken.WithValue(SyntaxKind.IdentifierToken, underscoreToken.LeadingTrivia.Node, underscoreToken.Text, underscoreToken.ValueText, underscoreToken.TrailingTrivia.Node);
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(declarationExpressionSyntax.designation.Kind);
				}
				return _syntaxFactory.ForEachStatement(forEachKeyword, openParenToken, declarationExpressionSyntax.Type, identifier, syntaxToken, expression, closeParenToken, statement);
			}
		}
		return _syntaxFactory.ForEachVariableStatement(forEachKeyword, openParenToken, expressionSyntax, syntaxToken, expression, closeParenToken, statement);
	}

	private static bool IsValidForeachVariable(ExpressionSyntax variable)
	{
		return variable.Kind switch
		{
			SyntaxKind.DeclarationExpression => true, 
			SyntaxKind.TupleExpression => true, 
			SyntaxKind.IdentifierName => ((IdentifierNameSyntax)variable).Identifier.ContextualKind == SyntaxKind.UnderscoreToken, 
			_ => false, 
		};
	}

	private GotoStatementSyntax ParseGotoStatement()
	{
		SyntaxToken gotoKeyword = EatToken(SyntaxKind.GotoKeyword);
		SyntaxToken syntaxToken = null;
		ExpressionSyntax expression = null;
		SyntaxKind kind;
		if (CurrentToken.Kind == SyntaxKind.CaseKeyword || CurrentToken.Kind == SyntaxKind.DefaultKeyword)
		{
			syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.CaseKeyword)
			{
				kind = SyntaxKind.GotoCaseStatement;
				expression = ParseExpressionCore();
			}
			else
			{
				kind = SyntaxKind.GotoDefaultStatement;
			}
		}
		else
		{
			kind = SyntaxKind.GotoStatement;
			expression = ParseIdentifierName();
		}
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.GotoStatement(kind, gotoKeyword, syntaxToken, expression, semicolonToken);
	}

	private IfStatementSyntax ParseIfStatement()
	{
		SyntaxToken ifKeyword = EatToken(SyntaxKind.IfKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax condition = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		ElseClauseSyntax elseClauseSyntax = ParseElseClauseOpt();
		return _syntaxFactory.IfStatement(ifKeyword, openParenToken, condition, closeParenToken, statement, elseClauseSyntax);
	}

	private ElseClauseSyntax ParseElseClauseOpt()
	{
		if (CurrentToken.Kind != SyntaxKind.ElseKeyword)
		{
			return null;
		}
		SyntaxToken elseKeyword = EatToken(SyntaxKind.ElseKeyword);
		StatementSyntax statement = ParseEmbeddedStatement();
		return _syntaxFactory.ElseClause(elseKeyword, statement);
	}

	private LockStatementSyntax ParseLockStatement()
	{
		SyntaxToken lockKeyword = EatToken(SyntaxKind.LockKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		return _syntaxFactory.LockStatement(lockKeyword, openParenToken, expression, closeParenToken, statement);
	}

	private ReturnStatementSyntax ParseReturnStatement()
	{
		SyntaxToken returnKeyword = EatToken(SyntaxKind.ReturnKeyword);
		ExpressionSyntax expression = null;
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			expression = ParsePossibleRefExpression();
		}
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.ReturnStatement(returnKeyword, expression, semicolonToken);
	}

	private YieldStatementSyntax ParseYieldStatement()
	{
		SyntaxToken node = SyntaxParser.ConvertToKeyword(EatToken());
		ExpressionSyntax expression = null;
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureIterators);
		SyntaxKind kind;
		SyntaxToken syntaxToken;
		if (CurrentToken.Kind == SyntaxKind.BreakKeyword)
		{
			kind = SyntaxKind.YieldBreakStatement;
			syntaxToken = EatToken();
		}
		else
		{
			kind = SyntaxKind.YieldReturnStatement;
			syntaxToken = EatToken(SyntaxKind.ReturnKeyword);
			if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_EmptyYield);
			}
			else
			{
				expression = ParseExpressionCore();
			}
		}
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.YieldStatement(kind, node, syntaxToken, expression, semicolonToken);
	}

	private SwitchStatementSyntax ParseSwitchStatement()
	{
		SyntaxToken switchKeyword = EatToken(SyntaxKind.SwitchKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.WRN_EmptySwitch);
		}
		SyntaxListBuilder<SwitchSectionSyntax> syntaxListBuilder = _pool.Allocate<SwitchSectionSyntax>();
		try
		{
			while (IsPossibleSwitchSection())
			{
				SwitchSectionSyntax node = ParseSwitchSection();
				syntaxListBuilder.Add(node);
			}
			SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return _syntaxFactory.SwitchStatement(switchKeyword, openParenToken, expression, closeParenToken, syntaxToken, syntaxListBuilder, closeBraceToken);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsPossibleSwitchSection()
	{
		if (CurrentToken.Kind != SyntaxKind.CaseKeyword)
		{
			if (CurrentToken.Kind == SyntaxKind.DefaultKeyword)
			{
				return PeekToken(1).Kind != SyntaxKind.OpenParenToken;
			}
			return false;
		}
		return true;
	}

	private SwitchSectionSyntax ParseSwitchSection()
	{
		SyntaxListBuilder<SwitchLabelSyntax> syntaxListBuilder = _pool.Allocate<SwitchLabelSyntax>();
		SyntaxListBuilder<StatementSyntax> syntaxListBuilder2 = _pool.Allocate<StatementSyntax>();
		try
		{
			do
			{
				SwitchLabelSyntax node;
				if (CurrentToken.Kind == SyntaxKind.CaseKeyword)
				{
					SyntaxToken keyword = EatToken();
					if (CurrentToken.Kind == SyntaxKind.ColonToken)
					{
						ExpressionSyntax value = ParseIdentifierName(ErrorCode.ERR_ConstantExpected);
						SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
						node = _syntaxFactory.CaseSwitchLabel(keyword, value, colonToken);
					}
					else
					{
						CSharpSyntaxNode cSharpSyntaxNode = ParseExpressionOrPatternForCase();
						if (CurrentToken.ContextualKind == SyntaxKind.WhenKeyword && cSharpSyntaxNode is ExpressionSyntax)
						{
							cSharpSyntaxNode = _syntaxFactory.ConstantPattern((ExpressionSyntax)cSharpSyntaxNode);
						}
						if (cSharpSyntaxNode is PatternSyntax)
						{
							WhenClauseSyntax whenClause = ParseWhenClause();
							SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
							node = _syntaxFactory.CasePatternSwitchLabel(keyword, (PatternSyntax)cSharpSyntaxNode, whenClause, colonToken);
							node = CheckFeatureAvailability(node, MessageID.IDS_FeaturePatternMatching);
						}
						else
						{
							SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
							node = _syntaxFactory.CaseSwitchLabel(keyword, (ExpressionSyntax)cSharpSyntaxNode, colonToken);
						}
					}
				}
				else
				{
					SyntaxToken keyword = EatToken(SyntaxKind.DefaultKeyword);
					SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
					node = _syntaxFactory.DefaultSwitchLabel(keyword, colonToken);
				}
				syntaxListBuilder.Add(node);
			}
			while (IsPossibleSwitchSection());
			CSharpSyntaxNode previousNode = syntaxListBuilder[syntaxListBuilder.Count - 1];
			ParseStatements(ref previousNode, syntaxListBuilder2, stopOnSwitchSections: true);
			syntaxListBuilder[syntaxListBuilder.Count - 1] = (SwitchLabelSyntax)previousNode;
			return _syntaxFactory.SwitchSection(syntaxListBuilder, syntaxListBuilder2);
		}
		finally
		{
			_pool.Free(syntaxListBuilder2);
			_pool.Free(syntaxListBuilder);
		}
	}

	private ThrowStatementSyntax ParseThrowStatement()
	{
		SyntaxToken throwKeyword = EatToken(SyntaxKind.ThrowKeyword);
		ExpressionSyntax expression = null;
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			expression = ParseExpressionCore();
		}
		SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return _syntaxFactory.ThrowStatement(throwKeyword, expression, semicolonToken);
	}

	private UnsafeStatementSyntax ParseUnsafeStatement()
	{
		SyntaxToken unsafeKeyword = EatToken(SyntaxKind.UnsafeKeyword);
		BlockSyntax block = ParseBlock();
		return _syntaxFactory.UnsafeStatement(unsafeKeyword, block);
	}

	private UsingStatementSyntax ParseUsingStatement()
	{
		SyntaxToken usingKeyword = EatToken(SyntaxKind.UsingKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		VariableDeclarationSyntax declaration = null;
		ExpressionSyntax expression = null;
		ResetPoint resetPoint = GetResetPoint();
		ParseUsingExpression(ref declaration, ref expression, ref resetPoint);
		Release(ref resetPoint);
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		return _syntaxFactory.UsingStatement(usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
	}

	private void ParseUsingExpression(ref VariableDeclarationSyntax declaration, ref ExpressionSyntax expression, ref ResetPoint resetPoint)
	{
		if (IsAwaitExpression())
		{
			expression = ParseExpressionCore();
			return;
		}
		ScanTypeFlags scanTypeFlags = ((!IsQueryExpression(mayBeVariableDeclaration: true, mayBeMemberDeclaration: false)) ? ScanType() : ScanTypeFlags.NotType);
		if (scanTypeFlags == ScanTypeFlags.NullableType)
		{
			if (CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				Reset(ref resetPoint);
				expression = ParseExpressionCore();
				return;
			}
			switch (PeekToken(1).Kind)
			{
			default:
				Reset(ref resetPoint);
				expression = ParseExpressionCore();
				break;
			case SyntaxKind.CloseParenToken:
			case SyntaxKind.CommaToken:
				Reset(ref resetPoint);
				declaration = ParseVariableDeclaration();
				break;
			case SyntaxKind.EqualsToken:
				Reset(ref resetPoint);
				declaration = ParseVariableDeclaration();
				if (CurrentToken.Kind == SyntaxKind.ColonToken && declaration.Type.Kind == SyntaxKind.NullableType && SyntaxFacts.IsName(((NullableTypeSyntax)declaration.Type).ElementType.Kind) && declaration.Variables.Count == 1)
				{
					Reset(ref resetPoint);
					declaration = null;
					expression = ParseExpressionCore();
				}
				break;
			}
		}
		else if (IsUsingStatementVariableDeclaration(scanTypeFlags))
		{
			Reset(ref resetPoint);
			declaration = ParseVariableDeclaration();
		}
		else
		{
			Reset(ref resetPoint);
			expression = ParseExpressionCore();
		}
	}

	private bool IsUsingStatementVariableDeclaration(ScanTypeFlags st)
	{
		bool num = st == ScanTypeFlags.MustBeType && CurrentToken.Kind != SyntaxKind.DotToken;
		bool flag = st != ScanTypeFlags.NotType && CurrentToken.Kind == SyntaxKind.IdentifierToken;
		bool flag2 = st == ScanTypeFlags.NonGenericTypeOrExpression || PeekToken(1).Kind == SyntaxKind.EqualsToken;
		if (!num)
		{
			return flag & flag2;
		}
		return true;
	}

	private WhileStatementSyntax ParseWhileStatement()
	{
		SyntaxToken whileKeyword = EatToken(SyntaxKind.WhileKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax condition = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		return _syntaxFactory.WhileStatement(whileKeyword, openParenToken, condition, closeParenToken, statement);
	}

	private LabeledStatementSyntax ParseLabeledStatement()
	{
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		StatementSyntax statement = ParseStatementCore() ?? SyntaxFactory.EmptyStatement(EatToken(SyntaxKind.SemicolonToken));
		return _syntaxFactory.LabeledStatement(identifier, colonToken, statement);
	}

	private StatementSyntax ParseLocalDeclarationStatement()
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		ParseDeclarationModifiers(syntaxListBuilder);
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			ParseLocalDeclaration(builder, allowLocalFunctions: true, syntaxListBuilder.ToList(), out var type, out var localFunction);
			if (localFunction != null)
			{
				return localFunction;
			}
			if (syntaxListBuilder.Count > 0 && IsAccessibilityModifier(((SyntaxToken)syntaxListBuilder[0]).ContextualKind))
			{
				return null;
			}
			for (int i = 0; i < syntaxListBuilder.Count; i++)
			{
				SyntaxToken syntaxToken = (SyntaxToken)syntaxListBuilder[i];
				if (IsAdditionalLocalFunctionModifier(syntaxToken.ContextualKind))
				{
					syntaxListBuilder[i] = AddError(syntaxToken, ErrorCode.ERR_BadMemberFlag, syntaxToken.Text);
				}
			}
			SyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			return _syntaxFactory.LocalDeclarationStatement(syntaxListBuilder.ToList(), _syntaxFactory.VariableDeclaration(type, builder), semicolonToken);
		}
		finally
		{
			_pool.Free(in builder);
			_pool.Free(syntaxListBuilder);
		}
	}

	private VariableDesignationSyntax ParseDesignation()
	{
		VariableDesignationSyntax result;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
			SeparatedSyntaxListBuilder<VariableDesignationSyntax> builder = _pool.AllocateSeparated<VariableDesignationSyntax>();
			builder.Add(ParseDesignation());
			builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
			while (true)
			{
				builder.Add(ParseDesignation());
				if (CurrentToken.Kind != SyntaxKind.CommaToken)
				{
					break;
				}
				builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
			}
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			result = _syntaxFactory.ParenthesizedVariableDesignation(openParenToken, builder, closeParenToken);
			_pool.Free(in builder);
		}
		else
		{
			result = ParseSimpleDesignation();
		}
		return result;
	}

	private VariableDesignationSyntax ParseSimpleDesignation()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.UnderscoreToken)
		{
			SyntaxToken underscoreToken = EatContextualToken(SyntaxKind.UnderscoreToken);
			return _syntaxFactory.DiscardDesignation(underscoreToken);
		}
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);
		return _syntaxFactory.SingleVariableDesignation(identifier);
	}

	private WhenClauseSyntax ParseWhenClause()
	{
		if (CurrentToken.ContextualKind != SyntaxKind.WhenKeyword)
		{
			return null;
		}
		SyntaxToken whenKeyword = EatContextualToken(SyntaxKind.WhenKeyword);
		ExpressionSyntax condition = ParseSubExpression(Precedence.Expression);
		return _syntaxFactory.WhenClause(whenKeyword, condition);
	}

	private VariableDeclarationSyntax ParseVariableDeclaration()
	{
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> builder = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		ParseLocalDeclaration(builder, allowLocalFunctions: false, default, out var type, out var _);
		VariableDeclarationSyntax result = _syntaxFactory.VariableDeclaration(type, builder);
		_pool.Free(in builder);
		return result;
	}

	private void ParseLocalDeclaration(SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> variables, bool allowLocalFunctions, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, out TypeSyntax type, out LocalFunctionStatementSyntax localFunction)
	{
		type = (allowLocalFunctions ? ParseReturnType() : ParseType());
		VariableFlags variableFlags = VariableFlags.Local;
		if (mods.Any(8350))
		{
			variableFlags |= VariableFlags.Const;
		}
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfDeclarationClause;
		ParseVariableDeclarators(type, variableFlags, variables, variableDeclarationsExpected: true, allowLocalFunctions, mods, out localFunction);
		_termState = termState;
		if (allowLocalFunctions && localFunction == null)
		{
			PredefinedTypeSyntax obj = type as PredefinedTypeSyntax;
			if (obj != null && obj.Keyword.Kind == SyntaxKind.VoidKeyword)
			{
				type = AddError(type, ErrorCode.ERR_NoVoidHere);
			}
		}
	}

	private bool IsEndOfDeclarationClause()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind - 8211 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private void ParseDeclarationModifiers(SyntaxListBuilder list)
	{
		SyntaxKind contextualKind;
		while (IsDeclarationModifier(contextualKind = CurrentToken.ContextualKind) || IsAdditionalLocalFunctionModifier(contextualKind))
		{
			SyntaxToken syntaxToken;
			if (contextualKind == SyntaxKind.AsyncKeyword)
			{
				ResetPoint state = GetResetPoint();
				bool num = !IsPossibleStartOfTypeDeclaration(EatToken().Kind) && !IsDeclarationModifier(CurrentToken.Kind) && !IsAdditionalLocalFunctionModifier(CurrentToken.Kind) && (ScanType() == ScanTypeFlags.NotType || CurrentToken.Kind != SyntaxKind.IdentifierToken);
				Reset(ref state);
				Release(ref state);
				if (num)
				{
					break;
				}
				syntaxToken = EatContextualToken(contextualKind);
				if (contextualKind == SyntaxKind.AsyncKeyword)
				{
					syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureAsync);
				}
			}
			else
			{
				syntaxToken = EatToken();
			}
			if (contextualKind == SyntaxKind.StaticKeyword || contextualKind == SyntaxKind.ReadOnlyKeyword || contextualKind == SyntaxKind.VolatileKeyword)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadMemberFlag, syntaxToken.Text);
			}
			else if (list.Any(syntaxToken.RawKind))
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_TypeExpected, syntaxToken.Text);
			}
			list.Add(syntaxToken);
		}
	}

	private static bool IsDeclarationModifier(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.VolatileKeyword:
			return true;
		default:
			return false;
		}
	}

	private static bool IsAdditionalLocalFunctionModifier(SyntaxKind kind)
	{
		if (kind - 8343 <= (SyntaxKind)3 || kind == SyntaxKind.UnsafeKeyword || kind == SyntaxKind.AsyncKeyword)
		{
			return true;
		}
		return false;
	}

	private static bool IsAccessibilityModifier(SyntaxKind kind)
	{
		if (kind - 8343 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	private LocalFunctionStatementSyntax TryParseLocalFunctionStatementBody(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier)
	{
		ResetPoint state = GetResetPoint();
		bool flag = true;
		if (type.Kind == SyntaxKind.IdentifierName)
		{
			flag = ((IdentifierNameSyntax)type).Identifier.ContextualKind != SyntaxKind.AwaitKeyword;
		}
		bool isInAsync = IsInAsync;
		IsInAsync = false;
		SyntaxListBuilder syntaxListBuilder = null;
		for (int i = 0; i < modifiers.Count; i++)
		{
			switch (modifiers[i].ContextualKind)
			{
			case SyntaxKind.AsyncKeyword:
				IsInAsync = true;
				flag = true;
				continue;
			case SyntaxKind.UnsafeKeyword:
				flag = true;
				continue;
			case SyntaxKind.StaticKeyword:
			case SyntaxKind.ReadOnlyKeyword:
			case SyntaxKind.VolatileKeyword:
				continue;
			}
			if (syntaxListBuilder == null)
			{
				syntaxListBuilder = _pool.Allocate();
				syntaxListBuilder.AddRange(modifiers);
			}
			syntaxListBuilder[i] = AddError(modifiers[i], ErrorCode.ERR_BadMemberFlag, modifiers[i].Text);
		}
		if (syntaxListBuilder != null)
		{
			modifiers = syntaxListBuilder.ToList();
			_pool.Free(syntaxListBuilder);
		}
		TypeParameterListSyntax typeParameterList = ParseTypeParameterList();
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList();
		if (!flag)
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = parameterListSyntax.Parameters;
			for (int j = 0; j < parameters.Count; j++)
			{
				flag |= !parameters[j].ContainsDiagnostics;
				if (flag)
				{
					break;
				}
			}
		}
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder2 = default;
		if (CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
		{
			syntaxListBuilder2 = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
			ParseTypeParameterConstraintClauses(syntaxListBuilder2);
			flag = true;
		}
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon, parseSemicolonAfterBlock: false);
		IsInAsync = isInAsync;
		if (!flag && blockBody == null && expressionBody == null)
		{
			Reset(ref state);
			Release(ref state);
			return null;
		}
		Release(ref state);
		identifier = CheckFeatureAvailability(identifier, MessageID.IDS_FeatureLocalFunctions);
		return _syntaxFactory.LocalFunctionStatement(modifiers, type, identifier, typeParameterList, parameterListSyntax, syntaxListBuilder2, blockBody, expressionBody, semicolon);
	}

	private ExpressionStatementSyntax ParseExpressionStatement()
	{
		return ParseExpressionStatement(ParseExpressionCore());
	}

	private ExpressionStatementSyntax ParseExpressionStatement(ExpressionSyntax expression)
	{
		SyntaxToken semicolonToken = ((!IsScript || CurrentToken.Kind != SyntaxKind.EndOfFileToken) ? EatToken(SyntaxKind.SemicolonToken) : SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken));
		return _syntaxFactory.ExpressionStatement(expression, semicolonToken);
	}

	public ExpressionSyntax ParseExpression()
	{
		return ParseWithStackGuard(ParseExpressionCore, CreateMissingIdentifierName);
	}

	private ExpressionSyntax ParseExpressionCore()
	{
		return ParseSubExpression(Precedence.Expression);
	}

	private bool IsPossibleExpression()
	{
		SyntaxKind kind = CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.ColonColonToken:
		case SyntaxKind.TypeOfKeyword:
		case SyntaxKind.SizeOfKeyword:
		case SyntaxKind.NullKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.ThrowKeyword:
		case SyntaxKind.StackAllocKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.MakeRefKeyword:
		case SyntaxKind.RefTypeKeyword:
		case SyntaxKind.RefValueKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.BaseKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
		case SyntaxKind.InterpolatedStringStartToken:
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.InterpolatedStringToken:
			return true;
		case SyntaxKind.IdentifierToken:
			if (!IsTrueIdentifier())
			{
				return CurrentToken.ContextualKind == SyntaxKind.FromKeyword;
			}
			return true;
		default:
			if (!IsExpectedPrefixUnaryOperator(kind) && (!IsPredefinedType(kind) || kind == SyntaxKind.VoidKeyword) && !SyntaxFacts.IsAnyUnaryExpression(kind) && !SyntaxFacts.IsBinaryExpression(kind))
			{
				return SyntaxFacts.IsAssignmentExpressionOperatorToken(kind);
			}
			return true;
		}
	}

	private static bool IsInvalidSubExpression(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.IfKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.SwitchKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.TryKeyword:
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
		case SyntaxKind.LockKeyword:
		case SyntaxKind.GotoKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.ReturnKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsRightAssociative(SyntaxKind op)
	{
		if (op == SyntaxKind.CoalesceExpression || op - 8714 <= (SyntaxKind)10)
		{
			return true;
		}
		return false;
	}

	private static Precedence GetPrecedence(SyntaxKind op)
	{
		switch (op)
		{
		case SyntaxKind.SimpleAssignmentExpression:
		case SyntaxKind.AddAssignmentExpression:
		case SyntaxKind.SubtractAssignmentExpression:
		case SyntaxKind.MultiplyAssignmentExpression:
		case SyntaxKind.DivideAssignmentExpression:
		case SyntaxKind.ModuloAssignmentExpression:
		case SyntaxKind.AndAssignmentExpression:
		case SyntaxKind.ExclusiveOrAssignmentExpression:
		case SyntaxKind.OrAssignmentExpression:
		case SyntaxKind.LeftShiftAssignmentExpression:
		case SyntaxKind.RightShiftAssignmentExpression:
			return Precedence.Assignment;
		case SyntaxKind.CoalesceExpression:
			return Precedence.Coalescing;
		case SyntaxKind.LogicalOrExpression:
			return Precedence.ConditionalOr;
		case SyntaxKind.LogicalAndExpression:
			return Precedence.ConditionalAnd;
		case SyntaxKind.BitwiseOrExpression:
			return Precedence.LogicalOr;
		case SyntaxKind.ExclusiveOrExpression:
			return Precedence.LogicalXor;
		case SyntaxKind.BitwiseAndExpression:
			return Precedence.LogicalAnd;
		case SyntaxKind.EqualsExpression:
		case SyntaxKind.NotEqualsExpression:
			return Precedence.Equality;
		case SyntaxKind.IsPatternExpression:
		case SyntaxKind.LessThanExpression:
		case SyntaxKind.LessThanOrEqualExpression:
		case SyntaxKind.GreaterThanExpression:
		case SyntaxKind.GreaterThanOrEqualExpression:
		case SyntaxKind.IsExpression:
		case SyntaxKind.AsExpression:
			return Precedence.Relational;
		case SyntaxKind.LeftShiftExpression:
		case SyntaxKind.RightShiftExpression:
			return Precedence.Shift;
		case SyntaxKind.AddExpression:
		case SyntaxKind.SubtractExpression:
			return Precedence.Additive;
		case SyntaxKind.MultiplyExpression:
		case SyntaxKind.DivideExpression:
		case SyntaxKind.ModuloExpression:
			return Precedence.Mutiplicative;
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.UnaryMinusExpression:
		case SyntaxKind.BitwiseNotExpression:
		case SyntaxKind.LogicalNotExpression:
		case SyntaxKind.PreIncrementExpression:
		case SyntaxKind.PreDecrementExpression:
		case SyntaxKind.AwaitExpression:
		case SyntaxKind.TypeOfExpression:
		case SyntaxKind.SizeOfExpression:
		case SyntaxKind.CheckedExpression:
		case SyntaxKind.UncheckedExpression:
		case SyntaxKind.MakeRefExpression:
		case SyntaxKind.RefValueExpression:
		case SyntaxKind.RefTypeExpression:
			return Precedence.Unary;
		case SyntaxKind.CastExpression:
			return Precedence.Cast;
		case SyntaxKind.PointerIndirectionExpression:
			return Precedence.PointerIndirection;
		case SyntaxKind.AddressOfExpression:
			return Precedence.AddressOf;
		default:
			return Precedence.Expression;
		}
	}

	private static bool IsExpectedPrefixUnaryOperator(SyntaxKind kind)
	{
		if (SyntaxFacts.IsPrefixUnaryExpression(kind) && kind != SyntaxKind.RefKeyword)
		{
			return kind != SyntaxKind.OutKeyword;
		}
		return false;
	}

	private static bool IsExpectedBinaryOperator(SyntaxKind kind)
	{
		return SyntaxFacts.IsBinaryExpression(kind);
	}

	private static bool IsExpectedAssignmentOperator(SyntaxKind kind)
	{
		return SyntaxFacts.IsAssignmentExpressionOperatorToken(kind);
	}

	private bool IsPossibleAwaitExpressionStatement()
	{
		if (IsScript || IsInAsync)
		{
			return CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword;
		}
		return false;
	}

	private bool IsAwaitExpression()
	{
		if (CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword)
		{
			if (IsInAsync)
			{
				return true;
			}
			switch (PeekToken(1).Kind)
			{
			case SyntaxKind.TypeOfKeyword:
			case SyntaxKind.NullKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.DefaultKeyword:
			case SyntaxKind.NewKeyword:
			case SyntaxKind.ThisKeyword:
			case SyntaxKind.BaseKeyword:
			case SyntaxKind.DelegateKeyword:
			case SyntaxKind.CheckedKeyword:
			case SyntaxKind.UncheckedKeyword:
			case SyntaxKind.InterpolatedStringStartToken:
			case SyntaxKind.IdentifierToken:
			case SyntaxKind.NumericLiteralToken:
			case SyntaxKind.CharacterLiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.InterpolatedStringToken:
				return true;
			}
		}
		return false;
	}

	private ExpressionSyntax ParseSubExpression(Precedence precedence)
	{
		_recursionDepth++;
		StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
		ExpressionSyntax result = ParseSubExpressionCore(precedence);
		_recursionDepth--;
		return result;
	}

	private ExpressionSyntax ParseSubExpressionCore(Precedence precedence)
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (IsInvalidSubExpression(kind))
		{
			return AddError(CreateMissingIdentifierName(), ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
		}
		ExpressionSyntax expressionSyntax;
		if (IsExpectedPrefixUnaryOperator(kind))
		{
			SyntaxKind prefixUnaryExpression = SyntaxFacts.GetPrefixUnaryExpression(kind);
			Precedence precedence2 = GetPrecedence(prefixUnaryExpression);
			SyntaxToken operatorToken = EatToken();
			ExpressionSyntax operand = ParseSubExpression(precedence2);
			expressionSyntax = _syntaxFactory.PrefixUnaryExpression(prefixUnaryExpression, operatorToken, operand);
		}
		else if (IsAwaitExpression())
		{
			SyntaxKind prefixUnaryExpression = SyntaxKind.AwaitExpression;
			Precedence precedence2 = GetPrecedence(prefixUnaryExpression);
			SyntaxToken node = EatContextualToken(SyntaxKind.AwaitKeyword);
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureAsync);
			ExpressionSyntax expression = ParseSubExpression(precedence2);
			expressionSyntax = _syntaxFactory.AwaitExpression(node, expression);
		}
		else if (IsQueryExpression(mayBeVariableDeclaration: false, mayBeMemberDeclaration: false))
		{
			expressionSyntax = ParseQueryExpression(precedence);
		}
		else if (CurrentToken.ContextualKind == SyntaxKind.FromKeyword && IsInQuery)
		{
			SyntaxToken node2 = EatToken();
			node2 = AddError(node2, ErrorCode.ERR_InvalidExprTerm, CurrentToken.Text);
			expressionSyntax = AddTrailingSkippedSyntax(CreateMissingIdentifierName(), node2);
		}
		else
		{
			if (kind == SyntaxKind.ThrowKeyword)
			{
				ExpressionSyntax expressionSyntax2 = ParseThrowExpression();
				if (precedence > Precedence.Coalescing)
				{
					return AddError(expressionSyntax2, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
				}
				return expressionSyntax2;
			}
			expressionSyntax = ((!IsPossibleDeconstructionLeft(precedence)) ? ParseTerm(precedence) : ParseDeclarationExpression(ParseTypeMode.Normal, MessageID.IDS_FeatureTuples));
		}
		while (true)
		{
			kind = CurrentToken.ContextualKind;
			bool flag = false;
			SyntaxKind prefixUnaryExpression;
			if (IsExpectedBinaryOperator(kind))
			{
				prefixUnaryExpression = SyntaxFacts.GetBinaryExpression(kind);
			}
			else
			{
				if (!IsExpectedAssignmentOperator(kind))
				{
					break;
				}
				prefixUnaryExpression = SyntaxFacts.GetAssignmentExpression(kind);
				flag = true;
			}
			Precedence precedence2 = GetPrecedence(prefixUnaryExpression);
			bool flag2 = false;
			if (kind == SyntaxKind.GreaterThanToken && (PeekToken(1).Kind == SyntaxKind.GreaterThanToken || PeekToken(1).Kind == SyntaxKind.GreaterThanEqualsToken) && CurrentToken.GetTrailingTriviaWidth() == 0 && PeekToken(1).GetLeadingTriviaWidth() == 0)
			{
				if (PeekToken(1).Kind == SyntaxKind.GreaterThanToken)
				{
					prefixUnaryExpression = SyntaxFacts.GetBinaryExpression(SyntaxKind.GreaterThanGreaterThanToken);
				}
				else
				{
					prefixUnaryExpression = SyntaxFacts.GetAssignmentExpression(SyntaxKind.GreaterThanGreaterThanEqualsToken);
					flag = true;
				}
				precedence2 = GetPrecedence(prefixUnaryExpression);
				flag2 = true;
			}
			if (precedence2 < precedence || (precedence2 == precedence && !IsRightAssociative(prefixUnaryExpression)))
			{
				break;
			}
			SyntaxToken syntaxToken = EatContextualToken(kind);
			if (flag2)
			{
				SyntaxToken syntaxToken2 = EatToken();
				SyntaxKind kind2 = ((syntaxToken2.Kind == SyntaxKind.GreaterThanToken) ? SyntaxKind.GreaterThanGreaterThanToken : SyntaxKind.GreaterThanGreaterThanEqualsToken);
				syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), kind2, syntaxToken2.GetTrailingTrivia());
			}
			switch (prefixUnaryExpression)
			{
			case SyntaxKind.AsExpression:
			{
				TypeSyntax right = ParseType(ParseTypeMode.AsExpression);
				expressionSyntax = _syntaxFactory.BinaryExpression(prefixUnaryExpression, expressionSyntax, syntaxToken, right);
				continue;
			}
			case SyntaxKind.IsExpression:
				expressionSyntax = ParseIsExpression(expressionSyntax, syntaxToken);
				continue;
			}
			if (flag)
			{
				ExpressionSyntax right2 = ((prefixUnaryExpression == SyntaxKind.SimpleAssignmentExpression && CurrentToken.Kind == SyntaxKind.RefKeyword) ? CheckFeatureAvailability(ParsePossibleRefExpression(), MessageID.IDS_FeatureRefReassignment) : ParseSubExpression(precedence2));
				expressionSyntax = _syntaxFactory.AssignmentExpression(prefixUnaryExpression, expressionSyntax, syntaxToken, right2);
			}
			else
			{
				expressionSyntax = _syntaxFactory.BinaryExpression(prefixUnaryExpression, expressionSyntax, syntaxToken, ParseSubExpression(precedence2));
			}
		}
		if (kind == SyntaxKind.QuestionToken && precedence <= Precedence.Ternary)
		{
			SyntaxToken questionToken = EatToken();
			ExpressionSyntax whenTrue = ParsePossibleRefExpression();
			if (CurrentToken.Kind == SyntaxKind.EndOfFileToken && lexer.InterpolationFollowedByColon)
			{
				SyntaxToken colonToken = SyntaxFactory.MissingToken(SyntaxKind.ColonToken);
				IdentifierNameSyntax whenFalse = _syntaxFactory.IdentifierName(SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken));
				expressionSyntax = _syntaxFactory.ConditionalExpression(expressionSyntax, questionToken, whenTrue, colonToken, whenFalse);
				expressionSyntax = AddError(expressionSyntax, ErrorCode.ERR_ConditionalInInterpolation);
			}
			else
			{
				SyntaxToken colonToken2 = EatToken(SyntaxKind.ColonToken);
				ExpressionSyntax whenFalse2 = ParsePossibleRefExpression();
				expressionSyntax = _syntaxFactory.ConditionalExpression(expressionSyntax, questionToken, whenTrue, colonToken2, whenFalse2);
			}
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseDeclarationExpression(ParseTypeMode mode, MessageID feature)
	{
		TypeSyntax type = ParseType(mode);
		VariableDesignationSyntax variableDesignationSyntax = ParseDesignation();
		if (feature != MessageID.None)
		{
			variableDesignationSyntax = CheckFeatureAvailability(variableDesignationSyntax, feature);
		}
		return _syntaxFactory.DeclarationExpression(type, variableDesignationSyntax);
	}

	private ExpressionSyntax ParseThrowExpression()
	{
		SyntaxToken throwKeyword = EatToken(SyntaxKind.ThrowKeyword);
		ExpressionSyntax expression = ParseSubExpression(Precedence.Coalescing);
		ThrowExpressionSyntax node = _syntaxFactory.ThrowExpression(throwKeyword, expression);
		return CheckFeatureAvailability(node, MessageID.IDS_FeatureThrowExpression);
	}

	private ExpressionSyntax ParseIsExpression(ExpressionSyntax leftOperand, SyntaxToken opToken)
	{
		CSharpSyntaxNode cSharpSyntaxNode = ParseTypeOrPatternForIsOperator();
		if (cSharpSyntaxNode is PatternSyntax)
		{
			IsPatternExpressionSyntax node = _syntaxFactory.IsPatternExpression(leftOperand, opToken, (PatternSyntax)cSharpSyntaxNode);
			return CheckFeatureAvailability(node, MessageID.IDS_FeaturePatternMatching);
		}
		return _syntaxFactory.BinaryExpression(SyntaxKind.IsExpression, leftOperand, opToken, (TypeSyntax)cSharpSyntaxNode);
	}

	private ExpressionSyntax ParseTerm(Precedence precedence)
	{
		SyntaxKind kind = CurrentToken.Kind;
		ExpressionSyntax expressionSyntax;
		switch (kind)
		{
		case SyntaxKind.TypeOfKeyword:
			expressionSyntax = ParseTypeOfExpression();
			break;
		case SyntaxKind.DefaultKeyword:
			expressionSyntax = ParseDefaultExpression();
			break;
		case SyntaxKind.SizeOfKeyword:
			expressionSyntax = ParseSizeOfExpression();
			break;
		case SyntaxKind.MakeRefKeyword:
			expressionSyntax = ParseMakeRefExpression();
			break;
		case SyntaxKind.RefTypeKeyword:
			expressionSyntax = ParseRefTypeExpression();
			break;
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
			expressionSyntax = ParseCheckedOrUncheckedExpression();
			break;
		case SyntaxKind.RefValueKeyword:
			expressionSyntax = ParseRefValueExpression();
			break;
		case SyntaxKind.ColonColonToken:
			expressionSyntax = ParseQualifiedName(NameOptions.InExpression);
			break;
		case SyntaxKind.IdentifierToken:
			if (IsTrueIdentifier())
			{
				if (CurrentToken.ContextualKind == SyntaxKind.AsyncKeyword && PeekToken(1).Kind == SyntaxKind.DelegateKeyword)
				{
					expressionSyntax = ParseAnonymousMethodExpression();
				}
				else if (IsPossibleLambdaExpression(precedence))
				{
					expressionSyntax = ParseLambdaExpression();
				}
				else
				{
					expressionSyntax = ((!IsPossibleDeconstructionLeft(precedence)) ? ParseAliasQualifiedName(NameOptions.InExpression) : ParseDeclarationExpression(ParseTypeMode.Normal, MessageID.IDS_FeatureTuples));
				}
				break;
			}
			expressionSyntax = CreateMissingIdentifierName();
			expressionSyntax = AddError(expressionSyntax, ErrorCode.ERR_InvalidExprTerm, CurrentToken.Text);
			break;
		case SyntaxKind.ThisKeyword:
			expressionSyntax = _syntaxFactory.ThisExpression(EatToken());
			break;
		case SyntaxKind.BaseKeyword:
			expressionSyntax = _syntaxFactory.BaseExpression(EatToken());
			break;
		case SyntaxKind.NullKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
			expressionSyntax = _syntaxFactory.LiteralExpression(SyntaxFacts.GetLiteralExpression(kind), EatToken());
			break;
		case SyntaxKind.InterpolatedStringStartToken:
			throw new NotImplementedException();
		case SyntaxKind.InterpolatedStringToken:
			expressionSyntax = ParseInterpolatedStringToken();
			break;
		case SyntaxKind.OpenParenToken:
			expressionSyntax = ParseCastOrParenExpressionOrLambdaOrTuple(precedence);
			break;
		case SyntaxKind.NewKeyword:
			expressionSyntax = ParseNewExpression();
			break;
		case SyntaxKind.StackAllocKeyword:
			expressionSyntax = ParseStackAllocExpression();
			break;
		case SyntaxKind.DelegateKeyword:
			expressionSyntax = ParseAnonymousMethodExpression();
			break;
		default:
			if (IsPredefinedType(kind))
			{
				expressionSyntax = _syntaxFactory.PredefinedType(EatToken());
				if (CurrentToken.Kind != SyntaxKind.DotToken || kind == SyntaxKind.VoidKeyword)
				{
					expressionSyntax = AddError(expressionSyntax, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
				}
			}
			else
			{
				expressionSyntax = CreateMissingIdentifierName();
				expressionSyntax = ((kind != SyntaxKind.EndOfFileToken) ? AddError(expressionSyntax, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind)) : AddError(expressionSyntax, ErrorCode.ERR_ExpressionExpected));
			}
			break;
		}
		return ParsePostFixExpression(expressionSyntax);
	}

	private bool IsPossibleDeconstructionLeft(Precedence precedence)
	{
		if (precedence > Precedence.Assignment || (!CurrentToken.IsVar() && !IsPredefinedType(CurrentToken.Kind)))
		{
			return false;
		}
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			return CurrentToken.Kind == SyntaxKind.OpenParenToken && ScanDesignator() && CurrentToken.Kind == SyntaxKind.EqualsToken;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool ScanDesignator()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			if (kind == SyntaxKind.IdentifierToken && IsTrueIdentifier())
			{
				EatToken();
				return true;
			}
			return false;
		}
		while (true)
		{
			EatToken();
			if (!ScanDesignator())
			{
				break;
			}
			switch (CurrentToken.Kind)
			{
			case SyntaxKind.CommaToken:
				break;
			case SyntaxKind.CloseParenToken:
				EatToken();
				return true;
			default:
				return false;
			}
		}
		return false;
	}

	private bool IsPossibleLambdaExpression(Precedence precedence)
	{
		if (precedence <= Precedence.Assignment && PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			return true;
		}
		if (ScanAsyncLambda(precedence))
		{
			return true;
		}
		return false;
	}

	private ExpressionSyntax ParsePostFixExpression(ExpressionSyntax expr)
	{
		while (true)
		{
			SyntaxKind kind = CurrentToken.Kind;
			switch (kind)
			{
			case SyntaxKind.OpenParenToken:
				expr = _syntaxFactory.InvocationExpression(expr, ParseParenthesizedArgumentList());
				continue;
			case SyntaxKind.OpenBracketToken:
				expr = _syntaxFactory.ElementAccessExpression(expr, ParseBracketedArgumentList());
				continue;
			case SyntaxKind.MinusMinusToken:
			case SyntaxKind.PlusPlusToken:
				expr = _syntaxFactory.PostfixUnaryExpression(SyntaxFacts.GetPostfixUnaryExpression(kind), expr, EatToken());
				continue;
			case SyntaxKind.ColonColonToken:
				if (PeekToken(1).Kind == SyntaxKind.IdentifierToken)
				{
					SyntaxToken node = EatToken();
					node = AddError(node, ErrorCode.ERR_UnexpectedAliasedName);
					SyntaxToken operatorToken2 = ConvertToMissingWithTrailingTrivia(node, SyntaxKind.DotToken);
					expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, operatorToken2, ParseSimpleName(NameOptions.InExpression));
				}
				else
				{
					expr = AddTrailingSkippedSyntax(expr, EatTokenWithPrejudice(SyntaxKind.DotToken));
				}
				continue;
			case SyntaxKind.MinusGreaterThanToken:
				expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.PointerMemberAccessExpression, expr, EatToken(), ParseSimpleName(NameOptions.InExpression));
				continue;
			case SyntaxKind.DotToken:
				if (CurrentToken.TrailingTrivia.Any(8539) && PeekToken(1).Kind == SyntaxKind.IdentifierToken && PeekToken(2).ContextualKind == SyntaxKind.IdentifierToken)
				{
					expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, EatToken(), AddError(CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected));
					return expr;
				}
				expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, EatToken(), ParseSimpleName(NameOptions.InExpression));
				continue;
			case SyntaxKind.QuestionToken:
				if (CanStartConsequenceExpression(PeekToken(1).Kind))
				{
					SyntaxToken operatorToken = EatToken();
					ExpressionSyntax whenNotNull = ParseConsequenceSyntax();
					expr = _syntaxFactory.ConditionalAccessExpression(expr, operatorToken, whenNotNull);
					expr = CheckFeatureAvailability(expr, MessageID.IDS_FeatureNullPropagatingOperator);
					continue;
				}
				break;
			}
			break;
		}
		return expr;
	}

	private static bool CanStartConsequenceExpression(SyntaxKind kind)
	{
		if (kind != SyntaxKind.DotToken)
		{
			return kind == SyntaxKind.OpenBracketToken;
		}
		return true;
	}

	internal ExpressionSyntax ParseConsequenceSyntax()
	{
		SyntaxKind kind = CurrentToken.Kind;
		ExpressionSyntax expressionSyntax = null;
		switch (kind)
		{
		case SyntaxKind.DotToken:
			expressionSyntax = _syntaxFactory.MemberBindingExpression(EatToken(), ParseSimpleName(NameOptions.InExpression));
			break;
		case SyntaxKind.OpenBracketToken:
			expressionSyntax = _syntaxFactory.ElementBindingExpression(ParseBracketedArgumentList());
			break;
		}
		while (true)
		{
			switch (CurrentToken.Kind)
			{
			case SyntaxKind.OpenParenToken:
				expressionSyntax = _syntaxFactory.InvocationExpression(expressionSyntax, ParseParenthesizedArgumentList());
				break;
			case SyntaxKind.OpenBracketToken:
				expressionSyntax = _syntaxFactory.ElementAccessExpression(expressionSyntax, ParseBracketedArgumentList());
				break;
			case SyntaxKind.DotToken:
				expressionSyntax = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expressionSyntax, EatToken(), ParseSimpleName(NameOptions.InExpression));
				break;
			case SyntaxKind.QuestionToken:
				if (CanStartConsequenceExpression(PeekToken(1).Kind))
				{
					SyntaxToken operatorToken = EatToken();
					ExpressionSyntax whenNotNull = ParseConsequenceSyntax();
					expressionSyntax = _syntaxFactory.ConditionalAccessExpression(expressionSyntax, operatorToken, whenNotNull);
				}
				return expressionSyntax;
			default:
				return expressionSyntax;
			}
		}
	}

	internal ArgumentListSyntax ParseParenthesizedArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.ArgumentList)
		{
			return (ArgumentListSyntax)EatNode();
		}
		ParseArgumentList(out var openToken, out var arguments, out var closeToken, SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken);
		return _syntaxFactory.ArgumentList(openToken, arguments, closeToken);
	}

	internal BracketedArgumentListSyntax ParseBracketedArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && CurrentNodeKind == SyntaxKind.BracketedArgumentList)
		{
			return (BracketedArgumentListSyntax)EatNode();
		}
		ParseArgumentList(out var openToken, out var arguments, out var closeToken, SyntaxKind.OpenBracketToken, SyntaxKind.CloseBracketToken);
		return _syntaxFactory.BracketedArgumentList(openToken, arguments, closeToken);
	}

	private void ParseArgumentList(out SyntaxToken openToken, out Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, out SyntaxToken closeToken, SyntaxKind openKind, SyntaxKind closeKind)
	{
		bool flag = openKind == SyntaxKind.OpenBracketToken;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken || CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			openToken = EatTokenAsKind(openKind);
		}
		else
		{
			openToken = EatToken(openKind);
		}
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfArgumentList;
		SeparatedSyntaxListBuilder<ArgumentSyntax> item = default;
		try
		{
			if (CurrentToken.Kind != closeKind && CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				do
				{
					if (item.IsNull)
					{
						item = _pool.AllocateSeparated<ArgumentSyntax>();
					}
					if (!IsPossibleArgumentExpression() && CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						continue;
					}
					item.Add(ParseArgumentExpression(flag));
					while (CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.CloseBracketToken && CurrentToken.Kind != SyntaxKind.SemicolonToken)
					{
						if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleArgumentExpression())
						{
							item.AddSeparator(EatToken(SyntaxKind.CommaToken));
							item.Add(ParseArgumentExpression(flag));
						}
						else if (SkipBadArgumentListTokens(ref openToken, item, SyntaxKind.CommaToken, closeKind) == PostSkipAction.Abort)
						{
							break;
						}
					}
					break;
				}
				while (SkipBadArgumentListTokens(ref openToken, item, SyntaxKind.IdentifierToken, closeKind) == PostSkipAction.Continue);
			}
			else if (flag && CurrentToken.Kind == closeKind)
			{
				if (item.IsNull)
				{
					item = _pool.AllocateSeparated<ArgumentSyntax>();
				}
				item.Add(ParseArgumentExpression(flag));
			}
			_termState = termState;
			if (CurrentToken.Kind == SyntaxKind.CloseParenToken || CurrentToken.Kind == SyntaxKind.CloseBracketToken)
			{
				closeToken = EatTokenAsKind(closeKind);
			}
			else
			{
				closeToken = EatToken(closeKind);
			}
			arguments = item.ToList();
		}
		finally
		{
			if (!item.IsNull)
			{
				_pool.Free(in item);
			}
		}
	}

	private PostSkipAction SkipBadArgumentListTokens(ref SyntaxToken open, SeparatedSyntaxListBuilder<ArgumentSyntax> list, SyntaxKind expected, SyntaxKind closeKind)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref open, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleArgumentExpression(), (LanguageParser p) => p.CurrentToken.Kind == closeKind || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private bool IsEndOfArgumentList()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return CurrentToken.Kind == SyntaxKind.CloseBracketToken;
		}
		return true;
	}

	private bool IsPossibleArgumentExpression()
	{
		if (!IsValidArgumentRefKindKeyword(CurrentToken.Kind))
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private static bool IsValidArgumentRefKindKeyword(SyntaxKind kind)
	{
		if (kind - 8360 <= (SyntaxKind)2)
		{
			return true;
		}
		return false;
	}

	private ArgumentSyntax ParseArgumentExpression(bool isIndexer)
	{
		NameColonSyntax nameColon = null;
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.ColonToken)
		{
			IdentifierNameSyntax name = ParseIdentifierName();
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
			nameColon = _syntaxFactory.NameColon(name, colonToken);
			nameColon = CheckFeatureAvailability(nameColon, MessageID.IDS_FeatureNamedArgument);
		}
		SyntaxToken syntaxToken = null;
		if (IsValidArgumentRefKindKeyword(CurrentToken.Kind))
		{
			syntaxToken = EatToken();
		}
		ExpressionSyntax expression;
		if (isIndexer && (CurrentToken.Kind == SyntaxKind.CommaToken || CurrentToken.Kind == SyntaxKind.CloseBracketToken))
		{
			expression = ParseIdentifierName(ErrorCode.ERR_ValueExpected);
		}
		else if (CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			expression = ParseIdentifierName(ErrorCode.ERR_MissingArgument);
		}
		else
		{
			if (syntaxToken != null && syntaxToken.Kind == SyntaxKind.InKeyword)
			{
				syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureReadOnlyReferences);
			}
			expression = ((syntaxToken != null && syntaxToken.Kind == SyntaxKind.OutKeyword) ? ParseExpressionOrDeclaration(ParseTypeMode.Normal, MessageID.IDS_FeatureOutVar, permitTupleDesignation: false) : ParseSubExpression(Precedence.Expression));
		}
		return _syntaxFactory.Argument(nameColon, syntaxToken, expression);
	}

	private TypeOfExpressionSyntax ParseTypeOfExpression()
	{
		SyntaxToken keyword = EatToken();
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TypeSyntax type = ParseTypeOrVoid();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.TypeOfExpression(keyword, openParenToken, type, closeParenToken);
	}

	private ExpressionSyntax ParseDefaultExpression()
	{
		SyntaxToken node = EatToken();
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
			TypeSyntax type = ParseType();
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureDefault);
			return _syntaxFactory.DefaultExpression(node, openParenToken, type, closeParenToken);
		}
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureDefaultLiteral);
		return _syntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, node);
	}

	private SizeOfExpressionSyntax ParseSizeOfExpression()
	{
		SyntaxToken keyword = EatToken();
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TypeSyntax type = ParseType();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.SizeOfExpression(keyword, openParenToken, type, closeParenToken);
	}

	private MakeRefExpressionSyntax ParseMakeRefExpression()
	{
		SyntaxToken keyword = EatToken();
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseSubExpression(Precedence.Expression);
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.MakeRefExpression(keyword, openParenToken, expression, closeParenToken);
	}

	private RefTypeExpressionSyntax ParseRefTypeExpression()
	{
		SyntaxToken keyword = EatToken();
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseSubExpression(Precedence.Expression);
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.RefTypeExpression(keyword, openParenToken, expression, closeParenToken);
	}

	private CheckedExpressionSyntax ParseCheckedOrUncheckedExpression()
	{
		SyntaxToken syntaxToken = EatToken();
		SyntaxKind kind = ((syntaxToken.Kind == SyntaxKind.CheckedKeyword) ? SyntaxKind.CheckedExpression : SyntaxKind.UncheckedExpression);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseSubExpression(Precedence.Expression);
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.CheckedExpression(kind, syntaxToken, openParenToken, expression, closeParenToken);
	}

	private RefValueExpressionSyntax ParseRefValueExpression()
	{
		SyntaxToken keyword = EatToken(SyntaxKind.RefValueKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expression = ParseSubExpression(Precedence.Expression);
		SyntaxToken comma = EatToken(SyntaxKind.CommaToken);
		TypeSyntax type = ParseType();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		return _syntaxFactory.RefValueExpression(keyword, openParenToken, expression, comma, type, closeParenToken);
	}

	private bool ScanParenthesizedImplicitlyTypedLambda(Precedence precedence)
	{
		if (precedence > Precedence.Assignment)
		{
			return false;
		}
		if (PeekToken(1).Kind == SyntaxKind.IdentifierToken && (!IsInQuery || !IsTokenQueryContextualKeyword(PeekToken(1))) && PeekToken(2).Kind == SyntaxKind.CommaToken)
		{
			int num = 3;
			SyntaxToken syntaxToken;
			do
			{
				syntaxToken = PeekToken(num++);
			}
			while (syntaxToken.Kind == SyntaxKind.IdentifierToken || SyntaxFacts.IsPredefinedType(syntaxToken.Kind) || syntaxToken.Kind == SyntaxKind.CommaToken || (!IsInQuery && IsTokenQueryContextualKeyword(syntaxToken)));
			if (PeekToken(num - 1).Kind == SyntaxKind.CloseParenToken)
			{
				return PeekToken(num).Kind == SyntaxKind.EqualsGreaterThanToken;
			}
			return false;
		}
		if (IsTrueIdentifier(PeekToken(1)) && PeekToken(2).Kind == SyntaxKind.CloseParenToken && PeekToken(3).Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			return true;
		}
		if (PeekToken(1).Kind == SyntaxKind.CloseParenToken && PeekToken(2).Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			return true;
		}
		if (PeekToken(1).Kind == SyntaxKind.ParamsKeyword)
		{
			return true;
		}
		return false;
	}

	private bool ScanExplicitlyTypedLambda(Precedence precedence)
	{
		if (precedence > Precedence.Assignment)
		{
			return false;
		}
		ResetPoint state = GetResetPoint();
		try
		{
			bool flag = false;
			do
			{
				EatToken();
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.RefKeyword:
					EatToken();
					flag = true;
					if (CurrentToken.Kind == SyntaxKind.ReadOnlyKeyword)
					{
						EatToken();
					}
					break;
				case SyntaxKind.OutKeyword:
				case SyntaxKind.InKeyword:
				case SyntaxKind.ParamsKeyword:
					EatToken();
					flag = true;
					break;
				}
				if (CurrentToken.Kind == SyntaxKind.EndOfFileToken)
				{
					return flag;
				}
				if (ScanType() == ScanTypeFlags.NotType)
				{
					return false;
				}
				if (IsTrueIdentifier())
				{
					EatToken();
				}
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.EndOfFileToken:
					return flag;
				case SyntaxKind.CommaToken:
					break;
				case SyntaxKind.CloseParenToken:
					return PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken;
				default:
					return false;
				}
			}
			while (!flag);
			return true;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private ExpressionSyntax ParseCastOrParenExpressionOrLambdaOrTuple(Precedence precedence)
	{
		ResetPoint state = GetResetPoint();
		try
		{
			if (ScanParenthesizedImplicitlyTypedLambda(precedence))
			{
				return ParseLambdaExpression();
			}
			if (ScanCast() && !IsCurrentTokenQueryKeywordInQuery())
			{
				Reset(ref state);
				SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
				TypeSyntax type = ParseType();
				SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
				ExpressionSyntax expression = ParseSubExpression(Precedence.Cast);
				return _syntaxFactory.CastExpression(openParenToken, type, closeParenToken, expression);
			}
			Reset(ref state);
			if (ScanExplicitlyTypedLambda(precedence))
			{
				return ParseLambdaExpression();
			}
			Reset(ref state);
			SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenParenToken);
			ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.FirstElementOfPossibleTupleLiteral, MessageID.None, permitTupleDesignation: true);
			if (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				ArgumentSyntax firstArg = _syntaxFactory.Argument(null, null, expressionSyntax);
				return ParseTupleExpressionTail(syntaxToken, firstArg);
			}
			if (expressionSyntax.Kind == SyntaxKind.IdentifierName && CurrentToken.Kind == SyntaxKind.ColonToken)
			{
				NameColonSyntax nameColon = _syntaxFactory.NameColon((IdentifierNameSyntax)expressionSyntax, EatToken());
				expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.FirstElementOfPossibleTupleLiteral, MessageID.None, permitTupleDesignation: true);
				ArgumentSyntax firstArg2 = _syntaxFactory.Argument(nameColon, null, expressionSyntax);
				return ParseTupleExpressionTail(syntaxToken, firstArg2);
			}
			SyntaxToken closeParenToken2 = EatToken(SyntaxKind.CloseParenToken);
			return _syntaxFactory.ParenthesizedExpression(syntaxToken, expressionSyntax, closeParenToken2);
		}
		finally
		{
			Release(ref state);
		}
	}

	private TupleExpressionSyntax ParseTupleExpressionTail(SyntaxToken openParen, ArgumentSyntax firstArg)
	{
		SeparatedSyntaxListBuilder<ArgumentSyntax> builder = _pool.AllocateSeparated<ArgumentSyntax>();
		try
		{
			builder.Add(firstArg);
			while (CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				SyntaxToken separatorToken = EatToken(SyntaxKind.CommaToken);
				builder.AddSeparator(separatorToken);
				ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.AfterTupleComma, MessageID.None, permitTupleDesignation: true);
				ArgumentSyntax node;
				if (expressionSyntax.Kind == SyntaxKind.IdentifierName && CurrentToken.Kind == SyntaxKind.ColonToken)
				{
					NameColonSyntax nameColon = _syntaxFactory.NameColon((IdentifierNameSyntax)expressionSyntax, EatToken());
					expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.AfterTupleComma, MessageID.None, permitTupleDesignation: true);
					node = _syntaxFactory.Argument(nameColon, null, expressionSyntax);
				}
				else
				{
					node = _syntaxFactory.Argument(null, null, expressionSyntax);
				}
				builder.Add(node);
			}
			if (builder.Count < 2)
			{
				builder.AddSeparator(SyntaxFactory.MissingToken(SyntaxKind.CommaToken));
				IdentifierNameSyntax expression = AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TupleTooFewElements);
				builder.Add(_syntaxFactory.Argument(null, null, expression));
			}
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			TupleExpressionSyntax node2 = _syntaxFactory.TupleExpression(openParen, builder, closeParenToken);
			return CheckFeatureAvailability(node2, MessageID.IDS_FeatureTuples);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private bool ScanCast()
	{
		if (CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		EatToken();
		ScanTypeFlags scanTypeFlags = ScanType();
		if (scanTypeFlags == ScanTypeFlags.NotType)
		{
			return false;
		}
		if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return false;
		}
		if (scanTypeFlags == ScanTypeFlags.PointerOrMultiplication || scanTypeFlags == ScanTypeFlags.NullableType || scanTypeFlags == ScanTypeFlags.MustBeType || scanTypeFlags == ScanTypeFlags.AliasQualifiedName)
		{
			return true;
		}
		EatToken();
		if (scanTypeFlags == ScanTypeFlags.GenericTypeOrMethod || scanTypeFlags == ScanTypeFlags.GenericTypeOrExpression || scanTypeFlags == ScanTypeFlags.NonGenericTypeOrExpression || scanTypeFlags == ScanTypeFlags.TupleType)
		{
			return CanFollowCast(CurrentToken.Kind);
		}
		return false;
	}

	private bool ScanAsyncLambda(Precedence precedence)
	{
		if (precedence > Precedence.Assignment)
		{
			return false;
		}
		if (CurrentToken.ContextualKind != SyntaxKind.AsyncKeyword)
		{
			return false;
		}
		if (PeekToken(1).Kind == SyntaxKind.IdentifierToken && PeekToken(2).Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			return true;
		}
		if (PeekToken(1).Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		ResetPoint state = GetResetPoint();
		EatToken(SyntaxKind.IdentifierToken);
		bool result = ScanParenthesizedImplicitlyTypedLambda(precedence) || ScanExplicitlyTypedLambda(precedence);
		Reset(ref state);
		Release(ref state);
		return result;
	}

	private static bool CanFollowCast(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PercentToken:
		case SyntaxKind.CaretToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.EqualsToken:
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.ColonToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.CommaToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.SlashToken:
		case SyntaxKind.BarBarToken:
		case SyntaxKind.AmpersandAmpersandToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
		case SyntaxKind.QuestionQuestionToken:
		case SyntaxKind.MinusGreaterThanToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.LessThanLessThanToken:
		case SyntaxKind.LessThanLessThanEqualsToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.GreaterThanGreaterThanEqualsToken:
		case SyntaxKind.SlashEqualsToken:
		case SyntaxKind.AsteriskEqualsToken:
		case SyntaxKind.BarEqualsToken:
		case SyntaxKind.AmpersandEqualsToken:
		case SyntaxKind.PlusEqualsToken:
		case SyntaxKind.MinusEqualsToken:
		case SyntaxKind.CaretEqualsToken:
		case SyntaxKind.PercentEqualsToken:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.EndOfFileToken:
			return false;
		default:
			return true;
		}
	}

	private ExpressionSyntax ParseNewExpression()
	{
		if (IsAnonymousType())
		{
			return ParseAnonymousTypeExpression();
		}
		if (IsImplicitlyTypedArray())
		{
			return ParseImplicitlyTypedArrayCreation();
		}
		return ParseArrayOrObjectCreationExpression();
	}

	private bool IsAnonymousType()
	{
		if (CurrentToken.Kind == SyntaxKind.NewKeyword)
		{
			return PeekToken(1).Kind == SyntaxKind.OpenBraceToken;
		}
		return false;
	}

	private AnonymousObjectCreationExpressionSyntax ParseAnonymousTypeExpression()
	{
		SyntaxToken node = EatToken(SyntaxKind.NewKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureAnonymousTypes);
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		SeparatedSyntaxListBuilder<AnonymousObjectMemberDeclaratorSyntax> list = _pool.AllocateSeparated<AnonymousObjectMemberDeclaratorSyntax>();
		ParseAnonymousTypeMemberInitializers(ref openBrace, ref list);
		SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		AnonymousObjectCreationExpressionSyntax result = _syntaxFactory.AnonymousObjectCreationExpression(node, openBrace, list, closeBraceToken);
		_pool.Free(in list);
		return result;
	}

	private void ParseAnonymousTypeMemberInitializers(ref SyntaxToken openBrace, ref SeparatedSyntaxListBuilder<AnonymousObjectMemberDeclaratorSyntax> list)
	{
		if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return;
		}
		while (true)
		{
			if (IsPossibleExpression() || CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				list.Add(ParseAnonymousTypeMemberInitializer());
				while (true)
				{
					if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
					{
						return;
					}
					if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleExpression())
					{
						list.AddSeparator(EatToken(SyntaxKind.CommaToken));
						if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
						{
							return;
						}
						if (!IsPossibleExpression())
						{
							break;
						}
						list.Add(ParseAnonymousTypeMemberInitializer());
					}
					else if (SkipBadInitializerListTokens(ref openBrace, list, SyntaxKind.CommaToken) == PostSkipAction.Abort)
					{
						return;
					}
				}
			}
			else if (SkipBadInitializerListTokens(ref openBrace, list, SyntaxKind.IdentifierToken) != PostSkipAction.Continue)
			{
				break;
			}
		}
	}

	private AnonymousObjectMemberDeclaratorSyntax ParseAnonymousTypeMemberInitializer()
	{
		NameEqualsSyntax nameEquals = (IsNamedAssignment() ? ParseNameEquals() : null);
		ExpressionSyntax expression = ParseExpressionCore();
		return _syntaxFactory.AnonymousObjectMemberDeclarator(nameEquals, expression);
	}

	private bool IsInitializerMember()
	{
		if (!IsComplexElementInitializer() && !IsNamedAssignment() && !IsDictionaryInitializer())
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private bool IsComplexElementInitializer()
	{
		return CurrentToken.Kind == SyntaxKind.OpenBraceToken;
	}

	private bool IsNamedAssignment()
	{
		if (IsTrueIdentifier())
		{
			return PeekToken(1).Kind == SyntaxKind.EqualsToken;
		}
		return false;
	}

	private bool IsDictionaryInitializer()
	{
		return CurrentToken.Kind == SyntaxKind.OpenBracketToken;
	}

	private ExpressionSyntax ParseArrayOrObjectCreationExpression()
	{
		SyntaxToken newKeyword = EatToken(SyntaxKind.NewKeyword);
		bool flag = IsPossibleArrayCreationExpression();
		TypeSyntax typeSyntax = ParseType(flag ? ParseTypeMode.ArrayCreation : ParseTypeMode.Normal, flag);
		if (typeSyntax.Kind == SyntaxKind.ArrayType)
		{
			InitializerExpressionSyntax initializer = null;
			if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
			{
				initializer = ParseArrayInitializer();
			}
			else
			{
				ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = ((ArrayTypeSyntax)typeSyntax).RankSpecifiers[0];
				if (GetNumberOfNonOmittedArraySizes(arrayRankSpecifierSyntax) == 0)
				{
					typeSyntax = AddError(typeSyntax, arrayRankSpecifierSyntax, ErrorCode.ERR_MissingArraySize);
				}
			}
			return _syntaxFactory.ArrayCreationExpression(newKeyword, (ArrayTypeSyntax)typeSyntax, initializer);
		}
		ArgumentListSyntax argumentListSyntax = null;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			argumentListSyntax = ParseParenthesizedArgumentList();
		}
		InitializerExpressionSyntax initializerExpressionSyntax = null;
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			initializerExpressionSyntax = ParseObjectOrCollectionInitializer();
		}
		if (argumentListSyntax == null && initializerExpressionSyntax == null)
		{
			argumentListSyntax = _syntaxFactory.ArgumentList(EatToken(SyntaxKind.OpenParenToken, ErrorCode.ERR_BadNewExpr), default, SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken));
		}
		return _syntaxFactory.ObjectCreationExpression(newKeyword, typeSyntax, argumentListSyntax, initializerExpressionSyntax);
	}

	private static int GetNumberOfNonOmittedArraySizes(ArrayRankSpecifierSyntax rankSpec)
	{
		int count = rankSpec.Sizes.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (rankSpec.Sizes[i].Kind != SyntaxKind.OmittedArraySizeExpression)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsPossibleArrayCreationExpression()
	{
		ResetPoint state = GetResetPoint();
		try
		{
			return ScanNonArrayType() != ScanTypeFlags.NotType && CurrentToken.Kind == SyntaxKind.OpenBracketToken;
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private InitializerExpressionSyntax ParseObjectOrCollectionInitializer()
	{
		SyntaxToken startToken = EatToken(SyntaxKind.OpenBraceToken);
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
		try
		{
			ParseObjectOrCollectionInitializerMembers(ref startToken, builder, out var isObjectInitializer);
			startToken = CheckFeatureAvailability(startToken, isObjectInitializer ? MessageID.IDS_FeatureObjectInitializer : MessageID.IDS_FeatureCollectionInitializer);
			SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return _syntaxFactory.InitializerExpression(isObjectInitializer ? SyntaxKind.ObjectInitializerExpression : SyntaxKind.CollectionInitializerExpression, startToken, builder, closeBraceToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private void ParseObjectOrCollectionInitializerMembers(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ExpressionSyntax> list, out bool isObjectInitializer)
	{
		isObjectInitializer = true;
		if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return;
		}
		do
		{
			if (!IsInitializerMember() && CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				continue;
			}
			isObjectInitializer = false;
			list.Add(ParseObjectOrCollectionInitializerMember(ref isObjectInitializer));
			while (CurrentToken.Kind != SyntaxKind.CloseBraceToken)
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken || IsInitializerMember())
				{
					list.AddSeparator(EatToken(SyntaxKind.CommaToken));
					if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
					{
						break;
					}
					list.Add(ParseObjectOrCollectionInitializerMember(ref isObjectInitializer));
				}
				else if (SkipBadInitializerListTokens(ref startToken, list, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			break;
		}
		while (SkipBadInitializerListTokens(ref startToken, list, SyntaxKind.IdentifierToken) == PostSkipAction.Continue);
	}

	private ExpressionSyntax ParseObjectOrCollectionInitializerMember(ref bool isObjectInitializer)
	{
		if (IsComplexElementInitializer())
		{
			return ParseComplexElementInitializer();
		}
		if (IsDictionaryInitializer())
		{
			isObjectInitializer = true;
			ExpressionSyntax node = ParseDictionaryInitializer();
			return CheckFeatureAvailability(node, MessageID.IDS_FeatureDictionaryInitializer);
		}
		if (IsNamedAssignment())
		{
			isObjectInitializer = true;
			return ParseObjectInitializerNamedAssignment();
		}
		return ParseExpressionCore();
	}

	private PostSkipAction SkipBadInitializerListTokens<T>(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : CSharpSyntaxNode
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.IsTerminator(), expected);
	}

	private ExpressionSyntax ParseObjectInitializerNamedAssignment()
	{
		IdentifierNameSyntax left = ParseIdentifierName();
		SyntaxToken operatorToken = EatToken(SyntaxKind.EqualsToken);
		ExpressionSyntax right = ((CurrentToken.Kind != SyntaxKind.OpenBraceToken) ? ParseExpressionCore() : ParseObjectOrCollectionInitializer());
		return _syntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, operatorToken, right);
	}

	private ExpressionSyntax ParseDictionaryInitializer()
	{
		BracketedArgumentListSyntax argumentList = ParseBracketedArgumentList();
		SyntaxToken operatorToken = EatToken(SyntaxKind.EqualsToken);
		ExpressionSyntax right = ((CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseObjectOrCollectionInitializer() : ParseExpressionCore());
		ImplicitElementAccessSyntax left = _syntaxFactory.ImplicitElementAccess(argumentList);
		return _syntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, operatorToken, right);
	}

	private InitializerExpressionSyntax ParseComplexElementInitializer()
	{
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
		try
		{
			ParseExpressionsForComplexElementInitializer(ref openBrace, builder, out var closeBraceError);
			SyntaxToken syntaxToken = EatToken(SyntaxKind.CloseBraceToken);
			if (closeBraceError != null)
			{
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, closeBraceError);
			}
			return _syntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, openBrace, builder, syntaxToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private void ParseExpressionsForComplexElementInitializer(ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<ExpressionSyntax> list, out DiagnosticInfo closeBraceError)
	{
		closeBraceError = null;
		if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return;
		}
		do
		{
			if (!IsPossibleExpression() && CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				continue;
			}
			list.Add(ParseExpressionCore());
			while (CurrentToken.Kind != SyntaxKind.CloseBraceToken)
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleExpression())
				{
					list.AddSeparator(EatToken(SyntaxKind.CommaToken));
					if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
					{
						closeBraceError = SyntaxParser.MakeError(CurrentToken, ErrorCode.ERR_ExpressionExpected);
						break;
					}
					list.Add(ParseExpressionCore());
				}
				else if (SkipBadInitializerListTokens(ref openBrace, list, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			break;
		}
		while (SkipBadInitializerListTokens(ref openBrace, list, SyntaxKind.IdentifierToken) == PostSkipAction.Continue);
	}

	private bool IsImplicitlyTypedArray()
	{
		return PeekToken(1).Kind == SyntaxKind.OpenBracketToken;
	}

	private ImplicitArrayCreationExpressionSyntax ParseImplicitlyTypedArrayCreation()
	{
		SyntaxToken node = EatToken(SyntaxKind.NewKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureImplicitArray);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenBracketToken);
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		try
		{
			int lastTokenPosition = -1;
			while (IsMakingProgress(ref lastTokenPosition))
			{
				if (IsPossibleExpression())
				{
					ExpressionSyntax skippedSyntax = AddError(ParseExpressionCore(), ErrorCode.ERR_InvalidArray);
					if (syntaxListBuilder.Count == 0)
					{
						syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax);
					}
					else
					{
						AddTrailingSkippedSyntax(syntaxListBuilder, skippedSyntax);
					}
				}
				if (CurrentToken.Kind != SyntaxKind.CommaToken)
				{
					break;
				}
				syntaxListBuilder.Add(EatToken());
			}
			SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			InitializerExpressionSyntax initializer = ParseArrayInitializer();
			return _syntaxFactory.ImplicitArrayCreationExpression(node, syntaxToken, syntaxListBuilder.ToList(), closeBracketToken, initializer);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private InitializerExpressionSyntax ParseArrayInitializer()
	{
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
		try
		{
			if (CurrentToken.Kind != SyntaxKind.CloseBraceToken)
			{
				do
				{
					IL_002d:
					if (!IsPossibleVariableInitializer() && CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						continue;
					}
					builder.Add(ParseVariableInitializer());
					while (CurrentToken.Kind != SyntaxKind.CloseBraceToken)
					{
						if (IsPossibleVariableInitializer() || CurrentToken.Kind == SyntaxKind.CommaToken)
						{
							builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
							if (CurrentToken.Kind == SyntaxKind.CloseBraceToken)
							{
								break;
							}
							if (!IsPossibleVariableInitializer())
							{
								goto IL_002d;
							}
							builder.Add(ParseVariableInitializer());
						}
						else if (SkipBadArrayInitializerTokens(ref openBrace, builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
						{
							break;
						}
					}
					break;
				}
				while (SkipBadArrayInitializerTokens(ref openBrace, builder, SyntaxKind.CommaToken) == PostSkipAction.Continue);
			}
			SyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return _syntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, openBrace, builder, closeBraceToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private PostSkipAction SkipBadArrayInitializerTokens(ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBrace, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleVariableInitializer(), (LanguageParser p) => CurrentToken.Kind == SyntaxKind.CloseBraceToken || IsTerminator(), expected);
	}

	private ExpressionSyntax ParseStackAllocExpression()
	{
		if (IsImplicitlyTypedArray())
		{
			return ParseImplicitlyTypedStackAllocExpression();
		}
		return ParseRegularStackAllocExpression();
	}

	private ExpressionSyntax ParseImplicitlyTypedStackAllocExpression()
	{
		SyntaxToken node = EatToken(SyntaxKind.StackAllocKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureStackAllocInitializer);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenBracketToken);
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition))
		{
			if (IsPossibleExpression())
			{
				ExpressionSyntax skippedSyntax = AddError(ParseExpressionCore(), ErrorCode.ERR_InvalidStackAllocArray);
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax);
			}
			if (CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				break;
			}
			SyntaxToken skippedSyntax2 = AddError(EatToken(), ErrorCode.ERR_InvalidStackAllocArray);
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax2);
		}
		SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
		InitializerExpressionSyntax initializer = ParseArrayInitializer();
		return _syntaxFactory.ImplicitStackAllocArrayCreationExpression(node, syntaxToken, closeBracketToken, initializer);
	}

	private ExpressionSyntax ParseRegularStackAllocExpression()
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.StackAllocKeyword);
		TypeSyntax typeSyntax = ParseType(ParseTypeMode.Normal, expectSizes: true);
		InitializerExpressionSyntax initializer = null;
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureStackAllocInitializer);
			initializer = ParseArrayInitializer();
		}
		else if (typeSyntax.Kind == SyntaxKind.ArrayType)
		{
			ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = ((ArrayTypeSyntax)typeSyntax).RankSpecifiers[0];
			if (GetNumberOfNonOmittedArraySizes(arrayRankSpecifierSyntax) == 0)
			{
				typeSyntax = AddError(typeSyntax, arrayRankSpecifierSyntax, ErrorCode.ERR_MissingArraySize);
			}
		}
		return _syntaxFactory.StackAllocArrayCreationExpression(syntaxToken, typeSyntax, initializer);
	}

	private AnonymousMethodExpressionSyntax ParseAnonymousMethodExpression()
	{
		bool isInAsync = IsInAsync;
		IsInAsync = false;
		SyntaxToken asyncKeyword = null;
		if (CurrentToken.ContextualKind == SyntaxKind.AsyncKeyword)
		{
			asyncKeyword = EatContextualToken(SyntaxKind.AsyncKeyword);
			asyncKeyword = CheckFeatureAvailability(asyncKeyword, MessageID.IDS_FeatureAsync);
			IsInAsync = true;
		}
		SyntaxToken node = EatToken(SyntaxKind.DelegateKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureAnonDelegates);
		ParameterListSyntax parameterList = null;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			parameterList = ParseParenthesizedParameterList();
		}
		if (CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			SyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			return _syntaxFactory.AnonymousMethodExpression(asyncKeyword, node, parameterList, _syntaxFactory.Block(openBraceToken, default, SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken)));
		}
		BlockSyntax body = ParseBlock();
		IsInAsync = isInAsync;
		return _syntaxFactory.AnonymousMethodExpression(asyncKeyword, node, parameterList, body);
	}

	private LambdaExpressionSyntax ParseLambdaExpression()
	{
		bool isInAsync = IsInAsync;
		SyntaxToken asyncToken = null;
		if (CurrentToken.ContextualKind == SyntaxKind.AsyncKeyword && PeekToken(1).Kind != SyntaxKind.EqualsGreaterThanToken)
		{
			asyncToken = EatContextualToken(SyntaxKind.AsyncKeyword);
			asyncToken = CheckFeatureAvailability(asyncToken, MessageID.IDS_FeatureAsync);
			IsInAsync = true;
		}
		LambdaExpressionSyntax result = ParseLambdaExpression(asyncToken);
		IsInAsync = isInAsync;
		return result;
	}

	private LambdaExpressionSyntax ParseLambdaExpression(SyntaxToken asyncToken)
	{
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			ParameterListSyntax parameterList = ParseLambdaParameterList();
			SyntaxToken node = EatToken(SyntaxKind.EqualsGreaterThanToken);
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureLambda);
			CSharpSyntaxNode body = ParseLambdaBody();
			return _syntaxFactory.ParenthesizedLambdaExpression(asyncToken, parameterList, node, body);
		}
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken node2 = EatToken(SyntaxKind.EqualsGreaterThanToken);
		node2 = CheckFeatureAvailability(node2, MessageID.IDS_FeatureLambda);
		ParameterSyntax parameter = _syntaxFactory.Parameter(default, default, null, identifier, null);
		CSharpSyntaxNode body2 = ParseLambdaBody();
		return _syntaxFactory.SimpleLambdaExpression(asyncToken, parameter, node2, body2);
	}

	private CSharpSyntaxNode ParseLambdaBody()
	{
		if (CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return ParseBlock();
		}
		return ParsePossibleRefExpression();
	}

	private ParameterListSyntax ParseLambdaParameterList()
	{
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfParameterList;
		SeparatedSyntaxListBuilder<ParameterSyntax> builder = _pool.AllocateSeparated<ParameterSyntax>();
		try
		{
			if (CurrentToken.Kind != SyntaxKind.CloseParenToken)
			{
				do
				{
					if (CurrentToken.Kind != SyntaxKind.CommaToken && !IsPossibleLambdaParameter())
					{
						continue;
					}
					ParameterSyntax node = ParseLambdaParameter();
					builder.Add(node);
					int lastTokenPosition = -1;
					while (IsMakingProgress(ref lastTokenPosition) && CurrentToken.Kind != SyntaxKind.CloseParenToken)
					{
						if (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleLambdaParameter())
						{
							builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
							node = ParseLambdaParameter();
							builder.Add(node);
						}
						else if (SkipBadLambdaParameterListTokens(ref openParen, builder, SyntaxKind.CommaToken, SyntaxKind.CloseParenToken) == PostSkipAction.Abort)
						{
							break;
						}
					}
					break;
				}
				while (SkipBadLambdaParameterListTokens(ref openParen, builder, SyntaxKind.IdentifierToken, SyntaxKind.CloseParenToken) == PostSkipAction.Continue);
			}
			_termState = termState;
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			return _syntaxFactory.ParameterList(openParen, builder, closeParenToken);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private bool IsPossibleLambdaParameter()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.OutKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.ParamsKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		default:
			return IsPredefinedType(CurrentToken.Kind);
		}
	}

	private PostSkipAction SkipBadLambdaParameterListTokens(ref SyntaxToken openParen, SeparatedSyntaxListBuilder<ParameterSyntax> list, SyntaxKind expected, SyntaxKind closeKind)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleLambdaParameter(), (LanguageParser p) => p.CurrentToken.Kind == closeKind || p.IsTerminator(), expected);
	}

	private ParameterSyntax ParseLambdaParameter()
	{
		bool flag = IsParameterModifier(CurrentToken.Kind);
		TypeSyntax type = null;
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		if (ShouldParseLambdaParameterType(flag))
		{
			if (flag)
			{
				ParseParameterModifiers(syntaxListBuilder);
			}
			type = ParseType(ParseTypeMode.Parameter);
		}
		SyntaxToken identifier = ParseIdentifierToken();
		ParameterSyntax result = _syntaxFactory.Parameter(default, syntaxListBuilder.ToList(), type, identifier, null);
		_pool.Free(syntaxListBuilder);
		return result;
	}

	private bool ShouldParseLambdaParameterType(bool hasModifier)
	{
		if (hasModifier)
		{
			return true;
		}
		if (IsPredefinedType(CurrentToken.Kind))
		{
			return true;
		}
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return true;
		}
		if (IsTrueIdentifier(CurrentToken))
		{
			SyntaxToken syntaxToken = PeekToken(1);
			if (syntaxToken.Kind != SyntaxKind.CommaToken && syntaxToken.Kind != SyntaxKind.CloseParenToken && syntaxToken.Kind != SyntaxKind.EqualsGreaterThanToken && syntaxToken.Kind != SyntaxKind.OpenBraceToken)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsTokenQueryContextualKeyword(SyntaxToken token)
	{
		if (IsTokenStartOfNewQueryClause(token))
		{
			return true;
		}
		SyntaxKind contextualKind = token.ContextualKind;
		if (contextualKind == SyntaxKind.ByKeyword || contextualKind - 8430 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenStartOfNewQueryClause(SyntaxToken token)
	{
		SyntaxKind contextualKind = token.ContextualKind;
		if (contextualKind - 8421 <= (SyntaxKind)5 || contextualKind - 8428 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private bool IsQueryExpression(bool mayBeVariableDeclaration, bool mayBeMemberDeclaration)
	{
		if (CurrentToken.ContextualKind == SyntaxKind.FromKeyword)
		{
			return IsQueryExpressionAfterFrom(mayBeVariableDeclaration, mayBeMemberDeclaration);
		}
		return false;
	}

	private bool IsQueryExpressionAfterFrom(bool mayBeVariableDeclaration, bool mayBeMemberDeclaration)
	{
		SyntaxKind kind = PeekToken(1).Kind;
		if (IsPredefinedType(kind))
		{
			return true;
		}
		if (kind == SyntaxKind.IdentifierToken)
		{
			SyntaxKind kind2 = PeekToken(2).Kind;
			if (kind2 == SyntaxKind.InKeyword)
			{
				return true;
			}
			if (mayBeVariableDeclaration && (kind2 == SyntaxKind.SemicolonToken || kind2 == SyntaxKind.CommaToken || kind2 == SyntaxKind.EqualsToken))
			{
				return false;
			}
			if (!mayBeMemberDeclaration)
			{
				return true;
			}
			if (kind2 == SyntaxKind.OpenParenToken || kind2 == SyntaxKind.OpenBraceToken)
			{
				return false;
			}
		}
		ResetPoint state = GetResetPoint();
		try
		{
			EatToken();
			if (ScanType() != ScanTypeFlags.NotType && (CurrentToken.Kind == SyntaxKind.IdentifierToken || CurrentToken.Kind == SyntaxKind.InKeyword))
			{
				return true;
			}
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
		return false;
	}

	private QueryExpressionSyntax ParseQueryExpression(Precedence precedence)
	{
		EnterQuery();
		FromClauseSyntax node = ParseFromClause();
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureQueryExpression);
		if (precedence > Precedence.Assignment && IsStrict)
		{
			node = AddError(node, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(SyntaxKind.FromKeyword));
		}
		QueryBodySyntax body = ParseQueryBody();
		LeaveQuery();
		return _syntaxFactory.QueryExpression(node, body);
	}

	private QueryBodySyntax ParseQueryBody()
	{
		SyntaxListBuilder<QueryClauseSyntax> syntaxListBuilder = _pool.Allocate<QueryClauseSyntax>();
		try
		{
			QueryContinuationSyntax continuation = null;
			while (true)
			{
				switch (CurrentToken.ContextualKind)
				{
				case SyntaxKind.FromKeyword:
				{
					FromClauseSyntax node = ParseFromClause();
					syntaxListBuilder.Add(node);
					continue;
				}
				case SyntaxKind.JoinKeyword:
					syntaxListBuilder.Add(ParseJoinClause());
					continue;
				case SyntaxKind.LetKeyword:
					syntaxListBuilder.Add(ParseLetClause());
					continue;
				case SyntaxKind.WhereKeyword:
					syntaxListBuilder.Add(ParseWhereClause());
					continue;
				case SyntaxKind.OrderByKeyword:
					syntaxListBuilder.Add(ParseOrderByClause());
					continue;
				}
				SelectOrGroupClauseSyntax selectOrGroup = CurrentToken.ContextualKind switch
				{
					SyntaxKind.SelectKeyword => (SelectOrGroupClauseSyntax)ParseSelectClause(), 
					SyntaxKind.GroupKeyword => ParseGroupClause(), 
					_ => _syntaxFactory.SelectClause(EatToken(SyntaxKind.SelectKeyword, ErrorCode.ERR_ExpectedSelectOrGroup), CreateMissingIdentifierName()), 
				};
				if (CurrentToken.ContextualKind == SyntaxKind.IntoKeyword)
				{
					continuation = ParseQueryContinuation();
				}
				return _syntaxFactory.QueryBody(syntaxListBuilder, selectOrGroup, continuation);
			}
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private FromClauseSyntax ParseFromClause()
	{
		SyntaxToken node = EatContextualToken(SyntaxKind.FromKeyword);
		node = CheckFeatureAvailability(node, MessageID.IDS_FeatureQueryExpression);
		TypeSyntax type = null;
		if (PeekToken(1).Kind != SyntaxKind.InKeyword)
		{
			type = ParseType();
		}
		SyntaxToken syntaxToken;
		if (PeekToken(1).ContextualKind == SyntaxKind.InKeyword && (CurrentToken.Kind != SyntaxKind.IdentifierToken || SyntaxFacts.IsQueryContextualKeyword(CurrentToken.ContextualKind)))
		{
			syntaxToken = EatToken();
			syntaxToken = WithAdditionalDiagnostics(syntaxToken, GetExpectedTokenError(SyntaxKind.IdentifierToken, syntaxToken.ContextualKind, syntaxToken.GetLeadingTriviaWidth(), syntaxToken.Width));
			syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.IdentifierToken);
		}
		else
		{
			syntaxToken = ParseIdentifierToken();
		}
		SyntaxToken inKeyword = EatToken(SyntaxKind.InKeyword);
		ExpressionSyntax expression = ParseExpressionCore();
		return _syntaxFactory.FromClause(node, type, syntaxToken, inKeyword, expression);
	}

	private JoinClauseSyntax ParseJoinClause()
	{
		SyntaxToken joinKeyword = EatContextualToken(SyntaxKind.JoinKeyword);
		TypeSyntax type = null;
		if (PeekToken(1).Kind != SyntaxKind.InKeyword)
		{
			type = ParseType();
		}
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken inKeyword = EatToken(SyntaxKind.InKeyword);
		ExpressionSyntax inExpression = ParseExpressionCore();
		SyntaxToken onKeyword = EatContextualToken(SyntaxKind.OnKeyword, ErrorCode.ERR_ExpectedContextualKeywordOn);
		ExpressionSyntax leftExpression = ParseExpressionCore();
		SyntaxToken equalsKeyword = EatContextualToken(SyntaxKind.EqualsKeyword, ErrorCode.ERR_ExpectedContextualKeywordEquals);
		ExpressionSyntax rightExpression = ParseExpressionCore();
		JoinIntoClauseSyntax joinIntoClauseSyntax = null;
		if (CurrentToken.ContextualKind == SyntaxKind.IntoKeyword)
		{
			SyntaxToken intoKeyword = SyntaxParser.ConvertToKeyword(EatToken());
			SyntaxToken identifier2 = ParseIdentifierToken();
			joinIntoClauseSyntax = _syntaxFactory.JoinIntoClause(intoKeyword, identifier2);
		}
		return _syntaxFactory.JoinClause(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, joinIntoClauseSyntax);
	}

	private LetClauseSyntax ParseLetClause()
	{
		SyntaxToken letKeyword = EatContextualToken(SyntaxKind.LetKeyword);
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		ExpressionSyntax expression = ParseExpressionCore();
		return _syntaxFactory.LetClause(letKeyword, identifier, equalsToken, expression);
	}

	private WhereClauseSyntax ParseWhereClause()
	{
		SyntaxToken whereKeyword = EatContextualToken(SyntaxKind.WhereKeyword);
		ExpressionSyntax condition = ParseExpressionCore();
		return _syntaxFactory.WhereClause(whereKeyword, condition);
	}

	private OrderByClauseSyntax ParseOrderByClause()
	{
		SyntaxToken orderByKeyword = EatContextualToken(SyntaxKind.OrderByKeyword);
		SeparatedSyntaxListBuilder<OrderingSyntax> builder = _pool.AllocateSeparated<OrderingSyntax>();
		try
		{
			builder.Add(ParseOrdering());
			while (CurrentToken.Kind == SyntaxKind.CommaToken && CurrentToken.Kind != SyntaxKind.CloseParenToken && CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				if (CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
					builder.Add(ParseOrdering());
				}
				else if (SkipBadOrderingListTokens(builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
			return _syntaxFactory.OrderByClause(orderByKeyword, builder);
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private PostSkipAction SkipBadOrderingListTokens(SeparatedSyntaxListBuilder<OrderingSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsCurrentTokenQueryContextualKeyword || p.IsTerminator(), expected);
	}

	private OrderingSyntax ParseOrdering()
	{
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken syntaxToken = null;
		SyntaxKind kind = SyntaxKind.AscendingOrdering;
		if (CurrentToken.ContextualKind == SyntaxKind.AscendingKeyword || CurrentToken.ContextualKind == SyntaxKind.DescendingKeyword)
		{
			syntaxToken = SyntaxParser.ConvertToKeyword(EatToken());
			if (syntaxToken.Kind == SyntaxKind.DescendingKeyword)
			{
				kind = SyntaxKind.DescendingOrdering;
			}
		}
		return _syntaxFactory.Ordering(kind, expression, syntaxToken);
	}

	private SelectClauseSyntax ParseSelectClause()
	{
		SyntaxToken selectKeyword = EatContextualToken(SyntaxKind.SelectKeyword);
		ExpressionSyntax expression = ParseExpressionCore();
		return _syntaxFactory.SelectClause(selectKeyword, expression);
	}

	private GroupClauseSyntax ParseGroupClause()
	{
		SyntaxToken groupKeyword = EatContextualToken(SyntaxKind.GroupKeyword);
		ExpressionSyntax groupExpression = ParseExpressionCore();
		SyntaxToken byKeyword = EatContextualToken(SyntaxKind.ByKeyword, ErrorCode.ERR_ExpectedContextualKeywordBy);
		ExpressionSyntax byExpression = ParseExpressionCore();
		return _syntaxFactory.GroupClause(groupKeyword, groupExpression, byKeyword, byExpression);
	}

	private QueryContinuationSyntax ParseQueryContinuation()
	{
		SyntaxToken intoKeyword = EatContextualToken(SyntaxKind.IntoKeyword);
		SyntaxToken identifier = ParseIdentifierToken();
		QueryBodySyntax body = ParseQueryBody();
		return _syntaxFactory.QueryContinuation(intoKeyword, identifier, body);
	}

	internal static bool MatchesFactoryContext(GreenNode green, SyntaxFactoryContext context)
	{
		if (context.IsInAsync == green.ParsedInAsync)
		{
			return context.IsInQuery == green.ParsedInQuery;
		}
		return false;
	}

	private void EnterQuery()
	{
		_syntaxFactoryContext.QueryDepth++;
	}

	private void LeaveQuery()
	{
		_syntaxFactoryContext.QueryDepth--;
	}

	private new ResetPoint GetResetPoint()
	{
		return new ResetPoint(base.GetResetPoint(), _termState, _isInTry, _syntaxFactoryContext.IsInAsync, _syntaxFactoryContext.QueryDepth);
	}

	private void Reset(ref ResetPoint state)
	{
		_termState = state.TerminatorState;
		_isInTry = state.IsInTry;
		_syntaxFactoryContext.IsInAsync = state.IsInAsync;
		_syntaxFactoryContext.QueryDepth = state.QueryDepth;
		Reset(ref state.BaseResetPoint);
	}

	private void Release(ref ResetPoint state)
	{
		Release(ref state.BaseResetPoint);
	}

	internal TNode ConsumeUnexpectedTokens<TNode>(TNode node) where TNode : CSharpSyntaxNode
	{
		if (CurrentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return node;
		}
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
		while (CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			syntaxListBuilder.Add(EatToken());
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> syntaxList = syntaxListBuilder.ToList();
		_pool.Free(syntaxListBuilder);
		node = AddError(node, ErrorCode.ERR_UnexpectedToken, syntaxList[0].ToString());
		node = AddTrailingSkippedSyntax(node, syntaxList.Node);
		return node;
	}

	private string Substring(string s, int first, int last)
	{
		if (last >= s.Length)
		{
			last = s.Length - 1;
		}
		int num = last - first + 1;
		if (last <= s.Length && num > 0)
		{
			return s.Substring(first, num);
		}
		return string.Empty;
	}

	private ExpressionSyntax ParseInterpolatedStringToken()
	{
		SyntaxToken syntaxToken = EatToken();
		string valueText = syntaxToken.ValueText;
		bool flag = valueText.Length > 2 && valueText[1] == '@';
		ArrayBuilder<Lexer.Interpolation> instance = ArrayBuilder<Lexer.Interpolation>.GetInstance();
		SyntaxDiagnosticInfo error = null;
		bool closeQuoteMissing;
		using (Lexer lexer = new Lexer(SourceText.From(valueText), Options, allowPreprocessorDirectives: false))
		{
			Lexer.TokenInfo info = default;
			lexer.ScanInterpolatedStringLiteralTop(instance, flag, ref info, ref error, out closeQuoteMissing);
		}
		int num = ((!flag) ? 1 : 2);
		SyntaxToken stringStartToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), flag ? SyntaxKind.InterpolatedVerbatimStringStartToken : SyntaxKind.InterpolatedStringStartToken, null);
		int num2 = (closeQuoteMissing ? valueText.Length : (valueText.Length - 1));
		SyntaxToken stringEndToken = (closeQuoteMissing ? SyntaxFactory.MissingToken(SyntaxKind.InterpolatedStringEndToken).TokenWithTrailingTrivia(syntaxToken.GetTrailingTrivia()) : SyntaxFactory.Token(null, SyntaxKind.InterpolatedStringEndToken, syntaxToken.GetTrailingTrivia()));
		SyntaxListBuilder<InterpolatedStringContentSyntax> syntaxListBuilder = _pool.Allocate<InterpolatedStringContentSyntax>();
		if (instance.Count == 0)
		{
			string text = Substring(valueText, num + 1, num2 - 1);
			if (text.Length > 0)
			{
				SyntaxToken textToken = MakeStringToken(text, text, flag, SyntaxKind.InterpolatedStringTextToken);
				syntaxListBuilder.Add(SyntaxFactory.InterpolatedStringText(textToken));
			}
		}
		else
		{
			for (int i = 0; i < instance.Count; i++)
			{
				Lexer.Interpolation interpolation = instance[i];
				string text2 = Substring(valueText, (i == 0) ? (num + 1) : (instance[i - 1].CloseBracePosition + 1), interpolation.OpenBracePosition - 1);
				if (text2.Length > 0)
				{
					SyntaxToken textToken2 = MakeStringToken(text2, text2, flag, SyntaxKind.InterpolatedStringTextToken);
					syntaxListBuilder.Add(SyntaxFactory.InterpolatedStringText(textToken2));
				}
				InterpolationSyntax node = ParseInterpolation(valueText, interpolation, flag);
				syntaxListBuilder.Add(node);
			}
			string text3 = Substring(valueText, instance[instance.Count - 1].CloseBracePosition + 1, num2 - 1);
			if (text3.Length > 0)
			{
				SyntaxToken textToken3 = MakeStringToken(text3, text3, flag, SyntaxKind.InterpolatedStringTextToken);
				syntaxListBuilder.Add(SyntaxFactory.InterpolatedStringText(textToken3));
			}
		}
		instance.Free();
		InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax = SyntaxFactory.InterpolatedStringExpression(stringStartToken, syntaxListBuilder, stringEndToken);
		_pool.Free(syntaxListBuilder);
		if (error != null)
		{
			InterpolatedStringExpressionSyntax node2 = interpolatedStringExpressionSyntax;
			DiagnosticInfo[] diagnostics = new SyntaxDiagnosticInfo[1] { error };
			interpolatedStringExpressionSyntax = node2.WithDiagnosticsGreen(diagnostics);
		}
		return CheckFeatureAvailability(interpolatedStringExpressionSyntax, MessageID.IDS_FeatureInterpolatedStrings);
	}

	private InterpolationSyntax ParseInterpolation(string text, Lexer.Interpolation interpolation, bool isVerbatim)
	{
		InterpolationAlignmentClauseSyntax alignmentClause = null;
		InterpolationFormatClauseSyntax formatClause = null;
		SyntaxToken syntaxToken = (interpolation.CloseBraceMissing ? SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken) : SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
		SyntaxToken openBraceToken;
		ExpressionSyntax expr;
		using (Lexer lexer = new Lexer(SourceText.From(Substring(text, interpolation.OpenBracePosition, interpolation.HasColon ? (interpolation.ColonPosition - 1) : (interpolation.CloseBracePosition - 1))), Options, allowPreprocessorDirectives: false, interpolation.HasColon))
		{
			using LanguageParser languageParser = new LanguageParser(lexer, null, null);
			SyntaxToken commaToken = null;
			ExpressionSyntax alignmentExpression = null;
			languageParser.ParseInterpolationStart(out openBraceToken, out expr, out commaToken, out alignmentExpression);
			if (alignmentExpression != null)
			{
				alignmentClause = SyntaxFactory.InterpolationAlignmentClause(commaToken, alignmentExpression);
			}
			GreenNode leadingTrivia = languageParser.CurrentToken.GetLeadingTrivia();
			if (interpolation.HasColon)
			{
				SyntaxToken colonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).TokenWithLeadingTrivia(leadingTrivia);
				string text2 = Substring(text, interpolation.ColonPosition + 1, interpolation.FormatEndPosition);
				SyntaxToken formatStringToken = MakeStringToken(text2, text2, isVerbatim, SyntaxKind.InterpolatedStringTextToken);
				formatClause = SyntaxFactory.InterpolationFormatClause(colonToken, formatStringToken);
			}
			else
			{
				syntaxToken = syntaxToken.TokenWithLeadingTrivia(leadingTrivia);
			}
		}
		return SyntaxFactory.Interpolation(openBraceToken, expr, alignmentClause, formatClause, syntaxToken);
	}

	private SyntaxToken MakeStringToken(string text, string bodyText, bool isVerbatim, SyntaxKind kind)
	{
		string text2 = (isVerbatim ? "@\"" : "\"");
		using Lexer lexer = new Lexer(SourceText.From(text2 + bodyText + "\""), Options, allowPreprocessorDirectives: false);
		LexerMode mode = LexerMode.Syntax;
		SyntaxToken syntaxToken = lexer.Lex(ref mode);
		SyntaxToken syntaxToken2 = SyntaxFactory.Literal(null, text, kind, syntaxToken.ValueText, null);
		if (syntaxToken.ContainsDiagnostics)
		{
			syntaxToken2 = syntaxToken2.WithDiagnosticsGreen(MoveDiagnostics(syntaxToken.GetDiagnostics(), -text2.Length));
		}
		return syntaxToken2;
	}

	private static DiagnosticInfo[] MoveDiagnostics(DiagnosticInfo[] infos, int offset)
	{
		ArrayBuilder<DiagnosticInfo> instance = ArrayBuilder<DiagnosticInfo>.GetInstance();
		foreach (DiagnosticInfo diagnosticInfo in infos)
		{
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = diagnosticInfo as SyntaxDiagnosticInfo;
			instance.Add(syntaxDiagnosticInfo?.WithOffset(syntaxDiagnosticInfo.Offset + offset) ?? diagnosticInfo);
		}
		return instance.ToArrayAndFree();
	}

	private void ParseInterpolationStart(out SyntaxToken openBraceToken, out ExpressionSyntax expr, out SyntaxToken commaToken, out ExpressionSyntax alignmentExpression)
	{
		openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		expr = ParseExpressionCore();
		if (CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			commaToken = EatToken(SyntaxKind.CommaToken);
			alignmentExpression = ConsumeUnexpectedTokens(ParseExpressionCore());
		}
		else
		{
			commaToken = null;
			alignmentExpression = null;
			expr = ConsumeUnexpectedTokens(expr);
		}
	}

	private CSharpSyntaxNode ParseTypeOrPatternForIsOperator()
	{
		SyntaxKind kind = CurrentToken.Kind;
		Precedence precedence = GetPrecedence(SyntaxKind.IsPatternExpression);
		switch (kind)
		{
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.CommaToken:
			return ParseType(ParseTypeMode.AfterIs);
		default:
			if (SyntaxFacts.IsPredefinedType(kind) || (kind == SyntaxKind.IdentifierToken && (CurrentToken.ContextualKind != SyntaxKind.NameOfKeyword || PeekToken(1).Kind != SyntaxKind.OpenParenToken)))
			{
				ResetPoint state = GetResetPoint();
				try
				{
					TypeSyntax typeSyntax = ParseType(ParseTypeMode.AfterIs);
					if (!typeSyntax.IsMissing && IsTrueIdentifier())
					{
						VariableDesignationSyntax designation = ParseSimpleDesignation();
						return _syntaxFactory.DeclarationPattern(typeSyntax, designation);
					}
					kind = CurrentToken.ContextualKind;
					if ((!IsExpectedBinaryOperator(kind) || GetPrecedence(SyntaxFacts.GetBinaryExpression(kind)) <= precedence) && kind != SyntaxKind.DotToken)
					{
						return typeSyntax;
					}
					Reset(ref state);
				}
				finally
				{
					Release(ref state);
				}
			}
			return _syntaxFactory.ConstantPattern(ParseSubExpressionCore(precedence));
		}
	}

	private ExpressionSyntax ParseExpressionOrDeclaration(ParseTypeMode mode, MessageID feature, bool permitTupleDesignation)
	{
		if (!IsPossibleDeclarationExpression(mode, permitTupleDesignation))
		{
			return ParseSubExpression(Precedence.Expression);
		}
		return ParseDeclarationExpression(mode, feature);
	}

	private bool IsPossibleDeclarationExpression(ParseTypeMode mode, bool permitTupleDesignation)
	{
		if (IsInAsync && CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword)
		{
			return false;
		}
		ResetPoint state = GetResetPoint();
		try
		{
			bool flag = IsVarType();
			if (ScanType(mode, out var lastTokenOfType) == ScanTypeFlags.NotType)
			{
				return false;
			}
			if (!ScanDesignation(permitTupleDesignation && (flag || IsPredefinedType(lastTokenOfType.Kind))))
			{
				return false;
			}
			return mode switch
			{
				ParseTypeMode.FirstElementOfPossibleTupleLiteral => CurrentToken.Kind == SyntaxKind.CommaToken, 
				ParseTypeMode.AfterTupleComma => CurrentToken.Kind == SyntaxKind.CommaToken || CurrentToken.Kind == SyntaxKind.CloseParenToken, 
				_ => true, 
			};
		}
		finally
		{
			Reset(ref state);
			Release(ref state);
		}
	}

	private bool IsVarType()
	{
		if (!CurrentToken.IsVar())
		{
			return false;
		}
		switch (PeekToken(1).Kind)
		{
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.ColonColonToken:
			return false;
		default:
			return true;
		}
	}

	private bool ScanDesignation(bool permitTuple)
	{
		switch (CurrentToken.Kind)
		{
		default:
			return false;
		case SyntaxKind.IdentifierToken:
		{
			bool result2 = IsTrueIdentifier();
			EatToken();
			return result2;
		}
		case SyntaxKind.OpenParenToken:
		{
			if (!permitTuple)
			{
				return false;
			}
			bool result = false;
			while (true)
			{
				EatToken();
				if (!ScanDesignation(permitTuple: true))
				{
					break;
				}
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.CloseParenToken:
					EatToken();
					return result;
				case SyntaxKind.CommaToken:
					break;
				default:
					return false;
				}
				result = true;
			}
			return false;
		}
		}
	}

	private CSharpSyntaxNode ParseExpressionOrPatternForCase()
	{
		SyntaxKind kind = CurrentToken.Kind;
		CSharpSyntaxNode cSharpSyntaxNode = null;
		if ((SyntaxFacts.IsPredefinedType(kind) || kind == SyntaxKind.IdentifierToken) && CurrentToken.ContextualKind != SyntaxKind.NameOfKeyword)
		{
			ResetPoint state = GetResetPoint();
			try
			{
				TypeSyntax typeSyntax = ParseType(ParseTypeMode.AfterCase);
				if (!typeSyntax.IsMissing && IsTrueIdentifier() && CurrentToken.ContextualKind != SyntaxKind.WhenKeyword)
				{
					VariableDesignationSyntax designation = ParseSimpleDesignation();
					cSharpSyntaxNode = _syntaxFactory.DeclarationPattern(typeSyntax, designation);
				}
				if (cSharpSyntaxNode == null)
				{
					Reset(ref state);
					cSharpSyntaxNode = ParseSubExpression(Precedence.Expression);
				}
			}
			finally
			{
				Release(ref state);
			}
		}
		else
		{
			cSharpSyntaxNode = ParseSubExpression(Precedence.Expression);
		}
		return cSharpSyntaxNode;
	}
}
