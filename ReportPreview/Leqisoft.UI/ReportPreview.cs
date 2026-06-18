using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using C1.C1Preview;
using C1.Win.C1Preview;
using Leqisoft.DTO;
using Leqisoft.Model;

namespace Leqisoft.UI;

public class ReportPreview
{
	private readonly Graphics ScreenGraphics = Graphics.FromHwnd(IntPtr.Zero);

	private readonly C1PrintDocument _pd;

	private const int TITLE_MARGIN = 20;

	private Leqisoft.Model.Image _image;

	private Leqisoft.Model.Table _table;

	private PreviewKind _previewKind;

	private double fitAdapterEmpty;

	private double fitAapterContent;

	private int layers;

	private TreeNodeBase CurrentTreeNode
	{
		get
		{
			if (PreviewKind != 0)
			{
				if (PreviewKind != PreviewKind.Image)
				{
					return null;
				}
				return Image.TreeNode;
			}
			return Table.TreeNode;
		}
	}

	private Leqisoft.Model.Project CurrentProject
	{
		get
		{
			if (PreviewKind != 0)
			{
				if (PreviewKind != PreviewKind.Image)
				{
					return null;
				}
				return Image.Project;
			}
			return Table.Project;
		}
	}

	private RenderTable DataTable { get; set; }

	public C1PreviewPane View { get; } = new C1PreviewPane
	{
		Dock = DockStyle.Fill
	};


	public C1PrintDocument PrintDocument => _pd;

	public Leqisoft.Model.Table Table
	{
		get
		{
			return _table;
		}
		set
		{
			_table = value;
			PreviewKind = PreviewKind.Table;
		}
	}

	public Leqisoft.Model.Image Image
	{
		get
		{
			return _image;
		}
		set
		{
			_image = value;
			PreviewKind = PreviewKind.Image;
		}
	}

	public PreviewKind PreviewKind
	{
		get
		{
			return _previewKind;
		}
		set
		{
			_previewKind = value;
		}
	}

	public PageSetup PageSetup
	{
		get
		{
			if (PreviewKind != 0)
			{
				if (PreviewKind != PreviewKind.Image)
				{
					return null;
				}
				return Image.PageSetup;
			}
			return Table.PageSetup;
		}
	}

	private double DisplayZoom => Math.Min(PageSetup.HorizontalZoom, PageSetup.VerticalZoom);

	private double PageWidth => _pd.PageLayout.PageSettings.Width.ConvertUnit(UnitTypeEnum.Pixel, ScreenGraphics.DpiX);

	private double MarginLeft => _pd.PageLayout.PageSettings.LeftMargin.ConvertUnit(UnitTypeEnum.Pixel, ScreenGraphics.DpiX);

	private double MarginRight => _pd.PageLayout.PageSettings.RightMargin.ConvertUnit(UnitTypeEnum.Pixel, ScreenGraphics.DpiX);

	private bool ModelIsNull
	{
		get
		{
			if (PreviewKind != 0)
			{
				if (PreviewKind != PreviewKind.Image)
				{
					return true;
				}
				return Image == null;
			}
			return Table == null;
		}
	}

	public bool WidthLock { get; private set; }

	public bool HeightLock { get; private set; }

	public ReportPreview()
	{
		_pd = new C1PrintDocument
		{
			AllowNonReflowableDocs = true,
			UseGdiPlusTextRendering = true,
			DefaultUnit = UnitTypeEnum.Inch,
			ResolvedUnit = UnitTypeEnum.Inch
		};
		_pd.PageConfigure += _pd_PageConfigure;
		_pd.LongOperation += _pd_LongOperation;
	}

	public void CreatePaper()
	{
		switch (PreviewKind)
		{
		case PreviewKind.Table:
			TableCreatePaper();
			break;
		case PreviewKind.Image:
			ImageCreatePaper();
			break;
		default:
			throw new ArgumentOutOfRangeException("暂时只能打印表格图片文件");
		}
	}

	public void ApplySubTotal(int FixCols = 1)
	{
		SubTotalImpl(FixCols);
	}

	public void ChangeLeftMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.LeftMargin = margin;
			CreatePaper();
		}
	}

	public void ChangeRightMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.RightMargin = margin;
			CreatePaper();
		}
	}

	public void ChangeTopMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.TopMargin = margin;
			CreatePaper();
		}
	}

	public void ChangeBottomMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.BottomMargin = margin;
			CreatePaper();
		}
	}

	public void ChangeHeaderMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.HeaderMargin = margin;
			CreatePaper();
		}
	}

	public void ChangeFooterMargin(double margin)
	{
		if (!ModelIsNull)
		{
			PageSetup.FooterMargin = margin;
			CreatePaper();
		}
	}

	public void ChangePaper(PaperKind paperkind)
	{
		if (!ModelIsNull)
		{
			PageSetup.PaperKind = paperkind;
			CreatePaper();
		}
	}

	public void Portrait()
	{
		if (!ModelIsNull)
		{
			PageSetup.Direction = Direction.Vertical;
			CreatePaper();
		}
	}

	public void Landscape()
	{
		if (!ModelIsNull)
		{
			PageSetup.Direction = Direction.Horizontal;
			CreatePaper();
		}
	}

	public void ToggleRowIndexVisible()
	{
		if (!ModelIsNull)
		{
			PageSetup.IsPrintIndex = !PageSetup.IsPrintIndex;
			CreatePaper();
		}
	}

	public void ToggleNoteBorder()
	{
		if (!ModelIsNull)
		{
			PageSetup.HasNoteBorder = !PageSetup.HasNoteBorder;
			CreatePaper();
		}
	}

	public void FitPageHeight(bool IsFit)
	{
		if (!ModelIsNull)
		{
			PageSetup.FitPageHeight = IsFit;
			CreatePaper();
		}
	}

	public void FitPageWidth(bool IsFit)
	{
		if (!ModelIsNull)
		{
			PageSetup.FitPageWidth = IsFit;
			CreatePaper();
		}
	}

	public void SetFixedColCount(int count)
	{
		if (!ModelIsNull)
		{
			PageSetup.FixedPrintColsNum = count;
			CreatePaper();
		}
	}

	public void AllOnePage()
	{
		if (!ModelIsNull)
		{
			FitPageHeight(IsFit: true);
			FitPageWidth(IsFit: true);
		}
	}

	public void ChangeHorizZoom(double value)
	{
		if (!ModelIsNull)
		{
			PageSetup.FitPageWidth = false;
			PageSetup.HorizontalZoom = value;
			CreatePaper();
		}
	}

	public void ChangeVertZoom(double value)
	{
		if (!ModelIsNull)
		{
			PageSetup.FitPageHeight = false;
			PageSetup.VerticalZoom = value;
			CreatePaper();
		}
	}

	public void SetStartPageNo(int Start)
	{
		if (!ModelIsNull)
		{
			PageSetup.StartPageNo = Start;
			CreatePaper();
		}
	}

	public void OneColor(bool OneColor)
	{
		if (!ModelIsNull)
		{
			PageSetup.OneColor = OneColor;
			CreatePaper();
		}
	}

	public void PrintRange(string printRange)
	{
		if (!ModelIsNull)
		{
			PageSetup.PrintPageRange = printRange;
			Table.TagPageSetupDirty();
		}
	}

	public void PrintCopies(short Copies)
	{
		if (!ModelIsNull)
		{
			PageSetup.PrintCopies = Copies;
			Table.TagPageSetupDirty();
		}
	}

	public void PrintDialog()
	{
		if (ModelIsNull)
		{
			return;
		}
		try
		{
			CreatePaper();
			C1PrintDocument c1PrintDocument = View.Document as C1PrintDocument;
			c1PrintDocument.PrintDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	public void Print(PrinterSettings printerSettings)
	{
		if (ModelIsNull)
		{
			return;
		}
		try
		{
			printerSettings = printerSettings ?? new PrinterSettings();
			CreatePaper();
			C1PrintDocument c1PrintDocument = View.Document as C1PrintDocument;
			c1PrintDocument.Print(printerSettings, showProgress: false);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private void _pd_PageConfigure(C1PrintDocument sender, PageConfigureEventArgs e)
	{
		Dictionary<string, string> tagDic = new Dictionary<string, string>
		{
			["[PageNo]"] = $"{PageSetup.StartPageNo + e.Page.PageNo - 1}",
			["[PageCount]"] = $"{e.Page.PageCount}"
		};
		PageLayout pageLayout = new PageLayout();
		pageLayout.PageHeader = CreateHeader(tagDic);
		pageLayout.PageFooter = CreateFooter(tagDic);
		e.PageLayout = pageLayout;
	}

	private void _pd_LongOperation(object sender, LongOperationEventArgs e)
	{
	}

	private RenderObject CreateHeader(Dictionary<string, string> tagDic)
	{
		return PreviewKind switch
		{
			PreviewKind.Table => TableHeaderImpl(tagDic), 
			PreviewKind.Image => ImageHeaderImpl(tagDic), 
			_ => throw new ArgumentOutOfRangeException("暂时只能打印表格或图片文件"), 
		};
	}

	private RenderObject CreateFooter(Dictionary<string, string> tagDic)
	{
		return CreatePageFooter(tagDic);
	}

	private RenderTable CreatePageHeader(Dictionary<string, string> tagDic)
	{
		RenderTable renderTable = new RenderTable();
		renderTable.Cells[0, 0].Text = "  ";
		renderTable.Cells[0, 4].Text = "  ";
		if (PageSetup.FitPageWidth)
		{
			renderTable.Width = Unit.Auto;
			renderTable.Cols[0].Width = new Unit(fitAdapterEmpty, UnitTypeEnum.Inch);
			renderTable.Cols[4].Width = new Unit(fitAdapterEmpty, UnitTypeEnum.Inch);
			Unit width = new Unit(fitAapterContent, UnitTypeEnum.Inch);
			renderTable.Cols[1].Width = width;
			renderTable.Cols[2].Width = width;
			renderTable.Cols[3].Width = width;
		}
		else
		{
			renderTable.Cols[0].Visible = false;
			renderTable.Cols[4].Visible = false;
		}
		int num = 0;
		RenderRichText renderRichText = RenderFactory.CreateRichText("left");
		renderRichText.Rtf = HeaderFooterDeal(PageSetup.PageHeader.LeftValue, tagDic, HorizontalAlignment.Left);
		renderRichText.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[num, 1].RenderObject = renderRichText;
		RenderRichText renderRichText2 = RenderFactory.CreateRichText("center");
		renderRichText2.Rtf = HeaderFooterDeal(PageSetup.PageHeader.CenterValue, tagDic, HorizontalAlignment.Center);
		renderRichText2.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[num, 2].RenderObject = renderRichText2;
		RenderRichText renderRichText3 = RenderFactory.CreateRichText("right");
		renderRichText3.Rtf = HeaderFooterDeal(PageSetup.PageHeader.RightValue, tagDic, HorizontalAlignment.Right);
		renderRichText3.Style.TextAlignVert = AlignVertEnum.Top;
		renderTable.Cells[num, 3].RenderObject = renderRichText3;
		num++;
		renderTable.Cells[num, 1].SpanCols = 3;
		TableRow tableRow = renderTable.Rows[0];
		double num2 = PageSetup.TopMargin - PageSetup.HeaderMargin;
		tableRow.Height = new Unit((num2 > 0.0) ? num2 : 0.0, UnitTypeEnum.Mm);
		return renderTable;
	}

	private RenderObject CreatePageFooter(Dictionary<string, string> tagDic)
	{
		RenderTable renderTable = new RenderTable();
		int index = 0;
		renderTable.Cells[0, 0].SpanCols = 3;
		TableRow tableRow = renderTable.Rows[index];
		double num = PageSetup.BottomMargin - PageSetup.FooterMargin;
		tableRow.Height = new Unit((num > 0.0) ? num : 0.0, UnitTypeEnum.Mm);
		index = 1;
		RenderRichText renderRichText = RenderFactory.CreateRichText("left");
		renderRichText.Rtf = HeaderFooterDeal(PageSetup.PageFooter.LeftValue, tagDic, HorizontalAlignment.Left);
		renderRichText.Style.TextAlignVert = AlignVertEnum.Bottom;
		renderTable.Cells[index, 0].RenderObject = renderRichText;
		RenderRichText renderRichText2 = RenderFactory.CreateRichText("center");
		renderRichText2.Rtf = HeaderFooterDeal(PageSetup.PageFooter.CenterValue, tagDic, HorizontalAlignment.Center);
		renderRichText2.Style.TextAlignVert = AlignVertEnum.Bottom;
		renderTable.Cells[index, 1].RenderObject = renderRichText2;
		RenderRichText renderRichText3 = RenderFactory.CreateRichText("right");
		renderRichText3.Rtf = HeaderFooterDeal(PageSetup.PageFooter.RightValue, tagDic, HorizontalAlignment.Right);
		renderRichText3.Style.TextAlignVert = AlignVertEnum.Bottom;
		renderTable.Cells[index, 2].RenderObject = renderRichText3;
		return renderTable;
	}

	private void SetPageLayout()
	{
		_pd.PageLayout.PageSettings.Landscape = PageSetup.Direction == Direction.Horizontal;
		_pd.PageLayout.PageSettings.LeftMargin = new Unit(PageSetup.LeftMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.RightMargin = new Unit(PageSetup.RightMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.TopMargin = new Unit(PageSetup.HeaderMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.BottomMargin = new Unit(PageSetup.FooterMargin, UnitTypeEnum.Mm);
		_pd.PageLayout.PageSettings.PaperKind = PageSetup.PaperKind;
		_pd.PageLayout.PageSettings.Color = !PageSetup.OneColor;
	}

	private double GetAvaiableWidth()
	{
		double num = 1.0;
		if (_pd.IsGenerating)
		{
			num = _pd.AvailableBlockFlowWidth;
		}
		else
		{
			_pd.StartDoc();
			num = _pd.AvailableBlockFlowWidth;
			_pd.EndDoc();
		}
		return num;
	}

	private double GetAvaiableHeight()
	{
		double num = 1.0;
		if (_pd.IsGenerating)
		{
			num = _pd.AvailableBlockFlowHeight;
		}
		else
		{
			_pd.StartDoc();
			num = _pd.AvailableBlockFlowHeight;
			_pd.EndDoc();
		}
		return num;
	}

	private int[] RangeParse(string pageRange)
	{
		return (from t in pageRange
			where (t >= '0' && t <= '9') || t == '-'
			select (t >= '0' && t <= '9') ? Convert.ToInt32(t.ToString()) : (-1)).ToArray();
	}

	private float MeasureString(string str, Font font)
	{
		return ScreenGraphics.MeasureString(str, font).Width;
	}

	private void CellAlignParse(CellTextAlign align, out AlignHorzEnum alignHorz, out AlignVertEnum alignVert)
	{
		alignVert = AlignVertEnum.Center;
		alignHorz = AlignHorzEnum.Left;
		switch (align)
		{
		case CellTextAlign.BottomCenter:
			alignHorz = AlignHorzEnum.Center;
			alignVert = AlignVertEnum.Bottom;
			break;
		case CellTextAlign.MiddleCenter:
			alignHorz = AlignHorzEnum.Center;
			alignVert = AlignVertEnum.Center;
			break;
		case CellTextAlign.TopCenter:
			alignHorz = AlignHorzEnum.Center;
			alignVert = AlignVertEnum.Top;
			break;
		case CellTextAlign.BottomLeft:
			alignHorz = AlignHorzEnum.Left;
			alignVert = AlignVertEnum.Bottom;
			break;
		case CellTextAlign.MiddleLeft:
			alignHorz = AlignHorzEnum.Left;
			alignVert = AlignVertEnum.Center;
			break;
		case CellTextAlign.TopLeft:
			alignHorz = AlignHorzEnum.Left;
			alignVert = AlignVertEnum.Top;
			break;
		case CellTextAlign.BottomRight:
			alignHorz = AlignHorzEnum.Right;
			alignVert = AlignVertEnum.Bottom;
			break;
		case CellTextAlign.MiddleRight:
			alignHorz = AlignHorzEnum.Right;
			alignVert = AlignVertEnum.Center;
			break;
		case CellTextAlign.TopRight:
			alignHorz = AlignHorzEnum.Right;
			alignVert = AlignVertEnum.Top;
			break;
		}
	}

	private string OneColorDeal(string Rtf)
	{
		if (!PageSetup.OneColor)
		{
			return Rtf;
		}
		return RTFHelper.MakeBlackWhite(Rtf);
	}

	private Color DisplayForeColor(Color color)
	{
		if (!PageSetup.OneColor)
		{
			return color;
		}
		return Color.Black;
	}

	private Color DisplayBackColor(Color color)
	{
		if (!PageSetup.OneColor)
		{
			return color;
		}
		return Color.White;
	}

	private string HeaderFooterDeal(string str, Dictionary<string, string> tagDic, HorizontalAlignment align)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return str;
		}
		string rtf = PageVariableReplace(str, tagDic);
		rtf = OneColorDeal(rtf);
		return RTFHelper.SystemVariable(CurrentProject.DataReferenceManager, rtf, new DataReferenceEvaluationContext
		{
			CurrentTreeNode = CurrentTreeNode,
			Project = CurrentProject
		});
	}

	private string PageVariableReplace(string str, Dictionary<string, string> tagDic)
	{
		foreach (KeyValuePair<string, string> item in tagDic)
		{
			str = str?.Replace(item.Key, item.Value);
		}
		return str;
	}

	private Unit ConvertPixelNumberToUnit(double pixel, UnitTypeEnum unitType)
	{
		return new Unit(pixel, UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiY, unitType, 0f);
	}

	private void TableCreatePaper()
	{
		Table.TagPageSetupDirty();
		TableConfigFitWidth();
		SetPageLayout();
		TableCreatePaperImpl();
	}

	private void TableCreatePaperImpl()
	{
		_pd.StartDoc();
		double horizontalZoom = PageSetup.HorizontalZoom;
		double verticalZoom = PageSetup.VerticalZoom;
		double num = new Unit(Table.Columns.Sum((Leqisoft.Model.Column col) => col.Visible ? col.Width : 0), UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
		double num2 = (PageSetup.IsPrintIndex ? MeasureIndex() : 0.0);
		double value = (PageSetup.FitPageWidth ? _pd.AvailableBlockFlowWidth : (num + num2));
		PageSetup.HorizontalZoom = (PageSetup.FitPageWidth ? TempHorzZoom() : horizontalZoom);
		if (PageSetup.FitPageHeight)
		{
			double value2 = MeasureTableBorderHeight(Table);
			double num3 = 0.0;
			RenderRichText renderRichText = new RenderRichText(Table.Note);
			if (renderRichText.TextLength > 0)
			{
				_pd.Body.Children.Add(renderRichText);
				num3 = renderRichText.CalcSize(new Unit(value, UnitTypeEnum.Inch), Unit.Auto).Height;
			}
			double num4 = new Unit(Table.Rows.Sum((Leqisoft.Model.Row row) => row.Height) + Table.SumHeaderHeight(Table.GetNumCaptionRows()), UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiY, UnitTypeEnum.Inch, 0f);
			double num5 = new Unit(Table.Foot.Rows.Sum((TableTitleRow r) => r.Height), UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiY, UnitTypeEnum.Inch, 0f);
			double num6 = new Unit(value2, UnitTypeEnum.Point).ConvertUnit(UnitTypeEnum.Inch);
			double verticalZoom2 = (_pd.AvailableBlockFlowHeight - num3 - num5 - num6) / num4;
			PageSetup.VerticalZoom = verticalZoom2;
		}
		DataTable = new RenderTable();
		DataTable.Style.FlowAlign = FlowAlignEnum.Center;
		CreateMainTable();
		if (layers > 0)
		{
			DataTable.RowGroups[0, layers].PageHeader = true;
		}
		if (PageSetup.FixedPrintColsNum > 0)
		{
			DataTable.ColGroups[0, PageSetup.FixedPrintColsNum].PageHeader = true;
		}
		_pd.RenderBlock(DataTable);
		PageSetup.HorizontalZoom = horizontalZoom;
		PageSetup.VerticalZoom = verticalZoom;
		_pd.EndDoc();
		View.Document = _pd;
	}

	private RenderObject CreateMainTitle()
	{
		RenderText renderText = new RenderText();
		renderText.Text = (string)Table.Title.TitleCell.Value;
		UpdateStyleFromTitle(renderText.Style, Table.Title.TitleCell);
		renderText.Style.TextAlignHorz = AlignHorzEnum.Center;
		renderText.Style.TextAlignVert = AlignVertEnum.Center;
		renderText.Height = ConvertPixelNumberToUnit(Table.Title.TitleHeight, UnitTypeEnum.Inch);
		return renderText;
	}

	private RenderObject CreateSubTitle()
	{
		double avaiableWidth = GetAvaiableWidth();
		RenderTable renderTable = new RenderTable();
		if (Table.Title.Rows.Any((TableTitleRow r) => !string.IsNullOrWhiteSpace((string)r.Center.Value)))
		{
			renderTable.Cols[1].SizingMode = TableSizingModeEnum.Auto;
			for (int i = 0; i < Table.Title.Rows.Count; i++)
			{
				renderTable.Rows[i].Height = ConvertPixelNumberToUnit(Table.Title.Rows[i].Height, UnitTypeEnum.Inch);
				int col = 0;
				TableTitleCell left = Table.Title.Rows[i].Left;
				renderTable.Cells[i, col].Text = (string)left.Value;
				UpdateStyleFromTitle(renderTable.Cells[i, col].Style, left);
				renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Left;
				renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
				col = 1;
				TableTitleCell center = Table.Title.Rows[i].Center;
				renderTable.Cells[i, col].Text = (string)center.Value;
				UpdateStyleFromTitle(renderTable.Cells[i, col].Style, center);
				renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Center;
				renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
				col = 2;
				TableTitleCell right = Table.Title.Rows[i].Right;
				renderTable.Cells[i, col].Text = (string)right.Value;
				UpdateStyleFromTitle(renderTable.Cells[i, col].Style, right);
				renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Right;
				renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
			}
		}
		else
		{
			if (Table.Title.Rows.Count > 0)
			{
				int num = Table.Title.Rows.Select((TableTitleRow r) => (string)r.Left.Value).Max((string t) => t.Length);
				int num2 = Table.Title.Rows.Select((TableTitleRow r) => (string)r.Right.Value).Max((string t) => t.Length);
				if (num2 < 18)
				{
					renderTable.Cols[1].SizingMode = TableSizingModeEnum.Auto;
				}
				else if (num < 18)
				{
					renderTable.Cols[0].SizingMode = TableSizingModeEnum.Auto;
				}
			}
			for (int j = 0; j < Table.Title.Rows.Count; j++)
			{
				renderTable.Rows[j].Height = ConvertPixelNumberToUnit(Table.Title.Rows[j].Height, UnitTypeEnum.Inch);
				int col2 = 0;
				TableTitleCell left2 = Table.Title.Rows[j].Left;
				renderTable.Cells[j, col2].Text = (string)left2.Value;
				UpdateStyleFromTitle(renderTable.Cells[j, col2].Style, left2);
				renderTable.Cells[j, col2].Style.TextAlignHorz = AlignHorzEnum.Left;
				renderTable.Cells[j, col2].Style.TextAlignVert = AlignVertEnum.Center;
				col2 = 1;
				TableTitleCell right2 = Table.Title.Rows[j].Right;
				renderTable.Cells[j, col2].Text = (string)right2.Value;
				UpdateStyleFromTitle(renderTable.Cells[j, col2].Style, right2);
				renderTable.Cells[j, col2].Style.TextAlignHorz = AlignHorzEnum.Right;
				renderTable.Cells[j, col2].Style.TextAlignVert = AlignVertEnum.Center;
			}
		}
		return renderTable;
	}

	private void CreateMainTable()
	{
		DataTable.Width = Unit.Auto;
		DataTable.Height = Unit.Auto;
		DataTable.Style.GridLines.All = LineDef.Default;
		DataTable.CellStyle.TextIndent = new Unit(1.0, UnitTypeEnum.Point);
		MainTableDataImpl(Table.Columns.Count);
		MainTableSettupImpl();
	}

	private RenderObject MainTableDataImpl(int printNum)
	{
		int PrintNum = printNum;
		RenderTable result = new RenderTable();
		if (PrintNum <= 0)
		{
			return null;
		}
		layers = 0;
		if (Table.HeaderMode == TableHeaderMode.Custom)
		{
			CreateCaption();
			MergeCaption();
		}
		CreateContent();
		MergeContent();
		return result;
		void CreateCaption()
		{
			Dictionary<int, List<string>> dictionary = Table.Columns.ToDictionary((Leqisoft.Model.Column t) => t.Index, (Leqisoft.Model.Column t) => t.CaptionDisplay.Split('_').ToList());
			layers = ((dictionary.Count != 0) ? dictionary.Max((KeyValuePair<int, List<string>> t) => t.Value.Count) : 0);
			for (int l = 0; l < layers; l++)
			{
				double pixel = (double)Table.GetHeaderHeight(l) * PageSetup.VerticalZoom;
				DataTable.Rows[l].Height = ConvertPixelNumberToUnit(pixel, UnitTypeEnum.Inch);
				for (int m = 0; m < PrintNum; m++)
				{
					int num2 = (Table.Columns[m].Visible ? Table.Columns[m].Width : 0);
					DataTable.Cols[m].Width = ConvertPixelNumberToUnit((double)num2 * PageSetup.HorizontalZoom, UnitTypeEnum.Inch);
					RenderText renderText4 = new RenderText
					{
						Text = ((l >= dictionary[m].Count) ? string.Empty : dictionary[m][l].ToString())
					};
					UpdateStyleFromCellStyle(renderText4.Style, Table.Columns[m].CaptionStyle);
					DataTable.Cells[l, m].RenderObject = renderText4;
				}
			}
		}
		void CreateContent()
		{
			for (int i = 0; i < Table.Rows.Count; i++)
			{
				Leqisoft.Model.Row row = Table.Rows[i];
				Unit unit = new Unit((double)row.Height * PageSetup.VerticalZoom, UnitTypeEnum.Pixel);
				DataTable.Rows[i + layers].Height = unit.ConvertUnit(ScreenGraphics.DpiY, UnitTypeEnum.Inch, 0f);
				for (int j = 0; j < PrintNum; j++)
				{
					Leqisoft.Model.Cell cell2 = Table.Cells.Get(i, j);
					RenderObject renderObject2 = CreateRenderCell(cell2);
					DataTable.Cells[i + layers, j].RenderObject = renderObject2;
				}
			}
			for (int k = 0; k < PrintNum; k++)
			{
				int num = (Table.Columns[k].Visible ? Table.Columns[k].Width : 0);
				DataTable.Cols[k].Width = ConvertPixelNumberToUnit((double)num * PageSetup.HorizontalZoom, UnitTypeEnum.Inch);
			}
		}
		RenderObject CreateRenderCell(Leqisoft.Model.Cell cell)
		{
			RenderObject renderObject = null;
			if (cell.Value is bool)
			{
				DataFormatType formatType = cell.DisplayFormat.FormatType;
				if ((uint)(formatType - 8) <= 2u)
				{
					renderObject = new RenderText();
					RenderText renderText = (RenderText)renderObject;
					Dictionary<bool, string> formatDictForBool = cell.DisplayFormat.GetFormatDictForBool();
					renderText.Text = formatDictForBool[bool.Parse(cell.Value.ToString())];
				}
				else
				{
					renderObject = new RenderText();
					RenderText renderText2 = (RenderText)renderObject;
					renderText2.Text = (bool.Parse(cell.Value.ToString()) ? "√" : "");
				}
			}
			else
			{
				renderObject = new RenderText();
				RenderText renderText3 = (RenderText)renderObject;
				renderText3.Text = cell.GetDisplayValue();
			}
			UpdateStyleFromCell(renderObject.Style, cell);
			return renderObject;
		}
		void MergeCaption()
		{
			List<CellRange> mergeInfo = Table.GetMergeInfo(visibleOnly: false);
			foreach (CellRange item in mergeInfo)
			{
				if (item.c1 < printNum && item.c2 < printNum)
				{
					CustomMerge(item.r1, item.c1, item.r2, item.c2);
				}
				else if (item.c1 < item.c2 && item.c1 < printNum)
				{
					CustomMerge(item.r1, item.c1, item.r2, printNum - 1);
				}
				else if (item.c2 < item.c1 && item.c2 < printNum)
				{
					CustomMerge(item.r1, item.c2, item.r2, printNum - 1);
				}
			}
		}
		void MergeContent()
		{
			foreach (CellMerge mergedCell in Table.MergedCells)
			{
				CustomMerge(mergedCell.TopLeft.Row.Index + layers, mergedCell.TopLeft.Column.Index, mergedCell.BottomRight.Row.Index + layers, mergedCell.BottomRight.Column.Index);
			}
		}
	}

	private RenderObject MainTableSettupImpl()
	{
		DataTable.SplitHorzBehavior = SplitBehaviorEnum.SplitIfNeeded;
		PageSetup.IsPrintIndex = false;
		if (PageSetup.IsPrintIndex)
		{
			SetIndex();
		}
		if (PageSetup.HasNoteBorder)
		{
			RichTextBox richTextBox = new RichTextBox
			{
				Rtf = Table.Note
			};
			if (richTextBox.TextLength > 0 && Table.Columns.Count > 0)
			{
				SetNote();
			}
			SetHideCols();
			SetTableStyle(Table.BorderStyle, DataTable, layers);
		}
		else
		{
			SetHideCols();
			SetTableStyle(Table.BorderStyle, DataTable, layers);
			RichTextBox richTextBox2 = new RichTextBox
			{
				Rtf = Table.Note
			};
			if (richTextBox2.TextLength > 0 && Table.Columns.Count > 0)
			{
				SetNote();
				DataTable.Rows[DataTable.Rows.Count - 1].Style.Borders.Bottom = new LineDef("0pt", Color.Black);
				DataTable.Cells[DataTable.Rows.Count - 1, 0].Style.Borders.Left = new LineDef("0pt", Color.Black);
				DataTable.Cells[DataTable.Rows.Count - 1, 0].Style.Borders.Right = new LineDef("0pt", Color.Black);
			}
		}
		if (Table.Foot != null && Table.Foot.Rows.Count > 0)
		{
			SetTableFoot();
			DataTable.Rows[DataTable.Rows.Count - 1].Style.Borders.Bottom = new LineDef("0pt", Color.Black);
			DataTable.Cells[DataTable.Rows.Count - 1, 0].Style.Borders.Left = new LineDef("0pt", Color.Black);
			DataTable.Cells[DataTable.Rows.Count - 1, 0].Style.Borders.Right = new LineDef("0pt", Color.Black);
		}
		return DataTable;
		void SetHideCols()
		{
			int num = (PageSetup.IsPrintIndex ? 1 : 0);
			IEnumerable<int> source = from c in Table.Columns
				where !c.Visible
				select c into t
				select t.Index;
			foreach (int item in source.OrderByDescending((int t) => t))
			{
				DataTable.Cols.Delete(item + num, 1);
			}
		}
		void SetIndex()
		{
			DataTable.Cols.Insert(0, 1);
			Font defaultFont = GetDefaultFont();
			float num2 = MeasureString("序号", defaultFont);
			float num3 = MeasureString(Table.Rows.Count.ToString(), defaultFont);
			Unit unit = new Unit((num3 > num2) ? num3 : num2, UnitTypeEnum.Pixel);
			DataTable.Cols[0].Width = unit.ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
			DataTable.Cols[0].Style.Font = defaultFont;
			DataTable.Cols[0].Style.TextAlignHorz = AlignHorzEnum.Center;
			DataTable.Cols[0].Style.TextAlignVert = AlignVertEnum.Center;
			DataTable.Cells[0, 0].SpanRows = ((layers < 1) ? 1 : layers);
			DataTable.Cells[0, 0].Text = "序号";
			foreach (TableRow item2 in (IEnumerable)DataTable.Rows)
			{
				if (item2.Ordinal >= layers)
				{
					item2[0].Text = (item2.Ordinal - layers + 1).ToString();
					item2[0].Style.TextAlignVert = AlignVertEnum.Center;
					item2[0].Style.TextAlignHorz = AlignHorzEnum.Center;
				}
			}
		}
		void SetNote()
		{
			RenderRichText renderRichText = RenderFactory.CreateRichText("default");
			renderRichText.Rtf = OneColorDeal(Table.Note);
			DataTable.Cells[DataTable.Rows.Count, 0].SpanCols = DataTable.Cols.Count + (PageSetup.IsPrintIndex ? 1 : 0);
			DataTable.Cells[DataTable.Rows.Count - 1, 0].RenderObject = renderRichText;
			DataTable.Rows[DataTable.Rows.Count - 1].Height = renderRichText.Height;
		}
		void SetTableFoot()
		{
			if (Table.Foot != null)
			{
				RenderTable renderTable = new RenderTable();
				renderTable.Style.GridLines.All = new LineDef("0pt", Color.Black);
				for (int i = 0; i < Table.Foot.Rows.Count; i++)
				{
					renderTable.Rows[i].Height = ConvertPixelNumberToUnit(Table.Foot.Rows[i].Height, UnitTypeEnum.Inch);
					int col = 0;
					TableTitleCell left = Table.Foot.Rows[i].Left;
					renderTable.Cells[i, col].Text = (string)left.Value;
					UpdateStyleFromTitle(renderTable.Cells[i, col].Style, left);
					renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Left;
					renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
					col = 1;
					TableTitleCell center = Table.Foot.Rows[i].Center;
					renderTable.Cells[i, col].Text = (string)center.Value;
					UpdateStyleFromTitle(renderTable.Cells[i, col].Style, center);
					renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Center;
					renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
					col = 2;
					TableTitleCell right = Table.Foot.Rows[i].Right;
					renderTable.Cells[i, col].Text = (string)right.Value;
					UpdateStyleFromTitle(renderTable.Cells[i, col].Style, right);
					renderTable.Cells[i, col].Style.TextAlignHorz = AlignHorzEnum.Right;
					renderTable.Cells[i, col].Style.TextAlignVert = AlignVertEnum.Center;
				}
				DataTable.Cells[DataTable.Rows.Count, 0].SpanCols = DataTable.Cols.Count + (PageSetup.IsPrintIndex ? 1 : 0);
				DataTable.Cells[DataTable.Rows.Count - 1, 0].RenderObject = renderTable;
			}
		}
	}

	private RenderObject TableHeaderImpl(Dictionary<string, string> tagDic)
	{
		RenderTable renderTable = CreatePageHeader(tagDic);
		int num = renderTable.Rows.Count - 1;
		num++;
		RenderObject renderObject = CreateMainTitle();
		renderTable.Cells[num, 1].SpanCols = 3;
		renderTable.Cells[num, 1].RenderObject = renderObject;
		num++;
		RenderObject renderObject2 = CreateSubTitle();
		renderTable.Cells[num, 1].SpanCols = 3;
		renderTable.Cells[num, 1].RenderObject = renderObject2;
		return renderTable;
	}

	private void TableConfigFitWidth()
	{
		if (PageSetup.FitPageWidth)
		{
			Unit unit = MeasureTable();
			double num = TempHorzZoom();
			double avaiableWidth = GetAvaiableWidth();
			double num2 = MeasureBorders();
			double num3 = FinalIndex();
			double num4 = 1.0 + (double)(Table.Columns.Where((Leqisoft.Model.Column c) => c.Visible).Count() / 7 + 1) / 10.0;
			fitAdapterEmpty = (avaiableWidth - (unit.Value * num + num3 + num2)) / 2.0 * ((num4 > 5.0) ? 5.0 : num4);
			fitAapterContent = unit.Value / 3.0 * num;
		}
		else
		{
			fitAdapterEmpty = 0.0;
		}
	}

	private void SubTotalImpl(int FixCols)
	{
		Dictionary<Tuple<int, int>, int> dictionary = new Dictionary<Tuple<int, int>, int>();
		if (Table.SubTotal.GroupColumns.Count <= 0)
		{
			return;
		}
		int num = Table.SubTotal.GroupColumns.Min((Id64 t) => Table.Columns.GetById(t).Index);
		int count = Table.SubTotal.GroupColumns.Count;
		int num2 = 0;
		Dictionary<Tuple<int, int, Id64>, double> dictionary2 = Table.SubTotal.Apply();
		foreach (KeyValuePair<Tuple<int, int, Id64>, double> item in dictionary2)
		{
			if (dictionary.ContainsKey(Tuple.Create(item.Key.Item1, item.Key.Item2)))
			{
				num2 = dictionary[Tuple.Create(item.Key.Item1, item.Key.Item2)];
			}
			else
			{
				if (Table.SubTotal.Direction == DirectionEnum.Top)
				{
					num2 = item.Key.Item1 + FixCols + dictionary.Count;
					if (num2 > 0)
					{
						DataTable.Rows.Insert(num2, 1);
						DataTable.Rows[num2].Height = DataTable.Rows[num2 + 1].Height;
					}
				}
				else if (Table.SubTotal.Direction == DirectionEnum.Bottom)
				{
					num2 = item.Key.Item2 + FixCols + 1 + dictionary.Count;
					if (num2 < DataTable.Rows.Count)
					{
						DataTable.Rows.Insert(num2, 1);
					}
					DataTable.Rows[num2].Height = DataTable.Rows[num2 - 1].Height;
				}
				dictionary.Add(Tuple.Create(item.Key.Item1, item.Key.Item2), num2);
				DataTable.Cells[num2, num + 1].SpanCols = count;
				DataTable.Rows[num2][num + 1].Text = Table.SubTotal.TotalName;
				DataTable.Rows[num2][num + 1].Style.TextAlignVert = AlignVertEnum.Center;
				DataTable.Rows[num2][num + 1].Style.TextAlignHorz = AlignHorzEnum.Center;
			}
			DataTable.Rows[num2][Table.Columns.GetById(item.Key.Item3).Index + 1].Text = item.Value.ToString();
			DataTable.Rows[num2][Table.Columns.GetById(item.Key.Item3).Index + 1].Style.TextAlignVert = AlignVertEnum.Center;
		}
	}

	private double FinalIndex()
	{
		if (!PageSetup.IsPrintIndex)
		{
			return 0.0;
		}
		return MeasureIndex();
	}

	private Font GetDefaultFont()
	{
		return new Font(Table.DefaultStyle.FontFamily, Table.DefaultStyle.FontSize.Value);
	}

	private double TempHorzZoom()
	{
		double num = new Unit(Table.Columns.Sum((Leqisoft.Model.Column col) => col.Visible ? col.Width : 0), UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
		double num2 = (PageSetup.IsPrintIndex ? MeasureIndex() : 0.0);
		double avaiableWidth = GetAvaiableWidth();
		double num3 = MeasureBorders();
		double num4 = (avaiableWidth - num2 - num3) / num;
		return num4 * 0.97;
	}

	private void CustomMerge(int r1, int c1, int r2, int c2)
	{
		DataTable.Cells[r1, c1].SpanCols = ((c2 - c1 <= 0) ? 1 : (c2 - c1 + 1));
		DataTable.Cells[r1, c1].SpanRows = ((r2 - r1 <= 0) ? 1 : (r2 - r1 + 1));
	}

	private Unit MeasureTable()
	{
		return new Unit(Table.Columns.Sum((Leqisoft.Model.Column col) => col.Visible ? col.Width : 0), UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
	}

	private double MeasureIndex()
	{
		float num = MeasureString("序号", GetDefaultFont());
		float num2 = MeasureString(Table.Rows.Count.ToString(), GetDefaultFont());
		float num3 = ((num > num2) ? num : num2);
		return new Unit(num3, UnitTypeEnum.Pixel).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
	}

	private double MeasureBorders()
	{
		double value = MeasureTableBorderWidth(Table);
		return new Unit(value, UnitTypeEnum.Point).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Inch, 0f);
	}

	private double MeasureTableBorderWidth(Leqisoft.Model.Table table)
	{
		double num = 0.0;
		double num2 = 1.3;
		TableBorderStyle borderStyle = table.BorderStyle;
		switch (borderStyle.LeftRightLine)
		{
		case LineStyle.Thick:
			num += 4.0 * num2;
			break;
		case LineStyle.Thin:
		case LineStyle.Dash:
			num += 2.0 * num2;
			break;
		}
		int num3 = table.Columns.Where((Leqisoft.Model.Column c) => c.Visible).Count() - 1;
		if (PageSetup.IsPrintIndex)
		{
			num3++;
		}
		switch (borderStyle.BodyLine)
		{
		case LineStyle.Thick:
			num += 2.0 * num2 * (double)num3;
			break;
		case LineStyle.Thin:
		case LineStyle.Dash:
			num += 1.0 * num2 * (double)num3;
			break;
		}
		return num;
	}

	private double MeasureTableBorderHeight(Leqisoft.Model.Table table)
	{
		double num = 0.0;
		double num2 = 1.4;
		TableBorderStyle borderStyle = table.BorderStyle;
		int num3 = table.Rows.Count - 2;
		if (!string.IsNullOrWhiteSpace(table.Note))
		{
			num3++;
		}
		switch (borderStyle.BodyLine)
		{
		case LineStyle.Thick:
			num += 2.0 * num2 * (double)num3;
			break;
		case LineStyle.Thin:
		case LineStyle.Dash:
			num += 1.0 * num2 * (double)num3;
			break;
		}
		switch (borderStyle.UpDownLine)
		{
		case LineStyle.Thick:
			num += 40.0 * num2;
			break;
		case LineStyle.Thin:
		case LineStyle.Dash:
			num += 2.0 * num2;
			break;
		}
		switch (borderStyle.SecondLine)
		{
		case LineStyle.Thick:
			num += 20.0 * num2;
			break;
		case LineStyle.Thin:
		case LineStyle.Dash:
			num += 1.0 * num2;
			break;
		}
		return num;
	}

	private void UpdateStyleFromTitle(Style renderStyle, TableTitleCell titleCell)
	{
		CellAlignParse(titleCell.Align, out var alignHorz, out var alignVert);
		renderStyle.GridLines.All = LineDef.Empty;
		renderStyle.TextAlignHorz = alignHorz;
		renderStyle.TextAlignVert = alignVert;
		renderStyle.FontBold = titleCell.Bold;
		renderStyle.FontItalic = titleCell.Italic;
		renderStyle.FontUnderline = titleCell.Underline;
		renderStyle.FontStrikeout = titleCell.Strikeout;
		renderStyle.TextColor = DisplayForeColor(titleCell.ForeColor);
		renderStyle.FontSize = titleCell.FontSize;
		renderStyle.FontName = titleCell.FontFamily;
	}

	private void UpdateStyleFromCell(Style renderStyle, Leqisoft.Model.Cell cell)
	{
		CellAlignParse(cell.DisplayAlign, out var alignHorz, out var alignVert);
		renderStyle.BackColor = DisplayBackColor(cell.DisplayBackColor);
		renderStyle.TextAlignHorz = alignHorz;
		renderStyle.TextAlignVert = alignVert;
		renderStyle.TextColor = DisplayForeColor(cell.DisplayForeColor);
		renderStyle.FontBold = cell.Style?.Bold ?? cell.DisplayBold;
		renderStyle.FontName = cell.Style?.FontFamily ?? cell.DisplayFontFamily;
		renderStyle.FontSize = Convert.ToSingle((double)cell.DisplayFontSize * DisplayZoom);
		if (alignHorz == AlignHorzEnum.Right)
		{
			renderStyle.Spacing = new Offsets
			{
				Right = new Unit(0.5, UnitTypeEnum.Mm)
			};
		}
		if (alignHorz == AlignHorzEnum.Left)
		{
			renderStyle.Spacing = new Offsets
			{
				Left = new Unit(0.5, UnitTypeEnum.Mm)
			};
		}
	}

	private void UpdateStyleFromCellStyle(Style renderStyle, Leqisoft.Model.CellStyle cellStyle)
	{
		renderStyle.FontBold = cellStyle.Bold.GetValueOrDefault();
		renderStyle.BackColor = DisplayBackColor(cellStyle.BackColor ?? Color.Transparent);
		renderStyle.TextColor = DisplayForeColor(cellStyle.ForeColor ?? Color.Transparent);
		renderStyle.FontName = cellStyle.FontFamily;
		CellAlignParse(cellStyle.Align.Value, out var alignHorz, out var alignVert);
		renderStyle.TextAlignHorz = alignHorz;
		renderStyle.TextAlignVert = alignVert;
		renderStyle.FontSize = (float)((double?)cellStyle.FontSize * DisplayZoom).Value;
	}

	public void SetTableStyle(TableBorderStyle tableStyle, RenderTable RenTable, int CaptionRows)
	{
		if (tableStyle != null)
		{
			Style style = RenTable.Rows[0].Style;
			Style style2 = RenTable.Rows[RenTable.Rows.Count - 1].Style;
			Style style3 = RenTable.Rows[CaptionRows - 1].Style;
			Style style4 = RenTable.Cols[0].Style;
			Style style5 = RenTable.Cols[RenTable.Cols.Count - 1].Style;
			switch (tableStyle.BodyLine)
			{
			case LineStyle.Thick:
				RenTable.Style.GridLines.All = new LineDef("2pt", Color.Black);
				break;
			case LineStyle.Thin:
				RenTable.Style.GridLines.All = new LineDef("1pt", Color.Black);
				break;
			case LineStyle.Dash:
				RenTable.Style.GridLines.All = new LineDef("1pt", Color.Black, DashStyle.Dot);
				break;
			case LineStyle.None:
				DataTable.Style.GridLines.All = new LineDef("0pt", Color.Black);
				break;
			}
			switch (tableStyle.UpDownLine)
			{
			case LineStyle.Thick:
				style.Borders.Top = new LineDef("2pt", Color.Black);
				style2.Borders.Bottom = new LineDef("2pt", Color.Black);
				break;
			case LineStyle.Thin:
				style.Borders.Top = new LineDef("1pt", Color.Black);
				style2.Borders.Bottom = new LineDef("1pt", Color.Black);
				break;
			case LineStyle.Dash:
				style.Borders.Top = new LineDef("1pt", Color.Black, DashStyle.Dot);
				style2.Borders.Bottom = new LineDef("1pt", Color.Black, DashStyle.Dot);
				break;
			case LineStyle.None:
				style.Borders.Top = new LineDef("0pt", Color.Black);
				style2.Borders.Bottom = new LineDef("0pt", Color.Black);
				break;
			}
			switch (tableStyle.SecondLine)
			{
			case LineStyle.Thick:
				style3.Borders.Bottom = new LineDef("2pt", Color.Black);
				break;
			case LineStyle.Thin:
				style3.Borders.Bottom = new LineDef("1pt", Color.Black);
				break;
			case LineStyle.Dash:
				style3.Borders.Bottom = new LineDef("1pt", Color.Black, DashStyle.Dot);
				break;
			case LineStyle.None:
				style3.Borders.Bottom = new LineDef("0pt", Color.Black);
				break;
			}
			switch (tableStyle.LeftRightLine)
			{
			case LineStyle.Thick:
				style4.Borders.Left = new LineDef("2pt", Color.Black);
				style5.Borders.Right = new LineDef("2pt", Color.Black);
				break;
			case LineStyle.Thin:
				style4.Borders.Left = new LineDef("1pt", Color.Black);
				style5.Borders.Right = new LineDef("1pt", Color.Black);
				break;
			case LineStyle.Dash:
				style4.Borders.Left = new LineDef("1pt", Color.Black, DashStyle.Dot);
				style5.Borders.Right = new LineDef("1pt", Color.Black, DashStyle.Dot);
				break;
			case LineStyle.None:
				style4.Borders.Left = new LineDef("0pt", Color.Black);
				style5.Borders.Right = new LineDef("0pt", Color.Black);
				break;
			}
		}
	}

	private RenderObject ImageHeaderImpl(Dictionary<string, string> tagDic)
	{
		return CreatePageHeader(tagDic);
	}

	private void ImageConfigFitWidth()
	{
		fitAdapterEmpty = 0.0;
		fitAapterContent = GetAvaiableWidth() / 3.0;
	}

	private void ImageCreatePaperImpl()
	{
		WidthLock = false;
		HeightLock = false;
		System.Drawing.Image graphicsImage = Image.GetGraphicsImage();
		double wdt2 = graphicsImage.Width;
		double hgt2 = graphicsImage.Height;
		double pageWidth = new Unit(GetAvaiableWidth(), UnitTypeEnum.Inch).ConvertUnit(ScreenGraphics.DpiX, UnitTypeEnum.Pixel, ScreenGraphics.DpiX);
		double pageHeight = new Unit(GetAvaiableHeight(), UnitTypeEnum.Inch).ConvertUnit(ScreenGraphics.DpiY, UnitTypeEnum.Pixel, ScreenGraphics.DpiY);
		double hvzoom = wdt2 / hgt2;
		if (wdt2 > pageWidth || hgt2 > pageHeight)
		{
			GetMaxSize(out wdt2, out hgt2);
			WidthLock = wdt2 == pageWidth;
			HeightLock = hgt2 == pageHeight;
		}
		if (PageSetup.FitPageWidth && PageSetup.FitPageHeight)
		{
			wdt2 = pageWidth;
			hgt2 = pageHeight;
		}
		else if (PageSetup.FitPageWidth || PageSetup.FitPageHeight)
		{
			GetMaxSize(out wdt2, out hgt2);
		}
		else
		{
			wdt2 *= PageSetup.HorizontalZoom;
			hgt2 *= PageSetup.VerticalZoom;
			wdt2 = ((wdt2 < pageWidth) ? wdt2 : pageWidth);
			hgt2 = ((hgt2 < pageHeight) ? hgt2 : pageHeight);
		}
		wdt2 = ((wdt2 > 1.0) ? wdt2 : 1.0);
		hgt2 = ((hgt2 > 1.0) ? hgt2 : 1.0);
		Bitmap image = new Bitmap((int)wdt2, (int)hgt2);
		using (Graphics graphics = Graphics.FromImage(image))
		{
			graphics.DrawImage(graphicsImage, 0f, 0f, (float)wdt2, (float)hgt2);
		}
		RenderImage renderImage = new RenderImage(image);
		renderImage.Style.FlowAlign = FlowAlignEnum.Center;
		_pd.StartDoc();
		_pd.RenderBlock(renderImage);
		_pd.EndDoc();
		View.Document = _pd;
		void GetMaxSize(out double wdt, out double hgt)
		{
			double num = pageWidth / hvzoom;
			if (num < pageHeight)
			{
				wdt = pageWidth;
				hgt = wdt / hvzoom;
			}
			else
			{
				hgt = pageHeight;
				wdt = hgt * hvzoom;
			}
		}
	}

	private void ImageCreatePaper()
	{
		ImageConfigFitWidth();
		SetPageLayout();
		ImageCreatePaperImpl();
	}
}
