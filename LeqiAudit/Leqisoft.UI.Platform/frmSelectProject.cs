using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1Tile;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 项目管理风格的项目选择对话框
/// 用于跨项目引用时选择来源项目
/// </summary>
public class frmSelectProject : C1RibbonForm
{
    private C1TileControlEx _tileControl;
    private C1Button _btnOk;
    private C1Button _btnCancel;
    private C1Label _lblTitle;

    private Leqisoft.DTO.Project _selectedProject;
    private readonly Guid _currentProjectId;

    /// <summary>用户选中的项目</summary>
    public Leqisoft.DTO.Project SelectedProject => _selectedProject;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentProjectId">当前项目ID，用于排除自己</param>
    public frmSelectProject(Guid currentProjectId)
    {
        _currentProjectId = currentProjectId;
        InitializeComponent();
        this.Load += FrmSelectProject_Load;
    }

    private async void FrmSelectProject_Load(object sender, EventArgs e)
    {
        await PopulateProjects();
    }

    private async Task PopulateProjects()
    {
        _tileControl.Groups.Clear();

        try
        {
            IEnumerable<Leqisoft.DTO.Project> projects;

            if (StorageRouter.IsLocalMode)
            {
                // 本地模式：通过 StorageRouter 获取所有项目
                projects = await StorageRouter.GetProjects();
            }
            else
            {
                // 服务器模式：通过 WebApiClient 获取（使用 StorageRouter 路由）
                projects = await StorageRouter.GetProjects();
            }

            // 过滤：排除当前项目
            var filteredProjects = projects
                .Where(p => p.Id != _currentProjectId)
                .OrderByDescending(p => p.CreateTime)
                .ToList();

            if (filteredProjects.Count == 0)
            {
                _lblTitle.Text = "没有可用的项目";
                return;
            }

            // 创建 Tile 组
            var group = new C1.Win.C1Tile.Group();

            foreach (var project in filteredProjects)
            {
                var tile = new Tile
                {
                    Text = project.Name,
                    HorizontalSize = 5,
                    VerticalSize = 4,
                    Tag = project
                };

                // 设置样式
                tile.BackColor = Color.White;

                group.Tiles.Add(tile);
            }

            _tileControl.Groups.Add(group);
        }
        catch (Exception ex)
        {
            _lblTitle.Text = "加载项目列表失败: " + ex.Message;
        }
    }

    private void _tileControl_DoubleClickTile(object sender, Tile e)
    {
        _selectedProject = e.Tag as Leqisoft.DTO.Project;
        if (_selectedProject != null)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    private void _btnOk_Click(object sender, EventArgs e)
    {
        _selectedProject = _tileControl.SelectedTile?.Tag as Leqisoft.DTO.Project;
        if (_selectedProject != null)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        else
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一个项目");
        }
    }

    private void _btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void InitializeComponent()
    {
        this._tileControl = new C1TileControlEx();
        this._btnOk = new C1.Win.C1Input.C1Button();
        this._btnCancel = new C1.Win.C1Input.C1Button();
        this._lblTitle = new C1.Win.C1Input.C1Label();

        //
        // _tileControl
        //
        this._tileControl.AllowChecking = false;
        this._tileControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this._tileControl.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this._tileControl.CellWidth = 10;
        this._tileControl.CellHeight = 10;
        this._tileControl.Location = new System.Drawing.Point(12, 50);
        this._tileControl.Name = "_tileControl";
        this._tileControl.Size = new System.Drawing.Size(760, 420);
        this._tileControl.TabIndex = 0;
        this._tileControl.DoubleClickTile += _tileControl_DoubleClickTile;

        //
        // _lblTitle
        //
        this._lblTitle.AutoSize = true;
        this._lblTitle.BackColor = System.Drawing.Color.Transparent;
        this._lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._lblTitle.Font = new System.Drawing.Font("Microsoft YaHei", 12f, System.Drawing.FontStyle.Bold);
        this._lblTitle.ForeColor = System.Drawing.Color.Black;
        this._lblTitle.Location = new System.Drawing.Point(12, 15);
        this._lblTitle.Name = "_lblTitle";
        this._lblTitle.Size = new System.Drawing.Size(200, 22);
        this._lblTitle.TabIndex = 1;
        this._lblTitle.Text = "请选择来源项目";
        this._lblTitle.TextDetached = true;

        //
        // _btnOk
        //
        this._btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this._btnOk.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
        this._btnOk.Location = new System.Drawing.Point(592, 480);
        this._btnOk.Name = "_btnOk";
        this._btnOk.Size = new System.Drawing.Size(87, 33);
        this._btnOk.TabIndex = 2;
        this._btnOk.Text = "确定";
        this._btnOk.UseVisualStyleBackColor = true;
        this._btnOk.Click += _btnOk_Click;

        //
        // _btnCancel
        //
        this._btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this._btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
        this._btnCancel.Location = new System.Drawing.Point(685, 480);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new System.Drawing.Size(87, 33);
        this._btnCancel.TabIndex = 3;
        this._btnCancel.Text = "取消";
        this._btnCancel.UseVisualStyleBackColor = true;
        this._btnCancel.Click += _btnCancel_Click;

        //
        // frmSelectProject
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 525);
        this.Controls.Add(this._lblTitle);
        this.Controls.Add(this._tileControl);
        this.Controls.Add(this._btnOk);
        this.Controls.Add(this._btnCancel);
        this.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
        this.Name = "frmSelectProject";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "选择来源项目";
    }
}
