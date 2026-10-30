using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DocumentationCommentCompiler : CSharpSymbolVisitor
{
	private struct TemporaryStringBuilder(int indentDepth)
	{
		public readonly PooledStringBuilder Pooled = PooledStringBuilder.GetInstance();

		public readonly int InitialIndentDepth = indentDepth;
	}

	private class DocumentationCommentWalker : CSharpSyntaxWalker
	{
		private readonly CSharpCompilation _compilation;

		private readonly DiagnosticBag _diagnostics;

		private readonly Symbol _memberSymbol;

		private readonly TextWriter _writer;

		private readonly ArrayBuilder<CSharpSyntaxNode> _includeElementNodes;

		private HashSet<ParameterSymbol> _documentedParameters;

		private HashSet<TypeParameterSymbol> _documentedTypeParameters;

		private DocumentationCommentWalker(CSharpCompilation compilation, DiagnosticBag diagnostics, Symbol memberSymbol, TextWriter writer, ArrayBuilder<CSharpSyntaxNode> includeElementNodes, HashSet<ParameterSymbol> documentedParameters, HashSet<TypeParameterSymbol> documentedTypeParameters)
			: base(SyntaxWalkerDepth.StructuredTrivia)
		{
			_compilation = compilation;
			_diagnostics = diagnostics;
			_memberSymbol = memberSymbol;
			_writer = writer;
			_includeElementNodes = includeElementNodes;
			_documentedParameters = documentedParameters;
			_documentedTypeParameters = documentedTypeParameters;
		}

		public static string GetSubstitutedText(CSharpCompilation compilation, DiagnosticBag diagnostics, Symbol symbol, DocumentationCommentTriviaSyntax trivia, ArrayBuilder<CSharpSyntaxNode> includeElementNodes, ref HashSet<ParameterSymbol> documentedParameters, ref HashSet<TypeParameterSymbol> documentedTypeParameters)
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			using (StringWriter writer = new StringWriter(instance.Builder, CultureInfo.InvariantCulture))
			{
				DocumentationCommentWalker documentationCommentWalker = new DocumentationCommentWalker(compilation, diagnostics, symbol, writer, includeElementNodes, documentedParameters, documentedTypeParameters);
				documentationCommentWalker.Visit(trivia);
				documentedParameters = documentationCommentWalker._documentedParameters;
				documentedTypeParameters = documentationCommentWalker._documentedTypeParameters;
			}
			return instance.ToStringAndFree();
		}

		public override void DefaultVisit(SyntaxNode node)
		{
			SyntaxKind syntaxKind = node.Kind();
			bool flag = node.SyntaxTree.ReportDocumentationCommentDiagnostics();
			if (syntaxKind == SyntaxKind.XmlCrefAttribute)
			{
				XmlCrefAttributeSyntax xmlCrefAttributeSyntax = (XmlCrefAttributeSyntax)node;
				CrefSyntax cref = xmlCrefAttributeSyntax.Cref;
				Binder binder = _compilation.GetBinderFactory(cref.SyntaxTree).GetBinder(cref);
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				string documentationCommentId = GetDocumentationCommentId(cref, binder, instance);
				if (flag)
				{
					_diagnostics.AddRange(instance);
				}
				instance.Free();
				if (_writer != null)
				{
					Visit(xmlCrefAttributeSyntax.Name);
					VisitToken(xmlCrefAttributeSyntax.EqualsToken);
					xmlCrefAttributeSyntax.StartQuoteToken.WriteTo(_writer, leading: true, trailing: false);
					_writer.Write(documentationCommentId);
					xmlCrefAttributeSyntax.EndQuoteToken.WriteTo(_writer, leading: false, trailing: true);
				}
				return;
			}
			if (flag && syntaxKind == SyntaxKind.XmlNameAttribute)
			{
				XmlNameAttributeSyntax xmlNameAttributeSyntax = (XmlNameAttributeSyntax)node;
				Binder binder2 = _compilation.GetBinderFactory(xmlNameAttributeSyntax.SyntaxTree).GetBinder(xmlNameAttributeSyntax, xmlNameAttributeSyntax.Identifier.SpanStart);
				BindName(xmlNameAttributeSyntax, binder2, _memberSymbol, ref _documentedParameters, ref _documentedTypeParameters, _diagnostics);
			}
			if (_includeElementNodes != null)
			{
				XmlNameSyntax xmlNameSyntax = null;
				switch (syntaxKind)
				{
				case SyntaxKind.XmlEmptyElement:
					xmlNameSyntax = ((XmlEmptyElementSyntax)node).Name;
					break;
				case SyntaxKind.XmlElementStartTag:
					xmlNameSyntax = ((XmlElementStartTagSyntax)node).Name;
					break;
				}
				if (xmlNameSyntax != null && xmlNameSyntax.Prefix == null && DocumentationCommentXmlNames.ElementEquals(xmlNameSyntax.LocalName.ValueText, "include"))
				{
					_includeElementNodes.Add((CSharpSyntaxNode)node);
				}
			}
			base.DefaultVisit(node);
		}

		public override void VisitToken(SyntaxToken token)
		{
			if (_writer != null)
			{
				token.WriteTo(_writer);
			}
			base.VisitToken(token);
		}
	}

	private class IncludeElementExpander
	{
		private readonly Symbol _memberSymbol;

		private readonly ImmutableArray<CSharpSyntaxNode> _sourceIncludeElementNodes;

		private readonly CSharpCompilation _compilation;

		private readonly DiagnosticBag _diagnostics;

		private readonly CancellationToken _cancellationToken;

		private int _nextSourceIncludeElementIndex;

		private HashSet<Location> _inProgressIncludeElementNodes;

		private HashSet<ParameterSymbol> _documentedParameters;

		private HashSet<TypeParameterSymbol> _documentedTypeParameters;

		private DocumentationCommentIncludeCache _includedFileCache;

		private IncludeElementExpander(Symbol memberSymbol, ImmutableArray<CSharpSyntaxNode> sourceIncludeElementNodes, CSharpCompilation compilation, HashSet<ParameterSymbol> documentedParameters, HashSet<TypeParameterSymbol> documentedTypeParameters, DocumentationCommentIncludeCache includedFileCache, DiagnosticBag diagnostics, CancellationToken cancellationToken)
		{
			_memberSymbol = memberSymbol;
			_sourceIncludeElementNodes = sourceIncludeElementNodes;
			_compilation = compilation;
			_diagnostics = diagnostics;
			_cancellationToken = cancellationToken;
			_documentedParameters = documentedParameters;
			_documentedTypeParameters = documentedTypeParameters;
			_includedFileCache = includedFileCache;
			_nextSourceIncludeElementIndex = 0;
		}

		public static void ProcessIncludes(string unprocessed, Symbol memberSymbol, ImmutableArray<CSharpSyntaxNode> sourceIncludeElementNodes, CSharpCompilation compilation, ref HashSet<ParameterSymbol> documentedParameters, ref HashSet<TypeParameterSymbol> documentedTypeParameters, ref DocumentationCommentIncludeCache includedFileCache, TextWriter writer, DiagnosticBag diagnostics, CancellationToken cancellationToken)
		{
			if (sourceIncludeElementNodes.IsEmpty)
			{
				writer?.Write(unprocessed);
				return;
			}
			XDocument node;
			try
			{
				node = XDocument.Parse(unprocessed, LoadOptions.PreserveWhitespace);
			}
			catch (XmlException)
			{
				writer?.Write(unprocessed);
				return;
			}
			cancellationToken.ThrowIfCancellationRequested();
			IncludeElementExpander includeElementExpander = new IncludeElementExpander(memberSymbol, sourceIncludeElementNodes, compilation, documentedParameters, documentedTypeParameters, includedFileCache, diagnostics, cancellationToken);
			XNode[] array = includeElementExpander.Rewrite(node, null, null);
			foreach (XNode value in array)
			{
				cancellationToken.ThrowIfCancellationRequested();
				writer?.Write(value);
			}
			documentedParameters = includeElementExpander._documentedParameters;
			documentedTypeParameters = includeElementExpander._documentedTypeParameters;
			includedFileCache = includeElementExpander._includedFileCache;
		}

		private XNode[] RewriteMany(XNode[] nodes, string currentXmlFilePath, CSharpSyntaxNode originatingSyntax)
		{
			ArrayBuilder<XNode> arrayBuilder = null;
			foreach (XNode node in nodes)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<XNode>.GetInstance();
				}
				arrayBuilder.AddRange(Rewrite(node, currentXmlFilePath, originatingSyntax));
			}
			if (arrayBuilder != null)
			{
				return arrayBuilder.ToArrayAndFree();
			}
			return Array.Empty<XNode>();
		}

		private XNode[] Rewrite(XNode node, string currentXmlFilePath, CSharpSyntaxNode originatingSyntax)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			string commentMessage = null;
			if (node.NodeType == XmlNodeType.Element)
			{
				XElement xElement = (XElement)node;
				if (ElementNameIs(xElement, "include"))
				{
					XNode[] array = RewriteIncludeElement(xElement, currentXmlFilePath, originatingSyntax, out commentMessage);
					if (array != null)
					{
						return array;
					}
				}
			}
			if (node is XContainer xContainer)
			{
				IEnumerable<XNode> enumerable = xContainer.Nodes();
				XContainer xContainer2 = xContainer.Copy(copyAttributeAnnotations: false);
				if (enumerable != null)
				{
					XNode[] array2 = RewriteMany(enumerable.ToArray(), currentXmlFilePath, originatingSyntax);
					object[] content = array2;
					xContainer2.ReplaceNodes(content);
				}
				if (xContainer2.NodeType == XmlNodeType.Element && originatingSyntax != null)
				{
					XElement xElement2 = (XElement)xContainer2;
					foreach (XAttribute item in xElement2.Attributes())
					{
						if (AttributeNameIs(item, "cref"))
						{
							BindAndReplaceCref(item, originatingSyntax);
						}
						else if (AttributeNameIs(item, "name"))
						{
							if (ElementNameIs(xElement2, "param") || ElementNameIs(xElement2, "paramref"))
							{
								BindName(item, originatingSyntax, isParameter: true);
							}
							else if (ElementNameIs(xElement2, "typeparam") || ElementNameIs(xElement2, "typeparamref"))
							{
								BindName(item, originatingSyntax, isParameter: false);
							}
						}
					}
				}
				if (commentMessage != null)
				{
					XComment xComment = new XComment(commentMessage);
					return new XNode[2] { xComment, xContainer2 };
				}
				return new XNode[1] { xContainer2 };
			}
			return new XNode[1] { node.Copy(copyAttributeAnnotations: false) };
		}

		private static bool ElementNameIs(XElement element, string name)
		{
			if (string.IsNullOrEmpty(element.Name.NamespaceName))
			{
				return DocumentationCommentXmlNames.ElementEquals(element.Name.LocalName, name);
			}
			return false;
		}

		private static bool AttributeNameIs(XAttribute attribute, string name)
		{
			if (string.IsNullOrEmpty(attribute.Name.NamespaceName))
			{
				return DocumentationCommentXmlNames.AttributeEquals(attribute.Name.LocalName, name);
			}
			return false;
		}

		private XNode[] RewriteIncludeElement(XElement includeElement, string currentXmlFilePath, CSharpSyntaxNode originatingSyntax, out string commentMessage)
		{
			Location includeElementLocation = GetIncludeElementLocation(includeElement, ref currentXmlFilePath, ref originatingSyntax);
			bool flag = originatingSyntax.SyntaxTree.ReportDocumentationCommentDiagnostics();
			if (!EnterIncludeElement(includeElementLocation))
			{
				XAttribute xAttribute = includeElement.Attribute(XName.Get("file"));
				XAttribute? xAttribute2 = includeElement.Attribute(XName.Get("path"));
				string value = xAttribute.Value;
				string value2 = xAttribute2.Value;
				if (flag)
				{
					_diagnostics.Add(ErrorCode.WRN_FailedInclude, includeElementLocation, value, value2, new LocalizableErrorArgument(MessageID.IDS_OperationCausedStackOverflow));
				}
				commentMessage = ErrorFacts.GetMessage(MessageID.IDS_XMLNOINCLUDE, CultureInfo.CurrentUICulture);
				return new XNode[2]
				{
					new XComment(commentMessage),
					includeElement.Copy(copyAttributeAnnotations: false)
				};
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				XAttribute xAttribute3 = includeElement.Attribute(XName.Get("file"));
				XAttribute xAttribute4 = includeElement.Attribute(XName.Get("path"));
				bool flag2 = xAttribute3 != null;
				bool flag3 = xAttribute4 != null;
				if (!flag2 || !flag3)
				{
					LocalizableErrorArgument localizableErrorArgument = (flag2 ? MessageID.IDS_XMLMISSINGINCLUDEPATH.Localize() : MessageID.IDS_XMLMISSINGINCLUDEFILE.Localize());
					instance.Add(ErrorCode.WRN_InvalidInclude, includeElementLocation, localizableErrorArgument);
					commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLBADINCLUDE);
					return null;
				}
				string value3 = xAttribute4.Value;
				string value4 = xAttribute3.Value;
				XmlReferenceResolver xmlReferenceResolver = _compilation.Options.XmlReferenceResolver;
				if (xmlReferenceResolver == null)
				{
					instance.Add(ErrorCode.WRN_FailedInclude, includeElementLocation, value4, value3, new CodeAnalysisResourcesLocalizableErrorArgument("XmlReferencesNotSupported"));
					commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLFAILEDINCLUDE);
					return null;
				}
				string text = xmlReferenceResolver.ResolveReference(value4, currentXmlFilePath);
				if (text == null)
				{
					instance.Add(ErrorCode.WRN_FailedInclude, includeElementLocation, value4, value3, new CodeAnalysisResourcesLocalizableErrorArgument("FileNotFound"));
					commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLFAILEDINCLUDE);
					return null;
				}
				if (_includedFileCache == null)
				{
					_includedFileCache = new DocumentationCommentIncludeCache(xmlReferenceResolver);
				}
				try
				{
					XDocument orMakeDocument;
					try
					{
						orMakeDocument = _includedFileCache.GetOrMakeDocument(text);
					}
					catch (IOException ex)
					{
						instance.Add(ErrorCode.WRN_FailedInclude, includeElementLocation, value4, value3, ex.Message);
						commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLFAILEDINCLUDE);
						return null;
					}
					XElement[] array = XmlUtilities.TrySelectElements(orMakeDocument, value3, out var errorMessage, out var invalidXPath);
					if (array == null)
					{
						instance.Add(ErrorCode.WRN_FailedInclude, includeElementLocation, value4, value3, errorMessage);
						commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLFAILEDINCLUDE);
						if (invalidXPath)
						{
							return null;
						}
						if (includeElementLocation.IsInSource)
						{
							return new XNode[1]
							{
								new XComment(commentMessage)
							};
						}
						commentMessage = null;
						return Array.Empty<XNode>();
					}
					if (array != null && array.Length != 0)
					{
						XNode[] nodes = array;
						XNode[] array2 = RewriteMany(nodes, text, originatingSyntax);
						if (array2.Length != 0)
						{
							commentMessage = null;
							return array2;
						}
					}
					commentMessage = MakeCommentMessage(includeElementLocation, MessageID.IDS_XMLNOINCLUDE);
					return null;
				}
				catch (XmlException ex2)
				{
					Location location = XmlLocation.Create(ex2, text);
					instance.Add(ErrorCode.WRN_XMLParseIncludeError, location, GetDescription(ex2));
					if (includeElementLocation.IsInSource)
					{
						commentMessage = string.Format(ErrorFacts.GetMessage(MessageID.IDS_XMLIGNORED2, CultureInfo.CurrentUICulture), text);
						return new XNode[1]
						{
							new XComment(commentMessage)
						};
					}
					commentMessage = null;
					return Array.Empty<XNode>();
				}
			}
			finally
			{
				if (flag)
				{
					_diagnostics.AddRange(instance);
				}
				instance.Free();
				LeaveIncludeElement(includeElementLocation);
			}
		}

		private static string MakeCommentMessage(Location location, MessageID messageId)
		{
			if (location.IsInSource)
			{
				return ErrorFacts.GetMessage(messageId, CultureInfo.CurrentUICulture);
			}
			return null;
		}

		private bool EnterIncludeElement(Location location)
		{
			if (_inProgressIncludeElementNodes == null)
			{
				_inProgressIncludeElementNodes = new HashSet<Location>();
			}
			return _inProgressIncludeElementNodes.Add(location);
		}

		private bool LeaveIncludeElement(Location location)
		{
			return _inProgressIncludeElementNodes.Remove(location);
		}

		private Location GetIncludeElementLocation(XElement includeElement, ref string currentXmlFilePath, ref CSharpSyntaxNode originatingSyntax)
		{
			Location location = includeElement.Annotation<Location>();
			if (location != null)
			{
				return location;
			}
			if (currentXmlFilePath == null)
			{
				originatingSyntax = _sourceIncludeElementNodes[_nextSourceIncludeElementIndex];
				location = originatingSyntax.Location;
				_nextSourceIncludeElementIndex++;
				currentXmlFilePath = location.GetLineSpan().Path;
			}
			else
			{
				location = XmlLocation.Create(includeElement, currentXmlFilePath);
			}
			includeElement.AddAnnotation(location);
			return location;
		}

		private void BindAndReplaceCref(XAttribute attribute, CSharpSyntaxNode originatingSyntax)
		{
			CrefSyntax crefSyntax = SyntaxFactory.ParseCref(attribute.Value);
			if (crefSyntax != null)
			{
				Location location = originatingSyntax.Location;
				RecordSyntaxDiagnostics(crefSyntax, location);
				MemberDeclarationSyntax associatedMemberForXmlSyntax = BinderFactory.GetAssociatedMemberForXmlSyntax(originatingSyntax);
				Binder binder = BinderFactory.MakeCrefBinder(crefSyntax, associatedMemberForXmlSyntax, _compilation.GetBinderFactory(associatedMemberForXmlSyntax.SyntaxTree));
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				attribute.Value = GetDocumentationCommentId(crefSyntax, binder, instance);
				RecordBindingDiagnostics(instance, location);
				instance.Free();
			}
		}

		private void BindName(XAttribute attribute, CSharpSyntaxNode originatingSyntax, bool isParameter)
		{
			XmlNameAttributeSyntax xmlNameAttributeSyntax = ParseNameAttribute(attribute.ToString(), attribute.Parent.Name.LocalName);
			Location location = originatingSyntax.Location;
			RecordSyntaxDiagnostics(xmlNameAttributeSyntax, location);
			BinderFactory.GetAssociatedMemberForXmlSyntax(originatingSyntax);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			Binder binder = MakeNameBinder(isParameter, _memberSymbol, _compilation);
			DocumentationCommentCompiler.BindName(xmlNameAttributeSyntax, binder, _memberSymbol, ref _documentedParameters, ref _documentedTypeParameters, instance);
			RecordBindingDiagnostics(instance, location);
			instance.Free();
		}

		private static Binder MakeNameBinder(bool isParameter, Symbol memberSymbol, CSharpCompilation compilation)
		{
			Binder binder = new BuckStopsHereBinder(compilation);
			Symbol containingSymbol = memberSymbol.ContainingSymbol;
			binder = binder.WithContainingMemberOrLambda(containingSymbol);
			if (isParameter)
			{
				ImmutableArray<ParameterSymbol> parameters = ImmutableArray<ParameterSymbol>.Empty;
				switch (memberSymbol.Kind)
				{
				case SymbolKind.Method:
					parameters = ((MethodSymbol)memberSymbol).Parameters;
					break;
				case SymbolKind.Property:
					parameters = ((PropertySymbol)memberSymbol).Parameters;
					break;
				case SymbolKind.ErrorType:
				case SymbolKind.NamedType:
				{
					NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)memberSymbol;
					if (namedTypeSymbol.IsDelegateType())
					{
						parameters = namedTypeSymbol.DelegateInvokeMethod.Parameters;
					}
					break;
				}
				}
				if (parameters.Length > 0)
				{
					binder = new WithParametersBinder(parameters, binder);
				}
			}
			else
			{
				switch (memberSymbol.Kind)
				{
				case SymbolKind.ErrorType:
				case SymbolKind.NamedType:
				{
					NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)memberSymbol;
					if (namedTypeSymbol2.Arity > 0)
					{
						binder = new WithClassTypeParametersBinder(namedTypeSymbol2, binder);
					}
					break;
				}
				case SymbolKind.Method:
				{
					MethodSymbol methodSymbol = (MethodSymbol)memberSymbol;
					if (methodSymbol.Arity > 0)
					{
						binder = new WithMethodTypeParametersBinder(methodSymbol, binder);
					}
					break;
				}
				}
			}
			return binder;
		}

		private static XmlNameAttributeSyntax ParseNameAttribute(string attributeText, string elementName)
		{
			return (XmlNameAttributeSyntax)((XmlEmptyElementSyntax)((DocumentationCommentTriviaSyntax)SyntaxFactory.ParseLeadingTrivia($"/// <{elementName} {attributeText}/>", CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose)).ElementAt(0).GetStructure()).Content[1]).Attributes[0];
		}

		private void RecordSyntaxDiagnostics(CSharpSyntaxNode treelessSyntax, Location sourceLocation)
		{
			if (!treelessSyntax.ContainsDiagnostics || !sourceLocation.SourceTree.ReportDocumentationCommentDiagnostics())
			{
				return;
			}
			foreach (Diagnostic diagnostic in CSharpSyntaxTree.Dummy.GetDiagnostics(treelessSyntax))
			{
				_diagnostics.Add(diagnostic.WithLocation(sourceLocation));
			}
		}

		private void RecordBindingDiagnostics(DiagnosticBag bindingDiagnostics, Location sourceLocation)
		{
			if (bindingDiagnostics.IsEmptyWithoutResolution || !sourceLocation.SourceTree.ReportDocumentationCommentDiagnostics())
			{
				return;
			}
			foreach (Diagnostic item in bindingDiagnostics.AsEnumerable())
			{
				_diagnostics.Add(item.WithLocation(sourceLocation));
			}
		}
	}

	private readonly string _assemblyName;

	private readonly CSharpCompilation _compilation;

	private readonly TextWriter _writer;

	private readonly SyntaxTree _filterTree;

	private readonly TextSpan? _filterSpanWithinTree;

	private readonly bool _processIncludes;

	private readonly bool _isForSingleSymbol;

	private readonly DiagnosticBag _diagnostics;

	private readonly CancellationToken _cancellationToken;

	private SyntaxNodeLocationComparer _lazyComparer;

	private DocumentationCommentIncludeCache _includedFileCache;

	private int _indentDepth;

	private Stack<TemporaryStringBuilder> _temporaryStringBuilders;

	private static readonly string[] s_newLineSequences = new string[3] { "\r\n", "\r", "\n" };

	private IComparer<CSharpSyntaxNode> Comparer
	{
		get
		{
			if (_lazyComparer == null)
			{
				_lazyComparer = new SyntaxNodeLocationComparer(_compilation);
			}
			return _lazyComparer;
		}
	}

	private DocumentationCommentCompiler(string assemblyName, CSharpCompilation compilation, TextWriter writer, SyntaxTree filterTree, TextSpan? filterSpanWithinTree, bool processIncludes, bool isForSingleSymbol, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		_assemblyName = assemblyName;
		_compilation = compilation;
		_writer = writer;
		_filterTree = filterTree;
		_filterSpanWithinTree = filterSpanWithinTree;
		_processIncludes = processIncludes;
		_isForSingleSymbol = isForSingleSymbol;
		_diagnostics = diagnostics;
		_cancellationToken = cancellationToken;
	}

	public static void WriteDocumentationCommentXml(CSharpCompilation compilation, string assemblyName, Stream xmlDocStream, DiagnosticBag diagnostics, CancellationToken cancellationToken, SyntaxTree filterTree = null, TextSpan? filterSpanWithinTree = null)
	{
		StreamWriter streamWriter = null;
		if (xmlDocStream != null && xmlDocStream.CanWrite)
		{
			streamWriter = new StreamWriter(xmlDocStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false), 1024, leaveOpen: true);
		}
		try
		{
			using (streamWriter)
			{
				new DocumentationCommentCompiler(assemblyName ?? compilation.SourceAssembly.Name, compilation, streamWriter, filterTree, filterSpanWithinTree, processIncludes: true, isForSingleSymbol: false, diagnostics, cancellationToken).Visit(compilation.SourceAssembly.GlobalNamespace);
				streamWriter?.Flush();
			}
		}
		catch (Exception ex)
		{
			diagnostics.Add(ErrorCode.ERR_DocFileGen, Location.None, ex.Message);
		}
		if (filterTree != null)
		{
			UnprocessedDocumentationCommentFinder.ReportUnprocessed(filterTree, filterSpanWithinTree, diagnostics, cancellationToken);
			return;
		}
		foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
		{
			UnprocessedDocumentationCommentFinder.ReportUnprocessed(syntaxTree, null, diagnostics, cancellationToken);
		}
	}

	internal static string GetDocumentationCommentXml(Symbol symbol, bool processIncludes, CancellationToken cancellationToken)
	{
		CSharpCompilation declaringCompilation = symbol.DeclaringCompilation;
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringWriter stringWriter = new StringWriter(instance.Builder);
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		new DocumentationCommentCompiler(null, declaringCompilation, stringWriter, null, null, processIncludes, isForSingleSymbol: true, instance2, cancellationToken).Visit(symbol);
		instance2.Free();
		stringWriter.Dispose();
		return instance.ToStringAndFree();
	}

	public override void VisitNamespace(NamespaceSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (symbol.IsGlobalNamespace)
		{
			WriteLine("<?xml version=\"1.0\"?>");
			WriteLine("<doc>");
			Indent();
			if (!_compilation.Options.OutputKind.IsNetModule())
			{
				WriteLine("<assembly>");
				Indent();
				WriteLine("<name>{0}</name>", _assemblyName);
				Unindent();
				WriteLine("</assembly>");
			}
			WriteLine("<members>");
			Indent();
		}
		foreach (Symbol member in symbol.GetMembers())
		{
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			member.Accept(this);
		}
		if (symbol.IsGlobalNamespace)
		{
			Unindent();
			WriteLine("</members>");
			Unindent();
			WriteLine("</doc>");
		}
	}

	public override void VisitNamedType(NamedTypeSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_filterTree != null && !symbol.IsDefinedInSourceTree(_filterTree, _filterSpanWithinTree))
		{
			return;
		}
		DefaultVisit(symbol);
		if (!_isForSingleSymbol)
		{
			foreach (Symbol member in symbol.GetMembers())
			{
				cancellationToken = _cancellationToken;
				cancellationToken.ThrowIfCancellationRequested();
				member.Accept(this);
			}
		}
	}

	public override void DefaultVisit(Symbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (symbol.IsImplicitlyDeclared || symbol.IsAccessor() || (_filterTree != null && !symbol.IsDefinedInSourceTree(_filterTree, _filterSpanWithinTree)))
		{
			return;
		}
		bool flag = symbol.IsPartialDefinition();
		if (flag)
		{
			MethodSymbol partialImplementationPart = ((MethodSymbol)symbol).PartialImplementationPart;
			if ((object)partialImplementationPart != null)
			{
				Visit(partialImplementationPart);
			}
		}
		if (!TryGetDocumentationCommentNodes(symbol, out var maxDocumentationMode, out var nodes))
		{
			string message = ErrorFacts.GetMessage(MessageID.IDS_XMLIGNORED, CultureInfo.CurrentUICulture);
			WriteLine(string.Format(CultureInfo.CurrentUICulture, message, symbol.GetDocumentationCommentId()));
			return;
		}
		if (nodes.IsEmpty)
		{
			if ((int)maxDocumentationMode >= 2 && RequiresDocumentationComment(symbol))
			{
				Location locationInTreeReportingDocumentationCommentDiagnostics = GetLocationInTreeReportingDocumentationCommentDiagnostics(symbol);
				if (locationInTreeReportingDocumentationCommentDiagnostics != null)
				{
					_diagnostics.Add(ErrorCode.WRN_MissingXMLComment, locationInTreeReportingDocumentationCommentDiagnostics, symbol);
				}
			}
			return;
		}
		cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		bool flag2 = GetLocationInTreeReportingDocumentationCommentDiagnostics(symbol) != null;
		if (!TryProcessDocumentationCommentTriviaNodes(symbol, flag, nodes, flag2, out var withUnprocessedIncludes, out var haveParseError, out var documentedTypeParameters, out var documentedParameters, out var includeElementNodes))
		{
			return;
		}
		if (haveParseError)
		{
			string message2 = ErrorFacts.GetMessage(MessageID.IDS_XMLIGNORED, CultureInfo.CurrentUICulture);
			WriteLine(string.Format(CultureInfo.CurrentUICulture, message2, symbol.GetDocumentationCommentId()));
			return;
		}
		if (!includeElementNodes.IsDefaultOrEmpty)
		{
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			TextWriter writer = (flag ? null : _writer);
			IncludeElementExpander.ProcessIncludes(withUnprocessedIncludes, symbol, includeElementNodes, _compilation, ref documentedParameters, ref documentedTypeParameters, ref _includedFileCache, writer, _diagnostics, _cancellationToken);
		}
		else if (_writer != null && !flag)
		{
			Write(withUnprocessedIncludes);
		}
		if (!flag2)
		{
			return;
		}
		cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (documentedParameters != null)
		{
			foreach (ParameterSymbol parameter in GetParameters(symbol))
			{
				if (!documentedParameters.Contains(parameter))
				{
					Location location = parameter.Locations[0];
					_diagnostics.Add(ErrorCode.WRN_MissingParamTag, location, parameter.Name, symbol);
				}
			}
		}
		if (documentedTypeParameters == null)
		{
			return;
		}
		foreach (TypeParameterSymbol typeParameter in GetTypeParameters(symbol))
		{
			if (!documentedTypeParameters.Contains(typeParameter))
			{
				Location location2 = typeParameter.Locations[0];
				_diagnostics.Add(ErrorCode.WRN_MissingTypeParamTag, location2, typeParameter, symbol);
			}
		}
	}

	private bool TryProcessDocumentationCommentTriviaNodes(Symbol symbol, bool isPartialMethodDefinitionPart, ImmutableArray<DocumentationCommentTriviaSyntax> docCommentNodes, bool reportParameterOrTypeParameterDiagnostics, out string withUnprocessedIncludes, out bool haveParseError, out HashSet<TypeParameterSymbol> documentedTypeParameters, out HashSet<ParameterSymbol> documentedParameters, out ImmutableArray<CSharpSyntaxNode> includeElementNodes)
	{
		bool flag = false;
		ArrayBuilder<CSharpSyntaxNode> arrayBuilder = null;
		documentedParameters = null;
		documentedTypeParameters = null;
		haveParseError = false;
		foreach (DocumentationCommentTriviaSyntax item in docCommentNodes)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			bool flag2 = item.SyntaxTree.ReportDocumentationCommentDiagnostics();
			if (!flag)
			{
				BeginTemporaryString();
				if (_processIncludes)
				{
					arrayBuilder = ArrayBuilder<CSharpSyntaxNode>.GetInstance();
				}
				if (!isPartialMethodDefinitionPart || _processIncludes)
				{
					WriteLine("<member name=\"{0}\">", symbol.GetDocumentationCommentId());
					Indent();
				}
				flag = true;
			}
			string substitutedText = DocumentationCommentWalker.GetSubstitutedText(_compilation, _diagnostics, symbol, item, arrayBuilder, ref documentedParameters, ref documentedTypeParameters);
			string text = FormatComment(substitutedText);
			XmlException ex = XmlDocumentationCommentTextReader.ParseAndGetException(text);
			if (ex != null)
			{
				haveParseError = true;
				if (flag2)
				{
					Location location = new SourceLocation(item.SyntaxTree, new TextSpan(item.SpanStart, 0));
					_diagnostics.Add(ErrorCode.WRN_XMLParseError, location, GetDescription(ex));
				}
			}
			if (!isPartialMethodDefinitionPart || _processIncludes)
			{
				Write(text);
			}
		}
		if (!flag)
		{
			withUnprocessedIncludes = null;
			includeElementNodes = default;
			return false;
		}
		if (!isPartialMethodDefinitionPart || _processIncludes)
		{
			Unindent();
			WriteLine("</member>");
		}
		withUnprocessedIncludes = GetAndEndTemporaryString();
		includeElementNodes = (_processIncludes ? arrayBuilder.ToImmutableAndFree() : default(ImmutableArray<CSharpSyntaxNode>));
		return true;
	}

	private static Location GetLocationInTreeReportingDocumentationCommentDiagnostics(Symbol symbol)
	{
		foreach (Location location in symbol.Locations)
		{
			if (location.SourceTree.ReportDocumentationCommentDiagnostics())
			{
				return location;
			}
		}
		return null;
	}

	private static ImmutableArray<ParameterSymbol> GetParameters(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.NamedType:
		{
			MethodSymbol delegateInvokeMethod = ((NamedTypeSymbol)symbol).DelegateInvokeMethod;
			if ((object)delegateInvokeMethod != null)
			{
				return delegateInvokeMethod.Parameters;
			}
			break;
		}
		case SymbolKind.Event:
		case SymbolKind.Method:
		case SymbolKind.Property:
			return symbol.GetParameters();
		}
		return ImmutableArray<ParameterSymbol>.Empty;
	}

	private static ImmutableArray<TypeParameterSymbol> GetTypeParameters(Symbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.ErrorType || kind == SymbolKind.Method || kind == SymbolKind.NamedType)
		{
			return symbol.GetMemberTypeParameters();
		}
		return ImmutableArray<TypeParameterSymbol>.Empty;
	}

	private static bool RequiresDocumentationComment(Symbol symbol)
	{
		if (symbol.IsImplicitlyDeclared || symbol.IsAccessor())
		{
			return false;
		}
		while ((object)symbol != null)
		{
			Accessibility declaredAccessibility = symbol.DeclaredAccessibility;
			if (declaredAccessibility == Accessibility.Protected || (uint)(declaredAccessibility - 5) <= 1u)
			{
				symbol = symbol.ContainingType;
				continue;
			}
			return false;
		}
		return true;
	}

	private bool TryGetDocumentationCommentNodes(Symbol symbol, out DocumentationMode maxDocumentationMode, out ImmutableArray<DocumentationCommentTriviaSyntax> nodes)
	{
		maxDocumentationMode = DocumentationMode.None;
		nodes = default;
		ArrayBuilder<DocumentationCommentTriviaSyntax> arrayBuilder = null;
		foreach (SyntaxReference declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
		{
			DocumentationMode documentationMode = declaringSyntaxReference.SyntaxTree.Options.DocumentationMode;
			maxDocumentationMode = (((int)documentationMode > (int)maxDocumentationMode) ? documentationMode : maxDocumentationMode);
			foreach (DocumentationCommentTriviaSyntax item in SourceDocumentationCommentUtils.GetDocumentationCommentTriviaFromSyntaxNode((CSharpSyntaxNode)declaringSyntaxReference.GetSyntax(), _diagnostics))
			{
				if (ContainsXmlParseDiagnostic(item))
				{
					arrayBuilder?.Free();
					return false;
				}
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<DocumentationCommentTriviaSyntax>.GetInstance();
				}
				arrayBuilder.Add(item);
			}
		}
		if (arrayBuilder == null)
		{
			nodes = ImmutableArray<DocumentationCommentTriviaSyntax>.Empty;
		}
		else
		{
			arrayBuilder.Sort(Comparer);
			nodes = arrayBuilder.ToImmutableAndFree();
		}
		return true;
	}

	private static bool ContainsXmlParseDiagnostic(DocumentationCommentTriviaSyntax node)
	{
		if (!node.ContainsDiagnostics)
		{
			return false;
		}
		foreach (Diagnostic diagnostic in node.GetDiagnostics())
		{
			if (diagnostic.Code == 1570)
			{
				return true;
			}
		}
		return false;
	}

	private string FormatComment(string substitutedText)
	{
		BeginTemporaryString();
		if (TrimmedStringStartsWith(substitutedText, "///"))
		{
			WriteFormattedSingleLineComment(substitutedText);
		}
		else
		{
			string[] array = substitutedText.Split(s_newLineSequences, StringSplitOptions.None);
			int num = array.Length;
			if (string.IsNullOrEmpty(array[num - 1]))
			{
				num--;
			}
			WriteFormattedMultiLineComment(array, num);
		}
		return GetAndEndTemporaryString();
	}

	private static int GetIndexOfFirstNonWhitespaceChar(string str)
	{
		return GetIndexOfFirstNonWhitespaceChar(str, 0, str.Length);
	}

	private static int GetIndexOfFirstNonWhitespaceChar(string str, int start, int end)
	{
		while (start < end && SyntaxFacts.IsWhitespace(str[start]))
		{
			start++;
		}
		return start;
	}

	private static bool TrimmedStringStartsWith(string str, string prefix)
	{
		int indexOfFirstNonWhitespaceChar = GetIndexOfFirstNonWhitespaceChar(str);
		if (str.Length - indexOfFirstNonWhitespaceChar < prefix.Length)
		{
			return false;
		}
		for (int i = 0; i < prefix.Length; i++)
		{
			if (prefix[i] != str[i + indexOfFirstNonWhitespaceChar])
			{
				return false;
			}
		}
		return true;
	}

	private static int IndexOfNewLine(string str, int start, out int newLineLength)
	{
		while (start < str.Length)
		{
			switch (str[start])
			{
			case '\r':
				if (start + 1 < str.Length && str[start + 1] == '\n')
				{
					newLineLength = 2;
				}
				else
				{
					newLineLength = 1;
				}
				return start;
			case '\n':
				newLineLength = 1;
				return start;
			}
			start++;
		}
		newLineLength = 0;
		return start;
	}

	private void WriteFormattedSingleLineComment(string text)
	{
		bool flag = true;
		int num = 0;
		while (num < text.Length)
		{
			int num2 = IndexOfNewLine(text, num, out var newLineLength);
			int indexOfFirstNonWhitespaceChar = GetIndexOfFirstNonWhitespaceChar(text, num, num2);
			if (num2 - indexOfFirstNonWhitespaceChar < 4 || !SyntaxFacts.IsWhitespace(text[indexOfFirstNonWhitespaceChar + 3]))
			{
				flag = false;
				break;
			}
			num = num2 + newLineLength;
		}
		int num3 = (flag ? 4 : 3);
		int num4 = 0;
		while (num4 < text.Length)
		{
			int num5 = IndexOfNewLine(text, num4, out var newLineLength2);
			int num6 = GetIndexOfFirstNonWhitespaceChar(text, num4, num5) + num3;
			WriteSubStringLine(text, num6, num5 - num6);
			num4 = num5 + newLineLength2;
		}
	}

	private void WriteFormattedMultiLineComment(string[] lines, int numLines)
	{
		bool flag = lines[0].Trim() == "/**";
		bool flag2 = lines[numLines - 1].Trim() == "*/";
		if (flag2)
		{
			numLines--;
		}
		int startIndex = 0;
		if (numLines > 1)
		{
			string text = FindMultiLineCommentPattern(lines[1]);
			if (text != null)
			{
				bool flag3 = true;
				for (int i = 2; i < numLines; i++)
				{
					string text2 = LongestCommonPrefix(text, lines[i]);
					if (string.IsNullOrWhiteSpace(text2))
					{
						flag3 = false;
						break;
					}
					text = text2;
				}
				if (flag3)
				{
					startIndex = text.Length;
				}
			}
		}
		if (!flag)
		{
			string text3 = lines[0].TrimStart(null);
			if (!flag2 && numLines == 1)
			{
				text3 = TrimEndOfMultiLineComment(text3);
			}
			WriteLine(text3.Substring(SyntaxFacts.IsWhitespace(text3[3]) ? 4 : 3));
		}
		for (int j = 1; j < numLines; j++)
		{
			string text4 = lines[j].Substring(startIndex);
			if (!flag2 && j == numLines - 1)
			{
				text4 = TrimEndOfMultiLineComment(text4);
			}
			WriteLine(text4);
		}
	}

	private static string TrimEndOfMultiLineComment(string trimmed)
	{
		int num = trimmed.IndexOf("*/", StringComparison.Ordinal);
		if (num >= 0)
		{
			trimmed = trimmed.Substring(0, num);
		}
		return trimmed;
	}

	private static string FindMultiLineCommentPattern(string line)
	{
		int num = 0;
		bool flag = false;
		foreach (char c in line)
		{
			if (SyntaxFacts.IsWhitespace(c))
			{
				num++;
				continue;
			}
			if (flag || c != '*')
			{
				break;
			}
			num++;
			flag = true;
		}
		if (!flag)
		{
			return null;
		}
		return line.Substring(0, num);
	}

	private static string LongestCommonPrefix(string str1, string str2)
	{
		int i = 0;
		for (int num = Math.Min(str1.Length, str2.Length); i < num && str1[i] == str2[i]; i++)
		{
		}
		return str1.Substring(0, i);
	}

	private static string GetDocumentationCommentId(CrefSyntax crefSyntax, Binder binder, DiagnosticBag diagnostics)
	{
		if (crefSyntax.ContainsDiagnostics)
		{
			return ToBadCrefString(crefSyntax);
		}
		ImmutableArray<Symbol> immutableArray = binder.BindCref(crefSyntax, out var ambiguityWinner, diagnostics);
		Symbol symbol;
		switch (immutableArray.Length)
		{
		case 0:
			return ToBadCrefString(crefSyntax);
		case 1:
			symbol = immutableArray[0];
			break;
		default:
			symbol = ambiguityWinner;
			break;
		}
		if (symbol.Kind == SymbolKind.Alias)
		{
			symbol = ((AliasSymbol)symbol).GetAliasTarget(null);
		}
		return symbol.OriginalDefinition.GetDocumentationCommentId();
	}

	private static string ToBadCrefString(CrefSyntax cref)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		cref.WriteTo(stringWriter);
		return "!:" + stringWriter.ToString().Replace("{", "&lt;").Replace("}", "&gt;");
	}

	private static void BindName(XmlNameAttributeSyntax syntax, Binder binder, Symbol memberSymbol, ref HashSet<ParameterSymbol> documentedParameters, ref HashSet<TypeParameterSymbol> documentedTypeParameters, DiagnosticBag diagnostics)
	{
		XmlNameAttributeElementKind elementKind = syntax.GetElementKind();
		switch (elementKind)
		{
		case XmlNameAttributeElementKind.Parameter:
			if (documentedParameters == null)
			{
				documentedParameters = new HashSet<ParameterSymbol>();
			}
			break;
		case XmlNameAttributeElementKind.TypeParameter:
			if (documentedTypeParameters == null)
			{
				documentedTypeParameters = new HashSet<TypeParameterSymbol>();
			}
			break;
		}
		IdentifierNameSyntax identifier = syntax.Identifier;
		if (identifier.ContainsDiagnostics)
		{
			return;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		ImmutableArray<Symbol> immutableArray = binder.BindXmlNameAttribute(syntax, ref useSiteDiagnostics);
		diagnostics.Add(syntax, useSiteDiagnostics);
		if (immutableArray.IsEmpty)
		{
			switch (elementKind)
			{
			case XmlNameAttributeElementKind.Parameter:
				diagnostics.Add(ErrorCode.WRN_UnmatchedParamTag, identifier.Location, identifier);
				break;
			case XmlNameAttributeElementKind.ParameterReference:
				diagnostics.Add(ErrorCode.WRN_UnmatchedParamRefTag, identifier.Location, identifier, memberSymbol);
				break;
			case XmlNameAttributeElementKind.TypeParameter:
				diagnostics.Add(ErrorCode.WRN_UnmatchedTypeParamTag, identifier.Location, identifier);
				break;
			case XmlNameAttributeElementKind.TypeParameterReference:
				diagnostics.Add(ErrorCode.WRN_UnmatchedTypeParamRefTag, identifier.Location, identifier, memberSymbol);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(elementKind);
			}
			return;
		}
		foreach (Symbol item in immutableArray)
		{
			switch (elementKind)
			{
			case XmlNameAttributeElementKind.Parameter:
			{
				ParameterSymbol parameterSymbol = (ParameterSymbol)item;
				if (!parameterSymbol.ContainingSymbol.IsAccessor() && !documentedParameters.Add(parameterSymbol))
				{
					diagnostics.Add(ErrorCode.WRN_DuplicateParamTag, syntax.Location, identifier);
				}
				break;
			}
			case XmlNameAttributeElementKind.TypeParameter:
				if (!documentedTypeParameters.Add((TypeParameterSymbol)item))
				{
					diagnostics.Add(ErrorCode.WRN_DuplicateTypeParamTag, syntax.Location, identifier);
				}
				break;
			}
		}
	}

	private void BeginTemporaryString()
	{
		if (_temporaryStringBuilders == null)
		{
			_temporaryStringBuilders = new Stack<TemporaryStringBuilder>();
		}
		_temporaryStringBuilders.Push(new TemporaryStringBuilder(_indentDepth));
	}

	private string GetAndEndTemporaryString()
	{
		TemporaryStringBuilder temporaryStringBuilder = _temporaryStringBuilders.Pop();
		_indentDepth = temporaryStringBuilder.InitialIndentDepth;
		return temporaryStringBuilder.Pooled.ToStringAndFree();
	}

	private void Indent()
	{
		_indentDepth++;
	}

	private void Unindent()
	{
		_indentDepth--;
	}

	private void Write(string indentedAndWrappedString)
	{
		if (_temporaryStringBuilders != null && _temporaryStringBuilders.Count > 0)
		{
			_temporaryStringBuilders.Peek().Pooled.Builder.Append(indentedAndWrappedString);
		}
		else if (_writer != null)
		{
			_writer.Write(indentedAndWrappedString);
		}
	}

	private void WriteLine(string message)
	{
		Stack<TemporaryStringBuilder> temporaryStringBuilders = _temporaryStringBuilders;
		if (temporaryStringBuilders != null && temporaryStringBuilders.Count > 0)
		{
			StringBuilder builder = _temporaryStringBuilders.Peek().Pooled.Builder;
			builder.Append(MakeIndent(_indentDepth));
			builder.AppendLine(message);
		}
		else if (_writer != null)
		{
			_writer.Write(MakeIndent(_indentDepth));
			_writer.WriteLine(message);
		}
	}

	private void WriteSubStringLine(string message, int start, int length)
	{
		Stack<TemporaryStringBuilder> temporaryStringBuilders = _temporaryStringBuilders;
		if (temporaryStringBuilders != null && temporaryStringBuilders.Count > 0)
		{
			StringBuilder builder = _temporaryStringBuilders.Peek().Pooled.Builder;
			builder.Append(MakeIndent(_indentDepth));
			builder.Append(message, start, length);
			builder.AppendLine();
		}
		else if (_writer != null)
		{
			_writer.Write(MakeIndent(_indentDepth));
			for (int i = 0; i < length; i++)
			{
				_writer.Write(message[start + i]);
			}
			_writer.WriteLine();
		}
	}

	private void WriteLine(string format, params object[] args)
	{
		WriteLine(string.Format(format, args));
	}

	private static string MakeIndent(int depth)
	{
		return depth switch
		{
			0 => "", 
			1 => "    ", 
			2 => "        ", 
			3 => "            ", 
			_ => new string(' ', depth * 4), 
		};
	}

	private static string GetDescription(XmlException e)
	{
		string message = e.Message;
		try
		{
			string text = string.Format(new ResourceManager("System.Xml", typeof(XmlException).GetTypeInfo().Assembly).GetString("Xml_MessageWithErrorPosition"), "", e.LineNumber, e.LinePosition);
			int num = message.IndexOf(text, StringComparison.Ordinal);
			return (num < 0) ? message : message.Remove(num, text.Length);
		}
		catch
		{
			return message;
		}
	}
}
