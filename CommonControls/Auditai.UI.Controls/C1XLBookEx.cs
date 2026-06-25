using System;
using System.Drawing;
using System.Drawing.Printing;
using C1.C1Excel;
using C1.Win.C1FlexGrid;
using Auditai.Model;

namespace Auditai.UI.Controls;

public class C1XLBookEx : C1XLBook
{
	public static XLAlignHorzEnum ToAlignHorz(CellTextAlign a)
	{
		switch (a)
		{
		case CellTextAlign.TopLeft:
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.BottomLeft:
			return XLAlignHorzEnum.Left;
		case CellTextAlign.TopCenter:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.BottomCenter:
			return XLAlignHorzEnum.Center;
		case CellTextAlign.TopRight:
		case CellTextAlign.MiddleRight:
		case CellTextAlign.BottomRight:
			return XLAlignHorzEnum.Right;
		default:
			return XLAlignHorzEnum.Undefined;
		}
	}

	public static XLAlignVertEnum ToAlignVert(CellTextAlign a)
	{
		switch (a)
		{
		case CellTextAlign.TopLeft:
		case CellTextAlign.TopCenter:
		case CellTextAlign.TopRight:
			return XLAlignVertEnum.Top;
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.MiddleRight:
			return XLAlignVertEnum.Center;
		case CellTextAlign.BottomLeft:
		case CellTextAlign.BottomCenter:
		case CellTextAlign.BottomRight:
			return XLAlignVertEnum.Bottom;
		default:
			return XLAlignVertEnum.Undefined;
		}
	}

	public static XLLineStyleEnum ToLineStyle(int width)
	{
		return width switch
		{
			0 => XLLineStyleEnum.None, 
			1 => XLLineStyleEnum.Thin, 
			2 => XLLineStyleEnum.Medium, 
			_ => XLLineStyleEnum.None, 
		};
	}

	public static TicketBorder ToTicketBorder(XLLineStyleEnum ls)
	{
		TicketBorder ticketBorder = new TicketBorder();
		TicketBorder ticketBorder2 = ticketBorder;
		ticketBorder2.Width = ls switch
		{
			XLLineStyleEnum.None => 0, 
			XLLineStyleEnum.Thin => 1, 
			XLLineStyleEnum.Medium => 2, 
			_ => 1, 
		};
		return ticketBorder;
	}

	public static int RowHeightToPixel(XLRow r)
	{
		if (r.Height == -1)
		{
			return C1XLBook.TwipsToPixels(r.Sheet.DefaultRowHeight);
		}
		return C1XLBook.TwipsToPixels(r.Height);
	}

	public static int ColumnWidthToPixel(XLColumn c)
	{
		if (c.Width == -1)
		{
			return C1XLBook.TwipsToPixels(c.Sheet.DefaultColumnWidth);
		}
		return C1XLBook.TwipsToPixels(c.Width);
	}

	public static Auditai.Model.CellStyle ToCellStyle(XLStyle s)
	{
		return new Auditai.Model.CellStyle
		{
			Id = Project.Current.GetNextId(),
			Align = ToAlign(s),
			ForeColor = s.ForeColor,
			BackColor = s.BackColor,
			FontSize = s.Font.Size,
			FontFamily = s.Font.FontFamily.Name,
			Bold = s.Font.Bold,
			Italic = s.Font.Italic,
			Underline = s.Font.Underline,
			Margin = C1XLBook.TwipsToPixels(s.Indent * 180)
		};
	}

	public static CellTextAlign ToAlign(XLStyle s)
	{
		XLAlignHorzEnum alignHorz = s.AlignHorz;
		XLAlignVertEnum alignVert = s.AlignVert;
		switch (alignHorz)
		{
		case XLAlignHorzEnum.Left:
			switch (alignVert)
			{
			case XLAlignVertEnum.Top:
				return CellTextAlign.TopLeft;
			case XLAlignVertEnum.Center:
				return CellTextAlign.MiddleLeft;
			case XLAlignVertEnum.Bottom:
				return CellTextAlign.BottomLeft;
			}
			break;
		case XLAlignHorzEnum.Center:
			switch (alignVert)
			{
			case XLAlignVertEnum.Top:
				return CellTextAlign.TopCenter;
			case XLAlignVertEnum.Center:
				return CellTextAlign.MiddleCenter;
			case XLAlignVertEnum.Bottom:
				return CellTextAlign.BottomCenter;
			}
			break;
		case XLAlignHorzEnum.Right:
			switch (alignVert)
			{
			case XLAlignVertEnum.Top:
				return CellTextAlign.TopRight;
			case XLAlignVertEnum.Center:
				return CellTextAlign.MiddleRight;
			case XLAlignVertEnum.Bottom:
				return CellTextAlign.BottomRight;
			}
			break;
		}
		return CellTextAlign.MiddleLeft;
	}

	public static void PopulateC1FlexGridCellStyle(XLStyle s, C1.Win.C1FlexGrid.CellStyle cs)
	{
		cs.BackColor = s.BackColor;
		cs.Font = s.Font;
		cs.ForeColor = s.ForeColor;
		cs.Margins = new Margins(s.Indent, 0, 0, 0);
		cs.TextAlign = C1FlexGridEx.ToTextAlign(ToAlign(s));
	}

	public void PopulateTicketDesignCellStyle(XLStyle s, TicketDesignCellVM c)
	{
		if (s == null)
		{
			c.Bold = base.DefaultFont.Bold;
			c.FontFamily = base.DefaultFont.Name;
			c.FontSize = base.DefaultFont.SizeInPoints;
			c.Italic = base.DefaultFont.Italic;
			return;
		}
		c.Align = ToAlign(s);
		c.BackColor = (Util.RgbEquals(s.BackColor, Color.White) ? Color.White : s.BackColor);
		c.Bold = s.Font.Bold;
		c.FontFamily = s.Font.Name;
		c.FontSize = s.Font.SizeInPoints;
		c.ForeColor = s.ForeColor;
		c.Indent = C1XLBook.TwipsToPixels(s.Indent * 180);
		c.Italic = s.Font.Italic;
		c.Top = ToTicketBorder(s.BorderTop);
		c.Right = ToTicketBorder(s.BorderRight);
		c.Bottom = ToTicketBorder(s.BorderBottom);
		c.Left = ToTicketBorder(s.BorderLeft);
	}

	public static int GetValidRowCount(XLSheet sheet)
	{
		if (sheet.Rows.Count == 0)
		{
			return 0;
		}
		int num = ((sheet.Columns.Count < 50) ? sheet.Columns.Count : 50);
		int num2 = sheet.Rows.Count - 1;
		while (num2 >= sheet.Rows.Count - 10 && num2 >= 0)
		{
			for (int i = 0; i < num; i++)
			{
				XLCell cell = sheet.GetCell(num2, i);
				if (cell != null && !string.IsNullOrEmpty(cell.Text))
				{
					return num2 + 1;
				}
			}
			num2--;
		}
		if (sheet.Rows.Count <= 10)
		{
			return 0;
		}
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < sheet.Rows.Count; j++)
		{
			for (int k = 0; k < num; k++)
			{
				XLCell cell2 = sheet.GetCell(j, k);
				if (cell2 != null && !string.IsNullOrEmpty(cell2.Text))
				{
					num4 += num3;
					num3 = 0;
					num4++;
					break;
				}
				if (k == num - 1)
				{
					num3++;
					if (num3 < 500)
					{
						break;
					}
					return num4;
				}
			}
		}
		return num4;
	}

	public static int GetValidColCount(XLSheet sheet)
	{
		if (sheet.Columns.Count == 0)
		{
			return 0;
		}
		int num = ((sheet.Rows.Count < 500) ? sheet.Rows.Count : 500);
		int num2 = sheet.Columns.Count - 1;
		while (num2 >= sheet.Columns.Count - 10 && num2 >= 0)
		{
			for (int i = 0; i < num; i++)
			{
				XLCell cell = sheet.GetCell(i, num2);
				if (cell != null && !string.IsNullOrEmpty(cell.Text))
				{
					return num2 + 1;
				}
			}
			num2--;
		}
		if (sheet.Columns.Count <= 10)
		{
			return 0;
		}
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < sheet.Columns.Count; j++)
		{
			for (int k = 0; k < num; k++)
			{
				XLCell cell2 = sheet.GetCell(k, j);
				if (cell2 != null && !string.IsNullOrEmpty(cell2.Text))
				{
					num4 += num3;
					num3 = 0;
					num4++;
					break;
				}
				if (k == num - 1)
				{
					num3++;
					if (num3 < 10)
					{
						break;
					}
					return num4;
				}
			}
		}
		return num4;
	}

	public static int PixelToIndent(int pixel)
	{
		return (int)Math.Round((double)C1XLBook.PixelsToTwips(pixel) / 180.0, 0);
	}
}
