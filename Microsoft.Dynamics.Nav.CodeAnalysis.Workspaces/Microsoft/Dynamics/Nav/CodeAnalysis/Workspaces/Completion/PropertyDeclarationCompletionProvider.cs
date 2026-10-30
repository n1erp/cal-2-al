using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyDeclarationCompletionProvider : SymbolCompletionProvider
{
	private static readonly ImmutableArray<string> Tags = ImmutableArray.Create("Property");

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		using (Logger.LogBlock(FunctionId.Completion_PropertyDeclarationProvider_ProvideCompletionsAsync, cancellationToken))
		{
			foreach (PropertyTypeInfo item in await RecommendPropertiesAsync(document, memberSyntaxContext as MemberSyntaxContext, context.Options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				context.AddItem(CreateItem(item));
			}
		}
	}

	protected virtual Task<IEnumerable<PropertyTypeInfo>> RecommendPropertiesAsync(Document document, MemberSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!context.General.HasFlag(GeneralContexts.PropertyDeclaration))
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<PropertyTypeInfo>());
		}
		SyntaxNode parent = context.LeftToken.Parent;
		if (parent.Kind == SyntaxKind.PropertyName)
		{
			parent = parent.Parent.Parent.Parent;
		}
		if (parent.Kind == SyntaxKind.Property)
		{
			parent = parent.Parent.Parent;
		}
		IEnumerable<PropertyTypeInfo> allDefinedPropertyNames = parent.GetAllDefinedPropertyNames();
		allDefinedPropertyNames = FilterModifiableProperties(context, parent, allDefinedPropertyNames);
		allDefinedPropertyNames = FilterAlreadyDeclaredProperties(context, parent, allDefinedPropertyNames, cancellationToken);
		return Task.FromResult(allDefinedPropertyNames);
	}

	private static IEnumerable<PropertyTypeInfo> FilterAlreadyDeclaredProperties(MemberSyntaxContext context, SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties, CancellationToken cancellationToken)
	{
		ImmutableHashSet<string> declaredProperties = (from s in context.SemanticModel.LookupSymbols(anchorNode, LookupOptions.OnlyPropertyDeclarations, null, null, cancellationToken)
			select s.Name).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
		return properties.Where((PropertyTypeInfo p) => !declaredProperties.Contains(p.Name));
	}

	private static IEnumerable<PropertyTypeInfo> FilterModifiableProperties(MemberSyntaxContext context, SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties)
	{
		if (anchorNode.IsKind(SyntaxKind.PageCustomizationObject, SyntaxKind.PageExtensionObject, SyntaxKind.TableExtensionObject) || anchorNode.Kind.IsModifyChange())
		{
			bool flag = context.Page.HasFlag(PageContexts.View);
			if (!flag && context.DeclaringObject.IsKind(SyntaxKind.TableExtensionObject, SyntaxKind.PageExtensionObject))
			{
				properties = properties.Where((PropertyTypeInfo p) => p.Modification != ModificationKind.None);
			}
			else if (flag || context.DeclaringObject.IsKind(SyntaxKind.PageCustomizationObject))
			{
				properties = properties.Where((PropertyTypeInfo p) => p.Modification == ModificationKind.Customizations);
			}
		}
		return properties;
	}

	private static CompletionItem CreateItem(PropertyTypeInfo property)
	{
		string text = CreateDisplayParts(property.Name).ToDisplayString();
		string documentation = property.GetDocumentation();
		string name = property.Name;
		string descriptionText = documentation;
		string detailText = text;
		return CommonCompletionItem.Create(name, default, Glyph.Property, descriptionText, detailText, null, null, null, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, isMarkdownDocs: true, null, Tags);
	}

	private static ImmutableArray<SymbolDisplayPart> CreateDisplayParts(string property)
	{
		List<SymbolDisplayPart> list = new List<SymbolDisplayPart>();
		list.AddText(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.Property, property));
		return list.ToImmutableArray();
	}
}
