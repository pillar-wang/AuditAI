/* ===== 仪表盘主框架逻辑 ===== */

(function () {
    'use strict';

    const navList = document.getElementById('navList');
    const navItems = navList.querySelectorAll('.nav-item');
    const logoutItem = document.getElementById('logoutItem');
    const contentFrame = document.getElementById('contentFrame');
    const adminNameEl = document.getElementById('adminName');
    const changePwdBtn = document.getElementById('changePwdBtn');

    // 显示当前管理员名称
    function renderAdminName() {
        const userInfo = getUserInfo();
        if (userInfo) {
            adminNameEl.textContent = userInfo.name || userInfo.userName || '管理员';
        } else {
            adminNameEl.textContent = '管理员';
        }
    }

    // 切换导航激活态并加载页面到 iframe
    function selectNav(navItem) {
        navItems.forEach(function (item) { item.classList.remove('active'); });
        navItem.classList.add('active');
        const page = navItem.getAttribute('data-page');
        if (page) {
            contentFrame.src = page;
        }
    }

    // 绑定导航点击
    navItems.forEach(function (item) {
        item.addEventListener('click', function () {
            selectNav(item);
        });
    });

    // 修改密码
    changePwdBtn.addEventListener('click', function () {
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.innerHTML = `
            <div class="modal">
                <div class="modal-header">
                    <span class="modal-title">修改密码</span>
                    <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>原密码</label>
                        <input type="password" id="oldPwd" placeholder="请输入原密码" required>
                    </div>
                    <div class="form-group">
                        <label>新密码</label>
                        <input type="password" id="newPwd" placeholder="请输入新密码（至少6位）" required>
                    </div>
                    <div class="form-group">
                        <label>确认新密码</label>
                        <input type="password" id="confirmPwd" placeholder="请再次输入新密码" required>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-default" onclick="this.closest('.modal-overlay').remove()">取消</button>
                    <button class="btn btn-primary" id="savePwdBtn">确认修改</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const saveBtn = modal.querySelector('#savePwdBtn');
        saveBtn.addEventListener('click', async function () {
            const oldPwd = modal.querySelector('#oldPwd').value;
            const newPwd = modal.querySelector('#newPwd').value;
            const confirmPwd = modal.querySelector('#confirmPwd').value;

            if (!oldPwd || !newPwd || !confirmPwd) {
                alert('请填写完整信息');
                return;
            }
            if (newPwd.length < 6) {
                alert('新密码长度至少6位');
                return;
            }
            if (newPwd !== confirmPwd) {
                alert('两次输入的新密码不一致');
                return;
            }

            saveBtn.disabled = true;
            saveBtn.textContent = '处理中...';

            try {
                const res = await apiPost('/Admin/ChangePassword', { oldPassword: oldPwd, newPassword: newPwd });
                if (res.success) {
                    alert('密码修改成功，请重新登录');
                    modal.remove();
                    logout();
                } else {
                    alert(res.message || '修改失败');
                }
            } catch (err) {
                alert('修改失败：' + err.message);
            } finally {
                saveBtn.disabled = false;
                saveBtn.textContent = '确认修改';
            }
        });
    });

    // 登出
    logoutItem.addEventListener('click', async function () {
        const ok = await confirmDialog('确定要退出登录吗？');
        if (ok) {
            logout();
        }
    });

    // 页面加载初始化
    function init() {
        const auth = getAuth();
        const userInfo = getUserInfo();
        const isAdmin = userInfo && (userInfo.isSystemAdmin === true || userInfo.IsSystemAdmin === true ||
                                     userInfo.isSystemAdmin === 'true' || userInfo.IsSystemAdmin === 'true' ||
                                     userInfo.isSystemAdmin === 1 || userInfo.IsSystemAdmin === 1);
        
        if (!auth || !userInfo || !isAdmin) {
            clearAuth();
            window.location.href = '/index.html';
            return;
        }
        
        renderAdminName();
        startTokenRefresh();
    }

    init();
})();
