using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Auditai.Model;

namespace Auditai.UI.Platform;

internal class ProjectBak
{
	private bool closed;

	private string rootPath;

	private Thread worker;

	private ExcelContex context = new ExcelContex();

	private ProjectExport exporter = new ProjectExport();

	private Queue<TreeNodeBase> taskQueue = new Queue<TreeNodeBase>();

	private Dictionary<TreeNodeBase, AutoResetEvent> waiters = new Dictionary<TreeNodeBase, AutoResetEvent>();

	private Dictionary<long, string> treeNodeFileNameMap = new Dictionary<long, string>();

	public static string GetProjectBakPath(User user, Project project)
	{
		return Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory), "/AuditAI数据备份/", user.UserName + "/" + project.Number + " " + project.Name + "/");
	}

	public ProjectBak(string rootPath)
	{
		this.rootPath = rootPath;
	}

	public void Start()
	{
		if (closed)
		{
			return;
		}
		worker = new Thread((ThreadStart)delegate
		{
			while (!closed)
			{
				if (taskQueue.Count > 0)
				{
					TreeNodeBase treeNode = taskQueue.Dequeue();
					try
					{
						if (!treeNode.Permissions.CanRead())
						{
							waiters[treeNode].Set();
						}
						else
						{
							string relativePath = treeNode.GetRelativePath();
							string fullPath = Path.Combine(rootPath, relativePath);
							if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
							}
							ThreadPool.QueueUserWorkItem(async delegate
							{
								_ = 2;
								try
								{
									if (!(treeNode is TreeTableNode treeTableNode))
									{
										if (!(treeNode is TreeDocumentNode docNode))
										{
											if (!(treeNode is TreePdfNode pdfNode))
											{
												if (treeNode is TreeImageNode imageNode)
												{
													await exporter.SaveImage(imageNode, fullPath);
												}
											}
											else
											{
												await exporter.SavePdf(pdfNode, fullPath);
											}
										}
										else
										{
											exporter.SaveDocument(docNode, fullPath);
										}
									}
									else
									{
										exporter.SaveTable(treeTableNode, fullPath, context);
										await exporter.ExportTableAttachments(treeTableNode.Table, fullPath);
									}
								}
								catch (Exception)
								{
								}
								finally
								{
									waiters[treeNode].Set();
								}
							}, null);
						}
					}
					catch (Exception)
					{
						waiters[treeNode].Set();
					}
				}
			}
		});
		worker.IsBackground = true;
		worker.Start();
	}

	public void SetEnable(bool e)
	{
		closed = !e;
	}

	public async Task WaitFinish()
	{
		if (closed)
		{
			return;
		}
		try
		{
			foreach (AutoResetEvent value in waiters.Values)
			{
				value.WaitOne();
			}
			closed = true;
			try
			{
				await context.ExportFormula();
			}
			catch (Exception)
			{
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			if (worker != null)
			{
				try
				{
					worker.Abort();
				}
				catch (Exception)
				{
				}
			}
		}
	}

	public void Bak(TreeNodeBase treeNode)
	{
		if (!closed && treeNode != null)
		{
			waiters.Add(treeNode, new AutoResetEvent(initialState: false));
			taskQueue.Enqueue(treeNode);
		}
	}
}
