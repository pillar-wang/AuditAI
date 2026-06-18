using System;

namespace Leqisoft.UI.Platform;

public class ParagraphTooLongException : ApplicationException
{
	public int ParaIndex { get; }

	public override string Message => "段落数据太长，请删除或缩小段落中的图片内容";

	private ParagraphTooLongException()
	{
	}

	public ParagraphTooLongException(int paraIndex)
	{
		ParaIndex = paraIndex;
	}
}
