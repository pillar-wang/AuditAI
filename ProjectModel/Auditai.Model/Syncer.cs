using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.Model;

public static class Syncer
{
	public static bool Disabled = true;

	private class TreeNodeSyncTuple
	{
		public TreeNodeBase model;

		public Id64 gid;

		public Id64? pid;

		public int desireIndex;
	}

	public static async Task<PushResult> Push(Table table, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled) return new PushResult();
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.1f, reportCallback);
		TaskProgressValueUpdater taskProgressValueUpdater2 = new TaskProgressValueUpdater(0.1f, 0.9f, reportCallback);
		int num = table.Columns.Count + table.RemovedColumns.Count + table.Rows.Count + table.RemovedRows.Count + table.Cells.Count + table.RemovedRows.Count + table.CellStyles.Count() + table.MergedCells.Count + table.MergesToDelete.Count;
		int num2 = 0;
		table.LoadAndReturn();
		PushTable pushTable = new PushTable();
		Auditai.DTO.Table table2 = table.ToDto();
		pushTable.Id = table2.Id.Value;
		pushTable.ProjectId = ByteString.CopyFrom(table.Project.Id.ToByteArray());
		pushTable.Version = table2.Version;
		if (table.Version == 0)
		{
			pushTable.Title = table2.Title;
			pushTable.HeaderHeights = table2.HeaderHeights;
			pushTable.DefaultStyleId = table2.DefaultStyleId.Value;
			pushTable.PageSetup = table2.PageSetup;
			pushTable.ConsolidateSettings = table2.ConsolidateSettings;
			pushTable.BorderStyle = table2.BorderStyle;
			pushTable.FrozenCols = table2.FrozenCols;
			pushTable.HeaderMode = table2.HeaderMode;
			pushTable.CollectSource = table2.CollectSource;
			pushTable.Locker = table2.Locker;
			pushTable.FilterInfo = table2.FilterInfo;
			pushTable.Foot = table2.Foot;
			pushTable.RowOwnerExclusive = table2.RowOwnerExclusive;
			pushTable.RowOwnerLoad = table2.RowOwnerLoad;
			pushTable.RowOwnerLoadShare = ByteString.CopyFrom(table2.RowOwnerLoadShare);
			pushTable.Ticket = table2.Ticket;
			pushTable.ControlFormula = table2.ControlFormula;
			pushTable.Mask = -1;
		}
		else
		{
			if (table.Dirty.AnySet())
			{
				if (table.Dirty.IsTitleDirty)
				{
					pushTable.Title = table2.Title;
				}
				if (table.Dirty.IsHeaderHeightsDirty)
				{
					pushTable.HeaderHeights = table2.HeaderHeights;
				}
				if (table.Dirty.IsDefaultStyleDirty)
				{
					pushTable.DefaultStyleId = table2.DefaultStyleId.Value;
				}
				if (table.Dirty.IsPageSetupDirty)
				{
					pushTable.PageSetup = table2.PageSetup;
				}
				if (table.Dirty.IsConsolidateSettingsDirty)
				{
					pushTable.ConsolidateSettings = table2.ConsolidateSettings;
				}
				if (table.Dirty.IsBorderStyleDirty)
				{
					pushTable.BorderStyle = table2.BorderStyle;
				}
				if (table.Dirty.IsFrozenColsDirty)
				{
					pushTable.FrozenCols = table2.FrozenCols;
				}
				if (table.Dirty.IsHeaderModeDirty)
				{
					pushTable.HeaderMode = table2.HeaderMode;
				}
				if (table.Dirty.IsCollectSourceDirty)
				{
					pushTable.CollectSource = table2.CollectSource;
				}
				if (table.Dirty.IsLockerDirty)
				{
					pushTable.Locker = table2.Locker;
				}
				if (table.Dirty.IsFilterDirty)
				{
					pushTable.FilterInfo = table2.FilterInfo;
				}
				if (table.Dirty.IsFootDirty)
				{
					pushTable.Foot = table2.Foot;
				}
				if (table.Dirty.IsRowOwnerExclusiveDirty)
				{
					pushTable.RowOwnerExclusive = table2.RowOwnerExclusive;
				}
				if (table.Dirty.IsRowOwnerLoadDirty)
				{
					pushTable.RowOwnerLoad = table2.RowOwnerLoad;
				}
				if (table.Dirty.IsRowOwnerLoadShareDirty)
				{
					pushTable.RowOwnerLoadShare = ByteString.CopyFrom(table2.RowOwnerLoadShare);
				}
				if (table.Dirty.IsTicketDirty)
				{
					pushTable.Ticket = table2.Ticket;
				}
				if (table.Dirty.IsControlFormulaDirty)
				{
					pushTable.ControlFormula = table2.ControlFormula;
				}
			}
			pushTable.Mask = table.Dirty.ToInt();
		}
		RepeatedField<PushColumn> columns = pushTable.Columns;
		foreach (Column column2 in table.Columns)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (column2.Status == SyncStatus.New)
			{
				Auditai.DTO.Column column = column2.ToDto();
				columns.Add(new PushColumn
				{
					Action = 1,
					Id = column.Id.Value,
					Width = column.Width,
					Index = column.Index,
					Caption = column.Caption,
					Visible = column.Visible,
					StyleId = (column.StyleId.HasValue ? new Int64Value
					{
						Value = column.StyleId.Value.Value
					} : null),
					CaptionStyle = column.CaptionStyle,
					ConsolidateAttribs = column.ConsolidateAttribs,
					Formula = column.Formula,
					SubtotalAttribs = column.SubtotalAttribs,
					Permissions = column.Permissions,
					CaptionFormula = column.CaptionFormula,
					CrossAttributes = ByteString.CopyFrom(column.CrossAttributes),
					Mask = -1
				});
			}
			else
			{
				if (column2.Status != SyncStatus.Synced)
				{
					continue;
				}
				ColumnDirtyMask dirty = column2.Dirty;
				dirty.IsIndexDirty = column2.IsIndexDirty;
				if (dirty.AnySet())
				{
					PushColumn pushColumn = new PushColumn();
					pushColumn.Action = 2;
					pushColumn.Id = column2.Id.Value;
					if (dirty.IsWidthDirty)
					{
						pushColumn.Width = column2.Width;
					}
					if (dirty.IsIndexDirty)
					{
						pushColumn.Index = column2.Index;
					}
					if (dirty.IsCaptionDirty)
					{
						pushColumn.Caption = column2.Caption;
					}
					if (dirty.IsVisibleDirty)
					{
						pushColumn.Visible = column2.Visible;
					}
					if (dirty.IsStyleDirty)
					{
						pushColumn.StyleId = ((column2.Style == null) ? null : new Int64Value
						{
							Value = column2.Style.Id.Value
						});
					}
					if (dirty.IsCaptionStyleDirty)
					{
						pushColumn.CaptionStyle = column2.CaptionStyle.Serialize();
					}
					if (dirty.IsConsolidateAttribsDirty)
					{
						pushColumn.ConsolidateAttribs = column2.SerializeConsolidateAttributes();
					}
					if (dirty.IsFormulaDirty)
					{
						pushColumn.Formula = column2.Formula;
					}
					if (dirty.IsSubtotalAttribDirty)
					{
						pushColumn.SubtotalAttribs = (int)column2.SubtotalAttributes;
					}
					if (dirty.IsPermissionsDirty)
					{
						pushColumn.Permissions = column2.Permissions.Serialize();
					}
					if (dirty.IsCaptionFormulaDirty)
					{
						pushColumn.CaptionFormula = column2.CaptionFormula;
					}
					if (dirty.IsCrossAttributesDirty)
					{
						pushColumn.CrossAttributes = ByteString.CopyFrom(column2.CrossAttributes.Serialize());
					}
					pushColumn.Mask = dirty.ToInt();
					columns.Add(pushColumn);
				}
			}
		}
		foreach (Id64 removedColumn in table.RemovedColumns)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			columns.Add(new PushColumn
			{
				Action = 3,
				Id = removedColumn.Value
			});
		}
		RepeatedField<PushRow> rows = pushTable.Rows;
		foreach (Row row2 in table.Rows)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (row2.Status == SyncStatus.New)
			{
				Auditai.DTO.Row row = row2.ToDto();
				rows.Add(new PushRow
				{
					Action = 1,
					Id = row.Id.Value,
					Height = row.Height,
					Index = row.Index,
					Visible = row.Visible,
					Locker = row.Locked,
					Role = row.Role,
					Permissions = row.Permissions,
					Creator = row.Creator,
					Mask = -1
				});
			}
			else
			{
				if (row2.Status != SyncStatus.Synced)
				{
					continue;
				}
				RowDirtyMask dirty2 = row2.Dirty;
				dirty2.IsIndexDirty = row2.IsIndexDirty;
				if (dirty2.AnySet())
				{
					PushRow pushRow = new PushRow();
					pushRow.Action = 2;
					pushRow.Id = row2.Id.Value;
					if (dirty2.IsHeightDirty)
					{
						pushRow.Height = row2.Height;
					}
					if (dirty2.IsIndexDirty)
					{
						pushRow.Index = row2.Index;
					}
					if (dirty2.IsVisibleDirty)
					{
						pushRow.Visible = row2.Visible;
					}
					if (dirty2.IsLockerDirty)
					{
						pushRow.Locker = row2.Locker;
					}
					if (dirty2.IsRoleDirty)
					{
						pushRow.Role = (int)row2.Role;
					}
					if (dirty2.IsPermissionsDirty)
					{
						pushRow.Permissions = row2.Permissions.Serialize();
					}
					if (dirty2.IsCreatorDirty)
					{
						pushRow.Creator = row2.Creator;
					}
					pushRow.Mask = dirty2.ToInt();
					rows.Add(pushRow);
				}
			}
		}
		foreach (Id64 removedRow in table.RemovedRows)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			rows.Add(new PushRow
			{
				Action = 3,
				Id = removedRow.Value
			});
		}
		RepeatedField<PushCell> cells = pushTable.Cells;
		foreach (Cell cell2 in table.Cells)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (cell2.Status == SyncStatus.New)
			{
				Auditai.DTO.Cell cell = cell2.ToDto();
				cells.Add(new PushCell
				{
					Action = 1,
					Id = cell.Id.Value,
					CId = cell.ColumnId.Value,
					RId = cell.RowId.Value,
					Value = ByteString.CopyFrom(cell.Value.GetBytes()),
					Formula = cell.Formula,
					StyleId = (cell.StyleId.HasValue ? new Int64Value
					{
						Value = cell.StyleId.Value.Value
					} : null),
					CollectSource = cell.CollectSource,
					HeaderFormula = cell.HeaderFormula,
					Mask = -1
				});
			}
			else if (cell2.Status == SyncStatus.Synced && cell2.Dirty.AnySet())
			{
				PushCell pushCell = new PushCell();
				pushCell.Action = 2;
				pushCell.Id = cell2.Id.Value;
				CellDirtyMask dirty3 = cell2.Dirty;
				if (dirty3.IsValueDirty)
				{
					pushCell.Value = ByteString.CopyFrom(cell2.GetBinaryValue().GetBytes());
				}
				if (dirty3.IsFormulaDirty)
				{
					pushCell.Formula = cell2.Formula;
				}
				if (dirty3.IsStyleDirty)
				{
					pushCell.StyleId = ((cell2.Style == null) ? null : new Int64Value
					{
						Value = cell2.Style.Id.Value
					});
				}
				if (dirty3.IsCollectSourceDirty)
				{
					pushCell.CollectSource = cell2.CollectSource;
				}
				if (dirty3.IsHeaderFormulaDirty)
				{
					pushCell.HeaderFormula = cell2.HeaderFormula;
				}
				pushCell.Mask = dirty3.ToInt();
				cells.Add(pushCell);
			}
		}
		foreach (Id64 removedCell in table.RemovedCells)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			cells.Add(new PushCell
			{
				Action = 3,
				Id = removedCell.Value
			});
		}
		RepeatedField<PushCellStyle> cellStyles = pushTable.CellStyles;
		foreach (CellStyle cellStyle in table.CellStyles)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (cellStyle.Status == SyncStatus.New)
			{
				PushCellStyle pushCellStyle = new PushCellStyle();
				pushCellStyle.Id = cellStyle.Id.Value;
				pushCellStyle.FontFamily = cellStyle.FontFamily ?? string.Empty;
				pushCellStyle.FontSize = cellStyle.FontSize.GetValueOrDefault();
				pushCellStyle.ForeColor = cellStyle.ForeColor.GetValueOrDefault().ToArgb();
				pushCellStyle.BackColor = cellStyle.BackColor.GetValueOrDefault().ToArgb();
				pushCellStyle.Align = (int)cellStyle.Align.GetValueOrDefault();
				pushCellStyle.Margin = cellStyle.Margin.GetValueOrDefault();
				pushCellStyle.Bold = cellStyle.Bold.GetValueOrDefault();
				pushCellStyle.Italic = cellStyle.Italic.GetValueOrDefault();
				pushCellStyle.Underline = cellStyle.Underline.GetValueOrDefault();
				pushCellStyle.DataType = Util.DataTypeToNullableInt(cellStyle.DataType).GetValueOrDefault();
				pushCellStyle.Format = cellStyle.Format?.Serialize() ?? string.Empty;
				pushCellStyle.Locker = cellStyle.Locker.GetValueOrDefault();
				pushCellStyle.DefaultValue = cellStyle.DefaultValue ?? string.Empty;
				pushCellStyle.Comment = cellStyle.Comment ?? string.Empty;
				pushCellStyle.Mask = cellStyle.GetMask().ToInt();
				cellStyles.Add(pushCellStyle);
			}
		}
		RepeatedField<PushMerge> merges = pushTable.Merges;
		foreach (CellMerge mergedCell in table.MergedCells)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (mergedCell.Status == SyncStatus.New)
			{
				merges.Add(new PushMerge
				{
					Action = 1,
					Id = mergedCell.Id.Value,
					TopLeft = mergedCell.TopLeft.Id.Value,
					BottomRight = mergedCell.BottomRight.Id.Value
				});
			}
		}
		foreach (Id64 removedMerge in table.RemovedMerges)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			merges.Add(new PushMerge
			{
				Action = 3,
				Id = removedMerge.Value
			});
		}
		RepeatedField<PushCellAttachment> cellAttachments = pushTable.CellAttachments;
		foreach (KeyValuePair<Id64, CellAttachments> dicCellAttachment in table.CellPropManager.DicCellAttachments)
		{
			if (dicCellAttachment.Value.Status == SyncStatus.New)
			{
				cellAttachments.Add(new PushCellAttachment
				{
					Action = 1,
					TableId = table.Id.Value,
					CellId = dicCellAttachment.Key.Value,
					Attachments = ByteString.CopyFrom(dicCellAttachment.Value.Serialize())
				});
			}
			else if (dicCellAttachment.Value.Status == SyncStatus.Synced && dicCellAttachment.Value.Dirty)
			{
				cellAttachments.Add(new PushCellAttachment
				{
					Action = 2,
					TableId = table.Id.Value,
					CellId = dicCellAttachment.Key.Value,
					Attachments = ByteString.CopyFrom(dicCellAttachment.Value.Serialize())
				});
			}
		}
		JObject jObject = await StorageRouter.PushTable(pushTable, taskProgressValueUpdater2.UpdateProgress);
		if ((string)jObject["Result"] == "Success")
		{
			table.TreeNode.Version = (int)jObject["Version"];
			table.SetSynced();
			return PushResult.Success;
		}
		if ((string)jObject["Result"] == "OutOfDate")
		{
			return PushResult.OutOfDate;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task UpdateDataVersion(Project project)
	{
		if (Disabled) return;
		JObject jObject = new JObject();
		jObject.Add("Action", "PushProject");
		jObject.Add("Id", project.Id);
		jObject.Add("Version", project.Version);
		jObject.Add("UpdateDataVersion", true);
		if (StorageRouter.IsLocalMode) return;
		await WebApiClient.UpdateProjectVersion(jObject);
		project.Version++;
	}

	public static async Task<PushResult> Push(Project project)
	{
		if (Disabled) return new PushResult();
		JObject jObject = new JObject();
		jObject.Add("Action", "PushProject");
		jObject.Add("Id", project.Id);
		jObject.Add("Version", project.Version);
		JArray jArray = new JArray();
		jObject.Add("Groups", jArray);
		JArray jArray2 = new JArray();
		jObject.Add("Nodes", jArray2);
		JArray jArray3 = new JArray();
		jObject.Add("DataRefs", jArray3);
		JArray jArray4 = new JArray();
		jObject.Add("VFs", jArray4);
		foreach (ValidationFormula formula in project.ValidationManager.Formulas)
		{
			if (formula.Status == SyncStatus.New)
			{
				jArray4.Add(JObject.FromObject(new
				{
					Action = "New",
					Id = JToken.FromObject(formula.Id),
					LeftExpr = formula.LeftExpr,
					Op = formula.Operator.Code,
					RightExpr = formula.RightExpr,
					Note = formula.Note,
					TableId = formula.TableId
				}));
			}
			else if (formula.Status == SyncStatus.Synced && formula.IsDirty)
			{
				jArray4.Add(JObject.FromObject(new
				{
					Action = "Mod",
					Id = JToken.FromObject(formula.Id),
					LeftExpr = formula.LeftExpr,
					Op = formula.Operator.Code,
					RightExpr = formula.RightExpr,
					Note = formula.Note,
					TableId = formula.TableId
				}));
			}
		}
		foreach (Id64 item2 in project.ValidationManager._removed)
		{
			jArray4.Add(JObject.FromObject(new
			{
				Action = "Del",
				Id = JToken.FromObject(item2)
			}));
		}
		foreach (DataReference item3 in from dr in project.DataReferenceManager.Enumerate()
			where dr.Kind == DataReferenceKind.Text || dr.Kind == DataReferenceKind.CellRef
			select dr)
		{
			if (item3.Status == SyncStatus.New)
			{
				jArray3.Add(JObject.FromObject(new
				{
					Action = "New",
					Id = JToken.FromObject(item3.Id),
					Key = item3.Key,
					Value = item3.Value,
					Kind = item3.Kind
				}));
			}
			else if (item3.Status == SyncStatus.Synced && (item3.IsKeyDirty || item3.IsValueDirty))
			{
				JObject jObject2 = new JObject();
				jArray3.Add(jObject2);
				jObject2.Add("Action", "Mod");
				jObject2.Add("Id", JToken.FromObject(item3.Id));
				if (item3.IsKeyDirty)
				{
					jObject2.Add("Key", item3.Key);
				}
				if (item3.IsValueDirty)
				{
					jObject2.Add("Value", item3.Value);
				}
			}
		}
		foreach (Id64 item4 in project.DataReferenceManager._removed)
		{
			jArray3.Add(JObject.FromObject(new
			{
				Action = "Del",
				Id = item4.Value
			}));
		}
		foreach (TreeGroup treeGroup in project.TreeGroups)
		{
			if (treeGroup.Status == SyncStatus.New)
			{
				jArray.Add(JObject.FromObject(new
				{
					Action = "New",
					Id = JToken.FromObject(treeGroup.Id),
					Name = treeGroup.Name,
					Index = treeGroup.Index
				}));
			}
			else
			{
				if (treeGroup.Status != SyncStatus.Synced)
				{
					continue;
				}
				bool isNameDirty = treeGroup.IsNameDirty;
				bool isIndexDirty = treeGroup.IsIndexDirty;
				if (isNameDirty || isIndexDirty)
				{
					JObject jObject3 = new JObject();
					jObject3.Add("Action", "Mod");
					jObject3.Add("Id", JToken.FromObject(treeGroup.Id));
					if (isNameDirty)
					{
						jObject3.Add("Name", treeGroup.Name);
					}
					if (isIndexDirty)
					{
						jObject3.Add("Index", treeGroup.Index);
					}
					jArray.Add(jObject3);
				}
			}
		}
		foreach (Id64 removedTreeGroup in project.RemovedTreeGroups)
		{
			jArray.Add(JObject.FromObject(new
			{
				Action = "Del",
				Id = JToken.FromObject(removedTreeGroup)
			}));
		}
		foreach (TreeNodeBase allTreeNode in project.GetAllTreeNodes())
		{
			if (allTreeNode.Status == SyncStatus.New)
			{
				JObject item = JObject.FromObject(new
				{
					Action = "New",
					Id = JToken.FromObject(allTreeNode.Id),
					GroupId = JToken.FromObject(allTreeNode.Group.Id),
					ParentId = allTreeNode.Parent?.Id,
					Name = allTreeNode.Name,
					Index = allTreeNode.Index,
					Type = allTreeNode.GetCode(),
					Level = allTreeNode.Level,
					Number = allTreeNode.Number,
					Permissions = allTreeNode.Permissions.Serialize(),
					Visible = allTreeNode.Visible,
					RowWrite = allTreeNode.RowWrite,
					RowRead = allTreeNode.RowRead
				});
				jArray2.Add(item);
			}
			else
			{
				if (allTreeNode.Status != SyncStatus.Synced)
				{
					continue;
				}
				bool isNameDirty2 = allTreeNode.IsNameDirty;
				bool isIndexDirty2 = allTreeNode.IsIndexDirty;
				bool isNumberDirty = allTreeNode.IsNumberDirty;
				bool isGroupDirty = allTreeNode.IsGroupDirty;
				bool isParentDirty = allTreeNode.IsParentDirty;
				bool isPermissionsDirty = allTreeNode.IsPermissionsDirty;
				bool isVisibleDirty = allTreeNode.IsVisibleDirty;
				bool isRowWriteDirty = allTreeNode.IsRowWriteDirty;
				bool isRowReadDirty = allTreeNode.IsRowReadDirty;
				if (isNameDirty2 || isIndexDirty2 || isNumberDirty || isGroupDirty || isParentDirty || isPermissionsDirty || isVisibleDirty || isRowWriteDirty || isRowReadDirty)
				{
					JObject jObject4 = new JObject();
					jObject4.Add("Action", "Mod");
					jObject4.Add("Id", JToken.FromObject(allTreeNode.Id));
					if (isNameDirty2)
					{
						jObject4.Add("Name", allTreeNode.Name);
					}
					if (isIndexDirty2)
					{
						jObject4.Add("Index", allTreeNode.Index);
					}
					if (isNumberDirty)
					{
						jObject4.Add("Number", allTreeNode.Number);
					}
					if (isGroupDirty)
					{
						jObject4.Add("GroupId", JToken.FromObject(allTreeNode.Group.Id));
					}
					if (isParentDirty)
					{
						jObject4.Add("ParentId", (allTreeNode.Parent == null) ? null : JToken.FromObject(allTreeNode.Parent.Id));
					}
					if (isPermissionsDirty)
					{
						jObject4.Add("Permissions", allTreeNode.Permissions.Serialize());
					}
					if (isVisibleDirty)
					{
						jObject4.Add("Visible", allTreeNode.Visible);
					}
					if (isRowWriteDirty)
					{
						jObject4.Add("RowWrite", allTreeNode.RowWrite);
					}
					if (isRowReadDirty)
					{
						jObject4.Add("RowRead", allTreeNode.RowRead);
					}
					jArray2.Add(jObject4);
				}
			}
		}
		foreach (Id64 removedTreeNode in project.RemovedTreeNodes)
		{
			jArray2.Add(JObject.FromObject(new
			{
				Action = "Del",
				Id = JToken.FromObject(removedTreeNode)
			}));
		}
		if (StorageRouter.IsLocalMode)
		{
			project.SetSynced();
			return PushResult.Success;
		}
		JObject jObject5 = await WebApiClient.PushProject(project.Id, jObject);
		if ((string)jObject5["Result"] == "Success")
		{
			project.Version = (int)jObject5["Version"];
			project.SetSynced();
			return PushResult.Success;
		}
		if ((string)jObject5["Result"] == "NoContent")
		{
			return PushResult.NoContent;
		}
		if ((string)jObject5["Result"] == "OutOfDate")
		{
			return PushResult.OutOfDate;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PushResult> Push(Document document, TaskProgressValueReportCallback reportCallback = null)
	{
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.1f, reportCallback);
		TaskProgressValueUpdater taskProgressValueUpdater2 = new TaskProgressValueUpdater(0.1f, 0.9f, reportCallback);
		int num = document.Paragraphs.Count + document.RemovedParagraphs.Count;
		int num2 = 0;
		PushDocument pushDocument = new PushDocument();
		pushDocument.Id = document.Id.Value;
		pushDocument.ProjectId = ByteString.CopyFrom(document.Project.Id.ToByteArray());
		pushDocument.Version = document.Version;
		if (document.Version == 0)
		{
			pushDocument.Mask = -1;
			pushDocument.Locker = document.Locker;
			pushDocument.SectPr = document.SectPr;
			pushDocument.MergeTable = document.MergeTable.Value;
		}
		else
		{
			if (document.Dirty.AnySet())
			{
				if (document.Dirty.IsLockerDirty)
				{
					pushDocument.Locker = document.Locker;
				}
				if (document.Dirty.IsSectPrDirty)
				{
					pushDocument.SectPr = document.SectPr;
				}
				if (document.Dirty.IsMergeTableDirty)
				{
					pushDocument.MergeTable = document.MergeTable.Value;
				}
			}
			pushDocument.Mask = document.Dirty.ToInt();
		}
		RepeatedField<PushParagraph> paragraphs = pushDocument.Paragraphs;
		foreach (Paragraph paragraph2 in document.Paragraphs)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			Auditai.DTO.Paragraph paragraph = paragraph2.ToDto();
			if (paragraph2.Status == SyncStatus.New)
			{
				paragraphs.Add(new PushParagraph
				{
					Id = paragraph.Id.Value,
					Action = 1,
					Mask = -1,
					Index = paragraph.Index,
					Stream = ByteString.CopyFrom(paragraph.Stream),
					Comment = paragraph.Comment,
					Section = ((paragraph.Section == null) ? null : new BytesValue
					{
						Value = ByteString.CopyFrom(paragraph.Section)
					})
				});
			}
			else
			{
				if (paragraph2.Status != SyncStatus.Synced)
				{
					continue;
				}
				ParagraphDirtyMask dirty = paragraph2.Dirty;
				dirty.IsIndexDirty = paragraph2.IsIndexDirty;
				if (dirty.AnySet())
				{
					PushParagraph pushParagraph = new PushParagraph();
					pushParagraph.Action = 2;
					pushParagraph.Id = paragraph2.Id.Value;
					if (dirty.IsStreamDirty)
					{
						pushParagraph.Stream = ByteString.CopyFrom(paragraph.Stream);
						pushParagraph.Section = ((paragraph.Section == null) ? null : new BytesValue
						{
							Value = ByteString.CopyFrom(paragraph.Section)
						});
					}
					if (dirty.IsIndexDirty)
					{
						pushParagraph.Index = paragraph2.Index;
					}
					if (dirty.IsCommentDirty)
					{
						pushParagraph.Comment = paragraph.Comment;
					}
					pushParagraph.Mask = dirty.ToInt();
					paragraphs.Add(pushParagraph);
				}
			}
		}
		foreach (Id64 removedParagraph in document.RemovedParagraphs)
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			paragraphs.Add(new PushParagraph
			{
				Action = 3,
				Id = removedParagraph.Value
			});
		}
		JObject jObject = await StorageRouter.PushDocument(pushDocument, taskProgressValueUpdater2.UpdateProgress);
		if ((string)jObject["Result"] == "Success")
		{
			document.TreeNode.Version = (int)jObject["Version"];
			document.SetSynced();
			return PushResult.Success;
		}
		if ((string)jObject["Result"] == "OutOfDate")
		{
			return PushResult.OutOfDate;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PushResult> Push(Image image)
	{
		JObject request = new JObject
		{
			{ "Action", "PushImage" },
			{
				"Id",
				JToken.FromObject(image.Id)
			},
			{
				"ProjectId",
				image.Project.Id
			},
			{ "Version", image.Version }
		};
		if (image.Version == 0)
		{
			request.Add("FileId", image.FileId);
			request.Add("ZoomFactor", image.ZoomFactor);
			request.Add("CenterX", image.Center.X);
			request.Add("CenterY", image.Center.Y);
			request.Add("PageSetup", image.PageSetup.Serialize());
			request.Add("RotateFlip", (int)image.RotateFlip);
			await image.Project.FileCacheManager.Upload(image.FileId);
		}
		else if (image.Dirty > 0)
		{
			if (image.IsFileIdDirty)
			{
				request.Add("FileId", image.FileId);
				await image.Project.FileCacheManager.Upload(image.FileId);
			}
			if (image.IsCenterDirty)
			{
				request.Add("CenterX", image.Center.X);
				request.Add("CenterY", image.Center.Y);
			}
			if (image.IsZoomFactorDirty)
			{
				request.Add("ZoomFactor", image.ZoomFactor);
			}
			if (image.IsPageSetupDirty)
			{
				request.Add("PageSetup", image.PageSetup.Serialize());
			}
			if (image.IsRotateFlipDirty)
			{
				request.Add("RotateFlip", (int)image.RotateFlip);
			}
		}
		if (StorageRouter.IsLocalMode) { image.SetSynced(); return PushResult.Success; }
		JObject jObject = await WebApiClient.PushImage(request);
		if ((string)jObject["Result"] == "Success")
		{
			image.TreeNode.Version = (int)jObject["Version"];
			image.SetSynced();
			return PushResult.Success;
		}
		if ((string)jObject["Result"] == "OutOfDate")
		{
			return PushResult.OutOfDate;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PushResult> Push(Pdf pdf)
	{
		JObject request = new JObject
		{
			{ "Action", "PushPdf" },
			{
				"Id",
				JToken.FromObject(pdf.Id)
			},
			{
				"ProjectId",
				pdf.Project.Id
			},
			{ "Version", pdf.Version }
		};
		if (pdf.Version == 0)
		{
			request.Add("FileId", pdf.FileId);
			await pdf.Project.FileCacheManager.Upload(pdf.FileId);
		}
		else if (pdf.Dirty > 0 && pdf.IsFileIdDirty)
		{
			request.Add("FileId", pdf.FileId);
			await pdf.Project.FileCacheManager.Upload(pdf.FileId);
		}
		if (StorageRouter.IsLocalMode) { pdf.SetSynced(); return PushResult.Success; }
		JObject jObject = await WebApiClient.PushPdf(request);
		if ((string)jObject["Result"] == "Success")
		{
			pdf.TreeNode.Version = (int)jObject["Version"];
			pdf.SetSynced();
			return PushResult.Success;
		}
		if ((string)jObject["Result"] == "OutOfDate")
		{
			return PushResult.OutOfDate;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<IEnumerable<Tuple<Id64, int>>> QueryVersion(Guid projectId, IEnumerable<TreeTableNode> tableNodes)
	{
		if (Disabled || StorageRouter.IsLocalMode) return Enumerable.Empty<Tuple<Id64, int>>();
		JObject request = JObject.FromObject(new
		{
			Action = "QueryTableVersion",
			ProjectId = projectId,
			TableVersions = tableNodes.Select((TreeTableNode n) => new { n.Id })
		});
		return (await WebApiClient.QueryTableVersions(request)).Select((JToken ele) => Tuple.Create(new Id64(ele.Value<long>("Id")), ele.Value<int>("Version")));
	}

	public static async Task<IEnumerable<Tuple<Id64, int>>> QueryVersion(Guid projectId, IEnumerable<TreeDocumentNode> docNodes)
	{
		if (Disabled || StorageRouter.IsLocalMode) return Enumerable.Empty<Tuple<Id64, int>>();
		JObject request = JObject.FromObject(new
		{
			Action = "QueryDocumentVersion",
			ProjectId = projectId,
			DocVersions = docNodes.Select((TreeDocumentNode n) => new { n.Id })
		});
		return (await WebApiClient.QueryDocumentVersions(request)).Select((JToken ele) => Tuple.Create(new Id64(ele.Value<long>("Id")), ele.Value<int>("Version")));
	}

	public static async Task<IEnumerable<Tuple<Id64, int>>> QueryVersion(Guid projectId, IEnumerable<TreeImageNode> imageNodes)
	{
		if (Disabled || StorageRouter.IsLocalMode) return Enumerable.Empty<Tuple<Id64, int>>();
		JObject request = JObject.FromObject(new
		{
			Action = "QueryImageVersion",
			ProjectId = projectId,
			ImageVersions = imageNodes.Select((TreeImageNode n) => new { n.Id })
		});
		return (await WebApiClient.QueryImageVersions(request)).Select((JToken ele) => Tuple.Create(new Id64(ele.Value<long>("Id")), ele.Value<int>("Version")));
	}

	public static async Task<IEnumerable<Tuple<Id64, int>>> QueryVersion(Guid projectId, IEnumerable<TreePdfNode> pdfNodes)
	{
		if (Disabled || StorageRouter.IsLocalMode) return Enumerable.Empty<Tuple<Id64, int>>();
		JObject request = JObject.FromObject(new
		{
			Action = "QueryPdfVersion",
			ProjectId = projectId,
			PdfVersions = pdfNodes.Select((TreePdfNode n) => new { n.Id })
		});
		return (await WebApiClient.QueryPdfVersions(request)).Select((JToken ele) => Tuple.Create(new Id64(ele.Value<long>("Id")), ele.Value<int>("Version")));
	}

	public static async Task<PullResult> Pull(Table table, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled) return new PullResult();
		JObject request = JObject.FromObject(new
		{
			Action = "PullTable",
			Id = JToken.FromObject(table.Id),
			ProjectId = table.Project.Id,
			Version = table.Version
		});
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.9f, reportCallback);
		TaskProgressValueUpdater mergeDataProgressUpdater = new TaskProgressValueUpdater(0.9f, 0.1f, reportCallback);
		PullTable pullTable = await StorageRouter.PullTable(request, taskProgressValueUpdater.UpdateProgress).ConfigureAwait(continueOnCapturedContext: false);
		if (pullTable.Result == "NeedUpdate")
		{
			Merge(pullTable, table, mergeDataProgressUpdater.UpdateProgress);
			table.TreeNode.Version = pullTable.Version;
			return PullResult.Success;
		}
		if (pullTable.Result == "Latest")
		{
			return PullResult.AlreadyLatest;
		}
		if (pullTable.Result == "NotExist")
		{
			return PullResult.NotExist;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task Revert(Table table, int latestVersion, int revertVersion, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled || StorageRouter.IsLocalMode) return;
		JObject request = JObject.FromObject(new
		{
			Action = "RevertTable",
			TableId = JToken.FromObject(table.Id),
			Version = latestVersion,
			RevertVersion = revertVersion,
			ProjectId = table.Project.Id,
			IsRowOwnerLoad = (!table.IsManager() && table.RowOwnerLoad)
		});
		JObject jObject = await WebApiClient.RevertTable(request, reportCallback);
		table._loaded = false;
		table.TreeNode.Version = (int)jObject["Version"];
	}

	public static async Task Revert(Document document, int revertVersion, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled || StorageRouter.IsLocalMode) return;
		JObject request = JObject.FromObject(new
		{
			Action = "RevertDocument",
			DocId = JToken.FromObject(document.Id),
			Version = document.Version,
			RevertVersion = revertVersion,
			ProjectId = document.Project.Id
		});
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.9f, reportCallback);
		TaskProgressValueUpdater mergeProgressUpdater = new TaskProgressValueUpdater(0.9f, 0.1f, reportCallback);
		PullDocument pullDocument = await WebApiClient.RevertDocument(request, taskProgressValueUpdater.UpdateProgress);
		Merge(pullDocument, document, mergeProgressUpdater.UpdateProgress);
		document.TreeNode.Version = pullDocument.Version;
	}

	public static async Task RevertTemporary(Table table, int latestVersion, int revertVersion, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled || StorageRouter.IsLocalMode) return;
		JObject request = JObject.FromObject(new
		{
			Action = "RevertTable",
			TableId = JToken.FromObject(table.Id),
			Version = latestVersion,
			RevertVersion = revertVersion,
			ProjectId = table.Project.Id
		});
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.9f, reportCallback);
		Merge(reportCallback: new TaskProgressValueUpdater(0.9f, 0.1f, reportCallback).UpdateProgress, pullTable: await WebApiClient.GetTableRevertDiff(request, taskProgressValueUpdater.UpdateProgress), table: table);
		if (table.RowOwnerLoad)
		{
			table.LoadRowOwnerLoadView();
		}
	}

	public static async Task RevertTemporary(Document document, int latestVersion, int revertVersion, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled || StorageRouter.IsLocalMode) return;
		JObject request = JObject.FromObject(new
		{
			Action = "RevertDocument",
			DocId = JToken.FromObject(document.Id),
			Version = latestVersion,
			RevertVersion = revertVersion,
			ProjectId = document.Project.Id
		});
		Merge(await WebApiClient.GetDocumentRevertDiff(request, reportCallback), document);
	}

	public static async Task<Tuple<PullResult, bool>> Pull(Project project)
	{
		if (Disabled || StorageRouter.IsLocalMode) return new Tuple<PullResult, bool>(new PullResult(), false);
		JObject request = JObject.FromObject(new
		{
			Action = "PullProject",
			Id = project.Id,
			Version = project.Version
		});
		JObject jObject = await WebApiClient.PullProject(request);
		if ((string)jObject["Result"] == "NeedUpdate")
		{
			bool item = Merge(jObject, project);
			project.Version = (int)jObject["Version"];
			return Tuple.Create(PullResult.Success, item);
		}
		if ((string)jObject["Result"] == "Latest")
		{
			return Tuple.Create(PullResult.AlreadyLatest, item2: false);
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PullResult> Pull(Document document, TaskProgressValueReportCallback reportCallback = null)
	{
		if (Disabled) return new PullResult();
		JObject request = JObject.FromObject(new
		{
			Action = "PullDocument",
			Id = JToken.FromObject(document.Id),
			ProjectId = document.Project.Id,
			Version = document.Version
		});
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 0.9f, reportCallback);
		TaskProgressValueUpdater mergeDataProgressUpdater = new TaskProgressValueUpdater(0.9f, 0.1f, reportCallback);
		PullDocument pullDocument = await StorageRouter.PullDocument(request, taskProgressValueUpdater.UpdateProgress);
		if (pullDocument.Result == "NeedUpdate")
		{
			Merge(pullDocument, document, mergeDataProgressUpdater.UpdateProgress);
			document.TreeNode.Version = pullDocument.Version;
			return PullResult.Success;
		}
		if (pullDocument.Result == "Latest")
		{
			return PullResult.AlreadyLatest;
		}
		if (pullDocument.Result == "NotExist")
		{
			return PullResult.NotExist;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PullResult> Pull(Image image)
	{
		if (Disabled || StorageRouter.IsLocalMode) return new PullResult();
		JObject request = JObject.FromObject(new
		{
			Action = "PullImage",
			Id = JToken.FromObject(image.Id),
			ProjectId = image.Project.Id,
			Version = image.Version
		});
		JObject jObject = await WebApiClient.PullImage(request);
		if ((string)jObject["Result"] == "NeedUpdate")
		{
			Merge(jObject, image);
			image.TreeNode.Version = (int)jObject["Version"];
			return PullResult.Success;
		}
		if ((string)jObject["Result"] == "Latest")
		{
			return PullResult.AlreadyLatest;
		}
		if ((string)jObject["Result"] == "NotExist")
		{
			return PullResult.NotExist;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	public static async Task<PullResult> Pull(Pdf pdf)
	{
		if (Disabled || StorageRouter.IsLocalMode) return new PullResult();
		JObject request = JObject.FromObject(new
		{
			Action = "PullPdf",
			Id = JToken.FromObject(pdf.Id),
			ProjectId = pdf.Project.Id,
			Version = pdf.Version
		});
		JObject jObject = await WebApiClient.PullPdf(request);
		if ((string)jObject["Result"] == "NeedUpdate")
		{
			pdf.TreeNode.Version = (int)jObject["Version"];
			pdf.FileId = (Guid)jObject["FileId"];
			return PullResult.Success;
		}
		if ((string)jObject["Result"] == "Latest")
		{
			return PullResult.AlreadyLatest;
		}
		if ((string)jObject["Result"] == "NotExist")
		{
			return PullResult.NotExist;
		}
		throw new InvalidOperationException("不应出现的代码路径，检查json返回结果");
	}

	private static byte[] GetBytes(this OptionalBytes target)
	{
		return target.Value.ToByteArray();
	}

	public static void Merge(PullTable pullTable, Table table, TaskProgressValueReportCallback reportCallback = null)
	{
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 1f, reportCallback);
		OptionalString title = pullTable.Title;
		if (title != null && !table.Dirty.IsTitleDirty)
		{
			table.Title.Deserialize(title.Value);
		}
		OptionalString headerHeights = pullTable.HeaderHeights;
		if (headerHeights != null && !table.Dirty.IsHeaderHeightsDirty)
		{
			int[] array = table.DeserializeHeaderHeights(headerHeights.Value);
			table.HeaderHeights = array.Concat(table.HeaderHeights.Skip(array.Length)).ToArray();
		}
		OptionalString pageSetup = pullTable.PageSetup;
		if (pageSetup != null && !table.Dirty.IsPageSetupDirty)
		{
			table.PageSetup.Deserialize(pageSetup.Value);
		}
		OptionalString consolidateSettings = pullTable.ConsolidateSettings;
		if (consolidateSettings != null && !string.IsNullOrWhiteSpace(consolidateSettings.Value) && !table.Dirty.IsConsolidateSettingsDirty)
		{
			table.ConsolidateSettings = ConsolidateSettings.Deserialize(consolidateSettings.Value);
		}
		OptionalInt32 borderStyle = pullTable.BorderStyle;
		if (borderStyle != null && !table.Dirty.IsBorderStyleDirty)
		{
			table.BorderStyle = TableBorderStyles.FromNumber(borderStyle.Value);
		}
		OptionalInt32 frozenCols = pullTable.FrozenCols;
		if (frozenCols != null && !table.Dirty.IsFrozenColsDirty)
		{
			table.FrozenCols = frozenCols.Value;
		}
		OptionalInt32 headerMode = pullTable.HeaderMode;
		if (headerMode != null && !table.Dirty.IsHeaderModeDirty)
		{
			table.HeaderMode = (TableHeaderMode)headerMode.Value;
		}
		OptionalString collectSource = pullTable.CollectSource;
		if (collectSource != null && !table.Dirty.IsCollectSourceDirty)
		{
			table.CollectSource = collectSource.Value;
		}
		OptionalInt64 locker = pullTable.Locker;
		if (locker != null && !table.Dirty.IsLockerDirty)
		{
			table.Locker = locker.Value;
		}
		if (pullTable.FilterInfo != null && !table.Dirty.IsFilterDirty)
		{
			table.FilterInfo = pullTable.FilterInfo.Value;
		}
		if (pullTable.Foot != null && !table.Dirty.IsFootDirty)
		{
			table.Foot.Deserialize(pullTable.Foot.Value);
		}
		OptionalBytes rowOwnerLoadShare = pullTable.RowOwnerLoadShare;
		if (rowOwnerLoadShare != null && !table.Dirty.IsRowOwnerLoadShareDirty)
		{
			table.RowOwnerLoadShare.Deserialize(rowOwnerLoadShare.GetBytes());
		}
		OptionalString ticket = pullTable.Ticket;
		if (ticket != null && (!table.Dirty.IsTicketDirty || table.Ticket.IsDirtyDataOnlyIncludeCanOverrideByServerData))
		{
			table.Ticket.Deserialize(ticket.Value);
			table.Ticket.SetSynced();
			table.Dirty.IsTicketDirty = false;
		}
		OptionalString controlFormula = pullTable.ControlFormula;
		if (controlFormula != null && !table.Dirty.IsControlFormulaDirty)
		{
			table.ControlFormula = controlFormula.Value;
		}
		int num = pullTable.CellStyles.Count + pullTable.NewColumns.Count + pullTable.DelColumns.Count + pullTable.ModColumns.Count + pullTable.NewRows.Count + pullTable.DelRows.Count + pullTable.ModRows.Count + pullTable.NewCells.Count + pullTable.DelCells.Count + pullTable.ModCells.Count + pullTable.NewCellProps.Count + pullTable.DelCellProps.Count + pullTable.ModCellProps.Count + pullTable.DelMerges.Count + pullTable.NewMerges.Count;
		int num2 = 0;
		foreach (var item in pullTable.CellStyles.Select((PullCellStyle cs) => new
		{
			Id = cs.Id,
			FontFamily = (cs.FontFamily.IsNull ? null : cs.FontFamily.Value),
			FontSize = (cs.FontSize.IsNull ? null : new float?(cs.FontSize.Value)),
			ForeColor = (cs.ForeColor.IsNull ? null : new Color?(Color.FromArgb(cs.ForeColor.Value))),
			BackColor = (cs.BackColor.IsNull ? null : new Color?(Color.FromArgb(cs.BackColor.Value))),
			Align = (cs.Align.IsNull ? null : new int?(cs.Align.Value)),
			Margin = (cs.Margin.IsNull ? null : new int?(cs.Margin.Value)),
			Bold = (cs.Bold.IsNull ? null : new bool?(cs.Bold.Value)),
			Italic = (cs.Italic.IsNull ? null : new bool?(cs.Italic.Value)),
			Underline = (cs.Underline.IsNull ? null : new bool?(cs.Underline.Value)),
			DataType = (cs.DataType.IsNull ? null : new int?(cs.DataType.Value)),
			Format = (cs.Format.IsNull ? null : cs.Format.Value),
			Locker = (cs.Locker.IsNull ? null : new long?(cs.Locker.Value)),
			DefaultValue = (cs.DefaultValue.IsNull ? null : cs.DefaultValue.Value),
			Comment = (cs.Comment.IsNull ? null : cs.Comment.Value)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			CellStyle cellStyle = new CellStyle
			{
				Id = new Id64(item.Id),
				BackColor = item.BackColor,
				FontFamily = item.FontFamily,
				FontSize = item.FontSize,
				ForeColor = item.ForeColor,
				Align = (CellTextAlign?)item.Align,
				Margin = item.Margin,
				Bold = item.Bold,
				Italic = item.Italic,
				Underline = item.Underline,
				DataType = Util.NullableIntToDataType(item.DataType),
				Status = SyncStatus.Synced,
				Format = DataFormat.Parse(item.Format),
				Locker = item.Locker,
				DefaultValue = item.DefaultValue,
				Comment = item.Comment
			};
			table.CellStyles.Add(cellStyle);
		}
		long? defaultStyleId = ((pullTable.DefaultStyleId == null || pullTable.DefaultStyleId.IsNull) ? null : new long?(pullTable.DefaultStyleId.Value));
		if (defaultStyleId.HasValue)
		{
			table.DefaultStyle = table.CellStyles.First((CellStyle s) => s.Id.Value == defaultStyleId.Value);
			table.Dirty.IsDefaultStyleDirty = false;
		}
		if (table.Rows.Count > 0)
		{
			Row lastRow = table.Rows[table.Rows.Count - 1];
			if (lastRow.Role == RowRole.Total && table.RowOwnerLoad && table.Project.Users.FirstOrDefault((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == lastRow.Creator).Value == UserRole.Manager)
			{
				lastRow.Index = int.MaxValue;
			}
		}
		Dictionary<Id64, Column> dictionary = table.Columns.ToDictionary((Column col) => col.Id, (Column col) => col);
		LinkedList<Column> linkedList = new LinkedList<Column>(table.Columns);
		Dictionary<Id64, Row> dictionary2 = table.Rows.ToDictionary((Row row) => row.Id, (Row row) => row);
		LinkedList<Row> linkedList2 = new LinkedList<Row>(table.Rows);
		Dictionary<Id64, Cell> dictionary3 = table.Cells.ToDictionary((Cell c) => c.Id, (Cell c) => c);
		foreach (var add2 in pullTable.NewColumns.Select((PullColumn c) => new
		{
			Id = c.Id,
			Caption = c.Caption.Value,
			Index = c.Index.Value,
			Width = c.Width.Value,
			Visible = c.Visible.Value,
			StyleId = (c.StyleId.IsNull ? null : new long?(c.StyleId.Value)),
			CaptionStyle = c.CaptionStyle.Value,
			ConsolidateAttribs = c.ConsolidateAttribs.Value,
			Formula = c.Formula.Value,
			SubtotalAttribs = c.SubtotalAttribs.Value,
			Permissions = c.Permissions.Value,
			CaptionFormula = c.CaptionFormula.Value,
			CrossAttributes = c.CrossAttributes.GetBytes()
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			Column column = new Column
			{
				Id = new Id64(add2.Id),
				Table = table,
				Status = SyncStatus.Synced,
				Caption = add2.Caption,
				Width = add2.Width,
				Visible = add2.Visible,
				ServerIndex = add2.Index,
				Index = add2.Index,
				ConsolidateAttributes = ConsolidateAttributes.Deserialize(add2.ConsolidateAttribs),
				Formula = add2.Formula,
				SubtotalAttributes = (ColumnSubtotal)add2.SubtotalAttribs,
				CaptionFormula = add2.CaptionFormula
			};
			column.CaptionStyle.Deserialize(add2.CaptionStyle);
			column.Permissions.Deserialize(add2.Permissions);
			column.CrossAttributes.Deserialize(add2.CrossAttributes);
			if (add2.StyleId.HasValue)
			{
				column.Style = table.CellStyles.First((CellStyle s) => s.Id.Value == add2.StyleId);
			}
			if (dictionary.ContainsKey(column.Id))
			{
				linkedList.Find(dictionary[column.Id]).Value = column;
			}
			dictionary[column.Id] = column;
			foreach (Row item2 in table.Rows.Where((Row r) => r.Status == SyncStatus.New))
			{
				Cell cell = table.MakeNewCell();
				cell.Status = SyncStatus.New;
				cell.Row = item2;
				cell.Column = column;
				dictionary3[cell.Id] = cell;
			}
		}
		foreach (var item3 in pullTable.DelColumns.Select((PullColumn c) => new
		{
			Id = new Id64(c.Id)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary.TryGetValue(item3.Id, out var toDel2))
			{
				toDel2.Status = SyncStatus.ServerDeleted;
				dictionary.Remove(toDel2.Id);
				linkedList.Remove(toDel2);
				foreach (Cell item4 in dictionary3.Values.Where((Cell c) => c.Column == toDel2 && c.Status == SyncStatus.New).ToList())
				{
					dictionary3.Remove(item4.Id);
					table.CellsToDelete.Add(item4.Id);
					item4.Status = SyncStatus.ServerDeleted;
				}
			}
			table.RemovedColumns.Remove(item3.Id);
			table.ColumnsToDelete.Add(item3.Id);
		}
		foreach (var mod2 in pullTable.ModColumns.Select((PullColumn c) => new
		{
			Id = c.Id,
			Caption = c.Caption,
			Index = c.Index,
			Width = c.Width,
			Visible = c.Visible,
			HasStyleId = (c.StyleId != null),
			StyleId = c.StyleId,
			CaptionStyle = c.CaptionStyle,
			ConsolidateAttribs = c.ConsolidateAttribs,
			Formula = c.Formula,
			SubtotalAttribs = c.SubtotalAttribs,
			Permissions = c.Permissions,
			CaptionFormula = c.CaptionFormula,
			CrossAttributes = c.CrossAttributes
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (!dictionary.TryGetValue(new Id64(mod2.Id), out var value))
			{
				continue;
			}
			if (mod2.Caption != null && !value.Dirty.IsCaptionDirty)
			{
				value.Caption = mod2.Caption.Value;
			}
			if (mod2.Width != null && !value.Dirty.IsWidthDirty)
			{
				value.Width = mod2.Width.Value;
			}
			if (mod2.Visible != null && !value.Dirty.IsVisibleDirty)
			{
				value.Visible = mod2.Visible.Value;
			}
			if (mod2.Index != null)
			{
				value.ServerIndex = mod2.Index.Value;
				value.Index = mod2.Index.Value;
			}
			if (mod2.HasStyleId && !value.Dirty.IsStyleDirty)
			{
				if (!mod2.StyleId.IsNull)
				{
					value.Style = table.CellStyles.FirstOrDefault((CellStyle s) => s.Id.Value == mod2.StyleId.Value);
				}
				else
				{
					value.Style = null;
				}
			}
			if (mod2.CaptionStyle != null && !value.Dirty.IsCaptionStyleDirty)
			{
				value.CaptionStyle.Deserialize(mod2.CaptionStyle.Value);
			}
			if (mod2.ConsolidateAttribs != null && !value.Dirty.IsConsolidateAttribsDirty)
			{
				value.ConsolidateAttributes = ConsolidateAttributes.Deserialize(mod2.ConsolidateAttribs.Value);
			}
			if (mod2.Formula != null && !value.Dirty.IsFormulaDirty)
			{
				value.Formula = mod2.Formula.Value;
			}
			if (mod2.SubtotalAttribs != null && !value.Dirty.IsSubtotalAttribDirty)
			{
				value.SubtotalAttributes = (ColumnSubtotal)mod2.SubtotalAttribs.Value;
			}
			if (mod2.Permissions != null && !value.Dirty.IsPermissionsDirty)
			{
				value.Permissions.Deserialize(mod2.Permissions.Value);
			}
			if (mod2.CaptionFormula != null && !value.Dirty.IsCaptionFormulaDirty)
			{
				value.CaptionFormula = mod2.CaptionFormula.Value;
			}
			if (mod2.CrossAttributes != null && !value.Dirty.IsCrossAttributesDirty)
			{
				value.CrossAttributes.Deserialize(mod2.CrossAttributes.GetBytes());
			}
		}
		foreach (var item5 in pullTable.NewRows.Select((PullRow r) => new
		{
			Id = r.Id,
			Index = r.Index.Value,
			Height = r.Height.Value,
			Visible = r.Visible.Value,
			Locker = r.Locker.Value,
			Role = r.Role.Value,
			Permissions = r.Permissions.Value,
			Creator = r.Creator.Value
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			Row row2 = new Row
			{
				NeedSave = true,
				Id = new Id64(item5.Id),
				Table = table,
				Status = SyncStatus.Synced,
				Height = item5.Height,
				Visible = item5.Visible,
				ServerIndex = item5.Index,
				Index = item5.Index,
				Locker = item5.Locker,
				Role = (RowRole)item5.Role,
				Creator = item5.Creator
			};
			row2.Permissions.Deserialize(item5.Permissions);
			if (dictionary2.ContainsKey(row2.Id))
			{
				linkedList2.Find(dictionary2[row2.Id]).Value = row2;
			}
			dictionary2[row2.Id] = row2;
			foreach (Column item6 in table.Columns.Where((Column c) => c.Status == SyncStatus.New))
			{
				Cell cell2 = table.MakeNewCell();
				cell2.Status = SyncStatus.New;
				cell2.Row = row2;
				cell2.Column = item6;
				dictionary3[cell2.Id] = cell2;
			}
		}
		foreach (var item7 in pullTable.DelRows.Select((PullRow r) => new
		{
			Id = new Id64(r.Id)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary2.TryGetValue(item7.Id, out var toDel))
			{
				dictionary2.Remove(toDel.Id);
				linkedList2.Remove(toDel);
				toDel.Status = SyncStatus.ServerDeleted;
				foreach (Cell item8 in dictionary3.Values.Where((Cell c) => c.Row == toDel && c.Status == SyncStatus.New).ToList())
				{
					dictionary3.Remove(item8.Id);
					table.CellsToDelete.Add(item8.Id);
					item8.Status = SyncStatus.ServerDeleted;
				}
			}
			table.RemovedRows.Remove(item7.Id);
			table.RowsToDelete.Add(item7.Id);
		}
		foreach (var item9 in pullTable.ModRows.Select((PullRow r) => new { r.Id, r.Index, r.Height, r.Visible, r.Locker, r.Role, r.Permissions, r.Creator }))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary2.TryGetValue(new Id64(item9.Id), out var value2))
			{
				value2.NeedSave = true;
				if (item9.Height != null && !value2.Dirty.IsHeightDirty)
				{
					value2.Height = item9.Height.Value;
				}
				if (item9.Visible != null && !value2.Dirty.IsVisibleDirty)
				{
					value2.Visible = item9.Visible.Value;
				}
				if (item9.Index != null)
				{
					value2.ServerIndex = item9.Index.Value;
					value2.Index = item9.Index.Value;
				}
				if (item9.Locker != null && !value2.Dirty.IsLockerDirty)
				{
					value2.Locker = item9.Locker.Value;
				}
				if (item9.Role != null && !value2.Dirty.IsRoleDirty)
				{
					value2.Role = (RowRole)item9.Role.Value;
				}
				if (item9.Permissions != null && !value2.Dirty.IsPermissionsDirty)
				{
					value2.Permissions.Deserialize(item9.Permissions.Value);
				}
				if (item9.Creator != null && !value2.Dirty.IsCreatorDirty)
				{
					value2.Creator = item9.Creator.Value;
				}
			}
		}
		foreach (var add in pullTable.NewCells.Select((PullCell c) => new
		{
			Id = c.Id,
			cId = c.CId,
			rId = c.RId,
			Value = c.Value,
			Formula = c.Formula,
			StyleId = c.Style,
			CollectSource = c.CollectSource,
			HeaderFormula = c.HeaderFormula
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary.TryGetValue(new Id64(add.cId.Value), out var value3) && dictionary2.TryGetValue(new Id64(add.rId.Value), out var value4))
			{
				BinaryValue binaryValue = new BinaryValue(add.Value.GetBytes());
				Cell cell3 = new Cell
				{
					Id = new Id64(add.Id),
					Dirty = default(CellDirtyMask),
					Status = SyncStatus.Synced,
					Column = value3,
					Row = value4,
					Value = binaryValue.Value,
					NeedSave = true,
					Formula = add.Formula.Value,
					Style = ((!add.StyleId.IsNull) ? table.CellStyles.FirstOrDefault((CellStyle cs) => cs.Id.Value == add.StyleId.Value) : null),
					CollectSource = add.CollectSource.Value,
					HeaderFormula = add.HeaderFormula.Value
				};
				cell3.DeserializeCellPrivateData(binaryValue.AdditionalData);
				dictionary3[cell3.Id] = cell3;
			}
			else
			{
				table.RemovedCells.Add(new Id64(add.Id));
			}
		}
		foreach (var item10 in pullTable.DelCells.Select((PullCell c) => new { c.Id }))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			Id64 id = new Id64(item10.Id);
			if (dictionary3.TryGetValue(id, out var value5))
			{
				value5.Status = SyncStatus.ServerDeleted;
			}
			dictionary3.Remove(id);
			table.CellsToDelete.Add(id);
			table.RemovedCells.Remove(id);
		}
		foreach (var mod in pullTable.ModCells.Select((PullCell c) => new
		{
			Id = c.Id,
			Value = c.Value,
			Formula = c.Formula,
			HasStyleId = (c.Style != null),
			StyleId = c.Style,
			CollectSource = c.CollectSource,
			HasHeaderFormula = (c.HeaderFormula != null),
			HeaderFormula = c.HeaderFormula
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (!dictionary3.TryGetValue(new Id64(mod.Id), out var value6))
			{
				continue;
			}
			value6.NeedSave = true;
			if (mod.Value != null && !value6.Dirty.IsValueDirty)
			{
				BinaryValue binaryValue2 = new BinaryValue(mod.Value.GetBytes());
				value6.Value = binaryValue2.Value;
				value6.DeserializeCellPrivateData(binaryValue2.AdditionalData);
			}
			if (mod.Formula != null && !value6.Dirty.IsFormulaDirty)
			{
				value6.Formula = mod.Formula.Value;
			}
			if (mod.HasStyleId && !value6.Dirty.IsStyleDirty)
			{
				if (!mod.StyleId.IsNull)
				{
					value6.Style = table.CellStyles.FirstOrDefault((CellStyle cs) => cs.Id.Value == mod.StyleId.Value);
				}
				else
				{
					value6.Style = null;
				}
			}
			if (mod.CollectSource != null && !value6.Dirty.IsCollectSourceDirty)
			{
				value6.CollectSource = mod.CollectSource.Value;
			}
			if (mod.HasHeaderFormula && !value6.Dirty.IsHeaderFormulaDirty)
			{
				value6.HeaderFormula = mod.HeaderFormula.Value;
			}
		}
		LinkedList<Column> linkedList3 = new LinkedList<Column>(from c in dictionary.Values
			where c.Status != SyncStatus.New
			orderby c.Index
			select c);
		foreach (Column item11 in from c in dictionary.Values
			where c.Status == SyncStatus.New
			orderby c.Index
			select c)
		{
			LinkedListNode<Column> previous = linkedList.Find(item11).Previous;
			if (previous == null)
			{
				linkedList3.AddFirst(item11);
			}
			else
			{
				linkedList3.AddAfter(linkedList3.Find(previous.Value), item11);
			}
		}
		table.Columns.Clear();
		table.Columns._list.AddRange(linkedList3);
		table.Columns.ResetIndex();
		LinkedList<Row> linkedList4 = new LinkedList<Row>(from r in dictionary2.Values
			where r.Status != SyncStatus.New
			orderby r.Index
			select r);
		foreach (Row item12 in from r in dictionary2.Values
			where r.Status == SyncStatus.New
			orderby r.Index
			select r)
		{
			LinkedListNode<Row> previous2 = linkedList2.Find(item12).Previous;
			if (previous2 == null)
			{
				linkedList4.AddFirst(item12);
			}
			else
			{
				linkedList4.AddAfter(linkedList4.Find(previous2.Value), item12);
			}
		}
		table.Rows.Clear();
		table.Rows._list.AddRange(linkedList4);
		table.Rows.ResetIndex();
		table.HeaderRowCache.Clear();
		foreach (Row row3 in table.Rows)
		{
			if (row3.Role == RowRole.Header || row3.Role == RowRole.Fixed)
			{
				table.HeaderRowCache.Add(row3);
			}
		}
		table.Cells.Clear();
		foreach (Cell item13 in from c in dictionary3.Values
			orderby c.Row.Index, c.Column.Index
			select c)
		{
			table.Cells._list.Add(item13);
		}
		foreach (var del in pullTable.DelMerges.Select((PullMerge j) => new
		{
			Id = new Id64(j.Id)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			table.MergedCells.RemoveWhere((CellMerge m) => m.Id == del.Id);
			table.MergesToDelete.Add(del.Id);
			table.RemovedMerges.Remove(del.Id);
		}
		foreach (var item14 in pullTable.NewMerges.Select((PullMerge j) => new { j.Id, j.TopLeft, j.BottomRight }))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary3.TryGetValue(new Id64(item14.TopLeft.Value), out var value7) && dictionary3.TryGetValue(new Id64(item14.BottomRight.Value), out var value8))
			{
				table.MergedCells.Add(new CellMerge
				{
					Id = new Id64(item14.Id),
					TopLeft = value7,
					BottomRight = value8,
					Status = SyncStatus.Synced
				});
			}
		}
		table.RemoveInvalidMerges();
		foreach (var item15 in pullTable.DelCellProps.Select((PullCellProp cp) => new
		{
			cellId = cp.CellId
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
		}
		foreach (var item16 in pullTable.NewCellProps.Select((PullCellProp cp) => new
		{
			cellId = new Id64(cp.CellId),
			attachments = cp.Attachments.GetBytes()
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (!table.CellPropManager.DicCellAttachments.TryGetValue(item16.cellId, out var value9))
			{
				value9 = new CellAttachments();
				table.CellPropManager.DicCellAttachments.Add(item16.cellId, value9);
			}
			value9.Dirty = false;
			value9.Status = SyncStatus.Synced;
			value9.Deserialize(item16.attachments);
		}
		foreach (var item17 in pullTable.ModCellProps.Select((PullCellProp cp) => new
		{
			cellId = new Id64(cp.CellId),
			attachments = cp.Attachments.GetBytes()
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (table.CellPropManager.DicCellAttachments.TryGetValue(item17.cellId, out var value10))
			{
				value10.Dirty = false;
				value10.Deserialize(item17.attachments);
			}
		}
	}

	private static bool Merge(JObject response, Project project)
	{
		bool result = false;
		var gDic = project.TreeGroups.ToDictionary((TreeGroup g) => g.Id, (TreeGroup g) => new
		{
			model = g,
			desireIndex = g.Index
		});
		Dictionary<Id64, TreeNodeSyncTuple> nDic = project.GetAllTreeNodes().ToDictionary((TreeNodeBase n) => n.Id, (TreeNodeBase n) => new TreeNodeSyncTuple
		{
			model = n,
			gid = n.Group.Id,
			pid = n.Parent?.Id,
			desireIndex = n.Index
		});
		foreach (TreeGroup treeGroup in project.TreeGroups)
		{
			foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
			{
				ClearNodeChildren(rootNode);
			}
			treeGroup.RootNodes.Clear();
		}
		project.TreeGroups.Clear();
		var list = response["NewGroups"].Select((JToken group) => new
		{
			Id = new Id64(group.Value<long>("Id")),
			Name = (string)group["Name"],
			Index = (int)group["Index"]
		}).ToList();
		foreach (var item in list)
		{
			result = true;
			if (gDic.ContainsKey(item.Id))
			{
				TreeGroup model = gDic[item.Id].model;
				model.Dirty = 0;
				model.Name = item.Name;
				model.ServerIndex = item.Index;
				model.Status = SyncStatus.Synced;
			}
			else
			{
				gDic.Add(item.Id, new
				{
					model = new TreeGroup
					{
						Id = item.Id,
						Dirty = 0,
						Name = item.Name,
						Project = project,
						ServerIndex = item.Index,
						Status = SyncStatus.Synced
					},
					desireIndex = item.Index
				});
			}
		}
		var enumerable = response["ModGroups"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id")),
			Name = (string)item["Name"],
			Index = (int?)item["Index"]
		});
		foreach (var item2 in enumerable)
		{
			result = true;
			if (gDic.TryGetValue(item2.Id, out var value))
			{
				if (item2.Name != null && !value.model.IsNameDirty)
				{
					value.model.Name = item2.Name;
				}
				if (item2.Index.HasValue)
				{
					value.model.ServerIndex = item2.Index.Value;
					gDic[item2.Id] = new
					{
						model = value.model,
						desireIndex = item2.Index.Value
					};
				}
			}
		}
		HashSet<Id64> hashSet = new HashSet<Id64>(response["DelGroups"].Select((JToken item) => new Id64(item.Value<long>("Id"))));
		foreach (Id64 item3 in hashSet)
		{
			result = true;
			gDic.Remove(item3);
			project.TreeGroupsToDelete.Add(item3);
		}
		foreach (TreeGroup item4 in from tup in gDic.Values
			orderby tup.desireIndex
			select tup.model)
		{
			project.TreeGroups.Add(item4);
		}
		foreach (var item5 in response["NewNodes"].Select((JToken node) => new
		{
			Id = new Id64(node.Value<long>("Id")),
			GroupId = new Id64(node.Value<long>("GroupId")),
			ParentId = Id64.FromNullableLong(node.Value<long?>("ParentId")),
			Name = (string)node["Name"],
			Index = (int)node["Index"],
			Type = (int)node["Type"],
			Number = (string)node["Number"],
			Permissions = (string)node["Permissions"],
			Visible = (bool)node["Visible"],
			RowWrite = (bool)node["RowWrite"],
			RowRead = (bool)node["RowRead"]
		}))
		{
			result = true;
			if (nDic.ContainsKey(item5.Id))
			{
				TreeNodeSyncTuple treeNodeSyncTuple = nDic[item5.Id];
				treeNodeSyncTuple.model.ServerIndex = item5.Index;
				treeNodeSyncTuple.model.Name = item5.Name;
				treeNodeSyncTuple.model.Dirty = 0;
				treeNodeSyncTuple.model.Status = SyncStatus.Synced;
				treeNodeSyncTuple.desireIndex = item5.Index;
				treeNodeSyncTuple.gid = item5.GroupId;
				treeNodeSyncTuple.pid = item5.ParentId;
				treeNodeSyncTuple.model.Number = item5.Number;
				treeNodeSyncTuple.model.Permissions.Deserialize(item5.Permissions);
				treeNodeSyncTuple.model.Visible = item5.Visible;
				treeNodeSyncTuple.model.RowWrite = item5.RowWrite;
				treeNodeSyncTuple.model.RowRead = item5.RowRead;
				continue;
			}
			TreeNodeBase treeNodeBase = TreeNodeBase.CreateInstanceFromCode(item5.Type);
			treeNodeBase.Id = item5.Id;
			treeNodeBase.Name = item5.Name;
			treeNodeBase.Status = SyncStatus.Synced;
			treeNodeBase.ServerIndex = item5.Index;
			treeNodeBase.Version = -1;
			treeNodeBase.Number = item5.Number;
			treeNodeBase.Permissions.Deserialize(item5.Permissions);
			treeNodeBase.Visible = item5.Visible;
			treeNodeBase.RowWrite = item5.RowWrite;
			treeNodeBase.RowRead = item5.RowRead;
			if (treeNodeBase is TreeTableNode treeTableNode)
			{
				treeTableNode.Table._loaded = true;
			}
			if (treeNodeBase is TreeDocumentNode treeDocumentNode)
			{
				treeDocumentNode.Document._isLoaded = true;
			}
			if (treeNodeBase is TreeImageNode treeImageNode)
			{
				treeImageNode.Image._isLoaded = true;
			}
			if (treeNodeBase is TreePdfNode treePdfNode)
			{
				treePdfNode.Pdf._isLoaded = true;
			}
			nDic.Add(item5.Id, new TreeNodeSyncTuple
			{
				model = treeNodeBase,
				gid = item5.GroupId,
				pid = item5.ParentId,
				desireIndex = item5.Index
			});
		}
		foreach (var item6 in response["ModNodes"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id")),
			Name = (string)item["Name"],
			Index = (int?)item["Index"],
			Number = (string)item["Number"],
			NeedModGroupId = (item["GroupId"] != null),
			GroupId = new Id64(item.Value<long>("GroupId")),
			NeedModParentId = (item["ParentId"] != null),
			ParentId = Id64.FromNullableLong(item.Value<long?>("ParentId")),
			Permissions = (string)item["Permissions"],
			Visible = (bool?)item["Visible"],
			RowWrite = (bool?)item["RowWrite"],
			RowRead = (bool?)item["RowRead"]
		}))
		{
			result = true;
			if (nDic.TryGetValue(item6.Id, out var value2))
			{
				if (item6.Name != null && !value2.model.IsNameDirty)
				{
					value2.model.Name = item6.Name;
				}
				if (item6.Index.HasValue)
				{
					value2.model.ServerIndex = item6.Index.Value;
					nDic[item6.Id].desireIndex = item6.Index.Value;
				}
				if (item6.Number != null && !value2.model.IsNumberDirty)
				{
					value2.model.Number = item6.Number;
				}
				if (item6.NeedModGroupId && !value2.model.IsGroupDirty)
				{
					value2.gid = item6.GroupId;
				}
				if (item6.NeedModParentId && !value2.model.IsParentDirty)
				{
					value2.pid = item6.ParentId;
				}
				if (item6.Permissions != null && !value2.model.IsPermissionsDirty)
				{
					value2.model.Permissions.Deserialize(item6.Permissions);
				}
				if (item6.Visible.HasValue && !value2.model.IsVisibleDirty)
				{
					value2.model.Visible = item6.Visible.Value;
				}
				if (item6.RowWrite.HasValue && !value2.model.IsRowWriteDirty)
				{
					value2.model.RowWrite = item6.RowWrite.Value;
				}
				if (item6.RowRead.HasValue && !value2.model.IsRowReadDirty)
				{
					value2.model.RowRead = item6.RowRead.Value;
					(value2.model as TreeTableNode).Table._loaded = false;
				}
			}
		}
		HashSet<Id64> hashSet2 = new HashSet<Id64>(response["DelNodes"].Select((JToken item) => new Id64(item.Value<long>("Id"))));
		foreach (Id64 item7 in hashSet2)
		{
			result = true;
			nDic.Remove(item7);
			project.TreeNodesToDelete.Add(item7);
		}
		foreach (TreeNodeSyncTuple item8 in nDic.Values.ToList())
		{
			if (!RootExists(item8))
			{
				nDic.Remove(item8.model.Id);
				project.RemovedTreeNodes.Add(item8.model.Id);
			}
		}
		project._dicTableNodes.Clear();
		foreach (KeyValuePair<Id64, TreeNodeSyncTuple> item9 in nDic)
		{
			if (item9.Value.model is TreeTableNode value3)
			{
				project._dicTableNodes.Add(item9.Key, value3);
			}
		}
		List<TreeNodeSyncTuple> list2 = nDic.Values.OrderBy((TreeNodeSyncTuple tup) => tup.desireIndex).ToList();
		foreach (var item10 in list2.Join(project.TreeGroups, (TreeNodeSyncTuple outer) => outer.gid, (TreeGroup inner) => inner.Id, (TreeNodeSyncTuple tuple, TreeGroup group) => new { tuple, group }))
		{
			item10.tuple.model.Group = item10.group;
			if (!item10.tuple.pid.HasValue)
			{
				item10.group.RootNodes.Add(item10.tuple.model);
				item10.tuple.model.Parent = null;
			}
		}
		foreach (var item11 in list2.Join(list2, (TreeNodeSyncTuple outer) => outer.model.Id, (TreeNodeSyncTuple inner) => inner.pid, (TreeNodeSyncTuple parent, TreeNodeSyncTuple child) => new { parent, child }))
		{
			((TreeDirectoryNode)item11.parent.model).Children.Add(item11.child.model);
			item11.child.model.Parent = (TreeDirectoryNode)item11.parent.model;
		}
		foreach (TreeNodeBase allTreeNode in project.GetAllTreeNodes())
		{
			if (!allTreeNode.AllAncestorsVisible())
			{
				allTreeNode.UpdateVisible(v: false);
			}
		}
		foreach (var item12 in response["NewRefs"].Select((JToken obj) => new
		{
			Id = new Id64(obj.Value<long>("Id")),
			Key = obj.Value<string>("Key"),
			Value = obj.Value<string>("Value"),
			Kind = obj.Value<int>("Kind")
		}))
		{
			if (project.DataReferenceManager.Exists(item12.Key))
			{
				DataReference dataReference = project.DataReferenceManager.Get(item12.Key);
				dataReference.Value = item12.Value;
				dataReference.Kind = (DataReferenceKind)item12.Kind;
				dataReference.SetSynced();
			}
			else
			{
				project.DataReferenceManager._dic.Add(item12.Key, new DataReference
				{
					Id = item12.Id,
					Key = item12.Key,
					Dirty = 0,
					Status = SyncStatus.Synced,
					Value = item12.Value,
					Kind = (DataReferenceKind)item12.Kind
				});
			}
		}
		foreach (var del in response["DelRefs"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id"))
		}))
		{
			DataReference dataReference2 = project.DataReferenceManager.Enumerate().FirstOrDefault((DataReference g) => g.Id == del.Id);
			if (dataReference2 != null)
			{
				project.DataReferenceManager._dic.Remove(dataReference2.Key);
				project.DataReferenceManager._toDelete.Add(dataReference2.Id);
			}
		}
		foreach (var mod in response["ModRefs"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id")),
			Key = item.Value<string>("Key"),
			CellRef = item.Value<string>("CellRef"),
			Value = item.Value<string>("Value")
		}))
		{
			DataReference dataReference3 = project.DataReferenceManager.Enumerate().FirstOrDefault((DataReference g) => g.Id == mod.Id);
			if (dataReference3 != null)
			{
				if (mod.Key != null && !dataReference3.IsKeyDirty)
				{
					dataReference3.Key = mod.Key;
				}
				if (mod.Value != null && !dataReference3.IsValueDirty)
				{
					dataReference3.Value = mod.Value;
				}
			}
		}
		Dictionary<Id64, ValidationFormula> dictionary = project.ValidationManager.Formulas.ToDictionary((ValidationFormula f) => f.Id, (ValidationFormula f) => f);
		foreach (var item13 in response["DelVFs"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id"))
		}))
		{
			if (dictionary.ContainsKey(item13.Id))
			{
				dictionary.Remove(item13.Id);
				project.ValidationManager._toDelete.Add(item13.Id);
			}
			else if (project.ValidationManager._removed.Contains(item13.Id))
			{
				project.ValidationManager._removed.Remove(item13.Id);
				project.ValidationManager._toDelete.Add(item13.Id);
			}
		}
		foreach (var item14 in response["NewVFs"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id")),
			LeftExpr = item.Value<string>("LeftExpr"),
			Op = item.Value<int>("Op"),
			RightExpr = item.Value<string>("RightExpr"),
			Note = item.Value<string>("Note"),
			TableId = new Id64(item.Value<long>("TableId"))
		}))
		{
			dictionary.Add(item14.Id, new ValidationFormula
			{
				Id = item14.Id,
				IsDirty = false,
				LeftExpr = item14.LeftExpr,
				Operator = ValidationOperator.FromCode(item14.Op),
				Note = item14.Note,
				RightExpr = item14.RightExpr,
				TableId = item14.TableId,
				Status = SyncStatus.Synced
			});
		}
		foreach (var item15 in response["ModVFs"].Select((JToken item) => new
		{
			Id = new Id64(item.Value<long>("Id")),
			LeftExpr = item.Value<string>("LeftExpr"),
			Op = item.Value<int>("Op"),
			RightExpr = item.Value<string>("RightExpr"),
			Note = item.Value<string>("Note"),
			TableId = new Id64(item.Value<long>("TableId"))
		}))
		{
			if (dictionary.TryGetValue(item15.Id, out var value4) && !value4.IsDirty)
			{
				value4.LeftExpr = item15.LeftExpr;
				value4.Operator = ValidationOperator.FromCode(item15.Op);
				value4.RightExpr = item15.RightExpr;
				value4.Note = item15.Note;
				value4.TableId = item15.TableId;
				value4.Status = SyncStatus.Synced;
			}
		}
		foreach (ValidationFormula item16 in dictionary.Values.ToList())
		{
			if (!nDic.ContainsKey(item16.TableId))
			{
				dictionary.Remove(item16.Id);
				project.ValidationManager._toDelete.Add(item16.Id);
			}
		}
		project.ValidationManager.Formulas.Clear();
		project.ValidationManager.Formulas.AddRange(dictionary.Values);
		project._treeNodeFormulaNamesExpired = true;
		return result;
		static void ClearNodeChildren(TreeNodeBase n)
		{
			if (n is TreeDirectoryNode treeDirectoryNode)
			{
				foreach (TreeNodeBase child in treeDirectoryNode.Children)
				{
					ClearNodeChildren(child);
				}
				treeDirectoryNode.Children.Clear();
			}
		}
		bool RootExists(TreeNodeSyncTuple tup)
		{
			if (tup.pid.HasValue)
			{
				if (nDic.TryGetValue(tup.pid.Value, out var value5))
				{
					return RootExists(value5);
				}
				return false;
			}
			return gDic.ContainsKey(tup.gid);
		}
	}

	public static void Merge(PullDocument response, Document document, TaskProgressValueReportCallback reportCallback = null)
	{
		TaskProgressValueUpdater taskProgressValueUpdater = new TaskProgressValueUpdater(0f, 1f, reportCallback);
		document.LoadAndReturn();
		if (response.Locker != null && !document.Dirty.IsLockerDirty)
		{
			document.Locker = response.Locker.Value;
		}
		if (response.SectPr != null && !document.Dirty.IsSectPrDirty)
		{
			document.SectPr = response.SectPr.Value;
		}
		if (response.MergeTable != null && !document.Dirty.IsMergeTableDirty)
		{
			document.MergeTable = new Id64(document.MergeTable.Value);
		}
		Dictionary<Id64, Paragraph> dictionary = document.Paragraphs.ToDictionary((Paragraph p) => p.Id, (Paragraph p) => p);
		List<Paragraph> list = document.Paragraphs.ToList();
		int num = response.NewParagraphs.Count + response.ModParagraphs.Count + response.DelParagraphs.Count;
		int num2 = 0;
		foreach (var item in response.NewParagraphs.Select((PullParagraph paragraph) => new
		{
			Id = new Id64(paragraph.Id),
			Stream = paragraph.Stream.GetBytes(),
			Index = paragraph.Index.Value,
			Section = ((paragraph.Section == null || paragraph.Section.IsNull) ? null : paragraph.Section.Value.ToByteArray()),
			Comment = paragraph.Comment.Value
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			Paragraph value = new Paragraph
			{
				Id = item.Id,
				Dirty = default(ParagraphDirtyMask),
				Document = document,
				ServerIndex = item.Index,
				Status = SyncStatus.Synced,
				Stream = Encoding.UTF8.GetString(ZipCompressor.Decompress(item.Stream)),
				Section = ((item.Section == null) ? null : Encoding.UTF8.GetString(ZipCompressor.Decompress(item.Section))),
				Comment = item.Comment
			};
			dictionary[item.Id] = value;
		}
		foreach (var item2 in response.DelParagraphs.Select((PullParagraph item) => new
		{
			Id = new Id64(item.Id)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			document.ParagraphsToDelete.Add(item2.Id);
			document.RemovedParagraphs.Remove(item2.Id);
			dictionary.Remove(item2.Id);
		}
		foreach (var item3 in response.ModParagraphs.Select((PullParagraph item) => new
		{
			Id = new Id64(item.Id),
			Stream = ((item.Stream == null) ? null : item.Stream.GetBytes()),
			Index = ((item.Index == null) ? null : new int?(item.Index.Value)),
			Section = ((item.Section == null || item.Section.IsNull) ? null : item.Section.Value.ToByteArray()),
			Comment = ((item.Comment == null) ? null : item.Comment.Value),
			HasComment = (item.Comment != null)
		}))
		{
			taskProgressValueUpdater.UpdateProgress(num2++, num);
			if (dictionary.TryGetValue(item3.Id, out var value2))
			{
				if (item3.Stream != null && !value2.Dirty.IsStreamDirty)
				{
					value2.Stream = Encoding.UTF8.GetString(ZipCompressor.Decompress(item3.Stream));
					value2.Section = ((item3.Section == null) ? null : Encoding.UTF8.GetString(ZipCompressor.Decompress(item3.Section)));
				}
				if (item3.Index.HasValue)
				{
					value2.ServerIndex = item3.Index.Value;
				}
				if (item3.HasComment)
				{
					value2.Comment = item3.Comment;
				}
			}
		}
		document.Paragraphs.Clear();
		foreach (Paragraph item4 in from p in dictionary.Values
			where p.Status != SyncStatus.New
			orderby p.ServerIndex
			select p)
		{
			document.Paragraphs.Add(item4);
		}
		foreach (Paragraph item5 in from p in dictionary.Values
			where p.Status == SyncStatus.New
			orderby p.Index
			select p)
		{
			int index = list[item5.Index - 1].Index + 1;
			document.Paragraphs.Insert(index, item5);
		}
	}

	private static void Merge(JObject response, Image image)
	{
		image.LoadAndReturn();
		if (response.TryGetValue("FileId", out var value) && !image.IsFileIdDirty)
		{
			image.FileId = (Guid)value;
		}
		if (response.TryGetValue("CenterX", out var value2) && response.TryGetValue("CenterY", out var value3) && !image.IsCenterDirty)
		{
			image.Center = new PointF((float)value2, (float)value3);
		}
		if (response.TryGetValue("ZoomFactor", out var value4) && !image.IsZoomFactorDirty)
		{
			image.ZoomFactor = (float)value4;
		}
		if (response.TryGetValue("PageSetup", out var value5) && !image.IsPageSetupDirty)
		{
			image.PageSetup.Deserialize((string)value5);
		}
		if (response.TryGetValue("RotateFlip", out var value6) && !image.IsRotateFlipDirty)
		{
			image.RotateFlip = (RotateFlipType)(int)value6;
		}
	}
}
