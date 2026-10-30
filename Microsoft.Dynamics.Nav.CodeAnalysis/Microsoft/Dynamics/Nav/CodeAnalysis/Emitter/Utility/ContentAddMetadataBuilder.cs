using System;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal abstract class ContentAddMetadataBuilder<T, TU, TV> : ContentDestinationMetadataBuilder<ContentAddMetadataBuilder<T, TU, TV>, TU> where T : ContentAddMetadataBuilder<T, TU, TV> where TU : Symbol, ISymbolWithId where TV : Symbol, ISymbolWithId
{
	private TV Target { get; set; }

	private TU Anchor { get; set; }

	private ChangeKind AddChangeKind { get; set; }

	private Action<XmlWriter, TV> ContentWriteAction { get; set; }

	protected ContentAddMetadataBuilder(TU anchorSymbol, TV target)
		: base(new ChangeKindMapping(ChangeKind.AddFirst, ChangeKind.AddLast, ChangeKind.AddBefore, ChangeKind.AddAfter))
	{
		DebugAssertHelper.Assert(target != null);
		DebugAssertHelper.Assert(target.Kind == SymbolKind.View || anchorSymbol != null);
		Anchor = anchorSymbol;
		Target = target;
	}

	public T ForAddType(ChangeKind addChangeKind)
	{
		if ((uint)(addChangeKind - 1) <= 3u)
		{
			AddChangeKind = addChangeKind;
			return this as T;
		}
		throw ExceptionUtilities.UnexpectedValue(addChangeKind);
	}

	public T WithContentWriteAction(Action<XmlWriter, TV> contentWriteAction)
	{
		ContentWriteAction = contentWriteAction;
		return this as T;
	}

	protected void SetContentMetadata(ref ContentAddMetadata metadata)
	{
		ContentDestinationMetadata metadata2 = metadata;
		base.SetContentMetadata(ref metadata2, AddChangeKind, Anchor);
		metadata.SemanticKind = GetSemanticKind(Anchor);
		metadata.ContentWriteAction = (XmlWriter xmlWriter) =>
		{
			ContentWriteAction(xmlWriter, Target);
		};
	}

	public ContentAddMetadata Build()
	{
		ContentAddMetadata metadata = new ContentAddMetadata();
		SetContentMetadata(ref metadata);
		return metadata;
	}
}
