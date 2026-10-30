using System;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

[Flags]
internal enum LexerMode
{
	Syntax = 1,
	DebuggerSyntax = 2,
	Directive = 4,
	XmlDocComment = 8,
	XmlElementTag = 0x10,
	XmlAttributeTextQuote = 0x20,
	XmlAttributeTextDoubleQuote = 0x40,
	XmlCrefQuote = 0x80,
	XmlCrefDoubleQuote = 0x100,
	XmlNameQuote = 0x200,
	XmlNameDoubleQuote = 0x400,
	XmlCDataSectionText = 0x800,
	XmlCommentText = 0x1000,
	XmlProcessingInstructionText = 0x2000,
	XmlCharacter = 0x4000,
	MaskLexMode = 0xFFFF,
	XmlDocCommentLocationStart = 0,
	XmlDocCommentLocationInterior = 0x10000,
	XmlDocCommentLocationExterior = 0x20000,
	XmlDocCommentLocationEnd = 0x40000,
	MaskXmlDocCommentLocation = 0xF0000,
	XmlDocCommentStyleSingleLine = 0,
	XmlDocCommentStyleDelimited = 0x100000,
	MaskXmlDocCommentStyle = 0x300000,
	None = 0
}
