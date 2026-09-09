/* ===== 邀请管理页面逻辑 ===== */

(function () {
    'use strict';

    const PAGE_SIZE = 20;

    // 角色映射：0=经理, 1=助理, 2=审核, 3=编辑, 4=普通用户
    const ROLE_MAP = {
        0: '经理',
        1: '助理',
        2: '审核',
        3: '编辑',
        4: '普通用户'
    };

    let currentPage = 1;

    const tbody = document.getElementById('invitationBody');

    // 角色 -> 中文文本
    function roleText(role) {
        return ROLE_MAP[role] !== undefined ? ROLE_MAP[role] : '未知';
    }

    // 计算实际展示状态：Pending 且已过到期时间 -> 视为已过期
    function resolveStatus(inv) {
        if (inv.status === 'Pending' && inv.expiresAt) {
            const exp = new Date(inv.expiresAt);
            if (!isNaN(exp.getTime()) && exp.getTime() < Date.now()) {
                return 'Expired';
            }
        }
        return inv.status;
    }

    // 状态 -> HTML 徽章
    function statusBadge(inv) {
        const status = resolveStatus(inv);
        let text = '';
        let cls = '';
        switch (status) {
            case 'Pending':
                text = '待接受';
                cls = 'badge trial';
                break;
            case 'Accepted':
                text = '已接受';
                cls = 'badge active';
                break;
            case 'Expired':
                text = '已过期';
                cls = 'badge expired';
                break;
            case 'Revoked':
                text = '已撤销';
                cls = 'badge inactive';
                break;
            default:
                text = status || '未知';
                cls = 'badge inactive';
        }
        return '<span class="' + cls + '">' + escapeHtml(text) + '</span>';
    }

    // 渲染表格
    function renderTable(invitations) {
        if (!invitations || invitations.length === 0) {
            tbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray">暂无邀请记录</td></tr>';
            return;
        }

        const rows = invitations.map(function (inv) {
            const canRevoke = resolveStatus(inv) === 'Pending';
            const revokeBtn = canRevoke
                ? '<button class="btn btn-danger btn-sm" data-token="' + escapeHtml(inv.inviteToken) + '">' + ICONS.x + '撤销</button>'
                : '<span class="text-gray">—</span>';

            return '<tr>' +
                '<td>' + escapeHtml(inv.id) + '</td>' +
                '<td>' + escapeHtml(inv.teamName) + '</td>' +
                '<td>' + escapeHtml(inv.invitedByName) + '</td>' +
                '<td>' + escapeHtml(inv.email) + '</td>' +
                '<td>' + escapeHtml(inv.phone) + '</td>' +
                '<td>' + escapeHtml(roleText(inv.role)) + '</td>' +
                '<td>' + escapeHtml(formatDate(inv.expiresAt)) + '</td>' +
                '<td>' + statusBadge(inv) + '</td>' +
                '<td>' + escapeHtml(formatDate(inv.createTime)) + '</td>' +
                '<td>' + revokeBtn + '</td>' +
                '</tr>';
        });
        tbody.innerHTML = rows.join('');
    }

    // 渲染分页
    function renderPager(total) {
        const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
        renderPagination('pagination', currentPage, totalPages, function (page) {
            currentPage = page;
            loadInvitations();
        });
    }

    // 加载邀请列表
    async function loadInvitations() {
        tbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray">加载中...</td></tr>';
        try {
            const data = await apiGet('/Admin/Invitations', {
                page: currentPage,
                pageSize: PAGE_SIZE
            });
            const invitations = (data && data.invitations) || [];
            const total = (data && typeof data.total === 'number') ? data.total : invitations.length;
            renderTable(invitations);
            renderPager(total);
        } catch (e) {
            tbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray">加载失败：' + escapeHtml(e.message || '未知错误') + '</td></tr>';
            showToast(e.message || '加载邀请列表失败', 'error');
        }
    }

    // 撤销邀请
    async function revokeInvitation(token) {
        const ok = await confirmDialog('确定要撤销此邀请吗？撤销后被邀请人将无法通过此链接加入');
        if (!ok) return;
        try {
            await apiPost('/Admin/RevokeInvitation', { inviteToken: token });
            showToast('邀请已撤销', 'success');
            await loadInvitations();
        } catch (e) {
            showToast(e.message || '撤销失败', 'error');
        }
    }

    // 事件委托：撤销按钮点击
    tbody.addEventListener('click', function (e) {
        const btn = e.target.closest('button[data-token]');
        if (!btn) return;
        revokeInvitation(btn.getAttribute('data-token'));
    });

    // 初始化
    function init() {
        if (!requireAdmin()) return;
        loadInvitations();
    }

    init();
})();
