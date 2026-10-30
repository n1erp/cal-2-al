using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportDataItemSyntax : ReportDataItemElementSyntax
{
	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _dataItemTable;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _elements;

	private SyntaxNode _triggers;

	public SyntaxToken DataItemKeyword => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._dataItemKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public ObjectNameOrIdSyntax DataItemTable => GetRed(ref _dataItemTable, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxList<ReportDataItemElementSyntax> Elements => new SyntaxList<ReportDataItemElementSyntax>(GetRed(ref _elements, 8));

	public SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 9));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalReportDataItemSyntax)Node)._closeBraceToken, GetChildPosition(10), GetChildIndex(10));

	internal ReportDataItemSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _dataItemTable, 4), 
			7 => GetRed(ref _propertyList, 7), 
			8 => GetRed(ref _elements, 8), 
			9 => GetRed(ref _triggers, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			4 => _dataItemTable, 
			7 => _propertyList, 
			8 => _elements, 
			9 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataItem(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportDataItem(this);
	}

	public ReportDataItemSyntax Update(SyntaxToken dataItemKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax dataItemTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ReportDataItemElementSyntax> elements, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (dataItemKeyword != DataItemKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || dataItemTable != DataItemTable || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || elements != Elements || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			ReportDataItemSyntax reportDataItemSyntax = SyntaxFactory.ReportDataItem(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportDataItemSyntax.WithAnnotations(annotations);
			}
			return reportDataItemSyntax;
		}
		return this;
	}

	public ReportDataItemSyntax WithDataItemKeyword(SyntaxToken dataItemKeyword)
	{
		return Update(dataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(DataItemKeyword, openParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, semicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithDataItemTable(ObjectNameOrIdSyntax dataItemTable)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, dataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, closeParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, openBraceToken, PropertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, propertyList, Elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithElements(SyntaxList<ReportDataItemElementSyntax> elements)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, elements, Triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, triggers, CloseBraceToken);
	}

	public ReportDataItemSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(DataItemKeyword, OpenParenthesisToken, Name, SemicolonToken, DataItemTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Elements, Triggers, closeBraceToken);
	}

	public ReportDataItemSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public ReportDataItemSyntax AddElements(params ReportDataItemElementSyntax[] items)
	{
		return WithElements(Elements.AddRange(items));
	}

	public ReportDataItemSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
