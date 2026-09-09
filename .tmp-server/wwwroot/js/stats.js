/* ===== 仪表盘统计页逻辑 ===== */

(function () {
    'use strict';

    // 套餐类型映射
    const PLAN_TYPE_MAP = {
        0: 'Trial',
        1: 'Standard',
        2: 'Professional',
        3: 'Ultimate'
    };

    // 统计卡片配置：key 对应接口字段，cls 对应左侧边框颜色，icon 对应图标
    const STAT_CARDS = [
        { key: 'totalUsers',         label: '总用户数',        cls: '',            icon: 'users' },
        { key: 'activeUsers',        label: '活跃用户',        cls: 'success',     icon: 'userCheck' },
        { key: 'totalTeams',         label: '团队总数',        cls: 'purple',      icon: 'building' },
        { key: 'totalProjects',      label: '项目总数',        cls: 'warning',     icon: 'folder' },
        { key: 'totalLicenses',      label: 'License 总数',    cls: 'info',        icon: 'key' },
        { key: 'expiringLicenses',   label: '即将到期',        cls: 'danger',      icon: 'clock' },
        { key: 'pendingInvitations', label: '待接受邀请',      cls: 'warning',     icon: 'send' },
        { key: 'newUsersLast7Days',  label: '近7天新增',       cls: 'success',     icon: 'userPlus' }
    ];

    const statGrid = document.getElementById('statGrid');
    const licenseTbody = document.getElementById('licenseTbody');

    // 渲染加载中的卡片
    function renderLoadingCards() {
        statGrid.innerHTML = STAT_CARDS.map(function (c) {
            return '<div class="stat-card ' + c.cls + '">' +
                '<div class="stat-icon">' + (ICONS[c.icon] || '') + '</div>' +
                '<div class="stat-value">加载中...</div>' +
                '<div class="stat-label">' + c.label + '</div>' +
                '</div>';
        }).join('');
    }

    // 渲染统计卡片数据
    function renderCards(stats) {
        statGrid.innerHTML = STAT_CARDS.map(function (c) {
            const val = stats[c.key];
            let extra = '';
            if (c.key === 'expiringLicenses' && typeof val === 'number' && val > 0) {
                extra = '<div class="stat-warn">' + ICONS.alertTri + ' 需关注</div>';
            }
            const display = (val !== undefined && val !== null) ? val : '—';
            return '<div class="stat-card ' + c.cls + '">' +
                '<div class="stat-icon">' + (ICONS[c.icon] || '') + '</div>' +
                '<div class="stat-value">' + display + '</div>' +
                '<div class="stat-label">' + c.label + '</div>' +
                extra +
                '</div>';
        }).join('');
    }

    // 卡片错误占位
    function renderErrorCards() {
        statGrid.innerHTML = STAT_CARDS.map(function (c) {
            return '<div class="stat-card ' + c.cls + '">' +
                '<div class="stat-icon">' + (ICONS[c.icon] || '') + '</div>' +
                '<div class="stat-value">—</div>' +
                '<div class="stat-label">' + c.label + '</div>' +
                '</div>';
        }).join('');
    }

    // 套餐类型文本
    function planTypeText(t) {
        return PLAN_TYPE_MAP[t] || ('未知(' + t + ')');
    }

    // 剩余天数徽章：<7 红色，7-30 橙色
    function daysRemainingBadge(days) {
        if (typeof days !== 'number') days = parseInt(days, 10) || 0;
        if (days < 7) {
            return '<span class="badge expired">' + days + ' 天</span>';
        }
        return '<span class="badge trial">' + days + ' 天</span>';
    }

    // 状态徽章
    function statusBadge(isActive) {
        return isActive
            ? '<span class="badge active">活跃</span>'
            : '<span class="badge inactive">停用</span>';
    }

    // 渲染 License 表格
    function renderLicenses(licenses) {
        if (!licenses || licenses.length === 0) {
            licenseTbody.innerHTML = '<tr class="empty-row"><td colspan="7">暂无即将到期的 License</td></tr>';
            return;
        }
        licenseTbody.innerHTML = licenses.map(function (l) {
            const seats = (l.seats !== undefined && l.seats !== null) ? l.seats : '—';
            return '<tr>' +
                '<td><code>' + escapeHtml(l.licenseKey) + '</code></td>' +
                '<td>' + escapeHtml(l.teamName) + '</td>' +
                '<td>' + escapeHtml(planTypeText(l.planType)) + '</td>' +
                '<td>' + seats + '</td>' +
                '<td>' + formatDate(l.endDate) + '</td>' +
                '<td>' + daysRemainingBadge(l.daysRemaining) + '</td>' +
                '<td>' + statusBadge(l.isActive) + '</td>' +
                '</tr>';
        }).join('');
    }

    // 加载所有数据
    async function loadData() {
        renderLoadingCards();

        // 加载统计数据
        try {
            const statsRes = await apiGet('/Admin/Stats');
            renderCards(statsRes || {});
        } catch (err) {
            showToast(err.message || '加载统计数据失败', 'error');
            renderErrorCards();
        }

        // 加载即将到期的 License
        try {
            const expRes = await apiGet('/Admin/Licenses/Expiring', { days: 30 });
            renderLicenses((expRes && expRes.licenses) || []);
        } catch (err) {
            showToast(err.message || '加载 License 数据失败', 'error');
            licenseTbody.innerHTML = '<tr class="empty-row"><td colspan="7">数据加载失败</td></tr>';
        }
    }

    // 初始化
    function init() {
        if (!requireAdmin()) {
            return;
        }
        loadData();
    }

    init();
})();
