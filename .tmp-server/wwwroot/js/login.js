/* ===== 登录页逻辑 ===== */

(function () {
    'use strict';

    const form = document.getElementById('loginForm');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const loginBtn = document.getElementById('loginBtn');

    // 页面加载时显示从其它页面透传过来的 toast（如“会话已过期”）
    function showPendingToast() {
        const raw = sessionStorage.getItem('login_toast');
        if (raw) {
            sessionStorage.removeItem('login_toast');
            try {
                const t = JSON.parse(raw);
                showToast(t.message, t.type);
            } catch (e) { /* ignore */ }
        }
    }

    // 已登录用户直接进入后台
    function redirectIfLoggedIn() {
        const auth = getAuth();
        const userInfo = getUserInfo();
        const isAdmin = userInfo && (userInfo.isSystemAdmin === true || userInfo.IsSystemAdmin === true || 
                                     userInfo.isSystemAdmin === 'true' || userInfo.IsSystemAdmin === 'true' ||
                                     userInfo.isSystemAdmin === 1 || userInfo.IsSystemAdmin === 1);
        if (auth && isAdmin) {
            window.location.href = '/dashboard.html';
        }
    }

    // 设置按钮加载状态
    function setLoading(loading) {
        if (loading) {
            loginBtn.disabled = true;
            loginBtn.dataset.originalText = loginBtn.textContent;
            loginBtn.textContent = '登录中...';
        } else {
            loginBtn.disabled = false;
            if (loginBtn.dataset.originalText) {
                loginBtn.textContent = loginBtn.dataset.originalText;
            }
        }
    }

    // 执行登录
async function doLogin() {
    const userName = usernameInput.value.trim();
    const password = passwordInput.value;

    if (!userName || !password) {
        showToast('请输入用户名和密码', 'error');
        return;
    }

    setLoading(true);
    try {
        const hashPassword = await sha256Encrypt(password);
        const data = await apiGet('/User/AccountLogin', {
            userName: userName,
            password: hashPassword,
            version: 1,
            hasProcess: 0
        });

            // 响应结构：{ Item1: { userId, tokenValue, ... }, Item2: { id, userName, isSystemAdmin, ... } }
            const tokenInfo = (data && data.Item1) || {};
            const userInfo = (data && data.Item2) || {};
            const tokenValue = tokenInfo.tokenValue || tokenInfo.TokenValue;
            const userId = userInfo.id || userInfo.Id || tokenInfo.userId;

            if (!tokenValue || !userId) {
                showToast('登录失败：响应数据异常', 'error');
                return;
            }

            // 管理员权限校验（兼容 PascalCase/IsSystemAdmin 和 camelCase/isSystemAdmin，以及字符串/布尔值）
            const isAdmin = userInfo.isSystemAdmin === true || userInfo.IsSystemAdmin === true || 
                            userInfo.isSystemAdmin === 'true' || userInfo.IsSystemAdmin === 'true' ||
                            userInfo.isSystemAdmin === 1 || userInfo.IsSystemAdmin === 1;
            if (!isAdmin) {
                showToast('无管理员权限，无法访问后台', 'error');
                clearAuth();
                return;
            }

            // 确保保存的用户信息中包含 isSystemAdmin 字段
            const saveUserInfo = {
                id: userInfo.id || userInfo.Id,
                userName: userInfo.userName || userInfo.UserName,
                name: userInfo.name || userInfo.Name,
                isSystemAdmin: true
            };

            setAuth(userId, tokenValue, saveUserInfo);
            startTokenRefresh();
            showToast('登录成功', 'success');
            setTimeout(function () {
                window.location.href = '/dashboard.html';
            }, 300);
        } catch (e) {
            showToast(e.message || '登录失败', 'error');
        } finally {
            setLoading(false);
        }
    }

    // 表单提交
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        doLogin();
    });

    // 初始化
    showPendingToast();
    redirectIfLoggedIn();
    usernameInput.focus();
})();
