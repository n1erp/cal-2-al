using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public struct TaggedText
{
	public string Tag { get; }

	public string Text { get; }

	public TaggedText(string tag, string text)
	{
		if (tag == null)
		{
			throw new ArgumentNullException("tag");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		Tag = tag;
		Text = text;
	}

	public override string ToString()
	{
		return Text;
	}

	public static bool operator ==(TaggedText taggedText1, TaggedText taggedText2)
	{
		if (string.Compare(taggedText1.Text, taggedText2.Text, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return string.Compare(taggedText1.Tag, taggedText2.Tag, StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}

	public static bool operator !=(TaggedText taggedText1, TaggedText taggedText2)
	{
		return !(taggedText1 == taggedText2);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		TaggedText taggedText = (TaggedText)obj;
		return this == taggedText;
	}

	public override int GetHashCode()
	{
		return Tag.GetHashCode() ^ Text.GetHashCode();
	}
}
