// api.js — shared API client for Admin Panel
// API_BASE is declared in auth.js (load auth.js first)

const AdminApi = (() => {
    function authHeaders() {
        const token = (typeof TokenStore !== 'undefined') ? TokenStore.getAccess() : null;
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    async function request(path, options = {}, isRetry = false) {
        const url = `${API_BASE}${path}`;
        const config = {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                'Accept':       'application/json',
                ...authHeaders(),
                ...(options.headers || {})
            }
        };

        try {
            const res = await fetch(url, config);

            if (res.status === 401 && !isRetry) {
                // Try silent refresh once, then retry original request
                const refreshed = (typeof refreshAccessToken !== 'undefined')
                    ? await refreshAccessToken()
                    : false;
                if (refreshed) return request(path, options, true);

                window.showToast?.('Session expired — please log in again.', 'error');
                setTimeout(() => {
                    if (typeof redirectToLogin !== 'undefined') redirectToLogin();
                    else window.location.href = 'login.html';
                }, 1500);
                return null;
            }

            if (res.status === 204) return null;

            const data = await res.json();
            if (!res.ok) {
                const msg = data?.error || data?.message || data?.title || 'Server error';
                throw new Error(msg);
            }
            return data;
        } catch (err) {
            if (err.name === 'TypeError') {
                window.showToast?.('Cannot reach the backend. Is it running?', 'error');
            } else {
                window.showToast?.(err.message, 'error');
            }
            throw err;
        }
    }

    return {
        get:    (path)       => request(path),
        post:   (path, body) => request(path, { method: 'POST',   body: JSON.stringify(body) }),
        put:    (path, body) => request(path, { method: 'PUT',    body: JSON.stringify(body) }),
        delete: (path)       => request(path, { method: 'DELETE' }),
    };
})();

// ── UI helpers ──────────────────────────────────────────────
function setTableLoading(tbodyEl, cols) {
    if (!tbodyEl) return;
    const skeletonRow = Array.from({ length: 5 }, () => `
        <tr class="animate-pulse">
            ${Array.from({ length: cols }, () =>
                `<td><div class="h-4 bg-navy-700 rounded w-3/4"></div></td>`
            ).join('')}
        </tr>`).join('');
    tbodyEl.innerHTML = skeletonRow;
}

function setTableEmpty(tbodyEl, cols, message = 'No records found.') {
    if (!tbodyEl) return;
    tbodyEl.innerHTML = `
        <tr>
            <td colspan="${cols}" class="py-12 text-center text-gray-500">
                <i data-lucide="inbox" class="w-8 h-8 mx-auto mb-2 opacity-40"></i>
                <p class="text-sm">${message}</p>
            </td>
        </tr>`;
    if (window.lucide) lucide.createIcons();
}

function fmtDate(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: '2-digit' });
}

function fmtMoney(n, cur = 'USD') {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: cur }).format(n);
}

function statusBadge(status) {
    const map = {
        active:     'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
        completed:  'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
        approved:   'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
        pending:    'bg-amber-500/10  text-amber-400   border-amber-500/20',
        rejected:   'bg-red-500/10    text-red-400     border-red-500/20',
        failed:     'bg-red-500/10    text-red-400     border-red-500/20',
        suspended:  'bg-red-500/10    text-red-400     border-red-500/20',
        inactive:   'bg-gray-500/10   text-gray-400    border-gray-500/20',
        expired:    'bg-gray-500/10   text-gray-400    border-gray-500/20',
        refunded:   'bg-blue-500/10   text-blue-400    border-blue-500/20',
    };
    const cls = map[status?.toLowerCase()] || 'bg-gray-500/10 text-gray-400 border-gray-500/20';
    const label = status ? status.charAt(0).toUpperCase() + status.slice(1) : '—';
    return `<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${cls}">${label}</span>`;
}
