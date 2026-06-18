using System;
using C1.C1Preview;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls;

public class C1PrintDocumentEx : C1PrintDocument
{
	public static AlignHorzEnum ToAlignHorz(CellTextAlign align)
	{
		switch (align)
		{
		case CellTextAlign.TopCenter:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.BottomCenter:
			return AlignHorzEnum.Center;
		case CellTextAlign.TopLeft:
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.BottomLeft:
			return AlignHorzEnum.Left;
		case CellTextAlign.TopRight:
		case CellTextAlign.MiddleRight:
		case CellTextAlign.BottomRight:
			return AlignHorzEnum.Right;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static AlignVertEnum ToAlignVert(CellTextAlign align)
	{
		switch (align)
		{
		case CellTextAlign.BottomLeft:
		case CellTextAlign.BottomCenter:
		case CellTextAlign.BottomRight:
			return AlignVertEnum.Bottom;
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.MiddleRight:
			return AlignVertEnum.Center;
		case CellTextAlign.TopLeft:
		case CellTextAlign.TopCenter:
		case CellTextAlign.TopRight:
			return AlignVertEnum.Top;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public Unit CalcWidth(RenderObject ro)
	{
		base.Body.Children.Add(ro);
		SizeD sizeD = ro.CalcSize(Unit.Auto, Unit.Auto);
		base.Body.Children.Remove(ro);
		return new Unit(sizeD.Width, base.ResolvedUnit);
	}

	public Unit CalcHeight(RenderObject ro)
	{
		base.Body.Children.Add(ro);
		SizeD sizeD = ro.CalcSize(Unit.Auto, Unit.Auto);
		base.Body.Children.Remove(ro);
		return new Unit(sizeD.Height, base.ResolvedUnit);
	}

	public Unit UnitAdd(Unit lhs, Unit rhs)
	{
		double num = ConvertUnit(lhs, base.ResolvedUnit);
		double num2 = ConvertUnit(rhs, base.ResolvedUnit);
		return new Unit(num + num2, base.ResolvedUnit);
	}

	public Unit UnitSubtract(Unit lhs, Unit rhs)
	{
		double num = ConvertUnit(lhs, base.ResolvedUnit);
		double num2 = ConvertUnit(rhs, base.ResolvedUnit);
		return new Unit(num - num2, base.ResolvedUnit);
	}

	public bool UnitGreaterThan(Unit lhs, Unit rhs)
	{
		return UnitSubtract(lhs, rhs).Value > 0.0;
	}
}
