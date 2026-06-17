// auth.js — JWT token manager for Admin Panel
// Must load BEFORE api.js on every protected page.
// On login.html itself, do NOT include an auth guard redirect.

// ── Backend base URL (single source of truth) ─────────────────
const API_BASE = 'https://localhost:50872';

const AUTH_STORE = {
    ACCESS_KEY:   'admin_access_token',
    REFRESH_KEY:  'admin_refresh_token',
    USER_KEY:     'admin_user',
    EXPIRES_KEY:  'admin_token_expires',
};

const TokenStore = {
    save(accessToken, refreshToken, expiresAt, user) {
        localStorage.setItem(AUTH_STORE.ACCESS_KEY,  accessToken);
        localStorage.setItem(AUTH_STORE.REFRESH_KEY, refreshToken);
        localStorage.setItem(AUTH_STORE.EXPIRES_KEY, new Date(expiresAt).getTime());
        localStorage.setItem(AUTH_STORE.USER_KEY,    JSON.stringify(user));
    },

    getAccess()  { return localStorage.getItem(AUTH_STORE.ACCESS_KEY); },
    getRefresh() { return localStorage.getItem(AUTH_STORE.REFRESH_KEY); },
    getExpiry()  { return parseInt(localStorage.getItem(AUTH_STORE.EXPIRES_KEY) || '0'); },
    getUser()    {
        try { return JSON.parse(localStorage.getItem(AUTH_STORE.USER_KEY) || 'null'); }
        catch { return null; }
    },

    clear() {
        Object.values(AUTH_STORE).forEach(k => localStorage.removeItem(k));
    },

    isExpired() {
        const exp = TokenStore.getExpiry();
        return !exp || Date.now() >= exp;
    },

    expiresInMs() {
        const exp = TokenStore.getExpiry();
        return exp ? exp - Date.now() : 0;
    }
};

// ── Decode JWT payload (no verification — server validates) ──
function decodeJwt(token) {
    try {
        const payload = token.split('.')[1];
        return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    } catch { return null; }
}

// ── Refresh access token silently ─────────────────────────────
async function refreshAccessToken() {
    const user = TokenStore.getUser();
    const refreshToken = TokenStore.getRefresh();
    if (!user?.id || !refreshToken) return false;

    try {
        const res = await fetch(`${API_BASE}/api/auth/admin/refresh`, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify({ userId: user.id, refreshToken })
        });
        if (!res.ok) return false;

        const data = await res.json();
        TokenStore.save(data.accessToken, data.refreshToken, data.expiresAt, user);
        scheduleRefresh();
        return true;
    } catch {
        return false;
    }
}

// ── Schedule proactive refresh 60 s before expiry ────────────
let _refreshTimer = null;
function scheduleRefresh() {
    clearTimeout(_refreshTimer);
    const msLeft = TokenStore.expiresInMs() - 60_000; // 1 min before expiry
    if (msLeft <= 0) return;
    _refreshTimer = setTimeout(async () => {
        const ok = await refreshAccessToken();
        if (!ok) redirectToLogin();
    }, msLeft);
}

// ── Auth guard (call on every protected page) ─────────────────
function redirectToLogin() {
    TokenStore.clear();
    window.location.href = 'login.html';
}

async function requireAuth() {
    const token = TokenStore.getAccess();
    if (!token) { redirectToLogin(); return; }

    if (TokenStore.isExpired()) {
        const ok = await refreshAccessToken();
        if (!ok) { redirectToLogin(); return; }
    } else {
        scheduleRefresh();
    }

    // Populate sidebar user info
    const user = TokenStore.getUser();
    if (user) {
        document.querySelectorAll('.sidebar-user-name').forEach(el => el.textContent = user.fullName || 'Admin');
        document.querySelectorAll('.sidebar-user-email').forEach(el => el.textContent = user.email || '');
        if (user.photoUrl) {
            document.querySelectorAll('.sidebar-user-photo').forEach(el => { el.src = user.photoUrl; });
        }
    }
}

// ── Logout ────────────────────────────────────────────────────
async function adminLogout() {
    const token = TokenStore.getAccess();
    if (token) {
        try {
            await fetch(`${API_BASE}/api/auth/admin/logout`, {
                method:  'POST',
                headers: { 'Authorization': `Bearer ${token}` }
            });
        } catch { /* ignore network errors on logout */ }
    }
    TokenStore.clear();
    clearTimeout(_refreshTimer);
    window.location.href = 'login.html';
}

// ── Wire logout button automatically (delegation — works for dynamically added buttons) ──
document.addEventListener('click', (e) => {
    if (e.target.closest('[data-action="logout"]')) {
        e.preventDefault();
        adminLogout();
    }
});
