/* ===== AuditAI 管理后台 - 公共工具函数 ===== */

// API 基础路径
const API_BASE = '/api';

// localStorage 存储键
const STORAGE_KEYS = {
    USER_ID: 'admin_user_id',
    TOKEN: 'admin_token',
    USER_INFO: 'admin_user_info'
};

/* ---------- SVG 图标库 ---------- */
// 内联 SVG 图标：24x24 viewBox, stroke=currentColor, 2px stroke
// 用于 JS 动态生成的按钮，保证与 HTML 内联图标风格一致
const _SVG_ATTRS = 'class="icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"';
const ICONS = {
    search:    `<svg ${_SVG_ATTRS}><circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/></svg>`,
    plus:      `<svg ${_SVG_ATTRS}><path d="M5 12h14"/><path d="M12 5v14"/></svg>`,
    userPlus:  `<svg ${_SVG_ATTRS}><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="19" x2="19" y1="8" y2="14"/><line x1="22" x2="16" y1="11" y2="11"/></svg>`,
    edit:      `<svg ${_SVG_ATTRS}><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>`,
    trash:     `<svg ${_SVG_ATTRS}><path d="M3 6h18"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" x2="10" y1="11" y2="17"/><line x1="14" x2="14" y1="11" y2="17"/></svg>`,
    download:  `<svg ${_SVG_ATTRS}><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" x2="12" y1="15" y2="3"/></svg>`,
    upload:    `<svg ${_SVG_ATTRS}><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" x2="12" y1="3" y2="15"/></svg>`,
    refresh:   `<svg ${_SVG_ATTRS}><path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"/><path d="M21 3v5h-5"/><path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"/><path d="M3 21v-5h5"/></svg>`,
    reset:     `<svg ${_SVG_ATTRS}><path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"/><path d="M3 3v5h5"/></svg>`,
    clock:     `<svg ${_SVG_ATTRS}><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>`,
    settings:  `<svg ${_SVG_ATTRS}><path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/></svg>`,
    power:     `<svg ${_SVG_ATTRS}><path d="M12 2v10"/><path d="M18.4 6.6a9 9 0 1 1-12.8 0"/></svg>`,
    key:       `<svg ${_SVG_ATTRS}><circle cx="7.5" cy="15.5" r="5.5"/><path d="M21 2l-9.6 9.6"/><path d="M15.5 7.5l3 3"/></svg>`,
    users:     `<svg ${_SVG_ATTRS}><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
    unlink:    `<svg ${_SVG_ATTRS}><path d="M18.84 12.25 21.46 9.6"/><path d="M8.34 10.57a2.85 2.85 0 0 1-2.1-4.5l5.56-5.55a2.85 2.85 0 0 1 4 4L13.2 6.84"/><path d="m7.6 7.6 6.8 6.8"/><path d="M12.87 21.1a2.85 2.85 0 0 1-4.16-3.98l5.56-5.56"/></svg>`,
    save:      `<svg ${_SVG_ATTRS}><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>`,
    x:         `<svg ${_SVG_ATTRS}><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>`,
    check:     `<svg ${_SVG_ATTRS}><polyline points="20 6 9 17 4 12"/></svg>`,
    eye:       `<svg ${_SVG_ATTRS}><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/></svg>`,
    upload2:   `<svg ${_SVG_ATTRS}><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" x2="12" y1="3" y2="15"/></svg>`,
    sparkles:  `<svg ${_SVG_ATTRS}><path d="M9.937 15.5A2 2 0 0 0 8.5 14.063L6.1 13.5l2.4-.563A2 2 0 0 0 9.937 11.5L10.5 9.1l.563 2.4A2 2 0 0 0 12.5 12.937l2.4.563-2.4.563a2 2 0 0 0-1.437 1.437L10.5 17.5z"/><path d="M14.5 19.5 15 21l.5 1.5L16 21l1.5-.5L16 20l-.5-1.5L15 20z"/></svg>`,
    send:      `<svg ${_SVG_ATTRS}><path d="M14.536 21.686a.5.5 0 0 0 .837-.35l.013-9.302A2 2 0 0 0 13.815 10.3l-9.317-.014a.5.5 0 0 1-.177-.968L20.3 1.316a.5.5 0 0 1 .684.684l-8.002 15.983a.5.5 0 0 1-.968-.177Z"/></svg>`,
    building:  `<svg ${_SVG_ATTRS}><path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18Z"/><path d="M6 12H4a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h2"/><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2"/><path d="M10 6h4"/><path d="M10 10h4"/><path d="M10 14h4"/><path d="M10 18h4"/></svg>`,
    folder:    `<svg ${_SVG_ATTRS}><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"/></svg>`,
    alertTri:  `<svg ${_SVG_ATTRS}><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" x2="12" y1="9" y2="13"/><line x1="12" x2="12.01" y1="17" y2="17"/></svg>`,
    userCheck: `<svg ${_SVG_ATTRS}><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>`,
    info:      `<svg ${_SVG_ATTRS}><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>`,
    note:      `<svg ${_SVG_ATTRS}><path d="M18.5 2.5a2.14 2.14 0 0 0-3 0L3.5 14.5a2.14 2.14 0 0 0 0 3l3 3a2.14 2.14 0 0 0 3 0L21.5 8.5a2.14 2.14 0 0 0 0-3Z"/><path d="m15 5 3 3"/><path d="M9 21h8"/></svg>`,
};

/* ---------- 登录态存取 ---------- */

// 获取已存储的鉴权信息，未登录返回 null
function getAuth() {
    const userId = localStorage.getItem(STORAGE_KEYS.USER_ID);
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (!userId || !token) {
        return null;
    }
    return { userId: userId, token: token };
}

// 保存鉴权信息到 localStorage
function setAuth(userId, token, userInfo) {
    localStorage.setItem(STORAGE_KEYS.USER_ID, userId);
    localStorage.setItem(STORAGE_KEYS.TOKEN, token);
    if (userInfo) {
        localStorage.setItem(STORAGE_KEYS.USER_INFO, JSON.stringify(userInfo));
    }
}

// 清除鉴权信息
function clearAuth() {
    stopTokenRefresh();
    localStorage.removeItem(STORAGE_KEYS.USER_ID);
    localStorage.removeItem(STORAGE_KEYS.TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER_INFO);
}

/* ---------- Token 自动刷新 ---------- */

let tokenRefreshTimer = null;
const TOKEN_REFRESH_INTERVAL = 60000; // 60秒刷新一次

// 刷新 Token
async function refreshToken() {
    const auth = getAuth();
    if (!auth) return;
    try {
        const data = await apiGet('/User/UpdateToken');
        if (data && data.tokenValue) {
            setAuth(auth.userId, data.tokenValue, getUserInfo());
        }
    } catch (e) {
        console.warn('Token 刷新失败:', e.message);
    }
}

// 启动 Token 刷新定时器
function startTokenRefresh() {
    stopTokenRefresh();
    tokenRefreshTimer = setInterval(refreshToken, TOKEN_REFRESH_INTERVAL);
}

// 停止 Token 刷新定时器
function stopTokenRefresh() {
    if (tokenRefreshTimer) {
        clearInterval(tokenRefreshTimer);
        tokenRefreshTimer = null;
    }
}

// 获取已登录用户信息（含 isSystemAdmin 等字段）
function getUserInfo() {
    const raw = localStorage.getItem(STORAGE_KEYS.USER_INFO);
    if (!raw) return null;
    try {
        return JSON.parse(raw);
    } catch (e) {
        return null;
    }
}

/* ---------- 统一跳转登录 ---------- */

// 跳转登录页，可通过 sessionStorage 传递一条 toast 提示
function redirectToLogin(toastMessage, toastType) {
    if (toastMessage) {
        sessionStorage.setItem('login_toast', JSON.stringify({
            message: toastMessage,
            type: toastType || 'info'
        }));
    }
    window.location.href = '/index.html';
}

/* ---------- API 调用封装 ---------- */

// 统一请求处理：注入鉴权头、处理 401/403、解析响应
async function apiRequest(method, path, params, body) {
    const auth = getAuth();
    const headers = {
        'Content-Type': 'application/json'
    };
    if (auth) {
        headers['UserId'] = auth.userId;
        headers['Token'] = auth.token;
    }

    let url = API_BASE + path;
    if (params && typeof params === 'object') {
        const query = new URLSearchParams();
        for (const key in params) {
            if (params[key] !== undefined && params[key] !== null) {
                query.append(key, params[key]);
            }
        }
        const qs = query.toString();
        if (qs) {
            url += (url.indexOf('?') >= 0 ? '&' : '?') + qs;
        }
    }

    const options = { method: method, headers: headers };
    if (body !== undefined && body !== null) {
        options.body = JSON.stringify(body);
    }

    let resp;
    try {
        resp = await fetch(url, options);
    } catch (e) {
        throw new Error('网络请求失败：' + (e && e.message ? e.message : e));
    }

    // 401 统一跳登录页（登录接口除外，避免密码错误被当成会话过期）
    if (resp.status === 401) {
        const isLoginApi = path === '/User/AccountLogin' || path === '/User/Login';
        if (!isLoginApi) {
            onAuthFail('会话已过期，请重新登录');
            return null;
        }
    }

    if (resp.status === 403) {
        showToast('无权限', 'error');
        throw new Error('无权限');
    }

    // 解析响应体（兼容 JSON / 纯文本）
    let data = null;
    const text = await resp.text();
    if (text) {
        try {
            data = JSON.parse(text);
        } catch (e) {
            data = text;
        }
    }

    if (!resp.ok) {
        let msg = '请求失败 (' + resp.status + ')';
        if (data && typeof data === 'object') {
            msg = data.message || data.error || data.msg || data.Message || msg;
        } else if (typeof data === 'string' && data) {
            msg = data;
        }
        throw new Error(msg);
    }

    return transformKeysToCamelCase(data);
}

// 将字符串首字母转为小写
function camelCase(str) {
    if (!str) return str;
    return str.charAt(0).toLowerCase() + str.slice(1);
}

// 递归将对象/数组的所有键从 PascalCase 转为 camelCase
// 特殊处理：.NET Tuple 的 Item1/Item2 字段保留不变
function transformKeysToCamelCase(obj) {
    if (obj === null || typeof obj !== 'object') {
        return obj;
    }
    if (Array.isArray(obj)) {
        return obj.map(item => transformKeysToCamelCase(item));
    }
    const result = {};
    for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
            let newKey = key;
            if (!/^Item\d+$/.test(key)) {
                newKey = camelCase(key);
            }
            result[newKey] = transformKeysToCamelCase(obj[key]);
        }
    }
    return result;
}

// SHA256 哈希函数（与客户端 Encrypts.SHA256Encrypt 一致，不包含 URL 编码）
// 浏览器发送请求时会自动进行 URL 编码，所以不需要在这里编码
// 注意：crypto.subtle 仅在 HTTPS/localhost 下可用；HTTP 环境需降级为纯 JS 实现
async function sha256Encrypt(str) {
    if (!str) return '';
    // 优先使用原生 crypto.subtle（HTTPS / localhost）
    if (typeof crypto !== 'undefined' && crypto.subtle && crypto.subtle.digest) {
        const encoder = new TextEncoder();
        const data = encoder.encode(str);
        const hash = await crypto.subtle.digest('SHA-256', data);
        return btoa(String.fromCharCode.apply(null, new Uint8Array(hash)));
    }
    // HTTP 环境降级：纯 JS 实现 SHA-256
    return sha256PureJS(str);
}

// 纯 JavaScript SHA-256 实现（用于 HTTP 环境下 crypto.subtle 不可用时）
function sha256PureJS(str) {
    function rrot(x, n) { return (x >>> n) | (x << (32 - n)); }
    var K = [0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
             0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
             0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
             0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
             0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
             0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
             0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
             0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2];
    var H = [0x6a09e667,0xbb67ae85,0x3c6ef372,0xa54ff53a,0x510e527f,0x9b05688c,0x1f83d9ab,0x5be0cd19];
    var bytes = new TextEncoder().encode(str);
    var l = bytes.length, bitLen = l * 8;
    var padLen = (((l + 9) + 63) >> 6) << 6;
    var padded = new Uint8Array(padLen);
    padded.set(bytes);
    padded[l] = 0x80;
    var dv = new DataView(padded.buffer);
    dv.setUint32(padLen - 4, bitLen >>> 0, false);
    dv.setUint32(padLen - 8, Math.floor(bitLen / 0x100000000), false);
    for (var i = 0; i < padLen; i += 64) {
        var w = new Array(64);
        for (var j = 0; j < 16; j++) w[j] = dv.getUint32(i + j * 4, false);
        for (var j = 16; j < 64; j++) {
            var s0 = rrot(w[j-15],7) ^ rrot(w[j-15],18) ^ (w[j-15] >>> 3);
            var s1 = rrot(w[j-2],17) ^ rrot(w[j-2],19) ^ (w[j-2] >>> 10);
            w[j] = (w[j-16] + s0 + w[j-7] + s1) >>> 0;
        }
        var a=H[0],b=H[1],c=H[2],d=H[3],e=H[4],f=H[5],g=H[6],h=H[7];
        for (var j = 0; j < 64; j++) {
            var S1 = rrot(e,6) ^ rrot(e,11) ^ rrot(e,25);
            var ch = (e & f) ^ (~e & g);
            var t1 = (h + S1 + ch + K[j] + w[j]) >>> 0;
            var S0 = rrot(a,2) ^ rrot(a,13) ^ rrot(a,22);
            var mj = (a & b) ^ (a & c) ^ (b & c);
            var t2 = (S0 + mj) >>> 0;
            h=g; g=f; f=e; e=(d+t1)>>>0; d=c; c=b; b=a; a=(t1+t2)>>>0;
        }
        H[0]=(H[0]+a)>>>0; H[1]=(H[1]+b)>>>0; H[2]=(H[2]+c)>>>0; H[3]=(H[3]+d)>>>0;
        H[4]=(H[4]+e)>>>0; H[5]=(H[5]+f)>>>0; H[6]=(H[6]+g)>>>0; H[7]=(H[7]+h)>>>0;
    }
    var out = new Uint8Array(32);
    var odv = new DataView(out.buffer);
    for (var j = 0; j < 8; j++) odv.setUint32(j * 4, H[j], false);
    var bin = '';
    for (var j = 0; j < 32; j++) bin += String.fromCharCode(out[j]);
    return btoa(bin);
}

// GET 请求
async function apiGet(path, params) {
    return await apiRequest('GET', path, params, null);
}

// POST 请求
async function apiPost(path, body) {
    return await apiRequest('POST', path, null, body);
}

/* ---------- 认证失败统一跳转 ---------- */

// 认证失败时统一处理：清除登录态，顶层跳转到登录页
// 子页面/iframe 中也会跳顶层，避免卡死在空白页
function onAuthFail(message) {
    clearAuth();
    if (window === window.top) {
        redirectToLogin(message || '会话已过期', 'error');
    } else {
        // iframe 内认证失败：直接跳顶层登录页
        try { window.top.location.href = '/index.html'; } catch (e) { /* ignore cross-origin */ }
    }
}

/* ---------- 团队列表下拉选项（供新增/编辑用户使用） ---------- */

// 获取团队列表，返回 [{Id, TeamName, ...}]
let _teamsCache = null;
async function getTeamsList() {
    if (_teamsCache) return _teamsCache;
    const res = await apiGet('/Admin/Teams');
    if (res && res.teams) {
        _teamsCache = res.teams;
        return res.teams;
    }
    return [];
}
function clearTeamsCache() { _teamsCache = null; }

// 渲染团队选择 <select> HTML
function teamSelectHtml(selectedTeamId) {
    const teams = _teamsCache || [];
    let html = '<select id="editTeamId"><option value="">-- 无团队 --</option>';
    teams.forEach(function (t) {
        var selected = (t.id === selectedTeamId || t.Id === selectedTeamId) ? ' selected' : '';
        var teamName = escapeHtml(t.name || t.Name || '');
        var teamId = t.id || t.Id || '';
        html += '<option value="' + escapeHtml(String(teamId)) + '"' + selected + '>' + teamName + '</option>';
    });
    html += '</select>';
    return html;
}

/* ---------- 登录态校验 ---------- */

// 检查登录态，未登录则静默跳转；有 Token 但权限不足才提示会话过期
function requireAdmin() {
    const auth = getAuth();
    if (!auth) {
        // 未登录：静默跳转到登录页，不弹“会话已过期”
        if (window === window.top) {
            window.location.href = '/index.html';
        } else {
            try { window.top.location.href = '/index.html'; } catch (e) { /* ignore cross-origin */ }
        }
        return false;
    }
    const userInfo = getUserInfo();
    const isAdmin = userInfo && (userInfo.isSystemAdmin === true || userInfo.IsSystemAdmin === true ||
                                 userInfo.isSystemAdmin === 'true' || userInfo.IsSystemAdmin === 'true' ||
                                 userInfo.isSystemAdmin === 1 || userInfo.IsSystemAdmin === 1);
    if (!userInfo || !isAdmin) {
        clearAuth();
        onAuthFail('无管理员权限，无法访问后台');
        return false;
    }
    return true;
}

/* ---------- Toast 通知 ---------- */

// 显示 toast 通知，type: success|error|info，3 秒后自动消失
function showToast(message, type) {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = 'toast ' + (type || 'info');

    const iconMap = { success: ICONS.check, error: ICONS.x, info: ICONS.info };
    const icon = document.createElement('span');
    icon.className = 'toast-icon';
    icon.innerHTML = iconMap[type] || iconMap.info;

    const span = document.createElement('span');
    span.textContent = message;

    toast.appendChild(icon);
    toast.appendChild(span);
    container.appendChild(toast);

    setTimeout(function () {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 3000);
}

/* ---------- 确认对话框 ---------- */

// 显示确认对话框，返回 Promise<boolean>
function confirmDialog(message) {
    return new Promise(function (resolve) {
        const overlay = document.createElement('div');
        overlay.className = 'modal-overlay';

        const card = document.createElement('div');
        card.className = 'modal-card confirm-dialog';

        const header = document.createElement('div');
        header.className = 'modal-header';

        const title = document.createElement('div');
        title.className = 'modal-title';
        title.textContent = '确认操作';

        const closeBtn = document.createElement('button');
        closeBtn.className = 'modal-close';
        closeBtn.innerHTML = ICONS.x;

        const body = document.createElement('div');
        body.className = 'modal-body';

        const iconWrap = document.createElement('div');
        iconWrap.className = 'confirm-icon';
        iconWrap.innerHTML = ICONS.alertTri;

        const msgEl = document.createElement('div');
        msgEl.className = 'confirm-message';
        msgEl.textContent = message;

        body.appendChild(iconWrap);
        body.appendChild(msgEl);

        const footer = document.createElement('div');
        footer.className = 'modal-footer';

        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'btn';
        cancelBtn.innerHTML = ICONS.x + '取消';

        const okBtn = document.createElement('button');
        okBtn.className = 'btn btn-primary';
        okBtn.innerHTML = ICONS.check + '确定';

        header.appendChild(title);
        header.appendChild(closeBtn);
        footer.appendChild(cancelBtn);
        footer.appendChild(okBtn);
        card.appendChild(header);
        card.appendChild(body);
        card.appendChild(footer);
        overlay.appendChild(card);
        document.body.appendChild(overlay);

        function close(value) {
            if (overlay.parentNode) {
                overlay.parentNode.removeChild(overlay);
            }
            resolve(value);
        }

        cancelBtn.addEventListener('click', function () { close(false); });
        okBtn.addEventListener('click', function () { close(true); });
        closeBtn.addEventListener('click', function () { close(false); });
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) { close(false); }
        });
    });
}

/* ---------- 分页渲染 ---------- */

// 渲染分页控件：« ‹ 1 2 3 ... ›
// onPageChange(page) 在点击页码时被调用
function renderPagination(containerId, currentPage, totalPages, onPageChange) {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = '';
    container.className = 'pagination';

    currentPage = parseInt(currentPage, 10) || 1;
    totalPages = parseInt(totalPages, 10) || 1;
    if (totalPages < 1) totalPages = 1;
    if (currentPage < 1) currentPage = 1;
    if (currentPage > totalPages) currentPage = totalPages;

    function createButton(text, page, opts) {
        opts = opts || {};
        const btn = document.createElement('button');
        btn.textContent = text;
        if (opts.disabled) btn.disabled = true;
        if (opts.active) btn.classList.add('active');
        if (opts.ellipsis) btn.classList.add('ellipsis');
        if (!opts.disabled && !opts.ellipsis) {
            btn.addEventListener('click', function () {
                if (typeof onPageChange === 'function' && page !== currentPage) {
                    onPageChange(page);
                }
            });
        }
        return btn;
    }

    // « 第一页
    container.appendChild(createButton('«', 1, { disabled: currentPage === 1 }));
    // ‹ 上一页
    container.appendChild(createButton('‹', currentPage - 1, { disabled: currentPage === 1 }));

    // 计算需要显示的页码范围
    const pages = [];
    const showRange = 2; // 当前页前后各显示 2 页
    let start = Math.max(1, currentPage - showRange);
    let end = Math.min(totalPages, currentPage + showRange);

    if (start > 1) {
        pages.push(1);
        if (start > 2) pages.push('...');
    }
    for (let i = start; i <= end; i++) {
        pages.push(i);
    }
    if (end < totalPages) {
        if (end < totalPages - 1) pages.push('...');
        pages.push(totalPages);
    }

    pages.forEach(function (p) {
        if (p === '...') {
            container.appendChild(createButton('…', 0, { ellipsis: true }));
        } else {
            container.appendChild(createButton(String(p), p, { active: p === currentPage }));
        }
    });

    // › 下一页
    container.appendChild(createButton('›', currentPage + 1, { disabled: currentPage === totalPages }));
    // » 末页
    container.appendChild(createButton('»', totalPages, { disabled: currentPage === totalPages }));
}

/* ---------- 登出 ---------- */

function logout() {
    clearAuth();
    window.location.href = '/index.html';
}

/* ---------- 日期格式化 ---------- */

// 格式化日期：纯日期参数返回 YYYY-MM-DD，含时间返回 YYYY-MM-DD HH:mm
function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return String(dateStr);

    const pad = function (n) { return n < 10 ? '0' + n : '' + n; };
    const y = d.getFullYear();
    const m = pad(d.getMonth() + 1);
    const day = pad(d.getDate());
    const h = pad(d.getHours());
    const min = pad(d.getMinutes());

    // 判断原始字符串是否包含时间部分
    const str = String(dateStr);
    const hasTime = /[ T]\d{1,2}:\d{1,2}/.test(str) || str.indexOf('T') >= 0;
    return hasTime ? (y + '-' + m + '-' + day + ' ' + h + ':' + min) : (y + '-' + m + '-' + day);
}

/* ---------- HTML 转义 ---------- */

function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
