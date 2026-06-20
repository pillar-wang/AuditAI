﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;
using TXTextControl;

using LqParagraph = Leqisoft.Model.Paragraph;

namespace Leqisoft.UI.Platform;

public class DocumentEditor : UserControl
{
	private TextControlEx _textControl;
	private bool _isControlCreated;
	private bool _isDocumentLoaded;
	private string _lastTempFile;
	private DocumentStructure _structure;
	private SplitContainer _splitContainer;
	private C1SplitContainer _viewContainer;
	private C1SplitterPanel _pnlToolbar;
	private C1ToolBar _toolbar;
	private Panel _innerPanel;
	private TXTextControl.RulerBar _horizontalRulerBar;
	private TXTextControl.RulerBar _verticalRulerBar;

	private bool _isUndoing;
	private bool _isRefreshingApplicationField;
	#pragma warning disable CS0414
	private bool canRibbonStatusSetting;
	private TooltipBox _lastTtpComment;
	#pragma warning restore CS0414
	private PageSettingTarget _pageSettingTarget;

	private ContextMenuStrip _validationContextMenu;
	private ContextMenuStrip _removeValidationContextMenu;
	private ContextMenuStrip _formulaValidationContextMenu;

	private readonly C1Command cmdRefreshAll = new C1Command();
	private readonly C1CommandLink lnkRefreshAll = new C1CommandLink();
	private readonly C1Command cmdRefreshTable = new C1Command();
	private readonly C1CommandLink lnkRefreshTable = new C1CommandLink();
	private readonly C1Command cmdRefreshAllTables = new C1Command();
	private readonly C1CommandLink lnkRefreshAllTables = new C1CommandLink();
	private readonly C1Command cmdRefreshAllFields = new C1Command();
	private readonly C1CommandLink lnkRefreshAllFields = new C1CommandLink();
	private readonly C1Command cmdValidate = new C1Command();
	private readonly C1CommandLink lnkValidate = new C1CommandLink();
	private readonly C1Command cmdValidationMgmt = new C1Command();
	private readonly C1CommandLink lnkValidationMgmt = new C1CommandLink();
	private readonly C1Command cmdLock = new C1Command();
	private readonly C1CommandLink lnkLock = new C1CommandLink();
	private readonly C1Command cmdRefTable = new C1Command();
	private readonly C1CommandLink lnkRefTable = new C1CommandLink();
	private readonly C1Command cmdSmartLayout = new C1Command();
	private readonly C1CommandLink lnkSmartLayout = new C1CommandLink();
	private readonly C1Command cmdExportDoc = new C1Command();
	private readonly C1CommandLink lnkExportDoc = new C1CommandLink();
	private readonly C1Command cmdRefreshTable2 = new C1Command();
	private readonly C1Command cmdFollowTable2 = new C1Command();
	private readonly C1Command cmdImportTable = new C1Command();
	private readonly C1Command cmdGenerateConfirmation = new C1Command();

	private readonly Cursor _curFormatPainter = new Cursor(new MemoryStream(Resources.cursordoc));
	private FormatPainterContext _formatPainterContext;
	public bool IsFormatPainting;

	public TextControl Tx => _textControl;
	public dynamic Document { get; set; }
	public Control View { get; set; }
	public bool DraftMode { get; set; }
	public bool NeedSave { get; set; }
	public dynamic ToolBar => _toolbar;
	public bool DocBadFlag { get; set; }
	public dynamic _tx => _textControl;
	public TooltipBox _ttpComment { get; set; }
	public PageSettingTarget PageSettingTarget { get; set; }

	public DocumentEditor()
	{
		try
		{
			_textControl = new TextControlEx
			{
				Dock = DockStyle.Fill,
				Visible = true
			};
			_textControl.TextFieldClicked += _textControl_TextFieldClicked;
			_textControl.InputPositionChanged += _textControl_InputPositionChanged;
			// 监听 TextChanged 事件，文本变化时更新撤销/恢复按钮状态
			_textControl.TextChanged += _textControl_TextChanged;
			// 监听右键菜单打开事件，选区非空时追加"添加为校验点"菜单项
			_textControl.TextContextMenuOpening += _textControl_TextContextMenuOpening;
			InitializeValidationContextMenu();
		}
		catch (Exception ex)
		{
			ex.Log("TextControl license error");
		}
		_ttpComment = new TooltipBox
		{
			Opacity = 0.8,
			IsBalloon = true
		};

		// 创建 C1SplitContainer 作为 View，右侧放工具栏
		_viewContainer = new C1SplitContainer
		{
			BorderWidth = 0,
			Dock = DockStyle.Fill,
		};

		// 右侧工具栏面板（先加 Right 面板）
		_pnlToolbar = new C1SplitterPanel
		{
			Collapsible = false,
			KeepRelativeSize = false,
			Width = 80,
			Resizable = false,
			Dock = PanelDockStyle.Right,
		};

		// 创建垂直工具栏
		_toolbar = new C1ToolBar
		{
			Horizontal = false,
			Dock = DockStyle.Fill,
			ButtonLookVert = ButtonLookFlags.TextAndImage,
			MinButtonSize = 40,
			HideFirstDelimiter = true,
			ShowToolTips = false,
		};

		InitializeToolbarCommands();

		_pnlToolbar.Controls.Add(_toolbar);

		// 主内容面板
		var pnlMain = new C1SplitterPanel
		{
			Resizable = false,
		};

		// 主内容面板内放一个 Panel，用于承载 _textControl 和结构图 SplitContainer
		_innerPanel = new Panel { Dock = DockStyle.Fill };
		pnlMain.Controls.Add(_innerPanel);

		// 先加 Right 面板，再加主内容面板（和 TableEditor 顺序一致）
		_viewContainer.Panels.Add(_pnlToolbar);
		_viewContainer.Panels.Add(pnlMain);

		View = _viewContainer;

		if (_textControl != null)
		{
			_innerPanel.Controls.Add(_textControl);
			_textControl.CreateControl();
		}
	}

	private void InitializeToolbarCommands()
	{
		var imageProcess = MainForm.ImageProcess;

		// 引用表格
		cmdRefTable.Image = Resources.DocWholeRefresh;
		cmdRefTable.CommandStateQuery += (s, e) => cmdRefTable.Text = "引用表格";
		cmdRefTable.Click += (s, e) => InsertRefTable();
		lnkRefTable.Command = cmdRefTable;
		_toolbar.CommandLinks.Add(lnkRefTable);

		// 智能排版
		cmdSmartLayout.Image = Resources.DocWholeRefresh;
		cmdSmartLayout.CommandStateQuery += (s, e) => cmdSmartLayout.Text = "智能排版";
		cmdSmartLayout.Click += (s, e) => SmartLayout();
		lnkSmartLayout.Command = cmdSmartLayout;
		_toolbar.CommandLinks.Add(lnkSmartLayout);

		// 全文刷新
		cmdRefreshAll.Image = Resources.DocWholeRefresh;
		cmdRefreshAll.CommandStateQuery += (s, e) => cmdRefreshAll.Text = "全文刷新";
		cmdRefreshAll.Click += (s, e) => RefreshDocumentAll();
		lnkRefreshAll.Command = cmdRefreshAll;
		_toolbar.CommandLinks.Add(lnkRefreshAll);

		// 全表刷新
		cmdRefreshAllTables.Image = Resources.TableWholeReflush;
		cmdRefreshAllTables.CommandStateQuery += (s, e) => cmdRefreshAllTables.Text = "全表刷新";
		cmdRefreshAllTables.Click += (s, e) => RefreshAllTables();
		lnkRefreshAllTables.Command = cmdRefreshAllTables;
		_toolbar.CommandLinks.Add(lnkRefreshAllTables);

		// 全域刷新
		cmdRefreshAllFields.Image = Resources.DocWholeRefresh;
		cmdRefreshAllFields.CommandStateQuery += (s, e) => cmdRefreshAllFields.Text = "全域刷新";
		cmdRefreshAllFields.Click += (s, e) => RefreshAllFields();
		lnkRefreshAllFields.Command = cmdRefreshAllFields;
		_toolbar.CommandLinks.Add(lnkRefreshAllFields);

		// 全文校验
		cmdValidate.Image = Resources.ValidateDocument;
		cmdValidate.CommandStateQuery += (s, e) => cmdValidate.Text = "全文校验";
		cmdValidate.Click += (s, e) => StartValidate();
		lnkValidate.Command = cmdValidate;
		lnkValidate.Delimiter = true;
		_toolbar.CommandLinks.Add(lnkValidate);

		// 校验域管理
		cmdValidationMgmt.Image = Resources.ValidationSettings;
		cmdValidationMgmt.CommandStateQuery += (s, e) => cmdValidationMgmt.Text = "校验域管理";
		cmdValidationMgmt.Click += (s, e) =>
		{
			using (var dlg = new frmDocValidationMgmt(this))
				dlg.ShowDialog();
		};
		lnkValidationMgmt.Command = cmdValidationMgmt;
		_toolbar.CommandLinks.Add(lnkValidationMgmt);

		// 导出文档
		cmdExportDoc.Image = Resources.DocWholeRefresh;
		cmdExportDoc.CommandStateQuery += (s, e) => cmdExportDoc.Text = "导出文档";
		cmdExportDoc.Click += (s, e) => ExportDocumentDialog();
		lnkExportDoc.Command = cmdExportDoc;
		_toolbar.CommandLinks.Add(lnkExportDoc);

		foreach (C1CommandLink commandLink in _toolbar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
	}

	private void EnsureControlCreated()
	{
		if (_textControl != null && !_isControlCreated)
		{
			_isControlCreated = true;
			_textControl.CreateControl();
		}
	}

	public void PopulateDocument(params object[] args)
	{
		var doc = Document as Leqisoft.Model.Document;
		if (doc == null)
		{
			DocBadFlag = true;
			return;
		}

		// 加载文档数据
		doc.LoadAndReturn();

		if (!doc.LocalExists)
		{
			DocBadFlag = true;
			return;
		}

		if (doc.IsCorrupted)
		{
			// 本地模式下，文档可能只是缺少 Document 表记录而非真正损坏
			// 尝试创建默认的 Document 记录并重新加载
			try
			{
				var project = doc.Project;
				if (project != null && Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				{
					var defaultDto = new Leqisoft.DTO.Document
					{
						Id = doc.Id,
						Version = doc.Version,
						Locker = 0,
						SectPr = Leqisoft.Model.Properties.Resource.DefaultSectPr,
						MergeTable = Leqisoft.DTO.Id64.Zero,
						Dirty = 0
					};
					project.Dal.SaveDocument(defaultDto);
					doc.ReloadFromDb();
				}
				if (doc.IsCorrupted)
				{
					DocBadFlag = true;
					return;
				}
			}
			catch
			{
				DocBadFlag = true;
				return;
			}
		}

		// 即使没有段落也尝试加载（MakePackage 会生成只含 SectPr 的空文档）
		try
		{
			EnsureControlCreated();
			LoadDocumentFromModel();
			_isDocumentLoaded = true;
		}
		catch (Exception ex)
		{
			ex.Log($"DocumentEditor.PopulateDocument DocId={doc.Id}");
			DocBadFlag = true;
		}
	}

	private void LoadDocumentFromModel()
	{
		var doc = Document as Leqisoft.Model.Document;
		if (doc == null) return;

		var tup = doc.MakePackage();
		if (tup == null || string.IsNullOrEmpty(tup.Item1) || !File.Exists(tup.Item1))
		{
			DocBadFlag = true;
			return;
		}

		// 清理上一次的临时文件
		CleanupTempFile();

		_lastTempFile = tup.Item1;
		byte[] fileBytes = File.ReadAllBytes(_lastTempFile);
		if (fileBytes.Length == 0)
		{
			DocBadFlag = true;
			return;
		}

		var loadSettings = new LoadSettings
		{
			ApplicationFieldFormat = ApplicationFieldFormat.MSWord
		};
		_textControl.Load(fileBytes, BinaryStreamType.WordprocessingML, loadSettings);
		_textControl.EditMode = EditMode.Edit;

		// 设置 ApplicationField 高亮模式，使域可见（始终高亮显示）
		try
		{
			var highlightColor = Color.FromArgb(80, 120, 160, 210);  // 半透明蓝底纹(仅屏幕显示，不打印)

			// 高亮正文中的域
			var fields = _textControl.ApplicationFields;
			if (fields != null && fields.Count > 0)
			{
				HighlightApplicationFields(fields, highlightColor);
			}

			// 高亮各节页眉/页脚中的域
			var sections = _textControl.Sections;
			if (sections != null && sections.Count > 0)
			{
				var en = sections.GetEnumerator();
				try
				{
					while (en.MoveNext())
					{
						var sec = en.Current as TXTextControl.Section;
						if (sec == null) continue;

						var hfCollection = sec.HeadersAndFooters;
						if (hfCollection == null) continue;

						HighlightHeaderFooterFields(hfCollection, TXTextControl.HeaderFooterType.Header, highlightColor);
						HighlightHeaderFooterFields(hfCollection, TXTextControl.HeaderFooterType.Footer, highlightColor);
					}
				}
				finally
				{
					var disp = en as IDisposable;
					if (disp != null) disp.Dispose();
				}
			}
		}
		catch (Exception)
		{
		}

		// 升级旧格式书签：FromDto 已将数据库中的 @ 转为 _
		// 但如果文档是从外部导入的（没有经过 FromDto 转换），DocumentTarget 中可能仍有 lsbm@
		try
		{
			var dts = _textControl.DocumentTargets;
			if (dts != null && dts.Count > 0)
			{
				foreach (TXTextControl.DocumentTarget dt in dts)
				{
					if (dt.TargetName != null && dt.TargetName.StartsWith("lsbm@"))
					{
						// 旧格式书签仍在 OOXML 中（外部导入），@2{TableId} 已被截断
						// 解析剩余部分，用 _ 格式重写（不含 TableId）
						if (LeqiBookmark.TryParse(dt.TargetName, out var lsbm))
						{
							_textControl.Select(dt.Start, 0);
							_textControl.DocumentTargets.Remove(dt);
							_textControl.DocumentTargets.Add(new TXTextControl.DocumentTarget(lsbm.GetString()));
						}
					}
				}
			}
		}
		catch (Exception) { }

		// 文档加载完成后执行文档校验健康检查
		try
		{
			var formulas = Leqisoft.Model.Project.Current?.ValidationManager?.Formulas;
			if (formulas != null && formulas.Count > 0)
			{
				var appFields = _textControl.ApplicationFields;
				if (appFields != null && appFields.Count > 0)
				{
					var fieldIds = new HashSet<Leqisoft.DTO.Id64>();
					foreach (TXTextControl.ApplicationField f in appFields)
					{
						if (f != null && f.Parameters != null && f.Parameters.Length >= 3)
						{
							long fid;
							if (long.TryParse(f.Parameters[2], out fid) && fid > 0)
								fieldIds.Add(new Leqisoft.DTO.Id64(fid));
						}
					}

					if (fieldIds.Count > 0)
					{
						var healthResult = DocumentValidationHealthChecker.Check(formulas, fieldIds);
						if (healthResult.HasIssues)
						{
							string msg = $"文档校验健康检查发现 {healthResult.OrphanCount} 条失效规则（关联域已丢失）。";
							if (healthResult.OrphanCount > 0)
							{
								msg += "\n\n失效规则：\n" + string.Join("\n", healthResult.Messages.Take(5));
								if (healthResult.Messages.Count > 5)
									msg += $"\n...及其他 {healthResult.Messages.Count - 5} 条";
							}
							System.Diagnostics.Trace.WriteLine(msg);
						}
					}
				}
			}
		}
		catch (Exception ex) { ex.Log("DocumentEditor.LoadDocumentFromModel - HealthCheck"); }
	}

	private void HighlightApplicationFields(TXTextControl.ApplicationFieldCollection fields, Color highlightColor)
	{
		// DocValidation 域的浅黄色底纹（区分于 Formula 域的半透明蓝）
		Color docValidationColor = Color.FromArgb(80, 255, 235, 130);
		foreach (ApplicationField f in fields)
		{
			if (f != null)
			{
				f.HighlightMode = HighlightMode.Always;
				f.DoubledInputPosition = true;

				// 按 Parameters[0] 区分域类型差异化设置属性
				bool isDocValidation = f.Parameters != null && f.Parameters.Length > 0
					&& f.Parameters[0] == "DocValidation";
				if (isDocValidation)
				{
					// DocValidation 域：用户可编辑披露数据，不可删除锚点
					f.HighlightColor = docValidationColor;
					f.Editable = true;
					f.Deleteable = false;
				}
				else
				{
					// Formula 域：机器生成求值结果，不可编辑，可删除
					f.HighlightColor = highlightColor;
					f.Editable = false;
					f.Deleteable = true;
				}
			}
		}
	}

	private void HighlightHeaderFooterFields(TXTextControl.HeaderFooterCollection hfCollection, TXTextControl.HeaderFooterType type, Color highlightColor)
	{
		var hf = hfCollection.GetItem(type);
		if (hf == null) return;
		var hfFields = hf.ApplicationFields;
		if (hfFields != null && hfFields.Count > 0)
		{
			HighlightApplicationFields(hfFields, highlightColor);
		}
	}

	private void CleanupTempFile()
	{
		if (!string.IsNullOrEmpty(_lastTempFile) && File.Exists(_lastTempFile))
		{
			try { File.Delete(_lastTempFile); } catch { }
			_lastTempFile = null;
		}
	}

	public void LoadDocPrint(params object[] args)
	{
		// 如果已经加载过，不再重复加载
		if (_isDocumentLoaded) return;

		var doc = Document as Leqisoft.Model.Document;
		if (doc == null || DocBadFlag) return;

		try
		{
			EnsureControlCreated();
			LoadDocumentFromModel();
			_isDocumentLoaded = true;
		}
		catch (Exception ex)
		{
			ex.Log($"DocumentEditor.LoadDocPrint DocId={doc.Id}");
		}
	}

	public void SaveToModel(params object[] args)
	{
		var doc = Document as Leqisoft.Model.Document;
		if (doc == null || _textControl == null) return;

		try
		{
			// 保存前确保 Document.MergeTable 已设置
			// 兼容旧文档：如果 MergeTable 缺失但从书签中可以恢复，则回填
			if (doc.MergeTable == null || doc.MergeTable.IsZero())
			{
				try
				{
					var dts = ((dynamic)_textControl).DocumentTargets;
					int count = dts.Count;
					for (int i = 1; i <= count; i++)
					{
						var dt = dts[i] as TXTextControl.DocumentTarget;
						if (dt != null && LeqiBookmark.TryParse(dt.TargetName, out var lsbm) && lsbm.TableId != null)
						{
							doc.MergeTable = Leqisoft.DTO.Id64.ParseBase64(lsbm.TableId);
							break;
						}
					}
				}
				catch { }
			}

			_textControl.Save(out byte[] docxBytes, BinaryStreamType.WordprocessingML);
			ParseDocxToParagraphs(doc, docxBytes);
			NeedSave = false;
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	private void ParseDocxToParagraphs(Leqisoft.Model.Document doc, byte[] docxBytes)
	{
		// 将 docx 保存为临时文件
		string tempFile = Path.GetTempFileName();
		try
		{
			File.WriteAllBytes(tempFile, docxBytes);

			// 使用 System.IO.Packaging 解析 docx 文件
			using (var package = System.IO.Packaging.Package.Open(tempFile, FileMode.Open, FileAccess.Read))
			{
				var docPart = package.GetPart(new Uri("/word/document.xml", UriKind.Relative));

				// 预加载图片 Part：rId -> Base64字符串
				var imageParts = new Dictionary<string, string>();
				foreach (var rel in docPart.GetRelationships())
				{
					if (rel.RelationshipType == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image")
					{
						try
						{
							var imgUri = PackUriHelper.ResolvePartUri(docPart.Uri, rel.TargetUri);
							var imgPart = package.GetPart(imgUri);
							using (var ms = new MemoryStream())
							{
								imgPart.GetStream().CopyTo(ms);
								imageParts[rel.Id] = Convert.ToBase64String(ms.ToArray());
							}
						}
						catch { /* 图片 Part 可能不存在 */ }
					}
				}

				// 预加载页眉/页脚 Part：rId -> XML字符串
				var headerFooterParts = new Dictionary<string, string>();
				foreach (var rel in docPart.GetRelationships())
				{
					if (rel.RelationshipType == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/header"
						|| rel.RelationshipType == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer")
					{
						try
						{
							var hfUri = PackUriHelper.ResolvePartUri(docPart.Uri, rel.TargetUri);
							var hfPart = package.GetPart(hfUri);
							using (var sr = new StreamReader(hfPart.GetStream()))
							{
								headerFooterParts[rel.Id] = sr.ReadToEnd();
							}
						}
						catch { }
					}
				}

				using (var stream = docPart.GetStream())
				using (var reader = new StreamReader(stream))
				{
					string xmlContent = reader.ReadToEnd();
					var xDoc = XDocument.Parse(xmlContent);
					var ns_w = Leqisoft.Model.Document.xmlns_w;
					var ns_r = Leqisoft.Model.Document.xmlns_r;
					var ns_a = Leqisoft.Model.Document.xmlns_a;

					var body = xDoc.Root.Element(ns_w + "body");
					if (body == null) return;

					// 收集所有段落和表格元素（排除只含 sectPr 的分节符段落，它们属于前一个段落的 Section）
					var rawElements = body.Elements().ToList();
					var elements = new List<XElement>();       // 实际段落/表格
					var sectionMap = new Dictionary<int, string>(); // index -> sectPr XML

					for (int i = 0; i < rawElements.Count; i++)
					{
						var el = rawElements[i];
						if (el.Name == ns_w + "p")
						{
							// 检查是否是分节符段落：<w:p><w:pPr><w:sectPr>...</w:sectPr></w:pPr></w:p>
							var pPr = el.Element(ns_w + "pPr");
							var sectPr = pPr?.Element(ns_w + "sectPr");
							if (sectPr != null)
							{
								// 这是分节符段落，属于前一个段落（或文档级 sectPr）
								// 还原页眉/页脚内容到 sectPr 中
								RestoreHeaderFooterInSectPr(sectPr, ns_w, ns_r, ns_a, headerFooterParts, imageParts);
								// 将 sectPr XML 关联到前一个元素
								int prevIdx = elements.Count - 1;
								if (prevIdx >= 0)
								{
									sectionMap[prevIdx] = sectPr.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
								}
								continue; // 不加入 elements 列表
							}
						}
						if (el.Name == ns_w + "p" || el.Name == ns_w + "tbl")
						{
							elements.Add(el);
						}
					}

					// 更新文档段落
					for (int i = 0; i < elements.Count; i++)
					{
						var el = elements[i];

						// 反向转换：hMerge -> gridSpan（表格合并单元格）
						if (el.Name.LocalName == "tbl")
						{
							FixHMergeToGridSpan(el, ns_w);
						}

						// 反向转换：图片 r:embed 引用 -> Base64 内嵌 <data> 元素
						foreach (var blip in el.Descendants(ns_a + "blip").ToList())
						{
							var embedAttr = blip.Attribute(ns_r + "embed");
							if (embedAttr != null && imageParts.TryGetValue(embedAttr.Value, out var base64))
							{
								// 移除 r:embed 属性，添加 <data> 子元素
								embedAttr.Remove();
								blip.Add(new XElement("data", new XAttribute("ContentType", "image/bmp"), base64));
							}
						}

						string elementXml = el.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
						string sectionXml = sectionMap.TryGetValue(i, out var s) ? s : null;

						if (i < doc.Paragraphs.Count)
						{
							doc.Paragraphs[i].UpdateStream(elementXml, sectionXml);
						}
						else
						{
							var para = new LqParagraph
							{
								Id = Project.Current.GetNextId(),
								Index = i,
								Stream = elementXml,
								Section = sectionXml,
								Status = SyncStatus.New,
								Document = doc
							};
							doc.Paragraphs.Add(para);
						}
					}

					// 删除多余的段落（用户在编辑器中删除内容后，docx 元素数量会少于原有段落数）
					while (doc.Paragraphs.Count > elements.Count)
					{
						int lastIdx = doc.Paragraphs.Count - 1;
						var removed = doc.Paragraphs[lastIdx];
						doc.Paragraphs.RemoveAt(lastIdx);
						doc.RemovedParagraphs.Add(removed.Id);
					}

					// 重新编号段落索引
					for (int i = 0; i < doc.Paragraphs.Count; i++)
					{
						doc.Paragraphs[i].Index = i;
					}

					// 更新文档级 SectPr（body 末尾的 sectPr）
					var docSectPr = body.Element(ns_w + "sectPr");
					if (docSectPr != null)
					{
						RestoreHeaderFooterInSectPr(docSectPr, ns_w, ns_r, ns_a, headerFooterParts, imageParts);
						doc.UpdateSectPr(docSectPr.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
					}
				}
			}
		}
		finally
		{
			try { File.Delete(tempFile); } catch { }
		}
	}

	/// <summary>
	/// 反向转换：hMerge -> gridSpan（TX TextControl 使用 hMerge，内部存储使用 gridSpan）
	/// 对应 MakePackage 中的 gridSpan -> hMerge 转换
	/// </summary>
	private static void FixHMergeToGridSpan(XElement tbl, XNamespace ns_w)
	{
		foreach (var row in tbl.Elements(ns_w + "tr").ToList())
		{
			var cells = row.Elements(ns_w + "tc").ToList();
			for (int ci = 0; ci < cells.Count; ci++)
			{
				var tc = cells[ci];
				var tcPr = tc.Element(ns_w + "tcPr");
				if (tcPr == null) continue;

				var hMerge = tcPr.Element(ns_w + "hMerge");
				if (hMerge == null) continue;

				var valAttr = hMerge.Attribute(ns_w + "val");
				// hMerge val="restart" 表示合并起始单元格
				if (valAttr != null && valAttr.Value == "restart")
				{
					// 计算合并跨度：统计后续 continue 的 tc 数量
					int span = 1;
					for (int k = ci + 1; k < cells.Count; k++)
					{
						var nextTcPr = cells[k].Element(ns_w + "tcPr");
						var nextHMerge = nextTcPr?.Element(ns_w + "hMerge");
						var nextVal = nextHMerge?.Attribute(ns_w + "val");
						// continue 可以是 val="continue" 或无 val 属性（默认 continue）
						if (nextHMerge != null && (nextVal == null || nextVal.Value == "continue"))
						{
							span++;
						}
						else
						{
							break;
						}
					}

					// 移除 hMerge，添加 gridSpan
					hMerge.Remove();
					if (span > 1)
					{
						tcPr.Add(new XElement(ns_w + "gridSpan", new XAttribute(ns_w + "val", span)));
					}
				}
			}

			// 移除所有 hMerge=continue 的 tc（它们是 MakePackage 插入的额外单元格）
			var continueCells = row.Elements(ns_w + "tc").Where(tc =>
			{
				var tcPr = tc.Element(ns_w + "tcPr");
				var hm = tcPr?.Element(ns_w + "hMerge");
				if (hm == null) return false;
				var v = hm.Attribute(ns_w + "val");
				return v == null || v.Value == "continue";
			}).ToList();

			foreach (var cc in continueCells)
			{
				cc.Remove();
			}
		}
	}

	/// <summary>
	/// 还原 sectPr 中的页眉/页脚内容：从独立的 header/footer Part 读取 XML 并嵌入到 sectPr 中
	/// 对应 MakePackage 中将 hdr/ftr 内容提取为独立 Part 的反向操作
	/// </summary>
	private static void RestoreHeaderFooterInSectPr(XElement sectPr, XNamespace ns_w, XNamespace ns_r, XNamespace ns_a,
		Dictionary<string, string> headerFooterParts, Dictionary<string, string> imageParts)
	{
		// 还原页眉
		foreach (var hdrRef in sectPr.Elements(ns_w + "headerReference").ToList())
		{
			var idAttr = hdrRef.Attribute(ns_r + "id");
			if (idAttr != null && headerFooterParts.TryGetValue(idAttr.Value, out var hdrXml))
			{
				try
				{
					var hdrElement = XElement.Parse(hdrXml);
					// 还原页眉中的图片引用为 Base64 内嵌
					foreach (var blip in hdrElement.Descendants(ns_a + "blip").ToList())
					{
						var embedAttr = blip.Attribute(ns_r + "embed");
						if (embedAttr != null && imageParts.TryGetValue(embedAttr.Value, out var base64))
						{
							embedAttr.Remove();
							blip.Add(new XElement("data", new XAttribute("ContentType", "image/bmp"), base64));
						}
					}
					// 将 hdr 内容嵌入到 sectPr 中
					sectPr.Add(hdrElement);
				}
				catch { }
			}
		}

		// 还原页脚
		foreach (var ftrRef in sectPr.Elements(ns_w + "footerReference").ToList())
		{
			var idAttr = ftrRef.Attribute(ns_r + "id");
			if (idAttr != null && headerFooterParts.TryGetValue(idAttr.Value, out var ftrXml))
			{
				try
				{
					var ftrElement = XElement.Parse(ftrXml);
					foreach (var blip in ftrElement.Descendants(ns_a + "blip").ToList())
					{
						var embedAttr = blip.Attribute(ns_r + "embed");
						if (embedAttr != null && imageParts.TryGetValue(embedAttr.Value, out var base64))
						{
							embedAttr.Remove();
							blip.Add(new XElement("data", new XAttribute("ContentType", "image/bmp"), base64));
						}
					}
					sectPr.Add(ftrElement);
				}
				catch { }
			}
		}
	}

	public void SetFontSize(int fontSize) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.FontSize = fontSize * 20); OnFormatChanged(); } }
	public void SetFontSize(float fontSize) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.FontSize = (int)(fontSize * 20f)); OnFormatChanged(); } }
	public void SetZoomFactor(int percentage) { _textControl.Zoom(percentage); }
	public void SetZoomFactor(TXTextControl.ZoomOption zo) { _textControl.Zoom(zo); }
	public void SetZoomFactorDialog() { var num = InputForm.Numeric("缩放比例", "输入文档界面缩放比例（百分比，100为原始大小）"); if (num.HasValue) { _textControl.ZoomFactor = (int)num.Value; } }
	public void Indent() { _textControl.Selection.IncreaseIndent(); }
	public void Unindent() { _textControl.Selection.DecreaseIndent(); }
	public void SetFont(FontFamily fontFamily) { SetFont(fontFamily.Name); }
	public void SetFont(string fontName) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.FontName = fontName); OnFormatChanged(); } }
	public void LoadDocument(string fileName)
	{
		try
		{
			var ext = System.IO.Path.GetExtension(fileName).ToLower();
			var streamType = ext == ".docx" ? TXTextControl.StreamType.WordprocessingML
				: ext == ".doc" ? TXTextControl.StreamType.MSWord
				: TXTextControl.StreamType.RichTextFormat;
			_textControl.Load(fileName, streamType);
		}
		catch (Exception ex) { ex.Log("DocumentEditor.LoadDocument"); }
	}

	public void SetBold(bool bold) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.Bold = bold); OnFormatChanged(); } }
	public void SetItalic(bool italic) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.Italic = italic); OnFormatChanged(); } }
	public void SetDoubleUnderline(bool underline) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.Underline = underline ? TXTextControl.FontUnderlineStyle.Doubled : TXTextControl.FontUnderlineStyle.None); OnFormatChanged(); } }

	public void SetAbsoluteParaBelowSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void SetParaAboveSpacing(params object[] args) { if (args?.Length > 0 && args[0] is double d) _textControl.Selection.ParagraphFormat.TopDistance = (int)(d * 1440); }
	public void SetAbsoluteParaAboveSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void SetParaBelowSpacing(params object[] args) { if (args?.Length > 0 && args[0] is double d) _textControl.Selection.ParagraphFormat.BottomDistance = (int)(d * 1440); }
	public void SetParaBelowSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void RibbonMiddleCenterClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Center, TXTextControl.HorizontalAlignment.Center); }
	public void RibbonBottomCenterClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Bottom, TXTextControl.HorizontalAlignment.Center); }
	public void RibbonBottomRightClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Bottom, TXTextControl.HorizontalAlignment.Right); }
	public void RibbonBottomLeftClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Bottom, TXTextControl.HorizontalAlignment.Left); }
	public void RibbonMiddleLeftClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Center, TXTextControl.HorizontalAlignment.Left); }
	public void RibbonTopRightClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Top, TXTextControl.HorizontalAlignment.Right); }
	public void RibbonTopLeftClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Top, TXTextControl.HorizontalAlignment.Left); }
	public void RibbonTopCenterClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Top, TXTextControl.HorizontalAlignment.Center); }
	public void RibbonMiddleRightClicked(params object[] args) { SetCellAlignment(TXTextControl.VerticalAlignment.Center, TXTextControl.HorizontalAlignment.Right); }
	public void RibbonDecreaseColumnWidthClicked(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		foreach (TXTextControl.TableColumn col in table.Columns)
		{
			try { col.Width = Math.Max(200, col.Width - 200); } catch { }
		}
	}
	public void RibbonDecreaseRowHeightClicked(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		foreach (TXTextControl.TableRow row in table.Rows)
		{
			try { row.MinimumHeight = Math.Max(0, row.MinimumHeight - 100); } catch { }
		}
	}
	public void RibbonIncreaseColumnWidthClicked(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		foreach (TXTextControl.TableColumn col in table.Columns)
		{
			try { col.Width += 200; } catch { }
		}
	}
	public void RibbonIncreaseRowHeightClicked(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		foreach (TXTextControl.TableRow row in table.Rows)
		{
			try { row.MinimumHeight += 100; } catch { }
		}
	}
	public void RibbonSetTableBorder(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		if (args?.Length > 0 && args[0] is Leqisoft.Model.TableBorderStyle style)
		{
			int thinWidth = 15;
			int thickWidth = 30;
			int upDownWidth = style.UpDownLine == Leqisoft.Model.LineStyle.Thick ? thickWidth : (style.UpDownLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
			int leftRightWidth = style.LeftRightLine == Leqisoft.Model.LineStyle.Thick ? thickWidth : (style.LeftRightLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
			int bodyWidth = style.BodyLine == Leqisoft.Model.LineStyle.Thick ? thickWidth : (style.BodyLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
			int secondWidth = style.SecondLine == Leqisoft.Model.LineStyle.Thick ? thickWidth : (style.SecondLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);

			foreach (TXTextControl.TableCell cell in table.Cells)
			{
				try
				{
					var fmt = cell.CellFormat;
					int row = cell.Row;
					int col = cell.Column;
					fmt.TopBorderWidth = (row == 1) ? upDownWidth : secondWidth;
					fmt.BottomBorderWidth = (row == table.Rows.Count) ? upDownWidth : bodyWidth;
					fmt.LeftBorderWidth = (col == 1) ? leftRightWidth : bodyWidth;
					fmt.RightBorderWidth = (col == table.Columns.Count) ? leftRightWidth : bodyWidth;
					cell.CellFormat = fmt;
				}
				catch { }
			}
		}
	}
	public void PageSettingTargetDocumentClicked(params object[] args)
	{
		this.PageSettingTarget = Leqisoft.UI.Platform.PageSettingTarget.Document;
		AppCommands.ApplySelection.IsPressed = false;
	}

	public void PageSettingTargetSelectionClicked(params object[] args)
	{
		this.PageSettingTarget = Leqisoft.UI.Platform.PageSettingTarget.Selection;
		AppCommands.ApplyDocument.IsPressed = false;
	}
	public void SetBold(params object[] args) { SetBold(args?.Length > 0 && args[0] is bool b ? b : true); }
	public void Copy(params object[] args) { _textControl.Copy(); }
	public void Cut(params object[] args) { _textControl.Cut(); }
	public void Paste(params object[] args) { _textControl.Paste(); }
	public void Undo(params object[] args) { if (_textControl.CanUndo) _textControl.Undo(); }
	public void Redo(params object[] args) { if (_textControl.CanRedo) _textControl.Redo(); }
	public void SetItalic(params object[] args) { SetItalic(args?.Length > 0 && args[0] is bool b ? b : true); }
	public void SetUnderline(params object[] args) { if (!IsDocumentLocked()) { bool underline = !(args?.Length > 0 && args[0] is bool b && !b); PreserveSelection(sel => sel.Underline = underline ? TXTextControl.FontUnderlineStyle.Single : TXTextControl.FontUnderlineStyle.None); OnFormatChanged(); } }
	public void SetDoubleUnderline(params object[] args) { SetDoubleUnderline(args?.Length > 0 && args[0] is bool b ? b : true); }
	public void Subscript(params object[] args) { _textControl.InputFormat.Subscript = !(args?.Length > 0 && args[0] is bool b && !b); }
	public void Superscript(params object[] args) { _textControl.InputFormat.Superscript = !(args?.Length > 0 && args[0] is bool b && !b); }
	public void SetForeColor(params object[] args) { if (args?.Length > 0 && args[0] is Color c) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.ForeColor = c); OnFormatChanged(); } } }
	public void SetBackColor(params object[] args) { if (args?.Length > 0 && args[0] is Color c) { if (!IsDocumentLocked()) { PreserveSelection(sel => sel.TextBackColor = c); OnFormatChanged(); } } }
	public void SetSidaStyle(params object[] args) { RibbonSetTableBorder(new object[] { Leqisoft.Model.TableBorderStyles.ThickBorderThinBody }); }
	public void SetLineSpacing(params object[] args) { if (args?.Length > 0 && args[0] is int pct) _textControl.Selection.ParagraphFormat.LineSpacing = pct; }
	public void SetLineSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void SetAbsoluteLineSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void SetParaAboveSpacingDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }
	public void AlignLeft(params object[] args) { _textControl.Selection.ParagraphFormat.Alignment = TXTextControl.HorizontalAlignment.Left; }
	public void AlignCenter(params object[] args) { _textControl.Selection.ParagraphFormat.Alignment = TXTextControl.HorizontalAlignment.Center; }
	public void AlignRight(params object[] args) { _textControl.Selection.ParagraphFormat.Alignment = TXTextControl.HorizontalAlignment.Right; }
	public void AlignJustify(params object[] args) { _textControl.Selection.ParagraphFormat.Alignment = TXTextControl.HorizontalAlignment.Justify; }
	public void GrowFont(params object[] args) { _textControl.Selection.GrowFont(); }
	public void ShrinkFont(params object[] args) { _textControl.Selection.ShrinkFont(); }
	public void FontDialog(params object[] args) { _textControl.FontDialog(); }
	public void FirstIndentIncrease(params object[] args) { _textControl.Selection.ParagraphFormat.LeftIndent += 720; }
	public void FirstIndentDecrease(params object[] args) { _textControl.Selection.ParagraphFormat.LeftIndent = Math.Max(0, _textControl.Selection.ParagraphFormat.LeftIndent - 720); }
	public void InsertTable(params object[] args)
	{
		if (IsDocumentLocked() || GetCurrentApplicationField() != null || GetParaStartApplicationField() != null || GetCurrentTable() != null)
		{
			return;
		}
		DetachEvents();
		OnChanged();
		if (_textControl.Tables.Add())
		{
			_textControl.Select(_textControl.Selection.Start - 1, 0);
			TXTextControl.Table currentTable = GetCurrentTable();
			SetTableBorder(currentTable, Leqisoft.Model.TableBorderStyles.Grid);
			foreach (TXTextControl.TableRow row in currentTable.Rows)
			{
				row.MinimumHeight = 567;
			}
			foreach (TXTextControl.TableColumn column in currentTable.Columns)
			{
				column.CellFormat.VerticalAlignment = TXTextControl.VerticalAlignment.Center;
			}
			currentTable.Select();
			_textControl.Selection.ParagraphFormat.LeftIndent = 0;
			_textControl.Selection.ParagraphFormat.HangingIndent = 0;
			_textControl.Select(currentTable.Cells.GetItem(1, 1).Start - 1, 0);
		}
		_textControl.ClearUndo();
		AttachEvents();
	}
	public void InsertImage(params object[] args)
	{
		using (var dlg = new OpenFileDialog { Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|所有文件|*.*" })
		{
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var img = System.Drawing.Image.FromFile(dlg.FileName);
				_textControl.Images.Add(new TXTextControl.Image(img), _textControl.InputPosition.TextPosition);
			}
		}
	}
	public void InsertHeader(params object[] args)
	{
		if (IsDocumentLocked()) return;
		_textControl.HeadersAndFooters.Add(TXTextControl.HeaderFooterType.Header);
		var hf = _textControl.HeadersAndFooters.GetItem(TXTextControl.HeaderFooterType.Header);
		if (hf != null)
		{
			hf.ConnectedToPrevious = false;
			hf.Activate();
		}
	}
	public void InsertFooter(params object[] args)
	{
		if (IsDocumentLocked()) return;
		_textControl.HeadersAndFooters.Add(TXTextControl.HeaderFooterType.Footer);
		var hf = _textControl.HeadersAndFooters.GetItem(TXTextControl.HeaderFooterType.Footer);
		if (hf != null)
		{
			hf.ConnectedToPrevious = false;
			hf.Activate();
		}
	}
	public void InsertPageBreak(params object[] args) { _textControl.Sections.Add(TXTextControl.SectionBreakKind.BeginAtNewPage); }
	public void InsertSectionBreak(params object[] args) { _textControl.Sections.Add(TXTextControl.SectionBreakKind.BeginAtNewLine); }
	public void InsertTextFrame(params object[] args) { _textControl.TextFrames.Add(new TXTextControl.TextFrame(new System.Drawing.Size(3000, 2000)), _textControl.InputPosition.TextPosition); }
	public void InsertMergeField(params object[] args)
	{
		var field = new TXTextControl.ApplicationField(
			TXTextControl.ApplicationFieldFormat.MSWord,
			"MERGEFIELD",
			"«FieldName»",
			new string[] { "Formula", "" }
		);
		_textControl.ApplicationFields.Add(field);
	}

	/// <summary>
	/// 将当前选区转换为 DocValidation 域（选区转域校验功能）。
	/// 选区文字保留为域内文本，用户可在弹出的对话框中编辑校验规则。
	/// </summary>
	public void AddValidationPoint()
	{
		if (_textControl == null) return;
		if (IsDocumentLocked()) return;
		if (GetCurrentApplicationField() != null) return;  // 不允许在已有域内嵌套

		// 1. 检查选区是否非空
		if (_textControl.Selection.Length == 0) return;

		// 2. 提取选区文字与位置（对话框可能令控件失焦，先保存）
		int selStart = _textControl.Selection.Start;
		int selLength = _textControl.Selection.Length;
		string selectedText = _textControl.Selection.Text;

		// 3. 提取数字作为默认左值
		double? defaultValue = ExtractNumber(selectedText);

		// 4. 弹出校验规则编辑对话框
		using (var dlg = new frmDocValidationRuleEditor())
		{
			dlg.LeftValue = defaultValue?.ToString() ?? selectedText;
			dlg.SelectedText = selectedText;
			if (dlg.ShowDialog() != DialogResult.OK) return;

			// 5. 创建 DocValidation 域包裹选区
			Leqisoft.DTO.Id64 fieldId = Project.Current.GetNextId();
			string ruleJson = dlg.GetRuleJson();

			// 恢复选区（对话框期间可能改变）
			_textControl.Select(selStart, selLength);

			var field = new TXTextControl.ApplicationField(
				TXTextControl.ApplicationFieldFormat.MSWord,
				"MERGEFIELD",
				selectedText,  // 保留原文
				new string[] { "DocValidation", ruleJson, fieldId.ToString() }
			)
			{
				Deleteable = false,
				Editable = true,
				DoubledInputPosition = true,
				HighlightMode = TXTextControl.HighlightMode.Always,
				HighlightColor = Color.FromArgb(80, 255, 235, 130)  // 浅黄
			};

			// 6. 用域替换选区
		_textControl.ApplicationFields.Add(field);
		OnChanged();
		}
	}

	/// <summary>
	/// 解除当前光标所在的 DocValidation 域：保留文字内容，移除域锚点及关联的校验规则。
	/// </summary>
	public void RemoveValidationPoint()
	{
		if (_textControl == null) return;

		// 1. 获取当前光标所在的 DocValidation 域
		var field = GetCurrentApplicationField();
		if (field == null || field.TypeName != "MERGEFIELD"
			|| field.Parameters == null || field.Parameters.Length < 1
			|| field.Parameters[0] != "DocValidation") return;

		// 2. 确认删除
		if (Leqisoft.UI.Controls.MessageBox.Show(
				MessageBoxIcon.Question,
				"确定要解除此校验点吗？文字内容将保留，校验规则将被删除。")
			!= DialogResult.Yes) return;

		// 3. 提取域 Id 用于删除关联规则
		long fieldId = 0;
		if (field.Parameters.Length >= 3)
		{
			long.TryParse(field.Parameters[2], out fieldId);
		}

		// 4. 移除域（保留文字）
		try
		{
			_textControl.ApplicationFields.Remove(field, keepText: true);
		}
		catch (Exception ex)
		{
			ex.Log("RemoveValidationPoint");
			return;
		}

		// 5. 删除关联的校验规则
		if (fieldId > 0)
		{
			try
			{
				// 从 ValidationManager 中查找并删除 DocumentFieldId 匹配的规则
				var rulesToRemove = Leqisoft.Model.Project.Current.ValidationManager.Formulas
					.Where(f => f.DocumentFieldId == new Leqisoft.DTO.Id64(fieldId))
					.ToList();
				foreach (var rule in rulesToRemove)
				{
					Leqisoft.Model.Project.Current.ValidationManager.RemoveOne(rule);
				}
			}
			catch (Exception ex) { ex.Log("RemoveValidationPoint - remove rules"); }
		}

		OnChanged();
	}

	/// <summary>
	/// 判断指定 ApplicationField 是否为 DocValidation 域。
	/// </summary>
	private static bool IsDocValidationField(TXTextControl.ApplicationField field)
	{
		return field != null
			&& field.TypeName == "MERGEFIELD"
			&& field.Parameters != null
			&& field.Parameters.Length >= 1
			&& field.Parameters[0] == "DocValidation";
	}

	/// <summary>
	/// 为当前光标所在的 Formula 域添加稽核规则。
	/// 将规则写入域 Parameters[3]，并同步创建 ValidationFormula 记录。
	/// </summary>
	public void AddValidationRuleToFormulaField()
	{
		if (_textControl == null) return;
		if (IsDocumentLocked()) return;

		// 1. 获取当前光标所在的 ApplicationField，必须是 Formula 域
		var field = GetCurrentApplicationField();
		if (field == null || field.TypeName != "MERGEFIELD"
			|| field.Parameters == null || field.Parameters.Length < 2
			|| field.Parameters[0] != "Formula") return;

		// 2. 求值当前公式作为默认左值
		string formula = field.Parameters[1];
		string currentValue = string.Empty;
		try
		{
			currentValue = FormulaEditor.EvaluateDocumentFormula(formula, calculateTable: false);
		}
		catch { }

		// 3. 弹出规则编辑对话框（复用 frmDocValidationRuleEditor）
		using (var dlg = new frmDocValidationRuleEditor())
		{
			dlg.LeftValue = currentValue;
			dlg.SelectedText = field.Text;
			if (dlg.ShowDialog() != DialogResult.OK) return;

			// 4. 将稽核规则写入域 Parameters[3]
			string ruleJson = dlg.GetRuleJson();
			var newParams = new List<string>(field.Parameters);
			while (newParams.Count < 4) newParams.Add(string.Empty);
			newParams[3] = ruleJson;
			field.Parameters = newParams.ToArray();

			// 5. 同步到数据库（创建 ValidationFormula 记录）
			try
			{
				long fieldId = long.Parse(field.Parameters[2]);
				var vf = new Leqisoft.Model.ValidationFormula
				{
					Id = Leqisoft.Model.Project.Current.GetNextId(),
					LeftExpr = formula,
					RightExpr = dlg.RightExpression,
					Note = dlg.Note,
					DocumentFieldId = new Leqisoft.DTO.Id64(fieldId),
					IsDirty = true,
					Status = Leqisoft.Model.SyncStatus.New
				};
				vf.Operator = dlg.GetOperator();

				// 移除同一文档域的旧规则，避免重复
				var existing = Leqisoft.Model.Project.Current.ValidationManager.Formulas
					.FindAll(f => f.DocumentFieldId == vf.DocumentFieldId);
				foreach (var old in existing)
				{
					Leqisoft.Model.Project.Current.ValidationManager.Formulas.Remove(old);
				}

				Leqisoft.Model.Project.Current.ValidationManager.Formulas.Add(vf);
			}
			catch (Exception ex) { ex.Log("AddValidationRuleToFormulaField"); }

			OnChanged();
		}
	}

	/// <summary>
	/// 判断指定 ApplicationField 是否为 Formula 域。
	/// </summary>
	private static bool IsFormulaField(TXTextControl.ApplicationField field)
	{
		return field != null
			&& field.TypeName == "MERGEFIELD"
			&& field.Parameters != null
			&& field.Parameters.Length >= 2
			&& field.Parameters[0] == "Formula";
	}

	/// <summary>
	/// 从文本中提取第一个数值（支持千分位逗号、小数）。
	/// </summary>
	private static double? ExtractNumber(string text)
	{
		if (string.IsNullOrEmpty(text)) return null;
		var match = System.Text.RegularExpressions.Regex.Match(text, @"[\d,]+\.?\d*");
		if (match.Success)
		{
			string numStr = match.Value.Replace(",", "");
			if (double.TryParse(numStr, out double result))
				return result;
		}
		return null;
	}
	public void InsertVariable(params object[] args)
	{
		var field = new TXTextControl.TextField("Variable");
		_textControl.TextFields.Add(field);
	}
	public void MergeCells(params object[] args)
	{
		var table = _textControl.Tables.GetItem();
		if (table != null && table.CanMergeCells) table.MergeCells();
	}
	public void UnmergeCells()
	{
		if (IsDocumentLocked())
		{
			return;
		}
		GetCurrentTable()?.SplitCells();
	}
	public void ImportTable()
	{
		if (IsDocumentLocked()) return;
		if (GetCurrentApplicationField() != null) return;
		if (GetParaStartApplicationField() != null) return;
		if (GetCurrentTable() != null) return;

		var form = new FormSelectNode();
		form.Project = Program.MainForm.CurrentProject;
		form.ShowImportTable();
		var selectedTableNode = form.SelectedTableNode;
		if (selectedTableNode == null) return;
		InsertModelTable(selectedTableNode.Table);
	}
	public void ImportTable(params object[] args) { ImportTable(); }

	private void InsertModelTable(Leqisoft.Model.Table table)
	{
		if (!_textControl.Tables.CanAdd)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前插入点不允许插入表格。");
			return;
		}

		table.LoadAndReturn(false);
		table.CalculateRecursive();
		if (table.Columns.VisibleCount == 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格为空！");
			return;
		}

		canRibbonStatusSetting = false;
		DetachEvents();
		Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
		OnChanged();

		try
		{
			_textControl.Selection.Text = "\n";
			_textControl.Tables.Add(table.Rows.Count + getCaptionRows(table), table.Columns.VisibleCount);
			_textControl.Select(_textControl.Selection.Start - 1, 0);
			var txTable = GetCurrentTable();

			txTable.Select();
			SetLineSpacing(100);
			SetParaAboveSpacing(0.0);
			SetParaBelowSpacing(0.0);
			_textControl.Selection.ParagraphFormat.HangingIndent = 0;
			_textControl.Selection.ParagraphFormat.LeftIndent = 0;

			SetColumnWidth(txTable, table);
			SetRowHeight(txTable, table);
			SetDefaultStyleWithFormat(txTable, table.DefaultStyle);
			SetColumnStyleWithFormat(txTable, table);
			SetCaptionStyleWithFormat(txTable, table);
			SetCaptionContentsWithFormat(txTable, table);
			MergeCaptionCells(table, txTable);
			SetBodyContentsWithFormat(txTable, table);
			MergeBodyCells(txTable, table);

			txTable.Select();

			if (AppCommands.TableStyle4.IsSelected)
			{
				SetSidaStyle(txTable);
			}
			else if (AppCommands.PendingCustomStyle != null && AppCommands.PendingCustomStyle.IsCustomStyle)
			{
				// 自定义样式：应用 PendingCustomStyle
				SetCustomTableBorder(txTable, AppCommands.PendingCustomStyle, getCaptionRows(table));
			}
			else
			{
				var borderStyle = AppCommands.TableStyle.SelectedStyle ?? Leqisoft.Model.TableBorderStyles.Grid;
				SetTableBorder(txTable, borderStyle, getCaptionRows(table));
			}

			Application.DoEvents();

			var lsbm = new LeqiBookmark
			{
				ParaIdBase64 = Project.Current.GetNextId().ToBase64(),
				Status = LeqiBookmarkStatus.New,
				TableId = table.Id.ToBase64()
			};
			if (AppCommands.TableStyle4.IsSelected)
			{
				lsbm.TableStyle = 5;
			}
			else if (AppCommands.PendingCustomStyle != null && AppCommands.PendingCustomStyle.IsCustomStyle)
			{
				// 自定义样式：记录 TableStyle=6 并序列化 JSON
				lsbm.TableStyle = Leqisoft.Model.TableBorderStyles.Custom;
				lsbm.CustomBorderStyleJson = AppCommands.PendingCustomStyle.ToJson();
				// 使用后清空，避免污染后续插入
				AppCommands.PendingCustomStyle = null;
			}
			else
			{
				lsbm.TableStyle = (AppCommands.TableStyle.SelectedStyle ?? Leqisoft.Model.TableBorderStyles.Grid).InternalNumber;
			}

			AddNewBookmark(txTable, lsbm);
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "发生错误：" + ex.Message);
		}
		finally
		{
			_textControl.ClearUndo();
			canRibbonStatusSetting = true;
			Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
			AttachEvents();
		}
	}

	private int PixelToTwip(int pixels)
	{
		using (var g = _textControl.CreateGraphics())
		{
			return (int)((float)pixels / g.DpiX * 1440f);
		}
	}

	private int getCaptionRows(Leqisoft.Model.Table table)
	{
		int result = 0;
		if (table.HeaderMode != (Leqisoft.Model.TableHeaderMode)1)
			result = table.GetNumVisibleCaptionRows();
		return result;
	}

	private void AddNewBookmark(TXTextControl.Table table, LeqiBookmark lsbm)
	{
		try
		{
			var item = table.Cells.GetItem(1, 1);
			int start = item.Start - 1;
			_tx.Select(start, 0);
			DocumentTarget documentTarget = new DocumentTarget(lsbm.GetString());
			_tx.DocumentTargets.Add(documentTarget);
		}
		catch (Exception ex) { ex.Log("DocumentEditor.AddNewBookmark"); }
	}

	private bool _isCheckingTableRef;
	private bool _isRefreshingTable;
	private int _lastTableId;
	/// <summary>
	/// 缓存书签 ParaId→TableId 映射，防止 OOXML 保存后 TableId 丢失。
	/// 每次成功找到 TableId 时更新，找不到时从中恢复。
	/// </summary>
	private Dictionary<string, string> _bookmarkTableIdCache = new Dictionary<string, string>();

	/// <summary>
	/// 通过书签的 ParaIdBase64 查找来源表格。
	/// 当书签没有 TableId 时（旧格式书签或 OOXML 保存丢失 TableId），
	/// 使用 Paragraph 的 Stream 内容中的 ApplicationField 查找引用的 TableId。
	/// </summary>
	private Leqisoft.Model.Table FindTableByParaId(string paraIdBase64)
	{
		if (string.IsNullOrEmpty(paraIdBase64)) return null;
		try
		{
			var paraId = Leqisoft.DTO.Id64.ParseBase64(paraIdBase64);
			var doc = Document as Leqisoft.Model.Document;
			var project = Program.MainForm.CurrentProject;

			// 先检查缓存中是否有这个 ParaId 的 TableId
			if (_bookmarkTableIdCache.TryGetValue(paraIdBase64, out var cachedTableId))
			{
				if (project != null)
				{
					var cachedId = Leqisoft.DTO.Id64.ParseBase64(cachedTableId);
					var cachedTable = project.GetTableById(cachedId);
					if (cachedTable != null)
					{
						return cachedTable;
					}
				}
			}

			// 用项目表格查找（ParaId 可能就是 TreeTableNode.Id）
			if (project != null)
			{
				var tableById = project.GetTableById(paraId);
				if (tableById != null)
				{
					return tableById;
				}
				var nodeById = project.GetNodeById(paraId);
				if (nodeById is Leqisoft.Model.TreeTableNode tableNode)
				{
					return tableNode.Table;
				}
			}

			// 在 Document 的 Paragraphs 中查找对应 Paragraph
			var paragraph = doc?.Paragraphs.FirstOrDefault(p => p.Id.Equals(paraId));
			if (paragraph == null)
			{
				return null;
			}

			// 从 Paragraph 的 Stream 中解析书签，获取完整的 LeqiBookmark（可能包含 TableId）
			if (!string.IsNullOrEmpty(paragraph.Stream))
			{
				try
				{
					var xElement = System.Xml.Linq.XElement.Parse(paragraph.Stream);
					var ns = xElement.GetNamespaceOfPrefix("w");
					var bookmarkStart = xElement.Descendants(ns + "bookmarkStart").FirstOrDefault();
					if (bookmarkStart != null)
					{
						var name = (string)bookmarkStart.Attribute(ns + "name");
					LeqiBookmark bm = null;
						if (!string.IsNullOrEmpty(name) && LeqiBookmark.TryParse(name, out bm) && bm.TableId != null)
						{
							var id = Leqisoft.DTO.Id64.ParseBase64(bm.TableId);
							var table = project?.GetTableById(id);
							if (table != null)
							{
								return table;
							}
						}
					}
					else
					{
					}
				}
				catch (Exception)
				{
				}
			}
			else
			{
			}

			// 回退：从 Paragraph 的 Stream 中查找 MERGEFIELD Formula 域，提取引用的 TableId
			if (!string.IsNullOrEmpty(paragraph.Stream))
			{
				try
				{
					var xElement = System.Xml.Linq.XElement.Parse(paragraph.Stream);
					var ns = xElement.GetNamespaceOfPrefix("w");
					foreach (var instrText in xElement.Descendants(ns + "instrText"))
					{
						var text = instrText.Value?.Trim();
						if (text != null && text.StartsWith("MERGEFIELD Formula", StringComparison.OrdinalIgnoreCase))
						{
							// 提取公式参数
							var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 3)
							{
								var formula = parts.Skip(2).FirstOrDefault(p => !p.StartsWith("\\"));
								if (!string.IsNullOrWhiteSpace(formula))
								{
									var evaluator = new FormulaEvaluator(formula);
									var ids = evaluator.GetReferredTableIds();
									if (ids.Count > 0)
									{
										var table = project?.GetTableById(ids.First());
										if (table != null)
									{
										return table;
									}
									}
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}

			// 最后尝试 MergeTable 回退（仅当 doc 本身有 MergeTable 设定时）
			if (doc != null && doc.MergeTable != null && !doc.MergeTable.IsZero())
			{
				var mergeTable = project?.GetTableById(new Leqisoft.DTO.Id64(doc.MergeTable.Value));
				if (mergeTable != null)
				{
					return mergeTable;
				}
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	private bool GetRefTable(TXTextControl.Table txTable, out LeqiBookmark bookmark, out Leqisoft.Model.Table refTable)
	{
		refTable = null;
		bookmark = null;
		try
		{
			var firstCell = txTable.Cells.GetItem(1, 1);
			if (firstCell == null) return false;
			int targetPos = firstCell.Start - 1;

			// 使用守卫标志防止 Select 触发 InputPositionChanged 递归
			_isCheckingTableRef = true;
			try
			{
				// 方法1：Select + GetItem 方式查找表格前面的 DocumentTarget 书签
			_tx.Select(targetPos, 0);
			DocumentTarget item = _tx.DocumentTargets.GetItem();

			if (item != null && !string.IsNullOrEmpty(item.TargetName) && LeqiBookmark.TryParse(item.TargetName, out bookmark))
			{
				// 1a：书签有 TableId，直接查找
				if (bookmark.TableId != null)
					{
						// 缓存 ParaId→TableId 映射
						if (bookmark.ParaIdBase64 != null)
							_bookmarkTableIdCache[bookmark.ParaIdBase64] = bookmark.TableId;
						Leqisoft.DTO.Id64 id = Leqisoft.DTO.Id64.ParseBase64(bookmark.TableId);
						var project = Program.MainForm.CurrentProject;
						refTable = project?.GetTableById(id);
						return refTable != null;
					}
					// 1b：书签没有 TableId，先从缓存恢复
					if (bookmark.ParaIdBase64 != null)
					{
						if (_bookmarkTableIdCache.TryGetValue(bookmark.ParaIdBase64, out var cachedTableId))
						{
							bookmark.TableId = cachedTableId;
							Leqisoft.DTO.Id64 id = Leqisoft.DTO.Id64.ParseBase64(cachedTableId);
							var project = Program.MainForm.CurrentProject;
							refTable = project?.GetTableById(id);
							if (refTable != null) return true;
						}
						refTable = FindTableByParaId(bookmark.ParaIdBase64);
						if (refTable != null)
						{
							return true;
						}
					}
				}

				// 方法2：枚举所有 DocumentTarget，查找位置在表格前面的最近 LeqiBookmark
				var allTargets = _textControl.DocumentTargets;
				if (allTargets != null)
				{
					int tableStart = firstCell.Start;
					LeqiBookmark bestBookmark = null;
					int bestDistance = int.MaxValue;

					foreach (DocumentTarget dt in allTargets)
					{
						try
						{
							if (string.IsNullOrEmpty(dt.TargetName)) continue;
							if (!LeqiBookmark.TryParse(dt.TargetName, out var bm)) continue;
							if (bm.TableId == null && bm.ParaIdBase64 == null) continue;

							int dtStart = dt.Start;
							int distance = tableStart - dtStart;
							// 放宽范围到 500，应对刷新后表格位置变化
							if (distance > 0 && distance < 500 && distance < bestDistance)
							{
								bestDistance = distance;
								bestBookmark = bm;
							}
						}
						catch { }
					}

					if (bestBookmark != null)
					{
						bookmark = bestBookmark;
						if (bestBookmark.TableId != null)
						{
							// 缓存 ParaId→TableId 映射
							if (bestBookmark.ParaIdBase64 != null)
								_bookmarkTableIdCache[bestBookmark.ParaIdBase64] = bestBookmark.TableId;
							Leqisoft.DTO.Id64 id = Leqisoft.DTO.Id64.ParseBase64(bestBookmark.TableId);
							var project = Program.MainForm.CurrentProject;
							refTable = project?.GetTableById(id);
						}
						else if (bestBookmark.ParaIdBase64 != null)
						{
							// 先检查缓存
							if (_bookmarkTableIdCache.TryGetValue(bestBookmark.ParaIdBase64, out var cachedTableId))
							{
								bestBookmark.TableId = cachedTableId;
								Leqisoft.DTO.Id64 id = Leqisoft.DTO.Id64.ParseBase64(cachedTableId);
								var project = Program.MainForm.CurrentProject;
								refTable = project?.GetTableById(id);
							}
							else
							{
								refTable = FindTableByParaId(bestBookmark.ParaIdBase64);
							}
						}
					return refTable != null;
					}
				}

			}
			finally
			{
				try { _tx.Select(firstCell.Start, 0); } catch { }
				_isCheckingTableRef = false;
			}
		}
		catch (Exception ex) { ex.Log("DocumentEditor.GetRefTable"); }
		return false;
	}

	// ==== Table formatting helper methods (from original IL) ====

	private void SetColumnWidth(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int totalWidth = PixelToTwip(modelTable.Columns.WhereVisible.Sum(c => c.Width));
		int actualWidth = 0;
		foreach (TXTextControl.TableColumn col in txTable.Columns)
			actualWidth += col.Width;
		double ratio = (double)actualWidth / totalWidth;
		foreach (TXTextControl.TableColumn col in txTable.Columns)
		{
			var modelCol = modelTable.Columns.GetVisibleColumnAt(col.Column - 1);
			if (modelCol == null) continue;
			int newWidth = (int)(PixelToTwip(modelCol.Width) * ratio);
			if (col.Width != newWidth)
				col.Width = newWidth;
		}
	}

	private void SetRowHeight(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		foreach (TXTextControl.TableRow row in txTable.Rows)
		{
			int rowIdx = row.Row - 1;
			int height = 30;
			if (rowIdx < capRows)
				height = PixelToTwip(modelTable.GetHeaderHeight(rowIdx));
			else
				height = PixelToTwip(modelTable.Rows[rowIdx - capRows].Height);
			if (row.MinimumHeight != height)
				row.MinimumHeight = height;
		}
	}

	private void SetDefaultStyleWithFormat(TXTextControl.Table txTable, Leqisoft.Model.CellStyle style)
{
	if (style == null) return;
	txTable.Select();
	SetSelectionStyleWithFormat(style);
}

	private void SetColumnStyleWithFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		foreach (TXTextControl.TableColumn col in txTable.Columns)
		{
			var modelCol = modelTable.Columns.GetVisibleColumnAt(col.Column - 1);
			if (modelCol.Style == null) continue;
			col.Select();
			SetSelectionStyleWithFormat(modelCol.Style);
		}
	}

	private void SetCaptionStyleWithFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		foreach (TXTextControl.TableColumn col in txTable.Columns)
		{
			var modelCol = modelTable.Columns.GetVisibleColumnAt(col.Column - 1);
			txTable.Select(1, col.Column, capRows, col.Column);
			SetSelectionStyleWithFormat(modelCol.CaptionStyle);

			if (modelCol.CaptionStyle.Align.HasValue)
			{
				ConvertAlign(modelCol.CaptionStyle.Align.Value, out var hAlign, out var vAlign);
				if (_tx.Selection.ParagraphFormat.Alignment != hAlign)
					_tx.Selection.ParagraphFormat.Alignment = hAlign;

				for (int r = 1; r <= capRows; r++)
				{
					var cell = txTable.Cells.GetItem(r, col.Column);
					if (cell != null && cell.CellFormat.VerticalAlignment != vAlign)
						cell.CellFormat.VerticalAlignment = vAlign;
				}
			}
		}
	}

	private void SetCaptionContentsWithFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		for (int r = 0; r < capRows; r++)
		{
			for (int c = 0; c < modelTable.Columns.VisibleCount; c++)
			{
				var cell = txTable.Cells.GetItem(r + 1, c + 1);
				var parts = modelTable.Columns.GetVisibleColumnAt(c).CaptionDisplay.Split('_').ToList();
				string text = r < parts.Count ? parts[r] : string.Empty;
				if (cell.Text != text)
					cell.Text = text;
			}
			Application.DoEvents();
		}
	}

	private void MergeCaptionCells(Leqisoft.Model.Table modelTable, TXTextControl.Table txTable)
	{
		foreach (var range in modelTable.GetMergeInfo(true))
		{
			MergeCells(txTable, range.r1, range.c1, range.r2, range.c2);
		}
	}

	private void SetBodyContentsWithFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		for (int r = 0; r < modelTable.Rows.Count; r++)
		{
			for (int c = 0; c < modelTable.Columns.VisibleCount; c++)
			{
				var modelCol = modelTable.Columns.GetVisibleColumnAt(c);
				var sourceCell = modelTable[r, modelCol.Index];
				int txRow = r + capRows + 1;
				int txCol = c + 1;
				var txCell = txTable.Cells.GetItem(txRow, txCol);

				if (txCell.Text != sourceCell.GetDisplayValue(true))
					txCell.Text = sourceCell.GetDisplayValue(true);

				txCell.Select();
				if (!ColorEquals(_tx.Selection.TextBackColor, sourceCell.DisplayBackColor))
					_tx.Selection.TextBackColor = sourceCell.DisplayBackColor;

				ConvertAlign(sourceCell.DisplayAlign, out var hAlign, out var vAlign);
				if (_tx.Selection.ParagraphFormat.Alignment != hAlign)
					_tx.Selection.ParagraphFormat.Alignment = hAlign;
				if (txCell.CellFormat.VerticalAlignment != vAlign)
					txCell.CellFormat.VerticalAlignment = vAlign;

				if (sourceCell.Style != null)
					SetSelectionStyleWithFormat(sourceCell.Style);
			}
			Application.DoEvents();
		}
	}

	private void MergeBodyCells(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		foreach (var range in modelTable.GetCellMergesVisible())
		{
			MergeCells(txTable, range.r1 + capRows, range.c1, range.r2 + capRows, range.c2);
		}
	}

	private static void ConvertAlign(Leqisoft.Model.CellTextAlign align, out TXTextControl.HorizontalAlignment hAlign, out TXTextControl.VerticalAlignment vAlign)
	{
		switch (align)
		{
			case Leqisoft.Model.CellTextAlign.TopLeft:
				hAlign = TXTextControl.HorizontalAlignment.Left; vAlign = TXTextControl.VerticalAlignment.Top; break;
			case Leqisoft.Model.CellTextAlign.TopCenter:
				hAlign = TXTextControl.HorizontalAlignment.Center; vAlign = TXTextControl.VerticalAlignment.Top; break;
			case Leqisoft.Model.CellTextAlign.TopRight:
				hAlign = TXTextControl.HorizontalAlignment.Right; vAlign = TXTextControl.VerticalAlignment.Top; break;
			case Leqisoft.Model.CellTextAlign.MiddleLeft:
				hAlign = TXTextControl.HorizontalAlignment.Left; vAlign = TXTextControl.VerticalAlignment.Center; break;
			case Leqisoft.Model.CellTextAlign.MiddleCenter:
				hAlign = TXTextControl.HorizontalAlignment.Center; vAlign = TXTextControl.VerticalAlignment.Center; break;
			case Leqisoft.Model.CellTextAlign.MiddleRight:
				hAlign = TXTextControl.HorizontalAlignment.Right; vAlign = TXTextControl.VerticalAlignment.Center; break;
			case Leqisoft.Model.CellTextAlign.BottomLeft:
				hAlign = TXTextControl.HorizontalAlignment.Left; vAlign = TXTextControl.VerticalAlignment.Bottom; break;
			case Leqisoft.Model.CellTextAlign.BottomCenter:
				hAlign = TXTextControl.HorizontalAlignment.Center; vAlign = TXTextControl.VerticalAlignment.Bottom; break;
			case Leqisoft.Model.CellTextAlign.BottomRight:
				hAlign = TXTextControl.HorizontalAlignment.Right; vAlign = TXTextControl.VerticalAlignment.Bottom; break;
			default:
				hAlign = TXTextControl.HorizontalAlignment.Left; vAlign = TXTextControl.VerticalAlignment.Top; break;
		}
	}

	private static bool ColorEquals(System.Drawing.Color a, System.Drawing.Color b)
	{
		return a.ToArgb() == b.ToArgb();
	}

	private System.Collections.Generic.IEnumerable<TXTextControl.Table> GetSelectionTables()
	{
		if (_tx.Selection == null) yield break;
		int count = _tx.Tables.Count;
		for (int i = 1; i <= count; i++)
		{
			var table = _tx.Tables.GetItem(i);
			if (table != null && table.Start >= _tx.Selection.Start && table.Start <= _tx.Selection.Start + _tx.Selection.Length)
				yield return table;
		}
	}

	private void TagTableStyle(TXTextControl.Table table, int style)
	{
		try { ((dynamic)table).Tag = style; }
		catch { }
	}

	private void MergeCells(TXTextControl.Table table, int r1, int c1, int r2, int c2)
	{
		if (table == null) return;
		try
		{
			// 原始 IL 对传入的 0-based 索引 +1 转为 TX 的 1-based 索引
			r1++;
			c1++;
			r2++;
			c2++;

			// 合并前保存第一个单元格的文本，合并后若文本被覆盖则恢复
			var firstCell = table.Cells.GetItem(r1, c1);
			string oldText = firstCell?.Text;

			table.Select(r1, c1, r2, c2);
			table.MergeCells();

			// 合并后如果第一个单元格的文本改变了，恢复原文本
			if (firstCell != null && firstCell.Text != oldText)
				firstCell.Text = oldText;
		}
		catch { }
	}

	private void SetSelectionStyleWithFormat(Leqisoft.Model.CellStyle style)
	{
		if (style == null) return;

		if (!string.IsNullOrEmpty(style.FontFamily) && _tx.Selection.FontName != style.FontFamily)
			_tx.Selection.FontName = style.FontFamily;

		if (style.FontSize.HasValue)
		{
			int fontSize = (int)(style.FontSize.Value * 20f);
			if (_tx.Selection.FontSize != fontSize)
				_tx.Selection.FontSize = fontSize;
		}

		if (style.ForeColor.HasValue)
		{
			var color = style.ForeColor.Value;
			if (!ColorEquals(_tx.Selection.ForeColor, color))
				_tx.Selection.ForeColor = color;
		}

		if (style.Bold.HasValue && _tx.Selection.Bold != style.Bold.Value)
			_tx.Selection.Bold = style.Bold.Value;

		if (style.Italic.HasValue && _tx.Selection.Italic != style.Italic.Value)
			_tx.Selection.Italic = style.Italic.Value;

		if (style.Underline.HasValue)
		{
			var ulStyle = style.Underline.Value
				? TXTextControl.FontUnderlineStyle.Single
				: TXTextControl.FontUnderlineStyle.None;
			if (_tx.Selection.Underline != ulStyle)
				_tx.Selection.Underline = ulStyle;
		}

		if (style.Margin.HasValue)
		{
			int margin = PixelToTwip(style.Margin.Value);
			if (_tx.Selection.ParagraphFormat.LeftIndent != margin)
				_tx.Selection.ParagraphFormat.LeftIndent = margin;
		}
	}

	private void SetSidaStyle(TXTextControl.Table txTable)
	{
		try
		{
			txTable.Select();
			SetTableBorder(txTable, Leqisoft.Model.TableBorderStyles.Grid, 0);
		}
		catch { }
	}
	public void Import(params object[] args)
	{
		using (var dlg = new OpenFileDialog { Filter = "Word文档|*.docx;*.doc;*.rtf|所有文件|*.*" })
		{
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					var ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
					var streamType = ext == ".docx" ? TXTextControl.StreamType.WordprocessingML
						: ext == ".doc" ? TXTextControl.StreamType.MSWord
						: TXTextControl.StreamType.RichTextFormat;
					_textControl.Load(dlg.FileName, streamType);
					NeedSave = true;
				}
				catch (Exception ex) { ex.Log("DocumentEditor.Import"); }
			}
		}
	}
	public void Export(params object[] args) { ExportDocument(args); }
	public void ExportDocument(params object[] args)
	{
		using (var dlg = new SaveFileDialog { Filter = "Word文档|*.docx|Word 97-2003|*.doc|RTF|*.rtf", DefaultExt = ".docx" })
		{
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					var ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
					var streamType = ext == ".doc" ? TXTextControl.StreamType.MSWord
						: ext == ".rtf" ? TXTextControl.StreamType.RichTextFormat
						: TXTextControl.StreamType.WordprocessingML;
					_textControl.Save(dlg.FileName, streamType);
				}
				catch (Exception ex) { ex.Log("DocumentEditor.ExportDocument"); }
			}
		}
	}
	public void ExportDocumentDialog(params object[] args) { ExportDocument(args); }
	public void InsertRefTable(params object[] args)
	{
		try
		{
			// 弹出选择表格对话框
			var formSelectNode = new FormSelectNode();
			formSelectNode.Project = Program.MainForm.CurrentProject;
			if (formSelectNode.ShowImportTable() != DialogResult.OK)
			{
				return;
			}

			var selectedTableNode = formSelectNode.SelectedTableNode;
			if (selectedTableNode == null) return;

			var sourceTable = selectedTableNode.Table;
			if (sourceTable == null) return;

			sourceTable.LoadAndReturn(false);
			sourceTable.CalculateRecursive();
			if (sourceTable.Columns.VisibleCount == 0)
			{
				return;
			}

			canRibbonStatusSetting = false;
			DetachEvents();
			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			OnChanged();

			try
			{
				_textControl.Selection.Text = "\n";
				_textControl.Tables.Add(sourceTable.Rows.Count + getCaptionRows(sourceTable), sourceTable.Columns.VisibleCount);
				_textControl.Select(_textControl.Selection.Start - 1, 0);
				var txTable = GetCurrentTable();

				txTable.Select();
				SetLineSpacing(100);
				SetParaAboveSpacing(0.0);
				SetParaBelowSpacing(0.0);
				_textControl.Selection.ParagraphFormat.HangingIndent = 0;
				_textControl.Selection.ParagraphFormat.LeftIndent = 0;

				// 使用与 InsertModelTable 相同的恢复后的格式化方法
				SetColumnWidth(txTable, sourceTable);
				SetRowHeight(txTable, sourceTable);
				SetDefaultStyleWithFormat(txTable, sourceTable.DefaultStyle);
				SetColumnStyleWithFormat(txTable, sourceTable);
				SetCaptionStyleWithFormat(txTable, sourceTable);
				SetCaptionContentsWithFormat(txTable, sourceTable);
				MergeCaptionCells(sourceTable, txTable);
				SetBodyContentsWithFormat(txTable, sourceTable);
				MergeBodyCells(txTable, sourceTable);

				txTable.Select();
				var borderStyle = AppCommands.TableStyle.SelectedStyle ?? Leqisoft.Model.TableBorderStyles.Grid;
				SetTableBorder(txTable, borderStyle, getCaptionRows(sourceTable));

				Application.DoEvents();

				// 创建引用表格的 DocumentTarget 书签
				var lsbm = new LeqiBookmark
				{
					TableId = sourceTable.Id.ToBase64(),
					Status = LeqiBookmarkStatus.New
				};
				AddNewBookmark(txTable, lsbm);

				// 设置 Document.MergeTable 用于持久化
				var doc = Document as Leqisoft.Model.Document;
				if (doc != null)
					doc.MergeTable = sourceTable.Id;

				NeedSave = true;
			}
			catch (Exception ex)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "发生错误：" + ex.Message);
			}
			finally
			{
				_textControl.ClearUndo();
				canRibbonStatusSetting = true;
				Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
				AttachEvents();
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.InsertRefTable");
		}
	}
	public void SmartLayout(params object[] args)
	{
		var dialog = new SmartLayoutDialog();
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			ApplySmartLayout(dialog.Settings);
		}
	}

	private void ApplySmartLayout(SmartLayoutSettings settings)
	{
		if (_textControl == null) return;

		try
		{
			_textControl.BeginUndoAction("智能排版");

			if (settings.IndentFirstLine)
			{
				IndentFirstLineTwoChars();
			}

			if (settings.RemoveIndentForCentered)
			{
				RemoveIndentForCenteredParagraphs();
			}

			if (settings.UnifyParagraphStyles)
			{
				UnifyParagraphStyles();
			}

			if (settings.RenumberParagraphs)
			{
				RenumberParagraphs();
			}

			if (settings.RemoveLeadingSpaces)
			{
				RemoveLeadingSpaces();
			}

			_textControl.EndUndoAction();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.ApplySmartLayout");
		}
	}

	private void IndentFirstLineTwoChars()
	{
		var selection = _textControl.Selection;
		if (selection == null) return;

		var format = selection.ParagraphFormat;
		if (format != null)
		{
			format.LeftIndent = 42;
			format.HangingIndent = -42;
		}
	}

	private void RemoveIndentForCenteredParagraphs()
	{
		var selection = _textControl.Selection;
		if (selection == null) return;

		var format = selection.ParagraphFormat;
		if (format != null && format.Alignment == TXTextControl.HorizontalAlignment.Center)
		{
			format.LeftIndent = 0;
			format.HangingIndent = 0;
		}
	}

	private void UnifyParagraphStyles()
	{
		var selection = _textControl.Selection;
		if (selection == null) return;

		var format = selection.ParagraphFormat;
		if (format != null)
		{
			_textControl.SelectAll();
			var allSelection = _textControl.Selection;
			if (allSelection != null)
			{
				allSelection.ParagraphFormat = format;
			}
		}
	}

	private void RenumberParagraphs()
	{
		string text = _textControl.Selection.Text;
		if (string.IsNullOrEmpty(text)) return;

		string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		List<string> renumberedLines = new List<string>();
		int number = 1;

		foreach (string line in lines)
		{
			string trimmedLine = line.Trim();
			if (!string.IsNullOrWhiteSpace(trimmedLine) && trimmedLine.Length > 0 && char.IsDigit(trimmedLine[0]))
			{
				int j = 0;
				while (j < trimmedLine.Length && (char.IsDigit(trimmedLine[j]) || trimmedLine[j] == '.' || trimmedLine[j] == '、' || trimmedLine[j] == ' '))
				{
					j++;
				}
				renumberedLines.Add(number + "." + trimmedLine.Substring(j));
				number++;
			}
			else
			{
				renumberedLines.Add(line);
			}
		}

		_textControl.Selection.Text = string.Join("\r\n", renumberedLines);
	}

	private void RemoveLeadingSpaces()
	{
		string text = _textControl.Selection.Text;
		if (string.IsNullOrEmpty(text)) return;

		string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		List<string> cleanedLines = new List<string>();

		foreach (string line in lines)
		{
			cleanedLines.Add(line.TrimStart(' ', '\t'));
		}

		_textControl.Selection.Text = string.Join("\r\n", cleanedLines);
	}

	public class SmartLayoutSettings
	{
		public bool IndentFirstLine { get; set; } = true;
		public bool RemoveIndentForCentered { get; set; } = true;
		public bool UnifyParagraphStyles { get; set; } = true;
		public bool RenumberParagraphs { get; set; } = true;
		public bool RemoveLeadingSpaces { get; set; } = false;
	}

	public class SmartLayoutDialog : Form
	{
		private CheckBox chkIndentFirstLine;
		private CheckBox chkRemoveIndentForCentered;
		private CheckBox chkUnifyStyles;
		private CheckBox chkRenumber;
		private CheckBox chkRemoveLeadingSpaces;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;

		public SmartLayoutSettings Settings { get; private set; } = new SmartLayoutSettings();

		public SmartLayoutDialog()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			Text = "智能排版";
			Size = new Size(320, 220);
			StartPosition = FormStartPosition.CenterScreen;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;

			chkIndentFirstLine = new CheckBox
			{
				Text = "段落首行缩进两个字符",
				Location = new Point(20, 20),
				Size = new Size(260, 20),
				Checked = true
			};
			Controls.Add(chkIndentFirstLine);

			chkRemoveIndentForCentered = new CheckBox
			{
				Text = "水平居中段落取消缩进",
				Location = new Point(20, 45),
				Size = new Size(260, 20),
				Checked = true
			};
			Controls.Add(chkRemoveIndentForCentered);

			chkUnifyStyles = new CheckBox
			{
				Text = "相同类型段落统一样式",
				Location = new Point(20, 70),
				Size = new Size(260, 20),
				Checked = true
			};
			Controls.Add(chkUnifyStyles);

			chkRenumber = new CheckBox
			{
				Text = "自动重排段落编号",
				Location = new Point(20, 95),
				Size = new Size(260, 20),
				Checked = true
			};
			Controls.Add(chkRenumber);

			chkRemoveLeadingSpaces = new CheckBox
			{
				Text = "删除段落首行空格",
				Location = new Point(20, 120),
				Size = new Size(260, 20),
				Checked = false
			};
			Controls.Add(chkRemoveLeadingSpaces);

			btnOK = new System.Windows.Forms.Button
			{
				Text = "确定",
				Location = new Point(100, 160),
				Size = new Size(80, 25),
				DialogResult = DialogResult.OK
			};
			btnOK.Click += BtnOK_Click;
			Controls.Add(btnOK);

			btnCancel = new System.Windows.Forms.Button
			{
				Text = "取消",
				Location = new Point(190, 160),
				Size = new Size(80, 25),
				DialogResult = DialogResult.Cancel
			};
			Controls.Add(btnCancel);

			AcceptButton = btnOK;
			CancelButton = btnCancel;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Settings.IndentFirstLine = chkIndentFirstLine.Checked;
			Settings.RemoveIndentForCentered = chkRemoveIndentForCentered.Checked;
			Settings.UnifyParagraphStyles = chkUnifyStyles.Checked;
			Settings.RenumberParagraphs = chkRenumber.Checked;
			Settings.RemoveLeadingSpaces = chkRemoveLeadingSpaces.Checked;
		}
	}
	public void ExportPdfDocumentDialog(params object[] args)
	{
		using (var dlg = new SaveFileDialog { Filter = "PDF文件|*.pdf", DefaultExt = ".pdf" })
		{
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try { _textControl.Save(dlg.FileName, TXTextControl.StreamType.AdobePDF); }
				catch (Exception ex) { ex.Log("DocumentEditor.ExportPdf"); }
			}
		}
	}
	public void Print(params object[] args) { _textControl.Print(Document?.TreeNode?.Name ?? "Document"); }
	public void Landscape(params object[] args) { _textControl.Selection.SectionFormat.Landscape = true; }
	public void Portrait(params object[] args) { _textControl.Selection.SectionFormat.Landscape = false; }
	public void SetPaperKind(params object[] args)
	{
		if (args?.Length > 0 && args[0] is System.Drawing.Printing.PaperKind kind)
		{
			PaperKindToSize(kind, out int w, out int h);
			_textControl.Selection.SectionFormat.PageSize = new TXTextControl.PageSize(w, h);
		}
	}
	public void SetPaperCustom(params object[] args)
	{
		if (args?.Length > 2 && args[0] is double w && args[1] is double h)
		{
			_textControl.Selection.SectionFormat.PageSize = new TXTextControl.PageSize((int)(w * 56.7), (int)(h * 56.7));
		}
	}
	public void SetPageTopMargin(params object[] args) { if (args?.Length > 0 && args[0] is double d) { var m = _textControl.Selection.SectionFormat.PageMargins; m.Top = (int)(d * 56.7); _textControl.Selection.SectionFormat.PageMargins = m; } }
	public void SetPageBottomMargin(params object[] args) { if (args?.Length > 0 && args[0] is double d) { var m = _textControl.Selection.SectionFormat.PageMargins; m.Bottom = (int)(d * 56.7); _textControl.Selection.SectionFormat.PageMargins = m; } }
	public void SetPageLeftMargin(params object[] args) { if (args?.Length > 0 && args[0] is double d) { var m = _textControl.Selection.SectionFormat.PageMargins; m.Left = (int)(d * 56.7); _textControl.Selection.SectionFormat.PageMargins = m; } }
	public void SetPageRightMargin(params object[] args) { if (args?.Length > 0 && args[0] is double d) { var m = _textControl.Selection.SectionFormat.PageMargins; m.Right = (int)(d * 56.7); _textControl.Selection.SectionFormat.PageMargins = m; } }
	public void SetHeaderMargin(params object[] args)
	{
		if (args?.Length <= 0 || !(args[0] is int v)) return;
		if (IsDocumentLocked()) return;

		switch (PageSettingTarget)
		{
			case PageSettingTarget.Selection:
			{
				var hf = _textControl.Sections.GetItem()?.HeadersAndFooters?.GetItem(TXTextControl.HeaderFooterType.Header);
				if (hf != null)
					hf.Distance = v * 20;
				break;
			}
			case PageSettingTarget.Document:
			{
				var en = _textControl.Sections.GetEnumerator();
				try
				{
					while (en.MoveNext())
					{
						var sec = en.Current as TXTextControl.Section;
						if (sec == null) continue;
						_textControl.Select(sec.Start, 0);
						var hf = sec.HeadersAndFooters?.GetItem(TXTextControl.HeaderFooterType.Header);
						if (hf != null)
							hf.Distance = v * 20;
					}
				}
				finally
				{
					var disp = en as IDisposable;
					if (disp != null) disp.Dispose();
				}
				break;
			}
		}
	}
	public void SetFooterMargin(params object[] args)
	{
		if (args?.Length <= 0 || !(args[0] is int v)) return;
		if (IsDocumentLocked()) return;

		switch (PageSettingTarget)
		{
			case PageSettingTarget.Selection:
			{
				var hf = _textControl.Sections.GetItem()?.HeadersAndFooters?.GetItem(TXTextControl.HeaderFooterType.Footer);
				if (hf != null)
					hf.Distance = v * 20;
				break;
			}
			case PageSettingTarget.Document:
			{
				var en = _textControl.Sections.GetEnumerator();
				try
				{
					while (en.MoveNext())
					{
						var sec = en.Current as TXTextControl.Section;
						if (sec == null) continue;
						_textControl.Select(sec.Start, 0);
						var hf = sec.HeadersAndFooters?.GetItem(TXTextControl.HeaderFooterType.Footer);
						if (hf != null)
							hf.Distance = v * 20;
					}
				}
				finally
				{
					var disp = en as IDisposable;
					if (disp != null) disp.Dispose();
				}
				break;
			}
		}
	}
	public void SetColumnCount(params object[] args) { if (args?.Length > 0 && args[0] is int n) _textControl.Selection.SectionFormat.Columns = n; }
	public void SetColumnCountDialog(params object[] args) { _textControl.FontDialog(); }
	public void ShowHorizontalRuler(params object[] args)
	{
		try
		{
			if (_horizontalRulerBar == null)
			{
				_horizontalRulerBar = new TXTextControl.RulerBar();
				_horizontalRulerBar.Dock = DockStyle.Top;
				_innerPanel?.Controls.Add(_horizontalRulerBar);
			}
			_textControl.RulerBar = _horizontalRulerBar;
			_horizontalRulerBar.Visible = true;
		}
		catch { }
	}
	public void HideHorizontalRuler(params object[] args)
	{
		try
		{
			if (_horizontalRulerBar != null) _horizontalRulerBar.Visible = false;
			_textControl.RulerBar = null;
		}
		catch { }
	}
	public void ShowVerticalRuler(params object[] args)
	{
		try
		{
			if (_verticalRulerBar == null)
			{
				_verticalRulerBar = new TXTextControl.RulerBar();
				_verticalRulerBar.Dock = DockStyle.Left;
				_innerPanel?.Controls.Add(_verticalRulerBar);
			}
			_textControl.VerticalRulerBar = _verticalRulerBar;
			_verticalRulerBar.Visible = true;
		}
		catch { }
	}
	public void HideVerticalRuler(params object[] args)
	{
		try
		{
			if (_verticalRulerBar != null) _verticalRulerBar.Visible = false;
			_textControl.VerticalRulerBar = null;
		}
		catch { }
	}
	public void ShowToolbar(params object[] args) { _pnlToolbar?.Show(); }
	public void HideToolbar(params object[] args) { _pnlToolbar?.Hide(); }
	public void ShowMarks(params object[] args) { _textControl.ControlChars = true; }
	public void HideMarks(params object[] args) { _textControl.ControlChars = false; }
	/// <summary>
	/// 获取文档结构图实例（懒创建，与 ShowStructure 一致）。
	/// 供批量应用等外部命令访问已创建的 DocumentStructure。
	/// </summary>
	public DocumentStructure Structure
	{
		get
		{
			if (_structure == null)
			{
				_structure = new DocumentStructure(this);
			}
			return _structure;
		}
	}

	public void ShowStructure(params object[] args)
	{
		if (_structure == null)
		{
			_structure = new DocumentStructure(this);
		}

		Program.MainForm.BindControlToNavigationPanel(_structure.View);

		AppCommands.ShowDocumentNavigator.IsPressed = true;
		Program.MainForm.ShowNavigationPanel();
	}

	public void HideStructure(params object[] args)
	{
		AppCommands.ShowDocumentNavigator.IsPressed = false;
		Program.MainForm.HideNavigationPanel();
	}
	public void PageMode(params object[] args) { _textControl.ViewMode = TXTextControl.ViewMode.PageView; }
	public void EnterPreviewMode(params object[] args) { _textControl.PrintPreview(Document?.TreeNode?.Name ?? "Document"); }
	public void LeavePreviewMode(params object[] args) { _textControl.ViewMode = TXTextControl.ViewMode.PageView; }
	public void LockDocument(params object[] args) { _textControl.EditMode = TXTextControl.EditMode.ReadAndSelect; }
	public void UnlockDocument(params object[] args) { _textControl.EditMode = TXTextControl.EditMode.Edit; }
	public void Replace(params object[] args) { _textControl.Find(); }
	public void NextError(params object[] args)
	{
		try
		{
			if (Program.MainForm?.DocumentValidationResultCache == null) return;

			// 从当前位置向后查找第一个未通过的校验域
			int currentPos = _textControl.InputPosition.TextPosition;
			int totalFields = _textControl.ApplicationFields.Count;
			for (int i = 0; i < totalFields; i++)
			{
				var f = _textControl.ApplicationFields[i];
				if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null
					|| f.Parameters.Length < 3) continue;

				long fid;
				if (!long.TryParse(f.Parameters[2], out fid) || fid <= 0) continue;
				if (f.Start <= currentPos) continue;

				var key = new Leqisoft.DTO.Id64(fid);
				if (Program.MainForm.DocumentValidationResultCache.TryGetValue(key, out var list)
					&& list != null && list.Count > 0 && !list[0].Passed)
				{
					_textControl.Select(f.Start, f.Length);
					ShowValidationTooltip(f, list[0]);
					return;
				}
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有更多错误。");
		}
		catch { }
	}
	public void PreviousError(params object[] args)
	{
		try
		{
			if (Program.MainForm?.DocumentValidationResultCache == null) return;

			int currentPos = _textControl.InputPosition.TextPosition;
			TXTextControl.ApplicationField lastError = null;
			ValidationResult lastResult = null;

			int totalFields = _textControl.ApplicationFields.Count;
			for (int i = 0; i < totalFields; i++)
			{
				var f = _textControl.ApplicationFields[i];
				if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null
					|| f.Parameters.Length < 3) continue;
				if (f.Start >= currentPos) continue;

				long fid;
				if (!long.TryParse(f.Parameters[2], out fid) || fid <= 0) continue;

				var key = new Leqisoft.DTO.Id64(fid);
				if (Program.MainForm.DocumentValidationResultCache.TryGetValue(key, out var list)
					&& list != null && list.Count > 0 && !list[0].Passed)
				{
					lastError = f;
					lastResult = list[0];
				}
			}

			if (lastError != null)
			{
				_textControl.Select(lastError.Start, lastError.Length);
				ShowValidationTooltip(lastError, lastResult);
			}
			else
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有更多错误。");
		}
		catch { }
	}
	public void StartValidate(params object[] args)
	{
		try
		{
			int formulaErrorCount = 0;
			int formulaTotalCount = 0;
			int docValidationErrorCount = 0;
			int docValidationTotalCount = 0;
			int formatErrorCount = 0;
			var results = new Dictionary<Leqisoft.DTO.Id64, List<ValidationResult>>();

			// === 第一步：格式合规校验 ===
			try
			{
				var project = Leqisoft.Model.Project.Current;
				if (project?.FormatComplianceChecker != null && Document is Leqisoft.Model.Document doc)
				{
					var complianceResults = project.FormatComplianceChecker.ValidateAll(doc);
					formatErrorCount = complianceResults.Count(r => !r.Passed);
				}
			}
			catch (Exception ex) { ex.Log("StartValidate.FormatCompliance"); }

			// === 第二步：文档域稽核校验（Formula + DocValidation）===
			foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
			{
				if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null) continue;

				if (f.Parameters[0] == "Formula" && f.Parameters.Length >= 4
					&& !string.IsNullOrWhiteSpace(f.Parameters[3]))
				{
					// Formula 域带稽核规则
					formulaTotalCount++;
					var result = ValidateFormulaField(f);
					if (result != null)
					{
						long fid = 0;
						if (f.Parameters.Length >= 3)
							long.TryParse(f.Parameters[2], out fid);
						var key = new Leqisoft.DTO.Id64(fid);
						results[key] = new List<ValidationResult> { result };
						if (!result.Passed) formulaErrorCount++;

						f.HighlightMode = TXTextControl.HighlightMode.Always;
						f.DoubledInputPosition = true;
						f.HighlightColor = result.Passed
							? Color.FromArgb(80, 140, 220, 120)  // 绿色
							: Color.FromArgb(80, 240, 140, 140);  // 红色
					}
				}
				else if (f.Parameters[0] == "DocValidation" && f.Parameters.Length >= 2
					&& !string.IsNullOrWhiteSpace(f.Parameters[1]))
				{
					// DocValidation 域
					docValidationTotalCount++;
					var result = ValidateDocValidationField(f);
					if (result != null)
					{
						long fid = 0;
						if (f.Parameters.Length >= 3)
							long.TryParse(f.Parameters[2], out fid);
						var key = new Leqisoft.DTO.Id64(fid);
						results[key] = new List<ValidationResult> { result };
						if (!result.Passed) docValidationErrorCount++;

						f.HighlightMode = TXTextControl.HighlightMode.Always;
						f.DoubledInputPosition = true;
						f.HighlightColor = result.Passed
							? Color.FromArgb(80, 140, 220, 120)  // 绿色
							: Color.FromArgb(80, 240, 140, 140);  // 红色
					}
				}
			}

			// 存入缓存
			if (Program.MainForm != null)
			{
				Program.MainForm.DocumentValidationResultCache.Clear();
				foreach (var kv in results)
				{
					Program.MainForm.DocumentValidationResultCache[kv.Key] = kv.Value;
				}
			}

			// 跨文档校验
			int crossErrorCount = 0;
			int crossTotalCount = 0;
			try
			{
				var project = Leqisoft.Model.Project.Current;
				if (project != null && project.CrossDocumentValidationRules != null)
				{
					crossTotalCount = project.CrossDocumentValidationRules
						.Count(r => r.SourceDocumentId == ((Document as Leqisoft.Model.Document)?.Id ?? Leqisoft.DTO.Id64.Zero) ||
						            r.TargetDocumentId == ((Document as Leqisoft.Model.Document)?.Id ?? Leqisoft.DTO.Id64.Zero));
					crossErrorCount = ValidateCrossDocument();
				}
			}
			catch (Exception ex) { ex.Log("StartValidate.CrossDocument"); }

			// 弹窗
			int grandTotal = formulaTotalCount + docValidationTotalCount + crossTotalCount;
			int grandErrors = formulaErrorCount + docValidationErrorCount + crossErrorCount + formatErrorCount;

			if (grandTotal == 0 && formatErrorCount == 0)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文档中没有校验点或格式合规规则。");
			}
			else
			{
				string msg = "文档校验结果：\n\n";
				if (formulaTotalCount > 0 || docValidationTotalCount > 0)
					msg += "📋 稽核校验：共 " + (formulaTotalCount + docValidationTotalCount) + " 项，" + (formulaErrorCount + docValidationErrorCount) + " 个错误\n";
				if (crossTotalCount > 0)
					msg += "🔗 跨文档校验：共 " + crossTotalCount + " 项，" + crossErrorCount + " 个错误\n";
				if (formatErrorCount > 0)
					msg += "📐 格式合规：" + formatErrorCount + " 个问题\n";
				msg += "\n总计：" + grandErrors + " 个错误";
				if (grandErrors == 0)
					msg = "文档校验通过（共 " + grandTotal + " 项），未发现错误。";

				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, msg);
			}
		}
		catch (Exception ex) { ex.Log("DocumentEditor.StartValidate"); }
	}

	private void ShowValidationTooltip(TXTextControl.ApplicationField field, ValidationResult result)
	{
		try
		{
			if (field == null || result == null || _ttpComment == null) return;

			string detail = "";
			if (result.Source != null)
				detail += "类型：" + result.Source + "\n";
			detail += "左值：" + result.LeftValue + "\n";
			detail += "右值：" + result.RightValue + "\n";
			detail += "结果：" + (result.Passed ? "✓ 通过" : "✗ 未通过");

			_ttpComment.SetText("校验详情", detail, canClose: true);
			var mousePos = Control.MousePosition;
			var clientPos = _textControl.PointToClient(mousePos);
			_ttpComment.Show(_textControl, new Point(clientPos.X + 10, clientPos.Y + 10));
		}
		catch (Exception ex) { ex.Log("ShowValidationTooltip"); }
	}

	/// <summary>
	/// 校验 Formula 域：左值为域当前显示值，右值为规则中的右表达式求值。
	/// </summary>
	private static ValidationResult ValidateFormulaField(TXTextControl.ApplicationField f)
	{
		try
		{
			string formula = f.Parameters[1];
			string ruleJson = f.Parameters[3];
			string leftValue = f.Text;

			// 解析规则 JSON
			JObject rule = JObject.Parse(ruleJson);
			string rightExpr = rule["rightExpr"]?.ToString() ?? "";
			int opCode = Convert.ToInt32(rule["operator"]);

			// 求值右表达式
			string rightValue = FormulaEditor.EvaluateDocumentFormula(rightExpr, calculateTable: false);

			// 比较
			bool passed = CompareValues(leftValue, rightValue, opCode);

			return new ValidationResult
			{
				Source = null,
				IsValid = true,
				Passed = passed,
				LeftValue = leftValue,
				RightValue = rightValue
			};
		}
		catch { return null; }
	}

	/// <summary>
	/// 校验 DocValidation 域：左值为域原文（用户输入），右值为规则中的右表达式求值。
	/// </summary>
	private static ValidationResult ValidateDocValidationField(TXTextControl.ApplicationField f)
	{
		try
		{
			string ruleJson = f.Parameters[1];
			string leftValue = f.Text;

			// 解析规则 JSON
			JObject rule = JObject.Parse(ruleJson);
			string rightExpr = rule["rightExpr"]?.ToString() ?? "";
			int opCode = Convert.ToInt32(rule["operator"]);

			// 求值右表达式
			string rightValue = FormulaEditor.EvaluateDocumentFormula(rightExpr, calculateTable: false);

			// 比较
			bool passed = CompareValues(leftValue, rightValue, opCode);

			return new ValidationResult
			{
				Source = null,
				IsValid = true,
				Passed = passed,
				LeftValue = leftValue,
				RightValue = rightValue
			};
		}
		catch { return null; }
	}

	/// <summary>
	/// 值比较：优先数值比较，否则字符串比较。
	/// 运算符编码与 ValidationOperator.Code 对齐：=→0, >→1, >=→2, <→3, <=→4, <>→5
	/// </summary>
	private static bool CompareValues(string left, string right, int opCode)
	{
		double l, r;
		if (double.TryParse(left, out l) && double.TryParse(right, out r))
		{
			return CompareDouble(l, r, opCode);
		}
		// 字符串比较
		switch (opCode)
		{
			case 0: return left == right;
			case 5: return left != right;
			default: return false;
		}
	}

	private static bool CompareDouble(double l, double r, int opCode)
	{
		switch (opCode)
		{
			case 0: return Math.Abs(l - r) < 0.0001;  // =
			case 1: return l > r;                       // >
			case 2: return l >= r;                      // >=
			case 3: return l < r;                       // <
			case 4: return l <= r;                      // <=
			case 5: return Math.Abs(l - r) >= 0.0001;   // <>
			default: return false;
		}
	}

	/// <summary>
	/// 校验当前文档中涉及的跨文档校验规则。
	/// 规则中 SourceDocumentId/TargetDocumentId 需至少有一个为当前文档。
	/// 在两端域值都可读时进行比较并着色，否则标记为暂不支持。
	/// </summary>
	public int ValidateCrossDocument()
	{
		int errorCount = 0;

		var project = Leqisoft.Model.Project.Current;
		if (project == null || project.CrossDocumentValidationRules == null || project.CrossDocumentValidationRules.Count == 0)
			return 0;

		// 获取当前文档 Id
		var currentDoc = Document as Leqisoft.Model.Document;
		if (currentDoc == null || currentDoc.Id == null)
			return 0;

		Leqisoft.DTO.Id64 currentDocId = currentDoc.Id;

		// 过滤只与当前文档相关的规则
		var relatedRules = project.CrossDocumentValidationRules
			.Where(r => r.SourceDocumentId == currentDocId || r.TargetDocumentId == currentDocId)
			.ToList();

		if (relatedRules.Count == 0)
			return 0;

		// 预构建当前文档的域值字典（fieldId → fieldText）
		var currentFieldValues = new Dictionary<Leqisoft.DTO.Id64, string>();
		foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
		{
			if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 3)
				continue;
			long fid;
			if (long.TryParse(f.Parameters[2], out fid))
			{
				currentFieldValues[new Leqisoft.DTO.Id64(fid)] = f.Text ?? "";
			}
		}

		foreach (var rule in relatedRules)
		{
			try
			{
				string sourceValue = null;
				string targetValue = null;
				bool canCompare = false;

				// 获取源域值
				if (rule.SourceDocumentId == currentDocId)
				{
					currentFieldValues.TryGetValue(rule.SourceFieldId, out sourceValue);
				}

				// 获取目标域值
				if (rule.TargetDocumentId == currentDocId)
				{
					currentFieldValues.TryGetValue(rule.TargetFieldId, out targetValue);
				}

				// 如果双方都有值，进行比较
				if (sourceValue != null && targetValue != null)
				{
					canCompare = true;
				}

				bool passed = true;
				if (canCompare)
				{
					passed = CompareValues(sourceValue, targetValue, rule.Operator);
					if (!passed) errorCount++;
				}

				// 着色当前文档中涉及的域
				HighlightCrossField(rule.SourceDocumentId == currentDocId ? rule.SourceFieldId : rule.TargetFieldId,
					canCompare ? passed : true,
					canCompare);
			}
			catch (Exception ex) { ex.Log("ValidateCrossDocument"); }
		}

		return errorCount;
	}

	/// <summary>
	/// 对当前文档中指定的域进行着色。
	/// </summary>
	private void HighlightCrossField(Leqisoft.DTO.Id64 fieldId, bool passed, bool canCompare)
	{
		foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
		{
			if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 3)
				continue;
			long fid;
			if (long.TryParse(f.Parameters[2], out fid) && new Leqisoft.DTO.Id64(fid) == fieldId)
			{
				f.HighlightMode = TXTextControl.HighlightMode.Always;
				f.DoubledInputPosition = true;

				if (!canCompare)
				{
					// 灰色表示暂无法比较（另一文档域值不可读）
					f.HighlightColor = Color.FromArgb(80, 180, 180, 180);
				}
				else if (passed)
				{
					f.HighlightColor = Color.FromArgb(80, 140, 220, 120);  // 绿色
				}
				else
				{
					f.HighlightColor = Color.FromArgb(80, 240, 140, 140);  // 红色
				}
			}
		}
	}

	public void GenerateConfirmation(params object[] args)
	{
		// TODO: 需要实现确认书生成逻辑
		// MainForm.GenerateConfirmationFromDocument() 中调用，传入 List<Dictionary<string, Cell>> 数据
		// 遍历文档中的 MERGEFIELD 域，用数据替换占位符
		try
		{
			if (args == null || args.Length == 0) return;
			var data = args[0] as System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, Leqisoft.Model.Cell>>;
			if (data == null || data.Count == 0) return;

			// 用第一行数据替换文档中的域
			var firstRow = data[0];
			foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
			{
				try
				{
					if (f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
					string formula = f.Parameters[1];
					if (string.IsNullOrWhiteSpace(formula)) continue;

					// 尝试从数据中匹配
					foreach (var kv in firstRow)
					{
						if (formula.Contains(kv.Key) && kv.Value != null)
						{
							f.Text = kv.Value.GetDisplayValue() ?? "";
							break;
						}
					}
				}
				catch { }
			}
			NeedSave = true;
		}
		catch (Exception ex) { ex.Log("DocumentEditor.GenerateConfirmation"); }
	}
	public void MakeIds(params object[] args)
	{
		try
		{
			DetachEvents();
			_textControl.TextParts.Activate(GetMainText());

			var starts = new List<int>();
			var toRemove = new List<DocumentTarget>();

			var enumerator = _textControl.DocumentTargets.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					var dt = (DocumentTarget)enumerator.Current;
					if (IsBookmarkAtStart(dt))
					{
						if (LeqiBookmark.TryParse(dt.TargetName, out LeqiBookmark _))
						{
							starts.Add(dt.Start - 1);
						}
					}
					else
					{
						TagCurrentPOrTableModified(dt.Start - 1);
						toRemove.Add(dt);
					}
				}
			}
			finally
			{
				if (enumerator is IDisposable disp)
					disp.Dispose();
			}

			foreach (var dt in toRemove)
			{
				_textControl.DocumentTargets.Remove(dt);
			}

			var startsEnumerator = starts.GetEnumerator();
			bool hasNext = startsEnumerator.MoveNext();
			int currentPosition = 0;
			int totalChars = _textControl.TextChars.Count;

			while (currentPosition < totalChars)
			{
				_textControl.InputPosition = new InputPosition(currentPosition);

				if (!hasNext || currentPosition >= startsEnumerator.Current)
				{
					var bookmark = new LeqiBookmark
					{
						ParaIdBase64 = Project.Current.GetNextId().ToBase64(),
						Status = LeqiBookmarkStatus.New
					};
					var newDt = new DocumentTarget(bookmark.GetString());
					_textControl.DocumentTargets.Add(newDt);
				}
				else
				{
					hasNext = startsEnumerator.MoveNext();
				}

				var table = GetCurrentTable();
				if (table != null)
				{
					table = table.OuterMostTable ?? table;
					table.Select();
					currentPosition += _textControl.Selection.Length;
				}
				else
				{
					currentPosition += _textControl.Paragraphs.GetItem(currentPosition).Length;
				}
			}
		}
		finally
		{
			AttachEvents();
		}
	}
	public void RefreshAllTablesAndFormulas(params object[] args) { RefreshAllTablesAndFormulas().Wait(); }
	public void RefreshTableWithFormat(params object[] args)
	{
		try
		{
			if (IsDocumentLocked()) return;
			var txTable = GetCurrentTable();
			if (txTable == null) return;

			int savedStart = _textControl.Selection.Start;
			int savedLength = _textControl.Selection.Length;

			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;

			DetachEvents();

			RefreshModelTableWithFormatImpl(txTable);

			AttachEvents();
			Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
			_textControl.Select(savedStart, savedLength);
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshTableWithFormat");
		}
	}

	public void RefreshTableWithoutFormat()
	{
		try
		{
			if (IsDocumentLocked()) return;
			var txTable = GetCurrentTable();
			if (txTable == null) return;

			int savedStart = _textControl.Selection.Start;
			int savedLength = _textControl.Selection.Length;

			DetachEvents();

			if (GetRefTable(txTable, out var bookmark, out var refTable))
			{
				if (refTable == null)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格不存在。");
					return;
				}

				int txRows = txTable.Rows.Count;
				int modelRows = getCaptionRows(refTable) + refTable.Rows.Count;
				int txCols = txTable.Columns.Count;
				int modelCols = refTable.Columns.Count;

				if (txRows == modelRows && txCols == modelCols)
				{
					RefreshTableContentsImpl(txTable, refTable);
				}
				else
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
						"拟刷新的表格与源表格的行列不一致，无法刷新。");
				}
			}

			AttachEvents();
			_textControl.Select(savedStart, savedLength);
		}
		catch (Exception ex) { ex.Log("DocumentEditor.RefreshTableWithoutFormat"); }
	}

	public void RefreshAllTablesWithoutFormat()
	{
		try
		{
			if (IsDocumentLocked()) return;
			DetachEvents();

			int count = _textControl.Tables.Count;
			TXTextControl.Table[] tables = new TXTextControl.Table[count];
			_textControl.Tables.CopyTo(tables, 0);

			for (int i = 0; i < tables.Length; i++)
			{
				var txTable = tables[i];
				if (txTable == null || txTable.Cells.Count == 0) continue;

				if (GetRefTable(txTable, out var bookmark, out var refTable))
				{
					if (refTable != null)
					{
						int txRows = txTable.Rows.Count;
						int modelRows = getCaptionRows(refTable) + refTable.Rows.Count;
						int txCols = txTable.Columns.Count;
						int modelCols = refTable.Columns.Count;

						if (txRows == modelRows && txCols == modelCols)
						{
							RefreshTableContentsImpl(txTable, refTable);
						}
					}
				}
			}

			AttachEvents();
		}
		catch (Exception ex) { ex.Log("DocumentEditor.RefreshAllTablesWithoutFormat"); }
	}

	public Task RefreshAllReferences()
	{
		return Task.Run(() =>
		{
			try
			{
				if (IsDocumentLocked()) return;
				DetachEvents();

				int count = _textControl.Tables.Count;
				TXTextControl.Table[] tables = new TXTextControl.Table[count];
				_textControl.Tables.CopyTo(tables, 0);
				foreach (var txTable in tables)
				{
					if (txTable == null || txTable.Cells.Count == 0) continue;
					if (GetRefTable(txTable, out var bookmark, out var refTable))
					{
						if (refTable != null)
						{
							int txRows = txTable.Rows.Count;
							int modelRows = getCaptionRows(refTable) + refTable.Rows.Count;
							int txCols = txTable.Columns.Count;
							int modelCols = refTable.Columns.Count;
							if (txRows == modelRows && txCols == modelCols)
							{
								RefreshTableContentsImpl(txTable, refTable);
							}
						}
					}
				}

				AttachEvents();
			}
			catch { }
		});
	}

	private Leqisoft.Model.Table RefreshModelTableWithFormatImpl(TXTextControl.Table txTable)
	{
		_isRefreshingTable = true;
		try
		{
			if (txTable == null) return null;

			if (!GetRefTable(txTable, out var bookmark, out var refTable))
				return null;

			if (refTable == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "表格不存在", MessageBoxButtons.OK, "", false);
				return null;
			}

			refTable.LoadAndReturn(false);
			refTable.CalculateRecursive();

			// 选中表格前位置以获取 DocumentTarget（书签）
			int firstCellStart = txTable.Cells.GetItem(1, 1).Start;
			_tx.Select(firstCellStart - 1, 0);
			var dt = ((dynamic)_tx).DocumentTargets.GetItem() as TXTextControl.DocumentTarget;

			if (refTable.Columns.VisibleCount == 0)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Exclamation, "表格为空！", MessageBoxButtons.OK, "", false);
				return refTable;
			}

			// 如果书签样式为 5（SidaStyle），先恢复下划线格式
			if (bookmark?.TableStyle.HasValue == true && bookmark.TableStyle.Value == 5)
				ResumeSidaStyle(txTable);

			// === 调整行数 ===
			int totalModelRows = refTable.Rows.Count + getCaptionRows(refTable);
			int currentRows = txTable.Rows.Count;

			if (currentRows > totalModelRows)
			{
				int rowsToRemove = currentRows - totalModelRows;
				RemoveRows(txTable, Enumerable.Range(totalModelRows + 1, rowsToRemove));
			}
			else if (currentRows < totalModelRows)
			{
				int rowsToAdd = totalModelRows - currentRows;
				var lastBodyCell = txTable.Cells.GetItem(currentRows, 1);
				if (lastBodyCell != null)
				{
					lastBodyCell.Select();
					_tx.Select(_tx.Selection.Start, 0);
				}
				txTable.Rows.Add(TableAddPosition.After, rowsToAdd);
			}

			// === 调整列数 ===
			int visibleCols = refTable.Columns.VisibleCount;
			int currentCols = txTable.Columns.Count;

			if (currentCols > visibleCols)
			{
				int colsToRemove = currentCols - visibleCols;
				RemoveColumns(txTable, Enumerable.Range(visibleCols + 1, colsToRemove));
			}
			else if (currentCols < visibleCols)
			{
				int colsToAdd = visibleCols - currentCols;
				var lastColCell = txTable.Cells.GetItem(1, currentCols);
				if (lastColCell != null)
				{
					lastColCell.Select();
					_tx.Select(_tx.Selection.Start, 0);
				}
				for (int i = 0; i < colsToAdd; i++)
					InsertCol(txTable, TableAddPosition.After);
			}

			// 拆分合并的单元格（原始顺序：先增删行列，再拆分单元格）
			txTable.Select();
			txTable.SplitCells();

			// 设置列宽和行高（在 MergeCells 之前，与原始二进制保持一致）
			SetColumnWidth(txTable, refTable);
			SetRowHeight(txTable, refTable);

			// 填充内容（使用不带格式的版本保留原有字体等格式）
			SetCaptionStyleWithoutFormat(txTable, refTable);
			SetCaptionContentsWithFormat(txTable, refTable);
			MergeCaptionCells(refTable, txTable);
			SetBodyContentsWithoutFormat(txTable, refTable);
			MergeBodyCells(txTable, refTable);

			// 应用表格边框样式
		if (bookmark?.TableStyle.HasValue == true && bookmark.TableStyle.Value == 5)
			{
				SetSidaStyle(txTable);
			}
			else if (bookmark?.TableStyle.HasValue == true && bookmark.TableStyle.Value == Leqisoft.Model.TableBorderStyles.Custom)
			{
				// 自定义样式：从书签的 CustomBorderStyleJson 反序列化
				if (!string.IsNullOrEmpty(bookmark.CustomBorderStyleJson))
				{
					var customStyle = Leqisoft.Model.TableBorderStyle.FromJson(bookmark.CustomBorderStyleJson);
					if (customStyle != null)
					{
						txTable.Select();
						SetCustomTableBorder(txTable, customStyle, getCaptionRows(refTable));
					}
					else
					{
						// 反序列化失败，降级为默认样式
						txTable.Select();
						SetTableBorder(txTable, Leqisoft.Model.TableBorderStyles.Grid, getCaptionRows(refTable));
					}
				}
				else
				{
					// 无 JSON 配置，降级为默认样式
					txTable.Select();
					SetTableBorder(txTable, Leqisoft.Model.TableBorderStyles.Grid, getCaptionRows(refTable));
				}
			}
			else
			{
				var borderStyle = bookmark?.TableStyle.HasValue == true
					? Leqisoft.Model.TableBorderStyles.FromNumber(bookmark.TableStyle.Value)
					: refTable.BorderStyle;
				txTable.Select();
				SetTableBorder(txTable, borderStyle, getCaptionRows(refTable));
			}

			Application.DoEvents();

			// 更新书签
		if (bookmark != null)
		{
			ModifyBookmark(txTable, bookmark);
		}

			NeedSave = true;
			return refTable;
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshModelTableWithFormatImpl");
			_tx.ClearUndo();
			return null;
		}
		finally
		{
			_isRefreshingTable = false;
			// 刷新后更新表格位置缓存，防止 InputPositionChanged 重复弹出
			try
			{
				var firstCell = txTable?.Cells.GetItem(1, 1);
				if (firstCell != null)
					_lastTableId = firstCell.Start;
			} catch { }
		}
	}

	private void RefreshTableContentsImpl(TXTextControl.Table txTable, Leqisoft.Model.Table refTable)
	{
		try
		{
			// Reload source table data
			refTable.LoadAndReturn();

			int captionRows = getCaptionRows(refTable);
			int totalRows = txTable.Rows.Count;
			int totalCols = Math.Min(txTable.Columns.Count, refTable.Columns.VisibleCount);

			for (int r = 1; r <= totalRows; r++)
			{
				bool isCaption = r <= captionRows;
				int dataRow = r - captionRows - 1; // 0-based index in model rows

				for (int c = 1; c <= totalCols; c++)
				{
					try
					{
						var txCell = txTable.Cells.GetItem(r, c);
						if (txCell == null) continue;

						txCell.Select();

						if (isCaption)
						{
							// Caption row: set column name
							var column = refTable.Columns[c - 1];
							txCell.Text = column?.CaptionDisplay ?? "";
						}
						else if (dataRow >= 0 && dataRow < refTable.Rows.Count)
						{
							// Data row: set display value or formula reference
							var sourceCell = refTable[dataRow, c - 1];
							if (sourceCell != null)
							{
								txCell.Text = sourceCell.GetDisplayValue() ?? "";
							}
						}
					}
					catch { }
				}
			}
		}
		catch (Exception ex) { ex.Log("DocumentEditor.RefreshTableContentsImpl"); }
	}

	// ==== Missing helper methods restored from original IL ====

	private void SetCaptionStyleWithoutFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		var colEnum = txTable.Columns.GetEnumerator();
		try
		{
			while (colEnum.MoveNext())
			{
				var txCol = (TXTextControl.TableColumn)colEnum.Current;
				var modelCol = modelTable.Columns[txCol.Column - 1];
				if (modelCol?.CaptionStyle?.Align.HasValue == true)
				{
					// 选中该列的页眉行
					txTable.Select(1, txCol.Column, capRows, txCol.Column);
					ConvertAlign(modelCol.CaptionStyle.Align.Value, out var hAlign, out var vAlign);
					if (_tx.Selection.ParagraphFormat.Alignment != hAlign)
						_tx.Selection.ParagraphFormat.Alignment = hAlign;
					// 设置每行单元格的垂直对齐
					for (int r = 1; r <= capRows; r++)
					{
						var cell = txTable.Cells.GetItem(r, txCol.Column);
						if (cell != null && cell.CellFormat.VerticalAlignment != vAlign)
							cell.CellFormat.VerticalAlignment = vAlign;
					}
				}
			}
		}
		finally
		{
			var disposable = colEnum as IDisposable;
			if (disposable != null) disposable.Dispose();
		}
	}

	private void SetBodyContentsWithoutFormat(TXTextControl.Table txTable, Leqisoft.Model.Table modelTable)
	{
		int capRows = getCaptionRows(modelTable);
		for (int dataRow = 0; dataRow < modelTable.Rows.Count; dataRow++)
		{
			for (int visibleColIdx = 0; visibleColIdx < modelTable.Columns.VisibleCount; visibleColIdx++)
			{
				var col = modelTable.Columns.GetVisibleColumnAt(visibleColIdx);
				if (col == null) continue;
				var sourceCell = modelTable[dataRow, col.Index];
				if (sourceCell == null) continue;

				int txRow = dataRow + capRows + 1;
				int txCol = visibleColIdx + 1;
				var txCell = txTable.Cells.GetItem(txRow, txCol);
				if (txCell == null) continue;

				// 仅当内容变化时才更新（WithoutFormat = 不强制格式）
				string newValue = sourceCell.GetDisplayValue(true);
				if (txCell.Text != newValue)
					txCell.Text = newValue;

				// 设置对齐
				txCell.Select();
				ConvertAlign(sourceCell.DisplayAlign, out var hAlign, out var vAlign);
				if (_tx.Selection.ParagraphFormat.Alignment != hAlign)
					_tx.Selection.ParagraphFormat.Alignment = hAlign;
				if (txCell.CellFormat.VerticalAlignment != vAlign)
					txCell.CellFormat.VerticalAlignment = vAlign;
			}
			Application.DoEvents();
		}
	}

	private void RemoveRows(TXTextControl.Table table, IEnumerable<int> rowIndices)
	{
		foreach (int idx in rowIndices.OrderByDescending(i => i))
		{
			if (idx > table.Rows.Count)
				continue;
			var cell = table.Cells.GetItem(idx, 1);
			if (cell != null)
			{
				cell.Select();
				table.Rows.Remove();
			}
		}
	}

	private void RemoveColumns(TXTextControl.Table table, IEnumerable<int> columnIndices)
	{
		foreach (int idx in columnIndices.OrderByDescending(i => i))
		{
			if (idx > table.Columns.Count)
				continue;
			var cell = table.Cells.GetItem(1, idx);
			if (cell != null)
			{
				cell.Select();
				table.Columns.Remove();
			}
		}
	}

	private void InsertCol(TXTextControl.Table table, TXTextControl.TableAddPosition position)
	{
		table.Columns.Add(position);
	}

	private void ModifyBookmark(TXTextControl.Table table, LeqiBookmark bookmark)
	{
		bookmark.TagModifiedIfNotNew();
		string newName = bookmark.GetString();

		// 将 ParaId→TableId 缓存起来，防止 OOXML 保存后丢失
		if (bookmark.ParaIdBase64 != null && bookmark.TableId != null)
			_bookmarkTableIdCache[bookmark.ParaIdBase64] = bookmark.TableId;

		// 通过 ParaIdBase64 匹配找到正确的 DocumentTarget，而不是用位置查找
		// （刷新后表格位置可能变化，位置查找会找到错误的书签）
		var allTargets = _textControl.DocumentTargets;
		if (allTargets != null && !string.IsNullOrEmpty(bookmark.ParaIdBase64))
		{
			foreach (TXTextControl.DocumentTarget dt in allTargets)
			{
				try
				{
					if (dt.TargetName != null && LeqiBookmark.TryParse(dt.TargetName, out var bm))
					{
						if (bm.ParaIdBase64 == bookmark.ParaIdBase64)
						{
							dt.TargetName = newName;
							return;
						}
					}
				}
				catch { }
			}
		}

		// 回退：如果没找到匹配的书签，在表格前位置创建新的
		var firstCell = table.Cells.GetItem(1, 1);
		if (firstCell == null) return;

		_isCheckingTableRef = true;
		try
		{
			_tx.Select(firstCell.Start - 1, 0);
			var newDt = new TXTextControl.DocumentTarget(newName);
			_tx.DocumentTargets.Add(newDt);
		}
		catch (Exception ex) { ex.Log("DocumentEditor.ModifyBookmark"); }
		finally
		{
			try { _tx.Select(firstCell.Start, 0); } catch { }
			_isCheckingTableRef = false;
		}
	}

	private void ResumeSidaStyle(TXTextControl.Table table)
	{
		if (table == null) return;
		if (table.Rows.Count < 1 || table.Columns.Count < 1) return;

		table.Select();
		// 如果当前没有下划线格式（非 20992 = FontUnderlineStyle.Words），则设置
		if (_tx.Selection.Underline != (TXTextControl.FontUnderlineStyle)20992)
			_tx.Selection.Underline = (TXTextControl.FontUnderlineStyle)20992;
	}

	public void BeginFormatPainter()
	{
		if (IsDocumentLocked() || IsFormatPainting)
		{
			return;
		}
		AppCommands.FormatBrush.IsPressed = true;
		_textControl.Cursor = _curFormatPainter;
		_textControl.MouseUp += Tx_MouseUp_FormatPainter;
		_textControl.KeyDown += Tx_KeyDown_FormatPainter;
		_formatPainterContext = FormatPainterContext.FromSelection(_textControl.Selection);
		IsFormatPainting = true;
		AppCommands.Information.ShowInformation("状态提示", "当前处于格式刷状态，按Esc键可退出格式刷状态。");
		Program.MainForm.SwitchStateTo(MainFormView.DocFormatBrush);
	}
	public void EndFormatPainter()
	{
		if (!IsFormatPainting)
		{
			return;
		}
		_textControl.Cursor = Cursors.IBeam;
		_textControl.MouseUp -= Tx_MouseUp_FormatPainter;
		_textControl.KeyDown -= Tx_KeyDown_FormatPainter;
		AppCommands.FormatBrush.IsPressed = false;
		IsFormatPainting = false;
		AppCommands.Information.HideInformation();
		Program.MainForm.SwitchStateTo(MainFormView.Document);
	}

	private void Tx_MouseUp_FormatPainter(object sender, MouseEventArgs e)
	{
		_formatPainterContext?.Apply(_textControl.Selection);
		OnFormatChanged();
	}

	private void Tx_KeyDown_FormatPainter(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			EndFormatPainter();
		}
	}

	/// <summary>
	/// 初始化选区转域校验用的右键菜单（仅当选区非空时显示）。
	/// </summary>
	private void InitializeValidationContextMenu()
	{
		_validationContextMenu = new ContextMenuStrip();
		_validationContextMenu.Items.Add("添加为校验点", null, (s, e) => AddValidationPoint());
		_validationContextMenu.Items.Add(new ToolStripSeparator());
		_validationContextMenu.Items.Add("复制", null, (s, e) => { try { _textControl.Copy(); } catch { } });
		_validationContextMenu.Items.Add("剪切", null, (s, e) => { try { _textControl.Cut(); } catch { } });
		_validationContextMenu.Items.Add("粘贴", null, (s, e) => { try { _textControl.Paste(); } catch { } });

		_removeValidationContextMenu = new ContextMenuStrip();
		_removeValidationContextMenu.Items.Add("解除校验点", null, (s, e) => RemoveValidationPoint());

		// Formula 域右键菜单：添加稽核规则
		_formulaValidationContextMenu = new ContextMenuStrip();
		_formulaValidationContextMenu.Items.Add("添加稽核规则", null, (s, e) => AddValidationRuleToFormulaField());
		_formulaValidationContextMenu.Items.Add(new ToolStripSeparator());
		_formulaValidationContextMenu.Items.Add("复制", null, (s, e) => { try { _textControl.Copy(); } catch { } });
		_formulaValidationContextMenu.Items.Add("粘贴", null, (s, e) => { try { _textControl.Paste(); } catch { } });
	}

	/// <summary>
	/// 右键菜单即将打开时：根据光标位置决定显示哪些菜单项。
	/// - 光标在 DocValidation 域上 → 显示"解除校验点"
	/// - 光标在 Formula 域上 → 显示"添加稽核规则"
	/// - 选区非空 → 显示"添加为校验点"等
	/// </summary>
	private void _textControl_TextContextMenuOpening(object sender, TXTextControl.TextContextMenuEventArgs e)
	{
		try
		{
			if (_textControl == null) return;

			var field = GetCurrentApplicationField();

			// 优先检测 DocValidation 域：光标在域上时显示"解除校验点"
			if (IsDocValidationField(field))
			{
				e.Cancel = true;
				var clientPos = _textControl.PointToClient(e.Location);
				_removeValidationContextMenu.Show(_textControl, clientPos);
				return;
			}

			// 光标在 Formula 域上时显示"添加稽核规则"
			if (IsFormulaField(field))
			{
				e.Cancel = true;
				var clientPos = _textControl.PointToClient(e.Location);
				_formulaValidationContextMenu.Show(_textControl, clientPos);
				return;
			}

			// 选区非空时显示"添加为校验点"菜单
			if (_textControl.Selection.Length > 0)
			{
				e.Cancel = true;
				var clientPos = _textControl.PointToClient(e.Location);
				_validationContextMenu.Show(_textControl, clientPos);
			}
		}
		catch { }
	}

	public void AttachEvents(params object[] args)
	{
		// DocumentStructure 在生成结构图后调用，恢复事件响应
		try
		{
			_textControl.TextFieldClicked += _textControl_TextFieldClicked;
			_textControl.InputPositionChanged += _textControl_InputPositionChanged;
			// 监听 TextChanged 事件，文本变化时更新撤销/恢复按钮状态
			_textControl.TextChanged += _textControl_TextChanged;
		}
		catch { }
	}
	public void DetachEvents(params object[] args)
	{
		// DocumentStructure 在生成结构图前调用，暂停事件响应避免干扰
		try
		{
			_textControl.TextFieldClicked -= _textControl_TextFieldClicked;
			_textControl.InputPositionChanged -= _textControl_InputPositionChanged;
			_textControl.TextChanged -= _textControl_TextChanged;
		}
		catch { }
	}
	public void Tx_InputPositionChanged(object sender, EventArgs e)
	{
		if (_isUndoing) return;
		if (_isRefreshingApplicationField) return;

		var doc = Document as Leqisoft.Model.Document;
		if (doc != null)
		{
			TreeNodeCacheState state;
			if (TreeNodeStateCache.Contains(doc.Id))
			{
				state = TreeNodeStateCache.Get(doc.Id);
			}
			else
			{
				state = new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.Document,
					ScrollPosition = Point.Empty
				};
			}
			state.Selection = new Rectangle(_textControl.Selection.Start, _textControl.Selection.Length, 0, 0);
			if (!TreeNodeStateCache.Contains(doc.Id))
			{
				TreeNodeStateCache.Set(doc.Id, state);
			}
		}

		var af = _textControl.ApplicationFields.GetItem();
		if (af != null && _textControl.Selection.Length == 0)
		{
			int textPos = _textControl.InputPosition.TextPosition;
			if (af.Length > 1)
			{
				int afStart = af.Start - 1;
				if (afStart + af.Length == textPos || afStart == textPos)
				{
					_textControl.InputPosition = new TXTextControl.InputPosition(textPos, (TXTextControl.TextFieldPosition)2);
				}
			}
		}

		_lastTtpComment = null;
		PopulateToolbar();
		Program.MainForm.FormulaEditor.Context.Kind = Leqisoft.Model.FormulaContextKind.Document;
		Program.MainForm.FormulaEditor.AF = af;
		Program.MainForm.FormulaEditor.Populate();
	}

	public void PopulateToolbar()
	{
		bool hasTable = GetCurrentTable() != null;
		cmdRefreshTable2.Visible = hasTable;
		cmdFollowTable2.Visible = hasTable;
		cmdImportTable.Visible = !hasTable;
		bool canGenerate = TryGetMergeTable(out var _, out var __);
		cmdGenerateConfirmation.Visible = hasTable && canGenerate;
	}

	public void OnChanged(params object[] args) { NeedSave = true; }
	public void OnEnterView(params object[] args)
	{
		// MainForm.SwitchStateTo 中调用，传入 bool isPreview
		// args[0] = true 表示预览模式，false 表示编辑模式
		try
		{
			if (args?.Length > 0 && args[0] is bool isPreview && isPreview)
			{
				EnterPreviewMode();
			}

			// 进入文档视图时自动显示文档结构图面板（含"点击此处生成文档结构图"链接）
			ShowStructure();

			_textControl.Focus();
		}
		catch { }
	}
	public void OnFormulaEditorBeganEditing()
	{
		try
		{
			Program.MainForm.SuspendNavPanelVisible();
			Program.MainForm.SwitchStateTo(MainFormView.EditingFormula);
			View.Enabled = false;
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
		}
	}

	public void OnFormulaEditorFinishedEditing()
	{
		View.Enabled = true;
		Program.MainForm.SwitchStateTo(MainFormView.Document);
	}
	public Color GetEmptyAreaBackgroundColor()
	{
		if (_textControl == null) return Color.White;
		return _textControl.DisplayColors.DesktopColor;
	}
	public bool TryGetMergeTable(out object mergeTable, out long tableId)
	{
		mergeTable = null;
		tableId = 0;
		try
		{
			var table = _textControl.Tables.GetItem();
			if (table != null && IsReferencedTable(table))
			{
				// 方法1：通过 Document.MergeTable 获取引用表格ID（原始软件数据库层方式）
				try
				{
					var doc = Document as Leqisoft.Model.Document;
					if (doc != null && doc.MergeTable != null && doc.MergeTable.Value != 0)
					{
						var project = Project.Current;
						var node = project?.GetNodeById(new Leqisoft.DTO.Id64(doc.MergeTable.Value)) as TreeTableNode;
						if (node != null)
						{
							mergeTable = node.Table;
							tableId = node.Table.Id.Value;
							return true;
						}
					}
				}
				catch (Exception)
				{
				}

				// 方法2：通过 DocumentTarget 获取引用表格ID
				_tx.Select(table.Cells.GetItem(1, 1).Start - 1, 0);
				DocumentTarget dt = _tx.DocumentTargets.GetItem();
				if (dt != null && LeqiBookmark.TryParse(dt.TargetName, out var lsbm) && lsbm.TableId != null)
				{
					Leqisoft.DTO.Id64 id = Leqisoft.DTO.Id64.ParseBase64(lsbm.TableId);
					var project = Project.Current;
					var node = project?.GetNodeById(id) as TreeTableNode;
					if (node != null)
					{
						mergeTable = node.Table;
						tableId = node.Table.Id.Value;
						return true;
					}
				}

				// 方法2：通过 ApplicationField 的公式获取引用表格ID（当前版本的方式）
				foreach (TXTextControl.TableCell cell in table.Cells)
				{
					try
					{
						int cellStart = cell.Start;
						int cellEnd = cellStart + cell.Length;
						foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
						{
							if (f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
							int fieldEnd = f.Start + f.Length;
							if (f.Start >= cellStart && fieldEnd <= cellEnd)
							{
								string formula = f.Parameters[1];
								if (!string.IsNullOrWhiteSpace(formula))
								{
									var evaluator = new FormulaEvaluator(formula);
									var ids = evaluator.GetReferredTableIds();
									if (ids.Count > 0)
									{
										var project = Project.Current;
										var node = project?.GetNodeById(ids.First()) as TreeTableNode;
										if (node != null)
										{
											mergeTable = node.Table;
											tableId = node.Table.Id.Value;
											return true;
										}
									}
								}
							}
						}
					}
					catch { }
				}
			}
		}
		catch { }
		return false;
	}
	public async Task RefreshAllTablesAndFormulas()
	{
		try
		{
			foreach (TXTextControl.Table table in _textControl.Tables)
			{
				try
				{
					if (IsReferencedTable(table))
					{
						RefreshTableFormulas(table);
					}
				}
				catch { }
			}
		}
		catch { }
		await Task.CompletedTask;
	}

	/// <summary>
	/// 全表刷新：遍历文档中所有表格，逐个执行带格式的表格刷新（从源数据重新加载并调整行列/样式/书签）。
	/// </summary>
	public void RefreshAllTables()
	{
		try
		{
			if (IsDocumentLocked()) return;

			int savedStart = _textControl.Selection.Start;
			int savedLength = _textControl.Selection.Length;

			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			DetachEvents();

			try
			{
				int count = _textControl.Tables.Count;
				TXTextControl.Table[] tables = new TXTextControl.Table[count];
				_textControl.Tables.CopyTo(tables, 0);

				// 过滤出有书签引用的表格
				var refTables = tables.Where(txTable =>
				{
					if (txTable == null || txTable.Cells.Count == 0) return false;
					return GetRefTable(txTable, out _, out var refTable) && refTable != null;
				}).ToList();

				if (refTables.Count == 0) return;

				Leqisoft.UI.Controls.Util.ProcessItemsWithProgress(
					_textControl,
					refTables,
					"正在刷新表格",
					(txTable, i, total) => RefreshModelTableWithFormatImpl(txTable));
			}
			finally
			{
				AttachEvents();
				Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
				_textControl.Select(savedStart, savedLength);
			}

			OnChanged();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshAllTables");
		}
	}

	/// <summary>
	/// 全域刷新：遍历文档正文及页眉页脚中所有 MERGEFIELD Formula 域，重新计算并更新显示值。
	/// </summary>
	public void RefreshAllFields()
	{
		try
		{
			if (IsDocumentLocked()) return;

			int savedStart = _textControl.Selection.Start;
			int savedLength = _textControl.Selection.Length;

			DetachEvents();
			try
			{
				// 刷新正文域
				RefreshFieldsInCollection(_textControl.ApplicationFields);

				// 刷新页眉页脚中的域
				try
				{
					foreach (TXTextControl.HeaderFooter hf in _textControl.HeadersAndFooters)
					{
						try { RefreshFieldsInCollection(hf.ApplicationFields); }
						catch { }
					}
				}
				catch { }
			}
			finally
			{
				AttachEvents();
				_textControl.Select(savedStart, savedLength);
			}

			OnChanged();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshAllFields");
		}
	}

	private void RefreshFieldsInCollection(TXTextControl.ApplicationFieldCollection fields)
	{
		if (fields == null) return;
		// 先收集到数组，避免刷新过程中集合变动导致迭代异常
		int count = fields.Count;
		TXTextControl.ApplicationField[] arr = new TXTextControl.ApplicationField[count];
		fields.CopyTo(arr, 0);

		foreach (var field in arr)
		{
			try
			{
				if (field == null) continue;
				// 仅刷新 Formula 域（求值域）；跳过 DocValidation 域（其 Text 是用户原文，不应被公式求值覆盖）
				if (field.TypeName != "MERGEFIELD" || field.Parameters == null
					|| field.Parameters.Length < 2 || field.Parameters[0] != "Formula") continue;

				string formula = field.Parameters[1];
				if (string.IsNullOrWhiteSpace(formula)) continue;

				string result = FormulaEditor.EvaluateDocumentFormula(formula, calculateTable: false);
				if (string.IsNullOrEmpty(result)) continue;

				field.Text = result;
				// 保留 Parameters[3]（稽核规则 JSON），避免刷新公式时丢失
				var refreshedParams = new List<string> { "Formula", formula, field.Parameters[2] };
				if (field.Parameters.Length >= 4) refreshedParams.Add(field.Parameters[3]);
				field.Parameters = refreshedParams.ToArray();
			}
			catch (Exception ex) { ex.Log("DocumentEditor.RefreshAllFields[field]"); }
		}
	}

	/// <summary>
	/// 全文刷新：先执行全表刷新（带格式），再执行全域刷新（公式域）。
	/// </summary>
	public void RefreshDocumentAll()
	{
		try
		{
			if (IsDocumentLocked()) return;

			int savedStart = _textControl.Selection.Start;
			int savedLength = _textControl.Selection.Length;

			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			DetachEvents();

			try
			{
				new ProgressForm<object>(async delegate(IProgress<Leqisoft.DTO.ProgressInfo> iProg)
				{
					// Step 1: 全表刷新
					int count = _textControl.Tables.Count;
					TXTextControl.Table[] tables = new TXTextControl.Table[count];
					_textControl.Tables.CopyTo(tables, 0);

					var refTables = tables.Where(txTable =>
					{
						if (txTable == null || txTable.Cells.Count == 0) return false;
						return GetRefTable(txTable, out _, out var refTable) && refTable != null;
					}).ToList();

					int total = refTables.Count;
					for (int i = 0; i < total; i++)
					{
						iProg.Report(new Leqisoft.DTO.ProgressInfo
						{
							MainCaption = $"正在刷新表格 ({i + 1}/{total})...",
							MainProgress = (int)((double)i / total * 80.0)
						});

						int idx = i;
						_textControl.Invoke(new Action(() =>
						{
							RefreshModelTableWithFormatImpl(refTables[idx]);
						}));

						await Task.Delay(1);
					}

					// Step 2: 全域刷新（公式域）
					iProg.Report(new Leqisoft.DTO.ProgressInfo
					{
						MainCaption = "正在计算公式域...",
						MainProgress = 80
					});

					_textControl.Invoke(new Action(RefreshAllFieldsInternal));

					iProg.Report(new Leqisoft.DTO.ProgressInfo
					{
						MainCaption = "全文刷新完成",
						MainProgress = 100
					});
					return (object)null;
				}).ShowDialog();
			}
			finally
			{
				AttachEvents();
				Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
				_textControl.Select(savedStart, savedLength);
			}

			OnChanged();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshDocumentAll");
		}
	}

	private void RefreshAllTablesInternal()
	{
		try
		{
			int count = _textControl.Tables.Count;
			TXTextControl.Table[] tables = new TXTextControl.Table[count];
			_textControl.Tables.CopyTo(tables, 0);

			foreach (var txTable in tables)
			{
				try
				{
					if (txTable == null || txTable.Cells.Count == 0) continue;
					if (GetRefTable(txTable, out var bookmark, out var refTable) && refTable != null)
					{
						RefreshModelTableWithFormatImpl(txTable);
					}
				}
				catch (Exception ex) { ex.Log("DocumentEditor.RefreshAllTablesInternal[iter]"); }
			}
		}
		catch (Exception ex) { ex.Log("DocumentEditor.RefreshAllTablesInternal"); }
	}

	private void RefreshAllFieldsInternal()
	{
		try
		{
			RefreshFieldsInCollection(_textControl.ApplicationFields);

			try
			{
				foreach (TXTextControl.HeaderFooter hf in _textControl.HeadersAndFooters)
				{
					try { RefreshFieldsInCollection(hf.ApplicationFields); }
					catch { }
				}
			}
			catch { }
		}
		catch (Exception ex) { ex.Log("DocumentEditor.RefreshAllFieldsInternal"); }
	}
	public byte[] Export()
	{
		try
		{
			byte[] data;
			_textControl.Save(out data, TXTextControl.BinaryStreamType.WordprocessingML);
			return data;
		}
		catch { return null; }
	}
	public bool GlobalHighlightMode { get; set; } = true;
	public Color GetFieldColor(params object[] args) { return Color.FromArgb(200, 220, 240); }
	public bool HasValidFormula(params object[] args)
	{
		try
		{
			var field = _textControl.ApplicationFields.GetItem();
			if (field != null && field.TypeName == "MERGEFIELD" && field.Parameters != null && field.Parameters.Length >= 2 && field.Parameters[0] == "Formula")
			{
				return !string.IsNullOrWhiteSpace(field.Parameters[1]);
			}
		}
		catch { }
		return false;
	}
	public string GetFormulaFromAF(params object[] args)
	{
		try
		{
			var field = _textControl.ApplicationFields.GetItem();
			if (field != null && field.TypeName == "MERGEFIELD" && field.Parameters != null && field.Parameters.Length >= 2 && field.Parameters[0] == "Formula")
			{
				return field.Parameters[1] ?? "";
			}
		}
		catch { }
		return "";
	}
	public void ParaFormatDialog(params object[] args) { _textControl.ParagraphFormatDialog(); }

	private bool _isShowingFieldTooltip;

	private void _textControl_TextFieldClicked(object sender, TXTextControl.TextFieldEventArgs e)
	{
		try
		{
			var field = e.TextField;

			if (field == null) return;

			// 检查运行时类型
			var appField = field as TXTextControl.ApplicationField;

			if (appField == null)
			{
				// 如果直接转换失败，尝试通过 ApplicationFields.GetItem()
				try
				{
					var getItemField = _textControl.ApplicationFields.GetItem();
					if (getItemField != null)
					{
						appField = getItemField;
					}
				}
				catch (Exception)
				{
				}
			}

			if (appField == null)
			{
				return;
			}


			var xBody = new System.Xml.Linq.XElement("div");
			var linkDic = new Dictionary<string, object>();

			xBody.Add(new System.Xml.Linq.XElement("b", "运算公式"));
			xBody.Add(new System.Xml.Linq.XElement("p", appField.Text ?? "无公式"));
			xBody.Add(new System.Xml.Linq.XElement("hr"));

			var table = new System.Xml.Linq.XElement("table");
			var row1 = new System.Xml.Linq.XElement("tr");
			var row2 = new System.Xml.Linq.XElement("tr");

			linkDic.Add("refresh", appField);
			row1.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "refresh"), "刷新运算结果")));

			linkDic.Add("delete", appField);
			row1.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "delete"), "删除运算结果")));

			linkDic.Add("nohighlight", appField);
			row2.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "nohighlight"), "不高亮显示")));

			linkDic.Add("trace", appField);
			row2.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "trace"), "追踪数据")));

			table.Add(row1);
			table.Add(row2);
			xBody.Add(table);

			// 设置标志，防止 InputPositionChanged 立即关闭或覆盖域提示框
			_isShowingFieldTooltip = true;

			_ttpComment.SetText("运算公式", xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(linkDic);
			_ttpComment.LinkClicked += _ttpComment_LinkClicked;

			// 绑定输入位置变化事件，光标离开域时关闭提示框
			_textControl.InputPositionChanged += OnInputPositionChangedForTooltip;

			// 使用鼠标当前位置显示提示框
			var mousePos = Control.MousePosition;
			var clientPos = _textControl.PointToClient(mousePos);
			_ttpComment.Show(_textControl, new Point(clientPos.X + 10, clientPos.Y + 10));
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor._textControl_TextFieldClicked");
		}
	}

	/// <summary>
	/// 光标位置变化时检查是否需要关闭提示框（光标离开域时关闭）
	/// </summary>
	private void OnInputPositionChangedForTooltip(object sender, EventArgs e)
	{
		try
		{
			// 跳过刚点击域后的首次 InputPositionChanged，避免立即关闭刚显示的提示框
			if (_isShowingFieldTooltip)
			{
				_isShowingFieldTooltip = false;
				return;
			}

			var currentField = _textControl.ApplicationFields.GetItem();
			if (currentField == null)
			{
				HideTooltip();
			}
		}
		catch
		{
			HideTooltip();
		}
	}

	/// <summary>
	/// 隐藏提示框并清理事件绑定
	/// </summary>
	private void HideTooltip()
	{
		_textControl.InputPositionChanged -= OnInputPositionChangedForTooltip;
		_textControl.MouseDown -= OnTextControlMouseDown;
		_ttpComment.Hide();
	}

	private void OnTextControlMouseDown(object sender, MouseEventArgs e)
	{
		HideTooltip();
	}

	private void _ttpComment_LinkClicked(object sender, object e)
	{
		try
		{
			var href = sender as string;
			if (string.IsNullOrEmpty(href))
			{
				HideTooltip();
				return;
			}

			// 追踪导航链接（tracecell_/tracecol_/tracetable_/tracerange_）
			if (href.StartsWith("tracecell_") || href.StartsWith("tracecol_") || href.StartsWith("tracetable_") || href.StartsWith("tracerange_"))
			{
				HideTooltip();
				HandleTraceNavigation(href, e);
				return;
			}

			if (!(e is TXTextControl.ApplicationField appField))
			{
				HideTooltip();
				return;
			}

			switch (href)
			{
				case "refresh":
					RefreshFieldValue(appField);
					break;
				case "delete":
					DeleteField(appField);
					break;
				case "nohighlight":
				appField.HighlightMode = TXTextControl.HighlightMode.Never;
				break;
			case "trace":
				HideTooltip();
				TraceFieldSource(appField);
				return;  // 不执行默认的 Hide()，由 TraceFieldSource 自行管理提示框生命周期
			}
			HideTooltip();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor._ttpComment_LinkClicked");
			HideTooltip();
		}
	}

	private void RefreshFieldValue(TXTextControl.ApplicationField field)
	{
		try
		{
			// 获取域的公式（存储在 Parameters[1] 中）
			if (field.Parameters != null && field.Parameters.Length >= 2 && field.Parameters[0] == "Formula")
			{
				string formula = field.Parameters[1];
				string result = FormulaEditor.EvaluateDocumentFormula(formula, calculateTable: false);

				// 与 FormulaEditor 更新域的方式完全一致：直接设置 Text 和 Parameters
				field.Text = result;
				// 保留 Parameters[3]（稽核规则 JSON），避免刷新公式时丢失
				var refreshedParams = new List<string> { "Formula", formula, field.Parameters[2] };
				if (field.Parameters.Length >= 4) refreshedParams.Add(field.Parameters[3]);
				field.Parameters = refreshedParams.ToArray();

				_textControl.Select(field.Start - 1 + field.Length, 0);
				OnChanged();
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshFieldValue");
		}
	}

	private void DeleteField(TXTextControl.ApplicationField field)
	{
		try
		{
			_textControl.ApplicationFields.Remove(field);
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.DeleteField");
		}
	}

	/// <summary>
	/// 处理追踪导航链接（跳转到表格/列/单元格）
	/// </summary>
	private void HandleTraceNavigation(string href, object tag)
	{
		try
		{
			if (href.StartsWith("tracecell_") && tag is Leqisoft.Model.Cell cell)
			{
				NavigateToCell(cell);
			}
			else if (href.StartsWith("tracecol_") && tag is Leqisoft.Model.Column column)
			{
				NavigateToColumn(column);
			}
			else if (href.StartsWith("tracetable_") && tag is TreeTableNode tableNode)
			{
				NavigateToTable(tableNode);
			}
			else if (href.StartsWith("tracerange_") && tag is RangeOperand range)
			{
				NavigateToRange(range);
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.HandleTraceNavigation");
		}
	}

	/// <summary>
	/// 导航到单元格
	/// </summary>
	private void NavigateToCell(Leqisoft.Model.Cell cell)
	{
		if (cell?.Column?.Table == null) return;
		if (cell.Column.Table != Program.MainForm.CurrentTable)
		{
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
		}
		Program.MainForm.TableEditor.Select(cell.Row.Index, cell.Column.Index);
	}

	/// <summary>
	/// 导航到列
	/// </summary>
	private void NavigateToColumn(Leqisoft.Model.Column column)
	{
		if (column?.Table == null) return;
		if (column.Table != Program.MainForm.CurrentTable)
		{
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
		}
		Program.MainForm.TableEditor.SelectColumn(column.Index);
	}

	/// <summary>
	/// 导航到表格
	/// </summary>
	private void NavigateToTable(TreeTableNode tableNode)
	{
		if (tableNode == null) return;
		Program.MainForm.ProjectHierarchy.FindAndSelectNode(tableNode);
	}

	/// <summary>
	/// 导航到范围
	/// </summary>
	private void NavigateToRange(RangeOperand range)
	{
		if (range?.Table == null) return;
		if (range.Table != Program.MainForm.CurrentTable)
		{
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(range.Table.TreeNode);
		}
		Program.MainForm.TableEditor.Select(range.TopLeft.Row.Index, range.TopLeft.Column.Index,
			range.BottomRight.Row.Index, range.BottomRight.Column.Index);
	}

	/// <summary>
	/// 追踪数据来源
	/// </summary>
	private void TraceFieldSource(TXTextControl.ApplicationField appField)
	{
		try
		{
			if (appField.Parameters == null || appField.Parameters.Length < 2 || appField.Parameters[0] != "Formula")
				return;

			string formula = appField.Parameters[1];
			if (string.IsNullOrWhiteSpace(formula)) return;

			var project = Leqisoft.Model.Project.Current;
			if (project == null) return;

			var resolver = new FormulaReferenceModelResolver(project);
			var evaluator = new FormulaEvaluator(formula);

			// 获取引用的表格
			var tableIds = evaluator.GetReferredTableIds();
			var refs = evaluator.GetReferences(resolver);

			var xBody = new System.Xml.Linq.XElement("div");
			xBody.Add(new System.Xml.Linq.XElement("b", "数据来源追踪（点击跳转）"));

			var tagDic = new Dictionary<string, object>();
			int linkIndex = 0;

			// 表格引用
			if (tableIds.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用表格 ==="));
				foreach (var tid in tableIds)
				{
					var table = project.GetTableById(tid);
					string name = table?.GetCanonicalName() ?? $"表格ID={tid}";
					string href = $"tracetable_{linkIndex++}";
					// 存储 TreeTableNode，避免直接使用 Id64 类型
					var tableNode = project.GetNodeById(tid) as TreeTableNode;
					tagDic[href] = tableNode;
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📊 " + name)));
				}
			}

			// 列引用
			if (refs.ColumnReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用列 ==="));
				foreach (var col in refs.ColumnReferences)
				{
					string href = $"tracecol_{linkIndex++}";
					tagDic[href] = col;
					string caption = col.Caption ?? $"列[{col.Index}]";
					string tableName = col.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📋 " + caption + " (" + tableName + ")")));
				}
			}

			// 单元格引用
			if (refs.CellReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用单元格 ==="));
				foreach (var cell in refs.CellReferences)
				{
					string href = $"tracecell_{linkIndex++}";
					tagDic[href] = cell;
					string val = cell.GetDisplayValue(applyZeroFormat: false);
					string tableName = cell.Column?.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📌 第" + (cell.Row.Index + 1) + "行: " + val + " (" + tableName + ")")));
				}
			}

			// 范围引用
			if (refs.RangeReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用区域 ==="));
				foreach (var range in refs.RangeReferences)
				{
					string href = $"tracerange_{linkIndex++}";
					tagDic[href] = range;
					string tableName = range.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), $"📐 {range.TopLeft.Row.Index + 1}:{range.TopLeft.Column.Index + 1} ~ {range.BottomRight.Row.Index + 1}:{range.BottomRight.Column.Index + 1} ({tableName})")));
				}
			}

			// 显示解析后的公式
			try
			{
				string display = evaluator.GetDisplayString(resolver);
				xBody.Add(new System.Xml.Linq.XElement("hr"));
				xBody.Add(new System.Xml.Linq.XElement("p", "解析公式: " + display));
			}
			catch
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "解析公式: " + formula));
			}

			_ttpComment.SetText("追踪数据", xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(tagDic);
			_textControl.MouseDown += OnTextControlMouseDown;

			var mousePos = Control.MousePosition;
			var clientPos = _textControl.PointToClient(mousePos);
			_ttpComment.Show(_textControl, new Point(clientPos.X + 10, clientPos.Y + 10));
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.TraceFieldSource");
		}
	}

	/// <summary>
	/// 检查表格前面是否有 DocumentTarget 书签（原始软件的引用表格标记方式）
	/// 遍历文档中所有 DocumentTarget，检查其位置是否在当前表格的第一个单元格前一位置
	/// </summary>
	private TXTextControl.Table GetDirectlyPrecedingTable(TXTextControl.Table table)
	{
		try
		{
			int textPosition = _tx.InputPosition.TextPosition;
			int start = table.Cells.GetItem(1, 1).Start;
			if (start < 2) return null;
			TXTextControl.InputPosition inputPosition = new TXTextControl.InputPosition(start - 2);
			_tx.InputPosition = inputPosition;
			TXTextControl.Table item = _tx.Tables.GetItem();
			_tx.Select(textPosition, 0);
			return item;
		}
		catch (Exception ex) { ex.Log("DocumentEditor.GetDirectlyPrecedingTable"); }
		return null;
	}

	private bool IsReferencedTable(TXTextControl.Table table)
	{
		if (_isCheckingTableRef) return false;
		try
		{
			_isCheckingTableRef = true;
			var firstCell = table.Cells.GetItem(1, 1);
			if (firstCell == null) return false;
			int targetPos = firstCell.Start - 1;

			// 方法1：Select + GetItem 方式查找表格前面的 DocumentTarget 书签
			_tx.Select(targetPos, 0);
			DocumentTarget dt = _tx.DocumentTargets.GetItem();

			// 恢复光标到表格中
			_tx.Select(firstCell.Start, 0);

			if (dt != null && !string.IsNullOrEmpty(dt.TargetName) && LeqiBookmark.TryParse(dt.TargetName, out _))
				return true;

			// 方法2：枚举所有 DocumentTarget，查找位置在表格前面的 LeqiBookmark
			var allTargets = _textControl.DocumentTargets;
			if (allTargets != null)
			{
				int tableStart = firstCell.Start;
				foreach (DocumentTarget target in allTargets)
				{
					try
					{
						if (string.IsNullOrEmpty(target.TargetName)) continue;
						if (!LeqiBookmark.TryParse(target.TargetName, out _)) continue;
						int dtStart = target.Start;
						int distance = tableStart - dtStart;
						if (distance > 0 && distance < 100)
							return true;
					}
					catch { }
				}
			}

			// 方法3：检查 ApplicationFields 是否在单元格内
			var fields = _textControl.ApplicationFields;
			if (fields != null && fields.Count > 0)
			{
				foreach (TXTextControl.TableCell cell in table.Cells)
				{
					try
					{
						int cellStart = cell.Start;
						int cellEnd = cellStart + cell.Length;
						foreach (TXTextControl.ApplicationField f in fields)
						{
							if (f.TypeName != "MERGEFIELD") continue;
							if (f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
							int fieldEnd = f.Start + f.Length;
							if (f.Start >= cellStart && fieldEnd <= cellEnd)
								return true;
						}
					}
					catch { }
				}
			}
		}
		catch { }
		finally { _isCheckingTableRef = false; }
		return false;
	}

	private void _textControl_TextChanged(object sender, EventArgs e)
	{
		// 文本变化时更新撤销/恢复按钮状态
		Program.MainForm.UpdateUndoRedoButtonState();
	}

	private void _textControl_InputPositionChanged(object sender, EventArgs e)
	{
		try
		{
			if (_textControl == null || !_isDocumentLoaded) return;
			if (_isCheckingTableRef) return; // 防止 GetRefTable/IsReferencedTable 中 Select 导致的递归
			if (_isRefreshingTable) return;  // 防止刷新表格时大量 Select 导致重复弹出

			// 更新撤销/恢复按钮状态
			Program.MainForm.UpdateUndoRedoButtonState();

			// 域提示框正在显示时，不覆盖为表格操作提示框
			if (_isShowingFieldTooltip)
			{
				_isShowingFieldTooltip = false;
				return;
			}

			TXTextControl.Table table = _textControl.Tables.GetItem();
			if (table != null)
			{
				// 用表格起始位置比较，避免每次 GetItem 返回新对象导致重复触发
				int tableId = table.Cells.GetItem(1, 1)?.Start ?? 0;
				if (tableId != _lastTableId)
				{
					_lastTableId = tableId;
					ShowTableContextMenu(table);
				}
			}
			else
			{
				_lastTableId = 0;
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor._textControl_InputPositionChanged");
		}
	}

	private void ShowTableContextMenu(TXTextControl.Table table)
	{
		var xBody = new System.Xml.Linq.XElement("div");
		var linkDic = new Dictionary<string, object>();

		xBody.Add(new System.Xml.Linq.XElement("b", "表格操作"));
		xBody.Add(new System.Xml.Linq.XElement("hr"));

		var tableElement = new System.Xml.Linq.XElement("table");
		var row1 = new System.Xml.Linq.XElement("tr");
		var row2 = new System.Xml.Linq.XElement("tr");

		linkDic.Add("refreshtable", table);
		row1.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "refreshtable"), "刷新表格")));

		linkDic.Add("deletetable", table);
		row1.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "deletetable"), "删除表格")));

		linkDic.Add("tracktable", table);
		row2.Add(new System.Xml.Linq.XElement("td", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", "tracktable"), "追踪表格")));

		tableElement.Add(row1);
		tableElement.Add(row2);
		xBody.Add(tableElement);

		_ttpComment.SetText("表格操作", xBody.ToString(), canClose: true);
		_ttpComment.SetTagDic(linkDic);
		_ttpComment.LinkClicked += _ttpComment_TableLinkClicked;

		_textControl.InputPositionChanged += OnTableContextMenuInputPositionChanged;

		var mousePos = Control.MousePosition;
		var clientPos = _textControl.PointToClient(mousePos);
		_ttpComment.Show(_textControl, new Point(clientPos.X + 10, clientPos.Y + 10));
	}

	private void OnTableContextMenuInputPositionChanged(object sender, EventArgs e)
	{
		try
		{
			var table = _textControl.Tables.GetItem();
			if (table == null)
			{
				HideTableContextMenu();
			}
		}
		catch
		{
			HideTableContextMenu();
		}
	}

	private void HideTableContextMenu()
	{
		_textControl.InputPositionChanged -= OnTableContextMenuInputPositionChanged;
		_ttpComment.LinkClicked -= _ttpComment_TableLinkClicked;
		_ttpComment.Hide();
	}

	private void _ttpComment_TableLinkClicked(object sender, object e)
	{
		try
		{
			var href = sender as string;
			if (string.IsNullOrEmpty(href))
			{
				HideTableContextMenu();
				return;
			}

			if (href.StartsWith("tracetable_"))
			{
				HideTableContextMenu();
				return;
			}

			if (!(e is TXTextControl.Table table))
			{
				HideTableContextMenu();
				return;
			}

			switch (href)
			{
				case "refreshtable":
					HideTableContextMenu();
					RefreshTable(table);
					break;
				case "deletetable":
					DeleteTable(table);
					break;
				case "tracktable":
					TrackTable(table);
					break;
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor._ttpComment_TableLinkClicked");
			HideTableContextMenu();
		}
	}

	/// <summary>
	/// 追踪表格提示框的链接点击处理
	/// </summary>
	private void _ttpComment_TrackTableLinkClicked(object sender, object e)
	{
		try
		{
			var href = sender as string;
			if (string.IsNullOrEmpty(href))
			{
				HideTooltip();
				return;
			}

			if (href.StartsWith("tracetable_"))
			{
				HideTooltip();
				HandleTraceNavigation(href, e);
				return;
			}

			HideTooltip();
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor._ttpComment_TrackTableLinkClicked");
			HideTooltip();
		}
	}

	private void RefreshTable(TXTextControl.Table table)
	{
		_isRefreshingTable = true;
		try
		{
			if (table == null)
			{
				return;
			}

			// 预先关闭表格操作提示框，避免在操作过程中持续弹出干扰
			HideTableContextMenu();

			// 通过书签获取来源表格，从源数据刷新（原地调整行列，无需删除重建）
			bool foundRef = GetRefTable(table, out var bm, out var refTable);

			if (foundRef && refTable != null)
			{
				RefreshModelTableWithFormatImpl(table);
				return;
			}

			RefreshTableFormulas(table);
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.RefreshTable");
		}
		finally
		{
			_isRefreshingTable = false;
		}
	}

	/// <summary>
	/// 获取引用表格的来源表格 ID
	/// 优先从 DocumentTarget 书签获取，失败时回退到 Document.MergeTable
	/// </summary>
	private Leqisoft.DTO.Id64? GetSourceTableId(TXTextControl.Table table = null)
	{
		try
		{
			if (table == null)
				table = _textControl.Tables.GetItem();
			if (table != null)
			{
				var firstCell = table.Cells.GetItem(1, 1);
				if (firstCell != null)
				{
					int targetPos = firstCell.Start - 1;

					// 使用守卫标志防止 Select 触发 InputPositionChanged 递归
					_isCheckingTableRef = true;
					try
					{
						// 方法1a：通过 Select + GetItem 获取 DocumentTarget 书签
					_tx.Select(targetPos, 0);
					DocumentTarget dt = _tx.DocumentTargets.GetItem();
					if (dt != null && !string.IsNullOrEmpty(dt.TargetName) && LeqiBookmark.TryParse(dt.TargetName, out var lsbm))
					{
						// 有 TableId，直接返回
						if (lsbm.TableId != null)
							{
								// 缓存 ParaId→TableId 映射
								if (lsbm.ParaIdBase64 != null)
									_bookmarkTableIdCache[lsbm.ParaIdBase64] = lsbm.TableId;
								return Leqisoft.DTO.Id64.ParseBase64(lsbm.TableId);
							}
							// 没有 TableId，先从缓存恢复
							if (lsbm.ParaIdBase64 != null)
							{
								if (_bookmarkTableIdCache.TryGetValue(lsbm.ParaIdBase64, out var cachedTableId))
								{
									return Leqisoft.DTO.Id64.ParseBase64(cachedTableId);
								}
								var refTable = FindTableByParaId(lsbm.ParaIdBase64);
								if (refTable != null)
								{
									return refTable.Id;
								}
							}
						}

						// 方法1b：枚举所有 DocumentTarget，查找位置在表格前面最近的 LeqiBookmark
						var allTargets = _textControl.DocumentTargets;
						if (allTargets != null)
						{
							int tableStart = firstCell.Start;
							LeqiBookmark bestBookmark = null;
							int bestDistance = int.MaxValue;

							foreach (DocumentTarget target in allTargets)
							{
								try
								{
									if (string.IsNullOrEmpty(target.TargetName)) continue;
									if (!LeqiBookmark.TryParse(target.TargetName, out var bm)) continue;
									if (bm.TableId == null && bm.ParaIdBase64 == null) continue;

									int dtStart = target.Start;
									int distance = tableStart - dtStart;
									if (distance > 0 && distance < 500 && distance < bestDistance)
									{
										bestDistance = distance;
										bestBookmark = bm;
									}
								}
								catch { }
							}

							if (bestBookmark != null)
							{
								if (bestBookmark.TableId != null)
								{
									// 缓存 ParaId→TableId 映射
									if (bestBookmark.ParaIdBase64 != null)
									_bookmarkTableIdCache[bestBookmark.ParaIdBase64] = bestBookmark.TableId;
								return Leqisoft.DTO.Id64.ParseBase64(bestBookmark.TableId);
							}
							if (bestBookmark.ParaIdBase64 != null)
							{
								// 先检查缓存
								if (_bookmarkTableIdCache.TryGetValue(bestBookmark.ParaIdBase64, out var cachedTableId))
								{
									return Leqisoft.DTO.Id64.ParseBase64(cachedTableId);
								}
								var refTable = FindTableByParaId(bestBookmark.ParaIdBase64);
								if (refTable != null)
								{
									return refTable.Id;
								}
							}
							}
						}
					}
					finally
					{
						try { _tx.Select(firstCell.Start, 0); } catch { }
						_isCheckingTableRef = false;
					}
				}

				// 方法2：通过表格内公式解析引用的表格ID
				var fields = _textControl.ApplicationFields;
				if (fields != null && fields.Count > 0)
				{
					// 先列出所有 fields 的类型和位置
					int tableStart = table.Cells.GetItem(1, 1)?.Start ?? 0;
					int tableEnd = tableStart;
					try
					{
						var lastCell = table.Cells.GetItem(table.Rows.Count, table.Columns.Count);
						if (lastCell != null) tableEnd = lastCell.Start + lastCell.Length;
					} catch { }

					foreach (TXTextControl.TableCell cell in table.Cells)
					{
						try
						{
							int cellStart = cell.Start;
							int cellEnd = cellStart + cell.Length;
							foreach (TXTextControl.ApplicationField f in fields)
							{
								if (f.TypeName != "MERGEFIELD") continue;
								if (f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
								int fieldEnd = f.Start + f.Length;
								if (f.Start >= cellStart && fieldEnd <= cellEnd)
								{
								string formula = f.Parameters[1];
								if (!string.IsNullOrWhiteSpace(formula))
								{
									var evaluator = new FormulaEvaluator(formula);
									var ids = evaluator.GetReferredTableIds();
									if (ids.Count > 0)
									{
										return ids.First();
									}
								}
								}
							}
						}
						catch { }
					}
				}

				// 方法4：回退到 Document.MergeTable（原始软件数据库层方式）
				try
				{
					var doc = Document as Leqisoft.Model.Document;
					if (doc != null && doc.MergeTable != null && !doc.MergeTable.IsZero())
					{
						var project = Program.MainForm.CurrentProject;
						if (project != null)
						{
							var mergeTable = project.GetTableById(new Leqisoft.DTO.Id64(doc.MergeTable.Value));
							if (mergeTable != null)
							{
								return new Leqisoft.DTO.Id64(doc.MergeTable.Value);
							}
						}
					}
				}
				catch (Exception)
				{
				}

			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	private void DeleteTable(TXTextControl.Table table)
	{
		try
		{
			if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要删除此表格吗？") == DialogResult.Yes)
			{
				HideTableContextMenu();
				DetachEvents();
				try
				{
					// 使用TX Text Control的Tables.Remove()真正删除整个表格结构
					table.Select();
					_textControl.Tables.Remove();
					_lastTableId = 0;
				}
				finally
				{
					AttachEvents();
				}
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.DeleteTable");
		}
	}

	private void TrackTable(TXTextControl.Table table)
	{
		try
		{
			var project = Leqisoft.Model.Project.Current;
			if (project == null)
			{
				return;
			}

			int linkIndex = 0;
			var tagDic = new Dictionary<string, object>();
			var xBody = new System.Xml.Linq.XElement("div");
			xBody.Add(new System.Xml.Linq.XElement("b", "数据来源"));

			bool hasSource = false;

			// 方法1：通过书签或 Document.MergeTable 获取来源表格
			var sourceTableId = GetSourceTableId(table);
			if (sourceTableId.HasValue)
			{
				var tableNode = project.GetNodeById(sourceTableId.Value) as TreeTableNode;
				if (tableNode != null)
				{
					string name = tableNode.Table?.GetCanonicalName() ?? $"表格ID={sourceTableId.Value}";
					string href = $"tracetable_{linkIndex++}";
					tagDic[href] = tableNode;
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📊 " + name)));
					hasSource = true;
				}
				else
				{
					string href = $"tracetable_{linkIndex++}";
					xBody.Add(new System.Xml.Linq.XElement("p", $"📊 来源表格ID={sourceTableId.Value}（节点不存在）"));
					hasSource = true;
				}
			}

			// 方法2：通过表格内公式解析引用的表格ID
			var fields = _textControl.ApplicationFields;
			if (fields != null && fields.Count > 0)
			{
				var formulas = new List<string>();
				foreach (TXTextControl.TableCell cell in table.Cells)
				{
					try
					{
						int cellStart = cell.Start;
						int cellEnd = cellStart + cell.Length;
						foreach (TXTextControl.ApplicationField f in fields)
						{
							if (f.TypeName != "MERGEFIELD") continue;
							if (f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
							int fieldEnd = f.Start + f.Length;
							if (f.Start >= cellStart && fieldEnd <= cellEnd)
								formulas.Add(f.Parameters[1]);
						}
					}
					catch { }
				}

				var referredTableIds = new HashSet<Leqisoft.DTO.Id64>();
				foreach (var formula in formulas)
				{
					if (string.IsNullOrWhiteSpace(formula)) continue;
					var evaluator = new FormulaEvaluator(formula);
					var ids = evaluator.GetReferredTableIds();
					foreach (var id in ids)
						referredTableIds.Add(id);
				}

				if (referredTableIds.Count > 0)
				{
					if (hasSource)
						xBody.Add(new System.Xml.Linq.XElement("hr"));
					foreach (var tid in referredTableIds)
					{
						var tableNode = project.GetNodeById(tid) as TreeTableNode;
						string name = tableNode?.Table?.GetCanonicalName() ?? $"表格ID={tid}";
						string href = $"tracetable_{linkIndex++}";
						tagDic[href] = tableNode;
						xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📊 " + name)));
						hasSource = true;
					}
				}
			}

			if (!hasSource) return;

			_ttpComment.SetText("追踪表格", xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(tagDic);

			// 绑定链接点击事件和鼠标事件
			HideTableContextMenu();
			_ttpComment.LinkClicked += _ttpComment_TrackTableLinkClicked;
			_textControl.MouseDown += OnTextControlMouseDown;

			var mousePos = Control.MousePosition;
			var clientPos = _textControl.PointToClient(mousePos);
			_ttpComment.Show(_textControl, new Point(clientPos.X + 10, clientPos.Y + 10));
		}
		catch (Exception ex)
		{
			ex.Log("DocumentEditor.TrackTable");
		}
	}

	private void SetCellAlignment(TXTextControl.VerticalAlignment vAlign, TXTextControl.HorizontalAlignment hAlign)
	{
		var table = _textControl.Tables.GetItem();
		if (table == null) return;
		foreach (TXTextControl.TableCell cell in table.Cells)
		{
			try
			{
				cell.CellFormat.VerticalAlignment = vAlign;
				_textControl.Select(cell.Start, cell.Length);
				_textControl.Selection.ParagraphFormat.Alignment = hAlign;
			}
			catch { }
		}
	}

	private void SetMargin(string attr, int valueTwips)
	{
		var margins = _textControl.Selection.SectionFormat.PageMargins;
		switch (attr)
		{
			case "Top": margins.Top = valueTwips; break;
			case "Bottom": margins.Bottom = valueTwips; break;
			case "Left": margins.Left = valueTwips; break;
			case "Right": margins.Right = valueTwips; break;
		}
		_textControl.Selection.SectionFormat.PageMargins = margins;
	}

	private static void PaperKindToSize(System.Drawing.Printing.PaperKind kind, out int width, out int height)
	{
		switch (kind)
		{
			case System.Drawing.Printing.PaperKind.A3: width = 16838; height = 23811; break;
			case System.Drawing.Printing.PaperKind.A4: width = 11906; height = 16838; break;
			case System.Drawing.Printing.PaperKind.B4: width = 13040; height = 18520; break;
			case System.Drawing.Printing.PaperKind.B5: width = 10118; height = 14331; break;
			case System.Drawing.Printing.PaperKind.Letter: width = 12240; height = 15840; break;
			default: width = 11906; height = 16838; break;
		}
	}

	private void RefreshTableFormulas(TXTextControl.Table table)
	{
		var project = Project.Current;
		if (project == null) return;

		foreach (TXTextControl.TableCell cell in table.Cells)
		{
			try
			{
				int cellStart = cell.Start;
				int cellEnd = cellStart + cell.Length;
				foreach (TXTextControl.ApplicationField f in _textControl.ApplicationFields)
				{
					if (f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 2 || f.Parameters[0] != "Formula") continue;
					int fieldEnd = f.Start + f.Length;
					if (f.Start >= cellStart && fieldEnd <= cellEnd)
					{
						string formula = f.Parameters[1];
						if (!string.IsNullOrWhiteSpace(formula))
						{
							var evaluator = new FormulaEvaluator(formula);
							var result = evaluator.Evaluate();
							if (result != null)
							{
								f.Text = result.ToString();
							}
						}
					}
				}
			}
			catch { }
		}
	}

	public bool IsDocumentLocked()
	{
		try
		{
			if (Document?.Locker == 0L)
			{
				return !Document?.TreeNode?.HasWritePermission() ?? true;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public TXTextControl.Table GetCurrentTable()
	{
		return _textControl?.Tables?.GetItem();
	}

	internal TXTextControl.ApplicationField GetCurrentApplicationField()
	{
		try
		{
			return _textControl?.ApplicationFields?.GetItem();
		}
		catch { return null; }
	}

	private TXTextControl.ApplicationField GetParaStartApplicationField()
	{
		try
		{
			var field = _textControl?.ApplicationFields?.GetItem();
			if (field != null && _textControl.Selection.Start == field.Start)
				return field;
			return null;
		}
		catch { return null; }
	}

	void PreserveSelection(Action<TXTextControl.Selection> action)
	{
		Point scrollLocation = _textControl.ScrollLocation;
		TXTextControl.Selection selection = _textControl.Selection;
		int start = selection.Start;
		int length = selection.Length;
		action(selection);
		if (length == 0)
		{
			_textControl.Select(start, length);
		}
		else
		{
			_textControl.Select(start, 0);
			TXTextControl.Table currentTable = GetCurrentTable();
			_textControl.Select(start + length - 1, 0);
			TXTextControl.Table currentTable2 = GetCurrentTable();
			if (currentTable == null || currentTable2 == null)
			{
				_textControl.Select(start, length);
			}
			else if (currentTable.NestedLevel != currentTable2.NestedLevel)
			{
				_textControl.Select(start, length);
			}
			else if (currentTable.Cells.GetItem(1, 1).Start == currentTable2.Cells.GetItem(1, 1).Start)
			{
				_textControl.Select(start, 0);
				TXTextControl.TableCell item = currentTable.Cells.GetItem();
				_textControl.Select(start + length - 1, 0);
				TXTextControl.TableCell item2 = currentTable.Cells.GetItem();
				if (item.Row == item2.Row && item.Column == item2.Column)
				{
					if (item.Column == currentTable.Columns.Count && item.Length == length - 1)
					{
						item.Select();
					}
					else
					{
						_textControl.Select(start, length);
					}
				}
				else
				{
					currentTable.Select(item.Row, item.Column, item2.Row, item2.Column);
				}
			}
			else
			{
				_textControl.Select(start, length);
			}
		}
		_textControl.ScrollLocation = scrollLocation;
	}

	MainText GetMainText() => _textControl.TextParts.GetMainText();

	bool IsBookmarkAtStart(DocumentTarget dt)
	{
		_textControl.Select(dt.Start - 1, 0);
		var table = GetCurrentTable();
		if (table != null)
		{
			return dt.Start == table.Cells.GetItem(1, 1).Start;
		}
		return dt.Start == _textControl.Paragraphs.GetItem(dt.Start - 1).Start;
	}

	void TagCurrentPOrTableModified(int position)
	{
		PreserveSelection(delegate
		{
			_textControl.Select(position, 0);
			var table = GetCurrentTable();

			int modifiedPos;
			if (table != null)
			{
				modifiedPos = table.Cells.GetItem(1, 1).Start - 1;
			}
			else
			{
				modifiedPos = _textControl.Paragraphs.GetItem(_textControl.InputPosition.TextPosition).Start - 1;
			}

			_textControl.Select(modifiedPos, 0);
			var dt = _textControl.DocumentTargets.GetItem();

			if (dt != null && LeqiBookmark.TryParse(dt.TargetName, out var bookmark))
			{
				bookmark.TagModifiedIfNotNew();
				if (bookmark.GetString() != dt.TargetName)
				{
					dt.TargetName = bookmark.GetString();
				}
			}
		});
	}

	private void OnFormatChanged()
	{
		NeedSave = true;
	}

	private void SetTableBorder(TXTextControl.Table txTable, Leqisoft.Model.TableBorderStyle tableStyle, int captionLayers = 1)
	{
		if (IsDocumentLocked() || txTable == null || tableStyle == null)
		{
			return;
		}

		// 如果是自定义样式，委托给 SetCustomTableBorder
		if (tableStyle.IsCustomStyle)
		{
			SetCustomTableBorder(txTable, tableStyle, captionLayers);
			return;
		}

		PreserveSelection(delegate
		{
			_textControl.SuspendDrawing();
			try
			{
				int thinWidth = 10;
				int thickWidth = 30;
				foreach (TXTextControl.TableCell cell in txTable.Cells)
				{
					try
					{
						var fmt = cell.CellFormat;
						int row = cell.Row;
						int col = cell.Column;
						bool isTopRow = row <= captionLayers;
						bool isBottomRow = row == txTable.Rows.Count;
						bool isLeftCol = col == 1;
						bool isRightCol = col == txTable.Columns.Count;

						// Up/Down border (top and bottom of table)
						int upDownWidth = tableStyle.UpDownLine == Leqisoft.Model.LineStyle.Thick ? thickWidth
							: (tableStyle.UpDownLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
						// Left/Right border
						int leftRightWidth = tableStyle.LeftRightLine == Leqisoft.Model.LineStyle.Thick ? thickWidth
							: (tableStyle.LeftRightLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
						// Body line
						int bodyWidth = tableStyle.BodyLine == Leqisoft.Model.LineStyle.Thick ? thickWidth
							: (tableStyle.BodyLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);
						// Second line (between caption and body)
						int secondWidth = tableStyle.SecondLine == Leqisoft.Model.LineStyle.Thick ? thickWidth
							: (tableStyle.SecondLine == Leqisoft.Model.LineStyle.Thin ? thinWidth : 0);

						fmt.TopBorderWidth = isTopRow ? upDownWidth : secondWidth;
						fmt.BottomBorderWidth = isBottomRow ? upDownWidth : bodyWidth;
						fmt.LeftBorderWidth = isLeftCol ? leftRightWidth : bodyWidth;
						fmt.RightBorderWidth = isRightCol ? leftRightWidth : bodyWidth;
						cell.CellFormat = fmt;
					}
					catch { }
				}
			}
			finally
			{
				_textControl.ResumeDrawing();
			}
		});
	}

	/// <summary>
	/// 应用自定义细粒度表格边框样式（按 10 个独立位置分别设置边框）
	/// </summary>
	/// <param name="txTable">TX Text Control 表格</param>
	/// <param name="style">自定义边框样式（IsCustomStyle 必须为 true）</param>
	/// <param name="captionLayers">表头行数</param>
	private void SetCustomTableBorder(TXTextControl.Table txTable, Leqisoft.Model.TableBorderStyle style, int captionLayers = 1)
	{
		if (IsDocumentLocked() || txTable == null || style == null || !style.IsCustomStyle)
			return;

		PreserveSelection(delegate
		{
			_textControl.SuspendDrawing();
			try
			{
				foreach (TXTextControl.TableCell cell in txTable.Cells)
				{
					try
					{
						var fmt = cell.CellFormat;
						int row = cell.Row;
						int col = cell.Column;
						bool isTopRow = row <= captionLayers;
						bool isBottomRow = row == txTable.Rows.Count;
						bool isLeftCol = col == 1;
						bool isRightCol = col == txTable.Columns.Count;
						bool isHeaderBottomRow = row == captionLayers; // 表头最后一行

						// 顶部边框
						if (isTopRow)
							fmt.TopBorderWidth = style.HeaderTop?.ToTwips() ?? 0;
						else if (row == captionLayers + 1)
							fmt.TopBorderWidth = style.HeaderBottom?.ToTwips() ?? 0; // 表头下部分隔线
						else
							fmt.TopBorderWidth = style.InnerHorizontal?.ToTwips() ?? 0;

						// 底部边框
						if (isBottomRow)
							fmt.BottomBorderWidth = style.TableBottom?.ToTwips() ?? 0;
						else if (isHeaderBottomRow)
							fmt.BottomBorderWidth = style.HeaderBottom?.ToTwips() ?? 0; // 表头下部分隔线
						else
							fmt.BottomBorderWidth = style.InnerHorizontal?.ToTwips() ?? 0;

						// 左侧边框
						if (isLeftCol)
						{
							if (isTopRow)
								fmt.LeftBorderWidth = style.HeaderLeft?.ToTwips() ?? 0;
							else
								fmt.LeftBorderWidth = style.TableLeft?.ToTwips() ?? 0;
						}
						else
							fmt.LeftBorderWidth = style.InnerVertical?.ToTwips() ?? 0;

						// 右侧边框
						if (isRightCol)
						{
							if (isTopRow)
								fmt.RightBorderWidth = style.HeaderRight?.ToTwips() ?? 0;
							else
								fmt.RightBorderWidth = style.TableRight?.ToTwips() ?? 0;
						}
						else
							fmt.RightBorderWidth = style.InnerVertical?.ToTwips() ?? 0;

						cell.CellFormat = fmt;
					}
					catch { }
				}

				// 关键词行加粗下划线
			if (style.KeywordRowBoldUnderline)
			{
				ApplyKeywordRowBoldUnderline(txTable, captionLayers, style.KeywordList);
			}
			}
			finally
			{
				_textControl.ResumeDrawing();
			}
		});
	}

	/// <summary>
	/// 对表格中的关键词行应用加粗+下划线格式
	/// </summary>
	/// <param name="txTable">TX 表格</param>
	/// <param name="captionLayers">表头行数</param>
	/// <param name="keywordList">自定义关键词列表（逗号分隔），为空时使用默认关键词</param>
	private void ApplyKeywordRowBoldUnderline(TXTextControl.Table txTable, int captionLayers, string keywordList = null)
	{
		if (txTable == null) return;

		// 解析关键词列表
		string[] keywords;
		if (string.IsNullOrEmpty(keywordList))
		{
			keywords = new[] { "合计", "小计", "总计", "关键词" };
		}
		else
		{
			keywords = keywordList.Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < keywords.Length; i++)
				keywords[i] = keywords[i].Trim();
		}

		for (int r = captionLayers + 1; r <= txTable.Rows.Count; r++)
		{
			try
			{
				var row = txTable.Rows[r];
				if (row == null) continue;

				// 检查行是否为关键词行（通过第一列文本匹配自定义关键词列表）
				bool isKeywordRow = false;
				try
				{
					var firstCell = txTable.Cells.GetItem(r, 1);
					if (firstCell != null && !string.IsNullOrEmpty(firstCell.Text))
					{
						string text = firstCell.Text.Trim();
						foreach (var kw in keywords)
						{
							if (!string.IsNullOrEmpty(kw) && text.Contains(kw))
							{
								isKeywordRow = true;
								break;
							}
						}
					}
				}
				catch { }

				if (!isKeywordRow) continue;

				// 遍历该行所有单元格，设置加粗+下划线
				for (int c = 1; c <= txTable.Columns.Count; c++)
				{
					try
					{
						var cell = txTable.Cells.GetItem(r, c);
						if (cell == null) continue;
						cell.Select();
						_textControl.Selection.Bold = true;
						_textControl.Selection.Underline = TXTextControl.FontUnderlineStyle.Single;
					}
					catch { }
			}
		}
		catch { }
		}
	}

	/// <summary>
	/// 批量应用表格样式到多个表格
	/// </summary>
	/// <param name="tablesInRange">范围内的表格书签列表 (Position, LeqiBookmark)</param>
	/// <param name="style">要应用的边框样式</param>
	/// <param name="progressCallback">进度回调 (current, total)，可为 null</param>
	/// <returns>成功应用的表格数量</returns>
	public int BatchApplyTableStyle(List<(int Position, LeqiBookmark Bookmark)> tablesInRange, Leqisoft.Model.TableBorderStyle style, Action<int, int> progressCallback = null)
	{
		if (tablesInRange == null || tablesInRange.Count == 0 || style == null)
			return 0;

		int successCount = 0;
		int total = tablesInRange.Count;

		_textControl.BeginUndoAction("批量应用表格样式");
		try
		{
			_textControl.SuspendDrawing();
			try
			{
				for (int i = 0; i < tablesInRange.Count; i++)
				{
					var item = tablesInRange[i];
					try
					{
						// 通过书签位置定位表格
						_textControl.Select(item.Position, 0);
						var txTable = _textControl.Tables.GetItem();
						if (txTable == null)
						{
							// 尝试在附近位置查找表格
							// 书签位于表格第一个单元格前一位置，所以需要 +1
							_textControl.Select(item.Position + 1, 0);
							txTable = _textControl.Tables.GetItem();
						}

						if (txTable == null)
						{
							progressCallback?.Invoke(i + 1, total);
							continue;
						}

						// 表头行数：批量场景下无法获取 Model.Table，使用默认值 1
						int captionLayers = 1;

						// 应用自定义边框样式
						SetCustomTableBorder(txTable, style, captionLayers);

						// 更新书签: 设置 TableStyle=6 (自定义) 并写入 CustomBorderStyleJson
						var updatedBookmark = new LeqiBookmark
						{
							ParaIdBase64 = item.Bookmark.ParaIdBase64,
							Status = item.Bookmark.Status,
							TableId = item.Bookmark.TableId,
							VariableId = item.Bookmark.VariableId,
							TableStyle = Leqisoft.Model.TableBorderStyles.Custom, // 6
							CustomBorderStyleJson = style.ToJson()
						};
						updatedBookmark.TagModifiedIfNotNew();
						ModifyBookmark(txTable, updatedBookmark);

						successCount++;
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"批量应用样式失败 (位置 {item.Position}): {ex.Message}");
					}

					// 报告进度
					progressCallback?.Invoke(i + 1, total);
				}
			}
			finally
			{
				_textControl.ResumeDrawing();
			}
		}
		finally
		{
			_textControl.EndUndoAction();
		}
		OnFormatChanged();

		return successCount;
	}
}
