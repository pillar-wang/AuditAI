using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class SnapshotManager
{
	private class TableFormulaRecycleProcesser
	{
		public Dictionary<Id64, Table> _tableDic = new Dictionary<Id64, Table>();

		protected TreeTableNode _treeTableNode;

		protected Dictionary<Id64, Column> _oldColumnDic;

		protected Dictionary<Id64, Cell> _oldCellDic;

		protected Dictionary<Id64, Column> _newColumnDic;

		protected Dictionary<Id64, Cell> _newCellDic;

		protected List<ValidationFormula> _validationFormulaList;

		protected Id64 _oldTableId;

		public TableFormulaRecycleProcesser(Id64 oldTableId, TreeTableNode treeNode, Dictionary<Id64, Column> oldColumnDic, Dictionary<Id64, Cell> oldCellDic, Dictionary<Id64, Column> newColumnDic, Dictionary<Id64, Cell> newCellDic, List<ValidationFormula> validationFormulaList)
		{
			_oldTableId = oldTableId;
			_treeTableNode = treeNode;
			_oldColumnDic = oldColumnDic;
			_oldCellDic = oldCellDic;
			_newColumnDic = newColumnDic;
			_newCellDic = newCellDic;
			_validationFormulaList = validationFormulaList;
		}

		public void ReverFormula()
		{
			FormulaReferenceModelResolverForRecycleFile resolver = new FormulaReferenceModelResolverForRecycleFile(Project.Current, _oldTableId, _treeTableNode, _oldCellDic, _oldColumnDic, _newCellDic, _newColumnDic);
			_treeTableNode.DuplicateFormulasForRecycleFile(_oldTableId, _treeTableNode.Table, _tableDic, resolver, _validationFormulaList);
		}
	}

	private Project _owner;

	private ProjectDAL _dal => _owner.Dal;

	public SnapshotManager(Project owner)
	{
		_owner = owner;
	}

	public void SaveSnapshot(Table table, bool isDeleting)
	{
		int num = _dal.GetLastSnapshotId() + 1;
		string text = Path.Combine(GetDirectory().FullName, $"{num}.db");
		File.Delete(text);
		TableDAL tableDAL = new TableDAL(text);
		Auditai.DTO.Table dto = table.ToDto();
		tableDAL.SaveTable(dto);
		tableDAL.SaveColumns(table.Columns.Select((Column c) => c.ToDto()));
		tableDAL.SaveRows(table.Rows.Select((Row r) => r.ToDto()));
		tableDAL.SaveCells(table.Cells.Select((Cell c) => c.ToDto()));
		tableDAL.SaveCellStyles(table.CellStyles.Select((CellStyle cs) => cs.ToDto()));
		tableDAL.SaveValidationFormulas((from f in Project.Current.ValidationManager.Formulas
			where f.TableId == table.Id
			select f into u
			select u.ToDto()).ToList());
		tableDAL.SaveCellProps(table.CellPropManager.DicCellAttachments.Select((KeyValuePair<Id64, CellAttachments> kv) => new CellProp
		{
			TableId = table.Id,
			CellId = kv.Key,
			Dirty = (kv.Value.Dirty ? 1 : 0),
			Status = (int)kv.Value.Status,
			Attachments = kv.Value.Serialize()
		}).ToList());
		tableDAL.SaveMerges(table.MergedCells.Select((CellMerge m) => new Merge
		{
			Id = m.Id.Value,
			TableId = table.Id.Value,
			TopLeft = m.TopLeft.Id.Value,
			BottomRight = m.BottomRight.Id.Value,
			Status = (int)m.Status
		}));
		if (isDeleting)
		{
			_dal.DeleteSnapshots(table.TreeNode.Id);
		}
		_dal.SaveSnapshot(new SnapshotInfo
		{
			Id = num,
			DateTime = DateTime.Now,
			Kind = 0,
			Name = table.TreeNode.Name,
			Size = (int)new FileInfo(text).Length,
			TreeNodeId = table.TreeNode.Id,
			Deleted = isDeleting
		});
	}

	public void SaveSnapshot(Document document, bool isDeleting)
	{
		int num = _dal.GetLastSnapshotId() + 1;
		string text = Path.Combine(GetDirectory().FullName, $"{num}.db");
		File.Delete(text);
		DocumentDAL documentDAL = new DocumentDAL(text);
		Auditai.DTO.Document dto = document.ToDto();
		documentDAL.SaveDocument(dto);
		documentDAL.SaveParagraphs(document.Paragraphs.Select((Paragraph p) => p.ToDto()));
		if (isDeleting)
		{
			_dal.DeleteSnapshots(document.TreeNode.Id);
		}
		_dal.SaveSnapshot(new SnapshotInfo
		{
			Id = num,
			DateTime = DateTime.Now,
			Kind = 1,
			Name = document.TreeNode.Name,
			Size = (int)new FileInfo(text).Length,
			TreeNodeId = document.TreeNode.Id,
			Deleted = isDeleting
		});
	}

	public void SaveSnapshot(Image image, bool isDeleting)
	{
		FileCacheManager fileCacheManager = new FileCacheManager(image.Project);
		int num = _dal.GetLastSnapshotId() + 1;
		string text = Path.Combine(GetDirectory().FullName, $"{num}.db");
		File.Delete(text);
		ImageDAL imageDAL = new ImageDAL(text);
		Auditai.DTO.Image dto = image.ToDto();
		imageDAL.SaveImage(dto);
		if (isDeleting)
		{
			_dal.DeleteSnapshots(image.TreeNode.Id);
		}
		_dal.SaveSnapshot(new SnapshotInfo
		{
			Id = num,
			DateTime = DateTime.Now,
			Kind = 2,
			Name = image.TreeNode.Name,
			Size = (int)(fileCacheManager.Exists(image.FileId) ? fileCacheManager.GetFileSize(image.FileId) : new FileInfo(text).Length),
			TreeNodeId = image.TreeNode.Id,
			Deleted = isDeleting
		});
	}

	public void SaveSnapshot(Pdf pdf, bool isDeleting)
	{
		FileCacheManager fileCacheManager = new FileCacheManager(pdf.Project);
		int num = _dal.GetLastSnapshotId() + 1;
		string text = Path.Combine(GetDirectory().FullName, $"{num}.db");
		File.Delete(text);
		PdfDAL pdfDAL = new PdfDAL(text);
		Auditai.DTO.Pdf dto = pdf.ToDto();
		pdfDAL.SavePdf(dto);
		if (isDeleting)
		{
			_dal.DeleteSnapshots(pdf.TreeNode.Id);
		}
		_dal.SaveSnapshot(new SnapshotInfo
		{
			Id = num,
			DateTime = DateTime.Now,
			Kind = 3,
			Name = pdf.TreeNode.Name,
			Size = (int)(fileCacheManager.Exists(pdf.FileId) ? fileCacheManager.GetFileSize(pdf.FileId) : new FileInfo(text).Length),
			TreeNodeId = pdf.TreeNode.Id,
			Deleted = isDeleting
		});
	}

	public List<SnapshotInfo> GetSnapshots(TreeNodeBase node)
	{
		return _dal.GetSnapshots(node.Id).ToList();
	}

	public List<SnapshotInfo> GetRecycleList()
	{
		return _dal.GetRecycleList().ToList();
	}

	public TreeTableNode GetSnapshotTable(SnapshotInfo si)
	{
		TreeTableNode treeTableNode = new TreeTableNode
		{
			Id = Project.Current.GetNextId(),
			Dirty = 0,
			Name = si.Name,
			Status = SyncStatus.New,
			Version = 0
		};
		TableDAL tableDAL = new TableDAL(Path.Combine(GetDirectory().FullName, $"{si.Id}.db"));
		Auditai.DTO.Table table = tableDAL.GetTable();
		treeTableNode.Table._loaded = true;
		treeTableNode.Table.NeedSave = true;
		treeTableNode.Table.Title.Deserialize(table.Title);
		treeTableNode.Table.PageSetup.Deserialize(table.PageSetup);
		treeTableNode.Table.HeaderHeights = treeTableNode.Table.DeserializeHeaderHeights(table.HeaderHeights);
		treeTableNode.Table.ConsolidateSettings = ConsolidateSettings.Deserialize(table.ConsolidateSettings);
		treeTableNode.Table.BorderStyle = TableBorderStyles.FromNumber(table.BorderStyle);
		treeTableNode.Table.FrozenCols = table.FrozenCols;
		treeTableNode.Table.HeaderMode = (TableHeaderMode)table.HeaderMode;
		treeTableNode.Table.CollectSource = table.CollectSource;
		treeTableNode.Table.Locker = table.Locker;
		treeTableNode.Table.FilterInfo = table.FilterInfo;
		treeTableNode.Table.Foot.Deserialize(table.Foot);
		treeTableNode.Table.RowOwnerLoadShare.Deserialize(table.RowOwnerLoadShare);
		treeTableNode.Table.Ticket.Deserialize(table.Ticket);
		treeTableNode.Table.ControlFormula = table.ControlFormula;
		Dictionary<Id64, CellStyle> dictionary = new Dictionary<Id64, CellStyle>();
		foreach (Auditai.DTO.CellStyle cellStyle2 in tableDAL.GetCellStyles())
		{
			Id64 nextId = Project.Current.GetNextId();
			CellStyle cellStyle = new CellStyle
			{
				Id = nextId,
				FontSize = cellStyle2.FontSize,
				BackColor = cellStyle2.BackColor.ToNullableColor(),
				ForeColor = cellStyle2.ForeColor.ToNullableColor(),
				FontFamily = cellStyle2.FontFamily,
				Align = (CellTextAlign?)cellStyle2.Align,
				Margin = cellStyle2.Margin,
				Bold = cellStyle2.Bold,
				Italic = cellStyle2.Italic,
				Underline = cellStyle2.Underline,
				Format = DataFormat.Parse(cellStyle2.Format),
				Locker = cellStyle2.Locked,
				Status = SyncStatus.New,
				DataType = Util.NullableIntToDataType(cellStyle2.DataType),
				DefaultValue = cellStyle2.DefaultValue,
				Comment = cellStyle2.Comment
			};
			dictionary.Add(cellStyle2.Id, cellStyle);
			treeTableNode.Table.CellStyles.Add(cellStyle);
		}
		treeTableNode.Table.DefaultStyle = dictionary[table.DefaultStyleId];
		Dictionary<Id64, Column> dictionary2 = new Dictionary<Id64, Column>();
		Dictionary<Id64, Column> dictionary3 = new Dictionary<Id64, Column>();
		foreach (Auditai.DTO.Column column2 in tableDAL.GetColumns())
		{
			Column column = new Column
			{
				Table = treeTableNode.Table,
				Id = Project.Current.GetNextId(),
				Status = SyncStatus.New,
				Index = column2.Index,
				ServerIndex = column2.ServerIndex,
				Caption = column2.Caption,
				Width = column2.Width,
				Visible = column2.Visible,
				Formula = column2.Formula,
				ConsolidateAttributes = (string.IsNullOrWhiteSpace(column2.ConsolidateAttribs) ? null : ConsolidateAttributes.Deserialize(column2.ConsolidateAttribs)),
				CaptionFormula = column2.CaptionFormula,
				SubtotalAttributes = (ColumnSubtotal)column2.SubtotalAttribs
			};
			column.Permissions.Deserialize(column2.Permissions);
			column.CrossAttributes.Deserialize(column2.CrossAttributes);
			column.CaptionStyle.Deserialize(column2.CaptionStyle);
			if (column2.StyleId.HasValue)
			{
				column.Style = dictionary[column2.StyleId.Value];
			}
			dictionary2.Add(column2.Id, column);
			dictionary3.Add(column.Id, column);
			treeTableNode.Table.Columns._list.Add(column);
		}
		treeTableNode.Table.Columns.ResetIndex();
		foreach (Auditai.DTO.Row row2 in tableDAL.GetRows())
		{
			Row row = new Row
			{
				Table = treeTableNode.Table,
				Id = Project.Current.GetNextId(),
				Index = row2.Index,
				ServerIndex = row2.ServerIndex,
				Status = SyncStatus.New,
				Visible = row2.Visible,
				Height = row2.Height,
				Locker = row2.Locked,
				Role = (RowRole)row2.Role,
				NeedSave = true,
				Creator = User.Current.Id
			};
			row.Permissions.Deserialize(row2.Permissions);
			treeTableNode.Table.Rows._list.Add(row);
		}
		treeTableNode.Table.Rows.ResetIndex();
		List<Auditai.DTO.Cell> list = tableDAL.GetCells().ToList();
		Dictionary<Id64, Cell> dictionary4 = new Dictionary<Id64, Cell>();
		Dictionary<Id64, Cell> dictionary5 = new Dictionary<Id64, Cell>();
		for (int i = 0; i < treeTableNode.Table.Rows.Count; i++)
		{
			for (int j = 0; j < treeTableNode.Table.Columns.Count; j++)
			{
				Auditai.DTO.Cell cell = list[i * treeTableNode.Table.Columns.Count + j];
				Cell cell2 = new Cell
				{
					Row = treeTableNode.Table.Rows[i],
					Column = treeTableNode.Table.Columns[j],
					Id = Project.Current.GetNextId(),
					Value = cell.Value.Value,
					Status = SyncStatus.New,
					Formula = cell.Formula,
					CollectSource = cell.CollectSource,
					HeaderFormula = cell.HeaderFormula,
					NeedSave = true
				};
				if (cell.StyleId.HasValue)
				{
					cell2.Style = dictionary[cell.StyleId.Value];
				}
				cell2.DeserializeCellPrivateData(cell.Value.AdditionalData);
				dictionary4.Add(cell.Id, cell2);
				dictionary5.Add(cell2.Id, cell2);
				treeTableNode.Table.Cells._list.Add(cell2);
			}
		}
		foreach (CellProp cellProp in tableDAL.GetCellProps())
		{
			CellAttachments cellAttachments = new CellAttachments();
			Cell cell3 = dictionary4[cellProp.CellId];
			cellAttachments.Deserialize(cellProp.Attachments);
			foreach (CellAttachment attachment in cellAttachments.Attachments)
			{
				treeTableNode.Table.CellPropManager.AddAttachment(cell3, attachment.FileId, attachment.Name);
			}
		}
		List<ValidationFormula> validationFormulaList = tableDAL.GetValidationFormulas().Select(ValidationFormula.FromDto).ToList();
		TableFormulaRecycleProcesser tableFormulaRecycleProcesser = new TableFormulaRecycleProcesser(table.Id, treeTableNode, dictionary2, dictionary4, dictionary3, dictionary5, validationFormulaList);
		tableFormulaRecycleProcesser._tableDic.Add(table.Id, treeTableNode.Table);
		tableFormulaRecycleProcesser.ReverFormula();
		return treeTableNode;
	}

	public TreeDocumentNode GetSnapshotDocument(SnapshotInfo si)
	{
		TreeDocumentNode treeDocumentNode = new TreeDocumentNode
		{
			Id = Project.Current.GetNextId(),
			Dirty = 0,
			Name = si.Name,
			Status = SyncStatus.New,
			Version = 0
		};
		treeDocumentNode.Document._isLoaded = true;
		DocumentDAL documentDAL = new DocumentDAL(Path.Combine(GetDirectory().FullName, $"{si.Id}.db"));
		Auditai.DTO.Document document = documentDAL.GetDocument();
		treeDocumentNode.Document.SectPr = document.SectPr;
		treeDocumentNode.Document.MergeTable = document.MergeTable;
		foreach (Auditai.DTO.Paragraph paragraph2 in documentDAL.GetParagraphs())
		{
			Paragraph paragraph = Paragraph.FromDto(paragraph2);
			paragraph.Id = Project.Current.GetNextId();
			paragraph.Document = treeDocumentNode.Document;
			paragraph.RemoveBookmark();
			treeDocumentNode.Document.Paragraphs.Add(paragraph);
		}
		return treeDocumentNode;
	}

	public TreeImageNode GetSnapshotImage(SnapshotInfo si)
	{
		TreeImageNode treeImageNode = new TreeImageNode
		{
			Id = Project.Current.GetNextId(),
			Dirty = 0,
			Name = si.Name,
			Status = SyncStatus.New,
			Version = 0
		};
		treeImageNode.Image._isLoaded = true;
		treeImageNode.Image.NeedSave = true;
		ImageDAL imageDAL = new ImageDAL(Path.Combine(GetDirectory().FullName, $"{si.Id}.db"));
		Auditai.DTO.Image image = imageDAL.GetImage();
		treeImageNode.Image.FileId = image.FileId;
		treeImageNode.Image.Center = new PointF(image.CenterX, image.CenterY);
		treeImageNode.Image.ZoomFactor = image.ZoomFactor;
		treeImageNode.Image.RotateFlip = (RotateFlipType)image.RotateFlip;
		treeImageNode.Image.PageSetup.Deserialize(image.PageSetup);
		return treeImageNode;
	}

	public TreePdfNode GetSnapshotPdf(SnapshotInfo si)
	{
		TreePdfNode treePdfNode = new TreePdfNode
		{
			Id = Project.Current.GetNextId(),
			Dirty = 0,
			Name = si.Name,
			Status = SyncStatus.New,
			Version = 0
		};
		treePdfNode.Pdf._isLoaded = true;
		treePdfNode.Pdf._isFirstOpened = true;
		PdfDAL pdfDAL = new PdfDAL(Path.Combine(GetDirectory().FullName, $"{si.Id}.db"));
		Auditai.DTO.Pdf pdf = pdfDAL.GetPdf();
		treePdfNode.Pdf.FileId = pdf.FileId;
		return treePdfNode;
	}

	public void DeleteSnapshot(SnapshotInfo si)
	{
		string path = Path.Combine(GetDirectory().FullName, $"{si.Id}.db");
		try
		{
			File.Delete(path);
		}
		catch
		{
		}
		_dal.DeleteSnapshot(si);
	}

	private DirectoryInfo GetDirectory()
	{
		return Directory.CreateDirectory(Path.Combine("data", User.Current.Id.ToString(), _owner.Id.ToString(), "Snapshots"));
	}
}
