/* ===== 激活码管理页逻辑 =====
 *
 * 服务端 API（详见 Program.cs 2922-3047）：
 *   POST /api/Admin/ActivationCodes/Import   body: { codes: "..." }           → { success, total, batchId }
 *   POST /api/Admin/ActivationCodes/Generate body: { count: 100 }            → { codes: [...], batchId, count }
 *   GET  /api/Admin/ActivationCodes/List?status=0|1|2                         → { items: [...], total }
 *   POST /api/Admin/ActivationCodes/Disable  body: { id: 123 }              → { success: true }
 *   POST /api/Admin/ActivationCodes/Delete   body: { id: 123 }              → { success: true }
 *
 * 注意：common.js 的 apiRequest 会把 PascalCase 响应键自动转为 camelCase，
 * 因此本文件读取响应字段一律使用 camelCase（如 data.success / data.items / item.usedByUserId）。
 * 鉴权：apiRequest 自动从 localStorage 注入 UserId / Token 请求头，无需手工添加 Authorization。
 */

(function () {
    'use strict';

    // ===== 状态映射：0=未使用（绿）；1=已使用（灰）；2=已禁用（红） =====
    var STATUS_MAP = {
        0: { name: '未使用', cls: 'unused' },
        1: { name: '已使用', cls: 'used' },
        2: { name: '已禁用', cls: 'disabled' }
    };

    // ===== 当前列表缓存（供导出 CSV 使用） =====
    var currentItems = [];

    // ===== DOM 引用 =====
    var tableBody = document.getElementById('codesTableBody');
    var statusFilter = document.getElementById('statusFilter');
    var filterTip = document.getElementById('filterTip');
    var btnImport = document.getElementById('btnImport');
    var btnGenerate = document.getElementById('btnGenerate');
    var btnRefresh = document.getElementById('btnRefresh');
    var btnExport = document.getElementById('btnExport');

    // ===== 工具函数 =====

    // 状态徽章 HTML
    function getStatusBadge(status) {
        var info = STATUS_MAP[status] || { name: '未知', cls: 'used' };
        return '<span class="ac-badge ' + info.cls + '">' + info.name + '</span>';
    }

    // 显示当前筛选下的总数提示
    function updateFilterTip(total) {
        var statusText = statusFilter.value === '' ? '全部' : (STATUS_MAP[statusFilter.value] || {}).name;
        filterTip.textContent = '当前筛选：' + statusText + '，共 ' + (total || 0) + ' 条';
    }

    // CSV 字段转义：含逗号/引号/换行则用双引号包裹，内部双引号翻倍
    function csvEscape(value) {
        var s = String(value == null ? '' : value);
        if (s.indexOf(',') >= 0 || s.indexOf('"') >= 0 || s.indexOf('\n') >= 0 || s.indexOf('\r') >= 0) {
            return '"' + s.replace(/"/g, '""') + '"';
        }
        return s;
    }

    // ===== 渲染表格 =====
    function renderTable(items) {
        if (!items || items.length === 0) {
            tableBody.innerHTML = '<tr class="ac-empty-row"><td colspan="9">暂无数据</td></tr>';
            return;
        }

        var html = items.map(function (item) {
            var id = item.id != null ? item.id : '';
            var code = escapeHtml(item.code || '');
            var statusBadge = getStatusBadge(item.status);
            var usedBy = item.usedByUserId != null
                ? escapeHtml(String(item.usedByUserId))
                : '<span class="text-gray">—</span>';
            var machine = item.machineCode
                ? escapeHtml(item.machineCode)
                : '<span class="text-gray">—</span>';
            var usedAt = item.usedAt
                ? formatDate(item.usedAt)
                : '<span class="text-gray">—</span>';
            var batch = item.batchId
                ? escapeHtml(item.batchId)
                : '<span class="text-gray">—</span>';
            var createdAt = item.createdAt ? formatDate(item.createdAt) : '';

            // 操作按钮：仅"未使用"状态可禁用；删除任意状态可用
            var canDisable = item.status === 0;
            var disableBtn = canDisable
                ? '<button data-action="disable" data-id="' + id + '">' + ICONS.power + '禁用</button>'
                : '<button disabled title="仅未使用的激活码可禁用">' + ICONS.power + '禁用</button>';
            var deleteBtn = '<button class="danger" data-action="delete" data-id="' + id + '">' + ICONS.trash + '删除</button>';

            return '<tr>' +
                '<td>' + id + '</td>' +
                '<td class="ac-code-text">' + code + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + usedBy + '</td>' +
                '<td>' + machine + '</td>' +
                '<td>' + usedAt + '</td>' +
                '<td>' + batch + '</td>' +
                '<td>' + createdAt + '</td>' +
                '<td><div class="ac-row-actions">' + disableBtn + deleteBtn + '</div></td>' +
                '</tr>';
        }).join('');

        tableBody.innerHTML = html;
    }

    // ===== 加载列表 =====
    // 调用 GET /api/Admin/ActivationCodes/List?status=X
    function loadList(statusFilterValue) {
        var params = {};
        if (statusFilterValue !== '' && statusFilterValue !== null && statusFilterValue !== undefined) {
            params.status = statusFilterValue;
        }
        tableBody.innerHTML = '<tr class="ac-empty-row"><td colspan="9">加载中...</td></tr>';
        updateFilterTip('');

        apiGet('/Admin/ActivationCodes/List', params).then(function (data) {
            currentItems = (data && data.items) || [];
            renderTable(currentItems);
            updateFilterTip((data && data.total) || currentItems.length);
        }).catch(function (e) {
            currentItems = [];
            tableBody.innerHTML = '<tr class="ac-empty-row"><td colspan="9">加载失败：' +
                escapeHtml(e.message || '未知错误') + '</td></tr>';
            showToast(e.message || '加载失败', 'error');
        });
    }

    // ===== 通用弹窗构建 =====
    function buildModal(title, bodyHtml, okText, wide) {
        var overlay = document.createElement('div');
        overlay.className = 'ac-modal-overlay';

        var card = document.createElement('div');
        card.className = 'ac-modal-card' + (wide ? ' wide' : '');
        card.innerHTML =
            '<div class="ac-modal-header">' +
            '<div class="ac-modal-title">' + escapeHtml(title) + '</div>' +
            '<button class="ac-modal-close" data-act="close">' + ICONS.x + '</button>' +
            '</div>' +
            '<div class="ac-modal-body">' + bodyHtml + '</div>' +
            '<div class="ac-modal-footer">' +
                '<button class="ac-btn secondary" data-act="cancel">取消</button>' +
                '<button class="ac-btn" data-act="ok">' + escapeHtml(okText || '确定') + '</button>' +
            '</div>';

        overlay.appendChild(card);
        document.body.appendChild(overlay);

        function close() {
            if (overlay.parentNode) overlay.parentNode.removeChild(overlay);
        }

        // 点击遮罩关闭
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) close();
        });
        overlay.querySelector('[data-act="cancel"]').addEventListener('click', close);
        overlay.querySelector('[data-act="close"]').addEventListener('click', close);

        return {
            overlay: overlay,
            card: card,
            body: card.querySelector('.ac-modal-body'),
            okBtn: card.querySelector('[data-act="ok"]'),
            close: close
        };
    }

    // ===== 批量导入弹窗 =====
    function showImportDialog() {
        var bodyHtml =
            '<div class="ac-form-group">' +
                '<label>激活码列表（每行一个）</label>' +
                '<textarea class="ac-textarea" id="importCodes" placeholder="ABCD-1234-WXYZ-9F8G&#10;EFGH-5678-IJKL-0MNP"></textarea>' +
            '</div>' +
            '<div class="ac-gen-meta">已存在的激活码将自动跳过去重。</div>';

        var modal = buildModal('批量导入激活码', bodyHtml, '导入', false);
        modal.okBtn.addEventListener('click', function () {
            confirmImport(modal);
        });
    }

    // 确认导入：调用 Import API
    function confirmImport(modal) {
        var codesText = document.getElementById('importCodes').value;
        if (!codesText || !codesText.trim()) {
            showToast('请输入至少一个激活码', 'error');
            return;
        }

        modal.okBtn.disabled = true;
        // 调用 POST /api/Admin/ActivationCodes/Import
        // body: { codes: "..." }
        apiPost('/Admin/ActivationCodes/Import', { codes: codesText }).then(function (data) {
            var success = (data && data.success) || 0;
            var total = (data && data.total) || 0;
            var batchId = (data && data.batchId) || '';
            showToast('导入成功：成功 ' + success + ' / 总计 ' + total + '（批次 ' + batchId + '）', 'success');
            modal.close();
            loadList(statusFilter.value);
        }).catch(function (e) {
            showToast(e.message || '导入失败', 'error');
        }).then(function () {
            modal.okBtn.disabled = false;
        });
    }

    // ===== 批量生成弹窗 =====
    function showGenerateDialog() {
        var bodyHtml =
            '<div class="ac-form-group">' +
                '<label>生成数量（1-1000）</label>' +
                '<input type="number" class="ac-input" id="genCount" value="100" min="1" max="1000">' +
            '</div>' +
            '<div class="ac-form-group" id="genResultBox" style="display:none;">' +
                '<label>生成结果（可选中复制）</label>' +
                '<div class="ac-gen-result" id="genResult"></div>' +
                '<div class="ac-gen-meta" id="genMeta"></div>' +
            '</div>';

        var modal = buildModal('批量生成激活码', bodyHtml, '生成', false);
        var state = { generated: false };

        modal.okBtn.addEventListener('click', function () {
            // 生成成功后按钮文案改为"关闭"，再次点击关闭弹窗
            if (state.generated) {
                modal.close();
                loadList(statusFilter.value);
                return;
            }
            confirmGenerate(modal, state);
        });
    }

    // 确认生成：调用 Generate API
    function confirmGenerate(modal, state) {
        var count = parseInt(document.getElementById('genCount').value, 10);
        if (!count || count < 1 || count > 1000) {
            showToast('数量必须在 1-1000 之间', 'error');
            return;
        }

        modal.okBtn.disabled = true;
        // 调用 POST /api/Admin/ActivationCodes/Generate
        // body: { count: N }
        apiPost('/Admin/ActivationCodes/Generate', { count: count }).then(function (data) {
            var codes = (data && data.codes) || [];
            var batchId = (data && data.batchId) || '';

            // 展示生成结果
            document.getElementById('genResult').textContent = codes.join('\n');
            document.getElementById('genMeta').textContent =
                '共 ' + codes.length + ' 个激活码；批次 ID：' + batchId;
            document.getElementById('genResultBox').style.display = 'block';

            // 切换按钮为"关闭"
            state.generated = true;
            modal.okBtn.textContent = '关闭';
            showToast('成功生成 ' + codes.length + ' 个激活码（批次 ' + batchId + '）', 'success');
        }).catch(function (e) {
            showToast(e.message || '生成失败', 'error');
        }).then(function () {
            // 无论成功失败都重新启用按钮（成功后让用户能点"关闭"）
            modal.okBtn.disabled = false;
        });
    }

    // ===== 禁用激活码 =====
    // 调用 POST /api/Admin/ActivationCodes/Disable  body: { id: <id> }
    function disableCode(id) {
        apiPost('/Admin/ActivationCodes/Disable', { id: id }).then(function (data) {
            if (data && data.success) {
                showToast('已禁用', 'success');
                loadList(statusFilter.value);
            } else {
                showToast('禁用失败', 'error');
            }
        }).catch(function (e) {
            showToast(e.message || '禁用失败', 'error');
        });
    }

    // ===== 删除激活码 =====
    // 调用 POST /api/Admin/ActivationCodes/Delete  body: { id: <id> }
    // 删除前由调用方 confirm 确认
    function deleteCode(id) {
        apiPost('/Admin/ActivationCodes/Delete', { id: id }).then(function (data) {
            if (data && data.success) {
                showToast('已删除', 'success');
                loadList(statusFilter.value);
            } else {
                showToast('删除失败', 'error');
            }
        }).catch(function (e) {
            showToast(e.message || '删除失败', 'error');
        });
    }

    // ===== 导出 CSV =====
    // 将当前 currentItems 导出为 CSV 文件（含 BOM 以便 Excel 正确识别 UTF-8）
    function exportCsv() {
        if (!currentItems || currentItems.length === 0) {
            showToast('当前列表为空，无法导出', 'error');
            return;
        }

        var headers = ['ID', '激活码', '状态', '使用人ID', '设备码', '使用时间', '批次', '创建时间'];
        var rows = currentItems.map(function (item) {
            var statusName = (STATUS_MAP[item.status] || {}).name || '';
            return [
                item.id != null ? item.id : '',
                csvEscape(item.code || ''),
                csvEscape(statusName),
                item.usedByUserId != null ? item.usedByUserId : '',
                csvEscape(item.machineCode || ''),
                csvEscape(item.usedAt ? formatDate(item.usedAt) : ''),
                csvEscape(item.batchId || ''),
                csvEscape(item.createdAt ? formatDate(item.createdAt) : '')
            ].join(',');
        });

        // \uFEFF = UTF-8 BOM，确保 Excel 中文不乱码
        var csv = '\uFEFF' + headers.join(',') + '\n' + rows.join('\n');
        var blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);

        var link = document.createElement('a');
        link.href = url;
        var now = new Date();
        var pad = function (n) { return n < 10 ? '0' + n : '' + n; };
        var fname = 'activation-codes-' +
            now.getFullYear() + pad(now.getMonth() + 1) + pad(now.getDate()) +
            '-' + pad(now.getHours()) + pad(now.getMinutes()) + pad(now.getSeconds()) + '.csv';
        link.download = fname;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);

        showToast('已导出 ' + currentItems.length + ' 条记录', 'success');
    }

    // ===== 事件绑定 =====
    // 状态筛选变化
    statusFilter.addEventListener('change', function () {
        loadList(statusFilter.value);
    });

    // 顶部按钮
    btnImport.addEventListener('click', showImportDialog);
    btnGenerate.addEventListener('click', showGenerateDialog);
    btnRefresh.addEventListener('click', function () {
        loadList(statusFilter.value);
    });
    btnExport.addEventListener('click', exportCsv);

    // 表格行操作（事件委托）：禁用/删除均需 confirm
    tableBody.addEventListener('click', function (e) {
        var btn = e.target.closest('button[data-action]');
        if (!btn) return;
        var action = btn.getAttribute('data-action');
        var id = parseInt(btn.getAttribute('data-id'), 10);
        if (!id) return;

        if (action === 'disable') {
            confirmDialog('确定要禁用该激活码吗？禁用后该码将无法使用。').then(function (ok) {
                if (ok) disableCode(id);
            });
        } else if (action === 'delete') {
            confirmDialog('确定要删除该激活码吗？此操作不可撤销。').then(function (ok) {
                if (ok) deleteCode(id);
            });
        }
    });

    // ===== 初始化 =====
    function init() {
        if (!requireAdmin()) return;
        loadList('');
    }

    init();
})();
