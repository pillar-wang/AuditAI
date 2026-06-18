﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.CommonControls;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;

using C1FlexGridEx = Leqisoft.UI.Controls.C1FlexGridEx;
using SDImage = System.Drawing.Image;
using TreeGroup = Leqisoft.Model.TreeGroup;

namespace Leqisoft.UI.Platform;

public class ProjectHierarchy
{
    private C1FlexGridEx _grid;
    private ContextMenuStrip _contextMenu;

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
    private C1Command cmdCopyTable = new C1Command();
    private C1Command cmdCopyDocument = new C1Command();
    private C1Command cmdCopyDirectory = new C1Command();
    private C1Command cmdCopyImage = new C1Command();
    private C1Command cmdCopyPdf = new C1Command();
    private C1Command cmdPasteNode = new C1Command();
    private C1Command cmdPasteTable = new C1Command();
    private C1Command cmdPasteDocument = new C1Command();
    private C1Command cmdPasteDirectory = new C1Command();
    private C1Command cmdPasteImage = new C1Command();
    private C1Command cmdPastePdf = new C1Command();
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
    private C1Command cmdPasteRootTable = new C1Command();
    private C1Command cmdPasteRootDocument = new C1Command();
    private C1Command cmdPasteRootDirectory = new C1Command();
    private C1Command cmdPasteRootImage = new C1Command();
    private C1Command cmdPasteRootPdf = new C1Command();
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
    private C1CommandLink lnkCopyTable = new C1CommandLink();
    private C1CommandLink lnkCopyDocument = new C1CommandLink();
    private C1CommandLink lnkCopyDirectory = new C1CommandLink();
    private C1CommandLink lnkCopyImage = new C1CommandLink();
    private C1CommandLink lnkCopyPdf = new C1CommandLink();
    private C1CommandLink lnkPasteNode = new C1CommandLink();
    private C1CommandLink lnkPasteTable = new C1CommandLink();
    private C1CommandLink lnkPasteDocument = new C1CommandLink();
    private C1CommandLink lnkPasteDirectory = new C1CommandLink();
    private C1CommandLink lnkPasteImage = new C1CommandLink();
    private C1CommandLink lnkPastePdf = new C1CommandLink();
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
    private C1CommandLink lnkPasteRootTable = new C1CommandLink();
    private C1CommandLink lnkPasteRootDocument = new C1CommandLink();
    private C1CommandLink lnkPasteRootDirectory = new C1CommandLink();
    private C1CommandLink lnkPasteRootImage = new C1CommandLink();
    private C1CommandLink lnkPasteRootPdf = new C1CommandLink();
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

    // 其他字段
    private List<TreeGroupView> _groups;
    private frmSearch frmSearch;
    private LazyExcute lazySearchExcute = new LazyExcute();

    public class TreeGroupView
    {
        public C1FlexGridBase Grid { get; set; }
        public object Page { get; set; }
        public SDImage GetTreeNodeIcon(TreeNodeBase node)
        {
            if (node is TreeDirectoryNode) return Resources.TreeDir;
            if (node is TreeDocumentNode) return Resources.TreeDoc;
            if (node is TreeTableNode) return Resources.TreeTable;
            if (node is TreeImageNode) return Resources.TreeDoc;
            if (node is TreePdfNode) return Resources.TreeDoc;
            throw new ArgumentOutOfRangeException();
        }
    }

    public Control View { get; private set; }
    public dynamic SelectedNode { get; set; }
    public TreeGroupView _currentGroup { get; set; }
    public bool IsInOpeningSomeTreeNode { get; set; }
    public bool NumberShown { get; set; }
    public Leqisoft.Model.Project Project { get; set; }

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

        View = _grid;

        SecondTrigger.Trigger.Tick += Trigger_Tick;

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
        ctxTreeNode.CommandLinks.Add(lnkInsertDirectory);

        // cmdInsertTable
        cmdInsertTable.CommandStateQuery += CmdInsertTable_CommandStateQuery;
        cmdInsertTable.Click += CmdInsertTable_Click;
        lnkInsertTable.Command = cmdInsertTable;
        ctxTreeNode.CommandLinks.Add(lnkInsertTable);

        // cmdInsertDocument
        cmdInsertDocument.CommandStateQuery += CmdInsertDocument_CommandStateQuery;
        cmdInsertDocument.Click += CmdInsertDocument_Click;
        lnkInsertDocument.Command = cmdInsertDocument;
        ctxTreeNode.CommandLinks.Add(lnkInsertDocument);

        // cmdInsertImage
        cmdInsertImage.CommandStateQuery += CmdInsertImage_CommandStateQuery;
        cmdInsertImage.Click += CmdInsertImage_Click;
        lnkInsertImage.Command = cmdInsertImage;

        // cmdInsertPdf
        cmdInsertPdf.CommandStateQuery += CmdInsertPdf_CommandStateQuery;
        cmdInsertPdf.Click += CmdInsertPdf_Click;
        lnkInsertPdf.Command = cmdInsertPdf;

        // cmdAppendChildDirectory
        cmdAppendChildDirectory.CommandStateQuery += CmdAppendChildDirectory_CommandStateQuery;
        cmdAppendChildDirectory.Click += CmdAppendChildDirectory_Click;
        lnkAppendChildDirectory.Command = cmdAppendChildDirectory;
        ctxTreeNode.CommandLinks.Add(lnkAppendChildDirectory);

        // cmdAppendChildTable
        cmdAppendChildTable.CommandStateQuery += CmdAppendChildTable_CommandStateQuery;
        cmdAppendChildTable.Click += CmdAppendChildTable_Click;
        lnkAppendChildTable.Command = cmdAppendChildTable;
        ctxTreeNode.CommandLinks.Add(lnkAppendChildTable);

        // cmdAppendChildDocument
        cmdAppendChildDocument.CommandStateQuery += CmdAppendChildDocument_CommandStateQuery;
        cmdAppendChildDocument.Click += CmdAppendChildDocument_Click;
        lnkAppendChildDocument.Command = cmdAppendChildDocument;
        ctxTreeNode.CommandLinks.Add(lnkAppendChildDocument);

        // cmdAppendChildImage
        cmdAppendChildImage.CommandStateQuery += CmdAppendChildImage_CommandStateQuery;
        cmdAppendChildImage.Click += CmdAppendChildImage_Click;
        lnkAppendChildImage.Command = cmdAppendChildImage;

        // cmdAppendChildPdf
        cmdAppendChildPdf.CommandStateQuery += CmdAppendChildPdf_CommandStateQuery;
        cmdAppendChildPdf.Click += CmdAppendChildPdf_Click;
        lnkAppendChildPdf.Command = cmdAppendChildPdf;

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

        // cmdCopyTable
        cmdCopyTable.CommandStateQuery += CmdCopyTable_CommandStateQuery;
        cmdCopyTable.Click += CmdCopyTable_Click;
        lnkCopyTable.Command = cmdCopyTable;
        ctxTreeNode.CommandLinks.Add(lnkCopyTable);

        // cmdCopyDocument
        cmdCopyDocument.CommandStateQuery += CmdCopyDocument_CommandStateQuery;
        cmdCopyDocument.Click += CmdCopyDocument_Click;
        lnkCopyDocument.Command = cmdCopyDocument;
        ctxTreeNode.CommandLinks.Add(lnkCopyDocument);

        // cmdCopyDirectory
        cmdCopyDirectory.CommandStateQuery += CmdCopyDirectory_CommandStateQuery;
        cmdCopyDirectory.Click += CmdCopyDirectory_Click;
        lnkCopyDirectory.Command = cmdCopyDirectory;
        ctxTreeNode.CommandLinks.Add(lnkCopyDirectory);

        // cmdCopyImage
        cmdCopyImage.CommandStateQuery += CmdCopyImage_CommandStateQuery;
        cmdCopyImage.Click += CmdCopyImage_Click;
        lnkCopyImage.Command = cmdCopyImage;
        ctxTreeNode.CommandLinks.Add(lnkCopyImage);

        // cmdCopyPdf
        cmdCopyPdf.CommandStateQuery += CmdCopyPdf_CommandStateQuery;
        cmdCopyPdf.Click += CmdCopyPdf_Click;
        lnkCopyPdf.Command = cmdCopyPdf;
        ctxTreeNode.CommandLinks.Add(lnkCopyPdf);

        // cmdPasteNode
        cmdPasteNode.CommandStateQuery += CmdPasteNode_CommandStateQuery;
        cmdPasteNode.Click += CmdPasteNode_Click;
        lnkPasteNode.Command = cmdPasteNode;
        ctxTreeNode.CommandLinks.Add(lnkPasteNode);

        // cmdPasteTable
        cmdPasteTable.CommandStateQuery += CmdPasteTable_CommandStateQuery;
        cmdPasteTable.Click += CmdPasteTable_Click;
        lnkPasteTable.Command = cmdPasteTable;
        ctxTreeNode.CommandLinks.Add(lnkPasteTable);

        // cmdPasteDocument
        cmdPasteDocument.CommandStateQuery += CmdPasteDocument_CommandStateQuery;
        cmdPasteDocument.Click += CmdPasteDocument_Click;
        lnkPasteDocument.Command = cmdPasteDocument;
        ctxTreeNode.CommandLinks.Add(lnkPasteDocument);

        // cmdPasteDirectory
        cmdPasteDirectory.CommandStateQuery += CmdPasteDirectory_CommandStateQuery;
        cmdPasteDirectory.Click += CmdPasteDirectory_Click;
        lnkPasteDirectory.Command = cmdPasteDirectory;
        ctxTreeNode.CommandLinks.Add(lnkPasteDirectory);

        // cmdPasteImage
        cmdPasteImage.CommandStateQuery += CmdPasteImage_CommandStateQuery;
        cmdPasteImage.Click += CmdPasteImage_Click;
        lnkPasteImage.Command = cmdPasteImage;
        ctxTreeNode.CommandLinks.Add(lnkPasteImage);

        // cmdPastePdf
        cmdPastePdf.CommandStateQuery += CmdPastePdf_CommandStateQuery;
        cmdPastePdf.Click += CmdPastePdf_Click;
        lnkPastePdf.Command = cmdPastePdf;
        ctxTreeNode.CommandLinks.Add(lnkPastePdf);

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
        cmdReload.Image = Leqisoft.UI.Platform.Properties.ContextResources.ctxReloadFile;
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
        ctxTreeNode.CommandLinks.Add(lnkNodeImportFile);

        // cmdNodeImportExcel
        cmdNodeImportExcel.CommandStateQuery += CmdNodeImportExcel_CommandStateQuery;
        cmdNodeImportExcel.Click += CmdNodeImportExcel_Click;
        lnkNodeImportExcel.Command = cmdNodeImportExcel;
        ctxTreeNode.CommandLinks.Add(lnkNodeImportExcel);

        // cmdNodeImportWord
        cmdNodeImportWord.CommandStateQuery += CmdNodeImportWord_CommandStateQuery;
        cmdNodeImportWord.Click += CmdNodeImportWord_Click;
        lnkNodeImportWord.Command = cmdNodeImportWord;
        ctxTreeNode.CommandLinks.Add(lnkNodeImportWord);

        // cmdNodeImportImage
        cmdNodeImportImage.CommandStateQuery += CmdNodeImportImage_CommandStateQuery;
        cmdNodeImportImage.Click += CmdNodeImportImage_Click;
        lnkNodeImportImage.Command = cmdNodeImportImage;
        ctxTreeNode.CommandLinks.Add(lnkNodeImportImage);

        // cmdNodeImportPdf
        cmdNodeImportPdf.CommandStateQuery += CmdNodeImportPdf_CommandStateQuery;
        cmdNodeImportPdf.Click += CmdNodeImportPdf_Click;
        lnkNodeImportPdf.Command = cmdNodeImportPdf;
        ctxTreeNode.CommandLinks.Add(lnkNodeImportPdf);

        // cmdNodeImportFolder
        cmdNodeImportFolder.CommandStateQuery += CmdNodeImportFolder_CommandStateQuery;
        cmdNodeImportFolder.Click += CmdNodeImportFolder_Click;
        lnkNodeImportFolder.Command = cmdNodeImportFolder;
        ctxTreeNode.CommandLinks.Add(lnkNodeImportFolder);

        // ctxBatchOperation 子菜单
        ctxBatchOperation.Text = "批量操作";
        ctxBatchOperation.Image = Leqisoft.UI.Platform.Properties.Resources.BatchOperation16;
        lnkBatchOperation.Command = ctxBatchOperation;
        ctxTreeNode.CommandLinks.Add(lnkBatchOperation);

        // cmdBatchHideFile
        cmdBatchHideFile.Text = "批量隐藏文件";
        cmdBatchHideFile.Image = Leqisoft.UI.Platform.Properties.Resources.BatchHideNodes16;
        cmdBatchHideFile.Click += CmdBatchHideFile_Click;
        lnkBatchHideFile.Command = cmdBatchHideFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchHideFile);

        // cmdBatchDeleteFile
        cmdBatchDeleteFile.Text = "批量删除文件";
        cmdBatchDeleteFile.Image = Leqisoft.UI.Platform.Properties.Resources.BatchRemoveNodes16;
        cmdBatchDeleteFile.Click += CmdBatchDeleteFile_Click;
        cmdBatchDeleteFile.CommandStateQuery += CmdBatchDeleteFile_CommandStateQuery;
        lnkBatchDeleteFile.Command = cmdBatchDeleteFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchDeleteFile);

        // cmdBatchEditIndex
        cmdBatchEditIndex.Text = "批量编辑索引号";
        cmdBatchEditIndex.Image = Leqisoft.UI.Platform.Properties.Resources.EditNodesNumber16;
        cmdBatchEditIndex.Click += CmdBatchEditIndex_Click;
        lnkBatchEditIndex.Command = cmdBatchEditIndex;
        ctxBatchOperation.CommandLinks.Add(lnkBatchEditIndex);

        // cmdBatchExportFile
        cmdBatchExportFile.Text = "批量导出文件";
        cmdBatchExportFile.Image = Leqisoft.UI.Platform.Properties.Resources.BatchExport16;
        cmdBatchExportFile.Click += CmdBatchExportFile_Click;
        lnkBatchExportFile.Command = cmdBatchExportFile;
        ctxBatchOperation.CommandLinks.Add(lnkBatchExportFile);

        // cmdBatchPrintFile
        cmdBatchPrintFile.Text = "批量打印文件";
        cmdBatchPrintFile.Image = Leqisoft.UI.Platform.Properties.Resources.BatchPrint16;
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
        lnkPasteNode.Delimiter = true;
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
        ctxTreeEmpty.CommandLinks.Add(lnkAppendRootDirectory);

        // cmdAppendRootTable
        cmdAppendRootTable.CommandStateQuery += CmdAppendRootTable_CommandStateQuery;
        cmdAppendRootTable.Click += CmdAppendRootTable_Click;
        lnkAppendRootTable.Command = cmdAppendRootTable;
        ctxTreeEmpty.CommandLinks.Add(lnkAppendRootTable);

        // cmdAppendRootDocument
        cmdAppendRootDocument.CommandStateQuery += CmdAppendRootDocument_CommandStateQuery;
        cmdAppendRootDocument.Click += CmdAppendRootDocument_Click;
        lnkAppendRootDocument.Command = cmdAppendRootDocument;
        ctxTreeEmpty.CommandLinks.Add(lnkAppendRootDocument);

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

        // cmdPasteRootNode
        cmdPasteRootNode.CommandStateQuery += CmdPasteRootNode_CommandStateQuery;
        cmdPasteRootNode.Click += CmdPasteRootNode_Click;
        lnkPasteRootNode.Command = cmdPasteRootNode;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootNode);

        // cmdPasteRootTable
        cmdPasteRootTable.CommandStateQuery += CmdPasteRootTable_CommandStateQuery;
        cmdPasteRootTable.Click += CmdPasteRootTable_Click;
        lnkPasteRootTable.Command = cmdPasteRootTable;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootTable);

        // cmdPasteRootDocument
        cmdPasteRootDocument.CommandStateQuery += CmdPasteRootDocument_CommandStateQuery;
        cmdPasteRootDocument.Click += CmdPasteRootDocument_Click;
        lnkPasteRootDocument.Command = cmdPasteRootDocument;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootDocument);

        // cmdPasteRootDirectory
        cmdPasteRootDirectory.CommandStateQuery += CmdPasteRootDirectory_CommandStateQuery;
        cmdPasteRootDirectory.Click += CmdPasteRootDirectory_Click;
        lnkPasteRootDirectory.Command = cmdPasteRootDirectory;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootDirectory);

        // cmdPasteRootImage
        cmdPasteRootImage.CommandStateQuery += CmdPasteRootImage_CommandStateQuery;
        cmdPasteRootImage.Click += CmdPasteRootImage_Click;
        lnkPasteRootImage.Command = cmdPasteRootImage;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootImage);

        // cmdPasteRootPdf
        cmdPasteRootPdf.CommandStateQuery += CmdPasteRootPdf_CommandStateQuery;
        cmdPasteRootPdf.Click += CmdPasteRootPdf_Click;
        lnkPasteRootPdf.Command = cmdPasteRootPdf;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteRootPdf);

        // lnkPasteGroup4 -> cmdPasteGroupClickOnGridTree (ctxTreeEmpty)
        lnkPasteGroup4.Command = cmdPasteGroupClickOnGridTree;
        ctxTreeEmpty.CommandLinks.Add(lnkPasteGroup4);

        // cmdEmptyImportFile
        cmdEmptyImportFile.CommandStateQuery += CmdEmptyImportFile_CommandStateQuery;
        cmdEmptyImportFile.Click += CmdEmptyImportFile_Click;
        lnkEmptyImportFile.Command = cmdEmptyImportFile;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportFile);

        // cmdEmptyImportExcel
        cmdEmptyImportExcel.CommandStateQuery += CmdEmptyImportExcel_CommandStateQuery;
        cmdEmptyImportExcel.Click += CmdEmptyImportExcel_Click;
        lnkEmptyImportExcel.Command = cmdEmptyImportExcel;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportExcel);

        // cmdEmptyImportWord
        cmdEmptyImportWord.CommandStateQuery += CmdEmptyImportWord_CommandStateQuery;
        cmdEmptyImportWord.Click += CmdEmptyImportWord_Click;
        lnkEmptyImportWord.Command = cmdEmptyImportWord;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportWord);

        // cmdEmptyImportImage
        cmdEmptyImportImage.CommandStateQuery += CmdEmptyImportImage_CommandStateQuery;
        cmdEmptyImportImage.Click += CmdEmptyImportImage_Click;
        lnkEmptyImportImage.Command = cmdEmptyImportImage;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportImage);

        // cmdEmptyImportPdf
        cmdEmptyImportPdf.CommandStateQuery += CmdEmptyImportPdf_CommandStateQuery;
        cmdEmptyImportPdf.Click += CmdEmptyImportPdf_Click;
        lnkEmptyImportPdf.Command = cmdEmptyImportPdf;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportPdf);

        // cmdEmptyImportFolder
        cmdEmptyImportFolder.CommandStateQuery += CmdEmptyImportFolder_CommandStateQuery;
        cmdEmptyImportFolder.Click += CmdEmptyImportFolder_Click;
        lnkEmptyImportFolder.Command = cmdEmptyImportFolder;
        ctxTreeEmpty.CommandLinks.Add(lnkEmptyImportFolder);

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
        frmSearch = new frmSearch();
        frmSearch.SelectNode += FrmSearch_SelectNode;
        lazySearchExcute.SetAction(LazySearchExcute_Action);
        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        _contextMenu = new ContextMenuStrip();

        // 分组操作菜单
        var miMoveUpGroup = new ToolStripMenuItem("上移分组", null, OnMoveUpGroup) { Name = "上移分组" };
        var miMoveDownGroup = new ToolStripMenuItem("下移分组", null, OnMoveDownGroup) { Name = "下移分组" };
        var sepGroup = new ToolStripSeparator();
        var miNewGroup = new ToolStripMenuItem("新建分组", null, OnNewGroup) { Name = "新建分组" };
        var miDeleteGroup = new ToolStripMenuItem("删除分组", null, OnDeleteGroup) { Name = "删除分组" };
        var miCopyGroup = new ToolStripMenuItem("复制分组", null, OnCopyGroup) { Name = "复制分组" };
        var miRenameGroup = new ToolStripMenuItem("重命名分组", null, OnRenameGroup) { Name = "重命名分组" };

        // 节点操作菜单
        var miNewDir = new ToolStripMenuItem("新建文件夹", null, OnNewDirectory) { Name = "新建文件夹" };
        var miNewTable = new ToolStripMenuItem("新建表格", null, OnNewTable) { Name = "新建表格" };
        var miNewDoc = new ToolStripMenuItem("新建文档", null, OnNewDocument) { Name = "新建文档" };
        var miImportFile = new ToolStripMenuItem("导入文件...", null, OnImportFile) { Name = "导入文件..." };
        var miImportFolder = new ToolStripMenuItem("导入文件夹...", null, OnImportFolder) { Name = "导入文件夹..." };
        var sep1 = new ToolStripSeparator();
        var miRename = new ToolStripMenuItem("重命名", null, OnRename) { Name = "重命名" };
        var miDelete = new ToolStripMenuItem("删除", null, OnDelete) { Name = "删除" };
        var sep2 = new ToolStripSeparator();
        var miMoveUp = new ToolStripMenuItem("上移", null, OnMoveUp) { Name = "上移" };
        var miMoveDown = new ToolStripMenuItem("下移", null, OnMoveDown) { Name = "下移" };

        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            miMoveUpGroup, miMoveDownGroup, sepGroup,
            miNewGroup, miDeleteGroup, miCopyGroup, miRenameGroup,
            sep1,
            miNewDir, miNewTable, miNewDoc, miImportFile, miImportFolder,
            sep2, miRename, miDelete,
            miMoveUp, miMoveDown
        });
    }

    private void ShowContextMenu(object selectedItem)
    {
        foreach (ToolStripItem item in _contextMenu.Items)
        {
            item.Visible = false;
        }

        if (selectedItem is TreeGroup group)
        {
            // 分组节点 - 支持上移、下移、新建、删除、复制、重命名
            _contextMenu.Items["上移分组"].Visible = group.CanMoveUp1;
            _contextMenu.Items["下移分组"].Visible = group.CanMoveDown1;
            _contextMenu.Items["新建分组"].Visible = true;
            _contextMenu.Items["删除分组"].Visible = true;
            _contextMenu.Items["复制分组"].Visible = true;
            _contextMenu.Items["重命名分组"].Visible = true;
        }
        else if (selectedItem == null)
        {
            // 空白区域右键 - 在当前分组下新建节点和新建分组
            _contextMenu.Items["新建分组"].Visible = true;
            _contextMenu.Items["新建文件夹"].Visible = true;
            _contextMenu.Items["新建表格"].Visible = true;
            _contextMenu.Items["新建文档"].Visible = true;
            _contextMenu.Items["导入文件..."].Visible = true;
            _contextMenu.Items["导入文件夹..."].Visible = true;
        }
        else if (selectedItem is TreeDirectoryNode)
        {
            // 文件夹节点 - 可新建子项、重命名、删除、上移、下移
            _contextMenu.Items["新建文件夹"].Visible = true;
            _contextMenu.Items["新建表格"].Visible = true;
            _contextMenu.Items["新建文档"].Visible = true;
            _contextMenu.Items["导入文件..."].Visible = true;
            _contextMenu.Items["导入文件夹..."].Visible = true;
            _contextMenu.Items["重命名"].Visible = true;
            _contextMenu.Items["删除"].Visible = true;
            _contextMenu.Items["上移"].Visible = true;
            _contextMenu.Items["下移"].Visible = true;
        }
        else if (selectedItem is TreeNodeBase)
        {
            // 表格/文档/图片/PDF节点 - 可重命名、删除、上移、下移
            _contextMenu.Items["重命名"].Visible = true;
            _contextMenu.Items["删除"].Visible = true;
            _contextMenu.Items["上移"].Visible = true;
            _contextMenu.Items["下移"].Visible = true;
        }

        _contextMenu.Show(_grid, _grid.PointToClient(Cursor.Position));
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
            SelectedNode = null;
            _grid.BeginUpdate();
            _grid.Rows.Count = _grid.Rows.Fixed;

            if (Project?.TreeGroups == null)
            {
                _grid.EndUpdate();
                return;
            }

            foreach (Leqisoft.Model.TreeGroup treeGroup in Project.TreeGroups)
            {
                Node node = _grid.Rows.AddNode(0);
                node.Key = treeGroup;
                node.Data = treeGroup.Name;
                node.Image = Resources.TreeDir;

                foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
                {
                    AddTreeNode(rootNode, node);
                }
                node.Collapsed = true;
            }
            _grid.EndUpdate();
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
                }
                else if (node.Key is TreeNodeBase tnb)
                {
                    SelectedNode = tnb;
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
                        _grid.Row = ht.Row;
                        ShowContextMenu(group);
                    }
                    else if (node.Key is TreeNodeBase tnb)
                    {
                        SelectedNode = tnb;
                        _grid.Row = ht.Row;
                        ShowContextMenu(tnb);
                    }
                }
                else
                {
                    ShowContextMenu(null);
                }
            }
            else
            {
                ShowContextMenu(null);
            }
        }
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
    }
    private void CmdInsertImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdInsertPdf_Click(object sender, ClickEventArgs e)
    {
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
    }
    private void CmdAppendChildImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendChildPdf_Click(object sender, ClickEventArgs e)
    {
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
    }
    private void CmdHideNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdShowNodes_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdShowNodes_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdSearchNodes_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdSearchNodes_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCutNode_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCutNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyTable_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCopyTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyDocument_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCopyDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyDirectory_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCopyDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyImage_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCopyImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdCopyPdf_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdCopyPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteNode_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteTable_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteDocument_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteDirectory_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteImage_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPastePdf_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPastePdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdRenameNode_Click(object sender, ClickEventArgs e) => OnRename(null, EventArgs.Empty);
    private void CmdRenameNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdEditNumber_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdEditNumber_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdReload_Click(object sender, ClickEventArgs e)
    {
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
    }
    private void CmdNodeImportExcel_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportWord_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdNodeImportWord_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportImage_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdNodeImportImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportPdf_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdNodeImportPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdNodeImportFolder_Click(object sender, ClickEventArgs e) => OnImportFolder(null, EventArgs.Empty);
    private void CmdNodeImportFolder_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    #endregion

    #region 批量操作命令

    private void CmdBatchHideFile_Click(object sender, ClickEventArgs e)
    {
    }

    private void CmdBatchDeleteFile_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdBatchDeleteFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdBatchEditIndex_Click(object sender, ClickEventArgs e)
    {
    }

    private void CmdBatchExportFile_Click(object sender, ClickEventArgs e)
    {
    }

    private void CmdBatchPrintFile_Click(object sender, ClickEventArgs e)
    {
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
    }
    private void CmdAppendRootImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdAppendRootPdf_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdAppendRootPdf_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootNode_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteRootNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootTable_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteRootTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootDocument_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteRootDocument_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootDirectory_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteRootDirectory_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootImage_Click(object sender, ClickEventArgs e)
    {
    }
    private void CmdPasteRootImage_CommandStateQuery(object sender, CommandStateQueryEventArgs e) => e.Enabled = true;

    private void CmdPasteRootPdf_Click(object sender, ClickEventArgs e)
    {
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

    #endregion

    #endregion
}
