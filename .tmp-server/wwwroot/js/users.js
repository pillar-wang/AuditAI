/* ===== 用户管理页面逻辑 ===== */

(function () {
    'use strict';

    // ===== 常量 =====
    var PAGE_SIZE = 20;

    // 角色映射：0=经理, 1=助理, 2=审核, 3=编辑, 4=普通用户
    var ROLE_MAP = {
        0: '经理',
        1: '助理',
        2: '审核',
        3: '编辑',
        4: '普通用户'
    };

    // ===== 状态 =====
    var currentPage = 1;
    var keyword = '';
    var isActiveFilter = ''; // '' | 'true' | 'false'
    var loading = false;
    var usersCache = {}; // id -> user，用于操作按钮回查

    // ===== DOM 引用 =====
    var searchInput = document.getElementById('searchInput');
    var statusFilter = document.getElementById('statusFilter');
    var searchBtn = document.getElementById('searchBtn');
    var resetBtn = document.getElementById('resetBtn');
    var addUserBtn = document.getElementById('addUserBtn');
    var tableBody = document.getElementById('usersTableBody');
    var loadingTip = document.getElementById('loadingTip');
    var emptyTip = document.getElementById('emptyTip');

    // ===== 初始化 =====
    function init() {
        if (!requireAdmin()) {
            return;
        }
        bindEvents();
        loadUsers();
    }

    function bindEvents() {
        searchBtn.addEventListener('click', onSearch);
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                e.preventDefault();
                onSearch();
            }
        });
        resetBtn.addEventListener('click', onReset);
        addUserBtn.addEventListener('click', openAddModal);
    }

    function onSearch() {
        keyword = searchInput.value.trim();
        isActiveFilter = statusFilter.value;
        currentPage = 1;
        loadUsers();
    }

    function onReset() {
        searchInput.value = '';
        statusFilter.value = '';
        keyword = '';
        isActiveFilter = '';
        currentPage = 1;
        loadUsers();
    }

    // ===== 加载用户列表 =====
    function loadUsers() {
        if (loading) return;
        loading = true;
        showLoading(true);
        hideEmpty();
        tableBody.innerHTML = '';

        var params = {
            page: currentPage,
            pageSize: PAGE_SIZE
        };
        if (keyword) {
            params.keyword = keyword;
        }
        if (isActiveFilter !== '') {
            params.isActive = isActiveFilter;
        }

        apiGet('/Admin/Users', params).then(function (data) {
            var users = (data && data.users) || [];
            var total = (data && data.total) || 0;
            var pageSize = (data && data.pageSize) || PAGE_SIZE;
            renderTable(users);

            var totalPages = Math.max(1, Math.ceil(total / pageSize));
            renderPagination('pagination', currentPage, totalPages, function (page) {
                currentPage = page;
                loadUsers();
            });

            loading = false;
            showLoading(false);
        }).catch(function (e) {
            loading = false;
            showLoading(false);
            showToast((e && e.message) || '加载用户列表失败', 'error');
            renderPagination('pagination', currentPage, 1, function (page) {
                currentPage = page;
                loadUsers();
            });
        });
    }

    function showLoading(show) {
        if (loadingTip) loadingTip.style.display = show ? '' : 'none';
    }

    function showEmpty(show) {
        if (emptyTip) emptyTip.style.display = show ? '' : 'none';
    }

    function hideEmpty() {
        if (emptyTip) emptyTip.style.display = 'none';
    }

    // ===== 渲染表格 =====
    function renderTable(users) {
        usersCache = {};
        if (!users.length) {
            tableBody.innerHTML = '';
            showEmpty(true);
            return;
        }
        showEmpty(false);

        var currentUserId = getCurrentUserId();

        var html = users.map(function (u) {
            usersCache[u.id] = u;
            return buildRow(u, currentUserId);
        }).join('');
        tableBody.innerHTML = html;

        // 绑定操作按钮事件
        var btns = tableBody.querySelectorAll('[data-action]');
        for (var i = 0; i < btns.length; i++) {
            btns[i].addEventListener('click', function () {
                var action = this.getAttribute('data-action');
                var id = Number(this.getAttribute('data-id'));
                handleAction(action, id);
            });
        }
    }

    function buildRow(u, currentUserId) {
        var roleText = ROLE_MAP[u.role] !== undefined ? ROLE_MAP[u.role] : '未知';
        var statusBadge = u.isActive
            ? '<span class="badge active">启用</span>'
            : '<span class="badge inactive">禁用</span>';

        var isSysAdmin = u.isSystemAdmin === true || u.IsSystemAdmin === true;
        var sysBadge = isSysAdmin
            ? '<span class="badge sys-badge">系统管理员</span>'
            : '';

        var nameCell = '<td class="name-cell">' + escapeHtml(u.name || '') + sysBadge + '</td>';

        var isSelf = currentUserId !== null && String(u.id) === String(currentUserId);
        var toggleText = u.isActive ? '禁用' : '启用';

        var editBtn = '<button class="btn btn-sm" data-action="edit" data-id="' + escapeHtml(u.id) + '">' + ICONS.edit + '编辑</button>';
        var joinTeamBtn = '<button class="btn btn-sm" data-action="joinTeam" data-id="' + escapeHtml(u.id) + '">' + ICONS.userPlus + '加入团队</button>';
        var resetBtn = '<button class="btn btn-sm" data-action="reset" data-id="' + escapeHtml(u.id) + '">' + ICONS.key + '重置密码</button>';
        var toggleBtn = '<button class="btn btn-sm" data-action="toggle" data-id="' + escapeHtml(u.id) + '">' + ICONS.power + toggleText + '</button>';
        var deleteBtn;
        if (isSelf) {
            deleteBtn = '<button class="btn btn-sm btn-danger" data-action="delete" data-id="' + escapeHtml(u.id) + '" disabled title="不能删除当前登录账号">' + ICONS.trash + '删除</button>';
        } else {
            deleteBtn = '<button class="btn btn-sm btn-danger" data-action="delete" data-id="' + escapeHtml(u.id) + '">' + ICONS.trash + '删除</button>';
        }

        return '<tr>' +
            '<td>' + escapeHtml(u.id) + '</td>' +
            '<td>' + escapeHtml(u.userName || '') + '</td>' +
            nameCell +
            '<td>' + escapeHtml(u.phone || '') + '</td>' +
            '<td>' + escapeHtml(u.email || '') + '</td>' +
            '<td>' + escapeHtml(roleText) + '</td>' +
            '<td>' + escapeHtml(u.teamName || '') + '</td>' +
            '<td>' + statusBadge + '</td>' +
            '<td>' + escapeHtml(formatDate(u.createTime)) + '</td>' +
            '<td><div class="ops-cell">' + editBtn + joinTeamBtn + resetBtn + toggleBtn + deleteBtn + '</div></td>' +
            '</tr>';
    }

    // 获取当前登录管理员 ID（兼容 userInfo.id 与 auth.userId）
    function getCurrentUserId() {
        var userInfo = getUserInfo();
        if (userInfo && userInfo.id !== undefined && userInfo.id !== null) {
            return userInfo.id;
        }
        var auth = getAuth();
        if (auth && auth.userId) {
            return auth.userId;
        }
        return null;
    }

    // ===== 操作分发 =====
    function handleAction(action, id) {
        var user = usersCache[id];
        if (!user) {
            showToast('用户数据不存在，请刷新重试', 'error');
            return;
        }
        if (action === 'edit') {
            openEditModal(user);
        } else if (action === 'joinTeam') {
            openJoinTeamModal(user);
        } else if (action === 'reset') {
            openPasswordModal(user);
        } else if (action === 'toggle') {
            handleToggle(user);
        } else if (action === 'delete') {
            handleDelete(user);
        }
    }

    // ===== 通用模态框 =====
    // 返回 overlay 元素；onSave(overlay, close) 在点击保存时触发
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

    // 复选框表单组 HTML
    function checkboxGroup(id, label, checked) {
        return '<div class="form-group">' +
            '<label class="checkbox-label">' +
            '<input type="checkbox" id="' + id + '"' + (checked ? ' checked' : '') + '>' +
            escapeHtml(label) +
            '</label>' +
            '</div>';
    }

    // ===== 编辑用户 =====
    function openEditModal(user) {
        var roleOptions = '';
        Object.keys(ROLE_MAP).forEach(function (key) {
            var selected = Number(user.role) === Number(key) ? ' selected' : '';
            roleOptions += '<option value="' + key + '"' + selected + '>' + escapeHtml(ROLE_MAP[key]) + '</option>';
        });

        var userTeamId = user.teamId || user.TeamId || '';
        var bodyHtml =
            '<div class="form-group"><label>用户名 *</label>' +
            '<input type="text" id="editUserName" value="' + escapeHtml(user.userName || '') + '" placeholder="请输入用户名"></div>' +
            '<div class="form-group"><label>姓名</label>' +
            '<input type="text" id="editName" value="' + escapeHtml(user.name || '') + '"></div>' +
            '<div class="form-group"><label>手机号</label>' +
            '<input type="text" id="editPhone" value="' + escapeHtml(user.phone || '') + '"></div>' +
            '<div class="form-group"><label>邮箱</label>' +
            '<input type="email" id="editEmail" value="' + escapeHtml(user.email || '') + '"></div>' +
            '<div class="form-group"><label>所属团队</label>' +
            '<select id="editTeamId"><option value="">-- 无团队 --</option></select>' +
            '<div class="form-tip">变更所属团队会同步更新用户的主团队，并将其加入新团队的成员列表（保留原团队成员关系）。</div></div>' +
            '<div class="form-group"><label>角色</label>' +
            '<select id="editRole">' + roleOptions + '</select></div>' +
            checkboxGroup('editTeamAdmin', '团队管理员', !!user.isTeamAdmin) +
            checkboxGroup('editDataAdmin', '数据管理员', !!user.isDataAdmin) +
            checkboxGroup('editActive', '启用', !!user.isActive);

        showModal('编辑用户', bodyHtml, function (overlay, close) {
            var userName = overlay.querySelector('#editUserName').value.trim();
            var name = overlay.querySelector('#editName').value.trim();
            var phone = overlay.querySelector('#editPhone').value.trim();
            var email = overlay.querySelector('#editEmail').value.trim();
            var teamId = overlay.querySelector('#editTeamId').value;
            var role = Number(overlay.querySelector('#editRole').value);
            var isTeamAdmin = overlay.querySelector('#editTeamAdmin').checked;
            var isDataAdmin = overlay.querySelector('#editDataAdmin').checked;
            var isActive = overlay.querySelector('#editActive').checked;

            if (!userName) {
                showToast('请输入用户名', 'error');
                return;
            }
            if (!name) {
                showToast('请输入姓名', 'error');
                return;
            }

            submitEdit(user.id, {
                userName: userName,
                name: name,
                phone: phone,
                email: email,
                teamId: teamId,
                role: role,
                isTeamAdmin: isTeamAdmin,
                isDataAdmin: isDataAdmin,
                isActive: isActive
            }, close);
        }, '保存');

        // 异步填充团队下拉框并选中当前值（每次打开都强制刷新缓存，确保新增团队可见）
        if (typeof clearTeamsCache === 'function') clearTeamsCache();
        fillTeamOptions('editTeamId', userTeamId || '');
    }

    function submitEdit(userId, data, close) {
        var body = { userId: userId };
        Object.keys(data).forEach(function (k) { body[k] = data[k]; });

        apiPost('/Admin/UpdateUser', body).then(function (res) {
            if (res && res.success) {
                showToast('用户信息已更新', 'success');
                close();
                loadUsers();
            } else {
                showToast((res && res.error) || '更新失败', 'error');
            }
        }).catch(function (e) {
            showToast((e && e.message) || '更新失败', 'error');
        });
    }

    // ===== 加入其他团队（多团队配置）=====
    // 后端复用 /api/Project/AddUserToTeam 端点（管理端无专用端点），需要 admin Token 调用。
    // 该端点会校验调用者 IsTeamAdmin/IsSystemAdmin，admin 用户满足条件。
    function openJoinTeamModal(user) {
        var displayName = user.name || user.userName || ('#' + user.id);
        clearTeamsCache(); // 刷新团队列表缓存
        var bodyHtml =
            '<div class="form-group"><label>用户名</label>' +
            '<input type="text" value="' + escapeHtml(user.userName || '') + '" disabled></div>' +
            '<div class="form-group"><label>当前主团队</label>' +
            '<input type="text" value="' + escapeHtml(user.teamName || '无') + '" disabled></div>' +
            '<div class="form-group"><label>加入团队</label>' +
            '<select id="joinTeamId"><option value="">加载中...</option></select></div>' +
            '<div class="form-tip">此操作将该用户加入所选团队的成员列表，用户可在客户端切换到该团队。不会改变主团队（如需更改主团队请用"编辑"功能）。</div>';

        showModal('加入团队 - ' + displayName, bodyHtml, function (overlay, close) {
            var teamId = overlay.querySelector('#joinTeamId').value;
            if (!teamId) {
                showToast('请选择要加入的团队', 'error');
                return;
            }
            // 调用管理端专用端点（8958 端口限制只允许 /api/Admin/*，/api/Project/* 会被 403）
            apiPost('/Admin/AddUserToTeam', { UserId: user.id, TeamId: teamId }).then(function () {
                showToast('已将 ' + displayName + ' 加入团队', 'success');
                close();
            }).catch(function (e) {
                showToast((e && e.message) || '加入团队失败', 'error');
            });
        }, '加入');

        // 异步填充团队下拉
        fillTeamOptions('joinTeamId', user.teamId || '');
    }

    // ===== 重置密码 =====
    function openPasswordModal(user) {
        var displayName = user.name || user.userName || ('#' + user.id);
        var bodyHtml =
            '<div class="form-group"><label>用户名</label>' +
            '<input type="text" value="' + escapeHtml(user.userName || '') + '" disabled></div>' +
            '<div class="form-group"><label>新密码</label>' +
            '<input type="password" id="newPwd" placeholder="请输入新密码"></div>' +
            '<div class="form-group"><label>确认密码</label>' +
            '<input type="password" id="confirmPwd" placeholder="请再次输入新密码"></div>';

        showModal('重置密码 - ' + displayName, bodyHtml, function (overlay, close) {
            var newPwd = overlay.querySelector('#newPwd').value;
            var confirmPwd = overlay.querySelector('#confirmPwd').value;

            if (!newPwd) {
                showToast('请输入新密码', 'error');
                return;
            }
            if (newPwd !== confirmPwd) {
                showToast('两次输入的密码不一致', 'error');
                return;
            }

            submitResetPassword(user.id, newPwd, close);
        }, '重置');
    }

    function submitResetPassword(userId, newPassword, close) {
        apiPost('/Admin/ResetUserPassword', { userId: userId, newPassword: newPassword }).then(function (res) {
            if (res && res.success) {
                showToast('密码已重置', 'success');
                close();
                loadUsers();
            } else {
                showToast((res && res.error) || '重置失败', 'error');
            }
        }).catch(function (e) {
            showToast((e && e.message) || '重置失败', 'error');
        });
    }

    // ===== 禁用/启用 =====
    function handleToggle(user) {
        var action = user.isActive ? '禁用' : '启用';
        var displayName = user.name || user.userName || ('#' + user.id);
        confirmDialog('确定要' + action + '用户 ' + displayName + ' 吗？').then(function (ok) {
            if (!ok) return;
            apiPost('/Admin/ToggleUserActive', { userId: user.id }).then(function (res) {
                // 接口返回新的启用状态 { isActive: false }
                if (res && res.isActive !== undefined) {
                    showToast(action + '成功', 'success');
                    loadUsers();
                } else if (res && res.success) {
                    showToast(action + '成功', 'success');
                    loadUsers();
                } else {
                    showToast((res && res.error) || (action + '失败'), 'error');
                }
            }).catch(function (e) {
                showToast((e && e.message) || (action + '失败'), 'error');
            });
        });
    }

    // ===== 删除用户 =====
    function handleDelete(user) {
        var displayName = user.name || user.userName || ('#' + user.id);
        confirmDialog('确定要删除用户 ' + displayName + ' 吗？此操作不可恢复').then(function (ok) {
            if (!ok) return;
            apiPost('/Admin/DeleteUser', { userId: user.id }).then(function (res) {
                if (res && res.success) {
                    showToast('用户已删除', 'success');
                    loadUsers();
                } else {
                    showToast((res && res.error) || '删除失败', 'error');
                }
            }).catch(function (e) {
                showToast((e && e.message) || '删除失败', 'error');
            });
        });
    }

    // ===== 新增用户 =====
    function openAddModal() {
        var roleOptions = '';
        Object.keys(ROLE_MAP).forEach(function (key) {
            roleOptions += '<option value="' + key + '">' + escapeHtml(ROLE_MAP[key]) + '</option>';
        });

        var bodyHtml =
            '<div class="form-group"><label>用户名 *</label>' +
            '<input type="text" id="addUserName" placeholder="请输入用户名"></div>' +
            '<div class="form-group"><label>姓名</label>' +
            '<input type="text" id="addName" placeholder="请输入姓名"></div>' +
            '<div class="form-group"><label>密码 *</label>' +
            '<input type="password" id="addPassword" placeholder="请输入密码"></div>' +
            '<div class="form-group"><label>确认密码 *</label>' +
            '<input type="password" id="addConfirmPassword" placeholder="请再次输入密码"></div>' +
            '<div class="form-group"><label>手机号</label>' +
            '<input type="text" id="addPhone" placeholder="请输入手机号"></div>' +
            '<div class="form-group"><label>邮箱</label>' +
            '<input type="email" id="addEmail" placeholder="请输入邮箱"></div>' +
            '<div class="form-group"><label>所属团队</label>' +
            '<select id="addTeamId"><option value="">-- 无团队 --</option></select></div>' +
            '<div class="form-group"><label>角色</label>' +
            '<select id="addRole">' + roleOptions + '</select></div>' +
            checkboxGroup('addTeamAdmin', '团队管理员', false) +
            checkboxGroup('addSystemAdmin', '系统管理员', false);

        showModal('新增用户', bodyHtml, function (overlay, close) {
            var userName = overlay.querySelector('#addUserName').value.trim();
            var name = overlay.querySelector('#addName').value.trim();
            var password = overlay.querySelector('#addPassword').value;
            var confirmPassword = overlay.querySelector('#addConfirmPassword').value;
            var phone = overlay.querySelector('#addPhone').value.trim();
            var email = overlay.querySelector('#addEmail').value.trim();
            var teamId = overlay.querySelector('#addTeamId').value;
            var role = Number(overlay.querySelector('#addRole').value);
            var isTeamAdmin = overlay.querySelector('#addTeamAdmin').checked;
            var isSystemAdmin = overlay.querySelector('#addSystemAdmin').checked;

            if (!userName) {
                showToast('请输入用户名', 'error');
                return;
            }
            if (!password) {
                showToast('请输入密码', 'error');
                return;
            }
            if (password !== confirmPassword) {
                showToast('两次输入的密码不一致', 'error');
                return;
            }

            submitAddUser({
                userName: userName,
                name: name,
                password: password,
                phone: phone,
                email: email,
                teamId: teamId,
                role: role,
                isTeamAdmin: isTeamAdmin,
                isSystemAdmin: isSystemAdmin
            }, close);
        }, '创建');

        // 异步填充团队下拉框（每次打开都强制刷新缓存，确保新增团队可见）
        if (typeof clearTeamsCache === 'function') clearTeamsCache();
        fillTeamOptions('addTeamId', '');
    }

    async function fillTeamOptions(selectId, selectedTeamId) {
        var selectEl = document.getElementById(selectId);
        if (!selectEl) return;
        try {
            var teams = await getTeamsList();
            selectEl.innerHTML = '<option value="">-- 无团队 --</option>';
            teams.forEach(function (t) {
                var opt = document.createElement('option');
                var tid = t.id || t.Id || '';
                opt.value = tid;
                opt.textContent = t.name || t.Name || '';
                if (tid && tid === selectedTeamId) opt.selected = true;
                selectEl.appendChild(opt);
            });
        } catch (e) {
            console.error('加载团队列表失败:', e);
        }
    }

    function submitAddUser(data, close) {
        apiPost('/Admin/CreateUser', data).then(function (res) {
            if (res && res.success) {
                showToast('用户创建成功', 'success');
                close();
                loadUsers();
            } else {
                showToast((res && res.message) || (res && res.error) || '创建失败', 'error');
            }
        }).catch(function (e) {
            showToast((e && e.message) || '创建失败', 'error');
        });
    }

    init();
})();
