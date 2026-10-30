namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class CSharpSyntaxVisitor<TResult>
{
	public virtual TResult VisitIdentifierName(IdentifierNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedName(QualifiedNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGenericName(GenericNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeArgumentList(TypeArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPredefinedType(PredefinedTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArrayType(ArrayTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPointerType(PointerTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitNullableType(NullableTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTupleType(TupleTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTupleElement(TupleElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOmittedTypeArgument(OmittedTypeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRefType(RefTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTupleExpression(TupleExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAwaitExpression(AwaitExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElementBindingExpression(ElementBindingExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAssignmentExpression(AssignmentExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConditionalExpression(ConditionalExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitThisExpression(ThisExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBaseExpression(BaseExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMakeRefExpression(MakeRefExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRefTypeExpression(RefTypeExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRefValueExpression(RefValueExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCheckedExpression(CheckedExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDefaultExpression(DefaultExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeOfExpression(TypeOfExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSizeOfExpression(SizeOfExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArgumentList(ArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArgument(ArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitNameColon(NameColonSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDeclarationExpression(DeclarationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCastExpression(CastExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRefExpression(RefExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInitializerExpression(InitializerExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryExpression(QueryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryBody(QueryBodySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFromClause(FromClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLetClause(LetClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitJoinClause(JoinClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitJoinIntoClause(JoinIntoClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhereClause(WhereClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrderByClause(OrderByClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrdering(OrderingSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSelectClause(SelectClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGroupClause(GroupClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryContinuation(QueryContinuationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIsPatternExpression(IsPatternExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitThrowExpression(ThrowExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhenClause(WhenClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDeclarationPattern(DeclarationPatternSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstantPattern(ConstantPatternSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterpolation(InterpolationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGlobalStatement(GlobalStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBlock(BlockSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDiscardDesignation(DiscardDesignationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEmptyStatement(EmptyStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabeledStatement(LabeledStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGotoStatement(GotoStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBreakStatement(BreakStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitContinueStatement(ContinueStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReturnStatement(ReturnStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitThrowStatement(ThrowStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitYieldStatement(YieldStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhileStatement(WhileStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDoStatement(DoStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForStatement(ForStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForEachStatement(ForEachStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUsingStatement(UsingStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFixedStatement(FixedStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCheckedStatement(CheckedStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUnsafeStatement(UnsafeStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLockStatement(LockStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfStatement(IfStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElseClause(ElseClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSwitchStatement(SwitchStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSwitchSection(SwitchSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTryStatement(TryStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCatchClause(CatchClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCatchDeclaration(CatchDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCatchFilterClause(CatchFilterClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFinallyClause(FinallyClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCompilationUnit(CompilationUnitSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUsingDirective(UsingDirectiveSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeList(AttributeListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeTargetSpecifier(AttributeTargetSpecifierSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttribute(AttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeArgument(AttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitNameEquals(NameEqualsSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeParameterList(TypeParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeParameter(TypeParameterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStructDeclaration(StructDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumDeclaration(EnumDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDelegateDeclaration(DelegateDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBaseList(BaseListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleBaseType(SimpleBaseTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstructorConstraint(ConstructorConstraintSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitClassOrStructConstraint(ClassOrStructConstraintSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeConstraint(TypeConstraintSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldDeclaration(FieldDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstructorInitializer(ConstructorInitializerSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDestructorDeclaration(DestructorDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventDeclaration(EventDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIndexerDeclaration(IndexerDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAccessorList(AccessorListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAccessorDeclaration(AccessorDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameterList(ParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedParameterList(BracketedParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameter(ParameterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIncompleteMember(IncompleteMemberSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTypeCref(TypeCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedCref(QualifiedCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitNameMemberCref(NameMemberCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOperatorMemberCref(OperatorMemberCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCrefParameterList(CrefParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCrefParameter(CrefParameterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlElement(XmlElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlElementStartTag(XmlElementStartTagSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlElementEndTag(XmlElementEndTagSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlEmptyElement(XmlEmptyElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlName(XmlNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPrefix(XmlPrefixSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlTextAttribute(XmlTextAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlNameAttribute(XmlNameAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlText(XmlTextSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlCDataSection(XmlCDataSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlComment(XmlCommentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLoadDirectiveTrivia(LoadDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitShebangDirectiveTrivia(ShebangDirectiveTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult Visit(CSharpSyntaxNode node)
	{
		if (node == null)
		{
			return default;
		}
		return node.Accept(this);
	}

	public virtual TResult VisitToken(SyntaxToken token)
	{
		return DefaultVisit(token);
	}

	public virtual TResult VisitTrivia(SyntaxTrivia trivia)
	{
		return DefaultVisit(trivia);
	}

	protected virtual TResult DefaultVisit(CSharpSyntaxNode node)
	{
		return default;
	}
}
internal abstract class CSharpSyntaxVisitor
{
	public virtual void VisitIdentifierName(IdentifierNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedName(QualifiedNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGenericName(GenericNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeArgumentList(TypeArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPredefinedType(PredefinedTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArrayType(ArrayTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPointerType(PointerTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitNullableType(NullableTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTupleType(TupleTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTupleElement(TupleElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOmittedTypeArgument(OmittedTypeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRefType(RefTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTupleExpression(TupleExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAwaitExpression(AwaitExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElementBindingExpression(ElementBindingExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAssignmentExpression(AssignmentExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConditionalExpression(ConditionalExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitThisExpression(ThisExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBaseExpression(BaseExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMakeRefExpression(MakeRefExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRefTypeExpression(RefTypeExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRefValueExpression(RefValueExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCheckedExpression(CheckedExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDefaultExpression(DefaultExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeOfExpression(TypeOfExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSizeOfExpression(SizeOfExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArgumentList(ArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArgument(ArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitNameColon(NameColonSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDeclarationExpression(DeclarationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCastExpression(CastExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRefExpression(RefExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInitializerExpression(InitializerExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryExpression(QueryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryBody(QueryBodySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFromClause(FromClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLetClause(LetClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitJoinClause(JoinClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitJoinIntoClause(JoinIntoClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhereClause(WhereClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrderByClause(OrderByClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrdering(OrderingSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSelectClause(SelectClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGroupClause(GroupClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryContinuation(QueryContinuationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIsPatternExpression(IsPatternExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitThrowExpression(ThrowExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhenClause(WhenClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDeclarationPattern(DeclarationPatternSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstantPattern(ConstantPatternSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterpolation(InterpolationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGlobalStatement(GlobalStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBlock(BlockSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDiscardDesignation(DiscardDesignationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEmptyStatement(EmptyStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabeledStatement(LabeledStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGotoStatement(GotoStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBreakStatement(BreakStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitContinueStatement(ContinueStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReturnStatement(ReturnStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitThrowStatement(ThrowStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitYieldStatement(YieldStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhileStatement(WhileStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDoStatement(DoStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForStatement(ForStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForEachStatement(ForEachStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUsingStatement(UsingStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFixedStatement(FixedStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCheckedStatement(CheckedStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUnsafeStatement(UnsafeStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLockStatement(LockStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfStatement(IfStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElseClause(ElseClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSwitchStatement(SwitchStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSwitchSection(SwitchSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTryStatement(TryStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCatchClause(CatchClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCatchDeclaration(CatchDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCatchFilterClause(CatchFilterClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFinallyClause(FinallyClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCompilationUnit(CompilationUnitSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUsingDirective(UsingDirectiveSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeList(AttributeListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeTargetSpecifier(AttributeTargetSpecifierSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttribute(AttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeArgument(AttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitNameEquals(NameEqualsSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeParameterList(TypeParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeParameter(TypeParameterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStructDeclaration(StructDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumDeclaration(EnumDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBaseList(BaseListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleBaseType(SimpleBaseTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstructorConstraint(ConstructorConstraintSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitClassOrStructConstraint(ClassOrStructConstraintSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeConstraint(TypeConstraintSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldDeclaration(FieldDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstructorInitializer(ConstructorInitializerSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventDeclaration(EventDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAccessorList(AccessorListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameterList(ParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedParameterList(BracketedParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameter(ParameterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIncompleteMember(IncompleteMemberSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTypeCref(TypeCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedCref(QualifiedCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitNameMemberCref(NameMemberCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOperatorMemberCref(OperatorMemberCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCrefParameterList(CrefParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCrefParameter(CrefParameterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlElement(XmlElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlElementStartTag(XmlElementStartTagSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlElementEndTag(XmlElementEndTagSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlEmptyElement(XmlEmptyElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlName(XmlNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPrefix(XmlPrefixSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlTextAttribute(XmlTextAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlCrefAttribute(XmlCrefAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlNameAttribute(XmlNameAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlText(XmlTextSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlCDataSection(XmlCDataSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlComment(XmlCommentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLoadDirectiveTrivia(LoadDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitShebangDirectiveTrivia(ShebangDirectiveTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void Visit(CSharpSyntaxNode node)
	{
		node?.Accept(this);
	}

	public virtual void VisitToken(SyntaxToken token)
	{
		DefaultVisit(token);
	}

	public virtual void VisitTrivia(SyntaxTrivia trivia)
	{
		DefaultVisit(trivia);
	}

	public virtual void DefaultVisit(CSharpSyntaxNode node)
	{
	}
}
