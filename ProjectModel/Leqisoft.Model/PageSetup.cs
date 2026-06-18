using System.Drawing.Printing;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class PageSetup
{
	public Direction Direction { get; set; }

	public PaperKind PaperKind { get; set; }

	public double PaperWidth { get; set; }

	public double PaperHeight { get; set; }

	public bool IsPrintIndex { get; set; }

	public bool HasNoteBorder { get; set; }

	public double HorizontalZoom { get; set; }

	public double VerticalZoom { get; set; }

	[JsonProperty("LeftMargin")]
	public double LeftMargin { get; set; }

	[JsonProperty("BottomMargin")]
	public double BottomMargin { get; set; }

	[JsonProperty("TopMargin")]
	public double TopMargin { get; set; }

	[JsonProperty("RightMargin")]
	public double RightMargin { get; set; }

	public double HeaderMargin { get; set; } = 15.0;


	public double FooterMargin { get; set; } = 15.0;


	public int FixedPrintColsNum { get; set; }

	public string PrintPageRange { get; set; }

	public short PrintCopies { get; set; } = 1;


	public bool OneColor { get; set; } = true;


	public bool FitPageWidth { get; set; }

	public bool FitPageHeight { get; set; }

	public int StartPageNo { get; set; } = 1;


	public HeaderFooter PageHeader { get; }

	public HeaderFooter PageFooter { get; }

	public PageSetup()
	{
		LeftMargin = 31.8;
		RightMargin = 31.8;
		TopMargin = 25.4;
		BottomMargin = 25.4;
		HeaderMargin = 15.0;
		FooterMargin = 15.0;
		IsPrintIndex = false;
		HorizontalZoom = 1.0;
		VerticalZoom = 1.0;
		Direction = Direction.Vertical;
		PaperKind = PaperKind.A4;
		PageHeader = new HeaderFooter();
		PageFooter = new HeaderFooter();
		PageHeader.Height = 15.0;
		PageFooter.Height = 17.5;
		FixedPrintColsNum = 1;
		PrintCopies = 1;
		PrintPageRange = "全部";
		FitPageWidth = true;
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void Deserialize(string value)
	{
		JsonConvert.PopulateObject(value, this);
	}
}
