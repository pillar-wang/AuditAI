﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Leqisoft.DTO;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 跨项目数据引用状态仪表板
/// </summary>
public class frmCrossProjectRefStatus : Form
{
    private readonly Leqisoft.Model.Project _currentProject;
    private readonly CrossProjectDataRefStore _store;
    private readonly CrossProjectDataRefManager _manager;
    private readonly CrossProjectRefCache _cache;
    private readonly CrossProjectRefAuthProvider _authProvider;

    // 关键指标卡片
    private Panel _pnlStats;
    private Label _lblTotalRefs;
    private Label _lblEnabledRefs;
    private Label _lblErrorRefs;
    private Label _lblCacheHitRate;
    private Label _lblLastRefreshTime;
    private Label _lblSourceAvailability;

    // 健康检查网格
    private C1FlexGrid _gridHealth;

    // 按钮
    private C1Button _btnRunHealthCheck;
    private C1Button _btnClose;

public frmCrossProjectRefStatus(Leqisoft.Model.Project currentProject)
    {
        _currentProject = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
        _store = new CrossProjectDataRefStore(currentProject);
        _manager = new CrossProjectDataRefManager(currentProject);
        _cache = new CrossProjectRefCache(Leqisoft.Model.User.Current?.Id ?? 1);
        _authProvider = new CrossProjectRefAuthProvider(currentProject);

        InitializeComponent();
        this.Load += async (s, e) => await LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "跨项目数据引用状态仪表板";
        this.Size = new Size(850, 650);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Microsoft YaHei", 9f);
        this.MinimumSize = new Size(700, 500);

        // 顶部关键指标卡片
        _pnlStats = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(10) };

        int cardWidth = 130;
        int cardHeight = 80;
        int startX = 15;
        int startY = 20;
        var labels = new Dictionary<string, Label>
        {
            {"引用总数", _lblTotalRefs = new Label{Text="0", Font=new Font("Microsoft YaHei", 18f, FontStyle.Bold), AutoSize=true, ForeColor=Color.FromArgb(0,120,215)}},
            {"已启用", _lblEnabledRefs = new Label{Text="0", Font=new Font("Microsoft YaHei", 18f, FontStyle.Bold), AutoSize=true, ForeColor=Color.Green}},
            {"异常", _lblErrorRefs = new Label{Text="0", Font=new Font("Microsoft YaHei", 18f, FontStyle.Bold), AutoSize=true, ForeColor=Color.Red}},
            {"缓存命中", _lblCacheHitRate = new Label{Text="0%", Font=new Font("Microsoft YaHei", 18f, FontStyle.Bold), AutoSize=true, ForeColor=Color.FromArgb(0,120,215)}},
        };

        int idx = 0;
        foreach (var kv in labels)
        {
            var card = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(startX + idx * (cardWidth + 15), startY),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var title = new Label { Text = kv.Key, Location = new Point(5, 5), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei", 9f) };
            kv.Value.Location = new Point(5, 30);
            card.Controls.Add(title);
            card.Controls.Add(kv.Value);
            _pnlStats.Controls.Add(card);
            idx++;
        }

        this.Controls.Add(_pnlStats);

        // 健康检查网格
        _gridHealth = new C1FlexGrid { Dock = DockStyle.Fill, Location = new Point(0, 130) };
        _gridHealth.Cols.Count = 6;
        _gridHealth[0, 0] = "引用名称";
        _gridHealth[0, 1] = "来源项目";
        _gridHealth[0, 2] = "来源表";
        _gridHealth[0, 3] = "健康状态";
        _gridHealth[0, 4] = "详情";
        _gridHealth[0, 5] = "修复建议";
        _gridHealth.Cols[0].Width = 150;
        _gridHealth.Cols[1].Width = 150;
        _gridHealth.Cols[2].Width = 100;
        _gridHealth.Cols[3].Width = 100;
        _gridHealth.Cols[4].Width = 150;
        _gridHealth.Cols[5].Width = 150;
        _gridHealth.AllowEditing = false;
        this.Controls.Add(_gridHealth);

        // 底部按钮
        var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(240, 240, 240) };
        _btnRunHealthCheck = new C1Button { Text = "运行健康检查", Location = new Point(10, 10), Size = new Size(130, 30) };
        _btnRunHealthCheck.Click += async (s, e) => await RunHealthCheck();
        _btnClose = new C1Button { Text = "关闭", Location = new Point(150, 10), Size = new Size(90, 30), DialogResult = DialogResult.Cancel };
        pnlButtons.Controls.AddRange(new Control[] { _btnRunHealthCheck, _btnClose });
        this.Controls.Add(pnlButtons);
        this.CancelButton = _btnClose;
    }

    private async System.Threading.Tasks.Task LoadData()
    {
        try
        {
            var allRefs = await _store.LoadAll();
            var enabledRefs = allRefs.Where(r => r.Enabled).ToList();

            _lblTotalRefs.Text = allRefs.Count.ToString();
            _lblEnabledRefs.Text = enabledRefs.Count.ToString();

            var stats = _cache.GetCacheStats();
            _lblCacheHitRate.Text = $"{stats.HitRate}%";

            // 统计异常引用
            int errorCount = 0;
            foreach (var r in allRefs)
            {
                string dbPath = GetDbPath(r.SourceProjectId);
                if (!System.IO.File.Exists(dbPath)) errorCount++;
            }
            _lblErrorRefs.Text = errorCount.ToString();

            // 自动健康检查
            await RunHealthCheck();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"加载状态数据失败：{ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task RunHealthCheck()
    {
        try
        {
            var allRefs = await _store.LoadAll();
            _gridHealth.Rows.Count = 1;

            int errorCount = 0;
            foreach (var r in allRefs)
            {
                int row = _gridHealth.Rows.Count;
                _gridHealth.Rows.Add();
                _gridHealth[row, 0] = r.Name;
                _gridHealth[row, 1] = r.SourceProjectId.ToString().Substring(0, 8) + "...";
                _gridHealth[row, 2] = r.SourceTableId.Value.ToString();

                string status = "正常";
                string detail = "所有检查通过";
                string suggestion = "";
                Color statusColor = Color.Green;

                // 1. 检查来源项目文件
                string dbPath = GetDbPath(r.SourceProjectId);
                if (!System.IO.File.Exists(dbPath))
                {
                    status = "异常";
                    detail = "来源项目文件不存在";
                    suggestion = "恢复项目文件或删除此引用";
                    statusColor = Color.Red;
                }
                else
                {
                    // 2. 检查授权
                    bool authorized = _authProvider.CheckTableAccess(r.SourceProjectId, _currentProject.Id, r.SourceTableId);
                    if (!authorized)
                    {
                        status = "异常";
                        detail = "授权已失效或不匹配";
                        suggestion = "联系来源项目管理重新授权";
                        statusColor = Color.Red;
                    }
                    else
                    {
                        // 3. 检查缓存
                        var cachedData = _cache.GetCachedData(r.Id, dbPath, 60);
                        if (cachedData != null)
                        {
                            detail = "缓存有效";
                        }
                    }
                }

                _gridHealth[row, 3] = status;
                _gridHealth[row, 4] = detail;
                _gridHealth[row, 5] = suggestion;
                _gridHealth.Rows[row].StyleNew.ForeColor = statusColor;

                if (status == "异常") errorCount++;
            }

            _lblErrorRefs.Text = errorCount.ToString();
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Information, $"健康检查完成！共 {allRefs.Count} 个引用，{errorCount} 个异常。");
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"健康检查失败：{ex.Message}");
        }
    }

    private string GetDbPath(Guid projectId)
    {
        long userId = Leqisoft.Model.User.Current?.Id ?? 1;
        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", userId.ToString(), $"{projectId}.db");
    }

    /// <summary>
    /// 在 frmCrossProjectDataRef 中调用此方法打开仪表板
    /// </summary>
    public static DialogResult ShowDashboard(Leqisoft.Model.Project project)
    {
        using var dashboard = new frmCrossProjectRefStatus(project);
        return dashboard.ShowDialog();
    }
}