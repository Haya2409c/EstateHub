// users-api.js — connects Users page to the API
document.addEventListener('DOMContentLoaded', () => {
    const agentsTableId  = 'users-agents-table';
    const sellersTableId = 'users-sellers-table';
    const agentsTbody    = document.getElementById(agentsTableId)?.querySelector('tbody');
    const sellersTbody   = document.getElementById(sellersTableId)?.querySelector('tbody');

    if (!agentsTbody && !sellersTbody) return;

    // ── Row builder ────────────────────────────────────────
    function buildRow(user) {
        const isLocked  = user.isLocked;
        const statusVal = isLocked ? 'suspended' : 'active';
        const joinDate  = fmtDate(user.createdAt);

        const toggleIcon  = isLocked
            ? '<i data-lucide="play-circle" class="w-4 h-4"></i>'
            : '<i data-lucide="pause-circle" class="w-4 h-4"></i>';
        const toggleTitle = isLocked ? 'Activate' : 'Deactivate';
        const toggleColor = isLocked
            ? 'text-emerald-500 hover:bg-emerald-500/10'
            : 'text-amber-500 hover:bg-amber-500/10';

        const initials = (user.fullName || '?')
            .split(' ').map(w => w[0]).join('').substring(0, 2).toUpperCase();

        const tr = document.createElement('tr');
        tr.dataset.id     = user.id;
        tr.dataset.status = statusVal;
        tr.className = 'hover:bg-navy-900/50 dark:hover:bg-navy-800/50 transition-colors';
        tr.innerHTML = `
            <td>
                <div class="flex items-center gap-3">
                    <div class="w-9 h-9 rounded-full bg-navy-600 flex items-center justify-center text-sm font-semibold text-white flex-shrink-0">
                        ${user.photoUrl
                            ? `<img src="${user.photoUrl}" class="w-9 h-9 rounded-full object-cover" alt="">`
                            : initials}
                    </div>
                    <div>
                        <p class="text-sm font-medium text-white">${user.fullName}</p>
                        <p class="text-xs text-gray-400">${user.email || '—'}</p>
                    </div>
                </div>
            </td>
            <td class="text-sm text-gray-300">${user.accountType}</td>
            <td class="text-sm text-gray-400 font-mono text-xs">${joinDate}</td>
            <td>${statusBadge(statusVal)}</td>
            <td class="text-right whitespace-nowrap">
                <button class="btn-toggle-status p-1.5 ${toggleColor} rounded transition-colors" title="${toggleTitle}" data-action="toggle-status">${toggleIcon}</button>
                <button class="btn-delete-user p-1.5 text-red-500 hover:bg-red-500/10 rounded transition-colors ml-1" title="Delete" data-action="delete"><i data-lucide="trash-2" class="w-4 h-4"></i></button>
            </td>`;
        return tr;
    }

    // ── Load users ────────────────────────────────────────
    async function loadUsers(accountType, tbodyEl, tableId) {
        if (!tbodyEl) return;
        setTableLoading(tbodyEl, 5);
        try {
            const data = await AdminApi.get(`/api/admin/users?accountType=${accountType}&pageSize=100`);
            tbodyEl.innerHTML = '';
            const items = data?.items || [];
            if (items.length === 0) {
                setTableEmpty(tbodyEl, 5, `No ${accountType.toLowerCase()}s found.`);
                return;
            }
            items.forEach(u => tbodyEl.appendChild(buildRow(u)));
            if (window.lucide) lucide.createIcons();
            if (window.initTablePagination) initTablePagination(tableId);
        } catch (_) {
            setTableEmpty(tbodyEl, 5, `Failed to load ${accountType.toLowerCase()}s.`);
        }
    }

    // ── Action delegation ─────────────────────────────────
    async function handleAction(e) {
        const row = e.target.closest('tr');
        if (!row) return;
        const id = row.dataset.id;

        // Toggle status
        const toggleBtn = e.target.closest('.btn-toggle-status');
        if (toggleBtn) {
            e.preventDefault();
            e.stopImmediatePropagation(); // prevent shared.js toggling UI only

            try {
                const res = await AdminApi.put(`/api/admin/users/${id}/toggle-status`);
                if (!res) return;

                const nowLocked = res.isLocked;
                row.dataset.status = nowLocked ? 'suspended' : 'active';

                const badge = row.querySelector('td span.rounded-full');
                if (badge) badge.outerHTML = statusBadge(nowLocked ? 'suspended' : 'active');

                toggleBtn.className = `btn-toggle-status p-1.5 ${nowLocked ? 'text-emerald-500 hover:bg-emerald-500/10' : 'text-amber-500 hover:bg-amber-500/10'} rounded transition-colors`;
                toggleBtn.title = nowLocked ? 'Activate' : 'Deactivate';
                toggleBtn.innerHTML = nowLocked
                    ? '<i data-lucide="play-circle" class="w-4 h-4"></i>'
                    : '<i data-lucide="pause-circle" class="w-4 h-4"></i>';
                if (window.lucide) lucide.createIcons();

                window.showToast(res.message, 'success');
            } catch (_) {}
        }

        // Delete
        const delBtn = e.target.closest('.btn-delete-user');
        if (delBtn) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if (!confirm('Delete this user permanently? This cannot be undone.')) return;
            try {
                await AdminApi.delete(`/api/admin/users/${id}`);
                const tableEl = row.closest('table');
                row.remove();
                if (tableEl && window.initTablePagination) initTablePagination(tableEl.id);
                window.showToast('User deleted.', 'success');
            } catch (_) {}
        }
    }

    document.getElementById(agentsTableId)?.querySelector('tbody')
        ?.addEventListener('click', handleAction, true);
    document.getElementById(sellersTableId)?.querySelector('tbody')
        ?.addEventListener('click', handleAction, true);

    // ── Search box ─────────────────────────────────────────
    const searchInput = document.querySelector('input[data-table-search]');
    let searchTimer;
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(async () => {
                const q = searchInput.value.trim();
                if (!q) {
                    loadUsers('Agent', agentsTbody, agentsTableId);
                    loadUsers('Seller', sellersTbody, sellersTableId);
                    return;
                }
                // search in already-loaded DOM (shared.js handles client-side filter)
            }, 300);
        });
    }

    // Initial load
    loadUsers('Agent',  agentsTbody,  agentsTableId);
    loadUsers('Seller', sellersTbody, sellersTableId);
});
