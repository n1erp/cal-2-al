using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceMemberFieldSymbolFromDeclarator : SourceMemberFieldSymbol
{
	private readonly bool _hasInitializer;

	private TypeSymbol _lazyType;

	private int _lazyFieldTypeInferred;

	private ImmutableArray<CustomModifier> _lazyCustomModifiers;

	protected sealed override TypeSyntax TypeSyntax => GetFieldDeclaration(VariableDeclaratorNode).Declaration.Type;

	protected sealed override SyntaxTokenList ModifiersTokenList => GetFieldDeclaration(VariableDeclaratorNode).Modifiers;

	public sealed override bool HasInitializer => _hasInitializer;

	protected VariableDeclaratorSyntax VariableDeclaratorNode => (VariableDeclaratorSyntax)SyntaxNode;

	protected override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList
	{
		get
		{
			if (containingType.AnyMemberHasAttributes)
			{
				return GetFieldDeclaration(SyntaxNode).AttributeLists;
			}
			return default;
		}
	}

	internal override bool HasPointerType
	{
		get
		{
			if ((object)_lazyType != null)
			{
				return _lazyType.IsPointerType();
			}
			return IsPointerFieldSyntactically();
		}
	}

	public sealed override ImmutableArray<CustomModifier> CustomModifiers
	{
		get
		{
			if (_lazyCustomModifiers.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyCustomModifiers, base.CustomModifiers, default);
			}
			return _lazyCustomModifiers;
		}
	}

	internal SourceMemberFieldSymbolFromDeclarator(SourceMemberContainerTypeSymbol containingType, VariableDeclaratorSyntax declarator, DeclarationModifiers modifiers, bool modifierErrors, DiagnosticBag diagnostics)
		: base(containingType, modifiers, declarator.Identifier.ValueText, declarator.GetReference(), declarator.Identifier.GetLocation())
	{
		_hasInitializer = declarator.Initializer != null;
		CheckAccessibility(diagnostics);
		if (!modifierErrors)
		{
			ReportModifiersDiagnostics(diagnostics);
		}
	}

	private static BaseFieldDeclarationSyntax GetFieldDeclaration(CSharpSyntaxNode declarator)
	{
		return (BaseFieldDeclarationSyntax)declarator.Parent.Parent;
	}

	private bool IsPointerFieldSyntactically()
	{
		VariableDeclarationSyntax declaration = GetFieldDeclaration(VariableDeclaratorNode).Declaration;
		if (declaration.Type.Kind() == SyntaxKind.PointerType)
		{
			return true;
		}
		foreach (VariableDeclaratorSyntax variable in declaration.Variables)
		{
			BracketedArgumentListSyntax argumentList = variable.ArgumentList;
			if (argumentList != null && argumentList.Arguments.Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	internal sealed override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if ((object)_lazyType != null)
		{
			return _lazyType;
		}
		VariableDeclaratorSyntax variableDeclaratorNode = VariableDeclaratorNode;
		BaseFieldDeclarationSyntax fieldDeclaration = GetFieldDeclaration(variableDeclaratorNode);
		TypeSyntax type = fieldDeclaration.Declaration.Type;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		Symbol associatedSymbol = AssociatedSymbol;
		TypeSymbol typeSymbol;
		if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Event)
		{
			EventSymbol eventSymbol = (EventSymbol)associatedSymbol;
			if (eventSymbol.IsWindowsRuntimeEvent)
			{
				NamedTypeSymbol wellKnownType = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T);
				Binder.ReportUseSiteDiagnostics(wellKnownType, instance2, ErrorLocation);
				typeSymbol = wellKnownType.Construct(eventSymbol.Type);
			}
			else
			{
				typeSymbol = eventSymbol.Type;
			}
		}
		else
		{
			Binder binder = declaringCompilation.GetBinderFactory(SyntaxTree).GetBinder(type);
			binder = binder.WithContainingMemberOrLambda(this);
			if (!ContainingType.IsScriptClass)
			{
				typeSymbol = binder.BindType(type, instance2);
			}
			else
			{
				typeSymbol = binder.BindTypeOrVarKeyword(type, instance, out var isVar);
				if (isVar)
				{
					if (IsConst)
					{
						instance2.Add(ErrorCode.ERR_ImplicitlyTypedVariableCannotBeConst, type.Location);
					}
					if (fieldsBeingBound.ContainsReference(this))
					{
						instance.Add(ErrorCode.ERR_RecursivelyTypedVariable, ErrorLocation, this);
						typeSymbol = null;
					}
					else if (fieldDeclaration.Declaration.Variables.Count > 1)
					{
						instance2.Add(ErrorCode.ERR_ImplicitlyTypedVariableMultipleDeclarator, type.Location);
					}
					else if (IsConst && ContainingType.IsScriptClass)
					{
						typeSymbol = null;
					}
					else
					{
						fieldsBeingBound = new ConsList<FieldSymbol>(this, fieldsBeingBound);
						BoundExpression boundExpression = new ImplicitlyTypedFieldBinder(binder, fieldsBeingBound).BindInferredVariableInitializer(instance, RefKind.None, variableDeclaratorNode.Initializer, variableDeclaratorNode);
						if (boundExpression != null)
						{
							if ((object)boundExpression.Type != null && !boundExpression.Type.IsErrorType())
							{
								typeSymbol = boundExpression.Type;
							}
							_lazyFieldTypeInferred = 1;
						}
					}
					if ((object)typeSymbol == null)
					{
						typeSymbol = binder.CreateErrorType("var");
					}
				}
			}
			if (IsFixed)
			{
				typeSymbol = new PointerTypeSymbol(typeSymbol);
				if (ContainingType.TypeKind != TypeKind.Struct)
				{
					instance.Add(ErrorCode.ERR_FixedNotInStruct, ErrorLocation);
				}
				if (((PointerTypeSymbol)typeSymbol).PointedAtType.FixedBufferElementSizeInBytes() == 0)
				{
					Location location = type.Location;
					instance.Add(ErrorCode.ERR_IllegalFixedType, location);
				}
				if (!binder.InUnsafeRegion)
				{
					instance2.Add(ErrorCode.ERR_UnsafeNeeded, variableDeclaratorNode.Location);
				}
			}
		}
		if ((object)Interlocked.CompareExchange(ref _lazyType, typeSymbol, null) == null)
		{
			TypeChecks(typeSymbol, instance);
			declaringCompilation.DeclarationDiagnostics.AddRange(instance);
			if (fieldDeclaration.Declaration.Variables[0] == variableDeclaratorNode)
			{
				declaringCompilation.DeclarationDiagnostics.AddRange(instance2);
			}
			state.NotePartComplete(CompletionPart.Type);
		}
		instance.Free();
		instance2.Free();
		return _lazyType;
	}

	internal bool FieldTypeInferred(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if (!ContainingType.IsScriptClass)
		{
			return false;
		}
		GetFieldType(fieldsBeingBound);
		if (_lazyFieldTypeInferred == 0)
		{
			return Volatile.Read(in _lazyFieldTypeInferred) != 0;
		}
		return true;
	}

	protected sealed override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics)
	{
		if (!IsConst || VariableDeclaratorNode.Initializer == null)
		{
			return null;
		}
		return ConstantValueUtils.EvaluateFieldConstant(this, VariableDeclaratorNode.Initializer, dependencies, earlyDecodingWellKnownAttributes, diagnostics);
	}

	internal override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (SyntaxTree == tree)
		{
			if (!definedWithinSpan.HasValue)
			{
				return true;
			}
			BaseFieldDeclarationSyntax fieldDeclaration = GetFieldDeclaration(SyntaxNode);
			if (fieldDeclaration.SyntaxTree.HasCompilationUnitRoot)
			{
				return fieldDeclaration.Span.IntersectsWith(definedWithinSpan.Value);
			}
			return false;
		}
		return false;
	}
}
