using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class ProjectExport
{
	private int currentProgress;

	private int totalProgress = 100;

	public Leqisoft.Model.Project Project { get; set; }

	public event EventHandler<ProgressArgs> ProgressChanged;

	public async Task<DialogResult> SaveDialog()
	{
		if (Project == null)
		{
			throw new NullReferenceException(StringConstBase.Current.Project + "为空");
		}
		FolderBrowserDialog _fd = new FolderBrowserDialog
		{
			Description = "请选择保存路径"
		};
		if (_fd.ShowDialog() == DialogResult.OK)
		{
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
			{
				ProgressChanged += progressDeal(progress);
				try
				{
					await SaveProjectImpl(_fd.SelectedPath);
				}
				finally
				{
					ProgressChanged -= progressDeal(progress);
				}
				return Task.FromResult(new object());
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			return DialogResult.OK;
		}
		return DialogResult.Cancel;
		EventHandler<ProgressArgs> progressDeal(IProgress<ProgressInfo> progress)
		{
			return delegate(object s1, ProgressArgs e1)
			{
				switch (e1.Type)
				{
				case ProgressEnum.CreatDir:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				case ProgressEnum.SaveDoc:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				case ProgressEnum.SaveTable:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				case ProgressEnum.CreateTable:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				case ProgressEnum.SaveImage:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				case ProgressEnum.SavePdf:
					progress.Report(new ProgressInfo
					{
						MainCaption = e1.Message,
						MainProgress = e1.Progress * 100 / totalProgress
					});
					break;
				}
			};
		}
	}

	public async void Save(string savePath)
	{
		if (Project == null)
		{
			throw new NullReferenceException(StringConstBase.Current.Project + "为空");
		}
		await SaveProjectImpl(savePath);
	}

	public async Task SaveTableAsync(TreeTableNode tableNode, string fullPath, ExcelContex contex)
	{
		await Task.Factory.StartNew(delegate
		{
			SaveTable(tableNode, fullPath, contex);
		});
		await ExportTableAttachments(tableNode.Table, fullPath);
	}

	public void SaveTable(TreeTableNode tableNode, string fullPath, ExcelContex contex)
	{
		Leqisoft.Model.Table table = tableNode.Table;
		table.LoadAndReturn();
		if (!table.LocalExists || table.IsCorrupted)
		{
			return;
		}
		try
		{
			ReportExportToExcel reportExportToExcel = new ReportExportToExcel
			{
				Table = table,
				PageSetup = table.PageSetup,
				WaterMarkPageSetup = null
			};
			reportExportToExcel.SaveValue(fullPath, contex);
		}
		catch (ArgumentException ex)
		{
			throw new ArgumentException(ex.Message + " " + fullPath);
		}
	}

	public async Task ExportTableAttachments(Leqisoft.Model.Table table, string tableFilePath)
	{
		table.LoadAndReturn();
		if (!table.LocalExists || table.IsCorrupted || table.CellPropManager.DicCellAttachments.Count == 0)
		{
			return;
		}
		try
		{
			string text = Path.GetFileNameWithoutExtension(tableFilePath) + "的单元格附件";
			char[] invalidPathChars = Path.GetInvalidPathChars();
			foreach (char oldChar in invalidPathChars)
			{
				text = text.Replace(oldChar, '-');
			}
			string finalDirName = Path.Combine(Path.GetDirectoryName(tableFilePath), text);
			while (!Directory.Exists(finalDirName))
			{
				Directory.CreateDirectory(finalDirName);
			}
			foreach (Leqisoft.Model.CellAttachments value in table.CellPropManager.DicCellAttachments.Values)
			{
				if (value.Status == SyncStatus.LocalDeleted || value.Status == SyncStatus.ServerDeleted)
				{
					continue;
				}
				foreach (CellAttachment attachment in value.Attachments)
				{
					Guid fileId = attachment.FileId;
					await table.Project.FileCacheManager.DownloadIfNotExist(fileId);
					string text2 = ((attachment.Name == null) ? string.Empty : attachment.Name);
					char[] invalidPathChars2 = Path.GetInvalidPathChars();
					foreach (char oldChar2 in invalidPathChars2)
					{
						text2 = text2.Replace(oldChar2, '-');
					}
					string extension = Path.GetExtension(text2);
					text2 = Path.GetFileNameWithoutExtension(text2);
					string text3 = Path.Combine(finalDirName, text2) + extension;
					int num = 0;
					while (File.Exists(text3))
					{
						num++;
						text3 = $"{Path.Combine(finalDirName, text2)}({num}){extension}";
						if (num == int.MaxValue)
						{
							break;
						}
					}
					table.Project.FileCacheManager.DuplicateTo(fileId, text3);
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log("导出表格" + tableFilePath + "的附件时发生了未预期的异常");
		}
	}

	public async Task SaveDocumentAsync(TreeDocumentNode docNode, string fullPath)
	{
		await Task.Factory.StartNew(delegate
		{
			SaveDocument(docNode, fullPath);
		});
	}

	public void SaveDocument(TreeDocumentNode docNode, string fullPath)
	{
		Leqisoft.Model.Document document = docNode.Document;
		document.LoadAndReturn();
		if (!document.LocalExists || document.IsCorrupted)
		{
			return;
		}
		try
		{
			ReportExportToWord2 reportExportToWord = new ReportExportToWord2
			{
				Document = document
			};
			reportExportToWord.Save(fullPath);
		}
		catch (ArgumentException ex)
		{
			throw new ArgumentException(ex.Message + " " + fullPath);
		}
	}

	public async Task SaveImage(TreeImageNode imageNode, string fullPath)
	{
		Guid fileId = imageNode.Image.LoadAndReturn().FileId;
		FileCacheManager fcm = imageNode.Project.FileCacheManager;
		await fcm.DownloadIfNotExist(fileId);
		fcm.GetPath(fileId);
		System.Drawing.Image graphicsImage = imageNode.Image.GetGraphicsImage();
		graphicsImage.Save(fullPath);
	}

	public async Task SavePdf(TreePdfNode pdfNode, string fullPath)
	{
		Guid fileId = pdfNode.Pdf.LoadAndReturn().FileId;
		FileCacheManager fcm = pdfNode.Project.FileCacheManager;
		await fcm.DownloadIfNotExist(fileId);
		string path = fcm.GetPath(fileId);
		File.Copy(path, fullPath);
	}

	private async Task SaveProjectImpl(string path)
	{
		Dictionary<long, string> nodeDirMap = new Dictionary<long, string>();
		currentProgress = 0;
		string tail;
		string rootPath = NonRepeatDir(Path.Combine(path, Project.Name), out tail);
		Directory.CreateDirectory(rootPath);
		Dictionary<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> groupList = GetGroupList(Project);
		totalProgress = groupList.SelectMany((KeyValuePair<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> t) => t.Value).Count() + (from n in groupList.SelectMany((KeyValuePair<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> t) => t.Value)
			where n is TreeTableNode
			select n).Count();
		ExcelContex exportContex = new ExcelContex();
		exportContex.BeforeTableSave += delegate(object s1, string e1)
		{
			OnProgressChanged(new ProgressArgs
			{
				Type = ProgressEnum.CreateTable,
				Progress = ++currentProgress,
				Message = "正在创建表格 " + e1
			});
		};
		foreach (KeyValuePair<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> item in groupList)
		{
			string tail2;
			string path2 = NonRepeatDir(Path.Combine(rootPath, item.Key.Name), out tail2);
			Directory.CreateDirectory(path2);
			nodeDirMap.Add(item.Key.Id.Value, item.Key.Name + tail2);
			foreach (TreeNodeBase item2 in item.Value)
			{
				try
				{
					if (!(item2 is TreeDirectoryNode treeDirectoryNode))
					{
						if (!(item2 is TreeDocumentNode treeDocumentNode))
						{
							if (!(item2 is TreeTableNode treeTableNode))
							{
								if (!(item2 is TreeImageNode treeImageNode))
								{
									if (item2 is TreePdfNode treePdfNode)
									{
										OnProgressChanged(new ProgressArgs
										{
											Type = ProgressEnum.SavePdf,
											Progress = ++currentProgress,
											Message = "正在导出PDF " + treePdfNode.Name
										});
										string path3 = Path.Combine(rootPath, GetRelativePath(treePdfNode, nodeDirMap));
										string fullPath = NonRepeatFile(Path.Combine(path3, RemoveInvalidChars(treePdfNode.Name) + ".pdf"), out tail);
										await SavePdf(treePdfNode, fullPath);
									}
									else
									{
										await Task.Delay(1);
									}
									continue;
								}
								OnProgressChanged(new ProgressArgs
								{
									Type = ProgressEnum.SaveImage,
									Progress = ++currentProgress,
									Message = "正在导出图片 " + treeImageNode.Name
								});
								System.Drawing.Image graphicsImage = treeImageNode.Image.GetGraphicsImage();
								string path4 = Path.Combine(rootPath, GetRelativePath(treeImageNode, nodeDirMap));
								string text;
								try
								{
									text = GetExtension(graphicsImage);
								}
								catch (ArgumentOutOfRangeException)
								{
									text = ".jpg";
								}
								string fullPath2 = NonRepeatFile(Path.Combine(path4, RemoveInvalidChars(treeImageNode.Name) + text), out tail);
								await SaveImage(treeImageNode, fullPath2);
							}
							else
							{
								OnProgressChanged(new ProgressArgs
								{
									Type = ProgressEnum.SaveTable,
									Progress = ++currentProgress,
									Message = "正在导出表格 " + treeTableNode.Name
								});
								string path5 = Path.Combine(rootPath, GetRelativePath(treeTableNode, nodeDirMap));
								string fullPath3 = NonRepeatFile(Path.Combine(path5, RemoveInvalidChars(treeTableNode.Name) + ".xlsx"), out tail);
								await SaveTableAsync(treeTableNode, fullPath3, exportContex);
							}
						}
						else
						{
							OnProgressChanged(new ProgressArgs
							{
								Type = ProgressEnum.SaveDoc,
								Progress = ++currentProgress,
								Message = "正在导出文档 " + treeDocumentNode.Name
							});
							string path6 = Path.Combine(rootPath, GetRelativePath(treeDocumentNode, nodeDirMap));
							string fullPath4 = NonRepeatFile(Path.Combine(path6, RemoveInvalidChars(treeDocumentNode.Name) + ".docx"), out tail);
							await SaveDocumentAsync(treeDocumentNode, fullPath4);
						}
					}
					else
					{
						OnProgressChanged(new ProgressArgs
						{
							Type = ProgressEnum.CreatDir,
							Progress = ++currentProgress,
							Message = "正在创建文件夹 " + treeDirectoryNode.Name
						});
						string tail3;
						string path7 = NonRepeatDir(Path.Combine(rootPath, GetRelativePath(treeDirectoryNode, nodeDirMap)), out tail3);
						Directory.CreateDirectory(path7);
						nodeDirMap.Add(treeDirectoryNode.Id.Value, treeDirectoryNode.Name + tail3);
					}
				}
				catch (IOException)
				{
				}
				catch (Exception)
				{
				}
			}
		}
		await exportContex.ExportFormula();
	}

	private Dictionary<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> GetGroupList(Leqisoft.Model.Project project)
	{
		Dictionary<Leqisoft.Model.TreeGroup, List<TreeNodeBase>> dictionary = new Dictionary<Leqisoft.Model.TreeGroup, List<TreeNodeBase>>();
		foreach (Leqisoft.Model.TreeGroup treeGroup in project.TreeGroups)
		{
			List<TreeNodeBase> list = new List<TreeNodeBase>();
			foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
			{
				addChilds(rootNode, list);
			}
			dictionary.Add(treeGroup, list);
		}
		return dictionary;
		static void addChilds(TreeNodeBase node, List<TreeNodeBase> treeNodes)
		{
			if (!(node is TreeDirectoryNode treeDirectoryNode))
			{
				if (!(node is TreeDocumentNode item))
				{
					if (!(node is TreeTableNode item2))
					{
						if (!(node is TreeImageNode item3))
						{
							if (node is TreePdfNode item4)
							{
								treeNodes.Add(item4);
							}
						}
						else
						{
							treeNodes.Add(item3);
						}
					}
					else
					{
						treeNodes.Add(item2);
					}
				}
				else
				{
					treeNodes.Add(item);
				}
			}
			else
			{
				treeNodes.Add(treeDirectoryNode);
				if (treeDirectoryNode.Children.Count > 0)
				{
					foreach (TreeNodeBase child in treeDirectoryNode.Children)
					{
						addChilds(child, treeNodes);
					}
				}
			}
		}
	}

	private string NonRepeatDir(string path, out string tail)
	{
		int num = 0;
		string text = path;
		tail = string.Empty;
		while (Directory.Exists(path))
		{
			num++;
			tail = $"({num})";
			path = text + tail;
		}
		return path;
	}

	private string NonRepeatFile(string path, out string tail)
	{
		int num = 0;
		tail = string.Empty;
		string extension = Path.GetExtension(path);
		string text = path.Replace(extension, string.Empty);
		while (Directory.Exists(path))
		{
			num++;
			tail = $"({num})";
			path = text + tail + extension;
		}
		return path;
	}

	public string GetRelativePath(TreeNodeBase treenode, Dictionary<long, string> specificDir)
	{
		List<string> list = new List<string>();
		if (treenode is TreeDirectoryNode treeDirectoryNode)
		{
			if (specificDir.ContainsKey(treeDirectoryNode.Id.Value))
			{
				list.Add(specificDir[treeDirectoryNode.Id.Value]);
			}
			else
			{
				list.Add(treeDirectoryNode.Name);
			}
		}
		for (TreeDirectoryNode parent = treenode.Parent; parent != null; parent = parent.Parent)
		{
			if (specificDir.ContainsKey(parent.Id.Value))
			{
				list.Add(specificDir[parent.Id.Value]);
			}
			else
			{
				list.Add(parent.Name);
			}
		}
		if (treenode.Group != null)
		{
			if (specificDir.ContainsKey(treenode.Group.Id.Value))
			{
				list.Add(specificDir[treenode.Group.Id.Value]);
			}
			else
			{
				list.Add(treenode.Group.Name);
			}
		}
		list.Reverse();
		return string.Join("\\", list);
	}

	public string GetRelativePath(TreeNodeBase treenode)
	{
		List<string> list = new List<string>();
		if (treenode is TreeDirectoryNode treeDirectoryNode)
		{
			list.Add(treeDirectoryNode.Name);
		}
		for (TreeDirectoryNode parent = treenode.Parent; parent != null; parent = parent.Parent)
		{
			list.Add(parent.Name);
		}
		if (treenode.Group != null)
		{
			list.Add(treenode.Group.Name);
		}
		list.Reverse();
		return string.Join("\\", list);
	}

	public string RemoveInvalidChars(string path)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidFileNameChars)
		{
			path.Replace(c.ToString(), string.Empty);
		}
		return path.Replace("\r", "").Replace("\n", "").Replace("/", "_")
			.Replace("\\", "_")
			.Trim();
	}

	public string GetExtension(System.Drawing.Image image)
	{
		if (image.RawFormat.Equals(ImageFormat.Bmp))
		{
			return ".bmp";
		}
		if (image.RawFormat.Equals(ImageFormat.Gif))
		{
			return ".gif";
		}
		if (image.RawFormat.Equals(ImageFormat.Jpeg))
		{
			return ".jpeg";
		}
		if (image.RawFormat.Equals(ImageFormat.Png))
		{
			return ".png";
		}
		if (image.RawFormat.Equals(ImageFormat.Tiff))
		{
			return ".tiff";
		}
		if (image.RawFormat.Equals(ImageFormat.Icon))
		{
			return ".icon";
		}
		if (image.RawFormat.Equals(ImageFormat.Emf))
		{
			return ".emf";
		}
		if (image.RawFormat.Equals(ImageFormat.Exif))
		{
			return ".exif";
		}
		if (image.RawFormat.Equals(ImageFormat.Wmf))
		{
			return ".wmf";
		}
		throw new ArgumentOutOfRangeException("无法识别文件类型");
	}

	protected void OnProgressChanged(ProgressArgs args)
	{
		this.ProgressChanged?.Invoke(this, args);
	}
}
