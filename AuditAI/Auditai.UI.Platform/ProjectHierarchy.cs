﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using MessageBox = Auditai.UI.Controls.MessageBox;
using Table = Auditai.Model.Table;

using C1FlexGridEx = Auditai.UI.Controls.C1FlexGridEx;
using SDImage = System.Drawing.Image;
using TreeGroup = Auditai.Model.TreeGroup;

namespace Auditai.UI.Platform;

public class ProjectHierarchy
{
    private C1FlexGridEx _grid;

    // 分组右键菜单命令
    private C1Command cmdMoveUpGroup = new C1Command();
    private C1Command cmdMoveDownGroup = new C1Command();
    private C1Command cmdAddGroup = new C1Command();
    private C1Command cmdRemoveGroup = new C1Command();
    private C1Command cmdCopyGroup = new C1Command();
    private C1Command cmdPasteGroupClickOnGridTree = new C1Command();
    private C1Command cmdPasteGroup = new C1Command();
    private C1Command cmdRenameGroup = new C1Command();

    // 节点右键菜单命令
    private C1Command cmdInsertDirectory = new C1Command();
    private C1Command cmdInsertTable = new C1Command();
    private C1Command cmdInsertDocument = new C1Command();
    private C1Command cmdInsertImage = new C1Command();
    private C1Command cmdInsertPdf = new C1Command();
    private C1Command cmdAppendChildDirectory = new C1Command();
    private C1Command cmdAppendChildTable = new C1Command();
    private C1Command cmdAppendChildDocument = new C1Command();
    private C1Command cmdAppendChildImage = new C1Command();
    private C1Command cmdAppendChildPdf = new C1Command();
    private C1Command cmdMoveUpNode = new C1Command();
    private C1Command cmdMoveDownNode = new C1Command();
    private C1Command cmdRemoveNode = new C1Command();
    private C1Command cmdHideNode = new C1Command();
    private C1Command cmdShowNodes = new C1Command();
    private C1Command cmdSearchNodes = new C1Command();
    private C1Command cmdCutNode = new C1Command();
    private C1Command cmdCopy = new C1Command();
    private C1Command cmdPasteNode = new C1Command();
    private C1Command cmdRenameNode = new C1Command();
    private C1Command cmdEditNumber = new C1Command();
    private C1Command cmdReload = new C1Command();
    private C1Command cmdSyncTable = new C1Command();
    private C1Command cmdSyncDocument = new C1Command();
    private C1Command cmdNodeImportFile = new C1Command();
    private C1Command cmdNodeImportExcel = new C1Command();
    private C1Command cmdNodeImportWord = new C1Command();
    private C1Command cmdNodeImportImage = new C1Command();
    private C1Command cmdNodeImportPdf = new C1Command();
    private C1Command cmdNodeImportFolder = new C1Command();

    // 批量操作命令
    private C1Command cmdBatchHideFile = new C1Command();
    private C1Command cmdBatchUnhideFile = new C1Command();
    private C1Command cmdBatchDeleteFile = new C1Command();
    private C1Command cmdBatchEditIndex = new C1Command();
    private C1Command cmdBatchExportFile = new C1Command();
    private C1Command cmdBatchPrintFile = new C1Command();

    // 空白区域右键菜单命令
    private C1Command cmdAppendRootDirectory = new C1Command();
    private C1Command cmdAppendRootTable = new C1Command();
    private C1Command cmdAppendRootDocument = new C1Command();
    private C1Command cmdAppendRootImage = new C1Command();
    private C1Command cmdAppendRootPdf = new C1Command();
    private C1Command cmdPasteRootNode = new C1Command();
    private C1Command cmdEmptyImportFile = new C1Command();
    private C1Command cmdEmptyImportExcel = new C1Command();
    private C1Command cmdEmptyImportWord = new C1Command();
    private C1Command cmdEmptyImportImage = new C1Command();
    private C1Command cmdEmptyImportPdf = new C1Command();
    private C1Command cmdEmptyImportFolder = new C1Command();

    // 命令链接
    private C1CommandLink lnkMoveUpGroup = new C1CommandLink();
    private C1CommandLink lnkMoveDownGroup = new C1CommandLink();
    private C1CommandLink lnkAddGroup = new C1CommandLink();
    private C1CommandLink lnkAddGroup2 = new C1CommandLink();
    private C1CommandLink lnkRemoveGroup = new C1CommandLink();
    private C1CommandLink lnkCopyGroup = new C1CommandLink();
    private C1CommandLink lnkPasteGroup = new C1CommandLink();
    private C1CommandLink lnkPasteGroup2 = new C1CommandLink();
    private C1CommandLink lnkPasteGroup3 = new C1CommandLink();
    private C1CommandLink lnkPasteGroup4 = new C1CommandLink();
    private C1CommandLink lnkRenameGroup = new C1CommandLink();
    private C1CommandLink lnkInsertDirectory = new C1CommandLink();
    private C1CommandLink lnkInsertTable = new C1CommandLink();
    private C1CommandLink lnkInsertDocument = new C1CommandLink();
    private C1CommandLink lnkInsertImage = new C1CommandLink();
    private C1CommandLink lnkInsertPdf = new C1CommandLink();
    private C1CommandLink lnkAppendChildDirectory = new C1CommandLink();
    private C1CommandLink lnkAppendChildTable = new C1CommandLink();
    private C1CommandLink lnkAppendChildDocument = new C1CommandLink();
    private C1CommandLink lnkAppendChildImage = new C1CommandLink();
    private C1CommandLink lnkAppendChildPdf = new C1CommandLink();
    private C1CommandLink lnkMoveUpNode = new C1CommandLink();
    private C1CommandLink lnkMoveDownNode = new C1CommandLink();
    private C1CommandLink lnkRemoveNode = new C1CommandLink();
    private C1CommandLink lnkHideNode = new C1CommandLink();
    private C1CommandLink lnkShowNodes = new C1CommandLink();
    private C1CommandLink lnkShowNodes2 = new C1CommandLink();
    private C1CommandLink lnkSearchNodes = new C1CommandLink();
    private C1CommandLink lnkSearchNodes2 = new C1CommandLink();
    private C1CommandLink lnkCutNode = new C1CommandLink();
    private C1CommandLink lnkRenameNode = new C1CommandLink();
    private C1CommandLink lnkEditNumber = new C1CommandLink();
    private C1CommandLink lnkReload = new C1CommandLink();
    private C1CommandLink lnkSyncTable = new C1CommandLink();
    private C1CommandLink lnkSyncDocument = new C1CommandLink();
    private C1CommandLink lnkNodeImportFile = new C1CommandLink();
    private C1CommandLink lnkNodeImportExcel = new C1CommandLink();
    private C1CommandLink lnkNodeImportWord = new C1CommandLink();
    private C1CommandLink lnkNodeImportImage = new C1CommandLink();
    private C1CommandLink lnkNodeImportPdf = new C1CommandLink();
    private C1CommandLink lnkNodeImportFolder = new C1CommandLink();
    private C1CommandLink lnkBatchOperation = new C1CommandLink();
    private C1CommandLink lnkBatchOperation2 = new C1CommandLink();
    private C1CommandLink lnkBatchHideFile = new C1CommandLink();
    private C1CommandLink lnkBatchUnhideFile = new C1CommandLink();
    private C1CommandLink lnkBatchDeleteFile = new C1CommandLink();
    private C1CommandLink lnkBatchEditIndex = new C1CommandLink();
    private C1CommandLink lnkBatchExportFile = new C1CommandLink();
    private C1CommandLink lnkBatchPrintFile = new C1CommandLink();
    private C1CommandLink lnkPushNode = new C1CommandLink();
    private C1CommandLink lnkAppendRootDirectory = new C1CommandLink();
    private C1CommandLink lnkAppendRootTable = new C1CommandLink();
    private C1CommandLink lnkAppendRootDocument = new C1CommandLink();
    private C1CommandLink lnkAppendRootImage = new C1CommandLink();
    private C1CommandLink lnkAppendRootPdf = new C1CommandLink();
    private C1CommandLink lnkPasteRootNode = new C1CommandLink();
    private C1CommandLink lnkEmptyImportFile = new C1CommandLink();
    private C1CommandLink lnkEmptyImportExcel = new C1CommandLink();
    private C1CommandLink lnkEmptyImportWord = new C1CommandLink();
    private C1CommandLink lnkEmptyImportImage = new C1CommandLink();
    private C1CommandLink lnkEmptyImportPdf = new C1CommandLink();
    private C1CommandLink lnkEmptyImportFolder = new C1CommandLink();

    // 上下文菜单
    private C1ContextMenu ctxTreeGroup = new C1ContextMenu();
    private C1ContextMenu ctxTreeNode = new C1ContextMenu();
    private C1ContextMenu ctxTreeEmpty = new C1ContextMenu();
    private C1ContextMenu ctxTreeNothing = new C1ContextMenu();
    private C1ContextMenu ctxBatchOperation = new C1ContextMenu();
    private C1ContextMenu ctxProjectMember = new C1ContextMenu();

    // 子菜单
    private C1CommandMenu mnuInsert = new C1CommandMenu();
    private C1CommandMenu mnuAppendChild = new C1CommandMenu();
    private C1CommandMenu mnuNodeImport = new C1CommandMenu();
    private C1CommandMenu mnuAppendRoot = new C1CommandMenu();
    private C1CommandMenu mnuEmptyImport = new C1CommandMenu();

    // 其他字段
    private List<TreeGroupView> _groups;
    private frmSearch frmSearch;
    private LazyExcute lazySearchExcute = new LazyExcute();
    private TreeNodeBase firstImportNode;
    private Dictionary<Id64, Table> _dicDupFormula = new Dictionary<Id64, Table>();
    public ProjectImport ImportProject;

    public class TreeGroupView
    {
        public C1FlexGridBase Grid { get; set; }
        public object Page { get; set; }
        public TreeGroup Model { get; set; }
        public SDImage GetTreeNodeIcon(TreeNodeBase node)
        {
            if (node is TreeDirectoryNode) return Resources.TreeDir;
            if (node is TreeDocumentNode) return Resources.TreeDoc;
            if (node is TreeTableNode) return Resources.TreeTable;
            if (node is TreeImageNode) return Resources.TreeDoc;
            if (node is TreePdfNode) return Resources.TreeDoc;
            throw new ArgumentOutOfRangeException();
        }
        public void PopulateDirectoryNode(TreeDirectoryNode dirNode, Node gridNode)
        {
        }
    }

    public C1OutBarEx View { get; private set; }
    public dynamic SelectedNode { get; set; }
    public TreeGroupView _currentGroup { get; set; }
    public bool IsInOpeningSomeTreeNode { get; set; }
    public bool NumberShown { get; set; }
    public Auditai.Model.Project Project { get; set; }

    public event EventHandler<C1.Win.C1FlexGrid.Row> TreeNodeCollapsed;

    public enum CutCopyModeEnum
    {
        None = 0,
        Cut = 1,
        Copy = 2
    }

    public CutCopyModeEnum _cutCopyMode { get; set; }
    public CutCopyModeEnum CutCopyMode { get; set; }

    public event EventHandler TreeNodeSelected;

    public ProjectHierarchy()
    {
        _grid = new C1FlexGridEx
        {
            Dock = DockStyle.Fill,
            AllowEditing = false,
            ExtendLastCol = true,
            BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
            SelectionMode = SelectionModeEnum.Cell
        };
        _grid.Styles.Normal.Border.Width = 0;
        _grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
        _grid.Styles.Alternate.Border.Width = 0;
        _grid.Styles.Alternate.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
        _grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
        _grid.DrawMode = DrawModeEnum.Normal;
        _grid.Tree.Style = TreeStyleFlags.Symbols | TreeStyleFlags.ButtonBar;
        _grid.Rows.DefaultSize = 30;
        _grid.Rows.Count = 0;
        _grid.Rows.Fixed = 1;
        _grid.Cols.Count = 1;
        _grid.Cols.Fixed = 0;
        _grid.Tree.Column = 0;
        _grid.Cols[0].Width = 200;
        _grid.MouseClick += _grid_MouseClick;
        _grid.MouseDoubleClick += _grid_MouseDoubleClick;

        Initialize();
    }

    private void Initialize()
    {
        NumberShown = UserSet.Config.ShowNumber;

        var outBar = new C1OutBarEx
        {
            Dock = DockStyle.Fill,
            ShowScrollButtons = false
        };
        outBar.MouseClick += View_MouseClick;

        var page = new C1OutPage();
        page.Controls.Add(_grid);
        outBar.Pages.Add(page);
        var groupView = new TreeGroupView { Grid = _grid, Page = page };
        page.Tag = groupView;
        _currentGroup = groupView;
        View = outBar;

        SecondTrigger.Trigger.Tick += Trigger_Tick;

        // ---- 设置所有命令的 Text 和 Image ----
        cmdMoveUpGroup.Text = "上移分组";
        cmdMoveDownGroup.Text = "下移分组";
        cmdAddGroup.Text = "新建分组";
        cmdRemoveGroup.Text = "删除分组";
        cmdCopyGroup.Text = "复制分组";
        cmdRenameGroup.Text = "重命名分组";
        cmdRenameGroup.Image = Auditai.UI.Platform.Properties.ContextResources.ctxMofify;
        cmdPasteGroup.Text = "粘贴分组";
        cmdPasteGroupClickOnGridTree.Text = "粘贴分组";

        cmdInsertDirectory.Text = "新建文件夹";
        cmdInsertTable.Text = "新建表格";
        cmdInsertTable.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertTable;
        cmdInsertDocument.Text = "新建文档";
        cmdInsertImage.Text = "新建图片";
        cmdInsertImage.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertImage;
        cmdInsertPdf.Text = "新建PDF";

        cmdAppendChildDirectory.Text = "追加文件夹";
        cmdAppendChildTable.Text = "追加表格";
        cmdAppendChildDocument.Text = "追加文档";
        cmdAppendChildImage.Text = "追加图片";
        cmdAppendChildPdf.Text = "追加PDF";

        cmdMoveUpNode.Text = "上移";
        cmdMoveDownNode.Text = "下移";
        cmdRemoveNode.Text = "删除";
        cmdRemoveNode.Image = Auditai.UI.Platform.Properties.ContextResources.ctxDelete;
        cmdHideNode.Text = "隐藏";

        cmdShowNodes.Text = "取消隐藏";
        cmdShowNodes.Image = Auditai.UI.Platform.Properties.ContextResources.ctxSearch;
        cmdSearchNodes.Text = "搜索节点";
        cmdSearchNodes.Image = Auditai.UI.Platform.Properties.ContextResources.ctxSearch;

        cmdCutNode.Text = "剪切";
        cmdCutNode.Image = Auditai.UI.Platform.Properties.ContextResources.ctxCut;

        cmdCopy.Text = "复制";
        cmdCopy.Image = Auditai.UI.Platform.Properties.ContextResources.ctxCopy;

        cmdPasteNode.Text = "粘贴";
        cmdPasteNode.Image = Auditai.UI.Platform.Properties.ContextResources.ctxPaste;

        cmdRenameNode.Text = "重命名";
        cmdRenameNode.Image = Auditai.UI.Platform.Properties.ContextResources.ctxMofify;
        cmdEditNumber.Text = "编辑索引号";
        cmdEditNumber.Image = Auditai.UI.Platform.Properties.ContextResources.ctxNumber;
        cmdReload.Text = "重新加载";
        cmdReload.Image = Auditai.UI.Platform.Properties.ContextResources.ctxReloadFile;
        cmdSyncTable.Text = "同步表格";
        cmdSyncTable.Image = Auditai.UI.Platform.Properties.ContextResources.ctxRefreshTable;
        cmdSyncDocument.Text = "同步文档";
        cmdSyncDocument.Image = Auditai.UI.Platform.Properties.ContextResources.ctxRefresh;

        cmdNodeImportFile.Text = "导入文件";
        cmdNodeImportFile.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdNodeImportExcel.Text = "导入Excel";
        cmdNodeImportExcel.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdNodeImportWord.Text = "导入Word";
        cmdNodeImportWord.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdNodeImportImage.Text = "导入图片";
        cmdNodeImportImage.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdNodeImportPdf.Text = "导入PDF";
        cmdNodeImportPdf.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdNodeImportFolder.Text = "导入文件夹";
        cmdNodeImportFolder.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;

        cmdAppendRootDirectory.Text = "新建文件夹";
        cmdAppendRootTable.Text = "新建表格";
        cmdAppendRootTable.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertTable;
        cmdAppendRootDocument.Text = "新建文档";
        cmdAppendRootImage.Text = "新建图片";
        cmdAppendRootImage.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertImage;
        cmdAppendRootPdf.Text = "新建PDF";

        cmdPasteRootNode.Text = "粘贴";
        cmdPasteRootNode.Image = Auditai.UI.Platform.Properties.ContextResources.ctxPaste;

        cmdEmptyImportFile.Text = "导入文件";
        cmdEmptyImportFile.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdEmptyImportExcel.Text = "导入Excel";
        cmdEmptyImportExcel.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdEmptyImportWord.Text = "导入Word";
        cmdEmptyImportWord.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdEmptyImportImage.Text = "导入图片";
        cmdEmptyImportImage.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdEmptyImportPdf.Text = "导入PDF";
        cmdEmptyImportPdf.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        cmdEmptyImportFolder.Text = "导入文件夹";
        cmdEmptyImportFolder.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;

        // ---- 补充缺失的图标 ----
        cmdMoveUpGroup.Image = Auditai.UI.Platform.Properties.Resources.MoveUp;
        cmdMoveDownGroup.Image = Auditai.UI.Platform.Properties.Resources.MoveDown;
        cmdRemoveGroup.Image = Auditai.UI.Platform.Properties.ContextResources.ctxDelete;
        cmdCopyGroup.Image = Auditai.UI.Platform.Properties.ContextResources.ctxCopy;

        cmdMoveUpNode.Image = Auditai.UI.Platform.Properties.Resources.MoveUp;
        cmdMoveDownNode.Image = Auditai.UI.Platform.Properties.Resources.MoveDown;
        cmdHideNode.Image = Auditai.UI.Platform.Properties.Resources.HideNodes;

        cmdInsertDirectory.Image = Auditai.UI.Platform.Properties.Resources.TreeDir;
        cmdInsertDocument.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;
        cmdInsertPdf.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;

        cmdAppendChildDirectory.Image = Auditai.UI.Platform.Properties.Resources.TreeDir;
        cmdAppendChildTable.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertTable;
        cmdAppendChildDocument.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;
        cmdAppendChildImage.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertImage;
        cmdAppendChildPdf.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;

        cmdAppendRootDirectory.Image = Auditai.UI.Platform.Properties.Resources.TreeDir;
        cmdAppendRootDocument.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;
        cmdAppendRootPdf.Image = Auditai.UI.Platform.Properties.Resources.TreeDoc;

        // ---- 补充缺失的命令图标 ----
        cmdAddGroup.Image = Auditai.UI.Platform.Properties.ContextResources.ctxAppendRow;
        cmdPasteGroup.Image = Auditai.UI.Platform.Properties.ContextResources.ctxPaste;
        cmdPasteGroupClickOnGridTree.Image = Auditai.UI.Platform.Properties.ContextResources.ctxPaste;

        // ---- 设置子菜单 ----
        mnuInsert.Text = "新建";
        mnuInsert.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertTable;
        mnuAppendChild.Text = "追加";
        mnuAppendChild.Image = Auditai.UI.Platform.Properties.ContextResources.ctxAppendRow;
        mnuNodeImport.Text = "导入";
        mnuNodeImport.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;
        mnuAppendRoot.Text = "新建";
        mnuAppendRoot.Image = Auditai.UI.Platform.Properties.ContextResources.ctxInsertTable;
        mnuEmptyImport.Text = "导入";
        mnuEmptyImport.Image = Auditai.UI.Platform.Properties.ContextResources.ctxImport;

        // 配置 新建 子菜单
        mnuInsert.CommandLinks.Add(lnkInsertDirectory);
        mnuInsert.CommandLinks.Add(lnkInsertTable);
        mnuInsert.CommandLinks.Add(lnkInsertDocument);
        mnuInsert.CommandLinks.Add(lnkInsertImage);
        mnuInsert.CommandLinks.Add(lnkInsertPdf);

        // 配置 追加 子菜单
        mnuAppendChild.CommandLinks.Add(lnkAppendChildDirectory);
        mnuAppendChild.CommandLinks.Add(lnkAppendChildTable);
        mnuAppendChild.CommandLinks.Add(lnkAppendChildDocument);
        mnuAppendChild.CommandLinks.Add(lnkAppendChildImage);
        mnuAppendChild.CommandLinks.Add(lnkAppendChildPdf);

        // 配置 导入 子菜单
        mnuNodeImport.CommandLinks.Add(lnkNodeImportFile);
        mnuNodeImport.CommandLinks.Add(lnkNodeImportExcel);
        mnuNodeImport.CommandLinks.Add(lnkNodeImportWord);
        mnuNodeImport.CommandLinks.Add(lnkNodeImportImage);
        mnuNodeImport.CommandLinks.Add(lnkNodeImportPdf);
        mnuNodeImport.CommandLinks.Add(lnkNodeImportFolder);

        // 配置 空白区域-新建 子菜单
        mnuAppendRoot.CommandLinks.Add(lnkAppendRootDirectory);
        mnuAppendRoot.CommandLinks.Add(lnkAppendRootTable);
        mnuAppendRoot.CommandLinks.Add(lnkAppendRootDocument);
        mnuAppendRoot.CommandLinks.Add(lnkAppendRootImage);
        mnuAppendRoot.CommandLinks.Add(lnkAppendRootPdf);

        // 配置 空白区域-导入 子菜单
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportFile);
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportExcel);
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportWord);
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportImage);
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportPdf);
        mnuEmptyImport.CommandLinks.Add(lnkEmptyImportFolder);

        // cmdMoveUpGroup
        cmdMoveUpGroup.CommandStateQuery += CmdMoveUpGroup_CommandStateQuery;
        cmdMoveUpGroup.Click += CmdMoveUpGroup_Click;
        lnkMoveUpGroup.Command = cmdMoveUpGroup;
        ctxTreeGroup.CommandLinks.Add(lnkMoveUpGroup);

        // cmdMoveDownGroup
        cmdMoveDownGroup.CommandStateQuery += CmdMoveDownGroup_CommandStateQuery;
        cmdMoveDownGroup.Click += CmdMoveDownGroup_Click;
        lnkMoveDownGroup.Command = cmdMoveDownGroup;
        ctxTreeGroup.CommandLinks.Add(lnkMoveDownGroup);

        // cmdAddGroup
        cmdAddGroup.CommandStateQuery += CmdAddGroup_CommandStateQuery;
        cmdAddGroup.Click += CmdAddGroup_Click;
        lnkAddGroup.Command = cmdAddGroup;
        ctxTreeGroup.CommandLinks.Add(lnkAddGroup);

        // lnkAddGroup2 -> cmdAddGroup (ctxTreeNothing)
        lnkAddGroup2.Command = cmdAddGroup;
        ctxTreeNothing.CommandLinks.Add(lnkAddGroup2);

        // cmdRemoveGroup
        cmdRemoveGroup.CommandStateQuery += CmdRemoveGroup_CommandStateQuery;
        cmdRemoveGroup.Click += CmdRemoveGroup_Click;
        lnkRemoveGroup.Command = cmdRemoveGroup;
        ctxTreeGroup.CommandLinks.Add(lnkRemoveGroup);

        // cmdCopyGroup
        cmdCopyGroup.CommandStateQuery += CmdCopyGroup_CommandStateQuery;
        cmdCopyGroup.Click += CmdCopyGroup_Click;
        lnkCopyGroup.Command = cmdCopyGroup;
        lnkCopyGroup.Delimiter = true;
        ctxTreeGroup.CommandLinks.Add(lnkCopyGroup);

        // cmdPasteGroupClickOnGridTree
        cmdPasteGroupClickOnGridTree.CommandStateQuery += CmdPasteGroupClickOnGridTree_CommandStateQuery;
        cmdPasteGroupClickOnGridTree.Click += CmdPasteGroupClickOnGridTree_Click;

        // cmdPasteGroup
        cmdPasteGroup.CommandStateQuery += CmdPasteGroup_CommandStateQuery;
        cmdPasteGroup.Click += CmdPasteGroup_Click;
        lnkPasteGroup.Command = cmdPasteGroup;
        ctxTreeGroup.CommandLinks.Add(lnkPasteGroup);

        // lnkPasteGroup2 -> cmdPasteGroup (ctxTreeNothing)
        lnkPasteGroup2.Command = cmdPasteGroup;
        ctxTreeNothing.CommandLinks.Add(lnkPasteGroup2);

        // cmdRenameGroup
        cmdRenameGroup.CommandStateQuery += CmdRenameGroup_CommandStateQuery;
        cmdRenameGroup.Click += CmdRenameGroup_Click;
        lnkRenameGroup.Command = cmdRenameGroup;
        ctxTreeGroup.CommandLinks.Add(lnkRenameGroup);

        lnkAddGroup.Delimiter = true;
        lnkRenameGroup.Delimiter = true;

        // cmdInsertDirectory
        cmdInsertDirectory.CommandStateQuery += CmdInsertDirectory_CommandStateQuery;
        cmdInsertDirectory.Click += CmdInsertDirectory_Click;
        lnkInsertDirectory.Command = cmdInsertDirectory;

        // cmdInsertTable
        cmdInsertTable.CommandStateQuery += CmdInsertTable_CommandStateQuery;
        cmdInsertTable.Click += CmdInsertTable_Click;
        lnkInsertTable.Command = cmdInsertTable;

        // cmdInsertDocument
        cmdInsertDocument.CommandStateQuery += CmdInsertDocument_CommandStateQuery;
        cmdInsertDocument.Click += CmdInsertDocument_Click;
        lnkInsertDocument.Command = cmdInsertDocument;

        // cmdInsertImage
        cmdInsertImage.CommandStateQuery += CmdInsertImage_CommandStateQuery;
        cmdInsertImage.Click += CmdInsertImage_Click;
        lnkInsertImage.Command = cmdInsertImage;

        // cmdInsertPdf
        cmdInsertPdf.CommandStateQuery += CmdInsertPdf_CommandStateQuery;
        cmdInsertPdf.Click += CmdInsertPdf_Click;
        lnkInsertPdf.Command = cmdInsertPdf;

        // 添加 新建 子菜单到节点菜单
        var lnkInsert = new C1CommandLink();
        lnkInsert.Command = mnuInsert;
        ctxTreeNode.CommandLinks.Add(lnkInsert);

        // cmdAppendChildDirectory
        cmdAppendChildDirectory.CommandStateQuery += CmdAppendChildDirectory_CommandStateQuery;
        cmdAppendChildDirectory.Click += CmdAppendChildDirectory_Click;
        lnkAppendChildDirectory.Command = cmdAppendChildDirectory;

        // cmdAppendChildTable
        cmdAppendChildTable.CommandStateQuery += CmdAppendChildTable_CommandStateQuery;
        cmdAppendChildTable.Click += CmdAppendChildTable_Click;
        lnkAppendChildTable.Command = cmdAppendChildTable;

        // cmdAppendChildDocument
        cmdAppendChildDocument.CommandStateQuery += CmdAppendChildDocument_CommandStateQuery;
        cmdAppendChildDocument.Click += CmdAppendChildDocument_Click;
        lnkAppendChildDocument.Command = cmdAppendChildDocument;

        // cmdAppendChildImage
        cmdAppendChildImage.CommandStateQuery += CmdAppendChildImage_CommandStateQuery;
        cmdAppendChildImage.Click += CmdAppendChildImage_Click;
        lnkAppendChildImage.Command = cmdAppendChildImage;

        // cmdAppendChildPdf
        cmdAppendChildPdf.CommandStateQuery += CmdAppendChildPdf_CommandStateQuery;
        cmdAppendChildPdf.Click += CmdAppendChildPdf_Click;
        lnkAppendChildPdf.Command = cmdAppendChildPdf;

        // 添加 追加 子菜单到节点菜单
        var lnkAppend = new C1CommandLink();
        lnkAppend.Command = mnuAppendChild;
        ctxTreeNode.CommandLinks.Add(lnkAppend);

        // cmdMoveUpNode
        cmdMoveUpNode.CommandStateQuery += CmdMoveUpNode_CommandStateQuery;
        cmdMoveUpNode.Click += CmdMoveUpNode_Click;
        lnkMoveUpNode.Command = cmdMoveUpNode;

        // cmdMoveDownNode
        cmdMoveDownNode.CommandStateQuery += CmdMoveDownNode_CommandStateQuery;
        cmdMoveDownNode.Click += CmdMoveDownNode_Click;
        lnkMoveDownNode.Command = cmdMoveDownNode;

        // cmdRemoveNode
        cmdRemoveNode.CommandStateQuery += CmdRemoveNode_CommandStateQuery;
        cmdRemoveNode.Click += CmdRemoveNode_Click;
        lnkRemoveNode.Command = cmdRemoveNode;
        ctxTreeNode.CommandLinks.Add(lnkRemoveNode);

        // cmdHideNode
        cmdHideNode.CommandStateQuery += CmdHideNode_CommandStateQuery;
        cmdHideNode.Click += CmdHideNode_Click;
        lnkHideNode.Command = cmdHideNode;
        ctxTreeNode.CommandLinks.Add(lnkHideNode);

        // cmdShowNodes
        cmdShowNodes.CommandStateQuery += CmdShowNodes_CommandStateQuery;
        cmdShowNodes.Click += CmdShowNodes_Click;
        lnkShowNodes.Command = cmdShowNodes;
        ctxTreeNode.CommandLinks.Add(lnkShowNodes);

        // cmdSearchNodes
        cmdSearchNodes.CommandStateQuery += CmdSearchNodes_CommandStateQuery;
        cmdSearchNodes.Click += CmdSearchNodes_Click;
        lnkSearchNodes.Command = cmdSearchNodes;
        ctxTreeNode.CommandLinks.Add(lnkSearchNodes);

        // cmdCutNode
        cmdCutNode.CommandStateQuery += CmdCutNode_CommandStateQuery;
        cmdCutNode.Click += CmdCutNode_Click;
        lnkCutNode.Command = cmdCutNode;
        ctxTreeNode.CommandLinks.Add(lnkCutNode);

        // cmdCopy（统一复制命令
        cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
        cmdCopy.Click += CmdCopy_Click;
        var lnkCopy = new C1CommandLink();
        lnkCopy.Command = cmdCopy;
        ctxTreeNode.CommandLinks.Add(lnkCopy);

        // cmdPasteNode（统一粘贴命令）
        cmdPasteNode.CommandStateQuery += CmdPasteNode_CommandStateQuery;
        cmdPasteNode.Click += CmdPasteNode_Click;
        var lnkPaste = new C1CommandLink();
        lnkPaste.Command = cmdPasteNode;
        lnkPaste.Delimiter = true;
        ctxTreeNode.CommandLinks.Add(lnkPaste);

        // lnkPasteGroup3 -> cmdPasteGroupClickOnGridTree (ctxTreeNode)
        lnkPasteGroup3.Command = cmdPasteGroupClickOnGridTree;
        ctxTreeNode.CommandLinks.Add(lnkPasteGroup3);

        // cmdRenameNode
        cmdRenameNode.CommandStateQuery += CmdRenameNode_CommandStateQuery;
        cmdRenameNode.Click += CmdRenameNode_Click;
        lnkRenameNode.Command = cmdRenameNode;
        ctxTreeNode.CommandLinks.Add(lnkRenameNode);

        // cmdEditNumber
        cmdEditNumber.CommandStateQuery += CmdEditNumber_CommandStateQuery;
        cmdEditNumber.Click += CmdEditNumber_Click;
        lnkEditNumber.Command = cmdEditNumber;
        ctxTreeNode.CommandLinks.Add(lnkEditNumber);

        // cmdReload
        cmdReload.Image = Auditai.UI.Platform.Properties.ContextResources.ctxReloadFile;
        cmdReload.CommandStateQuery += CmdReload_CommandStateQuery;
        cmdReload.Click += CmdReload_Click;
        lnkReload.Command = cmdReload;

        // cmdSyncTable
        cmdSyncTable.CommandStateQuery += CmdSyncTable_CommandStateQuery;
        cmdSyncTable.Click += CmdSyncTable_Click;
        lnkSyncTable.Command = cmdSyncTable;

        // cmdSyncDocument
        cmdSyncDocument.CommandStateQuery += CmdSyncDocument_CommandStateQuery;
        cmdSyncDocument.Click += CmdSyncDocument_Click;
        lnkSyncDocument.Command = cmdSyncDocument;

        // cmdNodeImportFile
        cmdNodeImportFile.CommandStateQuery += CmdNodeImportFile_CommandStateQuery;
        cmdNodeImportFile.Click += CmdNodeImportFile_Click;
        lnkNodeImportFile.Command = cmdNodeImportFile;

        // cmdNodeImportExcel
        cmdNodeImportExcel.CommandStateQuery += CmdNodeImportExcel_CommandStateQuery;
        cmdNodeImportExcel.Click += CmdNodeImportExcel_Click;
        lnkNodeImportExcel.Command = cmdNodeImportExcel;

        // cmdNodeImportWord
        cmdNodeImportWord.CommandStateQuery += CmdNodeImportWord_CommandStateQuery;
        cmdNodeImportWord.Click += CmdNodeImportWord_Click;
        lnkNodeImportWord.Command = cmdNodeImportWord;

        // cmdNodeImportImage
        cmdNodeImportImage.CommandStateQuery += CmdNodeImportImage_CommandStateQuery;
        cmdNodeImportImage.Click += CmdNodeImportImage_Click;
        lnkNodeImportImage.Command = cmdNodeImportImage;

        // cmdNodeImportPdf
        cmdNodeImportPdf.CommandStateQuery += CmdNodeImportPdf_CommandStateQuery;
        cmdNodeImportPdf.Click += CmdNodeImportPdf_Click;
        lnkNodeImportPdf.Command = cmdNodeImportPdf;

        // cmdNodeImportFolder
        cmdNodeImportFolder.CommandStateQuery += CmdNodeImportFolder_CommandStateQuery;
        cmdNodeImportFolder.Click += CmdNodeImportFolder_Click;
        lnkNodeImportFolder.Command = cmdNodeImportFolder;

        // 添加 导入 子菜单到节点菜单
        var lnkImport = new C1CommandLink();
        lnkImport.Command = mnuNodeImport;
        lnkImport.Delimiter = true;
        ctxTreeNode.CommandLinks.Add(lnkImport);

        // ctxBatchOperation 子菜单
        ctxBatchOperation.Text = "批量操作";
        ctxBatchOperation.Image = Auditai.UI.Platform.Properties.Resources.BatchOperation16;
        lnkBatchOperation.Command = ctxBatchOperation;
        ctxTreeNode.CommandLinks.Add(lnkBatchOperation);

        // cmdBatchHideFile
        cmdBatchHideFile.Text = "批量隐藏文件";
        cmdBatchHideFile.Image = Auditai.UI.Platform.Properties.Resources.BatchHideNodes16;
        cmdBatchHideFile.Click += CmdBatchHideFile_Click;
        lnkBatchHideFile.Command = cmdBatchHideFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchHideFile);

        // cmdBatchUnhideFile
        cmdBatchUnhideFile.Text = "批量取消隐藏";
        cmdBatchUnhideFile.Image = Auditai.UI.Platform.Properties.ContextResources.ctxSearch;
        cmdBatchUnhideFile.Click += CmdBatchUnhideFile_Click;
        cmdBatchUnhideFile.CommandStateQuery += CmdBatchUnhideFile_CommandStateQuery;
        lnkBatchUnhideFile.Command = cmdBatchUnhideFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchUnhideFile);

        // cmdBatchDeleteFile
        cmdBatchDeleteFile.Text = "批量删除文件";
        cmdBatchDeleteFile.Image = Auditai.UI.Platform.Properties.Resources.BatchRemoveNodes16;
        cmdBatchDeleteFile.Click += CmdBatchDeleteFile_Click;
        cmdBatchDeleteFile.CommandStateQuery += CmdBatchDeleteFile_CommandStateQuery;
        lnkBatchDeleteFile.Command = cmdBatchDeleteFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchDeleteFile);

        // cmdBatchEditIndex
        cmdBatchEditIndex.Text = "批量编辑索引号";
        cmdBatchEditIndex.Image = Auditai.UI.Platform.Properties.Resources.EditNodesNumber16;
        cmdBatchEditIndex.Click += CmdBatchEditIndex_Click;
        cmdBatchEditIndex.CommandStateQuery += CmdBatchEditIndex_CommandStateQuery;
        lnkBatchEditIndex.Command = cmdBatchEditIndex;
        ctxBatchOperation.CommandLinks.Add(lnkBatchEditIndex);

        // cmdBatchExportFile
        cmdBatchExportFile.Text = "批量导出文件";
        cmdBatchExportFile.Image = Auditai.UI.Platform.Properties.Resources.BatchExport16;
        cmdBatchExportFile.Click += CmdBatchExportFile_Click;
        lnkBatchExportFile.Command = cmdBatchExportFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchExportFile);

        // cmdBatchPrintFile
        cmdBatchPrintFile.Text = "批量打印文件";
        cmdBatchPrintFile.Image = Auditai.UI.Platform.Properties.Resources.BatchPrint16;
        cmdBatchPrintFile.Click += CmdBatchPrintFile_Click;
        lnkBatchPrintFile.Command = cmdBatchPrintFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchPrintFile);

        // ctxProjectMember 子菜单
        ctxProjectMember.CommandStateQuery += ctxProjectMember_CommandStateQuery;
        lnkPushNode.Command = ctxProjectMember;
        ctxTreeNode.CommandLinks.Add(lnkPushNode);

        // ctxTreeNode 分隔符
        lnkMoveUpNode.Delimiter = true;
        lnkRemoveNode.Delimiter = true;
        lnkCutNode.Delimiter = true;
        lnkRenameNode.Delimiter = true;
        lnkReload.Delimiter = true;
        lnkNodeImportFile.Delimiter = true;
        lnkNodeImportExcel.Delimiter = true;
        lnkBatchOperation.Delimiter = true;

        // ctxTreeEmpty 命令
        // cmdAppendRootDirectory
        cmdAppendRootDirectory.CommandStateQuery += CmdAppendRootDirectory_CommandStateQuery;
        cmdAppendRootDirectory.Click += CmdAppendRootDirectory_Click;
        lnkAppendRootDirectory.Command = cmdAppendRootDirectory;

        // cmdAppendRootTable
        cmdAppendRootTable.CommandStateQuery += CmdAppendRootTable_CommandStateQuery;
        cmdAppendRootTable.Click += CmdAppendRootTable_Click;
        lnkAppendRootTable.Command = cmdAppendRootTable;

        // cmdAppendRootDocument
        cmdAppendRootDocument.CommandStateQuery += CmdAppendRootDocument_CommandStateQuery;
        cmdAppendRootDocument.Click += CmdAppendRootDocument_Click;
        lnkAppendRootDocument.Command = cmdAppendRootDocument;

        // lnkShowNodes2 -> cmdShowNodes (ctxTreeEmpty)
        lnkShowNodes2.Command = cmdShowNodes;
        ctxTreeEmpty.CommandLinks.Add(lnkShowNodes2);

        // lnkSearchNodes2 -> cmdSearchNodes (ctxTreeEmpty)
        lnkSearchNodes2.Command = cmdSearchNodes;
        ctxTreeEmpty.CommandLinks.Add(lnkSearchNodes2);

        // cmdAppendRootImage
        cmdAppendRootImage.CommandStateQuery += CmdAppendRootImage_CommandStateQuery;
        cmdAppendRootImage.Click += CmdAppendRootImage_Click;
        lnkAppendRootImage.Command = cmdAppendRootImage;

        // cmdAppendRootPdf
        cmdAppendRootPdf.CommandStateQuery += CmdAppendRootPdf_CommandStateQuery;
        cmdAppendRootPdf.Click += CmdAppendRootPdf_Click;
        lnkAppendRootPdf.Command = cmdAppendRootPdf;

        // 添加 新建 子菜单到空白区域菜单
        var lnkAppendRoot2 = new C1CommandLink();
        lnkAppendRoot2.Command = mnuAppendRoot;
        ctxTreeEmpty.CommandLinks.Add(lnkAppendRoot2);

        // cmdPasteRootNode（统一粘贴命令）
        cmdPasteRootNode.CommandStateQuery += CmdPasteRootNode_CommandStateQuery;
        cmdPasteRootNode.Click += CmdPasteRootNode_Click;
        var lnkPasteRoot2 = new C1CommandLink();
        lnkPasteRoot2.Command = cmdPasteRootNode;
        lnkPasteRoot2.Delimiter = true;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRoot2);

        // lnkPasteGroup4 -> cmdPasteGroupClickOnGridTree (ctxTreeEmpty)
        lnkPasteGroup4.Command = cmdPasteGroupClickOnGridTree;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteGroup4);

        // cmdEmptyImportFile
        cmdEmptyImportFile.CommandStateQuery += CmdEmptyImportFile_CommandStateQuery;
        cmdEmptyImportFile.Click += CmdEmptyImportFile_Click;
        lnkEmptyImportFile.Command = cmdEmptyImportFile;

        // cmdEmptyImportExcel
        cmdEmptyImportExcel.CommandStateQuery += CmdEmptyImportExcel_CommandStateQuery;
        cmdEmptyImportExcel.Click += CmdEmptyImportExcel_Click;
        lnkEmptyImportExcel.Command = cmdEmptyImportExcel;

        // cmdEmptyImportWord
        cmdEmptyImportWord.CommandStateQuery += CmdEmptyImportWord_CommandStateQuery;
        cmdEmptyImportWord.Click += CmdEmptyImportWord_Click;
        lnkEmptyImportWord.Command = cmdEmptyImportWord;

        // cmdEmptyImportImage
        cmdEmptyImportImage.CommandStateQuery += CmdEmptyImportImage_CommandStateQuery;
        cmdEmptyImportImage.Click += CmdEmptyImportImage_Click;
        lnkEmptyImportImage.Command = cmdEmptyImportImage;

        // cmdEmptyImportPdf
        cmdEmptyImportPdf.CommandStateQuery += CmdEmptyImportPdf_CommandStateQuery;
        cmdEmptyImportPdf.Click += CmdEmptyImportPdf_Click;
        lnkEmptyImportPdf.Command = cmdEmptyImportPdf;

        // cmdEmptyImportFolder
        cmdEmptyImportFolder.CommandStateQuery += CmdEmptyImportFolder_CommandStateQuery;
        cmdEmptyImportFolder.Click += CmdEmptyImportFolder_Click;
        lnkEmptyImportFolder.Command = cmdEmptyImportFolder;

        // 添加 导入 子菜单到空白区域菜单
        var lnkEmptyImport2 = new C1CommandLink();
        lnkEmptyImport2.Command = mnuEmptyImport;
        lnkEmptyImport2.Delimiter = true;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImport2);

        // ctxTreeEmpty 分隔符
        lnkPasteRootNode.Delimiter = true;
        lnkEmptyImportFile.Delimiter = true;
        lnkEmptyImportExcel.Delimiter = true;
        lnkShowNodes2.Delimiter = true;
        lnkPasteGroup4.Delimiter = true;

        // lnkBatchOperation2 -> ctxBatchOperation (ctxTreeEmpty)
        lnkBatchOperation2.Command = ctxBatchOperation;
        lnkBatchOperation2.Delimiter = true;
        ctxTreeEmpty.CommandLinks.Add(lnkBatchOperation2);

        // 事件和延迟执行
        TreeNodeCollapsed += ProjectHierarchy_TreeNodeCollapsed;
        View.SelectedPageChanged += View_SelectedPageChanged;
        frmSearch = new frmSearch();
        frmSearch.SelectNode += FrmSearch_SelectNode;
        lazySearchExcute.SetAction(LazySearchExcute_Action);
    }

    #region 右键菜单操作

    #region 分组操作

    private void OnMoveUpGroup(object sender, EventArgs e)
    {
        var selectedGroup = SelectedNode as TreeGroup;
        if (selectedGroup == null) return;

        selectedGroup.MoveUp1();
        Populate();
        FindAndSelectGroup(selectedGroup);
    }

    private void OnMoveDownGroup(object sender, EventArgs e)
    {
        var selectedGroup = SelectedNode as TreeGroup;
        if (selectedGroup == null) return;

        selectedGroup.MoveDown1();
        Populate();
        FindAndSelectGroup(selectedGroup);
    }

    private void OnNewGroup(object sender, EventArgs e)
    {
        if (Project == null) return;

        TreeGroup newGroup = Project.AppendTreeGroup();
        Populate();
        FindAndSelectGroup(newGroup);
    }

    private void OnDeleteGroup(object sender, EventArgs e)
    {
        var selectedGroup = SelectedNode as TreeGroup;
        if (selectedGroup == null) return;

        if (System.Windows.Forms.MessageBox.Show($"确定要删除分组 \"{selectedGroup.Name}\" 吗？", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            selectedGroup.Remove();
            SelectedNode = null;
            Populate();
        }
        catch (Exception ex)
        {
            ex.Log();
            System.Windows.Forms.MessageBox.Show("删除分组失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnCopyGroup(object sender, EventArgs e)
    {
        var selectedGroup = SelectedNode as TreeGroup;
        if (selectedGroup == null) return;

        try
        {
            TreeGroup newGroup = Project.AppendTreeGroup();
            newGroup.UpdateName(selectedGroup.Name + " (副本)");

            foreach (TreeNodeBase rootNode in selectedGroup.RootNodes)
            {
                TreeNodeBase clonedNode = CloneTreeNode(rootNode);
                if (clonedNode != null)
                {
                    newGroup.InsertRootNode(clonedNode, newGroup.RootNodes.Count);
                }
            }

            Populate();
            FindAndSelectGroup(newGroup);
        }
        catch (Exception ex)
        {
            ex.Log();
            System.Windows.Forms.MessageBox.Show("复制分组失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private TreeNodeBase CloneTreeNode(TreeNodeBase source)
    {
        if (source is TreeDirectoryNode dirNode)
        {
            TreeDirectoryNode newDir = dirNode.DuplicateDirectory();
            foreach (TreeNodeBase child in dirNode.Children)
            {
                TreeNodeBase clonedChild = CloneTreeNode(child);
                if (clonedChild != null)
                {
                    newDir.InsertChildNode(clonedChild, newDir.Children.Count);
                }
            }
            return newDir;
        }
        else if (source is TreeTableNode tableNode)
        {
            return tableNode.DuplicateTable();
        }
        else if (source is TreeDocumentNode docNode)
        {
            return docNode.DuplicateDocument();
        }
        else if (source is TreeImageNode imgNode)
        {
            return imgNode.DuplicateImage();
        }
        else if (source is TreePdfNode pdfNode)
        {
            return pdfNode.DuplicatePdf();
        }
        return null;
    }

    private void OnRenameGroup(object sender, EventArgs e)
    {
        var selectedGroup = SelectedNode as TreeGroup;
        if (selectedGroup == null) return;

        string newName = PromptForName("重命名分组", "请输入新的分组名称：", selectedGroup.Name);
        if (newName != null && newName != selectedGroup.Name)
        {
            selectedGroup.UpdateName(newName);
            Populate();
            FindAndSelectGroup(selectedGroup);
        }
    }

    private void FindAndSelectGroup(TreeGroup group)
    {
        for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
            if (row.IsNode && row.Node.Key is TreeGroup g && g.Id == group.Id)
            {
                _grid.Row = i;
                _grid.Select(i, 0, i, 0);
                return;
            }
        }
    }

    #endregion

    private void OnNewDirectory(object sender, EventArgs e)
    {
        var selectedNode = SelectedNode as TreeNodeBase;
        TreeDirectoryNode newDir;
        if (selectedNode is TreeDirectoryNode dirNode)
        {
            newDir = dirNode.InsertChildDirectory(dirNode.Children.Count);
        }
        else if (selectedNode == null || selectedNode is TreeTableNode || selectedNode is TreeDocumentNode)
        {
            var group = GetCurrentGroup();
            if (group != null)
                newDir = group.InsertRootDirectory(group.RootNodes.Count);
            else return;
        }
        else return;

        Populate();
        FindAndSelectNode(newDir);
    }

    private void OnNewTable(object sender, EventArgs e)
    {
        var selectedNode = SelectedNode as TreeNodeBase;
        TreeTableNode newTable;
        if (selectedNode is TreeDirectoryNode dirNode)
        {
            newTable = dirNode.InsertChildTable(dirNode.Children.Count);
        }
        else
        {
            var group = GetCurrentGroup();
            if (group != null)
                newTable = group.InsertRootTable(group.RootNodes.Count);
            else return;
        }

        Populate();
        FindAndSelectNode(newTable);
    }

    private void OnNewDocument(object sender, EventArgs e)
    {
        var selectedNode = SelectedNode as TreeNodeBase;
        TreeDocumentNode newDoc;
        if (selectedNode is TreeDirectoryNode dirNode)
        {
            newDoc = dirNode.InsertChildDocument(dirNode.Children.Count);
        }
        else
        {
            var group = GetCurrentGroup();
            if (group != null)
                newDoc = group.InsertRootDocument(group.RootNodes.Count);
            else return;
        }

        Populate();
        FindAndSelectNode(newDoc);
    }

    private void OnImportFile(object sender, EventArgs e)
    {
        using (var dlg = new OpenFileDialog
        {
            Filter = "所有支持的文件|*.xls;*.xlsx;*.doc;*.docx;*.pdf;*.bmp;*.jpg;*.png;*.gif;*.tif;*.tiff|Excel文件|*.xls;*.xlsx|Word文件|*.doc;*.docx|PDF文件|*.pdf|图片文件|*.bmp;*.jpg;*.png;*.gif;*.tif;*.tiff|所有文件|*.*",
            Multiselect = true
        })
        {
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var selectedNode = SelectedNode as TreeNodeBase;
            object parentNode = null;
            int index = -1;

            if (selectedNode is TreeDirectoryNode dirNode)
            {
                parentNode = dirNode;
                index = dirNode.Children.Count;
            }
            else
            {
                var group = GetCurrentGroup();
                if (group != null)
                {
                    parentNode = group;
                    index = group.RootNodes.Count;
                }
                else return;
            }

            try
            {
                var importer = new ProjectImport(_grid);
                importer.ImportFiles(parentNode, index, dlg.FileNames);
                Populate();
            }
            catch (Exception ex)
            {
                ex.Log();
                System.Windows.Forms.MessageBox.Show("导入文件失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnImportFolder(object sender, EventArgs e)
    {
        using (var dlg = new FolderBrowserDialog { Description = "选择要导入的文件夹" })
        {
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var selectedNode = SelectedNode as TreeNodeBase;
            object parentNode = null;
            int index = -1;

            if (selectedNode is TreeDirectoryNode dirNode)
            {
                parentNode = dirNode;
                index = dirNode.Children.Count;
            }
            else
            {
                var group = GetCurrentGroup();
                if (group != null)
                {
                    parentNode = group;
                    index = group.RootNodes.Count;
                }
                else return;
            }

            try
            {
                var importer = new ProjectImport(_grid);
                importer.ImportFolder(parentNode, index, dlg.SelectedPath);
                Populate();
            }
            catch (Exception ex)
            {
                ex.Log();
                System.Windows.Forms.MessageBox.Show("导入文件夹失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnRename(object sender, EventArgs e)
    {
        var selectedNode = SelectedNode as TreeNodeBase;
        if (selectedNode == null) return;

        string newName = PromptForName("重命名", "请输入新名称：", selectedNode.Name);
        if (newName != null && newName != selectedNode.Name)
        {
            selectedNode.UpdateName(newName);
            Populate();
            FindAndSelectNode(selectedNode);
        }
    }

    private void OnDelete(object sender, EventArgs e)
    {
        var selectedNode = SelectedNode as TreeNodeBase;
        if (selectedNode == null) return;

        if (System.Windows.Forms.MessageBox.Show($"确定要删除 \"{selectedNode.Name}\" 吗？", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            if (selectedNode is TreeDirectoryNode dirNode)
            {
                dirNode.Remove();
            }
            else if (selectedNode is TreeTableNode tableNode)
            {
                tableNode.Remove();
            }
            else if (selectedNode is TreeDocumentNode docNode)
            {
                docNode.Remove();
            }
            else if (selectedNode is TreeImageNode imgNode)
            {
                imgNode.Remove();
            }
            else if (selectedNode is TreePdfNode pdfNode)
            {
                pdfNode.Remove();
            }

            SelectedNode = null;
            Populate();
        }
        catch (Exception ex)
        {
            ex.Log();
            System.Windows.Forms.MessageBox.Show("删除失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnMoveUp(object sender, EventArgs e)
    {
        MoveUpNode();
    }

    private void OnMoveDown(object sender, EventArgs e)
    {
        MoveDownNode();
    }

    #endregion

    #region 辅助方法

    private TreeGroup GetCurrentGroup()
    {
        if (Project == null) return null;
        if (Project.TreeGroups.Count == 0) return null;
        return Project.TreeGroups[0];
    }

    private string PromptForName(string title, string prompt, string defaultValue)
    {
        string result = defaultValue;
        Form promptForm = new Form
        {
            Text = title,
            Size = new Size(350, 150),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };
        var lbl = new Label { Text = prompt, Location = new Point(10, 15), AutoSize = true };
        var txt = new TextBox { Text = defaultValue, Location = new Point(10, 40), Size = new Size(310, 25) };
        var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(160, 75), Size = new Size(75, 25) };
        var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(245, 75), Size = new Size(75, 25) };
        promptForm.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
        promptForm.AcceptButton = btnOk;
        promptForm.CancelButton = btnCancel;

        if (promptForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
        {
            return txt.Text;
        }
        return null;
    }

    #endregion

    #region 核心方法

    public void Populate()
        {
            // 保存当前展开状态
            var expandedKeys = new HashSet<object>();
            SaveExpandedState(_grid, expandedKeys);

            SelectedNode = null;
            _grid.BeginUpdate();
            _grid.Rows.Count = _grid.Rows.Fixed;

            if (Project?.TreeGroups == null)
            {
                _grid.EndUpdate();
                return;
            }

            foreach (Auditai.Model.TreeGroup treeGroup in Project.TreeGroups)
            {
                Node node = _grid.Rows.AddNode(0);
                node.Key = treeGroup;
                node.Data = treeGroup.Name;
                node.Image = Resources.TreeDir;

                foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
                {
                    AddTreeNode(rootNode, node);
                }
                // 恢复展开状态
                node.Collapsed = !expandedKeys.Contains(treeGroup);
            }
            _grid.EndUpdate();
        }

    private void SaveExpandedState(C1FlexGridBase grid, HashSet<object> expandedKeys)
    {
        for (int r = grid.Rows.Fixed; r < grid.Rows.Count; r++)
        {
            var row = grid.Rows[r];
            if (row.IsNode && !row.Node.Collapsed && row.Node.Key != null)
            {
                expandedKeys.Add(row.Node.Key);
            }
        }
    }

    private void AddTreeNode(TreeNodeBase treeNode, Node parentNode)
    {
        Node node = null;
        if (treeNode is TreeDirectoryNode dirNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, dirNode.Name, dirNode, Resources.TreeDir);
            foreach (TreeNodeBase child in dirNode.Children)
            {
                AddTreeNode(child, node);
            }
            node.Expanded = false;
        }
        else if (treeNode is TreeTableNode tableNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, tableNode.Name, tableNode, Resources.TreeTable);
        }
        else if (treeNode is TreeDocumentNode docNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, docNode.Name, docNode, Resources.TreeDoc);
        }
        else if (treeNode is TreeImageNode imgNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, imgNode.Name, imgNode, Resources.TreeDoc);
        }
        else if (treeNode is TreePdfNode pdfNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, pdfNode.Name, pdfNode, Resources.TreeDoc);
        }

        if (!treeNode.Visible && node != null)
        {
            node.Row.Visible = false;
        }

        // 显示索引号
        if (NumberShown && node != null && !string.IsNullOrEmpty(treeNode.Number))
        {
            node.Data = treeNode.Number + " " + treeNode.Name;
        }
    }

    public bool FindAndSelectNode(params object[] args)
    {
        TreeNodeBase targetNode = null;
        if (args.Length > 0)
        {
            targetNode = args[0] as TreeNodeBase;
        }
        if (targetNode == null) return false;

        for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
            if (row.UserData is TreeNodeBase treeNodeBase && treeNodeBase.Id == targetNode.Id)
            {
                SelectedNode = targetNode;
                _grid.Row = i;

                // 确保父节点展开
                Node node = row.Node;
                while (node != null)
                {
                    node = node.Parent;
                    if (node != null) node.Collapsed = false;
                }

                _grid.Select(i, 0, i, 0);
                return true;
            }
        }
        return false;
    }

    public dynamic FindNode(TreeNodeBase node)
    {
        if (node == null) return null;
        for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
            if (row.UserData is TreeNodeBase treeNodeBase && treeNodeBase.Id == node.Id)
            {
                return row.Node;
            }
        }
        return null;
    }

    public void Invalidate()
    {
        _grid.Invalidate();
    }

    public bool HasWritePermission()
    {
        var sn = SelectedNode as TreeNodeBase;
        return sn == null || sn.HasWritePermission();
    }

    public TreeGroupView GetTreeGroupView(object grid)
    {
        return new TreeGroupView { Grid = grid as C1FlexGridBase };
    }

    public void FinishEditorInputStatus(bool isCancelInput)
    {
        try
        {
            if (_currentGroup == null) return;
            var grid = _currentGroup.Grid;
            if (grid == null) return;
            if (grid.Editor == null) return;
            if (isCancelInput)
            {
                grid.FinishEditing(true);
            }
            else
            {
                if (!grid.FinishEditing(false))
                {
                    grid.FinishEditing(true);
                }
            }
        }
        catch (Exception ex)
        {
            ex.Log("结束表格表底区的编辑输入状态时发生了未预期的异常");
        }
    }

    public int GetAllFileNodesTotalCount()
    {
        if (Project?.TreeGroups == null) return 0;
        int count = 0;
        foreach (var group in Project.TreeGroups)
        {
            count += CountFileNodes(group.RootNodes);
        }
        return count;
    }

    private int CountFileNodes(List<TreeNodeBase> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node is TreeDirectoryNode dirNode)
                count += CountFileNodes(dirNode.Children);
            else
                count++;
        }
        return count;
    }

    public void MoveUpNode()
    {
        var sn = SelectedNode as TreeNodeBase;
        if (sn == null) return;

        if (sn.Parent is TreeDirectoryNode parent)
        {
            int idx = parent.Children.IndexOf(sn);
            if (idx > 0)
            {
                parent.Children.RemoveAt(idx);
                parent.Children.Insert(idx - 1, sn);
                Populate();
                FindAndSelectNode(sn);
            }
        }
        else
        {
            var group = sn.Group;
            if (group != null)
            {
                int idx = group.RootNodes.IndexOf(sn);
                if (idx > 0)
                {
                    group.RootNodes.RemoveAt(idx);
                    group.RootNodes.Insert(idx - 1, sn);
                    Populate();
                    FindAndSelectNode(sn);
                }
            }
        }
    }

    public void MoveDownNode()
    {
        var sn = SelectedNode as TreeNodeBase;
        if (sn == null) return;

        if (sn.Parent is TreeDirectoryNode parent)
        {
            int idx = parent.Children.IndexOf(sn);
            if (idx < parent.Children.Count - 1)
            {
                parent.Children.RemoveAt(idx);
                parent.Children.Insert(idx + 1, sn);
                Populate();
                FindAndSelectNode(sn);
            }
        }
        else
        {
            var group = sn.Group;
            if (group != null)
            {
                int idx = group.RootNodes.IndexOf(sn);
                if (idx < group.RootNodes.Count - 1)
                {
                    group.RootNodes.RemoveAt(idx);
                    group.RootNodes.Insert(idx + 1, sn);
                    Populate();
                    FindAndSelectNode(sn);
                }
            }
        }
    }

    public void RecycleNode()
    {
        OnDelete(null, EventArgs.Empty);
    }

    public void ReloadNode()
    {
        var sn = SelectedNode as TreeNodeBase;
        Populate();
        if (sn != null) FindAndSelectNode(sn);
    }

    public void ShowNumber()
    {
        NumberShown = true;
        Populate();
    }

    public void HideNumber()
    {
        NumberShown = false;
        Populate();
    }

    public bool CanRemoveNode(params object[] args)
    {
        return SelectedNode != null;
    }

    private void UpdateCurrentGroupModel(TreeGroup group)
    {
        if (_currentGroup != null && group != null)
        {
            _currentGroup.Model = group;
        }
    }

    #endregion

    private void _grid_MouseClick(object sender, MouseEventArgs e)
    {
        HitTestInfo hitTestInfo = _grid.HitTest();
        if (hitTestInfo.Type == HitTestTypeEnum.Cell)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
            if (row.IsNode)
            {
                Node node = row.Node;
                
                if (node.Key is TreeGroup group)
                {
                    SelectedNode = group;
                    UpdateCurrentGroupModel(group);
                }
                else if (node.Key is TreeNodeBase tnb)
                {
                    SelectedNode = tnb;
                    UpdateCurrentGroupModel(tnb.Group);
                    // 仅左键点击时触发选中事件，右键不触发以避免不必要的文档加载
                    if (e.Button == MouseButtons.Left)
                        TreeNodeSelected?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // 左键点击树列时展开/折叠
        if (e.Button == MouseButtons.Left && hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == _grid.Tree.Column)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
            if (row.IsNode)
            {
                Node node = row.Node;
                node.Collapsed = !node.Collapsed;
            }
        }

        // 右键菜单
        if (e.Button == MouseButtons.Right)
        {
            HitTestInfo ht = _grid.HitTest();
            if (ht.Type == HitTestTypeEnum.Cell)
            {
                C1.Win.C1FlexGrid.Row row = _grid.Rows[ht.Row];
                if (row.IsNode)
                {
                    Node node = row.Node;
                    if (node.Key is TreeGroup group)
                    {
                        SelectedNode = group;
                        UpdateCurrentGroupModel(group);
                        _grid.Row = ht.Row;
                        ctxTreeGroup.ShowContextMenu(_grid, e.Location);
                    }
                    else if (node.Key is TreeNodeBase tnb)
                    {
                        SelectedNode = tnb;
                        UpdateCurrentGroupModel(tnb.Group);
                        _grid.Row = ht.Row;
                        ctxTreeNode.ShowContextMenu(_grid, e.Location);
                    }
                }
                else
                {
                    ctxTreeEmpty.ShowContextMenu(_grid, e.Location);
                }
            }
            else
            {
                ctxTreeNothing.ShowContextMenu(_grid, e.Location);
            }
        }
    }

    private void View_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;

        var outBar = (C1OutBarEx)sender;
        if (outBar.HotPage == null)
            ctxTreeNothing.ShowContextMenu(View, e.Location);
        else
            ctxTreeGroup.ShowContextMenu(View, e.Location);
    }

    private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        HitTestInfo hitTestInfo = _grid.HitTest();
        if (hitTestInfo.Type == HitTestTypeEnum.Cell)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
            if (row.IsNode)
            {
                Node node = row.Node;
                TreeNodeBase tnb = node.Key as TreeNodeBase;
                SelectedNode = tnb;
                TreeNodeSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    #region Initialize 事件存根

    private void Trigger_Tick(object sender, EventArgs e)
    {
    }

    private void ProjectHierarchy_TreeNodeCollapsed(object sender, C1.Win.C1FlexGrid.Row e)
    {
    }

    private void View_SelectedPageChanged(object sender, EventArgs e)
    {
        _currentGroup = View.SelectedPage?.Tag as TreeGroupView;
        var en = View.Pages.GetEnumerator();
        try
        {
            while (en.MoveNext())
            {
                var page = (C1OutPage)en.Current;
                if (_currentGroup?.Page == page) continue;
                var groupView = page.Tag as TreeGroupView;
                if (groupView != null)
                    groupView.Grid.Row = -1;
            }
        }
        finally
        {
            var disp = en as IDisposable;
            if (disp != null) disp.Dispose();
        }
        RefreshOpenNode(false);
    }

    private void ctxProjectMember_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Enabled = true;
    }

    private void FrmSearch_SelectNode(object sender, TreeNodeBase e)
    {
        FindAndSelectNode(e);
    }

    private void LazySearchExcute_Action()
    {
        if (Project == null || frmSearch == null) return;
        frmSearch.Project = Project;
        frmSearch.Show();
        frmSearch.Activate();
    }

    #endregion

    #region Cmd*_Click 和 Cmd*_CommandStateQuery 方法存根

    #region 分组命令

    private void CmdMoveUpGroup_Click(object sender, ClickEventArgs e) => OnMoveUpGroup(null, EventArgs.Empty);
    private void CmdMoveUpGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdMoveDownGroup_Click(object sender, ClickEventArgs e) => OnMoveDownGroup(null, EventArgs.Empty);
    private void CmdMoveDownGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAddGroup_Click(object sender, ClickEventArgs e) => OnNewGroup(null, EventArgs.Empty);
    private void CmdAddGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdRemoveGroup_Click(object sender, ClickEventArgs e) => OnDeleteGroup(null, EventArgs.Empty);
    private void CmdRemoveGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyGroup_Click(object sender, ClickEventArgs e) => OnCopyGroup(null, EventArgs.Empty);
    private void CmdCopyGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteGroupClickOnGridTree_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteGroupClickOnGridTree_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteGroup_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdRenameGroup_Click(object sender, ClickEventArgs e) => OnRenameGroup(null, EventArgs.Empty);
    private void CmdRenameGroup_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 节点插入命令

    private void CmdInsertDirectory_Click(object sender, ClickEventArgs e) => OnNewDirectory(null, EventArgs.Empty);
    private void CmdInsertDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdInsertTable_Click(object sender, ClickEventArgs e) => OnNewTable(null, EventArgs.Empty);
    private void CmdInsertTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdInsertDocument_Click(object sender, ClickEventArgs e) => OnNewDocument(null, EventArgs.Empty);
    private void CmdInsertDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdInsertImage_Click(object sender, ClickEventArgs e)
    {
        if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
        var imageId = SelectImage();
        if (!imageId.HasValue) return;
        int index = SelectedNode.Index;
        TreeNodeBase newNode = null;
        if (SelectedNode.IsRoot)
        {
            newNode = _currentGroup.Model.InsertRootImage(index, imageId.Value);
        }
        else
        {
            newNode = SelectedNode.Parent.InsertChildImage(index, imageId.Value);
        }
        var grid = _currentGroup.Grid;
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, Resources.TreeDoc);
        grid.Row = node.Row.Index;
    }
    private void CmdInsertImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdInsertPdf_Click(object sender, ClickEventArgs e)
    {
        if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
        var pdfId = SelectPdf();
        if (!pdfId.HasValue) return;
        int index = SelectedNode.Index;
        TreeNodeBase newNode = null;
        if (SelectedNode.IsRoot)
        {
            newNode = _currentGroup.Model.InsertRootPdf(index, pdfId.Value);
        }
        else
        {
            newNode = SelectedNode.Parent.InsertChildPdf(index, pdfId.Value);
        }
        var grid = _currentGroup.Grid;
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, Resources.TreeDoc);
        grid.Row = node.Row.Index;
    }
    private void CmdInsertPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildDirectory_Click(object sender, ClickEventArgs e) => OnNewDirectory(null, EventArgs.Empty);
    private void CmdAppendChildDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildTable_Click(object sender, ClickEventArgs e) => OnNewTable(null, EventArgs.Empty);
    private void CmdAppendChildTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildDocument_Click(object sender, ClickEventArgs e) => OnNewDocument(null, EventArgs.Empty);
    private void CmdAppendChildDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildImage_Click(object sender, ClickEventArgs e)
    {
        if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
        var imageId = SelectImage();
        if (!imageId.HasValue) return;
        var dirNode = (TreeDirectoryNode)SelectedNode;
        var newNode = dirNode.InsertChildImage(dirNode.Children.Count, imageId.Value);
        var grid = _currentGroup.Grid;
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, Resources.TreeDoc);
        grid.Row = node.Row.Index;
    }
    private void CmdAppendChildImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildPdf_Click(object sender, ClickEventArgs e)
    {
        if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
        var pdfId = SelectPdf();
        if (!pdfId.HasValue) return;
        var dirNode = (TreeDirectoryNode)SelectedNode;
        var newNode = dirNode.InsertChildPdf(dirNode.Children.Count, pdfId.Value);
        var grid = _currentGroup.Grid;
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, Resources.TreeDoc);
        grid.Row = node.Row.Index;
    }
    private void CmdAppendChildPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 节点操作命令

    private void CmdMoveUpNode_Click(object sender, ClickEventArgs e) => OnMoveUp(null, EventArgs.Empty);
    private void CmdMoveUpNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdMoveDownNode_Click(object sender, ClickEventArgs e) => OnMoveDown(null, EventArgs.Empty);
    private void CmdMoveDownNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdRemoveNode_Click(object sender, ClickEventArgs e) => OnDelete(null, EventArgs.Empty);
    private void CmdRemoveNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdHideNode_Click(object sender, ClickEventArgs e)
    {
        if (_currentGroup == null) return;
        var node = SelectedNode as TreeNodeBase;
        if (node == null) return;

        if (node.Visible)
        {
            if (!CanRemoveNode(node))
            {
                var dirNode = node as TreeDirectoryNode;
                if (dirNode != null)
                {
                    var cantDeleteNode = dirNode.GetFirstCantDeleteDescendant();
                    MessageBox.Show(MessageBoxIcon.None, string.Concat("因您没有该文件夹下【", cantDeleteNode.Name, "】文件的【", cantDeleteNode.GetDontHavePermissionString(), "】权限，因此，无法对该文件夹及其下所有文件执行隐藏操作 。"), MessageBoxButtons.OK, "", scroll: false);
                    return;
                }
                MessageBox.Show(MessageBoxIcon.None, string.Concat("因您没有该文件的【", node.GetDontHavePermissionString(), "】权限，因此，无法对该文件执行隐藏操作。"), MessageBoxButtons.OK, "", scroll: false);
                return;
            }
            node.UpdateVisible(false);
            var gridNode = _currentGroup.Grid.Rows[_currentGroup.Grid.Row].Node;
            ((C1FlexGridEx)_currentGroup.Grid).SetSubtreeVisible(gridNode, false);
            Program.MainForm.SwitchToEmptyView();
        }
        else
        {
            node.UpdateVisible(true);
            var gridNode = _currentGroup.Grid.Rows[_currentGroup.Grid.Row].Node;
            ((C1FlexGridEx)_currentGroup.Grid).SetSubtreeVisible(gridNode, true);
        }
    }
    private void CmdHideNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        var node = SelectedNode as TreeNodeBase;
        e.Enabled = node != null;
        if (node != null)
        {
            cmdHideNode.Text = node.Visible ? "隐藏" : "取消隐藏";
        }
    }

    private void CmdShowNodes_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var selectedNode = SelectedNode as TreeNodeBase;
            var form = new frmNodeSelector();
            form.Project = Project;
            if (form.ShowUnhide() != DialogResult.OK) return;

            foreach (var node in form.Selected)
            {
                node.UpdateVisible(true);
            }

            Populate();
            if (selectedNode != null && selectedNode.Visible)
            {
                FindAndSelectNode(selectedNode);
            }
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void ShowAllNodesRecursive(List<TreeNodeBase> nodes)
    {
        if (nodes == null) return;
        foreach (var node in nodes)
        {
            node.UpdateVisible(true);
            if (node is TreeDirectoryNode dirNode)
            {
                ShowAllNodesRecursive(dirNode.Children);
            }
        }
    }
    private void CmdShowNodes_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdSearchNodes_Click(object sender, ClickEventArgs e)
    {
        lazySearchExcute.Excute();
    }
    private void CmdSearchNodes_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCutNode_Click(object sender, ClickEventArgs e)
    {
        if (ShowUnableCutDialog(SelectedNode)) return;
        SetCutInfo();
    }
    private void CmdCutNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopy_Click(object sender, ClickEventArgs e)
    {
        var node = SelectedNode as TreeNodeBase;
        if (node == null) return;
        if (ShowUnableCopyDialog(node)) return;
        if (node is TreeDocumentNode docNode && docNode.Document.Paragraphs.Count <= 0) return;
        _cutCopyMode = CutCopyModeEnum.Copy;
        CutCopyMode = CutCopyModeEnum.Copy;
        ClipboardManager.Instance.ProjectHierarchyNode = node;
    }
    private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        var node = SelectedNode as TreeNodeBase;
        e.Enabled = node != null && (
            node is TreeTableNode ||
            node is TreeDocumentNode ||
            node is TreeDirectoryNode ||
            node is TreeImageNode ||
            node is TreePdfNode);
    }

    private void CmdPasteNode_Click(object sender, ClickEventArgs e)
    {
        if (_currentGroup == null || _currentGroup.Grid == null) return;
        var clipNode = ClipboardManager.Instance.ProjectHierarchyNode;
        if (clipNode == null) return;
        if (_cutCopyMode != CutCopyModeEnum.Cut && _cutCopyMode != CutCopyModeEnum.Copy) return;
        DoPaste();
    }
    private void CmdPasteNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        var clipNode = ClipboardManager.Instance.ProjectHierarchyNode;
        e.Enabled = clipNode != null && (
            _cutCopyMode == CutCopyModeEnum.Cut ||
            _cutCopyMode == CutCopyModeEnum.Copy);
    }

    private void CmdPasteTable_Click(object sender, ClickEventArgs e)
    {
        CopyPasteNonRootTable();
    }
    private void CmdPasteTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteDocument_Click(object sender, ClickEventArgs e)
    {
        CopyPasteNonRootDocument();
    }
    private void CmdPasteDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteDirectory_Click(object sender, ClickEventArgs e)
    {
        CopyPasteNonRootDirectory();
    }
    private void CmdPasteDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteImage_Click(object sender, ClickEventArgs e)
    {
        CopyPasteNonRootImage();
    }
    private void CmdPasteImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPastePdf_Click(object sender, ClickEventArgs e)
    {
        CopyPasteNonRootPdf();
    }
    private void CmdPastePdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdRenameNode_Click(object sender, ClickEventArgs e) => OnRename(null, EventArgs.Empty);
    private void CmdRenameNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEditNumber_Click(object sender, ClickEventArgs e)
    {
        EditNumber();
    }
    private void CmdEditNumber_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdReload_Click(object sender, ClickEventArgs e)
    {
        ReloadNode();
    }
    private void CmdReload_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdSyncTable_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdSyncTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdSyncDocument_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdSyncDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 节点导入命令

    private void CmdNodeImportFile_Click(object sender, ClickEventArgs e) => OnImportFile(null, EventArgs.Empty);
    private void CmdNodeImportFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportExcel_Click(object sender, ClickEventArgs e)
    {
        SelectNodeImportExcel();
    }
    private void CmdNodeImportExcel_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportWord_Click(object sender, ClickEventArgs e)
    {
        if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
        firstImportNode = null;
        SelectNodeImportWord();
        if (firstImportNode != null) FindAndSelectNode(firstImportNode);
    }
    private void CmdNodeImportWord_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportImage_Click(object sender, ClickEventArgs e)
    {
        SelectNodeImportImage();
    }
    private void CmdNodeImportImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportPdf_Click(object sender, ClickEventArgs e)
    {
        SelectNodeImportPdf();
    }
    private void CmdNodeImportPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportFolder_Click(object sender, ClickEventArgs e) => OnImportFolder(null, EventArgs.Empty);
    private void CmdNodeImportFolder_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 批量操作命令

    private List<TreeNodeBase> GetBatchOperationTargetNodes()
    {
        var result = new List<TreeNodeBase>();
        if (_currentGroup?.Model == null) return result;

        var selectedNode = SelectedNode as TreeNodeBase;
        if (selectedNode is TreeDirectoryNode dirNode)
        {
            foreach (var child in dirNode.Children)
            {
                if (!(child is TreeDirectoryNode))
                    result.Add(child);
            }
        }
        else
        {
            foreach (var root in _currentGroup.Model.RootNodes)
            {
                if (!(root is TreeDirectoryNode))
                    result.Add(root);
            }
        }

        return result;
    }

    private void CmdBatchHideFile_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var selectedNode = SelectedNode as TreeNodeBase;
            var form = new frmNodeSelector();
            form.Project = Project;
            if (form.ShowHide() != DialogResult.OK) return;

            foreach (var node in form.Selected)
            {
                if (CanRemoveNode(node))
                {
                    node.UpdateVisible(false);
                }
            }

            Populate();
            if (selectedNode != null && selectedNode.Visible)
            {
                FindAndSelectNode(selectedNode);
            }
            else
            {
                Program.MainForm.SwitchToEmptyView();
            }
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CmdBatchUnhideFile_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var selectedNode = SelectedNode as TreeNodeBase;
            var form = new frmNodeSelector();
            form.Project = Project;
            if (form.ShowUnhide() != DialogResult.OK) return;

            foreach (var node in form.Selected)
            {
                node.UpdateVisible(true);
            }

            Populate();
            if (selectedNode != null && selectedNode.Visible)
            {
                FindAndSelectNode(selectedNode);
            }
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CmdBatchDeleteFile_Click(object sender, ClickEventArgs e)
    {
        try
        {
            Program.MainForm.RemoveNodes();
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }
    private void CmdBatchUnhideFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;
    private void CmdBatchDeleteFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdBatchEditIndex_Click(object sender, ClickEventArgs e)
    {
        try
        {
            Program.MainForm.NodesIndexEdit();
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CmdBatchEditIndex_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Enabled = true;
    }

    private async void CmdBatchExportFile_Click(object sender, ClickEventArgs e)
    {
        try
        {
            await Program.MainForm.BatchExport("批量导出");
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private async void CmdBatchPrintFile_Click(object sender, ClickEventArgs e)
    {
        try
        {
            await Program.MainForm.BatchPrint_Click("批量打印");
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    #endregion

    #region 空白区域根节点命令

    private void CmdAppendRootDirectory_Click(object sender, ClickEventArgs e) => OnNewDirectory(null, EventArgs.Empty);
    private void CmdAppendRootDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendRootTable_Click(object sender, ClickEventArgs e) => OnNewTable(null, EventArgs.Empty);
    private void CmdAppendRootTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendRootDocument_Click(object sender, ClickEventArgs e) => OnNewDocument(null, EventArgs.Empty);
    private void CmdAppendRootDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendRootImage_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var imageId = SelectImage();
            if (!imageId.HasValue) return;
            var groupModel = _currentGroup.Model;
            var newNode = groupModel.InsertRootImage(groupModel.RootNodes.Count, imageId.Value);
            var grid = _currentGroup.Grid;
            var node = grid.Rows.AddNode(0);
            node.Data = newNode.Name;
            node.Key = newNode;
            node.Image = Resources.TreeDoc;
            grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "错误", scroll: false);
        }
    }
    private void CmdAppendRootImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendRootPdf_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var pdfId = SelectPdf();
            if (!pdfId.HasValue) return;
            var groupModel = _currentGroup.Model;
            var newNode = groupModel.InsertRootPdf(groupModel.RootNodes.Count, pdfId.Value);
            var grid = _currentGroup.Grid;
            var node = grid.Rows.AddNode(0);
            node.Data = newNode.Name;
            node.Key = newNode;
            node.Image = Resources.TreeDoc;
            grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "错误", scroll: false);
        }
    }
    private void CmdAppendRootPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootNode_Click(object sender, ClickEventArgs e)
    {
        if (ClipboardManager.Instance.ProjectHierarchyNode == null) return;
        if (ClipboardManager.Instance.ProjectHierarchyNode is TreeDirectoryNode)
        {
            CutPasteRoot();
        }
        else
        {
            DoPaste();
        }
    }
    private void CmdPasteRootNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootTable_Click(object sender, ClickEventArgs e)
    {
        CopyPasteRootTable();
    }
    private void CmdPasteRootTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootDocument_Click(object sender, ClickEventArgs e)
    {
        CopyPasteRootDocument();
    }
    private void CmdPasteRootDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootDirectory_Click(object sender, ClickEventArgs e)
    {
        CopyPasteRootDirectory();
    }
    private void CmdPasteRootDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootImage_Click(object sender, ClickEventArgs e)
    {
        CopyPasteRootImage();
    }
    private void CmdPasteRootImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootPdf_Click(object sender, ClickEventArgs e)
    {
        CopyPasteRootPdf();
    }
    private void CmdPasteRootPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 空白区域导入命令

    private void CmdEmptyImportFile_Click(object sender, ClickEventArgs e) => OnImportFile(null, EventArgs.Empty);
    private void CmdEmptyImportFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportExcel_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdEmptyImportExcel_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportWord_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdEmptyImportWord_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportImage_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdEmptyImportImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportPdf_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdEmptyImportPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportFolder_Click(object sender, ClickEventArgs e) => OnImportFolder(null, EventArgs.Empty);
    private void CmdEmptyImportFolder_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CopyPasteNonRootPdf()
    {
        try
        {
            var dup = (ClipboardManager.Instance.ProjectHierarchyNode as TreePdfNode)?.DuplicatePdf();
            if (dup == null) return;
            var grid = _currentGroup.Grid;
            var node = grid.Rows[grid.Row].Node;
            if (SelectedNode.IsRoot)
            {
                var rootNodes = _currentGroup.Model.RootNodes;
                if (rootNodes.Any(n => n.Name == dup.Name))
                    dup.Name += "-副本";
                _currentGroup.Model.InsertRootNode(dup, SelectedNode.Index);
                node = grid.Rows.AddNode(0);
                node.Data = dup.Name;
                node.Key = dup;
                node.Image = Resources.TreeDoc;
            }
            else
            {
                var parent = SelectedNode.Parent;
                var children = parent.Children;
                if (((List<TreeNodeBase>)children).Any(n => n.Name == dup.Name))
                    dup.Name += "-副本";
                parent.InsertChildNode(dup, SelectedNode.Index);
                node = node.AddNode(NodeTypeEnum.LastChild, dup.Name, dup, Resources.TreeDoc);
            }
            grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void EditNumber()
    {
        if (!HasWritePermission()) return;
        var number = SelectedNode.Number;
        var text = InputForm.Text("编辑索引号", "请输入或修改索引号：", number, 128);
        if (text == null) return;
        text = text.Replace("", "").Replace("\n", "");
        SelectedNode.UpdateNumber(text);
        _currentGroup.Grid.Invalidate();
    }

    private Guid? SelectImage()
    {
        var guid = Guid.NewGuid();
        var dialog = new OpenFileDialog
        {
            Filter = "支持的图片格式|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff|bmp|*.bmp|gif|*.gif|jpg|*.jpg;*.jpeg|png|*.png|tiff|*.tif;*.tiff",
            Multiselect = false,
            Title = "选择图片文件"
        };
        if (dialog.ShowDialog() != DialogResult.OK)
            return null;
        try
        {
            using (var image = System.Drawing.Image.FromFile(dialog.FileName))
            {
            }
            _currentGroup.Model.Project.FileCacheManager.CopyFrom(dialog.FileName, guid);
            return guid;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "打开图片文件时发生错误。", MessageBoxButtons.OK, "", scroll: false);
            return null;
        }
    }

    private Guid? SelectPdf()
    {
        var guid = Guid.NewGuid();
        var dialog = new OpenFileDialog
        {
            Filter = "PDF|*.pdf",
            Multiselect = false,
            Title = "选择 PDF 文件"
        };
        if (dialog.ShowDialog() != DialogResult.OK)
            return null;
        try
        {
            _currentGroup.Model.Project.FileCacheManager.CopyFrom(dialog.FileName, guid);
            return guid;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "打开 PDF 文件时发生错误。", MessageBoxButtons.OK, "", scroll: false);
            return null;
        }
    }

    private void CutPasteRoot()
    {
        var node = ClipboardManager.Instance.ProjectHierarchyNode;
        if (node == null) return;
        var gridNode = _currentGroup.Grid.Rows.AddNode(0);
        gridNode.Data = node.Name;
        gridNode.Key = node;
        gridNode.Image = _currentGroup.GetTreeNodeIcon(node);
        var dirNode = node as TreeDirectoryNode;
        if (dirNode != null)
            _currentGroup.PopulateDirectoryNode(dirNode, gridNode);
        node.MoveTo(_currentGroup.Model);
    }

    private void DoPaste()
    {
        if (_currentGroup == null || _currentGroup.Grid == null) return;
        var mode = _cutCopyMode;
        if (mode == CutCopyModeEnum.Cut)
        {
            var node = ClipboardManager.Instance.ProjectHierarchyNode;
            if (node == null) return;
            if (node.Status != SyncStatus.New && node.Status != SyncStatus.Synced)
                return;
            var row = FindNode(node);
            if (_currentGroup.Grid.Row >= 0)
                CutPasteNonRoot();
            else
                CutPasteRoot();
            if (row != null)
                row.RemoveNode();
            _cutCopyMode = CutCopyModeEnum.None;
            return;
        }
        if (mode == CutCopyModeEnum.Copy)
        {
            if (_currentGroup.Grid.Row >= 0)
            {
                var node = ClipboardManager.Instance.ProjectHierarchyNode;
                if (node is TreeTableNode)
                    CopyPasteNonRootTable();
                else if (node is TreeDocumentNode)
                    CopyPasteNonRootDocument();
                else if (node is TreeDirectoryNode)
                    CopyPasteNonRootDirectory();
                else if (node is TreeImageNode)
                    CopyPasteNonRootImage();
                else if (node is TreePdfNode)
                    CopyPasteNonRootPdf();
                return;
            }
            else
            {
                var node = ClipboardManager.Instance.ProjectHierarchyNode;
                if (node is TreeTableNode)
                    CopyPasteRootTable();
                else if (node is TreeDocumentNode)
                    CopyPasteRootDocument();
                else if (node is TreeDirectoryNode)
                    CopyPasteRootDirectory();
                else if (node is TreeImageNode)
                    CopyPasteRootImage();
                else if (node is TreePdfNode)
                    CopyPasteRootPdf();
                return;
            }
        }
    }

    private void ManageSnapshots()
    {
        if (SelectedNode == null) return;
        try
        {
            var form = new ManageSnapshots();
            if (form.ShowSnapshots() != DialogResult.OK) return;
            var snapshot = form.SelectedSnapshot;
            TreeNodeBase node = null;
            SDImage icon = null;
            switch (snapshot.Kind)
            {
                case 0: // Table
                    node = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotTable(snapshot);
                    icon = Resources.TreeTable;
                    break;
                case 1: // Document
                    var docNode = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotDocument(snapshot);
                    node = docNode;
                    icon = Resources.TreeDoc;
                    Program.MainForm.CurrentDocumentEditor = new DocumentEditor { Document = docNode.Document, NeedSave = true };
                    Program.MainForm.AddDocumentEditor(Program.MainForm.CurrentDocumentEditor);
                    Program.MainForm.CurrentDocumentEditor.PopulateDocument(false, true);
                    break;
                case 2: // Image
                    node = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotImage(snapshot);
                    icon = Resources.TreeDoc;
                    break;
                case 3: // Pdf
                    node = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotPdf(snapshot);
                    icon = Resources.TreeDoc;
                    break;
            }
            node.Name += " - 历史版本";
            var gridNode = _currentGroup.Grid.Rows.AddNode(0);
            gridNode.Data = node.Name;
            gridNode.Key = node;
            gridNode.Image = icon;
            _currentGroup.Model.InsertRootNode(node, _currentGroup.Model.RootNodes.Count);
            _currentGroup.Grid.Row = gridNode.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "由于此历史版本存储的版本过低，无法恢复。请选择更新的历史版本重试。", MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void SelectNodeImportWord()
    {
        var model = _currentGroup?.Model;
        if (model == null)
        {
            MessageBox.Show(MessageBoxIcon.None, "请选择导入分组", MessageBoxButtons.OK, "", scroll: false);
            return;
        }
        CreateImportIfNotExist();
        var dialog = new OpenFileDialog
        {
            Filter = "Word|*.docx",
            Title = "选择 Word 文件",
            Multiselect = true
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            ImportProject.AfterImportNode += ImportProject_AfterImportNode;
            var selectedNode = SelectedNode;
            if (selectedNode is TreeDirectoryNode dirNode)
            {
                ImportProject.ImportFiles(dirNode, dirNode.Children.Count, dialog.FileNames);
            }
            else
            {
                if (SelectedNode.Parent == null)
                {
                    ImportProject.ImportFiles(SelectedNode.Group, SelectedNode.Index, dialog.FileNames);
                }
                else
                {
                    ImportProject.ImportFiles(SelectedNode.Parent, SelectedNode.Index, dialog.FileNames);
                }
            }
        }
        finally
        {
            ImportProject.AfterImportNode -= ImportProject_AfterImportNode;
        }
        AddDocumentEditor(ImportProject.DocumentEditors);
    }

    private void CopyPasteRootTable()
    {
        try
        {
            var source = (TreeTableNode)ClipboardManager.Instance.ProjectHierarchyNode;
            var dup = source.DuplicateTable();
            if (dup == null) return;
            _dicDupFormula.Clear();
            _dicDupFormula[source.Id] = dup.Table;
            // dup.Table.DuplicateFormulas(_dicDupFormula);
            // if (source.ProjectGuid != Program.MainForm.CurrentProject.Guid)
            // {
            //     var formulas = dup.Table.GetAllFormulas();
            // }
            var rootNodes = _currentGroup.Model.RootNodes;
            if (rootNodes.Any(n => n.Name == dup.Name))
                dup.Name += "-副本";
            _currentGroup.Model.InsertRootNode(dup, _currentGroup.Model.RootNodes.Count);
            var node = _currentGroup.Grid.Rows.AddNode(0);
            node.Data = dup.Name;
            node.Key = dup;
            node.Image = Resources.TreeTable;
            _currentGroup.Grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteRootDocument()
    {
        try
        {
            var source = (TreeDocumentNode)ClipboardManager.Instance.ProjectHierarchyNode;
            var dup = source.DuplicateDocument();
            if (dup == null) return;
            Program.MainForm.CurrentProject.ThrowIfMaxExceeded();
            var rootNodes = _currentGroup.Model.RootNodes;
            if (rootNodes.Any(n => n.Name == dup.Name))
                dup.Name += "-副本";
            _currentGroup.Model.InsertRootNode(dup, _currentGroup.Model.RootNodes.Count);
            var node = _currentGroup.Grid.Rows.AddNode(0);
            node.Data = dup.Name;
            node.Key = dup;
            node.Image = Resources.TreeDoc;
            _currentGroup.Grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteRootDirectory()
    {
        try
        {
            var source = (TreeDirectoryNode)ClipboardManager.Instance.ProjectHierarchyNode;
            if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;
            var dup = source.DuplicateDirectory();
            if (dup == null) return;
            var sb = new System.Text.StringBuilder();
            DuplicateDirectory(source, dup, null, sb);
            // if (source.ProjectGuid != Program.MainForm.CurrentProject.Guid)
            // {
            //     foreach (var tableNode in dup.GetDescendants().OfType<TreeTableNode>())
            //     {
            //         _dicDupFormula.Clear();
            //         _dicDupFormula[source.Id] = tableNode.Table;
            //         tableNode.Table.DuplicateFormulas(_dicDupFormula);
            //     }
            // }
            if (sb.Length > 0)
            {
                MessageBox.Show(MessageBoxIcon.Error, "以下几个文件从服务器下载数据失败，请重试\r\n" + sb.ToString(), MessageBoxButtons.OK, "", scroll: false);
            }
            _currentGroup.Model.InsertRootNode(dup, _currentGroup.Model.RootNodes.Count);
            var node = _currentGroup.Grid.Rows.AddNode(0);
            node.Data = dup.Name;
            node.Key = dup;
            node.Image = Resources.TreeDir;
            _currentGroup.PopulateDirectoryNode(dup, node);
            _currentGroup.Grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteRootImage()
    {
        try
        {
            var source = (TreeImageNode)ClipboardManager.Instance.ProjectHierarchyNode;
            var dup = source.DuplicateImage();
            if (dup == null) return;
            var rootNodes = _currentGroup.Model.RootNodes;
            if (rootNodes.Any(n => n.Name == dup.Name))
                dup.Name += "-副本";
            _currentGroup.Model.InsertRootNode(dup, _currentGroup.Model.RootNodes.Count);
            var node = _currentGroup.Grid.Rows.AddNode(0);
            node.Data = dup.Name;
            node.Key = dup;
            node.Image = Resources.TreeDoc;
            _currentGroup.Grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteRootPdf()
    {
        try
        {
            var source = (TreePdfNode)ClipboardManager.Instance.ProjectHierarchyNode;
            var dup = source.DuplicatePdf();
            if (dup == null) return;
            var rootNodes = _currentGroup.Model.RootNodes;
            if (rootNodes.Any(n => n.Name == dup.Name))
                dup.Name += "-副本";
            _currentGroup.Model.InsertRootNode(dup, _currentGroup.Model.RootNodes.Count);
            var node = _currentGroup.Grid.Rows.AddNode(0);
            node.Data = dup.Name;
            node.Key = dup;
            node.Image = Resources.TreeDoc;
            _currentGroup.Grid.Row = node.Row.Index;
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void SelectNodeImportExcel()
    {
        var model = _currentGroup?.Model;
        if (model == null)
        {
            MessageBox.Show(MessageBoxIcon.None, "请选择导入分组", MessageBoxButtons.OK, "", scroll: false);
            return;
        }
        CreateImportIfNotExist();
        var dialog = new OpenFileDialog
        {
            Filter = "Excel|*.xls;*.xlsx",
            Title = "选择 Excel 文件",
            Multiselect = true
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            ImportProject.AfterImportNode += ImportProject_AfterImportNode;
            ImportProject.ImportFiles(_currentGroup.Model, _currentGroup.Model.RootNodes.Count, dialog.FileNames);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "导入失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
        finally
        {
            ImportProject.AfterImportNode -= ImportProject_AfterImportNode;
        }
        AddDocumentEditor(ImportProject.DocumentEditors);
    }

    private void SelectNodeImportImage()
    {
        var model = _currentGroup?.Model;
        if (model == null)
        {
            MessageBox.Show(MessageBoxIcon.None, "请选择导入分组", MessageBoxButtons.OK, "", scroll: false);
            return;
        }
        CreateImportIfNotExist();
        var dialog = new OpenFileDialog
        {
            Filter = "支持的图片格式|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff|bmp|*.bmp|gif|*.gif|jpg|*.jpg;*.jpeg|png|*.png|tiff|*.tif;*.tiff",
            Title = "选择图片文件",
            Multiselect = true
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            ImportProject.AfterImportNode += ImportProject_AfterImportNode;
            ImportProject.ImportFiles(_currentGroup.Model, _currentGroup.Model.RootNodes.Count, dialog.FileNames);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "导入失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
        finally
        {
            ImportProject.AfterImportNode -= ImportProject_AfterImportNode;
        }
        AddDocumentEditor(ImportProject.DocumentEditors);
    }

    private void SelectNodeImportPdf()
    {
        var model = _currentGroup?.Model;
        if (model == null)
        {
            MessageBox.Show(MessageBoxIcon.None, "请选择导入分组", MessageBoxButtons.OK, "", scroll: false);
            return;
        }
        CreateImportIfNotExist();
        var dialog = new OpenFileDialog
        {
            Filter = "PDF|*.pdf",
            Title = "选择 PDF 文件",
            Multiselect = false
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            ImportProject.AfterImportNode += ImportProject_AfterImportNode;
            ImportProject.ImportFiles(_currentGroup.Model, _currentGroup.Model.RootNodes.Count, dialog.FileNames);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, "导入失败！失败原因：" + ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
        finally
        {
            ImportProject.AfterImportNode -= ImportProject_AfterImportNode;
        }
        AddDocumentEditor(ImportProject.DocumentEditors);
    }

    #endregion

    #region 粘贴到指定位置实现

    private void GetPasteTarget(TreeNodeBase targetNode, out TreeDirectoryNode parentDir, out int insertIndex)
    {
        parentDir = null;
        insertIndex = 0;
        if (targetNode == null) return;

        if (targetNode is TreeDirectoryNode dirNode)
        {
            parentDir = dirNode;
            insertIndex = dirNode.Children.Count;
        }
        else
        {
            parentDir = targetNode.Parent;
            insertIndex = targetNode.Index + 1;
        }
    }

    private string GetUniqueName(string name, TreeDirectoryNode parentDir)
    {
        string newName = name;
        int suffix = 1;
        System.Collections.Generic.IEnumerable<TreeNodeBase> siblings;

        if (parentDir != null)
            siblings = parentDir.Children;
        else
            siblings = _currentGroup.Model.RootNodes;

        while (siblings.Any(n => n.Name == newName))
        {
            newName = name + "-副本" + (suffix > 1 ? suffix.ToString() : "");
            suffix++;
        }

        return newName;
    }

    private void CutPasteNonRoot()
    {
        try
        {
            var targetNode = SelectedNode as TreeNodeBase;
            var sourceNode = ClipboardManager.Instance.ProjectHierarchyNode;
            if (targetNode == null || sourceNode == null) return;

            GetPasteTarget(targetNode, out var parentDir, out var insertIndex);

            if (parentDir != null)
            {
                sourceNode.MoveTo(parentDir, insertIndex);
            }
            else
            {
                sourceNode.MoveTo(_currentGroup.Model, insertIndex);
            }

            Populate();
            FindAndSelectNode(sourceNode);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteNonRootTable()
    {
        try
        {
            var targetNode = SelectedNode as TreeNodeBase;
            var source = ClipboardManager.Instance.ProjectHierarchyNode as TreeTableNode;
            if (targetNode == null || source == null) return;

            var dup = source.DuplicateTable();
            if (dup == null) return;

            _dicDupFormula.Clear();
            _dicDupFormula[source.Id] = dup.Table;

            GetPasteTarget(targetNode, out var parentDir, out var insertIndex);
            dup.Name = GetUniqueName(dup.Name, parentDir);

            if (parentDir != null)
            {
                parentDir.InsertChildNode(dup, insertIndex);
            }
            else
            {
                _currentGroup.Model.InsertRootNode(dup, insertIndex);
            }

            Populate();
            FindAndSelectNode(dup);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteNonRootDocument()
    {
        try
        {
            var targetNode = SelectedNode as TreeNodeBase;
            var source = ClipboardManager.Instance.ProjectHierarchyNode as TreeDocumentNode;
            if (targetNode == null || source == null) return;

            var dup = source.DuplicateDocument();
            if (dup == null) return;

            Program.MainForm.CurrentProject.ThrowIfMaxExceeded();

            GetPasteTarget(targetNode, out var parentDir, out var insertIndex);
            dup.Name = GetUniqueName(dup.Name, parentDir);

            if (parentDir != null)
            {
                parentDir.InsertChildNode(dup, insertIndex);
            }
            else
            {
                _currentGroup.Model.InsertRootNode(dup, insertIndex);
            }

            Populate();
            FindAndSelectNode(dup);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteNonRootDirectory()
    {
        try
        {
            var targetNode = SelectedNode as TreeNodeBase;
            var source = ClipboardManager.Instance.ProjectHierarchyNode as TreeDirectoryNode;
            if (targetNode == null || source == null) return;

            if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit(() => GetAllFileNodesTotalCount())) return;

            var dup = source.DuplicateDirectory();
            if (dup == null) return;

            var sb = new System.Text.StringBuilder();
            DuplicateDirectory(source, dup, null, sb);

            if (sb.Length > 0)
            {
                MessageBox.Show(MessageBoxIcon.Error, "以下几个文件从服务器下载数据失败，请重试\r\n" + sb.ToString(), MessageBoxButtons.OK, "", scroll: false);
            }

            GetPasteTarget(targetNode, out var parentDir, out var insertIndex);
            dup.Name = GetUniqueName(dup.Name, parentDir);

            if (parentDir != null)
            {
                parentDir.InsertChildNode(dup, insertIndex);
            }
            else
            {
                _currentGroup.Model.InsertRootNode(dup, insertIndex);
            }

            Populate();
            FindAndSelectNode(dup);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    private void CopyPasteNonRootImage()
    {
        try
        {
            var targetNode = SelectedNode as TreeNodeBase;
            var source = ClipboardManager.Instance.ProjectHierarchyNode as TreeImageNode;
            if (targetNode == null || source == null) return;

            var dup = source.DuplicateImage();
            if (dup == null) return;

            GetPasteTarget(targetNode, out var parentDir, out var insertIndex);
            dup.Name = GetUniqueName(dup.Name, parentDir);

            if (parentDir != null)
            {
                parentDir.InsertChildNode(dup, insertIndex);
            }
            else
            {
                _currentGroup.Model.InsertRootNode(dup, insertIndex);
            }

            Populate();
            FindAndSelectNode(dup);
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }

    #endregion

    private void CreateImportIfNotExist()
    {
    }

    private void ImportProject_AfterImportNode(object sender, EventArgs e)
    {
    }

    private void AddDocumentEditor(List<DocumentEditor> editors)
    {
    }

    private void DuplicateDirectory(TreeDirectoryNode source, TreeDirectoryNode dup, Node node, System.Text.StringBuilder sb)
    {
    }

    private void RefreshOpenNode(bool b)
    {
    }

    private void SetCutInfo()
    {
        var node = SelectedNode as TreeNodeBase;
        if (node == null) return;
        _cutCopyMode = CutCopyModeEnum.Cut;
        CutCopyMode = CutCopyModeEnum.Cut;
        ClipboardManager.Instance.ProjectHierarchyNode = node;
    }

    private bool ShowUnableCutDialog(TreeNodeBase node)
    {
        return false;
    }

    private bool ShowUnableCopyDialog(TreeNodeBase node)
    {
        return false;
    }

    #endregion
}
