using Newtonsoft.Json;

namespace Auditai.DTO;

public class DocStyle
{
	private int _firstRowIndent;

	private int _beforeParagraph;

	private int _afterParagraph;

	private string _paragraphSpace;

	[JsonProperty(PropertyName = "FontStyle")]
	public FontSetting FontStyle { get; set; }

	[JsonProperty(PropertyName = "ParagraphSpace")]
	public string ParagraphSpace
	{
		get
		{
			if (!float.TryParse(_paragraphSpace, out var _))
			{
				return "1.5";
			}
			return _paragraphSpace;
		}
		set
		{
			_paragraphSpace = value;
		}
	}

	[JsonProperty(PropertyName = "BeforeParagraph")]
	public int BeforeParagraph
	{
		get
		{
			if (_beforeParagraph < 0)
			{
				return 0;
			}
			if (_beforeParagraph > 5000)
			{
				return 5000;
			}
			return _beforeParagraph;
		}
		set
		{
			_beforeParagraph = value;
		}
	}

	[JsonProperty(PropertyName = "AfterParagraph")]
	public int AfterParagraph
	{
		get
		{
			if (_afterParagraph < 0)
			{
				return 0;
			}
			if (_afterParagraph > 5000)
			{
				return 5000;
			}
			return _afterParagraph;
		}
		set
		{
			_afterParagraph = value;
		}
	}

	[JsonProperty(PropertyName = "FirstRowIndent")]
	public int FirstRowIndent
	{
		get
		{
			if (_firstRowIndent < 0)
			{
				return 0;
			}
			if (_firstRowIndent > 5000)
			{
				return 5000;
			}
			return _firstRowIndent;
		}
		set
		{
			_firstRowIndent = value;
		}
	}

	public DocStyle()
	{
		FontStyle = new FontSetting();
		ParagraphSpace = "1.5";
		BeforeParagraph = 0;
		AfterParagraph = 0;
		FirstRowIndent = 0;
	}
}
