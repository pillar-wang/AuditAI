using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Auditai.DTO;

namespace Auditai.Model;

public class TreeDirectoryNode : TreeNodeBase
{
	[CompilerGenerated]
	private sealed class _003CGetDescendants_003Ed__5 : IEnumerable<TreeNodeBase>, IEnumerable, IEnumerator<TreeNodeBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private TreeNodeBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public TreeDirectoryNode _003C_003E4__this;

		private List<TreeNodeBase>.Enumerator _003C_003E7__wrap1;

		private TreeNodeBase _003Cchild_003E5__3;

		private IEnumerator<TreeNodeBase> _003C_003E7__wrap3;

		TreeNodeBase IEnumerator<TreeNodeBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetDescendants_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<TreeNodeBase>.Enumerator);
			_003Cchild_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				TreeDirectoryNode treeDirectoryNode = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = treeDirectoryNode.Children.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					if (_003Cchild_003E5__3 is TreeDirectoryNode treeDirectoryNode2)
					{
						_003C_003E7__wrap3 = treeDirectoryNode2.GetDescendants().GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00d1;
					}
					goto IL_00eb;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00d1;
					}
					IL_00eb:
					_003Cchild_003E5__3 = null;
					break;
					IL_00d1:
					if (_003C_003E7__wrap3.MoveNext())
					{
						TreeNodeBase current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					goto IL_00eb;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Cchild_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<TreeNodeBase>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TreeNodeBase> IEnumerable<TreeNodeBase>.GetEnumerator()
		{
			_003CGetDescendants_003Ed__5 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetDescendants_003Ed__5(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TreeNodeBase>)this).GetEnumerator();
		}
	}

	public List<TreeNodeBase> Children { get; } = new List<TreeNodeBase>();


	public override List<TreeNodeBase> Siblings
	{
		get
		{
			if (!base.IsRoot)
			{
				return base.Siblings;
			}
			return base.Group.RootNodes;
		}
	}

	[IteratorStateMachine(typeof(_003CGetDescendants_003Ed__5))]
	public IEnumerable<TreeNodeBase> GetDescendants()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetDescendants_003Ed__5(-2)
		{
			_003C_003E4__this = this
		};
	}

	public TreeDirectoryNode InsertChildDirectory(int index)
	{
		TreeDirectoryNode treeDirectoryNode = new TreeDirectoryNode
		{
			Id = Project.Current.GetNextId()
		};
		InsertChildNode(treeDirectoryNode, index);
		treeDirectoryNode.Name = GetNewDirectoryName();
		return treeDirectoryNode;
	}

	public TreeTableNode InsertChildTable(int index, InitTableMode tableMode = InitTableMode.Default)
	{
		base.Project.ThrowIfMaxExceeded();
		TreeTableNode treeTableNode = new TreeTableNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewTableName()
		};
		InsertChildNode(treeTableNode, index);
		treeTableNode.Table.InitTableForCreate(tableMode);
		if (UserSet.Config.IsTitleFitTableName)
		{
			treeTableNode.Table.Title.TitleCell.Value = treeTableNode.Name;
		}
		return treeTableNode;
	}

	public TreeDocumentNode InsertChildDocument(int index)
	{
		base.Project.ThrowIfMaxExceeded();
		TreeDocumentNode treeDocumentNode = new TreeDocumentNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewDocumentName()
		};
		treeDocumentNode.Document._isLoaded = true;
		InsertChildNode(treeDocumentNode, index);
		return treeDocumentNode;
	}

	public TreeImageNode InsertChildImage(int index, Guid fileId)
	{
		base.Project.ThrowIfMaxExceeded();
		TreeImageNode treeImageNode = new TreeImageNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewImageName(),
			IsEntityDirty = true
		};
		treeImageNode.Image._isLoaded = true;
		treeImageNode.Image.FileId = fileId;
		treeImageNode.Image.NeedSave = true;
		InsertChildNode(treeImageNode, index);
		return treeImageNode;
	}

	public TreePdfNode InsertChildPdf(int index, Guid fileId)
	{
		base.Project.ThrowIfMaxExceeded();
		TreePdfNode treePdfNode = new TreePdfNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewPdfName(),
			IsEntityDirty = true
		};
		treePdfNode.Pdf._isLoaded = true;
		treePdfNode.Pdf.FileId = fileId;
		InsertChildNode(treePdfNode, index);
		return treePdfNode;
	}

	public override void Remove()
	{
		base.Remove();
		foreach (TreeNodeBase item in GetDescendants().ToList())
		{
			item.Remove();
		}
	}

	public void InsertChildNode(TreeNodeBase child, int index)
	{
		child.Parent = this;
		child.Group = base.Group;
		child.Level = base.Level + 1;
		child.Visible = true;
		Children.Insert(index, child);
		base.Project.NeedSave = true;
		base.Project._treeNodeFormulaNamesExpired = true;
		if (child is TreeTableNode value)
		{
			base.Project._dicTableNodes.Add(child.Id, value);
		}
	}

	public TreeDirectoryNode DuplicateDirectory()
	{
		return new TreeDirectoryNode
		{
			Id = Project.Current.GetNextId(),
			Name = base.Name,
			Group = base.Group
		};
	}

	public override void MoveTo(TreeGroup tg, int index)
	{
		base.MoveTo(tg, index);
		foreach (TreeNodeBase descendant in GetDescendants())
		{
			descendant.UpdateGroup(tg);
		}
	}

	public override void MoveTo(TreeGroup tg)
	{
		base.MoveTo(tg);
		foreach (TreeNodeBase descendant in GetDescendants())
		{
			descendant.UpdateGroup(tg);
		}
	}

	public override void MoveTo(TreeDirectoryNode tdn)
	{
		base.MoveTo(tdn);
		foreach (TreeNodeBase descendant in GetDescendants())
		{
			descendant.UpdateGroup(tdn.Group);
		}
	}

	public override void MoveTo(TreeDirectoryNode tdn, int index)
	{
		base.MoveTo(tdn, index);
		foreach (TreeNodeBase descendant in GetDescendants())
		{
			descendant.UpdateGroup(tdn.Group);
		}
	}

	public TreeNodeBase GetFirstCantDeleteDescendant()
	{
		return (from n in GetDescendants()
			where !(n is TreeDirectoryNode)
			select n).FirstOrDefault((TreeNodeBase n) => !n.Permissions.CanDelete());
	}

	private string GetNewDirectoryName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in base.Group.GetAllNodes().OfType<TreeDirectoryNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建文件夹{num}"));
		return $"新建文件夹{num}";
	}

	private string GetNewTableName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in base.Group.GetAllNodes().OfType<TreeTableNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建表格{num}"));
		return $"新建表格{num}";
	}

	private string GetNewDocumentName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in base.Group.GetAllNodes().OfType<TreeDocumentNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建文档{num}"));
		return $"新建文档{num}";
	}

	private string GetNewImageName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in base.Group.GetAllNodes().OfType<TreeImageNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建图片{num}"));
		return $"新建图片{num}";
	}

	private string GetNewPdfName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in base.Group.GetAllNodes().OfType<TreePdfNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建 PDF{num}"));
		return $"新建 PDF{num}";
	}

	internal override void ServerRemove()
	{
		base.ServerRemove();
		for (int num = Children.Count - 1; num >= 0; num--)
		{
			Children[num].ServerRemove();
		}
	}

	protected internal override int GetCode()
	{
		return 0;
	}

	public override void UpdateVisible(bool v)
	{
		foreach (TreeNodeBase descendant in GetDescendants())
		{
			descendant.UpdateVisible(v);
		}
		base.UpdateVisible(v);
	}
}
