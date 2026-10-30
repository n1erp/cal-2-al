using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class PageExtensionMetadataEmitter : PageBaseMetadataEmitter<ApplicationObjectExtensionTypeSymbol>
{
	internal PageExtensionMetadataEmitter(ApplicationObjectExtensionTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("MetadataRuntimeDeltas", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
			xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", Symbol.Name);
			if (Symbol.Target != null)
			{
				WritePageExtensionProperties(xmlWriter);
				WriteChangeSymbols(xmlWriter);
				WriteGlobalExpressions(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		});
	}

	private void WritePageExtensionProperties(XmlWriter xmlWriter)
	{
		if (!Symbol.Properties.Any())
		{
			return;
		}
		ImmutableArray<PropertySymbol> immutableArray = Symbol.Properties;
		PropertySymbol captionMLProperty = immutableArray.FirstOrDefault((PropertySymbol p) => p.PropertyKind.IsEitherCaptionProperty());
		if (captionMLProperty != null)
		{
			if (ObjectMetadataEmitHelper.TryGetCaptionMLPropertyValue(captionMLProperty, out var propertyValue))
			{
				xmlWriter.WriteStartElement("PropertyChange");
				xmlWriter.WriteAttributeString("PropertyName", "CaptionML");
				xmlWriter.WriteAttributeString("PropertyValue", propertyValue);
				xmlWriter.WriteEndElement();
			}
			immutableArray = immutableArray.WhereAsArray((PropertySymbol p) => p != captionMLProperty);
		}
		if (immutableArray.Length > 0)
		{
			xmlWriter.WriteStartElement("PagePropertiesChange");
			WriteProperties(xmlWriter, Symbol, immutableArray, shouldOutputDefaultValues: false);
			xmlWriter.WriteEndElement();
		}
	}

	private void WriteChangeSymbols(XmlWriter xmlWriter)
	{
		foreach (Symbol member in Symbol.GetMembers())
		{
			if (member.Kind == SymbolKind.Change)
			{
				HandleChangeSymbols(xmlWriter, (ChangeSymbol)member);
			}
		}
	}

	private void HandleChangeSymbols(XmlWriter xmlWriter, ChangeSymbol changeSymbol)
	{
		DebugAssertHelper.Assert(changeSymbol != null);
		switch (changeSymbol.ChangeKind)
		{
		case ChangeKind.MoveFirst:
		case ChangeKind.MoveLast:
		case ChangeKind.MoveBefore:
		case ChangeKind.MoveAfter:
			WriteContentMove(xmlWriter, (SourceChangeMoveSymbol)changeSymbol);
			break;
		case ChangeKind.AddFirst:
		case ChangeKind.AddLast:
		case ChangeKind.AddBefore:
		case ChangeKind.AddAfter:
			WriteAddSymbol(xmlWriter, (SourceChangeAddSymbol)changeSymbol, WriteControl, WriteAction, WriteViewAction, WriteViewDefinitionsList);
			break;
		case ChangeKind.Modify:
			WriteModifySymbol(xmlWriter, (SourceChangeModifySymbol)changeSymbol, WriteProperties);
			break;
		}
	}

	internal static void WriteAddSymbol(XmlWriter xmlWriter, SourceChangeAddSymbol addedSymbol, Action<XmlWriter, SourceControlSymbol, Dictionary<string, string>> writeControl, Action<XmlWriter, ActionSymbol> writeAction, Action<XmlWriter, ViewSymbol> writeViewAction, Action<XmlWriter, IEnumerable<ViewSymbol>> writeViewDefinitions)
	{
		WriteContentAdd(xmlWriter, addedSymbol, writeControl, writeAction, writeViewAction, writeViewDefinitions);
		HashSet<string> alreadyAddedSourceExpressions = new HashSet<string>();
		WriteControlExpressions(xmlWriter, addedSymbol.AddedSymbols.OfType<SourceControlSymbol>(), alreadyAddedSourceExpressions);
	}

	internal static void WriteMoveSymbol(XmlWriter xmlWriter, SourceChangeMoveSymbol sourceMoveSymbol)
	{
		WriteContentMove(xmlWriter, sourceMoveSymbol);
	}

	internal static void WriteModifySymbol(XmlWriter xmlWriter, SourceChangeModifySymbol modifySymbol, Action<XmlWriter, Symbol, IEnumerable<PropertySymbol>, bool> writeProperties)
	{
		WriteContentModify(xmlWriter, modifySymbol, writeProperties);
		WriteAdditionalRepeaterExpressionsIfAny(xmlWriter, modifySymbol);
	}

	private static void WriteAdditionalRepeaterExpressionsIfAny(XmlWriter xmlWriter, SourceChangeModifySymbol modifySymbol)
	{
		if (modifySymbol.Target is ControlSymbol { ControlKind: ControlKind.Repeater })
		{
			PageMetadataEmitHelper.WriteIndentationColumnExpression(xmlWriter, modifySymbol);
		}
	}

	private static void WriteContentMove(XmlWriter xmlWriter, SourceChangeMoveSymbol sourceMoveSymbol)
	{
		bool flag = sourceMoveSymbol.Anchor.IsKind(SymbolKind.ErrorType);
		if (flag)
		{
			DebugAssertHelper.Assert(sourceMoveSymbol.DeclaringCompilation.Options.Target.IsPersonalizationTarget());
			WriteMissingAnchorSymbol(xmlWriter, sourceMoveSymbol, sourceMoveSymbol.Anchor);
		}
		Symbol symbol = null;
		foreach (Symbol movedSymbol in sourceMoveSymbol.MovedSymbols)
		{
			if (movedSymbol.IsKind(SymbolKind.ErrorType))
			{
				WriteMissingTargetSymbol(xmlWriter, sourceMoveSymbol, movedSymbol);
			}
			else if (!flag)
			{
				ContentMoveMetadata contentMoveMetadata = sourceMoveSymbol.ChangeTargetKind switch
				{
					SymbolKind.Control => new ControlMoveMetadataBuilder(sourceMoveSymbol.Anchor as ControlSymbol, movedSymbol as ControlSymbol).WithPreviousSymbol(symbol as ControlSymbol).ForMoveType(sourceMoveSymbol.ChangeKind).Build(), 
					SymbolKind.Action => new ActionMoveMetadataBuilder(sourceMoveSymbol.Anchor as ActionSymbol, movedSymbol as ActionSymbol).WithPreviousSymbol(symbol as ActionSymbol).ForMoveType(sourceMoveSymbol.ChangeKind).Build(), 
					SymbolKind.View => new ViewActionMoveMetadataBuilder(sourceMoveSymbol.Anchor as ViewSymbol, movedSymbol as ViewSymbol).WithPreviousSymbol(symbol as ViewSymbol).ForMoveType(sourceMoveSymbol.ChangeKind).Build(), 
					_ => throw ExceptionUtilities.UnexpectedValue(sourceMoveSymbol.ChangeTargetKind), 
				};
				DebugAssertHelper.Assert(contentMoveMetadata != null);
				xmlWriter.WriteStartElement(contentMoveMetadata.ContentElementName);
				xmlWriter.WriteAttributeString("TargetID", contentMoveMetadata.TargetId.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("TargetName", contentMoveMetadata.TargetName);
				xmlWriter.WriteAttributeString("TargetType", contentMoveMetadata.TargetType);
				xmlWriter.WriteAttributeString("ParentContainer", contentMoveMetadata.ParentContainer);
				if (contentMoveMetadata.AnchorId.HasValue)
				{
					xmlWriter.WriteAttributeString("AnchorId", contentMoveMetadata.AnchorId.Value.ToString(CultureInfo.InvariantCulture));
				}
				if (!string.IsNullOrWhiteSpace(contentMoveMetadata.AnchorName))
				{
					xmlWriter.WriteAttributeString("AnchorName", contentMoveMetadata.AnchorName);
				}
				xmlWriter.WriteAttributeString("Operation", contentMoveMetadata.Operation.ToString());
				WriteDeltaSemanticKindIfNeeded(xmlWriter, contentMoveMetadata);
				xmlWriter.WriteEndElement();
				symbol = movedSymbol;
			}
		}
	}

	private static void WriteMissingTargetSymbol(XmlWriter xmlWriter, ChangeSymbol sourceMoveSymbol, ISymbol moveSymbol)
	{
		ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)moveSymbol;
		xmlWriter.WriteStartElement("Error");
		xmlWriter.WriteAttributeString("ChangeKind", sourceMoveSymbol.ChangeKind.ToString());
		xmlWriter.WriteAttributeString("TargetName", errorTypeSymbol.Name);
		xmlWriter.WriteAttributeString("ErrorCode", errorTypeSymbol.ErrorInfo.Code.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteEndElement();
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private static void WriteContentAdd(XmlWriter xmlWriter, ChangeAddSymbol addContentSymbol, Action<XmlWriter, SourceControlSymbol, Dictionary<string, string>> writeControl, Action<XmlWriter, ActionSymbol> writeAction, Action<XmlWriter, ViewSymbol> writeViewAction, Action<XmlWriter, IEnumerable<ViewSymbol>> writeViewDefinitions)
	{
		Symbol anchor = addContentSymbol.Anchor;
		Symbol symbol = null;
		Dictionary<string, string> alreadyAddedSourceExpressions = new Dictionary<string, string>();
		PooledList<ViewSymbol> instance = PooledList<ViewSymbol>.GetInstance();
		try
		{
			PageTypeKind pageType = addContentSymbol.GetTargetPage().PageType;
			foreach (Symbol addedSymbol in addContentSymbol.AddedSymbols)
			{
				ContentAddMetadata metadata;
				switch (addedSymbol.Kind)
				{
				case SymbolKind.Action:
					metadata = new ActionAddMetadataBuilder(anchor as ActionSymbol, addedSymbol as SourceActionSymbol).ForAddType(addContentSymbol.ChangeKind).WithPreviousSymbol(symbol as ActionSymbol).WithContentWriteAction(writeAction)
						.Build();
					break;
				case SymbolKind.Control:
					metadata = new ControlAddMetadataBuilder(anchor as ControlSymbol, addedSymbol as SourceControlSymbol).ForAddType(addContentSymbol.ChangeKind).WithPreviousSymbol(symbol as ControlSymbol).WithContentWriteAction((XmlWriter arg, SourceControlSymbol control) =>
					{
						writeControl(arg, control, alreadyAddedSourceExpressions);
					})
						.Build();
					break;
				case SymbolKind.View:
				{
					if (!pageType.IsViewSupported())
					{
						continue;
					}
					SourceViewSymbol sourceViewSymbol = (SourceViewSymbol)addedSymbol;
					metadata = new ViewActionAddMetadataBuilder(anchor as ViewSymbol, sourceViewSymbol).ForAddType(addContentSymbol.ChangeKind).WithPreviousSymbol(symbol as ViewSymbol).WithContentWriteAction((XmlWriter arg, SourceViewSymbol view) =>
					{
						writeViewAction(arg, view);
					})
						.Build();
					instance.Add(sourceViewSymbol);
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(addedSymbol.Kind);
				}
				WriteContentAddElement(xmlWriter, metadata);
				symbol = addedSymbol;
			}
			if (pageType.IsViewSupported())
			{
				writeViewDefinitions(xmlWriter, instance);
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private static void WriteContentAddElement(XmlWriter xmlWriter, ContentAddMetadata metadata)
	{
		xmlWriter.WriteStartElement(metadata.ContentElementName);
		xmlWriter.WriteAttributeString("ParentContainer", metadata.ParentContainer);
		if (metadata.AnchorId.HasValue)
		{
			xmlWriter.WriteAttributeString("AnchorId", metadata.AnchorId.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (!string.IsNullOrWhiteSpace(metadata.AnchorName))
		{
			xmlWriter.WriteAttributeString("AnchorName", metadata.AnchorName);
		}
		WriteDeltaSemanticKindIfNeeded(xmlWriter, metadata);
		xmlWriter.WriteAttributeString("Operation", metadata.Operation.ToString());
		metadata.ContentWriteAction(xmlWriter);
		xmlWriter.WriteEndElement();
	}

	private static void WriteContentModify(XmlWriter xmlWriter, SourceChangeModifySymbol modifySymbol, Action<XmlWriter, Symbol, IEnumerable<PropertySymbol>, bool> writeProperties)
	{
		if (modifySymbol.IsInvalidModifySymbol())
		{
			DebugAssertHelper.Assert(modifySymbol.DeclaringCompilation.Options.Target.IsPersonalizationTarget());
			WriteMissingTargetSymbol(xmlWriter, modifySymbol, modifySymbol.Target);
			return;
		}
		ContentModifyMetadata contentModifyMetadata = modifySymbol.ChangeTargetKind switch
		{
			SymbolKind.Action => new ActionModifyMetadataBuilder().ForTarget(modifySymbol.Target as ActionSymbol).Build(), 
			SymbolKind.Control => new ControlModifyMetadataBuilder().ForTarget(modifySymbol.Target as ControlSymbol).Build(), 
			SymbolKind.View => new ViewActionModifyMetadataBuilder().ForTarget(modifySymbol.Target as ViewSymbol).Build(), 
			_ => throw ExceptionUtilities.UnexpectedValue(modifySymbol.ChangeTargetKind), 
		};
		xmlWriter.WriteStartElement(contentModifyMetadata.ContentElementName);
		xmlWriter.WriteAttributeString("TargetID", contentModifyMetadata.TargetId.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("TargetName", contentModifyMetadata.TargetName);
		xmlWriter.WriteAttributeString("TargetType", contentModifyMetadata.TargetType);
		WriteDeltaSemanticKindIfNeeded(xmlWriter, contentModifyMetadata);
		writeProperties(xmlWriter, modifySymbol, modifySymbol.Properties, arg4: false);
		foreach (SourceTriggerSymbol item in modifySymbol.Triggers.Cast<SourceTriggerSymbol>())
		{
			xmlWriter.WriteAttributeString(item.TriggerInfo.Name, item.MetadataName);
		}
		xmlWriter.WriteEndElement();
	}

	private static void WriteMissingAnchorSymbol(XmlWriter xmlWriter, ChangeMoveSymbol changeSymbol, ISymbol anchorSymbol)
	{
		DebugAssertHelper.Assert(anchorSymbol.IsKind(SymbolKind.ErrorType));
		ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)anchorSymbol;
		xmlWriter.WriteStartElement("Error");
		xmlWriter.WriteAttributeString("ChangeKind", changeSymbol.ChangeKind.ToString());
		xmlWriter.WriteAttributeString("AnchorName", errorTypeSymbol.Name);
		xmlWriter.WriteAttributeString("ErrorCode", errorTypeSymbol.ErrorInfo.Code.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteEndElement();
	}

	private void WriteGlobalExpressions(XmlWriter xmlWriter)
	{
		ImmutableHashSet<Symbol> implicitInDataset = InDataSetVisitor.GetAllImplicitInDatasetVariables(Symbol);
		foreach (VariableSymbol item in Symbol.GlobalVariables.Where((VariableSymbol g) => g.IsInDataSet || implicitInDataset.Contains(g)))
		{
			PageMetadataEmitHelper.WriteVariableInDataSetExpression(xmlWriter, item);
		}
		BoundCodeExpressionPropertyValue boundPropertyValue = Symbol.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.DataCaptionExpression);
		PageBaseMetadataEmitter<PageExtensionTypeSymbol>.WriteDataCaptionExpression(xmlWriter, boundPropertyValue);
	}

	private static void WriteControlExpressions(XmlWriter xmlWriter, IEnumerable<SourceControlSymbol> controls, HashSet<string> alreadyAddedSourceExpressions)
	{
		foreach (SourceControlSymbol control in controls)
		{
			PageMetadataEmitHelper.WriteControlExpressions(xmlWriter, control, alreadyAddedSourceExpressions);
			WriteControlExpressions(xmlWriter, control.Controls.Cast<SourceControlSymbol>(), alreadyAddedSourceExpressions);
		}
	}

	private static void WriteDeltaSemanticKindIfNeeded(XmlWriter xmlWriter, ContentMetadata metadata)
	{
		if (!string.IsNullOrEmpty(metadata.SemanticKind))
		{
			xmlWriter.WriteAttributeString("SemanticKind", metadata.SemanticKind);
		}
	}
}
