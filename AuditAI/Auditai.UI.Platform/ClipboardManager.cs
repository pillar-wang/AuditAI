using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class ClipboardManager
{
	private TreeNodeBase _projectHierarchyNode;

	private List<ValidationFormula> _validationFormulas;

	public static ClipboardManager Instance { get; } = new ClipboardManager();


	public ProjectHierarchy.TreeGroupView ProjectHierarchyCopyedGroup { get; set; }

	public TreeNodeBase ProjectHierarchyNode
	{
		get
		{
			if (ClipboardContainsText())
			{
				return null;
			}
			if (Mode != ClipboardMode.ProjectHierarchyNode)
			{
				return null;
			}
			return _projectHierarchyNode;
		}
		set
		{
			Clear();
			_projectHierarchyNode = value;
			Mode = ClipboardMode.ProjectHierarchyNode;
		}
	}

	public List<ValidationFormula> ValidationFormulas
	{
		get
		{
			if (ClipboardContainsText())
			{
				return null;
			}
			if (Mode != ClipboardMode.ValidationFormulas)
			{
				return null;
			}
			return _validationFormulas;
		}
		set
		{
			Clear();
			_validationFormulas = value;
			Mode = ClipboardMode.ValidationFormulas;
		}
	}

	public ClipboardMode Mode { get; private set; }

	private ClipboardManager()
	{
	}

	public void Clear()
	{
		_projectHierarchyNode = null;
		_validationFormulas = null;
		Mode = ClipboardMode.None;
		try
		{
			Clipboard.Clear();
		}
		catch (ExternalException)
		{
		}
	}

	private bool ClipboardContainsText()
	{
		try
		{
			return Clipboard.ContainsText();
		}
		catch (ExternalException)
		{
			return false;
		}
	}
}
