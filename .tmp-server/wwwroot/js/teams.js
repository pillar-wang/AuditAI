/* ===== 团队管理页面逻辑 ===== */

(function () {
    'use strict';

    const PAGE_SIZE = 20;

    // 枚举映射
    const LEVEL_MAP = { 0: '无', 1: '标准', 2: '专业', 3: '旗舰' };
    const PAYSTATUS_MAP = {
        0: { text: '试用',   cls: 'trial' },     // orange
        1: { text: '已付费', cls: 'active' },    // green
        2: { text: '已过期', cls: 'expired' },   // red
        3: { text: '免费',   cls: 'inactive' }   // gray
    };
    const ROLE_MAP = { 0: '经理', 1: '助理', 2: '审核', 3: '编辑', 4: '普通用户' };

    // 运行时状态
    let currentPage = 1;
    let currentKeyword = '';
    let teamCache = {};          // teamId -> team 对象，供编辑时取值
    let currentMembersTeamId = null;

    // 元素引用
    const keywordInput = document.getElementById('keywordInput');
    const searchBtn = document.getElementById('searchBtn');
    const resetBtn = document.getElementById('resetBtn');
    const addTeamBtn = document.getElementById('addTeamBtn');
    const teamsTbody = document.getElementById('teamsTbody');
    const paginationEl = document.getElementById('pagination');
    const totalCountInfo = document.getElementById('totalCountInfo');

    const editModal = document.getElementById('editModal');
    const editTeamIdEl = document.getElementById('editTeamId');
    const editNameEl = document.getElementById('editName');
    const editLevelEl = document.getElementById('editLevel');
    const editPayStatusEl = document.getElementById('editPayStatus');
    const editMaxUsersEl = document.getElementById('editMaxUsers');
    const editMaxProjectsEl = document.getElementById('editMaxProjects');
    const editMaxTemplatesEl = document.getElementById('editMaxTemplates');
    const editCancelBtn = document.getElementById('editCancelBtn');
    const editSaveBtn = document.getElementById('editSaveBtn');

    const membersModal = document.getElementById('membersModal');
    const membersTeamNameEl = document.getElementById('membersTeamName');
    const membersTbody = document.getElementById('membersTbody');
    const membersCloseBtn = document.getElementById('membersCloseBtn');

    /* ---------- 工具函数 ---------- */

    function levelText(level) {
        return LEVEL_MAP[level] !== undefined ? LEVEL_MAP[level] : String(level);
    }

    function roleText(role) {
        return ROLE_MAP[role] !== undefined ? ROLE_MAP[role] : String(role);
    }

    function payStatusBadge(payStatus) {
        const ps = PAYSTATUS_MAP[payStatus] || { text: String(payStatus), cls: 'inactive' };
        return '<span class="badge ' + ps.cls + '">' + escapeHtml(ps.text) + '</span>';
    }

    /* ---------- 加载团队列表 ---------- */

    async function loadTeams() {
        teamsTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;">加载中...</td></tr>';
        paginationEl.innerHTML = '';
        totalCountInfo.textContent = '';

        try {
            const data = await apiGet('/Admin/Teams', {
                page: currentPage,
                pageSize: PAGE_SIZE,
                keyword: currentKeyword
            });

            const teams = (data && data.teams) || [];
            const total = (data && typeof data.total === 'number') ? data.total : teams.length;

            // 缓存当前页团队
            teamCache = {};
            teams.forEach(function (t) { teamCache[t.id] = t; });

            renderTeams(teams);
            totalCountInfo.textContent = '共 ' + total + ' 条';

            const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
            renderPagination('pagination', currentPage, totalPages, function (page) {
                currentPage = page;
                loadTeams();
            });
        } catch (e) {
            teamsTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;">加载失败：' +
                escapeHtml(e && e.message ? e.message : '') + '</td></tr>';
            showToast('加载团队列表失败', 'error');
        }
    }

    function renderTeams(teams) {
        if (!teams || teams.length === 0) {
            teamsTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;">暂无数据</td></tr>';
            return;
        }

        teamsTbody.innerHTML = teams.map(function (t) {
            return '<tr>' +
                '<td>' + escapeHtml(t.name) + '</td>' +
                '<td>' + escapeHtml(levelText(t.level)) + '</td>' +
                '<td>' + payStatusBadge(t.payStatus) + '</td>' +
                '<td>' + escapeHtml(t.ownerName) + '</td>' +
                '<td>' + escapeHtml(String(t.memberCount)) + '</td>' +
                '<td>' + escapeHtml(String(t.projectCount)) + '</td>' +
                '<td>' + escapeHtml(String(t.maxUsers)) + '</td>' +
                '<td>' + escapeHtml(String(t.maxProjects)) + '</td>' +
                '<td>' + escapeHtml(formatDate(t.licenseDate)) + '</td>' +
                '<td><div class="op-btns">' +
                    '<button class="btn btn-sm btn-primary" data-act="edit" data-id="' + escapeHtml(t.id) + '">' + ICONS.edit + '编辑</button>' +
                    '<button class="btn btn-sm" data-act="members" data-id="' + escapeHtml(t.id) + '">' + ICONS.users + '查看成员</button>' +
                    '<button class="btn btn-sm btn-danger" data-act="delete" data-id="' + escapeHtml(t.id) + '" data-name="' + escapeHtml(t.name) + '">' + ICONS.trash + '删除</button>' +
                '</div></td>' +
                '</tr>';
        }).join('');
    }

    /* ---------- 表格操作（事件委托） ---------- */

    teamsTbody.addEventListener('click', function (e) {
        const btn = e.target.closest('button[data-act]');
        if (!btn) return;
        const act = btn.getAttribute('data-act');
        const teamId = btn.getAttribute('data-id');
        if (act === 'edit') {
            openEditModal(teamId);
        } else if (act === 'members') {
            openMembersModal(teamId);
        } else if (act === 'delete') {
            const teamName = btn.getAttribute('data-name') || '该团队';
            deleteTeam(teamId, teamName);
        }
    });

    /* ---------- 删除团队 ---------- */

    async function deleteTeam(teamId, teamName) {
        const ok = await confirmDialog('确定要删除团队 "' + teamName + '" 吗？\n删除后该团队下的所有项目将无法访问，此操作不可撤销。');
        if (!ok) return;

        try {
            await apiPost('/Admin/DeleteTeam', { teamId: teamId });
            showToast('团队已删除', 'success');
            loadTeams();
        } catch (e) {
            showToast('删除失败：' + (e && e.message ? e.message : ''), 'error');
        }
    }

    /* ---------- 搜索 ---------- */

    function doSearch() {
        currentKeyword = (keywordInput.value || '').trim();
        currentPage = 1;
        loadTeams();
    }

    searchBtn.addEventListener('click', doSearch);
    keywordInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            doSearch();
        }
    });
    resetBtn.addEventListener('click', function () {
        keywordInput.value = '';
        currentKeyword = '';
        currentPage = 1;
        loadTeams();
    });

    /* ---------- 新增团队 ---------- */

    function showModal(title, bodyHtml, onSave, saveText) {
        var overlay = document.createElement('div');
        overlay.className = 'modal-overlay';
        overlay.innerHTML =
            '<div class="modal-card" style="width:480px;">' +
            '<div class="modal-header">' +
            '<div class="modal-title">' + escapeHtml(title) + '</div>' +
            '<button class="modal-close" data-role="close">' + ICONS.x + '</button>' +
            '</div>' +
            '<div class="modal-body">' + bodyHtml + '</div>' +
            '<div class="modal-footer">' +
            '<button class="btn" data-role="cancel">取消</button>' +
            '<button class="btn btn-primary" data-role="save">' + escapeHtml(saveText || '保存') + '</button>' +
            '</div>' +
            '</div>';
        document.body.appendChild(overlay);

        function close() {
            if (overlay.parentNode) {
                overlay.parentNode.removeChild(overlay);
            }
        }

        overlay.querySelector('[data-role="cancel"]').addEventListener('click', close);
        overlay.querySelector('[data-role="close"]').addEventListener('click', close);
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) close();
        });
        overlay.querySelector('[data-role="save"]').addEventListener('click', function () {
            onSave(overlay, close);
        });

        return overlay;
    }

    addTeamBtn.addEventListener('click', function () {
        var bodyHtml =
            '<div class="form-group"><label>团队名称 *</label>' +
            '<input type="text" id="addTeamName" placeholder="请输入团队名称"></div>' +
            '<div class="form-group"><label>类型</label>' +
            '<select id="addTeamType">' +
            '<option value="0">默认</option>' +
            '<option value="1">企业</option>' +
            '<option value="2">个人</option>' +
            '</select></div>';

        showModal('新增团队', bodyHtml, function (overlay, close) {
            var teamName = overlay.querySelector('#addTeamName').value.trim();
            var type = Number(overlay.querySelector('#addTeamType').value);

            if (!teamName) {
                showToast('请输入团队名称', 'error');
                return;
            }

            apiPost('/Admin/CreateTeam', { teamName: teamName, type: type }).then(function (res) {
                if (res && res.success) {
                    showToast('团队创建成功', 'success');
                    close();
                    loadTeams();
                } else {
                    showToast((res && res.message) || (res && res.error) || '创建失败', 'error');
                }
            }).catch(function (e) {
                showToast((e && e.message) || '创建失败', 'error');
            });
        }, '创建');
    });

    /* ---------- 编辑团队 ---------- */

    function openEditModal(teamId) {
        const team = teamCache[teamId];
        if (!team) {
            showToast('团队信息未找到，请刷新列表', 'error');
            return;
        }
        editTeamIdEl.value = team.id;
        editNameEl.value = team.name || '';
        editLevelEl.value = String(team.level);
        editPayStatusEl.value = String(team.payStatus);
        editMaxUsersEl.value = team.maxUsers != null ? team.maxUsers : '';
        editMaxProjectsEl.value = team.maxProjects != null ? team.maxProjects : '';
        editMaxTemplatesEl.value = team.maxTemplates != null ? team.maxTemplates : '';
        editModal.style.display = 'flex';
    }

    function closeEditModal() {
        editModal.style.display = 'none';
    }

    editCancelBtn.addEventListener('click', closeEditModal);
    editModal.addEventListener('click', function (e) {
        if (e.target === editModal) closeEditModal();
    });

    editSaveBtn.addEventListener('click', async function () {
        const teamId = editTeamIdEl.value;
        const name = (editNameEl.value || '').trim();
        if (!name) {
            showToast('请输入团队名', 'error');
            return;
        }
        const level = parseInt(editLevelEl.value, 10);
        const payStatus = parseInt(editPayStatusEl.value, 10);
        const maxUsers = parseInt(editMaxUsersEl.value, 10);
        const maxProjects = parseInt(editMaxProjectsEl.value, 10);
        const maxTemplates = parseInt(editMaxTemplatesEl.value, 10);

        if (isNaN(maxUsers) || maxUsers < 0) {
            showToast('最大用户数无效', 'error');
            return;
        }
        if (isNaN(maxProjects) || maxProjects < 0) {
            showToast('最大项目数无效', 'error');
            return;
        }
        if (isNaN(maxTemplates) || maxTemplates < 0) {
            showToast('最大模板数无效', 'error');
            return;
        }

        const body = {
            teamId: teamId,
            name: name,
            level: level,
            payStatus: payStatus,
            maxUsers: maxUsers,
            maxProjects: maxProjects,
            maxTemplates: maxTemplates
        };

        editSaveBtn.disabled = true;
        editSaveBtn.textContent = '保存中...';
        try {
            await apiPost('/Admin/UpdateTeam', body);
            showToast('保存成功', 'success');
            closeEditModal();
            await loadTeams();
        } catch (e) {
            showToast('保存失败：' + (e && e.message ? e.message : ''), 'error');
        } finally {
            editSaveBtn.disabled = false;
            editSaveBtn.textContent = '保存';
        }
    });

    /* ---------- 成员列表 ---------- */

    function openMembersModal(teamId) {
        const team = teamCache[teamId];
        currentMembersTeamId = teamId;
        membersTeamNameEl.textContent = team ? '— ' + (team.name || '') : '';
        membersModal.style.display = 'flex';
        loadMembers(teamId);
    }

    function closeMembersModal() {
        membersModal.style.display = 'none';
        membersTbody.innerHTML = '<tr><td colspan="8" class="modal-loading">加载中...</td></tr>';
        currentMembersTeamId = null;
    }

    membersCloseBtn.addEventListener('click', closeMembersModal);
    membersModal.addEventListener('click', function (e) {
        if (e.target === membersModal) closeMembersModal();
    });

    async function loadMembers(teamId) {
        membersTbody.innerHTML = '<tr><td colspan="8" class="modal-loading">加载中...</td></tr>';
        try {
            const data = await apiGet('/Admin/Teams/' + encodeURIComponent(teamId) + '/Members');
            const members = (data && data.members) || [];
            renderMembers(members);
        } catch (e) {
            membersTbody.innerHTML = '<tr><td colspan="8" class="modal-loading">加载失败：' +
                escapeHtml(e && e.message ? e.message : '') + '</td></tr>';
            showToast('加载成员列表失败', 'error');
        }
    }

    function renderMembers(members) {
        if (!members || members.length === 0) {
            membersTbody.innerHTML = '<tr><td colspan="8" class="modal-loading">暂无成员</td></tr>';
            return;
        }
        membersTbody.innerHTML = members.map(function (m) {
            const adminBadge = m.isTeamAdmin
                ? '<span class="badge active">管理员</span>'
                : '<span class="badge inactive">普通成员</span>';
            const activeBadge = m.isActive
                ? '<span class="badge active">启用</span>'
                : '<span class="badge inactive">禁用</span>';
            return '<tr>' +
                '<td>' + escapeHtml(m.userName) + '</td>' +
                '<td>' + escapeHtml(m.name) + '</td>' +
                '<td>' + escapeHtml(m.phone) + '</td>' +
                '<td>' + escapeHtml(m.email) + '</td>' +
                '<td>' + escapeHtml(roleText(m.role)) + '</td>' +
                '<td>' + adminBadge + '</td>' +
                '<td>' + activeBadge + '</td>' +
                '<td><button class="btn btn-sm btn-danger" data-act="remove" ' +
                    'data-userid="' + escapeHtml(String(m.id)) + '" ' +
                    'data-name="' + escapeHtml(m.name || m.userName || '') + '">' + ICONS.x + '移除</button></td>' +
                '</tr>';
        }).join('');
    }

    // 成员操作（事件委托）
    membersTbody.addEventListener('click', async function (e) {
        const btn = e.target.closest('button[data-act="remove"]');
        if (!btn) return;
        const userId = parseInt(btn.getAttribute('data-userid'), 10);
        const displayName = btn.getAttribute('data-name') || '该成员';
        const teamId = currentMembersTeamId;
        if (!teamId || isNaN(userId)) return;

        const ok = await confirmDialog('确定要将成员 ' + displayName + ' 从团队中移除吗？');
        if (!ok) return;

        btn.disabled = true;
        try {
            await apiPost('/Admin/RemoveTeamMember', { userId: userId, teamId: teamId });
            showToast('已移除成员', 'success');
            await loadMembers(teamId);
            // 成员数可能变化，刷新团队列表
            await loadTeams();
        } catch (err) {
            showToast('移除失败：' + (err && err.message ? err.message : ''), 'error');
            btn.disabled = false;
        }
    });

    /* ---------- 初始化 ---------- */

    function init() {
        if (!requireAdmin()) return;
        loadTeams();
    }

    init();
})();
