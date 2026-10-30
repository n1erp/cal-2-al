using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeclarationTreeBuilder : CSharpSyntaxVisitor<SingleNamespaceOrTypeDeclaration>
{
	private readonly SyntaxTree _syntaxTree;

	private readonly string _scriptClassName;

	private readonly bool _isSubmission;

	private static readonly ObjectPool<ImmutableHashSet<string>.Builder> s_memberNameBuilderPool = new ObjectPool<ImmutableHashSet<string>.Builder>(() => ImmutableHashSet.CreateBuilder<string>());

	private DeclarationTreeBuilder(SyntaxTree syntaxTree, string scriptClassName, bool isSubmission)
	{
		_syntaxTree = syntaxTree;
		_scriptClassName = scriptClassName;
		_isSubmission = isSubmission;
	}

	public static RootSingleNamespaceDeclaration ForTree(SyntaxTree syntaxTree, string scriptClassName, bool isSubmission)
	{
		return (RootSingleNamespaceDeclaration)new DeclarationTreeBuilder(syntaxTree, scriptClassName, isSubmission).Visit(syntaxTree.GetRoot());
	}

	private ImmutableArray<SingleNamespaceOrTypeDeclaration> VisitNamespaceChildren(CSharpSyntaxNode node, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax> internalMembers)
	{
		if (members.Count == 0)
		{
			return ImmutableArray<SingleNamespaceOrTypeDeclaration>.Empty;
		}
		bool flag = false;
		ArrayBuilder<SingleNamespaceOrTypeDeclaration> instance = ArrayBuilder<SingleNamespaceOrTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax item in members)
		{
			SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = Visit(item);
			if (singleNamespaceOrTypeDeclaration != null)
			{
				instance.Add(singleNamespaceOrTypeDeclaration);
			}
			else
			{
				flag = flag || item.Kind() != SyntaxKind.IncompleteMember;
			}
		}
		if (flag)
		{
			SingleTypeDeclaration.TypeDeclarationFlags declFlags = SingleTypeDeclaration.TypeDeclarationFlags.None;
			ImmutableHashSet<string> nonTypeMemberNames = GetNonTypeMemberNames(internalMembers, ref declFlags);
			SyntaxReference reference = _syntaxTree.GetReference(node);
			instance.Add(CreateImplicitClass(nonTypeMemberNames, reference, declFlags));
		}
		return instance.ToImmutableAndFree();
	}

	private static SingleNamespaceOrTypeDeclaration CreateImplicitClass(ImmutableHashSet<string> memberNames, SyntaxReference container, SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		return new SingleTypeDeclaration(DeclarationKind.ImplicitClass, "<invalid-global-code>", 0, DeclarationModifiers.Sealed | DeclarationModifiers.Internal | DeclarationModifiers.Partial, declFlags, container, new SourceLocation(container), memberNames, ImmutableArray<SingleTypeDeclaration>.Empty, ImmutableArray<Diagnostic>.Empty);
	}

	private RootSingleNamespaceDeclaration CreateScriptRootDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members = compilationUnit.Members;
		ArrayBuilder<SingleNamespaceOrTypeDeclaration> instance = ArrayBuilder<SingleNamespaceOrTypeDeclaration>.GetInstance();
		ArrayBuilder<SingleTypeDeclaration> instance2 = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax item in members)
		{
			SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = Visit(item);
			if (singleNamespaceOrTypeDeclaration != null)
			{
				if (singleNamespaceOrTypeDeclaration.Kind == DeclarationKind.Namespace)
				{
					instance.Add(singleNamespaceOrTypeDeclaration);
				}
				else
				{
					instance2.Add((SingleTypeDeclaration)singleNamespaceOrTypeDeclaration);
				}
			}
		}
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = SingleTypeDeclaration.TypeDeclarationFlags.None;
		ImmutableHashSet<string> nonTypeMemberNames = GetNonTypeMemberNames(((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)compilationUnit.Green).Members, ref declFlags);
		instance.Add(CreateScriptClass(compilationUnit, instance2.ToImmutableAndFree(), nonTypeMemberNames, declFlags));
		return new RootSingleNamespaceDeclaration(compilationUnit.Usings.Any(), compilationUnit.Externs.Any(), _syntaxTree.GetReference(compilationUnit), instance.ToImmutableAndFree(), GetReferenceDirectives(compilationUnit), compilationUnit.AttributeLists.Any());
	}

	private static ImmutableArray<ReferenceDirective> GetReferenceDirectives(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		IList<Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax> referenceDirectives = compilationUnit.GetReferenceDirectives((Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax d) => !d.File.ContainsDiagnostics && !string.IsNullOrEmpty(d.File.ValueText));
		if (referenceDirectives.Count == 0)
		{
			return ImmutableArray<ReferenceDirective>.Empty;
		}
		ArrayBuilder<ReferenceDirective> instance = ArrayBuilder<ReferenceDirective>.GetInstance(referenceDirectives.Count);
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax item in referenceDirectives)
		{
			instance.Add(new ReferenceDirective(item.File.ValueText, new SourceLocation(item)));
		}
		return instance.ToImmutableAndFree();
	}

	private SingleNamespaceOrTypeDeclaration CreateScriptClass(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax parent, ImmutableArray<SingleTypeDeclaration> children, ImmutableHashSet<string> memberNames, SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		SyntaxReference reference = _syntaxTree.GetReference(parent);
		string[] array = _scriptClassName.Split(new char[1] { '.' });
		SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = new SingleTypeDeclaration(_isSubmission ? DeclarationKind.Submission : DeclarationKind.Script, array.Last(), 0, DeclarationModifiers.Sealed | DeclarationModifiers.Internal | DeclarationModifiers.Partial, declFlags, reference, new SourceLocation(reference), memberNames, children, ImmutableArray<Diagnostic>.Empty);
		for (int num = array.Length - 2; num >= 0; num--)
		{
			singleNamespaceOrTypeDeclaration = SingleNamespaceDeclaration.Create(array[num], hasUsings: false, hasExternAliases: false, reference, new SourceLocation(reference), ImmutableArray.Create(singleNamespaceOrTypeDeclaration), ImmutableArray<Diagnostic>.Empty);
		}
		return singleNamespaceOrTypeDeclaration;
	}

	public override SingleNamespaceOrTypeDeclaration VisitCompilationUnit(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		if (_syntaxTree.Options.Kind != SourceCodeKind.Regular)
		{
			return CreateScriptRootDeclaration(compilationUnit);
		}
		ImmutableArray<SingleNamespaceOrTypeDeclaration> children = VisitNamespaceChildren(compilationUnit, compilationUnit.Members, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)compilationUnit.Green).Members);
		return new RootSingleNamespaceDeclaration(compilationUnit.Usings.Any(), compilationUnit.Externs.Any(), _syntaxTree.GetReference(compilationUnit), children, ImmutableArray<ReferenceDirective>.Empty, compilationUnit.AttributeLists.Any());
	}

	public override SingleNamespaceOrTypeDeclaration VisitNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax node)
	{
		ImmutableArray<SingleNamespaceOrTypeDeclaration> children = VisitNamespaceChildren(node, node.Members, node.Green.Members);
		bool hasUsings = node.Usings.Any();
		bool hasExternAliases = node.Externs.Any();
		Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax nameSyntax = node.Name;
		CSharpSyntaxNode node2 = node;
		while (nameSyntax is Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax)
		{
			SingleNamespaceDeclaration singleNamespaceDeclaration = SingleNamespaceDeclaration.Create(qualifiedNameSyntax.Right.Identifier.ValueText, hasUsings, hasExternAliases, _syntaxTree.GetReference(node2), new SourceLocation(qualifiedNameSyntax.Right), children, ImmutableArray<Diagnostic>.Empty);
			SingleNamespaceOrTypeDeclaration[] items = new SingleNamespaceDeclaration[1] { singleNamespaceDeclaration };
			children = items.AsImmutableOrNull();
			node2 = (nameSyntax = qualifiedNameSyntax.Left);
			hasUsings = false;
			hasExternAliases = false;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (ContainsGeneric(node.Name))
		{
			instance.Add(ErrorCode.ERR_UnexpectedGenericName, node.Name.GetLocation());
		}
		if (ContainsAlias(node.Name))
		{
			instance.Add(ErrorCode.ERR_UnexpectedAliasedName, node.Name.GetLocation());
		}
		return SingleNamespaceDeclaration.Create(nameSyntax.GetUnqualifiedName().Identifier.ValueText, hasUsings, hasExternAliases, _syntaxTree.GetReference(node2), new SourceLocation(nameSyntax), children, instance.ToReadOnlyAndFree());
	}

	private static bool ContainsAlias(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return name.Kind() switch
		{
			SyntaxKind.GenericName => false, 
			SyntaxKind.AliasQualifiedName => true, 
			SyntaxKind.QualifiedName => ContainsAlias(((Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)name).Left), 
			_ => false, 
		};
	}

	private static bool ContainsGeneric(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		switch (name.Kind())
		{
		case SyntaxKind.GenericName:
			return true;
		case SyntaxKind.AliasQualifiedName:
			return ContainsGeneric(((Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)name).Name);
		case SyntaxKind.QualifiedName:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)name;
			if (!ContainsGeneric(qualifiedNameSyntax.Left))
			{
				return ContainsGeneric(qualifiedNameSyntax.Right);
			}
			return true;
		}
		default:
			return false;
		}
	}

	public override SingleNamespaceOrTypeDeclaration VisitClassDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Class);
	}

	public override SingleNamespaceOrTypeDeclaration VisitStructDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Struct);
	}

	public override SingleNamespaceOrTypeDeclaration VisitInterfaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Interface);
	}

	private SingleNamespaceOrTypeDeclaration VisitTypeDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax node, DeclarationKind kind)
	{
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		if (node.BaseList != null)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasBaseDeclarations;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (node.Arity == 0)
		{
			Symbol.ReportErrorIfHasConstraints(node.ConstraintClauses, instance);
		}
		ImmutableHashSet<string> nonTypeMemberNames = GetNonTypeMemberNames(((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeDeclarationSyntax)node.Green).Members, ref declFlags);
		DeclarationModifiers declarationModifiers = node.Modifiers.ToDeclarationModifiers(instance);
		DeclarationKind kind2 = kind;
		string valueText = node.Identifier.ValueText;
		DeclarationModifiers modifiers = declarationModifiers;
		return new SingleTypeDeclaration(kind2, valueText, node.Arity, modifiers, declFlags, _syntaxTree.GetReference(node), new SourceLocation(node.Identifier), nonTypeMemberNames, VisitTypeChildren(node), instance.ToReadOnlyAndFree());
	}

	private ImmutableArray<SingleTypeDeclaration> VisitTypeChildren(Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax node)
	{
		if (node.Members.Count == 0)
		{
			return ImmutableArray<SingleTypeDeclaration>.Empty;
		}
		ArrayBuilder<SingleTypeDeclaration> instance = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax member in node.Members)
		{
			if (Visit(member) is SingleTypeDeclaration item)
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override SingleNamespaceOrTypeDeclaration VisitDelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax node)
	{
		SingleTypeDeclaration.TypeDeclarationFlags typeDeclarationFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (node.Arity == 0)
		{
			Symbol.ReportErrorIfHasConstraints(node.ConstraintClauses, instance);
		}
		typeDeclarationFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		DeclarationModifiers declarationModifiers = node.Modifiers.ToDeclarationModifiers(instance);
		string valueText = node.Identifier.ValueText;
		DeclarationModifiers modifiers = declarationModifiers;
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = typeDeclarationFlags;
		return new SingleTypeDeclaration(DeclarationKind.Delegate, valueText, node.Arity, modifiers, declFlags, _syntaxTree.GetReference(node), new SourceLocation(node.Identifier), ImmutableHashSet<string>.Empty, ImmutableArray<SingleTypeDeclaration>.Empty, instance.ToReadOnlyAndFree());
	}

	public override SingleNamespaceOrTypeDeclaration VisitEnumDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax node)
	{
		SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> members = node.Members;
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		if (node.BaseList != null)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasBaseDeclarations;
		}
		ImmutableHashSet<string> enumMemberNames = GetEnumMemberNames(members, ref declFlags);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		DeclarationModifiers modifiers = node.Modifiers.ToDeclarationModifiers(instance);
		return new SingleTypeDeclaration(DeclarationKind.Enum, node.Identifier.ValueText, 0, modifiers, declFlags, _syntaxTree.GetReference(node), new SourceLocation(node.Identifier), enumMemberNames, ImmutableArray<SingleTypeDeclaration>.Empty, instance.ToReadOnlyAndFree());
	}

	private static ImmutableHashSet<string> ToImmutableAndFree(ImmutableHashSet<string>.Builder builder)
	{
		ImmutableHashSet<string> result = builder.ToImmutable();
		builder.Clear();
		s_memberNameBuilderPool.Free(builder);
		return result;
	}

	private static ImmutableHashSet<string> GetEnumMemberNames(SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> members, ref SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		int count = members.Count;
		ImmutableHashSet<string>.Builder builder = s_memberNameBuilderPool.Allocate();
		if (count != 0)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		}
		bool flag = false;
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax item in members)
		{
			builder.Add(item.Identifier.ValueText);
			if (!flag && item.AttributeLists.Any())
			{
				flag = true;
			}
		}
		if (flag)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasAttributes;
		}
		return ToImmutableAndFree(builder);
	}

	private static ImmutableHashSet<string> GetNonTypeMemberNames(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax> members, ref SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		bool flag = false;
		bool flag2 = false;
		bool anyNonTypeMembers = false;
		ImmutableHashSet<string>.Builder builder = s_memberNameBuilderPool.Allocate();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax item in members)
		{
			AddNonTypeMemberNames(item, builder, ref anyNonTypeMembers);
			if (!flag && CheckMethodMemberForExtensionSyntax(item))
			{
				flag = true;
			}
			if (!flag2 && CheckMemberForAttributes(item))
			{
				flag2 = true;
			}
		}
		if (flag)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasExtensionMethodSyntax;
		}
		if (flag2)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasAttributes;
		}
		if (anyNonTypeMembers)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		}
		return ToImmutableAndFree(builder);
	}

	private static bool CheckMethodMemberForExtensionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member)
	{
		if (member.Kind == SyntaxKind.MethodDeclaration)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax parameterList = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)member).parameterList;
			if (parameterList != null)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax> parameters = parameterList.Parameters;
				if (parameters.Count != 0)
				{
					foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken modifier in parameters[0].Modifiers)
					{
						if (modifier.Kind == SyntaxKind.ThisKeyword)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private static bool CheckMemberForAttributes(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member)
	{
		switch (member.Kind)
		{
		case SyntaxKind.CompilationUnit:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)member).AttributeLists.Any();
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.EnumDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseTypeDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.DelegateDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DelegateDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseFieldDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseMethodDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BasePropertyDeclarationSyntax)member;
			bool flag = basePropertyDeclarationSyntax.AttributeLists.Any();
			if (!flag && basePropertyDeclarationSyntax.AccessorList != null)
			{
				foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax accessor in basePropertyDeclarationSyntax.AccessorList.Accessors)
				{
					flag |= accessor.AttributeLists.Any();
				}
			}
			return flag;
		}
		default:
			return false;
		}
	}

	private static void AddNonTypeMemberNames(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member, ImmutableHashSet<string>.Builder set, ref bool anyNonTypeMembers)
	{
		switch (member.Kind)
		{
		case SyntaxKind.FieldDeclaration:
		{
			anyNonTypeMembers = true;
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclaratorSyntax> variables = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)member).Declaration.Variables;
			int count = variables.Count;
			for (int i = 0; i < count; i++)
			{
				set.Add(variables[i].Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.EventFieldDeclaration:
		{
			anyNonTypeMembers = true;
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclaratorSyntax> variables2 = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventFieldDeclarationSyntax)member).Declaration.Variables;
			int count2 = variables2.Count;
			for (int j = 0; j < count2; j++)
			{
				set.Add(variables2[j].Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.MethodDeclaration:
		{
			anyNonTypeMembers = true;
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax methodDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)member;
			if (methodDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(methodDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.PropertyDeclaration:
		{
			anyNonTypeMembers = true;
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax propertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax)member;
			if (propertyDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(propertyDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.EventDeclaration:
		{
			anyNonTypeMembers = true;
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax eventDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax)member;
			if (eventDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(eventDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.ConstructorDeclaration:
			anyNonTypeMembers = true;
			set.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorDeclarationSyntax)member).Modifiers.Any(8347) ? ".cctor" : ".ctor");
			break;
		case SyntaxKind.DestructorDeclaration:
			anyNonTypeMembers = true;
			set.Add("Finalize");
			break;
		case SyntaxKind.IndexerDeclaration:
			anyNonTypeMembers = true;
			set.Add("this[]");
			break;
		case SyntaxKind.OperatorDeclaration:
		{
			anyNonTypeMembers = true;
			string item = OperatorFacts.OperatorNameFromDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)member);
			set.Add(item);
			break;
		}
		case SyntaxKind.ConversionOperatorDeclaration:
			anyNonTypeMembers = true;
			set.Add((((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)member).ImplicitOrExplicitKeyword.Kind == SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit");
			break;
		case SyntaxKind.GlobalStatement:
			anyNonTypeMembers = true;
			break;
		}
	}
}
