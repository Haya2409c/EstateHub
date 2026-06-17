// listings-api.js — connects Listings page to the API
document.addEventListener('DOMContentLoaded', () => {
    const tableId = 'listings-table';
    const table   = document.getElementById(tableId);
    const tbody   = table?.querySelector('tbody');
    if (!table || !tbody) return;

    // ── Row builder ────────────────────────────────────────
    function buildRow(prop) {
        const imgStyle     = prop.thumbnailUrl
            ? `background-image: url('${prop.thumbnailUrl}')`
            : '';
        const userName     = prop.agent  || prop.sellerId?.substring(0, 8) || '—';
        const userType     = prop.agent  ? 'Agent' : 'Seller';
        const category     = prop.category || '—';
        const location     = '—';
        const date         = fmtDate(prop.listedDate);
        const approvalVal  = (prop.approvalStatus || 'Pending').toLowerCase();
        const listingType  = prop.listingType || '—'; // buy / rent / sell

        const approveBtn = approvalVal !== 'active'
            ? `<button class="btn-approve p-1.5 text-emerald-600 hover:bg-emerald-500/10 rounded transition-colors" title="Approve"><i data-lucide="check" class="w-4 h-4"></i></button>`
            : '';
        const rejectBtn = approvalVal !== 'rejected'
            ? `<button class="btn-reject p-1.5 text-red-600 hover:bg-red-500/10 rounded transition-colors" title="Reject"><i data-lucide="x" class="w-4 h-4"></i></button>`
            : '';

        const tr = document.createElement('tr');
        tr.dataset.id     = prop.id;
        tr.dataset.status = approvalVal;
        tr.dataset.category = (category || '').toLowerCase();
        tr.className = 'hover:bg-navy-900/50 dark:hover:bg-navy-800/50 transition-colors group';
        tr.innerHTML = `
            <td class="text-center font-mono text-gray-400">#${prop.id}</td>
            <td>
                <div class="flex items-center gap-3">
                    <div class="w-12 h-12 rounded-lg bg-navy-700 flex-shrink-0 bg-cover bg-center" style="${imgStyle}"></div>
                    <div>
                        <p class="font-medium text-white group-hover:text-gold-500 transition-colors">${prop.title}</p>
                        <p class="text-xs text-gray-400 mt-0.5 font-mono">${fmtMoney(prop.price)}</p>
                    </div>
                </div>
            </td>
            <td>
                <p class="text-sm font-medium text-white">${userName}</p>
                <p class="text-xs text-gray-400 mt-0.5">${userType}</p>
            </td>
            <td>
                <p class="text-sm text-gray-300">${category}</p>
                <p class="text-xs text-gray-400 mt-0.5">${location}</p>
            </td>
            <td><p class="text-sm text-gray-300 font-mono text-xs">${date}</p></td>
            <td>${statusBadge(prop.approvalStatus || 'Pending')}</td>
            <td class="text-right">
                <div class="flex items-center justify-end gap-2">
                    ${approveBtn}
                    ${rejectBtn}
                    <button class="btn-delete-listing p-1.5 text-gray-400 hover:text-red-400 hover:bg-red-500/10 rounded transition-colors ml-1" title="Delete"><i data-lucide="trash-2" class="w-4 h-4"></i></button>
                </div>
            </td>`;
        return tr;
    }

    // ── Load listings ─────────────────────────────────────
    async function loadListings(params = {}) {
        setTableLoading(tbody, 7);
        try {
            const qs = new URLSearchParams({ pageSize: 50, ...params }).toString();
            const data = await AdminApi.get(`/api/admin/listings?${qs}`);
            tbody.innerHTML = '';
            const items = data?.items || [];
            if (items.length === 0) {
                setTableEmpty(tbody, 7, 'No listings found.');
                return;
            }
            items.forEach(prop => tbody.appendChild(buildRow(prop)));
            if (window.lucide) lucide.createIcons();
            if (window.initTablePagination) initTablePagination(tableId);
        } catch (_) {
            setTableEmpty(tbody, 7, 'Failed to load listings.');
        }
    }

    // ── Action buttons (delegation) ───────────────────────
    tbody.addEventListener('click', async (e) => {
        const row = e.target.closest('tr');
        if (!row) return;
        const id = parseInt(row.dataset.id);

        // Approve
        if (e.target.closest('.btn-approve')) {
            e.preventDefault();
            try {
                await AdminApi.put(`/api/admin/listings/${id}/approve`);
                row.dataset.status = 'active';
                row.querySelector('td:nth-child(6)').innerHTML = statusBadge('Active');
                row.querySelector('.btn-approve')?.remove();
                // Replace action buttons
                const actions = row.querySelector('.flex.items-center.justify-end');
                if (actions) {
                    actions.querySelector('.btn-approve')?.remove();
                    if (window.lucide) lucide.createIcons();
                }
                window.showToast('Listing approved.', 'success');
            } catch (_) {}
        }

        // Reject
        if (e.target.closest('.btn-reject')) {
            e.preventDefault();
            try {
                await AdminApi.put(`/api/admin/listings/${id}/reject`);
                row.dataset.status = 'rejected';
                row.querySelector('td:nth-child(6)').innerHTML = statusBadge('Rejected');
                const actions = row.querySelector('.flex.items-center.justify-end');
                if (actions) {
                    actions.querySelector('.btn-reject')?.remove();
                    actions.querySelector('.btn-approve')?.remove();
                    if (window.lucide) lucide.createIcons();
                }
                window.showToast('Listing rejected.', 'success');
            } catch (_) {}
        }

        // Delete
        if (e.target.closest('.btn-delete-listing')) {
            e.preventDefault();
            e.stopPropagation();
            if (!confirm('Delete this listing permanently?')) return;
            try {
                await AdminApi.delete(`/api/admin/listings/${id}`);
                row.remove();
                if (window.initTablePagination) initTablePagination(tableId);
                window.showToast('Listing deleted.', 'success');
            } catch (_) {}
        }
    });

    // ── Filter by status select ───────────────────────────
    const statusSelect = document.querySelector('[data-table-filter-status="listings-table"]');
    const filterBtn    = document.querySelector('[data-table-filter-trigger="listings-table"]');
    if (filterBtn) {
        filterBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const status = statusSelect?.value || '';
            loadListings(status ? { status } : {});
        });
    }

    loadListings();
});
