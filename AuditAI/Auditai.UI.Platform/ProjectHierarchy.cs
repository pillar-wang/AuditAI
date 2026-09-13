﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
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

    // 修复 BUG: 此前批量导出/打印按钮无防重复点击保护，耗时操作期间用户可多次点击，
    // 导致并发批量任务，资源占用激增。使用原子标志防止重入。
    private int _isBatchProcessing;

    // 修复 BUG: 树节点左侧展开/折叠按钮区域点击不应触发节点选中导航
    // 在 BeforeMouseDown 中检测并标记，在 MouseClick 中据此阻止 TreeNodeSelected 事件
    private bool _isTreeButtonAreaClicked;

    // 修复 BUG: 双击节点时 MouseClick 会触发 2 次、MouseDoubleClick 再触发 1 次
    // TreeNodeSelected，导致重复异步导航产生竞态（表格重复打开/误切视图）。通过
    // 时间差+同行检测，将双击压缩为仅第一击导航一次，其余击跳过。双击相对"打开"的
    // 额外动作（如展开下级文件夹）在 MouseDoubleClick 中单独处理。
    private const long DoubleClickSlopTicks = 400L * TimeSpan.TicksPerMillisecond;
    private long _lastClickNavTicks = -1;
    private int _lastClickNavRow = -1;

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
    private C1Command cmdFillStatusReport = new C1Command();

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
    private C1CommandLink lnkFillStatusReport = new C1CommandLink();
    private C1CommandLink lnkFillStatusReport2 = new C1CommandLink();
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
            if (node is TreeDirectoryNode) return IconRes.TreeDir;
            if (node is TreeDocumentNode) return IconRes.TreeDoc;
            if (node is TreeTableNode) return IconRes.TreeTable;
            if (node is TreeImageNode) return IconRes.TreeDoc;
            if (node is TreePdfNode) return IconRes.TreeDoc;
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
            SelectionMode = SelectionModeEnum.Cell,
            Font = new Font("微软雅黑", 10.5f)
        };
        _grid.Rows.Count = 0;
        _grid.Rows.Fixed = 0;
        _grid.Cols.Count = 1;
        _grid.Cols.Fixed = 0;
        _grid.Tree.Column = 0;
        _grid.Rows.DefaultSize = 40;
        _grid.Cols[0].Width = 200;
        _grid.MouseClick += _grid_MouseClick;
        _grid.MouseDoubleClick += _grid_MouseDoubleClick;
        _grid.BeforeMouseDown += _grid_BeforeMouseDown;

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
        cmdRenameGroup.Image = Auditai.UI.Platform.IconRes.ctxMofify;
        cmdPasteGroup.Text = "粘贴分组";
        cmdPasteGroupClickOnGridTree.Text = "粘贴分组";

        cmdInsertDirectory.Text = "新建文件夹";
        cmdInsertTable.Text = "新建表格";
        cmdInsertTable.Image = Auditai.UI.Platform.IconRes.ctxInsertTable;
        cmdInsertDocument.Text = "新建文档";
        cmdInsertImage.Text = "新建图片";
        cmdInsertImage.Image = Auditai.UI.Platform.IconRes.ctxInsertImage;
        cmdInsertPdf.Text = "新建PDF";

        cmdAppendChildDirectory.Text = "追加文件夹";
        cmdAppendChildTable.Text = "追加表格";
        cmdAppendChildDocument.Text = "追加文档";
        cmdAppendChildImage.Text = "追加图片";
        cmdAppendChildPdf.Text = "追加PDF";

        cmdMoveUpNode.Text = "上移";
        cmdMoveDownNode.Text = "下移";
        cmdRemoveNode.Text = "删除";
        cmdRemoveNode.Image = Auditai.UI.Platform.IconRes.ctxDelete;
        cmdHideNode.Text = "隐藏";

        cmdShowNodes.Text = "取消隐藏";
        cmdShowNodes.Image = Auditai.UI.Platform.IconRes.ctxSearch;
        cmdSearchNodes.Text = "搜索节点";
        cmdSearchNodes.Image = Auditai.UI.Platform.IconRes.ctxSearch;

        cmdFillStatusReport.Text = "填报情况统计";
        cmdFillStatusReport.Image = Auditai.UI.Platform.IconRes.FillStatusReport16;

        cmdCutNode.Text = "剪切";
        cmdCutNode.Image = Auditai.UI.Platform.IconRes.ctxCut;

        cmdCopy.Text = "复制";
        cmdCopy.Image = Auditai.UI.Platform.IconRes.ctxCopy;

        cmdPasteNode.Text = "粘贴";
        cmdPasteNode.Image = Auditai.UI.Platform.IconRes.ctxPaste;

        cmdRenameNode.Text = "重命名";
        cmdRenameNode.Image = Auditai.UI.Platform.IconRes.ctxMofify;
        cmdEditNumber.Text = "编辑索引号";
        cmdEditNumber.Image = Auditai.UI.Platform.IconRes.ctxNumber;
        cmdReload.Text = "重新加载";
        cmdReload.Image = Auditai.UI.Platform.IconRes.ctxReloadFile;
        cmdSyncTable.Text = "同步表格";
        cmdSyncTable.Image = Auditai.UI.Platform.IconRes.ctxRefreshTable;
        cmdSyncDocument.Text = "同步文档";
        cmdSyncDocument.Image = Auditai.UI.Platform.IconRes.ctxRefresh;

        cmdNodeImportFile.Text = "导入文件";
        cmdNodeImportFile.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdNodeImportExcel.Text = "导入Excel";
        cmdNodeImportExcel.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdNodeImportWord.Text = "导入Word";
        cmdNodeImportWord.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdNodeImportImage.Text = "导入图片";
        cmdNodeImportImage.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdNodeImportPdf.Text = "导入PDF";
        cmdNodeImportPdf.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdNodeImportFolder.Text = "导入文件夹";
        cmdNodeImportFolder.Image = Auditai.UI.Platform.IconRes.ctxImport;

        cmdAppendRootDirectory.Text = "新建文件夹";
        cmdAppendRootTable.Text = "新建表格";
        cmdAppendRootTable.Image = Auditai.UI.Platform.IconRes.ctxInsertTable;
        cmdAppendRootDocument.Text = "新建文档";
        cmdAppendRootImage.Text = "新建图片";
        cmdAppendRootImage.Image = Auditai.UI.Platform.IconRes.ctxInsertImage;
        cmdAppendRootPdf.Text = "新建PDF";

        cmdPasteRootNode.Text = "粘贴";
        cmdPasteRootNode.Image = Auditai.UI.Platform.IconRes.ctxPaste;

        cmdEmptyImportFile.Text = "导入文件";
        cmdEmptyImportFile.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdEmptyImportExcel.Text = "导入Excel";
        cmdEmptyImportExcel.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdEmptyImportWord.Text = "导入Word";
        cmdEmptyImportWord.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdEmptyImportImage.Text = "导入图片";
        cmdEmptyImportImage.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdEmptyImportPdf.Text = "导入PDF";
        cmdEmptyImportPdf.Image = Auditai.UI.Platform.IconRes.ctxImport;
        cmdEmptyImportFolder.Text = "导入文件夹";
        cmdEmptyImportFolder.Image = Auditai.UI.Platform.IconRes.ctxImport;

        // ---- 补充缺失的图标 ----
        cmdMoveUpGroup.Image = Auditai.UI.Platform.IconRes.MoveUp;
        cmdMoveDownGroup.Image = Auditai.UI.Platform.IconRes.MoveDown;
        cmdRemoveGroup.Image = Auditai.UI.Platform.IconRes.ctxDelete;
        cmdCopyGroup.Image = Auditai.UI.Platform.IconRes.ctxCopy;

        cmdMoveUpNode.Image = Auditai.UI.Platform.IconRes.MoveUp;
        cmdMoveDownNode.Image = Auditai.UI.Platform.IconRes.MoveDown;
        cmdHideNode.Image = Auditai.UI.Platform.IconRes.HideNodes;

        cmdInsertDirectory.Image = Auditai.UI.Platform.IconRes.TreeDir;
        cmdInsertDocument.Image = Auditai.UI.Platform.IconRes.TreeDoc;
        cmdInsertPdf.Image = Auditai.UI.Platform.IconRes.TreeDoc;

        cmdAppendChildDirectory.Image = Auditai.UI.Platform.IconRes.TreeDir;
        cmdAppendChildTable.Image = Auditai.UI.Platform.IconRes.ctxInsertTable;
        cmdAppendChildDocument.Image = Auditai.UI.Platform.IconRes.TreeDoc;
        cmdAppendChildImage.Image = Auditai.UI.Platform.IconRes.ctxInsertImage;
        cmdAppendChildPdf.Image = Auditai.UI.Platform.IconRes.TreeDoc;

        cmdAppendRootDirectory.Image = Auditai.UI.Platform.IconRes.TreeDir;
        cmdAppendRootDocument.Image = Auditai.UI.Platform.IconRes.TreeDoc;
        cmdAppendRootPdf.Image = Auditai.UI.Platform.IconRes.TreeDoc;

        // ---- 补充缺失的命令图标 ----
        cmdAddGroup.Image = Auditai.UI.Platform.IconRes.ctxAppendRow;
        cmdPasteGroup.Image = Auditai.UI.Platform.IconRes.ctxPaste;
        cmdPasteGroupClickOnGridTree.Image = Auditai.UI.Platform.IconRes.ctxPaste;

        // ---- 设置子菜单 ----
        mnuInsert.Text = "新建";
        mnuInsert.Image = Auditai.UI.Platform.IconRes.ctxInsertTable;
        mnuAppendChild.Text = "追加";
        mnuAppendChild.Image = Auditai.UI.Platform.IconRes.ctxAppendRow;
        mnuNodeImport.Text = "导入";
        mnuNodeImport.Image = Auditai.UI.Platform.IconRes.ctxImport;
        mnuAppendRoot.Text = "新建";
        mnuAppendRoot.Image = Auditai.UI.Platform.IconRes.ctxInsertTable;
        mnuEmptyImport.Text = "导入";
        mnuEmptyImport.Image = Auditai.UI.Platform.IconRes.ctxImport;

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
        cmdReload.Image = Auditai.UI.Platform.IconRes.ctxReloadFile;
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
        ctxBatchOperation.Image = Auditai.UI.Platform.IconRes.BatchOperation16;
        lnkBatchOperation.Command = ctxBatchOperation;
        ctxTreeNode.CommandLinks.Add(lnkBatchOperation);

        // cmdBatchHideFile
        cmdBatchHideFile.Text = "批量隐藏文件";
        cmdBatchHideFile.Image = Auditai.UI.Platform.IconRes.BatchHideNodes16;
        cmdBatchHideFile.Click += CmdBatchHideFile_Click;
        lnkBatchHideFile.Command = cmdBatchHideFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchHideFile);

        // cmdBatchUnhideFile
        cmdBatchUnhideFile.Text = "批量取消隐藏";
        cmdBatchUnhideFile.Image = Auditai.UI.Platform.IconRes.ctxSearch;
        cmdBatchUnhideFile.Click += CmdBatchUnhideFile_Click;
        cmdBatchUnhideFile.CommandStateQuery += CmdBatchUnhideFile_CommandStateQuery;
        lnkBatchUnhideFile.Command = cmdBatchUnhideFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchUnhideFile);

        // cmdBatchDeleteFile
        cmdBatchDeleteFile.Text = "批量删除文件";
        cmdBatchDeleteFile.Image = Auditai.UI.Platform.IconRes.BatchRemoveNodes16;
        cmdBatchDeleteFile.Click += CmdBatchDeleteFile_Click;
        cmdBatchDeleteFile.CommandStateQuery += CmdBatchDeleteFile_CommandStateQuery;
        lnkBatchDeleteFile.Command = cmdBatchDeleteFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchDeleteFile);

        // cmdBatchEditIndex
        cmdBatchEditIndex.Text = "批量编辑索引号";
        cmdBatchEditIndex.Image = Auditai.UI.Platform.IconRes.EditNodesNumber16;
        cmdBatchEditIndex.Click += CmdBatchEditIndex_Click;
        cmdBatchEditIndex.CommandStateQuery += CmdBatchEditIndex_CommandStateQuery;
        lnkBatchEditIndex.Command = cmdBatchEditIndex;
        ctxBatchOperation.CommandLinks.Add(lnkBatchEditIndex);

        // cmdBatchExportFile
        cmdBatchExportFile.Text = "批量导出文件";
        cmdBatchExportFile.Image = Auditai.UI.Platform.IconRes.BatchExport16;
        cmdBatchExportFile.Click += CmdBatchExportFile_Click;
        lnkBatchExportFile.Command = cmdBatchExportFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchExportFile);

        // cmdBatchPrintFile
        cmdBatchPrintFile.Text = "批量打印文件";
        cmdBatchPrintFile.Image = Auditai.UI.Platform.IconRes.BatchPrint16;
        cmdBatchPrintFile.Click += CmdBatchPrintFile_Click;
        lnkBatchPrintFile.Command = cmdBatchPrintFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchPrintFile);

        // cmdFillStatusReport
        cmdFillStatusReport.CommandStateQuery += CmdFillStatusReport_CommandStateQuery;
        cmdFillStatusReport.Click += CmdFillStatusReport_Click;
        lnkFillStatusReport.Command = cmdFillStatusReport;
        lnkFillStatusReport.Delimiter = true;
        ctxTreeNode.CommandLinks.Add(lnkFillStatusReport);

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

        // lnkFillStatusReport2 -> cmdFillStatusReport (ctxTreeEmpty)
        lnkFillStatusReport2.Command = cmdFillStatusReport;
        ctxTreeEmpty.CommandLinks.Add(lnkFillStatusReport2);

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

        if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, $"确定要删除分组 \"{selectedGroup.Name}\" 吗？",
            MessageBoxButtons.YesNo, "确认删除") != DialogResult.Yes)
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除分组失败：" + ex.Message, MessageBoxButtons.OK, "错误");
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "复制分组失败：" + ex.Message, MessageBoxButtons.OK, "错误");
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
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "导入文件失败：" + ex.Message, MessageBoxButtons.OK, "错误");
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
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "导入文件夹失败：" + ex.Message, MessageBoxButtons.OK, "错误");
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

        if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, $"确定要删除 \"{selectedNode.Name}\" 吗？",
            MessageBoxButtons.YesNo, "确认删除") != DialogResult.Yes)
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, "删除失败：" + ex.Message, MessageBoxButtons.OK, "错误");
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
                node.Image = IconRes.TreeDir;

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
            node = parentNode.AddNode(NodeTypeEnum.LastChild, dirNode.Name, dirNode, IconRes.TreeDir);
            foreach (TreeNodeBase child in dirNode.Children)
            {
                AddTreeNode(child, node);
            }
            node.Expanded = false;
        }
        else if (treeNode is TreeTableNode tableNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, tableNode.Name, tableNode, IconRes.TreeTable);
        }
        else if (treeNode is TreeDocumentNode docNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, docNode.Name, docNode, IconRes.TreeDoc);
        }
        else if (treeNode is TreeImageNode imgNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, imgNode.Name, imgNode, IconRes.TreeDoc);
        }
        else if (treeNode is TreePdfNode pdfNode)
        {
            node = parentNode.AddNode(NodeTypeEnum.LastChild, pdfNode.Name, pdfNode, IconRes.TreeDoc);
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

    /// <summary>
    /// 应用 Google Blue 主题精修
    /// 在 C1Theme 基础上叠加导航树特有的样式调整
    /// </summary>
    public void SetTheme()
    {
        var theme = Theme.SelectedAuditaiTheme;
        if (theme == null || theme.Name != "auditai_GoogleBlue")
        {
            return;
        }

        // === 字体与行高 ===
        _grid.Font = AuditTheme.FontBody;
        _grid.Rows.DefaultSize = 28;

        // === 基础样式 ===
        // 正常行：白底深字
        _grid.Styles.Normal.ForeColor = AuditTheme.Text;
        _grid.Styles.Normal.BackColor = AuditTheme.Surface;
        _grid.Styles.Normal.TextAlign = TextAlignEnum.LeftCenter;

        // 选中行：淡蓝背景 + 深色文字（Google 风格不做全蓝填充）
        _grid.Styles.Highlight.BackColor = AuditTheme.BrandSubtle;
        _grid.Styles.Highlight.ForeColor = AuditTheme.Text;
        _grid.Styles.Highlight.Font = AuditTheme.FontBodyBold;

        // 焦点单元格：与选中行一致（去掉焦点框的割裂感）
        _grid.Styles.Focus.BackColor = AuditTheme.BrandSubtle;
        _grid.Styles.Focus.ForeColor = AuditTheme.Text;
        _grid.Styles.Focus.Font = AuditTheme.FontBodyBold;

        // === 树形结构样式 ===
        // 缩进量（16px，4px 网格的 4 倍）
        _grid.Tree.Indent = 16;

        // 树线样式：细灰线（Google 风格简洁、低调）
        _grid.Tree.Style = TreeStyleFlags.Simple;
        _grid.Tree.LineColor = AuditTheme.Border;

        // === 网格线与边框 ===
        _grid.Cols[0].StyleDisplay.Border.Color = AuditTheme.Border;
        _grid.Cols[0].StyleDisplay.Border.Direction = BorderDirEnum.Horizontal;

        // 去掉选中时的虚线焦点框（Google 风格不用虚线框）
        _grid.FocusRect = FocusRectEnum.None;

        // 选中整行模式（更现代的选中方式）
        _grid.SelectionMode = SelectionModeEnum.Row;

        // 行高微调：叶子节点和组节点统一 28px
        _grid.Rows.DefaultSize = 28;

        // === C1OutBar 外层样式 ===
        if (View is C1OutBarEx outBar)
        {
            // C1OutBar 背景色
            outBar.BackColor = AuditTheme.Surface;
            outBar.ForeColor = AuditTheme.Text;

            // 页面标题样式（分组标题栏）
            foreach (C1OutPage page in outBar.Pages)
            {
                page.BackColor = AuditTheme.Surface;
                page.ForeColor = AuditTheme.Text;
            }
        }

        // 注册自绘事件，实现选中行左侧蓝色指示条
        _grid.DrawMode = DrawModeEnum.OwnerDraw;
        _grid.OwnerDrawCell -= Grid_OwnerDrawCell;
        _grid.OwnerDrawCell += Grid_OwnerDrawCell;
    }

    /// <summary>
    /// 导航树自绘：实现 Google 风格的选中行左侧蓝色指示条
    /// </summary>
    private void Grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
    {
        // 只处理普通行（非固定行）
        if (e.Row < _grid.Rows.Fixed) return;

        // 判断是否为选中行
        bool isSelected = _grid.Selection.Contains(e.Row, e.Col);
        bool isHot = _grid.MouseRow == e.Row && _grid.MouseCol == e.Col;

        // 先让 C1 画默认内容
        e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Border | DrawCellFlags.Content);

        // 选中行：左侧画 3px 蓝色指示条
        if (isSelected)
        {
            using (var brush = new SolidBrush(AuditTheme.Brand))
            {
                e.Graphics.FillRectangle(brush, e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);
            }
        }

        // 告诉 C1 我们已经画完了
        e.Handled = true;
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

    /// <summary>
    /// 检测点击是否在树节点的展开/折叠按钮区域（单元格左侧约20像素）。
    /// 使用 BeforeMouseDown 事件参数进行命中测试，比 MouseClick 更可靠。
    /// </summary>
    private bool IsTreeButtonArea(int x, int y)
    {
        HitTestInfo hitTestInfo = _grid.HitTest(x, y);
        if (hitTestInfo.Type != HitTestTypeEnum.Cell)
            return false;

        if (hitTestInfo.Column != _grid.Tree.Column)
            return false;

        C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
        if (!row.IsNode)
            return false;

        // 获取单元格矩形，树按钮（折叠箭头+节点图标）区域约占单元格左侧若干像素。
        // 取 32px：除覆盖折叠箭头本身外，也覆盖箭头右侧一小段——这里点击本意是折叠/展开，
        // 若判定窗口过窄会被当作普通节点导航触发误切换。
        Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
        const int treeButtonAreaWidth = 32;
        return (x - cellRect.Left) < treeButtonAreaWidth && (x - cellRect.Left) >= 0;
    }

    /// <summary>
    /// 在鼠标按下前检测是否点击了树按钮区域，若是则标记状态供 MouseClick 使用。
    /// 这能可靠地阻止点击树按钮时触发节点选中导航事件。
    /// </summary>
    private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isTreeButtonAreaClicked = IsTreeButtonArea(e.X, e.Y);
        }
        else
        {
            _isTreeButtonAreaClicked = false;
        }
    }

    private void _grid_MouseClick(object sender, MouseEventArgs e)
    {
        // 使用事件坐标命中，避免无参 HitTest()（取内部当前鼠标位置）在滚动/布局后与事件坐标错位，
        // 导致点折叠箭头附近误命中其他位置节点而触发导航。
        HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
        if (hitTestInfo.Type == HitTestTypeEnum.Cell)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
            if (row.IsNode)
            {
                Node node = row.Node;

                // 优先使用 BeforeMouseDown 中计算的标记；若不可用则回退到像素检测
                bool isTreeButtonClick = _isTreeButtonAreaClicked;
                if (!isTreeButtonClick && e.Button == MouseButtons.Left)
                {
                    isTreeButtonClick = IsTreeButtonArea(e.X, e.Y);
                }

                if (node.Key is TreeGroup group)
                {
                    // 点击树按钮区域时不更新分组模型，避免误触导致分组切换
                    if (!isTreeButtonClick)
                    {
                        SelectedNode = group;
                        UpdateCurrentGroupModel(group);
                    }
                }
                else if (node.Key is TreeNodeBase tnb)
                {
                    // 点击树按钮区域时不更新选中状态，防止触发选中导航事件
                    if (!isTreeButtonClick)
                    {
                        // 双击去重：仅第一击触发导航。同一行在很短时间内再次点击
                        // （双击的第二击）跳过，交给 MouseDoubleClick 统一"打开"，
                        // 避免 3 次异步导航竞态导致表格重复打开或误切视图。
                        long nowTicks = System.DateTime.Now.Ticks;
                        bool isSecondClickOfDoubleClick = _lastClickNavRow == hitTestInfo.Row
                            && nowTicks - _lastClickNavTicks <= DoubleClickSlopTicks;
                        _lastClickNavTicks = nowTicks;
                        _lastClickNavRow = hitTestInfo.Row;
                        if (!isSecondClickOfDoubleClick)
                        {
                            SelectedNode = tnb;
                            UpdateCurrentGroupModel(tnb.Group);
                            TreeNodeSelected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }

                // 仅在点击树按钮区域时触发展开/折叠
                if (e.Button == MouseButtons.Left && isTreeButtonClick)
                {
                    node.Collapsed = !node.Collapsed;
                }
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
                        NativeMenuShim.Show(ctxTreeGroup, _grid, e.Location);
                    }
                    else if (node.Key is TreeNodeBase tnb)
                    {
                        SelectedNode = tnb;
                        UpdateCurrentGroupModel(tnb.Group);
                        _grid.Row = ht.Row;
                        NativeMenuShim.Show(ctxTreeNode, _grid, e.Location);
                    }
                }
                else
                {
                    NativeMenuShim.Show(ctxTreeEmpty, _grid, e.Location);
                }
            }
            else
            {
                NativeMenuShim.Show(ctxTreeNothing, _grid, e.Location);
            }
        }

        // 重置树按钮点击标记
        _isTreeButtonAreaClicked = false;
    }

    private void View_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;

        var outBar = (C1OutBarEx)sender;
        if (outBar.HotPage == null)
            NativeMenuShim.Show(ctxTreeNothing, View, e.Location);
        else
            NativeMenuShim.Show(ctxTreeGroup, View, e.Location);
    }

    private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
        if (hitTestInfo.Type == HitTestTypeEnum.Cell)
        {
            C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
            if (row.IsNode)
            {
                // 双击也需检查是否点击在树按钮区域，该区域交给折叠逻辑处理
                bool isTreeButtonClick = IsTreeButtonArea(e.X, e.Y);
                if (!isTreeButtonClick)
                {
                    Node node = row.Node;
                    // 双击"打开"的额外动作：
                    // - 文件夹节点：展开下级子项（第一击已触发选中，这里补充展开）
                    // - 表格/文档/图片/PDF 节点：第一击已通过 TreeNodeSelected 打开，
                    //   这里不再触发导航，避免 3 次异步导航竞态（重复打开/误切视图）。
                    if (node.Key is TreeDirectoryNode)
                    {
                        node.Collapsed = false;
                    }
                }
            }
        }
        _lastClickNavTicks = System.DateTime.Now.Ticks;
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
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, IconRes.TreeDoc);
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
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, IconRes.TreeDoc);
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
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, IconRes.TreeDoc);
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
        var node = grid.Rows[grid.Row].Node.AddNode(NodeTypeEnum.LastChild, newNode.Name, newNode, IconRes.TreeDoc);
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
            if (node is TreeTableNode && MessageBox.Show(MessageBoxIcon.Question, "设置后批量校验、全量重算、批量导出、批量打印将自动跳过该表格；可随时在【填报情况统计】中恢复填报。确定将该表格设为【不必填报】吗？", MessageBoxButtons.OKCancel, "不必填报", scroll: false) != DialogResult.OK)
            {
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
            if (node is TreeTableNode)
            {
                cmdHideNode.Text = node.Visible ? "不必填报" : "恢复填报";
            }
            else
            {
                cmdHideNode.Text = node.Visible ? "隐藏" : "取消隐藏";
            }
        }
    }

    private void CmdFillStatusReport_Click(object sender, ClickEventArgs e)
    {
        try
        {
            var form = new frmFillStatusReport();
            form.Project = Project;
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
    }
    private void CmdFillStatusReport_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Enabled = SelectedNode == null || SelectedNode is TreeGroup || SelectedNode is TreeDirectoryNode;
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
        // 修复 BUG: 防止用户在 BatchExport 期间重复点击触发并发批量任务。
        if (System.Threading.Interlocked.CompareExchange(ref _isBatchProcessing, 1, 0) != 0) return;
        try
        {
            await Program.MainForm.BatchExport("批量导出");
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref _isBatchProcessing, 0);
        }
    }

    private async void CmdBatchPrintFile_Click(object sender, ClickEventArgs e)
    {
        // 修复 BUG: 防止用户在 BatchPrint 期间重复点击触发并发批量任务。
        if (System.Threading.Interlocked.CompareExchange(ref _isBatchProcessing, 1, 0) != 0) return;
        try
        {
            await Program.MainForm.BatchPrint_Click("批量打印");
        }
        catch (Exception ex)
        {
            ex.Log(null);
            MessageBox.Show(MessageBoxIcon.Error, ex.Message, MessageBoxButtons.OK, "", scroll: false);
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref _isBatchProcessing, 0);
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
            node.Image = IconRes.TreeDoc;
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
            node.Image = IconRes.TreeDoc;
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

    private void CmdEmptyImportExcel_Click(object sender, ClickEventArgs e) => SelectNodeImportExcel();
    private void CmdEmptyImportExcel_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportWord_Click(object sender, ClickEventArgs e) => SelectNodeImportWord();
    private void CmdEmptyImportWord_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportImage_Click(object sender, ClickEventArgs e) => SelectNodeImportImage();
    private void CmdEmptyImportImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEmptyImportPdf_Click(object sender, ClickEventArgs e) => SelectNodeImportPdf();
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
                node.Image = IconRes.TreeDoc;
            }
            else
            {
                var parent = SelectedNode.Parent;
                var children = parent.Children;
                if (((List<TreeNodeBase>)children).Any(n => n.Name == dup.Name))
                    dup.Name += "-副本";
                parent.InsertChildNode(dup, SelectedNode.Index);
                node = node.AddNode(NodeTypeEnum.LastChild, dup.Name, dup, IconRes.TreeDoc);
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
        using (var dialog = new OpenFileDialog
        {
            Filter = "支持的图片格式|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff|bmp|*.bmp|gif|*.gif|jpg|*.jpg;*.jpeg|png|*.png|tiff|*.tif;*.tiff",
            Multiselect = false,
            Title = "选择图片文件"
        })
        {
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
    }

    private Guid? SelectPdf()
    {
        var guid = Guid.NewGuid();
        using (var dialog = new OpenFileDialog
        {
            Filter = "PDF|*.pdf",
            Multiselect = false,
            Title = "选择 PDF 文件"
        })
        {
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
                    icon = IconRes.TreeTable;
                    break;
                case 1: // Document
                    var docNode = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotDocument(snapshot);
                    node = docNode;
                    icon = IconRes.TreeDoc;
                    Program.MainForm.CurrentDocumentEditor = new DocumentEditor { Document = docNode.Document, NeedSave = true };
                    Program.MainForm.AddDocumentEditor(Program.MainForm.CurrentDocumentEditor);
                    Program.MainForm.CurrentDocumentEditor.PopulateDocument(false, true);
                    break;
                case 2: // Image
                    node = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotImage(snapshot);
                    icon = IconRes.TreeDoc;
                    break;
                case 3: // Pdf
                    node = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshotPdf(snapshot);
                    icon = IconRes.TreeDoc;
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
        using (var dialog = new OpenFileDialog
        {
            Filter = "Word|*.docx",
            Title = "选择 Word 文件",
            Multiselect = true
        })
        {
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                ImportProject.AfterImportNode += ImportProject_AfterImportNode;
                var selectedNode = SelectedNode;
                if (selectedNode is TreeDirectoryNode dirNode)
                {
                    ImportProject.ImportFiles(dirNode, dirNode.Children.Count, dialog.FileNames);
                }
                else if (selectedNode is TreeDocumentNode || selectedNode is TreeTableNode
                         || selectedNode is TreeImageNode || selectedNode is TreePdfNode)
                {
                    if (selectedNode.Parent == null)
                    {
                        ImportProject.ImportFiles(selectedNode.Group, selectedNode.Index, dialog.FileNames);
                    }
                    else
                    {
                        ImportProject.ImportFiles(selectedNode.Parent, selectedNode.Index, dialog.FileNames);
                    }
                }
            }
            finally
            {
                ImportProject.AfterImportNode -= ImportProject_AfterImportNode;
            }
            AddDocumentEditor(ImportProject.DocumentEditors);
        }
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
            node.Image = IconRes.TreeTable;
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
            node.Image = IconRes.TreeDoc;
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
            node.Image = IconRes.TreeDir;
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
            node.Image = IconRes.TreeDoc;
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
            node.Image = IconRes.TreeDoc;
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
        using (var dialog = new OpenFileDialog
        {
            Filter = "Excel|*.xls;*.xlsx",
            Title = "选择 Excel 文件",
            Multiselect = true
        })
        {
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
        using (var dialog = new OpenFileDialog
        {
            Filter = "支持的图片格式|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff|bmp|*.bmp|gif|*.gif|jpg|*.jpg;*.jpeg|png|*.png|tiff|*.tif;*.tiff",
            Title = "选择图片文件",
            Multiselect = true
        })
        {
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
        using (var dialog = new OpenFileDialog
        {
            Filter = "PDF|*.pdf",
            Title = "选择 PDF 文件",
            Multiselect = false
        })
        {
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
        if (ImportProject == null)
        {
            ImportProject = new ProjectImport(View);
        }
    }

    private void ImportProject_AfterImportNode(object sender, ImportNodeArgs e)
    {
        if (firstImportNode == null && !(e.AppendNode is TreeDirectoryNode))
        {
            firstImportNode = e.AppendNode;
        }
        try
        {
            AddImportedNodeToGrid(e);
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    private void AddImportedNodeToGrid(ImportNodeArgs e)
    {
        var appendNode = e.AppendNode;
        if (appendNode == null) return;
        var img = GetNodeImage(e.Type);
        Node parentNode = null;

        if (e.ParentNode is TreeGroup treeGroup)
        {
            // 在单网格架构中，分组为根级节点，Key == TreeGroup
            for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
            {
                var row = _grid.Rows[i];
                if (row.IsNode && row.Node.Level == 0 && Equals(row.Node.Key, treeGroup))
                {
                    parentNode = row.Node;
                    break;
                }
            }
        }
        else if (e.ParentNode is TreeDirectoryNode dirNode)
        {
            parentNode = FindNode(dirNode);
        }

        if (parentNode == null) return;

        var siblings = parentNode.Nodes.Cast<Node>().Where(n => n.Level == parentNode.Level + 1).ToList();
        if (e.Index >= siblings.Count)
        {
            parentNode.AddNode(NodeTypeEnum.LastChild, appendNode.Name, appendNode, img);
        }
        else
        {
            siblings[e.Index].AddNode(NodeTypeEnum.PreviousSibling, appendNode.Name, appendNode, img);
        }

        static Bitmap GetNodeImage(ImportTypeEnum tp)
        {
            switch (tp)
            {
                case ImportTypeEnum.Dir: return IconRes.TreeDir;
                case ImportTypeEnum.Doc: return IconRes.TreeDoc;
                case ImportTypeEnum.Table:
                case ImportTypeEnum.Sheet: return IconRes.TreeTable;
                case ImportTypeEnum.Image: return Auditai.UI.Platform.IconRes.TreeImage;
                case ImportTypeEnum.Pdf: return Auditai.UI.Platform.IconRes.TreePdf;
                default: return IconRes.TreeDoc;
            }
        }
    }

    private void AddDocumentEditor(List<DocumentEditor> editors)
    {
        foreach (var editor in editors)
        {
            Program.MainForm.AddDocumentEditor(editor);
        }
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
