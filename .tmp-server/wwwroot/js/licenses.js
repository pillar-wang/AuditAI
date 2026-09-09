/* ===== License 管理页逻辑 ===== */

(function () {
    'use strict';

    // ===== 常量 =====
    const PAGE_SIZE = 20;
    const PLAN_TYPE_MAP = {
        0: { name: '试用', cls: 'plan-trial' },
        1: { name: '标准', cls: 'plan-standard' },
        2: { name: '专业', cls: 'plan-pro' },
        3: { name: '旗舰', cls: 'plan-ultimate' }
    };

    // ===== 状态 =====
    let currentPage = 1;
    let currentKeyword = '';
    let totalPages = 1;
    let currentLicenses = [];

    // ===== DOM =====
    const searchInput = document.getElementById('searchInput');
    const searchBtn = document.getElementById('searchBtn');
    const createBtn = document.getElementById('createBtn');
    const tableBody = document.getElementById('licenseTableBody');

    // ===== 工具函数 =====
    function getPlanInfo(planType) {
        return PLAN_TYPE_MAP[planType] || { name: '未知', cls: 'plan-unknown' };
    }

    // 剩余天数徽章：<0 已过期(红)；0-7 即将到期(红)；8-30 橙；>30 绿
    function getDaysRemainingBadge(days) {
        if (days === null || days === undefined) {
            return '<span class="badge inactive">未知</span>';
        }
        if (days < 0) {
            return '<span class="badge expired">已过期</span>';
        }
        if (days <= 7) {
            return '<span class="badge expired">即将到期 ' + days + '天</span>';
        }
        if (days <= 30) {
            return '<span class="badge trial">剩余 ' + days + '天</span>';
        }
        return '<span class="badge active">有效 ' + days + '天</span>';
    }

    function getActiveBadge(isActive) {
        return isActive
            ? '<span class="badge active">启用</span>'
            : '<span class="badge inactive">停用</span>';
    }

    function findLicense(id) {
        id = parseInt(id, 10);
        return currentLicenses.find(function (l) { return l.id === id; });
    }

    // 默认到期日期：今天 + 365 天，格式 YYYY-MM-DD
    function defaultEndDate() {
        const d = new Date();
        d.setDate(d.getDate() + 365);
        const pad = function (n) { return n < 10 ? '0' + n : '' + n; };
        return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());
    }

    // ===== 渲染表格 =====
    function renderTable(licenses) {
        if (!licenses || licenses.length === 0) {
            tableBody.innerHTML = '<tr class="empty-row"><td colspan="11">暂无数据</td></tr>';
            return;
        }

        const html = licenses.map(function (lic) {
            const plan = getPlanInfo(lic.planType);
            const daysBadge = getDaysRemainingBadge(lic.daysRemaining);
            const activeBadge = getActiveBadge(lic.isActive);
            const teamName = lic.teamName ? escapeHtml(lic.teamName) : '<span class="text-gray">—</span>';
            const licenseKey = escapeHtml(lic.licenseKey);

            const ops = [
                '<button class="btn btn-sm" data-action="renew" data-id="' + lic.id + '">' + ICONS.clock + '续费</button>',
                '<button class="btn btn-sm" data-action="quota" data-id="' + lic.id + '">' + ICONS.settings + '修改配额</button>',
                '<button class="btn btn-sm btn-danger" data-action="deactivate" data-id="' + lic.id + '">' + ICONS.unlink + '解绑机器</button>'
            ].join('');

            return '<tr>' +
                '<td class="license-key">' + licenseKey + '</td>' +
                '<td>' + teamName + '</td>' +
                '<td><span class="badge plan-badge ' + plan.cls + '">' + plan.name + '</span></td>' +
                '<td>' + lic.seats + '</td>' +
                '<td>' + lic.maxProjects + '</td>' +
                '<td>' + formatDate(lic.startDate) + '</td>' +
                '<td>' + formatDate(lic.endDate) + '</td>' +
                '<td>' + daysBadge + '</td>' +
                '<td>' + (lic.activationCount || 0) + '</td>' +
                '<td>' + activeBadge + '</td>' +
                '<td class="table-actions">' + ops + '</td>' +
                '</tr>';
        }).join('');

        tableBody.innerHTML = html;
    }

    // ===== 加载 License 列表 =====
    async function loadLicenses() {
        tableBody.innerHTML = '<tr class="empty-row"><td colspan="11">加载中...</td></tr>';
        try {
            const data = await apiGet('/Admin/Licenses', {
                page: currentPage,
                pageSize: PAGE_SIZE,
                keyword: currentKeyword
            });
            currentLicenses = (data && data.licenses) || [];
            const total = (data && data.total) || 0;
            totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
            renderTable(currentLicenses);
            renderPagination('pagination', currentPage, totalPages, function (page) {
                currentPage = page;
                loadLicenses();
            });
        } catch (e) {
            tableBody.innerHTML = '<tr class="empty-row"><td colspan="11">加载失败：' + escapeHtml(e.message || '未知错误') + '</td></tr>';
            showToast(e.message || '加载失败', 'error');
        }
    }

    // ===== 通用弹窗构建 =====
    function buildModal(title, bodyHtml, okText, wide) {
        const overlay = document.createElement('div');
        overlay.className = 'modal-overlay';

        const card = document.createElement('div');
        card.className = 'modal-card' + (wide ? ' wide' : '');
        card.innerHTML =
            '<div class="modal-header">' +
            '<div class="modal-title">' + escapeHtml(title) + '</div>' +
            '<button class="modal-close" data-act="close">' + ICONS.x + '</button>' +
            '</div>' +
            '<div class="modal-body">' + bodyHtml + '</div>' +
            '<div class="modal-footer">' +
                '<button class="btn" data-act="cancel">取消</button>' +
                '<button class="btn btn-primary" data-act="ok">' + escapeHtml(okText || '确定') + '</button>' +
            '</div>';

        overlay.appendChild(card);
        document.body.appendChild(overlay);

        function close() {
            if (overlay.parentNode) overlay.parentNode.removeChild(overlay);
        }

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) close();
        });
        overlay.querySelector('[data-act="cancel"]').addEventListener('click', close);
        overlay.querySelector('[data-act="close"]').addEventListener('click', close);

        return {
            overlay: overlay,
            body: card.querySelector('.modal-body'),
            okBtn: card.querySelector('[data-act="ok"]'),
            close: close
        };
    }

    // ===== 创建 License 弹窗 =====
    function openCreateModal() {
        const bodyHtml =
            '<div class="form-group">' +
                '<label>团队 ID (GUID)</label>' +
                '<input type="text" id="createTeamId" placeholder="请输入团队 GUID">' +
            '</div>' +
            '<div class="form-group">' +
                '<label>套餐类型</label>' +
                '<select id="createPlanType">' +
                    '<option value="0">试用</option>' +
                    '<option value="1" selected>标准</option>' +
                    '<option value="2">专业</option>' +
                    '<option value="3">旗舰</option>' +
                '</select>' +
            '</div>' +
            '<div class="quota-row">' +
                '<div class="form-group"><label>座位数</label><input type="number" id="createSeats" value="1" min="1"></div>' +
                '<div class="form-group"><label>最大项目数</label><input type="number" id="createMaxProjects" value="1" min="1"></div>' +
                '<div class="form-group"><label>最大模板数</label><input type="number" id="createMaxTemplates" value="1" min="1"></div>' +
            '</div>' +
            '<div class="form-group">' +
                '<label>到期日期</label>' +
                '<input type="date" id="createEndDate">' +
            '</div>';

        const modal = buildModal('创建 License', bodyHtml, '创建', true);
        document.getElementById('createEndDate').value = defaultEndDate();

        modal.okBtn.addEventListener('click', async function () {
            const teamId = document.getElementById('createTeamId').value.trim();
            const planType = parseInt(document.getElementById('createPlanType').value, 10);
            const seats = parseInt(document.getElementById('createSeats').value, 10);
            const maxProjects = parseInt(document.getElementById('createMaxProjects').value, 10);
            const maxTemplates = parseInt(document.getElementById('createMaxTemplates').value, 10);
            const endDate = document.getElementById('createEndDate').value;

            if (!teamId) { showToast('请输入团队 GUID', 'error'); return; }
            if (!endDate) { showToast('请选择到期日期', 'error'); return; }
            if (!seats || seats < 1) { showToast('座位数必须大于 0', 'error'); return; }
            if (!maxProjects || maxProjects < 1) { showToast('最大项目数必须大于 0', 'error'); return; }
            if (!maxTemplates || maxTemplates < 1) { showToast('最大模板数必须大于 0', 'error'); return; }

            modal.okBtn.disabled = true;
            try {
                const data = await apiPost('/Admin/CreateLicense', {
                    teamId: teamId,
                    planType: planType,
                    seats: seats,
                    maxProjects: maxProjects,
                    maxTemplates: maxTemplates,
                    endDate: endDate
                });
                const key = data && data.licenseKey;
                showToast('创建成功，License Key：' + (key || ''), 'success');
                modal.close();
                loadLicenses();
            } catch (e) {
                showToast(e.message || '创建失败', 'error');
            } finally {
                modal.okBtn.disabled = false;
            }
        });
    }

    // ===== 续费弹窗 =====
    function openRenewModal(licenseId) {
        const lic = findLicense(licenseId);
        if (!lic) { showToast('License 不存在', 'error'); return; }

        const currentEnd = formatDate(lic.endDate);
        const bodyHtml =
            '<div class="current-info"><span class="label">当前到期日期：</span><span class="value">' + escapeHtml(currentEnd) + '</span></div>' +
            '<div class="form-group">' +
                '<label>延长天数</label>' +
                '<input type="number" id="renewDays" value="365" min="1">' +
                '<div class="quick-days">' +
                    '<button type="button" class="btn btn-sm" data-days="30">30天</button>' +
                    '<button type="button" class="btn btn-sm" data-days="90">90天</button>' +
                    '<button type="button" class="btn btn-sm" data-days="180">180天</button>' +
                    '<button type="button" class="btn btn-sm" data-days="365">365天</button>' +
                '</div>' +
            '</div>';

        const modal = buildModal('续费 License', bodyHtml, '确认续费', false);
        const daysInput = document.getElementById('renewDays');

        modal.body.querySelectorAll('.quick-days button').forEach(function (btn) {
            btn.addEventListener('click', function () {
                daysInput.value = btn.getAttribute('data-days');
            });
        });

        modal.okBtn.addEventListener('click', async function () {
            const extendDays = parseInt(daysInput.value, 10);
            if (!extendDays || extendDays < 1) { showToast('请输入有效的延长天数', 'error'); return; }

            modal.okBtn.disabled = true;
            try {
                const data = await apiPost('/Admin/RenewLicense', {
                    licenseId: licenseId,
                    extendDays: extendDays
                });
                const newEnd = data && data.newEndDate ? formatDate(data.newEndDate) : '';
                showToast('续费成功，新到期日期：' + newEnd, 'success');
                modal.close();
                loadLicenses();
            } catch (e) {
                showToast(e.message || '续费失败', 'error');
            } finally {
                modal.okBtn.disabled = false;
            }
        });
    }

    // ===== 修改配额弹窗 =====
    function openQuotaModal(licenseId) {
        const lic = findLicense(licenseId);
        if (!lic) { showToast('License 不存在', 'error'); return; }

        const bodyHtml =
            '<div class="quota-row">' +
                '<div class="form-group"><label>座位数</label><input type="number" id="quotaSeats" value="' + lic.seats + '" min="1"></div>' +
                '<div class="form-group"><label>最大项目数</label><input type="number" id="quotaMaxProjects" value="' + lic.maxProjects + '" min="1"></div>' +
                '<div class="form-group"><label>最大模板数</label><input type="number" id="quotaMaxTemplates" value="' + (lic.maxTemplates || 0) + '" min="1"></div>' +
            '</div>';

        const modal = buildModal('修改配额', bodyHtml, '保存', true);

        modal.okBtn.addEventListener('click', async function () {
            const seats = parseInt(document.getElementById('quotaSeats').value, 10);
            const maxProjects = parseInt(document.getElementById('quotaMaxProjects').value, 10);
            const maxTemplates = parseInt(document.getElementById('quotaMaxTemplates').value, 10);

            if (!seats || seats < 1) { showToast('座位数必须大于 0', 'error'); return; }
            if (!maxProjects || maxProjects < 1) { showToast('最大项目数必须大于 0', 'error'); return; }
            if (!maxTemplates || maxTemplates < 1) { showToast('最大模板数必须大于 0', 'error'); return; }

            modal.okBtn.disabled = true;
            try {
                await apiPost('/Admin/UpdateLicenseQuota', {
                    licenseId: licenseId,
                    seats: seats,
                    maxProjects: maxProjects,
                    maxTemplates: maxTemplates
                });
                showToast('配额修改成功', 'success');
                modal.close();
                loadLicenses();
            } catch (e) {
                showToast(e.message || '修改失败', 'error');
            } finally {
                modal.okBtn.disabled = false;
            }
        });
    }

    // ===== 解绑机器弹窗 =====
    function openDeactivateModal(licenseId) {
        const bodyHtml =
            '<div class="form-group">' +
                '<label>机器码</label>' +
                '<input type="text" id="deactivateMachineCode" placeholder="请输入需要解绑的机器码">' +
            '</div>';

        const modal = buildModal('解绑机器', bodyHtml, '确认解绑', false);

        modal.okBtn.addEventListener('click', async function () {
            const machineCode = document.getElementById('deactivateMachineCode').value.trim();
            if (!machineCode) { showToast('请输入机器码', 'error'); return; }

            const confirmed = await confirmDialog('确定要解绑该机器吗？此操作不可撤销。');
            if (!confirmed) return;

            modal.okBtn.disabled = true;
            try {
                await apiPost('/Admin/DeactivateMachine', {
                    licenseId: licenseId,
                    machineCode: machineCode
                });
                showToast('机器解绑成功', 'success');
                modal.close();
                loadLicenses();
            } catch (e) {
                showToast(e.message || '解绑失败', 'error');
            } finally {
                modal.okBtn.disabled = false;
            }
        });
    }

    // ===== 事件绑定 =====
    // 搜索按钮
    searchBtn.addEventListener('click', function () {
        currentKeyword = searchInput.value.trim();
        currentPage = 1;
        loadLicenses();
    });

    // 搜索框回车触发搜索
    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            currentKeyword = searchInput.value.trim();
            currentPage = 1;
            loadLicenses();
        }
    });

    // 创建按钮
    createBtn.addEventListener('click', openCreateModal);

    // 表格操作按钮（事件委托）
    tableBody.addEventListener('click', function (e) {
        const btn = e.target.closest('button[data-action]');
        if (!btn) return;
        const action = btn.getAttribute('data-action');
        const id = parseInt(btn.getAttribute('data-id'), 10);
        if (!id) return;

        if (action === 'renew') {
            openRenewModal(id);
        } else if (action === 'quota') {
            openQuotaModal(id);
        } else if (action === 'deactivate') {
            openDeactivateModal(id);
        }
    });

    // ===== 初始化 =====
    function init() {
        if (!requireAdmin()) return;
        loadLicenses();
    }

    init();
})();
