// reports-api.js — connects Reports page to the API
document.addEventListener('DOMContentLoaded', () => {
    const table  = document.getElementById('payment-history-table');
    const tbody  = table?.querySelector('tbody');

    const statRevenue      = document.getElementById('stat-total-revenue');
    const statTransactions = document.getElementById('stat-total-transactions');
    const statRefunds      = document.getElementById('stat-total-refunds');

    const fromInput    = document.getElementById('filter-from-date');
    const toInput      = document.getElementById('filter-to-date');
    const applyBtn     = document.getElementById('btn-apply-date-filter');
    const resetBtn     = document.getElementById('btn-reset-date-filter');
    const exportBtn    = document.getElementById('export-csv-btn');

    let currentParams = {};

    // ── Summary stat cards ────────────────────────────────
    async function loadSummary() {
        try {
            const data = await AdminApi.get('/api/admin/reports/summary');
            if (!data) return;
            if (statRevenue)      statRevenue.innerText      = fmtMoney(data.totalRevenue);
            if (statTransactions) statTransactions.innerText = data.totalUsers?.toLocaleString() || '0';
            if (statRefunds)      statRefunds.innerText      = data.activeSubscriptions?.toLocaleString() || '0';
        } catch (_) {}
    }

    // ── Transaction row builder ───────────────────────────
    function buildRow(tx) {
        const d = new Date(tx.createdAt);
        const dateStr = fmtDate(tx.createdAt);
        const timeStr = d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

        const tr = document.createElement('tr');
        tr.dataset.date   = tx.createdAt?.substring(0, 10) || '';
        tr.dataset.status = (tx.status || '').toLowerCase();
        tr.className = 'hover:bg-navy-900/50 dark:hover:bg-navy-800/50 transition-colors';
        tr.innerHTML = `
            <td class="font-mono text-xs text-gray-400">${tx.transactionId}</td>
            <td>
                <p class="text-sm text-gray-300 font-mono text-xs">${dateStr}</p>
                <p class="text-xs text-gray-500 font-mono">${timeStr}</p>
            </td>
            <td>
                <p class="text-sm font-medium text-white">${tx.customerName || '—'}</p>
                <p class="text-xs text-gray-400">${tx.customerEmail || ''}</p>
            </td>
            <td class="text-sm text-gray-300">${tx.package || '—'}</td>
            <td class="font-mono text-sm font-medium text-white">${fmtMoney(tx.amount, tx.currency)}</td>
            <td>${statusBadge(tx.status)}</td>`;
        return tr;
    }

    // ── Load transactions ─────────────────────────────────
    async function loadTransactions(params = {}) {
        if (!tbody) return;
        setTableLoading(tbody, 6);
        currentParams = params;

        try {
            const qs = new URLSearchParams({ pageSize: 100, ...params }).toString();
            const data = await AdminApi.get(`/api/admin/reports/transactions?${qs}`);
            tbody.innerHTML = '';

            const items = data?.items || [];
            if (items.length === 0) {
                setTableEmpty(tbody, 6, 'No transactions found for the selected period.');
                return;
            }

            items.forEach(tx => tbody.appendChild(buildRow(tx)));
            if (window.lucide) lucide.createIcons();

            // Update stat cards from filtered result
            if (statTransactions) statTransactions.innerText = data.total?.toLocaleString() || '0';
            if (statRevenue)      statRevenue.innerText      = fmtMoney(data.totalRevenue || 0);

            if (window.initTablePagination) {
                table.querySelectorAll('tbody tr').forEach(r => r.dataset.searchMatch = 'true');
                initTablePagination('payment-history-table');
            }
        } catch (_) {
            setTableEmpty(tbody, 6, 'Failed to load transactions.');
        }
    }

    // ── Date filter buttons ───────────────────────────────
    function checkChanges() {
        if (!fromInput || !toInput) return;
        const hasVal = fromInput.value && toInput.value;
        if (applyBtn) {
            applyBtn.disabled = !hasVal;
            applyBtn.classList.toggle('opacity-50',        !hasVal);
            applyBtn.classList.toggle('cursor-not-allowed', !hasVal);
        }
        if (resetBtn) {
            resetBtn.disabled = !hasVal;
            resetBtn.classList.toggle('opacity-50',        !hasVal);
            resetBtn.classList.toggle('cursor-not-allowed', !hasVal);
        }
    }

    fromInput?.addEventListener('change', checkChanges);
    toInput?.addEventListener('change',   checkChanges);
    checkChanges();

    if (applyBtn) {
        applyBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            if (!fromInput?.value || !toInput?.value) {
                window.showToast('Please select both dates.', 'error');
                return;
            }

            const orig = applyBtn.innerHTML;
            applyBtn.disabled = true;
            applyBtn.innerHTML = '<i data-lucide="loader-2" class="w-4 h-4 animate-spin inline mr-1"></i> Filtering...';
            if (window.lucide) lucide.createIcons();

            await loadTransactions({ from: fromInput.value, to: toInput.value });
            window.showToast('Filter applied.', 'success');

            applyBtn.disabled = false;
            applyBtn.innerHTML = orig;
            if (window.lucide) lucide.createIcons();
        });
    }

    if (resetBtn) {
        resetBtn.addEventListener('click', (e) => {
            e.preventDefault();
            if (fromInput) fromInput.value = '';
            if (toInput)   toInput.value   = '';
            checkChanges();
            loadTransactions();
            loadSummary();
            window.showToast('Filter reset.', 'info');
        });
    }

    // ── CSV Export ─────────────────────────────────────────
    if (exportBtn) {
        exportBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const rows = Array.from(tbody?.querySelectorAll('tr') || []);
            if (rows.length === 0) {
                window.showToast('No data to export.', 'warning');
                return;
            }
            const headers = ['Transaction ID', 'Date', 'Customer', 'Plan', 'Amount', 'Status'];
            const csvRows = [headers.join(',')];
            rows.forEach(r => {
                const cells = Array.from(r.querySelectorAll('td'));
                const vals = cells.map(c => `"${c.innerText.trim().replace(/"/g, '""')}"`);
                csvRows.push(vals.slice(0, 6).join(','));
            });
            const blob = new Blob([csvRows.join('\n')], { type: 'text/csv' });
            const url  = URL.createObjectURL(blob);
            const a    = document.createElement('a');
            a.href     = url;
            a.download = `transactions_${new Date().toISOString().substring(0, 10)}.csv`;
            a.click();
            URL.revokeObjectURL(url);
            window.showToast('CSV exported.', 'success');
        });
    }

    // Initial load
    loadSummary();
    loadTransactions();
});
