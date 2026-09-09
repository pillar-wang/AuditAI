/* ===== 模板管理页面逻辑 ===== */
(function () {
    'use strict';

    const ROLE_MAP = { 0: '经理', 1: '助理', 2: '审核', 3: '编辑', 4: '普通用户' };

    let currentKeyword = '';
    let templates = [];
    let publishingTemplateId = null;
    let publishingTemplateName = '';

    const keywordInput = document.getElementById('keywordInput');
    const searchBtn = document.getElementById('searchBtn');
    const resetBtn = document.getElementById('resetBtn');
    const templatesTbody = document.getElementById('templatesTbody');
    const totalCountInfo = document.getElementById('totalCountInfo');

    const membersModal = document.getElementById('membersModal');
    const membersModalTitle = document.getElementById('membersModalTitle');
    const membersModalBody = document.getElementById('membersModalBody');
    const membersCloseBtn = document.getElementById('membersCloseBtn');

    const publishModal = document.getElementById('publishModal');
    const publishCancelBtn = document.getElementById('publishCancelBtn');
    const publishConfirmBtn = document.getElementById('publishConfirmBtn');
    const publishTemplateName = document.getElementById('publishTemplateName');
    const publishResult = document.getElementById('publishResult');

    /* ---------- 加载模板列表 ---------- */

    async function loadTemplates() {
        templatesTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;">加载中...</td></tr>';
        totalCountInfo.textContent = '';

        try {
            const data = await apiGet('/Admin/AllTemplates', { keyword: currentKeyword });
            templates = (data && data.templates) || [];
            renderTemplates();
        } catch (e) {
            templatesTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;color:var(--error);">加载失败：' + escapeHtml(e.message) + '</td></tr>';
        }
    }

    function renderTemplates() {
        if (!templates.length) {
            templatesTbody.innerHTML = '<tr><td colspan="10" class="text-center text-gray" style="padding:32px;">暂无模板</td></tr>';
            totalCountInfo.textContent = '共 0 条';
            return;
        }

        totalCountInfo.textContent = '共 ' + templates.length + ' 条';
        templatesTbody.innerHTML = templates.map((t, i) => {
            var typeTag = t.systemBuild
                ? '<span class="tag tag-system">系统模板</span>'
                : '<span class="tag tag-team">团队模板</span>';

            var creator = t.creator ? escapeHtml(t.creator.name || t.creator.userName || '') : '<span style="color:var(--text-gray);">-</span>';

            var users = t.users || [];
            var editorCount = users.filter(u => u.role === 3).length;
            var userCount = users.filter(u => u.role === 4).length;

            var editorCell = editorCount > 0
                ? '<span class="tag tag-editor">' + editorCount + ' 人</span>'
                : '<span style="color:var(--text-gray);">0</span>';
            var userCell = userCount > 0
                ? '<span class="tag tag-user">' + userCount + ' 人</span>'
                : '<span style="color:var(--text-gray);">0</span>';

            var note = t.note ? ' title="' + escapeHtml(t.note) + '"' : '';
            var nameCell = escapeHtml(t.name || '') + (note ? ' <span style="color:var(--text-gray);font-size:11px;display:inline-flex;align-items:center;gap:2px;">' + ICONS.note + '</span>' : '');

            var timeStr = t.createTime ? escapeHtml(t.createTime.substring(0, 16).replace('T', ' ')) : '';

            var ops = '<div class="op-btns">';
            ops += '<button class="btn-view" data-act="view" data-id="' + t.id + '">' + ICONS.users + '成员</button>';
            // 系统模板：发布到所有团队；团队模板：提升为系统模板
            if (t.systemBuild) {
                ops += '<button class="btn-publish" data-act="publish" data-id="' + t.id + '" data-name="' + escapeHtml(t.name) + '">' + ICONS.send + '发布</button>';
            } else {
                ops += '<button class="btn-promote" data-act="promote" data-id="' + t.id + '" data-name="' + escapeHtml(t.name) + '">' + ICONS.sparkles + '提升</button>';
            }
            ops += '</div>';

            return '<tr>' +
                '<td>' + (i + 1) + '</td>' +
                '<td>' + nameCell + '</td>' +
                '<td>' + escapeHtml(t.number || '') + '</td>' +
                '<td>' + escapeHtml(t.category || '') + '</td>' +
                '<td>' + typeTag + '</td>' +
                '<td>' + creator + '</td>' +
                '<td>' + editorCell + '</td>' +
                '<td>' + userCell + '</td>' +
                '<td style="white-space:nowrap;">' + timeStr + '</td>' +
                '<td>' + ops + '</td>' +
                '</tr>';
        }).join('');
    }

    /* ---------- 成员详情 ---------- */

    function showMembers(templateId) {
        var t = templates.find(x => x.id === templateId);
        if (!t) return;

        var users = t.users || [];
        var editors = users.filter(u => u.role === 3);
        var normalUsers = users.filter(u => u.role === 4);

        membersModalTitle.textContent = '模板成员 - ' + (t.name || '');

        var html = '';
        if (t.teamVisible) {
            html += '<div style="margin-bottom:12px;padding:8px;background:var(--row-hover);border-radius:var(--radius-input);font-size:13px;">';
            html += '<span style="display:inline-flex;align-items:center;gap:4px;">' + ICONS.eye + ' 该模板对 <strong>全体成员</strong> 可见（所有同事可用）</span>';
            html += '</div>';
        }

        if (!users.length) {
            html += '<div style="color:var(--text-gray);text-align:center;padding:16px;">暂无成员记录</div>';
        } else {
            if (editors.length) {
                html += '<div style="margin:8px 0 4px;font-weight:600;color:var(--orange);font-size:13px;">可编辑的用户（' + editors.length + ' 人）</div>';
                editors.forEach(u => {
                    html += '<div class="member-row">';
                    html += '<span class="member-name">' + escapeHtml(u.name || u.userName || '') + '</span>';
                    html += '<span class="member-role"><span class="tag tag-editor">' + ROLE_MAP[u.role] + '</span></span>';
                    html += '</div>';
                });
            }
            if (normalUsers.length) {
                html += '<div style="margin:12px 0 4px;font-weight:600;color:var(--purple);font-size:13px;">可使用的用户（' + normalUsers.length + ' 人）</div>';
                normalUsers.forEach(u => {
                    html += '<div class="member-row">';
                    html += '<span class="member-name">' + escapeHtml(u.name || u.userName || '') + '</span>';
                    html += '<span class="member-role"><span class="tag tag-user">' + ROLE_MAP[u.role] + '</span></span>';
                    html += '</div>';
                });
            }
        }

        membersModalBody.innerHTML = html;
        membersModal.classList.add('active');
    }

    function hideMembers() {
        membersModal.classList.remove('active');
    }

    /* ---------- 发布模板 ---------- */

    function showPublish(templateId, templateName) {
        publishingTemplateId = templateId;
        publishingTemplateName = templateName;
        publishTemplateName.textContent = templateName;
        publishResult.style.display = 'none';
        publishResult.innerHTML = '';
        publishConfirmBtn.disabled = false;
        publishConfirmBtn.textContent = '确认发布';
        publishModal.classList.add('active');
    }

    function hidePublish() {
        publishModal.classList.remove('active');
        publishingTemplateId = null;
        publishingTemplateName = '';
    }

    async function doPublish() {
        if (!publishingTemplateId) return;
        publishConfirmBtn.disabled = true;
        publishConfirmBtn.textContent = '发布中...';

        try {
            var data = await apiPost('/Admin/PushTemplateToAllTeams', { projectId: publishingTemplateId });
            var teams = (data && data.teams) || [];

            var html = '<div class="publish-result"><div style="font-weight:600;margin-bottom:8px;">发布完成，已推送到 ' + teams.length + ' 个团队</div>';
            if (teams.length) {
                html += '<div style="max-height:200px;overflow-y:auto;">';
                teams.forEach(t => {
                    html += '<div class="team-item"><span>' + escapeHtml(t.name || '未知团队') + '</span><span style="display:inline-flex;align-items:center;gap:2px;color:' + (t.success ? 'var(--success)' : 'var(--error)') + ';">' + (t.success ? ICONS.check + '成功' : ICONS.x + '失败') + '</span></div>';
                });
                html += '</div>';
            }
            html += '</div>';
            publishResult.innerHTML = html;
            publishResult.style.display = 'block';

            showToast('发布成功', 'success');
            loadTemplates();
        } catch (e) {
            showToast('发布失败：' + e.message, 'error');
            publishConfirmBtn.disabled = false;
            publishConfirmBtn.textContent = '确认发布';
        }
    }

    /* ---------- 提升为系统模板 ---------- */

    const promoteModal = document.getElementById('promoteModal');
    const promoteCancelBtn = document.getElementById('promoteCancelBtn');
    const promoteConfirmBtn = document.getElementById('promoteConfirmBtn');
    const promoteTemplateName = document.getElementById('promoteTemplateName');
    let promotingTemplateId = null;

    function showPromote(templateId, templateName) {
        promotingTemplateId = templateId;
        promoteTemplateName.textContent = templateName;
        promoteConfirmBtn.disabled = false;
        promoteConfirmBtn.textContent = '确认提升';
        promoteModal.classList.add('active');
    }

    function hidePromote() {
        promoteModal.classList.remove('active');
        promotingTemplateId = null;
    }

    async function doPromote() {
        if (!promotingTemplateId) return;
        promoteConfirmBtn.disabled = true;
        promoteConfirmBtn.textContent = '提升中...';

        try {
            await apiPost('/Admin/PromoteToSystemTemplate', { projectId: promotingTemplateId });
            showToast('提升成功，模板已成为系统模板', 'success');
            hidePromote();
            loadTemplates();
        } catch (e) {
            showToast('提升失败：' + e.message, 'error');
            promoteConfirmBtn.disabled = false;
            promoteConfirmBtn.textContent = '确认提升';
        }
    }

    /* ---------- 事件绑定 ---------- */

    searchBtn.addEventListener('click', function () {
        currentKeyword = keywordInput.value.trim();
        loadTemplates();
    });

    resetBtn.addEventListener('click', function () {
        keywordInput.value = '';
        currentKeyword = '';
        loadTemplates();
    });

    keywordInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') searchBtn.click();
    });

    templatesTbody.addEventListener('click', function (e) {
        var btn = e.target.closest('button[data-act]');
        if (!btn) return;
        var act = btn.getAttribute('data-act');
        var id = btn.getAttribute('data-id');
        if (act === 'view') showMembers(id);
        else if (act === 'publish') showPublish(id, btn.getAttribute('data-name'));
        else if (act === 'promote') showPromote(id, btn.getAttribute('data-name'));
    });

    membersCloseBtn.addEventListener('click', hideMembers);
    membersModal.addEventListener('click', function (e) {
        if (e.target === membersModal) hideMembers();
    });

    publishCancelBtn.addEventListener('click', hidePublish);
    publishConfirmBtn.addEventListener('click', doPublish);
    publishModal.addEventListener('click', function (e) {
        if (e.target === publishModal) hidePublish();
    });

    promoteCancelBtn.addEventListener('click', hidePromote);
    promoteConfirmBtn.addEventListener('click', doPromote);
    promoteModal.addEventListener('click', function (e) {
        if (e.target === promoteModal) hidePromote();
    });

    /* ---------- 初始化 ---------- */

    loadTemplates();
})();
