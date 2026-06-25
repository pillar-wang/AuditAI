using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Auditai.Model;
using Auditai.UI.Controls;
using Microsoft.Experimental.IO;

namespace Auditai.UI.Platform;

public class ProjectImport
{
	private delegate void InvokeDelegate();

	private enum FileKind
	{
		Pdf,
		Word,
		Excel,
		Image,
		Folder,
		Unknow
	}

	private const int MAX_C1EXCEL_ROW_COUNT = 10000;

	private readonly List<TreeNodeBase> _appendNodes = new List<TreeNodeBase>();

	private readonly Control _owner;

	private HandlerContext HandlerContext = new HandlerContext();

	public List<DocumentEditor> DocumentEditors { get; private set; } = new List<DocumentEditor>();


	public event EventHandler<ImportNodeArgs> AfterImportNode;

	public ProjectImport(Control owner)
	{
		_owner = owner;
	}

	public void ImportFiles(object parentNode, int index, string[] files)
	{
		Initialize();
		foreach (string file in files)
		{
			ImportFileImpl(parentNode, index++, file);
		}
	}

	public void ImportFolder(object parentNode, int index, string folder)
	{
		Initialize();
		ImportFolderImpl(parentNode, index, folder);
	}

	public void Initialize()
	{
		_appendNodes.Clear();
		DocumentEditors.Clear();
	}

	public TreeNodeBase GetFirstAppend()
	{
		return _appendNodes.FirstOrDefault((TreeNodeBase n) => n is TreeTableNode || n is TreeDocumentNode || n is TreePdfNode || n is TreeImageNode);
	}

	private void ImportFileImpl(object parent, int index, string file)
	{
		FileKind fileKind = GetFileKind(file);
		if (!(parent is TreeGroup parent2))
		{
			if (!(parent is TreeDirectoryNode parent3))
			{
				throw new ArgumentOutOfRangeException("父节点类型不正确");
			}
			switch (fileKind)
			{
			case FileKind.Word:
				ImportDoc(parent3, index, file);
				break;
			case FileKind.Excel:
				ImportTable(parent3, index, file);
				break;
			case FileKind.Image:
				ImportImage(parent3, index, file);
				break;
			case FileKind.Pdf:
				ImportPdf(parent3, index, file);
				break;
			}
		}
		else
		{
			switch (fileKind)
			{
			case FileKind.Word:
				ImportDoc(parent2, index, file);
				break;
			case FileKind.Excel:
				ImportTable(parent2, index, file);
				break;
			case FileKind.Image:
				ImportImage(parent2, index, file);
				break;
			case FileKind.Pdf:
				ImportPdf(parent2, index, file);
				break;
			}
		}
	}

	private void ImportFolderImpl(object parent, int index, string folder)
	{
		Dictionary<string, FileKind> files = GetAllChilds(folder);
		if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => (files != null) ? files.Count : 0))
		{
			return;
		}
		TreeDirectoryNode treeDirectoryNode = null;
		if (!(parent is TreeGroup parent2))
		{
			if (!(parent is TreeDirectoryNode parent3))
			{
				throw new ArgumentOutOfRangeException("父节点类型不正确");
			}
			treeDirectoryNode = ImportDir(parent3, index, folder);
		}
		else
		{
			treeDirectoryNode = ImportDir(parent2, index, folder);
		}
		Dictionary<string, TreeDirectoryNode> dictionary = new Dictionary<string, TreeDirectoryNode> { { folder, treeDirectoryNode } };
		foreach (KeyValuePair<string, FileKind> item in files)
		{
			try
			{
				string key = item.Key;
				if (item.Value == FileKind.Folder)
				{
					TreeDirectoryNode treeDirectoryNode2 = dictionary[Directory.GetParent(key).FullName];
					TreeDirectoryNode value = ImportDir(treeDirectoryNode2, treeDirectoryNode2.Children.Count, key);
					dictionary.Add(key, value);
				}
				else
				{
					TreeDirectoryNode treeDirectoryNode3 = dictionary[Path.GetDirectoryName(key)];
					ImportFileImpl(treeDirectoryNode3, treeDirectoryNode3.Children.Count, key);
				}
			}
			catch (Exception ex)
			{
				ex.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, item.Key + "导入失败！失败原因：" + ex.Message);
			}
		}
	}

	private TreeDocumentNode ImportDoc(TreeGroup parent, int index, string file)
	{
		bool flag = false;
		TreeDocumentNode treeDocumentNode = parent.InsertRootDocument(index);
		try
		{
			DocumentEditor item = HandlerContext.docHandler.Import(file, treeDocumentNode);
			treeDocumentNode.UpdateName(Path.GetFileNameWithoutExtension(file));
			DocumentEditors.Add(item);
		}
		catch (Exception ex)
		{
			ex.Log();
			flag = true;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		if (flag)
		{
			treeDocumentNode.Remove();
			return null;
		}
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Doc,
			ParentNode = parent,
			Index = index,
			AppendNode = treeDocumentNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treeDocumentNode;
	}

	private TreeDocumentNode ImportDoc(TreeDirectoryNode parent, int index, string file)
	{
		bool flag = false;
		TreeDocumentNode treeDocumentNode = parent.InsertChildDocument(index);
		try
		{
			DocumentEditor item = HandlerContext.docHandler.Import(file, treeDocumentNode);
			treeDocumentNode.UpdateName(Path.GetFileNameWithoutExtension(file));
			DocumentEditors.Add(item);
		}
		catch (Exception ex)
		{
			ex.Log();
			flag = true;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		if (flag)
		{
			treeDocumentNode.Remove();
			return null;
		}
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Doc,
			ParentNode = parent,
			Index = index,
			AppendNode = treeDocumentNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treeDocumentNode;
	}

	private TreeNodeBase ImportTable(TreeGroup parent, int index, string file)
	{
		return ImportTableImpl(index, file, parent, (int index) => parent.InsertRootTable(index, InitTableMode.Empty), (int index) => parent.InsertRootDirectory(index));
	}

	private TreeNodeBase ImportTable(TreeDirectoryNode parent, int index, string file)
	{
		return ImportTableImpl(index, file, parent, (int index) => parent.InsertChildTable(index, InitTableMode.Empty), (int index) => parent.InsertChildDirectory(index));
	}

	private TreeNodeBase ImportTableImpl(int index, string file, object parent, Func<int, TreeTableNode> genTableNode, Func<int, TreeDirectoryNode> genDirNode)
	{
		if (BigExcelImporter.GetMaxRowCount(file) > 10000)
		{
			using (BigExcelImporter bigExcelImporter = new BigExcelImporter(file))
			{
				if (bigExcelImporter.SheetCount == 1)
				{
					TreeTableNode treeTableNode = genTableNode(index);
					bigExcelImporter.Import(treeTableNode.Table);
					treeTableNode.UpdateName(Path.GetFileNameWithoutExtension(file));
					OnAfterImportNode(new ImportNodeArgs
					{
						Type = ImportTypeEnum.Table,
						ParentNode = parent,
						Index = index,
						AppendNode = treeTableNode,
						Message = "正在导入 " + Path.GetFileName(file),
						OnProgress = true
					});
					return treeTableNode;
				}
				if (bigExcelImporter.SheetCount > 1)
				{
					TreeDirectoryNode treeDirectoryNode = genDirNode(index);
					treeDirectoryNode.UpdateName(Path.GetFileNameWithoutExtension(file));
					OnAfterImportNode(new ImportNodeArgs
					{
						Type = ImportTypeEnum.Dir,
						ParentNode = parent,
						Index = index,
						AppendNode = treeDirectoryNode,
						Message = "正在导入 " + treeDirectoryNode.Name
					});
					for (int i = 0; i < bigExcelImporter.SheetCount; i++)
					{
						TreeTableNode treeTableNode2 = treeDirectoryNode.InsertChildTable(treeDirectoryNode.Children.Count, InitTableMode.Empty);
						bigExcelImporter.Import(treeTableNode2.Table);
						treeTableNode2.UpdateName(bigExcelImporter.SheetName);
						OnAfterImportNode(new ImportNodeArgs
						{
							Type = ImportTypeEnum.Sheet,
							ParentNode = treeDirectoryNode,
							Index = i,
							AppendNode = treeTableNode2,
							Message = "正在导入" + Path.GetFileNameWithoutExtension(file) + " Sheet" + bigExcelImporter.SheetName
						});
						bigExcelImporter.NextSheet();
					}
					OnAfterImportNode(new ImportNodeArgs
					{
						OnProgress = true,
						Message = treeDirectoryNode.Name + " 导入成功"
					});
					return treeDirectoryNode;
				}
				return null;
			}
		}
		try
		{
			ImportExcel importExcel = new ImportExcel();
			importExcel.Load(file);
			if (importExcel.SheetCount == 1)
			{
				importExcel.Next();
				TreeTableNode treeTableNode3 = genTableNode(index);
				importExcel.Import(treeTableNode3.Table);
				importExcel.GenerateFormula();
				treeTableNode3.UpdateName(Path.GetFileNameWithoutExtension(file));
				treeTableNode3.Table.Title.TitleCell.Value = treeTableNode3.Name;
				OnAfterImportNode(new ImportNodeArgs
				{
					Type = ImportTypeEnum.Table,
					ParentNode = parent,
					Index = index,
					AppendNode = treeTableNode3,
					Message = "正在导入 " + Path.GetFileName(file),
					OnProgress = true
				});
				return treeTableNode3;
			}
			if (importExcel.SheetCount > 1)
			{
				TreeDirectoryNode treeDirectoryNode2 = genDirNode(index);
				treeDirectoryNode2.UpdateName(Path.GetFileNameWithoutExtension(file));
				OnAfterImportNode(new ImportNodeArgs
				{
					Type = ImportTypeEnum.Dir,
					ParentNode = parent,
					Index = index,
					AppendNode = treeDirectoryNode2,
					Message = "正在导入 " + treeDirectoryNode2.Name
				});
				int num = 0;
				while (importExcel.HasNext())
				{
					if (!importExcel.CurrentEmpty())
					{
						TreeTableNode treeTableNode4 = treeDirectoryNode2.InsertChildTable(treeDirectoryNode2.Children.Count, InitTableMode.Empty);
						importExcel.Import(treeTableNode4.Table);
						treeTableNode4.UpdateName(importExcel.CurrentSheet.Name);
						treeTableNode4.Table.Title.TitleCell.Value = treeTableNode4.Name;
						OnAfterImportNode(new ImportNodeArgs
						{
							Type = ImportTypeEnum.Sheet,
							ParentNode = treeDirectoryNode2,
							Index = num++,
							AppendNode = treeTableNode4,
							Message = "正在导入" + Path.GetFileNameWithoutExtension(file) + " Sheet" + importExcel.CurrentSheet.Name
						});
					}
				}
				importExcel.GenerateFormula();
				OnAfterImportNode(new ImportNodeArgs
				{
					OnProgress = true,
					Message = treeDirectoryNode2.Name + " 导入成功"
				});
				return treeDirectoryNode2;
			}
			return null;
		}
		catch (EndOfStreamException exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导入文件时出现错误，无法导入。若文件是xls格式，建议另存为xlsx格式后再次尝试导入。");
			return null;
		}
		catch (IOException ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return null;
		}
	}

	private TreeImageNode ImportImage(TreeGroup parent, int index, string file)
	{
		Guid guid = Guid.NewGuid();
		try
		{
			double fileSizeInM = GetFileSizeInM(file);
			if (SoftwareLicenseManager.IsImportImageFileOutOfLicenseLimit(fileSizeInM))
			{
				return null;
			}
			using (System.Drawing.Image image = System.Drawing.Image.FromFile(file))
			{
				double num = image.Width;
				double num2 = image.Height;
				if (num > 5000.0 || num2 > 5000.0)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您导入的图片文件过大，将自动缩小为适合尺寸！");
					file = Path.GetTempFileName();
					double num3 = num / num2;
					if (num > num2)
					{
						num = 5000.0;
						num2 = num / num3;
					}
					else
					{
						num2 = 5000.0;
						num = num2 * num3;
					}
					System.Drawing.Image image2 = ResizeImage(image, (int)num, (int)num2);
					image2.Save(file);
				}
			}
			parent.Project.FileCacheManager.CopyFrom(file, guid);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开图片文件时发生错误。");
			return null;
		}
		TreeImageNode treeImageNode = parent.InsertRootImage(index, guid);
		treeImageNode.UpdateName(Path.GetFileNameWithoutExtension(file));
		treeImageNode.Image._isFirstOpened = true;
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Image,
			ParentNode = parent,
			Index = index,
			AppendNode = treeImageNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treeImageNode;
	}

	private TreeImageNode ImportImage(TreeDirectoryNode parent, int index, string file)
	{
		Guid guid = Guid.NewGuid();
		try
		{
			double fileSizeInM = GetFileSizeInM(file);
			if (SoftwareLicenseManager.IsImportImageFileOutOfLicenseLimit(fileSizeInM))
			{
				return null;
			}
			using (System.Drawing.Image image = System.Drawing.Image.FromFile(file))
			{
				double num = image.Width;
				double num2 = image.Height;
				if (num > 5000.0 || num2 > 5000.0)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您导入的图片文件过大，将自动缩小为适合尺寸！");
					file = Path.GetTempFileName();
					double num3 = num / num2;
					if (num > num2)
					{
						num = 5000.0;
						num2 = num / num3;
					}
					else
					{
						num2 = 5000.0;
						num = num2 * num3;
					}
					System.Drawing.Image image2 = ResizeImage(image, (int)num, (int)num2);
					image2.Save(file);
				}
			}
			parent.Project.FileCacheManager.CopyFrom(file, guid);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开图片文件时发生错误。");
			return null;
		}
		TreeImageNode treeImageNode = parent.InsertChildImage(index, guid);
		treeImageNode.UpdateName(Path.GetFileNameWithoutExtension(file));
		treeImageNode.Image._isFirstOpened = true;
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Image,
			ParentNode = parent,
			Index = index,
			AppendNode = treeImageNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treeImageNode;
	}

	private TreePdfNode ImportPdf(TreeGroup parent, int index, string file)
	{
		Guid guid = Guid.NewGuid();
		try
		{
			double fileSizeInM = GetFileSizeInM(file);
			if (SoftwareLicenseManager.IsImportPDFFileOutOfLicenseLimit(fileSizeInM))
			{
				return null;
			}
			parent.Project.FileCacheManager.CopyFrom(file, guid);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开 PDF 文件时发生错误。");
			return null;
		}
		TreePdfNode treePdfNode = parent.InsertRootPdf(index, guid);
		treePdfNode.UpdateName(Path.GetFileNameWithoutExtension(file));
		treePdfNode.Pdf._isFirstOpened = true;
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Pdf,
			ParentNode = parent,
			Index = index,
			AppendNode = treePdfNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treePdfNode;
	}

	private TreePdfNode ImportPdf(TreeDirectoryNode parent, int index, string file)
	{
		Guid guid = Guid.NewGuid();
		try
		{
			double fileSizeInM = GetFileSizeInM(file);
			if (SoftwareLicenseManager.IsImportPDFFileOutOfLicenseLimit(fileSizeInM))
			{
				return null;
			}
			parent.Project.FileCacheManager.CopyFrom(file, guid);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开 PDF 文件时发生错误。");
			return null;
		}
		TreePdfNode treePdfNode = parent.InsertChildPdf(index, guid);
		treePdfNode.UpdateName(Path.GetFileNameWithoutExtension(file));
		treePdfNode.Pdf._isFirstOpened = true;
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Pdf,
			ParentNode = parent,
			Index = index,
			AppendNode = treePdfNode,
			Message = "正在导入 " + Path.GetFileName(file),
			OnProgress = true
		});
		return treePdfNode;
	}

	private TreeDirectoryNode ImportDir(TreeDirectoryNode parent, int index, string folder)
	{
		TreeDirectoryNode treeDirectoryNode = parent.InsertChildDirectory(index);
		treeDirectoryNode.UpdateName(FinalFolderName(folder));
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Dir,
			ParentNode = parent,
			Index = index,
			AppendNode = treeDirectoryNode,
			Message = "正在导入 " + treeDirectoryNode.Name
		});
		return treeDirectoryNode;
	}

	private TreeDirectoryNode ImportDir(TreeGroup parent, int index, string folder)
	{
		TreeDirectoryNode treeDirectoryNode = parent.InsertRootDirectory(index);
		treeDirectoryNode.UpdateName(FinalFolderName(folder));
		OnAfterImportNode(new ImportNodeArgs
		{
			Type = ImportTypeEnum.Dir,
			ParentNode = parent,
			Index = index,
			Message = "正在导入 " + treeDirectoryNode.Name
		});
		return treeDirectoryNode;
	}

	private string FinalFolderName(string path)
	{
		return path.Remove(0, path.LastIndexOf('\\') + 1).TrimStart('\\');
	}

	private Dictionary<string, FileKind> GetAllChilds(string path)
	{
		Dictionary<string, FileKind> dictionary = new Dictionary<string, FileKind>();
		getAllChilds(path, dictionary);
		dictionary.Remove(path);
		return dictionary;
		void getAllChilds(string _path, Dictionary<string, FileKind> _childs)
		{
			FileKind fileKind = GetFileKind(_path);
			switch (fileKind)
			{
			case FileKind.Folder:
			{
				_childs.Add(_path, FileKind.Folder);
				List<string> source = LongPathDirectory.EnumerateDirectories(_path).ToList();
				List<string> source2 = LongPathDirectory.EnumerateFiles(_path).ToList();
				IEnumerable<string> enumerable = source.OrderBy((string d) => d).Concat(source2.OrderBy((string f) => f));
				{
					foreach (string item in enumerable)
					{
						getAllChilds(item, _childs);
					}
					break;
				}
			}
			default:
				_childs.Add(_path, fileKind);
				break;
			case FileKind.Unknow:
				break;
			}
		}
	}

	protected void OnAfterImportNode(ImportNodeArgs args)
	{
		this.AfterImportNode?.Invoke(this, args);
		_appendNodes.Add(args.AppendNode);
	}

	private System.Drawing.Image ResizeImage(System.Drawing.Image image, int twidth, int theight)
	{
		Bitmap bitmap = new Bitmap(twidth, theight);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
		return bitmap;
	}

	private double GetFileSizeInM(string file)
	{
		FileInfo fileInfo = new FileInfo(file);
		float num = (float)fileInfo.Length * 1f / 1048576f;
		return num;
	}

	private FileKind GetFileKind(string file)
	{
		string extension = Path.GetExtension(file);
		if (string.IsNullOrEmpty(extension) && Directory.Exists(file))
		{
			return FileKind.Folder;
		}
		string text = extension.ToLower();
		if (text != null)
		{
			int length = text.Length;
			if (length != 4)
			{
				if (length == 5)
				{
					char c = text[1];
					if ((uint)c <= 106u)
					{
						if (c != 'd')
						{
							if (c == 'j' && text == ".jpeg")
							{
								goto IL_015f;
							}
						}
						else if (text == ".docx")
						{
							return FileKind.Word;
						}
					}
					else if (c != 't')
					{
						if (c == 'x' && text == ".xlsx")
						{
							goto IL_015b;
						}
					}
					else if (text == ".tiff")
					{
						goto IL_015f;
					}
				}
			}
			else
			{
				char c = text[1];
				if ((uint)c <= 106u)
				{
					if (c != 'b')
					{
						if (c != 'g')
						{
							if (c == 'j' && text == ".jpg")
							{
								goto IL_015f;
							}
						}
						else if (text == ".gif")
						{
							goto IL_015f;
						}
					}
					else if (text == ".bmp")
					{
						goto IL_015f;
					}
				}
				else if (c != 'p')
				{
					if (c != 't')
					{
						if (c == 'x' && text == ".xls")
						{
							goto IL_015b;
						}
					}
					else if (text == ".tif")
					{
						goto IL_015f;
					}
				}
				else
				{
					if (text == ".pdf")
					{
						return FileKind.Pdf;
					}
					if (text == ".png")
					{
						goto IL_015f;
					}
				}
			}
		}
		return FileKind.Unknow;
		IL_015b:
		return FileKind.Excel;
		IL_015f:
		return FileKind.Image;
	}
}
