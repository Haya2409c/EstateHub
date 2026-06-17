// packages-api.js — connects Packages page to the API
document.addEventListener('DOMContentLoaded', () => {
    const tableId = 'packages-table';
    const table   = document.getElementById(tableId);
    const tbody   = table?.querySelector('tbody');
    if (!table || !tbody) return;

    const form         = document.getElementById('packageForm');
    const submitBtn    = document.getElementById('pkg-submit-btn');
    const modalTitle   = document.getElementById('modalTitle');
    const packageModal = document.getElementById('packageModal');
    const deleteModal  = document.getElementById('pkgDeleteConfirmModal');
    const confirmDel   = document.getElementById('pkg-confirm-delete-btn');

    const f = {
        name:     document.getElementById('pkg-name'),
        price:    document.getElementById('pkg-price'),
        duration: document.getElementById('pkg-duration'),
        listings: document.getElementById('pkg-listings'),
        images:   document.getElementById('pkg-images'),
        status:   document.getElementById('pkg-status'),
        desc:     document.getElementById('pkg-desc'),
    };

    let currentMode  = 'create';
    let editingRow   = null;
    let editingId    = null;
    let deletingRow  = null;
    let deletingId   = null;

    // ── Row builder ────────────────────────────────────────
    function buildRow(pkg) {
        const listingsDisplay = pkg.listingLimit === null ? 'Unlimited' : pkg.listingLimit;
        const isActive = pkg.isActive;
        const tr = document.createElement('tr');
        tr.dataset.id       = pkg.id;
        tr.dataset.status   = isActive ? 'active' : 'inactive';
        tr.dataset.desc     = pkg.description || '';
        tr.className = 'hover:bg-navy-900/50 dark:hover:bg-navy-800/50 transition-colors';
        tr.innerHTML = `
            <td class="font-medium text-white">${pkg.name}</td>
            <td class="font-mono text-gray-300">$${parseFloat(pkg.price).toFixed(2)}</td>
            <td class="font-mono text-gray-300">${pkg.durationDays} Days</td>
            <td class="font-mono text-gray-300">${listingsDisplay}</td>
            <td class="font-mono text-gray-300">${pkg.imageLimit ?? '—'}</td>
            <td>${statusBadge(isActive ? 'active' : 'inactive')}</td>
            <td class="text-right whitespace-nowrap">
                <button class="btn-edit-package p-1.5 text-blue-500 hover:bg-blue-500/10 rounded transition-colors" title="Edit"><i data-lucide="edit-2" class="w-4 h-4"></i></button>
                <button class="btn-delete-package p-1.5 text-red-500 hover:bg-red-500/10 rounded transition-colors ml-1" title="Delete"><i data-lucide="trash-2" class="w-4 h-4"></i></button>
            </td>`;
        return tr;
    }

    // ── Load packages ─────────────────────────────────────
    async function loadPackages() {
        setTableLoading(tbody, 7);
        try {
            const data = await AdminApi.get('/api/admin/packages');
            tbody.innerHTML = '';
            if (!data || data.length === 0) {
                setTableEmpty(tbody, 7, 'No packages found. Create one to get started.');
                return;
            }
            data.forEach(pkg => tbody.appendChild(buildRow(pkg)));
            if (window.lucide) lucide.createIcons();
            if (window.initTablePagination) initTablePagination(tableId);
        } catch (_) {
            setTableEmpty(tbody, 7, 'Failed to load packages.');
        }
    }

    // ── Edit button click (delegation) ───────────────────
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('.btn-edit-package');
        if (editBtn) {
            e.preventDefault();
            const row = editBtn.closest('tr');
            if (!row) return;
            currentMode = 'edit';
            editingRow  = row;
            editingId   = parseInt(row.dataset.id);

            f.name.value     = row.cells[0].innerText.trim();
            f.price.value    = row.cells[1].innerText.replace('$', '').trim();
            f.duration.value = row.cells[2].innerText.replace(' Days', '').trim();
            const lim = row.cells[3].innerText.trim();
            f.listings.value = lim === 'Unlimited' ? '-1' : lim;
            f.images.value   = row.cells[4].innerText.trim() === '—' ? '' : row.cells[4].innerText.trim();
            f.status.value   = row.dataset.status === 'active' ? 'Active' : 'Inactive';
            f.desc.value     = row.dataset.desc || '';

            modalTitle.innerText   = 'Edit Package';
            submitBtn.innerText    = 'Save Changes';
            packageModal.classList.remove('hidden');
            packageModal.classList.add('flex');
            document.body.style.overflow = 'hidden';
        }

        const delBtn = e.target.closest('.btn-delete-package');
        if (delBtn) {
            e.preventDefault();
            e.stopPropagation();
            const row = delBtn.closest('tr');
            if (!row) return;
            deletingRow = row;
            deletingId  = parseInt(row.dataset.id);
            deleteModal.classList.remove('hidden');
            deleteModal.classList.add('flex');
            document.body.style.overflow = 'hidden';
        }
    });

    // ── Confirm delete ────────────────────────────────────
    if (confirmDel) {
        confirmDel.addEventListener('click', async (e) => {
            e.preventDefault();
            if (!deletingId) return;

            const orig = confirmDel.innerHTML;
            confirmDel.disabled = true;
            confirmDel.innerHTML = '<i data-lucide="loader-2" class="w-4 h-4 animate-spin inline mr-1"></i> Deleting...';
            if (window.lucide) lucide.createIcons();

            try {
                await AdminApi.delete(`/api/admin/packages/${deletingId}`);
                deletingRow?.remove();
                if (window.initTablePagination) initTablePagination(tableId);
                window.showToast('Package deleted.', 'success');
            } catch (_) { /* toast shown */ } finally {
                confirmDel.disabled = false;
                confirmDel.innerHTML = orig;
                deleteModal.classList.add('hidden');
                deleteModal.classList.remove('flex');
                document.body.style.overflow = '';
                deletingRow = null;
                deletingId  = null;
            }
        });
    }

    // ── Form submit (Create / Edit) ───────────────────────
    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            e.stopImmediatePropagation(); // prevent shared.js from running its generic handler

            const listingsVal = parseInt(f.listings.value);
            const payload = {
                name:         f.name.value.trim(),
                description:  f.desc.value.trim(),
                price:        parseFloat(f.price.value),
                durationDays: parseInt(f.duration.value),
                listingLimit: listingsVal === -1 ? null : listingsVal,
                imageLimit:   f.images.value ? parseInt(f.images.value) : null,
                isActive:     f.status.value === 'Active',
                isFeatured:   false,
                sortOrder:    0,
            };

            const orig = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = `<i data-lucide="loader-2" class="w-4 h-4 animate-spin inline mr-1"></i> ${currentMode === 'create' ? 'Creating...' : 'Saving...'}`;
            if (window.lucide) lucide.createIcons();

            try {
                if (currentMode === 'create') {
                    const created = await AdminApi.post('/api/admin/packages', payload);
                    if (created) tbody.appendChild(buildRow(created));
                    window.showToast('Package created.', 'success');
                } else {
                    const updated = await AdminApi.put(`/api/admin/packages/${editingId}`, payload);
                    if (updated && editingRow) {
                        const newRow = buildRow(updated);
                        editingRow.replaceWith(newRow);
                    }
                    window.showToast('Package updated.', 'success');
                }

                if (window.lucide) lucide.createIcons();
                if (window.initTablePagination) initTablePagination(tableId);

                packageModal.classList.add('hidden');
                packageModal.classList.remove('flex');
                document.body.style.overflow = '';
            } catch (_) { /* toast shown */ } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = orig;
                if (window.lucide) lucide.createIcons();
            }
        }, true); // capture = true to run before shared.js
    }

    // ── Create button — reset form mode ──────────────────
    const createBtn = document.querySelector('[data-modal-target="packageModal"]');
    if (createBtn) {
        createBtn.addEventListener('click', () => {
            currentMode  = 'create';
            editingRow   = null;
            editingId    = null;
            if (modalTitle) modalTitle.innerText = 'Create New Package';
            if (submitBtn)  submitBtn.innerText  = 'Create Package';
            if (form) form.reset(); // clear stale values from previous edit
        });
    }

    loadPackages();
});
