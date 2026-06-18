using System;
using System.Collections;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Leqisoft.UI.Platform;

public class C1DropDownControlEx : C1DropDownControl
{
	public Func<object, object> ConvertEditorInputValueToOwnerCotrolNeedValue;

	public Type EditorDataType { get; set; }

	public object EditorInitValue { get; set; }

	public override object C1EditorGetValue()
	{
		if (ConvertEditorInputValueToOwnerCotrolNeedValue == null)
		{
			return base.C1EditorGetValue();
		}
		return ConvertEditorInputValueToOwnerCotrolNeedValue(base.C1EditorGetValue());
	}

	public override void C1EditorInitialize(object value, IDictionary attrs)
	{
		object value2 = value;
		if (EditorDataType != null)
		{
			attrs["DataType"] = EditorDataType;
			if (EditorInitValue != null)
			{
				value2 = EditorInitValue;
			}
		}
		base.C1EditorInitialize(value2, attrs);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 770)
		{
			try
			{
				base.WndProc(ref m);
				return;
			}
			catch (ArgumentNullException)
			{
				return;
			}
			catch (Exception ex2)
			{
				if (ex2.Message == C1InputStrings.errTextPropertyIsReadonly)
				{
					return;
				}
				throw;
			}
		}
		base.WndProc(ref m);
	}
}
